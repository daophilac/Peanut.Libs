using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Peanut.Libs.Specialized {
    /// <summary>
    /// Provides basic methods for registering types to a dependency container.<br/>
    /// </summary>
    public interface IContainerRegister {
        /// <summary>
        /// Registers a type implementation that a dependency container will create new instances
        /// everytime it resolves.<br/>
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TImplementation">
        ///     An implementation/subclass of <typeparamref name="TSource"/>.
        /// </typeparam>
        /// <returns>The current object. This helps registering multiple types simpler.</returns>
        IContainerRegister Register<TSource, TImplementation>() where TImplementation : TSource;

        /// <summary>
        /// Register a singleton type implementation.<br/>
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TImplementation">
        ///     An implementation/subclass of <typeparamref name="TSource"/>.
        /// </typeparam>
        /// <returns>The current object. This helps registering multiple types simpler.</returns>
        IContainerRegister RegisterSingleton<TSource, TImplementation>() where TImplementation : TSource;

        /// <summary>
        /// Register a singleton type implementation but allow user code to give the object.<br/>
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TImplementation">
        ///     An implementation/subclass of <typeparamref name="TSource"/>.
        /// </typeparam>
        /// <param name="value">The singleton object.</param>
        /// <returns>The current object. This helps registering multiple types simpler.</returns>
        IContainerRegister RegisterSingleton<TSource, TImplementation>(TImplementation value)
            where TImplementation : TSource;

        /// <summary>
        /// Unregister a type from the dependency container.<br/>
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <returns>
        ///     True if source type is successfully unregistered. Otherwise, false.
        /// </returns>
        bool Unregister<TSource>();
    }

    /// <summary>
    /// Provides basic methods for resolving types from a dependency container.<br/>
    /// </summary>
    public interface IContainerResolver {
        /// <summary>
        /// Resolves a type.<br/>
        /// </summary>
        /// <typeparam name="TSource">The type that needs to be resolved.</typeparam>
        /// <returns>
        ///     An implementation/subclass instance of the type <typeparamref name="TSource"/>.
        /// </returns>
        TSource Resolve<TSource>();

        /// <summary>
        /// Determine whether the container can resolve the <typeparamref name="TSource"/> type.<br/>
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns>
        ///     True if the container can resolve the <typeparamref name="TSource"/> type.
        ///     Otherwise, false.
        /// </returns>
        bool CanResolve<TSource>();

        /// <summary>
        /// Creates an instance with the provided type.<br/>
        /// The <typeparamref name="T"/> type can be an unregistered type.<br/>
        /// </summary>
        /// <typeparam name="T">The source type.</typeparam>
        /// <returns>An instance of the type <typeparamref name="T"/>.</returns>
        T CreateInstance<T>();
    }

    /// <summary>
    /// Base class that inherits from both <see cref="IContainerRegister"/> and
    /// <see cref="IContainerResolver"/> interfaces to fully make a dependency container layout.<br/>
    /// </summary>
    public abstract class DependencyContainerBase : IContainerRegister, IContainerResolver {
        /// <inheritdoc/>
        public abstract IContainerRegister Register<TSource, TImplementation>() where TImplementation : TSource;

        /// <inheritdoc/>
        public abstract IContainerRegister RegisterSingleton<TSource, TImplementation>() where TImplementation : TSource;

        /// <inheritdoc/>
        public abstract IContainerRegister RegisterSingleton<TSource, TImplementation>(
            TImplementation value) where TImplementation : TSource;

        /// <inheritdoc/>
        public abstract bool Unregister<TSource>();

        /// <inheritdoc/>
        public abstract TSource Resolve<TSource>();

        /// <inheritdoc/>
        public abstract bool CanResolve<TSource>();

        /// <inheritdoc/>
        public abstract T CreateInstance<T>();
    }

    /// <summary>
    /// Subclass of the <see cref="DependencyContainerBase"/> class.<br/>
    /// This class is thread-safe.<br/>
    /// This class cannot be inherited.<br/>
    /// </summary>
    public sealed class DependencyContainer : DependencyContainerBase {
        private readonly Dictionary<Type, Type> registeredTypes = new();
        private readonly Dictionary<Type, object> singletons = new();
        private readonly object locker = new();

        /// <inheritdoc/>
        public override IContainerRegister Register<TSource, TImplementation>() {
            lock(locker) {
                Type typeSource = typeof(TSource);
                Type implementation = typeof(TImplementation);
                if (typeSource == implementation) {
                    if (typeSource.IsAbstract || typeSource.IsInterface) {
                        throw new ArgumentException($"Nonconcrete types cannot be the same.");
                    }
                }

                if (!typeSource.IsAssignableFrom(implementation)) {
                    throw new ArgumentException($"Type {typeSource} is not assignable from " +
                        $"{implementation}");
                }

                if (CanResolve_UnderLock(typeSource)) {
                    throw new AggregateException($"Type {typeSource} has already been registered.");
                }

                registeredTypes.Add(typeSource, implementation);

                return this;
            }
        }

        /// <inheritdoc/>
        public override IContainerRegister RegisterSingleton<TSource, TImplementation>() {
            lock (locker) {
                Register<TSource, TImplementation>();
                TSource singleton = (TSource)Resolve_UnderLock(typeof(TSource));
                Type type = typeof(TSource);
                singletons.Add(type, singleton);
                return this;
            }
        }

        /// <inheritdoc/>
        public override IContainerRegister RegisterSingleton<TSource, TImplementation>(
            TImplementation value) {
            lock (locker) {
                Register<TSource, TImplementation>();
                Type type = typeof(TSource);
                singletons.Add(type, value);
                return this;
            }
        }

        /// <inheritdoc/>
        public override bool Unregister<TSource>() {
            lock (locker) {
                Type t = typeof(TSource);
                singletons.Remove(t);
                return registeredTypes.Remove(t);
            }
        }

        /// <inheritdoc/>
        public override TSource Resolve<TSource>() {
            lock (locker) {
                return (TSource)Resolve_UnderLock(typeof(TSource));
            }
        }

        /// <inheritdoc/>
        public override bool CanResolve<TSource>() {
            lock (locker) {
                return CanResolve_UnderLock(typeof(TSource));
            }
        }

        /// <inheritdoc/>
        public override T CreateInstance<T>() {
            lock (locker) {
                Type type = typeof(T);
                if (type.IsAbstract || type.IsInterface) {
                    throw new ArgumentException("Generic types must be concrete classes.");
                }

                if (!CanResolve_UnderLock(type)) {
                    registeredTypes.Add(type, type);
                }
                return Resolve<T>();
            }
        }

        private object Resolve_UnderLock(Type typeSource) {
            if (singletons.ContainsKey(typeSource)) {
                return singletons[typeSource];
            }

            if (!CanResolve_UnderLock(typeSource)) {
                throw new AggregateException($"Cannot resolve the type {typeSource}.");
            }

            Type destinationType = registeredTypes[typeSource];

            bool foundAppropriateConstructor;
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance;
            foreach (var constructor in destinationType.GetConstructors(flags)) {
                foundAppropriateConstructor = true;
                foreach (var param in constructor.GetParameters()) {
                    if (!CanResolve_UnderLock(param.ParameterType)) {
                        foundAppropriateConstructor = false;
                        break;
                    }
                }
                if (foundAppropriateConstructor) {
                    var parametersInfos = constructor.GetParameters();
                    object?[] args = new object[parametersInfos.Length];
                    if (parametersInfos.Any()) {
                        for (int i = 0; i < parametersInfos.Length; i++) {
                            try {
                                args[i] = Resolve_UnderLock(parametersInfos[i].ParameterType);
                            }
                            catch (AggregateException) {
                                foundAppropriateConstructor = false;
                                break;
                            }
                        }
                        if (foundAppropriateConstructor) {
#pragma warning disable CS8603 // Possible null reference return.
                            return Activator.CreateInstance(destinationType, flags, null, args, null);
#pragma warning restore CS8603 // Possible null reference return.
                        }
                        else {
                            continue;
                        }
                    }
                    else {
#pragma warning disable CS8603 // Possible null reference return.
                        return Activator.CreateInstance(destinationType, flags, null, args, null);
#pragma warning restore CS8603 // Possible null reference return.
                    }
                }
            }
            throw new AggregateException($"Cannot resolve the type {typeSource} because the " +
                $"activator could not find an appropriate constructor.");
        }

        private bool CanResolve_UnderLock(Type type) {
            return registeredTypes.ContainsKey(type);
        }
    }
}
