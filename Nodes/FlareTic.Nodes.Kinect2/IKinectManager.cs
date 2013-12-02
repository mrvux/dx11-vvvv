using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlareTic.Nodes.Kinect2
{
    public interface IKinectManager
    {
        IntPtr ColorFrame { get; }
        IntPtr DepthFrame { get; }
        IntPtr IRFrame { get; }
        int DepthSize { get; }
        event EventHandler NewColorFrame;
        event EventHandler NewDepthFrame;
        event EventHandler NewIRFrame;
    }
}
