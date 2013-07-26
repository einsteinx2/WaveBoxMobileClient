using System;

namespace WaveBox.Client
{
	public class MissingServerUrlException : ServerInteractionException
	{
		public MissingServerUrlException()
		{
		}

		public MissingServerUrlException(string message): base(message)
		{
		}

		public MissingServerUrlException(string message, Exception innerException): base(message, innerException)
		{
		}
	}
}

