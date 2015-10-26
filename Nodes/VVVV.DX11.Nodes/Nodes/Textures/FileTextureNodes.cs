using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System.IO;

namespace VVVV.DX11.Nodes
{
    public class FileTexture1DLoadTask : FileTextureLoadTask<DX11Texture1D>
    {
        public FileTexture1DLoadTask(DX11RenderContext context, int slice, string path)
            : base(context, slice, path)
        {
        }

        protected override void DoProcess()
        {
            this.Resource = DX11Texture1D.FromFile(this.Context, path);
        }
    }

    public class FileTexture2DLoadTask : FileTextureLoadTask<DX11Texture2D>
    {
        public FileTexture2DLoadTask(DX11RenderContext context, int slice, string path)
            : base(context, slice, path)
        {
        }

        protected override void DoProcess()
        {
            this.Resource = DX11Texture2D.FromFile(this.Context, path);
            #if DEBUG
            if (this.Resource.Resource != null)
            {
                this.Resource.Resource.DebugName = "FileTextureAsync";
            }
            #endif
        }
    }

    public class FileTexture3DLoadTask : FileTextureLoadTask<DX11Texture3D>
    {
        public FileTexture3DLoadTask(DX11RenderContext context, int slice, string path)
            : base(context, slice, path)
        {
        }

        protected override void DoProcess()
        {
            this.Resource = DX11Texture3D.FromFile(this.Context, path);
        }


    }


    [PluginInfo(Name = "FileTexture", Category = "DX11", Version = "1d", Author = "vux")]
    public class FileTexture1dNode : FileTextureBaseNode<DX11Texture1D>
    {
        protected override bool Load(DX11RenderContext context, string path,out DX11Texture1D result)
        {
            try
            {
                result = DX11Texture1D.FromFile(context, path);

                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        protected override FileTextureLoadTask<DX11Texture1D> GetTask(DX11RenderContext context, string path, int slice)
        {
            FileTexture1DLoadTask task = new FileTexture1DLoadTask(context, slice, path);
            task.StatusChanged += new TaskStatusChangedDelegate(task_StatusChanged);
            context.ResourceScheduler.AddTask(task);
            return task;
        }

        void task_StatusChanged(IDX11ScheduledTask task)
        {
            FileTexture1DLoadTask ft = (FileTexture1DLoadTask)task;
            if (task.Status == eDX11SheduleTaskStatus.Completed)
            {
                this.FTextureOutput[ft.Slice][ft.Context] = ft.Resource;
                this.FValid[ft.Slice] = true;
            }
        }

        protected override void LoadInfo(int slice, string path)
        {
            
        }

        protected override void MarkInvalid(int slice)
        {

        }
    }




    [PluginInfo(Name="FileTexture",Category="DX11",Version="2d",Author="vux,tonfilm")]
    public class FileTexture2dNode : FileTextureBaseNode<DX11Texture2D>
    {

        [Output("Size", AsInt = true, Order = 400)]
        protected ISpread<Vector2> size;

        [Output("Format", Order = 401)]
        protected ISpread<SlimDX.DXGI.Format> format;

        void task_StatusChanged(IDX11ScheduledTask task)
        {
            FileTexture2DLoadTask ft = (FileTexture2DLoadTask)task;
            if (task.Status == eDX11SheduleTaskStatus.Completed)
            {
                this.FTextureOutput[ft.Slice][ft.Context] = ft.Resource;
                this.FValid[ft.Slice] = true;
            }
        }


        protected override bool Load(DX11RenderContext context, string path, out DX11Texture2D result)
        {
            try
            {
                if (Path.GetExtension(path).ToLower() == ".tga")
                {
                    result = null;
                    return false;
                }
                else
                {
                    ImageLoadInformation info = ImageLoadInformation.FromDefaults();
                    if (this.FInNoMips[0])
                    {
                        info.MipLevels = 1;
                    }
                    result = DX11Texture2D.FromFile(context, path, info);
                }
                
                #if DEBUG
                result.Resource.DebugName = "FileTexture";
                #endif
                
                return true;
            }
            catch
            {
                result = new DX11Texture2D();
                return false;
            }         
        }

        protected override FileTextureLoadTask<DX11Texture2D> GetTask(DX11RenderContext context, string path, int slice)
        {
            FileTexture2DLoadTask task = new FileTexture2DLoadTask(context, slice, path);
            task.StatusChanged += new TaskStatusChangedDelegate(task_StatusChanged);
            context.ResourceScheduler.AddTask(task);
            return task;
        }

        protected override void SliceCountchanged(int slicecount)
        {
            this.size.SliceCount = slicecount;
            this.format.SliceCount = slicecount;
        }

        protected override void LoadInfo(int slice, string path)
        {
            try
            {
                ImageInformation? info = ImageInformation.FromFile(path);
                if (info.HasValue)
                {
                    this.size[slice] = new Vector2(info.Value.Width, info.Value.Height);
                    this.format[slice] = info.Value.Format;
                }
                else
                {
                    MarkInvalid(slice);
                }
                
            }
            catch
            {
                MarkInvalid(slice);
            }
        }

        protected override void MarkInvalid(int slice)
        {
            this.size[slice] = new Vector2(-1, -1);
            this.format[slice] = SlimDX.DXGI.Format.Unknown;
        }
    }

    [PluginInfo(Name = "FileTexture", Category = "DX11", Version = "3d", Author = "vux")]
    public class FileTexture3dNode : FileTextureBaseNode<DX11Texture3D>
    {
        protected override bool Load(DX11RenderContext context, string path, out DX11Texture3D result)
        {
            try
            {
                if (this.FInNoMips[0])
                {
                    ImageLoadInformation loadinfo = new ImageLoadInformation();
                    loadinfo.MipLevels = 1;
                    result = DX11Texture3D.FromFile(context, path,loadinfo);
                }
                else
                {
                    result = DX11Texture3D.FromFile(context, path);
                }
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }


        protected override FileTextureLoadTask<DX11Texture3D> GetTask(DX11RenderContext context, string path, int slice)
        {
            FileTexture3DLoadTask task = new FileTexture3DLoadTask(context, slice, path);
            task.StatusChanged += new TaskStatusChangedDelegate(task_StatusChanged);
            context.ResourceScheduler.AddTask(task);
            return task;
        }

        void task_StatusChanged(IDX11ScheduledTask task)
        {
            FileTexture3DLoadTask ft = (FileTexture3DLoadTask)task;
            if (task.Status ==eDX11SheduleTaskStatus.Completed)
            {
                this.FTextureOutput[ft.Slice][ft.Context] = ft.Resource;
                this.FValid[ft.Slice] = true;
            }
        }

        protected override void LoadInfo(int slice, string path)
        {

        }

        protected override void MarkInvalid(int slice)
        {

        }
    }


}
