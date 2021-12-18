using NUnit.Framework;
using Peanut.Libs.Abstraction.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Abstraction.StateMachine {
    #region initialize fail
    internal class OnInitializeFailMachine : Peanut.Libs.Abstraction.StateMachine.StateMachine {
        protected override OperationResult OnInitialize() {
            Console.WriteLine(nameof(OnInitialize));
            return OperationResult.Fail(new($"{nameof(OnInitialize)} has failed."));
        }

        protected override Task<OperationResult> OnStart() {
            Console.WriteLine(nameof(OnStart));
            return Task.FromResult(OperationResult.Succeed());
        }
    }

    [Parallelizable(ParallelScope.All)]
    internal class TestOnInitializeFailMachine : TestMachine {
        [Test]
        public void InitializeFail() {
            using Scope<OnInitializeFailMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Failed, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Failed), G(machine.StateChanges));
        }

        [Test]
        public void OnInitializeFailedStart() {
            using Scope<OnInitializeFailMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Failed, machine.CurrentState);
            var result = machine.Start();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Failed), G(machine.StateChanges));
        }

        [Test]
        public void OnInitializedFailedPause() {
            using Scope<OnInitializeFailMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Failed, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Failed), G(machine.StateChanges));
        }

        [Test]
        public void OnInitializedFailedResume() {
            using Scope<OnInitializeFailMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Failed, machine.CurrentState);
            var result = machine.Resume();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Failed), G(machine.StateChanges));
        }

        [Test]
        public void OnInitializeFailedCancel() {
            using Scope<OnInitializeFailMachine> scope = new(false);
            var machine = scope.Machine;
            Assert.AreEqual(State.Failed, machine.CurrentState);
            var result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(CG(State.Initial, State.Failed), G(machine.StateChanges));
        }
    }
    #endregion

    #region working fail
    internal class WorkingFailMachine : Peanut.Libs.Abstraction.StateMachine.StateMachine {

        protected override async Task<OperationResult> OnStart() {
            for (int i = 0; i < 5; i++) {
                await PauseOrCancelIfRequested();
                await Task.Delay(1000);
            }
            return OperationResult.Fail(new($"{nameof(OnStart)} has failed."));
        }
    }

    [Parallelizable(ParallelScope.All)]
    internal class TestWorkingFailMachine : TestMachine {
        [Test]
        public async Task WorkingFail() {
            using Scope<WorkingFailMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(5500);
            Assert.AreEqual(State.Failed, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Failed), G(machine.StateChanges));
        }
    }
    #endregion

    #region waiting for pause or cancel but failed
    internal class WaitingFailMachine : Peanut.Libs.Abstraction.StateMachine.StateMachine {

        protected override async Task<OperationResult> OnStart() {
            for (int i = 0; i < 5; i++) {
                if (i == 2) {
                    return OperationResult.Fail(new($"{nameof(OnStart)} has failed."));
                }
                await PauseOrCancelIfRequested();
                await Task.Delay(1000);
            }
            return OperationResult.Succeed();
        }
    }

    [Parallelizable(ParallelScope.All)]
    internal class TestWaitingFailMachine : TestMachine {
        [Test]
        public async Task WaitingForPauseButFailed() {
            using Scope<WaitingFailMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(1500);
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Pause();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Failed, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Failed), G(machine.StateChanges));
        }

        [Test]
        public async Task WaitingForCancelButFailed() {
            using Scope<WaitingFailMachine> scope = new();
            var machine = scope.Machine;
            Assert.AreEqual(State.Working, machine.CurrentState);
            await Task.Delay(1500);
            Assert.AreEqual(State.Working, machine.CurrentState);
            var result = machine.Cancel();
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Console.WriteLine(result);
            Assert.AreEqual(State.Failed, machine.CurrentState);
            Assert.AreEqual(CG(State.Initial, State.Initialized, State.Working, State.Failed), G(machine.StateChanges));
        }
    }
    #endregion
}
