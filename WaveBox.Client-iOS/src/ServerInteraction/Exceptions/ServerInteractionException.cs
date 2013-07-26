using System;

namespace WaveBox.Client
{
	public class ServerInteractionException : Exception
	{
		public ServerInteractionException()
		{
		}

		public ServerInteractionException(string message): base(message)
		{
		}

		public ServerInteractionException(string message, Exception innerException): base(message, innerException)
		{
		}
	}
}

