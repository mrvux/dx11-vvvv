using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VVVV.DX11.Windows
{
    public class WindowDisplayCursor : IDisposable
    {
        private bool hideCursor;
        private Control control;

        private bool isCursorVisible = true;

        public bool HideCursor
        {
            get { return this.hideCursor; }
            set
            {
                this.hideCursor = value;
                if (!this.hideCursor && !this.isCursorVisible)
                {
                    this.isCursorVisible = true;
                    Cursor.Show();
                }
            }
        }

        public WindowDisplayCursor(Control control)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            this.control = control;
            this.control.MouseLeave += Control_MouseLeave;
            this.control.MouseEnter += Control_MouseEnter;
        }

        private void Control_MouseEnter(object sender, EventArgs e)
        {
            if (this.hideCursor && this.isCursorVisible)
            {
                Cursor.Hide();
                this.isCursorVisible = false;

            }
        }

        private void Control_MouseLeave(object sender, EventArgs e)
        {
            if (!this.isCursorVisible)
            {
                this.isCursorVisible = true;
                Cursor.Show();
            }
        }

        public void Dispose()
        {
            this.control.MouseLeave -= Control_MouseLeave;
            this.control.MouseEnter -= Control_MouseEnter;
        }
    }
}
