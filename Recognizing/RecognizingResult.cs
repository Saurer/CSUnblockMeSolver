using System.Collections.Generic;
using System.Drawing;
using CSUnblockMeSolver.Board;

namespace CSUnblockMeSolver.Recognizing {
    struct RecognizingResult {
        public IEnumerable<Block> Blocks;
        public Rectangle Bounds;
    }
}