using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CSUnblockMeSolver.Board;
using CSUnblockMeSolver.Imaging;
using CSUnblockMeSolver.Recognizing;

namespace CSUnblockMeSolver {
    public partial class MainForm : Form {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        struct SolutionEventArgs {
            public Solution? Solution;
            public Rectangle ScreenBounds;
        }

        private int _index = 0;
        private event EventHandler<SolutionEventArgs> OnSolution;

        private RecognizingPattern _basePattern = new RecognizingPattern {
            BackgroundColors = new Color[] { Color.FromArgb(19, 21, 28) },
            MainColorSets = new Color[][] {
                new Color[] {
                    Color.FromArgb(243, 175, 1),
                    Color.FromArgb(154, 68, 0),
                    Color.FromArgb(239, 211, 130),
                    Color.FromArgb(255, 245, 73),
                },
                new Color[] {
                    Color.FromArgb(255, 121, 236),
                    Color.FromArgb(238, 42, 203),
                    Color.FromArgb(204, 44, 170),
                    Color.FromArgb(181, 31, 178),
                }
            },
            BackgroundTolerance = 50,
            MainTolerance = 35,
            OffsetRight = 5,
            Dimensions = 6
        };

        public MainForm() {
            InitializeComponent();
            int uniqueHotkeyId = 1;
            int hotKeyCode = (int)Keys.NumPad0;
            Boolean registered = RegisterHotKey(
                this.Handle, uniqueHotkeyId, 0x0000, hotKeyCode
            );
            OnSolution += (sender, e) => {
                if (e.Solution.HasValue) {
                    SimulateSolution(e);
                }
            };
        }

        protected override void WndProc(ref Message m) {
            if (m.Msg == 0x0312) {
                int id = m.WParam.ToInt32();

                if (id == 1) {
                    DoSolve();
                }
            }

            base.WndProc(ref m);
        }

        private Solution? Solve(IEnumerable<Block> blocks) {
            var progress = Solver.Solve(blocks.ToArray(), _basePattern.Dimensions);

            while (progress.MoveNext()) {
                if (progress.Current.Done && progress.Current.Solution.HasValue) {
                    _index = 0;
                    var solution = progress.Current.Solution.Value;
                    return solution;
                }
            }

            return null;
        }

        private void SimulateSolution(SolutionEventArgs e) {
            while (_index < e.Solution.Value.Moves.Count()) {
                int offsetX = e.ScreenBounds.Width / 6;
                int offsetY = e.ScreenBounds.Height / 6;

                var move = e.Solution.Value.Moves.ElementAt(_index++);
                Block block = move.BasePattern.Where(l => l.ID == move.Move.ID).Single();
                int startX = e.ScreenBounds.X;
                int startY = e.ScreenBounds.Y;
                int endX = startX;
                int endY = startY;

                switch (move.Move.Direction) {
                    case Direction.East:
                        startX += block.X * offsetX - offsetX / 2;
                        startY += block.Y * offsetY - offsetY / 2;
                        endX = startX + (move.Move.Count * offsetX);
                        endY = startY;
                        break;

                    case Direction.West:
                        startX += block.X * offsetX - offsetX / 2;
                        startY += block.Y * offsetY - offsetY / 2;
                        endX = startX - (move.Move.Count * offsetX);
                        endY = startY;
                        break;

                    case Direction.North:
                        startX += block.X * offsetX - offsetX / 2;
                        startY += block.Y * offsetY - offsetY / 2;
                        endX = startX;
                        endY = startY - (move.Move.Count * offsetY);
                        break;

                    case Direction.South:
                        startX += block.X * offsetX - offsetX / 2;
                        startY += block.Y * offsetY - offsetY / 2;
                        endX = startX;
                        endY = startY + (move.Move.Count * offsetY);
                        break;
                }

                MouseSimulator.Drag(startX, startY, endX, endY);
                System.Threading.Thread.Sleep(250);
            }
        }

        private void DoSolve() {
            SolveButton.Enabled = false;
            MapBox.Image = null;
            var form = new CaptureForm();
            form.OnCapture += (sender, e) => {
                var bitmap = ScreenCapture.Capture(e);

                new Thread(() => {
                    using (var processor = new ImageProcessor(bitmap)) {
                        var recognizer = new RecognizingProcessor(_basePattern, processor);
                        if (recognizer.TryRecognize(bitmap, out RecognizingResult result)) {
                            MapBox.Invoke((MethodInvoker)delegate {
                                MapBox.Image = recognizer.DrawDiag(result.Blocks, 390);
                            });

                            Solution? solution = Solve(result.Blocks);
                            OnSolution.Invoke(this, new SolutionEventArgs {
                                Solution = solution,
                                ScreenBounds = new Rectangle(
                                    e.X + result.Bounds.X,
                                    e.Y + result.Bounds.Y,
                                    result.Bounds.Width,
                                    result.Bounds.Height
                                )
                            });
                        }
                    }

                    SolveButton.Invoke((MethodInvoker)delegate {
                        SolveButton.Enabled = true;
                    });
                }).Start();
            };
        }

        private void SolveButtonClick(object sender, EventArgs e) {
            DoSolve();
        }
    }
}
