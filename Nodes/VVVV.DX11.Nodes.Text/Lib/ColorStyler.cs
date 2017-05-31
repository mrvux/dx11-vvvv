using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.DirectWrite;
using VVVV.Core.DirectWrite;
using System.Runtime.InteropServices;

namespace VVVV.DX11.Nodes.Text
{
    public unsafe class FWColorStyler : ITextStyler, IDisposable
    {
        private SharpFontWrapper.ColorRGBA colorStyle;

        public IntPtr pointer;
        public bool Enabled;
        public SlimDX.Color4 Color;
        public SlimDX.DirectWrite.TextRange Range;

        public void Apply(TextLayout layout)
        {
            if (Enabled)
            {
                var factory = FontWrapperFactory.GetFactory();

                var sc = this.Color;
                SharpDX.Color4 c = *(SharpDX.Color4*)&sc;

                colorStyle = factory.CreateColor(c);

                SharpDX.DirectWrite.TextLayout tl = new SharpDX.DirectWrite.TextLayout(layout.ComPointer);
                tl.SetDrawingEffect(colorStyle.NativePointer, new SharpDX.DirectWrite.TextRange(Range.StartPosition, Range.Length));
                Marshal.Release(colorStyle.NativePointer);
            }
        }

        public void Dispose()
        {
            if (colorStyle != null)
            {
                colorStyle.Dispose();
                colorStyle = null;
            }
        }
    }
}
