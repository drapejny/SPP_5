using NUnit.Framework;
using DependencyInjection.DependencyConfiguration;
using DependencyInjection.DependencyConfiguration.ImplementationData;
using DependencyInjection.DependencyProvider;
using LifeCycle = DependencyInjection.DependencyConfiguration.ImplementationData.LifeCycle;
using System.Collections;
using System.Collections.Generic;

namespace TestProject1
{
    class DependencyValidatorTest
    {

        [Test]
        public void correctDependencyConfigTest()
        {
            var dependencies = new DependencyConfig();
            dependencies.Register<IInterface, Class>();
            dependencies.Register<InnerInterface, InnerClass>();
            var validator = new ConfigValidator(dependencies);
            bool actual = validator.Validate();
            Assert.IsTrue(actual);
        }

        [Test]
        public void incorrectDependencyConfigTest()
        {
            var dependencies = new DependencyConfig();
            dependencies.Register<IInterface, Class>();
            var validator = new ConfigValidator(dependencies);
            bool actual = validator.Validate();
            Assert.IsFalse(actual);
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
    }
}
