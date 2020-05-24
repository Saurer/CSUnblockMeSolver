using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSUnblockMeSolver.Board {
    enum Orientation {
        Horizontal,
        Vertical
    }

    enum BlockType {
        Main,
        Block
    }

    enum Direction {
        North,
        East,
        South,
        West
    }

    struct Block {
        public int ID;
        public int X;
        public int Y;
        public Orientation Orientation;
        public int Size;
        public BlockType Type;
    }

    struct Move {
        public Direction Direction;
        public int ID;
        public int Count;
    }

    class Board {
        private Dictionary<string, bool> _map = new Dictionary<string, bool>();
        private Dictionary<int, int> _blocks = new Dictionary<int, int>();
        private int _mainIndex = -1;
        private bool? _isSolved = null;


        public int Size { get; private set; }
        public IEnumerable<Block> Pattern { get; protected set; }
        public string Hash { get; protected set; }

        public static Board FromPattern(Block[] pattern, int size) {
            var map = new Dictionary<string, bool>();
            var blocks = new Dictionary<int, int>();
            int main = -1;

            for (int i = 0; i < pattern.Length; i++) {
                Block block = pattern[i];
                Rectangle bbox = Board.GetBoundingBox(block);
                blocks[block.ID] = i;

                if (BlockType.Main == block.Type) {
                    main = i;
                }

                for (int x = bbox.Left; x < bbox.Right; x++) {
                    for (int y = bbox.Top; y < bbox.Bottom; y++) {
                        map[Board.GetCoord(x, y)] = true;
                    }
                }
            }

            return new Board(pattern, size, map, blocks, main);
        }

        private Board(
            IEnumerable<Block> pattern,
            int size,
            Dictionary<string, bool> map,
            Dictionary<int, int> blocks,
            int mainIndex
        ) {
            Pattern = pattern;
            Size = size;
            _map = map;
            _blocks = blocks;
            _mainIndex = mainIndex;
            Hash = GetHash();
        }

        public IEnumerable<Move> EnumerateMoves() {
            foreach (var block in Pattern) {
                foreach (var move in EnumerateBlockMoves(block.ID)) {
                    yield return move;
                }
            }
        }

        public IEnumerable<Move> EnumerateBlockMoves(int id) {
            int blockIndex = _blocks[id];
            Block block = Pattern.ElementAt(blockIndex);

            if (block.Size == 1) {
                yield break;
            }

            Rectangle bbox = GetBoundingBox(block);

            if (block.Orientation == Orientation.Vertical) {
                for (int y = bbox.Top - 1; y >= 1 && !_map.ContainsKey(Board.GetCoord(bbox.Left, y)); y--) {
                    yield return new Move {
                        ID = id,
                        Direction = Direction.North,
                        Count = bbox.Top - y
                    };
                }

                for (int y = bbox.Bottom; y <= Size && !_map.ContainsKey(Board.GetCoord(bbox.Left, y)); y++) {
                    yield return new Move {
                        ID = id,
                        Direction = Direction.South,
                        Count = y - bbox.Bottom + 1
                    };
                }
            }

            if (block.Orientation == Orientation.Horizontal) {
                for (int x = bbox.Left - 1; x >= 1 && !_map.ContainsKey(Board.GetCoord(x, bbox.Top)); x--) {
                    yield return new Move {
                        ID = id,
                        Direction = Direction.West,
                        Count = bbox.Left - x
                    };
                }

                for (int x = bbox.Right; x <= Size && !_map.ContainsKey(Board.GetCoord(x, bbox.Top)); x++) {
                    yield return new Move {
                        ID = id,
                        Direction = Direction.East,
                        Count = x - bbox.Right + 1
                    };
                }
            }
        }

        public Board MoveBlock(int id, Direction direction, int count) {
            int blockIndex = _blocks[id];
            Block block = Pattern.ElementAt(blockIndex);
            Block newBlock = block;
            var newPattern = new Block[this.Pattern.Count()];
            Array.Copy(this.Pattern.ToArray(), newPattern, newPattern.Length);
            var newMap = new Dictionary<string, Boolean>(_map);

            switch (direction) {
                case Direction.North:
                    newBlock.Y -= count;
                    break;

                case Direction.East:
                    newBlock.X += count;
                    break;

                case Direction.South:
                    newBlock.Y += count;
                    break;

                case Direction.West:
                    newBlock.X -= count;
                    break;
            }

            newPattern[blockIndex] = newBlock;

            Rectangle startBox = Board.GetBoundingBox(block);
            for (int x = startBox.Left; x < startBox.Right; x++) {
                for (int y = startBox.Top; y < startBox.Bottom; y++) {
                    newMap.Remove(Board.GetCoord(x, y));
                }
            }

            Rectangle endBox = Board.GetBoundingBox(newBlock);
            for (int x = endBox.Left; x < endBox.Right; x++) {
                for (int y = endBox.Top; y < endBox.Bottom; y++) {
                    newMap[Board.GetCoord(x, y)] = true;
                }
            }

            return new Board(newPattern, Size, newMap, _blocks, _mainIndex);
        }

        public Board MoveBlock(Move move) {
            return MoveBlock(move.ID, move.Direction, move.Count);
        }

        public bool IsSolved {
            get {
                if (!_isSolved.HasValue) {
                    Block main = Pattern.ElementAt(_mainIndex);
                    for (int x = main.X + main.Size; x <= Size; x++) {
                        if (_map.ContainsKey(Board.GetCoord(x, main.Y)) && _map[Board.GetCoord(x, main.Y)]) {
                            _isSolved = false;
                            return false;
                        }
                    }
                    _isSolved = true;
                }

                return this._isSolved.Value;
            }
        }

        private string GetHash() {
            var sb = new StringBuilder();
            foreach (Block block in Pattern) {
                sb.Append(block.X);
                sb.Append(":");
                sb.Append(block.Y);
                sb.Append(",");
            }

            return sb.ToString();
        }

        private static string GetCoord(int x, int y) {
            return x + ":" + y;
        }

        public static Rectangle GetBoundingBox(Block block) {
            int startX = block.X;
            int startY = block.Y;
            int endX = block.X;
            int endY = block.Y;

            if (block.Orientation == Orientation.Horizontal) {
                endX += block.Size;
                endY += 1;
            }
            if (block.Orientation == Orientation.Vertical) {
                endX += 1;
                endY += block.Size;
            }

            return new Rectangle(startX, startY, endX - startX, endY - startY);
        }
    }
}