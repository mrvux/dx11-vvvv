using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using SlimDX;
using FeralTic.Core.Maths;
using SlimDX.DirectWrite;

using DWriteFactory = SlimDX.DirectWrite.Factory;
using System.ComponentModel.Composition;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "HitTest", Category = "String", Version = "DirectWrite", Author = "vux")]
    public class HitTestTextNode : IPluginEvaluate
    {
        [Input("Text Layout",CheckIfChanged=true)]
        protected Pin<TextLayout> FLayout;

        [Input("Position")]
        protected IDiffSpread<Vector2> FPosition;

        [Output("Hit")]
        protected ISpread<bool> FHit;

        [Output("Index")]
        protected ISpread<int> FIndex;

        private DWriteFactory dwFactory;

        [ImportingConstructor()]
        public HitTestTextNode(DWriteFactory dwFactory)
        {
            this.dwFactory = dwFactory;
        }

        public void Evaluate(int SpreadMax)
        {
            if (!FLayout.IsConnected)
            {
                this.FHit.SliceCount = 0;
                this.FIndex.SliceCount = 0;
                return;
            }

            if (this.FLayout.IsChanged || this.FPosition.IsChanged)
            {
                this.FIndex.SliceCount = SpreadMax;
                this.FHit.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    TextLayout layout = this.FLayout[i];

                    bool hit;
                    bool trail;
                    var result = layout.HitTestPoint(this.FPosition[i].X,this.FPosition[i].Y,out trail, out hit);

                    this.FHit[i] = hit;
                    this.FIndex[i] = result.TextPosition;
                }
            }
        }
    }
}
