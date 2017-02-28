using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VVVV.Core.DirectWrite;

using VVVV.Utils.Streams;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Generic;


namespace VVVV.DX11.Nodes.Nodes.Utils
{

        // ------------------- Zip -------------------
        static class ZipInfo
        {
            public const string HELP = "Interleaves all Input spreads.";
            public const string TAGS = "interleave, join, generic, spreadop";
        }

        [PluginInfo(Name = "Zip", Category = "TextStyle", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
        public class ValueZipNode : Zip<ITextStyler>
        {

        }

        [PluginInfo(Name = "Zip", Category = "TextStyle", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
        public class ValueBinSizeZipNode : Zip<IInStream<ITextStyler>>
        {

        }

        // ------------------ UnZip ------------------

        static class UnzipInfo
        {
            public const string HELP = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it.";
            public const string HELPBIN = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it. With Bin Size.";
            public const string TAGS = "split, generic, spreadop";
        }

        [PluginInfo(Name = "Unzip", Category = "TextStyle", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
        public class ValueUnzipNode : Unzip<ITextStyler>
        {

        }

        [PluginInfo(Name = "Unzip", Category = "TextStyle", Version = "Bin", Help = UnzipInfo.HELPBIN, Tags = UnzipInfo.TAGS)]
        public class ValueBinSizeUnzipNode : Unzip<IInStream<ITextStyler>>
        {

        }


}
