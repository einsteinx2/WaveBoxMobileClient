using System;

namespace WaveBox.Client
{
	public class MissingApiParameterException : ServerInteractionException
	{
		public MissingApiParameterException()
		{
		}

		public MissingApiParameterException(string message): base(message)
		{
		}

		public MissingApiParameterException(string message, Exception innerException): base(message, innerException)
		{
		}
	}
}

