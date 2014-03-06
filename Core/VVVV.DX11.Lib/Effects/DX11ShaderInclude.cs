using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.D3DCompiler;
using System.IO;
using System.Reflection;

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
        private static Dictionary<string, string> systemincludes = new Dictionary<string, string>();

        public static void AddSystemInclude(string name, string content)
        {
            systemincludes[name] = content;
        }

        public static void AddSystemInclude(string name, Assembly assembly, string path)
        {
            var textStreamReader = new StreamReader(assembly.GetManifestResourceStream(path));
            string code = textStreamReader.ReadToEnd();
            textStreamReader.Dispose();
            AddSystemInclude(name, code);
        }


        //lets the factory set the file path
        public string ParentPath
        {
            get;
            set;
        }

        //lets the factory get the include type for error handling
        public IncludeType LastIncludeType
        {
            get;
            protected set;
        }

        public void Close(Stream stream)
        {
            stream.Close();
        }

        public void Open(IncludeType type, string fileName, Stream parentStream, out Stream stream)
        {
            if (type == IncludeType.Local)
            {
                stream = new FileStream(Path.Combine(ParentPath, fileName), FileMode.Open, FileAccess.Read);
            }
            else
            {
                if (systemincludes.ContainsKey(fileName))
                {
                    stream = systemincludes[fileName].ToStream();
                }


                stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            }

            //set include type, so the factory can know
            LastIncludeType = type;
        }
    }
}
