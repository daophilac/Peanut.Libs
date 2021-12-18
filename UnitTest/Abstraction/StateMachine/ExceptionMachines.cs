using NUnit.Framework;
using Peanut.Libs.Abstraction.StateMachine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Abstraction.StateMachine {
    #region working exception
    internal class WorkingExceptionMachine : Peanut.Libs.Abstraction.StateMachine.StateMachine {
        internal int LoopCount { get; private set; }

        protected override async Task<OperationResult> OnStart() {
            for (LoopCount = 0; LoopCount < 5; LoopCount++) {
                if (LoopCount == 2) {
                    throw new TestException();
                }
                var result = await PauseOrCancelIfRequested();
                await Task.Delay(1000);
            }
            Console.WriteLine("Finished");
            return OperationResult.Succeed();
        }
    }

    [Parallelizable(ParallelScope.All)]
    internal class TestWorkingExceptionMachine : TestMachine {
        [Test]
        public async Task WorkingException() {
            using Scope<WorkingExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(2500);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Faulted), G(machine.StateChanges));
        }

        [Test]
        public async Task OnExceptionStart() {
            using Scope<WorkingExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(2500);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            var result = machine.Start();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Faulted), G(machine.StateChanges));
        }

        [Test]
        public async Task OnExceptionPause() {
            using Scope<WorkingExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(2500);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Faulted), G(machine.StateChanges));
        }

        [Test]
        public async Task OnExceptionResume() {
            using Scope<WorkingExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(2500);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            var result = machine.Resume();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Faulted), G(machine.StateChanges));
        }

        [Test]
        public async Task OnExceptionCancel() {
            using Scope<WorkingExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(2500);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            var result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Faulted), G(machine.StateChanges));
        }
    }
    #endregion

    #region resume handle exception
    internal class ResumeHandleExceptionMachine : Peanut.Libs.Abstraction.StateMachine.StateMachine {
        internal long ElapsedTime { get; private set; }
        internal int LoopCount { get; private set; }

        protected override async Task<OperationResult> OnStart() {
            Console.WriteLine(nameof(OnStart));
            Stopwatch stopwatch = new();
            stopwatch.Start();
            for (LoopCount = 0; LoopCount < 5; LoopCount++) {
                var result = await PauseOrCancelIfRequested();
                if (result != null) {
                    if (result.HasResumed) {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasResumed));
                        throw new TestException();
                    }
                    else {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasCanceled));
                        break;
                    }
                }
                await Task.Delay(1000);
            }
            stopwatch.Stop();
            ElapsedTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine("Finished");
            return OperationResult.Succeed();
        }
    }

    [Parallelizable(ParallelScope.All)]
    internal class TestResumeHandleExceptionMachine : TestMachine {
        [Test]
        public async Task ResumeHandleException() {
            using Scope<ResumeHandleExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            result = machine.Resume();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(1500);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Pausing, State.Working, State.Faulted), G(machine.StateChanges));
        }
    }
    #endregion

    #region cancel handle exception
    internal class CancelHandleExceptionMachine : Peanut.Libs.Abstraction.StateMachine.StateMachine {
        internal long ElapsedTime { get; private set; }
        internal int LoopCount { get; private set; }

        protected override async Task<OperationResult> OnStart() {
            Console.WriteLine(nameof(OnStart));
            Stopwatch stopwatch = new();
            stopwatch.Start();
            for (LoopCount = 0; LoopCount < 5; LoopCount++) {
                var result = await PauseOrCancelIfRequested();
                if (result != null) {
                    if (result.HasResumed) {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasResumed));
                    }
                    else {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasCanceled));
                        throw new TestException();
                    }
                }
                await Task.Delay(1000);
            }
            stopwatch.Stop();
            ElapsedTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine("Finished");
            return OperationResult.Succeed();
        }
    }

    [Parallelizable(ParallelScope.All)]
    internal class TestCancelHandleExceptionMachine : TestMachine {
        [Test]
        public void WorkingCancelHandleException() {
            using Scope<CancelHandleExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Faulted), G(machine.StateChanges));
        }

        [Test]
        public void PausingCancelHandleException() {
            using Scope<CancelHandleExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Pausing, State.Faulted), G(machine.StateChanges));
        }
    }
    #endregion

    #region waiting for pause exception
    internal class WaitingForPauseExceptionMachine : Peanut.Libs.Abstraction.StateMachine.StateMachine {
        internal long ElapsedTime { get; private set; }
        internal int LoopCount { get; private set; }

        protected override async Task<OperationResult> OnStart() {
            Console.WriteLine(nameof(OnStart));
            Stopwatch stopwatch = new();
            stopwatch.Start();
            for (LoopCount = 0; LoopCount < 5; LoopCount++) {
                await Task.Delay(1000);
                if (LoopCount == 2) {
                    Console.WriteLine("nnnnn");
                    throw new TestException();
                }
                var result = await PauseOrCancelIfRequested();
                if (result != null) {
                    if (result.HasResumed) {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasResumed));
                    }
                    else {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasCanceled));
                        break;
                    }
                }
            }
            stopwatch.Stop();
            ElapsedTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine("Finished");
            return OperationResult.Succeed();
        }
    }

    [Parallelizable(ParallelScope.All)]
    internal class TestWaitingForPauseExceptionMachine : TestMachine {
        [Test]
        public async Task WaitingForPauseException() {
            using Scope<WaitingForPauseExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(2500);
            var result = machine.Pause();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Faulted), G(machine.StateChanges));
        }
    }
    #endregion

    #region waiting for cancel exception
    internal class WaitingForCancelExceptionMachine : Peanut.Libs.Abstraction.StateMachine.StateMachine {
        internal long ElapsedTime { get; private set; }
        internal int LoopCount { get; private set; }

        protected override async Task<OperationResult> OnStart() {
            Console.WriteLine(nameof(OnStart));
            Stopwatch stopwatch = new();
            stopwatch.Start();
            for (LoopCount = 0; LoopCount < 5; LoopCount++) {
                await Task.Delay(1000);
                if (LoopCount == 2) {
                    Console.WriteLine("nnnnn");
                    throw new TestException();
                }
                var result = await PauseOrCancelIfRequested();
                if (result != null) {
                    if (result.HasResumed) {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasResumed));
                    }
                    else {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasCanceled));
                        break;
                    }
                }
            }
            stopwatch.Stop();
            ElapsedTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine("Finished");
            return OperationResult.Succeed();
        }
    }

    [Parallelizable(ParallelScope.All)]
    internal class TestWaitingForCancelExceptionMachine : TestMachine {
        [Test]
        public async Task WaitingForCancelException() {
            using Scope<WaitingForCancelExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(2500);
            var result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Faulted), G(machine.StateChanges));
        }
    }
    #endregion

    #region on initialize exception
    internal class OnInitializeExceptionMachine : Peanut.Libs.Abstraction.StateMachine.StateMachine {
        internal long ElapsedTime { get; private set; }
        internal int LoopCount { get; private set; }

        protected override OperationResult OnInitialize() {
            Console.WriteLine(nameof(OnInitialize));
            throw new TestException();
        }

        protected override Task<OperationResult> OnStart() {
            Console.WriteLine(nameof(OnStart));
            return Task.FromResult(OperationResult.Succeed());
        }
    }

    [Parallelizable(ParallelScope.All)]
    internal class TestOnInitializeExceptionMachine : TestMachine {
        [Test]
        public void OnInitializeException() {
            Assert.Throws<System.Reflection.TargetInvocationException>(() => {
                Scope<OnInitializeExceptionMachine> scope = new();
            });
        }
    }
    #endregion

    #region on pause exception
    internal class OnPauseExceptionMachine : Peanut.Libs.Abstraction.StateMachine.StateMachine {
        internal long ElapsedTime { get; private set; }
        internal int LoopCount { get; private set; }

        protected override async Task<OperationResult> OnStart() {
            Console.WriteLine(nameof(OnStart));
            Stopwatch stopwatch = new();
            stopwatch.Start();
            for (LoopCount = 0; LoopCount < 5; LoopCount++) {
                var result = await PauseOrCancelIfRequested();
                if (result != null) {
                    if (result.HasResumed) {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasResumed));
                    }
                    else {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasCanceled));
                        break;
                    }
                }
                await Task.Delay(1000);
            }
            stopwatch.Stop();
            ElapsedTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine("Finished");
            return OperationResult.Succeed();
        }

        protected override void OnPause() {
            Console.WriteLine(nameof(OnPause));
            throw new TestException();
        }
    }

    [Parallelizable(ParallelScope.All)]
    internal class TestOnPauseExceptionMachine : TestMachine {
        [Test]
        public void OnPauseException() {
            Scope<OnPauseExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Assert.IsNotNull(result.Error.Exception);
            Console.WriteLine(result);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Faulted), G(machine.StateChanges));
        }
    }
    #endregion

    #region on resume exception
    internal class OnResumeExceptionMachine : Peanut.Libs.Abstraction.StateMachine.StateMachine {
        internal long ElapsedTime { get; private set; }
        internal int LoopCount { get; private set; }

        protected override async Task<OperationResult> OnStart() {
            Console.WriteLine(nameof(OnStart));
            Stopwatch stopwatch = new();
            stopwatch.Start();
            for (LoopCount = 0; LoopCount < 5; LoopCount++) {
                var result = await PauseOrCancelIfRequested();
                if (result != null) {
                    if (result.HasResumed) {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasResumed));
                    }
                    else {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasCanceled));
                        break;
                    }
                }
                await Task.Delay(1000);
            }
            stopwatch.Stop();
            ElapsedTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine("Finished");
            return OperationResult.Succeed();
        }

        protected override void OnResume() {
            Console.WriteLine(nameof(OnResume));
            throw new TestException();
        }
    }

    [Parallelizable(ParallelScope.All)]
    internal class TestOnResumeExceptionMachine : TestMachine {
        [Test]
        public void OnResumeException() {
            Scope<OnResumeExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            result = machine.Resume();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Assert.IsNotNull(result.Error.Exception);
            Console.WriteLine(result);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Pausing, State.Faulted), G(machine.StateChanges));
        }
    }
    #endregion

    #region on cancel exception
    internal class OnCancelExceptionMachine : Peanut.Libs.Abstraction.StateMachine.StateMachine {
        internal long ElapsedTime { get; private set; }
        internal int LoopCount { get; private set; }

        protected override async Task<OperationResult> OnStart() {
            Console.WriteLine(nameof(OnStart));
            Stopwatch stopwatch = new();
            stopwatch.Start();
            for (LoopCount = 0; LoopCount < 5; LoopCount++) {
                var result = await PauseOrCancelIfRequested();
                if (result != null) {
                    if (result.HasResumed) {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasResumed));
                    }
                    else {
                        await Task.Delay(1000);
                        Console.WriteLine(nameof(result.HasCanceled));
                        break;
                    }
                }
                await Task.Delay(1000);
            }
            stopwatch.Stop();
            ElapsedTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine("Finished");
            return OperationResult.Succeed();
        }

        protected override void OnCancel() {
            Console.WriteLine(nameof(OnCancel));
            throw new TestException();
        }
    }

    [Parallelizable(ParallelScope.All)]
    internal class TestOnCancelExceptionMachine : TestMachine {
        [Test]
        public void InitializedOnCancelException() {
            Scope<OnCancelExceptionMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Initialized, machine.CurrentState);
            var result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Assert.IsNotNull(result.Error.Exception);
            Console.WriteLine(result);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Faulted), G(machine.StateChanges));
        }

        [Test]
        public void InitializedRestartException() {
            Scope<OnCancelExceptionMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Initialized, machine.CurrentState);
            var result = machine.Restart();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Assert.IsNotNull(result.Error.Exception);
            Console.WriteLine(result);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Faulted), G(machine.StateChanges));
        }

        [Test]
        public void WorkingOnCancelException() {
            Scope<OnCancelExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Assert.IsNotNull(result.Error.Exception);
            Console.WriteLine(result);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Faulted), G(machine.StateChanges));
        }

        [Test]
        public void WorkingRestartException() {
            Scope<OnCancelExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Restart();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Assert.IsNotNull(result.Error.Exception);
            Console.WriteLine(result);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Faulted), G(machine.StateChanges));
        }

        [Test]
        public void PausingOnCancelException() {
            Scope<OnCancelExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Assert.IsNotNull(result.Error.Exception);
            Console.WriteLine(result);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Pausing, State.Faulted), G(machine.StateChanges));
        }

        [Test]
        public void PausingRestartException() {
            Scope<OnCancelExceptionMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            result = machine.Restart();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Assert.IsNotNull(result.Error.Exception);
            Console.WriteLine(result);
            Assert.AreEqual(State.Faulted, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Pausing, State.Faulted), G(machine.StateChanges));
        }
    }
    #endregion
    internal class TestException : Exception {
        internal TestException() : base() {

        }

        internal TestException(string message) : base(message) {

        }

        internal TestException(string message, Exception innerException) : base(message, innerException) {

        }
    }
}
