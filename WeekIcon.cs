#region Using statements

using System.Globalization;
using System.Runtime.Versioning;

#endregion Using statements

namespace WeekNumberLite2
{
    internal static class WeekIcon
    {
        #region Icon Size

        private const int _iconSize = (int)IconSize.Icon512;

        #endregion Icon Size

        #region Internal static functions

        [SupportedOSPlatform("windows")]
        internal static Icon GetIcon(int weekNumber)
        {
            Icon? icon = null;
            using (Bitmap bitmap = new(_iconSize, _iconSize))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.TextContrast = 1;
                DrawBackgroundOnGraphics(graphics, _iconSize);
                DrawWeekNumberLite2OnGraphics(weekNumber, graphics, _iconSize);
                IntPtr bHicon = bitmap.GetHicon();
                Icon newIcon = Icon.FromHandle(bHicon);
                icon = new Icon(newIcon, _iconSize, _iconSize);
                CleanupIcon(ref newIcon);
            }
            return icon;
        }

        [SupportedOSPlatform("windows")]
        internal static void CleanupIcon(ref Icon icon)
        {
            if (icon is null)
            {
                return;
            }

            _ = NativeMethods.DestroyIcon(icon.Handle);
            icon.Dispose();
        }

        #endregion Internal static functions

        #region Privare static helper methods

        [SupportedOSPlatform("windows")]
        private static void DrawBackgroundOnGraphics(Graphics graphics, int size = 0)
        {
            if (size == 0)
            {
                size = _iconSize;
            }

            Color backgroundColor = Color.Black;
            Color foregroundColor = Color.LightGray;
            using SolidBrush foregroundBrush = new(foregroundColor);
            using SolidBrush backgroundBrush = new(backgroundColor);
            float inset = (float)Math.Abs(size * .03125);
            graphics?.FillRectangle(backgroundBrush, inset, inset, size - inset, size - inset);
            using (Pen pen = new(foregroundColor, inset * 2))
            {
                graphics?.DrawRectangle(pen, inset, inset, size - (inset * 2), size - (inset * 2));
            }

            float leftInset = (float)Math.Abs(size * .15625);
            graphics?.FillRectangle(foregroundBrush, leftInset, inset / 2, inset * 3, inset * 5);
            float rightInset = (float)Math.Abs(size * .75);
            graphics?.FillRectangle(foregroundBrush, rightInset, inset / 2, inset * 3, inset * 5);
        }

        [SupportedOSPlatform("windows")]
        private static void DrawWeekNumberLite2OnGraphics(int WeekNumberLite2, Graphics graphics, int size = 0)
        {
            if (size == 0)
            {
                size = _iconSize;
            }

            float fontSize = (float)Math.Abs(size * .78125);
            float insetX = (float)-(size > (int)IconSize.Icon16 ? Math.Abs(fontSize * .12) : Math.Abs(fontSize * .07));
            float insetY = (float)(size > (int)IconSize.Icon16 ? Math.Abs(fontSize * .2) : Math.Abs(fontSize * .08));
            Color foregroundColor = Color.White;
            using Font font = new(FontFamily.GenericMonospace, fontSize, FontStyle.Bold, GraphicsUnit.Pixel, 0, false);
            using Brush brush = new SolidBrush(foregroundColor);
            graphics?.DrawString(WeekNumberLite2.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'), font, brush, insetX, insetY);
        }

        #endregion Private static helper methods
    }
}