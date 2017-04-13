using VVVV.PluginInterfaces.V2;

using SlimDX;
using System;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "WithinView", Category = "DX11.Layer", Version = "")]
    public class WithinViewNode : AbstractDX11LayerSpaceNode
    {
        protected override int LayerCount
        {
            get { return 1; }
        }

        protected override void UpdateSettings(DX11RenderSettings settings, int slice)
        {
            settings.View = Matrix.Identity;
            settings.ViewProjection = settings.Projection;
        }
    }

    [PluginInfo(Name = "WithinProjection", Category = "DX11.Layer", Version = "")]
    public class WithinProjectionNode : AbstractDX11LayerSpaceNode
    {
        [Input("Preserve Aspect", DefaultValue=0, IsSingle=true)]
        protected ISpread<bool> FAspect;

        [Input("Preserve Crop", DefaultValue = 0, IsSingle = true)]
        protected ISpread<bool> FCrop;

        protected override int LayerCount
        {
            get { return 1; }
        }

        protected override void UpdateSettings(DX11RenderSettings settings, int slice)
        {
            settings.View = Matrix.Identity;

            settings.Projection = Matrix.Identity;
            if (FAspect[0])
            {
                settings.Projection = settings.Aspect;
            }
            if (FCrop[0])
            {
                settings.Projection = settings.Projection * settings.Crop;
            }
            settings.ViewProjection = settings.View * settings.Projection;
        }
    }

    [PluginInfo(Name = "DepthOnly", Category = "DX11.Layer", Version = "")]
    public class DepthOnlyNode : AbstractDX11LayerSpaceNode
    {
        protected override int LayerCount
        {
            get { return 1; }
        }

        protected override void UpdateSettings(DX11RenderSettings settings, int slice)
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

        [Input("Aspect Ratio")]
        protected ISpread<Matrix> FAspect;

        [Input("Crop")]
        protected ISpread<Matrix> FCrop;

        protected override int LayerCount
        {
            get { return SpreadUtils.SpreadMax(FView, FProjection, FAspect, FCrop); }
        }

        protected override void UpdateSettings(DX11RenderSettings settings, int slice)
        {
            settings.ApplyTransforms(FView[slice], FProjection[slice], FAspect[slice], FCrop[slice]);
        }
    }

    [PluginInfo(Name = "PixelBillBoard", Category = "DX11.Layer", Version = "")]
    public class PixelBillBoardNode : AbstractDX11LayerSpaceNode
    {
	    [Input("Transform In", IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<Matrix> FTransformIn;

        [Input("Double Scale", IsSingle = true, Order = 50)]
        protected ISpread<bool> FDoubleScale;

        [Input("Top Left", IsSingle = true, Order = 51)]
        protected ISpread<bool> FTopLeft;

        protected override int LayerCount
        {
            get { return SpreadUtils.SpreadMax(FTransformIn, FDoubleScale, FTopLeft); }
        }

        protected override void UpdateSettings(DX11RenderSettings settings, int slice)
        {
            float f = this.FDoubleScale[slice] ? 2.0f : 1.0f;

            settings.View = Matrix.Identity;
            settings.Projection = Matrix.Scaling(f / settings.RenderWidth,f / settings.RenderHeight, 1.0f) * FTransformIn[slice];

            if (FTopLeft[slice])
            {
                float tx = (float)settings.RenderWidth * 0.5f;
                float ty = (float)settings.RenderHeight * 0.5f;
                settings.Projection = Matrix.Translation(-tx, ty, 0.0f) * settings.Projection;
            }

            settings.ViewProjection = settings.Projection;
        }
    }

    [PluginInfo(Name = "AspectRatio", Category = "DX11.Layer", Version = "")]
    public class AspectRatioNode : AbstractDX11LayerSpaceNode
    {
        [Input("Transform In", IsSingle = true)]
        protected ISpread<Matrix> FTransformIn;

        [Input("Uniform Scale", DefaultValue=1, IsSingle = true)]
        protected ISpread<float> FScale;

        [Input("Alignment", DefaultEnumEntry = "FitIn", EnumName = "AspectRatioAlignment", IsSingle = true)]
        protected ISpread<EnumEntry> FAlign;

        protected override int LayerCount
        {
            get { return 1; }
        }

        protected override void UpdateSettings(DX11RenderSettings settings, int slice)
        {
            float w = settings.RenderWidth;
            float h = settings.RenderHeight;

            float sx, sy;

            #region Build scale
            if (FAlign[0].Name == "FitOut")
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
            else if (FAlign[0].Name == "FitIn")
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
