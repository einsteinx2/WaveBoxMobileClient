using System;
using Ninject.Modules;
using WaveBox.Core;
using WaveBox.Client;

namespace Wave.iOS
{
	public class iOSModule : NinjectModule
	{
		public override void Load() 
		{
			// Repositories
			Bind<IDatabase>().To<Database>().InSingletonScope();
			Bind<IClientDatabase>().To<Database>().InSingletonScope();
			Bind<IClientPlatformSettings>().To<iOSPlatformSettings>().InSingletonScope();
		}
	}
}
