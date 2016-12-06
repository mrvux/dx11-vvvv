using SlimDX;
using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.DX11
{
    public class TextObject
    {
        public TextFormat TextFormat;
        public string Text;
        public Matrix Matrix;
        public Color4 Color;
    }

    public class DX11CachedText
    {
        public DX11CachedText(TextLayout tl, Matrix mat, Color4 color)
        {
            this.TextLayout = tl;
            this.Matrix = mat;
            this.Color = color;
        }

        public TextLayout TextLayout;
        public Matrix Matrix;
        public Color4 Color;
    }

    public class DX11TextObjectCache : IDisposable
    {
        public DX11CachedText[] objects;

        public DX11TextObjectCache(IEnumerable<DX11CachedText> objects)
        {
            this.objects = objects.ToArray();
        }

        public DX11CachedText Get(int index)
        {
            return objects[index];
        }

        public void Dispose()
        {
            for (int i = 0; i < this.objects.Length; i++)
            {
                this.objects[i].TextLayout.Dispose();
            }
        }
    }
}
