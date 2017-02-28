using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VVVV.DX11.Nodes;

namespace FeralTic.DX11.Resources
{

    public enum eImageFormat { Bmp = 1, Jpeg = 2, Png = 3, Tiff = 4, Gif = 5, Hdp = 6,Dds = 128, Tga = 129 }


    [StructLayout(LayoutKind.Sequential)]
    public struct ImageMetadata
    {
        public long Width;
        public long Height;
        public long Depth;
        public long ArraySize;
        public long MipLevels;
        public int MiscFlags;
        public int MiscFlags2;
        public SlimDX.DXGI.Format Format;
        public SlimDX.Direct3D11.ResourceDimension Dimension;
    }


    public class TextureLoader
    {
        public class NativeMethods
        {
            public static ImageMetadata LoadMetadataFromFile(string path)
            {
                if (IntPtr.Size == 8)
                {
                    return NativeMethods64.LoadMetadataFromFile(path);
                }
                else
                {
                    return NativeMethods32.LoadMetadataFromFile(path);
                }
            }

            public static ImageMetadata LoadMetadataFromMemory(IntPtr dataPointer, int dataLength)
            {
                if (IntPtr.Size == 8)
                {
                    return NativeMethods64.LoadMetadataFromMemory(dataPointer, dataLength);
                }
                else
                {
                    return NativeMethods32.LoadMetadataFromMemory(dataPointer, dataLength);
                }
            }

            public static long LoadTextureFromFile(IntPtr device, string path,out IntPtr resource, int miplevels)
            {
                if (IntPtr.Size == 8)
                {
                    return NativeMethods64.LoadTextureFromFile(device, path, out resource, miplevels);
                }
                else
                {
                    return NativeMethods32.LoadTextureFromFile(device, path, out resource, miplevels);
                }
            }

            public static long SaveTextureToFile(IntPtr device, IntPtr context, IntPtr resource, string path, int format)
            {
                if (IntPtr.Size == 8)
                {
                    return NativeMethods64.SaveTextureToFile(device, context,resource, path, format);
                }
                else
                {
                    return NativeMethods32.SaveTextureToFile(device, context, resource, path, format);
                }
            }

            public static long SaveCompressedTextureToFile(IntPtr device, IntPtr context, IntPtr resource, string path, int blocktype)
            {
                if (IntPtr.Size == 8)
                {
                    return NativeMethods64.SaveCompressedTextureToFile(device, context, resource, path, blocktype);
                }
                else
                {
                    return NativeMethods32.SaveCompressedTextureToFile(device, context, resource, path, blocktype);
                }
            }

            public static long LoadTextureFromMemory(IntPtr device, IntPtr dataPointer, int dataLength, out IntPtr resource)
            {
                if (IntPtr.Size == 8)
                {
                    return NativeMethods64.LoadTextureFromMemory(device, dataPointer, dataLength, out resource);
                }
                else
                {
                    return NativeMethods32.LoadTextureFromMemory(device, dataPointer, dataLength, out resource);
                }
            }

            public static long SaveCompressedTextureToMemory(IntPtr device, IntPtr context, IntPtr resource, int blocktype, out IntPtr data, out int size, out IntPtr blobPtr)
            {
                if (IntPtr.Size == 8)
                {
                    return NativeMethods64.SaveCompressedTextureToMemory(device, context, resource, blocktype, out data, out size, out blobPtr);
                }
                else
                {
                    return NativeMethods32.SaveCompressedTextureToMemory(device, context, resource, blocktype, out data, out size, out blobPtr);
                }
            }

            public static void DeleteBlob(IntPtr blob)
            {
                if (IntPtr.Size == 8)
                {
                    NativeMethods64.DeleteBlob(blob);
                }
                else
                {
                    NativeMethods32.DeleteBlob(blob);
                }
            }
        }

        private class NativeMethods64
        {
            [DllImport("DirectXTexLib_x64", CharSet = CharSet.Unicode)]
            public static extern ImageMetadata LoadMetadataFromFile(string path);

            [DllImport("DirectXTexLib_x64", CharSet = CharSet.Unicode)]
            public static extern ImageMetadata LoadMetadataFromMemory(IntPtr dataPointer, int dataLength);

            [DllImport("DirectXTexLib_x64", CharSet = CharSet.Unicode)]
            public static extern long LoadTextureFromFile(IntPtr device, string path, out IntPtr resource, int miplevels);

            [DllImport("DirectXTexLib_x64", CharSet = CharSet.Unicode)]
            public static extern long LoadTextureFromMemory(IntPtr device, IntPtr dataPointer, int dataLength, out IntPtr resource);

            [DllImport("DirectXTexLib_x64", CharSet = CharSet.Unicode)]
            public static extern long SaveTextureToFile(IntPtr device, IntPtr context, IntPtr resource, string path, int format);

            [DllImport("DirectXTexLib_x64", CharSet = CharSet.Unicode)]
            public static extern long SaveCompressedTextureToFile(IntPtr device, IntPtr context, IntPtr resource, string path, int blocktype);

            [DllImport("DirectXTexLib_x64", CharSet = CharSet.Unicode)]
            public static extern long SaveCompressedTextureToMemory(IntPtr device, IntPtr context, IntPtr resource, int blocktype, out IntPtr data, out int size, out IntPtr blobPtr);

            [DllImport("DirectXTexLib_x64", CharSet = CharSet.Unicode)]
            public static extern void DeleteBlob(IntPtr blob);
        }


        private class NativeMethods32
        {
            [DllImport("DirectXTexLib_x86", CharSet = CharSet.Unicode)]
            public static extern ImageMetadata LoadMetadataFromFile(string path);

            [DllImport("DirectXTexLib_x86", CharSet = CharSet.Unicode)]
            public static extern ImageMetadata LoadMetadataFromMemory(IntPtr dataPointer, int dataLength);

            [DllImport("DirectXTexLib_x86",CharSet=CharSet.Unicode)]
            public static extern long LoadTextureFromFile(IntPtr device, string path,out IntPtr resource, int miplevels);

            [DllImport("DirectXTexLib_x86", CharSet = CharSet.Unicode)]
            public static extern long SaveTextureToFile(IntPtr device,IntPtr context,IntPtr resource, string path,int format);

            [DllImport("DirectXTexLib_x86", CharSet = CharSet.Unicode)]
            public static extern long LoadTextureFromMemory(IntPtr device, IntPtr dataPointer, int dataLength, out IntPtr resource);

            [DllImport("DirectXTexLib_x86", CharSet = CharSet.Unicode)]
            public static extern long SaveCompressedTextureToFile(IntPtr device, IntPtr context, IntPtr resource, string path, int blocktype);

            [DllImport("DirectXTexLib_x86", CharSet = CharSet.Unicode)]
            public static extern long SaveCompressedTextureToMemory(IntPtr device, IntPtr context, IntPtr resource, int blocktype, out IntPtr data, out int size, out IntPtr blobPtr);

            [DllImport("DirectXTexLib_x86", CharSet = CharSet.Unicode)]
            public static extern void DeleteBlob(IntPtr blob);
        }

        public static void SaveToFileCompressed(DX11RenderContext device, DX11Texture2D texture, string path, DdsBlockType blockType)
        {
            long retcode = NativeMethods.SaveCompressedTextureToFile(device.Device.ComPointer, device.CurrentDeviceContext.ComPointer,
                texture.Resource.ComPointer, path, (int)blockType);

            if (retcode < 0)
            {
                throw new Exception("Failed to Save Texture");
            }
        }

        public static void 
            SaveToMemoryCompressed(DX11RenderContext device, DX11Texture2D texture, DdsBlockType blockType, out IntPtr data, out int size, out IntPtr blob)
        {
            long retcode = NativeMethods.SaveCompressedTextureToMemory(device.Device.ComPointer, device.CurrentDeviceContext.ComPointer,
                texture.Resource.ComPointer, (int)blockType, out data, out size,out blob);

            if (retcode < 0)
            {
                throw new Exception("Failed to Save Texture");
            }
        }
    }
}
