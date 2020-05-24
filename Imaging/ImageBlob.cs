using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CSUnblockMeSolver.Imaging {
    class ImageBlob {
        private Rectangle? _bounds;

        public readonly IEnumerable<Color> ReferenceColors;
        public readonly IEnumerable<Point> Points;
        public int Size { get; private set; }
        public Rectangle Bounds {
            get {
                if (!_bounds.HasValue) {
                    var xAxis = Points.OrderBy(l => l.X);
                    var yAxis = Points.OrderBy(l => l.Y);

                    int left = xAxis.First().X;
                    int right = xAxis.Last().X;
                    int top = yAxis.First().Y;
                    int bottom = yAxis.Last().Y;
                    _bounds = new Rectangle(left, top, right - left, bottom - top);
                }

                return _bounds.Value;
            }
        }

        public ImageBlob(IEnumerable<Color> referenceColors, IEnumerable<Point> points) {
            ReferenceColors = referenceColors;
            Points = points;
            Size = points.Count();
        }
    }
}