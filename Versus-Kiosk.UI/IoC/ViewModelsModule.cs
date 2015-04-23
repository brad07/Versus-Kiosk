using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersusKiosk.Domain.IoC;
using VersusKiosk.UI.Hardware;
using VersusKiosk.UI.Main;
using VersusKiosk.UI.Pages;
using VersusKiosk.UI.SetUp;

namespace VersusKiosk.UI.IoC
{
	public class ViewModelsModule : NinjectModule
	{
		private Injector Injector;

		public ViewModelsModule(Injector injector)
		{
			this.Injector = injector;
		}

		public override void Load()
		{
			Bind<Injector>().ToConstant(this.Injector);
			Bind<MainViewModel>().ToSelf().InSingletonScope();
			Bind<Scales>().ToSelf().InSingletonScope();
			
			// pages
			Bind<IntroViewModel>().ToSelf();
			Bind<AdminLoginViewModel>().ToSelf();
			Bind<AdminViewModel>().ToSelf();
			Bind<PlayerDetailsViewModel>().ToSelf();
			Bind<NicknameViewModel>().ToSelf();
			Bind<ChooseWorkoutViewModel>().ToSelf();
			Bind<ScanViewModel>().ToSelf();
			Bind<EnterEmailViewModel>().ToSelf();
			Bind<StartViewModel>().ToSelf();

			// dialogs
			Bind<StationActionViewModel>().ToSelf();
			Bind<InitialSetupViewModel>().ToSelf();
		}
	}
}
