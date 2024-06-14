using Microsoft.Win32;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Peanut.Libs.OS {
    /// <summary>
    /// Provides functionalities to deal with associated icons.
    /// </summary>
    public partial class FileAssiocationInfo {
#nullable disable
        /// <summary>
        /// Gets the ProgId.
        /// </summary>
        public string ProgId { get; private set; }

        /// <summary>
        /// Gets the application name.
        /// </summary>
        public string ApplicationName { get; private set; }
#nullable enable

#nullable disable
        /// <summary>
        /// Gets the executable path of the application.
        /// </summary>
        public string ApplicationPath { get; private set; }
#nullable enable

        /// <summary>
        /// Gets the list of found large icons.
        /// </summary>
        public Icon[]? IconsLarge { get; private set; }

        /// <summary>
        /// Gets the list of found small icons.
        /// </summary>
        public Icon[]? IconsSmall { get; private set; }

        /// <summary>
        /// Gets the icon large index which is used to display to program icon.
        /// </summary>
        public int IconIndex { get; private set; } = -1;

        /// <summary>
        /// Creates a bitmap using the found large icons and the <see cref="IconIndex"/>.
        /// </summary>
        /// <returns>The created bitmap.</returns>
        public Bitmap? CreateIconImage() {
            if (IconsLarge == null) {
                return null;
            }

            return IconsLarge[IconIndex].ToBitmap();
        }
    }

    [SupportedOSPlatform("windows")]
    public partial class FileAssiocationInfo {
        [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf8)]
        private static partial uint ExtractIconEx(string lpszFile, int nIconIndex, [Out] IntPtr[]? phiconLarge,
            [Out] IntPtr[]? phiconSmall, uint nIcons);

        [LibraryImport("user32.dll")]
        private static partial int DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// Gets a list of <see cref="FileAssiocationInfo"/> items with respect to a specific file extension.
        /// </summary>
        /// <param name="extension">The extension to be found.</param>
        /// <returns>The list of found items.</returns>
        public static List<FileAssiocationInfo>? GetAssociatedPrograms(string extension) {
            RegistryKey? extensionKey = Registry.ClassesRoot.OpenSubKey($".{extension.ToLower()}\\OpenWithProgIDs");
            if (extensionKey == null) {
                return null;
            }

            string[] progIds = extensionKey.GetValueNames();
            if (progIds.Length == 0) {
                return null;
            }

            List<FileAssiocationInfo> result = new();
            foreach (string progId in progIds) {
                RegistryKey? progKey = Registry.ClassesRoot.OpenSubKey($"{progId}\\Application");
                if (progKey != null) {
                    FileAssiocationInfo info = Construct(progKey);
                    info.ProgId = progId;
                    result.Add(info);
                }
                else {
                    progKey = Registry.ClassesRoot.OpenSubKey($"{progId}\\DefaultIcon");
                    if (progKey != null) {
                        FileAssiocationInfo info = Construct(progKey);
                        if (info.ApplicationPath != null
                            && info.ApplicationPath.ToLower().EndsWith(".exe")) {
                            info.ApplicationName = progId;
                            info.ProgId = progId;
                            result.Add(info);
                        }
                    }
                }
            }
            return result;
        }

        private static FileAssiocationInfo Construct(RegistryKey key) {
            string[] valueNames = key.GetValueNames();
            FileAssiocationInfo info = new();
            foreach (string name in valueNames) {
                ProcessValue(info, key, name);
            }
            return info;
        }

        private static void ProcessValue(FileAssiocationInfo info, RegistryKey key, string valueName) {
            string? value  = key.GetValue(valueName) as string;
            if (value == null) {
                return;
            }
            switch (valueName) {
                case "ApplicationName":
                    info.ApplicationName = value;
                    break;
                case "":
                case "ApplicationIcon":
                    string[] parts = value.Split(',').Select(x => x.Trim('"')).ToArray();
                    string path = parts[0];
                    if (File.Exists(parts[0])) {
                        info.ApplicationPath = parts[0];
                        if (parts.Length >= 2 && int.TryParse(parts[1], out int index)) {
                            info.IconIndex = Math.Max(0, index);
                        }
                        else {
                            info.IconIndex = 0;
                        }
                        (Icon[] iconsLarge, Icon[] iconsSmall)? icons = GetAssociatedIcons(info.ApplicationPath);
                        if (icons != null) {
                            info.IconsLarge = icons.Value.iconsLarge;
                            info.IconsSmall = icons.Value.iconsSmall;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Get associated icons from an executable file.
        /// </summary>
        /// <param name="executablePath">The full path of the executable file.</param>
        /// <returns>
        ///     A tuple containing 2 arrays, 1 for large icons and 1 for small icons.<br/>
        ///     <see langword="null"/> if no icons were found.
        /// </returns>
        private static (Icon[] iconsLarge, Icon[] iconsSmall)? GetAssociatedIcons(string executablePath) {
            uint iconsCount = ExtractIconEx(executablePath, -1, null, null, 1);
            if (iconsCount > 0) {
                IntPtr[] pIconsLarge = new IntPtr[iconsCount];
                IntPtr[] pIconsSmall = new IntPtr[iconsCount];
                ExtractIconEx(executablePath, 0, pIconsLarge, pIconsSmall, iconsCount);

                Icon[] iconsLarge = new Icon[iconsCount];
                Icon[] iconsSmall = new Icon[iconsCount];
                for (int x = 0; x < iconsCount; x++) {
                    if (pIconsLarge != null) {
                        iconsLarge[x] = (Icon)Icon.FromHandle(pIconsLarge[x]).Clone();
                        DestroyIcon(pIconsLarge[x]);
                    }
                    if (pIconsSmall != null) {
                        iconsSmall[x] = (Icon)Icon.FromHandle(pIconsSmall[x]).Clone();
                        DestroyIcon(pIconsSmall[x]);
                    }
                }
                return (iconsLarge, iconsSmall);
            }
            return null;
        }
    }
}
