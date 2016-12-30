using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using AssimpNet;
using SlimDX;

namespace VVVV.DX11.Nodes.AssetImport
{
    [PluginInfo(Name = "Info", Category = "Assimp", Version ="AnimationChannel", Author = "vux,flateric")]
    public class AssimpChannelInfoNode : IPluginEvaluate
    {
        [Input("Channels")]
        protected IDiffSpread<AssimpAnimationChannel> FInChannels;

        [Input("Ignore Duplicates", IsSingle =true)]
        protected IDiffSpread<bool> FinIgnoreDups;

        [Output("Node Name")]
        protected ISpread<string> FOutName;

        [Output("Start Time")]
        protected ISpread<double> FOutStart;

        [Output("End Time")]
        protected ISpread<double> FOutEnd;

        [Output("Length")]
        protected ISpread<double> FOutLength;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInChannels.IsChanged || this.FinIgnoreDups.IsChanged)
            {
                this.FOutName.SliceCount = this.FInChannels.SliceCount;
                this.FOutLength.SliceCount = this.FInChannels.SliceCount;
                this.FOutStart.SliceCount = this.FInChannels.SliceCount;
                this.FOutEnd.SliceCount = this.FInChannels.SliceCount;
                

                for (int i = 0; i < this.FInChannels.SliceCount; i++)
                {
                    double starttime = double.MaxValue;
                    double endtime = double.MinValue;

                    AssimpAnimationChannel chan = this.FInChannels[i];
                    this.FOutName[i] = chan.Name;
                    
                    for (int j = 0; j < chan.PositionKeys.Count; j++)
                    {
                        if (chan.PositionKeys.Count > 1)
                        {
                            if (this.FinIgnoreDups[0])
                            {
                                if (chan.PositionKeys.Count > 2)
                                {
                                    if (j == 0)
                                    {
                                        if (chan.PositionKeys[j].Value != chan.PositionKeys[j + 1].Value)
                                        {
                                            if (chan.PositionKeys[j].Time > endtime)
                                            {
                                                endtime = chan.PositionKeys[j].Time;
                                            }
                                            if (chan.PositionKeys[j].Time < starttime)
                                            {
                                                starttime = chan.PositionKeys[j].Time;
                                            }
                                        }

                                    }
                                    else if (j == chan.PositionKeys.Count -1)
                                    {
                                        if (chan.PositionKeys[j].Value != chan.PositionKeys[j - 1].Value)
                                        {
                                            if (chan.PositionKeys[j].Time > endtime)
                                            {
                                                endtime = chan.PositionKeys[j].Time;
                                            }
                                            if (chan.PositionKeys[j].Time < starttime)
                                            {
                                                starttime = chan.PositionKeys[j].Time;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (chan.PositionKeys[j].Time > endtime)
                                        {
                                            endtime = chan.PositionKeys[j].Time;
                                        }
                                        if (chan.PositionKeys[j].Time < starttime)
                                        {
                                            starttime = chan.PositionKeys[j].Time;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (chan.PositionKeys[j].Time > endtime)
                                {
                                    endtime = chan.PositionKeys[j].Time;
                                }
                                if (chan.PositionKeys[j].Time < starttime)
                                {
                                    starttime = chan.PositionKeys[j].Time;
                                }
                            }
                        }
                    }

                    for (int j = 0; j < chan.ScalingKeys.Count; j++)
                    {
                        if (this.FinIgnoreDups[0])
                        {
                            if (chan.ScalingKeys.Count > 2)
                            {
                                if (j == 0)
                                {
                                    if (chan.ScalingKeys[j].Value != chan.ScalingKeys[j + 1].Value)
                                    {
                                        if (chan.ScalingKeys[j].Time > endtime)
                                        {
                                            endtime = chan.ScalingKeys[j].Time;
                                        }
                                        if (chan.ScalingKeys[j].Time < starttime)
                                        {
                                            starttime = chan.ScalingKeys[j].Time;
                                        }
                                    }

                                }
                                else if (j == chan.ScalingKeys.Count - 1)
                                {
                                    if (chan.ScalingKeys[j].Value != chan.ScalingKeys[j - 1].Value)
                                    {
                                        if (chan.ScalingKeys[j].Time > endtime)
                                        {
                                            endtime = chan.ScalingKeys[j].Time;
                                        }
                                        if (chan.ScalingKeys[j].Time < starttime)
                                        {
                                            starttime = chan.ScalingKeys[j].Time;
                                        }
                                    }

                                }
                                else
                                {
                                    if (chan.ScalingKeys.Count > 1)
                                    {
                                        if (chan.ScalingKeys[j].Time > endtime)
                                        {
                                            endtime = chan.ScalingKeys[j].Time;
                                        }
                                        if (chan.ScalingKeys[j].Time < starttime)
                                        {
                                            starttime = chan.ScalingKeys[j].Time;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (chan.ScalingKeys.Count > 1)
                            {
                                if (chan.ScalingKeys[j].Time > endtime)
                                {
                                    endtime = chan.ScalingKeys[j].Time;
                                }
                                if (chan.ScalingKeys[j].Time < starttime)
                                {
                                    starttime = chan.ScalingKeys[j].Time;
                                }
                            }
                        }
                    }

                    for (int j = 0; j < chan.RotationKeys.Count; j++)
                    {
                        if (this.FinIgnoreDups[0])
                        {
                            if (chan.RotationKeys.Count > 2)
                            {
                                if (j == 0 )
                                {
                                    if (chan.RotationKeys[j].Value != chan.RotationKeys[j + 1].Value)
                                    {
                                        if (chan.RotationKeys[j].Time > endtime)
                                        {
                                            endtime = chan.RotationKeys[j].Time;
                                        }
                                        if (chan.RotationKeys[j].Time < starttime)
                                        {
                                            starttime = chan.RotationKeys[j].Time;
                                        }
                                    }

                                }
                                else if (j == chan.RotationKeys.Count - 1)
                                {
                                    if (chan.RotationKeys[j].Value != chan.RotationKeys[j - 1].Value)
                                    {
                                        if (chan.RotationKeys[j].Time > endtime)
                                        {
                                            endtime = chan.RotationKeys[j].Time;
                                        }
                                        if (chan.RotationKeys[j].Time < starttime)
                                        {
                                            starttime = chan.RotationKeys[j].Time;
                                        }
                                    }
                                }
                                else
                                {
                                    if (chan.RotationKeys[j].Time > endtime)
                                    {
                                        endtime = chan.RotationKeys[j].Time;
                                    }
                                    if (chan.RotationKeys[j].Time < starttime)
                                    {
                                        starttime = chan.RotationKeys[j].Time;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (chan.RotationKeys.Count > 1)
                            {
                                if (chan.RotationKeys[j].Time > endtime)
                                {
                                    endtime = chan.RotationKeys[j].Time;
                                }
                                if (chan.RotationKeys[j].Time < starttime)
                                {
                                    starttime = chan.RotationKeys[j].Time;
                                }
                            }
                        }
                    }

                    this.FOutStart[i] = starttime;
                    this.FOutEnd[i] = endtime;
                    this.FOutLength[i] = endtime - starttime;

                }
            }
        }

    }
}
