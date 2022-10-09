using System;
using System.Linq;
using System.Text;
using DWord = System.UInt32;
using QWord = System.UInt64;
using System.Runtime.InteropServices;

namespace VirtualProcessor.Devices
{

    public class cPICDevice : cDevice
    {
        sCombinedPic s;
        private const byte MAX_IRQ_NUM = 15; /*0 - 7*/
        private string[] mIRQOwner = new string[MAX_IRQ_NUM];
        static object syncRoot = new object();

        #region cDevice Interface Related Methods
        public cPICDevice(cDeviceBlock DevBlock)
        {
            mParent = DevBlock;
            mDeviceId = "06d8089b-df85-43c1-8c85-67197690ade4";
            mDeviceClass = eDeviceClass.PIC;
            mName = "PIC";
        }

        public override void InitDevice()
        {
            mIOHandlers = new sIOHandler[4];
            mIOHandlers[0].Device = this; mIOHandlers[0].PortNum = 0x20; mIOHandlers[0].Direction = eDataDirection.IO_InOut;
            mIOHandlers[1].Device = this; mIOHandlers[1].PortNum = 0x21; mIOHandlers[1].Direction = eDataDirection.IO_InOut;
            mIOHandlers[2].Device = this; mIOHandlers[2].PortNum = 0xA0; mIOHandlers[2].Direction = eDataDirection.IO_InOut;
            mIOHandlers[3].Device = this; mIOHandlers[3].PortNum = 0xA1; mIOHandlers[3].Direction = eDataDirection.IO_InOut;
            base.InitDevice();

            s.master_pic.single_PIC = 0;
            s.master_pic.interrupt_offset = 0x08; /* IRQ0 = INT 0x08 */
            /* slave PIC connected to IRQ2 of master */
            s.master_pic.slave_connect_mask = 0x04;
            s.master_pic.sfnm = 0; /* normal nested mode */
            s.master_pic.buffered_mode = 0; /* unbuffered mode */
            s.master_pic.master_slave = 1; /* master PIC */
            s.master_pic.auto_eoi = 0; /* manual EOI from CPU */
            s.master_pic.imr = 0xFF; /* all IRQ's initially masked */
            s.master_pic.isr = 0x00; /* no IRQ's in service */
            s.master_pic.irr = 0x00; /* no IRQ's requested */
            s.master_pic.read_reg_select = 0; /* IRR */
            s.master_pic.irq = 0;
            s.master_pic.INT = false;
            s.master_pic.init.in_init = false;
            s.master_pic.init.requires_4 = false;
            s.master_pic.init.byte_expected = 0;
            s.master_pic.special_mask = false;
            s.master_pic.lowest_priority = 7;
            s.master_pic.polled = false;
            s.master_pic.rotate_on_autoeoi = false;
            s.master_pic.edge_level = 0;
            s.master_pic.IRQ_in = 0;

            s.slave_pic.single_PIC = 0;
            s.slave_pic.interrupt_offset = 0x70; /* IRQ8 = INT 0x70 */
            s.slave_pic.slave_id = 0x02; /* slave PIC connected to IRQ2 of master */
            s.slave_pic.sfnm = 0; /* normal nested mode */
            s.slave_pic.buffered_mode = 0; /* unbuffered mode */
            s.slave_pic.master_slave = 0; /* slave PIC */
            s.slave_pic.auto_eoi = 0; /* manual EOI from CPU */
            s.slave_pic.imr = 0xFF; /* all IRQ's initially masked */
            s.slave_pic.isr = 0x00; /* no IRQ's in service */
            s.slave_pic.irr = 0x00; /* no IRQ's requested */
            s.slave_pic.read_reg_select = 0; /* IRR */
            s.slave_pic.irq = 0;
            s.slave_pic.INT = false;
            s.slave_pic.init.in_init = false;
            s.slave_pic.init.requires_4 = false;
            s.slave_pic.init.byte_expected = 0;
            s.slave_pic.special_mask = false;
            s.slave_pic.lowest_priority = 7;
            s.slave_pic.polled = false;
            s.slave_pic.rotate_on_autoeoi = false;
            s.slave_pic.edge_level = 0;
            s.slave_pic.IRQ_in = 0;
        }
        public override void ResetDevice()
        {

        }
        public override void HandleIO(sPortValue IO, eDataDirection Direction)
        {
            if (mParent.mSystem.Debuggies.DebugPIC)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "NEW: D=" + Direction + ", P=" + IO.Portnum.ToString("X4") + ", V=" + IO.Value.ToString("X8") + ", MISR=" + s.master_pic.isr.ToString("X2") + ", MIRR=" + s.master_pic.irr.ToString("X2") + ", MIMR=" + s.master_pic.imr.ToString("X2") + ", SISR=" + s.slave_pic.isr.ToString("X2") + ", SIRR=" + s.slave_pic.irr.ToString("X2") + ", SIMR=" + s.slave_pic.imr.ToString("X2"));
            switch (Direction)
            {
                case eDataDirection.IO_In:
                    Handle_IN(IO, Direction);
                    break;
                case eDataDirection.IO_Out:
                    Handle_OUT(IO, Direction);
                    break;
            }
        }
        public void Handle_IN(sPortValue IO, eDataDirection Direction)
        {
            if (mParent.mSystem.Debuggies.DebugPIC)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "IO read from " + IO.Portnum.ToString("X4"));

            /*
             8259A PIC
             */

            if ((IO.Portnum == 0x20 || IO.Portnum == 0x21) && s.master_pic.polled)
            {
                // In polled mode. Treat this as an interrupt acknowledge
                clear_highest_interrupt(ref s.master_pic);
                s.master_pic.polled = false;
                service_master_pic();
                mParent.mProc.ports.mPorts[IO.Portnum] = (DWord)((IO.Size == TypeCode.Byte) ? s.master_pic.irq : (s.master_pic.irq) << 8 | (s.master_pic.irq));  // Return the current irq requested
                return;
            }

            if ((IO.Portnum == 0xa0 || IO.Portnum == 0xa1) && s.slave_pic.polled)
            {
                // In polled mode. Treat this as an interrupt acknowledge
                clear_highest_interrupt(ref s.slave_pic);
                s.slave_pic.polled = false;
                service_slave_pic();
                mParent.mProc.ports.mPorts[IO.Portnum] = (DWord)((IO.Size == TypeCode.Byte) ? s.slave_pic.irq : (s.slave_pic.irq) << 8 | (s.slave_pic.irq));  // Return the current irq requested
            }

            switch (IO.Portnum)
            {
                case 0x20:
                    if (s.master_pic.read_reg_select > 0)
                    { /* ISR */
                        if (mParent.mSystem.Debuggies.DebugPIC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "read master ISR = " + s.master_pic.isr.ToString("X2"));
                        mParent.mProc.ports.mPorts[IO.Portnum] = s.master_pic.isr;
                    }
                    else
                    { /* IRR */
                        if (mParent.mSystem.Debuggies.DebugPIC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "read master IRR = " + s.master_pic.irr.ToString("X2"));
                        mParent.mProc.ports.mPorts[IO.Portnum] = s.master_pic.irr;
                    }
                    return;
                case 0x21:
                    if (mParent.mSystem.Debuggies.DebugPIC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "read master IMR = " + s.master_pic.imr.ToString("X2"));
                    mParent.mProc.ports.mPorts[IO.Portnum] = s.master_pic.imr;
                    return;
                case 0xA0:
                    if (s.slave_pic.read_reg_select > 0)
                    { /* ISR */
                        if (mParent.mSystem.Debuggies.DebugPIC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "read slave ISR = " + s.slave_pic.isr.ToString("X2"));
                        mParent.mProc.ports.mPorts[IO.Portnum] = s.slave_pic.isr;
                    }
                    else
                    { /* IRR */
                        if (mParent.mSystem.Debuggies.DebugPIC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "read slave IRR = " + s.slave_pic.irr.ToString("X2"));
                        mParent.mProc.ports.mPorts[IO.Portnum] = s.slave_pic.irr;
                    }
                    return;
                case 0xA1:
                    if (mParent.mSystem.Debuggies.DebugPIC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "read slave IMR = " + s.slave_pic.imr.ToString("X2"));
                    mParent.mProc.ports.mPorts[IO.Portnum] = s.slave_pic.imr;
                    return;
            }

            throw new Exception("io read to IO.Portnum " + IO.Portnum.ToString("X4"));
        }
        public void Handle_OUT(sPortValue IO, eDataDirection Direction)
        {
            if (mParent.mSystem.Debuggies.DebugPIC)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "IO write to " + IO.Portnum.ToString("X4") + " = " + IO.Value.ToString("X2"));

            /*
             8259A PIC
             */

            switch (IO.Portnum)
            {
                case 0x20:
                    if ((IO.Value & 0x10) == 0x10)
                    { /* initialization command 1 */
                        if (mParent.mSystem.Debuggies.DebugPIC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "master: init command 1 found");
                        if (mParent.mSystem.Debuggies.DebugPIC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "        requires 4 = " + (IO.Value & 0x01));
                        if (mParent.mSystem.Debuggies.DebugPIC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "        cascade mode: [0=cascade,1=single] " + ((IO.Value & 0x02) >> 1));
                        s.master_pic.init.in_init = true;
                        s.master_pic.init.requires_4 = (IO.Value & 0x01) == 0x01;
                        s.master_pic.init.byte_expected = 2; /* operation command 2 */
                        s.master_pic.imr = 0x00; /* clear the irq mask register */
                        s.master_pic.isr = 0x00; /* no IRQ's in service */
                        s.master_pic.irr = 0x00; /* no IRQ's requested */
                        s.master_pic.lowest_priority = 7;
                        s.master_pic.INT = false; /* reprogramming clears previous INTR request */
                        s.master_pic.auto_eoi = 0;
                        s.master_pic.rotate_on_autoeoi = false;
                        if ((IO.Value & 0x02) == 0x02)
                            throw new Exception(("master: ICW1: single mode not supported"));
                        if ((IO.Value & 0x08) == 0x08)
                        {
                            throw new Exception(("master: ICW1: level sensitive mode not supported"));
                        }
                        else
                        {
                            if (mParent.mSystem.Debuggies.DebugPIC)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "master: ICW1: edge triggered mode selected");
                        }
                        //TODO: BX_CLEAR_INTR();
                        mParent.mProc.Signals.INTR = false;
                        return;
                    }

                    if ((IO.Value & 0x18) == 0x08)
                    { /* OCW3 */
                        byte special_mask, poll, read_op;

                        special_mask = (byte)((IO.Value & 0x60) >> 5);
                        poll = (byte)((IO.Value & 0x04) >> 2);
                        read_op = (byte)((IO.Value & 0x03));
                        if (poll > 0)
                        {
                            s.master_pic.polled = true;
                            return;
                        }
                        if (read_op == 0x02) /* read IRR */
                            s.master_pic.read_reg_select = 0;
                        else if (read_op == 0x03) /* read ISR */
                            s.master_pic.read_reg_select = 1;
                        if (special_mask == 0x02)
                        { /* cancel special mask */
                            s.master_pic.special_mask = false;
                        }
                        else if (special_mask == 0x03)
                        { /* set specific mask */
                            s.master_pic.special_mask = true;
                            service_master_pic();
                        }
                        return;
                    }

                    /* OCW2 */
                    switch (IO.Value)
                    {
                        case 0x00: // Rotate in auto eoi mode clear
                        case 0x80: // Rotate in auto eoi mode set
                            s.master_pic.rotate_on_autoeoi = (IO.Value != 0);
                            break;

                        case 0xA0: // Rotate on non-specific end of interrupt
                        case 0x20: /* end of interrupt command */
                            if (mParent.mSystem.Debuggies.DebugPIC)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "Handle_OUT: Master: Clearing highest. BEFORE: ISR=" + s.master_pic.isr.ToString("X2") + ", IRR = " + s.master_pic.irr.ToString("X2") + ", IMR = " + s.master_pic.imr.ToString("X2"));

                            clear_highest_interrupt(ref s.master_pic);
                                if (mParent.mSystem.Debuggies.DebugPIC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "Handle_OUT: Master: Clearing highest. AFTER: ISR=" + s.master_pic.isr.ToString("X2") + ", IRR = " + s.master_pic.irr.ToString("X2") + ", IMR = " + s.master_pic.imr.ToString("X2"));

                            if (IO.Value == 0xA0)
                            {// Rotate in Auto-EOI mode
                                s.master_pic.lowest_priority++;
                                if (s.master_pic.lowest_priority > 7)
                                    s.master_pic.lowest_priority = 0;
                            }

                            service_master_pic();
                            break;

                        case 0x40: // Intel PIC spec-sheet seems to indicate this should be ignored
                            //BX_INFO(("IRQ no-op"));
                            break;

                        case 0x60: /* specific EOI 0 */
                        case 0x61: /* specific EOI 1 */
                        case 0x62: /* specific EOI 2 */
                        case 0x63: /* specific EOI 3 */
                        case 0x64: /* specific EOI 4 */
                        case 0x65: /* specific EOI 5 */
                        case 0x66: /* specific EOI 6 */
                        case 0x67: /* specific EOI 7 */
                            s.master_pic.isr &= (byte)~((1 << (int)(IO.Value - 0x60)));
                            service_master_pic();
                            break;

                        // IRQ lowest priority commands
                        case 0xC0: // 0 7 6 5 4 3 2 1
                        case 0xC1: // 1 0 7 6 5 4 3 2
                        case 0xC2: // 2 1 0 7 6 5 4 3
                        case 0xC3: // 3 2 1 0 7 6 5 4
                        case 0xC4: // 4 3 2 1 0 7 6 5
                        case 0xC5: // 5 4 3 2 1 0 7 6
                        case 0xC6: // 6 5 4 3 2 1 0 7
                        case 0xC7: // 7 6 5 4 3 2 1 0
                            //BX_INFO(("IRQ lowest command 0x%x", IO.Value));
                            s.master_pic.lowest_priority = (byte)(IO.Value - 0xC0);
                            break;

                        case 0xE0: // specific EOI and rotate 0
                        case 0xE1: // specific EOI and rotate 1
                        case 0xE2: // specific EOI and rotate 2
                        case 0xE3: // specific EOI and rotate 3
                        case 0xE4: // specific EOI and rotate 4
                        case 0xE5: // specific EOI and rotate 5
                        case 0xE6: // specific EOI and rotate 6
                        case 0xE7: // specific EOI and rotate 7
                            s.master_pic.isr &= (byte)~(1 << (int)(IO.Value - 0xE0));
                            s.master_pic.lowest_priority = (byte)((IO.Value - 0xE0));
                            service_master_pic();
                            break;

                        case 0x02: // single mode bit: 1 = single, 0 = cascade
                            // ignore. 386BSD writes this IO.Value but works with it ignored.
                            break;

                        default:
                            throw new Exception("write to port 20h = " + IO.Value.ToString("X2"));
                    } /* switch (IO.Value) */
                    break;

                case 0x21:
                    /* initialization mode operation */
                    if (s.master_pic.init.in_init)
                    {
                        switch (s.master_pic.init.byte_expected)
                        {
                            case 2:
                                s.master_pic.interrupt_offset = (byte)(IO.Value & 0xf8);
                                s.master_pic.init.byte_expected = 3;
                                if (mParent.mSystem.Debuggies.DebugPIC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "master: init command 2 = " + IO.Value);
                                if (mParent.mSystem.Debuggies.DebugPIC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "        offset = INT " + s.master_pic.interrupt_offset.ToString("X2"));
                                return;
                                break;
                            case 3:
                                if (mParent.mSystem.Debuggies.DebugPIC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "master: init command 3 = " + IO.Value.ToString("X2"));
                                if (s.master_pic.init.requires_4)
                                {
                                    s.master_pic.init.byte_expected = 4;
                                }
                                else
                                {
                                    s.master_pic.init.in_init = false;
                                }
                                return;
                                break;
                            case 4:
                                if (mParent.mSystem.Debuggies.DebugPIC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "master: init command 4 = " + IO.Value.ToString("X2"));
                                if ((IO.Value & 0x02) == 0x02)
                                {
                                    if (mParent.mSystem.Debuggies.DebugPIC)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "       auto EOI");
                                    s.master_pic.auto_eoi = 1;
                                }
                                else
                                {
                                    if (mParent.mSystem.Debuggies.DebugPIC)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "normal EOI interrupt");
                                    s.master_pic.auto_eoi = 0;
                                }
                                if ((IO.Value & 0x01) == 0x01)
                                {
                                    if (mParent.mSystem.Debuggies.DebugPIC)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "       80x86 mode");
                                }
                                else
                                    throw new Exception(("       not 80x86 mode"));
                                s.master_pic.init.in_init = false;
                                return;
                            default:
                                throw new Exception(("master expecting bad init command"));
                        }
                    }

                    /* normal operation */
                    if (mParent.mSystem.Debuggies.DebugPIC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "setting master pic IMR to " + IO.Value.ToString("X2"));
                    s.master_pic.imr = (byte)IO.Value;
                    service_master_pic();
                    return;

                case 0xA0:
                    if ((IO.Value & 0x10) == 0x10)
                    { /* initialization command 1 */
                        if (mParent.mSystem.Debuggies.DebugPIC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "slave: init command 1 found");
                        if (mParent.mSystem.Debuggies.DebugPIC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "       requires 4 = " + (IO.Value & 0x01));
                        if (mParent.mSystem.Debuggies.DebugPIC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "       cascade mode: [0=cascade,1=single] " + ((IO.Value & 0x02) >> 1));
                        s.slave_pic.init.in_init = true;
                        s.slave_pic.init.requires_4 = ((IO.Value & 0x01) == 0x01);
                        s.slave_pic.init.byte_expected = 2; /* operation command 2 */
                        s.slave_pic.imr = 0x00; /* clear irq mask */
                        s.slave_pic.isr = 0x00; /* no IRQ's in service */
                        s.slave_pic.irr = 0x00; /* no IRQ's requested */
                        s.slave_pic.lowest_priority = 7;
                        s.slave_pic.INT = false; /* reprogramming clears previous INTR request */
                        //TODO: Verify next is correct (0xfb)
                        s.master_pic.IRQ_in &= 0xFB; //(byte)~(1 << 2);
                        s.slave_pic.auto_eoi = 0;
                        s.slave_pic.rotate_on_autoeoi = false;
                        if ((IO.Value & 0x02) == 0x02)
                            throw new Exception(("slave: ICW1: single mode not supported"));
                        if ((IO.Value & 0x08) == 0x08)
                        {
                            throw new Exception(("slave: ICW1: level sensitive mode not supported"));
                        }
                        else
                        {
                            if (mParent.mSystem.Debuggies.DebugPIC)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "slave: ICW1: edge triggered mode selected");
                        }
                        return;
                    }

                    if ((IO.Value & 0x18) == 0x08)
                    { /* OCW3 */
                        byte special_mask, poll, read_op;

                        special_mask = (byte)((IO.Value & 0x60) >> 5);
                        poll = (byte)((IO.Value & 0x04) >> 2);
                        read_op = (byte)(IO.Value & 0x03);
                        if (poll > 0)
                        {
                            s.slave_pic.polled = true;
                            return;
                        }
                        if (read_op == 0x02) /* read IRR */
                            s.slave_pic.read_reg_select = 0;
                        else if (read_op == 0x03) /* read ISR */
                            s.slave_pic.read_reg_select = 1;
                        if (special_mask == 0x02)
                        { /* cancel special mask */
                            s.slave_pic.special_mask = false;
                        }
                        else if (special_mask == 0x03)
                        { /* set specific mask */
                            s.slave_pic.special_mask = true;
                            service_slave_pic();
                        }
                        return;
                    }

                    switch (IO.Value)
                    {
                        case 0x00: // Rotate in auto eoi mode clear
                        case 0x80: // Rotate in auto eoi mode set
                            s.slave_pic.rotate_on_autoeoi = (IO.Value != 0);
                            break;

                        case 0xA0: // Rotate on non-specific end of interrupt
                        case 0x20: /* end of interrupt command */
                            clear_highest_interrupt(ref s.slave_pic);

                            if (IO.Value == 0xA0)
                            {// Rotate in Auto-EOI mode
                                s.slave_pic.lowest_priority++;
                                if (s.slave_pic.lowest_priority > 7)
                                    s.slave_pic.lowest_priority = 0;
                            }

                            service_slave_pic();
                            break;

                        case 0x40: // Intel PIC spec-sheet seems to indicate this should be ignored
                            //BX_INFO(("IRQ no-op"));
                            break;

                        case 0x60: /* specific EOI 0 */
                        case 0x61: /* specific EOI 1 */
                        case 0x62: /* specific EOI 2 */
                        case 0x63: /* specific EOI 3 */
                        case 0x64: /* specific EOI 4 */
                        case 0x65: /* specific EOI 5 */
                        case 0x66: /* specific EOI 6 */
                        case 0x67: /* specific EOI 7 */
                            s.slave_pic.isr &= (byte)~(1 << (int)(IO.Value - 0x60));
                            service_slave_pic();
                            break;

                        // IRQ lowest priority commands
                        case 0xC0: // 0 7 6 5 4 3 2 1
                        case 0xC1: // 1 0 7 6 5 4 3 2
                        case 0xC2: // 2 1 0 7 6 5 4 3
                        case 0xC3: // 3 2 1 0 7 6 5 4
                        case 0xC4: // 4 3 2 1 0 7 6 5
                        case 0xC5: // 5 4 3 2 1 0 7 6
                        case 0xC6: // 6 5 4 3 2 1 0 7
                        case 0xC7: // 7 6 5 4 3 2 1 0
                            //BX_INFO(("IRQ lowest command 0x%x", IO.Value));
                            s.slave_pic.lowest_priority = (byte)(IO.Value - 0xC0);
                            break;

                        case 0xE0: // specific EOI and rotate 0
                        case 0xE1: // specific EOI and rotate 1
                        case 0xE2: // specific EOI and rotate 2
                        case 0xE3: // specific EOI and rotate 3
                        case 0xE4: // specific EOI and rotate 4
                        case 0xE5: // specific EOI and rotate 5
                        case 0xE6: // specific EOI and rotate 6
                        case 0xE7: // specific EOI and rotate 7
                            s.slave_pic.isr &= (byte)~(1 << (int)(IO.Value - 0xE0));
                            s.slave_pic.lowest_priority = (byte)(IO.Value - 0xE0);
                            service_slave_pic();
                            break;

                        case 0x02: // single mode bit: 1 = single, 0 = cascade
                            // ignore. 386BSD writes this IO.Value but works with it ignored.
                            break;

                        default:
                            throw new Exception(("write to port A0h = " + IO.Value.ToString("X2")));
                    } /* switch (IO.Value) */
                    break;

                case 0xA1:
                    /* initialization mode operation */
                    if (s.slave_pic.init.in_init)
                    {
                        switch (s.slave_pic.init.byte_expected)
                        {
                            case 2:
                                s.slave_pic.interrupt_offset = (byte)(IO.Value & 0xf8);
                                s.slave_pic.init.byte_expected = 3;
                                if (mParent.mSystem.Debuggies.DebugPIC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "slave: init command 2 = " + IO.Value.ToString("X2"));
                                if (mParent.mSystem.Debuggies.DebugPIC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "       offset = INT " + s.slave_pic.interrupt_offset.ToString("X2"));
                                return;
                            case 3:
                                if (mParent.mSystem.Debuggies.DebugPIC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "slave: init command 3 = " + IO.Value.ToString("X2"));
                                if (s.slave_pic.init.requires_4)
                                {
                                    s.slave_pic.init.byte_expected = 4;
                                }
                                else
                                {
                                    s.slave_pic.init.in_init = false;
                                }
                                return;
                            case 4:
                                if (mParent.mSystem.Debuggies.DebugPIC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "slave: init command 4 = " + IO.Value.ToString("X2"));
                                if ((IO.Value & 0x02) == 0x02)
                                {
                                    if (mParent.mSystem.Debuggies.DebugPIC)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "       auto EOI");
                                    s.slave_pic.auto_eoi = 1;
                                }
                                else
                                {
                                    if (mParent.mSystem.Debuggies.DebugPIC)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "normal EOI interrupt");
                                    s.slave_pic.auto_eoi = 0;
                                }
                                if ((IO.Value & 0x01) == 0x01)
                                {
                                    if (mParent.mSystem.Debuggies.DebugPIC)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "       80x86 mode");
                                }
                                else
                                    throw new Exception(("       not 80x86 mode"));
                                s.slave_pic.init.in_init = false;
                                return;
                            default:
                                throw new Exception(("slave: expecting bad init command"));
                        }
                    }

                    /* normal operation */
                    if (mParent.mSystem.Debuggies.DebugPIC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "setting slave pic IMR to " + IO.Value.ToString("X2"));
                    s.slave_pic.imr = (byte)(IO.Value);
                    service_slave_pic();
                    return;
            } /* switch (IO.Portnum) */
        }
        public override void DeviceThread()
        {
            return;
        }
        #endregion

        #region cPIC Methods
        void clear_highest_interrupt(ref pic_t pic)
        {
            int irq;
            int lowest_priority;
            int highest_priority;

            /* clear highest current in service bit */
            lowest_priority = pic.lowest_priority;
            highest_priority = lowest_priority + 1;
            if (highest_priority > 7)
                highest_priority = 0;

            irq = highest_priority;
            do
            {
                if ((pic.isr & (1 << irq)) > 0)
                {
                    pic.isr &= (byte)~(1 << irq);
                    break; /* Return mask of bit cleared. */
                }

                irq++;
                if (irq > 7)
                    irq = 0;
            } while (irq != highest_priority);
        }
        void service_master_pic()
        {
            lock (syncRoot)
            {
                byte unmasked_requests;
                int irq;
                byte isr, max_irq;
                byte highest_priority = (byte)(s.master_pic.lowest_priority + 1);
                if (highest_priority > 7)
                    highest_priority = 0;

                if (s.master_pic.INT)
                { /* last interrupt still not acknowleged */
                    return;
                }

                isr = s.master_pic.isr;
                if (s.master_pic.special_mask)
                {
                    /* all priorities may be enabled.  check all IRR bits except ones
                     * which have corresponding ISR bits set
                     */
                    max_irq = highest_priority;
                }
                else
                { /* normal mode */
                    /* Find the highest priority IRQ that is enabled due to current ISR */
                    max_irq = highest_priority;
                    if (isr > 0)
                    {
                        while ((isr & (1 << max_irq)) == 0)
                        {
                            max_irq++;
                            if (max_irq > 7)
                                max_irq = 0;
                        }
                        if (max_irq == highest_priority)
                        {
                            if (mParent.mSystem.Debuggies.DebugPIC)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "Service_Master: Highest priority interrupt in-service, no other priorities allowed. ISR=" + s.master_pic.isr.ToString("X2") + ", IRR = " + s.master_pic.irr.ToString("X2") + ", IMR = " + s.master_pic.imr.ToString("X2"));
                            return; /* Highest priority interrupt in-service,
                                     * no other priorities allowed */
                        }
                        if (max_irq > 7)
                            throw new Exception(("error in service_master_pic()"));
                    }
                }

                /* now, see if there are any higher priority requests */
                unmasked_requests = (byte)((s.master_pic.irr & ~s.master_pic.imr));
                if (unmasked_requests > 0)
                {
                    irq = highest_priority;
                    do
                    {
                        /* for special mode, since we're looking at all IRQ's, skip if
                         * current IRQ is already in-service
                         */
                        if (!(s.master_pic.special_mask == true && ((isr >> irq) & 0x01) == 0x1))
                        {
                            if ((unmasked_requests & (1 << irq)) > 0)
                            {
                                if (mParent.mSystem.Debuggies.DebugPIC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "signalling IRQ " + irq);
                                s.master_pic.INT = true;
                                s.master_pic.irq = (byte)irq;
                                //TODO: BX_RAISE_INTR();
                                mParent.mProc.Signals.INTR = true;
                                return;
                            } /* if (unmasked_requests & ... */
                        }

                        irq++;
                        if (irq > 7)
                            irq = 0;
                    } while (irq != max_irq); /* do ... */
                } /* if (unmasked_requests = ... */
            } //lock
        }
        void service_slave_pic()
        {

            lock (syncRoot)
            {

                byte unmasked_requests;
                int irq;
                byte isr, max_irq;
                byte highest_priority = (byte)(s.slave_pic.lowest_priority + 1);
                if (highest_priority > 7)
                    highest_priority = 0;

                if (s.slave_pic.INT)
                { /* last interrupt still not acknowleged */
                    return;
                }

                isr = s.slave_pic.isr;
                if (s.slave_pic.special_mask)
                {
                    /* all priorities may be enabled.  check all IRR bits except ones
                     * which have corresponding ISR bits set
                     */
                    max_irq = highest_priority;
                }
                else
                { /* normal mode */
                    /* Find the highest priority IRQ that is enabled due to current ISR */
                    max_irq = highest_priority;
                    if (isr > 0)
                    {
                        while ((isr & (1 << max_irq)) == 0)
                        {
                            max_irq++;
                            if (max_irq > 7)
                                max_irq = 0;
                        }
                        if (max_irq == highest_priority) return; /* Highest priority interrupt in-service,
                                                * no other priorities allowed */
                        if (max_irq > 7)
                            throw new Exception(("error in service_slave_pic()"));
                    }
                }

                /* now, see if there are any higher priority requests */
                unmasked_requests = (byte)((s.slave_pic.irr & ~s.slave_pic.imr));
                if (unmasked_requests > 0)
                {
                    irq = highest_priority;
                    do
                    {
                        /* for special mode, since we're looking at all IRQ's, skip if
                         * current IRQ is already in-service
                         */
                        if (!(s.slave_pic.special_mask == true && ((isr >> irq) & 0x01) == 0x1))
                        {
                            if ((unmasked_requests & (1 << irq)) > 0)
                            {
                                if (mParent.mSystem.Debuggies.DebugPIC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "slave: signalling IRQ " + (8 + irq));

                                s.slave_pic.INT = true;
                                s.slave_pic.irq = (byte)irq;
                                //TODO: raise_irq(2); /* request IRQ 2 on master pic */
                                RaiseIRQ(2);
                                return;
                            } /* if (unmasked_requests & ... */
                        }

                        irq++;
                        if (irq > 7)
                            irq = 0;

                    }
                    while (irq != max_irq); /* do ... */
                } /* if (unmasked_requests = ... */
            }//lock
        }
        internal void LowerIRQ(byte irq_no)
        {
            lock (syncRoot)
            {
                byte mask = (byte)(1 << (irq_no & 7));
                if ((irq_no <= 7) && ((s.master_pic.IRQ_in & mask) == mask))
                {
                    if (mParent.mSystem.Debuggies.DebugPIC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "IRQ line " + irq_no + " now low.  ISR=" + s.master_pic.isr.ToString("X2") + ", IRR = " + s.master_pic.irr.ToString("X2") + ", IMR = " + s.master_pic.imr.ToString("X2"));

                    s.master_pic.IRQ_in &= (byte)~(mask);
                    s.master_pic.irr &= (byte)~(mask);
                }
                else if ((irq_no > 7) && (irq_no <= 15) && ((s.slave_pic.IRQ_in & mask) == mask))
                {
                    if (mParent.mSystem.Debuggies.DebugPIC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "IRQ line " + irq_no + " now low.  ISR=" + s.master_pic.isr.ToString("X2") + ", IRR = " + s.master_pic.irr.ToString("X2") + ", IMR = " + s.master_pic.imr.ToString("X2"));
                    s.slave_pic.IRQ_in &= (byte)~(mask);
                    s.slave_pic.irr &= (byte)~(mask);
                }
            }
        }
        internal void RaiseIRQ(byte irq_no)
        {
            lock (syncRoot)
            {
                byte mask = (byte)(1 << (irq_no & 7));
                if ((irq_no <= 7) /*&& !((s.master_pic.IRQ_in & mask) == mask)*/)
                {
                    if (mParent.mSystem.Debuggies.DebugPIC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "IRQ line " + irq_no + " now high.  ISR=" + s.master_pic.isr.ToString("X2") + ", IRR = " + s.master_pic.irr.ToString("X2") + ", IMR = " + s.master_pic.imr.ToString("X2"));
                    s.master_pic.IRQ_in |= mask;
                    s.master_pic.irr |= mask;
                    service_master_pic();
                }
                else if ((irq_no > 7) && (irq_no <= 15) &&
                         !((s.slave_pic.IRQ_in & mask) == mask))
                {
                    if (mParent.mSystem.Debuggies.DebugPIC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "IRQ line " + irq_no + " now high.  ISR=" + s.master_pic.isr.ToString("X2") + ", IRR = " + s.master_pic.irr.ToString("X2") + ", IMR = " + s.master_pic.imr.ToString("X2"));
                    s.slave_pic.IRQ_in |= mask;
                    s.slave_pic.irr |= mask;
                    service_slave_pic();
                }
            }
        }
        internal byte PassIRQVectorToCPU()
        {
            byte vector;
            byte irq;

            lock (syncRoot)
            {
                mParent.mProc.Signals.INTR = false;
                s.master_pic.INT = false;
                // Check for spurious interrupt
                if (s.master_pic.irr == 0)
                {
                    if (mParent.mSystem.Debuggies.DebugPIC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "PassIRQVectorToCPU: Spurious interrupt. ISR=" + s.master_pic.isr.ToString("X2") + ", IRR = " + s.master_pic.irr.ToString("X2") + ", IMR = " + s.master_pic.imr.ToString("X2"));
                    return (byte)(s.master_pic.interrupt_offset + 7);
                }
                // In level sensitive mode don't clear the irr bit.
                //TODO: Verify next line
                if ((s.master_pic.edge_level & (1 << s.master_pic.irq)) == 0)
                    s.master_pic.irr &= (byte)~(1 << s.master_pic.irq);
                // In autoeoi mode don't set the isr bit.
                if (s.master_pic.auto_eoi == 0)
                    s.master_pic.isr |= (byte)(1 << s.master_pic.irq);
                else if (s.master_pic.rotate_on_autoeoi)
                    s.master_pic.lowest_priority = s.master_pic.irq;

                if (s.master_pic.irq != 2)
                {
                    irq = s.master_pic.irq;
                    vector = (byte)(irq + s.master_pic.interrupt_offset);
                }
                else
                { /* IRQ2 = slave pic IRQ8..15 */
                    s.slave_pic.INT = false;
                    s.master_pic.IRQ_in &= 0xFB; //~(1 << 2);
                    // Check for spurious interrupt
                    if (s.slave_pic.irr == 0)
                    {
                        return (byte)(s.slave_pic.interrupt_offset + 7);
                    }
                    irq = s.slave_pic.irq;
                    vector = (byte)(irq + s.slave_pic.interrupt_offset);
                    // In level sensitive mode don't clear the irr bit.
                    if ((s.slave_pic.edge_level & (1 << s.slave_pic.irq)) == 0)
                        s.slave_pic.irr &= (byte)~(1 << s.slave_pic.irq);
                    // In autoeoi mode don't set the isr bit.
                    if (s.slave_pic.auto_eoi == 0)
                        s.slave_pic.isr |= (byte)(1 << s.slave_pic.irq);
                    else if (s.slave_pic.rotate_on_autoeoi)
                        s.slave_pic.lowest_priority = s.slave_pic.irq;
                    service_slave_pic();
                    irq += 8; // for debug printing purposes
                }
                if (mParent.mSystem.Debuggies.DebugPIC)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "PassIRQVectorToCPU: Passing CPU vector " + vector + " so that it can service IRQ # " + irq + ". ISR=" + s.master_pic.isr.ToString("X2") + ", IRR = " + s.master_pic.irr.ToString("X2") + ", IMR = " + s.master_pic.imr.ToString("X2"));
            }
            service_master_pic();
            return (vector);
        }
        internal bool RegisterIRQ(cDevice Device, int IRQNumber)
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
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, lTemp.ToString());
                return false;
            }
            mIRQOwner[IRQNumber] = Device.DeviceName;
            if (mParent.mSystem.Debuggies.DebugPIC)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.PIC, "IRQ# " + IRQNumber + " registered to " + Device.DeviceName);
            return true;
        }
        #endregion
    }
    public struct sInit
    {
        public bool in_init;
        public bool requires_4;
        public byte byte_expected;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct pic_t
    {
        [FieldOffset(0)]
        public byte single_PIC;        /* 0=cascaded PIC, 1=master only */
        [FieldOffset(1)]
        public byte interrupt_offset;  /* programmable interrupt vector offset */
        [FieldOffset(0)]
        public byte slave_connect_mask; /* for master, a bit for each interrupt line
                                   0=not connect to a slave, 1=connected */
        [FieldOffset(1)]
        public byte slave_id;           /* for slave, id number of slave PIC */
        [FieldOffset(2)]
        public byte sfnm;              /* specially fully nested mode: 0=no, 1=yes*/
        [FieldOffset(3)]
        public byte buffered_mode;     /* 0=no buffered mode, 1=buffered mode */
        [FieldOffset(4)]
        public byte master_slave;      /* master/slave: 0=slave PIC, 1=master PIC */
        [FieldOffset(5)]
        public byte auto_eoi;          /* 0=manual EOI, 1=automatic EOI */
        [FieldOffset(6)]
        public byte imr;               /* interrupt mask register, 1=masked */
        [FieldOffset(7)]
        public byte isr;               /* in service register */
        [FieldOffset(8)]
        public byte irr;               /* interrupt request register */
        [FieldOffset(9)]
        public byte read_reg_select;   /* 0=IRR, 1=ISR */
        [FieldOffset(10)]
        public byte irq;               /* current IRQ number */
        [FieldOffset(11)]
        public byte lowest_priority;   /* current lowest priority irq */
        [FieldOffset(12)]
        public bool INT;             /* INT request pin of PIC */
        [FieldOffset(13)]
        public byte IRQ_in;            /* IRQ pins of PIC */
        [FieldOffset(14)]
        public bool special_mask;
        [FieldOffset(15)]
        public bool polled;            /* Set when poll command is issued. */
        [FieldOffset(16)]
        public bool rotate_on_autoeoi; /* Set when should rotate in auto-eoi mode. */
        [FieldOffset(17)]
        public byte edge_level; /* bitmap for irq mode (0=edge, 1=level) */
        [FieldOffset(18)]
        public sInit init;
    }

    struct sCombinedPic
    {
        public pic_t master_pic;
        public pic_t slave_pic;
    }
}
