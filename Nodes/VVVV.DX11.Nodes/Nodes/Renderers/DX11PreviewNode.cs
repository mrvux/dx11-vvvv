﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using SlimDX;
using SlimDX.DXGI;
using VVVV.DX11.Lib.Devices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;

namespace VVVV.DX11.Nodes.Renderers
{
     [PluginInfo(Name = "Preview", Category = "DX11.Texture", Author = "vux", AutoEvaluate = true,
        InitialWindowHeight = 300, InitialWindowWidth = 400, InitialBoxWidth = 400, InitialBoxHeight = 300, InitialComponentMode = TComponentMode.InAWindow)]
    public class DX11PreviewNode :IDX11RendererProvider, 
         IDisposable,
         IPluginEvaluate, 
         IDX11RenderWindow, 
         IWin32Window, 
         ICustomQueryInterface, 
         IUserInputWindow
         //IBackgroundColor
    {
         private Control ctrl;

         [Input("Texture In")]
         protected Pin<DX11Resource<DX11Texture2D>> FIn;

         [Input("Index")]
         protected ISpread<int> FIndex;
         
         [Input("Show Alpha", IsSingle = true)]
         protected ISpread<bool> FAlpha;
         
         [Input("Background Color", DefaultColor=new double[] { 0.5, 0.5, 0.5, 1 }, IsSingle = true)]
         protected ISpread<RGBAColor> FInBgColor;

         [Input("Enabled",DefaultValue=1)]
         protected ISpread<bool> FEnabled;

         [Output("Control", Order = 201, IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
         protected ISpread<Control> FOutCtrl;

         DX11Resource<DX11SwapChain> swapchain = new DX11Resource<DX11SwapChain>();

         private bool resized;

         public RGBAColor BackgroundColor
         {
             get { return new RGBAColor(0, 0, 0, 1); }
         }

         public IntPtr InputWindowHandle
         {
             get { return this.Handle; }
         }

         private IntPtr lasthandle = IntPtr.Zero;

         [ImportingConstructor()]
         public DX11PreviewNode(IPluginHost host, IIOFactory iofactory)
         {
             this.ctrl = new Control();
             this.ctrl.Dock = DockStyle.Fill;
             this.ctrl.Resize += new EventHandler(ctrl_Resize);
             this.ctrl.BackColor = System.Drawing.Color.Black;

             this.ctrl.PreviewKeyDown += new PreviewKeyDownEventHandler(ctrl_PreviewKeyDown);
             this.ctrl.KeyDown += new KeyEventHandler(ctrl_KeyDown);
         }

         void ctrl_KeyDown(object sender, KeyEventArgs e)
         {

         }

         void ctrl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
         {
 
         }

         public bool IsEnabled
         {
             get { return this.FEnabled[0]; }
         }

         public void Render(DX11RenderContext context)
         {
             if (this.lasthandle != this.Handle)
             {
                 if (this.swapchain != null)
                 {
                     if (this.swapchain.Contains(context)) { this.swapchain.Dispose(context); }
                 }
                 this.lasthandle = this.Handle;
             }

             if (!this.swapchain.Contains(context))
             {
                 this.swapchain[context] = new DX11SwapChain(context, this.Handle, SlimDX.DXGI.Format.R8G8B8A8_UNorm, new SampleDescription(1, 0));
             }

             if (this.resized)
             {
                 this.swapchain[context].Resize();
             }

             if (this.FEnabled[0])
             {
                 context.CurrentDeviceContext.ClearRenderTargetView(this.swapchain[context].RTV,new SlimDX.Color4(0,0,0,0));
             }

             if (this.FIn.PluginIO.IsConnected && this.FEnabled[0])
             {
                 int id = this.FIndex[0];
                 if (this.FIn[id].Contains(context))
                 {
                     context.RenderTargetStack.Push(this.swapchain[context]);
                     var rs = new DX11RenderState();
                     
                     if (FAlpha[0])
                     {
                     	rs.Blend = DX11BlendStates.Instance.GetState("Blend");
                     	context.CurrentDeviceContext.ClearRenderTargetView(this.swapchain[context].RTV, FInBgColor[0].Color);
                     }
                     context.RenderStateStack.Push(rs);
                     context.CleanShaderStages();
                                                   
                     context.Primitives.FullTriVS.GetVariableBySemantic("TEXTURE").AsResource().SetResource(this.FIn[id][context].SRV);
                     context.Primitives.FullScreenTriangle.Bind(null);
                     context.Primitives.ApplyFullTri();
                     context.Primitives.FullScreenTriangle.Draw();

                     context.RenderStateStack.Pop();
                     context.RenderTargetStack.Pop();
                     context.CleanUpPS();
                 }
             }  
         }

         public void Update(IPluginIO pin, DX11RenderContext context)
         {
             if (this.lasthandle != this.Handle)
             {
                 if (this.swapchain != null) 
                 {
                     if (this.swapchain.Contains(context)) { this.swapchain.Dispose(context); }
                 }
                 this.lasthandle = this.Handle;
             }

             if (!this.swapchain.Contains(context))
             {
                 this.swapchain[context] = new DX11SwapChain(context, this.Handle, SlimDX.DXGI.Format.R8G8B8A8_UNorm, new SampleDescription(1,0));
             }

             if (this.resized)
             {
                 this.swapchain[context].Resize();
             }
         }

         public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
         {
             this.swapchain.Dispose(context);
         }

         void ctrl_Resize(object sender, EventArgs e)
         {
             this.resized = true;
         }

         #region Random Stuff
         public IntPtr Handle
         {
             get { return ctrl.Handle; }
         }

         public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
         {
             if (iid.Equals(Guid.Parse("00000112-0000-0000-c000-000000000046")))
             {
                 ppv = Marshal.GetComInterfaceForObject(ctrl, typeof(IOleObject));
                 return CustomQueryInterfaceResult.Handled;
             }
             else if (iid.Equals(Guid.Parse("458AB8A2-A1EA-4d7b-8EBE-DEE5D3D9442C")))
             {
                 ppv = Marshal.GetComInterfaceForObject(ctrl, typeof(IWin32Window));
                 return CustomQueryInterfaceResult.Handled;
             }
             else
             {
                 ppv = IntPtr.Zero;
                 return CustomQueryInterfaceResult.NotHandled;
             }
         }
         #endregion

         #region Window Stuff
         public DX11RenderContext RenderContext
         {
             get;
             set;
         }

         public IntPtr WindowHandle
         {
             get { return ctrl.Handle; }
         }

         public bool IsVisible
         {
             get { return ctrl.Visible; }
         }

         public void Present()
         {
             this.resized = false;
             if (ctrl.Visible)
             {
                 try
                 {
                     this.swapchain[this.RenderContext].Present(0, PresentFlags.None);
                 }
                 catch
                 {
                 }
             }
         }
         #endregion

         public void Dispose()
         {
             this.swapchain.Dispose();
         }

         public void Evaluate(int SpreadMax)
         {
             this.FOutCtrl[0] = this.ctrl;
         }
    }
}
