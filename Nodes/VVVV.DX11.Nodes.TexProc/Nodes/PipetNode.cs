using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Queries;
using SlimDX;
using FeralTic.DX11.Resources;
using System.Reflection;
using SlimDX.Direct3D11;
using FeralTic.DX11.Geometry;
using FeralTic.Core;
using VVVV.DX11.Nodes.TexProc;

namespace VVVV.DX11.Nodes
{
    //[PluginInfo(Name = "Pipet", Category = "DX11.Texture", Version ="2d", Author = "vux")]
    public class PipetNode : IPluginEvaluate, IDX11ResourceHost/*, IDX11Queryable,*/ //System.ComponentModel.Composition.IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Texture In")]
        protected Pin<DX11Resource<DX11Texture2D>> textureInput;

        [Input("Coordinates")]
        protected ISpread<Vector2> coordinates;

        [Input("Mip Level")]
        protected ISpread<float> mipLevel;

        [Input("Double Buffer", IsSingle =true)]
        protected ISpread<bool> doubleBuffer;

        [Input("Pixel Coordinates", IsSingle = true)]
        protected ISpread<bool> pixelCoords;

        [Output("Output")]
        protected ISpread<Color4> output;

        //[Output("Query", Order = 200, IsSingle = true)]
        //protected ISpread<IDX11Queryable> queryable;

        private static DX11Effect effectSample;
        private static DX11Effect effectLoad;
        private static DX11ShaderInstance shaderSample = null;
        private static DX11ShaderInstance shaderLoad = null;

        private DX11DynamicStructuredBuffer coordinateBuffer;
        private DX11DynamicStructuredBuffer levelBuffer;

        private DX11RWStructuredBuffer writeBuffer;
        private DX11StagingStructuredBuffer readbackBuffer;


        public void OnImportsSatisfied()
        {
            effectSample = DX11Effect.FromResource(System.Reflection.Assembly.GetExecutingAssembly(), Consts.EffectPath + ".Pipet_Sample.fx");
            effectLoad = DX11Effect.FromResource(System.Reflection.Assembly.GetExecutingAssembly(), Consts.EffectPath + ".Pipet_Load.fx");
        }

        public void Evaluate(int SpreadMax)
        {
            //this.queryable[0] = this;
            this.output.SliceCount = SpreadMax;
        } 

        #region IDX11ResourceProvider Members
        public void Update(DX11RenderContext context)
        {
            if (shaderSample == null)
            {
                shaderSample = new DX11ShaderInstance(context, effectSample);
                shaderLoad = new DX11ShaderInstance(context, effectLoad);
            }

            DX11ShaderInstance instance = this.pixelCoords[0] ? shaderLoad : shaderSample;

            if (this.mipLevel.SliceCount > 1)
            {
                instance.SelectTechnique("ConstantLevel");

            }
            else
            {
                instance.SelectTechnique("DynamicLevel");
            }

            int totalCount;
            if (this.mipLevel.SliceCount > 1)
            {
                totalCount = SpreadUtils.SpreadMax(this.coordinates, this.mipLevel);

                instance.SetByName("UvCount", this.coordinates.SliceCount);
                instance.SetByName("LevelCount", this.mipLevel.SliceCount);
            }
            else
            {
                totalCount = this.coordinates.SliceCount;
                instance.SetByName("MipLevel", this.mipLevel[0]);
            }

            this.coordinateBuffer = this.coordinateBuffer.GetOrResize(context.Device, this.coordinates.SliceCount, 8);
            this.levelBuffer = this.levelBuffer.GetOrResize(context.Device, this.mipLevel.SliceCount, 4);

            this.writeBuffer = this.writeBuffer.GetOrResize(context.Device, totalCount, 16);
            this.readbackBuffer = this.readbackBuffer.GetOrResize(context.Device, totalCount, 16);

            instance.SetByName("TotalCount", totalCount);
            instance.SetByName("uvBuffer", coordinateBuffer.SRV);
            if (this.mipLevel.SliceCount > 1)
            {
                instance.SetByName("uvLevelBuffer", levelBuffer.SRV);
            }


            instance.SetByName("inputTexture", this.textureInput[0][context].SRV);
            instance.SetByName("OutputBuffer", writeBuffer.UAV);

            instance.ApplyPass(0);

            context.CurrentDeviceContext.CopyResource(this.writeBuffer.Buffer, this.readbackBuffer.Buffer);
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
 
        }
        #endregion

        //public event DX11QueryableDelegate BeginQuery;

        //public event DX11QueryableDelegate EndQuery;

        public void Dispose()
        {

        }
    }
}
