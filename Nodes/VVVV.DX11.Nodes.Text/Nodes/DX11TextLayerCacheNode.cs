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
    [PluginInfo(Name = "DrawTextCache", Author = "vux", Category = "DX11.Layer", Version = "Advanced")]
    public unsafe class DX11TextLayerCacheNode : IPluginEvaluate, IDX11LayerHost, IPartImportsSatisfiedNotification, IDisposable
    {
        private readonly SlimDX.DirectWrite.Factory dwFactory;
        private DX11TextObjectCache textCache;

        [Input("Text Renderer", Visibility = PinVisibility.OnlyInspector)]
        public Pin<DX11Resource<TextFontRenderer>> FTextRenderer;

        [Input("Text Format", AutoValidate = false)]
        public Pin<TextFormat> textFormat;

        [Input("Text Objects", AutoValidate = false)]
        public Pin<TextObject> textObjects;

        [Input("Rebuild Cache", IsSingle = true, IsBang = true, DefaultValue = 1, Order = 5)]
        public ISpread<bool> rebuildCache;

        [Input("Enabled", IsSingle = true, DefaultValue = 1, Order = 10)]
        public ISpread<bool> FInEnabled;

        [Output("Layer", IsSingle = true)]
        public ISpread<DX11Resource<DX11Layer>> FOutLayer;

        private int spreadMax;
        private List<DX11CachedText> cacheList = new List<DX11CachedText>();
        private DX11ContextElement<DX11ObjectRenderSettings> objectSettings = new DX11ContextElement<DX11ObjectRenderSettings>();


        [ImportingConstructor()]
        public DX11TextLayerCacheNode(SlimDX.DirectWrite.Factory dwFactory)
        {
            this.dwFactory = dwFactory;
        }

        public void OnImportsSatisfied()
        {
            this.FOutLayer[0] = new DX11Resource<DX11Layer>();
        }

        public void Evaluate(int SpreadMax)
        {
            this.spreadMax = SpreadMax;

            if (this.rebuildCache[0] || this.textCache == null)
            {
                this.textObjects.Sync();
                this.textFormat.Sync();

                if (this.textCache != null)
                {
                    this.textCache.Dispose();
                    this.textCache = null;
                }

                var defaultTextFormat = this.textFormat[0];

                this.cacheList.Clear();
                for (int i = 0; i < this.textObjects.SliceCount; i++)
                {
                    TextObject to = this.textObjects[i];
                    var tFormat = to.TextFormat != null ? to.TextFormat : defaultTextFormat;

                    TextLayout tl = new TextLayout(this.dwFactory, to.Text, tFormat);

                    SlimDX.Color4 c = to.Color;
                    DX11CachedText ct = new DX11CachedText(tl, to.Matrix, c);
                    cacheList.Add(ct);
                }

                this.textCache = new DX11TextObjectCache(cacheList);
            }
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

        public void Dispose()
        {
            if (this.textCache != null)
            {
                this.textCache.Dispose();
                this.textCache = null;
            }
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

                var sView = settings.View;
                var sProj = settings.Projection;

                SharpDX.Matrix view = *(SharpDX.Matrix*)&sView;
                SharpDX.Matrix projection = *(SharpDX.Matrix*)&sProj;


                var objectsettings = this.objectSettings[context];
                objectsettings.IterationCount = 1;
                objectsettings.Geometry = null;

                for (int i = 0; i < this.textCache.objects.Length; i++)
                {

                    SharpDX.Matrix mat = SharpDX.Matrix.Scaling(1.0f, -1.0f, 1.0f);
                    mat = SharpDX.Matrix.Multiply(mat, view);
                    mat = SharpDX.Matrix.Multiply(mat, projection);

                    objectsettings.DrawCallIndex = i;
                    objectsettings.WorldTransform = *(SlimDX.Matrix*)&mat;

                    if (settings.ValidateObject(objectsettings))
                    {

                        SlimDX.Color4 color = this.textCache.objects[i].Color;
                        SharpDX.Color4 sdxColor = *(SharpDX.Color4*)&color;

                        fw.DrawTextLayout(shaprdxContext, new SharpDX.DirectWrite.TextLayout(this.textCache.objects[i].TextLayout.ComPointer), SharpDX.Vector2.Zero,
                            mat, sdxColor, TextFlags.None);
                    }
                }

                //Apply old states back
                context.RenderStateStack.Apply();
                context.CleanShaderStages();
            }
        }
    }
}