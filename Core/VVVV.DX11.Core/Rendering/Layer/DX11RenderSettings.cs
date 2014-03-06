using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

using VVVV.DX11.Effects;

namespace VVVV.DX11
{
    public enum eRenderHint { Forward, MRT, Shadow, Overlay, Collector }


    public partial class DX11RenderSettings
    {
        public DX11RenderSettings()
        {
            this.View = Matrix.Identity;
            this.Projection = Matrix.Identity;
            this.Aspect = Matrix.Identity;
            this.Crop = Matrix.Identity;
            this.ViewProjection = Matrix.Identity;
            this.CustomSemantics = new List<IDX11RenderSemantic>();
            this.ObjectValidators = new List<IDX11ObjectValidator>();
            this.ResourceSemantics = new List<DX11Resource<IDX11RenderSemantic>>();
            this.RenderSpace = new DX11RenderSpace();
            this.PrefferedTechnique = "";
            this.ViewportCount = 1;
            this.ViewportIndex = 0;
            this.RenderHint = eRenderHint.Forward;
            this.SceneDescriptor = new DX11RenderScene();
        }

        public DX11RenderSpace RenderSpace { get; set; }

        public eRenderHint RenderHint { get; set; }

        public DX11RenderScene SceneDescriptor { get; set; }

        /// <summary>
        /// Renderer Width
        /// </summary>
        public int RenderWidth { get; set; }

        /// <summary>
        /// Renderer Height
        /// </summary>
        public int RenderHeight { get; set; }

        /// <summary>
        /// Renderer Depth
        /// </summary>
        public int RenderDepth { get; set; }

        /// <summary>
        /// Index of current viewport
        /// </summary>
        public int ViewportIndex { get; set; }

        /// <summary>
        /// Total number of viewports
        /// </summary>
        public int ViewportCount { get; set; }

        /// <summary>
        /// If true, asks the shader to keep current pipeline and not set other shader to null,
        /// this can be useful if your shader provides pixel/geometry shader and you want the node to provide a vs only
        /// </summary>
        public bool PreserveShaderStages { get; set; }

        public List<IDX11RenderSemantic> CustomSemantics { get; set; }

        public List<DX11Resource<IDX11RenderSemantic>> ResourceSemantics { get; set; }

        public List<IDX11ObjectValidator> ObjectValidators { get; set; }

        public bool ValidateObject(DX11ObjectRenderSettings obj)
        {
            foreach (IDX11ObjectValidator objval in this.ObjectValidators)
            {
                if (objval.Enabled)
                {
                    if (!objval.Validate(obj)) { return false; }
                }
            }
            return true;
        }

        public bool ApplySemantics(DX11ShaderInstance instance, List<IDX11CustomRenderVariable> variables)
        {
            foreach (IDX11RenderSemantic semantic in this.CustomSemantics)
            {
                if (!semantic.Apply(instance, variables)) { return false; }
            }

            foreach (DX11Resource<IDX11RenderSemantic> semantic in this.ResourceSemantics)
            {
                if (semantic[instance.RenderContext] != null)
                {
                    if (!semantic[instance.RenderContext].Apply(instance, variables)) { return false; }
                }
            }

            return true;
        }

        /// <summary>
        /// How many draw calls this shader is gonna do
        /// </summary>
        public int DrawCallCount { get; set; } 

        /// <summary>
        /// BackBuffer (If want to write with compute)
        /// </summary>
        public IDX11RWResource BackBuffer { get; set; }

        /// <summary>
        /// Readable buffer
        /// </summary>
        public IDX11ReadableResource ReadBuffer { get; set; }

        public IDX11Geometry Geometry { get; set; }

        /// <summary>
        /// Tells that we want to reset counter
        /// </summary>
        public bool ResetCounter { get; set; }

        public int CounterValue { get; set; }

        public bool DepthOnly { get; set; }

        public string PrefferedTechnique { get; set; }
    }
}
