using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;

namespace VirtualProcessor.Devices
{
    public class cHDC : cDevice, iDevice
    {

        public bool ControllerBusy = false;
        public bool mDirectionIsOut = false;

        public cHDC(cDeviceBlock DevBlock)
        {
            mParent = DevBlock;
            mDeviceId = "4A90E00C-F76A-11DE-B0DE-940756D89591";
            mDeviceClass = eDeviceClass.HD;
            mName = "Hard Drive Controller";
        }
        #region Declarations, Variables & Constants
        char[] model_no = new char[41] { 'G', 'e', 'n', 'e', 'r', 'i', 'c', ' ', '1', '2', '3', '4', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
        const int INDEX_PULSE_CYCLE = 10;
        const int PACKET_SIZE = 12;
        uint max_multiple_sectors = 0x3f; // was 0x3f
        uint curr_multiple_sectors = 0x3f; // was 0x3f        #endregion
        byte drive_select;

        sStruct[] s = new sStruct[2];
        sStruct sTemp = new sStruct(0);

        #endregion
        #region HDC Methods
        public void increment_address()
        {
            s[drive_select].controller.sector_count--;
            if (s[drive_select].controller.lba_mode)
            {
                ulong current_address = calculate_logical_address();
                current_address++;
                s[drive_select].controller.head_no = (byte)((current_address >> 24) & 0xf);
                s[drive_select].controller.cylinder_no = (Word)((current_address >> 8) & 0xffff);
                s[drive_select].controller.sector_no = (byte)((current_address) & 0xff);
            }
            else
            {
                s[drive_select].controller.sector_no++;
                if (s[drive_select].controller.sector_no > s[drive_select].hard_drive.sectors)
                {
                    s[drive_select].controller.sector_no = 1;
                    s[drive_select].controller.head_no++;
                    if (s[drive_select].controller.head_no >= s[drive_select].hard_drive.heads)
                    {
                        s[drive_select].controller.head_no = 0;
                        s[drive_select].controller.cylinder_no++;
                        if (s[drive_select].controller.cylinder_no >= s[drive_select].hard_drive.cylinders)
                            s[drive_select].controller.cylinder_no = s[drive_select].hard_drive.cylinders - 1;
                    }
                }
            }
        }
        ulong calculate_logical_address()
        {
            ulong logical_sector;

            if (s[drive_select].controller.lba_mode)
                logical_sector = ((ulong)(s[drive_select].controller.head_no << 24)) | (ulong)(s[drive_select].controller.cylinder_no << 8) | s[drive_select].controller.sector_no;
            else
                logical_sector = (ulong)(((s[drive_select].controller.cylinder_no *
                    s[drive_select].hard_drive.heads * s[drive_select].hard_drive.sectors))
                    + ((s[drive_select].controller.head_no * s[drive_select].hard_drive.sectors)) +
                    (ulong)(s[drive_select].controller.sector_no - 1));

            if (logical_sector == 0xffffffffffffffff)
            {
                System.Diagnostics.Debug.WriteLine("NOTE: logical_sector requested = 0xffffffffffffffff");
                logical_sector = 0;
            }
            if (logical_sector >= s[drive_select].hard_drive.cylinders * s[drive_select].hard_drive.heads * s[drive_select].hard_drive.sectors)
                throw new Exception($"disk: read sectors: out of bounds (sector={logical_sector}, max={s[drive_select].hard_drive.cylinders * s[drive_select].hard_drive.heads * s[drive_select].hard_drive.sectors})\n");
            return logical_sector;

        }
        void raise_interrupt()
        {
            if (!s[drive_select].controller.control.disable_irq)
            {
                if (mParent.mSystem.Debuggies.DebugHDC)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "Raising interrupt");
                mParent.mPIC.RaiseIRQ(14);
            }
            else
                if (mParent.mSystem.Debuggies.DebugHDC)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "Interrupt masked");
        }

        void lower_interrupt()
        {
            mParent.mPIC.LowerIRQ(14);
            if (mParent.mSystem.Debuggies.DebugHDC)
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "Lowered interrupt");
        }

        void command_aborted(Word value)
        {
            if (mParent.mSystem.Debuggies.DebugHDC)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: aborting on command: " + value.ToString("X4"));
            s[drive_select].controller.current_command = 0;
            s[drive_select].controller.status.busy = false;
            s[drive_select].controller.status.drive_ready = true;
            s[drive_select].controller.status.err = true;
            s[drive_select].controller.error_register = 0x04; // command ABORTED
            s[drive_select].controller.status.drq = false;
            s[drive_select].controller.status.seek_complete = false;
            s[drive_select].controller.status.corrected_data = false;
            s[drive_select].controller.buffer_index = 0;
            //This raise is duplicating others.  Removed from here and made sure ach call to command_aboted has a raise after it
            //raise_interrupt();
        }
        void identify_drive(byte drive)
        {
            ulong i;
            UInt32 temp32;
            UInt16 temp16;

            if (drive != drive_select)
            {
                throw new Exception("disk: identify_drive panic (drive != drive_select)\n");
            }
            // Identify Drive command return values definition
            //
            // This code is rehashed from some that was donated.
            // I'm using ANSI X3.221-1994, AT Attachment Interface for Disk Drives
            // and X3T10 2008D Working Draft for ATA-3


            // Word 0: general config bit-significant info
            //   Note: bits 1-5 and 8-14 are now "Vendor specific (obsolete)"
            //   bit 15: 0=ATA device
            //           1=ATAPI device
            //   bit 14: 1=format speed tolerance gap required
            //   bit 13: 1=track offset option available
            //   bit 12: 1=data strobe offset option available
            //   bit 11: 1=rotational speed tolerance is > 0,5% (typo?)
            //   bit 10: 1=disk transfer rate > 10Mbs
            //   bit  9: 1=disk transfer rate > 5Mbs but <= 10Mbs
            //   bit  8: 1=disk transfer rate <= 5Mbs
            //   bit  7: 1=removable cartridge drive
            //   bit  6: 1=fixed drive
            //   bit  5: 1=spindle motor control option implemented
            //   bit  4: 1=head switch time > 15 usec
            //   bit  3: 1=not MFM encoded
            //   bit  2: 1=soft sectored
            //   bit  1: 1=hard sectored
            //   bit  0: 0=reserved
            s[drive_select].id_drive[0] = 0x0440; //CLR 07/09/2013 - changed from 40 to 440 for the heck of it

            // Word 1: number of user-addressable cylinders in
            //   default translation mode.  If the value in words 60-61
            //   exceed 16,515,072, this word shall contain 16,383.
            s[drive_select].id_drive[1] = (ushort)s[drive_select].hard_drive.cylinders;

            // Word 2: reserved
            s[drive_select].id_drive[2] = 0;

            // Word 3: number of user-addressable heads in default
            //   translation mode
            s[drive_select].id_drive[3] = (ushort)s[drive_select].hard_drive.heads;

            // Word 4: # unformatted bytes per translated track in default xlate mode
            // Word 5: # unformatted bytes per sector in default xlated mode
            // Word 6: # user-addressable sectors per track in default xlate mode
            // Note: words 4,5 are now "Vendor specific (obsolete)"
            s[drive_select].id_drive[4] = (ushort)(512 * s[drive_select].hard_drive.sectors);
            s[drive_select].id_drive[5] = 512;
            s[drive_select].id_drive[6] = (ushort)s[drive_select].hard_drive.sectors;

            // Word 7-9: Vendor specific
            for (i = 7; i <= 9; i++)
                s[drive_select].id_drive[i] = 0;

            // Word 10-19: Serial number (20 ASCII characters, 0000h=not specified)
            // This field is right justified and padded with spaces (20h).
            for (i = 10; i <= 19; i++)
                s[drive_select].id_drive[i] = 0;

            // Word 20: buffer type
            //          0000h = not specified
            //          0001h = single ported single sector buffer which is
            //                  not capable of simulataneous data xfers to/from
            //                  the host and the disk.
            //          0002h = dual ported multi-sector buffer capable of
            //                  simulatenous data xfers to/from the host and disk.
            //          0003h = dual ported mutli-sector buffer capable of
            //                  simulatenous data xfers with a read caching
            //                  capability.
            //          0004h-ffffh = reserved
            s[drive_select].id_drive[20] = 0;

            // Word 21: buffer size in 512 byte increments, 0000h = not specified
            s[drive_select].id_drive[21] = 512; // 512 Sectors = 256kB cache

            // Word 22: # of ECC bytes available on read/write long cmds
            //          0000h = not specified
            s[drive_select].id_drive[22] = 4;

            // Word 23..26: Firmware revision (8 ascii chars, 0000h=not specified)
            // This field is left justified and padded with spaces (20h)
            for (i = 23; i <= 26; i++)
                s[drive_select].id_drive[i] = 0;

            // Word 27..46: Model number (40 ascii chars, 0000h=not specified)
            // This field is left justified and padded with spaces (20h)
            //  for (i=27; i<=46; i++)
            //    s[drive_select].id_drive[i] = 0;
            for (i = 0; i < 20; i++)
            {
                s[drive_select].id_drive[27 + i] = (ushort)((model_no[i * 2] << 8) |
                                              model_no[i * 2 + 1]);
            }

            // Word 47: 15-8 Vendor unique
            //           7-0 00h= read/write multiple commands not implemented
            //               xxh= maximum # of sectors that can be transferred
            //                    per interrupt on read and write multiple commands
            s[drive_select].id_drive[47] = (ushort)max_multiple_sectors;

            // Word 48: 0000h = cannot perform dword IO
            //          0001h = can    perform dword IO
            s[drive_select].id_drive[48] = 255;

            // Word 49: Capabilities
            //   15-10: 0 = reserved
            //       9: 1 = LBA supported
            //       8: 1 = DMA s11upported
            //     7-0: Vendor unique
            s[drive_select].id_drive[49] = (UInt16)1 << 9; //was 0 -- then 100

            // Word 50: Reserved
            s[drive_select].id_drive[50] = 0;

            // Word 51: 15-8 PIO data transfer cycle timing mode
            //           7-0 Vendor unique
            s[drive_select].id_drive[51] = 0x200;

            // Word 52: 15-8 DMA data transfer cycle timing mode
            //           7-0 Vendor unique
            s[drive_select].id_drive[52] = 0;  //0x200;

            // Word 53: 15-1 Reserved
            //             0 1=the fields reported in words 54-58 are valid
            //               0=the fields reported in words 54-58 may be valid
            s[drive_select].id_drive[53] = 0;

            // Word 54: # of user-addressable cylinders in curr xleate mode
            // Word 55: # of user-addressable heads in curr xlate mode
            // Word 56: # of user-addressable sectors/track in curr xlate mode
            s[drive_select].id_drive[54] = (ushort)s[drive_select].hard_drive.cylinders;
            s[drive_select].id_drive[55] = (ushort)s[drive_select].hard_drive.heads;
            s[drive_select].id_drive[56] = (ushort)s[drive_select].hard_drive.sectors;

            // Word 57-58: Current capacity in sectors
            // Excludes all sectors used for device specific purposes.
            temp32 = (uint)(
              s[drive_select].hard_drive.cylinders *
              s[drive_select].hard_drive.heads *
              s[drive_select].hard_drive.sectors);
            s[drive_select].id_drive[57] = (ushort)(temp32 & 0xffff); // LSW
            s[drive_select].id_drive[58] = (ushort)(temp32 >> 16);    // MSW

            // Word 59: 15-9 Reserved
            //             8 1=multiple sector setting is valid
            //           7-0 current setting for number of sectors that can be
            //               transferred per interrupt on R/W multiple commands
            s[drive_select].id_drive[59] = (ushort)(0x0000 | curr_multiple_sectors);

            // Word 60-61:
            // If drive supports LBA Mode, these words reflect total # of user
            // addressable sectors.  This value does not depend on the current
            // drive geometry.  If the drive does not support LBA mode, these
            // words shall be set to 0.
            ulong num_sects = s[drive_select].hard_drive.cylinders * s[drive_select].hard_drive.heads * s[drive_select].hard_drive.sectors;
            s[drive_select].id_drive[60] = (ushort)(num_sects & 0xffff); // LSW
            s[drive_select].id_drive[61] = (ushort)(num_sects >> 16); // MSW

            // Word 62: 15-8 single word DMA transfer mode active
            //           7-0 single word DMA transfer modes supported
            // The low order byte identifies by bit, all the Modes which are
            // supported e.g., if Mode 0 is supported bit 0 is set.
            // The high order byte contains a single bit set to indiciate
            // which mode is active.
            s[drive_select].id_drive[62] = 0x0;

            // Word 63: 15-8 multiword DMA transfer mode active
            //           7-0 multiword DMA transfer modes supported
            // The low order byte identifies by bit, all the Modes which are
            // supported e.g., if Mode 0 is supported bit 0 is set.
            // The high order byte contains a single bit set to indiciate
            // which mode is active.
            s[drive_select].id_drive[63] = 0x0;

            // Word 64-79 Reserved
            for (i = 64; i <= 79; i++)
                s[drive_select].id_drive[i] = 0;

            // Word 80: 15-5 reserved
            //             4 supports ATA/ATAPI-4
            //             3 supports ATA-3
            //             2 supports ATA-2
            //             1 supports ATA-1
            //             0 reserved
            s[drive_select].id_drive[80] = /*( 1 << 3) |*/ (1 << 2) | (1 << 1);

            // Word 81: Minor version number
            s[drive_select].id_drive[81] = 0;

            // Word 82: 15 obsolete
            //          14 NOP command supported
            //          13 READ BUFFER command supported
            //          12 WRITE BUFFER command supported
            //          11 obsolete
            //          10 Host protected area feature set supported
            //           9 DEVICE RESET command supported
            //           8 SERVICE interrupt supported
            //           7 release interrupt supported
            //           6 look-ahead supported
            //           5 write cache supported
            //           4 supports PACKET command feature set
            //           3 supports power management feature set
            //           2 supports removable media feature set
            //           1 supports securite mode feature set
            //           0 support SMART feature set
            s[drive_select].id_drive[82] = 1 << 14;
            s[drive_select].id_drive[83] = 1 << 14;
            s[drive_select].id_drive[84] = 1 << 14;
            s[drive_select].id_drive[85] = 1 << 14;
            s[drive_select].id_drive[86] = 0;
            s[drive_select].id_drive[87] = 1 << 14;

            for (i = 88; i <= 127; i++)
                s[drive_select].id_drive[i] = 0;

            // Word 128-159 Vendor unique
            for (i = 128; i <= 159; i++)
                s[drive_select].id_drive[i] = 0;

            // Word 160-255 Reserved
            for (i = 160; i <= 255; i++)
                s[drive_select].id_drive[i] = 0;
            if (mParent.mSystem.Debuggies.DebugHDC)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "Drive ID Info. initialized");
            // now convert the id_drive array (native 256 word format) to
            // the controller buffer (512 bytes)
            for (i = 0; i <= 255; i++)
            {
                temp16 = s[drive_select].id_drive[i];
                s[drive_select].controller.buffer[i * 2] = (byte)(temp16 & 0x00ff);
                s[drive_select].controller.buffer[i * 2 + 1] = (byte)(temp16 >> 8);
            }
        }
        #endregion

        #region cDevice Interface Related Methods
        public override void InitDevice()
        {
            int lCounter = 0;
            s[0] = new sStruct(0);
            s[1] = new sStruct(0);
            s[0].hard_drive = new default_image_t();
            s[1].hard_drive = new default_image_t();

            mName = "Hard disk controller";
            if (mParent.mSystem.mDriveCount >= 1)
            {
                s[0].hard_drive.cylinders = mParent.mSystem.Drives[0].Cylinders;
                s[0].hard_drive.heads = mParent.mSystem.Drives[0].Heads;
                s[0].hard_drive.sectors = mParent.mSystem.Drives[0].SectorsPerTrack;
                s[0].device_type = mParent.mSystem.Drives[0].DeviceType;
                s[0].hard_drive.open(mParent.mSystem.Drives[0].PathAndFile);
            }
            if (mParent.mSystem.mDriveCount == 2)
            {
                s[1].hard_drive.cylinders = mParent.mSystem.Drives[1].Cylinders;
                s[1].hard_drive.heads = mParent.mSystem.Drives[1].Heads;
                s[1].hard_drive.sectors = mParent.mSystem.Drives[1].SectorsPerTrack;
                s[1].device_type = mParent.mSystem.Drives[1].DeviceType;
                s[1].hard_drive.open(mParent.mSystem.Drives[1].PathAndFile);
            }

            for (int id = 0; id < 2; id++)
            {
                s[id].controller.status.busy = false;
                s[id].controller.status.drive_ready = true;
                s[id].controller.status.write_fault = false;
                s[id].controller.status.seek_complete = true;
                s[id].controller.status.drq = false;
                s[id].controller.status.corrected_data = false;
                s[id].controller.status.index_pulse = false;
                s[id].controller.status.index_pulse_count = 0;
                s[id].controller.status.err = false;

                s[id].controller.error_register = 0x01; // diagnostic code: no error
                s[id].controller.head_no = 0;
                s[id].controller.sector_count = 1;
                s[id].controller.sector_no = 1;
                s[id].controller.cylinder_no = 0;
                s[id].controller.current_command = 0x00;
                s[id].controller.buffer_index = 0;

                s[id].controller.control.reset = false;
                s[id].controller.control.disable_irq = false;

                s[id].controller.reset_in_progress = 0;

                s[id].controller.sectors_per_block = 0x80;
                s[id].controller.lba_mode = false;

                s[id].controller.features = 0;
            }
            mIOHandlers = new sIOHandler[10];
            for (Word c = 0x1f0; c <= 0x1f7; c++)
            {
                mIOHandlers[lCounter].Device = this; mIOHandlers[lCounter].PortNum = c; mIOHandlers[lCounter].Direction = eDataDirection.IO_InOut;
                lCounter++;
            }
            mIOHandlers[lCounter].Device = this; mIOHandlers[lCounter].PortNum = 0x03f6; mIOHandlers[lCounter].Direction = eDataDirection.IO_InOut;
            lCounter++;
            mIOHandlers[lCounter].Device = this; mIOHandlers[lCounter].PortNum = 0x03f7; mIOHandlers[lCounter].Direction = eDataDirection.IO_InOut;
            lCounter++;
            drive_select = 0;
            mParent.mPIC.RegisterIRQ(this, 14);
            base.InitDevice();
            DeviceThreadSleep = 10;
        }
        public override void RequestShutdown()
        {
            base.RequestShutdown();
            do { Thread.Sleep(100); } while (DeviceThreadActive);
            if (mParent.mSystem.mDriveCount >= 1)
            {
                s[0].hard_drive.close();
            }
            if (mParent.mSystem.mDriveCount == 2)
            {
                s[1].hard_drive.close();
            }
        }
        public override void HandleIO(sPortValue IO, eDataDirection Direction)
        {
            if (mParent.mSystem.Debuggies.DebugHDC)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "NEW: D=" + Direction + ", P=" + IO.Portnum.ToString("X4") + ", V=" + IO.Value.ToString("X8"));

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
            byte value8 = 0;
            UInt16 value16 = 0;
            UInt32 value32 = 0;

            //clr 07/14/2013 - Remarked out just to see ..
            //if (!(IO.Size == TypeCode.Byte) && IO.Portnum != 0x1f0)
            //    throw new Exception("disk: non-byte IO read to: " + IO.Portnum.ToString("X4"));
            switch (IO.Portnum)
            {
                case 0x1f0: // hard disk data (16bit)
                    if (s[drive_select].controller.status.drq == false)
                    {
                        if (mParent.mSystem.Debuggies.DebugHDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: IO read(1f0h) with drq == 0: last command was " + s[drive_select].controller.current_command.ToString("X4"));
                        return;
                    }
                    //throw new Exception("disk: IO read(1f0h) with drq == 0: last command was " + s[drive_select].controller.current_command.ToString("X4"));
                    switch (s[drive_select].controller.current_command)
                    {
                        case 0x20:
                        case 0x21:
                            //if (IO.Size != TypeCode.UInt16)
                            //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: non-word IO read from " + IO.Portnum.ToString("X4"));
                            //throw new Exception("disk: non-word IO read from " + IO.Portnum.ToString("X4"));
                            ControllerBusy = true;
                            if (s[drive_select].controller.buffer_index >= 512)
                                throw new Exception("disk: IO read(1f0): buffer_index >= 512\n");

                            if (s[drive_select].controller.buffer_index == 2 && (IO.Size == TypeCode.UInt32))
                                throw new Exception("You are here!");
                            mDirectionIsOut = false;
                            value32 = s[drive_select].controller.buffer[s[drive_select].controller.buffer_index];
                            value32 |= (Word)(s[drive_select].controller.buffer[s[drive_select].controller.buffer_index + 1] << 8);
                            s[drive_select].controller.buffer_index += 2;
                            if (IO.Size == TypeCode.UInt32)
                            {
                                value32 |= (UInt32)(s[drive_select].controller.buffer[s[drive_select].controller.buffer_index] << 16);
                                value32 |= (UInt32)(s[drive_select].controller.buffer[s[drive_select].controller.buffer_index + 1] << 24);
                                s[drive_select].controller.buffer_index += 2;
                            }

                            // if buffer completely read
                            if (s[drive_select].controller.buffer_index >= 512)
                            {
                                // update sector count, sector number, cylinder,
                                // drive, head, status
                                // if there are more sectors, read next one in...xxx
                                s[drive_select].controller.buffer_index = 0;
                                increment_address();
                                s[drive_select].controller.status.busy = false;
                                s[drive_select].controller.status.drive_ready = true;
                                s[drive_select].controller.status.write_fault = false;
                                //if (bx_options.newHardDriveSupport)
                                s[drive_select].controller.status.seek_complete = true;
                                s[drive_select].controller.status.corrected_data = false;
                                s[drive_select].controller.status.err = false;
                                if (s[drive_select].controller.sector_count == 0)
                                {
                                    ControllerBusy = false;
                                    s[drive_select].controller.status.drq = false;
                                }
                                else
                                {
                                    ulong logical_sector;
                                    long ret;

                                    s[drive_select].controller.status.drq = true;
                                    s[drive_select].controller.status.seek_complete = true;
                                    //Per BOCHS code I turned off the seek because at this point in the code we should be at the right offset in the file.
                                    logical_sector = calculate_logical_address();
                                    //ret = s[drive_select].hard_drive.lseek((long)(logical_sector * 512), SeekOrigin.Begin);
                                    //if (ret < 0)
                                    //    throw new Exception("disk: could not lseek() hard drive image file, logical sector = " + logical_sector);
                                    if (mParent.mSystem.Debuggies.DebugHDC)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "Reading HD" + drive_select + ", phys sect: " + logical_sector.ToString("X8"));
                                    ControllerBusy = true;
                                    ret = s[drive_select].hard_drive.read(ref s[drive_select].controller.buffer, 512);
                                    if (ret < 512)
                                        throw new Exception("disk: could not read() hard drive image file, logical sector = ");
                                    s[drive_select].controller.buffer_index = 0;
                                    raise_interrupt();
                                }
                            }
                            switch (IO.Size)
                            {
                                case TypeCode.Byte:
                                    value8 = (byte)value32;
                                    goto return_value8;
                                case TypeCode.UInt16:
                                    value16 = (UInt16)value32;
                                    goto return_value16;
                                default:
                                    goto return_value32;
                            }
                        case 0xec:    // Drive ID Command
                        case 0xa1:
                            //if (bx_options.newHardDriveSupport)
                            ulong index;

                            s[drive_select].controller.status.busy = false;
                            s[drive_select].controller.status.drive_ready = true;
                            s[drive_select].controller.status.write_fault = false;
                            s[drive_select].controller.status.seek_complete = false;
                            s[drive_select].controller.status.corrected_data = false;
                            s[drive_select].controller.status.err = false;

                            index = s[drive_select].controller.buffer_index;
                            value32 = s[drive_select].controller.buffer[index];
                            index++;
                            if (IO.Size != TypeCode.Byte)
                            {
                                value32 |= (UInt32)(s[drive_select].controller.buffer[index] << 8);
                                index++;
                            }
                            if (IO.Size == TypeCode.UInt32)
                            {
                                value32 |= (UInt32)(s[drive_select].controller.buffer[index] << 16);
                                value32 |= (UInt32)(s[drive_select].controller.buffer[index + 1] << 24);
                                index += 2;
                            }
                            s[drive_select].controller.buffer_index = (uint)index;
                            if (s[drive_select].controller.buffer_index >= 512)
                            {
                                s[drive_select].controller.status.drq = false;
                                if (mParent.mSystem.Debuggies.DebugHDC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "Read all drive ID Bytes ...");
                            }
                            switch (IO.Size)
                            {
                                case TypeCode.Byte:
                                    value8 = (byte)value32;
                                    goto return_value8;
                                case TypeCode.UInt16:
                                    value16 = (UInt16)value32;
                                    goto return_value16;
                                default:
                                    goto return_value32;
                            }

                        case 0xa0: //ATAPI??
                            break;
                        default:
                            throw new Exception("disk: IO read(1f0h): current command is: " + s[drive_select].controller.current_command.ToString("X4"));
                    }
                    break;
                case 0x1f1: // hard disk error register
                    s[drive_select].controller.status.err = false;
                    value8 = s[drive_select].controller.error_register;
                    goto return_value8;
                    break;

                case 0x1f2: // hard disk sector count / interrupt reason
                    //if (s[drive_select].controller.current_command == 0x20 ||
                    //    s[drive_select].controller.current_command == 0x21 ||
                    //    s[drive_select].controller.current_command == 0x30 ||
                    //    s[drive_select].controller.current_command == 0xA0)

                    if (s[drive_select].controller.sector_count > 0)
                    {
                        //s[drive_select].controller.features = sTemp.controller.features; sTemp.controller.features = 0;
                        //s[drive_select].controller.sector_count = sTemp.controller.sector_count; sTemp.controller.sector_count = 0;
                        //s[drive_select].controller.sector_no = sTemp.controller.sector_no; sTemp.controller.sector_no = 0;
                        //s[drive_select].controller.cylinder_no = sTemp.controller.cylinder_no; sTemp.controller.cylinder_no = 0;


                        s[drive_select].controller.head_no = (byte)(IO.Value & 0xf);
                        if (!s[drive_select].controller.lba_mode && ((IO.Value >> 6) & 1) == 1)
                            if (mParent.mSystem.Debuggies.DebugHDC)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: enabling LBA mode\n");
                        s[drive_select].controller.lba_mode = ((IO.Value >> 6) & 1) == 1;
                        sTemp = new sStruct();
                    }
                    value8 = s[drive_select].controller.sector_count;
                    goto return_value8;
                    throw new Exception("disk: IO read(0x1f2): current command not read/write");
                    break;
                case 0x1f3: // sector number
                    value8 = s[drive_select].controller.sector_no;
                    goto return_value8;
                case 0x1f4: // cylinder low
                    value8 = (byte)(s[drive_select].controller.cylinder_no & 0x00ff);
                    goto return_value8;
                case 0x1f5: // cylinder high
                    value8 = (byte)(s[drive_select].controller.cylinder_no >> 8);
                    goto return_value8;
                case 0x1f6: // hard disk drive and head register
                    value8 = (byte)((1 << 7) | // extended data field for ECC
                             (0 << 7) | // 1=LBA mode, 0=CHSmode
                             (1 << 5) | // 01b = 512 sector size
                             (drive_select << 4) |
                             (s[drive_select].controller.head_no << 0));
                    goto return_value8;
                case 0x1f7: // Hard Disk Status
                case 0x3f6: // Hard Disk Alternate Status
                    if (IO.Portnum == 0x1f7)
                            mParent.mSystem.DeviceBlock.mPIC.LowerIRQ(14);
                    if (drive_select == 1 && mParent.mSystem.mDriveCount == 1)
                        value8 = 0;
                    else
                    {
                        value8 = (byte)(
                          (System.Convert.ToByte(s[drive_select].controller.status.busy) << 7) +
                          (System.Convert.ToByte(s[drive_select].controller.status.drive_ready) << 6) +
                          (System.Convert.ToByte(s[drive_select].controller.status.write_fault) << 5) +
                          (System.Convert.ToByte(s[drive_select].controller.status.seek_complete) << 4) +
                          (System.Convert.ToByte(s[drive_select].controller.status.drq) << 3) +
                          (System.Convert.ToByte(s[drive_select].controller.status.corrected_data) << 2) +
                          (System.Convert.ToByte(s[drive_select].controller.status.index_pulse) << 1) +
                          (System.Convert.ToByte(s[drive_select].controller.status.err)));
                    }
                    s[drive_select].controller.status.index_pulse_count++;
                    s[drive_select].controller.status.index_pulse = false;
                    if (s[drive_select].controller.status.index_pulse_count >= INDEX_PULSE_CYCLE)
                    {
                        s[drive_select].controller.status.index_pulse = true;
                        s[drive_select].controller.status.index_pulse_count = 0;
                    }
                    if (mParent.mSystem.Debuggies.DebugHDC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "Status query, reply=" + value8.ToString("X2"));
                    goto return_value8;
                case 0x3f7: // Hard Disk Address Register
                    // Obsolete and unsupported register.  Not driven by hard
                    // disk controller.  Report all 1's.  If floppy controller
                    // is handling this address, it will call this function
                    // set/clear D7 (the only bit it handles), then return
                    // the combined value
                    value8 = 0xff;
                    goto return_value8;
                    break;
                default:
                    throw new Exception("hard drive: io read to address " + IO.Portnum.ToString("X4") + " unsupported");
            }
            throw new Exception("hard drive: shouldnt get here!");
        return_value32:
            //if (mParent.mSystem.Debuggies.DebugHDC)
            //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "32-bit read from " + IO.Portnum.ToString("X4"));
            mParent.mProc.ports.mPorts[IO.Portnum] = value32;
            return;
        return_value16:
            //if (mParent.mSystem.Debuggies.DebugHDC)
            //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "16-bit read from " + IO.Portnum.ToString("X4"));
            mParent.mProc.ports.mPorts[IO.Portnum] = value16;
            return;
        return_value8:
            //if (mParent.mSystem.Debuggies.DebugHDC)
            //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "8-bit read from " + IO.Portnum.ToString("X4"));
            mParent.mProc.ports.mPorts[IO.Portnum] = value8;
            return;
        }
        public void Handle_OUT(sPortValue IO)
        {
            ulong logical_sector;
            long ret;
            Boolean prev_control_reset;

            if (IO.Size != TypeCode.Byte && IO.Portnum != 0x1f0)
                throw new Exception("disk: non-byte IO write to: " + IO.Portnum.ToString("X4"));
            if (mParent.mSystem.Debuggies.DebugHDC)
            {
                StringBuilder msg = new StringBuilder();
                switch (IO.Size)
                {
                    case TypeCode.Byte:
                        msg.AppendFormat("8-bit write to {0} = {1}",
                              IO.Portnum.ToString("X4"), IO.Value.ToString("X4"));
                        break;
                    case TypeCode.UInt16:
                        msg.AppendFormat("16-bit write to {0} = {1}",
                              IO.Portnum.ToString("X4"), IO.Value.ToString("X4"));
                        break;
                    case TypeCode.UInt32:
                        msg.AppendFormat("32-bit write to {0} = {1}",
                              IO.Portnum.ToString("X4"), IO.Value.ToString("X4"));
                        break;
                    default:
                        msg.AppendFormat("Unknown write to {0} = {1}",
                              IO.Portnum.ToString("X4"), IO.Value.ToString("X4"));
                        break;
                }
            }

            switch (IO.Portnum)
            {

                case 0x1f0:
                    if (IO.Size != TypeCode.UInt16) //(clr: removed check for byte value 10/30)
                    {

                        if (mParent.mSystem.Debuggies.DebugHDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: non-word IO write to " + IO.Portnum.ToString("X4"));
                        //throw new Exception("disk: non-word IO write to " + IO.Portnum.ToString("X4"));
                    }
                    switch (s[drive_select].controller.current_command)
                    {
                        case 0x30:
                            ControllerBusy = true;
                            if (s[drive_select].controller.buffer_index >= 512)
                                throw new Exception("disk: IO read(1f0): buffer_index >= 512\n");
                            s[drive_select].controller.buffer[s[drive_select].controller.buffer_index] = (byte)IO.Value;
                            s[drive_select].controller.buffer[s[drive_select].controller.buffer_index + 1] = (byte)(IO.Value >> 8);
                            s[drive_select].controller.buffer_index += 2;
                            mDirectionIsOut = true;
                            if (IO.Size == TypeCode.UInt32)
                            {
                                s[drive_select].controller.buffer[s[drive_select].controller.buffer_index] = (byte)(IO.Value >> 16);
                                s[drive_select].controller.buffer[s[drive_select].controller.buffer_index + 1] = (byte)(IO.Value >> 24);
                                s[drive_select].controller.buffer_index += 2;
                            }
                            /* if buffer completely writtten */
                            if (s[drive_select].controller.buffer_index >= 512)
                            {
                                logical_sector = calculate_logical_address();
                                ret = s[drive_select].hard_drive.lseek((long)(logical_sector * 512), SeekOrigin.Begin);
                                if (ret < 0)
                                    throw new Exception("disk: could not lseek() hard drive image file, logical sector = " + logical_sector);
                                if (mParent.mSystem.Debuggies.DebugHDC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "Writing HD" + drive_select + ", phys sect: " + logical_sector.ToString("X8"));
                                ret = s[drive_select].hard_drive.write(ref s[drive_select].controller.buffer, 512);
                                if (ret < 0)
                                    throw new Exception("disk: could not write() hard drive image file, logical sector = " + logical_sector);
                                s[drive_select].controller.buffer_index = 0;
                                /* update sector count, sector number, cylinder,
                                 * drive, head, status
                                 * if there are more sectors, read next one in...*/
                                increment_address();
                                /* When the write is complete, controller clears the DRQ bit and
                                 * sets the BSY bit.
                                 * If at least one more sector is to be written, controller sets DRQ bit,
                                 * clears BSY bit, and issues IRQ 14*/
                                if (s[drive_select].controller.sector_count != 0)
                                {
                                    s[drive_select].controller.status.busy = false;
                                    s[drive_select].controller.status.drive_ready = true;
                                    s[drive_select].controller.status.drq = true;
                                    s[drive_select].controller.status.corrected_data = false;
                                    s[drive_select].controller.status.err = false;
                                }
                                else
                                { /* no more sectors to write */
                                    s[drive_select].controller.status.busy = false;
                                    s[drive_select].controller.status.drive_ready = true;
                                    s[drive_select].controller.status.drq = false;
                                    s[drive_select].controller.status.err = false;
                                    s[drive_select].controller.status.corrected_data = false;
                                    ControllerBusy = false;

                                }
                                raise_interrupt();

                            }
                            break;
                        case 0xa0: // PACKET
                            break;
                    }
                    break;
                case 0x1f1: /* hard disk write precompensation */
                    //sTemp.controller.features = IO.Value.mByte;
                    s[drive_select].controller.features = (byte)IO.Value;
                    break;
                case 0x1f2: /* hard disk sector count */
                    //sTemp.controller.sector_count = IO.Value.mByte;
                    s[drive_select].controller.sector_count = (byte)IO.Value;
                    break;
                case 0x1f3: /* hard disk sector number */
                    //sTemp.controller.sector_no = IO.Value.mByte;
                    s[drive_select].controller.sector_no = (byte)IO.Value;
                    break;
                case 0x1f4: /* hard disk cylinder low */
                    //sTemp.controller.cylinder_no = sTemp.controller.cylinder_no & 0xff00 | IO.Value.mByte;
                    s[drive_select].controller.cylinder_no = s[drive_select].controller.cylinder_no & 0xff00 | (byte)IO.Value;
                    break;
                case 0x1f5: /* hard disk cylinder high */
                    //sTemp.controller.cylinder_no = (Word)((IO.Value.mByte << 8) | (byte)(sTemp.controller.cylinder_no & 0xff));
                    s[drive_select].controller.cylinder_no = (Word)((IO.Value << 8) | (byte)(s[drive_select].controller.cylinder_no & 0xff));
                    break;
                case 0x1f6: // hard disk drive and head register
                    // b7 1
                    // b6 1=LBA mode, 0=CHS mode (LBA not supported)
                    // b5 1
                    // b4: DRV
                    // b3..0 HD3..HD0
                    drive_select = (byte)((IO.Value >> 4) & 0x01);

                    //if (sTemp.controller.sector_count > 0)
                    //{
                    //    s[drive_select].controller.features = sTemp.controller.features; sTemp.controller.features = 0;
                    //    s[drive_select].controller.sector_count = sTemp.controller.sector_count; sTemp.controller.sector_count = 0;
                    //    s[drive_select].controller.sector_no = sTemp.controller.sector_no; sTemp.controller.sector_no = 0;
                    //    s[drive_select].controller.cylinder_no = sTemp.controller.cylinder_no; sTemp.controller.cylinder_no = 0;
                    //    sTemp = new sStruct();
                    //}

                    s[drive_select].controller.head_no = (byte)(IO.Value & 0xf);
                    if (!s[drive_select].controller.lba_mode && ((IO.Value >> 6) & 1) == 1)
                        if (mParent.mSystem.Debuggies.DebugHDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: enabling LBA mode\n");
                    s[drive_select].controller.lba_mode = ((IO.Value >> 6) & 1) == 1;
                    break;
                case 0x1f7: // hard disk command
                    // (mch) Writes to the command register with drive_select != 0
                    // are ignored if no secondary device is present
                    // Writes to the command register clear the IRQ
                    if (drive_select != 0 && IO.Value != 0x90 && mParent.mSystem.mDriveCount == 1)
                        break;
                    if (s[drive_select].controller.status.busy)
                        throw new Exception("hard disk: command sent, controller BUSY");
                    if ((IO.Value & 0xf0) == 0x10)
                        IO.Value = 0x10;
                    switch (IO.Value)
                    {
                        case 0x10: // calibrate drive
                            if (s[drive_select].device_type != device_type_t.IDE_DISK)
                                throw new Exception("disk: calibrate drive issued to non-disk");
                            if (drive_select != 0 && mParent.mSystem.mDriveCount == 1)
                            {
                                s[drive_select].controller.error_register = 0x02; // Track 0 not found
                                s[drive_select].controller.status.busy = false;
                                s[drive_select].controller.status.drive_ready = true;
                                s[drive_select].controller.status.seek_complete = false;
                                s[drive_select].controller.status.drq = false;
                                s[drive_select].controller.status.err = true;
                                raise_interrupt();
                                if (mParent.mSystem.Debuggies.DebugHDC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: calibrate drive != 0, with diskd not present");
                                break;
                            }
                            /* move head to cylinder 0, issue IRQ 14 */
                            s[drive_select].controller.error_register = 0;
                            s[drive_select].controller.cylinder_no = 0;
                            s[drive_select].controller.status.busy = false;
                            s[drive_select].controller.status.drive_ready = true;
                            s[drive_select].controller.status.seek_complete = true;
                            s[drive_select].controller.status.drq = false;
                            s[drive_select].controller.status.err = false;
                            if (mParent.mSystem.Debuggies.DebugHDC)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: calibrate drive request complete");
                            raise_interrupt();
                            break;
                        case 0x20: // read multiple sectors, with retries
                        case 0x21: // read multiple sectors, without retries
                            /* update sector_no, always points to current sector
                             * after each sector is read to buffer, DRQ bit set and issue IRQ 14
                             * if interrupt handler transfers all data words into main memory,
                             * and more sectors to read, then set BSY bit again, clear DRQ and
                             * read next sector into buffer
                             * sector count of 0 means 256 sectors
                             */
                            if (s[drive_select].device_type != device_type_t.IDE_DISK)
                                throw new Exception("disk: read multiple issued to non-disk");
                            s[drive_select].controller.current_command = (byte)IO.Value;

                            // Lose98 accesses 0/0/0 in CHS mode
                            //if (s[drive_select].controller.lba_mode == false &&
                            //    s[drive_select].controller.head_no == 0 &&
                            //    s[drive_select].controller.cylinder_no == 0 &&
                            //    s[drive_select].controller.sector_no == 0)
                            //{
                            //    if (mParent.mSystem.Debuggies.DebugHDC)
                            //        mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: Read from 0/0/0, aborting command\n");
                            //    command_aborted(IO.Value.mWord);
                            //    break;xxx
                            //}
                            logical_sector = calculate_logical_address();
                            ret = s[drive_select].hard_drive.lseek((long)(logical_sector * 512), SeekOrigin.Begin);
                            if (ret < 0)
                                throw new Exception("disk: could not lseek() hard drive image file, logical sector = " + logical_sector);
                            if (mParent.mSystem.Debuggies.DebugHDC)
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "Reading HD" + drive_select + ", phys sect: " + logical_sector.ToString("X8"));
                            ControllerBusy = true;
                            ret = s[drive_select].hard_drive.read(ref s[drive_select].controller.buffer, 512);
                            if (ret < 512)
                                throw new Exception("disk: could not read() hard drive image file, logical sector = " + logical_sector);
                            s[drive_select].controller.error_register = 0;
                            s[drive_select].controller.status.busy = false;
                            s[drive_select].controller.status.drive_ready = true;
                            s[drive_select].controller.status.seek_complete = true;
                            s[drive_select].controller.status.drq = true;
                            s[drive_select].controller.status.corrected_data = false;
                            s[drive_select].controller.status.err = false;
                            s[drive_select].controller.buffer_index = 0;
                            raise_interrupt();
                            break;
                        case 0x30: /* write multiple sectors, with retries */
                            /* update sector_no, always points to current sector
                             * after each sector is read to buffer, DRQ bit set and issue IRQ 14
                             * if interrupt handler transfers all data words into main memory,
                             * and more sectors to read, then set BSY bit again, clear DRQ and
                             * read next sector into buffer
                             * sector count of 0 means 256 sectors*/
                            if (s[drive_select].device_type != device_type_t.IDE_DISK)
                                throw new Exception("disk: write multiple issued to non-disk");

                            if (s[drive_select].controller.status.busy)
                                throw new Exception("disk: write command: BSY bit set");
                            s[drive_select].controller.current_command = (byte)IO.Value;
                            // implicit seek done :^)
                            s[drive_select].controller.error_register = 0;
                            s[drive_select].controller.status.busy = false;
                            // s[drive_select].controller.status.drive_ready = 1;
                            s[drive_select].controller.status.seek_complete = true;
                            s[drive_select].controller.status.drq = true;
                            s[drive_select].controller.status.err = false;
                            s[drive_select].controller.buffer_index = 0;
                            break;
                        case 0x90: // Drive Diagnostic
                            if (s[drive_select].controller.status.busy)
                            {
                                throw new Exception("disk: diagnostic command: BSY bit set");
                            }
                            if (s[drive_select].device_type != device_type_t.IDE_DISK)
                                throw new Exception("disk: drive diagnostics issued to non-disk");
                            s[drive_select].controller.error_register = 0x81; // Drive 1 failed, no error on drive 0
                            // s[drive_select].controller.status.busy = 0; // not needed
                            s[drive_select].controller.status.drq = false;
                            s[drive_select].controller.status.err = false;
                            break;
                        case 0x91: // initialize drive parameters
                            if (s[drive_select].controller.status.busy)
                            {
                                throw new Exception("disk: init drive parameters command: BSY bit set");
                            }
                            if (s[drive_select].device_type != device_type_t.IDE_DISK)
                            {
                                if (mParent.mSystem.Debuggies.DebugHDC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: initialize drive parameters issued to non-disk");
                                command_aborted((Word)IO.Value);
                            }
                            // sets logical geometry of specified drive
                            if (mParent.mSystem.Debuggies.DebugHDC)
                            {
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "initialize drive params");
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "  sector count = " + s[drive_select].controller.sector_count);
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "  drive select = " + drive_select);
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "  head number = " + s[drive_select].controller.head_no);
                            }
                            if (drive_select != 0 && mParent.mSystem.mDriveCount == 1)
                            {
                                throw new Exception("disk: init drive params: drive != 0");
                                //s[drive_select].controller.error_register = 0x12;
                                s[drive_select].controller.status.busy = false;
                                s[drive_select].controller.status.drive_ready = true;
                                s[drive_select].controller.status.drq = false;
                                s[drive_select].controller.status.err = false;
                                if (mParent.mSystem.Debuggies.DebugHDC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: initialize drive parameters issued to non-existent disk 2");
                                raise_interrupt();
                                break;
                            }
                            if (s[drive_select].controller.sector_count != s[drive_select].hard_drive.sectors)
                            {
                                if (mParent.mSystem.Debuggies.DebugHDC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: init drive params: sector count doesnt match");
                                command_aborted((Word)IO.Value);
                            }
                            else if (s[drive_select].controller.head_no != (s[drive_select].hard_drive.heads - 1))
                            {
                                if (mParent.mSystem.Debuggies.DebugHDC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: init drive params: head number doesn't match");
                                command_aborted((Word)IO.Value);
                            }
                            s[drive_select].controller.status.busy = false;
                            s[drive_select].controller.status.drive_ready = true;
                            s[drive_select].controller.status.drq = false;
                            s[drive_select].controller.status.err = false;
                                if (mParent.mSystem.Debuggies.DebugHDC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: Raising interrupt for successful initialize ");
                            raise_interrupt();
                            break;
                        case 0xec: // Get Drive Info
                            //if (bx_options.newHardDriveSupport) 
                            {
                                if (mParent.mSystem.Debuggies.DebugHDC)
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: Drive ID Command issued : 0xec \n");

                                if (drive_select != 0 && mParent.mSystem.mDriveCount == 1)
                                {
                                    if (mParent.mSystem.Debuggies.DebugHDC)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: 2nd drive not present, aborting\n");
                                    command_aborted((Word)IO.Value);
                                    raise_interrupt();
                                    break;
                                }
                                if (s[drive_select].device_type == device_type_t.IDE_CDROM)
                                {
                                    s[drive_select].controller.head_no = 0;
                                    s[drive_select].controller.sector_count = 1;
                                    s[drive_select].controller.sector_no = 1;
                                    s[drive_select].controller.cylinder_no = 0xeb14;
                                    command_aborted((Word)0xec);
                                    raise_interrupt();
                                    break;
                                }
                                else
                                {
                                    s[drive_select].controller.current_command = (byte)IO.Value;
                                    s[drive_select].controller.error_register = 0;

                                    // See ATA/ATAPI-4, 8.12
                                    s[drive_select].controller.status.busy = false;
                                    s[drive_select].controller.status.drive_ready = true;
                                    s[drive_select].controller.status.write_fault = false;
                                    s[drive_select].controller.status.drq = true;
                                    s[drive_select].controller.status.err = false;

                                    s[drive_select].controller.status.seek_complete = true;
                                    s[drive_select].controller.status.corrected_data = false;

                                    s[drive_select].controller.buffer_index = 0;
                                    if (mParent.mSystem.Debuggies.DebugHDC)
                                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "Identify Drive processing complete");
                                    raise_interrupt();
                                    identify_drive(drive_select);
                                }
                            }
                            //    else {
                            //  if (mParent.mSystem.Debuggies.DebugHDC)
                            //      mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: old hard drive\n");
                            //      command_aborted(value);
                            //}
                            break;
                        case 0x40: //
                            //if (bx_options.newHardDriveSupport)
                            {
                                if (s[drive_select].device_type != device_type_t.IDE_DISK)
                                    throw new Exception("disk: read verify issued to non-disk\n");
                                //bx_printf("disk: Verify Command : 0x40 ! \n");
                                s[drive_select].controller.status.busy = false;
                                s[drive_select].controller.status.drive_ready = true;
                                s[drive_select].controller.status.drq = false;
                                s[drive_select].controller.status.err = false;
                                raise_interrupt();
                            }
                            //else
                            //{
                            //    bx_printf("disk: old hard drive\n");
                            //    command_aborted(value);
                            //}
                            break;
                        case 0xc6: // (mch) set multiple mode
                            if (s[drive_select].controller.sector_count != 128 &&
                            s[drive_select].controller.sector_count != 64 &&
                            s[drive_select].controller.sector_count != 32 &&
                            s[drive_select].controller.sector_count != 16 &&
                            s[drive_select].controller.sector_count != 8 &&
                            s[drive_select].controller.sector_count != 4 &&
                            s[drive_select].controller.sector_count != 2)
                            {
                                command_aborted((Word)IO.Value);
                                raise_interrupt();
                                break;
                            }
                            break;
                            if (s[drive_select].device_type != device_type_t.IDE_DISK)
                                throw new Exception("disk: set multiple mode issued to non-disk\n");

                            s[drive_select].controller.sectors_per_block = s[drive_select].controller.sector_count;
                            s[drive_select].controller.status.busy = false;
                            s[drive_select].controller.status.drive_ready = true;
                            s[drive_select].controller.status.write_fault = false;
                            s[drive_select].controller.status.drq = false;
                            s[drive_select].controller.status.err = false;
                            break;
                        // power management
                        case 0xe5: // Check power mode
                            s[drive_select].controller.status.busy = false;
                            s[drive_select].controller.status.drive_ready = true;
                            s[drive_select].controller.status.write_fault = false;
                            s[drive_select].controller.status.drq = false;
                            s[drive_select].controller.status.err = false;
                            s[drive_select].controller.sector_count = 0xff; // Active or Idle mode
                            raise_interrupt();
                            break;

                        default:
                            break;
                        //throw new Exception("IO write(1f7h): command: " + IO.Value.ToString("X4"));
                    }
                    break;
                case 0x3f6: // hard disk adapter control
                    // (mch) Even if device 1 was selected, a write to this register
                    // goes to device 0 (if device 1 is absent)
                    bool oldIRQ = s[drive_select].controller.control.disable_irq;
                    prev_control_reset = s[drive_select].controller.control.reset;
                    s[0].controller.control.reset = (IO.Value & 0x04) == 4;
                    s[1].controller.control.reset = (IO.Value & 0x04) == 4;
                    s[0].controller.control.disable_irq = (IO.Value & 0x02) == 0x2;
                    s[1].controller.control.disable_irq = (IO.Value & 0x02) == 0x2;
                    if (s[0].controller.control.disable_irq == true)
                    {
                            lower_interrupt();
                        if (mParent.mSystem.Debuggies.DebugHDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "nIEN set: Interrupts disabled");
                    }
                    else if (oldIRQ==true && s[drive_select].controller.control.disable_irq==false)
                    {
                        if (mParent.mSystem.Debuggies.DebugHDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "nIEN unset: Interrupts re-enabled");
                    }
                    if (!prev_control_reset && s[drive_select].controller.control.reset)
                    {
                        // transition from 0 to 1 causes all drives to reset
                        if (mParent.mSystem.Debuggies.DebugHDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "hard drive: RESET");

                        // (mch) SetC BSY, drive not ready
                        for (int id = 0; id < 2; id++)
                        {
                            s[id].controller.status.busy = true;
                            s[id].controller.status.drive_ready = false;
                            s[id].controller.reset_in_progress = 1;

                            s[id].controller.status.write_fault = false;
                            s[id].controller.status.seek_complete = true;
                            s[id].controller.status.drq = false;
                            s[id].controller.status.corrected_data = false;
                            s[id].controller.status.err = false;

                            s[id].controller.error_register = 0x01; // diagnostic code: no error

                            s[id].controller.current_command = 0x00;
                            s[id].controller.buffer_index = 0;

                            s[id].controller.sectors_per_block = 0x80;
                            s[id].controller.lba_mode = false;

                            s[id].controller.control.disable_irq = false;
                            lower_interrupt();
                        }
                    }
                    else if (s[drive_select].controller.reset_in_progress > 0 &&
                           !s[drive_select].controller.control.reset)
                    {
                        // Clear BSY and DRDY
                        if (mParent.mSystem.Debuggies.DebugHDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.HardDrive, "disk: Reset complete");
                        for (int id = 0; id < 2; id++)
                        {
                            s[id].controller.status.busy = false;
                            s[id].controller.status.drive_ready = true;
                            s[id].controller.reset_in_progress = 0;

                            // Device signature
                            if (s[id].device_type == device_type_t.IDE_DISK)
                            {
                                s[id].controller.head_no = 0;
                                s[id].controller.sector_count = 1;
                                s[id].controller.sector_no = 1;
                                s[id].controller.cylinder_no = 0;
                            }
                            else
                            {
                                s[id].controller.head_no = 0;
                                s[id].controller.sector_count = 1;
                                s[id].controller.sector_no = 1;
                                s[id].controller.cylinder_no = 0xeb14;
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception("hard drive: io write to address " + IO.Portnum.ToString("X4") + ": " + IO.Value.ToString("X4"));

            }
        }
        public override void DeviceThread()
        {
            //DeviceThreadActive = true;
            //while (1 == 1)
            //{
            //    if (!Busy)
            //        System.Threading.Thread.Sleep(DeviceThreadSleep);
            //    if (mShutdownRequested)
            //        break;
            //}
            DeviceThreadActive = false;
        }
        #endregion

        #region Additional Public structures and definitions
        public enum _sense
        {
            SENSE_NONE = 0, SENSE_NOT_READY = 2, SENSE_ILLEGAL_REQUEST = 5
        }
        public enum _asc
        {
            ASC_INV_FIELD_IN_CMD_PACKET = 0x24,
            ASC_MEDIUM_NOT_PRESENT = 0x3a,
            ASC_SAVING_PARAMETERS_NOT_SUPPORTED = 0x39,
            ASC_LOGICAL_BLOCK_OOR = 0x21
        }

        struct controller_t
        {
            public struct s_status
            {
                public Boolean busy;
                public Boolean drive_ready;
                public Boolean write_fault;
                public Boolean seek_complete;
                public Boolean drq;
                public Boolean corrected_data;
                public Boolean index_pulse;
                public uint index_pulse_count;
                public Boolean err;
            }
            public s_status status;
            public byte error_register;
            public byte head_no;
            public byte sector_count;
            public struct interrupt_reason
            {
                private uint reason;
                uint c_d { get { return reason & 0x01; } }
                uint i_o { get { return reason & 0x02; } }
                uint rel { get { return reason & 0x04; } }
                uint tag { get { return reason & 0xF8; } }
            }
            public byte sector_no;
            public ulong cylinder_no, byte_count;
            public byte[] buffer;
            public uint buffer_index;
            public byte current_command, sectors_per_block, reset_in_progress, features;
            public struct s_control
            {
                public bool reset;       // 0=normal, 1=reset controller
                public bool disable_irq; // 0=allow irq, 1=disable irq
            }
            public s_control control;
            public bool lba_mode;
            public controller_t(int dummy)
            {
                cylinder_no = byte_count = sector_count = sector_no = head_no = error_register =
                    current_command = sectors_per_block = reset_in_progress = features
                    = 0;
                lba_mode = false;
                buffer_index = 0;
                buffer = new byte[65535];
                status = new s_status();
                control = new s_control();
            }
        }
        struct sense_info_t
        {
            _sense sense_key;
            struct information
            {
                byte[] arr;
                information(int dummy)
                { arr = new byte[4]; }
            }
            struct specific_inf
            {
                byte[] arr;
                specific_inf(int dummy)
                { arr = new byte[4]; }
            }
            struct key_spec
            {
                byte[] arr;
                key_spec(int dummy)
                { arr = new byte[4]; }

            }
            byte fruc;
            byte asc;
            byte ascq;
        }
        struct error_recovery_t
        {
            byte[] data;
            error_recovery_t(int dummy)
            {
                data = new byte[8];
                data[0] = 0x01;
                data[1] = 0x06;
                data[2] = 0x00;
                data[3] = 0x05; // Try to recover 5 times
                data[4] = 0x00;
                data[5] = 0x00;
                data[6] = 0x00;
                data[7] = 0x00;
            }
        }

        abstract class device_image_t
        {
            // Open a image. Returns non-negative if successful.
            public abstract int open(string pathname);
            public abstract void close();
            public abstract long lseek(long offset, SeekOrigin whence);
            public abstract int read(ref byte[] buf, int count);
            public abstract int write(ref byte[] buf, int count);
            public ulong cylinders;
            public ulong heads;
            public ulong sectors;
        }
        class default_image_t : device_image_t
        {
            FileStream fd;

            public override int open(string pathname)
            {
                fd = new FileStream(pathname, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return 0;
            }

            public override void close()
            {
                fd.Close();
            }

            public override long lseek(long offset, SeekOrigin whence)
            {
                return fd.Seek(offset, whence);
            }

            public override int read(ref byte[] buf, int count)
            {
                int lRetVal = 0;
                lRetVal = fd.Read(buf, 0, 512);
                if (lRetVal > 512)
                    lRetVal = 512;
                return lRetVal;
            }

            public override int write(ref byte[] buf, int count)
            {
                fd.Write(buf, 0, count);
                fd.Flush();
                return 0;
            }



        }
        struct sStruct
        {
            public default_image_t hard_drive;
            public device_type_t device_type;
            // 512 byte buffer for ID drive command
            // These words are stored in native word endian format, as
            // they are fetched and returned via a return(), so
            // there's no need to keep them in x86 endian format.
            public Word[] id_drive;
            public controller_t controller;
            //cdrom_t cdrom;
            public sense_info_t sense;
            public sStruct(int dummy)
            {
                hard_drive = new default_image_t();
                controller = new controller_t(0);
                id_drive = new Word[256];
                sense = new sense_info_t();
                device_type = new device_type_t();
            }

            struct atapi_t
            {
                byte command;
                int drq_bytes;
                int total_bytes_remaining;
            };

        }
        #endregion
    }
    public enum device_type_t
    {
        IDE_DISK, IDE_CDROM, FLOPPY
    }
}
