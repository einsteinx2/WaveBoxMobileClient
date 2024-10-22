﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using System.IO;

namespace log4net
{
    public interface ILog
    {
		bool IsInfoEnabled { get; }
		bool IsDebugEnabled { get; }

        void Debug(object msg, params object[] objs);
        void Error(object msg, params object[] objs);
        void Info(object msg, params object[] objs);
        void Warn(object msg, params object[] objs);
        void Fatal(object msg, params object[] objs);
    }

    public class FileLogger
    {
        public static readonly FileLogger Instance = new FileLogger();
        private FileLogger()
        {
        }

        private Queue _que = new Queue();
        private StreamWriter _output;
        private string _filename;
        private int _sizeLimit = 0;
        private long _lastSize = 0;
        private DateTime _lastFileDate;
        private bool _showMethodName = false;
        private string _FilePath = "";
        System.Timers.Timer _saveTimer;

        public bool ShowMethodNames
        {
            get { return _showMethodName; }
        }

        public void Init(string filename, int sizelimitKB, bool showmethodnames)
        {
            _que = new Queue();
            _showMethodName = showmethodnames;
            _sizeLimit = sizelimitKB;
            _filename = filename;
            // handle folder names as well -> create dir etc.
            _FilePath = Path.GetDirectoryName(filename);
            if (_FilePath != "")
            {
                _FilePath = Directory.CreateDirectory(_FilePath).FullName;
                if (_FilePath.EndsWith("\\") == false)
                    _FilePath += "\\";
            }
            _output = new StreamWriter(filename, true);
            FileInfo fi = new FileInfo(filename);
            _lastSize = fi.Length;
            _lastFileDate = fi.LastWriteTime;

            _saveTimer = new System.Timers.Timer(500);
            _saveTimer.Elapsed += new System.Timers.ElapsedEventHandler(_saveTimer_Elapsed);
            _saveTimer.Enabled = true;
            _saveTimer.AutoReset = true;
        }

        void _saveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            WriteData();
        }

        public void ShutDown()
        {
            _saveTimer.Enabled = false;
            WriteData();
            if (_output != null)
            {
                _output.Flush();
                _output.Close();
                _output = null;
            }
        }

        object _writelock = new object();
        private void WriteData()
        {
            if (_output == null)
                return;
            lock (_writelock)
            {
                while (_que.Count > 0)
                {
                    object o = _que.Dequeue();
                    if (_output != null && o != null)
                    {
                        if (_sizeLimit > 0)
                        {
                            // implement size limited logs
                            // implement rolling logs
                            #region [  rolling size limit ]
                            _lastSize += ("" + o).Length;
                            if (_lastSize > _sizeLimit * 1000)
                            {
                                _output.Flush();
                                _output.Close();
                                int count = 1;
                                while (File.Exists(_FilePath + Path.GetFileNameWithoutExtension(_filename) + "." + count.ToString("0000")))
                                    count++;

                                File.Move(_filename,
                                    _FilePath +
                                    Path.GetFileNameWithoutExtension(_filename) +
                                    "." + count.ToString("0000"));
                                _output = new StreamWriter(_filename, true);
                                _lastSize = 0;
                            }
                            #endregion
                        }
                        if (DateTime.Now.Subtract(_lastFileDate).Days > 0)
                        {
                            // implement date logs
                            #region [  rolling dates  ]
                            _output.Flush();
                            _output.Close();
                            int count = 1;
                            while (File.Exists(_FilePath + Path.GetFileNameWithoutExtension(_filename) + "." + count.ToString("0000")))
                            {
                                File.Move(_FilePath + Path.GetFileNameWithoutExtension(_filename) + "." + count.ToString("0000"),
                                   _FilePath +
                                   Path.GetFileNameWithoutExtension(_filename) +
                                   "." + count.ToString("0000") +
                                   "." + _lastFileDate.ToString("yyyy-MM-dd"));
                                count++;
                            }
                            File.Move(_filename,
                               _FilePath +
                               Path.GetFileNameWithoutExtension(_filename) +
                               "." + count.ToString("0000") +
                               "." + _lastFileDate.ToString("yyyy-MM-dd"));

                            _output = new StreamWriter(_filename, true);
                            _lastFileDate = DateTime.Now;
                            _lastSize = 0;
                            #endregion
                        }
                        _output.Write(o);
                    }
                }
                if (_output != null)
                    _output.Flush();
            }
        }

        private string FormatLog(string log, string type, string meth, string msg, object[] objs)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            sb.Append("|");
            sb.Append(log);
            sb.Append("|");
            sb.Append(Thread.CurrentThread.ManagedThreadId.ToString());
            sb.Append("|");
            sb.Append(type);
            sb.Append("|");
            sb.Append(meth);
            sb.Append("| ");
            sb.AppendLine(msg);

            if (objs != null)
                foreach (object o in objs)
                    sb.AppendLine("" + o);

            return sb.ToString();
        }

        public void Log(string logtype, string type, string meth, string msg, params object[] objs)
        {
			Console.WriteLine(FormatLog(logtype, type, meth, msg, objs));
            //_que.Enqueue(FormatLog(logtype, type, meth, msg, objs));
        }
    }


    public class logger : ILog
    {
        public logger(Type type)
        {
            typename = type.Namespace + "." + type.Name;
        }

        private string typename = "";

        private void log(string logtype, string msg, params object[] objs)
        {
            string meth = "";
            if (FileLogger.Instance.ShowMethodNames)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(2);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                meth = sf.GetMethod().Name;
            }
            FileLogger.Instance.Log(logtype, typename, meth, msg, objs);
		}

        #region ILog Members

		public bool IsInfoEnabled { get { return true; } }

		public bool IsDebugEnabled { get { return true; } }

        public void Debug(object msg, params object[] objs)
        {
            log("DEBUG", "" + msg, objs);
        }

        public void Error(object msg, params object[] objs)
        {
            log("ERROR", "" + msg, objs);
        }

        public void Info(object msg, params object[] objs)
        {
            log("INFO", "" + msg, objs);
        }

        public void Warn(object msg, params object[] objs)
        {
            log("WARN", "" + msg, objs);
        }

        public void Fatal(object msg, params object[] objs)
        {
            log("FATAL", "" + msg, objs);
        }
        #endregion
    }

    public static class LogManager
    {
        public static ILog GetLogger(Type obj)
        {
            return new logger(obj);
        }

        public static void Configure(string filename, int sizelimitKB, bool showmethodnames)
        {
            FileLogger.Instance.Init(filename, sizelimitKB, showmethodnames);
        }

        public static void Shutdown()
        {
            FileLogger.Instance.ShutDown();
        }
    }

}
