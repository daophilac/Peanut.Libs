using NUnit.Framework;
using Peanut.Libs.Specialized;
using Peanut.Libs.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Game {
    public class Game30FPS : Peanut.Libs.Game.Game {
        public Game30FPS() {
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000f / 30);
        }
        protected override void Update(GameTime gameTime) {
            Console.WriteLine($"{nameof(Update)}: Elapsed time: {gameTime.ElapsedGameTime.TotalMilliseconds}. Total time: {gameTime.TotalGameTime.TotalMilliseconds}");
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            Console.WriteLine($"{nameof(Draw)}: Elapsed time: {gameTime.ElapsedGameTime.TotalMilliseconds}. Total time: {gameTime.TotalGameTime.TotalMilliseconds}");
            base.Draw(gameTime);
        }
    }

    public class TestGame30FPS {
        [Test]
        public void GameRunning30FPS() {
            Game30FPS game = new();
            Task.Run(async () => {
                await Task.Delay(5000);
                game.Exit();
            });
            game.Run();
            Assert.IsTrue(true);
        }
    }
}
