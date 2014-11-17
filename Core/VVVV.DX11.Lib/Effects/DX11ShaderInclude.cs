using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.D3DCompiler;
using System.IO;
using System.Reflection;
using FeralTic.DX11;

namespace VVVV.DX11.Lib.Effects
{
    public static class Ext
    {
        public static Stream ToStream(this string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }

    public class DX11ShaderInclude : Include
    {
        private FolderIncludeHandler sysHandler;

        public DX11ShaderInclude()
        {
            this.sysHandler = new FolderIncludeHandler();
        }

        //lets the factory set the file path
        public string ParentPath
        {
            get;
            set;
        }

        public void Close(Stream stream)
        {
            if (stream != null)
            {
                stream.Close();
            }
        }

        public void Open(IncludeType type, string fileName, Stream parentStream, out Stream stream)
        {
            if (type == IncludeType.Local)
            {
                stream = new FileStream(Path.Combine(ParentPath, fileName), FileMode.Open, FileAccess.Read);
            }
            else
            {
                this.sysHandler.Open(type, fileName, parentStream, out stream);
            }
        }
    }
}
