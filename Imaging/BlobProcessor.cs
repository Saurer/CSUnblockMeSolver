using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CSUnblockMeSolver.Imaging {
    class BlobProcessor {
        private IEnumerable<Color> _referenceColors;
        private Rectangle _searchBounds;
        private ImageProcessor _processor;
        private int? _tolerance;

        private BlobProcessor(ImageProcessor processor, Point point, int tolerance) {
            _processor = processor;
            _tolerance = tolerance;
            _referenceColors = new Color[] { processor.Image.GetPixel(point) };
            _searchBounds = new Rectangle(point.X, point.Y, 1, 1);
        }

        private BlobProcessor(ImageProcessor processor, IEnumerable<Color> referenceColors, Rectangle searchBounds, int tolerance) {
            _processor = processor;
            _referenceColors = referenceColors;
            _tolerance = tolerance;
            _searchBounds = searchBounds;
        }

        public static ImageBlob SelectBlob(ImageProcessor processor, int x, int y, int tolerance) {
            var blobProcessor = new BlobProcessor(processor, new Point(x, y), tolerance);
            return blobProcessor.Search().Single();
        }

        public static IEnumerable<ImageBlob> SelectBlobs(ImageProcessor processor, IEnumerable<Color> referenceColors, Rectangle searchBounds, int tolerance) {
            var blobProcessor = new BlobProcessor(processor, referenceColors, searchBounds, tolerance);
            return blobProcessor.Search();
        }

        public IEnumerable<ImageBlob> Search() {
            var history = new Dictionary<Point, bool>();
            var result = new List<ImageBlob>();

            for (int x = _searchBounds.X; x < _searchBounds.X + _searchBounds.Width; x++) {
                for (int y = _searchBounds.Y; y < _searchBounds.Y + _searchBounds.Height; y++) {
                    var points = new List<Point>();
                    var queue = new Queue<Point>();
                    queue.Enqueue(new Point(x, y));

                    while (queue.Count > 0) {
                        Point point = queue.Dequeue();

                        if (history.ContainsKey(point)) {
                            continue;
                        }

                        history.Add(point, true);

                        if (!_processor.ColorCloseTo(_processor.Image.GetPixel(point), _referenceColors, _tolerance.Value)) {
                            continue;
                        }

                        points.Add(point);

                        for (int xOffset = -1; xOffset <= 1; xOffset++) {
                            for (int yOffset = -1; yOffset <= 1; yOffset++) {
                                int searchX = point.X + xOffset;
                                int searchY = point.Y + yOffset;
                                var searchPoint = new Point(searchX, searchY);

                                if (_processor.IsWithinBounds(searchPoint) && !history.ContainsKey(searchPoint)) {
                                    Color searchColor = _processor.Image.GetPixel(searchPoint);
                                    if (_processor.ColorCloseTo(searchColor, _referenceColors, _tolerance.Value)) {
                                        points.Add(searchPoint);
                                        queue.Enqueue(searchPoint);
                                    }
                                }
                            }
                        }
                    }

                    if (points.Count > 0) {
                        result.Add(new ImageBlob(_referenceColors, points));
                    }
                }
            }

            return result;
        }
    }
}