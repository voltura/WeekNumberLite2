#region Using statements

using System.Globalization;
using System.Runtime.Versioning;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

#endregion Using statements

namespace WeekNumberLite2
{
    internal static class WeekIcon
    {
        #region Icon Sizes

        private static readonly int[] _iconSizes =
        {
            (int)IconSize.Icon16,
            (int)IconSize.Icon32,
            (int)IconSize.Icon48,
            (int)IconSize.Icon64,
            (int)IconSize.Icon128,
            (int)IconSize.Icon256,
            (int)IconSize.Icon512
        };

        private const int _iconSize = (int)IconSize.Icon512;

        #endregion Icon Sizes

        #region Internal static functions

        [SupportedOSPlatform("windows")]
        internal static Icon GetIcon(int weekNumber)
        {
            using MemoryStream iconStream = new();
            using BinaryWriter writer = new(iconStream);

            writer.Write((ushort)0); // reserved
            writer.Write((ushort)1); // icon type
            writer.Write((ushort)_iconSizes.Length);

            long imageOffset = 6 + (16 * _iconSizes.Length);
            List<byte[]> images = new();

            foreach (int size in _iconSizes)
            {
                using Bitmap bitmap = new(size, size);
                using Graphics graphics = Graphics.FromImage(bitmap);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.TextContrast = 1;
                DrawBackgroundOnGraphics(graphics, size);
                DrawWeekNumberLite2OnGraphics(weekNumber, graphics, size);
                using MemoryStream ms = new();
                bitmap.Save(ms, ImageFormat.Png);
                images.Add(ms.ToArray());
            }

            for (int i = 0; i < _iconSizes.Length; i++)
            {
                int size = _iconSizes[i];
                byte[] data = images[i];
                writer.Write((byte)(size >= 256 ? 0 : size));
                writer.Write((byte)(size >= 256 ? 0 : size));
                writer.Write((byte)0); // colors
                writer.Write((byte)0); // reserved
                writer.Write((ushort)1); // planes
                writer.Write((ushort)32); // bit count
                writer.Write(data.Length);
                writer.Write((uint)imageOffset);
                imageOffset += data.Length;
            }

            foreach (byte[] data in images)
            {
                writer.Write(data);
            }

            writer.Flush();
            iconStream.Position = 0;
            return new Icon(iconStream);
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