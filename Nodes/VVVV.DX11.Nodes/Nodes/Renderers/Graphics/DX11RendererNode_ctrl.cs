using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V1;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.DX11.Lib.Rendering;
using VVVV.DX11.Nodes.Renderers.Graphics.Touch;

namespace VVVV.DX11.Nodes
{
    public partial class DX11RendererNode : UserControl
    {
		[ImportingConstructor()]
        public DX11RendererNode(IPluginHost host, IIOFactory iofactory,IHDEHost hdehost)
        {
            
            InitializeComponent();

			this.FHost = host;
            this.hde = hdehost;
            this.BackColor = System.Drawing.Color.Black;

            this.cursorDisplay = new Windows.WindowDisplayCursor(this);
            this.Resize += DX11RendererNode_Resize;
            this.Load += new EventHandler(DX11RendererNode_Load);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(DX11RendererNode_MouseWheel);
            this.GotFocus += DX11RendererNode_GotFocus;
            this.LostFocus += DX11RendererNode_LostFocus;

            Touchdown += OnTouchDownHandler;
            Touchup += OnTouchUpHandler;
            TouchMove += OnTouchMoveHandler;

            this.depthmanager = new DepthBufferManager(host,iofactory);
            
        }

        private void DX11RendererNode_LostFocus(object sender, EventArgs e)
        {
            if (this.FInDisableShortCuts.SliceCount > 0 && this.FInDisableShortCuts[0])
            {
                this.hde.EnableShortCuts();
            }
        }

        private void DX11RendererNode_GotFocus(object sender, EventArgs e)
        {
            if (this.FInDisableShortCuts.SliceCount > 0 && this.FInDisableShortCuts[0])
            {
                this.hde.DisableShortCuts();
            }
        }

        private bool touchsupport;

        void DX11RendererNode_Load(object sender, EventArgs e)
        {
            if (!TouchConstants.RegisterTouchWindow(this.Handle, 0))
                this.touchsupport = false;
            else
                this.touchsupport = true;
        }

        void DX11RendererNode_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.wheel += e.Delta / 112;
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
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (this.FKeys.Contains(e.KeyCode))
            {
                this.FKeys.Remove(e.KeyCode);
            }
            base.OnKeyUp(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
        }
    }
}
