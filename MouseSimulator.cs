using System.Drawing;
using System.Runtime.InteropServices;

namespace CSUnblockMeSolver {
    static class MouseSimulator {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point) {
                return new Point(point.X, point.Y);
            }
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition() {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }

        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const uint MOUSEEVENTF_MOVE = 0x0001;

        public static void Drag(int startX, int startY, int endX, int endY) {
            SetCursorPos(startX, startY);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            if (startX < endX) {
                while (GetCursorPosition().X < endX) {
                    startX += 1;
                    mouse_event(MOUSEEVENTF_MOVE, 1, 0, 0, 0);
                    SetCursorPos(startX, startY);
                }
            }
            else {
                while (GetCursorPosition().X > endX) {
                    startX -= 1;
                    mouse_event(MOUSEEVENTF_MOVE, -1, 0, 0, 0);
                    SetCursorPos(startX, startY);
                }
            }

            if (startY < endY) {
                while (GetCursorPosition().Y < endY) {
                    startY += 1;
                    mouse_event(MOUSEEVENTF_MOVE, 0, 1, 0, 0);
                    SetCursorPos(startX, startY);
                }
            }
            else {
                while (GetCursorPosition().Y > endY) {
                    startY -= 1;
                    mouse_event(MOUSEEVENTF_MOVE, 0, -1, 0, 0);
                    SetCursorPos(startX, startY);
                }
            }
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
    }
}