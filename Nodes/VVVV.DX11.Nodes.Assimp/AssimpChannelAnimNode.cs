using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using AssimpNet;
using SlimDX;

namespace VVVV.DX11.Nodes.AssetImport
{
    [PluginInfo(Name = "AnimationChannels", Category = "Assimp",Version="Animation", Author = "vux,flateric")]
    public class AssimpAnimationChannelsNode : IPluginEvaluate
    {
        [Input("Channels")]
        protected IDiffSpread<AssimpAnimationChannel> FInChannels;

        [Input("Time")]
        protected IDiffSpread<double> FInTime;

        [Input("Duration")]
        protected IDiffSpread<double> FInDuration;

        [Input("Absolute Time")]
        protected IDiffSpread<bool> FInAbsoluteTime;

        [Output("Node Name")]
        protected ISpread<string> FOutName;

        [Output("Position")]
        protected ISpread<Vector3> FOutPos;

        [Output("Scale")]
        protected ISpread<Vector3> FOutScale;

        [Output("Rotation")]
        protected ISpread<Quaternion> FOutRotation;

        public void Evaluate(int SpreadMax)
        {
            if (SpreadMax == 0)
            {
                this.FOutName.SliceCount = 0;
                this.FOutPos.SliceCount = 0;
                this.FOutRotation.SliceCount = 0;
                this.FOutScale.SliceCount = 0;
            }
            else
            {
                if (this.FInChannels.IsChanged || this.FInTime.IsChanged)
                {
                    this.FOutName.SliceCount = this.FInChannels.SliceCount;
                    this.FOutPos.SliceCount = this.FInChannels.SliceCount;
                    this.FOutRotation.SliceCount = this.FInChannels.SliceCount;
                    this.FOutScale.SliceCount = this.FInChannels.SliceCount;


                    for (int i = 0; i < Math.Max(this.FInChannels.SliceCount, this.FInTime.SliceCount); i++)
                    {
                        AssimpAnimationChannel chan = this.FInChannels[i];
                        this.FOutName[i] = chan.Name;

                        double t = this.FInTime[i];
                        double duration = this.FInDuration[i];

                        double dt = this.FInAbsoluteTime[0] ? t : t * duration;

                        this.FOutPos[i] = this.InterpolatePosition(dt, chan);
                        this.FOutScale[i] = this.InterpolateScale(dt, chan);
                        this.FOutRotation[i] = this.InterpolateRotation(dt, chan);

                    }
                }
            }
        }

        private Vector3 InterpolatePosition(double dt, AssimpAnimationChannel chan)
        {
            if (chan.PositionKeys.Count == 1) { return chan.PositionKeys[0].Value; }

            Vector3 pos;
            int pc = chan.PositionKeys.Count - 1;
            if (dt < chan.PositionKeys[0].Time)
            { pos = chan.PositionKeys[0].Value; } //Before first element
            else if (dt > chan.PositionKeys[pc].Time)
            { pos = chan.PositionKeys[pc].Value; } //After last element
            else
            {
                double mindist = double.MaxValue;
                int idx = -1;
                for (int j = 0; j < chan.PositionKeys.Count; j++)
                {
                    double dist = Math.Abs(dt - chan.PositionKeys[j].Time);
                    if (dist < mindist) { mindist = dist; idx = j; }
                }

                AnimVectorKey curr = chan.PositionKeys[idx];

                //Interpolate with next or previous
                if (dt > chan.PositionKeys[idx].Time)
                {
                    //Interpolate with next
                    AnimVectorKey next = chan.PositionKeys[idx + 1];
                    double amount = (dt - curr.Time) / (next.Time - curr.Time);
                    pos = Vector3.Lerp(curr.Value, next.Value, Convert.ToSingle(amount));
                }
                else
                {
                    if (idx == 0)
                    {
                        pos = chan.PositionKeys[0].Value;
                    }
                    else
                    {
                        //Interpolate with previous
                        AnimVectorKey prev = chan.PositionKeys[idx - 1];
                        double amount = (dt - prev.Time) / (curr.Time - prev.Time);
                        pos = Vector3.Lerp(prev.Value, curr.Value, Convert.ToSingle(amount));
                    }
                }
            }
            return pos;
        }

        private Vector3 InterpolateScale(double dt, AssimpAnimationChannel chan)
        {
            if (chan.ScalingKeys.Count == 1) { return chan.ScalingKeys[0].Value; }

            Vector3 pos;
            int pc = chan.ScalingKeys.Count - 1;
            if (dt < chan.ScalingKeys[0].Time)
            { pos = chan.ScalingKeys[0].Value; } //Before first element
            else if (dt > chan.ScalingKeys[pc].Time)
            { pos = chan.ScalingKeys[pc].Value; } //After last element
            else
            {
                double mindist = double.MaxValue;
                int idx = -1;
                for (int j = 0; j < chan.ScalingKeys.Count; j++)
                {
                    double dist = Math.Abs(dt - chan.ScalingKeys[j].Time);
                    if (dist < mindist) { mindist = dist; idx = j; }
                }

                AnimVectorKey curr = chan.ScalingKeys[idx];

                //Interpolate with next or previous
                if (dt > chan.ScalingKeys[idx].Time)
                {
                    //Interpolate with next
                    AnimVectorKey next = chan.ScalingKeys[idx + 1];
                    double amount = (dt - curr.Time) / (next.Time - curr.Time);
                    pos = Vector3.Lerp(curr.Value, next.Value, Convert.ToSingle(amount));
                }
                else
                {
                    if (idx == 0)
                    {
                        pos = chan.ScalingKeys[0].Value;
                    }
                    else
                    {
                        //Interpolate with previous
                        AnimVectorKey prev = chan.ScalingKeys[idx - 1];
                        double amount = (dt - prev.Time) / (curr.Time - prev.Time);
                        pos = Vector3.Lerp(prev.Value, curr.Value, Convert.ToSingle(amount));
                    }
                }
            }
            return pos;
        }

        private Quaternion InterpolateRotation(double dt, AssimpAnimationChannel chan)
        {
            if (chan.RotationKeys.Count == 1) { return chan.RotationKeys[0].Value; }

            Quaternion quat;
            int pc = chan.RotationKeys.Count - 1;
            if (dt < chan.RotationKeys[0].Time)
            { quat = chan.RotationKeys[0].Value; } //Before first element
            else if (dt > chan.RotationKeys[pc].Time)
            { quat = chan.RotationKeys[pc].Value; } //After last element
            else
            {
                double mindist = double.MaxValue;
                int idx = -1;
                for (int j = 0; j < chan.RotationKeys.Count; j++)
                {
                    double dist = Math.Abs(dt - chan.RotationKeys[j].Time);
                    if (dist < mindist) { mindist = dist; idx = j; }
                }

                AnimQuatKey curr = chan.RotationKeys[idx];

                //Interpolate with next or previous
                if (dt > chan.RotationKeys[idx].Time)
                {
                    //Interpolate with next
                    AnimQuatKey next = chan.RotationKeys[idx + 1];
                    double amount = (dt - curr.Time) / (next.Time - curr.Time);
                    quat = Quaternion.Slerp(curr.Value, next.Value, Convert.ToSingle(amount));
                }
                else
                {
                    if (idx == 0)
                    {
                        quat = chan.RotationKeys[0].Value;
                    }
                    else
                    {
                        //Interpolate with previous
                        AnimQuatKey prev = chan.RotationKeys[idx - 1];
                        double amount = (dt - prev.Time) / (curr.Time - prev.Time);
                        quat = Quaternion.Slerp(prev.Value, curr.Value, Convert.ToSingle(amount));
                    }
                }
            }
            return quat;
        }
    }
}
