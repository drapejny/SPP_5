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


        // Z - X - Z тест
        [Test]
        public void CircularTest1()
        {
            var dependencies = new DependencyConfig();
            var provider = new DependencyProvider(dependencies);
            dependencies.Register<IZ, Z>(LifeCycle.Singleton, ImplNumber.First);
            dependencies.Register<IX, X>(LifeCycle.Singleton, ImplNumber.First);
            Z z = (Z)provider.Resolve<IZ>(ImplNumber.First);
            X x = (X)provider.Resolve<IX>(ImplNumber.First);
            Assert.IsTrue(z.ix.GetType().Equals(typeof(X)));
            Assert.IsTrue(x.iz.GetType().Equals(typeof(Z)));
        }

        //Q - W - E - Q тест
        [Test]
        public void CircularTest2()
        {
            var dependencies = new DependencyConfig();
            var provider = new DependencyProvider(dependencies);
            dependencies.Register<IQ, Q>(LifeCycle.Singleton, ImplNumber.First);
            dependencies.Register<IW, W>(LifeCycle.Singleton, ImplNumber.First);
            dependencies.Register<IE, E>(LifeCycle.Singleton, ImplNumber.First);
            Q q = (Q)provider.Resolve<IQ>(ImplNumber.First);
            W w = (W)provider.Resolve<IW>(ImplNumber.First);
            E e = (E)provider.Resolve<IE>(ImplNumber.Any);
            Assert.IsTrue(q.iw.GetType().Equals(typeof(W)));
            Assert.IsTrue(w.ie.GetType().Equals(typeof(E)));
            Assert.IsTrue(e.iq.GetType().Equals(typeof(Q)));
        }

        //Self - Self тест
        [Test]
        public void CircularTest3()
        {
            var dependencies = new DependencyConfig();
            var provider = new DependencyProvider(dependencies);
            dependencies.Register<ISelf, Self>(LifeCycle.Singleton, ImplNumber.First);
            Self self = (Self)provider.Resolve<ISelf>(ImplNumber.First);
            Assert.IsTrue(self.iself.GetType().Equals(typeof(Self)));
        }

        interface ISelf
        {
            void met();
        }

        class Self : ISelf
        {
            public ISelf iself { get; set; }
            public Self (ISelf self)
            {
                this.iself = self;
            }
            public void met()
            {
                throw new System.NotImplementedException();
            }
        }
        interface IZ
        {
            void met();
        }

        class Z : IZ
        {
            public IX ix { get; set; }
            public Z(IX ix)
            {
                this.ix = ix;
            }
            public void met()
            {
                throw new System.NotImplementedException();
            }
        }

        interface IX
        {
            void met();
        }

        class X : IX
        {
            public IZ iz { get; set; }
            public X(IZ iz)
            {
                this.iz = iz;
            }
            public void met()
            {
                throw new System.NotImplementedException();
            }
        }

        interface IQ
        {
            void met();
        }

        class Q : IQ
        {
            public IW iw { get; set; }
            public Q(IW iw)
            {
                this.iw = iw;
            }

            public void met()
            {
                throw new System.NotImplementedException();
            }
        }

        interface IW
        {
            void met();
        }

        class W : IW
        {
            public IE ie { get; set; }
            public W(IE ie)
            {
                this.ie = ie;
            }

            public void met()
            {
                throw new System.NotImplementedException();
            }
        }

        interface IE
        {
            void met();
        }

        class E : IE
        {
            public IQ iq { get; set; }
            public E(IQ iq)
            {
                this.iq = iq;
            }

            public void met()
            {
                throw new System.NotImplementedException();
            }
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