using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Peanut.Libs.Game {
    internal static class TimerHelper {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtQueryTimerResolution(out uint MinimumResolution, out uint MaximumResolution, out uint CurrentResolution);

        private static readonly double LowestSleepThreshold;

        static TimerHelper() {
            _ = NtQueryTimerResolution(out uint min, out uint max, out uint current);
            LowestSleepThreshold = 1.0 + (max / 10000.0);
        }

        /// <summary>
        /// Returns the current timer resolution in milliseconds
        /// </summary>
        public static double GetCurrentResolution() {
            NtQueryTimerResolution(out uint min, out uint max, out uint current);
            return current / 10000.0;
        }

        /// <summary>
        /// Sleeps as long as possible without exceeding the specified period
        /// </summary>
        public static void SleepForNoMoreThan(double milliseconds) {
            // Assumption is that Thread.Sleep(t) will sleep for at least (t), and at most (t + timerResolution)
            if (milliseconds < LowestSleepThreshold)
                return;
            var sleepTime = (int)(milliseconds - GetCurrentResolution());
            if (sleepTime > 0)
                Thread.Sleep(sleepTime);
        }
    }
}
