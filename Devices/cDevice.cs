using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Word = System.UInt16;
using DWord = System.UInt32;
using System.Threading;

namespace VirtualProcessor.Devices
{
    public enum eDeviceClass : int
    {
        PIC = 1,
        PIT = 2,
        Keyboard = 3,
        Floppy = 4,
        CMOS = 5,
        DMA = 6,
        HD = 7,
        Serial = 8,
        PS2 = 9
    }

    interface iDevice
    {
        void InitDevice();
        void HandleIO(sPortValue IO, eDataDirection Direction);
        void ResetDevice();
        //Every device gets a slice of time to do its own thing
        //This is the main thread for the device
        void DeviceThread();
        void RequestShutdown();
    }

    public abstract class acDevice : iDevice
    {
        protected string mName;
        public string DeviceName
        { get { return mName; } }
        protected string mDeviceId;
        public string DeviceID
        { get { return mDeviceId; } }
        protected eDeviceClass mDeviceClass;
        public eDeviceClass DeviceClass
        { get { return mDeviceClass; } }
        protected Boolean mShutdownRequested;
        protected int DeviceThreadSleep;
        protected bool DeviceThreadActive;

        public abstract void InitDevice();
        public abstract void HandleIO(sPortValue IO, eDataDirection Direction);
        public abstract void ResetDevice();
        public abstract void DeviceThread();
        public abstract void RequestShutdown();
        protected cDeviceBlock mParent;
    }

    public class cDevice : acDevice, iDevice
    {
        protected const int DEVICE_THREAD_SLEEP_TIMEOUT = 100;
        internal sIOHandler[] mIOHandlers;

        public override void ResetDevice()
        {}
        public override void InitDevice()
        {
            for (int c = 0; c < mIOHandlers.Count(); c++)
            {
                mParent.RegisterIOHandler(mIOHandlers[c].Device, mIOHandlers[c].PortNum, mIOHandlers[c].Direction);
                DeviceThreadSleep = DEVICE_THREAD_SLEEP_TIMEOUT;
            }
        }

        public override void RequestShutdown()
        {
            mShutdownRequested = true;
            DeviceThreadSleep = 0;
        }

        public override void DeviceThread()
        {}
        public override void HandleIO(sPortValue IO, eDataDirection Direction)
        {}
    }

}
