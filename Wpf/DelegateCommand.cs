using System;
using System.Windows.Input;

namespace Peanut.Libs.Wpf {
    /// <summary>
    /// An implementation of the <see cref="ICommand"/> interface that makes commanding simpler.<br/>
    /// </summary>
    public partial class DelegateCommand : ICommand {
        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged;

        private CanExecuteStrategy? canExecuteStrategy;
        private ExecuteStrategy? executeStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        protected DelegateCommand() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class
        /// that has a parameterless <see cref="ICommand.CanExecute(object?)"/> method and a
        /// parameterless <see cref="ICommand.Execute(object?)"/> method.<br/>
        /// </summary>
        /// <param name="canExecuteMethod">
        ///     A parameterless <see cref="ICommand.CanExecute(object?)"/> method.
        /// </param>
        /// <param name="executeMethod">
        ///     A parameterless <see cref="ICommand.Execute(object?)"/> method.
        /// </param>
        public DelegateCommand(Func<bool> canExecuteMethod, Action executeMethod) {
            canExecuteStrategy = new ParameterlessCanExecuteStrategy(canExecuteMethod);
            executeStrategy = new ParameterlessExecuteStrategy(executeMethod);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class
        /// that has a parameterless <see cref="ICommand.CanExecute(object?)"/> method and a
        /// parametered <see cref="ICommand.Execute(object?)"/> method.<br/>
        /// </summary>
        /// <param name="canExecuteMethod">
        ///     A parameterless <see cref="ICommand.CanExecute(object?)"/> method.
        /// </param>
        /// <param name="executeMethod">
        ///     A parametered <see cref="ICommand.Execute(object?)"/> method.
        /// </param>
        public DelegateCommand(Func<bool> canExecuteMethod, Action<object?> executeMethod) {
            canExecuteStrategy = new ParameterlessCanExecuteStrategy(canExecuteMethod);
            executeStrategy = new ParameteredExecuteStrategy(executeMethod);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class
        /// that has a parametered <see cref="ICommand.CanExecute(object?)"/> method and a
        /// parameterless <see cref="ICommand.Execute(object?)"/> method.<br/>
        /// </summary>
        /// <param name="canExecuteMethod">
        ///     A parametered <see cref="ICommand.CanExecute(object?)"/> method.
        /// </param>
        /// <param name="executeMethod">
        ///     A parameterless <see cref="ICommand.Execute(object?)"/> method.
        /// </param>
        public DelegateCommand(Func<object?, bool> canExecuteMethod, Action executeMethod) {
            canExecuteStrategy = new ParameteredCanExecuteStrategy(canExecuteMethod);
            executeStrategy = new ParameterlessExecuteStrategy(executeMethod);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class
        /// that has a parametered <see cref="ICommand.CanExecute(object?)"/> method and a
        /// parametered <see cref="ICommand.Execute(object?)"/> method.<br/>
        /// </summary>
        /// <param name="canExecuteMethod">
        ///     A parametered <see cref="ICommand.CanExecute(object?)"/> method.
        /// </param>
        /// <param name="executeMethod">
        ///     A parametered <see cref="ICommand.Execute(object?)"/> method.
        /// </param>
        public DelegateCommand(Func<object?, bool> canExecuteMethod, Action<object?> executeMethod) {
            canExecuteStrategy = new ParameteredCanExecuteStrategy(canExecuteMethod);
            executeStrategy = new ParameteredExecuteStrategy(executeMethod);
        }

        /// <summary>
        /// Sets the method being evaluated when the command checks if it can execute.<br/>
        /// </summary>
        /// <param name="canExecuteMethod">A parameterless func.</param>
        protected void SetCanExecuteMethod(Func<bool> canExecuteMethod) {
            canExecuteStrategy = new ParameterlessCanExecuteStrategy(canExecuteMethod);
        }

        /// <summary>
        /// Sets the method being evaluated when the command checks if it can execute.<br/>
        /// </summary>
        /// <param name="canExecuteMethod">A parametered func.</param>
        protected void SetCanExecuteMethod(Func<object?, bool> canExecuteMethod) {
            canExecuteStrategy = new ParameteredCanExecuteStrategy(canExecuteMethod);
        }

        /// <summary>
        /// Sets the method being invoked when the command executes.<br/>
        /// </summary>
        /// <param name="executeMethod">A parameterless action.</param>
        protected void SetExecuteMethod(Action executeMethod) {
            executeStrategy = new ParameterlessExecuteStrategy(executeMethod);
        }

        /// <summary>
        /// Sets the method being invoked when the command executes.<br/>
        /// </summary>
        /// <param name="executeMethod">A parametered action.</param>
        protected void SetExecuteMethod(Action<object?> executeMethod) {
            executeStrategy = new ParameteredExecuteStrategy(executeMethod);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.<br/>
        /// </summary>
        public void RaiseCanExecuteChanged() {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void Execute(object? parameter) {
            executeStrategy?.Execute(parameter);
        }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter) {
            return canExecuteStrategy != null && canExecuteStrategy.CanExecute(parameter);
        }
    }

    public partial class DelegateCommand {
        private abstract class ExecuteStrategy {
            abstract protected internal void Execute(object? parameter);
        }

        private abstract class CanExecuteStrategy {
            abstract protected internal bool CanExecute(object? parameter);
        }

        private class ParameterlessExecuteStrategy : ExecuteStrategy {
            private readonly Action executeMethod;

            internal ParameterlessExecuteStrategy(Action executeMethod) {
                this.executeMethod = executeMethod;
            }

            protected internal override void Execute(object? parameter) {
                executeMethod?.Invoke();
            }
        }

        private class ParameterlessCanExecuteStrategy : CanExecuteStrategy {
            private readonly Func<bool> canExecuteMethod;

            internal ParameterlessCanExecuteStrategy(Func<bool> canExecuteMethod) {
                this.canExecuteMethod = canExecuteMethod;
            }

            protected internal override bool CanExecute(object? parameter) {
                return canExecuteMethod != null && canExecuteMethod();
            }
        }

        private class ParameteredExecuteStrategy : ExecuteStrategy {
            private readonly Action<object?> executeMethod;

            internal ParameteredExecuteStrategy(Action<object?> executeMethod) {
                this.executeMethod = executeMethod;
            }

            protected internal override void Execute(object? parameter) {
                executeMethod?.Invoke(parameter);
            }
        }

        private class ParameteredCanExecuteStrategy : CanExecuteStrategy {
            private readonly Func<object?, bool> canExecuteMethod;

            internal ParameteredCanExecuteStrategy(Func<object?, bool> canExecuteMethod) {
                this.canExecuteMethod = canExecuteMethod;
            }

            protected internal override bool CanExecute(object? parameter) {
                return canExecuteMethod != null && canExecuteMethod(parameter);
            }
        }
    }
}
