using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using Microsoft.Kinect.Toolkit.Interaction;
using SlimDX;
using VVVV.Utils.VMath;

namespace VVVV.DX11.Nodes.Nodes
{
    [PluginInfo(Name = "UserInfo", Category = "Kinect", Version = "Microsoft", Author = "vux")]
    public class KinectUserInteractionInfoNode : IPluginEvaluate
    {
        [Input("User Info")]
        protected Pin<UserInfo> FInUI;

        [Output("Hand Count")]
        protected ISpread<int> FOutHandCount;

        [Output("Is Tracked")]
        protected ISpread<bool> FOutIsTracked;

        [Output("Is Pressed")]
        protected ISpread<bool> FOutIsPressed;

        [Output("Position")]
        protected ISpread<Vector2D> FOutHandPos;

        [Output("Press Extent")]
        protected ISpread<double> FOutExtent;

        [Output("Hand Type")]
        protected ISpread<string> FOutHandType;

        [Output("Hand Interaction Type")]
        protected ISpread<string> FOutHandInterType;



        public void Evaluate(int SpreadMax)
        {
            if (this.FInUI.PluginIO.IsConnected)
            {
                this.FOutHandPos.SliceCount = this.FInUI.SliceCount*2;
                this.FOutHandType.SliceCount = this.FInUI.SliceCount * 2;
                this.FOutHandInterType.SliceCount = this.FInUI.SliceCount * 2;
                this.FOutIsTracked.SliceCount = this.FInUI.SliceCount * 2;
                this.FOutIsPressed.SliceCount = this.FInUI.SliceCount * 2;
                this.FOutExtent.SliceCount = this.FInUI.SliceCount * 2;

                this.FOutHandCount.SliceCount = this.FInUI.SliceCount;

                int cnt = 0;
                for (int i = 0; i < this.FInUI.SliceCount; i++)
                {
                    UserInfo ui = this.FInUI[i];
                    this.FOutHandCount[i] = ui.HandPointers.Count;

                    for (int j = 0 ; j < ui.HandPointers.Count;j++)
                    {
                        this.FOutHandPos[cnt] = new Vector2D(ui.HandPointers[j].X, ui.HandPointers[j].Y);
                        this.FOutHandType[cnt] = ui.HandPointers[j].HandType.ToString();
                        this.FOutHandInterType[cnt] = ui.HandPointers[j].HandEventType.ToString();

                        this.FOutExtent[cnt] = ui.HandPointers[j].PressExtent;
                        this.FOutIsPressed[cnt] = ui.HandPointers[j].IsPressed;
                        this.FOutIsTracked[cnt] = ui.HandPointers[j].IsTracked;

                        cnt++;
                    }

                }
            }
            else
            {
                this.FOutHandPos.SliceCount = 0;
                this.FOutHandCount.SliceCount = 0;
                this.FOutHandType.SliceCount = 0;
                this.FOutHandInterType.SliceCount = 0;
                this.FOutIsTracked.SliceCount = 0;
                this.FOutIsPressed.SliceCount = 0;
                this.FOutExtent.SliceCount = 0;
            }
        }
    }
}
