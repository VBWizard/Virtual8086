using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;
//OLD Code
namespace VirtualProcessor.Devices
{
    public class cFloppyDevice : cDevice, iDevice
    {

        #region Declarations, Variables & Constants
        public enum sResetSource
        {
            HardReset,
            SoftReset
        }
        private const int CONFIG_PORT = 0x3F7, DATA_PORT = 0x03F5, STATUS_PORT = 0x03F4, DIGITAL_PORT = 0x03F2,
            FD_MS_MRQ = 0x80, FD_MS_DIO = 0x40, FD_MS_NDMA = 0x20, FD_MS_BUSY = 0x10, FD_MS_ACTD = 0x08,
            FD_MS_ACTC = 0x04, FD_MS_ACTB = 0x02, FD_MS_ACTA = 0x01, FROM_FLOPPY = 10, TO_FLOPPY = 11,
            FLOPPY_DMA_CHAN = 2;

        private TimeSpan mTimerDelay = new TimeSpan(0, 0, 0, 0, 1);
        public sInfo s;
        sResetSource mLastResetSource;
        #endregion

        public cFloppyDevice(cDeviceBlock DevBlock)
        {
            mParent = DevBlock;
            mDeviceId = "4A90E00C-F76A-11DE-B0DE-940756D89593";
            mDeviceClass = eDeviceClass.Floppy;
            mName = "Floppy";
            s = new sInfo(0);
        }
        #region FDC Methods
        private void FloppyCommand()
        {
            byte step_rate_time;
            byte head_unload_time;
            byte head_load_time;
            byte motor_on;
            byte head, drive, cylinder, sector, eot;
            byte sector_size, data_length;
            QWord logical_sector;


            //if (mParent.mSystem.Debuggies.DebugFDC)
            //{
            //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " FLOPPY COMMAND: ");
            //    for (i = 0; i < s.command_size; i++)
            //        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, OpDecoder.ValueToHex(s.command[i], 2) + " ");
            //}

            /* execute phase of command is in progress (non DMA mode) */
            s.main_status_reg |= 20;

            switch (s.command[0])
            {
                case 0x03: // specify
                    step_rate_time = (byte)(s.command[1] >> 4);
                    head_unload_time = (byte)(s.command[1] & 0x0f);
                    head_load_time = (byte)(s.command[2] >> 1);
                    s.main_status_reg = FD_MS_MRQ;
                    return;

                case 0x04: // get status
                    s.result[0] = 0x00;
                    s.result_size = 1;
                    s.result_index = 0;
                    s.main_status_reg = FD_MS_MRQ | FD_MS_DIO | FD_MS_BUSY;
                    return;

                case 0x07: // recalibrate
                    drive = (byte)(s.command[1] & 0x03);
                    s.DOR &= 0xfc;
                    s.DOR |= drive;
                    if (mParent.mSystem.Debuggies.DebugFDC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "floppy_command(): recalibrate drive " + drive + "");
                    if (drive > 1)
                        mParent.mSystem.HandleException(this, new Exception("floppy_command(): drive > 1"));
                    //motor_on = s.DOR & 0xf0;
                    motor_on = (byte)((s.DOR >> (drive + 4)) & 0x01);
                    if (motor_on == 0)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " floppy_command(): recal drive with motor off");
                    if (drive == 0)
                        s.DOR |= 0x10; // turn on MOTA
                    else
                        s.DOR |= 0x20; // turn on MOTB
                    s.cylinder[drive] = 0;
                    //TODO:Deail with activate_timer
                    //bx_pc_system.activate_timer( s.floppy_timer_index, bx_options.floppy_command_delay, 0 );
                    /* command head to track 0
                     * controller set to non-busy
                     * error condition noted in Status reg 0's equipment check bit
                     * seek end bit set to 1 in Status reg 0 regardless of outcome
                     */
                    /* data reg not ready, controller busy */
                    s.main_status_reg = FD_MS_DIO | FD_MS_BUSY;
                    s.pending_command = 0x07; // recalibrate pending
                    HandleRequestAt = System.DateTime.Now + mTimerDelay;
                    return;

                case 0x08: /* sense interrupt status */
                    // mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy," floppy_command sense interrupt status");
                    /* execution:
                     *   get status
                     * result:
                     *   no interupt
                     *   byte0 = status reg0
                     *   byte1 = current cylinder number (0 to 79)
                     */
                    /*s.status_reg0 = ;*/
                    drive = (byte)(s.DOR & 0x03);
                    //07/22/2013 - changed from what's below to zero because DLX Linux didn't like the 0xC0
                    //s.result[0] = 0;
                    s.result[0] = (byte)(0x20 | drive);
                    s.result[1] = s.cylinder[drive];
                    s.result_size = 2;
                    s.result_index = 0;

                    /* read ready */
                    s.main_status_reg = FD_MS_MRQ | FD_MS_DIO | FD_MS_BUSY;
                    if (mParent.mSystem.Debuggies.DebugFDC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " sense interrupt status");
                    return;

                case 0x0f: /* seek */
                    // mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy," floppy_command seek");
                    /* command:
                     *   byte0 = 0F
                     *   byte1 = drive & head select
                     *   byte2 = cylinder number
                     * execution:
                     *   postion head over specified cylinder
                     * result:
                     *   no result bytes, issues an interrupt
                     */
                    drive = (byte)(s.command[1] & 0x03);
                    s.DOR &= 0xfc;
                    s.DOR |= drive;

                    s.head[drive] = (byte)((s.command[1] >> 2) & 0x01);
                    s.cylinder[drive] = s.command[2];
                    if (drive > 1)
                        mParent.mSystem.HandleException(this, new Exception("floppy_command(): seek: drive>1"));
                    /* ??? should also check cylinder validity */
                    /* data reg not ready, controller busy */
                    s.main_status_reg = FD_MS_DIO | FD_MS_BUSY;
                    s.pending_command = 0x0f; /* seek pending */
                    HandleRequestAt = System.DateTime.Now + mTimerDelay;
                    return;

                case 0x13: // Configure
                    if (mParent.mSystem.Debuggies.DebugFDC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " floppy io: configure (mode= " + s.command[2] +
                     ", pretrack=" + s.command[3]);
                    s.result_size = 0;
                    s.result_index = 0;
                    s.pending_command = 0;
                    s.main_status_reg = FD_MS_MRQ | FD_MS_DIO | FD_MS_BUSY;
                    return;

                case 0x4a: // read ID
                    drive = (byte)(s.command[1] & 0x03);
                    s.DOR &= 0xfc;
                    s.DOR |= drive;

                    motor_on = (byte)((s.DOR >> (drive + 4)) & 0x01);
                    if (motor_on == 0)
                        mParent.mSystem.HandleException(this, new Exception("floppy_command(): 4a: motor not on"));
                    if (drive > 1)
                        mParent.mSystem.HandleException(this, new Exception("floppy io: 4a: bad drive #"));
                    s.result_size = 7;
                    s.result_index = 0;
                    s.result[0] = 0; /* ??? */
                    s.result[1] = 0;
                    s.result[2] = 0;
                    s.result[3] = s.cylinder[drive];
                    s.result[4] = 0; /* head */
                    s.result[5] = 0; /* sector at completion */
                    s.result[6] = 2;
                    //TODO:Deail with activate_timer
                    //bx_pc_system.activate_timer( s.floppy_timer_index, bx_options.floppy_command_delay, 0 );
                    /* data reg not ready, controller busy */
                    s.main_status_reg = FD_MS_DIO | FD_MS_BUSY;
                    s.pending_command = 0x4a; /* read ID pending */
                    HandleRequestAt = System.DateTime.Now + mTimerDelay;
                    return;

                case 0xe6: // read normal data
                case 0xc5: // write normal data
                    if ((s.DOR & 0x08) == 0)
                        mParent.mSystem.HandleException(this, new Exception("read/write command with DMA and int disabled"));
                    drive = (byte)(s.command[1] & 0x03);
                    s.DOR &= 0xfc;
                    s.DOR |= drive;

                    motor_on = (byte)((s.DOR >> (drive + 4)) & 0x01);
                    //motor_on = 1;
                    if (motor_on == 0)
                        mParent.mSystem.HandleException(this, new Exception("floppy_command(): read/write: motor not on"));
                    head = (byte)(s.command[3] & 0x01);
                    cylinder = s.command[2]; /* 0..79 depending */
                    sector = s.command[4];   /* 1..36 depending */
                    eot = s.command[6];      /* 1..36 depending */
                    sector_size = s.command[5];
                    data_length = s.command[8];
                    if (mParent.mSystem.Debuggies.DebugFDC)
                    {
                        string lTemp = /*"r/w normal data" + */"BEFORE: " + "   drive    = " + drive + "   head     = " + head +
                            "   cylinder = " + cylinder + "   sector   = " + sector + "   eot      = " + eot;
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, lTemp);
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " read/write normal data");
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " BEFORE");
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "   drive    = " + drive);
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "   head     = " + head);
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "   cylinder = " + cylinder);
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "   sector   = " + sector);
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "   eot      = " + eot);
                    }
                    if (drive > 1)
                        mParent.mSystem.HandleException(this, new Exception("floppy io: bad drive #"));
                    if (head > 1)
                        mParent.mSystem.HandleException(this, new Exception("floppy io: bad head #"));

                    if (s.media_present[drive] == false)
                    {
                        // media not in drive, return error

                        //         mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy," floppy_command: attempt to read/write sector %u,"
                        //                     " sectors/track=%u", (Word) sector,
                        //                     (Word) s.media[drive].sectors_per_track);
                        s.result_size = 7;
                        s.result_index = 0;
                        s.result[0] = (byte)(0x40 | (s.head[drive] << 2) | drive); // abnormal termination
                        s.result[1] = 0x25; // 0010 0101
                        s.result[2] = 0x31; // 0011 0001
                        s.result[3] = s.cylinder[drive];
                        s.result[4] = s.head[drive];
                        s.result[5] = s.sector[drive];
                        s.result[6] = 2; // sector size = 512

                        s.pending_command = 0;
                        s.main_status_reg = FD_MS_MRQ | FD_MS_DIO | FD_MS_BUSY;
                        if (mParent.mSystem.Debuggies.DebugFDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "Raising IRQ 6 for write");
                        //CLR 08/03/2015 - Changed to use new function which recognizes the dma_and_interrupt_enable value
                        RaiseIRQ(6);
                        return;
                    }

                    if (sector_size != 0x02)
                    { // 512 bytes
                        //mParent.mSystem.HandleException(this, new Exception("sector_size not 512"));
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " floppy io: Sector size not 512, returning error");
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " *** sector # " + sector);
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " *** cylinder " + cylinder);
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " *** head #" + head);
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " *** sect count " + eot);
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " *** sect size " + sector_size);
                        s.result_size = 7;
                        s.result_index = 0;
                        s.result[0] = (byte)(0x80 | (s.head[drive] << 2) | drive); // abnormal termination - 0x80=Invalid Command Issue, (IC). Command which was issued was never started.
                        s.result[1] = 0x00; // 0010 0101
                        s.result[2] = 0x00; // 0011 0001
                        s.result[3] = s.cylinder[drive];
                        s.result[4] = s.head[drive];
                        s.result[5] = s.sector[drive];
                        s.result[6] = 2; // sector size = 512

                        s.pending_command = 0;
                        s.main_status_reg = FD_MS_MRQ | FD_MS_DIO | FD_MS_BUSY;
                        if (mParent.mSystem.Debuggies.DebugFDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "Raising IRQ 6 for write");
                        //CLR 08/03/2015 - Changed to use new function which recognizes the dma_and_interrupt_enable value
                        RaiseIRQ(6);
                        return;
                    }
                    if (cylinder >= s.media[drive].tracks)
                    {
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " floppy io: normal read/write: params out of range");
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " *** sector # " + sector);
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " *** cylinder " + cylinder);
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " *** eot #" + eot);
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " *** head #" + head);
                        mParent.mSystem.HandleException(this, new Exception("bailing"));
                        return;
                    }

                    if (sector > s.media[drive].sectors_per_track)
                    {
                        // requested sector > last sector on track
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "ERROR: floppy_command: attempt to read/write sector," + sector +
                            ", sectors/track=" + s.media[drive].sectors_per_track);
                        // set controller to where drive would have left off
                        // after it discovered the sector was past EOT
                        s.cylinder[drive] = cylinder;
                        s.head[drive] = head;
                        s.sector[drive] = (byte)(s.media[drive].sectors_per_track);

                        s.result_size = 7;

                        s.result_index = 0;
                        // 0100 0HDD abnormal termination
                        s.result[0] = (byte)(0x40 | (s.head[drive] << 2) | drive);
                        // 1000 0101 end of cyl/NDAT/NID
                        s.result[1] = 0x86;
                        // 0000 0000
                        s.result[2] = 0x00;
                        s.result[3] = s.cylinder[drive];
                        s.result[4] = s.head[drive];
                        s.result[5] = s.sector[drive];
                        s.result[6] = 2; // sector size = 512
                        s.main_status_reg = FD_MS_DIO | FD_MS_BUSY;
                        s.pending_command = s.command[0];
                        HandleRequestAt = System.DateTime.Now + mTimerDelay;
                        return;
                    }


                    //if (cylinder != s.cylinder[drive])
                    //    if (mParent.mSystem.Debuggies.DebugFDC)
                    //        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, " floppy io: cylinder request != current cylinder");

                    logical_sector = (UInt64)((cylinder * 2 * s.media[drive].sectors_per_track) +
                                     (head * s.media[drive].sectors_per_track) +
                                     (sector - 1));
                    if (mParent.mSystem.Debuggies.DebugFDC)
                    {
                        String lOp = "";
                        if (s.command[0] == 0xc5)
                            lOp = "WRITING ";
                        else
                            lOp = "READING ";
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, lOp + "logical sector " + logical_sector);
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "Logical Sector (" + logical_sector.ToString("X6") + " =  ((" + cylinder.ToString("X2") + " * 2 * " + s.media[drive].sectors_per_track.ToString("X2") + ") + (" + head.ToString("X2") + " * " + s.media[drive].sectors_per_track.ToString("X2") + ") + (" + sector.ToString("X2") + " - 1))");
                    }
                    if (logical_sector >= s.media[drive].sectors)
                    {
                        mParent.mSystem.HandleException(this, new Exception("floppy io: logical sector out of bounds"));
                    }

                    s.cylinder[drive] = cylinder;
                    s.sector[drive] = sector;
                    s.head[drive] = head;

                    if (s.command[0] == 0xe6)
                    { // read
                        floppy_xfer(drive, (uint)(logical_sector * 512), s.floppy_buffer,
                                    512, FROM_FLOPPY);
                        s.floppy_buffer_index = 0;
                        mParent.SetDRQ(FLOPPY_DMA_CHAN, true);

                        /* data reg not ready, controller busy */
                        s.main_status_reg = FD_MS_MRQ | FD_MS_DIO | FD_MS_BUSY;
                        s.pending_command = s.command[0];
                        HandleRequestAt = System.DateTime.Now + mTimerDelay;
                        return;
                    }
                    else if (s.command[0] == 0xc5)
                    { // write
                        s.floppy_buffer_index = 0;
                        mParent.SetDRQ(FLOPPY_DMA_CHAN, true);

                        /* data reg not ready, controller busy */
                        s.main_status_reg = FD_MS_MRQ | FD_MS_DIO | FD_MS_BUSY;
                        s.pending_command = s.command[0];
                        HandleRequestAt = System.DateTime.Now + mTimerDelay;
                        return;
                    }
                    else
                        mParent.mSystem.HandleException(this, new Exception("floppy_command(): unknown read/write command"));
                    return;

                default:
                    mParent.mSystem.HandleException(this, new Exception("floppy_command(): unknown function"));
                    break;
            }
            mParent.mSystem.HandleException(this, new Exception("floppy_command()"));
        }
        private void floppy_xfer(byte drive, DWord offset, byte[] buffer, DWord bytes, byte direction)
        {
            long ret = 0;

            if (drive > 1)
                mParent.mSystem.HandleException(this, new Exception("floppy_xfer: drive > 1"));

            string lTemp = "";
            if (direction == FROM_FLOPPY)
                lTemp = "from";
            else
                lTemp = "to";
            if (mParent.mSystem.Debuggies.DebugFDC)
            {
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "drive=" + drive + ", offset=" + offset + " (" + offset.ToString("X8") + ")" +
                    ", abs sector=" + offset / 512 + ", bytes=" + bytes + ", direction=" + lTemp);
            }

            ret = s.media[drive].fd.Seek(offset, SeekOrigin.Begin);
            if (ret < 0)
            {
                mParent.mSystem.HandleException(this, new Exception("could not perform lseek() on floppy image file"));
            }

            if (direction == FROM_FLOPPY)
            {
                ret = s.media[drive].fd.Read(buffer, 0, (int)bytes);
                if (ret < bytes)
                {
                    /* ??? */
                    if (ret > 0)
                    {
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "partial read() on floppy image returns " + ret + ", " + bytes);
                    }
                    else
                    {
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "read() on floppy image returns 0");
                    }
                }
            }
            else
            { // TO_FLOPPY
                s.media[drive].fd.Write(buffer, 0, (int)bytes);
                s.media[drive].fd.Flush();
            }
        }
        internal bool evaluate_media(eFloppyType type, string path, ref floppy_t media)
        {
            char[] sTemp = new char[1024];

            if (type == eFloppyType.FLOPPY_NONE)
                return (false);

            //For now we are just dealing with files, so comment out the IF here and the ELSE section below
            /*  if ( S_ISREG(stat_buf.st_mode) ) 
            {*/
            // regular file
            switch (type)
            {
                case eFloppyType.FLOPPY_720K: // 720K 3.5"
                    media.type = eFloppyType.FLOPPY_720K;
                    media.sectors_per_track = 9;
                    media.tracks = 80;
                    media.heads = 2;
                    break;
                case eFloppyType.FLOPPY_1_2M: // 1.2M 5.25"
                    media.type = eFloppyType.FLOPPY_1_2M;
                    media.sectors_per_track = 15;
                    media.tracks = 80;
                    media.heads = 2;
                    break;
                case eFloppyType.FLOPPY_1_44M: // 1.44M 3.5"
                    media.type = eFloppyType.FLOPPY_1_44M;
                    /*if (stat_buf.st_size <= 1474560) {*/
                    media.sectors_per_track = 18;
                    media.tracks = 80;
                    media.heads = 2;
                    /*}
                  else if (stat_buf.st_size == 1720320) {
                    media.sectors_per_track = 21;
                    media.tracks            = 80;
                    media.heads             = 2;
                    }
              else if (stat_buf.st_size == 1763328) {
                    media.sectors_per_track = 21;
                    media.tracks            = 82;
                    media.heads             = 2;
                }
                  else {
                  mParent.mSystem.HandleException(this, new ExceptionNumber("Evaluate media: file " + path + " could not be evaluated"));
                    return(false);
                    }*/
                    break;
                case eFloppyType.FLOPPY_2_88M: // 2.88M 3.5"
                    media.type = eFloppyType.FLOPPY_2_88M;
                    media.sectors_per_track = 36;
                    media.tracks = 80;
                    media.heads = 2;
                    break;
                default:
                    mParent.mSystem.HandleException(this, new Exception("evaluate_media: unknown media type"));
                    break;
            }
            // open media file (image file or device)
            media.fd = new FileStream(path, FileMode.Open);
            media.Filename = path;
            media.sectors = (Word)(media.heads * media.tracks * media.sectors_per_track);
            return (true); // success
            /*    }

              else if ( S_ISCHR(stat_buf.st_mode)
            #if BX_WITH_MACOS == 0
            #ifdef S_ISBLK
                        || S_ISBLK(stat_buf.st_mode)
            #endif
            #endif
                       ) {
                // character or block device
                // assume media is formatted to typical geometry for drive
                switch (type) {
                  case BX_FLOPPY_720K: // 720K 3.5"
                    media.type              = BX_FLOPPY_720K;
                    media.sectors_per_track = 9;
                    media.tracks            = 80;
                    media.heads             = 2;
                    break;
                  case BX_FLOPPY_1_2: // 1.2M 5.25"
                    media.type              = BX_FLOPPY_1_2;
                    media.sectors_per_track = 15;
                    media.tracks            = 80;
                    media.heads             = 2;
                    break;
                  case BX_FLOPPY_1_44: // 1.44M 3.5"
                    media.type              = BX_FLOPPY_1_44;
                    media.sectors_per_track = 18;
                    media.tracks            = 80;
                    media.heads             = 2;
                    break;
                  case BX_FLOPPY_2_88: // 2.88M 3.5"
                    media.type              = BX_FLOPPY_2_88;
                    media.sectors_per_track = 36;
                    media.tracks            = 80;
                    media.heads             = 2;
                    break;
                  default:
                    bx_panic("floppy: evaluate_media: unknown media type");
                  }
                media.sectors = media.heads * media.tracks * media.sectors_per_track;
                return(1); // success
                }
              else {
                // unknown file type
                fprintf(stderr, "# floppy: unknown mode type");
                bx_printf("floppy: unknown mode type");
                return(0);
                }
            */
        }
        internal DateTime HandleRequestAt = new DateTime(2999, 1, 1);
        public void DMAWrite(ref byte data_byte)
        {
            // A DMA write is from I/O to Memory
            // We need to return then next data byte from the floppy buffer
            // to be transfered via the DMA to memory. (read block from floppy)


            data_byte = s.floppy_buffer[s.floppy_buffer_index++];
            if (s.floppy_buffer_index >= 512)
            {
                byte drive;

                drive = (byte)(s.DOR & 0x03);
                increment_sector(); // increment to next sector before retrieving next one
                s.floppy_buffer_index = 0;
                if (mParent.mProc.mTC)
                { // Terminal Count line, done
                    s.pending_command = 0;
                    s.main_status_reg = FD_MS_MRQ | FD_MS_DIO | FD_MS_BUSY;
                    s.result_size = 7;
                    s.result_index = 0;
                    s.result[0] = (byte)((s.head[drive] << 2) | drive);
                    s.result[1] = 0;
                    s.result[2] = 0;
                    s.result[3] = s.cylinder[drive];
                    s.result[4] = s.head[drive];
                    s.result[5] = s.sector[drive];
                    s.result[6] = 2;

                    if (mParent.mSystem.Debuggies.DebugFDC)
                    {
                        string lTemp = "AFTER: " + "  drive    = " + drive + "  head     = " + s.head[drive] + "  cylinder = " + s.cylinder[drive] +
                            "  sector   = " + s.sector[drive];
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, lTemp);
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "<<READ DONE>>");
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "AFTER");
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "  drive    = " + drive);
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "  head     = " + s.head[drive]);
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "  cylinder = " + s.cylinder[drive]);
                        //mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "  sector   = " + s.sector[drive]);
                    }
                    //if (mParent.mSystem.Debuggies.DebugFDC)
                    //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "Raising IRQ 6 in DMAWrite, TC=done");
                    //mParent.mPIC.RaiseIRQ(6);
                    mParent.SetDRQ(FLOPPY_DMA_CHAN, false);
                }
                else
                { // more data to transfer
                    DWord logical_sector;
                    logical_sector = (DWord)((s.cylinder[drive] * 2 *
                                      s.media[drive].sectors_per_track) +
                                     (s.head[drive] *
                                      s.media[drive].sectors_per_track) +
                                     (s.sector[drive] - 1));
                    floppy_xfer(drive, logical_sector * 512, s.floppy_buffer,
                                512, FROM_FLOPPY);
                }
            }
        }
        public void DMARead(byte data_byte)
        {
            // A DMA read is from Memory to I/O
            // We need to write the data_byte which was already transfered from memory
            // via DMA to I/O (write block to floppy)
            byte drive;
            UInt32 logical_sector;

            s.floppy_buffer[s.floppy_buffer_index++] = data_byte;
            if (s.floppy_buffer_index >= 512)
            {
                drive = (byte)(s.DOR & 0x03);
                logical_sector = (UInt32)((s.cylinder[drive] * 2 * s.media[drive].sectors_per_track) +
                 (s.head[drive] * s.media[drive].sectors_per_track) +
                 (s.sector[drive] - 1));
                floppy_xfer(drive, logical_sector * 512, s.floppy_buffer, 512, TO_FLOPPY);
                increment_sector();
                s.floppy_buffer_index = 0;
                //Skipped the next condition because I don't know what it is, so I just did the following unconditionally
                if (mParent.mProc.mTC)
                {
                    s.pending_command = 0;
                    s.main_status_reg = FD_MS_MRQ | FD_MS_DIO | FD_MS_BUSY;
                    s.result_size = 7;
                    s.result_index = 0;
                    s.result[0] = (byte)((s.head[drive] << 2) | drive);
                    s.result[1] = 0;
                    s.result[2] = 0;
                    s.result[3] = s.cylinder[drive];
                    s.result[4] = s.head[drive];
                    s.result[5] = s.sector[drive];
                    s.result[6] = 2;
                    mParent.SetDRQ(FLOPPY_DMA_CHAN, false);
                    if (mParent.mSystem.Debuggies.DebugFDC)
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "Raising IRQ 6 in DMARead, TC=done");
                    //CLR 08/03/2015 - Changed to use new function which recognizes the dma_and_interrupt_enable value
                    RaiseIRQ(6);
                }
            }
        }
        void increment_sector()
        {
            byte drive;

            drive = (byte)(s.DOR & 0x03);

            // values after completion of data xfer
            // ??? calculation depends on base_count being multiple of 512
            s.sector[drive]++;
            if (s.sector[drive] > s.media[drive].sectors_per_track)
            {
                s.sector[drive] -= (byte)(s.media[drive].sectors_per_track);
                s.head[drive]++;
                if (s.head[drive] > 1)
                {
                    s.head[drive] = 0;
                    s.cylinder[drive]++;
                    if (s.cylinder[drive] >= s.media[drive].tracks)
                    {
                        // SetC to 1 past last possible cylinder value.
                        // I notice if I set it to tracks-1, prama linux won't boot.
                        s.cylinder[drive] = (byte)(s.media[drive].tracks);
                        if (mParent.mSystem.Debuggies.DebugFDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "increment_sector: clamping cylinder to max");
                    }
                }
            }
        }
        #endregion

        public void LoadDrive(int DriveNum)
        {
            floppy_t test = new floppy_t();
            floppy_t test2 = new floppy_t();
            if (DriveNum == 1)
            {
                if (s.media[0].type == eFloppyType.FLOPPY_NONE)
                    s.num_supported_floppies++;
                evaluate_media(mParent.mSystem.FloppyACapacity, mParent.mSystem.FloppyAFile, ref test);
                s.media[0] = test;
                s.media_present[0] = true;
            }
            else
            {
                if (s.media[0].type == eFloppyType.FLOPPY_NONE)
                    s.num_supported_floppies++;
                evaluate_media(mParent.mSystem.FloppyACapacity, mParent.mSystem.FloppyBFile, ref test2);
                s.media[1] = test2;
                s.media_present[1] = true;
            }
        }

        #region cDevice Interface Related Methods
        public override void InitDevice()
        {
            if (mParent.mSystem.FloppyAFile != "")
            {
                LoadDrive(1);
                s.num_supported_floppies++;
            }
            if (mParent.mSystem.FloppyBFile != "")
            {
                LoadDrive(2);
                s.num_supported_floppies++;
            }

            mIOHandlers = new sIOHandler[4];
            mIOHandlers[0].Device = this; mIOHandlers[0].PortNum = CONFIG_PORT; mIOHandlers[0].Direction = eDataDirection.IO_InOut;
            mIOHandlers[1].Device = this; mIOHandlers[1].PortNum = DATA_PORT; mIOHandlers[1].Direction = eDataDirection.IO_InOut;
            mIOHandlers[2].Device = this; mIOHandlers[2].PortNum = STATUS_PORT; mIOHandlers[2].Direction = eDataDirection.IO_InOut;
            mIOHandlers[3].Device = this; mIOHandlers[3].PortNum = DIGITAL_PORT; mIOHandlers[3].Direction = eDataDirection.IO_InOut;
            mParent.mPIC.RegisterIRQ(this, 6);
            base.InitDevice();
            DeviceThreadSleep = 10;
        }
        public override void ResetDevice()
        {
            s.data_rate = 0; /* 500 Kbps */

            s.command_complete = true; /* waiting for new command */
            s.command_index = 0;
            s.command_size = 0;
            s.pending_command = 0;

            s.result_index = 0;
            s.result_size = 0;

            /* data register ready, not in DMA mode */
            s.main_status_reg = FD_MS_MRQ;
            s.status_reg0 = 0;
            s.status_reg1 = 0;
            s.status_reg2 = 0;
            s.status_reg3 = 0;
            s.DOR = 0x0c;
            // motor off, drive 3..0
            // DMA/INT enabled
            // normal operation
            // drive select 0

            // DIR affected only by hard reset
            //I think this is gumming up the works, removing temporarily (20100105 @ 11:35 am)
            if (mLastResetSource == sResetSource.HardReset)
            {
                s.DIR |= 0x80; // disk changed
            }
            for (int i = 0; i < 4; i++)
            {
                s.cylinder[i] = 0;
                s.head[i] = 0;
                s.sector[i] = 0;
            }
            s.floppy_buffer_index = 0;
        }
        public override void HandleIO(sPortValue IO, eDataDirection Direction)
        {
            if (mParent.mSystem.Debuggies.DebugFDC)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "NEW: D=" + Direction + ", P=" + IO.Portnum.ToString("X4") + ", V=" + IO.Value.ToString("X8"));
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
            byte value = 0;

            //if (mParent.mSystem.Debuggies.DebugFDC)
            //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "IN  on port " + OpDecoder.ValueToHex(IO.Portnum, 4) + ", Value = " + OpDecoder.ValueToHex(IO.Value, 8));

            switch (IO.Portnum)
            {
                case DIGITAL_PORT: //0x03F2
                    lock (mParent.mProc.ports.mPorts)
                        mParent.mProc.ports.mPorts[DIGITAL_PORT] = s.DOR;
                    break;
                case STATUS_PORT: //0x03F4
                    lock (mParent.mProc.ports.mPorts)
                        mParent.mProc.ports.mPorts[STATUS_PORT] = s.main_status_reg;
                    break;
                case DATA_PORT: //0x03F5
                    value = s.result[s.result_index++];
                    if (s.result_index >= s.result_size)
                    {
                        s.result_size = 0;
                        s.result_index = 0;
                        s.main_status_reg = FD_MS_MRQ;
                    }
                    lock (mParent.mProc.ports.mPorts)
                        mParent.mProc.ports.mPorts[DATA_PORT] = value;
                    break;
                case CONFIG_PORT: //0x03F7
                    lock (mParent.mProc.ports.mPorts)
                        mParent.mProc.ports.mPorts[CONFIG_PORT] = (byte)(s.DIR & 0x80);
                    break;
            }
        }

        public void RaiseIRQ(byte Which)
        {
            if ((s.DOR & 0x8) > 0)
                mParent.mPIC.RaiseIRQ(0x6);
        }
        
        public void Handle_OUT(sPortValue IO)
        {
            byte dma_and_interrupt_enable;
            byte normal_operation, prev_normal_operation;
            byte drive_select;
            byte motor_on_drive0, motor_on_drive1;

            //if (mParent.mSystem.Debuggies.DebugFDC)
            //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "OUT on port " + OpDecoder.ValueToHex(IO.Portnum, 4) + ", Value = " + OpDecoder.ValueToHex(IO.Value, 8));

            switch (IO.Portnum)
            {
                case DIGITAL_PORT: //0x03F2
                    motor_on_drive1 = (byte)(IO.Value & 0x20);
                    motor_on_drive0 = (byte)(IO.Value & 0x10);
                    dma_and_interrupt_enable = (byte)(IO.Value & 0x08);
                    normal_operation = (byte)(IO.Value & 0x04);
                    drive_select = (byte)(IO.Value & 0x03);
                    prev_normal_operation = (byte)(s.DOR & 0x04);
                    s.DOR = (byte)IO.Value;
                    if (prev_normal_operation == 0 && normal_operation != 0)
                    {
                        //CLR 08/03/2015 - Changed to use new function which recognizes the dma_and_interrupt_enable value
                        RaiseIRQ(6);
                        // transition from NORMAL to RESET
                        s.main_status_reg = FD_MS_BUSY;
                        s.pending_command = 0xfe; // RESET pending
                        //CLR 07/16/2013 - Linux is whining that it is taking too long to reset.  So for reset we won't wait for the device thread to call HandlePendingCommand.  We'll do it ourselves.
                        //HandleRequestAt = System.DateTime.Now + mTimerDelay;
                        CommandCompletion();
                        if (mParent.mSystem.Debuggies.DebugFDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "Transitioning from NORMAL to RESET");
                    }
                    else if (prev_normal_operation != 0 && normal_operation == 0)
                    {
                        // transition from RESET to NORMAL
                        //Original source had a #if 0 around the code below, not sure why but I'm removing it
                        //because it causes the drive to be shut off when the command sent was to turn on the motor
                        s.pending_command = 0xfe; // RESET pending
                        CommandCompletion();
                        if (mParent.mSystem.Debuggies.DebugFDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "Transitioning from RESET to NORMAL");
                    }
                    if (mParent.mSystem.Debuggies.DebugFDC)
                    {
                        StringBuilder lMsg = new StringBuilder();

                        lMsg.Append("io_write: DOR ");
                        lMsg.AppendFormat("\tmotor on, drive1 = {0} ", motor_on_drive1 > 0);
                        lMsg.AppendFormat("\tmotor on, drive0 = {0} ", motor_on_drive0 > 0);
                        lMsg.AppendFormat("\tdma_and_interrupt_enable= {0} ", (Word)dma_and_interrupt_enable);
                        lMsg.AppendFormat("\tnormal_operation= {0} ", (Word)normal_operation);
                        lMsg.AppendFormat("\tdrive_select= {0}", drive_select);
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, lMsg.ToString());
                    }
                    break;
                case STATUS_PORT:
                    //TODO: Deal with attempt to write status port!
                    break;
                case DATA_PORT:
                    if (s.command_complete)
                    {
                        if (s.pending_command != 0)
                        {
                            System.Diagnostics.Debug.Write(s.pending_command);
                            System.Diagnostics.Debug.WriteLine("command complete = " + s.command_complete);
                            mParent.mSystem.HandleException(this, new Exception("io: 3f5: receiving new comm (" + IO.Value.ToString("X2") + "), old one " + s.pending_command.ToString("X2") + " pending"));
                        }
                        s.command[0] = (byte)IO.Value;
                        s.command_complete = false;
                        s.command_index = 1;
                        /* read/write command in progress */
                        s.main_status_reg = FD_MS_MRQ | FD_MS_BUSY;
                        switch (IO.Value)
                        {
                            case 0x03: /* specify */
                                s.command_size = 3;
                                break;
                            case 0x04: // get status
                                s.command_size = 2;
                                break;
                            case 0x07: /* recalibrate */
                                s.command_size = 2;
                                break;
                            case 0x08: /* sense interrupt status */
                                s.command_size = 1;
                                FloppyCommand();
                                s.command_complete = true;
                                break;
                            case 0x0f: /* seek */
                                s.command_size = 3;
                                break;
                            case 0x4a: /* read ID */
                                s.command_size = 2;
                                break;
                            case 0xc5: /* write normal data */
                                s.command_size = 9;
                                break;
                            case 0xe6: /* read normal data */
                                s.command_size = 9;
                                break;

                            case 0x13: // Configure command (Enhanced)
                                s.command_size = 3;
                                break;

                            case 0x0e: // dump registers (Enhanced drives)
                            case 0x10: // Version command, standard controller returns 80h
                            case 0x18: // National Semiconductor version command; return 80h
                                // These commands are not implemented on the standard
                                // controller and return an error.  They are available on
                                // the enhanced controller.
                                s.command_size = 1;
                                s.result[0] = 0x80;
                                s.result_size = 1;
                                s.result_index = 0;
                                s.main_status_reg = FD_MS_MRQ | FD_MS_DIO | FD_MS_BUSY;
                                s.command_complete = true;
                                break;

                            default:
                                mParent.mSystem.HandleException(this, new Exception("io write:3f5: unsupported case " + IO.Value));
                                break;
                        }
                    }
                    else
                    {
                        s.command[s.command_index++] = (byte)IO.Value;
                        if (s.command_index == s.command_size)
                        {
                            /* read/write command not in progress any more */
                            FloppyCommand();
                            CommandCompletion();
                            s.command_complete = true;
                        }
                    }
                    //if (mParent.mSystem.Debuggies.DebugFDC)
                    //    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "io_write: diskette controller data");
                    break; //DATA_PORT
                case CONFIG_PORT: //0x03F7
                    s.data_rate = (byte)(IO.Value & 0x03);
                    break;
            }
        }
        public override void DeviceThread()
        {
            DeviceThreadActive = true;
            while (1 == 1)
            {
                Thread.Sleep(5);
                if (System.DateTime.Now > HandleRequestAt)
                {
                    CommandCompletion();
                    HandleRequestAt = new DateTime(2999, 1, 1);
                }
                if (mShutdownRequested)
                    break;
            }
            DeviceThreadActive = false;
        }
        public override void RequestShutdown()
        {
            base.RequestShutdown();
            do { Thread.Sleep(10); } while (DeviceThreadActive);
            if (s.media_present[0])
                try { s.media[0].fd.Close(); }
                catch { }
            if (s.media_present[1])
                try { s.media[1].fd.Close(); }
                catch { }
        }
        #endregion

        void CommandCompletion()
        {
            Boolean reset_changeline = false;

            reset_changeline = false;
            if (s.pending_command > 0)
            {
                switch (s.pending_command)
                {
                    case 0x07: // recal
                        /* write ready, not busy */
                        s.main_status_reg = FD_MS_MRQ;
                        if (mParent.mSystem.Debuggies.DebugFDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "Raising IRQ 6 for command 0x7 - Recalibrate");
                        //CLR 08/03/2015 - Changed to use new function which recognizes the dma_and_interrupt_enable value
                        RaiseIRQ(6);
                        reset_changeline = true;
                        s.pending_command = 0;
                        break;

                    case 0x0f: // seek
                        /* write ready, not busy */
                        s.main_status_reg = FD_MS_MRQ;
                        if (mParent.mSystem.Debuggies.DebugFDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "Raising IRQ 6 for command 0xf - Seek");
                        //CLR 08/03/2015 - Changed to use new function which recognizes the dma_and_interrupt_enable value
                        RaiseIRQ(6);
                        reset_changeline = true;
                        s.pending_command = 0;
                        break;


                    case 0x4a: /* read ID */
                    case 0xc5: // write normal data
                    case 0xe6: // read normal data
                        /* read ready, busy */
                        s.main_status_reg = FD_MS_MRQ | FD_MS_DIO;
                        if (mParent.mSystem.Debuggies.DebugFDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "Raising IRQ 6 for command " + s.pending_command.ToString("X2") + " - Read/Write/Read ID");
                        //CLR 08/03/2015 - Changed to use new function which recognizes the dma_and_interrupt_enable value
                        RaiseIRQ(6);
                        s.pending_command = 0;
                        break;

                    case 0xfe: // (contrived) RESET
                        //mParent.mPIC.RaiseIRQ(6);
                        mLastResetSource = sResetSource.SoftReset;
                        this.ResetDevice();
                        s.main_status_reg = FD_MS_MRQ;
                        if (mParent.mSystem.Debuggies.DebugFDC)
                            mParent.mSystem.PrintDebugMsg(eDebuggieNames.Floppy, "Raising IRQ 6 for command 0xFE - Reset");
                        s.pending_command = 0;
                        RaiseIRQ(6);
                        break;

                    default:
                        mParent.mSystem.HandleException(this, new Exception("timer(): unknown case " + s.pending_command));
                        break;
                }
                if (reset_changeline)
                {
                    byte drive = (byte)(s.DOR & 0x3);
                    if (drive > 1) return;
                    if (s.media_present[drive])
                        s.DIR = (byte)(s.DIR & ~0x80);
                }
            }
        }

        #region Additional Public structures and definitions
        public struct sInfo
        {
            public byte data_rate;

            public byte[] command; /* largest command size ??? */
            public byte command_index;
            public byte command_size;
            public Boolean command_complete;
            public byte pending_command;

            public byte[] result;
            public byte result_index;
            public byte result_size;

            public byte DOR; // Digital Ouput Register
            public byte TDR; // Tape Drive Register
            public byte[] cylinder; // really only using 2 drives
            public byte[] head;     // really only using 2 drives
            public byte[] sector;   // really only using 2 drives

            /* MAIN STATUS REGISTER
             * b7: MRQ: main request 1=data register ready     0=data register not ready
             * b6: DIO: data input/output:
             *     1=controller->CPU (ready for data read)
             *     0=CPU->controller (ready for data write)
             * b5: NDMA: non-DMA mode: 1=controller not in DMA modes
             *                         0=controller in DMA mode
             * b4: BUSY: instruction(device busy) 1=active 0=not active
             * b3-0: ACTD, ACTC, ACTB, ACTA:
             *       drive D,C,B,A in positioning mode 1=active 0=not active
             */
            public byte main_status_reg;

            public byte status_reg0;
            public byte status_reg1;
            public byte status_reg2;
            public byte status_reg3;

            public floppy_t[] media;
            public Word num_supported_floppies;
            public byte[] floppy_buffer; // 2 extra for good measure
            public Word floppy_buffer_index;
            public int floppy_timer_index;
            public Boolean[] media_present;
            public byte DIR; // Digital Input Register:
            // b7: 0=diskette is present and has not been changed
            //     1=diskette missing or changed

            public sInfo(byte Dummy)
            {
                command = new byte[10];
                result = new byte[10];
                cylinder = new byte[4];
                head = new byte[4];
                sector = new byte[4];
                media = new floppy_t[2];
                floppy_buffer = new byte[512 + 2];
                media_present = new Boolean[2] { false, false };
                data_rate = 0;
                command_index = 0;
                command_size = 0;
                command_complete = true;
                pending_command = 0;
                result_index = 0;
                result_size = 0;
                DOR = 0;
                TDR = 0;
                DIR = 0;
                main_status_reg = FD_MS_MRQ;
                status_reg0 = 0;
                status_reg1 = 0;
                status_reg2 = 0;
                status_reg3 = 0;
                num_supported_floppies = 0;
                floppy_buffer_index = 0;
                floppy_timer_index = 0;
            }

        }
        public struct floppy_t
        {
            public FileStream fd;         /* file descriptor of floppy image file */
            public Word sectors_per_track;    /* number of sectors/track */
            public Word sectors;    /* number of formatted sectors on diskette */
            public Word tracks;      /* number of tracks */
            public Word heads;      /* number of heads */
            public eFloppyType type;
            public string Filename;
        }

        public struct sFloppyType
        {
            public eFloppyType id;
            public byte trk;
            public byte hd;
            public byte spt;
            public Word sectors;
            public byte drive_mask;
            public sFloppyType(eFloppyType Id, byte Trk, byte Hd, byte Spt, Word Sectors, byte DriveMask)
            {
                id = Id; trk = Trk; hd = Hd; spt = Spt; sectors = Sectors; drive_mask = DriveMask;
            }
        }
        public static sFloppyType[] floppy_type = new sFloppyType[8] {
          new sFloppyType(eFloppyType.FLOPPY_160K, 40, 1, 8, 320, 0x03),
          new sFloppyType(eFloppyType.FLOPPY_180K, 40, 1, 9, 360, 0x03),
          new sFloppyType(eFloppyType.FLOPPY_320K, 40, 2, 8, 640, 0x03),
          new sFloppyType(eFloppyType.FLOPPY_360K, 40, 2, 9, 720, 0x03),
          new sFloppyType(eFloppyType.FLOPPY_720K, 80, 2, 9, 1440, 0x1f),
          new sFloppyType(eFloppyType.FLOPPY_1_2M,  80, 2, 15, 2400, 0x02),
          new sFloppyType(eFloppyType.FLOPPY_1_44M, 80, 2, 18, 2880, 0x18),
          new sFloppyType(eFloppyType.FLOPPY_2_88M, 80, 2, 36, 5760, 0x10) };
        #endregion
    }
}
