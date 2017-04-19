using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;

using System.Text.RegularExpressions;

namespace Moral.Util
{
    public static class WindowsUtil
    {
        //
        // スタートアップにショートカットを登録する
        //
        public static void RegiserStartUp_CurrentUserRun(string title = null)
        {
            //var path = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            //var filename = Path.GetFileName(Application.ExecutablePath);
            //var app = System.Windows.Application.Current;
            var ExecutablePath = Environment.GetCommandLineArgs()[0];

            if (title == null) {
                title = Path.GetFileNameWithoutExtension(ExecutablePath);
            }

            var path = GetStartupShortcutPath(title);

            makeShortcut(ExecutablePath, path);
        }

        //
        // スタートアップからショートカットを削除する
        //
        public static void UnregiserStartUp_CurrentUserRun(string title = null)
        {
            if (title == null)
            {
                var ExecutablePath = Environment.GetCommandLineArgs()[0];
                title = Path.GetFileNameWithoutExtension(ExecutablePath);
            }

            var path = GetStartupShortcutPath(title);
            File.Delete(path);
        }

        public static bool ExistsStartUp_CurrentUserRun(string title = null)
        {
            if (title == null)
            {
                var ExecutablePath = Environment.GetCommandLineArgs()[0];
                title = Path.GetFileNameWithoutExtension(ExecutablePath);
            }

            var path = GetStartupShortcutPath(title);
            return File.Exists(path); 
        }

        static string GetStartupShortcutPath(string title)
        {
            return System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                @"" + title + ".lnk");
        }

        public static void makeShortcut(string path, string target){
            //作成するショートカットのパス
            //string shortcutPath = System.IO.Path.Combine(
            //    Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory),
            //    @"MyApp.lnk");
            //ショートカットのリンク先
            //string targetPath = Application.ExecutablePath;
            
            //WshShellを作成
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
            dynamic shell = Activator.CreateInstance(t);
            //WshShortcutを作成
            var shortcut = shell.CreateShortcut(target);

            //リンク先
            shortcut.TargetPath = path;
            //アイコンのパス
            shortcut.IconLocation = path + ",0";
            //その他のプロパティも同様に設定できるため、省略

            //ショートカットを作成
            shortcut.Save();

            //後始末
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
        }


        public static Bitmap getIconToBMP(uint type, string path)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr hImg; //the handle to the system image list

            try
            {
                //Use this to get the small Icon
                hImg = Win32.SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | type);
                IntPtr hicon = IntPtr.Zero;
                if (shinfo.hIcon == IntPtr.Zero)
                {
                    path = getEnvPath(path);
                    hImg = Win32.SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | type);
                    if (shinfo.hIcon == IntPtr.Zero)
                    {
                        return null;
                    }
                }
                //The icon is returned in the hIcon member of the shinfo struct
                return (System.Drawing.Icon.FromHandle(shinfo.hIcon)).ToBitmap();
            }
            finally
            {
                DestroyIcon(shinfo.hIcon);
            }
        }

        public static Icon GetIconFromEXEDLL2(string path, bool iconSize = true)
        {
            path = Regex.Replace(path, "%(windir|SYSTEMROOT)%",
                   System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows));

            var FileInfo = new System.IO.FileInfo(path);
            string name = FileInfo.Name;
            string ext = FileInfo.Extension;
            string newname = Regex.Replace(name, ext + "$", "", RegexOptions.RightToLeft);
            //string newname = name.Replace(ext, "");
            string newext = Regex.Replace(ext, @",.*$", "", RegexOptions.RightToLeft);
            int id = 0;
            path = Path.Combine(FileInfo.DirectoryName, newname + newext);
            if (!FileInfo.Exists)
            {
                //
                // ファイルが無い場合は、[インデックス指定が含まれている]と疑ってみる
                //
                CaptureCollection ccl = Regex.Match(ext, @"([-]?\d+)$").Captures;

                // 正の値:0からのインデックス
                // 負の値:対象ファイル内のアイコンID
                id = int.Parse(ccl[0].Value);
            }

            return GetIconFromEXEDLL(path, id, iconSize);
        }

        public static Bitmap getIconToBMP2(string path)
        {
            try
            {
                return GetIconFromEXEDLL2(path).ToBitmap();
            }
            catch (Exception)
            {
                return null;
            }
        }

        //「dll名」と「アイコン番号」からアイコンを取得する  
        // IconSize  
        // true ：大きいアイコン  
        // false：小さいアイコン  
        public static Icon GetIconFromEXEDLL(string path, int iconIndex, bool iconSize)
        {
            try
            {
                Icon[] icons = new Icon[2];
                IntPtr largeIconHandle = IntPtr.Zero;
                IntPtr smallIconHandle = IntPtr.Zero;
                Win32.ExtractIconEx(path, iconIndex, out largeIconHandle, out smallIconHandle, 1);
                icons[0] = (Icon)Icon.FromHandle(largeIconHandle).Clone();
                icons[1] = (Icon)Icon.FromHandle(smallIconHandle).Clone();
                DestroyIcon(largeIconHandle);
                DestroyIcon(smallIconHandle);

                if (iconSize)
                {
                    return icons[0];
                }
                else
                {
                    return icons[1];
                }
            }
            catch (Exception) {
                return null;
            }
        }

        private static string getEnvPath(string cmdpath)
        {
            // 環境変数の取得（PATH要素）
            string variable = System.Environment.GetEnvironmentVariable("Path", System.EnvironmentVariableTarget.Process);

            // パスの分割
            string[] list = variable.Split(';');

            foreach (var path in list)
            {
                // 実体の存在チェック
                if (System.IO.File.Exists(System.IO.Path.Combine(path, cmdpath)))
                {
                    return System.IO.Path.Combine(path, cmdpath);
                }
            }
            return "";
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DestroyIcon(IntPtr hIcon);

    }

    public class Win32
    {
        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1; // 'Small icon
        public const uint SHGFI_EXTRALARGE = 0x2; // '  icon
        public const uint SHGFI_JUMBO = 0x4; // ' icon
        public const uint SHGFI_SYSICONINDEX = 0x000004000;

        public const int ILD_TRANSPARENT = 0x1;

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGELISTDRAWPARAMS
        {
            public int cbSize;
            public IntPtr himl;
            public int i;
            public IntPtr hdcDst;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap;    // x offest from the upperleft of bitmap
            public int yBitmap;    // y offset from the upperleft of bitmap
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }



        //IMAGE LIST
        public static Guid IID_IImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("shell32.dll", EntryPoint = "#727")]
        public static extern int SHGetImageList(uint iImageList, ref Guid riid, out IImageList ppv);

        [ComImportAttribute()]
        [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IImageList
        {
            [PreserveSig]
            int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);

            [PreserveSig]
            int ReplaceIcon(int i, IntPtr hicon, ref int pi);

            [PreserveSig]
            int SetOverlayImage(int iImage, int iOverlay);

            [PreserveSig]
            int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

            [PreserveSig]
            int AddMasked(IntPtr hbmImage, int crMask, ref int pi);

            [PreserveSig]
            int Draw(ref IMAGELISTDRAWPARAMS pimldp);

            [PreserveSig]
            int Remove(int i);

            [PreserveSig]
            int GetIcon(int i, int flags, ref IntPtr picon);
        }


        // ExtractIconEx 複数の引数指定方法により、オーバーロード定義する。
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        public static extern uint ExtractIconEx(
            [MarshalAs(UnmanagedType.LPTStr)]string lpszFile, int nIconIndex,
            IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        public static extern uint ExtractIconEx(
            [MarshalAs(UnmanagedType.LPTStr)]string lpszFile, int nIconIndex,
            IntPtr[] phiconLarge, IntPtr phiconSmall, uint nIcons);
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        public static extern uint ExtractIconEx(
            [MarshalAs(UnmanagedType.LPTStr)]string lpszFile, int nIconIndex,
            IntPtr phiconLarge, IntPtr[] phiconSmall, uint nIcons);
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        public static extern uint ExtractIconEx(
            [MarshalAs(UnmanagedType.LPTStr)]string lpszFile, int nIconIndex,
            out IntPtr phiconLarge, out IntPtr phiconSmall, uint nIcons);
        // ExtractIconEx関数  
        //[DllImport("shell32.dll", EntryPoint = "ExtractIconEx", CharSet = CharSet.Auto)]
        //public static extern int ExtractIconEx(
        //    [MarshalAs(UnmanagedType.LPTStr)] string file,
        //    int index,
        //    out IntPtr largeIconHandle,
        //    out IntPtr smallIconHandle,
        //    int icons
        //);

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }

    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };
}
