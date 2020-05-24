using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSUnblockMeSolver.Board;
using CSUnblockMeSolver.Imaging;

namespace CSUnblockMeSolver.Recognizing {
    enum Side {
        Left,
        Right,
        Top,
        Bottom
    }

    class RecognizingProcessor {
        private RecognizingPattern _pattern;
        private ImageProcessor _processor;

        private readonly Color COLOR_BACKGROUND = Color.Red;
        private readonly Color COLOR_FOREGROUND = Color.Blue;

        public RecognizingProcessor(RecognizingPattern pattern, ImageProcessor processor) {
            _pattern = pattern;
            _processor = processor;
        }

        public bool TryRecognize(Bitmap bitmap, out RecognizingResult result) {
            try {
                using (var processor = new ImageProcessor(bitmap)) {
                    var recognizer = new RecognizingProcessor(_pattern, processor);
                    var resultBlocks = new List<Block>();
                    Rectangle mapBounds = recognizer.FindBounds();
                    Rectangle mainBlockBounds = recognizer.FindMain(mapBounds);
                    IEnumerable<Point> background = recognizer.FindBackground(mapBounds);
                    Block mainBlock = recognizer.MainRectangleToBlock(mapBounds, mainBlockBounds);

                    processor.Fill(Color.Blue, mapBounds);
                    processor.Fill(Color.Red, background);
                    processor.Fill(Color.Red, mainBlockBounds);

                    IEnumerable<Point> blockPoints = recognizer.FindBlockPoints(mapBounds);
                    IEnumerable<Block> blocks = recognizer.FindBlocks(blockPoints, mapBounds);
                    int cellWidth = mapBounds.Width / _pattern.Dimensions;
                    int cellHeight = mapBounds.Height / _pattern.Dimensions;

                    mainBlock.ID = 0;
                    resultBlocks.Add(mainBlock);
                    int blockID = 1;
                    foreach (var block in blocks) {
                        var newBlock = block;
                        newBlock.ID = blockID++;
                        resultBlocks.Add(newBlock);
                    }

                    result = new RecognizingResult {
                        Blocks = resultBlocks,
                        Bounds = mapBounds
                    };
                    return true;
                }
            }
            catch {
                result = new RecognizingResult();
                return false;
            }
        }

        public Rectangle FindBounds() {
            IEnumerable<ImageBlob> blobs = BlobProcessor.SelectBlobs(_processor, _pattern.BackgroundColors, _processor.Image.Bounds, _pattern.BackgroundTolerance);
            ImageBlob biggestBlob = blobs.OrderByDescending(l => l.Size).First();
            return new Rectangle(biggestBlob.Bounds.X, biggestBlob.Bounds.Y, biggestBlob.Bounds.Width - _pattern.OffsetRight, biggestBlob.Bounds.Height);
        }

        public Rectangle FindMain(Rectangle mapBounds) {
            int cellWidth = mapBounds.Width / _pattern.Dimensions;
            int cellHeight = mapBounds.Height / _pattern.Dimensions;
            int tolerance = cellHeight / 4;
            int yMin = mapBounds.Y + cellHeight * 2 - tolerance;
            int yMax = mapBounds.Y + cellHeight * 2 + tolerance;
            int hMin = cellHeight - tolerance;
            int hMax = cellHeight + tolerance;
            int wMin = cellWidth * 2 - tolerance;
            int wMax = cellWidth * 2 + tolerance;

            foreach (var colorBatch in _pattern.MainColorSets) {
                IEnumerable<ImageBlob> stack = BlobProcessor.SelectBlobs(_processor, colorBatch, mapBounds, _pattern.MainTolerance)
                    .OrderByDescending(l => l.Size)
                    .Where(blob =>
                        // Is found on 3rd row from top
                        (blob.Bounds.Y > yMin && blob.Bounds.Y < yMax) &&

                        // Is roughly 2 columns width
                        (blob.Bounds.Width > wMin && blob.Bounds.Width < wMax) &&

                        // Is roughly 1 row height
                        (blob.Bounds.Height > hMin && blob.Bounds.Height < hMax)
                    );

                switch (stack.Count()) {
                    case 1:
                        return stack.Single().Bounds;
                    default: continue;
                }
            }

            throw new Exception("Not found");
        }

        public Block MainRectangleToBlock(Rectangle mapBounds, Rectangle bounds) {
            int cellWidth = mapBounds.Width / _pattern.Dimensions;
            return new Block {
                X = (int)Math.Floor((bounds.X - (double)mapBounds.X) / cellWidth) + 1,
                Y = 3,
                Orientation = Orientation.Horizontal,
                Size = 2,
                Type = BlockType.Main
            };
        }

        public IEnumerable<Point> FindBackground(Rectangle mapBounds) {
            return BlobProcessor.SelectBlobs(_processor, _pattern.BackgroundColors, mapBounds, _pattern.BackgroundTolerance)
                .OrderByDescending(l => l.Size)
                .First()
                .Points;
        }

        public IEnumerable<Point> FindBlockPoints(Rectangle mapBounds) {
            var result = new List<Point>();
            int cellWidth = mapBounds.Width / _pattern.Dimensions;
            int cellHeight = mapBounds.Height / _pattern.Dimensions;
            int offsetX = cellWidth / 4;
            int offsetY = cellHeight / 4;

            for (int x = 1; x <= _pattern.Dimensions; x++) {
                for (int y = 1; y <= _pattern.Dimensions; y++) {
                    int centerX = mapBounds.X + (cellWidth * x) - cellWidth / 2;
                    int centerY = mapBounds.Y + (cellHeight * y) - cellHeight / 2;

                    var colors = new Color[]{
                        _processor.Image.GetPixel(centerX - offsetX, centerY - offsetY),
                        _processor.Image.GetPixel(centerX + offsetX, centerY - offsetY),

                        _processor.Image.GetPixel(centerX, centerY),

                        _processor.Image.GetPixel(centerX - offsetX, centerY + offsetY),
                        _processor.Image.GetPixel(centerX + offsetX, centerY + offsetY),
                    };

                    if (colors.Count(l => COLOR_FOREGROUND.R == l.R && COLOR_FOREGROUND.G == l.G && COLOR_FOREGROUND.B == l.B) > colors.Length / 2) {
                        result.Add(new Point(x, y));
                    }
                }
            }

            return result;
        }

        public IEnumerable<Block> FindBlocks(IEnumerable<Point> blockPoints, Rectangle mapBounds) {
            var history = new Dictionary<Point, bool>();
            var blocks = new List<Block>();

            foreach (Point blockPoint in blockPoints) {
                var queue = new Queue<Point>();
                queue.Enqueue(blockPoint);
                int left = blockPoint.X;
                int top = blockPoint.Y;
                int right = left;
                int bottom = top;

                if (history.ContainsKey(blockPoint)) {
                    continue;
                }

                while (queue.Count > 0) {
                    Point point = queue.Dequeue();

                    if (history.ContainsKey(point)) {
                        continue;
                    }

                    history.Add(point, true);

                    if (point.X > 1 && FindIntersection(mapBounds, point, Side.Left)) {
                        var leftPoint = new Point(point.X - 1, point.Y);
                        if (!history.ContainsKey(leftPoint)) {
                            queue.Enqueue(leftPoint);
                            left = leftPoint.X;
                        }
                    }
                    if (point.X < _pattern.Dimensions && FindIntersection(mapBounds, point, Side.Right)) {
                        var rightPoint = new Point(point.X + 1, point.Y);
                        if (!history.ContainsKey(rightPoint)) {
                            queue.Enqueue(rightPoint);
                            right = rightPoint.X;
                        }
                    }
                    if (point.Y > 1 && FindIntersection(mapBounds, point, Side.Top)) {
                        var topPoint = new Point(point.X, point.Y - 1);
                        if (!history.ContainsKey(topPoint)) {
                            queue.Enqueue(topPoint);
                            top = topPoint.Y;
                        }
                    }
                    if (point.Y < _pattern.Dimensions && FindIntersection(mapBounds, point, Side.Bottom)) {
                        var bottomPoint = new Point(point.X, point.Y + 1);
                        if (!history.ContainsKey(bottomPoint)) {
                            queue.Enqueue(bottomPoint);
                            bottom = bottomPoint.Y;
                        }
                    }
                }

                if (left == right) {
                    blocks.Add(new Block {
                        X = left,
                        Y = top,
                        Orientation = Orientation.Vertical,
                        Size = bottom - top + 1,
                        Type = BlockType.Block
                    });
                }
                else if (top == bottom) {
                    blocks.Add(new Block {
                        X = left,
                        Y = top,
                        Orientation = Orientation.Horizontal,
                        Size = right - left + 1,
                        Type = BlockType.Block
                    });
                }
                // else throw?
            }

            return blocks;
        }

        public bool FindIntersection(Rectangle mapBounds, Point blockPoint, Side side) {
            int cellWidth = mapBounds.Width / _pattern.Dimensions;
            int cellHeight = mapBounds.Height / _pattern.Dimensions;
            int checkWidth = cellWidth / 2;
            int checkHeight = cellHeight / 2;
            int checks = 4;

            int mainStart = 0;
            int mainEnd = 0;
            int crossStart = 0;
            int crossOffset = 0;
            int found = 0;

            switch (side) {
                case Side.Left:
                    mainStart = mapBounds.X + (blockPoint.X - 1) * cellWidth - checkWidth / 2;
                    mainEnd = mapBounds.X + (blockPoint.X - 1) * cellWidth + checkWidth / 2;
                    crossStart = mapBounds.Y + (blockPoint.Y - 1) * cellHeight;
                    crossOffset = cellHeight / (checks + 1);
                    break;

                case Side.Right:
                    mainStart = mapBounds.X + blockPoint.X * cellWidth - checkWidth / 2;
                    mainEnd = mapBounds.X + blockPoint.X * cellWidth + checkWidth / 2;
                    crossStart = mapBounds.Y + (blockPoint.Y - 1) * cellHeight;
                    crossOffset = cellHeight / (checks + 1);
                    break;

                case Side.Top:
                    mainStart = mapBounds.Y + (blockPoint.Y - 1) * cellHeight - checkHeight / 2;
                    mainEnd = mapBounds.Y + (blockPoint.Y - 1) * cellHeight + checkHeight / 2;
                    crossStart = mapBounds.X + (blockPoint.X - 1) * cellWidth;
                    crossOffset = cellWidth / (checks + 1);
                    break;

                case Side.Bottom:
                    mainStart = mapBounds.Y + blockPoint.Y * cellHeight - checkHeight / 2;
                    mainEnd = mapBounds.Y + blockPoint.Y * cellHeight + checkHeight / 2;
                    crossStart = mapBounds.X + (blockPoint.X - 1) * cellWidth;
                    crossOffset = cellWidth / (checks + 1);
                    break;
            }

            for (int offset = 1; offset <= checks; offset++) {
                var colors = new List<Color>();
                int cross = crossStart + crossOffset * offset;

                for (int main = mainStart; main < mainEnd; main++) {
                    Color color = side switch
                    {
                        Side.Left => _processor.Image.GetPixel(main, cross),
                        Side.Right => _processor.Image.GetPixel(main, cross),
                        Side.Top => _processor.Image.GetPixel(cross, main),
                        Side.Bottom => _processor.Image.GetPixel(cross, main),
                        _ => COLOR_BACKGROUND
                    };
                    colors.Add(color);
                }

                if (!colors.Any(l => COLOR_BACKGROUND.R == l.R && COLOR_BACKGROUND.G == l.G && COLOR_BACKGROUND.B == l.B)) {
                    found++;
                }
            }

            return found >= checks / 2;
        }

        public Bitmap DrawDiag(IEnumerable<Block> blocks, int size) {
            var bitmap = new Bitmap(size, size);
            int cellWidth = size / _pattern.Dimensions;
            int cellHeight = size / _pattern.Dimensions;

            using (var image = new RawImage(bitmap)) {
                for (int x = 0; x < size; x++) {
                    for (int y = 0; y < size; y++) {
                        image.SetPixel(x, y, Color.Black);
                    }
                }

                foreach (Block block in blocks) {
                    var blockRect = new Rectangle(
                        cellWidth * (block.X - 1) + 5,
                        cellHeight * (block.Y - 1) + 5,
                        (cellWidth * (block.Orientation == Orientation.Horizontal ? block.Size : 1)) - 10,
                        (cellHeight * (block.Orientation == Orientation.Vertical ? block.Size : 1)) - 10
                    );

                    Color color = block switch
                    {
                        Block b when b.Type == BlockType.Main => Color.Yellow,
                        Block b when b.Size == 1 => Color.Gray,
                        Block b when b.Orientation == Orientation.Horizontal => Color.Green,
                        Block b when b.Orientation == Orientation.Vertical => Color.Blue,
                        _ => Color.Red
                    };

                    for (int x = blockRect.X; x < blockRect.X + blockRect.Width; x++) {
                        for (int y = blockRect.Y; y < blockRect.Y + blockRect.Height; y++) {
                            image.SetPixel(x, y, color);
                        }
                    }
                }
            }

            return bitmap;
        }
    }
}