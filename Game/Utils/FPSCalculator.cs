using Peanut.Libs.Specialized;
using System;
using System.Linq;

namespace Peanut.Libs.Game.Utils {
    /// <summary>
    /// An FPS calculator that uses the averaging over previous fps values approach.<br/>
    /// </summary>
    public class FPSCalculator {
        /// <summary>
        /// Gets or sets the amount of previous fps that need to take into account to determine the
        /// current fps of the running game.<br/>
        /// </summary>
        public int AverageOver {
            get => averageOver;
            set {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeException(nameof(AverageOver));
                }
                averageOver = value;
                fpsList = new(AverageOver);
            }
        }
        private int averageOver;

        private CircularLinkedList<double> fpsList;

        /// <summary>
        /// Initializes a new instance of the <see cref="FPSCalculator"/> class.<br/>
        /// </summary>
        /// <param name="averageOver">
        ///     The amount of previous fps that need to take into account to determine the
        ///     current fps of the running game.
        ///     The higher the number, the slower the changes on fps, but the changes are smooth.
        ///     The lower the number, the faster the changes on fps, but the changes are chaotic.
        /// </param>
#nullable disable
        public FPSCalculator(int averageOver = 1) {
#nullable enable
            AverageOver = averageOver;
        }

        /// <summary>
        /// Re-calculate the fps value at a frame.<br/>
        /// </summary>
        /// <param name="gameTime">The game time of a frame.</param>
        /// <returns>The calculated fps value.</returns>
        public double Recalculate(GameTime gameTime) {
            double fps = TimeSpan.FromSeconds(1) / gameTime.ElapsedGameTime;
            fpsList.SetNextAndAdvance(fps);
            double averageFps = fpsList.Values.Average();
            return averageFps;
        }
    }
}
