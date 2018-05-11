using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace VVVV.DX11.Internals
{
    /// <summary>
    /// Allowed Input Layout Types
    /// </summary>
    public enum eInputLayoutType
    {
        Binormal, BlendIndices, BlendWeight, Color, Normal, Position,
        PointSize, Tangent, TextureCoord,Texture, Velocity
    }

    /// <summary>
    /// Handles Layout creation. TODO : Manual index creation
    /// </summary>
    public class InputLayoutFactory
    {
        private static Dictionary<eInputLayoutType, string> layoutsemantics;

		#region Layout Semantics List
		public static Dictionary<eInputLayoutType, string> LayoutSemantics
        {
            get
            {
                if (layoutsemantics == null)
                {
                    //Build available layout semantics
                    layoutsemantics = new Dictionary<eInputLayoutType, string>();
                    layoutsemantics.Add(eInputLayoutType.Binormal, "BINORMAL");
                    layoutsemantics.Add(eInputLayoutType.BlendIndices, "BLENDINDICES");
                    layoutsemantics.Add(eInputLayoutType.BlendWeight, "BLENDWEIGHT");
                    layoutsemantics.Add(eInputLayoutType.Color, "COLOR");
                    layoutsemantics.Add(eInputLayoutType.Normal, "NORMAL");
                    layoutsemantics.Add(eInputLayoutType.PointSize, "PSIZE");
                    layoutsemantics.Add(eInputLayoutType.Position, "POSITION");
                    layoutsemantics.Add(eInputLayoutType.Tangent, "TANGENT");
                    layoutsemantics.Add(eInputLayoutType.TextureCoord, "TEXCOORD");
                    layoutsemantics.Add(eInputLayoutType.Texture, "TEXTURE");
                    layoutsemantics.Add(eInputLayoutType.Velocity, "VELOCITY");
                }
                return layoutsemantics;
            }
		}
		#endregion

		public static InputElement GetInputElement(eInputLayoutType type, Format format)
        {
            return GetInputElement(type, format,0);
        }

        public static InputElement GetInputElement(eInputLayoutType type, Format format,int index)
        {
            string name = LayoutSemantics[type];
            InputElement elem = new InputElement(name, index, format, 0);
            return elem;
		}

		public static InputElement GetInputElement(eInputLayoutType type, Format format, int index,int offset)
		{
			string name = LayoutSemantics[type];
			InputElement elem = new InputElement(name, index, format,offset, 0);
			return elem;
		}

		#region Auto Index Element Array
		public static void AutoIndex(InputElement[] elements)
        {
            Dictionary<string, int> semanticcount = new Dictionary<string, int>();

            for (int i = 0; i < elements.Length;i++ )
            {
                InputElement elem = elements[i];
                if (semanticcount.ContainsKey(elem.SemanticName))
                {
                    elem.SemanticIndex = semanticcount[elem.SemanticName];
                    semanticcount[elem.SemanticName]++;
                }
                else
                {
                    semanticcount.Add(elem.SemanticName, 1);
                    elem.SemanticIndex = 0;
                }
            }
		}
		#endregion
	}
}
