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
    public enum eRenderHint { Forward, MRT, Shadow, Overlay, Collector, ApplyOnly }


    public partial class DX11RenderSettings
    {
        public DX11RenderSettings()
        {
            this.View = Matrix.Identity;
            this.Projection = Matrix.Identity;
            this.Aspect = Matrix.Identity;
            this.Crop = Matrix.Identity;
            this.ViewProjection = Matrix.Identity;
            this.RawProjection = Matrix.Identity;
            this.CustomSemantics = new List<IDX11RenderSemantic>();
            this.ObjectValidators = new List<IDX11ObjectValidator>();
            this.ResourceSemantics = new List<DX11Resource<IDX11RenderSemantic>>();
            this.LayerOrder = null;
            this.RenderSpace = new DX11RenderSpace();
            this.PreferredTechniques = new List<string>();
            this.ViewportCount = 1;
            this.ViewportIndex = 0;
            this.RenderHint = eRenderHint.Forward;
            this.SceneDescriptor = new DX11RenderScene();
            this.WorldTransform = Matrix.Identity;
        }

        public Matrix WorldTransform;

        public DX11RenderSpace RenderSpace { get; set; }

        public eRenderHint RenderHint { get; set; }

        public DX11RenderScene SceneDescriptor { get; set; }


        /// <summary>
        /// Arbitrary tag to set a custom object for this layer
        /// </summary>
        public object Tag { get; set; }

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
        /// Layer opacity
        /// </summary>
        public float LayerOpacity = 1.0f;

        /// <summary>
        /// If true, asks the shader to keep current pipeline and not set other shader to null,
        /// this can be useful if your shader provides pixel/geometry shader and you want the node to provide a vs only
        /// </summary>
        public bool PreserveShaderStages { get; set; }

        public List<string> PreferredTechniques { get; set; }

        public List<IDX11RenderSemantic> CustomSemantics { get; set; }

        public List<DX11Resource<IDX11RenderSemantic>> ResourceSemantics { get; set; }

        public List<IDX11ObjectValidator> ObjectValidators { get; set; }

        public IDX11LayerOrder LayerOrder { get; set; }

        public bool ValidateObject(DX11ObjectRenderSettings obj)
        {
            for (int i = 0; i < this.ObjectValidators.Count; i++)
            {
                IDX11ObjectValidator objval = this.ObjectValidators[i];
                if (objval.Enabled)
                {
                    if (!objval.Validate(obj)) { return false; }
                }
            }
            return true;
        }

        public bool ApplySemantics(DX11ShaderInstance instance, List<IDX11CustomRenderVariable> variables)
        {
            for (int i = 0; i < this.CustomSemantics.Count; i++)
            {
                if (!this.CustomSemantics[i].Apply(instance, variables)) { return false; }
            }

            for (int i = 0; i < this.ResourceSemantics.Count; i++)
            {
                if (!this.ResourceSemantics[i][instance.RenderContext].Apply(instance, variables)) { return false; }
            }

            return true;
        }

        public int GetPreferredTechnique(DX11Effect shader)
        {
            string[] techniqueNames = shader.TechniqueNames;

            foreach (string pref in this.PreferredTechniques)
            {
                for (int i = 0; i < techniqueNames.Length; ++i)
                {
                    if (techniqueNames[i].ToLower() == pref)
                    {
                        return i;
                    }
                }
            }

            return -1;
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
    }
}
