using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Common.DependencyContainer {
    class NoDependencyClass {
        NoDependencyClass() {

        }
        internal string GetName() {
            return nameof(NoDependencyClass);
        }
    }
    class IncompatibleClassA {

    }
    class IncompatibleClassB {

    }
    abstract class CompatibleAbstractClass {
        internal abstract string GetName();
    }
    class CompatibleConcreteClass : CompatibleAbstractClass {
        internal override string GetName() {
            return nameof(CompatibleConcreteClass);
        }
    }

    [Parallelizable(ParallelScope.Children)]
    public class Tests {
        [SetUp]
        public void Setup() {
        }

        [Test]
        public void Register_TwoSameConcreteClasses_Pass() {
            Peanut.Libs.Specialized.DependencyContainer dependencyContainer = new();
            dependencyContainer.Register<NoDependencyClass, NoDependencyClass>();
            var aClass = dependencyContainer.Resolve<NoDependencyClass>();
            Assert.AreEqual("NoDependencyClass", aClass.GetName());
            Assert.Pass();
        }

        [Test]
        public void Register_TwoAbstractClasses_Throw() {
            Peanut.Libs.Specialized.DependencyContainer dependencyContainer = new();
            Assert.Throws<ArgumentException>(() => {
                dependencyContainer.Register<CompatibleAbstractClass, CompatibleAbstractClass>();
            });
        }

        [Test]
        public void RegisterTwice_Throws() {
            Peanut.Libs.Specialized.DependencyContainer dependencyContainer = new();
            dependencyContainer.Register<NoDependencyClass, NoDependencyClass>();
            Assert.Throws<AggregateException>(() => {
                dependencyContainer.Register<NoDependencyClass, NoDependencyClass>();
            });
        }

        [Test]
        public void CreateConcreteInstanceWithoutRegistering_Pass() {
            Peanut.Libs.Specialized.DependencyContainer dependencyContainer = new();
            var aClass = dependencyContainer.CreateInstance<NoDependencyClass>();
            Assert.AreEqual("NoDependencyClass", aClass.GetName());
            Assert.Pass();
        }

        [Test]
        public void CreateConcreteInstanceWithoutRegisteringTwice_Pass() {
            Peanut.Libs.Specialized.DependencyContainer dependencyContainer = new();
            var aClass = dependencyContainer.CreateInstance<NoDependencyClass>();
            var yetAnotherClass = dependencyContainer.CreateInstance<NoDependencyClass>();
            Assert.AreEqual("NoDependencyClass", aClass.GetName());
            Assert.AreEqual("NoDependencyClass", yetAnotherClass.GetName());
            Assert.Pass();
        }

        [Test]
        public void RegisterIncompatibleClasses_Throws() {
            Peanut.Libs.Specialized.DependencyContainer dependencyContainer = new();
            Assert.Throws<ArgumentException>(() => {
                dependencyContainer.Register<IncompatibleClassA, IncompatibleClassB>();
            });
            Assert.Throws<ArgumentException>(() => {
                dependencyContainer.Register<IncompatibleClassB, IncompatibleClassA>();
            });
        }

        [Test]
        public void RegisterCompatibleClasses_Pass() {
            Peanut.Libs.Specialized.DependencyContainer dependencyContainer = new();
            dependencyContainer.Register<CompatibleAbstractClass, CompatibleConcreteClass>();
            var aClass = dependencyContainer.Resolve<CompatibleAbstractClass>();
            Assert.AreEqual(nameof(CompatibleConcreteClass), aClass.GetName());
            Assert.Pass();
        }

        [Test]
        public void RegisterCompatibleClasses_ButWrongOrder_Throws() {
            Peanut.Libs.Specialized.DependencyContainer dependencyContainer = new();

            Assert.Throws<ArgumentException>(() => {
                // wrong order
                dependencyContainer.Register<CompatibleConcreteClass, CompatibleAbstractClass>();
            });
        }

        [Test]
        public void RegisterSingleton() {
            Peanut.Libs.Specialized.DependencyContainer dependencyContainer = new();
            dependencyContainer.Register<CompatibleAbstractClass, CompatibleConcreteClass>();
            var anInstance = dependencyContainer.Resolve<CompatibleAbstractClass>();
            var anotherInstance = dependencyContainer.Resolve<CompatibleAbstractClass>();
            Assert.AreNotEqual(anInstance, anotherInstance);

            dependencyContainer = new();
            dependencyContainer.RegisterSingleton<CompatibleAbstractClass, CompatibleConcreteClass>();
            anInstance = dependencyContainer.Resolve<CompatibleAbstractClass>();
            anotherInstance = dependencyContainer.Resolve<CompatibleAbstractClass>();
            Assert.AreEqual(anInstance, anotherInstance);
        }

        [Test]
        public void Unregister() {
            Peanut.Libs.Specialized.DependencyContainer dependencyContainer = new();
            Assert.IsFalse(dependencyContainer.Unregister<CompatibleAbstractClass>());
            dependencyContainer.Register<CompatibleAbstractClass, CompatibleConcreteClass>();
            Assert.IsTrue(dependencyContainer.Unregister<CompatibleAbstractClass>());
        }

        [Test]
        public void CanResolve() {
            Peanut.Libs.Specialized.DependencyContainer dependencyContainer = new();
            Assert.IsFalse(dependencyContainer.CanResolve<CompatibleAbstractClass>());
            dependencyContainer.Register<CompatibleAbstractClass, CompatibleConcreteClass>();
            Assert.IsTrue(dependencyContainer.CanResolve<CompatibleAbstractClass>());
        }
    }
}
