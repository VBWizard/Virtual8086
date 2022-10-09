using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;

namespace VirtualProcessor.Devices
{
    //Ports:
    //  21 = Master IMR (Operation Control Word 1 - Master OCW1)
    //      Bits = IRQs (i.e. Bit 7 = IRQ 7, Bit 6 = IRQ 6)
    //      IRQ = Slave IRQ
    //  A1 = Slave IMR (Slave OCW1)
    //Masking: 1 Disables, 0 Enables

    enum sPortDefs : int
    {
        Master_IMR = 0x21,
        Slave_IMR = 0xA1
    }

    enum ePICMode : int
    {
        Master = 0x20,
        Slave = 0xA0
    }

    //Need to make this a public class so that the Processor can talk to the PIC (to handle IRQs)
    public class cPICDevice : cDevice
    {
        #region Private Variables & Constants
        private const int MASTER_COMMAND_PORT = 0x20;
        private const int MASTER_IMR_PORT = 0x21;
        private const int SLAVE_COMMAND_PORT = 0xA0;
        private const int SLAVE_IMR_PORT = 0xA1;

        private bool mMasterIRRFound, mSlaveIRRFound;
        //IMR = Interrupt Mask Register - SetC by code via ports, also set by PIC when IRQ is being serviced
        //TODO: Changed to internal to test keyboard IRQ - set back to private!!!!
        internal byte mMasterIMR = 0;
        private byte mSlaveIMR = 0;
        //ISR = Interrupt Service Register (which interrupts are being serviced - SetC only by driver
        private byte mMasterISR = 0;
        private byte mSlaveISR = 0;
        //IRR = Interrupt Request Register - SetC only by devices when they request an IRQ
        private byte mMasterIRR = 0;
        private byte mSlaveIRR = 0;

        //Next value to send when OUT received on port 0x20 or 0xA0
        private byte mNextCommandPortValue = 0;

        //INT # of first IRQ Interrupt Service Routine (Master)
        private byte mMasterInterruptOffset = 0x08;
        //INT # of first IRQ Interrupt Service Routine (Slave)
        private byte mSlaveInterruptOffset = 0x70;

        //True when (OUT & 0x10)
        private Boolean mMasterInDeviceInit = false;
        private Boolean mSlaveInDeviceInit = false;
        private byte mMasterInitCommandExpected = 0;
        private byte mSlaveInitCommandExpected = 0;
        private byte mMasterRequires4 = 0;
        private byte mSlaveRequires4 = 0;
        private const byte MAX_IRQ_NUM = 15; /*0 - 7*/
        private string[] mIRQOwner = new string[MAX_IRQ_NUM];

        private byte HighestPICISROffset;
        #endregion

        public cPICDevice(cDeviceBlock DevBlock)
        {
            mParent = DevBlock;
            mDeviceId = "06d8089b-df85-43c1-8c85-67197690ade4";
            mDeviceClass = eDeviceClass.PIC;
            mName = "PIC";
        }

        #region PIC Methods
        public bool RegisterIRQ(cDevice Device, int IRQNumber)
        {
            StringBuilder lTemp = new StringBuilder();
            lTemp = new StringBuilder("");

            if (IRQNumber > MAX_IRQ_NUM)
                lTemp.AppendFormat("IRQ Invalid: Device {0} requested to register IRQ# ({1}) (max IRQ is ({2})", Device.DeviceName, IRQNumber, MAX_IRQ_NUM);
            else if (mIRQOwner[IRQNumber] != null)
                lTemp.AppendFormat("IRQ Conflict: Device {0} requested to register IRQ# {1} which is in use by {2}", Device.DeviceName, IRQNumber, mIRQOwner[IRQNumber]);

            if (lTemp.ToString() != "")
            {
                if (mParent.mSystem.Debuggies.DebugPIC)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC,lTemp.ToString());
                return false;
            }
            mIRQOwner[IRQNumber] = Device.DeviceName;
            if (mParent.mSystem.Debuggies.DebugPIC)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "IRQ# " + IRQNumber + " registered to " + Device.DeviceName);
            return true;
        }
        public bool UnRegisterIRQ(cDevice Device, int IRQNumber)
        {
            StringBuilder lTemp = new StringBuilder();
            lTemp = new StringBuilder("");

            if (IRQNumber > MAX_IRQ_NUM)
                lTemp.AppendFormat("IRQ Invalid: Device {0} requested to unregister IRQ# ({1}) (max IRQ is ({2})", Device.DeviceName, IRQNumber, MAX_IRQ_NUM);
            else if (mIRQOwner[IRQNumber] != Device.DeviceName)
                lTemp.AppendFormat("Device {0} requested to unregister IRQ# {1} which is owned by {2}", Device.DeviceName, IRQNumber, mIRQOwner[IRQNumber]);
            else if (mIRQOwner[IRQNumber] == "")
                lTemp.AppendFormat("Device {0} requested to unregister IRQ# {1} which is not registered", Device.DeviceName, IRQNumber);

            if (lTemp.ToString() != "")
            {
                if (mParent.mSystem.Debuggies.DebugPIC)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, lTemp.ToString());
                return false;
            }
            mIRQOwner[IRQNumber] = "";
            if (mParent.mSystem.Debuggies.DebugPIC)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "IRQ# " + IRQNumber + " un-registered by " + Device.DeviceName);
            return true;
        }
        /// <summary>
        /// Returns a byte with only the bit of the highest priority (lowest value) active IRQ bit set
        /// </summary>
        /// <param name="WhichPic"></param>
        /// <returns></returns>
        private byte HighestPriorityActiveIRQ(ePICMode WhichPic)
        {
            byte lISR = 0;
            if (WhichPic == ePICMode.Master)
                lISR = mMasterISR;
            else
                lISR = mSlaveISR;

            //Need to check this out to make sure it works right
            //TODO: Will this properly check bit 0? (cnt==0)
            for (byte cnt = 0; cnt < 8; cnt++)
                if (((byte)Math.Pow(2, cnt) & lISR) == Math.Pow(2, cnt))
                    return (byte)Math.Pow(2, cnt);
                    //return cnt;
            return 0;
        }
        private byte HighestPriorityRequestedIRQ(ePICMode WhichPic)
        {
            byte lIRR = 0;
            if (WhichPic == ePICMode.Master)
                lIRR = mMasterIRR;
            else
                lIRR = mSlaveIRR;

            //Need to check this out to make sure it works right
            //TODO: Will this properly check bit 0? (cnt==0)
            byte lRetVal=0xff;
            for (int cnt = 0; cnt < 8; cnt++)
                if (((byte)Math.Pow(2, cnt) & lIRR) == Math.Pow(2, cnt))
                {
                    lRetVal = (byte)Math.Pow(2, cnt);
                    break;
                }
            if (lRetVal == 0xff)
                lRetVal = 0;
            else
            {
                if (WhichPic == ePICMode.Master)
                    mMasterIRRFound = true;
                else
                    mSlaveIRRFound = true;
            }
            return lRetVal;
        }
        public void RaiseIRQ(byte IRQNum)
        {
            if (mParent.mSystem.Debuggies.DebugPIC && IRQNum>0)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "IRQ Raise Request, IRQ # " + IRQNum + " by " + Thread.CurrentThread.Name);
            //Create a byte with just the bit we want to set
            UInt16 lIRQNum = (UInt16)(1 << IRQNum);

            //Raise IRQ on Master PIC
            if (IRQNum < 8)
            {
                //If the IRR for the IRQ is already set just return without doing anything
                if ((mMasterIRR & lIRQNum) == lIRQNum)
                {
                    if (mParent.mSystem.Debuggies.DebugPIC && IRQNum > 0)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "(Master) IRQ # "  + IRQNum + " already raised, skipping request, IRR = " + mMasterIRR.ToString("X2"));
                    return;
                }
                //Raise
                mMasterIRR ^= (byte)(lIRQNum & 0x00ff);
                if (mParent.mSystem.Debuggies.DebugPIC /*&& IRQNum > 0*/)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "(Master) IRQ " + IRQNum + " raised  on PIC, IRR = " + mMasterIRR.ToString("X2"));
                //Moved ServicePic to the DeviceThread
                //ServicePic(ePICMode.Master);
            }
            else
            //Raise IRQ on Slave PIC
            {
                lIRQNum >>= 8;
                //If the IRR for the IRQ is already set just return without doing anything
                if ((mSlaveIRR & (lIRQNum)) == (lIRQNum))
                {
                    if (mParent.mSystem.Debuggies.DebugPIC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "(Slave) IRQ # " + IRQNum + " already raised, skipping request, IRR = " + mMasterIRR.ToString("X2"));
                    return;
                }
                //Raise
                mSlaveIRR ^= (byte)(lIRQNum);
                if (mParent.mSystem.Debuggies.DebugPIC)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "(Slave) IRQ " + IRQNum + " raised  on PIC, IRR = " + mSlaveIRR.ToString("X2"));
                //Moved ServicePic to the DeviceThread
                //ServicePic(ePICMode.Slave);
            }
        }
        public void LowerIRQ(byte IRQNum)
        {
            //Create a byte with just the bit we want to unset
            byte lIRQNum = 0xFF;
            byte lMyBit = (byte)(1 << IRQNum);

            //Lower IRQ on Master PIC
            if (IRQNum < 8)
            {
                //If the IRR for the IRQ is already set just return without doing anything
                if ((mMasterIRR | lIRQNum) == lIRQNum)
                    return;
                //Lower
                mMasterIRR ^= lMyBit;
            }
            else
            //Lower IRQ on Slave PIC
            {
                //If the IRR for the IRQ is already set just return without doing anything
                if ((mMasterIRR & lIRQNum) == lIRQNum)
                    return;
                //Lower
                mSlaveIRR ^= lIRQNum;
            }
        }
        /// <summary>
        /// Figure out which current IRQ is highest priority (lowest number) and if it isn't being serviced,
        /// tell the CPU to switch to it
        /// </summary>
        /// <param name="WhichPic"></param>
        private void ServicePic(ePICMode WhichPic)
        {
            byte lHighestRequested = HighestPriorityRequestedIRQ(WhichPic);
            byte lHighestActive = HighestPriorityActiveIRQ(WhichPic);

            if (lHighestRequested < lHighestActive || (lHighestActive == 0 & lHighestRequested > 0))
            {
                //MASTER:
                //Check for masked IRQ - 1 Disabled, 0 Enabled
                if ((WhichPic == ePICMode.Master) & (lHighestRequested & mMasterIMR) == 0 && mParent.mProc.Signals.INTR == false)
                {
                    //Update the ISR
                    mMasterISR |= lHighestRequested;
                    //Tell the processor to service the new highest priority IRQ
                    HighestPICISROffset = mMasterInterruptOffset;
                    if (mParent.mSystem.Debuggies.DebugPIC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "Master asserting CPU INTR pin for IRQ # " + GetIRQNumForSetBit(lHighestRequested, ePICMode.Master));
                    mParent.mProc.Signals.INTR = true;
                }
                //SLAVE:
                else if ((WhichPic == ePICMode.Slave) & ((HighestPriorityRequestedIRQ(WhichPic) & mSlaveIMR) == lHighestRequested) && mParent.mProc.Signals.INTR == false)
                {
                    //Update the ISR
                    mSlaveISR |= HighestPriorityRequestedIRQ(WhichPic);
                    //Tell the processor to service the new highest priority IRQ
                    HighestPICISROffset = mSlaveInterruptOffset;
                    if (mParent.mSystem.Debuggies.DebugPIC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "Slave    asserting CPU INTR pin for IRQ # " + (lHighestRequested+7));
                    lock (mParent.mProc) 
                    mParent.mProc.Signals.INTR = true;
                }
                else
                    return;
            }
        }
        private void EnterInitialization(ePICMode WhichPic, byte Value)
        {
            switch (WhichPic)
            {
                case ePICMode.Master:
                    mMasterInDeviceInit = true;
                    mMasterInitCommandExpected = 2;
                    mMasterRequires4 = (byte)(Value & 1);
                    break;
                case ePICMode.Slave:
                    mSlaveInDeviceInit = true;
                    mSlaveInitCommandExpected = 2;
                    mSlaveRequires4 = (byte)(Value & 1);
                    break;
            }

        }
        private void ProcessInitCommand(ePICMode WhichPic, byte Value)
        {
            switch (WhichPic)
            {
                case ePICMode.Master:

                    if (mMasterInitCommandExpected == 2)
                    {
                        if (mParent.mSystem.Debuggies.DebugPIC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "Master Interrupt Offset changed from " + mMasterInterruptOffset.ToString("X4") + " to " + ((byte)(Value & 0xf8)).ToString("x"));
                        mMasterInterruptOffset = (byte)(Value & 0xf8);
                        mMasterInitCommandExpected = 3;
                    }
                    else if (mMasterInitCommandExpected == 3)
                    {
                        if (mMasterRequires4 == 1)
                            mMasterInitCommandExpected = 4;
                        else
                            mMasterInDeviceInit = false;
                    }
                    else if (mMasterInitCommandExpected == 4)
                    {
                        //Do something with EOI not sure what yet
                        //FROM BIOS CODE: // In autoeoi mode don't set the isr bit.
                        mMasterInDeviceInit = false;
                    }
                    else if (++mMasterInitCommandExpected>=5)
                        mMasterInDeviceInit=false;
                    break;
                case ePICMode.Slave:
                    if (mSlaveInitCommandExpected == 2)
                    {
                        if (mParent.mSystem.Debuggies.DebugPIC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "Slave Interrupt Offset changed from " + mSlaveInterruptOffset.ToString("X4") + " to " + ((byte)(Value & 0xf8)).ToString("x"));
                        mSlaveInterruptOffset = (byte)(Value & 0xf8);
                        mSlaveInitCommandExpected = 3;
                    }
                    else if (mSlaveInitCommandExpected == 3)
                    {
                        if (mSlaveRequires4 == 1)
                            mSlaveInitCommandExpected = 4;
                        else
                            mSlaveInDeviceInit = false;
                    }
                    else if (mSlaveInitCommandExpected == 4)
                    {
                        //Do something with EOI not sure what yet
                        mSlaveInDeviceInit = false;
                    }
                    else if (++mSlaveInitCommandExpected >= 5)
                        mSlaveInDeviceInit = false;
                    break;
            }


        }
        public byte PassIRQVectorToCPU()
        {
            byte ActualIRQNum=0;
            //ServicePic(ePICMode.Master);
            //ServicePic(ePICMode.Slave);
            //TODO: Locking in MASTER only for now, need to update logic so we can pass slave IRQs as well!!!!
            try
            {
                lock (this)
                {
                    mMasterIRRFound = false;
                    mSlaveIRRFound = false;
                    byte temp = HighestPriorityRequestedIRQ(ePICMode.Slave);
                    if (temp > 0)
                        ActualIRQNum = GetIRQNumForSetBit(temp,ePICMode.Slave);
                    else
                            ActualIRQNum = GetIRQNumForSetBit(HighestPriorityRequestedIRQ(ePICMode.Master), ePICMode.Master);
                        mMasterIRRFound = false;
                        mSlaveIRRFound = false;
                }
            }
            catch (Exception exc)
            {
//#if !RELEASE
//                Debugger.Break();
//#endif
            }
            byte Vector=0;
            if (ActualIRQNum>7)
                Vector = (byte)(ActualIRQNum - 8 + mSlaveInterruptOffset);
            else
                Vector = (byte)(ActualIRQNum + mMasterInterruptOffset);
            if (mParent.mSystem.Debuggies.DebugPIC && ActualIRQNum>0)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "Passing CPU Vector of " + Vector.ToString("x").PadLeft(2,'0') + " so that it can service IRQ # " + ActualIRQNum);
            return (Vector);
        }
        private byte GetIRQNumForSetBit(byte InVal, ePICMode WhichPic)
        {
            byte lRetVal = 0xff;
            for (byte c = 0; c < 8; c++)
                if (Math.Pow(2,c) == InVal)
                    lRetVal = c;
            if (WhichPic == ePICMode.Slave & lRetVal != 0xff)
                lRetVal += 8;
            if (lRetVal == 0xff)
                lRetVal = 0;
            return lRetVal;
        }
        #endregion

        #region cDevice Interface Related Methods
        public override void InitDevice()
        {
            mMasterIMR = mSlaveIMR = 0xff;
            mMasterISR = mSlaveISR = mMasterIRR = mSlaveIRR = 0x00;

            mIOHandlers = new sIOHandler[4];
            mIOHandlers[0].Device = this; mIOHandlers[0].PortNum = MASTER_COMMAND_PORT; mIOHandlers[0].Direction = ePortDirection.IO_InOut;
            mIOHandlers[1].Device = this; mIOHandlers[1].PortNum = MASTER_IMR_PORT; mIOHandlers[1].Direction = ePortDirection.IO_InOut;
            mIOHandlers[2].Device = this; mIOHandlers[2].PortNum = SLAVE_COMMAND_PORT; mIOHandlers[2].Direction = ePortDirection.IO_InOut;
            mIOHandlers[3].Device = this; mIOHandlers[3].PortNum = SLAVE_IMR_PORT; mIOHandlers[3].Direction = ePortDirection.IO_InOut;
            base.InitDevice();
        }
        public override void ResetDevice()
        {
            mMasterIMR = mSlaveIMR = 0xff;
            mMasterISR = mSlaveISR = mMasterIRR = mSlaveIRR = 0x00;
        }
        public override void HandleIO(sPortValue IO, ePortDirection Direction)
        {
            switch (Direction)
            {
                case ePortDirection.IO_In:
                    Handle_IN(IO, Direction);
                    break;
                case ePortDirection.IO_Out:
                    Handle_OUT(IO, Direction);
                    break;
            }
        }
        public void Handle_IN(sPortValue IO, ePortDirection Direction)
        {
            switch (IO.Portnum.mDWord)
            {
                //Send Master IMR
                case MASTER_IMR_PORT:
                    mParent.mProc.ports.mPorts[MASTER_IMR_PORT] = mMasterIMR;
                    break;
                //Send Slave IMR
                case SLAVE_IMR_PORT:
                    mParent.mProc.ports.mPorts[SLAVE_IMR_PORT] = mMasterIMR;
                    break;
                //A command was sent OUT to port 20, so we return the value here
                case MASTER_COMMAND_PORT:
                    mParent.mProc.ports.mPorts[MASTER_COMMAND_PORT] = mNextCommandPortValue;
                    break;
                //A command was sent OUT to port A0, so we return the value here
                case SLAVE_COMMAND_PORT:
                    mParent.mProc.ports.mPorts[SLAVE_COMMAND_PORT] = mNextCommandPortValue;
                    break;

            }
        }
        public void Handle_OUT(sPortValue IO, ePortDirection Direction)
        {
            DWord Port = IO.Portnum.mDWord;
            switch (Port)
            {
                //SetC Master Interrupt Mask Register
                case MASTER_IMR_PORT:
                    if (mMasterInDeviceInit)
                        ProcessInitCommand(ePICMode.Master, IO.Value.mByte);
                    else
                    {
                        mMasterIMR = IO.Value.mByte;
                        if (mParent.mSystem.Debuggies.DebugPIC)
                        {
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "Master IMR set to " + mMasterIMR.ToString("X2"));
                        }
                    }
                    break;

                //SetC Slave Interrupt Mask Register
                case SLAVE_IMR_PORT:
                    if (mSlaveInDeviceInit)
                        ProcessInitCommand(ePICMode.Slave, IO.Value.mByte);
                    else
                    {
                        mSlaveIMR = IO.Value.mByte;
                        if (mParent.mSystem.Debuggies.DebugPIC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "Slave IMR set to " + mSlaveIMR.ToString("X2"));
                    }
                    break;

                case MASTER_COMMAND_PORT:
                case SLAVE_COMMAND_PORT:
                    if ( (IO.Value.mByte & 0x10) != 0)
                        if (Port == MASTER_COMMAND_PORT)
                            EnterInitialization(ePICMode.Master, IO.Value.mByte);
                        else
                            EnterInitialization(ePICMode.Slave, IO.Value.mByte);
                    
                    if (IO.Value.mByte == 0x20)
                    {
                        //Interrupt Service Routine has ended and is signaling EOI
                        //I believe XOR will clear just the (highest) bit we want but we need to double check
                    //System.Diagnostics.Debugger.Break();
                        //Clear (Master) ISR for highest active IRQ (since it should be the active one)
                        if (Port == MASTER_COMMAND_PORT)
                        {
                            if (mParent.mSystem.Debuggies.DebugPIC)
                            {
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "Last (Master) ISR Routine has signalled EOI, IRR = " + mMasterIRR.ToString("X2"));
                            }
                            mMasterIRR ^= HighestPriorityActiveIRQ(ePICMode.Master);
                            mMasterISR ^= HighestPriorityActiveIRQ(ePICMode.Master);
                            return;
                        }
                        else
                        //Clear (Slave) ISR for highest active IRQ (since it should be the active one)
                        {
                            if (mParent.mSystem.Debuggies.DebugPIC)
                            {
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "Last (Slave) ISR Routine has signalled EOI, IRR = " + mMasterIRR.ToString("X2"));
                            }
                            if (HighestPriorityActiveIRQ(ePICMode.Slave) == 0)
                                mSlaveIRR = 0;
                            else
                                mSlaveIRR ^= HighestPriorityActiveIRQ(ePICMode.Slave);
                            mSlaveISR ^= HighestPriorityActiveIRQ(ePICMode.Slave);
                            return;
                        }
                    }
                    //Master ISR has been requested, put it in output buffer for next OUT
                    else if (Port == MASTER_COMMAND_PORT & IO.Value.mByte == 0x0B)
                        mNextCommandPortValue = mMasterISR;
                    //Slave ISR has been requested, put it in output buffer for next OUT
                    else if (Port == SLAVE_COMMAND_PORT & IO.Value.mByte == 0x0B)
                        mNextCommandPortValue = mSlaveISR;
                    //Master IRR has been requested, put it in output buffer for next OUT
                    else if (Port == MASTER_COMMAND_PORT & IO.Value.mByte == 0x0A)
                        mNextCommandPortValue = mMasterIRR;
                    //Slave IRR has been requested, put it in output buffer for next OUT
                    else if (Port == SLAVE_COMMAND_PORT & IO.Value.mByte == 0x0A)
                        mNextCommandPortValue = mSlaveIRR;
                    break;
            }
        }
        public override void DeviceThread()
        {
            while (1 == 1)
            {
                Thread.Sleep(1);
                if (mShutdownRequested)
                    break;
                ServicePic(ePICMode.Master);
                ServicePic(ePICMode.Slave);
            }
        }
        #endregion

    }
}
