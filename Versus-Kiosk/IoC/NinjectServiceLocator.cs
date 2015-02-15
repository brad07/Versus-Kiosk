using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersusKiosk.Domain.IoC;
using VersusKiosk.UI.IoC;
using VersusKiosk.UI.Main;

namespace VersusKiosk.IoC
{
	public class NinjectServiceLocator
	{
		private Injector Injector;

		public NinjectServiceLocator()
		{
			//Injector.Init(new RepositoriesModule(), new ServicesModule(), new NHibernateModule(), new ViewModelsModule());
			this.Injector = new Injector();
			this.Injector.Init(new ViewModelsModule(this.Injector), new DomainModule());
		}

		// can this be obtained from the injector directly?
		public MainViewModel Main { get { return this.Injector.Get<MainViewModel>(); } }
	}
}
