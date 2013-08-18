using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace VVVV.DX11
{
    public class DX11RenderObject
    {
        public string ObjectType { get; set; }

        public object Descriptor { get; set; }

        public Matrix[] Transforms { get; set; }

    }

    public class DX11ObjectGroup
    {
        public DX11ObjectGroup()
        {
            this.RenderObjects = new List<DX11RenderObject>();
            this.Semantics = new List<IDX11RenderSemantic>();
        }

        public string ShaderName { get; set; }

        public List<DX11RenderObject> RenderObjects { get; set; }

        public List<IDX11RenderSemantic> Semantics { get; set; }
    }
}
