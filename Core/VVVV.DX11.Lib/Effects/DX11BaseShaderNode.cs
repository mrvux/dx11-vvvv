using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using VVVV.DX11.Internals.Effects;
using System.IO;
using VVVV.DX11.Nodes.Layers;

using FeralTic.DX11;
using FeralTic.DX11.Queries;


namespace VVVV.DX11.Lib.Effects
{
    public abstract class DX11BaseShaderNode : IPartImportsSatisfiedNotification, IDX11ShaderNodeWrapper, IDX11Queryable
    {
        protected IPluginHost FHost;
        protected IIOFactory FFactory;
        protected DX11Effect FShader;
        protected bool FInvalidate;

        [Input("Iterastion Count", Order = 10000, DefaultValue = 1,MinValue=1,Visibility=PinVisibility.OnlyInspector)]
        protected ISpread<int> FIter;

        [Input("Enabled", Order = 10000, DefaultValue = 1)]
        protected ISpread<bool> FInEnabled;

        protected IDiffSpread<EnumEntry> FInTechnique;
        protected string TechniqueEnumId;

        [Output("Compiled")]
        protected ISpread<bool> FOutCompiled;

        [Output("Query",Order=200, IsSingle=true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        #region Config Pins
        [Config("Path")]
        protected ISpread<string> FCfgSavePath;

        [Config("Do Save", IsBang = true)]
        protected IDiffSpread<bool> FCfgSave;
        #endregion

        #region Virtual and abstract
        protected virtual void ImportsSatistieds() { }
        #endregion

        #region Save shader
        void FCfgSave_Changed(IDiffSpread<bool> spread)
        {
            if (spread[0])
            {
                this.FShader.SaveByteCode(this.FCfgSavePath[0]);
            }
        }
        #endregion

        public void OnImportsSatisfied()
        {
            this.FCfgSave.Changed += new SpreadChangedEventHander<bool>(FCfgSave_Changed);
            //this.FInInclude.Changed += new SpreadChangedEventHander<DX11Include>(FInInclude_Changed);

            this.FOutQueryable[0] = this;

            this.ImportsSatistieds();
        }

        public abstract void SetShader(DX11Effect shader, bool isnew);

        protected void OnBeginQuery(DX11RenderContext context)
        {
            if (this.BeginQuery != null)
            {
                this.BeginQuery(context);
            }
        }

        protected void OnEndQuery(DX11RenderContext context)
        {
            if (this.EndQuery != null)
            {
                this.EndQuery(context);
            }
        }


        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;
    }
}