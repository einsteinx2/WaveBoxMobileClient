using System;
using System.Runtime.CompilerServices;

namespace WaveBox.Client.AudioEngine
{
	public class RingBuffer
	{
		private byte[] buffer;

		public int ReadPosition { get; set; }
		public int WritePosition { get; set; }
		public long TotalBytesDrained { get; set; }

		public RingBuffer(int bytes) 
		{
			buffer = new byte[bytes];
			Reset();
		}

		public void Reset() 
		{
			ReadPosition = 0;
			WritePosition = 0;
		}

		public int TotalLength() 
		{
			return buffer.Length;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public int FreeSpaceLength() 
		{
			return TotalLength() - FilledSpaceLength();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public int FilledSpaceLength() 
		{
			if (ReadPosition <= WritePosition) 
			{
				return WritePosition - ReadPosition;
			} 
			else 
			{
				// The write position has looped around
				return TotalLength() - ReadPosition + WritePosition;
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void AdvanceWritePosition(int writeLength) 
		{
			WritePosition += writeLength;
			if (WritePosition >= TotalLength()) 
			{
				WritePosition = WritePosition - TotalLength();
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void AdvanceReadPosition(int readLength) 
		{
			ReadPosition += readLength;
			if (ReadPosition >= TotalLength()) 
			{
				ReadPosition = ReadPosition - TotalLength();
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool FillWithBytes(byte[] bytes, int offset, int length) 
		{
			// Sanity check
			length = bytes.Length >= length ? length : bytes.Length;

			// Make sure there is space
			if (FreeSpaceLength() > length) 
			{
				int bytesUntilEnd = TotalLength() - WritePosition;
				if (length > bytesUntilEnd) 
				{
					// Split it between the end and beginning
					Array.Copy(bytes, offset, buffer, WritePosition, bytesUntilEnd);
					Array.Copy(bytes, offset + bytesUntilEnd, buffer, 0, length - bytesUntilEnd);
				} else {
					// Just copy in the bytes
					Array.Copy(bytes, offset, buffer, WritePosition, length);
				}

				AdvanceWritePosition(length);
				return true;
			}
			return false;
		}

		public bool FillWithBytes(byte[] bytes) 
		{
			return FillWithBytes(bytes, 0, bytes.Length);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public int DrainBytes(byte[] byteBuffer, int bufferLength) 
		{
			// Sanity checks: can't drain more than what is in the buffer
			bufferLength = FilledSpaceLength() >= bufferLength ? bufferLength : FilledSpaceLength();
			// And can't drain more than what fits in byteBuffer
			bufferLength = bufferLength > byteBuffer.Length ? byteBuffer.Length : bufferLength;

			if (bufferLength > 0) 
			{
				int bytesUntilEnd = TotalLength() - ReadPosition;
				if (bufferLength > bytesUntilEnd)
				{
					// Split it between the end and beginning
					Array.Copy(buffer, ReadPosition, byteBuffer, 0, bytesUntilEnd);
					Array.Copy(buffer, 0, byteBuffer, bytesUntilEnd, bufferLength - bytesUntilEnd);
				} 
				else 
				{
					// Just copy in the bytes
					Array.Copy(buffer, ReadPosition, byteBuffer, 0, bufferLength);
				}

				AdvanceReadPosition(bufferLength);
			}

			TotalBytesDrained += bufferLength;
			return bufferLength;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public byte[] DrainData(int length) 
		{
			length = length > FilledSpaceLength() ? FilledSpaceLength() : length;
			byte[] bytes = new byte[length];
			DrainBytes(bytes, length);
			return bytes;
		}

		public bool HasSpace(int length) 
		{
			return FreeSpaceLength() >= length;
		}
	}
}

