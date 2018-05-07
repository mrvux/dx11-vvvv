using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;


using FeralTic.DX11;
using FeralTic.DX11.Queries;

using VVVV.DX11.Lib.Devices;
using VVVV.DX11.Lib.RenderGraph;
using SlimDX.Direct3D11;
using FeralTic.Utils;


namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "About", Category = "DX11",Version="", Author = "vux",Tags= "", AutoEvaluate=true, Help ="Gets information about DirectX11 package, including author, contributors and patreon/private supporters")]
    public class DX11AboutNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        private readonly string[] contributorList = new string[]
        {
            "unc",
            "cloneproduction",
            "sebllll",
            "joreg",
            "dotprodukt",
            "alg",
            "azeno",
            "gumilastik",
            "fibo",
            "id144",
            "mhusinsky",
            "tmp",
            "mholub",
            "gregsn",
            "velcrome"
        };

        private readonly string[] supporterList = new string[]
        {
            "Irwin Quemener - CloneProduction",
            "Intolight",
            "Meso",
            "Wirmachenbunt",
            "Microdee",
            "Ivan Kabalin",
            "Daniel Huber",
            "m4d",
            "Chris Plant",
            "Kyle McLean",
            "Andres",
            "Lev Panov",
            "Natan Sinigaglia",
            "Antokhio",
            "Andres",
            "Paul",
            "Jonas Häutle"
        };

        [Output("Version")]
        protected ISpread<string> version;

        [Output("Author")]
        protected ISpread<string> author;

        [Output("Contributors")]
        protected ISpread<string> contributors;

        [Output("Supporters")]
        protected ISpread<string> supporters;

        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {
        }

        public void OnImportsSatisfied()
        {
            this.version[0] = "1.3";
            this.author[0] = "vux";

            this.contributors.AssignFrom(this.contributorList);
            this.supporters.AssignFrom(this.supporterList);
        }
        #endregion
    }
}
