
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
           // dependencies.Register<IA, A>(LifeCycle.Singleton, ImplNumber.First);
            dependencies.Register<IB, B>(LifeCycle.Singleton, ImplNumber.First);
            dependencies.Register<IC, C>(LifeCycle.Singleton, ImplNumber.First);
            //A a = (A)provider.Resolve<IA>(ImplNumber.First);
            //B b = (B)provider.Resolve<IB>(ImplNumber.First);
            //C c = (C)provider.Resolve<IC>(ImplNumber.First);
            A a = (A)provider.Resolve<A>(ImplNumber.First);
            Console.WriteLine(a.ib);
            Console.WriteLine(a.ic);

            //Console.WriteLine(a.ib);
            //Console.WriteLine(a.ic);
            //Console.WriteLine(b.ic);
            //Console.WriteLine(c.ia);


        }
    }

    interface IA
    {
        void met();
    }

    class A : IA
    {
        public IB ib { get; set; }
        public IC ic { get; set; }
        public A(IB ib, IC ic)
        {
            this.ic = ic;
            this.ib = ib;
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
        public IC ic { get; set; }
        public B(IC ic)
        {
            this.ic = ic;
        }

        public void met()
        {
            throw new System.NotImplementedException();
        }
    }

    interface IC
    {
        void met();
    }

    class C : IC
    {
        public IA ia { get; set; }
        public C(IA ia)
        {
            this.ia = ia;
        }

        public void met()
        {
            throw new System.NotImplementedException();
        }
    }
}