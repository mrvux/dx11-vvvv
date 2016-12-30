using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;
using DWriteFactory = SlimDX.DirectWrite.Factory;

namespace VVVV.DX11.Lib
{
    public static class DirectWriteFontUtils
    {
        public static void SetFontEnum(IHDEHost host, DWriteFactory dwfactory)
        {
            Factory f = new Factory(dwfactory.ComPointer);

            var fontCollection = f.GetSystemFontCollection(false);

            List<string> familyNames = new List<string>();
            for (int i = 0; i < fontCollection.FontFamilyCount; i++)
            {
                var family = fontCollection.GetFontFamily(i);
                if (family.FamilyNames.Count > 0)
                {
                    familyNames.Add(family.FamilyNames.GetString(0));
                }
            }
            familyNames.Sort();
            host.UpdateEnum("DirectWrite_Font_Families", "Arial", familyNames.ToArray());
        }
    }
}
