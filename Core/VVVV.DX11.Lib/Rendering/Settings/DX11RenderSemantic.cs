using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.DX11.Lib.Effects;
using FeralTic.Resources;

using FeralTic.DX11.Resources;
using FeralTic.DX11;
using VVVV.DX11.Effects;


namespace VVVV.DX11.Lib.Rendering
{
    /// <summary>
    /// Custom semantic that can be provided by the renderer
    /// </summary>
    public abstract class DX11RenderSemantic<T> : IDX11RenderSemantic
    {
        public DX11RenderSemantic(string semantic, bool mandatory)
        {
            this.Semantic = semantic;
            this.Mandatory = mandatory;
        }

        public T Data { get; set; }

        /// <summary>
        /// Types allowed for the variable
        /// </summary>
        public string[] TypeNames { get; protected set; }

        /// <summary>
        /// Semantic name
        /// </summary>
        public string Semantic { get; protected set; }

        /// <summary>
        /// If this semantic has not been bound, prevent the shader from running
        /// </summary>
        public bool Mandatory { get; protected set; }


        /// <summary>
        /// Try to find a custom semantic to a particular instance
        /// </summary>
        /// <param name="instance">Shader instance</param>
        /// <param name="variables">Custom variable list</param>
        /// <returns>If not mandatory, always return true, else, return true only if bound</returns>
        public bool Apply(DX11ShaderInstance instance, List<IDX11CustomRenderVariable> variables)
        {
            foreach (IDX11CustomRenderVariable variable in variables)
            {
                foreach(string typeName in this.TypeNames)
                {
                    if(variable.TypeName == typeName && variable.Semantic == this.Semantic)
                    {
                        this.ApplyVariable(variable.Name, instance);
                        return true;
                    }
                }
            }
            //Not bound
            return !this.Mandatory;
        }

        protected abstract void ApplyVariable(string name,DX11ShaderInstance instance);

        public void Dispose() { }
    }

}
