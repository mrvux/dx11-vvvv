using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using VVVV.DX11.Internals.Effects.Pins;
using VVVV.DX11.Internals;
using VVVV.DX11.Internals.Effects;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.DX11.Lib.Effects.Registries;
using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11;
using VVVV.DX11.Effects;

namespace VVVV.DX11.Lib.Effects
{
    public class DX11ShaderVariableManager
    {
        protected DX11Effect shader;
        private IPluginHost host;
        private IIOFactory iofactory;

        private ShaderPinDictionary shaderpins = new ShaderPinDictionary();
        private RenderVariableDictionary rendervariables = new RenderVariableDictionary();
        private WorldRenderVariableDictionary worldvariables = new WorldRenderVariableDictionary();

        private List<IDX11CustomRenderVariable> customvariables = new List<IDX11CustomRenderVariable>();

        private DX11RenderSettings globalsettings;

        public DX11ShaderVariableManager(IPluginHost host, IIOFactory iofactory)
        {
            this.host = host;
            this.iofactory = iofactory;
        }

        public void SetShader(DX11Effect shader)
        {
            this.shader = shader;
        }

        #region Create Shader Pins
        public void CreateShaderPins()
        {
            #region Build Pins
            for (int i = 0; i < this.shader.DefaultEffect.Description.GlobalVariableCount; i++)
            {
                EffectVariable var = this.shader.DefaultEffect.GetVariableByIndex(i);
                this.CreatePin(var);
            }
            #endregion
        }
        #endregion

        #region Update Shader Pins
        public void UpdateShaderPins()
        {
            //Get rid of custom variables
            this.customvariables.Clear();

            this.shaderpins.UpdateEffect(this.shader.DefaultEffect);

            this.rendervariables.UpdateEffect(this.shader.DefaultEffect);

            this.worldvariables.UpdateEffect(this.shader.DefaultEffect);

            //Add new pins
            for (int i = 0; i < this.shader.DefaultEffect.Description.GlobalVariableCount; i++)
            {
                EffectVariable var = this.shader.DefaultEffect.GetVariableByIndex(i);

                //Need to be added to one or the other
                if (!this.shaderpins.Contains(var.Description.Name)
                    && !this.rendervariables.Contains(var.Description.Name)
                    && !this.worldvariables.Contains(var.Description.Name))
                {
                    this.CreatePin(var);
                }
            }
        }
        #endregion

        #region Create Pin
        private void CreatePin(EffectVariable var)
        {
            
            if (var.AsInterface() != null)
            {
                if (var.LinkClasses().Length == 0)
                {
                    if (var.GetVariableType().Description.Elements == 0)
                    {
                        /*InterfaceShaderPin ip = new InterfaceShaderPin(var, this.host, this.iofactory);
                        ip.ParentEffect = this.shader.DefaultEffect;
                        this.shaderpins.Add(var.Description.Name, ip);*/
                    }

                }
                else
                {
                       RestrictedInterfaceShaderPin rp = new RestrictedInterfaceShaderPin();
                       rp.Initialize(this.iofactory, var);
                       rp.ParentEffect = this.shader.DefaultEffect;
                       this.shaderpins.Add(var.Description.Name, rp);
                }
                return;
            }

            //Search for render variable first
            if (ShaderPinFactory.IsRenderVariable(var))
            {
                IRenderVariable rv = ShaderPinFactory.GetRenderVariable(var, this.host,this.iofactory);
                this.rendervariables.Add(rv.Name, rv);
            }
            else if (ShaderPinFactory.IsWorldRenderVariable(var))
            {
                IWorldRenderVariable wv = ShaderPinFactory.GetWorldRenderVariable(var, this.host, this.iofactory);
                this.worldvariables.Add(wv.Name, wv);
            }
            else if (ShaderPinFactory.IsShaderPin(var))
            {
                IShaderPin sp = ShaderPinFactory.GetShaderPin(var, this.host, this.iofactory);
                if (sp != null) { this.shaderpins.Add(sp.Name, sp); }
            }
            else
            {
                if (var.Description.Semantic != "IMMUTABLE" && var.Description.Semantic != "")
                {
                    this.customvariables.Add(new DX11CustomRenderVariable(var));
                }
            }
        }
        #endregion

        public bool SetGlobalSettings(DX11ShaderInstance instance, DX11RenderSettings settings)
        {
            this.globalsettings = settings;

            return settings.ApplySemantics(instance, this.customvariables);
        }

        #region Apply Global
        public void ApplyGlobal(DX11ShaderInstance instance)
        {
            //this.globalsettings = settings;

            this.rendervariables.Apply(instance, this.globalsettings);

            this.shaderpins.Preprocess(instance);
        }
        #endregion

        #region Apply Per object
        public void ApplyPerObject(DX11RenderContext context, DX11ShaderInstance instance, DX11ObjectRenderSettings objectsettings, int slice)
        {
            this.worldvariables.Apply(instance, this.globalsettings, objectsettings);

            this.shaderpins.ApplySlice(instance, slice);

        }
        #endregion

        public int CalculateSpreadMax()
        {
            return this.shaderpins.SpreadMax;
        }

    }
}
