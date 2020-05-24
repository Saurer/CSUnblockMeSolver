using System.Collections.Generic;
using System.Drawing;

namespace CSUnblockMeSolver.Recognizing {
    struct RecognizingPattern {
        public IEnumerable<Color> BackgroundColors;
        public IEnumerable<IEnumerable<Color>> MainColorSets;
        public int BackgroundTolerance;
        public int MainTolerance;
        public int OffsetRight;
        public int Dimensions;
    }
}