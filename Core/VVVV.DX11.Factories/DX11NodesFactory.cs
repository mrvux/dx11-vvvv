﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Factories;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Pins;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins.Output;
using VVVV.Core.Logging;

using VVVV.DX11.Lib;
using VVVV.DX11.Lib.RenderGraph.Listeners;
using VVVV.DX11.Lib.RenderGraph;
using VVVV.DX11.Lib.Devices;
using VVVV.Hosting.IO;
using VVVV.Nodes;
using System.Diagnostics;
using FeralTic.DX11;
using VVVV.PluginInterfaces.V2.Graph;
using System.Windows.Forms;

using DWriteFactory = SlimDX.DirectWrite.Factory;

namespace VVVV.DX11.Factories
{
	[Export(typeof(IAddonFactory))]
    [Export(typeof(DX11NodesFactory))]
    [ComVisible(false)]
    public class DX11NodesFactory : IAddonFactory, IDisposable
	{
        private IHDEHost hdehost;

        private IORegistry ioreg;

        private DX11DisplayManager displaymanager;
        private IDX11RenderContextManager devicemanager;
        private DX11RenderManager rendermanager;

        private DX11GraphBuilder<IDX11ResourceProvider> graphbuilder;
        private ILogger logger;

        [Export]
        public DWriteFactory DirectWriteFactory { get; private set; }

        [ImportingConstructor()]
        public DX11NodesFactory(IHDEHost hdehost, DotNetPluginFactory dnfactory, INodeInfoFactory ni, IORegistry ioreg, ILogger logger)
		{
            DX11EnumFormatHelper.CreateNullDeviceFormat();

            this.hdehost = hdehost;
            this.ioreg = ioreg;
            this.logger = logger;
            this.hdehost.RootNode.Removed += new Core.CollectionDelegate<INode2>(RootNode_Removed);

            DX11ResourceRegistry reg = new DX11ResourceRegistry();

            this.ioreg.Register(reg, true);

            this.hdehost.MainLoop.OnPresent += GraphEventService_OnPresent;
            this.hdehost.MainLoop.OnPrepareGraph += GraphEventService_OnPrepareGraph;
            this.hdehost.MainLoop.OnRender += GraphEventService_OnRender;

            this.displaymanager = new DX11DisplayManager();

            this.DirectWriteFactory = new DWriteFactory(SlimDX.DirectWrite.FactoryType.Shared);

            string[] args = Environment.GetCommandLineArgs();

            foreach (string s in args)
            {
                string sl = s.ToLower();
                if (sl.StartsWith("/dx11mode:"))
                {
                    sl = sl.Replace("/dx11mode:", "");

                    if (sl == "permonitor")
                    {
                        this.devicemanager = new DX11PerMonitorDeviceManager(this.logger, this.displaymanager);
                    }
                    else if (sl == "peradapter")
                    {
                        this.devicemanager = new DX11PerAdapterDeviceManager(this.logger, this.displaymanager);
                    }
                    else if (sl.StartsWith("force"))
                    {
                        sl = sl.Replace("force", "");
                        try
                        {
                            int i = int.Parse(sl);
                            if (i >= 0 && i < this.displaymanager.AdapterCount)
                            {
                                this.devicemanager = new DX11AutoAdapterDeviceManager(this.logger, this.displaymanager, i);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

            if (this.devicemanager == null)
            {
                this.devicemanager = new DX11AutoAdapterDeviceManager(this.logger, this.displaymanager);
            }

           this.graphbuilder = new DX11GraphBuilder<IDX11ResourceProvider>(hdehost, reg);
           this.graphbuilder.RenderRequest += graphbuilder_OnRenderRequest;
           this.rendermanager = new DX11RenderManager(this.devicemanager, this.graphbuilder,this.logger);

            DX11GlobalDevice.DeviceManager = this.devicemanager;
            DX11GlobalDevice.RenderManager = this.rendermanager;

            this.BuildAAEnum();
		}

        void RootNode_Removed(Core.IViewableCollection<INode2> collection, INode2 item)
        {
            this.devicemanager.Dispose();
        }



        private void BuildAAEnum()
        {
            string[] aa = new string[] { "1", "2", "4", "8", "16", "32" };
            this.hdehost.UpdateEnum("DX11_AASamples", "1", aa);
        }

        void graphbuilder_OnRenderRequest(IDX11ResourceDataRetriever sender, IPluginHost host)
        {
            this.rendermanager.Render(sender, host);
        }

        void GraphEventService_OnRender(object sender, EventArgs e)
        {

            this.graphbuilder.Flush();
            DX11GlobalDevice.PendingLinksCount = this.graphbuilder.PendingLinkCount;
            DX11GlobalDevice.PendingPinsCount = this.graphbuilder.PendingPinCount;

            DX11GlobalDevice.Begin();
            this.rendermanager.Render();
            DX11GlobalDevice.End();
        }

        void GraphEventService_OnPrepareGraph(object sender, EventArgs e)
        {
            this.rendermanager.Reset();
        }

        void GraphEventService_OnPresent(object sender, EventArgs e)
        {
            this.rendermanager.Present();
            this.devicemanager.EndFrame();
        }

        #region Factory Stuff
        public void AddDir(string dir, bool recursive)
		{
		}

		public bool Clone(INodeInfo nodeInfo, string path, string name, string category, string version, out INodeInfo newNodeInfo)
		{
			newNodeInfo = null;
			return false;
		}

		public bool Create(INodeInfo nodeInfo, INode host)
		{
            return false;
		}

		public bool Delete(INodeInfo nodeInfo, INode host)
		{
            return false;
		}

        public INodeInfo[] ExtractNodeInfos(string filename, string arguments)
        {
            return new INodeInfo[0];
        }

		public string JobStdSubPath
		{
			get { return "bin"; }
		}

		public void RemoveDir(string dir)
		{
		}
        #endregion

        #region IAddonFactory Members


        public bool AllowCaching
        {
            get { return false;  }
        }


        public string Name
        {
            get { return "DX11"; }
        }

        #endregion

        public bool GetNodeListAttribute(INodeInfo nodeInfo, out string name, out string value)
        {
            name = "";
            value = "";
            return false;
        }

        public void ParseNodeEntry(System.Xml.XmlReader xmlReader, INodeInfo nodeInfo)
        {
            
        }

        public void Dispose()
        {
            Console.WriteLine("Dispose");
        }
    }
}

