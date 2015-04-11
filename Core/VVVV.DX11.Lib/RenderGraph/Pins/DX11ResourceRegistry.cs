using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Hosting.IO;
using VVVV.Utils.Streams;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.DX11;
using VVVV.DX11.Lib.RenderGraph.Pins;

namespace VVVV.Nodes
{
    public delegate void PinCreated(IPluginIO input);

    public class ResourceListener : IPinListener
    {
        public IPin pin;

        public IPin link;

        public ResourceListener(IPluginIO pin)
        {
            this.pin = (IPin)pin;
            this.pin.AddListener(this);
        }

        public void ChangedCB()
        {

        }

        public void ConnectedCB(IPin otherPin)
        {
            this.link = otherPin;
        }

        public void DisconnectedCB(IPin otherPin)
        {
            this.link = null;
        }

        public void StatusChangedCB()
        {
            
        }

        public void SubtypeChangedCB()
        {

        }
    }

    public class DX11ResourceRegistry : IIORegistry
    {
        public Dictionary<IPin, ResourceListener> pinconnections = new Dictionary<IPin, ResourceListener>();

        public DX11ResourceRegistry()
        {
        }

        public PluginInterfaces.V2.IIOContainer CreateIOContainer(PluginInterfaces.V2.IIOFactory factory, PluginInterfaces.V2.IOBuildContext context)
        {
            if (context.Direction == PinDirection.Input)
            {
                var t = context.DataType;
                var attribute = context.IOAttribute;
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(INodeIn)));

                Type restype = t.GetGenericArguments()[0];
                Type fulltype = typeof(DX11Resource<>).MakeGenericType(restype);
                var stream = Activator.CreateInstance(typeof(DX11ResourceInputStream<,>).MakeGenericType(fulltype, restype), container.RawIOObject) as IInStream;
                IPluginIO io = container.GetPluginIO();

                ResourceListener rl = new ResourceListener(io);

                this.pinconnections.Add(rl.pin, rl);
                return IOContainer.Create(context, stream, container);
            }

            if (context.Direction == PinDirection.Output)
            {
                var t = context.DataType;
                var attribute = context.IOAttribute;
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(INodeOut)));

                Type restype = t.GetGenericArguments()[0];
                Type fulltype = typeof(DX11Resource<>).MakeGenericType(restype);
                var stream = Activator.CreateInstance(typeof(DX11ResourceOutputStream<,>).MakeGenericType(fulltype,restype), container.RawIOObject) as IOutStream;
                return IOContainer.Create(context, stream, container);
            }

            return null;
        }

        public bool CanCreate(PluginInterfaces.V2.IOBuildContext context)
        {
            if (context.DataType == null) { return false; }
            if (context.DataType.IsGenericType && context.IOType.IsGenericType)
            {
                if ((context.DataType.GetGenericTypeDefinition() == typeof(DX11Resource<>)
                    && context.IOType.GetGenericTypeDefinition() == typeof(IInStream<>)
                    && context.Direction == PinDirection.Input)
                    ||
                    (context.DataType.GetGenericTypeDefinition() == typeof(DX11Resource<>)
                    && context.IOType.GetGenericTypeDefinition() == typeof(IOutStream<>)
                    && context.Direction == PinDirection.Output))
                {
                    return true;
                }
            }

            return false;
        }

        public void Register(IIORegistry registry, bool first)
        {

        }
    }
}
