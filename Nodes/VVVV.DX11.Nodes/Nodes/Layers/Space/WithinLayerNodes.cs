using VVVV.PluginInterfaces.V2;

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
	    [Input("Transform In", IsSingle = true, Visibility = PinVisibility.OnlyInspector)] 
		private ISpread<Matrix> FTransformIn;

        [Input("Double Scale", IsSingle = true, Order = 50)]
        private ISpread<bool> FDoubleScale;
        
		protected override void UpdateSettings(DX11RenderSettings settings)
        {
            float f = this.FDoubleScale[0] ? 2.0f : 1.0f;

            settings.View = Matrix.Identity;
            settings.Projection = Matrix.Scaling(f / settings.RenderWidth,f / settings.RenderHeight, 1.0f) * FTransformIn[0];
            settings.ViewProjection = settings.Projection;
        }
    }

    [PluginInfo(Name = "AspectRatio", Category = "DX11.Layer", Version = "")]
    public class AspectRatioNode : AbstractDX11LayerSpaceNode
    {
        [Input("Transform In", IsSingle = true)]
        private ISpread<Matrix> FTransformIn;

        [Input("Uniform Scale", DefaultValue=1, IsSingle = true)]
        ISpread<float> FScale;

        [Input("Alignment", DefaultEnumEntry = "FitIn", EnumName = "AspectRatioAlignment", IsSingle = true)]
        ISpread<EnumEntry> FAlign;

        protected override void UpdateSettings(DX11RenderSettings settings)
        {
            float w = settings.RenderWidth;
            float h = settings.RenderHeight;

            float sx, sy;

            #region Build scale
            if (FAlign[0].Name == "FitIn")
            {
                if (w > h)
                {
                    sx = h / w;
                    sy = 1.0f;
                }
                else
                {
                    sx = 1.0f;
                    sy = w / h;
                }
            }
            else if (FAlign[0].Name == "FitOut")
            {
                if (w > h)
                {
                    sx = 1.0f;
                    sy = w / h;
                }
                else
                {
                    sx = h / w;
                    sy = 1.0f;
                }
            }
            else if (FAlign[0].Name == "FitWidth")
            {
                sx = 1.0f;
                sy = w / h;
            }
            else //FitHeight
            {
                sy = 1.0f;
                sx = h / w;
            }
            #endregion

            Matrix aspect = Matrix.Scaling(sx* FScale[0], sy * FScale[0], 1.0f);

            settings.Projection = settings.Projection * aspect * FTransformIn[0];
            settings.ViewProjection = settings.View * settings.Projection;
        }
    }

    
}
