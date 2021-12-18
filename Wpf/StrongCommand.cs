using System;

namespace Peanut.Libs.Wpf {
    /// <summary>
    /// The interface of a parameterless strong command.<br/>
    /// </summary>
    public interface IStrongCommand {
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current
        /// state.<br/>
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this command can be executed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        bool CanExecute();

        /// <summary>
        /// Defines the method to be called when the command is invoked.<br/>
        /// </summary>
        void Execute();
    }

    /// <summary>
    /// The interface of a parametered strong command.<br/>
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    public interface IStrongCommand<T> {
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current
        /// state.<br/>
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>
        /// <see langword="true"/> if this command can be executed; otherwise,
        /// <see langword="false"/>.
        /// </returns>s
        bool CanExecute(T parameter);

        /// <summary>
        /// Defines the method to be called when the command is invoked.<br/>
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        void Execute(T parameter);
    }

    /// <summary>
    /// A delegate command that implements the <see cref="IStrongCommand"/> interface.<br/>
    /// </summary>
    public class StrongCommand : IStrongCommand {
        private readonly Func<bool> canExecuteDelegate;
        private readonly Action executeDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="StrongCommand"/> class.<br/>
        /// </summary>
        /// <param name="canExecuteDelegate">
        ///     A delegate that gets evaluated when the command needs to check whether it
        ///     can execute.
        /// </param>
        /// <param name="executeDelegate">
        ///     A delegate that gets invoked when the command executes.
        /// </param>
        public StrongCommand(Func<bool> canExecuteDelegate, Action executeDelegate) {
            this.canExecuteDelegate = canExecuteDelegate;
            this.executeDelegate = executeDelegate;
        }

        /// <inheritdoc/>
        public bool CanExecute() {
            return canExecuteDelegate();
        }

        /// <inheritdoc/>
        public void Execute() {
            executeDelegate();
        }
    }

    /// <summary>
    /// A delegate command that implements the <see cref="IStrongCommand{T}"/> interface.<br/>
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    public class StrongCommand<T> : IStrongCommand<T> {
        private readonly Func<T, bool> canExecuteDelegate;
        private readonly Action<T> executeDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="StrongCommand{T}"/> class.<br/>
        /// </summary>
        /// <param name="canExecuteDelegate">
        ///     A delegate that gets evaluated when the command needs to check whether it
        ///     can execute.
        /// </param>
        /// <param name="executeDelegate">
        ///     A delegate that gets invoked when the command executes.
        /// </param>
        public StrongCommand(Func<T, bool> canExecuteDelegate, Action<T> executeDelegate) {
            this.canExecuteDelegate = canExecuteDelegate;
            this.executeDelegate = executeDelegate;
        }

        /// <inheritdoc/>
        public bool CanExecute(T parameter) {
            return canExecuteDelegate(parameter);
        }

        /// <inheritdoc/>
        public void Execute(T parameter) {
            executeDelegate(parameter);
        }
    }
}
