using System;
using System.Collections.Generic;

namespace Peanut.Libs.Wpf {
    /// <summary>
    /// Base class for all view models.
    /// </summary>
    public abstract class SimpleViewModel : BindableBase, IDisposable {
        private List<SubscriptionToken>? tokens;

        /// <summary>
        /// Gets the instance of <see cref="IEventAggregator"/> associated with this view model.
        /// </summary>
        protected IEventAggregator EventAggregator { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleViewModel"/> class.
        /// </summary>
        protected SimpleViewModel() {
            DisposableManager.Add(this);
            EventAggregator = PeanutApplication.ContainerResolver.Resolve<IEventAggregator>();
        }

        /// <summary>
        /// Adds a <see cref="SubscriptionToken"/> to the internal list so that it can be managed properly.
        /// </summary>
        /// <param name="token">
        ///     The instance of <see cref="SubscriptionToken"/> created when subscribing a <see cref="PubSubEvent"/>
        /// </param>
        protected void AddToken(SubscriptionToken token) {
            tokens ??= new();
            tokens.Add(token);
        }

        /// <summary>
        /// Disposes managed code.
        /// </summary>
        protected virtual void DisposeManaged() { }

        /// <summary>
        /// Disposes unmanaged code.
        /// </summary>
        protected virtual void DisposeUnmanaged() { }

        private bool disposedValue;

        /// <summary>
        /// This method is meant to be cascaded when its inheritor disposes using the dispose pattern.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                    if (tokens != null) {
                        tokens.ForEach(token => token.Dispose());
                        tokens.Clear();
                    }
                    DisposeManaged();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                tokens = null;
                DisposeUnmanaged();
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        /// <summary>
        /// Destroys the view model.
        /// </summary>
        ~SimpleViewModel() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Disposes the view model.
        /// </summary>
        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// A subclass of the <see cref="SimpleViewModel"/> that has an injected service.
    /// </summary>
    /// <typeparam name="T">The type of the service to be injected.</typeparam>
    public abstract class SimpleViewModel<T> : SimpleViewModel where T : IService {
        /// <summary>
        /// Gets the injected service.
        /// </summary>
        protected T Service { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleViewModel"/> class.
        /// </summary>
        protected SimpleViewModel() {
            Service = PeanutApplication.ContainerResolver.Resolve<T>();
        }
    }

    /// <summary>
    /// A subclass of the <see cref="SimpleViewModel"/> which does not have injected services.<br/>
    /// But it does requires a model.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    public abstract class ViewModel<T> : SimpleViewModel where T : IModel {
        /// <summary>
        /// Gets or sets the internal model.
        /// </summary>
        protected T Model { get; set; }

#nullable disable
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel{T}"/> class.
        /// This constructor does not receive a model. So you should override the <see cref="GetModel"/> method
        /// in order for this class to get a hold of the model.
        /// </summary>
        protected ViewModel() { }
#nullable enable

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel{T}"/> class.
        /// This constructor already receives the model. So you don't need to override
        /// the <see cref="GetModel"/> method.
        /// </summary>
        /// <param name="model"></param>
        protected ViewModel(T model) {
            Model = model;
        }

#nullable disable
        /// <summary>
        /// Gets the associated model. If you create this view model using the parameterless constructor,
        /// you need to override this method.
        /// </summary>
        /// <returns></returns>
        protected virtual T GetModel() { return default; }
#nullable enable
    }

    /// <summary>
    /// A subclass of the <see cref="ViewModel{T}"/> class that has both an injected service and a model.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TService">The type of the service.</typeparam>
    public abstract class ViewModel<TModel, TService> : ViewModel<TModel>
        where TModel : IModel
        where TService : IService {
        /// <summary>
        /// Gets the injected service.
        /// </summary>
        protected TService Service { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel{T}"/> class.
        /// </summary>
        protected ViewModel() {
            Service = PeanutApplication.ContainerResolver.Resolve<TService>();
            Model = GetModel();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel{T}"/> class with the provided model.
        /// </summary>
        /// <param name="model"></param>
        protected ViewModel(TModel model) : base(model) {
            Service = PeanutApplication.ContainerResolver.Resolve<TService>();
        }
    }
}
