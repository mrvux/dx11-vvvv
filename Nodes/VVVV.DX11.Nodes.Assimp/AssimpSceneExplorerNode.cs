using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V2;
using AssimpNet;
using SlimDX;

namespace VVVV.DX11.Nodes.AssetImport
{
    public class AssimpTreeNode : TreeNode
    {
        private readonly AssimpNode node;

        public AssimpNode AssimpNode
        {
            get { return this.node; }
        }

        public override string ToString()
        {
            return this.node.Name + " (" + this.node.MeshCount + ")";
        }

        public AssimpTreeNode(AssimpNode node)
        {
            this.node = node;
            this.Text = this.ToString();
        }
    }

    [PluginInfo(Name = "SceneExplorer", Category = "Assimp", Version = "", Author = "vux,flateric", AutoEvaluate=true)]
    public partial class AssimpSceneExplorerNode : UserControl, IPluginEvaluate
    {
        [Input("Scene", IsSingle = true)]
        protected IDiffSpread<AssimpScene> FInScene;

        [Output("Mesh Id")]
        protected ISpread<int> FOutMeshId;

        [Output("World Transform")]
        protected ISpread<Matrix> FOutWorldTransform;

        private TreeView tv;
        private AssimpScene scene;

        public AssimpSceneExplorerNode()
        {
            InitializeComponent();
            this.tv = new TreeView();
            this.tv.Dock = DockStyle.Fill;
            this.Controls.Add(this.tv);
            this.tv.AfterSelect += tv_AfterSelect;
            this.tv.ShowNodeToolTips = true;
        }

        private AssimpNode selectednode;
        private bool invalidate = false;

        protected AssimpNode SelectedNode
        {
            get { return this.selectednode; }
            set
            {
                if (this.selectednode != value)
                {
                    this.selectednode = value;
                    this.invalidate = true;
                }
            }
        }

        void tv_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (this.scene == null)
            {
                return;
            }

            if (tv.SelectedNode is AssimpTreeNode)
            {
                this.SelectedNode = ((AssimpTreeNode)tv.SelectedNode).AssimpNode;
            }
            else
            {
                this.SelectedNode = this.scene.RootNode;
            }
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInScene.SliceCount == 0)
            {
                this.tv.Nodes.Clear();
                this.scene = null;
                this.FOutMeshId.SliceCount = 0;
                this.FOutWorldTransform.SliceCount = 0;
                return;
            }

            if (this.FInScene.IsChanged)
            {              
                this.scene = this.FInScene[0];

                if (this.scene != null)
                {
                    this.RebuildTreeview(this.FInScene[0]);
                    this.invalidate = true;
                    this.selectednode = this.scene.RootNode;
                }


            }
            
            if (this.invalidate && this.scene != null)
            {
                List<Matrix> transforms = new List<Matrix>();
                List<int> meshes = new List<int>();

                this.TraverseNode(this.SelectedNode, transforms, meshes);

                this.FOutWorldTransform.SliceCount = transforms.Count;
                this.FOutMeshId.SliceCount = meshes.Count;
                for (int i = 0; i < meshes.Count;i++)
                {
                    this.FOutWorldTransform[i] = transforms[i];
                    this.FOutMeshId[i] = meshes[i];
                }

                    this.invalidate = false;
            }
        }

        private void TraverseNode(AssimpNode node, List<Matrix> transforms, List<int> meshes)
        {
             for (int i = 0;i < node.MeshCount;i++)
             {
                 var am = this.scene.Meshes[node.MeshIndices[i]];

                 //Ignore invalid meshes
                 if (am.Indices.Count > 0 || am.VerticesCount > 0)
                 {
                     transforms.Add(node.RelativeTransform);
                     meshes.Add(node.MeshIndices[i]);
                 }
             }

            for (int i = 0; i < node.Children.Count; i++)
            {
               
                this.TraverseNode(node.Children[i], transforms, meshes);
            }
        }


        private void RebuildTreeview(AssimpScene scene)
        {
            this.tv.Nodes.Clear();

            TreeNode meshes = new TreeNode("Meshes");
            this.tv.Nodes.Add(meshes);

            for (int i = 0 ;i < scene.MeshCount; i++)
            {
                var mesh = scene.Meshes[i];
                TreeNode node = new TreeNode("Mesh " + i.ToString() + " (" + mesh.Indices.Count + ")");
                if (mesh.Indices.Count == 0)
                {
                    node.BackColor = Color.Red;
                } 
                else if (mesh.UvChannelCount == 0 || !mesh.HasNormals)
                {
                    node.BackColor = Color.Yellow;
                    StringBuilder tooltip = new StringBuilder();
                    if (mesh.UvChannelCount == 0)
                    {
                        tooltip.AppendLine("No UV channel");
                    }
                    if(!mesh.HasNormals)
                    {
                        tooltip.AppendLine("No Normals detected");
                    }
                    node.ToolTipText = tooltip.ToString();
                }
                meshes.Nodes.Add(node);
            }

            var root = new AssimpTreeNode(scene.RootNode);
            this.tv.Nodes.Add(root);
            this.AddNode(scene.RootNode, root);
        }

        private void AddNode(AssimpNode an, TreeNode tn)
        {
            for (int i = 0; i < an.Children.Count; i++)
            {           
                var node = new AssimpTreeNode(an.Children[i]);
                this.AddNode(an.Children[i], node);
                tn.Nodes.Add(node);
            }
        }
    }
}
