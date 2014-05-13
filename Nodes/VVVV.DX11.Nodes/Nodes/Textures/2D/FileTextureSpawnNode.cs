using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using VVVV.DX11.Lib;
using System.IO;
using VVVV.DX11.Lib.Devices;
using SlimDX.Direct3D11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "FileTexture", Category = "DX11", Version = "2d.Spawn", Author = "vux")]
    public class FileTextureSpawnNode : IPluginEvaluate, IDX11ResourceProvider, IDisposable
    { 
        private class DirectoryData
        {
            public DirectoryData(string basedir)
            {
                this.FilePath = Directory.GetFiles("*.png");
                this.FileCount = FilePath.Length;
            }

            public string[] FilePath;
            public int FileCount;
        }

        private class Spawner
        {
            public int Index;
            public int InitialFrame;
            public int CurrentFrame;
            public DirectoryData Data;
            public DX11Texture2D Texture;

            public void SetPosition(DX11RenderContext ctx, int frame)
            {
                int frameindex = frame - this.CurrentFrame;
                frameindex = frameindex < 0 ? 0 : frameindex;
                frameindex = frameindex > Data.FileCount - 1 ? Data.FileCount - 1 : frameindex;

                if (frameindex != CurrentFrame)
                {
                    if (this.Texture == null)
                    {
                        this.Texture.Dispose();
                    }

                    ImageLoadInformation info = ImageLoadInformation.FromDefaults();
                    info.MipLevels = 1;
                    this.Texture = DX11Texture2D.FromFile(ctx, Data.FilePath[frameindex],info);
                    CurrentFrame = frameindex;
                }
            }

            public void Dispose()
            {
                if (this.Texture != null)
                {
                    this.Texture.Dispose();
                }
            }
        }

        [Input("Base Folder", StringType = StringType.Directory)]
        protected IDiffSpread<string> FInPath;

        [Input("Spawn Index")]
        protected ISpread<int> FSpawnIndex;

        [Input("Frame Index")]
        protected IDiffSpread<int> FFrameIndex;

        [Input("Spawn", IsBang = true)]
        protected ISpread<bool> FSpawn;

        [Input("Load In Background", IsSingle = true)]
        protected ISpread<bool> FInBGLoad;

        [Output("Texture Out")]
        protected ISpread<DX11Resource<DX11Texture2D>> outtex;

        [Output("Index")]
        protected ISpread<int> outidx;

        [Output("Position")]
        protected ISpread<double> outpos;

        [Output("Dir Count")]
        protected ISpread<int> outdircount;

        private List<DirectoryData> directories = new List<DirectoryData>();
        private List<Spawner> spawners = new List<Spawner>();
        DX11RenderContext ctx;

        public FileTextureSpawnNode()
        {
            ctx = DX11GlobalDevice.DeviceManager.RenderContexts[0];
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInPath.IsChanged)
            {
                this.directories.Clear();
                string path = this.FInPath[0];

                try
                {
                    var subdirs = Directory.GetDirectories(path).ToList();
                    
                    subdirs.ForEach(sd => this.directories.Add(new DirectoryData(sd)));
                    this.outdircount.SliceCount = subdirs.Count;
                    for (int i = 0; i <this.directories.Count;i++)
                    {
                        this.outdircount[i] = this.directories[i].FileCount;
                    }
                }
                catch
                {
                    this.directories.Clear();
                    this.outdircount.SliceCount = 0;
                }
            }

            if (this.outdircount.SliceCount == 0)
            {
                return;
            }

            if (this.FSpawn[0] && this.FSpawnIndex.SliceCount > 0)
            {
                for (int i = 0; i < this.FSpawnIndex.SliceCount; i++)
                {
                    int idx = this.FSpawnIndex[i];

                    if (this.spawners.Any(s => s.Index == idx))
                    {
                        //Emit animation
                        Spawner sp = new Spawner();
                        sp.Index= idx;
                        sp.InitialFrame = this.FFrameIndex[0];
                        sp.CurrentFrame = 0;
                        sp.Data = this.directories[idx];
                        sp.Texture = DX11Texture2D.FromFile(ctx,sp.Data.FilePath[0]);

                        this.spawners.Add(sp);
                    }
                }
            }

            spawners.Where(s => s.CurrentFrame >= s.Data.FileCount).ToList().ForEach(s => { s.Dispose(); this.spawners.Remove(s); });

            this.outidx.SliceCount = spawners.Count;
            this.outpos.SliceCount = spawners.Count;
            this.outtex.SliceCount = spawners.Count;

            for (int i = 0; i < this.spawners.Count; i++)
            {
                Spawner s = this.spawners[i];
                this.outidx[i] = s.Index;
                this.outpos[i] = (double)s.CurrentFrame / (double)s.Data.FileCount;
                this.outtex[i] = new DX11Resource<DX11Texture2D>();
            }
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            for (int i = 0; i < this.outtex.SliceCount;i++)
            {
                this.outtex[i][context] = this.spawners[i].Texture;
            }
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {

        }

        public void Dispose()
        {
        }
    }
}
