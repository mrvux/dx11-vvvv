using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using AssimpNet;
using SlimDX;

namespace VVVV.DX11.Nodes.AssetImport
{
    [PluginInfo(Name = "Camera", Category = "Assimp", Version = "Transform", Author = "vux,flateric")]
    public class AssimpCameraTransformNode : IPluginEvaluate
    {
        [Input("Scene", IsSingle = true)]
        protected IDiffSpread<AssimpScene> FInScene;

        [Output("Name")]
        protected ISpread<string> FOutName;

        [Output("View")]
        protected ISpread<Matrix> FOutView;

        [Output("Projection")]
        protected ISpread<Matrix> FOutProj;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInScene.IsChanged)
            {
                if (this.FInScene[0] != null)
                {
                    this.FOutView.SliceCount = this.FInScene[0].Cameras.Count;
                    this.FOutProj.SliceCount = this.FInScene[0].Cameras.Count;
                    this.FOutName.SliceCount = this.FInScene[0].Cameras.Count;

                    for (int i = 0; i < this.FInScene[0].Cameras.Count; i++)
                    {
                        AssimpCamera cam = this.FInScene[0].Cameras[i];

                        Matrix proj = new Matrix();

                        //Mini near plane fix: make sure tiny bit > 0
                        float znear = cam.NearPlane <= 0.0f ? 0.0001f : cam.NearPlane;

                        Matrix.PerspectiveFovLH(cam.HFOV, cam.AspectRatio == 0 ? 1 : cam.AspectRatio, znear, cam.FarPlane,out proj);
                        Matrix view = Matrix.LookAtLH(cam.Position,cam.LookAt,cam.UpVector);

                        this.FOutView[i] = view;
                        this.FOutProj[i] = proj;
                        this.FOutName[i] = cam.Name;
                    }
                }
                else
                {
                    this.FOutName.SliceCount = 0;
                    this.FOutView.SliceCount = 0;
                    this.FOutProj.SliceCount = 0;
                }

            }
        }

    }
}
