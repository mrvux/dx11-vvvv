using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using AssimpNet;
using SlimDX;

namespace VVVV.DX11.Nodes.AssetImport
{
    [PluginInfo(Name="ParentNode",Category="Assimp", Version="",Author="vux,flateric")]
    public class AssimpParentWorldNode : IPluginEvaluate
    {
        [Input("Scene", IsSingle = true)]
        protected IDiffSpread<AssimpScene> FInScene;

        [Input("Root Node")]
        protected IDiffSpread<string> FInRootNode;

        [Output("Parent Local Transform")]
        protected ISpread<Matrix> FOutLocalTransform;

        [Output("Parent World Transform")]
        protected ISpread<Matrix> FOutWorldTransform;



        public void Evaluate(int SpreadMax)
        {
            if (this.FInScene.IsChanged || this.FInRootNode.IsChanged)
            {
                if (this.FInScene[0] != null)
                {
                    List<Tuple<AssimpNode, AssimpNode>> allnodes = new List<Tuple<AssimpNode, AssimpNode>>();
                    //string[] str = this.FInRootNode[0].Split("/".ToCharArray());

                    this.RecurseNodes(allnodes, this.FInScene[0].RootNode, this.FInScene[0].RootNode);

                    List<AssimpNode> filterednodes = new List<AssimpNode>();

                    for (int i = 0; i < this.FInRootNode.SliceCount; i++)
                    {
                        if (this.FInRootNode[i] != "")
                        {
                            Tuple<AssimpNode, AssimpNode> found = null;
                            foreach (Tuple<AssimpNode, AssimpNode> node in allnodes) { if (node.Item2.Name == this.FInRootNode[i]) { found = node; } }

                            if (found != null)
                            {
                                filterednodes.Add(found.Item1);
                            }
                        }
                        else
                        {
                            filterednodes.Add(this.FInScene[0].RootNode);
                        }
                    }

                    this.FOutWorldTransform.SliceCount = filterednodes.Count;
                    this.FOutLocalTransform.SliceCount = filterednodes.Count;

                    for (int i = 0; i < filterednodes.Count; i++)
                    {
                        this.FOutWorldTransform[i] = filterednodes[i].RelativeTransform;
                        this.FOutLocalTransform[i] = filterednodes[i].LocalTransform;
                    }
                }
                else
                {
                    this.FOutLocalTransform.SliceCount = 0;
                    this.FOutWorldTransform.SliceCount = 0;
                }

            }
        }

        private void RecurseNodes(List<Tuple<AssimpNode, AssimpNode>> nodes,AssimpNode parent, AssimpNode current)
        {
            nodes.Add(new Tuple<AssimpNode, AssimpNode>(parent, current));
            foreach (AssimpNode child in current.Children)
            {
                RecurseNodes(nodes,current, child);
            }
        }
    }
}
