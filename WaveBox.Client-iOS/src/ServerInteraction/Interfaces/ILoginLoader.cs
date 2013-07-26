using System;
using System.Threading.Tasks;

namespace WaveBox.Client.ServerInteraction
{
	public interface ILoginLoader
	{
		event LoginEventHandler LoginCompleted;

		Task Login();
	}
}

