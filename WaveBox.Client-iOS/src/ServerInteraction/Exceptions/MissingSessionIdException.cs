using System;

namespace WaveBox.Client
{
	public class MissingSessionIdException : ServerInteractionException
	{
		public MissingSessionIdException()
		{
		}

		public MissingSessionIdException(string message): base(message)
		{
		}

		public MissingSessionIdException(string message, Exception innerException): base(message, innerException)
		{
		}
	}
}

