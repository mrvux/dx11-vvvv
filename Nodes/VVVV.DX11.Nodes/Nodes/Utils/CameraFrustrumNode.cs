using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX;
using FeralTic.Core.Maths;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Frustrum", Category = "Transform", Version = "", Author = "vux")]
    public class CameraFrustrumNode : IPluginEvaluate
    {
        [Input("View")]
        protected ISpread<Matrix> FInView;

        [Input("Projection")]
        protected ISpread<Matrix> FInProjection;

        [Output("Frustrum")]
        protected ISpread<Frustrum> FOutFrustrum;

        [Output("Plane Equations")]
        protected ISpread<Vector4> FOutPlanes;

        public void Evaluate(int SpreadMax)
        {
            this.FOutFrustrum.SliceCount = SpreadMax;
            this.FOutPlanes.SliceCount = SpreadMax*6;
            int cnt = 0;
            for (int i = 0; i < SpreadMax; i++)
            {
                Frustrum f = new Frustrum(this.FInView[i], this.FInProjection[i]);
                this.FOutFrustrum[i] = f;

                for (int j = 0; j < 6; j++)
                {
                    this.FOutPlanes[cnt] = new Vector4(f.planes[j].Normal.X, f.planes[j].Normal.Y, f.planes[j].Normal.Z, f.planes[j].D);
                    cnt++;
                }
            }
        }
    }
}
