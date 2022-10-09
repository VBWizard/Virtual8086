using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using Word = System.UInt16;
using DWord = System.UInt32;

namespace VirtualProcessor
{

    public struct sPortValue
    {
        public UInt16 Portnum;
        public UInt32 Value;
        public TypeCode Size;
    }

    public class Ports
    {
        public UInt32[] mPorts;
        private static UInt32 mOutCount = 0, mInCount = 0;
        public Word ActivePort;
        internal PCSystem mSystem;

        public Ports(PCSystem pSystem)
        {
            mPorts = new UInt32[64 * 1024];
            mSystem = pSystem;
        }

        public void Out(UInt16 PortNum, UInt32 Value, TypeCode Size)
        {
            if (PortNum == 0x400 || PortNum == 0x401 /*|| PortNum == 0x402 || PortNum == 0x403*/)
            {
                mSystem.PrintDebugMsg(eDebuggieNames.Exceptions, "BOCHS_MESSAGE " + (char)((byte)(Value)));
                return;
            }
            ActivePort = PortNum;
            //if (PortNum == 0x1f0 || PortNum == 0x1f1 || PortNum == 0x1f2 || PortNum == 0x1f3 || PortNum == 0x1f4 || PortNum == 0x1f5 || PortNum == 0x1f6 || PortNum == 0x1f7 || PortNum == 0x3f6)
            //{
            //    sPortValue s = new sPortValue();
            //    s.Portnum = PortNum;
            //    s.Size = Size;
            //    s.Value = Value;
            //    mSystem.DeviceBlock.mHDC.Handle_OUT(s);
            //}
            //else
                //Used by the processor to capture data OUT request
                OnPortOutEvent(new CustomEventArgs(PortNum, Value, Size));
            mOutCount++;
            //mPorts[Portnum] = Value.mDWord;
            //Used for debugging to display what was sent OUT
           //OnPortOutDoneEvent(new CustomEventArgs(PortNum, Value, Size));
            ActivePort = 0;
        }

        public void Out(UInt16 PortNum, byte Value)
        {
            Out(PortNum, Value, TypeCode.Byte);
        }
        public void Out(UInt16 PortNum, UInt16 Value)
        {
            Out(PortNum, Value, TypeCode.UInt16);
        }
        public void Out(UInt16 PortNum, UInt32 Value)
        {
            Out(PortNum, Value, TypeCode.UInt32);
        }
        public UInt32 In(UInt16 PortNum, TypeCode Size)
        {
            UInt32 lValue = 0;

            ActivePort = PortNum;
            //if (PortNum == 0x1f0 || PortNum == 0x1f1 || PortNum == 0x1f2 || PortNum == 0x1f3 || PortNum == 0x1f4 || PortNum == 0x1f5 || PortNum == 0x1f6 || PortNum == 0x1f7 || PortNum == 0x3f6)
            //{
            //    sPortValue s = new sPortValue();
            //    s.Portnum = PortNum;
            //    s.Size = Size;
            //    s.Value = 0;
            //    mSystem.DeviceBlock.mHDC.Handle_IN(s);
            //}
            //else
                //Used by devices to capture data IN request
                OnPortInEvent(new CustomEventArgs(PortNum, lValue, Size));

            lValue = mPorts[PortNum];

            mInCount++;
            //OnPortInDoneEvent(new CustomEventArgs(PortNum, lValue, Size));
            ActivePort = 0;
            return lValue;
        }
        public byte In(UInt16 PortNum)
        {
            return (byte)In(PortNum, TypeCode.Byte);
        }

        public class CustomEventArgs : EventArgs
        {
            public CustomEventArgs(UInt16 Port, UInt32 Value, TypeCode Size)
            {
                //mPortValue = new sPortValue();
                mPortValue.Portnum = Port;
                mPortValue.Value = Value;
                mPortValue.Size = Size;
            }
            private sPortValue mPortValue;
            public sPortValue PortInfo
            {
                get { return mPortValue; }
            }
        }



        #region Event Handlers
        // Declare the event using EventHandler<T>
        public event EventHandler<CustomEventArgs> HandleDataOut;
        protected virtual void OnPortOutEvent(CustomEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CustomEventArgs> handler = HandleDataOut;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        // Declare the event using EventHandler<T>
        public event EventHandler<CustomEventArgs> HandleDataIn;
        protected virtual void OnPortInEvent(CustomEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CustomEventArgs> handler = HandleDataIn;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        public event EventHandler<CustomEventArgs> HandleDataOutDone;
        protected virtual void OnPortOutDoneEvent(CustomEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CustomEventArgs> handler = HandleDataOutDone;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        // Declare the event using EventHandler<T>
        public event EventHandler<CustomEventArgs> HandleDataInDone;
        protected virtual void OnPortInDoneEvent(CustomEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CustomEventArgs> handler = HandleDataInDone;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        #endregion


    }
}



