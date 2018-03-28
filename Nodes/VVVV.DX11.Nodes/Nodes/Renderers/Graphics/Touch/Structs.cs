using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.Utils.VMath;

namespace VVVV.DX11.Nodes.Renderers.Graphics.Touch
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TOUCHINPUT
    {
        public int x;
        public int y;
        public System.IntPtr hSource;
        public int dwID;
        public int dwFlags;
        public int dwMask;
        public int dwTime;
        public System.IntPtr dwExtraInfo;
        public int cxContact;
        public int cyContact;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTS
    {
        public short x;
        public short y;
    }

    public class TouchData
    {
        public int Id;
        public Vector2 Pos;
        public bool IsNew;

        public TouchData Clone()
        {
            TouchData t = new TouchData();
            t.Id = this.Id;
            t.Pos = this.Pos;
            t.IsNew = this.IsNew;

            return t;
        }

        public TouchData Clone(float sizex,float sizey)
        {
            TouchData t = new TouchData();

            t.Id = this.Id;

            float x = (float)VMath.Map(this.Pos.X, 0, sizex, -1.0f, 1.0f, TMapMode.Float);
            float y = (float)VMath.Map(this.Pos.Y, 0, sizey, 1.0f, -1.0f, TMapMode.Float);

            t.Pos = new Vector2(x, y);
            t.IsNew = this.IsNew;

            return t;
        }
    }

    public class MultiTouchState
    {
        private Dictionary<int, TouchData> touchdata = new Dictionary<int, TouchData>();
    }



    public class WMTouchEventArgs : System.EventArgs
    {
        // Private data members
        private int x;                  // touch x client coordinate in pixels
        private int y;                  // touch y client coordinate in pixels
        private int id;                 // contact ID
        private int mask;               // mask which fields in the structure are valid
        private int flags;              // flags
        private int time;               // touch event time
        private int contactX;           // x size of the contact area in pixels
        private int contactY;           // y size of the contact area in pixels
        private long touchDeviceId;

        // Access to data members
        public int LocationX
        {
            get { return x; }
            set { x = value; }
        }
        public int LocationY
        {
            get { return y; }
            set { y = value; }
        }
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public int Flags
        {
            get { return flags; }
            set { flags = value; }
        }
        public int Mask
        {
            get { return mask; }
            set { mask = value; }
        }
        public int Time
        {
            get { return time; }
            set { time = value; }
        }
        public int ContactX
        {
            get { return contactX; }
            set { contactX = value; }
        }
        public int ContactY
        {
            get { return contactY; }
            set { contactY = value; }
        }
        public long TouchDeviceID
        {
            get { return touchDeviceId; }
            set { touchDeviceId = value; }
        }

        public bool IsPrimaryContact
        {
            get { return (flags & TouchConstants.TOUCHEVENTF_PRIMARY) != 0; }
        }

        // Constructor
        public WMTouchEventArgs()
        {
        }
    }
}
