using System;
using System.Linq;

using VVVV.PluginInterfaces.V2;
using System.Drawing;

using VVVV.Utils.IO;
using System.Windows.Forms;
using VVVV.DX11.Nodes.Renderers.Graphics.Touch;
using System.Reflection;
using FormsMouseEventArgs = System.Windows.Forms.MouseEventArgs;
using System.Reactive.Linq;

namespace VVVV.DX11.Nodes
{
    public partial class DX11RendererNode 
    {
        private void CreateUserInputEventPins()
        {
            if (CreateMouseEventsOut())
            {
                CreateKeyboardEventsOut();
                CreateTouchEventsOut();
            }
        }

        private bool CreateMouseEventsOut()
        {
            bool isOlderMouse;
            var constructor = FindMouseConstructor(out isOlderMouse);

            if (constructor == null)
                return false;

            var oa = new OutputAttribute("Mouse Events");
            oa.IsSingle = true;
            var mouseEventOut = FIOFactory.CreatePin<Mouse>(oa);

            var mouseDowns = Observable.FromEventPattern<FormsMouseEventArgs>(this, "MouseDown")
                .Select(p => p.EventArgs.ToMouseDownNotification(this));
            var mouseMoves = Observable.FromEventPattern<FormsMouseEventArgs>(this, "MouseMove")
                .Select(p => p.EventArgs.ToMouseMoveNotification(this));
            var mouseUps = Observable.FromEventPattern<FormsMouseEventArgs>(this, "MouseUp")
                .Select(p => p.EventArgs.ToMouseUpNotification(this));
            var mouseClicks = Observable.FromEventPattern<FormsMouseEventArgs>(this, "MouseClick")
                .Select(p => p.EventArgs.ToMouseClickNotification(this));
            var mouseDoubleClicks = Observable.FromEventPattern<FormsMouseEventArgs>(this, "MouseDoubleClick")
                .Select(p => p.EventArgs.ToMouseDoubleClickNotification(this));
            var mouseWheels = Observable.FromEventPattern<FormsMouseEventArgs>(this, "MouseWheel")
                .Select(p => p.EventArgs.ToMouseWheelNotification(this));

            var mouseStream = mouseDowns
                        .Merge<MouseNotification>(mouseMoves)
                        .Merge(mouseUps)
                        .Merge(mouseClicks)
                        .Merge(mouseDoubleClicks)
                        .Merge(mouseWheels);

            if (isOlderMouse)
                mouseEventOut[0] = (Mouse)constructor.Invoke(new object[] { mouseStream, false });
            else
                mouseEventOut[0] = (Mouse)constructor.Invoke(new object[] { mouseStream, false, true });

            return true;
        }

        static ConstructorInfo FindMouseConstructor(out bool isOlderMouse)
        {
            //add mouse event output for newer vvvv versions
            var mouseType = typeof(Mouse);

            //newer mouse type
            isOlderMouse = false;
            var newerParams = new Type[] { typeof(IObservable<MouseNotification>), typeof(bool), typeof(bool) };
            var newerConstructor = mouseType.GetConstructor(newerParams);
            if (newerConstructor != null)
            {
                return newerConstructor;
            }

            //older mouse type
            isOlderMouse = true;
            var olderParams = new Type[] { typeof(IObservable<MouseNotification>), typeof(bool) };
            var olderConstructor = mouseType.GetConstructor(olderParams);
            if (olderConstructor != null)
            {
                return olderConstructor;
            }

            return null;
        }

        private void CreateKeyboardEventsOut()
        {
            var oa = new OutputAttribute("Keyboard Events");
            oa.IsSingle = true;
            var keyboardEventOut = FIOFactory.CreatePin<Keyboard>(oa);

            var keyDowns = Observable.FromEventPattern<KeyEventArgs>(this, "KeyDown")
                .Select(p => p.EventArgs.ToKeyDownNotification());
            var keyUps = Observable.FromEventPattern<KeyEventArgs>(this, "KeyUp")
                .Select(p => p.EventArgs.ToKeyUpNotification());
            var keyPresses = Observable.FromEventPattern<KeyPressEventArgs>(this, "KeyPress")
                .Select(p => p.EventArgs.ToKeyPressNotification());

            var keyboardStream = keyDowns
                .Merge<KeyNotification>(keyUps)
                .Merge(keyPresses);

            keyboardEventOut[0] = new Keyboard(keyboardStream);
        }

        private void CreateTouchEventsOut()
        {
            var oa = new OutputAttribute("Touch Events");
            oa.IsSingle = true;
            var touchEventOut = FIOFactory.CreatePin<TouchDevice>(oa);

            var touchDowns = Observable.FromEventPattern<WMTouchEventArgs>(this, "TouchDown")
                .Select(p => p.EventArgs.ToTouchDownNotification(this));
            var touchMoves = Observable.FromEventPattern<WMTouchEventArgs>(this, "TouchMove")
                .Select(p => p.EventArgs.ToTouchMoveNotification(this));
            var touchUps = Observable.FromEventPattern<WMTouchEventArgs>(this, "TouchUp")
                .Select(p => p.EventArgs.ToTouchUpNotification(this));

            var touchStream = touchDowns
                .Merge<TouchNotification>(touchMoves)
                .Merge(touchUps);

            touchEventOut[0] = new TouchDevice(touchStream);
        }
    }

    public static class EventExtensions
    {
        //mouse
        public static MouseMoveNotification ToMouseMoveNotification(this FormsMouseEventArgs args, Control relativeTo)
        {
            return new MouseMoveNotification(args.Location, relativeTo.ClientSize);
        }

        public static MouseDownNotification ToMouseDownNotification(this FormsMouseEventArgs args, Control relativeTo)
        {
            return new MouseDownNotification(args.Location, relativeTo.ClientSize, args.Button);
        }

        public static MouseUpNotification ToMouseUpNotification(this FormsMouseEventArgs args, Control relativeTo)
        {
            return new MouseUpNotification(args.Location, relativeTo.ClientSize, args.Button);
        }

        public static MouseClickNotification ToMouseClickNotification(this FormsMouseEventArgs args, Control relativeTo)
        {
            return new MouseClickNotification(args.Location, relativeTo.ClientSize, args.Button, 1);
        }

        public static MouseClickNotification ToMouseDoubleClickNotification(this FormsMouseEventArgs args, Control relativeTo)
        {
            return new MouseClickNotification(args.Location, relativeTo.ClientSize, args.Button, 2);
        }

        public static MouseWheelNotification ToMouseWheelNotification(this FormsMouseEventArgs args, Control relativeTo)
        {
            return new MouseWheelNotification(args.Location, relativeTo.ClientSize, args.Delta);
        }

        //keyboard
        public static KeyDownNotification ToKeyDownNotification(this KeyEventArgs eventArgs)
        {
            return new KeyDownNotification(eventArgs.KeyCode);
        }

        public static KeyUpNotification ToKeyUpNotification(this KeyEventArgs eventArgs)
        {
            return new KeyUpNotification(eventArgs.KeyCode);
        }

        public static KeyPressNotification ToKeyPressNotification(this KeyPressEventArgs eventArgs)
        {
            return new KeyPressNotification(eventArgs.KeyChar);
        }

        //touch
        enum TouchNotificationAge { Older, Intermediate, Newer };
        static TouchNotificationAge TouchAge;

        //cash found constructors as this gets called for each event
        static ConstructorInfo STouchDownNotificationConstructor;
        static ConstructorInfo STouchMoveNotificationConstructor;
        static ConstructorInfo STouchUpNotificationConstructor;

        public static TouchDownNotification ToTouchDownNotification(this WMTouchEventArgs eventArgs, Control relativeTo)
        {
            if (STouchDownNotificationConstructor == null) //find correct constructor
                STouchDownNotificationConstructor = FindTouchNotificationConstructor<TouchDownNotification>();

            return ConstructTouchNotification<TouchDownNotification>(STouchDownNotificationConstructor, eventArgs, relativeTo);
        }


        public static TouchMoveNotification ToTouchMoveNotification(this WMTouchEventArgs eventArgs, Control relativeTo)
        {
            if (STouchMoveNotificationConstructor == null) //find correct constructor
                STouchMoveNotificationConstructor = FindTouchNotificationConstructor<TouchMoveNotification>();

            return ConstructTouchNotification<TouchMoveNotification>(STouchMoveNotificationConstructor, eventArgs, relativeTo);

        }

        public static TouchUpNotification ToTouchUpNotification(this WMTouchEventArgs eventArgs, Control relativeTo)
        {
            if (STouchUpNotificationConstructor == null) //find correct constructor
                STouchUpNotificationConstructor = FindTouchNotificationConstructor<TouchUpNotification>();

            return ConstructTouchNotification<TouchUpNotification>(STouchUpNotificationConstructor, eventArgs, relativeTo);
        }

        static T ConstructTouchNotification<T>(ConstructorInfo constructor, WMTouchEventArgs eventArgs, Control relativeTo) where T : TouchNotification
        {
            switch (TouchAge)
            {
                case TouchNotificationAge.Newer:
                    return (T)constructor.Invoke(new object[] { eventArgs.Location(), relativeTo.ClientSize, eventArgs.Id, eventArgs.IsPrimaryContact, eventArgs.ContactArea(), eventArgs.TouchDeviceID });

                case TouchNotificationAge.Intermediate:
                    return (T)constructor.Invoke(new object[] { eventArgs.Location(), relativeTo.ClientSize, eventArgs.Id, eventArgs.IsPrimaryContact, eventArgs.ContactArea() });

                case TouchNotificationAge.Older:
                    return (T)constructor.Invoke(new object[] { eventArgs.Location(), relativeTo.ClientSize, eventArgs.Id, eventArgs.ContactArea() });
            }

            return null;
        }

        static ConstructorInfo FindTouchNotificationConstructor<T>() where T : TouchNotification
        {
            var touchNotificationType = typeof(T);

            var newerParams = new Type[] { typeof(Point), typeof(Size), typeof(int), typeof(bool), typeof(Size), typeof(long) };
            var newerConstructor = touchNotificationType.GetConstructor(newerParams);

            if (newerConstructor != null)
            {
                TouchAge = TouchNotificationAge.Newer;
                return newerConstructor;
            }

            var intermediateParams = new Type[] { typeof(Point), typeof(Size), typeof(int), typeof(bool), typeof(Size) };
            var intermediateConstructor = touchNotificationType.GetConstructor(intermediateParams);

            if (intermediateConstructor != null)
            {
                TouchAge = TouchNotificationAge.Intermediate;
                return intermediateConstructor;
            }

            var olderParams = new Type[] { typeof(Point), typeof(Size), typeof(int), typeof(Size) };
            var olderConstructor = touchNotificationType.GetConstructor(olderParams);

            if (olderConstructor != null)
            {
                TouchAge = TouchNotificationAge.Older;
                return olderConstructor;
            }

            return null;
        }

        public static Point Location(this WMTouchEventArgs args)
        {
            return new Point(args.LocationX, args.LocationY);
        }

        public static Size ContactArea(this WMTouchEventArgs args)
        {
            return new Size(args.ContactX, args.ContactY);
        }
    }
}