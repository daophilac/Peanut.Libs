using Peanut.Libs.Specialized;
using System.Windows;

namespace Peanut.Libs.Wpf {
    /// <summary>
    /// A subclass of the <see cref="Application"/> class that provides machenism for making
    /// MVVM application.<br/>
    /// </summary>
    public abstract class PeanutApplication : Application {
        /// <summary>
        /// Gets whether the application is in design time.<br/>
        /// </summary>
        public static bool IsDesignTime { get; private set; } = true;

        /// <summary>
        /// Gets the current <see cref="IContainerRegister"/> of the application.<br/>
        /// </summary>
        public static IContainerRegister ContainerRegister { get; } =
            new DependencyContainer().RegisterSingleton<IEventAggregator, EventAggregator>();

        /// <summary>
        /// Gets the current <see cref="IContainerResolver"/> of the application.<br/>
        /// </summary>
        public static IContainerResolver ContainerResolver { get; } =
            (IContainerResolver)ContainerRegister;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeanutApplication"/> class.<br/>
        /// </summary>
        public PeanutApplication() {
            IsDesignTime = false;
            RegisterType(ContainerRegister);
        }

        /// <inheritdoc/>
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            Window window = CreateShell();
            window.Show();
        }

        /// <summary>
        /// Registers dependencies.<br/>
        /// </summary>
        /// <param name="container">
        ///     The <see cref="IContainerRegister"/> used to register dependencies.
        /// </param>
        protected abstract void RegisterType(IContainerRegister container);

        /// <summary>
        /// Creates the main window of the application.<br/>
        /// </summary>
        /// <returns>The main window of the application.</returns>
        protected abstract Window CreateShell();
    }
}
