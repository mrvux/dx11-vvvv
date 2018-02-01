using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11.Nodes.TexProc
{
    public static class Consts
    {
        public const string EffectPath = "VVVV.DX11.Nodes.TexProc.effects";

        public static SharpDX.DXGI.Format ToSharpDX(this SlimDX.DXGI.Format slimfmt)
        {
            return (SharpDX.DXGI.Format)slimfmt;
        }


        public static bool IsSignedInt(this SlimDX.DXGI.Format fmt)
        {
            string s = fmt.ToString();
            return s.EndsWith("SInt");
        }

        public static bool IsUnsignedInt(this SlimDX.DXGI.Format fmt)
        {
            string s = fmt.ToString();
            return s.EndsWith("UInt");
        }

        public static SlimDX.DXGI.Format DefaultOutputForCompressed(this SlimDX.DXGI.Format fmt)
        {
            string s = fmt.ToString();
            if (s.StartsWith("BC"))
            {
                return SlimDX.DXGI.Format.R8G8B8A8_UNorm;
            }
            return fmt;
        }

        public static SlimDX.DXGI.Format GetSingleChannelEquivalent(this SlimDX.DXGI.Format fmt)
        {
            string s = fmt.ToString();

            //Default a few compressed
            if (fmt == SlimDX.DXGI.Format.BC1_UNorm || fmt == SlimDX.DXGI.Format.BC3_UNorm || fmt == SlimDX.DXGI.Format.BC7_UNorm)
            {
                return SlimDX.DXGI.Format.R8_UNorm;
            }

            if (s.StartsWith("R32"))
            {
                if (fmt.IsSignedInt())
                {
                    return SlimDX.DXGI.Format.R32_SInt;
                }
                if (fmt.IsUnsignedInt())
                {
                    return SlimDX.DXGI.Format.R32_UInt;
                }

                return SlimDX.DXGI.Format.R32_Float;

            }
            else if (s.StartsWith("R16"))
            {
                if (fmt.IsSignedInt())
                {
                    return SlimDX.DXGI.Format.R16_SInt;
                }
                if (fmt.IsUnsignedInt())
                {
                    return SlimDX.DXGI.Format.R16_UInt;
                }

                return SlimDX.DXGI.Format.R16_Float;

            }
            else if (s.StartsWith("R8"))
            {
                if (fmt.IsSignedInt())
                {
                    return SlimDX.DXGI.Format.R8_SInt;
                }
                if (fmt.IsUnsignedInt())
                {
                    return SlimDX.DXGI.Format.R8_UInt;
                }

                return SlimDX.DXGI.Format.R8_UNorm;

            }

            return SlimDX.DXGI.Format.Unknown;
        }
    }
}
