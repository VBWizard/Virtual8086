using System;
using System.Linq;
using System.Threading;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;
using bx_bool = System.Boolean;
using Bit8u = System.Byte;
using Bit16u = System.UInt16;
using Bit32u = System.UInt32;
using Unsigned = System.UInt64;

namespace VirtualProcessor.Devices
{

    public class cVGA : cDevice, iDevice
    {
        #region Private Variables & Constants
        private const int ADDRESS_EMPTY = 0xFF;
        public const int X_TILESIZE = 16;
        public const int Y_TILESIZE = 24;
        public s_vgaState s;
        #endregion

        byte port_3da_lastvalue = 0;
        public cVGA(cDeviceBlock DevBlock)
        {
            mParent = DevBlock;
            mDeviceId = "XYZABCVideo";
            mDeviceClass = eDeviceClass.Video;
            mName = "Bogus VGA";
        }

        #region Device Methods
        void init_standard_vga()
{

  // initialize VGA controllers and other internal stuff
    s.Initialize();
  s.vga_enabled = true;
  s.misc_output.color_emulation = true;
  s.misc_output.enable_ram = true;
  s.misc_output.horiz_sync_pol = true;
  s.misc_output.vert_sync_pol = true;

  s.attribute_ctrl.mode_ctrl.enable_line_graphics = true;
  s.line_offset=80;
  s.line_compare=1023;
  s.vertical_display_end=399;

  s.attribute_ctrl.video_enabled = true;
  s.attribute_ctrl.color_plane_enable = 0x0f;
  s.pel.dac_state = 0x01;
  s.pel.mask = 0xff;
  s.graphics_ctrl.memory_mapping = 2; // monochrome text mode

  s.sequencer.reset1 = true;
  s.sequencer.reset2 = true;
  s.sequencer.extended_mem = true; // display mem greater than 64K
  s.sequencer.odd_even = true; // use sequential addressing mode

  s.plane_shift = 16;
  s.dac_shift = 2;
  s.last_bpp = 8;
  s.htotal_usec = 31;
  s.vtotal_usec = 14285;

  s.max_xres = 800;
  s.max_yres = 600;

  s.vga_override = false;
  s.CRTC.address = ADDRESS_EMPTY;
  // initialize memory, handlers and timer (depending on extension)
  //extname = "none";
  //  s.memsize = 0x40000;
  //  if (s.memory == null)
  //    s.memory = new Bit8u[s.memsize];
  //  memset(s.memory, 0, s.memsize);
  //}
  //DEV_register_memory_handlers(BX_VGA_THIS_PTR, mem_read_handler, mem_write_handler,
  //                             0xa0000, 0xbffff);

  //TODO: CMOS update
   // video card with BIOS ROM
  //DEV_cmos_set_reg(0x14, (DEV_cmos_get_reg(0x14) & 0xcf) | 0x00);
}
        #endregion

        #region cDevice Interface Related Methods
        public override void InitDevice()
        {
            //Set up IO handlers and PIC Registration and then call parent InitDevice which will register IO handlers
            mIOHandlers = new sIOHandler[3];
            mIOHandlers[0].Device = this; mIOHandlers[0].PortNum = 0x3D4; mIOHandlers[0].Direction = eDataDirection.IO_InOut;
            mIOHandlers[1].Device = this; mIOHandlers[1].PortNum = 0x3D5; mIOHandlers[1].Direction = eDataDirection.IO_InOut;
            mIOHandlers[2].Device = this; mIOHandlers[1].PortNum = 0x3Da; mIOHandlers[1].Direction = eDataDirection.IO_InOut;
            //mParent.mPIC.RegisterIRQ(this, 6);
            base.InitDevice();
            init_standard_vga();
        }
        public override void ResetDevice()
        {
        }
        public override void HandleIO(sPortValue IO, eDataDirection Direction)
        {
            if (mParent.mSystem.Debuggies.DebugVideo)
                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Video, "NEW: D=" + Direction + ", P=" + IO.Portnum.ToString("X4") + ", V=" + IO.Value.ToString("X8"));
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
            switch (IO.Portnum)
            {
                case 0x03D4:
                    //invalid
                    break;
                case 0x3da:
                    if (port_3da_lastvalue == 0)
                        port_3da_lastvalue = 8;
                    else
                        port_3da_lastvalue = 0;
                    lock (mParent.mProc.ports.mPorts)
                        mParent.mProc.ports.mPorts[0x3da] = port_3da_lastvalue;
                    break;
            }
        }
        public void Handle_OUT(sPortValue IO)
        {
            switch (IO.Portnum)
            {
                case 0x03D4:
                    if (IO.Value < 0x19)
                        s.CRTC.address = (Bit8u)IO.Value;
                    break;
                case 0x3D5:
                    if (s.CRTC.address != ADDRESS_EMPTY && s.CRTC.address < 0x19)
                    {
                        s.CRTC.reg[s.CRTC.address] = (Bit8u)IO.Value;
                        s.CRTC.address = ADDRESS_EMPTY;
                    }
                    break;
            }
        }
        public override void DeviceThread()
        {
            while (1 == 1)
            {
                Thread.Sleep(DEVICE_THREAD_SLEEP_TIMEOUT);
                if (mShutdownRequested)
                    break;
            }
        }
        #endregion


        public struct s_misc_output
        {
            public bx_bool color_emulation;  // 1=color emulation, base address = 3Dx
            // 0=mono emulation,  base address = 3Bx
            public bx_bool enable_ram;       // enable CPU access to video memory if set
            public Bit8u clock_select;     // 0=25Mhz 1=28Mhz
            public bx_bool select_high_bank; // when in odd/even modes, select
            // high 64k bank if set
            public bx_bool horiz_sync_pol;   // bit6: negative if set
            public bx_bool vert_sync_pol;    // bit7: negative if set
            //   bit7,bit6 represent number of lines on display:
            //   0 = reserved
            //   1 = 400 lines
            //   2 = 350 lines
            //   3 - 480 lines
        }

        public struct s_CRTC
        {
            public Bit8u address;
            public Bit8u[] reg;            //0x19
            public bx_bool write_protect;
        }

        public struct s_mode_ctrl
        {
            public bx_bool graphics_alpha;
            public bx_bool display_type;
            public bx_bool enable_line_graphics;
            public bx_bool blink_intensity;
            public bx_bool pixel_panning_compat;
            public bx_bool pixel_clock_select;
            public bx_bool internal_palette_size;
        }
        
        public struct s_attribute_ctrl
        {
            public bx_bool flip_flop; /* 0 = address, 1 = data-write */
            public Unsigned address;  /* register number */
            public bx_bool video_enabled;
            public Bit8u[] palette_reg;       //16
            public Bit8u overscan_color;
            public Bit8u color_plane_enable;
            public Bit8u horiz_pel_panning;
            public Bit8u color_select;
            public s_mode_ctrl mode_ctrl;
        }

        public struct s_pel
        {
            public Bit8u write_data_register;
            public Bit8u write_data_cycle; /* 0, 1, 2 */
            public Bit8u read_data_register;
            public Bit8u read_data_cycle; /* 0, 1, 2 */
            public Bit8u dac_state;
            public Bit8u mask;
            public s_pel_data[] data; //256
        }

        public struct s_pel_data
        {
            public Bit8u red;
            public Bit8u green;
            public Bit8u blue;
        }

        public struct s_graphics_ctrl
        {
            public Bit8u index;
            public Bit8u set_reset;
            public Bit8u enable_set_reset;
            public Bit8u color_compare;
            public Bit8u data_rotate;
            public Bit8u raster_op;
            public Bit8u read_map_select;
            public Bit8u write_mode;
            public bx_bool read_mode;
            public bx_bool odd_even;
            public bx_bool chain_odd_even;
            public Bit8u shift_reg;
            public bx_bool graphics_alpha;
            public Bit8u memory_mapping; /* 0 = use A0000-BFFFF
                               * 1 = use A0000-AFFFF EGA/VGA graphics modes
                               * 2 = use B0000-B7FFF Monochrome modes
                               * 3 = use B8000-BFFFF CGA modes
                               */
            public Bit8u color_dont_care;
            public Bit8u bitmask;
            public Bit8u[] latch;      //4
        }

        public struct s_sequencer
        {
            public Bit8u index;
            public Bit8u map_mask;
            public bx_bool reset1;
            public bx_bool reset2;
            public Bit8u reg1;
            public Bit8u char_map_select;
            public bx_bool extended_mem;
            public bx_bool odd_even;
            public bx_bool chain_four;
            public bx_bool clear_screen;
        }

        public struct s_vgaState
        {

            public void Initialize()
            {
                text_snapshot = new Bit8u[128 * 1024];
                tile = new Bit8u[X_TILESIZE * Y_TILESIZE * 4];
                CRTC.reg = new Bit8u[0x19];
                attribute_ctrl.palette_reg = new Bit8u[16];
                pel.data = new s_pel_data[256];
                graphics_ctrl.latch = new Bit8u[4];
                memory = new Bit8u[0x40000];
            }
            public s_misc_output misc_output;
            public s_CRTC CRTC;
            public s_pel pel;
            public s_graphics_ctrl graphics_ctrl;
            public s_sequencer sequencer;
            public s_attribute_ctrl attribute_ctrl;
            public bx_bool vga_enabled;
            public bx_bool vga_mem_updated;
            public Unsigned line_offset;
            public Unsigned line_compare;
            public Unsigned vertical_display_end;
            public Unsigned blink_counter;
            public bx_bool vga_tile_updated; //pointer
            public Bit8u[] memory;               //pointer
            public Bit32u memsize;
            public Bit8u[] text_snapshot; // current text snapshot //128*1024
            public Bit8u[] tile; /**< Currently allocates the tile as large as needed. */ //[X_TILESIZE * Y_TILESIZE * 4]
            public Bit16u charmap_address;
            public bx_bool x_dotclockdiv2;
            public bx_bool y_doublescan;
            // h/v retrace timing
            public Bit32u htotal_usec;
            public Bit32u hbstart_usec;
            public Bit32u hbend_usec;
            public Bit32u vtotal_usec;
            public Bit32u vblank_usec;
            public Bit32u vrstart_usec;
            public Bit32u vrend_usec;
            // shift values for extensions
            public Bit8u plane_shift;
            public Bit32u plane_offset;
            public Bit8u dac_shift;
            // last active resolution and bpp
            public Bit16u last_xres;
            public Bit16u last_yres;
            public Bit8u last_bpp;
            public Bit8u last_msl;
            // maximum resolution and number of tiles
            public Bit16u max_xres;
            public Bit16u max_yres;
            public Bit16u num_x_tiles;
            public Bit16u num_y_tiles;
            // vga override mode
            public bx_bool vga_override;
            //bx_nonvga_device_c* nvgadev;
        }  // state information
    }
}