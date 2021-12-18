using System;

namespace Peanut.Libs.Abstraction.StateMachine {
    /// <summary>
    /// A base class providing logic handling ragarding to successfulness of tasks.
    /// </summary>
    public abstract class Result {
        /// <summary>
        /// Indicates if the task was successful.
        /// </summary>
        public bool Success { get; protected set; }

        /// <summary>
        /// Gets the error (if any) in case <see cref="Success"/> is <code>false</code>
        /// </summary>
        public Error? Error { get; protected set; }

        /// <summary>
        /// A protected constructor that assigns value to the <see cref="Success"/> property.
        /// </summary>
        /// <param name="success">A value that gets assigned to the <see cref="Success"/> property.</param>
        protected Result(bool success) {
            Success = success;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>Returns a string that represents the current object.</returns>
        public override string ToString() {
            if (Success) {
                return "Success";
            }
            else {
                if (Error == null) {
                    return "Error";
                }
                else {
                    if (Error.Exception != null && !string.IsNullOrEmpty(Error.Exception.Message)) {
                        return $"Error: {Error.Message}{Environment.NewLine}" +
                            $"Exception: {Error.Exception.Message}";
                    }
                    else {
                        return $"Error: {Error.Message}";
                    }
                }
            }
        }
    }

    /// <summary>
    /// An inherited <see cref="Result"/> class that is used to determine the successfulness
    /// of an operation performed by a <see cref="StateMachine"/>.
    /// </summary>
    public sealed class OperationResult : Result {
        private OperationResult(bool success) : base(success) { }

        /// <summary>
        /// Creates a new instance of the <see cref="OperationResult"/> class that has
        /// the <see cref="Result.Success"/> property set to true.
        /// </summary>
        public static OperationResult Succeed() => new(true);

        /// <summary>
        /// Creates a new instance of the <see cref="OperationResult"/> class that has
        /// the <see cref="Result.Success"/> property set to false.
        /// </summary>
        public static OperationResult Fail() => new(false);

        /// <summary>
        /// Creates a new instance of the <see cref="OperationResult"/> class that has
        /// the <see cref="Result.Success"/> property set to false and also assign a value
        /// to the <see cref="Result.Error"/> property.
        /// </summary>
        /// <param name="error">An error that gets assigned to the <see cref="Result.Error"/> property.</param>
        public static OperationResult Fail(Error error) {
            OperationResult result = Fail();
            result.Error = error;
            return result;
        }
    }

    /// <summary>
    /// An inherited <see cref="Result"/> class that is used to determine the successfulness
    /// of a command performed by user code.
    /// </summary>
    public class CommandResult : Result {
        private CommandResult(bool success) : base(success) { }

        /// <summary>
        /// Creates a new instance of the <see cref="CommandResult"/> class that has
        /// the <see cref="Result.Success"/> property set to true.
        /// </summary>
        internal static CommandResult Succeed() => new(true);

        /// <summary>
        /// Creates a new instance of the <see cref="CommandResult"/> class that has
        /// the <see cref="Result.Success"/> property set to false.
        /// </summary>
        internal static CommandResult Fail() => new(false);

        /// <summary>
        /// Creates a new instance of the <see cref="CommandResult"/> class that has
        /// the <see cref="Result.Success"/> property set to false and also assign a value
        /// to the <see cref="Result.Error"/> property.
        /// </summary>
        /// <param name="error">An error that gets assigned to the <see cref="Result.Error"/> property.</param>
        internal static CommandResult Fail(Error error) {
            CommandResult result = Fail();
            result.Error = error;
            return result;
        }

        internal static CommandResult InitialStart => Fail(Error.SystemError(1, $"Cannot start while initializing."));
        internal static CommandResult InitialPause => Fail(Error.SystemError(2, $"Cannot pause while initializing."));
        internal static CommandResult InitialResume => Fail(Error.SystemError(3, $"Cannot resume while initializing."));
        internal static CommandResult InitialCancel => Fail(Error.SystemError(4, $"Cannot cancel while initializing."));
        internal static CommandResult InitialRestart => Fail(Error.SystemError(5, $"Cannot restart while initializing."));
        internal static CommandResult InitializedPause => Fail(Error.SystemError(6, $"Cannot pause at initialized state."));
        internal static CommandResult InitializedResume => Fail(Error.SystemError(7, $"Cannot resume at initialized state."));
        internal static CommandResult WorkingStart => Fail(Error.SystemError(8, $"Cannot start while working."));
        internal static CommandResult WorkingResume => Fail(Error.SystemError(9, $"Cannot resume while working."));
        internal static CommandResult WorkingPauseNotRespondInTime => Fail(Error.SystemError(10, $"The operation did not pause in time."));
        internal static CommandResult WorkingPauseHasFinished => Fail(Error.SystemError(11, $"The operation has already come to finished state."));
        internal static CommandResult WorkingPauseHasFailed => Fail(Error.SystemError(12, $"The operation has come to failed state."));
        internal static CommandResult WorkingPauseHasFaulted => Fail(Error.SystemError(13, $"The operation has come to faulted state."));
        internal static CommandResult WorkingCancelNotRespondInTime => Fail(Error.SystemError(14, $"The operation did not cancel in time."));
        internal static CommandResult WorkingCancelNotHandleInTime => Fail(Error.SystemError(15, $"The operation did not handle cancellation in time."));
        internal static CommandResult WorkingCancelHasFinished => Fail(Error.SystemError(16, $"The operation has already come to finished state."));
        internal static CommandResult WorkingCancelHasFailed => Fail(Error.SystemError(17, $"The operation has come to failed state."));
        internal static CommandResult WorkingCancelHasFaulted => Fail(Error.SystemError(18, $"The operation has come to faulted state."));
        internal static CommandResult PausingStart => Fail(Error.SystemError(19, $"Cannot start while pausing."));
        internal static CommandResult PausingPause => Fail(Error.SystemError(20, $"Cannot pause while pausing."));
        internal static CommandResult PausingCancelNotHandleInTime => Fail(Error.SystemError(21, $"The operation did not handle cancellation in time."));
        internal static CommandResult FinishedStart => Fail(Error.SystemError(22, $"Cannot start when already finished."));
        internal static CommandResult FinishedPause => Fail(Error.SystemError(23, $"Cannot pause when already finished."));
        internal static CommandResult FinishedResume => Fail(Error.SystemError(24, $"Cannot resume when already finished."));
        internal static CommandResult FinishedCancel => Fail(Error.SystemError(25, $"Cannot cancel when already finished."));
        internal static CommandResult CanceledStart => Fail(Error.SystemError(26, $"Cannot start when already canceled."));
        internal static CommandResult CanceledPause => Fail(Error.SystemError(27, $"Cannot pause when already canceled."));
        internal static CommandResult CanceledResume => Fail(Error.SystemError(28, $"Cannot resume when already canceled."));
        internal static CommandResult CanceledCancel => Fail(Error.SystemError(29, $"Cannot cancel when already canceled."));
        internal static CommandResult FailedStart => Fail(Error.SystemError(30, $"Cannot start when already failed."));
        internal static CommandResult FailedPause => Fail(Error.SystemError(31, $"Cannot pause when already failed."));
        internal static CommandResult FailedResume => Fail(Error.SystemError(32, $"Cannot resume when already failed."));
        internal static CommandResult FailedCancel => Fail(Error.SystemError(33, $"Cannot cancel when already failed."));
        internal static CommandResult FaultedStart => Fail(Error.SystemError(34, $"Cannot start when already faulted."));
        internal static CommandResult FaultedPause => Fail(Error.SystemError(35, $"Cannot pause when already faulted."));
        internal static CommandResult FaultedResume => Fail(Error.SystemError(36, $"Cannot resume when already faulted."));
        internal static CommandResult FaultedCancel => Fail(Error.SystemError(37, $"Cannot cancel when already faulted."));
        internal static CommandResult WorkingPauseException(Exception ex) => Fail(Error.SystemError(38, $"An exception has occured during pausing.", ex));
        internal static CommandResult PausingResumeException(Exception ex) => Fail(Error.SystemError(39, $"An exception has occured during resuming.", ex));
        internal static CommandResult InitializedCancelException(Exception ex) => Fail(Error.SystemError(40, $"An exception has occured during cancelling.", ex));
        internal static CommandResult WorkingCancelException(Exception ex) => Fail(Error.SystemError(41, $"An exception has occured during cancelling.", ex));
        internal static CommandResult PausingCancelException(Exception ex) => Fail(Error.SystemError(42, $"An exception has occured during cancelling.", ex));
    }

    /// <summary>
    /// Provides information about a failed <see cref="Result"/>. This class cannot be inherited.
    /// </summary>
    public sealed class Error {
        /// <summary>
        /// Gets a value indicating whether the error is a pre-defined error written in
        /// the internal code.
        /// </summary>
        public bool IsSystemError { get; private set; }

        /// <summary>
        /// Gets a value representing the error.<br/>
        /// This value is user-defined.
        /// </summary>
        public int Code { get; private set; }

        /// <summary>
        /// The actual message of the error.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has an <see cref="Exception"/>.<br/>
        /// This property provides simplicity. Because internally it simply checks if
        /// <see cref="Exception"/> is not null.
        /// </summary>
        public bool HasException { get => Exception != null; }

        /// <summary>
        /// Gets the exception of this instance.
        /// </summary>
        public Exception? Exception { get; private set; }

        /// <summary>
        /// Initializes an instance of the <see cref="Error"/> class that sets a value
        /// for <see cref="Message"/> property.
        /// </summary>
        /// <param name="message">A message used to describe the error.</param>
        public Error(string message) {
            Message = message;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="Error"/> class that sets a value
        /// for <see cref="Message"/> property and a value for the <see cref="Code"/> property.
        /// </summary>
        /// <param name="message">A message used to describe the error.</param>
        /// <param name="code">A user-defined code used to describe the error.</param>
        public Error(string message, int code) : this(message) {
            Code = code;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="Error"/> class that sets a value
        /// for <see cref="Message"/> property, a value for the <see cref="Code"/> property
        /// and a value for the <see cref="Exception"/> property.
        /// </summary>
        /// <param name="message">A message used to describe the error.</param>
        /// <param name="code">A user-defined code used to describe the error.</param>
        /// <param name="exception">The exception of the error.</param>
        public Error(string message, int code, Exception exception) : this(message, code) {
            Exception = exception;
        }

        static internal Error SystemError(int code, string message) {
            Error error = new(message, code);
            error.IsSystemError = true;
            return error;
        }

        static internal Error SystemError(int code, string message, Exception exception) {
            Error error = SystemError(code, message);
            error.Exception = exception;
            return error;
        }
    }
}
