using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using DWriteFactory = SlimDX.DirectWrite.Factory;


namespace VVVV.Nodes.DirectWrite.TextLayer
{
    [PluginInfo(Name = "Typography", Category = "DirectWrite",Version="Styles", Tags = "layout,text", Author="vux")]
    public class FontTypographyNode : TextStyleBaseNode
    {
        [Input("Feature Tag")]
        protected IDiffSpread<FontFeatureTag> FFontInput;

        private DWriteFactory dwFactory;

        [ImportingConstructor()]
        public FontTypographyNode(DWriteFactory dwFactory)
        {
            this.dwFactory = dwFactory;
        }

        private class FontTypographyStyle : TextStyleBase
        {
            public FontFeatureTag tag;

            private DWriteFactory dwFactory;

            public FontTypographyStyle(DWriteFactory dwFactory)
            {
                this.dwFactory = dwFactory;
            }

            protected override void DoApply(TextLayout layout, TextRange range)
            {
                Typography tp = new Typography(this.dwFactory);
                tp.AddFeature(new FontFeature()
                    {
                        NameTag = tag,
                        Value = 1
                    });
                
                layout.SetTypography(tp, range);
            }
        }

        protected override TextStyleBaseNode.TextStyleBase CreateStyle(int slice)
        {
            return new FontTypographyStyle(this.dwFactory)
            {
                tag = FFontInput[slice]
            };
        }
    }
}


