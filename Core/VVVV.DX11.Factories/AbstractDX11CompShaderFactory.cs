using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.Hosting.Factories;

using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.IO;
using System.ComponentModel.Composition.Hosting;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Interfaces;
using VVVV.DX11.Nodes.Layers;
using System.Reflection;
using VVVV.DX11.Internals.Effects;
using System.IO;
using VVVV.DX11.Lib.RenderGraph.Listeners;
using FeralTic.DX11;
using VVVV.DX11.Lib.Effects;

namespace VVVV.DX11.Factories
{
    public abstract class AbstractDX11CompShaderFactory<T> : AbstractFileFactory<IInternalPluginHost> where T : IDX11ShaderNodeWrapper
    {
        protected abstract string NodeCategory { get; }
        protected abstract string NodeVersion { get; }

                //[Import]
        protected IHDEHost FHdeHost;

        [Import]
        protected DotNetPluginFactory FDotNetFactory;

        [Import]
        protected IORegistry FIORegistry;

        /*[Import]
        protected INodeInfoFactory FNodeInfoFactory;*/

        public event PluginCreatedDelegate PluginCreated;
        public event PluginDeletedDelegate PluginDeleted;

        private CompositionContainer FParentContainer;

        private readonly Dictionary<IPluginBase, PluginContainer> FPluginContainers;

        protected string ext;

        #region Constructor
        public AbstractDX11CompShaderFactory(CompositionContainer parentContainer, IHDEHost hdeHost, string exts) : base(exts)
        {
            this.ext = exts;
            this.FHdeHost = hdeHost;
            this.FParentContainer = parentContainer;
            FPluginContainers = new Dictionary<IPluginBase, PluginContainer>();
        }



        #endregion


        #region IAddonFactory

        public override string JobStdSubPath
        {
            get
            {
                return "dx11";
            }
        }

        protected override void AddSubDir(string dir, bool recursive)
        {
            // Ignore obj directories used by C# IDEs
            if (dir.EndsWith(@"\obj\x86") || dir.EndsWith(@"\obj\x64")) return;

            base.AddSubDir(dir, recursive);
        }

        protected override bool CreateNode(INodeInfo nodeInfo, IInternalPluginHost pluginHost)
        {
            if (nodeInfo.Type != NodeType.Plugin)
                return false;

            //get the code of the FXProject associated with the nodeinfos filename
            //effectHost.SetEffect(nodeInfo.Filename, project.Code);

            //compile shader

            var shader = DX11Effect.FromByteCode(nodeInfo.Filename);
            
            //create or update plugin
            if (pluginHost.Plugin == null)
            {
                //IPluginBase plug = this.FDotNetFactory.CreatePluginCustom(nodeInfo, pluginHost as IPluginHost2, typeof(DX11ShaderNode));
                nodeInfo.AutoEvaluate = false;
                nodeInfo.Arguments = typeof(T).ToString();

                var pluginContainer = new PluginContainer(pluginHost, FIORegistry, FParentContainer, FNodeInfoFactory, FDotNetFactory, typeof(T), nodeInfo);
                pluginHost.Plugin = pluginContainer;

                FPluginContainers[pluginContainer.PluginBase] = pluginContainer;

                IDX11ShaderNodeWrapper shaderNode = pluginContainer.PluginBase as IDX11ShaderNodeWrapper;
                shaderNode.SetShader(shader, true, nodeInfo.Filename);

                if (this.PluginCreated != null)
                {
                    this.PluginCreated(pluginContainer, pluginHost);
                }
            }
            else
            {
                PluginContainer container = pluginHost.Plugin as PluginContainer;
                var shaderNode = container.PluginBase as IDX11ShaderNodeWrapper;
                shaderNode.SetShader(shader, false, nodeInfo.Filename);
            }


            return true;
        }

        protected override bool DeleteNode(INodeInfo nodeInfo, IInternalPluginHost pluginHost)
        {
            var plugin = pluginHost.Plugin;

            var disposablePlugin = plugin as IDisposable;
            if (FPluginContainers.ContainsKey(plugin))
            {
                FPluginContainers[plugin].Dispose();
                FPluginContainers.Remove(plugin);
            }

            if (this.PluginDeleted != null)
            {
                this.PluginDeleted(pluginHost.Plugin);
            }
            return true;
        }

        /// <summary>
        /// Called by AbstractFileFactory to extract all node infos in given file.
        /// </summary>
        protected override IEnumerable<INodeInfo> LoadNodeInfos(string filename)
        {
            //Try this random hack
            var nodeInfos = new List<INodeInfo>();
            LoadNodeInfosFromFile(filename, filename, ref nodeInfos, true);
            return nodeInfos;
        }

        protected void LoadNodeInfosFromFile(string filename, string sourcefilename, ref List<INodeInfo> nodeInfos, bool commitUpdates)
        {
            if (filename.EndsWith(this.ext))
            {
                var nodeInfo = FNodeInfoFactory.CreateNodeInfo(Path.GetFileNameWithoutExtension(filename), this.NodeCategory,this.NodeVersion, filename, true);
                nodeInfo.Arguments = Assembly.GetExecutingAssembly().Location.ToLower();
                nodeInfo.Factory = this;
                nodeInfo.Type = NodeType.Plugin;
                nodeInfo.UserData = this;
                nodeInfo.AutoEvaluate = false;
                nodeInfos.Add(nodeInfo);
                nodeInfo.Factory = this;
                nodeInfo.CommitUpdate();
            }
        }
        #endregion


        protected virtual string GetAssemblyLocation(INodeInfo nodeInfo)
        {
            return nodeInfo.Filename;
        }

    }
}
