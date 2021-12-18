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
    public class SlowGame : Peanut.Libs.Game.Game {
        public SlowGame() {
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000f / 30);
        }
        protected override void Update(GameTime gameTime) {
            Console.WriteLine($"{nameof(Update)}: Elapsed time: {gameTime.ElapsedGameTime.TotalMilliseconds}. Total time: {gameTime.TotalGameTime.TotalMilliseconds}. {(gameTime.IsRunningSlowly ? "Slow" : "")}");
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            Console.WriteLine($"{nameof(Draw)}: Elapsed time: {gameTime.ElapsedGameTime.TotalMilliseconds}. Total time: {gameTime.TotalGameTime.TotalMilliseconds}");
            Thread.Sleep(1000 / 10);
            base.Draw(gameTime);
        }
    }

    public class TestSlowGame {
        [Test]
        public void GameRunningSlow() {
            SlowGame game = new();
            Task.Run(async () => {
                await Task.Delay(5000);
                game.Exit();
            });
            game.Run();
            Assert.IsTrue(true);
        }
    }
}
