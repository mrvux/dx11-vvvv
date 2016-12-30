using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using AssimpNet;
using SlimDX;

namespace VVVV.DX11.Nodes.AssetImport
{
    [PluginInfo(Name = "Animation", Category = "Assimp", Author = "vux,flateric")]
    public class AssimpAnimationTransformNode : IPluginEvaluate
    {
        [Input("Scene", IsSingle = true)]
        protected IDiffSpread<AssimpScene> FInScene;

        [Output("Name")]
        protected ISpread<string> FOutName;

        [Output("Duration")]
        protected ISpread<double> FOutDuration;

        [Output("Ticks Per Second")]
        protected ISpread<double> FOutTPS;

        [Output("Channels")]
        protected ISpread<ISpread<AssimpAnimationChannel>> FOutChannels;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInScene.IsChanged)
            {
                if (this.FInScene[0] != null)
                {
                    int animcnt = this.FInScene[0].Animations.Count;

                    this.FOutName.SliceCount = animcnt;
                    this.FOutDuration.SliceCount = animcnt;
                    this.FOutTPS.SliceCount = animcnt;
                    this.FOutChannels.SliceCount = animcnt;


                    for (int i = 0; i < animcnt; i++)
                    {
                        AssimpAnimation anim = this.FInScene[0].Animations[i];

                        this.FOutName[i] = anim.Name;
                        this.FOutDuration[i] = anim.Duration;
                        this.FOutTPS[i] = anim.TicksPerSecond;
                        this.FOutChannels[i].AssignFrom(anim.Channels);      
                    }
                }
                else
                {
                    this.FOutName.SliceCount = 0;
                    this.FOutDuration.SliceCount = 0;
                    this.FOutTPS.SliceCount = 0;
                    this.FOutChannels.SliceCount = 0;
                }

            }
        }

    }
}
