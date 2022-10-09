using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;
namespace VirtualProcessor.Devices
{
    public partial class cKbdDevice : cDevice, iDevice
    {
        public const int BXPN_KBD_SERIAL_DELAY = 30;
        public const byte BX_KBD_CONTROLLER_QSIZE = 5;
        public const Word TIMER_INTERVAL = 100;
        public const Word KEYBOARD_NUMBER_OF_KEYS = 119;
        public struct sKbdController
        {
            /* status bits matching the status port*/
            public bool pare; // Bit7, 1= parity error from keyboard/mouse - ignored.
            public bool tim;  // Bit6, 1= timeout from keyboard - ignored.
            public bool auxb; // Bit5, 1= mouse data waiting for CPU to read.
            public bool keyl; // Bit4, 1= keyswitch in lock position - ignored.
            public bool c_d; /*  Bit3, 1=command to port 64h, 0=data to port 60h */
            public bool sysf; // Bit2,
            public bool inpb; // Bit1,
            public bool outb; // Bit0, 1= keyboard data or mouse data ready for CPU
            //       check aux to see which. Or just keyboard
            //       data before AT style machines

            public Boolean scan_convert;
            public Boolean kbd_clock_enabled;
            public Boolean aux_clock_enabled;
            public Boolean allow_irq1;
            public Boolean allow_irq12;
            public byte kbd_output_buffer;
            public byte aux_output_buffer;
            public byte last_comm;
            public Boolean expecting_port60h;
            public Boolean expecting_mouse_parameter;
            public byte last_mouse_command;
            public Int32 timer_pending;
            public Boolean irq1_requested;
            public Boolean irq12_requested;
            public Boolean scancodes_translate;
            public Boolean expecting_scancodes_set;
            public byte current_scancodes_set;
            public bool bat_in_progress;
        }
        public struct sInternalBuffer
        {
            public int num_elements;
            public byte[] buffer;
            public int head;
            public Boolean expecting_typematic;
            public Boolean expecting_led_write;
            public byte delay;
            public byte repeat_rate;
            public byte led_status;
            public Boolean scanning_enabled;

            public sInternalBuffer(byte Delay, byte RepeatRate)
            {
                buffer = new byte[Global.BX_KBD_ELEMENTS];
                num_elements = 0;
                head = 0;
                expecting_led_write = false;
                expecting_typematic = false;
                this.delay = Delay;
                this.repeat_rate = RepeatRate;
                led_status = 0;
                scanning_enabled = false;
            }
        }
        public struct s_kbdState
        {
            public PCSystem mParent;
            public sKbdController kbd_controller;
            public byte[] controller_Q;
            public byte controller_Qsize;
            public byte controller_Qsource; // 0=keyboard, 1=mouse
            public byte command_byte;
            public sInternalBuffer kbd_internal_buffer;
            public byte mKbdCommandByte, mKbdStatusByte;

            public s_kbdState(cDeviceBlock Parent)
            {
                mParent = Parent.mSystem;controller_Q = new byte[BX_KBD_CONTROLLER_QSIZE];
                kbd_controller = new sKbdController();
                controller_Qsize = 16;
                controller_Qsource = 0;
                command_byte = 0;
                kbd_internal_buffer = new sInternalBuffer((byte)1, (byte)0x10);
                mKbdCommandByte = mKbdStatusByte = 0;

            }

        }
        public static s_kbdState s = new s_kbdState();
        int kbd_initialized = 0;
        System.Timers.Timer kbdTimer = new System.Timers.Timer();
        int kbdThreadsActive;
            

        public cKbdDevice(cDeviceBlock DevBlock)
        {
            mParent = DevBlock;
            mName = "8042 Keyboard controller";
            mDeviceId = "c6298daf-01c7-47b5-8b44-bd634d29b284";
            mDeviceClass = eDeviceClass.Keyboard;
        }

        #region Keyboard Methods
        void resetinternals(bool powerup)
        {
            s.kbd_internal_buffer.num_elements = 0;
            for (int i = 0; i < Global.BX_KBD_ELEMENTS; i++)
                s.kbd_internal_buffer.buffer[i] = 0;
            s.kbd_internal_buffer.head = 0;

            s.kbd_internal_buffer.expecting_typematic = false;

            // Default scancode set is mf2 (translation is controlled by the 8042)
            s.kbd_controller.expecting_scancodes_set = false;
            s.kbd_controller.current_scancodes_set = 1;

            if (powerup)
            {
                s.kbd_internal_buffer.expecting_led_write = false;
                s.kbd_internal_buffer.delay = 1; // 500 mS
                s.kbd_internal_buffer.repeat_rate = 0x0b; // 10.9 chars/sec
            }
        }
        void activate_timer()
        {
            if (s.kbd_controller.timer_pending == 0)
            {
                s.kbd_controller.timer_pending = 1;
            }
        }
        void controller_enQ(byte data, byte source)
        {
            // source is 0 for keyboard, 1 for mouse

            //BX_DEBUG(("controller_enQ(%02x) source=%02x", (uint) data,source));

            // see if we need to Q this byte from the controller
            // remember this includes mouse bytes.
            if (s.kbd_controller.outb)
            {
                //if (s.controller_Qsize >= BX_KBD_CONTROLLER_QSIZE)
                //  BX_PANIC(("controller_enq(): controller_Q full!"));
                s.controller_Q[s.controller_Qsize++] = data;
                s.controller_Qsource = source;
                return;
            }

            // the Q is empty
            if (source == 0)
            { // keyboard
                s.kbd_controller.kbd_output_buffer = data;
                s.kbd_controller.outb = true;
                s.kbd_controller.auxb = false;
                s.kbd_controller.inpb = false;
                if (s.kbd_controller.allow_irq1)
                    s.kbd_controller.irq1_requested = true;
            }
            else
            { // mouse
                s.kbd_controller.aux_output_buffer = data;
                s.kbd_controller.outb = true;
                s.kbd_controller.auxb = true;
                s.kbd_controller.inpb = false;
                if (s.kbd_controller.allow_irq12)
                    s.kbd_controller.irq12_requested = true;
            }
        }
        void set_kbd_clock_enable(byte value)
        {
            bool prev_kbd_clock_enabled;

            if (value == 0)
            {
                s.kbd_controller.kbd_clock_enabled = false;
            }
            else
            {
                /* is another byte waiting to be sent from the keyboard ? */
                prev_kbd_clock_enabled = s.kbd_controller.kbd_clock_enabled;
                s.kbd_controller.kbd_clock_enabled = true;

                if (prev_kbd_clock_enabled == false && s.kbd_controller.outb == false)
                {
                    activate_timer();
                }
            }
        }
        void set_aux_clock_enable(byte value)
        {
            bool prev_aux_clock_enabled;

            //BX_DEBUG(("set_aux_clock_enable(%u)", (uint) value));
            if (value == 0)
            {
                s.kbd_controller.aux_clock_enabled = false;
            }
            else
            {
                /* is another byte waiting to be sent from the keyboard ? */
                prev_aux_clock_enabled = s.kbd_controller.aux_clock_enabled;
                s.kbd_controller.aux_clock_enabled = true;
                if (prev_aux_clock_enabled == false && s.kbd_controller.outb == false)
                    activate_timer();
            }
        }
        void kbd_ctrl_to_kbd(byte value)
        {

            if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "controller passed byte " + value.ToString("X2") + " to keyboard");

            if (s.kbd_internal_buffer.expecting_typematic)
            {
                s.kbd_internal_buffer.expecting_typematic = false;
                s.kbd_internal_buffer.delay = (byte)((value >> 5) & 0x03);
                switch (s.kbd_internal_buffer.delay)
                {
                    case 0: if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "setting delay to 250 mS (unused)"); break;
                    case 1: if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "setting delay to 500 mS (unused)"); break;
                    case 2: if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "setting delay to 750 mS (unused)"); break;
                    case 3: if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "setting delay to 1000 mS (unused)"); break;
                }
                s.kbd_internal_buffer.repeat_rate = (byte)(value & 0x1f);
                double cps = 1 / ((double)(8 + (value & 0x07)) * Math.Exp(Math.Log(((double)2) * (double)((value >> 3) & 0x03)) * 0.00417));
                if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "setting repeat rate to " + cps + "  cps (unused)");
                kbd_enQ(0xFA); // send ACK
                return;
            }

            if (s.kbd_internal_buffer.expecting_led_write)
            {
                s.kbd_internal_buffer.expecting_led_write = false;
                s.kbd_internal_buffer.led_status = value;
                if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "LED status set to " + s.kbd_internal_buffer.led_status.ToString("X2"));
                kbd_enQ(0xFA); // send ACK %%%
                return;
            }

            if (s.kbd_controller.expecting_scancodes_set)
            {
                s.kbd_controller.expecting_scancodes_set = false;
                if (value != 0)
                {
                    if (value < 4)
                    {
                        s.kbd_controller.current_scancodes_set = (byte)(value - 1);
                        //BX_INFO(("Switched to scancode set %d",
                        //  (uint) s.kbd_controller.current_scancodes_set + 1));
                        kbd_enQ(0xFA);
                    }
                    else
                    {
                        //BX_ERROR(("Received scancodes set out of range: %d", value));
                        kbd_enQ(0xFF); // send ERROR
                    }
                }
                else
                {
                    // Send ACK (SF patch #1159626)
                    kbd_enQ(0xFA);
                    // Send current scancodes set to port 0x60
                    kbd_enQ((byte)(1 + (s.kbd_controller.current_scancodes_set)));
                }
                if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: New scancode set");
                return;
            }
            switch (value)
            {
                case 0x00: // ??? ignore and let OS timeout with no response
                    kbd_enQ(0xFA); // send ACK %%%
                    break;

                case 0x05: // ???
                    // (mch) trying to get this to work...
                    s.kbd_controller.sysf = true;
                    kbd_enQ_imm(0xfe);
                    break;

                case 0xed: // LED Write
                    s.kbd_internal_buffer.expecting_led_write = true;
                    if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: LED Write: First byte received");
                    kbd_enQ_imm(0xFA); // send ACK %%%
                    break;

                case 0xee: // echo
                    if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: echo - returning 0xEE");
                    kbd_enQ(0xEE); // return same byte (EEh) as echo diagnostic
                    break;

                case 0xf0: // Select alternate scan code set
                    s.kbd_controller.expecting_scancodes_set = true;
                    if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: Expecting scancode set info...");
                    kbd_enQ(0xFA); // send ACK
                    break;

                case 0xf2:  // identify keyboard
                    if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: identify keyboard command received");

                    //// XT sends nothing, AT sends ACK
                    //// MFII with translation sends ACK+ABh+41h
                    //// MFII without translation sends ACK+ABh+83h
                    //if (SIM->get_param_enum(BXPN_KBD_TYPE)->get() != BX_KBD_XT_TYPE) 
                    {
                        kbd_enQ(0xFA);
                        //if (SIM->get_param_enum(BXPN_KBD_TYPE)->get() == BX_KBD_MF_TYPE) 
                        {
                            //  kbd_enQ(0xAB);

                            if (s.kbd_controller.scancodes_translate)
                                kbd_enQ(0x41);
                            else
                                kbd_enQ(0x83);
                        }
                    }
                    break;

                case 0xf3:  // typematic info
                    s.kbd_internal_buffer.expecting_typematic = true;
                    if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: setting typematic info");
                    kbd_enQ(0xFA); // send ACK
                    break;

                case 0xf4:  // enable keyboard
                    s.kbd_internal_buffer.scanning_enabled = true;
                    kbd_enQ(0xFA); // send ACK
                    break;

                case 0xf5:  // reset keyboard to power-up settings and disable scanning
                    resetinternals(true);
                    kbd_enQ(0xFA); // send ACK
                    s.kbd_internal_buffer.scanning_enabled = false;
                    if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: reset-disable command received");
                    break;

                case 0xf6:  // reset keyboard to power-up settings and enable scanning
                    resetinternals(true);
                    kbd_enQ(0xFA); // send ACK
                    s.kbd_internal_buffer.scanning_enabled = true;
                    if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: reset-enable command received");
                    break;

                case 0xfe:  // resend. aiiee.
                    throw new Exception(("got 0xFE (resend)"));
                    break;

                case 0xff:  // reset: internal keyboard reset and afterwards the BAT
                    if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: reset command received");
                    resetinternals(true);
                    kbd_enQ(0xFA); // send ACK
                    s.kbd_controller.bat_in_progress = true;
                    kbd_enQ(0xAA); // BAT test passed
                    break;

                case 0xd3: //write mouse output buffer
                    if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: write mouse output buffer received");
                    //CLR: disabling ack, we have no mouse!
                    //kbd_enQ(0xfa);
                    break;

                case 0xf7:  // PS/2 Set All Keys To Typematic
                case 0xf8:  // PS/2 Set All Keys to Make/Break
                case 0xf9:  // PS/2 PS/2 Set All Keys to Make
                case 0xfa:  // PS/2 Set All Keys to Typematic Make/Break
                case 0xfb:  // PS/2 Set Key Type to Typematic
                case 0xfc:  // PS/2 Set Key Type to Make/Break
                case 0xfd:  // PS/2 Set Key Type to Make
                default:
                    if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: kbd_ctrl_to_kbd(): got value of " + value.ToString("X2"));
                    kbd_enQ(0xFE); /* send NACK */
                    break;
            }
        }
        void timer_handler(object source, System.Timers.ElapsedEventArgs e)
        {
            kbdThreadsActive++;
            kbdTimer.Stop();
            kbdTimer.AutoReset = false;
            uint retval;
            retval = periodic(1);
            if ((retval & 0x01) == 0x1)
            {
                mParent.mPIC.RaiseIRQ(1);
                if (mParent.mSystem.Debuggies.DebugKbd)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "timer_handler: Raising IRQ 1");
            }
            //if ((retval&0x02) == 0x2)
            //  DEV_pic_raise_irq(12);
            kbdTimer.Interval = BXPN_KBD_SERIAL_DELAY; 
            if (kbdThreadsActive == 1)
                kbdTimer.Start();
            kbdThreadsActive--;
        }
        static uint periodic(int usec_delta)
        {
            //uint count_before_paste=0;
            byte retval;

            if (s.kbd_controller.kbd_clock_enabled)
            {
                //if(++count_before_paste >= pastedelay) 
                //{
                //  // after the paste delay, consider adding moving more chars
                //  // from the paste buffer to the keyboard buffer.
                //  service_paste_buf();
                //  count_before_paste=0;
                //}
            }
            else
                if (s.mParent.Debuggies.DebugKbd)
                    s.mParent.PrintDebugMsg(eDebuggieNames.Keyboard, "periodic: Keyboard clock disalbed - kbd_clock_enabled=false");

            retval = (byte)(s.kbd_controller.irq1_requested ? 1 : 0 | (s.kbd_controller.irq12_requested ? 1 : 0 << 1));
            s.kbd_controller.irq1_requested = false;
            s.kbd_controller.irq12_requested = false;

            if (s.kbd_controller.timer_pending == 0)
            {
                return (retval);
            }
            else
                if (s.mParent.Debuggies.DebugKbd)
                    s.mParent.PrintDebugMsg(eDebuggieNames.Keyboard, "periodic: no timer pending");

            if (usec_delta >= s.kbd_controller.timer_pending)
            {
                s.kbd_controller.timer_pending = 0;
                if (s.mParent.Debuggies.DebugKbd)
                    s.mParent.PrintDebugMsg(eDebuggieNames.Keyboard, "periodic: timer - rrrriiiinnnngggg");
            }
            else
            {
                s.kbd_controller.timer_pending -= usec_delta;
                if (s.mParent.Debuggies.DebugKbd)
                    s.mParent.PrintDebugMsg(eDebuggieNames.Keyboard, "periodic: timer - incrementing timer");
                return (retval);
            }

            if (s.kbd_controller.outb)
            {
                return (retval);
                if (s.mParent.Debuggies.DebugKbd)
                    s.mParent.PrintDebugMsg(eDebuggieNames.Keyboard, "periodic: outb set, returning " + retval.ToString("X2"));
            }

            /* nothing in outb, look for possible data xfer from keyboard or mouse */
            if (s.kbd_internal_buffer.num_elements > 0 && (s.kbd_controller.kbd_clock_enabled || s.kbd_controller.bat_in_progress))
            {
                if (s.mParent.mProc.mSystem.Debuggies.DebugKbd)
                    s.mParent.PrintDebugMsg(eDebuggieNames.Keyboard, "service_keyboard: key in internal buffer waiting");
                s.kbd_controller.kbd_output_buffer =
                  s.kbd_internal_buffer.buffer[s.kbd_internal_buffer.head];
                s.kbd_controller.outb = true;
                // commented out since this would override the current state of the
                // mouse buffer flag - no bug seen - just seems wrong (das)
                    s.kbd_controller.auxb = false;
                s.kbd_internal_buffer.head = (s.kbd_internal_buffer.head + 1) %
                  Global.BX_KBD_ELEMENTS;
                s.kbd_internal_buffer.num_elements--;
                if (s.kbd_controller.allow_irq1)
                    s.kbd_controller.irq1_requested = true;
            }
            else
            {
                if (s.mParent.Debuggies.DebugKbd)
                    s.mParent.PrintDebugMsg(eDebuggieNames.Keyboard, "periodic: timer - num_elements = " + s.kbd_internal_buffer.num_elements + ", kbd_clock_enabled = " + s.kbd_controller.kbd_clock_enabled + ", bat_in_process = " + s.kbd_controller.bat_in_progress);
                //create_mouse_packet(0);
                //if (s.kbd_controller.aux_clock_enabled && s.mouse_internal_buffer.num_elements)
                //{
                //    BX_DEBUG(("service_keyboard: key(from mouse) in internal buffer waiting"));
                //    s.kbd_controller.aux_output_buffer =
                //  s.mouse_internal_buffer.buffer[s.mouse_internal_buffer.head];

                //    s.kbd_controller.outb = 1;
                //    s.kbd_controller.auxb = 1;
                //    s.mouse_internal_buffer.head = (s.mouse_internal_buffer.head + 1) %
                //  BX_MOUSE_BUFF_SIZE;
                //    s.mouse_internal_buffer.num_elements--;
                //    if (s.kbd_controller.allow_irq12)
                //        s.kbd_controller.irq12_requested = 1;
                //}
                //else
                {
                    if (s.mParent.Debuggies.DebugKbd)
                        s.mParent.PrintDebugMsg(eDebuggieNames.Keyboard, "service_keyboard(): no keys waiting");
                }
            }
            return (retval);
        }
        private void KbdEnqueue(byte Scancode)
        {
            int tail;
            //buffer is full so ignore scancode
            if (s.kbd_internal_buffer.num_elements >= Global.BX_KBD_ELEMENTS)
                return;
            else
            {
                if (mParent.mSystem.Debuggies.DebugKbd)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "Scancode (" + Scancode.ToString("X2") + ") received, adding to buffer tail.");
                tail = (s.kbd_internal_buffer.head + s.kbd_internal_buffer.num_elements) % Global.BX_KBD_ELEMENTS;
                s.kbd_internal_buffer.buffer[tail] = Scancode;
                s.kbd_internal_buffer.num_elements++;
            }
        }
        public void GenerateScanCode(QWord Key)
        {
            QWord scancode = Key;
            //KbdEnqueue((byte)(Key));
            //KbdController.irq1_requested = true;
            //return;
            // Ignore scancode if keyboard clock is driven low
            if (s.kbd_controller.kbd_clock_enabled == false)
                return;

            // Ignore scancode if scanning is disabled
            if (s.kbd_internal_buffer.scanning_enabled == false)
                return;
            kbd_enQ((byte)Key);
            //KbdEnqueue((byte)(Key));
            s.kbd_controller.irq1_requested = true;
            return;
            //if ((Key & Global.BX_KEY_RELEASED) == 0)
            //    scancode = (QWord)scancodes[(Key & 0xFF), KbdController.current_scancodes_set].Break;
            //else
            //    scancode = (QWord)scancodes[(Key & 0xFF), KbdController.current_scancodes_set].Make;
            if (s.kbd_controller.scancodes_translate)
            {
                // Translate before send
                byte escaped = 0x00;

                for (int i = 0; i < 8; i++)
                {
                    switch (i)
                    {
                        case 1:
                            if ((scancode & 0xf0) == scancode)
                                escaped = 0x80;
                            else
                            {
                                KbdEnqueue((byte)(translation8042[scancode & 0xf0] | escaped));
                                escaped = 0x00;
                            }
                            break;
                        case 2:
                            if ((scancode & 0xf000) == scancode)
                                escaped = 0x80;
                            else
                            {
                                KbdEnqueue((byte)(translation8042[scancode & 0xf000] | escaped));
                                escaped = 0x00;
                            }
                            break;
                        case 3:
                            if ((scancode & 0xf00000) == scancode)
                                escaped = 0x80;
                            else
                            {
                                KbdEnqueue((byte)(translation8042[scancode & 0xf00000] | escaped));
                                escaped = 0x00;
                            }
                            break;
                        case 4:
                            if ((scancode & 0xf0000000) == scancode)
                                escaped = 0x80;
                            else
                            {
                                KbdEnqueue((byte)(translation8042[scancode & 0xf0000000] | escaped));
                                escaped = 0x00;
                            }
                            break;
                    }
                }
            }
            else
            {
                // Send raw data
                for (int i = 0; i < 8; i++)
                    switch (i)
                    {
                        case 0:
                            KbdEnqueue((byte)(scancode & 0xf0));
                            break;
                        case 1:
                            KbdEnqueue((byte)((scancode >> 8) & 0xf0));
                            break;
                        case 2:
                            KbdEnqueue((byte)((scancode >> 16) & 0xf0));
                            break;
                        case 3:
                            KbdEnqueue((byte)((scancode >> 24) & 0xf0));
                            break;
                        case 4:
                            KbdEnqueue((byte)((scancode >> 32) & 0xf0));
                            break;
                        case 5:
                            KbdEnqueue((byte)((scancode >> 40) & 0xf0));
                            break;
                        case 6:
                            KbdEnqueue((byte)((scancode >> 48) & 0xf0));
                            break;
                        case 7:
                            KbdEnqueue((byte)((scancode >> 56) & 0xf0));
                            break;
                    }
            }
            s.kbd_controller.irq1_requested = true;
        }

        #endregion

        #region cDevice Interface Related Methods
        public override void InitDevice()
        {
            //Initialize the handlers structures which will be used by cDeivce to register our ports
            s = new s_kbdState(mParent);
            mIOHandlers = new sIOHandler[2];
            mIOHandlers[0].Device = this; mIOHandlers[0].PortNum = 0x64; mIOHandlers[0].Direction = eDataDirection.IO_InOut;
            mIOHandlers[1].Device = this; mIOHandlers[1].PortNum = 0x60; mIOHandlers[1].Direction = eDataDirection.IO_InOut;
            mParent.mPIC.RegisterIRQ(this, 1);
            base.InitDevice();
            DeviceThreadSleep = 50;
            kbdTimer.Interval = BXPN_KBD_SERIAL_DELAY;
            kbdTimer.AutoReset = false;
            kbdTimer.Elapsed += timer_handler;
            resetinternals(true);

            s.kbd_internal_buffer.led_status = 0;
            s.kbd_internal_buffer.scanning_enabled = true;

            //s.mouse_internal_buffer.num_elements = 0;
            //for (i = 0; i < BX_MOUSE_BUFF_SIZE; i++)
            //    s.mouse_internal_buffer.buffer[i] = 0;
            //s.mouse_internal_buffer.head = 0;

            s.kbd_controller.pare = false;
            s.kbd_controller.tim = false;
            s.kbd_controller.auxb = false;
            s.kbd_controller.keyl = true;
            s.kbd_controller.c_d = true;
            s.kbd_controller.sysf = false;
            s.kbd_controller.inpb = false;
            s.kbd_controller.outb = false;

            s.kbd_controller.kbd_clock_enabled = true;
            s.kbd_controller.aux_clock_enabled = false;
            s.kbd_controller.allow_irq1 = true;
            s.kbd_controller.allow_irq12 = true;
            s.kbd_controller.kbd_output_buffer = 0;
            s.kbd_controller.aux_output_buffer = 0;
            s.kbd_controller.last_comm = 0;
            s.kbd_controller.expecting_port60h = false;
            s.kbd_controller.irq1_requested = false;
            s.kbd_controller.irq12_requested = false;
            s.kbd_controller.expecting_mouse_parameter = false;
            s.kbd_controller.bat_in_progress = false;
            s.kbd_controller.scancodes_translate = true;

            s.kbd_controller.timer_pending = 0;

            //// Mouse initialization stuff
            //s.mouse.type = SIM->get_param_enum(BXPN_MOUSE_TYPE)->get();
            //s.mouse.sample_rate = 100; // reports per second
            //s.mouse.resolution_cpmm = 4;   // 4 counts per millimeter
            //s.mouse.scaling = 1;   /* 1:1 (default) */
            //s.mouse.mode = MOUSE_MODE_RESET;
            //s.mouse.enable = 0;
            //s.mouse.delayed_dx = 0;
            //s.mouse.delayed_dy = 0;
            //s.mouse.delayed_dz = 0;
            //s.mouse.im_request = 0; // wheel mouse mode request
            //s.mouse.im_mode = 0; // wheel mouse mode

            for (int i = 0; i < BX_KBD_CONTROLLER_QSIZE; i++)
                s.controller_Q[i] = 0;
            s.controller_Qsize = 0;
            s.controller_Qsource = 0;

            //// clear paste buffer
            //pastebuf = NULL;
            //pastebuf_len = 0;
            //pastebuf_ptr = 0;
            //paste_delay_changed(SIM->get_param_num(BXPN_KBD_PASTE_DELAY)->get());
            //paste_service = 0;
            //stop_paste = 0;

            //// mouse port installed on system board
            //DEV_cmos_set_reg(0x14, DEV_cmos_get_reg(0x14) | 0x04);
            kbdTimer.Start();
        }
        void kbd_enQ_imm(byte val)
        {
            if (s.kbd_internal_buffer.num_elements >= Global.BX_KBD_ELEMENTS)
            {
                throw new Exception(("internal keyboard buffer full (imm)"));
                return;
            }

            if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "kbd_enQ_imm: Enqueueing byte: " + val.ToString("X2"));

            /* enqueue scancode in multibyte internal keyboard buffer */
            /*
              int tail = (s.kbd_internal_buffer.head + s.kbd_internal_buffer.num_elements) %
                BX_KBD_ELEMENTS;
            */
            s.kbd_controller.kbd_output_buffer = val;
            s.kbd_controller.outb = true;

            if (s.kbd_controller.allow_irq1)
                s.kbd_controller.irq1_requested = true;
            else
            {
                if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "kbd_enQ_imm: IRQ1 not requested due to allow_irq1=false");
            }
        }
        void kbd_enQ(byte scancode)
        {
            int tail;

            if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "kbd_enQ(0x" + scancode.ToString("X2") + ")");

            if (s.kbd_internal_buffer.num_elements >= Global.BX_KBD_ELEMENTS)
            {
                if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "internal keyboard buffer full, ignoring scancode.(" + scancode.ToString("X2"));
                return;
            }
            /* enqueue scancode in multibyte internal keyboard buffer */
            //BX_DEBUG(("kbd_enQ: putting scancode 0x%02x in internal buffer",
            //    (uint) scancode));
            tail = (s.kbd_internal_buffer.head + s.kbd_internal_buffer.num_elements) % Global.BX_KBD_ELEMENTS;
            s.kbd_internal_buffer.buffer[tail] = scancode;
            s.kbd_internal_buffer.num_elements++;

            if (!s.kbd_controller.outb && s.kbd_controller.kbd_clock_enabled)
            {
                activate_timer();
                if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "activating timer...");
                return;
            }
        }
        public override void ResetDevice()
        {
        }
        public override void RequestShutdown()
        {
            kbdTimer.Stop();
            kbdTimer.Dispose();
            kbdTimer = null;
            base.RequestShutdown();
        }
        public override void HandleIO(sPortValue IO, eDataDirection Direction)
        {
            switch (Direction)
            {
                case eDataDirection.IO_In:
                    Handle_IN(IO, Direction);
                    if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "IO: D=" + Direction + ", P=" + IO.Portnum.ToString("X4") + ", V=" + mParent.mProc.ports.mPorts[IO.Portnum].ToString("X2"));
                    break;
                case eDataDirection.IO_Out:
                    if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "IO: D=" + Direction + ", P=" + IO.Portnum.ToString("X4") + ", V=" + IO.Value.ToString("X4"));
                    Handle_OUT(IO, Direction);
                    break;
            }
        }
        public void Handle_IN(sPortValue IO, eDataDirection Direction)
        {
            byte val;

            if (IO.Portnum == 0x60)
            { /* output buffer */
                if (s.kbd_controller.auxb)
                { /* mouse byte available */
                    val = s.kbd_controller.aux_output_buffer;
                    s.kbd_controller.aux_output_buffer = 0;
                    s.kbd_controller.outb = false;
                    s.kbd_controller.auxb = false;
                    s.kbd_controller.irq12_requested = false;

                    if (s.controller_Qsize > 0)
                    {
                        uint i;
                        s.kbd_controller.aux_output_buffer = s.controller_Q[0];
                        s.kbd_controller.outb = true;
                        s.kbd_controller.auxb = true;
                        if (s.kbd_controller.allow_irq12)
                            s.kbd_controller.irq12_requested = true;
                        for (i = 0; i < s.controller_Qsize - 1; i++)
                        {
                            // move Q elements towards head of queue by one
                            s.controller_Q[i] = s.controller_Q[i + 1];
                        }
                        s.controller_Qsize--;
                    }
                    //DEV_pic_lower_irq(12);
                    //activate_timer();
                    //BX_DEBUG(("[mouse] read from 0x%02x returns 0x%02x", address, val));
                    //return val;
                }
                else if (s.kbd_controller.outb)
                { /* kbd byte available */
                    val = s.kbd_controller.kbd_output_buffer;
                    s.kbd_controller.outb = false;
                    s.kbd_controller.auxb = false;
                    s.kbd_controller.irq1_requested = false;
                    s.kbd_controller.bat_in_progress = false;

                    if (s.controller_Qsize > 0)
                    {
                        int i;
                        s.kbd_controller.aux_output_buffer = s.controller_Q[0];
                        s.kbd_controller.outb = true;
                        s.kbd_controller.auxb = true;
                        if (s.kbd_controller.allow_irq1)
                            s.kbd_controller.irq1_requested = true;
                        for (i = 0; i < s.controller_Qsize - 1; i++)
                        {
                            // move Q elements towards head of queue by one
                            s.controller_Q[i] = s.controller_Q[i + 1];
                        }
                        if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "s.controller_Qsize: " + s.controller_Qsize);
                        s.controller_Qsize--;
                    }

                    mParent.mPIC.LowerIRQ(1);
                    if (mParent.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard,"Handle_IN - Lowering IRQ 1");
                    activate_timer();
                    //      BX_DEBUG(("READ(%02x) = %02x", (unsigned) address, (unsigned) val));
                    mParent.mProc.ports.mPorts[IO.Portnum] = val;
                    if (mParent.mProc.mSystem.Debuggies.DebugKbd)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "Handle_IN: returning key from buffer = " + val.ToString("X2"));
                    return;
                }
                else
                {
                    //BX_DEBUG(("num_elements = %d", s.kbd_internal_buffer.num_elements));
                    //BX_DEBUG(("read from port 60h with outb empty"));
                    mParent.mProc.ports.mPorts[IO.Portnum] = s.kbd_controller.kbd_output_buffer;
                    return;
                }
            }

            else if (IO.Portnum == 0x64)
            { /* status register */
                val = (byte)(((s.kbd_controller.pare ? 1 : 0) << 7) |
                    ((s.kbd_controller.tim ? 1 : 0) << 6) |
                    ((s.kbd_controller.auxb ? 1 : 0) << 5) |
                    ((s.kbd_controller.keyl ? 1 : 0) << 4) |
                    ((s.kbd_controller.c_d ? 1 : 0) << 3) |
                    ((s.kbd_controller.sysf ? 1 : 0) << 2) |
                    ((s.kbd_controller.inpb ? 1 : 0) << 1) |
                    ((s.kbd_controller.outb ? 1 : 0)));
                s.kbd_controller.tim = false;
                mParent.mProc.ports.mPorts[IO.Portnum] = val;
                return;
            }

            //BX_PANIC(("unknown address in io read to keyboard port %x",
            //(unsigned) address));
            return; /* keep compiler happy */
        }
        public void Handle_OUT(sPortValue IO, eDataDirection Direction)
        {
            byte command_byte;

            switch (IO.Portnum)
            {
                case 0x60: // input buffer
                    // if expecting data byte from command last sent to port 64h
                    if (s.kbd_controller.expecting_port60h)
                    {
                        s.kbd_controller.expecting_port60h = false;
                        // data byte written last to 0x60
                        s.kbd_controller.c_d = false;
                        if (s.kbd_controller.inpb)
                        {
                            throw new Exception("write to port 60h, not ready for write");
                        }
                        switch (s.kbd_controller.last_comm)
                        {
                            case 0x60: // write command byte
                                {
                                    bool scan_convert, disable_keyboard,
                                            disable_aux;

                                    scan_convert = ((IO.Value >> 6) & 0x01) == 0x01;
                                    disable_aux = ((IO.Value >> 5) & 0x01) == 0x01;
                                    disable_keyboard = ((IO.Value >> 4) & 0x01) == 0x01;
                                    s.kbd_controller.sysf = ((IO.Value >> 2) & 0x01) == 0x01;
                                    s.kbd_controller.allow_irq1 = ((IO.Value >> 0) & 0x01) == 0x01;
                                    s.kbd_controller.allow_irq12 = ((IO.Value >> 1) & 0x01) == 0x01;
                                    set_kbd_clock_enable((byte)(!disable_keyboard ? 1 : 0));
                                    set_aux_clock_enable((byte)(!disable_aux ? 1 : 0));
                                    if (s.kbd_controller.allow_irq12 && s.kbd_controller.auxb)
                                        s.kbd_controller.irq12_requested = true;
                                    else if (s.kbd_controller.allow_irq1 && s.kbd_controller.outb)
                                        s.kbd_controller.irq1_requested = true;
                                    if (mParent.mSystem.Debuggies.DebugKbd)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard,"Command byte received: " + IO.Value.ToString("X2"));
                                    //BX_DEBUG((" allow_irq12 set to %u",
                                    //  (uint) s.kbd_controller.allow_irq12));
                                    //if (!scan_convert)
                                    //  BX_INFO(("keyboard: scan convert turned off"));

                                    // (mch) NT needs this
                                    s.kbd_controller.scancodes_translate = scan_convert;
                                }
                                break;
                            case 0xcb: // write keyboard controller mode
                                if (mParent.mSystem.Debuggies.DebugKbd)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: write keyboard controller mode with value " + IO.Value.ToString("X2") + " - nop");
                                break;
                            case 0xd1: // write output port
                                //    BX_DEBUG(("write output port with value %02xh", (uint) value));
                                //BX_DEBUG(("write output port : %sable A20",(value & 0x02)?"en":"dis"));
                                mParent.mProc.mSystem.A20Status = (IO.Value & 0x02) != 0;
                                if ((IO.Value & 0x01) != 1)
                                {
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: write output port : processor reset requested!");
                                    //bx_pc_system.Reset(BX_RESET_SOFTWARE);
                                    mParent.mSystem.ResetSystem();
                                }
                                break;
                            case 0xd4: // Write to mouse
                            //  // I don't think this enables the AUX clock
                            //  //set_aux_clock_enable(1); // enable aux clock line
                            //  kbd_ctrl_to_mouse(IO.Value);
                            //  // ??? should I reset to previous value of aux enable?
                                if (mParent.mSystem.Debuggies.DebugKbd)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: Write to mouse - nop");
                                        break;

                            case 0xd3: // write mouse output buffer
                            //  // Queue in mouse output buffer
                            //  controller_enQ(IO.Value, 1);
                                if (mParent.mSystem.Debuggies.DebugKbd)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: write mouse output buffer - nop");
                              break;
                            case 0xd2:
                                // Queue in keyboard output buffer
                                if (mParent.mSystem.Debuggies.DebugKbd)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: Write keyboard output buffer");
                                controller_enQ((byte)IO.Value, 0);
                                break;

                            default:
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "=== unsupported write to port 60h(lastcomm=" + s.kbd_controller.last_comm.ToString("X2") + "): " + IO.Value.ToString("X2"));
                                break;
                        }
                    }
                    else
                    {
                        // data byte written last to 0x60
                        s.kbd_controller.c_d = false;
                        s.kbd_controller.expecting_port60h = false;
                        /* pass byte to keyboard */
                        /* ??? should conditionally pass to mouse device here ??? */
                        if (s.kbd_controller.kbd_clock_enabled == false)
                        {
                            set_kbd_clock_enable(1);
                        }
                        kbd_ctrl_to_kbd((byte)IO.Value);
                    }
                    break;

                case 0x64: // control register
                    // command byte written last to 0x64
                    s.kbd_controller.c_d = true;
                    s.kbd_controller.last_comm = (byte)IO.Value;
                    // most commands NOT expecting port60 write next
                    s.kbd_controller.expecting_port60h = false;

                    switch (IO.Value)
                    {
                        case 0x20: // get keyboard command byte
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: get keyboard command byte");
                            // controller output buffer must be empty
                            if (s.kbd_controller.outb)
                            {
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: get keyboard command byte - OUTB set and command 0x" + IO.Value.ToString("X2") + " encountered");
                                break;
                            }
                            command_byte = (byte)(
                                (s.kbd_controller.scancodes_translate ? 1 : 0 << 6) |
                                ((!s.kbd_controller.aux_clock_enabled ? 1 : 0) << 5) |
                                ((!s.kbd_controller.kbd_clock_enabled ? 1 : 0) << 4) |
                              (0 << 3) |
                              (s.kbd_controller.sysf ? 1 : 0 << 2) |
                              (s.kbd_controller.allow_irq12 ? 1 : 0 << 1) |
                              (s.kbd_controller.allow_irq1 ? 1 : 0 << 0));
                            controller_enQ(command_byte, 0);
                            break;
                        case 0x60: // write command byte
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: write command byte");
                            // following byte written to port 60h is command byte
                            s.kbd_controller.expecting_port60h = true;
                            break;

                        case 0xa0:
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: keyboard BIOS name not supported");
                            break;

                        case 0xa1:
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: keyboard BIOS version not supported");
                            break;

                        case 0xa7: // disable the aux device
                            set_aux_clock_enable(0);
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: aux device disabled");
                            break;
                        case 0xa8: // enable the aux device
                            set_aux_clock_enable(1);
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: aux device enabled");
                            break;
                        case 0xa9: // Test Mouse Port
                            // controller output buffer must be empty
                            if (s.kbd_controller.outb)
                            {
                                //BX_PANIC(("kbd: OUTB set and command 0x%02x encountered", IO.Value));
                                break;
                            }
                            //controller_enQ(0x00, 0); // no errors detected
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: Test Mouse Port - returning 'no mouse'");
                            controller_enQ(0xFF, 0);
                            break;
                        case 0xaa: // motherboard controller self test
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: Self Test");
                            if (kbd_initialized == 0)
                            {
                                s.controller_Qsize = 0;
                                s.kbd_controller.outb = false;
                                kbd_initialized++;
                            }
                            // controller output buffer must be empty
                            if (s.kbd_controller.outb)
                            {
                                //BX_ERROR(("kbd: OUTB set and command 0x%02x encountered", IO.Value));
                                break;
                            }
                            // (mch) Why is this commented out??? Enabling
                            s.kbd_controller.sysf = true; // self test complete
                            controller_enQ(0x55, 0); // controller OK
                            break;
                        case 0xab: // Interface Test
                            // controller output buffer must be empty
                            if (s.kbd_controller.outb)
                            {
                                //BX_PANIC(("kbd: OUTB set and command 0x%02x encountered", IO.Value));
                                break;
                            }
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: Interface test");
                            controller_enQ(0x00, 0);
                            break;
                        case 0xad: // disable keyboard
                            set_kbd_clock_enable(0);
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: keyboard disabled");
                            break;
                        case 0xae: // enable keyboard
                            set_kbd_clock_enable(1);
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: keyboard enabled");
                            break;
                        case 0xaf: // get controller version

                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "'COMMAND: get controller version' not supported yet");
                            break;
                        case 0xc0: // read input port
                            // controller output buffer must be empty
                            if (s.kbd_controller.outb)
                            {
                                //BX_PANIC(("kbd: OUTB set and command 0x%02x encountered", IO.Value));
                                throw new Exception("COMMAND: read input port - OUTB set and command " + IO.Value + " encountered");
                                break;
                            }
                            // keyboard not inhibited
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: read input port - keyboard not inhibited");
                            controller_enQ(0x80, 0);
                            break;
                        case 0xca: // read keyboard controller mode
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: read keyboard controller mode");
                            //controller_enQ(0x01, 0); // PS/2 (MCA)interface
                            controller_enQ(0x00, 0); //ISA interface - CLR
                            break;
                        case 0xcb: //  write keyboard controller mode
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: write keyboard controller mode");
                            // write keyboard controller mode to bit 0 of port 0x60
                            s.kbd_controller.expecting_port60h = true;
                            break;
                        case 0xd0: // read output port: next byte read from port 60h
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: io write to port 64h, command d0h (partial)");
                            // controller output buffer must be empty
                            if (s.kbd_controller.outb)
                            {
                                //BX_PANIC(("kbd: OUTB set and command 0x%02x encountered", IO.Value));
                                break;
                            }
                            controller_enQ((byte)((s.kbd_controller.irq12_requested ? 1 : 0 << 5) |
                                (s.kbd_controller.irq1_requested ? 1 : 0 << 4) |
                                (mParent.mProc.mSystem.A20Status ? 1 : 0 << 1) |
                                0x01), 0);
                            break;

                        case 0xd1: // write output port: next byte written to port 60h
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: write output port");
                            // following byte to port 60h written to output port
                            s.kbd_controller.expecting_port60h = true;
                            break;

                        case 0xd3: // write mouse output buffer
                            //FIXME: Why was this a panic?
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: io write 0x64: command = 0xD3(write mouse outb)");
                            // following byte to port 60h written to output port as mouse write.
                            s.kbd_controller.expecting_port60h = true;
                            break;

                        case 0xd4: // write to mouse
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: io write 0x64: command = 0xD4 (write to mouse)");
                            // following byte written to port 60h
                            s.kbd_controller.expecting_port60h = true;
                            break;

                        case 0xd2: // write keyboard output buffer
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: io write 0x64: write keyboard output buffer");
                            s.kbd_controller.expecting_port60h = true;
                            break;
                        case 0xdd: // Disable A20 Address Line
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: disable A20 address line");
                            mParent.mProc.mSystem.A20Status = false;
                            break;
                        case 0xdf: // Enable A20 Address Line
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: enable A20 address line");
                            mParent.mProc.mSystem.A20Status = true;
                            break;
                        case 0xc1: // Continuous Input Port Poll, Low
                        case 0xc2: // Continuous Input Port Poll, High
                        case 0xe0: // Read Test Inputs
                            throw new Exception("io write 0x64: command = " + IO.Value.ToString("X2") + "h");
                            break;

                        case 0xfe: // System (cpu?) Reset, transition to real mode
                            if (mParent.mSystem.Debuggies.DebugKbd)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: io write 0x64: command 0xfe: reset cpu");
                            //TODO: Code reset
                            //bx_pc_system.Reset(BX_RESET_SOFTWARE);
                            break;

                        default:
                            if (IO.Value == 0xff || (IO.Value >= 0xf0 && IO.Value <= 0xfd))
                            {
                                /* useless pulse output bit commands ??? */
                                if (mParent.mSystem.Debuggies.DebugKbd)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "COMMAND: io write to port 64h, useless command " + IO.Value.ToString("X2"));
                                return;
                            }
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Keyboard, "unsupported io write to keyboard port " + IO.Portnum.ToString("X4") + ", value = " + IO.Value.ToString("X2"));
                            break;
                    }
                    break;

                default:
                    throw new Exception(("unknown address in bx_keyb_c::write()"));
            }
        }
        public override void DeviceThread()
        {
            while (1 == 1)
            {
                Thread.Sleep(40);
                if (mShutdownRequested)
                    break;
            }
        }
        #endregion
    }
}
