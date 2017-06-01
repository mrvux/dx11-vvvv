using BulletSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Bullet.Core.Filters;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Bullet.Nodes.Bodies.Rigid.Filters
{
    [PluginInfo(Name = "IsStatic", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Filter interface to find static bodies only")]
    public class BulletStaticFilterNpde : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Output("Output", IsSingle = true)]
        protected ISpread<StaticOnlyFilter> output;


        public void OnImportsSatisfied()
        {
            this.output[0] = new StaticOnlyFilter();
        }

        public void Evaluate(int SpreadMax)
        {
        }
    }

    [PluginInfo(Name = "IsDynamic", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Filter interface to find dynamic bodies only")]
    public class BulletDynamicFilterNpde : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Output("Output", IsSingle = true)]
        protected ISpread<DynamicOnlyFilter> output;


        public void OnImportsSatisfied()
        {
            this.output[0] = new DynamicOnlyFilter();
        }

        public void Evaluate(int SpreadMax)
        {
        }
    }
}
