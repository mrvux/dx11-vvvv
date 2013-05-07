using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;

using SlimDX;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "WithinView", Category = "DX11.Layer", Version = "")]
    public class WithinViewNode : AbstractDX11LayerSpaceNode
    {
        protected override void UpdateSettings(DX11RenderSettings settings)
        {
            settings.View = Matrix.Identity;
            settings.ViewProjection = settings.Projection;
        }
    }

    [PluginInfo(Name = "WithinProjection", Category = "DX11.Layer", Version = "")]
    public class WithinProjectionNode : AbstractDX11LayerSpaceNode
    {
        protected override void UpdateSettings(DX11RenderSettings settings)
        {
            settings.Projection = Matrix.Identity;
            settings.View = Matrix.Identity;
            settings.ViewProjection = Matrix.Identity;
        }
    }

    [PluginInfo(Name = "DepthOnly", Category = "DX11.Layer", Version = "")]
    public class DepthOnlyNode : AbstractDX11LayerSpaceNode
    {
        protected override void UpdateSettings(DX11RenderSettings settings)
        {
            settings.DepthOnly = true;

        }
    }

    [PluginInfo(Name = "ViewProjection", Category = "DX11.Layer", Version = "")]
    public class ViewProjectionNode : AbstractDX11LayerSpaceNode
    {
        [Input("View")]
        protected ISpread<Matrix> FView;

        [Input("Projection")]
        protected ISpread<Matrix> FProjection;

        protected override void UpdateSettings(DX11RenderSettings settings)
        {
            settings.Projection = FProjection[0];
            settings.View = FView[0];
            settings.ViewProjection = settings.View * settings.Projection;
        }
    }

    [PluginInfo(Name = "PixelBillBoard", Category = "DX11.Layer", Version = "")]
    public class PixelBillBoardNode : AbstractDX11LayerSpaceNode
    {
        protected override void UpdateSettings(DX11RenderSettings settings)
        {
            settings.View = Matrix.Identity;
            settings.Projection = Matrix.Scaling(1.0f / (float)settings.RenderWidth, 1.0f / (float)settings.RenderHeight, 1.0f);
            settings.ViewProjection = settings.Projection;
        }
    }
}
