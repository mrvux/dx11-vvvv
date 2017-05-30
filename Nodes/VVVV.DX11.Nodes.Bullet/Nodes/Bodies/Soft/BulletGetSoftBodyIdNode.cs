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
	[PluginInfo(Name="GetSoftBodyId",Category="Bullet",
		Help = "Gets system id of a soft body,", Author = "vux")]
	public class BulletGetSoftBodyIdNode : IPluginEvaluate
	{
		[Input("Bodies")]
        protected Pin<SoftBody> FBodies;

		[Output("Id")]
        protected ISpread<int> FOutId;


        public void  Evaluate(int SpreadMax)
		{
            if (this.FBodies.IsConnected)
            {
                this.FOutId.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    SoftBody sb = this.FBodies[i];
                    SoftBodyCustomData custom = (SoftBodyCustomData)sb.UserObject;
                    this.FOutId[i] = custom.Id;
                }
            }
            else
            {
                this.FOutId.SliceCount = 0;
            }
		}
	}
}

