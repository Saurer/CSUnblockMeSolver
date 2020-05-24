using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CSUnblockMeSolver.Imaging {
    class ImageProcessor : IDisposable {
        public RawImage Image { get; private set; }

        public ImageProcessor(Bitmap bitmap) {
            Image = new RawImage(new Bitmap(bitmap));
        }

        public void Fill(Color color, IEnumerable<Point> points) {
            foreach (var point in points) {
                Image.SetPixel(point.X, point.Y, color);
            }
        }

        public void Fill(Color color, Rectangle rectangle) {
            for (int x = rectangle.X; x < rectangle.X + rectangle.Width; x++) {
                for (int y = rectangle.Y; y < rectangle.Y + rectangle.Height; y++) {
                    Image.SetPixel(x, y, color);
                }
            }
        }

        public bool ColorCloseTo(Color a, Color b, int tolerance) {
            return Math.Abs((a.R + a.G + a.B) - (b.R + b.G + b.B)) < tolerance;
        }

        public bool ColorCloseTo(Color a, IEnumerable<Color> b, int tolerance) {
            return b.Any(l => ColorCloseTo(a, l, tolerance));
        }

        public bool IsWithinBounds(int x, int y) {
            return Image.Bounds.Contains(x, y);
        }

        public bool IsWithinBounds(Point point) {
            return Image.Bounds.Contains(point);
        }

        public void Dispose() {
            Image.Dispose();
        }
    }
}