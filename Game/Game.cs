using System;
using System.Diagnostics;

namespace Peanut.Libs.Game {
    /// <summary>
    /// A simplified version of the Game class from the MonoGame framework.<br/>
    /// This class provides a fancy way for Windows applications to update their UI at a desired rate,
    /// just like how a game calculates physics and renders frames.<br/>
    /// </summary>
    public class Game {
        /// <summary>
        /// Indicates if this game is running with a fixed time between frames.
        /// When set to true the target time between frames is
        /// given by <see cref="TargetElapsedTime"/>.
        /// </summary>
        public bool IsFixedTimeStep { get; set; } = true;

        /// <summary>
        /// The time between frames when running with a fixed time step.
        /// <seealso cref="IsFixedTimeStep"/>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Target elapsed time must be strictly larger than zero.
        /// </exception>
        public TimeSpan TargetElapsedTime {
            get { return targetElapsedTime; }
            set {
                if (value <= TimeSpan.Zero) {
                    throw new ArgumentOutOfRangeException(
                        "The time must be positive and non-zero.", default(Exception));
                }

                if (value > maxElapsedTime) {
                    throw new ArgumentOutOfRangeException(
                        $"The time can not be larger than {nameof(MaxElapsedTime)}",
                        default(Exception));
                }

                if (value != targetElapsedTime) {
                    targetElapsedTime = value;
                }
            }
        }
        private TimeSpan targetElapsedTime = TimeSpan.FromTicks(166667); // 60fps

        /// <summary>
        /// The maximum amount of time we will frameskip over and only perform Update calls
        /// with no Draw calls.
        /// MonoGame extension.
        /// </summary>
        public TimeSpan MaxElapsedTime {
            get { return maxElapsedTime; }
            set {
                if (value < TimeSpan.Zero) {
                    throw new ArgumentOutOfRangeException(
                        "The time must be positive.", default(Exception));
                }

                if (value < targetElapsedTime) {
                    throw new ArgumentOutOfRangeException(
                        $"The time must be at least {nameof(TargetElapsedTime)}",
                        default(Exception));
                }

                maxElapsedTime = value;
            }
        }
        private TimeSpan maxElapsedTime = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Indicates if the game is the focused application.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the duration of sleep when the game is inactive.<br/>
        /// </summary>
        public TimeSpan InactiveSleepTime {
            get { return inactiveSleepTime; }
            set {
                if (value < TimeSpan.Zero) {
                    throw new ArgumentOutOfRangeException(
                        "The time must be positive.",
                        default(Exception));
                }

                inactiveSleepTime = value;
            }
        }
        private TimeSpan inactiveSleepTime = TimeSpan.FromSeconds(0.02);

        private Stopwatch? gameTimer;
        private readonly GameTime gameTime = new();
        private TimeSpan accumulatedElapsedTime;
        private long previousTicks;
        private int updateFrameLag;
        private bool suppressDraw;
        private bool shouldExit;

        /// <summary>
        /// Runs the game using the default <see cref="GameRunBehavior"/> for the current platform.
        /// </summary>
        public void Run() {
            Run(GameRunBehavior.Synchronous);
        }

        /// <summary>
        /// Runs the game asynchronously.<br/>
        /// </summary>
        public void RunAsynchronous() {
            Run(GameRunBehavior.Asynchronous);
        }

        /// <summary>
        /// Exit the game at the end of this tick.
        /// </summary>
        public void Exit() {
            shouldExit = true;
            suppressDraw = true;
        }

        /// <summary>
        /// Reset the elapsed game time to <see cref="TimeSpan.Zero"/>.
        /// </summary>
        public void ResetElapsedTime() {
            if (gameTimer != null) {
                gameTimer.Reset();
                gameTimer.Start();
            }

            accumulatedElapsedTime = TimeSpan.Zero;
            gameTime.ElapsedGameTime = TimeSpan.Zero;
            previousTicks = 0L;
        }

        /// <summary>
        /// Run the game.
        /// </summary>
        /// <param name="runBehavior">Indicate if the game should be run synchronously or asynchronously.</param>
        public void Run(GameRunBehavior runBehavior) {
            switch (runBehavior) {
                case GameRunBehavior.Asynchronous:
                    throw new NotSupportedException(
                        "The Windows platform does not support asynchronous run loops");
                case GameRunBehavior.Synchronous:
                    // XNA runs one Update even before showing the window
                    Update(new GameTime());

                    RunLoop();
                    break;
                default:
                    throw new ArgumentException(string.Format(
                        "Handling for the run behavior {0} is not implemented.", runBehavior));
            }
        }

        private void RunLoop() {
            if (gameTimer == null) {
                gameTimer = Stopwatch.StartNew();
            }
            do {
                Tick();
            }
            while (!shouldExit);
        }

        /// <summary>
        /// Run one iteration of the game loop.
        ///
        /// Makes at least one call to <see cref="Update"/>
        /// and exactly one call to <see cref="Draw"/> if drawing is not supressed.
        /// When <see cref="IsFixedTimeStep"/> is set to <code>false</code> this will
        /// make exactly one call to <see cref="Update"/>.
        /// </summary>
        public void Tick() {
        // NOTE: This code is very sensitive and can break very badly
        // with even what looks like a safe change. Be sure to test 
        // any change fully in both the fixed and variable timestep 
        // modes across multiple devices and platforms.

        RetryTick:
            if (!IsActive && (InactiveSleepTime.TotalMilliseconds >= 1.0)) {
                System.Threading.Thread.Sleep((int)InactiveSleepTime.TotalMilliseconds);
            }
#nullable disable
            long currentTicks = gameTimer.Elapsed.Ticks;
#nullable enable
            accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - previousTicks);
            
            previousTicks = currentTicks;

            if (IsFixedTimeStep && accumulatedElapsedTime < TargetElapsedTime) {
                // Sleep for as long as possible without overshooting the update time
                double sleepTime = (TargetElapsedTime - accumulatedElapsedTime).TotalMilliseconds;
                TimerHelper.SleepForNoMoreThan(sleepTime);
                // Keep looping until it's time to perform the next update
                goto RetryTick;
            }

            // Do not allow any update to take longer than our maximum.
            if (accumulatedElapsedTime > maxElapsedTime) {
                accumulatedElapsedTime = maxElapsedTime;
            }

            if (IsFixedTimeStep) {
                gameTime.ElapsedGameTime = TargetElapsedTime;
                int stepCount = 0;

                // Perform as many full fixed length time steps as we can.
                while (accumulatedElapsedTime >= TargetElapsedTime && !shouldExit) {
                    gameTime.TotalGameTime += TargetElapsedTime;
                    accumulatedElapsedTime -= TargetElapsedTime;
                    ++stepCount;

                    Update(gameTime);
                }

                // Every update after the first accumulates lag
                updateFrameLag += Math.Max(0, stepCount - 1);

                // If we think we are running slowly, wait until the lag clears before resetting it
                if (gameTime.IsRunningSlowly) {
                    if (updateFrameLag == 0) {
                        gameTime.IsRunningSlowly = false;
                    }
                }
                else if (updateFrameLag >= 5) {
                    //If we lag more than 5 frames, start thinking we are running slowly
                    gameTime.IsRunningSlowly = true;
                }

                // Every time we just do one update and one draw, then we are not running slowly,
                // so decrease the lag
                if (stepCount == 1 && updateFrameLag > 0) {
                    updateFrameLag--;
                }

                // Draw needs to know the total elapsed time
                // that occured for the fixed length updates.
                gameTime.ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
            }
            else {
                // Perform a single variable length update.
                gameTime.ElapsedGameTime = accumulatedElapsedTime;
                gameTime.TotalGameTime += accumulatedElapsedTime;
                accumulatedElapsedTime = TimeSpan.Zero;

                Update(gameTime);
            }

            // Draw unless the update suppressed it.
            if (suppressDraw) {
                suppressDraw = false;
            }
            else {
                Draw(gameTime);
            }
        }

        /// <summary>
        /// Called when the game should update.
        /// Override this to update your game.
        /// </summary>
        /// <param name="gameTime">
        ///     The elapsed time since the last call to <see cref="Update"/>.
        /// </param>
        protected virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Called when the game should draw a frame.
        /// Override this to render your game.
        /// </summary>
        /// <param name="gameTime">
        ///     A <see cref="GameTime"/> instance containing the elapsed time since the last call
        ///     to <see cref="Draw"/> and the total time elapsed since the game started.
        /// </param>
        protected virtual void Draw(GameTime gameTime) { }
    }
}
