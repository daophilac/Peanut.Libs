using System.Diagnostics;
using System.Reflection;

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
        public static Type? GetTypeOfCallingClass(int skipFrames) {
            StackFrame stackFrame = new(2 + skipFrames);
            if (stackFrame.GetMethod() is MethodBase method) {
                return method.DeclaringType;
            }
            return null;
        }

        /// <summary>
        /// Gets the type of the calling class.<br/>
        /// </summary>
        /// <returns>The type of the calling class.</returns>
        public static Type? GetTypeOfCallingClass() {
            return GetTypeOfCallingClass(0);
        }
    }
}
