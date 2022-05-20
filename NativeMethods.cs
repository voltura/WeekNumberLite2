#region Using statements

using System.Runtime.InteropServices;

#endregion Using statements

namespace WeekNumberLite2
{
    internal static class NativeMethods
    {
        #region External user32.dll function to free GDI+ icon from memory

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyIcon(IntPtr handle);

        #endregion External user32.dll function to free GDI+ icon from memory
    }
}