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
using SlimDX.DirectWrite;

namespace VVVV.DX11.Nodes.Text
{
    [PluginInfo(Name = "Text", Author = "vux", Category = "DX11.Layer", Version = "Advanced")]
    public unsafe class DX11TextLayerAdvancedNode : IPluginEvaluate, IDX11LayerHost, IPartImportsSatisfiedNotification
    {
        private readonly IIOFactory iofactory;
        private readonly SlimDX.DirectWrite.Factory dwFactory;

        [Input("Text Renderer", Visibility = PinVisibility.OnlyInspector)]
        public Pin<DX11Resource<TextFontRenderer>> FTextRenderer;

        [Input("Render State")]
        public Pin<DX11RenderState> FStateIn;

        [Input("Text Layout", CheckIfChanged = true)]
        public Pin<TextLayout> FLayout;

        public ITransformIn transformIn;

        [Input("Color", Order = 6, DefaultColor = new double[] { 1, 1, 1, 1 })]
        public ISpread<SlimDX.Color4> FInColor;

        [Input("Enabled", IsSingle = true, DefaultValue = 1, Order = 10)]
        public ISpread<bool> FInEnabled;

        [Output("Layer", IsSingle = true)]
        public ISpread<DX11Resource<DX11Layer>> FOutLayer;

        private int spreadMax;
        private DX11ContextElement<DX11ObjectRenderSettings> objectSettings = new DX11ContextElement<DX11ObjectRenderSettings>();

        [ImportingConstructor()]
        public DX11TextLayerAdvancedNode(IIOFactory factory, SlimDX.DirectWrite.Factory dwFactory)
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
            if (!this.objectSettings.Contains(context))
            {
                this.objectSettings[context] = new DX11ObjectRenderSettings();
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

                var objectsettings = this.objectSettings[context];
                objectsettings.IterationCount = 1;
                objectsettings.Geometry = null;

                for (int i = 0; i < this.spreadMax; i++)
                {
                    SharpDX.Matrix preScale = SharpDX.Matrix.Scaling(1.0f, -1.0f, 1.0f);

                    SharpDX.Matrix sm = matrixPointer[i % transformCount];

                    SharpDX.Matrix mat = SharpDX.Matrix.Multiply(preScale, sm);
                    mat = SharpDX.Matrix.Multiply(mat, view);
                    mat = SharpDX.Matrix.Multiply(mat, projection);

                    SlimDX.Color4 color = this.FInColor[i];
                    SharpDX.Color4 sdxColor = *(SharpDX.Color4*)&color;

                    objectsettings.DrawCallIndex = i;
                    objectsettings.WorldTransform = *(SlimDX.Matrix*)&mat;

                    if (settings.ValidateObject(objectsettings))
                    {
                        if (applyState)
                        {
                            var textLayout = this.FLayout[i];

                            if (textLayout != null)
                            {
                                renderStates.SetStates(shaprdxContext, 0);

                                context.RenderStateStack.Push(this.FStateIn[i]);

                                fw.DrawTextLayout(shaprdxContext, new SharpDX.DirectWrite.TextLayout(textLayout.ComPointer), SharpDX.Vector2.Zero,
                                    mat, sdxColor, TextFlags.StatePrepared);

                                context.RenderStateStack.Pop();
                            }
                        }
                        else
                        {
                            var textLayout = this.FLayout[i];

                            if (textLayout != null)
                            {
                                fw.DrawTextLayout(shaprdxContext, new SharpDX.DirectWrite.TextLayout(textLayout.ComPointer), SharpDX.Vector2.Zero,
                                    mat, sdxColor, TextFlags.None);
                            }
                        }
                    }
                }

                //Apply old states back
                context.RenderStateStack.Apply();
                context.CleanShaderStages();
            }
        }
    }
}