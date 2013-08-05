﻿using System;
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
    }




    [PluginInfo(Name="FileTexture",Category="DX11",Version="2d",Author="vux,tonfilm")]
    public class FileTexture2dNode : FileTextureBaseNode<DX11Texture2D>
    {

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
                result = DX11Texture2D.FromFile(context, path);
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
    }


}
