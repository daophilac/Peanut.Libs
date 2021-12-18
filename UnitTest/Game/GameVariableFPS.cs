using NUnit.Framework;
using Peanut.Libs.Specialized;
using Peanut.Libs.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest.Game {
    internal class GameVariableFPS : Peanut.Libs.Game.Game {
        public GameVariableFPS() {
            IsFixedTimeStep = false;
        }
        protected override void Update(GameTime gameTime) {
            Random random = new Random();
            Thread.Sleep(random.Next(20, 100));
            Console.WriteLine($"{nameof(Update)}: Elapsed time: {gameTime.ElapsedGameTime.TotalMilliseconds}. Total time: {gameTime.TotalGameTime.TotalMilliseconds}");
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            Random random = new Random();
            Thread.Sleep(random.Next(20, 100));
            Console.WriteLine($"{nameof(Draw)}: Elapsed time: {gameTime.ElapsedGameTime.TotalMilliseconds}. Total time: {gameTime.TotalGameTime.TotalMilliseconds}");
            base.Draw(gameTime);
        }
    }

    internal class TestGameVariableFPS {
        [Test]
        public void GameRunningVariableFPS() {
            GameVariableFPS game = new();
            Task.Run(async () => {
                await Task.Delay(5000);
                game.Exit();
            });
            game.Run();
            Assert.IsTrue(true);
        }
    }
}
