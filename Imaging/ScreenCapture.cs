using System.Drawing;
using System.Drawing.Imaging;

namespace CSUnblockMeSolver.Imaging {
    static class ScreenCapture {
        public static Bitmap Capture(Rectangle bounds) {
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bitmap)) {
                g.CopyFromScreen(new Point(bounds.X, bounds.Y), Point.Empty, bounds.Size);
                return bitmap;
            }
        }
    }
}