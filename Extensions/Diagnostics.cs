using System.Diagnostics;

namespace Peanut.Libs.Extensions {
    /// <summary>
    /// Provides extension methods related to diagnostics.<br/>
    /// </summary>
    public static class Diagnostics {
        /// <summary>
        /// Gets the type of the calling class.<br/>
        /// </summary>
        /// <param name="skipFrames">The amount of frames to be skipped.</param>
        /// <returns>The type of the calling class.</returns>
        public static Type GetTypeOfCallingClass(int skipFrames) {
#nullable disable
            return new StackFrame(2 + skipFrames).GetMethod().DeclaringType;
#nullable enable
        }

        /// <summary>
        /// Gets the type of the calling class.<br/>
        /// </summary>
        /// <returns>The type of the calling class.</returns>
        public static Type GetTypeOfCallingClass() {
            return GetTypeOfCallingClass(0);
        }
    }
}
