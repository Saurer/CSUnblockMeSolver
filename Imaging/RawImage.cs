using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CSUnblockMeSolver.Imaging {
    class RawImage : IDisposable {
        public Bitmap Bitmap { get; private set; }
        public BitmapData Data { get; private set; }
        public Rectangle Bounds { get; private set; }
        public byte[] RGB { get; private set; }

        public RawImage(Bitmap bitmap) {
            Bitmap = bitmap;
            Bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            Data = bitmap.LockBits(Bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
            int count = Math.Abs(Data.Stride) * Bounds.Height;
            RGB = new byte[count];
            Marshal.Copy(Data.Scan0, RGB, 0, count);
        }

        public Color GetPixel(int x, int y) {
            int index = GetPixelIndex(x, y);
            return Color.FromArgb(RGB[index + 2], RGB[index + 1], RGB[index]);
        }

        public Color GetPixel(Point point) {
            return GetPixel(point.X, point.Y);
        }

        public void SetPixel(int x, int y, Color color) {
            int index = GetPixelIndex(x, y);
            RGB[index + 3] = 255;
            RGB[index + 2] = color.R;
            RGB[index + 1] = color.G;
            RGB[index] = color.B;
        }

        public void SetPixel(Point point, Color color) {
            SetPixel(point.X, point.Y, color);
        }

        private int GetPixelIndex(int x, int y) {
            return (x + (y * Bounds.Width)) * 4;
        }

        public void Dispose() {
            Marshal.Copy(RGB, 0, Data.Scan0, RGB.Length);
            Bitmap.UnlockBits(Data);
        }
    }
}