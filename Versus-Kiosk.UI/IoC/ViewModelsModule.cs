﻿using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersusKiosk.Domain.IoC;
using VersusKiosk.UI.Main;
using VersusKiosk.UI.Pages;

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
			
			// pages
			Bind<IntroViewModel>().ToSelf();
			Bind<AdminLoginViewModel>().ToSelf();
			Bind<AdminViewModel>().ToSelf();
			Bind<PlayerDetailsViewModel>().ToSelf();
			Bind<ChooseWorkoutViewModel>().ToSelf();
			Bind<ScanViewModel>().ToSelf();
			Bind<EnterEmailViewModel>().ToSelf();
			Bind<StartViewModel>().ToSelf();

			// dialogs
			Bind<StationActionViewModel>().ToSelf();
		}
	}
}
