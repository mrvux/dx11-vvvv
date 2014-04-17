using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

using System.ComponentModel.Composition;

using FeralTic.DX11;
using FeralTic.DX11.Queries;

namespace VVVV.DX11.Nodes
{
    public abstract class AbstractQueryNode<T> : IPluginEvaluate, IPartImportsSatisfiedNotification where T : IDX11Query
    {
        [Input("Query Source", IsSingle = true, CheckIfChanged = true)]
        protected Pin<IDX11Queryable> FSource;

        bool invalidateconnect = false;

        private IDX11Queryable oldsource;

        protected T queryobject;

        protected abstract T CreateQueryObject(DX11RenderContext context);
        protected abstract void OnEvaluate();

        public void Evaluate(int SpreadMax)
        {
            if (this.invalidateconnect || this.FSource.IsChanged)
            {
                if (this.oldsource != null)
                {
                    this.oldsource.BeginQuery -= new DX11QueryableDelegate(AbstractQueryNode_BeginQuery);
                    this.oldsource.EndQuery -= new DX11QueryableDelegate(AbstractQueryNode_EndQuery);
                }
                if (this.FSource[0] != null)
                {

                    this.FSource[0].BeginQuery += new DX11QueryableDelegate(AbstractQueryNode_BeginQuery);
                    this.FSource[0].EndQuery += new DX11QueryableDelegate(AbstractQueryNode_EndQuery);

                   
                }
                this.oldsource = this.FSource[0];
                this.invalidateconnect = false;
            }

            if (this.queryobject != null && this.oldsource != null)
            {
                this.queryobject.GetData();
            }

            this.OnEvaluate();
        }

        public void OnImportsSatisfied()
        {
            this.FSource.Connected += new PinConnectionEventHandler(FSource_Connected);
            this.FSource.Disconnected += new PinConnectionEventHandler(FSource_Disconnected);
        }

        void FSource_Disconnected(object sender, PinConnectionEventArgs args)
        {
            this.invalidateconnect = true;
            if (this.oldsource != null)
            {
                this.oldsource.BeginQuery -= new DX11QueryableDelegate(AbstractQueryNode_BeginQuery);
                this.oldsource.EndQuery -= new DX11QueryableDelegate(AbstractQueryNode_EndQuery);
            }
            this.oldsource = null;
        }

        void AbstractQueryNode_EndQuery(DX11RenderContext context)
        {

            this.queryobject.Stop();
        }

        void AbstractQueryNode_BeginQuery(DX11RenderContext context)
        {
            if (this.queryobject == null) { this.queryobject = this.CreateQueryObject(context); }
            this.queryobject.Start();
        }


        void FSource_Connected(object sender, PinConnectionEventArgs args)
        {
            this.invalidateconnect = true;
        }


    }
}
