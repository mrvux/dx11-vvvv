using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.D3DCompiler;
using VVVV.PluginInterfaces.V1;
using System.ComponentModel.Composition;
using VVVV.DX11.Internals.Helpers;
using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Pins.Config;
using VVVV.Utils.VMath;
using VVVV.DX11.Lib.Devices;
using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.ManagedVCL;
using VVVV.DX11.Nodes.Renderers.Graphics.Touch;

namespace VVVV.DX11.Nodes
{
    public partial class DX11RendererNode : UserControl
    {
        private bool cvisible = true;

		[ImportingConstructor()]
        public DX11RendererNode(IPluginHost host, IIOFactory iofactory,IHDEHost hdehost)
        {
            
            InitializeComponent();
			this.FHost = host;
            this.hde = hdehost;
            this.BackColor = System.Drawing.Color.Black;

            //this.hde.BeforeComponentModeChange += new ComponentModeEventHandler(hde_BeforeComponentModeChange);
            
            this.Resize += DX11RendererNode_Resize;
            this.Load += new EventHandler(DX11RendererNode_Load);
            this.Click += new EventHandler(DX11RendererNode_Click);
            this.MouseEnter += new EventHandler(DX11RendererNode_MouseEnter);
            this.MouseLeave += new EventHandler(DX11RendererNode_MouseLeave);
            this.LostFocus += new EventHandler(DX11RendererNode_LostFocus);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(DX11RendererNode_MouseWheel);
            
            Touchdown += OnTouchDownHandler;
            Touchup += OnTouchUpHandler;
            TouchMove += OnTouchMoveHandler;

            this.depthmanager = new DepthBufferManager(host,iofactory);
            
        }

        private bool touchsupport;

        void DX11RendererNode_Load(object sender, EventArgs e)
        {
            if (!TouchConstants.RegisterTouchWindow(this.Handle, 0))
                this.touchsupport = false;
            else
                this.touchsupport = true;
        }

        void hde_BeforeComponentModeChange(object sender, ComponentModeEventArgs args)
        {
            /*if (args.ComponentMode == ComponentMode.Fullscreen)
            {
                this.FOutBackBuffer[0][this.RenderContext].SetFullScreen(true);
            }*/
        }

        void DX11RendererNode_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.wheel += e.Delta / 112;
        }

        void DX11RendererNode_Click(object sender, EventArgs e)
        {
            /*INode2 node = (INode2)this.host2;

            hde.SelectNodes(new INode2[1] { node });

            Console.Write("Test");*/
        }

        void DX11RendererNode_LostFocus(object sender, EventArgs e)
        {
            //Cursor.Show();
        }

        void DX11RendererNode_MouseLeave(object sender, EventArgs e)
        {
            if (!this.cvisible)
            {
                this.cvisible = true;
                Cursor.Show();
            }
        }

        void DX11RendererNode_MouseEnter(object sender, EventArgs e)
        {
            if (this.FInShowCursor.SliceCount > 0)
            {
                if (!this.FInShowCursor[0] && this.cvisible)
                {
                    Cursor.Hide();
                    this.cvisible = false;
                }
            }
        }

        private void DX11RendererNode_Resize(object sender, EventArgs e)
        {
            this.FResized = true;
        }

        private void DX11RendererNode_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                this.FResized = true;    
            }
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            double mx = e.X;
            double my = e.Y;

            mx = VMath.Map(mx, 0, this.Width, -1.0, 1.0, TMapMode.Clamp);
            my = VMath.Map(my, 0, this.Height, 1.0, -1.0, TMapMode.Clamp);
            this.FMousePos.x = mx;
            this.FMousePos.y = my;

            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left) { this.FMouseButtons.x = 1; }
            if (e.Button == System.Windows.Forms.MouseButtons.Middle) { this.FMouseButtons.y = 1; }
            if (e.Button == System.Windows.Forms.MouseButtons.Right) { this.FMouseButtons.z = 1; }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left) { this.FMouseButtons.x = 0; }
            if (e.Button == System.Windows.Forms.MouseButtons.Middle) { this.FMouseButtons.y = 0; }
            if (e.Button == System.Windows.Forms.MouseButtons.Right) { this.FMouseButtons.z = 0; }
            base.OnMouseUp(e);
        }

        protected override bool ProcessKeyEventArgs(ref Message m)
        {
            return base.ProcessKeyEventArgs(ref m);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            //e.KeyData == Keys.
            base.OnKeyDown(e);

            if (!this.FKeys.Contains(e.KeyCode))
            {
                this.FKeys.Add(e.KeyCode);
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (this.FKeys.Contains(e.KeyCode))
            {
                this.FKeys.Remove(e.KeyCode);
            }
            base.OnKeyUp(e);

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            e.Handled = true;
            //Se.SuppressKeyPress = true;
            base.OnKeyPress(e);
        }
    }
}
