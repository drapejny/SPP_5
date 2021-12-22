using NUnit.Framework;
using DependencyInjection.DependencyConfiguration;
using DependencyInjection.DependencyConfiguration.ImplementationData;
using DependencyInjection.DependencyProvider;
using LifeCycle = DependencyInjection.DependencyConfiguration.ImplementationData.LifeCycle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace DependencyConfigTest
{
    public class Tests
    {
        [Test]
        public void RegisterTest()
        {
            var dependencies = new DependencyConfig();
            dependencies.Register<IInterface, Class>();
            dependencies.Register<IInterface, Class1>();
            dependencies.Register<InnerInterface, InnerClass>();
            bool v1 = dependencies.DependenciesDictionary.ContainsKey(typeof(IInterface));
            bool v2 = dependencies.DependenciesDictionary.ContainsKey(typeof(InnerInterface));
            var implType1 = dependencies.DependenciesDictionary[typeof(IInterface)][0].ImplementationsType;
            var implType2 = dependencies.DependenciesDictionary[typeof(IInterface)][1].ImplementationsType;
            int count = dependencies.DependenciesDictionary.Keys.Count;
            Assert.IsTrue(v1);
            Assert.IsTrue(v2);
            Assert.AreEqual(implType1, typeof(Class));
            Assert.AreEqual(implType2, typeof(Class1));
            Assert.AreEqual(count, 2);
        }

        [Test]
        public void FailedRegistrationTest()
        {
            var dependencies = new DependencyConfig();
            Assert.Throws<System.ArgumentException>(() => dependencies.Register(typeof(IInterface), typeof(SingleClass), LifeCycle.InstancePerDependency, ImplNumber.First));
        }

        [Test]
        public void GenericTest()
        {
            var dependencies = new DependencyConfig();
            var provider = new DependencyProvider(dependencies);
            dependencies.Register<IInterface, Class1>();
            dependencies.Register<IService<IInterface>, ServiceImpl<IInterface>>();
            int count = dependencies.DependenciesDictionary.Keys.Count;
            var impleType1 = dependencies.DependenciesDictionary[typeof(IInterface)][0].ImplementationsType;
            var impleType2 = dependencies.DependenciesDictionary[typeof(IService<IInterface>)][0].ImplementationsType;
            Assert.AreEqual(impleType1, typeof(Class1));
            Assert.AreEqual(impleType2, typeof(ServiceImpl<IInterface>));
            Assert.AreEqual(2, count);
        }

        [Test]
        public void EnumerableTest()
        {
            var dependencies = new DependencyConfig();
            var provider = new DependencyProvider(dependencies);
            dependencies.Register<IInterface, Class>();
            dependencies.Register<IInterface, Class1>();
            dependencies.Register<InnerInterface, InnerClass>();
            var list = provider.Resolve<IEnumerable<IInterface>>();
            var implType1 = list.ToList()[0].GetType();
            var implType2 = list.ToList()[1].GetType();
            Assert.AreEqual(implType1, typeof(Class));
            Assert.AreEqual(implType2, typeof(Class1));
        }

        [Test]
        public void ImplNumberTest()
        {
            var dependencies = new DependencyConfig();
            var provider = new DependencyProvider(dependencies);
            dependencies.Register<IInterface, Class>(LifeCycle.InstancePerDependency, ImplNumber.Second);
            dependencies.Register<IInterface, Class1>(LifeCycle.InstancePerDependency, ImplNumber.First);
            dependencies.Register<InnerInterface, InnerClass>();
            var firstResult = provider.Resolve<IInterface>(ImplNumber.First);
            var secondResult = provider.Resolve<IInterface>(ImplNumber.Second);
            Assert.AreEqual(firstResult.GetType(), typeof(Class1));
            Assert.AreEqual(secondResult.GetType(), typeof(Class));
        }

        [Test]
        public void TtlTest()
        {
            var dependencies = new DependencyConfig();
            var provider = new DependencyProvider(dependencies);
            dependencies.Register<IInterface, Class>(LifeCycle.InstancePerDependency, ImplNumber.First);
            dependencies.Register<IInterface, Class1>(LifeCycle.InstancePerDependency, ImplNumber.Second);
            dependencies.Register<InnerInterface, InnerClass>();
            IInterface link1 = provider.Resolve<IInterface>(ImplNumber.First);
            IInterface link2 = provider.Resolve<IInterface>(ImplNumber.First);
            Assert.IsFalse(link1 == link2);
        }

       
        [Test]
        public void SingletoneTest()
        {
           var dependencies = new DependencyConfig();
            var provider = new DependencyProvider(dependencies);
            dependencies.Register<IInterface, Class1>(LifeCycle.Singleton, ImplNumber.First);
            IInterface link1 = provider.Resolve<IInterface>(ImplNumber.First);
            IInterface link2 = provider.Resolve<IInterface>(ImplNumber.First);
            Assert.IsTrue(link1 == link2);
        }


        interface IA
        {
            void met();
        }

        class A : IA
        {
            public IB ib;
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

        class B: IB
        {
            IA ia;
            public B(IA ia)
            {
                this.ia = ia;
            }

            public void met()
            {
                throw new System.NotImplementedException();
            }
        }
        interface IInterface
        {
            void met();
        }

        class Class : IInterface
        {
            public InnerInterface inner;

            public Class(InnerInterface inner)
            {
                this.inner = inner;
            }
            public void met()
            {
                throw new System.NotImplementedException();
            }
        }

        class Class1 : IInterface
        {
            public void met()
            {
                throw new System.NotImplementedException();
            }
        }
        interface InnerInterface
        {
            void met();
        }

        class InnerClass : InnerInterface
        {
            public void met()
            {
                throw new System.NotImplementedException();
            }
        }

        interface IService<T> where T : IInterface
        {

        }

        class ServiceImpl<T> : IService<T> where T : IInterface
        {
            public ServiceImpl(IInterface i)
            {

            }
        }


        class SingleClass
        {

        }
    }
}