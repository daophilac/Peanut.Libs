using NUnit.Framework;
using Peanut.Libs.Abstraction.StateMachine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest.Abstraction.StateMachine {
    internal class ProperMachine : Peanut.Libs.Abstraction.StateMachine.StateMachine {
        public long ElapsedTime { get; private set; }
        public int LoopCount { get; private set; }

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
    }

    internal class TestMachine {
        protected static string GenerateStateChangesRepresentation(IEnumerable<State> stateChanges) {
            return string.Join("->", stateChanges);
        }
        protected static string G(IEnumerable<State> stateChanges) {
            return GenerateStateChangesRepresentation(stateChanges);
        }
        protected static IEnumerable<State> CreateStateChanges(params State[] states) {
            return new LinkedList<State>(states);
        }
        protected static IEnumerable<State> C(params State[] states) {
            return CreateStateChanges(states);
        }
        protected static string CG(params State[] states) {
            return G(C(states));
        }

        protected class Scope<T> : IDisposable where T : Peanut.Libs.Abstraction.StateMachine.StateMachine, new() {
            public T Machine { get; }
            public Scope(bool startImmediately = true) {
                Machine = new();
                Machine.MaxWaitForPauseOrCancel = 1500;
                if (startImmediately) {
                    Machine.Start();
                }
            }

            public void Dispose() {
                //Console.WriteLine("Dispose gets called");
            }
        }
    }

    [Parallelizable(ParallelScope.All)]
    internal class TestProperMachine : TestMachine {
        #region possible user actions
        [Test]
        public void InitializedCancel() {
            using Scope<ProperMachine> scope = new(false);
            var machine = scope.Machine;
            var result = machine.Cancel();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Canceled, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Canceled), G(machine.StateChanges));
            Assert.Less(machine.ElapsedTime, 500);
        }

        [Test]
        public async Task StartFinish() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            await Task.Delay(5500);
            Assert.AreEqual(State.Finished, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Finished), G(machine.StateChanges));
        }

        [Test]
        public void WorkingCancel() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Cancel();
            Assert.IsTrue(result.Success);
            Console.WriteLine(result);
            Assert.AreEqual(State.Canceled, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Canceled), G(machine.StateChanges));
            Assert.Less(machine.ElapsedTime, 3000);
        }

        [Test]
        public async Task StartPauseResumeFinish() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(1000);
            var result = machine.Pause();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            await Task.Delay(1000);
            machine.Resume();
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(4500);
            Assert.AreEqual(State.Finished, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Pausing, State.Working, State.Finished), G(machine.StateChanges));
            Console.WriteLine(G(machine.StateChanges));
        }

        [Test]
        public async Task StartPauseResumeFinishWithoutWaiting() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            machine.Resume();
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(6100);
            Assert.AreEqual(State.Finished, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Pausing, State.Working, State.Finished), G(machine.StateChanges));
            Console.WriteLine(G(machine.StateChanges));
        }

        [Test]
        public async Task StartPauseCancel() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            result = machine.Cancel();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Canceled, machine.CurrentState);
            await Task.Delay(6100);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Pausing, State.Canceled), G(machine.StateChanges));
        }

        [Test]
        public void PauseNotRespondedInTime() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            machine.MaxWaitForPauseOrCancel = 500;
            var result = machine.Pause();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Working, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working), G(machine.StateChanges));
        }

        [Test]
        public void WorkingCancelNotRespondedInTime() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            machine.MaxWaitForPauseOrCancel = 500;
            var result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Working, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working), G(machine.StateChanges));
        }

        [Test]
        public void WorkingCancelNotHandledInTime() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            machine.MaxWaitForCancelHandling = 500;
            var result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Working, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working), G(machine.StateChanges));
        }

        [Test]
        public void PausingCancelNotHandledInTime() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            machine.MaxWaitForCancelHandling = 500;
            result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Pausing), G(machine.StateChanges));
        }

        [Test]
        public async Task WaitingForPauseButFinished() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(4500);
            var result = machine.Pause();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Finished, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Finished), G(machine.StateChanges));
        }

        [Test]
        public async Task WaitingForCancelButFinished() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(4500);
            var result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Finished, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Finished), G(machine.StateChanges));
        }
        #endregion

        #region restart related
        [Test]
        public void InitializedRestart() {
            using Scope<ProperMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Initialized, machine.CurrentState);
            var result = machine.Restart();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Working, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Canceled, State.Initial, State.Initialized, State.Working), G(machine.StateChanges));
        }

        [Test]
        public void WorkingRestart() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Restart();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Working, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Canceled, State.Initial, State.Initialized, State.Working), G(machine.StateChanges));
        }

        [Test]
        public async Task PausingRestart() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            result = machine.Restart();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(1500);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Pausing, State.Canceled, State.Initial, State.Initialized, State.Working), G(machine.StateChanges));
        }

        [Test]
        public async Task FinishedRestart() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(5500);
            Assert.AreEqual(State.Finished, machine.CurrentState);
            var result = machine.Restart();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Working, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Finished, State.Initial, State.Initialized, State.Working), G(machine.StateChanges));
        }

        [Test]
        public void CanceledRestart() {
            using Scope<ProperMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Initialized, machine.CurrentState);
            var result = machine.Cancel();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Canceled, machine.CurrentState);
            result = machine.Restart();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Working, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Canceled, State.Initial, State.Initialized, State.Working), G(machine.StateChanges));
        }
        #endregion

        #region false commands
        [Test]
        public void InitializedPause() {
            using Scope<ProperMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Initialized, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized), G(machine.StateChanges));
        }

        [Test]
        public void InitializedResume() {
            using Scope<ProperMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Initialized, machine.CurrentState);
            var result = machine.Resume();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized), G(machine.StateChanges));
        }

        [Test]
        public void WorkingStart() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Start();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Working, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working), G(machine.StateChanges));
        }

        [Test]
        public void WorkingResume() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Resume();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Working, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working), G(machine.StateChanges));
        }

        [Test]
        public void PausingStart() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            result = machine.Start();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Pausing), G(machine.StateChanges));
        }

        [Test]
        public void PausingPause() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            result = machine.Pause();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Pausing, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Pausing), G(machine.StateChanges));
        }

        [Test]
        public async Task FinishedStart() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(5500);
            Assert.AreEqual(State.Finished, machine.CurrentState);
            var result = machine.Start();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Finished), G(machine.StateChanges));
        }

        [Test]
        public async Task FinishedPause() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(5500);
            Assert.AreEqual(State.Finished, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Finished), G(machine.StateChanges));
        }

        [Test]
        public async Task FinishedResume() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(6100);
            Assert.AreEqual(State.Finished, machine.CurrentState);
            var result = machine.Resume();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Finished), G(machine.StateChanges));
        }

        [Test]
        public async Task FinishedCancel() {
            using Scope<ProperMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(5500);
            Assert.AreEqual(State.Finished, machine.CurrentState);
            var result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Finished), G(machine.StateChanges));
        }

        [Test]
        public void CanceledStart() {
            using Scope<ProperMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Initialized, machine.CurrentState);
            var result = machine.Cancel();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Canceled, machine.CurrentState);
            result = machine.Start();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Canceled), G(machine.StateChanges));
        }

        [Test]
        public void CanceledPause() {
            using Scope<ProperMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Initialized, machine.CurrentState);
            var result = machine.Cancel();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Canceled, machine.CurrentState);
            result = machine.Pause();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Canceled), G(machine.StateChanges));
        }

        [Test]
        public void CanceledResume() {
            using Scope<ProperMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Initialized, machine.CurrentState);
            var result = machine.Cancel();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Canceled, machine.CurrentState);
            result = machine.Resume();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Canceled), G(machine.StateChanges));
        }

        [Test]
        public void CanceledCancel() {
            using Scope<ProperMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Initialized, machine.CurrentState);
            var result = machine.Cancel();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(State.Canceled, machine.CurrentState);
            result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Canceled), G(machine.StateChanges));
        }
        #endregion
    }
}
