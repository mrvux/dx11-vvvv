using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp.SoftBody;
using VVVV.Utils.VMath;
using VVVV.Internals.Bullet;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="GetSoftBodyDetails",Category="Bullet",
		Help = "Gets some info about a soft body", Author = "vux")]
	public class BulletGetSoftBodyDetailsNode : IPluginEvaluate
	{
		[Input("Bodies")]
        protected Pin<SoftBody> FBodies;

		[Output("Nodes")]
        protected ISpread<ISpread<Vector3D>> FOutNodes;

		[Output("Mass")]
        protected ISpread<float> FOutMass;

		[Output("Custom")]
        protected ISpread<string> FOutCustom;

		[Output("Id")]
        protected ISpread<int> FOutId;
	
		public void  Evaluate(int SpreadMax)
		{
            if (this.FBodies.IsConnected)
            {
                this.FOutNodes.SliceCount = SpreadMax;
                this.FOutCustom.SliceCount = SpreadMax;
                this.FOutId.SliceCount = SpreadMax;
                this.FOutMass.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {

                    SoftBody sb = this.FBodies[i];
                    this.FOutNodes[i].SliceCount = sb.Nodes.Count;
                    //sb.Nodes[0].

                    for (int j = 0; j < sb.Nodes.Count; j++)
                    {
                        this.FOutNodes[i][j] = sb.Nodes[j].X.ToVVVVector();
                    }

                    this.FOutMass[i] = sb.TotalMass;

                    SoftBodyCustomData custom = (SoftBodyCustomData)sb.UserObject;
                    this.FOutCustom[i] = custom.Custom;
                    this.FOutId[i] = custom.Id;
                }
            }
            else
            {
                this.FOutNodes.SliceCount = 0;
                this.FOutCustom.SliceCount = 0;
                this.FOutId.SliceCount = 0;
                this.FOutMass.SliceCount = 0;
            }
		}
	}
}

