using System;
using System.Drawing;
using System.Windows.Forms;

namespace CSUnblockMeSolver {
    class CaptureForm : Form {
        private Point? _drawStartXY = null;
        private Point? _drawEndXY = null;
        private Rectangle? _rect = null;

        public CaptureForm() {
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.TopMost = true;
            this.Visible = true;
            this.Opacity = 0.5;
            this.DoubleBuffered = true;
            this.Paint += this.OnPaint;
            this.MouseDown += this.OnMouseDown;
            this.MouseMove += this.OnMouseMove;
            this.MouseUp += this.OnMouseUp;
        }

        public void CaptureRectangle() {
            _drawStartXY = null;
            _drawEndXY = null;
            this.Visible = true;
        }

        public event EventHandler<Rectangle> OnCapture;

        private void OnPaint(object sender, PaintEventArgs e) {
            if (_drawStartXY.HasValue && _drawEndXY.HasValue) {
                int x, y, width, height;

                if (_drawStartXY.Value.X < _drawEndXY.Value.X) {
                    x = _drawStartXY.Value.X;
                    width = _drawEndXY.Value.X - _drawStartXY.Value.X;
                }
                else {
                    x = _drawEndXY.Value.X;
                    width = _drawStartXY.Value.X - _drawEndXY.Value.X;
                }

                if (_drawStartXY.Value.Y < _drawEndXY.Value.Y) {
                    y = _drawStartXY.Value.Y;
                    height = _drawEndXY.Value.Y - _drawStartXY.Value.Y;
                }
                else {
                    y = _drawEndXY.Value.Y;
                    height = _drawStartXY.Value.Y - _drawEndXY.Value.Y;
                }

                this._rect = new Rectangle(x, y, width, height);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(50, Color.Red)), this._rect.Value);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            if (!_drawStartXY.HasValue) {
                return;
            }

            _drawEndXY = new Point(e.X, e.Y);
            this.Refresh();
        }

        private void OnMouseDown(object sender, MouseEventArgs e) {
            _drawStartXY = new Point(e.X, e.Y);
            this.Refresh();
        }

        private void OnMouseUp(object sender, MouseEventArgs e) {
            this.Refresh();
            this.Visible = false;

            if (_rect.HasValue) {
                OnCapture.Invoke(this, _rect.Value);
            }
        }
    }
}