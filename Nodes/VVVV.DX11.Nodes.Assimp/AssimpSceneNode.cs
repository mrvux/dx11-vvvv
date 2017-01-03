using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using AssimpNet;
using System.IO;
using VVVV.Core.Logging;
using System.ComponentModel.Composition;

namespace VVVV.DX11.Nodes.AssetImport
{
    [PluginInfo(Name = "SceneFile", Category = "DX11.Geometry", Version = "Assimp", Author = "vux,flateric")]
    public class AssimpSceneNode : IPluginEvaluate, IDisposable
    {
        [Input("Filename",StringType=StringType.Filename,IsSingle=true)]
        protected IDiffSpread<string> FInPath;

        [Input("Reload", IsBang = true, IsSingle = true)]
        protected ISpread<bool> FInReload;

        [Input("Preload Data", IsSingle = true)]
        protected ISpread<bool> FInPreload;

        [Output("Scene", IsSingle = true)]
        protected ISpread<AssimpScene> FOutScene;

        [Output("Mesh")]
        protected ISpread<AssimpMesh> FOutMeshes;
 
        [Output("Mesh Count", IsSingle=true)]
        protected ISpread<int> FOutMeshCount;

        [Output("Is Valid")]
        protected ISpread<bool> FOutValid;

        [Import()]
        protected ILogger FLogger;

        private AssimpScene scene;

        public void Evaluate(int SpreadMax)
        {

            if (this.FInPath.IsChanged || this.FInReload[0])
            {
                if (this.scene != null) { this.scene.Dispose(); }

                string p = this.FInPath[0];
                if (File.Exists(p))
                {
                    try
                    {
                        this.scene = new AssimpScene(p, this.FInPreload[0], false);
                        this.FOutValid[0] = true;
                        this.FOutMeshCount[0] = this.scene.MeshCount;
                        this.FOutMeshes.SliceCount = this.scene.MeshCount;

                        for (int i = 0; i < this.scene.MeshCount; i++)
                        {
                            this.FOutMeshes[i] = this.scene.Meshes[i];
                        }
                    }
                    catch (Exception ex)
                    {
                        this.FLogger.Log(ex);
                        this.FOutValid[0] = false;
                        this.FOutMeshCount[0] = 0;
                        this.FOutMeshes.SliceCount = 0;
                    }
                }
                else
                {
                    this.FOutValid[0] = false;
                    this.FOutMeshCount[0] = 0;
                    this.FOutMeshes.SliceCount = 0;
                }
                this.FOutScene[0] = this.scene;
            }
        }

        public void Dispose()
        {
            if (this.scene != null) { this.scene.Dispose(); }
        }
    }
}
