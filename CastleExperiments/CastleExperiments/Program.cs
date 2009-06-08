using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Castle.Windsor;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;

namespace CastleExperiments
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new WindsorContainer();
            container.Register(Component.For<IService>().ImplementedBy<Service>());
            container.Register(Component.For<ISomething>().ImplementedBy<Something>());

            var dict = new Dictionary<string, object>();
            dict.Add("hooHaa", "hey ho ");

            container.Resolve<IService>(dict);

            var svc = container.Resolve<IService>(new { hooHaa = "blahblah" });

            svc.Thingy("ha ha");

            Console.Read();
        }
    }

    public static class IoC
    {
        private static IWindsorContainer _container = new WindsorContainer();

        public static T Resolve<T>()
        {
            return _container.Resolve<T>();
        }
    }



    public class Service : IService
    {
        private string _stuff;
        private ISomething _something;

        public Service()
        {
            _stuff = "default";
        }

        public Service(string hooHaa, IKernel kernel)
        {
            _stuff = hooHaa;
            _something = kernel.Resolve<ISomething>(new { name = hooHaa });
        }


        public virtual void Thingy(string name)
        {
            Console.WriteLine(name);
            Console.WriteLine(_something.Hey());
        }
    }

    public class Something : ISomething
    {
        private string _name;
        public Something(string name)
        {
            _name = name;
        }

        public string Hey()
        {
            return _name;
        }
    }


}
