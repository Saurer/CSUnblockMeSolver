using System;
using System.Collections.Generic;
using System.Linq;

namespace CSUnblockMeSolver.Board {
    struct Step {
        public Board Board;
        public object Parent;
        public Move Move;
    }

    struct Solution {
        public IEnumerable<Block> BasePattern;
        public IEnumerable<SolutionStep> Moves;
        public int TimeSeconds;
        public int Iterations;
    }

    struct SolutionStep {
        public IEnumerable<Block> BasePattern;
        public Move Move;
    }

    struct Progress {
        public bool Done;
        public float Value;
        public Solution? Solution;
    }

    static class Solver {
        public static IEnumerator<Progress> Solve(Block[] pattern, int size, int stepSize = 5000) {
            Block main = pattern.Where(b => b.Type == BlockType.Main).Single();

            var queue = new Queue<Step>();
            var discovered = new Dictionary<string, bool>();
            var timeStart = DateTime.Now;
            int iterations = 0;
            float percent = 0;

            queue.Enqueue(
                new Step {
                    Board = Board.FromPattern(pattern, size),
                }
            );

            yield return new Progress() {
                Done = false,
                Value = 0
            };

            while (queue.Count > 0) {
                Step value = queue.Dequeue();
                iterations++;

                if (iterations % stepSize == 0) {
                    if (percent < 50) {
                        percent += iterations / queue.Count;
                    }
                    else if (percent < 65) {
                        percent += 5;
                    }
                    else if (percent < 85) {
                        percent += 1;
                    }
                    else if (percent < 95) {
                        percent += 0.5f;
                    }
                }

                yield return new Progress {
                    Done = false,
                    Value = percent
                };

                if (discovered.ContainsKey(value.Board.Hash)) {
                    continue;
                }
                else {
                    discovered[value.Board.Hash] = true;
                }

                if (value.Board.IsSolved) {
                    var resultMoves = new List<SolutionStep>();
                    Step solvingMove = GenerateSolvingMove(value);

                    resultMoves.Add(new SolutionStep {
                        BasePattern = value.Board.Pattern,
                        Move = solvingMove.Move
                    });

                    Step step = value;
                    while (null != step.Parent) {
                        Step next = (Step)step.Parent;
                        resultMoves.Add(new SolutionStep {
                            BasePattern = next.Board.Pattern,
                            Move = step.Move
                        });
                        step = next;
                    }

                    resultMoves.Reverse();
                    yield return new Progress {
                        Done = true,
                        Value = 100,
                        Solution = new Solution {
                            BasePattern = pattern,
                            Moves = resultMoves,
                            TimeSeconds = Convert.ToInt32((DateTime.Now - timeStart).TotalSeconds),
                            Iterations = iterations
                        }
                    };
                    yield break;
                }

                IEnumerable<Move> moves = value.Board.EnumerateMoves();
                foreach (Move move in moves) {
                    Board board = value.Board.MoveBlock(move);

                    if (!discovered.ContainsKey(board.Hash)) {
                        queue.Enqueue(new Step {
                            Board = board,
                            Parent = value,
                            Move = move,
                        });
                    }
                }
            }
        }

        public static Step GenerateSolvingMove(Step step) {
            IEnumerable<Block> pattern = step.Board.Pattern;
            Block main = pattern.Where(b => b.Type == BlockType.Main).Single();
            int count = step.Board.Size - Board.GetBoundingBox(main).Right + 1;
            var move = new Move {
                ID = main.ID,
                Direction = Direction.East,
                Count = count
            };
            return new Step {
                Board = step.Board.MoveBlock(move),
                Parent = step,
                Move = move
            };
        }
    }
}