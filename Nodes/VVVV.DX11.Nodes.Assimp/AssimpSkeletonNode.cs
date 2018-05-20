using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.SkeletonInterfaces;
using System.ComponentModel.Composition;
using AssimpNet;

namespace VVVV.DX11.Nodes.AssetImport
{
    [PluginInfo(Name = "Skeleton",
                Category = "Skeleton",
                Version = "Assimp",
                Author = "vux",
                Help = "Loads a skeleton from an Assimp Scene",
                Tags = "")]
    public class AssimpSkeletonNode : IPluginEvaluate
    {
        [Input("Scene", IsSingle = true, CheckIfChanged=true)]
        protected Pin<AssimpScene> FInScene;

        [Input("Root", IsSingle = true)]
        protected IDiffSpread<string> FInRoot;

        private INodeOut FSkeletonOutput;
        private Skeleton FSkeleton;

        [ImportingConstructor()]
        public AssimpSkeletonNode(IPluginHost host)
        {
            FSkeleton = new Skeleton();
            
            System.Guid[] guids = new System.Guid[1];
            guids[0] = new Guid("AB312E34-8025-40F2-8241-1958793F3D39");
            
            host.CreateNodeOutput("Skeleton", TSliceMode.Dynamic, TPinVisibility.True, out FSkeletonOutput);
            FSkeletonOutput.SetSubType2(typeof(Skeleton), guids, "Skeleton");
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInScene.IsConnected)
            {
                this.FSkeletonOutput.SliceCount = 1;

                if (this.FInScene.IsChanged || this.FInRoot.IsChanged)
                {
                    
                    FSkeleton.ClearAll();

                    List<AssimpNode> allnodes = new List<AssimpNode>();
                    this.RecurseNodes(allnodes, this.FInScene[0].RootNode);

                    AssimpNode found = null;
                    foreach (AssimpNode node in allnodes) { if (node.Name == this.FInRoot[0]) { found = node; } }

                    if (found != null)
                    {
                        int id = 0;
                        CreateSkeleton(ref FSkeleton, found, "",ref id);
                    }

                    FSkeletonOutput.SetInterface(FSkeleton);
                    FSkeletonOutput.MarkPinAsChanged();
                }
            }
            else
            {
                this.FSkeletonOutput.SliceCount = 0;
            }
        }

        #region helper
        private void CreateSkeleton(ref Skeleton skeleton, AssimpNode node,string parent, ref int id)
        {
            IJoint joint = new AssimpBoneWrapper(node);
            joint.Id = id;
            id++;
            if (skeleton.Root == null)
                skeleton.InsertJoint("", joint);
            else
                skeleton.InsertJoint(parent, joint);

            foreach (AssimpNode child in node.Children)
            {
                CreateSkeleton(ref skeleton,child, node.Name, ref id);
            }
        }
        #endregion

        private void RecurseNodes(List<AssimpNode> nodes, AssimpNode current)
        {
            nodes.Add(current);
            foreach (AssimpNode child in current.Children)
            {
                RecurseNodes(nodes, child);
            }
        }
    }
}
