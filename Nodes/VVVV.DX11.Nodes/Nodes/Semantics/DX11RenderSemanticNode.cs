using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

using VVVV.DX11.Lib.Rendering;



namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "RenderSemantic", Category = "DX11.Layer", Version = "Texture2D")]
    public class DX11Texture2dSemanticNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Input")]
        protected Pin<DX11Resource<DX11Texture2D>> FInput;

        [Input("Semantic", DefaultString = "SEMANTIC")]
        protected ISpread<string> FSemantic;

        [Input("Mandatory", DefaultValue = 0)]
        protected ISpread<bool> FMandatory;

        [Output("Output")]
        protected ISpread<DX11Resource<Texture2dRenderSemantic>> FOutput;

        public void Evaluate(int SpreadMax)
        {
            this.FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FOutput[i] == null) { this.FOutput[i] = new DX11Resource<Texture2dRenderSemantic>(); }
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInput.PluginIO.IsConnected)
            {
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    this.FOutput[i][context] = new Texture2dRenderSemantic(this.FSemantic[i], this.FMandatory[i]);

                    if (this.FInput[i].Contains(context))
                    {
                        this.FOutput[i][context].Data = this.FInput[i][context];
                    }
                    else
                    {
                        this.FOutput[i][context].Data = null;
                    }
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                this.FOutput[i].Dispose(context);
            }
        }
    }

    [PluginInfo(Name = "RenderSemantic", Category = "DX11.Layer", Version = "Texture2DArray")]
    public class DX11Texture2dArraySemanticNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Input")]
        protected Pin<DX11Resource<DX11Texture2D>> FInput;

        [Input("Semantic", DefaultString = "SEMANTIC")]
        protected ISpread<string> FSemantic;

        [Input("Mandatory", DefaultValue = 0)]
        protected ISpread<bool> FMandatory;

        [Output("Output")]
        protected ISpread<DX11Resource<Texture2dArrayRenderSemantic>> FOutput;

        public void Evaluate(int SpreadMax)
        {
            this.FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FOutput[i] == null) { this.FOutput[i] = new DX11Resource<Texture2dArrayRenderSemantic>(); }
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInput.PluginIO.IsConnected)
            {
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    this.FOutput[i][context] = new Texture2dArrayRenderSemantic(this.FSemantic[i], this.FMandatory[i]);

                    if (this.FInput[i].Contains(context))
                    {
                        this.FOutput[i][context].Data = this.FInput[i][context];
                    }
                    else
                    {
                        this.FOutput[i][context].Data = null;
                    }
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                this.FOutput[i].Dispose(context);
            }
        }
    }

    [PluginInfo(Name = "RenderSemantic", Category = "DX11.Layer", Version = "TextureCube")]
    public class DX11TextureCubeSemanticNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Input")]
        protected Pin<DX11Resource<DX11Texture2D>> FInput;

        [Input("Semantic", DefaultString = "SEMANTIC")]
        protected ISpread<string> FSemantic;

        [Input("Mandatory", DefaultValue = 0)]
        protected ISpread<bool> FMandatory;

        [Output("Output")]
        protected ISpread<DX11Resource<TextureCubeRenderSemantic>> FOutput;

        public void Evaluate(int SpreadMax)
        {
            this.FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FOutput[i] == null) { this.FOutput[i] = new DX11Resource<TextureCubeRenderSemantic>(); }
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInput.PluginIO.IsConnected)
            {
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    this.FOutput[i][context] = new TextureCubeRenderSemantic(this.FSemantic[i], this.FMandatory[i]);

                    if (this.FInput[i].Contains(context))
                    {
                        this.FOutput[i][context].Data = this.FInput[i][context];
                    }
                    else
                    {
                        this.FOutput[i][context].Data = null;
                    }
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                this.FOutput[i].Dispose(context);
            }
        }
    }

    [PluginInfo(Name = "RenderSemantic", Category = "DX11", Version = "StructuredBuffer")]
    public class DX11SBufferSemanticNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Input")]
        protected Pin<DX11Resource<IDX11ReadableStructureBuffer>> FInput;

        [Input("Semantic", DefaultString = "SEMANTIC")]
        protected ISpread<string> FSemantic;

        [Input("Mandatory", DefaultValue = 0)]
        protected ISpread<bool> FMandatory;

        [Output("Output")]
        protected ISpread<DX11Resource<StructuredBufferRenderSemantic>> FOutput;

        public void Evaluate(int SpreadMax)
        {
            this.FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FOutput[i] == null) { this.FOutput[i] = new DX11Resource<StructuredBufferRenderSemantic>(); }
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInput.PluginIO.IsConnected)
            {
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    this.FOutput[i][context] = new StructuredBufferRenderSemantic(this.FSemantic[i], this.FMandatory[i]);

                    if (this.FInput[i].Contains(context))
                    {
                        this.FOutput[i][context].Data = this.FInput[i][context];
                    }
                    else
                    {
                        this.FOutput[i][context].Data = null;
                    }
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                this.FOutput[i].Dispose(context);
            }
        }
    }

    [PluginInfo(Name = "RenderSemantic", Category = "DX11", Version = "ByteAddressBuffer")]
    public class DX11BABufferSemanticNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Input")]
        protected Pin<DX11Resource<IDX11ReadableResource>> FInput;

        [Input("Semantic", DefaultString = "SEMANTIC")]
        protected ISpread<string> FSemantic;

        [Input("Mandatory", DefaultValue = 0)]
        protected ISpread<bool> FMandatory;

        [Output("Output")]
        protected ISpread<DX11Resource<BufferRenderSemantic>> FOutput;

        public void Evaluate(int SpreadMax)
        {
            this.FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FOutput[i] == null) { this.FOutput[i] = new DX11Resource<BufferRenderSemantic>(); }
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInput.PluginIO.IsConnected)
            {
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    this.FOutput[i][context] = new BufferRenderSemantic(this.FSemantic[i], this.FMandatory[i]);

                    if (this.FInput[i].Contains(context))
                    {
                        this.FOutput[i][context].Data = this.FInput[i][context];
                    }
                    else
                    {
                        this.FOutput[i][context].Data = null;
                    }
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                this.FOutput[i].Dispose(context);
            }
        }
    }

    [PluginInfo(Name = "RenderSemantic", Category = "DX11", Version = "Texture3D")]
    public class DX11Texture3dSemanticNode : IPluginEvaluate, IDX11ResourceHost
    {
        [Input("Input")]
        protected Pin<DX11Resource<DX11Texture3D>> FInput;

        [Input("Semantic", DefaultString = "SEMANTIC")]
        protected ISpread<string> FSemantic;

        [Input("Mandatory", DefaultValue = 0)]
        protected ISpread<bool> FMandatory;

        [Output("Output")]
        protected ISpread<DX11Resource<Texture3dRenderSemantic>> FOutput;

        public void Evaluate(int SpreadMax)
        {
            this.FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FOutput[i] == null) { this.FOutput[i] = new DX11Resource<Texture3dRenderSemantic>(); }
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.FInput.PluginIO.IsConnected)
            {
                for (int i = 0; i < this.FOutput.SliceCount; i++)
                {
                    this.FOutput[i][context] = new Texture3dRenderSemantic(this.FSemantic[i], this.FMandatory[i]);

                    if (this.FInput[i].Contains(context))
                    {
                        this.FOutput[i][context].Data = this.FInput[i][context];
                    }
                    else
                    {
                        this.FOutput[i][context].Data = null;
                    }
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                this.FOutput[i].Dispose(context);
            }
        }
    }
}
