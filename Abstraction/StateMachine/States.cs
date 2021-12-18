using System;
using System.Threading.Tasks;

namespace Peanut.Libs.Abstraction.StateMachine {
    internal abstract class MachineState {
        protected const int RetryInterval = 50;
        protected internal abstract State State { get; }
        protected StateMachine Machine { get; set; }

        protected MachineState(StateMachine machine) {
            Machine = machine;
        }

        protected void ChangeState(State newState) {
            Machine.ChangeState(Machine.StateFactory.MakeState(newState));
        }

        protected internal async Task<PauseOrCancelResult?> PauseOrCancelIfRequested() {
            if (State != WorkingState.state) {
                throw new InvalidOperationException($"{nameof(PauseOrCancelIfRequested)} " +
                    $"method should not be called outside of Working state");
            }

            // Should never happen. Debugging purpose only
            if (Machine.TcsPause != null && Machine.TcsCancel != null) {
                throw new InvalidOperationException("The program has entered an illegal state");
            }

            if (Machine.TcsPause != null) {
                return await Machine.TcsPause.WaitForResult().ConfigureAwait(false);
            }
            if (Machine.TcsCancel != null) {
                return await Machine.TcsCancel.WaitForResult().ConfigureAwait(false);
            }
            
            return null;
        }

        protected internal abstract void Initialize();

        /// <summary>
        /// The <see cref="State"/> property will change to <see cref="State.Working"/> instantly
        /// when this method gets called.
        /// </summary>
        /// <returns></returns>
        protected internal abstract CommandResult Start();

        /// <summary>
        /// This method will wait for the code inside the <see cref="StateMachine.OnStart"/> method
        /// to call <see cref="PauseOrCancelIfRequested"/>.<br/>
        /// It will wait for the amount of time being configured using
        /// the <see cref="StateMachine.MaxWaitForPauseOrCancel"/> property.
        /// </summary>
        /// <returns></returns>
        protected internal abstract CommandResult Pause();

        /// <summary>
        /// The blocking method call <see cref="PauseOrCancelIfRequested"/> inside
        /// the <see cref="StateMachine.OnStart"/> method will be released when this method gets called.
        /// </summary>
        /// <returns></returns>
        protected internal abstract CommandResult Resume();

        /// <summary>
        /// If the <see cref="State"/> is <see cref="State.Working"/> or <see cref="State.Pausing"/>,
        /// the blocking method call <see cref="PauseOrCancelIfRequested"/> inside
        /// the <see cref="StateMachine.OnStart"/> method will be released when this method gets called.
        /// </summary>
        /// <returns></returns>
        protected internal abstract CommandResult Cancel();

        /// <summary>
        /// If the <see cref="State"/> is <see cref="State.Finished"/> or <see cref="State.Failed"/>
        /// or <see cref="State.Canceled"/> or <see cref="State.Faulted"/>,
        /// this method will just change the state to <see cref="State.Initial"/>,
        /// then call <see cref="StateMachine.OnInitialize"/> and then call
        /// <see cref="StateMachine.Start"/>.<br/>
        /// If the <see cref="State"/> is <see cref="State.Initialized"/> or <see cref="State.Working"/>
        /// or <see cref="State.Pausing"/>, this method will also do the same.
        /// The only difference is that it will first call the <see cref="StateMachine.Cancel"/> method.
        /// </summary>
        /// <returns></returns>
        protected internal virtual CommandResult Restart() {
            ChangeState(InitialState.state);
            Machine.Initialize();
            return Machine.Start();
        }
    }

    internal sealed class InitialState : MachineState {
        internal static readonly State state = State.Initial;
        protected internal override State State => state;

        internal InitialState(StateMachine machine) : base(machine) { }

        protected internal override void Initialize() {
            try {
                OperationResult operationResult = Machine.OnInitialize();
                if (!operationResult.Success) {
                    ChangeState(FailedState.state);
                }
                else {
                    ChangeState(InitializedState.state);
                }
            }
            catch {
                // Because we throw here, the constructor will fail, and the object will be null.
                // So, it's not necessary to change the state to Faulted here but
                // we will just do it for the sake of completioness.
                ChangeState(FaultedState.state);
                throw;
            }
        }

        // The initializing phase is constructor bound.
        // So it is not possible to perform any command while the machine is initializing.
        // But we will just return a failed result for completioness.
        protected internal override CommandResult Start() {
            return CommandResult.InitialStart;
        }

        protected internal override CommandResult Pause() {
            return CommandResult.InitialPause;
        }

        protected internal override CommandResult Resume() {
            return CommandResult.InitialResume;
        }

        protected internal override CommandResult Cancel() {
            return CommandResult.InitialCancel;
        }

        protected internal override CommandResult Restart() {
            return CommandResult.InitialRestart;
        }
    }

    internal sealed class InitializedState : MachineState {
        internal static readonly State state = State.Initialized;
        protected internal override State State => state;

        internal InitializedState(StateMachine machine) : base(machine) { }

        protected internal override void Initialize() {
            throw new InvalidOperationException();
        }

        protected internal override CommandResult Start() {
            ChangeState(WorkingState.state);
            Task.Factory.StartNew(async () => {
                try {
                    OperationResult operationResult = await Machine.OnStart().ConfigureAwait(false);
                    if (!operationResult.Success) {
                        ChangeState(FailedState.state);
                    }
                    else {
                        // Only 2 possibilities:
                        // 1. The operation returns because it was canceled.
                        // 2. The operation returns because it finished its task.
                        if (Machine.CancellationHandled.HasValue) {
                            Machine.CancellationHandled = true;
                        }
                        else {
                            ChangeState(FinishedState.state);
                        }
                    }
                }
                catch {
                    // The check is for when resuming or cancelling has encountered an exception.
                    // We already changed the state to Faulted. So we should not change it again.
                    if (Machine.CurrentState != FaultedState.state) {
                        ChangeState(FaultedState.state);
                    }
                    throw;
                }

            }, TaskCreationOptions.LongRunning);
            return CommandResult.Succeed();
        }

        protected internal override CommandResult Pause() {
            return CommandResult.InitializedPause;
        }

        protected internal override CommandResult Resume() {
            return CommandResult.InitializedResume;
        }

        protected internal override CommandResult Cancel() {
            try {
                Machine.OnCancel();
                ChangeState(CanceledState.state);
                return CommandResult.Succeed();
            }
            catch (Exception ex) {
                ChangeState(FaultedState.state);
                return CommandResult.InitializedCancelException(ex);
            }
        }

        protected internal override CommandResult Restart() {
            CommandResult cancelResult = Cancel();
            if (!cancelResult.Success) {
                return cancelResult;
            }
            return base.Restart();
        }
    }

    internal sealed class WorkingState : MachineState {
        internal static readonly State state = State.Working;
        protected internal override State State => state;

        internal WorkingState(StateMachine machine) : base(machine) { }

        protected internal override void Initialize() {
            throw new InvalidOperationException();
        }

        protected internal override CommandResult Start() {
            return CommandResult.WorkingStart;
        }

        protected internal override CommandResult Pause() {
            Machine.TcsPause = new();
            long pauseWaitAddUp = 0;
            while (!Machine.TcsPause.IsWaitingForResult) {
                if (Machine.CurrentState == FinishedState.state) {
                    return CommandResult.WorkingPauseHasFinished;
                }
                if (Machine.CurrentState == FailedState.state) {
                    return CommandResult.WorkingPauseHasFailed;
                }
                if (Machine.CurrentState == FaultedState.state) {
                    return CommandResult.WorkingPauseHasFaulted;
                }

                Task.Delay(RetryInterval).Wait();
                pauseWaitAddUp += RetryInterval;
                if (pauseWaitAddUp >= Machine.MaxWaitForPauseOrCancel || pauseWaitAddUp <= 0) {
                    return CommandResult.WorkingPauseNotRespondInTime;
                }
            }

            try {
                Machine.OnPause();
                ChangeState(PausingState.state);
                return CommandResult.Succeed();
            }
            catch (Exception ex) {
                ChangeState(FaultedState.state);
                Machine.TcsPause.SetException(ex);
                Machine.TcsPause = null;
                return CommandResult.WorkingPauseException(ex);
            }
        }

        protected internal override CommandResult Resume() {
            return CommandResult.WorkingResume;
        }

        protected internal override CommandResult Cancel() {
            Machine.TcsCancel = new();
            long cancelWaitAddUp = 0;
            while (!Machine.TcsCancel.IsWaitingForResult) {
                if (Machine.CurrentState == FinishedState.state) {
                    return CommandResult.WorkingCancelHasFinished;
                }
                if (Machine.CurrentState == FailedState.state) {
                    return CommandResult.WorkingCancelHasFailed;
                }
                if (Machine.CurrentState == FaultedState.state) {
                    return CommandResult.WorkingCancelHasFaulted;
                }

                Task.Delay(RetryInterval).Wait();
                cancelWaitAddUp += RetryInterval;
                if (cancelWaitAddUp >= Machine.MaxWaitForPauseOrCancel || cancelWaitAddUp <= 0) {
                    return CommandResult.WorkingCancelNotRespondInTime;
                }
            }

            try {
                Machine.OnCancel();
                Machine.CancellationHandled = false;
                long cancelHandlingWaitAddUp = 0;
                Machine.TcsCancel.SetResult(PauseOrCancelResult.Canceled);
                try {
                    while (!Machine.CancellationHandled.Value) {
                        if (Machine.CurrentState == FaultedState.state) {
                            return CommandResult.WorkingCancelHasFaulted;
                        }
                        Task.Delay(RetryInterval).Wait();
                        cancelHandlingWaitAddUp += RetryInterval;
                        if (cancelHandlingWaitAddUp >= Machine.MaxWaitForCancelHandling ||
                            cancelHandlingWaitAddUp <= 0) {
                            return CommandResult.WorkingCancelNotHandleInTime;
                        }
                    }
                    ChangeState(CanceledState.state);
                    return CommandResult.Succeed();
                }
                finally {
                    Machine.CancellationHandled = null;
                }
            }
            catch (Exception ex) {
                ChangeState(FaultedState.state);
                Machine.TcsCancel.SetException(ex);
                return CommandResult.WorkingCancelException(ex);
            }
            finally {
                Machine.TcsCancel = null;
            }
        }

        protected internal override CommandResult Restart() {
            CommandResult cancelResult = Cancel();
            if (!cancelResult.Success) {
                return cancelResult;
            }
            return base.Restart();
        }
    }

    internal sealed class PausingState : MachineState {
        internal static readonly State state = State.Pausing;
        protected internal override State State => state;

        internal PausingState(StateMachine machine) : base(machine) { }

        protected internal override void Initialize() {
            throw new InvalidOperationException();
        }

        protected internal override CommandResult Start() {
            return CommandResult.PausingStart;
        }

        protected internal override CommandResult Pause() {
            return CommandResult.PausingPause;
        }

        protected internal override CommandResult Resume() {
            try {
                Machine.OnResume();
                ChangeState(WorkingState.state);
#nullable disable
                Machine.TcsPause.SetResult(PauseOrCancelResult.Resumed);
#nullable enable
                return CommandResult.Succeed();
            }
            catch (Exception ex) {
                ChangeState(FaultedState.state);
#nullable disable
                Machine.TcsPause.SetException(ex);
#nullable enable
                return CommandResult.PausingResumeException(ex);
            }
            finally {
                Machine.TcsPause = null;
            }
        }

        protected internal override CommandResult Cancel() {
            try {
                Machine.OnCancel();
                Machine.CancellationHandled = false;
                long cancelHandlingWaitAddUp = 0;
#nullable disable
                Machine.TcsPause.SetResult(PauseOrCancelResult.Canceled);
#nullable enable
                try {
                    while (!Machine.CancellationHandled.Value) {
                        if (Machine.CurrentState == FaultedState.state) {
                            return CommandResult.WorkingCancelHasFaulted;
                        }
                        Task.Delay(RetryInterval).Wait();
                        cancelHandlingWaitAddUp += RetryInterval;
                        if (cancelHandlingWaitAddUp >= Machine.MaxWaitForCancelHandling ||
                            cancelHandlingWaitAddUp <= 0) {
                            return CommandResult.PausingCancelNotHandleInTime;
                        }
                    }
                    ChangeState(CanceledState.state);
                    return CommandResult.Succeed();
                }
                finally {
                    Machine.CancellationHandled = null;
                }
            }
            catch (Exception ex) {
                ChangeState(FaultedState.state);
#nullable disable
                Machine.TcsPause.SetException(ex);
#nullable enable
                return CommandResult.PausingCancelException(ex);
            }
            finally {
                Machine.TcsPause = null;
            }
        }

        protected internal override CommandResult Restart() {
            CommandResult cancelResult = Cancel();
            if (!cancelResult.Success) {
                return cancelResult;
            }
            return base.Restart();
        }
    }

    internal sealed class FinishedState : MachineState {
        internal static readonly State state = State.Finished;
        protected internal override State State => state;

        internal FinishedState(StateMachine machine) : base(machine) { }

        protected internal override void Initialize() {
            throw new InvalidOperationException();
        }

        protected internal override CommandResult Start() {
            return CommandResult.FinishedStart;
        }

        protected internal override CommandResult Pause() {
            return CommandResult.FinishedPause;
        }

        protected internal override CommandResult Resume() {
            return CommandResult.FinishedResume;
        }

        protected internal override CommandResult Cancel() {
            return CommandResult.FinishedCancel;
        }
    }

    internal sealed class CanceledState : MachineState {
        internal static readonly State state = State.Canceled;
        protected internal override State State => state;

        internal CanceledState(StateMachine machine) : base(machine) { }

        protected internal override void Initialize() {
            throw new InvalidOperationException();
        }

        protected internal override CommandResult Start() {
            return CommandResult.CanceledStart;
        }

        protected internal override CommandResult Pause() {
            return CommandResult.CanceledPause;
        }

        protected internal override CommandResult Resume() {
            return CommandResult.CanceledResume;
        }

        protected internal override CommandResult Cancel() {
            return CommandResult.CanceledCancel;
        }
    }

    internal sealed class FailedState : MachineState {
        internal static readonly State state = State.Failed;
        protected internal override State State => state;

        internal FailedState(StateMachine machine) : base(machine) { }

        protected internal override void Initialize() {
            throw new InvalidOperationException();
        }

        protected internal override CommandResult Start() {
            return CommandResult.FailedStart;
        }

        protected internal override CommandResult Pause() {
            return CommandResult.FailedPause;
        }

        protected internal override CommandResult Resume() {
            return CommandResult.FailedResume;
        }

        protected internal override CommandResult Cancel() {
            return CommandResult.FailedCancel;
        }
    }

    internal sealed class FaultedState : MachineState {
        internal static readonly State state = State.Faulted;
        protected internal override State State => state;

        internal FaultedState(StateMachine machine) : base(machine) { }

        protected internal override void Initialize() {
            throw new InvalidOperationException();
        }

        protected internal override CommandResult Start() {
            return CommandResult.FaultedStart;
        }

        protected internal override CommandResult Pause() {
            return CommandResult.FaultedPause;
        }

        protected internal override CommandResult Resume() {
            return CommandResult.FaultedResume;
        }

        protected internal override CommandResult Cancel() {
            return CommandResult.FaultedCancel;
        }
    }

    internal sealed class StateFactory {
        private readonly StateMachine Machine;
        internal StateFactory(StateMachine machine) {
            Machine = machine;
        }
        internal MachineState MakeState(State attachedState) {
            return attachedState switch {
                State.Initial => new InitialState(Machine),
                State.Initialized => new InitializedState(Machine),
                State.Working => new WorkingState(Machine),
                State.Pausing => new PausingState(Machine),
                State.Finished => new FinishedState(Machine),
                State.Canceled => new CanceledState(Machine),
                State.Failed => new FailedState(Machine),
                State.Faulted => new FaultedState(Machine),
                _ => throw new NotImplementedException()
            };
        }
    }

    /// <summary>
    /// Specifies states of an instance of the <see cref="StateMachine"/> class.
    /// </summary>
    public enum State {
        /// <summary>
        /// When a new instance of the <see cref="StateMachine"/> class is created,
        /// it has the <see cref="Initial"/> state.
        /// </summary>
        Initial,

        /// <summary>
        /// When a new instance of the <see cref="StateMachine"/> class is created successfully,
        /// it has the <see cref="Initialized"/> state.
        /// </summary>
        Initialized,

        /// <summary>
        /// When an instance of the <see cref="StateMachine"/> class is performing a task,
        /// it has the <see cref="Working"/> state.
        /// </summary>
        Working,

        /// <summary>
        /// When an instance of the <see cref="StateMachine"/> class gets paused while it is working,
        /// it has the <see cref="Pausing"/> state.
        /// </summary>
        Pausing,

        /// <summary>
        /// When an instance of the <see cref="StateMachine"/> class finishes its task,
        /// it has the <see cref="Finished"/> state.
        /// </summary>
        Finished,

        /// <summary>
        /// When an instance of the <see cref="StateMachine"/> class gets canceled,
        /// it has the <see cref="Canceled"/> state.
        /// </summary>
        Canceled,

        /// <summary>
        /// When an instance of the <see cref="StateMachine"/> class fails while performing
        /// the operation <see cref="StateMachine.OnInitialize"/> or
        /// <see cref="StateMachine.OnStart"/>, it has the <see cref="Failed"/> state.
        /// </summary>
        Failed,

        /// <summary>
        /// When an instance of the <see cref="StateMachine"/> class gets unhandled exceptions
        /// while performing an operation like <see cref="StateMachine.OnInitialize"/> or
        /// <see cref="StateMachine.OnStart"/> or <see cref="StateMachine.OnPause"/>
        /// or <see cref="StateMachine.OnResume"/> or <see cref="StateMachine.OnCancel"/>,
        /// it has the <see cref="Failed"/> state.
        /// </summary>
        Faulted
    }
}