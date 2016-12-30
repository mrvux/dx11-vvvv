using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using InputElement = SlimDX.Direct3D11.InputElement;

namespace VVVV.DX11.Nodes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Pos3Norm3VertexSDX
    {
        public Vector3 Position;
        public Vector3 Normals;

        private static InputElement[] layout;

        public static InputElement[] Layout
        {
            get
            {
                if (layout == null)
                {
                    layout = new InputElement[]
                    {
                        new InputElement("POSITION",0,SlimDX.DXGI.Format.R32G32B32_Float,0, 0),
                        new InputElement("NORMAL",0,SlimDX.DXGI.Format.R32G32B32_Float,12,0),
                    };
                }
                return layout;
            }
        }

        public static int VertexSize
        {
            get { return Marshal.SizeOf(typeof(Pos3Norm3VertexSDX)); }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Pos3Norm3Color4VertexSDX
    {
        public Vector3 Position;
        public Vector3 Normals;
        public Color4 Color;

        private static InputElement[] layout;

        public static InputElement[] Layout
        {
            get
            {
                if (layout == null)
                {
                    layout = new InputElement[]
                    {
                        new InputElement("POSITION",0,SlimDX.DXGI.Format.R32G32B32_Float,0, 0),
                        new InputElement("NORMAL",0,SlimDX.DXGI.Format.R32G32B32_Float,InputElement.AppendAligned,0),
                        new InputElement("COLOR",0,SlimDX.DXGI.Format.R32G32B32A32_Float,InputElement.AppendAligned,0)
                    };
                }
                return layout;
            }
        }

        public static int VertexSize
        {
            get { return Marshal.SizeOf(typeof(Pos3Norm3Color4VertexSDX)); }
        }
    }

}
