using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Resources;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "AllowFeedback", Category = "DX11.Geometry", Author = "microdee")]
    public class AllowFeedbackGeometryNode : AllowFeedback<DX11Resource<IDX11Geometry>> { }

    [PluginInfo(Name = "AllowFeedback", Category = "DX11.Texture", Version = "3d", Author = "microdee")]
    public class AllowFeedbackTexture3DNode : AllowFeedback<DX11Resource<DX11Texture3D>> { }

    [PluginInfo(Name = "AllowFeedback", Category = "DX11.Texture", Version = "2d", Author = "microdee")]
    public class AllowFeedbackTexture2DNode : AllowFeedback<DX11Resource<DX11Texture2D>> { }

    [PluginInfo(Name = "AllowFeedback", Category = "DX11.Texture", Version = "1d", Author = "microdee")]
    public class AllowFeedbackTexture1DNode : AllowFeedback<DX11Resource<DX11Texture1D>> { }

    [PluginInfo(Name = "AllowFeedback", Category = "DX11.Buffer", Version = "", Author = "microdee")]
    public class AllowFeedbackBufferNode : AllowFeedback<DX11Resource<IDX11ReadableStructureBuffer>> { }

    [PluginInfo(Name = "AllowFeedback", Category = "DX11.ReadableResource", Version = "", Author = "microdee")]
    public class AllowFeedbackReadableResourceNode : AllowFeedback<DX11Resource<IDX11ReadableResource>> { }

    [PluginInfo(Name = "AllowFeedback", Category = "DX11.Rawbuffer", Version = "", Author = "microdee")]
    public class AllowFeedbackRawbufferNode : AllowFeedback<DX11Resource<DX11RawBuffer>> { }

}
