using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using AssimpNet;
using SlimDX;

namespace VVVV.DX11.Nodes.AssetImport
{
    [PluginInfo(Name = "AnimationChannels", Category = "Assimp", Author = "vux,flateric")]
    public class AssimpChannelsNode : IPluginEvaluate
    {
        [Input("Channels")]
        protected IDiffSpread<AssimpAnimationChannel> FInChannels;

        [Output("Node Name")]
        protected ISpread<string> FOutName;

        [Output("Position Keys")]
        protected ISpread<ISpread<double>> FOutPosTime;

        [Output("Position Values")]
        protected ISpread<ISpread<Vector3>> FOutPosValues;

        [Output("Scaling Keys")]
        protected ISpread<ISpread<double>> FOutScaleTime;

        [Output("Scaling Values")]
        protected ISpread<ISpread<Vector3>> FOutScaleValues;

        [Output("Rotation Keys")]
        protected ISpread<ISpread<double>> FOutRotationTime;

        [Output("Rotation Values")]
        protected ISpread<ISpread<Quaternion>> FOutRotationValues;

        


        public void Evaluate(int SpreadMax)
        {
            if (this.FInChannels.IsChanged)
            {
                this.FOutName.SliceCount = this.FInChannels.SliceCount;
                this.FOutPosTime.SliceCount = this.FInChannels.SliceCount;
                this.FOutPosValues.SliceCount = this.FInChannels.SliceCount;
                this.FOutScaleTime.SliceCount = this.FInChannels.SliceCount;
                this.FOutScaleValues.SliceCount = this.FInChannels.SliceCount;
                this.FOutRotationTime.SliceCount = this.FInChannels.SliceCount;
                this.FOutRotationValues.SliceCount = this.FInChannels.SliceCount;

                for (int i = 0; i < this.FInChannels.SliceCount; i++)
                {
                    AssimpAnimationChannel chan = this.FInChannels[i];
                    this.FOutName[i] = chan.Name;
                    
                    //Position
                    this.FOutPosTime[i].SliceCount = chan.PositionKeys.Count;
                    this.FOutPosValues[i].SliceCount = chan.PositionKeys.Count;
                    
                    for (int j = 0; j < chan.PositionKeys.Count; j++)
                    {
                        this.FOutPosTime[i][j] = chan.PositionKeys[j].Time;
                        this.FOutPosValues[i][j] = chan.PositionKeys[j].Value;
                    }

                    //Scaling
                    this.FOutScaleTime[i].SliceCount = chan.ScalingKeys.Count;
                    this.FOutScaleValues[i].SliceCount = chan.ScalingKeys.Count;

                    for (int j = 0; j < chan.ScalingKeys.Count; j++)
                    {
                        this.FOutScaleTime[i][j] = chan.ScalingKeys[j].Time;
                        this.FOutScaleValues[i][j] = chan.ScalingKeys[j].Value;
                    }

                    //Rotation
                    this.FOutRotationTime[i].SliceCount = chan.RotationKeys.Count;
                    this.FOutRotationValues[i].SliceCount = chan.RotationKeys.Count;

                    for (int j = 0; j < chan.RotationKeys.Count; j++)
                    {
                        this.FOutRotationTime[i][j] = chan.RotationKeys[j].Time;
                        this.FOutRotationValues[i][j] = chan.RotationKeys[j].Value;
                    }
                }
            }
        }

    }
}
