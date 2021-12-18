using System;
using System.Reflection;

namespace Peanut.Libs.Specialized.Reflection {
    /// <summary>
    /// Helps creating open delegates for high performance method invocation.<br/>
    /// Open delegates do not root the target object so the object can be garbage collected.<br/>
    /// </summary>
    public static class OpenDelegateCreator {
        /// <summary>
        /// Creates an open delegate which represents a method that has no parameters and
        /// returns void.<br/>
        /// The <paramref name="method"/> needs to be a static method.<br/>
        /// </summary>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        public static Action CreateStaticVoid(this MethodInfo method) {
            method.AssertStaticReturnParameters(true, typeof(void), 0);
            return (Action)method.CreateDelegate(typeof(Action));
        }

        /// <summary>
        /// Creates an open delegate which represents a method that has no parameters and
        /// returns void.<br/>
        /// The <paramref name="method"/> needs to be an instance method.<br/>
        /// </summary>
        /// <typeparam name="TTarget">
        ///     The type that contains the <paramref name="method"/>.
        /// </typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        public static Action<TTarget> CreateInstanceVoid<TTarget>(this MethodInfo method)
            where TTarget : class {
            method.AssertStaticReturnParameters(false, typeof(void), 0);
            return (Action<TTarget>)method.CreateDelegate(typeof(Action<TTarget>), null);
        }

        /// <summary>
        /// Creates an open delegate which represents a method that has no parameters and
        /// returns void.<br/>
        /// The <paramref name="method"/> needs to be an instance method.<br/>
        /// This is another version of the <see cref="CreateInstanceVoid{TTarget}(MethodInfo)"/>
        /// method which is helpful when we do not know ahead of time what the target type is
        /// and we cannot use the generic method.<br/>
        /// </summary>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <param name="targetType">The type of the target object.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        public static Action<object> CreateInstanceVoid(this MethodInfo method, Type targetType) {
            targetType.AssertTargetIsClass(true);
#nullable disable
            MethodInfo genericMethod = typeof(OpenDelegateCreator).GetMethod(
                nameof(BoxCreateInstanceVoid), BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo constructedGenericMethod = genericMethod.MakeGenericMethod(targetType);
            object @delegate = constructedGenericMethod.Invoke(null, new object[] { method });
            return (Action<object>)@delegate;
#nullable enable
        }

        /// <summary>
        /// Helper method for <see cref="CreateInstanceVoid(MethodInfo, Type)"/>.<br/>
        /// This method will take the strongly typed delegate from the
        /// <see cref="CreateInstanceVoid{TTarget}(MethodInfo)"/> method and turn it into a
        /// weakly typed delegate.<br/>
        /// This method is meant to be invoked via reflection because it still has a type
        /// parameter.<br/>
        /// The <paramref name="method"/> needs to be an instance method.<br/>
        /// </summary>
        /// <typeparam name="TTarget">
        ///     The type that contains the <paramref name="method"/>.
        /// </typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        private static Action<object> BoxCreateInstanceVoid<TTarget>(this MethodInfo method)
            where TTarget : class {
            Action<TTarget> @delegate = method.CreateInstanceVoid<TTarget>();
            void boxedDelegate(object target) => @delegate((TTarget)target);
            return boxedDelegate;
        }

        /// <summary>
        /// Creates an open delegate which represents a method that has no parameters and
        /// returns <typeparamref name="TReturn"/>.<br/>
        /// The <paramref name="method"/> needs to be a static method.<br/>
        /// </summary>
        /// <typeparam name="TReturn">The return type of the <paramref name="method"/>.</typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        public static Func<TReturn> CreateStaticReturn<TReturn>(this MethodInfo method) {
            method.AssertStaticReturnParameters(true, typeof(TReturn), 0);
            return (Func<TReturn>)method.CreateDelegate(typeof(Func<TReturn>));
        }

        /// <summary>
        /// Creates an open delegate which represents a method that has no parameters and
        /// returns <typeparamref name="TReturn"/>.<br/>
        /// The <paramref name="method"/> needs to be an instance method.<br/>
        /// </summary>
        /// <typeparam name="TTarget">
        ///     The type that contains the <paramref name="method"/>.
        /// </typeparam>
        /// <typeparam name="TReturn">The return type of the <paramref name="method"/>.</typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        public static Func<TTarget, TReturn> CreateInstanceReturn<TTarget, TReturn>(
            this MethodInfo method) where TTarget : class {
            method.AssertStaticReturnParameters(false, typeof(TReturn), 0);
            return (Func<TTarget, TReturn>)method.CreateDelegate(
                typeof(Func<TTarget, TReturn>), null);
        }

        /// <summary>
        /// Creates an open delegate which represents a method that has no parameters and
        /// returns <typeparamref name="TReturn"/>.<br/>
        /// The <paramref name="method"/> needs to be an instance method.<br/>
        /// This is another version of the
        /// <see cref="CreateInstanceReturn{TTarget, TReturn}(MethodInfo)"/> method
        /// method which is helpful when we do not know ahead of time what the target type is
        /// and we cannot use the generic method. However, this method still requires that
        /// we do know ahead of time the return type.<br/>
        /// </summary>
        /// <typeparam name="TReturn">The return type of the <paramref name="method"/>.</typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <param name="targetType">The type of the target object.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        public static Func<object, TReturn> CreateInstanceReturn<TReturn>(
            this MethodInfo method, Type targetType) {
            targetType.AssertTargetIsClass(true);
#nullable disable
            MethodInfo genericMethod = typeof(OpenDelegateCreator).GetMethod(
                nameof(BoxCreateInstanceReturn), BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo constructedGenericMethod =
                genericMethod.MakeGenericMethod(targetType, typeof(TReturn));
            object @delegate = constructedGenericMethod.Invoke(null, new object[] { method });
            return (Func<object, TReturn>)@delegate;
#nullable enable
        }

        /// <summary>
        /// Helper method for <see cref="CreateInstanceReturn{TReturn}(MethodInfo, Type)"/>.<br/>
        /// This method will take the strongly typed delegate from the
        /// <see cref="CreateInstanceReturn{TTarget, TReturn}(MethodInfo)"/> method and turn it into
        /// a weakly typed delegate.<br/>
        /// This method is meant to be invoked via reflection because it still has type
        /// parameters.<br/>
        /// The <paramref name="method"/> needs to be an instance method.<br/>
        /// </summary>
        /// <typeparam name="TTarget">
        ///     The type that contains the <paramref name="method"/>.
        /// </typeparam>
        /// <typeparam name="TReturn">The return type of the <paramref name="method"/>.</typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        private static Func<object, TReturn> BoxCreateInstanceReturn<TTarget, TReturn>(
            this MethodInfo method) where TTarget : class {
            Func<TTarget, TReturn> @delegate = method.CreateInstanceReturn<TTarget, TReturn>();
            TReturn boxedDelegate(object target) => @delegate((TTarget)target);
            return boxedDelegate;
        }

        #region parametered methods.
        // Each amount of parameters will require implementing 8 different methods to cover
        // all the use cases.
        // 1. A strongly typed delegate for a static method that returns void.
        // 2. A strongly typed delegate for an instance method that returns void.
        // 3. A weakly typed delegate for an instance method that returns void.
        // 4. A helper method to create the weakly typed delegate.
        // 5. A strongly typed delegate for static method that returns a value.
        // 6. A strongly typed delegate for an instance method that returns a value.
        // 7. A weakly typed delegate for an instance method that returns a value.
        // 8. A healper method to create the weakly typed delegate.

        /// <summary>
        /// Creates an open delegate which represents a method that requires 1 parameter and
        /// returns void.<br/>
        /// The <paramref name="method"/> needs to be a static method.<br/>
        /// </summary>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        // 1. A strongly typed delegate for a static method that returns void.
        public static Action<TParam> CreateStaticVoidParam<TParam>(this MethodInfo method) {
            method.AssertStaticReturnParameters(true, typeof(void), 1);
            method.GetParameters()[0].AssertParameterType(typeof(TParam));
            return (Action<TParam>)method.CreateDelegate(typeof(Action<TParam>));
        }

        /// <summary>
        /// Creates an open delegate which represents a method that requires 1 parameter and
        /// returns void.<br/>
        /// The <paramref name="method"/> needs to be an instance method.<br/>
        /// </summary>
        /// <typeparam name="TTarget">
        ///     The type that contains the <paramref name="method"/>.
        /// </typeparam>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        // 2. A strongly typed delegate for an instance method that returns void.
        public static Action<TTarget, TParam> CreateInstanceVoidParam<TTarget, TParam>(
            this MethodInfo method) where TTarget : class {
            method.AssertStaticReturnParameters(false, typeof(void), 1);
            method.GetParameters()[0].AssertParameterType(typeof(TParam));
            return (Action<TTarget, TParam>)method.CreateDelegate(
                typeof(Action<TTarget, TParam>), null);
        }

        /// <summary>
        /// Creates an open delegate which represents a method that requires 1 parameter and
        /// returns void.<br/>
        /// The <paramref name="method"/> needs to be an instance method.<br/>
        /// This is another version of the
        /// <see cref="CreateInstanceVoidParam{TTarget, TParam}(MethodInfo)"/> method which is
        /// helpful when we do not know ahead of time what the target type is and we cannot use
        /// the generic method. However, this method still requires that we do know ahead of time
        /// the parameter type.<br/>
        /// </summary>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <param name="targetType">The type of the target object.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        // 3. A weakly typed delegate for an instance method that returns void.
        public static Action<object, TParam> CreateInstanceVoidParam<TParam>(
            this MethodInfo method, Type targetType) {
            targetType.AssertTargetIsClass(true);
#nullable disable
            MethodInfo genericMethod = typeof(OpenDelegateCreator).GetMethod(
                nameof(BoxCreateInstanceVoidParam), BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo constructedGenericMethod =
                genericMethod.MakeGenericMethod(targetType, typeof(TParam));
            object @delegate = constructedGenericMethod.Invoke(null, new object[] { method });
            return (Action<object, TParam>)@delegate;
#nullable enable
        }

        /// <summary>
        /// Helper method for <see cref="CreateInstanceVoidParam{TParam}(MethodInfo, Type)"/>.<br/>
        /// This method will take the strongly typed delegate from the
        /// <see cref="CreateInstanceVoidParam{TTarget, TParam}(MethodInfo)"/> method and turn it
        /// into a weakly typed delegate.<br/>
        /// This method is meant to be invoked via reflection because it still has type
        /// parameters.<br/>
        /// The <paramref name="method"/> needs to be an instance method.<br/>
        /// </summary>
        /// <typeparam name="TTarget">
        ///     The type that contains the <paramref name="method"/>.
        /// </typeparam>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        // 4. A helper method to create the weakly typed delegate.
        private static Action<object, TParam> BoxCreateInstanceVoidParam<TTarget, TParam>(
            this MethodInfo method) where TTarget : class {
            Action<TTarget, TParam> @delegate = method.CreateInstanceVoidParam<TTarget, TParam>();
            void boxedDelegate(object target, TParam param) => @delegate((TTarget)target, param);
            return boxedDelegate;
        }

        /// <summary>
        /// Creates an open delegate which represents a method that requires 1 parameter and
        /// returns <typeparamref name="TReturn"/>.<br/>
        /// The <paramref name="method"/> needs to be a static method.<br/>
        /// </summary>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <typeparam name="TReturn">The return type of the <paramref name="method"/>.</typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        // 5. A strongly typed delegate for static method that returns a value.
        public static Func<TParam, TReturn> CreateStaticReturnParam<TParam, TReturn>(this MethodInfo method) {
            method.AssertStaticReturnParameters(true, typeof(TReturn), 1);
            method.GetParameters()[0].AssertParameterType(typeof(TParam));
            return (Func<TParam, TReturn>)method.CreateDelegate(typeof(Func<TParam, TReturn>));
        }

        /// <summary>
        /// Creates an open delegate which represents a method that requires 1 parameter and
        /// returns <typeparamref name="TReturn"/>.<br/>
        /// The <paramref name="method"/> needs to be an instance method.<br/>
        /// </summary>
        /// <typeparam name="TTarget">
        ///     The type that contains the <paramref name="method"/>.
        /// </typeparam>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <typeparam name="TReturn">The return type of the <paramref name="method"/>.</typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        // 6. A strongly typed delegate for an instance method that returns a value.
        public static Func<TTarget, TParam, TReturn> CreateInstanceReturnParam<TTarget, TParam, TReturn>(
            this MethodInfo method) where TTarget : class {
            method.AssertStaticReturnParameters(false, typeof(TReturn), 1);
            method.GetParameters()[0].AssertParameterType(typeof(TParam));
            return (Func<TTarget, TParam, TReturn>)method.CreateDelegate(
                typeof(Func<TTarget, TParam, TReturn>), null);
        }

        /// <summary>
        /// Creates an open delegate which represents a method that requires 1 parameter and
        /// returns <typeparamref name="TReturn"/>.<br/>
        /// The <paramref name="method"/> needs to be an instance method.<br/>
        /// This is another version of the
        /// <see cref="CreateInstanceReturnParam{TTarget, TParam, TReturn}(MethodInfo)"/> method
        /// which is helpful when we do not know ahead of time what the target type is and we
        /// cannot use the generic method. However, this method still requires that we do know
        /// ahead of time the return and the parameter types.<br/>
        /// </summary>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <typeparam name="TReturn">The return type of the <paramref name="method"/>.</typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <param name="targetType">The type of the target object.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        // 7. A weakly typed delegate for an instance method that returns a value.
        public static Func<object, TParam, TReturn> CreateInstanceReturnParam<TParam, TReturn>(
            this MethodInfo method, Type targetType) {
            targetType.AssertTargetIsClass(true);
#nullable disable
            MethodInfo genericMethod = typeof(OpenDelegateCreator).GetMethod(
                nameof(BoxCreateInstanceReturnParam), BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo constructedGenericMethod =
                genericMethod.MakeGenericMethod(targetType, typeof(TParam), typeof(TReturn));
            object @delegate = constructedGenericMethod.Invoke(null, new object[] { method });
            return (Func<object, TParam, TReturn>)@delegate;
#nullable enable
        }

        /// <summary>
        /// Helper method for
        /// <see cref="CreateInstanceReturnParam{TParam, TReturn}(MethodInfo, Type)"/>.<br/>
        /// This method will take the strongly typed delegate from the
        /// <see cref="CreateInstanceReturnParam{TTarget, TParam, TReturn}(MethodInfo)"/> method
        /// and turn it into a weakly typed delegate.<br/>
        /// This method is meant to be invoked via reflection because it still has type
        /// parameters.<br/>
        /// </summary>
        /// <typeparam name="TTarget">
        ///     The type that contains the <paramref name="method"/>.
        /// </typeparam>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <typeparam name="TReturn">The return type of the <paramref name="method"/>.</typeparam>
        /// <param name="method">The method to which we create the delegate on.</param>
        /// <returns>The delegate for the <paramref name="method"/>.</returns>
        // 8. A healper method to create the weakly typed delegate.
        private static Func<object, TParam, TReturn>
            BoxCreateInstanceReturnParam<TTarget, TParam, TReturn>(this MethodInfo method)
            where TTarget : class {
            Func<TTarget, TParam, TReturn> @delegate =
                method.CreateInstanceReturnParam<TTarget, TParam, TReturn>();
            TReturn boxedDelegate(object target, TParam param) => @delegate((TTarget)target, param);
            return boxedDelegate;
        }
        #endregion

        /// <summary>
        /// Asserts if the <paramref name="method"/>'s IsStatic property has the desired value.<br/>
        /// </summary>
        /// <param name="method">The method to be checked.</param>
        /// <param name="expected">The desired value.</param>
        /// <exception cref="ArgumentException"></exception>
        private static void AssertStatic(this MethodInfo method, bool expected) {
            bool reality = method.IsStatic;
            if (reality != expected) {
                if (reality) {
                    throw new ArgumentException("The method needs to be an instance method.");
                }
                else {
                    throw new ArgumentException("The method needs to be a static method.");
                }
            }
        }

        /// <summary>
        /// Asserts if the <paramref name="method"/> has a desired return type.<br/>
        /// </summary>
        /// <param name="method">The method to be checked.</param>
        /// <param name="expected">The desired value.</param>
        /// <exception cref="ArgumentException"></exception>
        private static void AssertReturn(this MethodInfo method, Type expected) {
            Type reality = method.ReturnType;
            if (reality != expected) {
                if (reality == typeof(void)) {
                    throw new ArgumentException("The specified method does not return a value " +
                        "but a return type was provided.");
                }
                else if (expected == typeof(void)) {
                    throw new ArgumentException("The specified method returns a value but no " +
                        "return type was provided.");
                }
                else if (!expected.IsAssignableFrom(reality)) {
                    throw new ArgumentException("The return type of the specified method and the " +
                        "provided return typed are not compatible.");
                }
            }
        }

        /// <summary>
        /// Asserts if the <paramref name="method"/> requires the expected amount of parameters.<br/>
        /// </summary>
        /// <param name="method">The method to be checked.</param>
        /// <param name="expected">The desired value.</param>
        /// <exception cref="ArgumentException"></exception>
        private static void AssertParametersCount(this MethodInfo method, int expected) {
            int reality = method.GetParameters().Length;
            if (reality != expected) {
                if (reality == 0) {
                    throw new ArgumentException($"The method expects {reality} parameters " +
                        $"but {expected} were provided.");
                }
                else {
                    throw new ArgumentException($"The method expects {reality} parameters " +
                        $"but none was provided.");
                }
            }
        }

        /// <summary>
        /// Asserts if the <paramref name="parameter"/> has a desired type.<br/>
        /// </summary>
        /// <param name="parameter">The parameter to be checked.</param>
        /// <param name="expected">The desired value.</param>
        /// <exception cref="ArgumentException"></exception>
        private static void AssertParameterType(this ParameterInfo parameter, Type expected) {
            Type reality = parameter.ParameterType;
            if (reality != expected && !reality.IsAssignableFrom(expected)) {
                throw new ArgumentException($"A parameter expects a {reality} type " +
                        $"but {expected} was provided.");
            }
        }

        /// <summary>
        /// Asserts if the <paramref name="type"/> is a class.
        /// </summary>
        /// <param name="type">The type to be checked.</param>
        /// <param name="expected">The desired value.</param>
        /// <exception cref="ArgumentException"></exception>
        private static void AssertTargetIsClass(this Type type, bool expected) {
            bool reality = type.IsClass;
            if (reality != expected) {
                if (reality) {
                    // At the moment we don't have a test case for this condition.
                    // This is coded just for the sake of completioness.
                    throw new ArgumentException($"The target type is expected to not be a class " +
                        $"but in reality it is.");
                }
                else {
                    throw new ArgumentException($"The target type is expected to be a class " +
                        $"but in reality it is not.");
                }
            }
        }

        /// <summary>
        /// Asserts if the <paramref name="method"/>'s IsStatic property has the desired value.<br/>
        /// Asserts if the <paramref name="method"/> has a desired return type.<br/>
        /// Asserts if the <paramref name="method"/> requires the expected amount of parameters.<br/>
        /// This method internally calls the <see cref="AssertStatic(MethodInfo, bool)"/>,
        /// <see cref="AssertReturn(MethodInfo, Type)"/> and the
        /// <see cref="AssertParametersCount(MethodInfo, int)"/> methods.<br/>
        /// </summary>
        /// <param name="method">The method to be checked.</param>
        /// <param name="expectedStatic">The desired value for the IsStatic property.</param>
        /// <param name="expectedType">The desired return type.</param>
        /// <param name="expectedParametersCount">The desired number of parameters.</param>
        private static void AssertStaticReturnParameters(
            this MethodInfo method,
            bool expectedStatic,
            Type expectedType,
            int expectedParametersCount) {
            method.AssertStatic(expectedStatic);
            method.AssertReturn(expectedType);
            method.AssertParametersCount(expectedParametersCount);
        }
    }
}
