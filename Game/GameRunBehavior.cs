using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peanut.Libs.Game {
    /// <summary>
    /// Defines how <see cref="Game"/> should be runned.
    /// </summary>
    public enum GameRunBehavior {
        /// <summary>
        /// The game loop will be runned asynchronous.
        /// </summary>
        Asynchronous,
        /// <summary>
        /// The game loop will be runned synchronous.
        /// </summary>
        Synchronous
    }
}
