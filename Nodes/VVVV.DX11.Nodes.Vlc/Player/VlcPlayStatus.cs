using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.DX11.Vlc.Player
{
    public class VlcPlayStatus
    {
        public const int STATUS_INACTIVE = -11;
        public const int STATUS_NEWFILE = -10;
        public const int STATUS_OPENINGFILE = -9;
        public const int STATUS_GETPROPERTIES = -8;
        public const int STATUS_GETPROPERTIESOK = -7;
        public const int STATUS_GETFIRSTFRAME = -6;
        public const int STATUS_WATING = -5;
        public const int STATUS_IMAGE = -1;
        public const int STATUS_READY = 0;
        public const int STATUS_PLAYING = 1;
    }
}
