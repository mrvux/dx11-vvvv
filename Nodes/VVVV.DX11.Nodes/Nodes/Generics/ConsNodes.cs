using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Resources;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Cons", Category = "DX11.Geometry", Author = "vux")]
    public class ConsGeometryNode : DX11ResourceConsNode<IDX11Geometry> { }

    [PluginInfo(Name = "Cons", Category = "DX11.IndexedGeometry", Author = "vux")]
    public class ConsIndexedGeometryNode : DX11ResourceConsNode<DX11IndexedGeometry> { }

    [PluginInfo(Name = "Cons", Category = "DX11.Texture", Version = "3d", Author = "vux")]
    public class ConsTexture3DNode : DX11ResourceConsNode<DX11Texture3D> { }

    [PluginInfo(Name = "Cons", Category = "DX11.Texture", Version = "2d", Author = "vux")]
    public class ConsTexture2DNode : DX11ResourceConsNode<DX11Texture2D> { }

    [PluginInfo(Name = "Cons", Category = "DX11.Texture", Version = "1d", Author = "vux")]
    public class ConsTexture1DNode : DX11ResourceConsNode<DX11Texture1D> { }

    [PluginInfo(Name = "Cons", Category = "DX11.Buffer", Version = "", Author = "vux")]
    public class ConsBufferNode : DX11ResourceConsNode<IDX11ReadableStructureBuffer> { }

    [PluginInfo(Name = "Cons", Category = "DX11.Buffer RedableResource", Version = "", Author = "vux")]
    public class ConsBufferResourceNode : DX11ResourceConsNode<DX11RawBuffer> { }

    [PluginInfo(Name = "Cons", Category = "DX11.BufferRAW", Version = "", Author = "vux")]
    public class ConsBufferRAWNode : DX11ResourceConsNode<IDX11Buffer> { }

    [PluginInfo(Name = "Cons", Category = "DX11.ResourceSemantic", Version = "", Author = "vux")]
    public class ConsResourceSemanticNode : DX11ResourceConsNode<IDX11RenderSemantic> { }

    [PluginInfo(Name = "Cons", Category = "DX11.RenderSemantic", Version = "", Author = "vux")]
    public class ConsRenderSemanticNode : ConsNonNilNode<IDX11RenderSemantic> { }

    [PluginInfo(Name = "Cons", Category = "DX11.Validator", Version = "", Author = "vux")]
    public class ConsValidatorNode : ConsNonNilNode<IDX11ObjectValidator> { }

    [PluginInfo(Name = "Cons", Category = "DX11.Layer", Version = "", Author = "vux")]
    public class ConsLayerNode : DX11ResourceConsNode<DX11Layer> { }

}
