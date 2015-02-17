using Ninject;
using Ninject.Modules;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersusKiosk.Domain.IoC
{
	public class Injector
	{
		private StandardKernel Kernel { get; set; }

		public void Init(params INinjectModule[] modules)
		{
			Kernel = new StandardKernel(modules);
		}

		public T Get<T>()
		{
			return Kernel.Get<T>();
		}

		public T Get<T>(params IParameter[] parameters)
		{
			return Kernel.Get<T>(parameters);
		}

		public object Get(Type type)
		{
			try
			{
				return Kernel.Get(type);
			}
			catch (Exception e)
			{
				Debug.Assert(false, e.Message);
				throw e;
			}
		}

		public void Inject(params IParameter[] parameters)
		{
			Kernel.Inject(parameters);
		}

		public void Inject(object instance, params IParameter[] parameters)
		{
			Kernel.Inject(instance, parameters);
		}

		public void Load(params INinjectModule[] modules)
		{
			Kernel.Load(modules);
		}

	}
}
