using System;
using System.Linq;
using System.Text;
using System.Threading;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;
using bx_bool = System.Boolean;
using Bit64u = System.UInt64;
using Bit32u = System.UInt32;
using Bit16u = System.UInt16;
using Bit8u = System.Byte;

/* Taken from BOCHS 2.4.6
 * Emulator of an Intel 8254/82C54 Programmable Interval Timer.
 * Greg Alexander <yakovlev@usa.com>
 *
 *
 * Things I am unclear on (greg):
 * 1.)What happens if both the status and count registers are latched,
 *  but the first of the two count registers has already been read?
 *  I.E.:
 *   latch count 0 (16-bit)
 *   Read count 0 (read LSByte)
 *   READ_BACK status of count 0
 *   Read count 0 - do you get MSByte or status?
 *  This will be flagged as an error.
 * 2.)What happens when we latch the output in the middle of a 2-part
 *  unlatched read?
 * 3.)I assumed that programming a counter removes a latched status.
 * 4.)I implemented the 8254 description of mode 0, not the 82C54 one.
 * 5.)clock() calls represent a rising clock edge followed by a falling
 *  clock edge.
 * 6.)What happens when we trigger mode 1 in the middle of a 2-part
 *  write?
 */


namespace VirtualProcessor.Devices
{

    public class cPITNew : cDevice, iDevice
    {

        #region Private Variables & Constants
        const int MAX_COUNTER = 2;
        const int MAX_ADDRESS = 3;
        const int CONTROL_ADDRESS = 3;
        const int MAX_MODE = 5;
        //Important constant #defines:
        const UInt64 USEC_PER_SECOND = 1000;
        //1.193181MHz Clock
        const UInt64 TICKS_PER_SECOND = 1193181;
        public System.Timers.Timer[] mPICTimers = new System.Timers.Timer[3];

        internal s_timer s;
        internal UInt64 MasterUSEC = 0;
        #endregion

        public cPITNew(cDeviceBlock DevBlock)
        {
            mParent = DevBlock;
            mDeviceId = "fa7c7e0b-d9c3-4c3d-8bf9-8c882a022eaa";
            mDeviceClass = eDeviceClass.PIT;
            mName = "8254 PIT";
        }

        #region Device Methods
        int BX_MAX(int a, int b)
        {
            if (a > b)
                return a;
            return b;
        }
        int BX_MIN(int a, int b)
        {
            if (a > b)
                return b;
            return a;
        }
        //PIT tick to usec conversion functions:
        //Direct conversions:
        UInt64 TICKS_TO_MSEC(UInt64 a)
        {
            return (a / TimeSpan.TicksPerMillisecond);
        }
        UInt64 TICKS_TO_USEC(UInt64 a)
        {
            return (((a) * USEC_PER_SECOND) / TICKS_PER_SECOND);
        }
        UInt64 USEC_TO_TICKS(UInt64 a)
        {
            return (((a) * TICKS_PER_SECOND) / USEC_PER_SECOND);
        }

        void handle_timer(object source, System.Timers.ElapsedEventArgs e)
        {
            //Monitor.Enter(this);
            //try
            //{
            mPICTimers[0].Stop();
            MasterUSEC += (ulong)mPICTimers[0].Interval;
                Bit64u my_time_usec = MasterUSEC; // bx_virt_timer.time_usec();
                Bit64u time_passed = my_time_usec - s.last_usec;
                Bit32u time_passed32 = (Bit32u)time_passed;

                //BX_DEBUG(("entering timer handler"));

                if (time_passed32 > 0)
                {
                    periodic(time_passed32);
                }
                s.last_usec = s.last_usec + time_passed;
                if (time_passed > 0 || (s.last_next_event_time != s.timer.get_next_event_time()))
                {
                    //BX_DEBUG(("RESETting timer"));
                    //bx_virt_timer.deactivate_timer(s.timer_handle[0]);
                    //BX_DEBUG(("deactivated timer"));
                    if (s.timer.get_next_event_time() > 0)
                    {
                        //  bx_virt_timer.activate_timer(s.timer_handle[0],
                        //                               (Bit32u)BX_MAX(1,TICKS_TO_USEC(s.timer.get_next_event_time())),
                        //                               0);
                        //BX_DEBUG(("activated timer"));
                        mPICTimers[0].AutoReset = false;
                        mPICTimers[0].Interval = BX_MAX(1, (int)TICKS_TO_MSEC(s.timer.get_next_event_time())) + mParent.mProc.mTimerTickMSec;
                        if (mParent.mSystem.Debuggies.DebugPIT)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIT, "Setting timer in handle_timer (#1) for " + mPICTimers[0].Interval + " ms.");
                        mPICTimers[0].Start();
                    }
                    s.last_next_event_time = s.timer.get_next_event_time();
                    //BX_DEBUG(("s.last_usec="FMT_LL"d",s.last_usec));
                    //BX_DEBUG(("s.timer_id=%d",s.timer_handle[0]));
                    //BX_DEBUG(("s.timer.get_next_event_time=%x",s.timer.get_next_event_time()));
                    //BX_DEBUG(("s.last_next_event_time=%d",s.last_next_event_time));
                }
            //}
            //finally
            //{
            //    Monitor.Exit(this);
            //}
        }
        bool periodic(Bit32u usec_delta)
        {
            Bit32u ticks_delta = 0;

            s.total_usec += usec_delta;
            ticks_delta = (Bit32u)((USEC_TO_TICKS((s.total_usec))) - s.total_ticks);
            s.total_ticks += ticks_delta;

            while ((s.total_ticks >= TICKS_PER_SECOND) && (s.total_usec >= USEC_PER_SECOND))
            {
                s.total_ticks -= TICKS_PER_SECOND;
                s.total_usec -= USEC_PER_SECOND;
            }

            while (ticks_delta > 0)
            {
                Bit32u maxchange = s.timer.get_next_event_time();
                Bit32u timedelta = maxchange;
                if ((maxchange == 0) || (maxchange > ticks_delta))
                {
                    timedelta = ticks_delta;
                }
                s.timer.clock_all(timedelta);
                ticks_delta -= timedelta;
            }

            return false;
        }
        Bit16u get_timer(int Timer)
        {
            return s.timer.get_inlatch(Timer);
        }
        #endregion

        #region cDevice Interface Related Methods
        public override void InitDevice()
        {
            //Set up IO handlers and PIC Registration and then call parent InitDevice which will register IO handlers
            this.mName = "PIT";
            mIOHandlers = new sIOHandler[5];
            mIOHandlers[0].Device = this; mIOHandlers[0].PortNum = 0x40; mIOHandlers[0].Direction = eDataDirection.IO_InOut;
            mIOHandlers[1].Device = this; mIOHandlers[1].PortNum = 0x41; mIOHandlers[1].Direction = eDataDirection.IO_InOut;
            mIOHandlers[2].Device = this; mIOHandlers[2].PortNum = 0x42; mIOHandlers[2].Direction = eDataDirection.IO_InOut;
            mIOHandlers[3].Device = this; mIOHandlers[3].PortNum = 0x43; mIOHandlers[3].Direction = eDataDirection.IO_InOut;
            //mIOHandlers[4].Device = this; mIOHandlers[4].PortNum = 0x61; mIOHandlers[4].Direction = ePortDirection.IO_InOut;
            mParent.mPIC.RegisterIRQ(this, 0);
            base.InitDevice();

            s.speaker_data_on = 0;
            s.refresh_clock_div2 = false;
            s.timer.Init(mParent);

            s.timer.assigned_irq = new byte[3];
            s.timer.assigned_irq[0] = 0;

            //s.timer.init(); 
            //s.timer.set_OUT_handler(0, irq_handler);

            Bit64u my_time_usec = (ulong)DateTime.Now.Ticks;
            if (1 == 0)
            {
            //BX_DEBUG(("RESETting timer."));
            //bx_virt_timer.deactivate_timer(s.timer_handle[0]);
            //BX_DEBUG(("deactivated timer."));
            mPICTimers[0] = new System.Timers.Timer();
            mPICTimers[0].Elapsed += handle_timer;
            mPICTimers[1] = new System.Timers.Timer();
            mPICTimers[1].Elapsed += handle_timer;
            mPICTimers[2] = new System.Timers.Timer();
            mPICTimers[2].Elapsed += handle_timer;

            mPICTimers[0].Stop();
            mPICTimers[0].AutoReset = false;
            mPICTimers[0].Interval = BX_MAX(1, (int)TICKS_TO_MSEC(s.timer.get_next_event_time())) + mParent.mProc.mTimerTickMSec;
            if (mParent.mSystem.Debuggies.DebugPIT)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIT, "Setting timer in InitDevice for " + mPICTimers[0].Interval + " ms.");
            mPICTimers[0].Start();

            s.last_next_event_time = s.timer.get_next_event_time();
            s.last_usec = 0; //my_time_usec;

            s.total_ticks = 0;
            s.total_usec = 0;

            //BX_DEBUG(("finished init"));
            //BX_DEBUG(("s.last_usec="FMT_LL"d",s.last_usec));
            //BX_DEBUG(("s.timer_id=%d",s.timer_handle[0]));
            //BX_DEBUG(("s.timer.get_next_event_time=%d",s.timer.get_next_event_time()));
            //BX_DEBUG(("s.last_next_event_time=%d",s.last_next_event_time));
            base.InitDevice();
            DeviceThreadSleep = this.mParent.mProc.mTimerTickMSec;

        }

        public override void ResetDevice()
        {
            s.timer.reset(0);
        }
        public override void RequestShutdown()
        {
            base.RequestShutdown();
        }
        public override void HandleIO(sPortValue IO, eDataDirection Direction)
        {
            if (mParent.mSystem.Debuggies.DebugPIT)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIT, Direction + " on port " + IO.Portnum.ToString("X4") + " with value " + IO.Value.ToString("X4"));
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
            Bit8u value = 0;

            Bit64u my_time_usec = 0; //bx_virt_timer.time_usec();

            switch (IO.Portnum)
            {

                case 0x40: /* timer 0 - system ticks */
                    value = s.timer.read(0);
                    break;
                case 0x41: /* timer 1 read */
                    value = s.timer.read(1);
                    break;
                case 0x42: /* timer 2 read */
                    value = s.timer.read(2);
                    break;
                case 0x43: /* timer 1 read */
                    value = s.timer.read(3);
                    break;

                case 0x61:
                    /* AT, port 61h */
                    s.refresh_clock_div2 = (((my_time_usec / 15) & 1) == 1);
                    value = (byte)((s.timer.read_OUT(2) ? 1 : 0 << 5) |
                        (s.refresh_clock_div2 ? 1 : 0 << 4) |
                            (s.speaker_data_on << 1) |
                            (s.timer.read_GATE(2) ? 1 : 0));
                    break;

                default:
                    throw new Exception("unsupported io read from port 0x" + IO.Portnum.ToString("X4"));
            }

            //  BX_DEBUG(("read from port 0x%04x, value = 0x%02x", address, value));
            mParent.mProc.ports.mPorts[IO.Portnum] = value;
        }
        public void Handle_OUT(sPortValue IO)
        {
            Bit8u value;
            Bit64u my_time_usec = MasterUSEC; //bx_virt_timer.time_usec();
            Bit64u time_passed = my_time_usec - s.last_usec;
            Bit32u time_passed32 = (Bit32u)time_passed;

            if (time_passed32 > 0)
            {
                //periodic(time_passed32);
            }
            s.last_usec = s.last_usec + time_passed;

            value = (byte)IO.Value;

            //BX_DEBUG(("write to port 0x%04x, value = 0x%02x", address, value));

            switch (IO.Portnum)
            {
                case 0x40: /* timer 0: write count register */
                    if (mParent.mSystem.Debuggies.DebugPIT)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIT, "Timer 0: New write count register value=" + value.ToString("X2"));
                    s.timer.write(0, value);
                    break;

                case 0x41: /* timer 1: write count register */
                    if (mParent.mSystem.Debuggies.DebugPIT)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIT, "Timer 1: New write count register value=" + value.ToString("X2"));
                    s.timer.write(1, value);
                    break;

                case 0x42: /* timer 2: write count register */
                    if (mParent.mSystem.Debuggies.DebugPIT)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIT, "Timer 2: New write count register value=" + value.ToString("X2"));
                    s.timer.write(2, value);
                    break;

                case 0x43: /* timer 0-2 mode control */
                    s.timer.write(3, value);
                    if (mParent.mSystem.Debuggies.DebugPIT)
                    {
                        StringBuilder st = new StringBuilder("");
                        st.AppendFormat("New Control: Counter={0}, Binary={1}, Mode={2}, Latch={3}", ((value & 0xC0) >> 6),
                            ((value & 0x1) == 0x1).ToString(), ((value & 0xE) >> 1), ((value & 0x30) >> 4));
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIT, st.ToString());
                    }
                    break;

                case 0x61:
                    //s.speaker_data_on = (byte)((value >> 1) & 0x01);
                    //if (s.speaker_data_on > 0) 
                    //{
                    //  DEV_speaker_beep_on((float)(1193180.0 / get_timer(2)));
                    //} else 
                    //{
                    //  DEV_speaker_beep_off();
                    //}
                    /* ??? only on AT+ */
                    s.timer.set_GATE(2, (value & 0x01) == 1);
                    break;

                default:
                    throw new Exception("unsupported io write to port " + IO.Portnum.ToString("X4") + " = " + IO.Value.ToString("X2"));
            }

            if (time_passed > 0 || (s.last_next_event_time != s.timer.get_next_event_time()))
            {
                mPICTimers[0].Stop();
                //BX_DEBUG(("RESETting timer"));
                //bx_virt_timer.deactivate_timer(s.timer_handle[0]);
                //BX_DEBUG(("deactivated timer"));
                if (s.timer.get_next_event_time() > 0)
                {
                    //  bx_virt_timer.activate_timer(s.timer_handle[0],
                    //                               (Bit32u)BX_MAX(1,TICKS_TO_USEC(s.timer.get_next_event_time())),
                    //                               0);
                    //BX_DEBUG(("activated timer"));
                    mPICTimers[0].AutoReset = false;
                    mPICTimers[0].Interval = BX_MAX(1, (int)TICKS_TO_MSEC(s.timer.get_next_event_time())) + mParent.mProc.mTimerTickMSec;
                    if (mParent.mSystem.Debuggies.DebugPIT)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIT, "Setting timer in HANDLE_OUT (#2) for " + mPICTimers[0].Interval + " ms.");
                    mPICTimers[0].Start();
                }
            }
            s.last_next_event_time = s.timer.get_next_event_time();
            //BX_DEBUG(("s.last_usec="FMT_LL"d",s.last_usec));
            //BX_DEBUG(("s.timer_id=%d",s.timer_handle[0]));
            //BX_DEBUG(("s.timer.get_next_event_time=%x",s.timer.get_next_event_time()));
            //BX_DEBUG(("s.last_next_event_time=%d",s.last_next_event_time));
        }
        public override void DeviceThread()
        {
            mPICTimers[0].Stop();
            return;
            //            DeviceThreadActive = true;
            //            while (1 == 1)
            //            {
            //#if RELEASE
            //                Thread.Sleep(DeviceThreadSleep);
            //                //MasterUSEC += 55556; // USEC_PER_SECOND / 18;
            //                if (mShutdownRequested || DeviceThreadSleep == 0)
            //                    break;
            //                MasterUSEC += (UInt64)((UInt64)TICKS_PER_SECOND / (UInt64)(1000 / (DeviceThreadSleep)));
            //                //1193181 / (1000/53) = 
            //#else
            //                Thread.Sleep(DeviceThreadSleep/*3*/);
            //                if (mShutdownRequested || DeviceThreadSleep == 0)
            //                    break;
            //                MasterUSEC += (UInt64)((UInt64)TICKS_PER_SECOND / (UInt64)(1000 / (DeviceThreadSleep * 2)));
            //#endif
            //                handle_timer();
            //            }
            //            DeviceThreadActive = false;
        }
        #endregion
    }

    //struct s_timer
    //{
    //    public sPIT timer;
    //    public Bit8u speaker_data_on;
    //    public bx_bool refresh_clock_div2;
    //    public Bit64u last_usec;
    //    public Bit32u last_next_event_time;
    //    public Bit64u total_ticks;
    //    public Bit64u total_usec;
    //    //int[3]  timer_handle;
    //}
    //struct sPIT
    //{
    //    const int MAX_COUNTER = 2;
    //    const int MAX_ADDRESS = 3;
    //    const int CONTROL_ADDRESS = 3;
    //    const int MAX_MODE = 5;
    //    //Important constant #defines:
    //    const UInt64 USEC_PER_SECOND = 1000000;
    //    //1.193181MHz Clock
    //    const UInt64 TICKS_PER_SECOND = 1193181;
    //    counter_type[] counter;
    //    public byte[] assigned_irq;
    //    Bit8u controlword;
    //    cDeviceBlock mParent;
    //    public void Init(cDeviceBlock Parent)
    //    {
    //        mParent = Parent;
    //        counter = new counter_type[3];
    //        for (int i = 0; i < 3; i++)
    //        {
    //            //BX_DEBUG(("Setting read_state to LSB"));
    //            counter[i].read_state = rw_status.LSByte;
    //            counter[i].write_state = rw_status.LSByte;
    //            counter[i].GATE = true;
    //            counter[i].OUTpin = true;
    //            counter[i].triggerGATE = false;
    //            counter[i].mode = 4;
    //            counter[i].first_pass = false;
    //            counter[i].bcd_mode = false;
    //            counter[i].count = 0;
    //            counter[i].count_binary = 0;
    //            counter[i].state_bit_1 = false;
    //            counter[i].state_bit_2 = false;
    //            counter[i].null_count = false;
    //            counter[i].rw_mode = 1;
    //            counter[i].count_written = true;
    //            counter[i].count_LSB_latched = false;
    //            counter[i].count_MSB_latched = false;
    //            counter[i].status_latched = false;
    //            counter[i].next_change_time = 0;
    //            //counter[i].out_handler = null;
    //        }
    //    }
    //    public void latch_counter(ref counter_type thisctr)
    //    {
    //        if (thisctr.count_LSB_latched || thisctr.count_MSB_latched)
    //        {
    //            //Do nothing because previous latch has not been read.;
    //        }
    //        else
    //        {
    //            switch (thisctr.read_state)
    //            {
    //                case rw_status.MSByte:
    //                    thisctr.outlatch = (Bit16u)(thisctr.count & 0xFFFF);
    //                    thisctr.count_MSB_latched = true;
    //                    break;
    //                case rw_status.LSByte:
    //                    thisctr.outlatch = (Bit16u)(thisctr.count & 0xFFFF);
    //                    thisctr.count_LSB_latched = true;
    //                    break;
    //                case rw_status.LSByte_multiple:
    //                    thisctr.outlatch = (Bit16u)(thisctr.count & 0xFFFF);
    //                    thisctr.count_LSB_latched = true;
    //                    thisctr.count_MSB_latched = true;
    //                    break;
    //                case rw_status.MSByte_multiple:
    //                    //I guess latching and resetting to LSB first makes sense;
    //                    //BX_DEBUG(("Setting read_state to LSB_mult"));
    //                    thisctr.read_state = rw_status.LSByte_multiple;
    //                    thisctr.outlatch = (Bit16u)(thisctr.count & 0xFFFF);
    //                    thisctr.count_LSB_latched = true;
    //                    thisctr.count_MSB_latched = true;
    //                    break;
    //                default:
    //                    throw new Exception("Unknown read mode found during latch command.");
    //            }
    //        }
    //    }
    //    public void set_OUT(ref counter_type thisctr, bx_bool data)
    //    {
    //        if (thisctr.OUTpin != data)
    //        {
    //            thisctr.OUTpin = data;
    //            //throw new Exception("Something's supposed to happen here!!!");
    //            //if (thisctr.out_handler != null) {
    //            //out_handler(data);
    //            irq_handler(data, 0);

    //            //}
    //        }
    //    }
    //    public void set_count(ref counter_type thisctr, Bit32u data)
    //    {
    //        thisctr.count = data & 0xFFFF;
    //        set_binary_to_count(ref thisctr);
    //    }
    //    public void set_count_to_binary(ref counter_type thisctr)
    //    {
    //        if (thisctr.bcd_mode)
    //        {
    //            thisctr.count =
    //              (((thisctr.count_binary / 1) % 10) << 0) |
    //              (((thisctr.count_binary / 10) % 10) << 4) |
    //              (((thisctr.count_binary / 100) % 10) << 8) |
    //              (((thisctr.count_binary / 1000) % 10) << 12);
    //        }
    //        else
    //        {
    //            thisctr.count = thisctr.count_binary;
    //        }
    //    }
    //    public void set_binary_to_count(ref counter_type thisctr)
    //    {
    //        if (thisctr.bcd_mode)
    //        {
    //            thisctr.count_binary =
    //              (1 * ((thisctr.count >> 0) & 0xF)) +
    //              (10 * ((thisctr.count >> 4) & 0xF)) +
    //              (100 * ((thisctr.count >> 8) & 0xF)) +
    //              (1000 * ((thisctr.count >> 12) & 0xF));
    //        }
    //        else
    //        {
    //            thisctr.count_binary = thisctr.count;
    //        }
    //    }
    //    public void decrement(ref counter_type thisctr)
    //    {
    //        if (thisctr.count == 0)
    //        {
    //            if (thisctr.bcd_mode)
    //            {
    //                thisctr.count = 0x9999;
    //                thisctr.count_binary = 9999;
    //            }
    //            else
    //            {
    //                thisctr.count = 0xFFFF;
    //                thisctr.count_binary = 0xFFFF;
    //            }
    //        }
    //        else
    //        {
    //            thisctr.count_binary--;
    //            set_count_to_binary(ref thisctr);
    //        }
    //    }
    //    public void reset(UInt16 type) { }
    //    public void decrement_multiple(ref counter_type thisctr, Bit32u cycles)
    //    {
    //        while (cycles > 0)
    //        {
    //            if (cycles <= thisctr.count_binary)
    //            {
    //                thisctr.count_binary -= cycles;
    //                cycles -= cycles;
    //                set_count_to_binary(ref thisctr);
    //            }
    //            else
    //            {
    //                cycles -= (thisctr.count_binary + 1);
    //                thisctr.count_binary -= thisctr.count_binary;
    //                set_count_to_binary(ref thisctr);
    //                decrement(ref thisctr);
    //            }
    //        }
    //    }
    //    public void clock_multiple(Bit8u cnum, Bit32u cycles)
    //    {
    //        if (cnum > MAX_COUNTER)
    //        {
    //            throw new Exception("Counter number too high in clock");
    //        }
    //        else
    //        {
    //            while (cycles > 0)
    //            {
    //                if (counter[cnum].next_change_time == 0)
    //                {
    //                    if (counter[cnum].count_written)
    //                    {
    //                        switch (counter[cnum].mode)
    //                        {
    //                            case 0:
    //                                if (counter[cnum].GATE && (counter[cnum].write_state != rw_status.MSByte_multiple))
    //                                {
    //                                    decrement_multiple(ref counter[cnum], cycles);
    //                                }
    //                                break;
    //                            case 1:
    //                                decrement_multiple(ref counter[cnum], cycles);
    //                                break;
    //                            case 2:
    //                                if (!counter[cnum].first_pass && counter[cnum].GATE)
    //                                {
    //                                    decrement_multiple(ref counter[cnum], cycles);
    //                                }
    //                                break;
    //                            case 3:
    //                                if (!counter[cnum].first_pass && counter[cnum].GATE)
    //                                {
    //                                    decrement_multiple(ref counter[cnum], 2 * cycles);
    //                                }
    //                                break;
    //                            case 4:
    //                                if (counter[cnum].GATE)
    //                                {
    //                                    decrement_multiple(ref counter[cnum], cycles);
    //                                }
    //                                break;
    //                            case 5:
    //                                decrement_multiple(ref counter[cnum], cycles);
    //                                break;
    //                            default:
    //                                break;
    //                        }
    //                    }
    //                    cycles -= cycles;
    //                }
    //                else
    //                {
    //                    switch (counter[cnum].mode)
    //                    {
    //                        case 0:
    //                        case 1:
    //                        case 2:
    //                        case 4:
    //                        case 5:
    //                            if (counter[cnum].next_change_time > cycles)
    //                            {
    //                                decrement_multiple(ref counter[cnum], cycles);
    //                                counter[cnum].next_change_time -= cycles;
    //                                cycles -= cycles;
    //                            }
    //                            else
    //                            {
    //                                decrement_multiple(ref counter[cnum], (counter[cnum].next_change_time - 1));
    //                                cycles -= counter[cnum].next_change_time;
    //                                clock(cnum);  /** THis is part of the path to IRQ 0 being triggered **/
    //                            }
    //                            break;
    //                        case 3:
    //                            if (counter[cnum].next_change_time > cycles)
    //                            {
    //                                decrement_multiple(ref counter[cnum], cycles * 2);
    //                                counter[cnum].next_change_time -= cycles;
    //                                cycles -= cycles;
    //                            }
    //                            else
    //                            {
    //                                decrement_multiple(ref counter[cnum], (counter[cnum].next_change_time - 1) * 2);
    //                                cycles -= counter[cnum].next_change_time;
    //                                clock(cnum);
    //                            }
    //                            break;
    //                        default:
    //                            cycles -= cycles;
    //                            break;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    public void clock(Bit8u cnum)
    //    {
    //        if (cnum > MAX_COUNTER)
    //        {
    //            throw new Exception("Counter number too high in clock");
    //        }
    //        else
    //        {
    //            switch (counter[cnum].mode)
    //            {
    //                case 0:
    //                    if (counter[cnum].count_written)
    //                    {
    //                        if (counter[cnum].null_count)
    //                        {
    //                            set_count(ref counter[cnum], counter[cnum].inlatch);
    //                            if (counter[cnum].GATE)
    //                            {
    //                                if (counter[cnum].count_binary == 0)
    //                                {
    //                                    counter[cnum].next_change_time = 1;
    //                                }
    //                                else
    //                                {
    //                                    counter[cnum].next_change_time = counter[cnum].count_binary & 0xFFFF;
    //                                }
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = 0;
    //                            }
    //                            counter[cnum].null_count = false;
    //                        }
    //                        else
    //                        {
    //                            if (counter[cnum].GATE && (counter[cnum].write_state != rw_status.MSByte_multiple))
    //                            {
    //                                decrement(ref counter[cnum]);
    //                                if (!counter[cnum].OUTpin)
    //                                {
    //                                    counter[cnum].next_change_time = counter[cnum].count_binary & 0xFFFF;
    //                                    if (counter[cnum].count == 0)
    //                                    {
    //                                        set_OUT(ref counter[cnum], true);
    //                                    }
    //                                }
    //                                else
    //                                {
    //                                    counter[cnum].next_change_time = 0;
    //                                }
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = 0; //if the clock isn't moving.
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        counter[cnum].next_change_time = 0; //default to 0.
    //                    }
    //                    counter[cnum].triggerGATE = false;
    //                    break;
    //                case 1:
    //                    if (counter[cnum].count_written)
    //                    {
    //                        if (counter[cnum].triggerGATE)
    //                        {
    //                            set_count(ref counter[cnum], counter[cnum].inlatch);
    //                            if (counter[cnum].count_binary == 0)
    //                            {
    //                                counter[cnum].next_change_time = 1;
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = counter[cnum].count_binary & 0xFFFF;
    //                            }
    //                            counter[cnum].null_count = false;
    //                            set_OUT(ref counter[cnum], false);
    //                            if (counter[cnum].write_state == rw_status.MSByte_multiple)
    //                            {
    //                                throw new Exception("Undefined behavior when loading a half loaded count.");
    //                            }
    //                        }
    //                        else
    //                        {
    //                            decrement(ref counter[cnum]);
    //                            if (!counter[cnum].OUTpin)
    //                            {
    //                                if (counter[cnum].count_binary == 0)
    //                                {
    //                                    counter[cnum].next_change_time = 1;
    //                                }
    //                                else
    //                                {
    //                                    counter[cnum].next_change_time = counter[cnum].count_binary & 0xFFFF;
    //                                }
    //                                if (counter[cnum].count == 0)
    //                                {
    //                                    set_OUT(ref counter[cnum], true);
    //                                }
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = 0;
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        counter[cnum].next_change_time = 0; //default to 0.
    //                    }
    //                    counter[cnum].triggerGATE = false;
    //                    break;
    //                case 2:
    //                    if (counter[cnum].count_written)
    //                    {
    //                        if (counter[cnum].triggerGATE || counter[cnum].first_pass)
    //                        {
    //                            set_count(ref counter[cnum], counter[cnum].inlatch);
    //                            counter[cnum].next_change_time = (counter[cnum].count_binary - 1) & 0xFFFF;
    //                            counter[cnum].null_count = false;
    //                            if (counter[cnum].inlatch == 1)
    //                            {
    //                                throw new Exception("ERROR: count of 1 is invalid in pit mode 2.");
    //                            }
    //                            if (!counter[cnum].OUTpin)
    //                            {
    //                                set_OUT(ref counter[cnum], true);
    //                            }
    //                            if (counter[cnum].write_state == rw_status.MSByte_multiple)
    //                            {
    //                                //CLR - 07/25/2013: Don't know why this is here so I am removing it!
    //                                //Thread.Sleep(1);
    //                                if (counter[cnum].write_state == rw_status.MSByte_multiple)
    //                                    throw new Exception("Undefined behavior when loading a half loaded count.");
    //                            }
    //                            counter[cnum].first_pass = false;
    //                        }
    //                        else
    //                        {
    //                            if (counter[cnum].GATE)
    //                            {
    //                                decrement(ref counter[cnum]);
    //                                counter[cnum].next_change_time = (counter[cnum].count_binary - 1) & 0xFFFF;
    //                                if (counter[cnum].count == 1)
    //                                {
    //                                    counter[cnum].next_change_time = 1;
    //                                    set_OUT(ref counter[cnum], false);
    //                                    counter[cnum].first_pass = true;
    //                                }
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = 0;
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        counter[cnum].next_change_time = 0;
    //                    }
    //                    counter[cnum].triggerGATE = false;
    //                    break;
    //                case 3:
    //                    if (counter[cnum].count_written)
    //                    {
    //                        if ((counter[cnum].triggerGATE || counter[cnum].first_pass
    //                           || counter[cnum].state_bit_2) && counter[cnum].GATE)
    //                        {
    //                            set_count(ref counter[cnum], (Bit32u)(counter[cnum].inlatch & 0xFFFE));
    //                            counter[cnum].state_bit_1 = counter[cnum].inlatch == 0x1;
    //                            if (!counter[cnum].OUTpin || !counter[cnum].state_bit_1)
    //                            {
    //                                if (((counter[cnum].count_binary / 2) - 1) == 0)
    //                                {
    //                                    counter[cnum].next_change_time = 1;
    //                                }
    //                                else
    //                                {
    //                                    counter[cnum].next_change_time = ((counter[cnum].count_binary / 2) - 1) & 0xFFFF;
    //                                }
    //                            }
    //                            else
    //                            {
    //                                if ((counter[cnum].count_binary / 2) == 0)
    //                                {
    //                                    counter[cnum].next_change_time = 1;
    //                                }
    //                                else
    //                                {
    //                                    counter[cnum].next_change_time = (counter[cnum].count_binary / 2) & 0xFFFF;
    //                                }
    //                            }
    //                            counter[cnum].null_count = false;
    //                            if (counter[cnum].inlatch == 1)
    //                            {
    //                                throw new Exception("Count of 1 is invalid in pit mode 3.");
    //                            }
    //                            if (!counter[cnum].OUTpin)
    //                            {
    //                                set_OUT(ref counter[cnum], true);
    //                            }
    //                            else if (counter[cnum].OUTpin && !counter[cnum].first_pass)
    //                            {
    //                                set_OUT(ref counter[cnum], false);
    //                            }
    //                            if (counter[cnum].write_state == rw_status.MSByte_multiple)
    //                            {
    //                                throw new Exception("Undefined behavior when loading a half loaded count.");
    //                            }
    //                            counter[cnum].state_bit_2 = false;
    //                            counter[cnum].first_pass = false;
    //                        }
    //                        else
    //                        {
    //                            if (counter[cnum].GATE)
    //                            {
    //                                decrement(ref counter[cnum]);
    //                                //decrement(ref counter[cnum]);
    //                                if (!counter[cnum].OUTpin || !counter[cnum].state_bit_1)
    //                                {
    //                                    counter[cnum].next_change_time = ((counter[cnum].count_binary / 2) - 1) & 0xFFFF;
    //                                }
    //                                else
    //                                {
    //                                    counter[cnum].next_change_time = (counter[cnum].count_binary / 2) & 0xFFFF;
    //                                }
    //                                if (counter[cnum].count == 0)
    //                                {
    //                                    counter[cnum].state_bit_2 = true;
    //                                    counter[cnum].next_change_time = 1;
    //                                }
    //                                if ((counter[cnum].count == 2) &&
    //                                   (!counter[cnum].OUTpin || !counter[cnum].state_bit_1))
    //                                {
    //                                    counter[cnum].state_bit_2 = true;
    //                                    counter[cnum].next_change_time = 1;
    //                                }
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = 0;
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        counter[cnum].next_change_time = 0;
    //                    }
    //                    counter[cnum].triggerGATE = false;
    //                    break;
    //                case 4:
    //                    if (counter[cnum].count_written)
    //                    {
    //                        if (!counter[cnum].OUTpin)
    //                        {
    //                            set_OUT(ref counter[cnum], true);
    //                        }
    //                        if (counter[cnum].null_count)
    //                        {
    //                            set_count(ref counter[cnum], counter[cnum].inlatch);
    //                            if (counter[cnum].GATE)
    //                            {
    //                                if (counter[cnum].count_binary == 0)
    //                                {
    //                                    counter[cnum].next_change_time = 1; /*CLR: This is where IRQ 0 gets its normal heartbeat???*/
    //                                }
    //                                else
    //                                {
    //                                    counter[cnum].next_change_time = counter[cnum].count_binary & 0xFFFF;
    //                                }
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = 0;
    //                            }
    //                            counter[cnum].null_count = false;
    //                            if (counter[cnum].write_state == rw_status.MSByte_multiple)
    //                            {
    //                                throw new Exception("Undefined behavior when loading a half loaded count.");
    //                            }
    //                            counter[cnum].first_pass = true;
    //                        }
    //                        else
    //                        {
    //                            if (counter[cnum].GATE)
    //                            {
    //                                decrement(ref counter[cnum]);
    //                                if (counter[cnum].first_pass)
    //                                {
    //                                    counter[cnum].next_change_time = counter[cnum].count_binary & 0xFFFF; /*CLR: 2nd place where IRQ 0 gets heartbeat*/
    //                                    if (counter[cnum].count == 0)
    //                                    {
    //                                        set_OUT(ref counter[cnum], false); /* *** This is where IRQ 0 gets triggered ***/
    //                                        counter[cnum].next_change_time = 1;
    //                                        counter[cnum].first_pass = false;
    //                                    }
    //                                }
    //                                else
    //                                {
    //                                    counter[cnum].next_change_time = 0;
    //                                }
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = 0;
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        counter[cnum].next_change_time = 0;
    //                    }
    //                    counter[cnum].triggerGATE = false;
    //                    break;
    //                case 5:
    //                    if (counter[cnum].count_written)
    //                    {
    //                        if (!counter[cnum].OUTpin)
    //                        {
    //                            set_OUT(ref counter[cnum], true);
    //                        }
    //                        if (counter[cnum].triggerGATE)
    //                        {
    //                            set_count(ref counter[cnum], counter[cnum].inlatch);
    //                            if (counter[cnum].count_binary == 0)
    //                            {
    //                                counter[cnum].next_change_time = 1;
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = counter[cnum].count_binary & 0xFFFF;
    //                            }
    //                            counter[cnum].null_count = false;
    //                            if (counter[cnum].write_state == rw_status.MSByte_multiple)
    //                            {
    //                                throw new Exception("Undefined behavior when loading a half loaded count.");
    //                            }
    //                            counter[cnum].first_pass = true;
    //                        }
    //                        else
    //                        {
    //                            decrement(ref counter[cnum]);
    //                            if (counter[cnum].first_pass)
    //                            {
    //                                counter[cnum].next_change_time = counter[cnum].count_binary & 0xFFFF;
    //                                if (counter[cnum].count == 0)
    //                                {
    //                                    set_OUT(ref counter[cnum], false);
    //                                    counter[cnum].next_change_time = 1;
    //                                    counter[cnum].first_pass = false;
    //                                }
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = 0;
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        counter[cnum].next_change_time = 0;
    //                    }
    //                    counter[cnum].triggerGATE = false;
    //                    break;
    //                default:
    //                    throw new Exception("Mode not implemented.");
    //                    counter[cnum].next_change_time = 0;
    //                    counter[cnum].triggerGATE = false;
    //                    break;
    //            }
    //        }
    //    }
    //    public void clock_all(Bit32u cycles)
    //    {
    //        //BX_DEBUG(("clock_all:  cycles=%d",cycles));
    //        clock_multiple(0, cycles);
    //        clock_multiple(1, cycles);
    //        clock_multiple(2, cycles);
    //    }
    //    public Bit8u read(Bit8u address)
    //    {
    //        if (address > MAX_ADDRESS)
    //        {
    //            throw new Exception("Counter address incorrect in data read.");
    //        }
    //        else if (address == CONTROL_ADDRESS)
    //        {
    //            //BX_DEBUG(("PIT Read: Control Word Register."));
    //            //Read from control word register;
    //            /* This might be okay.  If so, 0 seems the most logical
    //             *  return value from looking at the docs.
    //             */
    //            throw new Exception("Read from control word register not defined.");
    //            return 0;
    //        }
    //        else
    //        {
    //            //Read from a counter;
    //            //BX_DEBUG(("PIT Read: Counter %d.",address));
    //            if (counter[address].status_latched)
    //            {
    //                //Latched Status Read;
    //                if (counter[address].count_MSB_latched &&
    //                  (counter[address].read_state == rw_status.MSByte_multiple))
    //                {
    //                    throw new Exception("Undefined output when status latched and count half read.");
    //                }
    //                else
    //                {
    //                    counter[address].status_latched = false;
    //                    return counter[address].status_latch;
    //                }
    //            }
    //            else
    //            {
    //                //Latched Count Read;
    //                if (counter[address].count_LSB_latched)
    //                {
    //                    //Read Least Significant Byte;
    //                    if (counter[address].read_state == rw_status.LSByte_multiple)
    //                    {
    //                        //BX_DEBUG(("Setting read_state to MSB_mult"));
    //                        counter[address].read_state = rw_status.MSByte_multiple;
    //                    }
    //                    counter[address].count_LSB_latched = false;
    //                    return ((Bit8u)(counter[address].outlatch & 0xFF));
    //                }
    //                else if (counter[address].count_MSB_latched)
    //                {
    //                    //Read Most Significant Byte;
    //                    if (counter[address].read_state == rw_status.MSByte_multiple)
    //                    {
    //                        //BX_DEBUG(("Setting read_state to LSB_mult"));
    //                        counter[address].read_state = rw_status.LSByte_multiple;
    //                    }
    //                    counter[address].count_MSB_latched = false;
    //                    return ((Bit8u)((counter[address].outlatch >> 8) & 0xFF));
    //                }
    //                else
    //                {
    //                    //Unlatched Count Read;
    //                    if (!((int)(counter[address].read_state) == 0x1))
    //                    {
    //                        //Read Least Significant Byte;
    //                        if (counter[address].read_state == rw_status.LSByte_multiple)
    //                        {
    //                            counter[address].read_state = rw_status.MSByte_multiple;
    //                            //BX_DEBUG(("Setting read_state to MSB_mult"));
    //                        }
    //                        return ((Bit8u)(counter[address].count & 0xFF));
    //                    }
    //                    else
    //                    {
    //                        //Read Most Significant Byte;
    //                        if (counter[address].read_state == rw_status.MSByte_multiple)
    //                        {
    //                            //BX_DEBUG(("Setting read_state to LSB_mult"));
    //                            counter[address].read_state = rw_status.LSByte_multiple;
    //                        }
    //                        return ((Bit8u)((counter[address].count >> 8) & 0xFF));
    //                    }
    //                }
    //            }
    //        }

    //        //Should only get here on errors;
    //        return 0;
    //    }
    //    public void write(Bit8u address, Bit8u data)
    //    {
    //        if (address > MAX_ADDRESS)
    //        {
    //            throw new Exception("Counter address incorrect in data write.");
    //        }
    //        else if (address == CONTROL_ADDRESS)
    //        {
    //            Bit8u SC, RW, M, BCD;
    //            controlword = data;
    //            //BX_DEBUG(("Control Word Write."));
    //            SC = (Bit8u)((controlword >> 6) & 0x3);
    //            RW = (Bit8u)((controlword >> 4) & 0x3);
    //            M = (Bit8u)((controlword >> 1) & 0x7);
    //            BCD = (Bit8u)(controlword & 0x1);
    //            if (SC == 3)
    //            {
    //                //READ_BACK command;
    //                int i;
    //                //BX_DEBUG(("READ_BACK command."));
    //                for (i = 0; i <= MAX_COUNTER; i++)
    //                {
    //                    if ((M >> i) == 1)
    //                    {
    //                        //If we are using this counter;
    //                        if (((controlword >> 5) != 1))
    //                        {
    //                            //Latch Count;
    //                            latch_counter(ref counter[i]);
    //                        }
    //                        if (((controlword >> 4) != 1))
    //                        {
    //                            //Latch Status;
    //                            if (counter[i].status_latched)
    //                            {
    //                                //Do nothing because latched status has not been read.;
    //                            }
    //                            else
    //                            {
    //                                counter[i].status_latch = (byte)(
    //                                  (byte)(((counter[i].OUTpin ? 1 : 0) & 0x1) << 7) |
    //                                  (byte)(((counter[i].null_count ? 1 : 0) & 0x1) << 6) |
    //                                  (byte)((counter[i].rw_mode & 0x3) << 4) |
    //                                  (byte)((counter[i].mode & 0x7) << 1) |
    //                                  (byte)((counter[i].bcd_mode ? 1 : 0) & 0x1));
    //                                counter[i].status_latched = true;
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                if (RW == 0)
    //                {
    //                    //Counter Latch command;
    //                    ////BX_DEBUG(("Counter Latch command.  SC=%d",SC));
    //                    latch_counter(ref counter[SC]);
    //                }
    //                else
    //                {
    //                    //Counter Program Command;
    //                    ////BX_DEBUG(("Counter Program command.  SC=%d, RW=%d, M=%d, BCD=%d",SC,RW,M,BCD));
    //                    counter[SC].null_count = true;
    //                    counter[SC].count_LSB_latched = false;
    //                    counter[SC].count_MSB_latched = false;
    //                    counter[SC].status_latched = false;
    //                    counter[SC].inlatch = 0;
    //                    counter[SC].count_written = false;
    //                    counter[SC].first_pass = true;
    //                    counter[SC].rw_mode = RW;
    //                    counter[SC].bcd_mode = (BCD > 0);
    //                    counter[SC].mode = M;
    //                    switch (RW)
    //                    {
    //                        case 0x1:
    //                            ////BX_DEBUG(("Setting read_state to LSB"));
    //                            counter[SC].read_state = rw_status.LSByte;
    //                            counter[SC].write_state = rw_status.LSByte;
    //                            break;
    //                        case 0x2:
    //                            ////BX_DEBUG(("Setting read_state to MSB"));
    //                            counter[SC].read_state = rw_status.MSByte;
    //                            counter[SC].write_state = rw_status.MSByte;
    //                            break;
    //                        case 0x3:
    //                            ////BX_DEBUG(("Setting read_state to LSB_mult"));
    //                            counter[SC].read_state = rw_status.LSByte_multiple;
    //                            counter[SC].write_state = rw_status.LSByte_multiple;
    //                            break;
    //                        default:
    //                            throw new Exception("RW field invalid in control word write.");
    //                            break;
    //                    }
    //                    //All modes except mode 0 have initial output of 1.;
    //                    if (M == 1)
    //                    {
    //                        set_OUT(ref counter[SC], true);
    //                    }
    //                    else
    //                    {
    //                        set_OUT(ref counter[SC], false);
    //                    }
    //                    counter[SC].next_change_time = 0;
    //                }
    //            }
    //        }
    //        else
    //        {
    //            //Write to counter initial value.
    //            ////BX_DEBUG(("Write Initial Count: counter=%d, count=%d",address,data));
    //            switch (counter[address].write_state)
    //            {
    //                case rw_status.LSByte_multiple:
    //                    counter[address].inlatch = data;
    //                    counter[address].write_state = rw_status.MSByte_multiple;
    //                    break;
    //                case rw_status.LSByte:
    //                    counter[address].inlatch = data;
    //                    counter[address].count_written = true;
    //                    break;
    //                case rw_status.MSByte_multiple:
    //                    counter[address].write_state = rw_status.LSByte_multiple;
    //                    counter[address].inlatch |= (Bit16u)(data << 8);
    //                    counter[address].count_written = true;
    //                    break;
    //                case rw_status.MSByte:
    //                    counter[address].inlatch = (Bit16u)(data << 8);
    //                    counter[address].count_written = true;
    //                    break;
    //                default:
    //                    throw new Exception("write counter in invalid write state.");
    //                    break;
    //            }
    //            if (counter[address].count_written && counter[address].write_state != rw_status.MSByte_multiple)
    //            {
    //                counter[address].null_count = true;
    //                set_count(ref counter[address], counter[address].inlatch);
    //            }
    //            switch (counter[address].mode)
    //            {
    //                case 0:
    //                    if (counter[address].write_state == rw_status.MSByte_multiple)
    //                    {
    //                        set_OUT(ref counter[address], false);
    //                    }
    //                    counter[address].next_change_time = 1;
    //                    break;
    //                case 1:
    //                    if (counter[address].triggerGATE)
    //                    { //for initial writes, if already saw trigger.
    //                        counter[address].next_change_time = 1;
    //                    } //Otherwise, no change.
    //                    break;
    //                case 6:
    //                case 2:
    //                    counter[address].next_change_time = 1; //FIXME: this could be loosened.
    //                    break;
    //                case 7:
    //                case 3:
    //                    counter[address].next_change_time = 1; //FIXME: this could be loosened.
    //                    break;
    //                case 4:
    //                    counter[address].next_change_time = 1;  /*CLR: IRQ 0 timer gets next_change_time here!  Is this where it gets initially set?*/
    //                    break;
    //                case 5:
    //                    if (counter[address].triggerGATE)
    //                    { //for initial writes, if already saw trigger.
    //                        counter[address].next_change_time = 1;
    //                    } //Otherwise, no change.
    //                    break;
    //            }
    //        }
    //    }
    //    public void set_GATE(Bit8u cnum, bx_bool data)
    //    {
    //        if (cnum > MAX_COUNTER)
    //        {
    //            throw new Exception("Counter number incorrect in 82C54 set_GATE");
    //        }
    //        else
    //        {
    //            if (!((counter[cnum].GATE && data) || (!(counter[cnum].GATE || data))))
    //            {
    //                //BX_INFO(("Changing GATE %d to: %d",cnum,data));
    //                counter[cnum].GATE = data;
    //                if (counter[cnum].GATE)
    //                {
    //                    counter[cnum].triggerGATE = true;
    //                }
    //                switch (counter[cnum].mode)
    //                {
    //                    case 0:
    //                        if (data && counter[cnum].count_written)
    //                        {
    //                            if (counter[cnum].null_count)
    //                            {
    //                                counter[cnum].next_change_time = 1;
    //                            }
    //                            else
    //                            {
    //                                if ((!counter[cnum].OUTpin) &&
    //                                     (counter[cnum].write_state != rw_status.MSByte_multiple))
    //                                {
    //                                    if (counter[cnum].count_binary == 0)
    //                                    {
    //                                        counter[cnum].next_change_time = 1;
    //                                    }
    //                                    else
    //                                    {
    //                                        counter[cnum].next_change_time = counter[cnum].count_binary & 0xFFFF;
    //                                    }
    //                                }
    //                                else
    //                                {
    //                                    counter[cnum].next_change_time = 0;
    //                                }
    //                            }
    //                        }
    //                        else
    //                        {
    //                            if (counter[cnum].null_count)
    //                            {
    //                                counter[cnum].next_change_time = 1;
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = 0;
    //                            }
    //                        }
    //                        break;
    //                    case 1:
    //                        if (data && counter[cnum].count_written)
    //                        { //only triggers cause a change.
    //                            counter[cnum].next_change_time = 1;
    //                        }
    //                        break;
    //                    case 2:
    //                        if (!data)
    //                        {
    //                            set_OUT(ref counter[cnum], true);
    //                            counter[cnum].next_change_time = 0;
    //                        }
    //                        else
    //                        {
    //                            if (counter[cnum].count_written)
    //                            {
    //                                counter[cnum].next_change_time = 1;
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = 0;
    //                            }
    //                        }
    //                        break;
    //                    case 3:
    //                        if (!data)
    //                        {
    //                            set_OUT(ref counter[cnum], true);
    //                            counter[cnum].first_pass = true;
    //                            counter[cnum].next_change_time = 0;
    //                        }
    //                        else
    //                        {
    //                            if (counter[cnum].count_written)
    //                            {
    //                                counter[cnum].next_change_time = 1;
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = 0;
    //                            }
    //                        }
    //                        break;
    //                    case 4:
    //                        if (!counter[cnum].OUTpin || counter[cnum].null_count)
    //                        {
    //                            counter[cnum].next_change_time = 1;
    //                        }
    //                        else
    //                        {
    //                            if (data && counter[cnum].count_written)
    //                            {
    //                                if (counter[cnum].first_pass)
    //                                {
    //                                    if (counter[cnum].count_binary == 0)
    //                                    {
    //                                        counter[cnum].next_change_time = 1;
    //                                    }
    //                                    else
    //                                    {
    //                                        counter[cnum].next_change_time = counter[cnum].count_binary & 0xFFFF;
    //                                    }
    //                                }
    //                                else
    //                                {
    //                                    counter[cnum].next_change_time = 0;
    //                                }
    //                            }
    //                            else
    //                            {
    //                                counter[cnum].next_change_time = 0;
    //                            }
    //                        }
    //                        break;
    //                    case 5:
    //                        if (data && counter[cnum].count_written)
    //                        { //only triggers cause a change.
    //                            counter[cnum].next_change_time = 1;
    //                        }
    //                        break;
    //                    default:
    //                        break;
    //                }
    //            }
    //        }
    //    }
    //    public bool read_OUT(Bit8u cnum)
    //    {
    //        if (cnum > MAX_COUNTER)
    //        {
    //            throw new Exception("Counter number incorrect in 82C54 read_OUT");
    //            return false;
    //        }

    //        return counter[cnum].OUTpin;
    //    }
    //    public bool read_GATE(Bit8u cnum)
    //    {
    //        if (cnum > MAX_COUNTER)
    //        {
    //            throw new Exception("Counter number incorrect in 82C54 read_GATE");
    //            return false;
    //        }

    //        return counter[cnum].GATE;
    //    }
    //    public Bit32u get_clock_event_time(Bit8u cnum)
    //    {
    //        if (cnum > MAX_COUNTER)
    //        {
    //            throw new Exception("Counter number incorrect in 82C54 read_GATE");
    //            return 0;
    //        }
    //        return counter[cnum].next_change_time;
    //    }
    //    public Bit32u get_next_event_time()
    //    {
    //        Bit32u time0 = get_clock_event_time(0);
    //        Bit32u time1 = get_clock_event_time(1);
    //        Bit32u time2 = get_clock_event_time(2);

    //        Bit32u outt = time0;
    //        if (time1 > 0 && (time1 < outt))
    //            outt = time1;
    //        if (time2 > 0 && (time2 < outt))
    //            outt = time2;
    //        return outt;
    //    }
    //    public Bit16u get_inlatch(int counternum)
    //    {
    //        return counter[counternum].inlatch;
    //    }
    //    void irq_handler(bx_bool raise, byte WhichIRQ)
    //    {
    //        if (raise)
    //        {
    //            if (mParent.mSystem.Debuggies.DebugPIT)
    //                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIT, "Raising IRQ 0 on thread # " + Thread.CurrentThread.ManagedThreadId);
    //            mParent.mPIC.RaiseIRQ(WhichIRQ);
    //            //DEV_pic_raise_irq(0);
    //        }
    //        else
    //        {
    //            if (mParent.mSystem.Debuggies.DebugPIT)
    //                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIT, "Lowering IRQ 0 on thread # " + Thread.CurrentThread.ManagedThreadId);
    //            mParent.mPIC.LowerIRQ(WhichIRQ);
    //            //DEV_pic_lower_irq(0);
    //        }
    //    }
    //    //void set_OUT_handler(Bit8u counternum, out_handler_t outh)
    //    //{
    //    //  counter[counternum].out_handler = outh;
    //    //}

    //}
    //enum rw_status : int
    //{
    //    LSByte = 0,
    //    MSByte = 1,
    //    LSByte_multiple = 2,
    //    MSByte_multiple = 3
    //}
    //enum real_RW_status
    //{
    //    LSB_real = 1,
    //    MSB_real = 2,
    //    BOTH_real = 3
    //}
    //struct counter_type
    //{
    //    //Chip IOs;
    //    public bx_bool GATE; //GATE Input value at end of cycle
    //    public bx_bool OUTpin; //OUT output this cycle

    //    //Architected state;
    //    public Bit32u count; //Counter value this cycle
    //    public Bit16u outlatch; //Output latch this cycle
    //    public Bit16u inlatch; //Input latch this cycle
    //    public Bit8u status_latch;

    //    //Status Register data;
    //    public Bit8u rw_mode; //2-bit R/W mode from command word register.
    //    public Bit8u mode; //3-bit mode from command word register.
    //    public bx_bool bcd_mode; //1-bit BCD vs. Binary setting.
    //    public bx_bool null_count; //Null count bit of status register.

    //    //Latch status data;
    //    public bx_bool count_LSB_latched;
    //    public bx_bool count_MSB_latched;
    //    public bx_bool status_latched;

    //    //Miscelaneous State;
    //    public Bit32u count_binary; //Value of the count in binary.
    //    public bx_bool triggerGATE; //Whether we saw GATE rise this cycle.
    //    public rw_status write_state; //Read state this cycle
    //    public rw_status read_state; //Read state this cycle
    //    public bx_bool count_written; //Whether a count written since programmed
    //    public bx_bool first_pass; //Whether or not this is the first loaded count.
    //    public bx_bool state_bit_1; //Miscelaneous state bits.
    //    public bx_bool state_bit_2;
    //    public Bit32u next_change_time; //Next time something besides count changes.
    //    //0 means never.
    //}
}
