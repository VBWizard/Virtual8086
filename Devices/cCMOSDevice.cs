using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;

namespace VirtualProcessor.Devices
{
    public class cCMOSDevice : cDevice, iDevice
    {

        #region Private Variables & Constants
        public const Word INDEX_PORT = 0x70, DATA_PORT = 0x71, CMOS_SIZE = 128;
        
        FileStream mCMOSFile;
        //TODO: Making protected incase other classes need to review.  If this turns out not to be the case (ports used only to access) then change to private
        protected byte[] mCMOS = new byte[CMOS_SIZE];
        int periodic_timer_index;
        int periodic_interval_usec;
        int one_second_timer_index;
        DateTime timeval;
        byte cmos_mem_address;
        Boolean mOneSecondTimerActive = false, mPeriodicTimerActive = false;
        public bool mPauseCMOSForUpdate = false;

        #endregion

        public cCMOSDevice(cDeviceBlock DevBlock)
        {
            mParent = DevBlock;
            mDeviceId = "0D89E128-F7E6-11DE-B87D-643756D89593";
            mDeviceClass = eDeviceClass.CMOS;
            //mName = "RAM/RTC Device";
            mName = "CMOS";
        }

        #region CMOS Methods

        void UpdateClock()
        {
            DateTime lNow = DateTime.Now;
            //Update values in CMOS
            //SetC second 
            mCMOS[0x00] = (byte)((lNow.Second / 10) << 4 | (lNow.Second % 10));
            //SetC minute 
            mCMOS[0x02] = (byte)((lNow.Minute / 10) << 4 | (lNow.Minute % 10));
            //SetC Hour 
            mCMOS[0x04] = (byte)((lNow.Hour / 10) << 4 | (lNow.Hour % 10));
            //SetC day of week (1-7) 
            mCMOS[0x06] = (byte)(lNow.DayOfWeek + 1);
            //SetC day of month 
            mCMOS[0x07] = (byte)lNow.Day;
            //SetC month
            mCMOS[0x08] = (byte)((lNow.Month / 10) << 4 | (lNow.Month % 10));
            //SetC Year
            mCMOS[0x09] = (byte)(((lNow.Year%100) / 10) << 4 | ((lNow.Year%100) % 10));
            //SetC Century
            mCMOS[0x32] = (byte)((((lNow.Year / 100) + 19) / 10) << 4 | (((lNow.Year / 100) + 19) % 10));
            mCMOSFile.Close();
            mCMOSFile = new FileStream(mParent.mSystem.mCMOSPathFile, FileMode.Open, FileAccess.ReadWrite);
            mCMOSFile.Write(mCMOS, 0, CMOS_SIZE);
            //mParent.mSystem.PrintDebugMsg(eDebuggieNames.CMOS, "Wrote time update ... array = " + mCMOS.Count() + ", CMOS_SIZE = " + CMOS_SIZE + ", cmos.bin size=");
        }

        void CRA_Change()
        {
            int nibble;

            // Periodic Interrupt timer
            nibble = (mCMOS[0x0a] & 0x0f);
            if (nibble == 0)
            {
                // No Periodic Interrupt Rate when 0, deactivate timer
                //bx_pc_system.deactivate_timer(periodic_timer_index);
                mPeriodicTimerActive = false;
                periodic_interval_usec = -1; // max value
            }
            else
            {
                // values 0001b and 0010b are the same as 1000b and 1001b
                if (nibble <= 2)
                    nibble += 7;
                periodic_interval_usec = (int)(1000000.0 / (32768.0 / (1 << (nibble - 1))));

                 //if Periodic Interrupt Enable bit set, activate timer
                if ((mCMOS[0x0b] & 0x40) > 0)
                    mPeriodicTimerActive = true;
                else
                    mPeriodicTimerActive = false;
            }
        }

        void Checksum_CMOS()
        {
            UInt16 sum = 0;
            for (int i = 0x10; i <= 0x2d; i++)
                sum += mCMOS[i];
            byte Hi = 0, Lo = 0;
            Hi = (byte)((sum >> 8) & 0xff);
            Lo = (byte)((sum & 0xff));
            mCMOS[0x2e] = Hi;
            mCMOS[0x2f] = Lo;
            
        }
        #endregion

        #region cDevice Interface Related Methods
        public override void InitDevice()
        {
            timeval = DateTime.Now;
            mCMOSFile = new FileStream(mParent.mSystem.mCMOSPathFile, FileMode.Open, FileAccess.ReadWrite);
            mCMOSFile.Read(mCMOS, 0, CMOS_SIZE);
            UpdateClock();
            mPeriodicTimerActive = false;
            if (mParent.mSystem.Debuggies.DebugCMOS)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.CMOS, "Successfully initialized CMOS from the image file " + mParent.mSystem.mCMOSPathFile);
            
            //SetC up IO handlers and PIC Registration and then call parent InitDevice which will register IO handlers
            mIOHandlers = new sIOHandler[2];
            mIOHandlers[0].Device = this; mIOHandlers[0].PortNum = INDEX_PORT; mIOHandlers[0].Direction = eDataDirection.IO_Out;
            mIOHandlers[1].Device = this; mIOHandlers[1].PortNum = DATA_PORT; mIOHandlers[1].Direction = eDataDirection.IO_InOut;
            mParent.mPIC.RegisterIRQ(this, 8);
            base.InitDevice();
            mOneSecondTimerActive = true;
            DeviceThreadSleep = 1000;
        }
        public override void ResetDevice()
        {
            mCMOS[0x0b] &= 0x8f;
            mCMOS[0x0c] = 0;
            CRA_Change();
            mOneSecondTimerActive = true;
            //??
            mPeriodicTimerActive = false;
        }

        public override void RequestShutdown()
        {
            base.RequestShutdown();
            do { Thread.Sleep(100); } while (DeviceThreadActive);
            if (mCMOSFile != null)
            {
                mCMOSFile.Close();
                mCMOSFile = null;
            }
        }

        public override void HandleIO(sPortValue IO, eDataDirection Direction)
        {
            switch (Direction)
            {
                case eDataDirection.IO_In:
                    Handle_IN(IO);
                    break;
                case eDataDirection.IO_Out:
                    Handle_OUT(IO);
                    break;
            }
        }
        public void Handle_IN(sPortValue IO)
        {
            byte Ret8 = 0;

            while (mPauseCMOSForUpdate)
                Thread.Sleep(1);
            //NOTE: No switch needed, we won't see anything IN on port 70 because we defined the IO handler as OUT only
            if (cmos_mem_address >= CMOS_SIZE)
                mParent.mSystem.HandleException(this, new Exception("unsupported cmos io read, register " + cmos_mem_address.ToString("X2") + " = " + (Word)IO.Value));
            Ret8 = mCMOS[cmos_mem_address];
            // all bits of Register C are cleared after a read occurs.
            if (cmos_mem_address == 0x0c)
                mCMOS[0x0c] = 0x00;
            mParent.mProc.ports.mPorts[DATA_PORT] = Ret8;
            if (mParent.mSystem.Debuggies.DebugCMOS)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.CMOS, "read of address " + cmos_mem_address.ToString("X2") +
                    ", reply: " + Ret8.ToString("X2"));
        }
        public void Handle_OUT(sPortValue IO)
        {
            while (mPauseCMOSForUpdate)
                Thread.Sleep(1);
            switch (IO.Portnum)
            {
                case INDEX_PORT: //0x70
                    cmos_mem_address = (byte)(IO.Value & 0x7F);
                    break;
                case DATA_PORT: //0x71
                    if (cmos_mem_address >= CMOS_SIZE)
                        mParent.mSystem.HandleException(this, new Exception("unsupported cmos io write, register " + cmos_mem_address.ToString("X2") + " = " + (Word)IO.Value));
                    switch (cmos_mem_address)
                    {
                        case 0x00: // seconds
                        case 0x01: // seconds alarm
                        case 0x02: // minutes
                        case 0x03: // minutes alarm
                        case 0x04: // hours
                        case 0x05: // hours alarm
                        case 0x06: // day of the week
                        case 0x07: // day of the month
                        case 0x08: // month
                        case 0x09: // year
                            mCMOS[cmos_mem_address] = (byte)IO.Value;
                            return;
                        case 0x0A:  // Control Register A
                            if ((((byte)IO.Value >> 4) & 0x07) != 0x02)
                            {
                                mCMOS[cmos_mem_address] = (byte)(IO.Value & 0x7f);
                                CRA_Change();
                            }
                            return;
                        case 0x0B: //Control Register B
                            if ((IO.Value & 0x04) > 0)
                                mParent.mSystem.HandleException(this, new Exception("write status reg B, binary format enabled."));
                            if ((IO.Value & 0x02) == 0)
                                mParent.mSystem.HandleException(this, new Exception("write status reg B, 12 hour mode enabled."));

                            IO.Value &= 0xf7; // bit3 always 0
                            // Note: setting bit 7 clears bit 4
                            if ((IO.Value & 0x80) > 0)
                                IO.Value &= 0xef;

                            byte prev_CRB;
                            prev_CRB = mCMOS[0x0b];
                            mCMOS[0x0b] = (byte)IO.Value;
                            if ((prev_CRB & 0x40) != (IO.Value & 0x40))
                            {
                                // Periodic Interrupt Enabled changed
                                if ((prev_CRB & 0x40) > 0)
                                {
                                    // transition from 1 to 0, deactivate timer
                                    //bx_pc_system.deactivate_timer(s.periodic_timer_index);
                                    mPeriodicTimerActive = false;
                                }
                                else
                                {
                                    // transition from 0 to 1
                                    // if rate select is not 0, activate timer
                                    if ((mCMOS[0x0a] & 0x0f) != 0)
                                    {
                                        //bx_pc_system.activate_timer(s.periodic_timer_index, s.periodic_interval_usec, 1);
                                        mPeriodicTimerActive = true;
                                    }
                                }
                            }
                            return;
                        case 0x0c: // Control Register C
                        case 0x0d: // Control Register D
                            mParent.mSystem.HandleException(this, new Exception("write to control register " + cmos_mem_address + " (read-only)"));
                            break;
                        case 0x0e: // diagnostic status
                            //Don't do anything ... diagnostic address
                            break;
                        case 0x0f: // shutdown status
                            switch (IO.Value)
                            {
                                case 0x00: /* proceed with normal POST (soft reset) */
                                    if (mParent.mSystem.Debuggies.DebugCMOS)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.CMOS, "ModRegRM_Reg 0F set to 0: shutdown action = normal POST");
                                    break;
                                case 0x02: /* shutdown after memory test */
                                    if (mParent.mSystem.Debuggies.DebugCMOS)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.CMOS, "ModRegRM_Reg 0Fh: request to change shutdown action to shutdown after memory test");
                                    break;
                                case 0x03:
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.CMOS, "ModRegRM_Reg 0Fh(03) : Shutdown after memory test !");
                                    break;
                                case 0x04: /* jump to disk bootstrap routine */
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.CMOS, "ModRegRM_Reg 0Fh: request to change shutdown action to jump to disk bootstrap routine.");
                                    break;
                                case 0x06:
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.CMOS, "ModRegRM_Reg 0Fh(06) : Shutdown after memory test !");
                                    break;
                                case 0x09: /* return to BIOS extended memory block move
                       (interrupt 15h, func 87h was in progress) */
                                    if (mParent.mSystem.Debuggies.DebugCMOS)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.CMOS, "ModRegRM_Reg 0Fh: request to change shutdown action to return to BIOS extended memory block move.");
                                    break;
                                case 0x0a: /* jump to DWORD pointer at 40:67 */
                                    if (mParent.mSystem.Debuggies.DebugCMOS)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.CMOS, "ModRegRM_Reg 0Fh: request to change shutdown action to jump to DWORD at 40:67");
                                    break;
                                default:
                                    mParent.mSystem.HandleException(this, new Exception("unsupported cmos io write to reg F, case " + (byte)IO.Value + "!"));
                                    break;
                            }
                            mCMOS[cmos_mem_address] = (byte)IO.Value;
                            break; //DATA_PORT
                    }
                    break;
                //Switch cmos_mem_address
            } //switch PortNum
        }
        public override void DeviceThread()
        {
            DeviceThreadActive = true;
            while (1 == 1)
            {
                Thread.Sleep(DeviceThreadSleep); //We want a 1 second update isntead of the norm
                if (mOneSecondTimerActive)
                    OneSecondTimerHandler();
                if (mPeriodicTimerActive)
                    PeriodicTimerHandler();
                if (mShutdownRequested)
                    break;
                if (mPauseCMOSForUpdate)
                {
                    mCMOSFile.Close();
                    while (mPauseCMOSForUpdate == true)
                    {
                        Thread.Sleep(10);
                    }
                    mCMOSFile = new FileStream(mParent.mSystem.mCMOSPathFile, FileMode.Open, FileAccess.ReadWrite);
                    mCMOSFile.Read(mCMOS, 0, CMOS_SIZE);

                }
            }
            DeviceThreadActive = false;
        }
        #endregion

        void OneSecondTimerHandler()
        {

            mCMOS[0x0a] |= 0x80;
            // update internal time/date buffer
            timeval = DateTime.Now;

            // Dont update CMOS user copy of time/date if CRB bit7 is 1
            // Nothing else do to
            if ((mCMOS[0x0b] & 0x80) > 0)
                return;

            UpdateClock();

            // if update interrupts are enabled, trip IRQ 8, and
            // update status register C
            if ((mCMOS[0x0b] & 0x10) > 0)
            {
                mCMOS[0x0c] |= 0x90; // Interrupt Request, Update Ended
                mParent.mPIC.RaiseIRQ(8);
            }

            // compare CMOS user copy of time/date to alarm time/date here
            if ((mCMOS[0x0b] & 0x20) > 0)
            {
                // Alarm interrupts enabled
                Boolean alarm_match = true;
                if ((mCMOS[0x01] & 0xc0) != 0xc0)
                {
                    // seconds alarm not in dont care mode
                    if (mCMOS[0x00] != mCMOS[0x01])
                        alarm_match = false;
                }
                if ((mCMOS[0x03] & 0xc0) != 0xc0)
                {
                    // minutes alarm not in dont care mode
                    if (mCMOS[0x02] != mCMOS[0x03])
                        alarm_match = false;
                }
                if ((mCMOS[0x05] & 0xc0) != 0xc0)
                {
                    // hours alarm not in dont care mode
                    if (mCMOS[0x04] != mCMOS[0x05])
                        alarm_match = false;
                }
                if (alarm_match)
                {
                    mCMOS[0x0c] |= 0xa0; // Interrupt Request, Alarm Int
                    mParent.mPIC.RaiseIRQ(8);
                }
            }
            mCMOS[0x0a] &= 0x7F;
        } //one_second_timer
        void PeriodicTimerHandler()
        {
            if ((mCMOS[0x0b] & 0x40) > 0)
            {
                mCMOS[0x0c] |= 0xc0;
                mParent.mPIC.RaiseIRQ(8);
            }

        }
    }
}
