using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using Word = System.UInt16;
using DWord = System.UInt32;

namespace VirtualProcessor.Devices
{
    public enum eIODeviceHandlerStatus
    {
        Unused,
        Used
    }
    public enum eDataDirection
    {
        Undefined = 0,
        IO_In = 1,
        IO_Out = 2,
        IO_InOut = 3,
        Mem_Read = 1,
        Mem_Write = 2,
        Mem_ReadWrite = 3
    }
    public struct sIOHandler
    {
        public cDevice Device;
        public  UInt32 PortNum;
        public eDataDirection Direction;
        public eIODeviceHandlerStatus Status;

        public sIOHandler(cDevice iDevice, UInt32 iPortNum, eDataDirection iDirection)
        {
            Device = iDevice;
            PortNum = iPortNum;
            Direction = iDirection;
            Status = eIODeviceHandlerStatus.Unused;
        }
    }

    public class cDeviceBlock
    {
        #region Declarations
        public const DWord MAX_HANDLED_DEVICES = 0x10000;
        //Parent (Processor)
        internal PCSystem mSystem;
        internal readonly Processor_80x86 mProc;
        
        //Thread tracking for each device's DeviceThread
        internal Thread[] mDeviceThreads;
        internal int[] mDeviceThreadTypes;
        internal bool mPoweredUp = false;
        //DEVICES:
        //Need to be public so that they can interact with the rest of the system
        public cKbdDevice mKeyboard;
        public cPICDevice mPIC;
        public cFloppyDevice mFloppy;
        public cSerial mSerial;
        public cCMOSDevice mCMOS;
        public cDMA mDMA;
        public cHDC mHDC;
        public cPS2 mPS2;
        public cPIT mPIT;

        cTestDevice mPICTest;

        //For now we'll create 64k ... later we can refine it if we don't need so many
        //Each device/port/direction gets 1 handler
        //If a port defines a a Direction of in & out it only needs 1 handler for both
        private sIOHandler[] mIODeviceHandlers = new sIOHandler[MAX_HANDLED_DEVICES];

        private Word mPortsHandledCount = 0;
        #endregion
        private Thread mWorkerThread;
        private Type mWorkerThreadCurrentClass;
        
        public cDeviceBlock ( PCSystem System )
        {
            for (int c = 0; c < MAX_HANDLED_DEVICES; c++)
                mIODeviceHandlers[c].Status = eIODeviceHandlerStatus.Unused;
            mSystem = System;
            mDeviceThreads = new Thread[20];
            mDeviceThreadTypes = new int[20];
            mSystem = System;
            mProc = System.mProc;
            //For now statically create the devices

            //PIC first because even the keyboard will request an IRQ (#1)
            mPIC = new cPICDevice(this);
            mPIC.InitDevice();
            mDeviceThreadTypes[1] = (int)eDeviceClass.PIC;
            mDeviceThreads[1] = new Thread(new ThreadStart(mPIC.DeviceThread));
            if (mDeviceThreads[1].Name == null || mDeviceThreads[1].Name == "")
                mDeviceThreads[1].Name = mPIC.DeviceName;

            mKeyboard = new cKbdDevice(this);
            mKeyboard.InitDevice();
            mDeviceThreadTypes[0] = (int)eDeviceClass.Keyboard;
            mDeviceThreads[0] = new Thread(new ThreadStart(mKeyboard.DeviceThread));
            mDeviceThreads[0].Start();
            if (mDeviceThreads[0].Name == null || mDeviceThreads[0].Name == "")
                mDeviceThreads[0].Name = mKeyboard.DeviceName;

            mSerial = new cSerial(this);
            mSerial.InitDevice();
            mDeviceThreadTypes[2] = (int)eDeviceClass.Serial;
            mDeviceThreads[2] = new Thread(new ThreadStart(mSerial.DeviceThread));
            mDeviceThreads[2].Start();
            if (mDeviceThreads[2].Name == null || mDeviceThreads[2].Name == "")
                mDeviceThreads[2].Name = mSerial.DeviceName;


            mFloppy = new cFloppyDevice(this);
            mFloppy.InitDevice();
            mDeviceThreadTypes[3] = (int)eDeviceClass.Keyboard;
            mDeviceThreads[3] = new Thread(new ThreadStart(mFloppy.DeviceThread));
            mDeviceThreads[3].Start();
            if (mDeviceThreads[3].Name == null || mDeviceThreads[3].Name == "")
                mDeviceThreads[3].Name = mFloppy.DeviceName;

            //CMOS is the largest thread # so it is the last to be killed on shutdown, giving it a chance to quit
            //(keeping in mind that it has a 1 second sleep)
            mCMOS = new cCMOSDevice(this);
            mCMOS.InitDevice();
            mDeviceThreadTypes[9] = (int)eDeviceClass.CMOS;
            mDeviceThreads[9] = new Thread(new ThreadStart(mCMOS.DeviceThread));
            mDeviceThreads[9].Start();
            if (mDeviceThreads[9].Name == null || mDeviceThreads[9].Name == "")
                mDeviceThreads[9].Name = mCMOS.DeviceName;

            mDMA = new cDMA(this);
            mDMA.InitDevice();
            mDeviceThreadTypes[5] = (int)eDeviceClass.DMA;
            mDeviceThreads[5] = new Thread(new ThreadStart(mDMA.DeviceThread));
            mDeviceThreads[5].Start();
            if (mDeviceThreads[5].Name == null || mDeviceThreads[5].Name == "")
                mDeviceThreads[5].Name = mDMA.DeviceName;

            mHDC = new cHDC(this);
            mHDC.InitDevice();
            mDeviceThreadTypes[6] = (int)eDeviceClass.HD;
            mDeviceThreads[6] = new Thread(new ThreadStart(mHDC.DeviceThread));
            mDeviceThreads[6].Start();
            if (mDeviceThreads[6].Name == null || mDeviceThreads[6].Name == "")
                mDeviceThreads[6].Name = mHDC.DeviceName;

            mPS2 = new cPS2(this);
            mPS2.InitDevice();
            mDeviceThreadTypes[7] = (int)eDeviceClass.PS2;
            mDeviceThreads[7] = new Thread(new ThreadStart(mPS2.DeviceThread));
            mDeviceThreads[7].Start();
            if (mDeviceThreads[7].Name == null || mDeviceThreads[7].Name == "")
                mDeviceThreads[7].Name = mPS2.DeviceName;

            mPIT = new cPIT(this);
            mPIT.InitDevice();
            mDeviceThreadTypes[8] = (int)eDeviceClass.PIT;
            mDeviceThreads[8] = new Thread(new ThreadStart(mPIT.DeviceThread));
            mDeviceThreads[8].Start();
            if (mDeviceThreads[8].Name == null || mDeviceThreads[8].Name == "")
                mDeviceThreads[8].Name = mPIT.DeviceName;
            mPoweredUp = true;

            mDeviceThreads[1].Start();

        }
        public void Shutdown()
        {
            //Request shutdown of devices
            //Each device's DeviceThread is responsible for looking at this boolean and 
            //finishing up its work when it sees it set
            mKeyboard.RequestShutdown();
            mPIC.RequestShutdown();
            mFloppy.RequestShutdown();
            mCMOS.RequestShutdown();
            mDMA.RequestShutdown();
            mHDC.RequestShutdown();
            mSerial.RequestShutdown();
            mPIT.RequestShutdown();
            //Give each device a reasonable amount of time to stop and then kill it
            Thread.Sleep(100);
            for (int c = 0; c < 20; c++)
            {
                if (mDeviceThreads[c]!=null)
                    if (mDeviceThreads[c].ThreadState != ThreadState.Stopped)
                {
                    if (mDeviceThreads[c].ThreadState != ThreadState.Stopped)
                        mDeviceThreads[c].Abort();
                }
            }

            //Disable all device port handlers
            for (int c = 0; c < MAX_HANDLED_DEVICES; c++)
                if (mIODeviceHandlers[c].Status == eIODeviceHandlerStatus.Used)
                    UnRegisterIOHandler(mIODeviceHandlers[c].Device, mIODeviceHandlers[c].PortNum);

            mPoweredUp = false;
        }
        public bool RegisterIOHandler(cDevice Device, UInt32 Port, eDataDirection Direction)
        {
            if (mIODeviceHandlers[Port].Status == eIODeviceHandlerStatus.Used)
                return false;
            mIODeviceHandlers[Port].Device = Device;
            mIODeviceHandlers[Port].Direction = Direction;
            mIODeviceHandlers[Port].Status = eIODeviceHandlerStatus.Used;
            mPortsHandledCount++;
            CheckForRegisterPortHandler();
            return true;
        }
        public bool UnRegisterIOHandler(cDevice Device, UInt32 Port)
        {
            if (mIODeviceHandlers[Port].Status == eIODeviceHandlerStatus.Unused)
                return false;
            mIODeviceHandlers[Port].Device = null;
            mIODeviceHandlers[Port].Direction = eDataDirection.Undefined;
            mIODeviceHandlers[Port].Status = eIODeviceHandlerStatus.Unused;
            mPortsHandledCount--;
            CheckForUnregisterPortHandler();
            return true;
        }
        public void RegisterIRQ(cDevice Device, int IRQNum)
        {
            bool lResult = mPIC.RegisterIRQ(Device, IRQNum);
            if (!lResult)
                mSystem.HandleException(this, new Exception("Device " + Device.DeviceName + " could not register IRQ # " + IRQNum + ", see debug for details"));

        }
        public void DMAWriteByte(DWord MemAddr, Word Channel)
        {
            byte lByte = 0x0C;
            while (mProc.Signals.LOCK)
                Thread.Sleep(1);
            if (Channel == 2)
                mFloppy.DMAWrite(ref lByte);
            sInstruction sIns = new sInstruction();
            mProc.mem.SetByte(mProc, ref sIns, MemAddr, lByte);
        }
        public void DMAReadByte(DWord MemAddr, Word Channel)
        {//i.e. Write from Memory to Device
            if (Channel == 2)
            {
                sInstruction sIns = new sInstruction();
                mFloppy.DMARead(mProc.mem.pMemory(mProc, MemAddr));
            }
        }

        public void SetDRQ(Byte Channel, bool Value)
        {
            mProc.DRQ[Channel] = Value;
            mDMA.DRQ(Channel, Value);
            if (!Value)
                return;
            while (mProc.Signals.HOLD)
                mDMA.RaiseHLDA();
        }

        #region EventHandlers
    
        void HandleDataInEvent(object sender, Ports.CustomEventArgs e)
        {
            if (mIODeviceHandlers[e.PortInfo.Portnum].Status == eIODeviceHandlerStatus.Used)
            {
                if (mIODeviceHandlers[e.PortInfo.Portnum].Direction == eDataDirection.IO_In || mIODeviceHandlers[e.PortInfo.Portnum].Direction == eDataDirection.IO_InOut)
                {
                    mIODeviceHandlers[e.PortInfo.Portnum].Device.HandleIO(e.PortInfo, eDataDirection.IO_In);
                }
                return;
            }
            mProc.ports.mPorts[e.PortInfo.Portnum] = 0xffffffff;
        }
        void HandleDataOutEvent(object sender, Ports.CustomEventArgs e)
        {
            if (mIODeviceHandlers[e.PortInfo.Portnum].Status == eIODeviceHandlerStatus.Used)
                if (mIODeviceHandlers[e.PortInfo.Portnum].Direction == eDataDirection.IO_Out || mIODeviceHandlers[e.PortInfo.Portnum].Direction == eDataDirection.IO_InOut)
                {
                    mIODeviceHandlers[e.PortInfo.Portnum].Device.HandleIO(e.PortInfo, eDataDirection.IO_Out);
                }

        }
        /// <summary>
        /// If we are no longer handling any IO events, unregister our port handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckForUnregisterPortHandler()
        {
            if (mPortsHandledCount == 0)
            {
                mProc.ports.HandleDataIn -= HandleDataInEvent;
                mProc.ports.HandleDataOut -= HandleDataOutEvent;
            }
        }
        /// <summary>
        /// If we are now handling IO events, register our port handler
        /// </summary>
        private void CheckForRegisterPortHandler()
        {
            //==1 means we just started handling events
            if (mPortsHandledCount == 1)
            {
                mProc.ports.HandleDataIn += HandleDataInEvent;
                mProc.ports.HandleDataOut += HandleDataOutEvent;
            }
        }
#endregion
    }
}
