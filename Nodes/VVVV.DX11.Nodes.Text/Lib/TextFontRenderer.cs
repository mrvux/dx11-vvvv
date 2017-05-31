using FeralTic.DX11.Resources;
using SharpFontWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11.Nodes.Text
{
    public class TextFontRenderer : IDX11Resource
    {
        private FontWrapper fontWrapper;

        public TextFontRenderer(FontWrapper fontWrapper)
        {
            if (fontWrapper == null)
                throw new ArgumentNullException("fontWrapper");

            this.fontWrapper = fontWrapper;
        }

        public FontWrapper FontWrapper
        {
            get { return this.fontWrapper; }
        }

        public void Dispose()
        {
            if (this.fontWrapper != null)
            {
                this.fontWrapper.Dispose();
                this.fontWrapper = null;
            }
        }
    }
}
