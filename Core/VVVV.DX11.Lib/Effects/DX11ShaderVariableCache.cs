using FeralTic.DX11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.DX11.Internals.Effects.Pins;

namespace VVVV.DX11.Lib.Effects
{
    public class DX11ShaderVariableCache
    {
        private List<Action<int>> shaderPinActions = new List<Action<int>>();
        private List<Action<int>> spreadedpins = new List<Action<int>>();
        private List<IShaderPin> shaderPins = new List<IShaderPin>();

        private List<Action<DX11RenderSettings>> globalActions = new List<Action<DX11RenderSettings>>();
        private List<Action<DX11RenderSettings, DX11ObjectRenderSettings>> worldActions = new List<Action<DX11RenderSettings, DX11ObjectRenderSettings>>();
        //private List<Action> 

        private DX11RenderSettings globalsettings;
        public DX11ShaderVariableCache(DX11RenderContext context,DX11ShaderInstance shader, DX11ShaderVariableManager shaderManager)
        {
            shaderPins = shaderManager.ShaderPins.VariablesList;
            for (int i = 0; i < shaderPins.Count;i++)
            {
                this.shaderPinActions.Add(shaderPins[i].CreateAction(shader));
            }
            var world = shaderManager.WorldVariables.VariablesList;
            for (int i = 0; i < world.Count; i++)
            {
                this.worldActions.Add(world[i].CreateAction(shader));
            }
            var global = shaderManager.RenderVariables.VariablesList;
            for (int i = 0; i < global.Count; i++)
            {
                this.globalActions.Add(global[i].CreateAction(shader));
            }
        }

        public void ApplyGlobals(DX11RenderSettings settings)
        {
            this.globalsettings = settings;
            this.spreadedpins.Clear();

            for (int i = 0; i < this.globalActions.Count; i++)
            {
                this.globalActions[i](settings);
            }

            for (int i = 0; i < this.shaderPinActions.Count; i++)
            {
                if (this.shaderPins[i].Constant)
                {
                    this.shaderPinActions[i](0);
                }
                else
                {
                    this.spreadedpins.Add(this.shaderPinActions[i]);
                }
                    
            }
        }

        public void ApplySlice(DX11ObjectRenderSettings objectsettings, int slice)
        {
            for (int i = 0; i < this.spreadedpins.Count; i++)
            {
                this.spreadedpins[i](slice);
            }
            for (int i = 0; i < this.worldActions.Count; i++)
            {
                this.worldActions[i](this.globalsettings, objectsettings);
            }
        }
    }
}
