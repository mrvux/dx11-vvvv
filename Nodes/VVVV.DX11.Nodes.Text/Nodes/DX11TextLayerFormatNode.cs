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
    [PluginInfo(Name = "Text", Author = "vux", Category = "DX11.Layer", Version = "Format")]
    public unsafe class DX11TextLayerFormatNode : IPluginEvaluate, IDX11LayerHost, IPartImportsSatisfiedNotification
    {
        private readonly IIOFactory iofactory;
        private readonly SlimDX.DirectWrite.Factory dwFactory;
        private SharpDX.Direct3D11.DeviceContext shaprdxContext;

        [Input("Text Renderer", Visibility = PinVisibility.OnlyInspector)]
        public Pin<DX11Resource<TextFontRenderer>> FTextRenderer;

        [Input("Render State")]
        public Pin<DX11RenderState> FStateIn;

        [Input("String", DefaultString = "DX11", Order = 0)]
        public ISpread<string> FInString;

        public ITransformIn transformIn;

        [Input("Text Format", Order = 2)]
        public Pin<SlimDX.DirectWrite.TextFormat> FFontInput;

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
        private DX11ContextElement<DX11ObjectRenderSettings> objectSettings = new DX11ContextElement<DX11ObjectRenderSettings>();

        [ImportingConstructor()]
        public DX11TextLayerFormatNode(IIOFactory factory, SlimDX.DirectWrite.Factory dwFactory)
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

            if (this.FInEnabled[0] && this.FFontInput.IsConnected)
            {
                float w = (float)settings.RenderWidth;
                float h = (float)settings.RenderHeight;

                if (shaprdxContext == null)
                {
                    shaprdxContext = new SharpDX.Direct3D11.DeviceContext(context.CurrentDeviceContext.ComPointer);
                }

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

                var sWorld = settings.WorldTransform;
                SharpDX.Matrix layerWorld = *(SharpDX.Matrix*)&sWorld;

                var objectsettings = this.objectSettings[context];
                objectsettings.IterationCount = 1;
                objectsettings.Geometry = null;

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
                    mat = SharpDX.Matrix.Multiply(mat, layerWorld);
                    mat = SharpDX.Matrix.Multiply(mat, view);
                    mat = SharpDX.Matrix.Multiply(mat, projection);

                    objectsettings.DrawCallIndex = i;
                    objectsettings.WorldTransform = *(SlimDX.Matrix*)&mat;
                    objectsettings.WorldTransform *= settings.WorldTransform;

                    string s = this.FInString[i];
                    if (s == null)
                    {
                        s = "";
                    }

                    if (this.FFontInput[i] != null)
                    {
                        if (settings.ValidateObject(objectsettings))
                        {
                            using (var tl = new SlimDX.DirectWrite.TextLayout(this.dwFactory, s, this.FFontInput[i]))
                            {
                                TextFlags flag = TextFlags.None;
                                tl.WordWrapping = SlimDX.DirectWrite.WordWrapping.NoWrap;


                                if (this.FHorizontalAlignInput[i].Index == 0) { tl.TextAlignment = SlimDX.DirectWrite.TextAlignment.Leading; }
                                else if (this.FHorizontalAlignInput[i].Index == 1) { tl.TextAlignment = SlimDX.DirectWrite.TextAlignment.Center; }
                                else if (this.FHorizontalAlignInput[i].Index == 2) { tl.TextAlignment = SlimDX.DirectWrite.TextAlignment.Trailing; }

                                if (this.FVerticalAlignInput[i].Index == 0) { tl.ParagraphAlignment = SlimDX.DirectWrite.ParagraphAlignment.Near; }
                                else if (this.FVerticalAlignInput[i].Index == 1) { tl.ParagraphAlignment = SlimDX.DirectWrite.ParagraphAlignment.Center; }
                                else if (this.FVerticalAlignInput[i].Index == 2) { tl.ParagraphAlignment = SlimDX.DirectWrite.ParagraphAlignment.Far; }

                                SlimDX.Color4 color = this.FInColor[i];
                                color.Alpha *= SharpDX.MathUtil.Clamp(settings.LayerOpacity, 0.0f, 1.0f);
                                SharpDX.Color4 sdxColor = *(SharpDX.Color4*)&color;

                                if (applyState)
                                {
                                    renderStates.SetStates(shaprdxContext, 0);

                                    context.RenderStateStack.Push(this.FStateIn[i]);

                                    fw.DrawTextLayout(shaprdxContext, new SharpDX.DirectWrite.TextLayout(tl.ComPointer), SharpDX.Vector2.Zero, mat, sdxColor, flag | TextFlags.StatePrepared);

                                    context.RenderStateStack.Pop();
                                }
                                else
                                {
                                    fw.DrawTextLayout(shaprdxContext, new SharpDX.DirectWrite.TextLayout(tl.ComPointer), SharpDX.Vector2.Zero, mat, sdxColor, flag);
                                }
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