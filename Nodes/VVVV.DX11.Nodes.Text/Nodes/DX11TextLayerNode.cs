using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeralTic.DX11;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;
using SharpFontWrapper;
using VVVV.PluginInterfaces.V1;

namespace VVVV.DX11.Nodes.Text
{
    [PluginInfo(Name = "Text", Author = "vux", Category = "DX11.Layer", Version = "")]
    public unsafe class DX11TextLayerNode : IPluginEvaluate, IDX11LayerHost, IPartImportsSatisfiedNotification
    {
        private readonly IIOFactory iofactory;
        private readonly SlimDX.DirectWrite.Factory dwFactory;

        [Input("Text Renderer", Visibility = PinVisibility.OnlyInspector)]
        public Pin<DX11Resource<TextFontRenderer>> FTextRenderer;

        [Input("Render State")]
        public Pin<DX11RenderState> FStateIn;

        [Input("String", DefaultString = "DX11", Order = 0)]
        public ISpread<string> FInString;

        public ITransformIn transformIn;

        [Input("Font", EnumName = "DirectWrite_Font_Families", Order = 2)]
        public ISpread<EnumEntry> FFontInput;

        [Input("Size", Order = 5, DefaultValue = 32)]
        public ISpread<float> FInSize;

        [Input("Color", Order = 6, DefaultColor = new double[] { 1, 1, 1, 1 })]
        public ISpread<SlimDX.Color4> FInColor;

        [Input("Horizontal Align", EnumName = "HorizontalAlign", Order = 7)]
        public ISpread<EnumEntry> FHorizontalAlignInput;

        [Input("Vertical Align", EnumName = "VerticalAlign", Order = 8)]
        public ISpread<EnumEntry> FVerticalAlignInput;

        [Input("Normalize", EnumName = "Normalize", Order = 9)]
        public ISpread<EnumEntry> FNormalizeInput;

        [Input("Enabled", IsSingle = true, DefaultValue = 1, Order = 10)]
        public ISpread<bool> FInEnabled;

        [Output("Layer", IsSingle = true)]
        public ISpread<DX11Resource<DX11Layer>> FOutLayer;

        private int spreadMax;

        [ImportingConstructor()]
        public DX11TextLayerNode(IIOFactory factory, SlimDX.DirectWrite.Factory dwFactory)
        {
            this.iofactory = factory;
            this.dwFactory = dwFactory;

            this.iofactory.PluginHost.CreateTransformInput("Transform In", TSliceMode.Dynamic, TPinVisibility.True, out this.transformIn);
            this.transformIn.Order = 1;
        }

        public void OnImportsSatisfied()
        {
            this.FOutLayer[0] = new DX11Resource<DX11Layer>();
        }

        public void Evaluate(int SpreadMax)
        {
            this.spreadMax = SpreadMax;
        }

        public void Update(DX11RenderContext context)
        {
            if (!this.FOutLayer[0].Contains(context))
            {
                this.FOutLayer[0][context] = new DX11Layer();
                this.FOutLayer[0][context].Render = this.Render;
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {

        }

        private void Render(DX11RenderContext context, DX11RenderSettings settings)
        {
            if (this.spreadMax == 0)
                return;

            if (this.FInEnabled[0])
            {
                float w = (float)settings.RenderWidth;
                float h = (float)settings.RenderHeight;
                SharpDX.Direct3D11.DeviceContext shaprdxContext = new SharpDX.Direct3D11.DeviceContext(context.CurrentDeviceContext.ComPointer);

                FontWrapper fw = this.FTextRenderer.IsConnected ? this.FTextRenderer[0][context].FontWrapper : FontWrapperFactory.GetWrapper(context, this.dwFactory);

                var renderStates = fw.RenderStates;

                float* rawMatPtr;
                int transformCount;
                this.transformIn.GetMatrixPointer(out transformCount, out rawMatPtr);

                SharpDX.Matrix* matrixPointer = (SharpDX.Matrix*)rawMatPtr;

                bool applyState = this.FStateIn.IsConnected;

                var sView = settings.View;
                var sProj = settings.Projection;

                SharpDX.Matrix view = *(SharpDX.Matrix*)&sView;
                SharpDX.Matrix projection = *(SharpDX.Matrix*)&sProj;

                for (int i = 0; i < this.spreadMax; i++)
                {
                    SharpDX.Matrix preScale = SharpDX.Matrix.Scaling(1.0f, -1.0f, 1.0f);

                    switch (this.FNormalizeInput[i].Index)
                    {
                        case 1: preScale = SharpDX.Matrix.Scaling(1.0f / w, -1.0f / w, 1.0f); break;
                        case 2: preScale = SharpDX.Matrix.Scaling(1.0f / h, -1.0f / h, 1.0f); break;
                        case 3: preScale = SharpDX.Matrix.Scaling(1.0f / w, -1.0f / h, 1.0f); break;
                    }


                    SharpDX.Matrix sm = matrixPointer[i % transformCount];

                    SharpDX.Matrix mat = SharpDX.Matrix.Multiply(preScale, sm);
                    mat = SharpDX.Matrix.Multiply(mat, view);
                    mat = SharpDX.Matrix.Multiply(mat, projection);

                    SlimDX.Color4 color = this.FInColor[i];
                    SharpDX.Color4 sdxColor = *(SharpDX.Color4*)&color;


                    TextFlags flag = TextFlags.NoWordWrapping;

                    if (this.FHorizontalAlignInput[i].Index == 0) { flag |= TextFlags.Left; }
                    else if (this.FHorizontalAlignInput[i].Index == 1) { flag |= TextFlags.Center; }
                    else if (this.FHorizontalAlignInput[i].Index == 2) { flag |= TextFlags.Right; }

                    if (this.FVerticalAlignInput[i].Index == 0) { flag |= TextFlags.Top; }
                    else if (this.FVerticalAlignInput[i].Index == 1) { flag |= TextFlags.VerticalCenter; }
                    else if (this.FVerticalAlignInput[i].Index == 2) { flag |= TextFlags.Bottom; }

                    if (applyState)
                    {
                        renderStates.SetStates(shaprdxContext, 0);

                        context.RenderStateStack.Push(this.FStateIn[i]);

                        fw.DrawString(shaprdxContext, this.FInString[i], this.FFontInput[i], this.FInSize[i],
                            mat, null, sdxColor, flag | TextFlags.StatePrepared);

                        context.RenderStateStack.Pop();
                    }
                    else
                    {
                        fw.DrawString(shaprdxContext, this.FInString[i], this.FFontInput[i], this.FInSize[i],
                            mat, null, sdxColor, flag);
                    }
                }

                //Apply old states back
                context.RenderStateStack.Apply();
                context.CleanShaderStages();
            }
        }
    }
}