using System;
using System.Linq;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;

namespace VirtualProcessor.Devices
{
    public class cDMA : cDevice, iDevice
    {

        #region Private Variables, Constants & Definitions
        struct StateInfo
        {
            public Boolean[] mask;
            public Boolean flip_flop;
            public byte status_reg;
            public byte command_reg;
            public byte request_reg;
            public byte temporary_reg;
            public sChannel[] chan;

            public StateInfo(byte Dummy)
            {
                mask = new Boolean[4];
                flip_flop = false;
                status_reg = 0;
                command_reg = 0;
                request_reg = 0;
                temporary_reg = 0;
                chan = new sChannel[4];
            }
        }
        struct sMode
        {
            public byte mode_type;
            public byte address_decrement;
            public byte autoinit_enable;
            public byte transfer_type;
        }
        struct sChannel
        {
            public sMode mode;
            public Word base_address;
            public Word current_address;
            public Word base_count;
            public Word current_count;
            public byte page_reg;
        }

        StateInfo s;
        byte[] channelindex = new byte[7] { 2, 3, 1, 0, 0, 0, 0 };
        public const int DMA_MODE_DEMAND = 0, DMA_MODE_SINGLE = 1, DMA_MODE_BLOCK = 2, DMA_MODE_CASCADE = 3;


        #endregion

        public cDMA(cDeviceBlock DevBlock)
        {
            s = new StateInfo(0);
            mParent = DevBlock;
            mDeviceId = "246c3fd0-f836-11de-8a39-0800200c9a66";
            mDeviceClass = eDeviceClass.Floppy;
            mName = "DMA";
        }

        #region DMA Methods
        public void DRQ(byte channel, bool val)
        {
            DWord dma_base, dma_roof;

            if (channel != 2)
                mParent.mSystem.HandleException(this, new Exception("bx_dma_c::DRQ(): channel " + channel.ToString("X4") + " != 2"));
            if (!val)
            {
                //mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA,"bx_dma_c::DRQ(): val == 0");
                // clear bit in status reg
                // deassert HRQ if not pending DRQ's ?
                // etc.
                s.status_reg &= (byte)(~(1 << (channel + 4)));
                return;
            }
            if (mParent.mSystem.Debuggies.DebugDMA)
            {
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "s.mask[2]: " + s.mask[2]);
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "s.flip_flop: " + s.flip_flop);
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "s.status_reg: " + s.status_reg);
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "mode_type: " + s.chan[channel].mode.mode_type);
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "address_decrement: " + s.chan[channel].mode.address_decrement);
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "autoinit_enable: " + s.chan[channel].mode.autoinit_enable);
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "transfer_type: " + s.chan[channel].mode.transfer_type);
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, ".base_address: " + s.chan[channel].base_address);
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, ".current_address: " + s.chan[channel].current_address);
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, ".base_count: " + s.chan[channel].base_count);
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, ".current_count: " + s.chan[channel].current_count);
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, ".page_reg: " + s.chan[channel].page_reg);
            }
            s.status_reg |= (byte)(1 << (channel + 4));


            if ((s.chan[channel].mode.mode_type != DMA_MODE_SINGLE) &&
                 (s.chan[channel].mode.mode_type != DMA_MODE_DEMAND))
                mParent.mSystem.HandleException(this, new Exception("bx_dma_c::DRQ: mode_type " + s.chan[channel].mode.mode_type + " not handled"));
            if (s.chan[channel].mode.address_decrement != 0)
                mParent.mSystem.HandleException(this, new Exception("bx_dma_c::DRQ: address_decrement != 0"));

            dma_base = (DWord)(s.chan[channel].page_reg << 16) | s.chan[channel].base_address;
            dma_roof = (dma_base + s.chan[channel].base_count);
            
            if (((dma_base & 0xffff0000) != (dma_roof & 0xffff0000)) & mParent.mSystem.Debuggies.DebugDMA)
            {
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "dma_base = " + dma_base);
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "dma_base_count = " + s.chan[channel].base_count);
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "dma_roof = " + dma_roof);
                mParent.mSystem.HandleException(this, new Exception(" DMA request outside 64k boundary"));
            }

            //mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA,"DRQ set up for single mode, increment, auto-init disabled, write");
            // should check mask register VS DREQ's in status register here?
            // assert Hold ReQuest line to CPU
            mParent.mProc.Signals.HOLD = true;
        }
        public void RaiseHLDA()
        {
            byte channel;
            DWord phy_addr;
            Boolean count_expired = false;

            // find highest priority channel
            for (channel = 0; channel < 4; channel++)
            {
                if (((s.status_reg & (1 << (channel + 4))) != 0) && (s.mask[channel] == false))
                {
                    break;
                }
            }
            if (channel >= 4)
            {
                // don't panic, just wait till they're unmasked
                //    bx_panic("hlda: no unmasked requests\n");
                return;
            }

            //bx_printf("hlda: OK in response to DRQ(%u)\n", (unsigned) channel);
            phy_addr = (DWord)(s.chan[channel].page_reg << 16) | s.chan[channel].current_address;
            mParent.mProc.DACK[channel] = true;
            // check for expiration of count, so we can signal TC and DACK(n)
            // at the same time.
            if (s.chan[channel].mode.address_decrement == 0)
            {
                // address increment
                s.chan[channel].current_address++;
                s.chan[channel].current_count--;
                if (s.chan[channel].current_count == 0xffff)
                    if (s.chan[channel].mode.autoinit_enable == 0)
                    {
                        // count expired, done with transfer
                        // assert TC, deassert HRQ & DACK(n) lines
                        s.status_reg |= (byte)(1 << channel); // hold TC in status reg
                        mParent.mProc.TC = true;

                        count_expired = true;
                    }
                    else
                    {
                        // count expired, but in autoinit mode
                        // reload count and base address
                        s.chan[channel].current_address =
                          s.chan[channel].base_address;
                        s.chan[channel].current_count =
                          s.chan[channel].base_count;
                    }

            }
            else
            {
                mParent.mSystem.HandleException(this, new Exception("hlda: decrement not implemented\n"));
            }

            if (s.chan[channel].mode.transfer_type == 1)
            { // write
                // xfer from I/O to Memory
                mParent.DMAWriteByte(phy_addr, channel);
            }
            else if (s.chan[channel].mode.transfer_type == 2)
            { // read
                // xfer from Memory to I/O
                mParent.DMAReadByte(phy_addr, channel);
            }
            else
            {
                mParent.mSystem.HandleException(this, new Exception("hlda: transfer_type of " + s.chan[channel].mode.transfer_type + " not handled"));
            }

            if (count_expired)
            {
                mParent.mProc.TC = false;           // clear TC, adapter card already notified
                mParent.mProc.Signals.HOLD = false;
                mParent.mProc.DACK[channel] = false;
                if (mParent.mSystem.Debuggies.DebugDMA)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "DMA Transfer completed");
            }
        }
        #endregion

        #region cDevice Interface Related Methods
        public override void InitDevice()
        {
            int lCounter = 0;
            //SetC up IO handlers and PIC Registration and then call parent InitDevice which will register IO handlers
            mIOHandlers = new sIOHandler[61];
            // 0000..000F
            for (Word c = 0; c <= 0x0F; c++)
            {
                mIOHandlers[lCounter].Device = this; mIOHandlers[lCounter].PortNum = c; mIOHandlers[lCounter].Direction = eDataDirection.IO_InOut;
                lCounter++;
            }
            // 00081..008D
            for (Word c = 0x81; c <= 0x8D; c++)
            {
                mIOHandlers[lCounter].Device = this; mIOHandlers[lCounter].PortNum = c; mIOHandlers[lCounter].Direction = eDataDirection.IO_InOut;
                lCounter++;
            }
            // 008F
            mIOHandlers[lCounter].Device = this; mIOHandlers[lCounter].PortNum = 0x8F; mIOHandlers[lCounter].Direction = eDataDirection.IO_InOut;
            lCounter++;

            // 000C0..00DE
            for (Word c = 0xC0; c <= 0xDE; c++)
            {
                mIOHandlers[lCounter].Device = this; mIOHandlers[lCounter].PortNum = c; mIOHandlers[lCounter].Direction = eDataDirection.IO_InOut;
                lCounter++;
            }

            s.mask[0] = true; // channel 0 masked
            s.mask[1] = true; // channel 1 masked
            s.mask[2] = true; // channel 2 masked
            s.mask[3] = true; // channel 3 masked
            s.flip_flop = false; /* cleared */
            s.status_reg = 0; // no requests, no terminal counts reached
            for (int c = 0; c < 4; c++)
            {
                s.chan[c].mode.mode_type = 0;         // demand mode
                s.chan[c].mode.address_decrement = 0; // address increment
                s.chan[c].mode.autoinit_enable = 0;   // autoinit disable
                s.chan[c].mode.transfer_type = 0;     // verify
                s.chan[c].base_address = 0;
                s.chan[c].current_address = 0;
                s.chan[c].base_count = 0;
                s.chan[c].current_count = 0;
                s.chan[c].page_reg = 0;
            }
            //mParent.mPIC.RegisterIRQ(this, 6);
            base.InitDevice();
        }
        public override void ResetDevice()
        {
            throw new Exception("Fill me in!!!");
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
            {
                byte retval;
                byte channel;

                if (mParent.mSystem.Debuggies.DebugDMA)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, " read addr=" + IO.Portnum.ToString("X4"));

                switch (IO.Portnum)
                {
                    case 0x00: /* DMA-1 current address, channel 0 */
                    case 0x02: /* DMA-1 current address, channel 1 */
                    case 0x04: /* DMA-1 current address, channel 2 */
                    case 0x06: /* DMA-1 current address, channel 3 */
                        channel = (byte)(IO.Portnum >> 1);
                        if (s.flip_flop == false)
                        {
                            s.flip_flop = !s.flip_flop;
                            lock (mParent.mProc.ports.mPorts)
                                mParent.mProc.ports.mPorts[0x06] = (byte)(s.chan[channel].current_address & 0xff);
                        }
                        else
                        {
                            s.flip_flop = !s.flip_flop;
                            lock (mParent.mProc.ports.mPorts)
                                mParent.mProc.ports.mPorts[0x06] = (byte)(s.chan[channel].current_address >> 8);
                        }
                        return;
                    case 0x01: /* DMA-1 current count, channel 0 */
                    case 0x03: /* DMA-1 current count, channel 1 */
                    case 0x05: /* DMA-1 current count, channel 2 */
                    case 0x07: /* DMA-1 current count, channel 3 */
                        channel = (byte)(IO.Portnum >> 1);
                        if (s.flip_flop == false)
                        {
                            s.flip_flop = !s.flip_flop;
                            lock (mParent.mProc.ports.mPorts)
                                mParent.mProc.ports.mPorts[0x07] = (byte)(s.chan[channel].current_count & 0xff);
                        }
                        else
                        {
                            s.flip_flop = !s.flip_flop;
                            lock (mParent.mProc.ports.mPorts)
                                mParent.mProc.ports.mPorts[0x07] = (byte)(s.chan[channel].current_count >> 8);
                        }
                        return;
                    case 0x08: // DMA-1 Status Register
                        // bit 7: 1 = channel 3 request
                        // bit 6: 1 = channel 2 request
                        // bit 5: 1 = channel 1 request
                        // bit 4: 1 = channel 0 request
                        // bit 3: 1 = channel 3 has reached terminal count
                        // bit 2: 1 = channel 2 has reached terminal count
                        // bit 1: 1 = channel 1 has reached terminal count
                        // bit 0: 1 = channel 0 has reached terminal count
                        // reading this register clears lower 4 bits (hold flags)
                        retval = s.status_reg;
                        s.status_reg &= 0xf0;
                        lock (mParent.mProc.ports.mPorts)
                            mParent.mProc.ports.mPorts[0x08] = (retval);
                        break;
                    case 0x0d: // temporary register
                        mParent.mSystem.HandleException(this, new Exception("read of temporary register"));
                        // Note: write to 0x0D clears temporary register
                        return;

                    case 0x0081: // DMA-1 page register, channel 2
                    case 0x0082: // DMA-1 page register, channel 3
                    case 0x0083: // DMA-1 page register, channel 1
                    case 0x0087: // DMA-1 page register, channel 0
                        channel = channelindex[IO.Portnum - 0x81];
                        lock (mParent.mProc.ports.mPorts)
                            mParent.mProc.ports.mPorts[0x87] = (s.chan[channel].page_reg);
                        return;
                    case 0x0084: // ???
                        return;
                    case 0x0089: // DMA-2 page register, channel 6
                    case 0x008a: // DMA-2 page register, channel 7
                    case 0x008b: //  page register, channel 5
                    case 0x008f: // DMA-2 page register, channel 4
                        channel = (channelindex[IO.Portnum - 0x89]);
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, " read: unsupported address=" + IO.Portnum.ToString("X4") + " (channel " + channel + ")");
                        lock (mParent.mProc.ports.mPorts)
                            mParent.mProc.ports.mPorts[0x8F] = (s.chan[channel].page_reg);
                        return;

                    case 0x00c0:
                    case 0x00c2:
                    case 0x00c4:
                    case 0x00c6:
                    case 0x00c8:
                    case 0x00ca:
                    case 0x00cc:
                    case 0x00ce:
                    case 0x00d0:
                    case 0x00d2:
                    case 0x00d4:
                    case 0x00d6:
                    case 0x00d8:
                    case 0x00da:
                    case 0x00dc:
                    case 0x00de:
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, " read: unsupported address=" + IO.Portnum.ToString("X4"));
                        return;

                    default:
                        //mParent.mSystem.HandleException(this, new ExceptionNumber(" read: unsupported address=" + OpDecoder.ValueToHex(IO.Portnum, 4)));
                        return;
                }
            }
        }

        public void Handle_OUT(sPortValue IO)
        {
            {
                byte set_mask_bit;
                byte channel;

                if (IO.Size == TypeCode.UInt16)
                {
                    if ((IO.Portnum == 0x0b))
                    {
                        sPortValue lTemp = IO;
                        lTemp.Size = TypeCode.Byte;
                        lTemp.Value = (byte)(IO.Value & 0xff);
                        Handle_OUT(lTemp);
                        lTemp.Value = (byte)(IO.Value & 0xff);
                        lTemp.Value = (byte)(IO.Value >> 8);
                        Handle_OUT(lTemp);
                        return;
                    }
                    //mParent.mSystem.HandleException(this, new Exception(" io write to address " + IO.Portnum.ToString("X4") ));
                }

                //if (mParent.mSystem.Debuggies.DebugDMA)
                //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, " write: address=" + OpDecoder.ValueToHex(IO.Portnum, 4) + " Value=" + IO.Value.mWord);

                switch (IO.Portnum)
                {
                    case 0x00:
                    case 0x02:
                    case 0x04:
                    case 0x06:
                        channel = (byte)(IO.Portnum >> 1);
                        if (mParent.mSystem.Debuggies.DebugDMA)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "  DMA-2 base and current count, channel " + channel.ToString("X4"));
                        if (s.flip_flop == false)
                        { /* 1st byte */
                            s.chan[channel].base_address = (Word)IO.Value;
                            s.chan[channel].current_address = (Word)IO.Value;
                        }
                        else
                        { /* 2nd byte */
                            s.chan[channel].base_address |= (Word)(IO.Value << 8);
                            s.chan[channel].current_address |= (Word)(IO.Value << 8);
                            if (mParent.mSystem.Debuggies.DebugDMA)
                            {
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "Channel 0, 2, 4 or 6");
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "    base = " + s.chan[channel].base_address.ToString("X4"));
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "    curr = " + s.chan[channel].current_address.ToString("X4"));
                            }
                        }
                        s.flip_flop = !s.flip_flop;
                        return;

                    case 0x01:
                    case 0x03:
                    case 0x05:
                    case 0x07:
                        channel = (byte)(IO.Portnum >> 1);
                        if (mParent.mSystem.Debuggies.DebugDMA)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "  DMA-1 base and current count, channel " + channel.ToString("X4"));
                        if (s.flip_flop == false)
                        { /* 1st byte */
                            s.chan[channel].base_count = (Word)IO.Value;
                            s.chan[channel].current_count = (Word)IO.Value;
                        }
                        else
                        { /* 2nd byte */
                            s.chan[channel].base_count |= (Word)(IO.Value << 8);
                            s.chan[channel].current_count |= (Word)(IO.Value << 8);
                            if (mParent.mSystem.Debuggies.DebugDMA)
                            {
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "Channel 1, 3, 5 or 7");
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "    base = " + s.chan[channel].base_count.ToString("X4"));
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "    curr = " + s.chan[channel].current_count.ToString("X4"));
                            }
                        }
                        s.flip_flop = !s.flip_flop;
                        return;

                    case 0x08: /* command register */
                        if (IO.Value != 0x04)
                            if (mParent.mSystem.Debuggies.DebugDMA)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, " write to 0008: " + IO.Value + " (" + IO.Value.ToString("X4") + ") not 04h");
                        s.command_reg = (byte)IO.Value;
                        return;

                    case 0x09: // request register
                        if (mParent.mSystem.Debuggies.DebugDMA)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "write to request register " + IO.Value.ToString("X4"));
                        // note: write to 0x0d clears this register
                        if ((IO.Value & 0x04) != 0)
                        {
                            // set request bit
                        }
                        else
                        {
                            byte channel2;

                            // clear request bit
                            channel2 = (byte)(IO.Value & 0x03);
                            s.status_reg &= (byte)(~(1 << (channel2 + 4)));
                            //if (mParent.mSystem.Debuggies.DebugDMA)
                            //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "cleared request bit for channel " + OpDecoder.ValueToHex(channel2, 4));
                        }
                        return;

                    case 0x0a:
                        set_mask_bit = (byte)(IO.Value & 0x04);
                        channel = (byte)(IO.Value & 0x03);
                        s.mask[channel] = (set_mask_bit > 0);
                        if (mParent.mSystem.Debuggies.DebugDMA)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "set_mask_bit=" + set_mask_bit + ", channel=" + channel.ToString("X") + ", mask now=" + s.mask[channel]);
                        return;

                    case 0x0b: /* dma-1 mode register */
                        channel = (byte)(IO.Value & 0x03);
                        s.chan[channel].mode.mode_type = (byte)((IO.Value >> 6) & 0x03);
                        s.chan[channel].mode.address_decrement = (byte)((IO.Value >> 5) & 0x01);
                        s.chan[channel].mode.autoinit_enable = (byte)((IO.Value >> 4) & 0x01);
                        s.chan[channel].mode.transfer_type = (byte)((IO.Value >> 2) & 0x03);
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA,"mode register[%u] = %02x",
                        //(unsigned) channel, (unsigned) IO.Value.mWord);
                        //if (mParent.mSystem.Debuggies.DebugDMA)
                        //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "mode register" + channel + " = " + OpDecoder.ValueToHex(IO.Value.mWord, 2));
                        return;

                    case 0x0c: /* dma-1 clear byte flip/flop */
                        //if (mParent.mSystem.Debuggies.DebugDMA)
                        //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "clear flip/flop");
                        s.flip_flop = false;
                        return;

                    case 0x0d: // master disable
                        /* ??? */
                        if (mParent.mSystem.Debuggies.DebugDMA)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, " master disable");
                        // writing any IO.Value.mWord to this port resets DMA controller 1
                        // same action as a hardware reset
                        // mask register is set (chan 0..3 disabled)
                        // command, status, request, temporary, and byte flip-flop are all cleared
                        s.mask[0] = true;
                        s.mask[1] = true;
                        s.mask[2] = true;
                        s.mask[3] = true;
                        s.command_reg = 0;
                        s.status_reg = 0;
                        s.request_reg = 0;
                        s.temporary_reg = 0;
                        s.flip_flop = false;
                        return;

                    case 0x0e: // clear mask register
                        //if (mParent.mSystem.Debuggies.DebugDMA)
                        //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "clear mask register");
                        s.mask[0] = false;
                        s.mask[1] = false;
                        s.mask[2] = false;
                        s.mask[3] = false;
                        return;

                    case 0x0f: // write all mask bits
                        //if (mParent.mSystem.Debuggies.DebugDMA)
                        //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "write all mask bits");
                        s.mask[0] = System.Convert.ToBoolean(IO.Value & 0x01); IO.Value >>= 1;
                        s.mask[1] = System.Convert.ToBoolean(IO.Value & 0x01); IO.Value >>= 1;
                        s.mask[2] = System.Convert.ToBoolean(IO.Value & 0x01); IO.Value >>= 1;
                        s.mask[3] = System.Convert.ToBoolean(IO.Value & 0x01);
                        return;

                    case 0x81: /* dma page register, channel 2 */
                    case 0x82: /* dma page register, channel 3 */
                    case 0x83: /* dma page register, channel 1 */
                    case 0x87: /* dma page register, channel 0 */
                        /* address bits A16-A23 for DMA channel */
                        channel = channelindex[IO.Portnum - 0x81];
                        s.chan[channel].page_reg = (byte)IO.Value;
                        //if (mParent.mSystem.Debuggies.DebugDMA)
                        //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "page register " + channel + " = " + OpDecoder.ValueToHex(IO.Value.mWord, 2));
                        return;

                    case 0x0084: // ???
                        return;

                    //case 0xd0: /* DMA-2 command register */
                    //  if (IO.Value.mWord != 0x04)
                    //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA,"DMA2: write command register: IO.Value.mWord(%02xh)!=04h",
                    //      (unsigned) IO.Value.mWord);
                    //  return;
                    //  break;

                    case 0x00c0:
                    case 0x00c2:
                    case 0x00c4:
                    case 0x00c6:
                    case 0x00c8:
                    case 0x00ca:
                    case 0x00cc:
                    case 0x00ce:
                    case 0x00d0:
                    case 0x00d2:
                    case 0x00d4:
                    case 0x00d6:
                    case 0x00d8:
                    case 0x00da:
                    case 0x00dc:
                    case 0x00de:
                        if (mParent.mSystem.Debuggies.DebugDMA)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "DMA(ignored): write: " + IO.Portnum.ToString("X2") + " = " + IO.Value.ToString("X4"));
                        return;

                    default:
                        if (mParent.mSystem.Debuggies.DebugDMA)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.DMA, "DMA(ignored): write: " + IO.Portnum.ToString("X2") + " = " + IO.Value.ToString("X4"));
                        break;
                }
            }
        }
        public override void DeviceThread()
        {
            //while (1 == 1)
            //{
            //    Thread.Sleep(DEVICE_THREAD_SLEEP_TIMEOUT);
            //    if (mShutdownRequested)
            //        break;
            //}
        }
        #endregion
    }
}
