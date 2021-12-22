
using DependencyInjection.DependencyConfiguration;
using DependencyInjection.DependencyConfiguration.ImplementationData;
using DependencyInjection.DependencyProvider;
using LifeCycle = DependencyInjection.DependencyConfiguration.ImplementationData.LifeCycle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System;
namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var dependencies = new DependencyConfig();
            var provider = new DependencyProvider(dependencies);
            dependencies.Register<IA, A>(LifeCycle.Singleton, ImplNumber.First);
            dependencies.Register<IB, B>(LifeCycle.Singleton, ImplNumber.First);
            A a = (A) provider.Resolve<IA>(ImplNumber.First);
            B b = (B)provider.Resolve<IB>(ImplNumber.First);

        }
    }

    interface IA
    {
        void met();
    }

    class A : IA
    {
        public IB ib { get; set; }
        public A(IB iB)
        {
            this.ib = iB;
        }

        public void met()
        {
            throw new System.NotImplementedException();
        }
    }

    interface IB
    {
        void met();
    }

    class B : IB
    {
        public IA ia { get; set; }
        public B(IA ia)
        {
            this.ia = ia;
        }

        public void met()
        {
            throw new System.NotImplementedException();
        }
    }
}
