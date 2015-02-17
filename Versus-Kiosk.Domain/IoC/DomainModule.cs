using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersusKiosk.Domain.Network;

namespace VersusKiosk.Domain.IoC
{
	public class DomainModule : NinjectModule
	{
		private Comms Comms = new Comms();

		public DomainModule()
		{
		}

		public override void Load()
		{
			Bind<Player>().ToSelf();
			Bind<Session>().ToSelf();
			Bind<Comms>().ToConstant(this.Comms);
		}
	}
}
