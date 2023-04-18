using NUnit.Framework;
using Peanut.Libs.Specialized.Reflection;
using System;
using System.Reflection;

namespace UnitTest.Common.CachedDelegate {
    public class BaseClass {

    }
    public class SubClass1 : BaseClass {

    }
    public struct CustomStruct {
        public void Operation1() {

        }
        public string Operation2() {
            return "hello";
        }
        public void Operation3(string a) { }
        public string Operation4(string a) { return a; }
    }
    public class CustomClass {
        public static void StaticVoid() {

        }
        public void InstanceVoid() {
            Console.WriteLine("Write something");
        }
        public static string StaticReturn() {
            return "b";
        }
        public string InstanceReturn() {
            return "a";
        }
        public static void StaticVoidParameter(string a) {
            Console.WriteLine(a);
        }
        public void InstanceVoidParameter(string a) {
            Console.WriteLine(a);
        }
        public static string StaticReturnParameter(string a) {
            return a;
        }
        public string InstanceReturnParameter(string a) {
            return a;
        }
        public static BaseClass StaticReturnBase() {
            return new();
        }
        public static SubClass1 StaticReturnSubClass() {
            return new();
        }
        public static void StaticVoidParamBase(BaseClass baseClass) {

        }
        public static void StaticVoidParamSubClass(SubClass1 subClass) {

        }
    }
    public class AnotherClass { }
    public class ThrowTests {
        private readonly Type t = typeof(CustomClass);

        [Test]
        public void CreateStaticVoidThrows() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.InstanceVoid));
            Assert.Throws<ArgumentException>(() => method.CreateStaticVoid());

            method = t.GetMethod(nameof(CustomClass.StaticReturn));
            Assert.Throws<ArgumentException>(() => method.CreateStaticVoid());

            method = t.GetMethod(nameof(CustomClass.StaticVoidParameter));
            Assert.Throws<ArgumentException>(() => method.CreateStaticVoid());

            method = t.GetMethod(nameof(CustomClass.StaticVoid));
            Assert.DoesNotThrow(() => method.CreateStaticVoid());
        }

        [Test]
        public void CreateInstanceVoidThrows() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.StaticVoid));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceVoid<CustomClass>());

            method = t.GetMethod(nameof(CustomClass.InstanceReturn));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceVoid<CustomClass>());

            method = t.GetMethod(nameof(CustomClass.InstanceVoidParameter));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceVoid<CustomClass>());

            method = t.GetMethod(nameof(CustomClass.InstanceVoid));
            Assert.DoesNotThrow(() => method.CreateInstanceVoid<CustomClass>());
        }

        [Test]
        public void CreateStaticReturnThrows() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.InstanceReturn));
            Assert.Throws<ArgumentException>(() => method.CreateStaticReturn<string>());

            method = t.GetMethod(nameof(CustomClass.StaticVoid));
            Assert.Throws<ArgumentException>(() => method.CreateStaticReturn<string>());

            method = t.GetMethod(nameof(CustomClass.StaticReturnParameter));
            Assert.Throws<ArgumentException>(() => method.CreateStaticReturn<string>());

            method = t.GetMethod(nameof(CustomClass.StaticReturn));
            Assert.DoesNotThrow(() => method.CreateStaticReturn<string>());
        }

        [Test]
        public void CreateInstaceReturnThrows() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.StaticReturn));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceReturn<CustomClass, string>());

            method = t.GetMethod(nameof(CustomClass.InstanceVoid));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceReturn<CustomClass, string>());

            method = t.GetMethod(nameof(CustomClass.InstanceReturnParameter));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceReturn<CustomClass, string>());

            method = t.GetMethod(nameof(CustomClass.InstanceReturn));
            Assert.DoesNotThrow(() => method.CreateInstanceReturn<CustomClass, string>());
        }

        [Test]
        public void CreateStaticVoidParamThrows() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.InstanceVoid));
            Assert.Throws<ArgumentException>(() => method.CreateStaticVoidParam<string>());

            method = t.GetMethod(nameof(CustomClass.StaticReturn));
            Assert.Throws<ArgumentException>(() => method.CreateStaticVoidParam<string>());

            method = t.GetMethod(nameof(CustomClass.StaticReturnParameter));
            Assert.Throws<ArgumentException>(() => method.CreateStaticVoidParam<string>());

            method = t.GetMethod(nameof(CustomClass.StaticVoidParameter));
            Assert.DoesNotThrow(() => method.CreateStaticVoidParam<string>());
        }

        [Test]
        public void CreateInstanceVoidParamThrows() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.StaticVoid));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceVoidParam<CustomClass, string>());

            method = t.GetMethod(nameof(CustomClass.InstanceReturn));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceVoidParam<CustomClass, string>());

            method = t.GetMethod(nameof(CustomClass.InstanceReturn));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceVoidParam<CustomClass, string>());

            method = t.GetMethod(nameof(CustomClass.InstanceVoidParameter));
            Assert.DoesNotThrow(() => method.CreateInstanceVoidParam<CustomClass, string>());
        }

        [Test]
        public void CreateStaticReturnParamThrows() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.InstanceReturn));
            Assert.Throws<ArgumentException>(() => method.CreateStaticReturnParam<string, string>());

            method = t.GetMethod(nameof(CustomClass.StaticVoid));
            Assert.Throws<ArgumentException>(() => method.CreateStaticReturnParam<string, string>());

            method = t.GetMethod(nameof(CustomClass.StaticVoid));
            Assert.Throws<ArgumentException>(() => method.CreateStaticReturnParam<string, string>());

            method = t.GetMethod(nameof(CustomClass.StaticReturnParameter));
            Assert.DoesNotThrow(() => method.CreateStaticReturnParam<string, string>());
        }

        [Test]
        public void CreateInstanceReturnParamThrows() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.StaticReturn));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceReturnParam<CustomClass, string, string>());

            method = t.GetMethod(nameof(CustomClass.InstanceVoid));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceReturnParam<CustomClass, string, string>());

            method = t.GetMethod(nameof(CustomClass.InstanceReturn));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceReturnParam<CustomClass, string, string>());

            method = t.GetMethod(nameof(CustomClass.InstanceReturnParameter));
            Assert.DoesNotThrow(() => method.CreateInstanceReturnParam<CustomClass, string, string>());
        }

        [Test]
        public void CreateInstanceVoidWeak() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.InstanceVoid));
            Action<object> action = method.CreateInstanceVoid(t);
            CustomClass aClass = new();
            AnotherClass anotherClass = new();
            Assert.DoesNotThrow(() => action(aClass));
            Assert.Throws<InvalidCastException>(() => action(anotherClass));
        }

        [Test]
        public void CreateInstanceReturnWeak() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.InstanceReturn));
            Func<object, string> action = method.CreateInstanceReturn<string>(t);
            CustomClass aClass = new();
            AnotherClass anotherClass = new();
            Assert.DoesNotThrow(() => action(aClass));
            Assert.Throws<InvalidCastException>(() => action(anotherClass));
        }

        [Test]
        public void CreateInstanceVoidParamWeak() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.InstanceVoidParameter));
            Action<object, string> action = method.CreateInstanceVoidParam<string>(t);
            CustomClass aClass = new();
            AnotherClass anotherClass = new();
            Assert.DoesNotThrow(() => action(aClass, "hello"));
            Assert.Throws<InvalidCastException>(() => action(anotherClass, "hell"));
        }

        [Test]
        public void CreateInstanceReturnParamWeak() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.InstanceReturnParameter));
            Func<object, string, string> action = method.CreateInstanceReturnParam<string, string>(t);
            CustomClass aClass = new();
            AnotherClass anotherClass = new();
            Assert.DoesNotThrow(() => action(aClass, "abc"));
            Assert.Throws<InvalidCastException>(() => action(anotherClass, "abc"));
        }

        [Test]
        public void WrongReturnTypeThrows() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.InstanceReturn));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceReturnParam<CustomClass, string, int>());
        }

        [Test]
        public void NotCompatibleReturnTypeThrows() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.StaticReturnBase));
            Assert.Throws<ArgumentException>(() => method.CreateStaticReturn<SubClass1>());

            method = t.GetMethod(nameof(CustomClass.StaticReturnSubClass));
            Assert.DoesNotThrow(() => method.CreateStaticReturn<BaseClass>());
        }

        [Test]
        public void WrongParameterTypeThrows() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.InstanceReturnParameter));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceReturnParam<CustomClass, int, string>());
        }

        [Test]
        public void NotCompatibleParamTypeThrows() {
            MethodInfo method = t.GetMethod(nameof(CustomClass.StaticVoidParamBase));
            Assert.DoesNotThrow(() => method.CreateStaticVoidParam<SubClass1>());

            method = t.GetMethod(nameof(CustomClass.StaticVoidParamSubClass));
            Assert.Throws<ArgumentException>(() => method.CreateStaticVoidParam<BaseClass>());
        }

        [Test]
        public void WeakledTypedDelegateStructThrows() {
            Type structType = typeof(CustomStruct);
            MethodInfo method = structType.GetMethod(nameof(CustomStruct.Operation1));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceVoid(structType));

            method = structType.GetMethod(nameof(CustomStruct.Operation2));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceReturn<string>(structType));

            method = structType.GetMethod(nameof(CustomStruct.Operation3));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceVoidParam<string>(structType));

            method = structType.GetMethod(nameof(CustomStruct.Operation4));
            Assert.Throws<ArgumentException>(() => method.CreateInstanceReturnParam<string, string>(structType));
        }
    }

    public class SimpleTests {
    }
}
