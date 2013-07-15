using System;

namespace WaveBox.Client
{
	public class BassGaplessPlayer
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public IBassGaplessPlayerDelegate Delegate { get; set; }
			
		public BassGaplessPlayer ()
		{
		}
	}
}

