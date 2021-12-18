using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peanut.Libs.Abstraction.StateMachine {
    /// <summary>
    /// A state machine that performs long-running tasks.
    /// </summary>
    public abstract class StateMachine {
        /// <summary>
        /// Get or set the time (in milliseconds) the machine will wait for the inherit class
        /// to call <see cref="PauseOrCancelIfRequested"/> method
        /// in order to perform pausing or cancelling.<br/>
        /// Default: <see cref="long.MaxValue"/>
        /// </summary>
        public long MaxWaitForPauseOrCancel { get; set; } = long.MaxValue;

        /// <summary>
        /// Get or set the time (in milliseconds) the machine will wait for the inherit class
        /// to terminate the task inside <see cref="OnStart"/> method.<br/>
        /// Default: <see cref="long.MaxValue"/>
        /// </summary>
        public long MaxWaitForCancelHandling { get; set; } = long.MaxValue;

        /// <summary>
        /// An enum indicates the current state of the machine.
        /// </summary>
        public State CurrentState { get => MachineState.State; }

        /// <summary>
        /// Containing all state transitions, in order.
        /// </summary>
        public LinkedList<State> StateChanges => new(stateChanges);
        private readonly LinkedList<State> stateChanges = new();

        /// <summary>
        /// The state object used for performing state specific operations.
        /// </summary>
        internal MachineState MachineState { get; set; }

        /// <summary>
        /// A custom <see cref="TaskCompletionSource"/> used for pausing handling.
        /// </summary>
        internal CustomTaskCompletionSource<PauseOrCancelResult>? TcsPause { get; set; }

        /// <summary>
        /// A custom <see cref="TaskCompletionSource"/> used for cancelling handling.
        /// </summary>
        internal CustomTaskCompletionSource<PauseOrCancelResult>? TcsCancel { get; set; }

        /// <summary>
        /// A flag used for communication about whether the <see cref="OnStart"/> method
        /// has terminated upon user's cancellation request.
        /// </summary>
        internal bool? CancellationHandled { get; set; }

        /// <summary>
        /// Used to make <see cref="MachineState"/> with ease.
        /// </summary>
        internal readonly StateFactory StateFactory;

        /// <summary>
        /// A lock used to ensure thread safe.
        /// </summary>
        private readonly object @lock = new();

        /// <summary>
        /// A protected constructor that will trigger the <see cref="OnInitialize"/> method
        /// and change to another state depending on the result of the method.
        /// </summary>
#nullable disable
        protected StateMachine() {
#nullable enable
            StateFactory = new(this);
            ChangeState(StateFactory.MakeState(InitialState.state));
            Initialize();
        }

        internal void ChangeState(MachineState state) {
            MachineState = state;
            stateChanges.AddLast(state.State);
        }

        internal void Initialize() {
            MachineState.Initialize();
        }

        /// <summary>
        /// Start performing long runng tasks.
        /// </summary>
        /// <returns></returns>
        public CommandResult Start() {
            lock (@lock) {
                return MachineState.Start();
            }
        }

        /// <summary>
        /// Try to pause the machine.<br/>
        /// This method will wait until the internal code finds a good place to pause.<br/>
        /// It will wait for the amount of time
        /// being configured using the <see cref="MaxWaitForPauseOrCancel"/> property.
        /// </summary>
        /// <returns></returns>
        public CommandResult Pause() {
            lock (@lock) {
                return MachineState.Pause();
            }
        }

        /// <summary>
        /// Try to resume the pausing machine.
        /// </summary>
        /// <returns></returns>
        public CommandResult Resume() {
            lock (@lock) {
                return MachineState.Resume();
            }
        }

        /// <summary>
        /// Try to cancel the machine.<br/>
        /// If the machine is working, this method will wait until
        /// the internal code finds a good place to cancel.<br/>
        /// It will wait for the amount of time
        /// being configured using the <see cref="MaxWaitForPauseOrCancel"/> property.<br/>
        /// After that, it will wait for the internal code to terminate the long running tasks.<br/>
        /// It will wait for the amount of time
        /// being configured using the <see cref="MaxWaitForCancelHandling"/> property.<br/>
        /// But if the machine is pausing, only the <see cref="MaxWaitForCancelHandling"/> property
        /// will be taken into account.
        /// </summary>
        /// <returns></returns>
        public CommandResult Cancel() {
            lock (@lock) {
                return MachineState.Cancel();
            }
        }

        /// <summary>
        /// Try to restart the machine.<br/>
        /// If the <see cref="CurrentState"/> is <see cref="State.Finished"/>
        /// or <see cref="State.Failed"/> or <see cref="State.Canceled"/>
        /// or <see cref="State.Faulted"/>, this method will just change the state
        /// to <see cref="State.Initial"/>, then it will internally do the initialization again.
        /// And then it will call the <see cref="Start"/> method.<br/>
        /// If the <see cref="CurrentState"/> is <see cref="State.Initialized"/>
        /// or <see cref="State.Working"/> or <see cref="State.Pausing"/>,
        /// this method will also do the same.
        /// The only difference is that it will first call the <see cref="Cancel"/> method.
        /// </summary>
        /// <returns></returns>
        public CommandResult Restart() {
            lock (@lock) {
                return MachineState.Restart();
            }
        }

        /// <summary>
        /// If the user requests a pause, this method will block until a resume or a cancel is made.<br/>
        /// If the user requests a cancel, this method will return almost instantly,
        /// depending on how long the <see cref="OnCancel"/> method takes.<br/>
        /// To check what was the command,
        /// examine the property <see cref="PauseOrCancelResult.HasResumed"/>
        /// or <see cref="PauseOrCancelResult.HasCanceled"/>.<br/>
        /// Keep in mind, the result only tells what was the command.
        /// It does not tell why this method got called in the first place.<br/>
        /// If the user requested cancel, or a pause and then cancel,
        /// the <see cref="PauseOrCancelResult.HasCanceled"/> property will be true for both cases.
        /// </summary>
        /// <returns></returns>
        protected async Task<PauseOrCancelResult?> PauseOrCancelIfRequested() {
            return await MachineState.PauseOrCancelIfRequested().ConfigureAwait(false);
        }

        /// <summary>
        /// Perform initialization here.<br/>
        /// This method gets called even before the constructor.
        /// So, do not rely on it to perform any tasks that use properties passed to the constructors.<br/>
        /// If an uncaught exception was thrown inside this method, the constructor will fail,
        /// returning a null object.<br/>
        /// Otherwise, the <see cref="CurrentState"/> property will instantly change
        /// from <see cref="State.Initial"/> to either <see cref="State.Initialized"/>
        /// or <see cref="State.Failed"/> depending on the result being returned.
        /// </summary>
        /// <returns></returns>
        protected internal virtual OperationResult OnInitialize() {
            return OperationResult.Succeed();
        }

        /// <summary>
        /// Perform long running tasks here.<br/>
        /// If an uncaught exception was thrown inside this method,
        /// the machine will enter the faulted state.
        /// The <see cref="CurrentState"/> property will change
        /// from <see cref="State.Initialized"/> to <see cref="State.Faulted"/> instantly.<br/>
        /// Otherwise, the <see cref="CurrentState"/> property will instantly change
        /// from <see cref="State.Initialized"/> to either <see cref="State.Finished"/>
        /// or <see cref="State.Failed"/> depending on the result being returned.
        /// </summary>
        /// <returns></returns>
        protected internal abstract Task<OperationResult> OnStart();

        /// <summary>
        /// By calling the <see cref="Pause"/> method, this method will get called.<br/>
        /// If an uncaught exception was thrown inside this method,
        /// the <see cref="Pause"/> method will return a failed result,
        /// containing the exception being thrown.
        /// Meanwhile, the blocking method call <see cref="PauseOrCancelIfRequested"/>
        /// inside the <see cref="OnStart"/> method will throw that exception.
        /// The machine will enter the faulted state. The <see cref="CurrentState"/> property
        /// will change from <see cref="State.Working"/> to <see cref="State.Faulted"/> instantly.<br/>
        /// Otherwise, the <see cref="CurrentState"/> property will instantly change
        /// from <see cref="State.Working"/> to <see cref="State.Pausing"/> after this method returns.
        /// </summary>
        protected internal virtual void OnPause() { }

        /// <summary>
        /// By calling the <see cref="Resume"/> method, this method will get called.<br/>
        /// If an uncaught exception was thrown inside this method,
        /// the <see cref="Resume"/> method will return a failed result,
        /// containing the exception being thrown.
        /// Meanwhile, the blocking method call <see cref="PauseOrCancelIfRequested"/> inside
        /// the <see cref="OnStart"/> method will throw that exception.
        /// The machine will enter the faulted state. The <see cref="CurrentState"/> property
        /// will change from <see cref="State.Pausing"/> to <see cref="State.Faulted"/> instantly.<br/>
        /// Otherwise, the <see cref="CurrentState"/> property will instantly change
        /// from <see cref="State.Pausing"/> to <see cref="State.Working"/> after this method returns.
        /// </summary>
        protected internal virtual void OnResume() { }

        /// <summary>
        /// By calling the <see cref="Cancel"/> method, this method will get called.<br/>
        /// If an uncaught exception was thrown inside this method
        /// and the <see cref="CurrentState"/> property is <see cref="State.Initialized"/>,
        /// the machine will enter the faulted state. The <see cref="CurrentState"/> property
        /// will change from <see cref="State.Initialized"/> to <see cref="State.Faulted"/> instantly
        /// after this method returns.<br/>
        /// But if the <see cref="CurrentState"/> property is <see cref="State.Working"/>
        /// or <see cref="State.Pausing"/>,
        /// the <see cref="Cancel"/> method will return a failed result,
        /// containing the exception being thrown.
        /// Meanwhile, the blocking method call <see cref="PauseOrCancelIfRequested"/> inside
        /// the <see cref="OnStart"/> method will throw that exception.<br/>
        /// And if there was no exception, there will be no guarantee that
        /// the <see cref="CurrentState"/> property will be <see cref="State.Canceled"/>
        /// after this method returns.
        /// Only after the <see cref="Cancel"/> method returns will we be sure that
        /// the <see cref="CurrentState"/> is <see cref="State.Canceled"/>.
        /// </summary>
        protected internal virtual void OnCancel() { }

        /// <summary>
        /// A temporary workaround for tracking the status of <see cref="TaskCompletionSource"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal sealed class CustomTaskCompletionSource<T> {
            private readonly TaskCompletionSource<T> taskCompletionSource = new();
            internal Task<T> Task => taskCompletionSource.Task;
            internal bool IsWaitingForResult { get; private set; }
            internal CustomTaskCompletionSource() { }

            public async Task<T> WaitForResult() {
                IsWaitingForResult = true;
                return await Task.ConfigureAwait(false);
            }

            public void SetResult(T result) {
                IsWaitingForResult = false;
                taskCompletionSource.SetResult(result);
            }

            public void SetException(Exception exception) {
                IsWaitingForResult = false;
                taskCompletionSource.SetException(exception);
            }
        }
    }

    /// <summary>
    /// Provides information about the result after a derived class
    /// calls the <see cref="StateMachine.PauseOrCancelIfRequested"/> method.
    /// </summary>
    public sealed class PauseOrCancelResult {
        /// <summary>
        /// Indicates whether the result was an act of resuming from the user.
        /// </summary>
        public bool HasResumed { get; }

        /// <summary>
        /// Indicates whether the result was an act of cancelling from the user.
        /// </summary>
        public bool HasCanceled { get; }

        private PauseOrCancelResult(bool hasResumed, bool hasCanceled) {
            HasResumed = hasResumed;
            HasCanceled = hasCanceled;
        }

        internal static readonly PauseOrCancelResult Resumed = new(true, false);
        internal static readonly PauseOrCancelResult Canceled = new(false, true);
    }
}