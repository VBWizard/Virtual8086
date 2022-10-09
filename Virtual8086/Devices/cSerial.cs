using System;
using System.Linq;
using System.Threading;
using Word = System.UInt16;
using DWord = System.UInt32;
using QWord = System.UInt64;

namespace VirtualProcessor.Devices
{
    public class cSerial : cDevice, iDevice
    {

        #region Private Variables & Constants
        const int MOUSE_BUFF_SIZE = 48;
        const int SERIAL_MAXDEV = 4;
        const int N_SERIAL_PORTS = 0;
        const int PC_CLOCK_XTL = 1843200;
        const int SER_RXIDLE = 0;
        const int SER_RXPOLL = 1;
        const int SER_RXWAIT = 2;
        const int SER_THR = 0;
        const int SER_RBR = 0;
        const int SER_IER = 1;
        const int SER_IIR = 2;
        const int SER_FCR = 2;
        const int SER_LCR = 3;
        const int SER_MCR = 4;
        const int SER_LSR = 5;
        const int SER_MSR = 6;
        const int SER_SCR = 7;
        const int SER_MODE_NULL = 0;
        const int SER_MODE_FILE = 1;
        const int SER_MODE_TERM = 2;
        const int SER_MODE_RAW = 3;
        const int SER_MODE_MOUSE = 4;
        const int SER_MODE_SOCKET_CLIENT = 5;
        const int SER_MODE_SOCKET_SERVER = 6;
        const int SER_MODE_PIPE_CLIENT = 7;
        const int SER_MODE_PIPE_SERVER = 8;
        enum serialInttypes
        {
            SER_INT_IER,
            SER_INT_RXDATA,
            SER_INT_TXHOLD,
            SER_INT_RXLSTAT,
            SER_INT_MODSTAT,
            SER_INT_FIFO
        };
        enum mouseTypes
        {
            MOUSE_TYPE_NONE,
            MOUSE_TYPE_PS2,
            MOUSE_TYPE_IMPS2,
            MOUSE_TYPE_BUS,
            MOUSE_TYPE_SERIAL,
            MOUSE_TYPE_SERIAL_WHEEL,
            MOUSE_TYPE_SERIAL_MSYS
        }

        mouse_buffer mouse_internal_buffer;
        public serial_t[] serialPorts;
        int detect_mouse;
        int mouse_port;
        mouseTypes mouse_type;
        int mouse_delayed_dx;
        int mouse_delayed_dy;
        int mouse_delayed_dz;
        UInt16[] ports;
        char[] name;
        char[] pname;
        byte i, count;
        #endregion
        int lCounter = 0;
        private bool mSetTHRFalse;

        public cSerial(cDeviceBlock DevBlock)
        {
            mParent = DevBlock;
            mDeviceId = "X";
            mDeviceClass = eDeviceClass.Serial;
            mName = "8250";

            serialPorts = new serial_t[N_SERIAL_PORTS];
            for (int cnt = 0; cnt < N_SERIAL_PORTS; cnt++)
                serialPorts[cnt] = new serial_t(0);
            ports = new UInt16[SERIAL_MAXDEV] { 0x03f8, 0x02f8, 0x03e8, 0x02e8 };
            name = new char[16];
            pname = new char[20];
            mouse_port = -1;
            mouse_type = mouseTypes.MOUSE_TYPE_NONE;
            mouse_internal_buffer = new mouse_buffer(0);
            mouse_internal_buffer.num_elements = 0;
            for (i = 0; i < MOUSE_BUFF_SIZE - 1; i++)
                mouse_internal_buffer.buffer[i] = (char)0;
            mouse_internal_buffer.head = 0;
            mouse_delayed_dx = 0;
            mouse_delayed_dy = 0;
            mIOHandlers = new sIOHandler[N_SERIAL_PORTS * 8];
            if (N_SERIAL_PORTS > 0)
                serialPorts[0].io_mode = SER_MODE_FILE;
            //serialPorts[1].io_mode = SER_MODE_FILE;
            //serialPorts[2].io_mode = SER_MODE_FILE;
            //serialPorts[3].io_mode = SER_MODE_FILE;
            for (i = 0; i < N_SERIAL_PORTS; i++)
            {
                //8 Port assignments for each serial port
                for (uint addr = ports[i]; addr < (uint)(ports[i] + 8); addr++)
                {
                    mIOHandlers[lCounter].Device = this; mIOHandlers[lCounter].PortNum = addr; mIOHandlers[lCounter++].Direction = eDataDirection.IO_InOut;
                }
                serialPorts[i].IRQ = (byte)(4 - (i & 1));
                if (i < 2)
                {
                    mParent.mPIC.RegisterIRQ(this, serialPorts[i].IRQ);
                }
                /* internal state */
                serialPorts[i].ls_ipending = false;
                serialPorts[i].ms_ipending = false;
                serialPorts[i].rx_ipending = false;
                serialPorts[i].fifo_ipending = false;
                serialPorts[i].ls_interrupt = false;
                serialPorts[i].ms_interrupt = false;
                serialPorts[i].rx_interrupt = false;
                serialPorts[i].tx_interrupt = false;
                serialPorts[i].fifo_interrupt = false;

                //if (serialPorts[i].tx_timer_index == NULL_TIMER_HANDLE)
                //{
                //    serialPorts[i].tx_timer_index =
                //      pc_system.register_timer(this, tx_timer_handler, 0,
                //                                  0, 0, "serial.tx"); // one-shot, inactive
                //}

                //if (serialPorts[i].rx_timer_index == NULL_TIMER_HANDLE)
                //{
                //    serialPorts[i].rx_timer_index =
                //      pc_system.register_timer(this, rx_timer_handler, 0,
                //                                  0, 0, "serial.rx"); // one-shot, inactive
                //}
                //if (serialPorts[i].fifo_timer_index == NULL_TIMER_HANDLE)
                //{
                //    serialPorts[i].fifo_timer_index =
                //      pc_system.register_timer(this, fifo_timer_handler, 0,
                //                                  0, 0, "serial.fifo"); // one-shot, inactive
                //}
                serialPorts[i].rx_pollstate = SER_RXIDLE;

                /* int enable: b0000 0000 */
                serialPorts[i].int_enable.rxdata_enable = false;
                serialPorts[i].int_enable.txhold_enable = false;
                serialPorts[i].int_enable.rxlstat_enable = false;
                serialPorts[i].int_enable.modstat_enable = false;

                /* int ID: b0000 0001 */
                serialPorts[i].int_ident.ipending = true;
                serialPorts[i].int_ident.int_ID = 0;

                /* FIFO control: b0000 0000 */
                serialPorts[i].fifo_cntl.enable = false;
                serialPorts[i].fifo_cntl.rxtrigger = 0;
                serialPorts[i].rx_fifo_end = 0;
                serialPorts[i].tx_fifo_end = 0;

                /* Line Control reg: b0000 0000 */
                serialPorts[i].line_cntl.wordlen_sel = 0;
                serialPorts[i].line_cntl.stopbits = false;
                serialPorts[i].line_cntl.parity_enable = false;
                serialPorts[i].line_cntl.evenparity_sel = false;
                serialPorts[i].line_cntl.stick_parity = false;
                serialPorts[i].line_cntl.break_cntl = false;
                serialPorts[i].line_cntl.dlab = false;

                /* Modem Control reg: b0000 0000 */
                serialPorts[i].modem_cntl.dtr = false;
                serialPorts[i].modem_cntl.rts = false;
                serialPorts[i].modem_cntl.out1 = false;
                serialPorts[i].modem_cntl.out2 = false;
                serialPorts[i].modem_cntl.local_loopback = false;

                /* Line Status register: b0110 0000 */
                serialPorts[i].line_status.rxdata_ready = false;
                serialPorts[i].line_status.overrun_error = false;
                serialPorts[i].line_status.parity_error = false;
                serialPorts[i].line_status.framing_error = false;
                serialPorts[i].line_status.break_int = false;
                serialPorts[i].line_status.thr_empty = true;
                serialPorts[i].line_status.tsr_empty = true;
                serialPorts[i].line_status.fifo_error = false;

                /* Modem Status register: bXXXX 0000 */
                serialPorts[i].modem_status.delta_cts = false;
                serialPorts[i].modem_status.delta_dsr = false;
                serialPorts[i].modem_status.ri_trailedge = false;
                serialPorts[i].modem_status.delta_dcd = false;
                serialPorts[i].modem_status.cts = false;
                serialPorts[i].modem_status.dsr = false;
                serialPorts[i].modem_status.ri = false;
                serialPorts[i].modem_status.dcd = false;

                serialPorts[i].scratch = 0;      /* scratch register */
                serialPorts[i].divisor_lsb = 1;  /* divisor-lsb register */
                serialPorts[i].divisor_msb = 0;  /* divisor-msb register */

                serialPorts[i].baudrate = 115200;
                serialPorts[i].io_mode = SER_MODE_NULL;

                serialPorts[i].fifo_tcb = new TimerCallback(fifo_timer);
                serialPorts[i].rx_tcb = new TimerCallback(rx_timer);
                serialPorts[i].tx_tcb = new TimerCallback(tx_timer);

                serialPorts[i].rx_timer = new System.Threading.Timer(serialPorts[i].rx_tcb, i, Timeout.Infinite, Timeout.Infinite);
                serialPorts[i].tx_timer = new System.Threading.Timer(serialPorts[i].tx_tcb, i, Timeout.Infinite, Timeout.Infinite);
                serialPorts[i].fifo_timer = new System.Threading.Timer(serialPorts[i].fifo_tcb, i, Timeout.Infinite, Timeout.Infinite);
            }
        }

        #region Device Methods
        void irq_handler(bool raise, byte WhichIRQ)
        {
            if (raise)
            {
                mParent.mPIC.RaiseIRQ(WhichIRQ);
                //DEV_pic_raise_irq(0);
            }
            else
            {
                mParent.mPIC.LowerIRQ(WhichIRQ);
                //DEV_pic_lower_irq(0);
            }
        }
        void lower_interrupt(byte port)
        {
            /* If there are no more ints pending, clear the irq */
            if ((!serialPorts[port].rx_interrupt) &&
                (!serialPorts[port].tx_interrupt) &&
                (!serialPorts[port].ls_interrupt) &&
                (!serialPorts[port].ms_interrupt) &&
                (!serialPorts[port].fifo_interrupt))
            {
                irq_handler(false, serialPorts[port].IRQ);
            }
        }
        void raise_interrupt(byte port, int type)
        {
            bool gen_int = false;

            switch (type)
            {
                case (int)serialInttypes.SER_INT_IER: /* IER has changed */
                    gen_int = true;
                    break;
                case (int)serialInttypes.SER_INT_RXDATA:
                    if (serialPorts[port].int_enable.rxdata_enable)
                    {
                        serialPorts[port].rx_interrupt = true;
                        gen_int = true;
                    }
                    else
                    {
                        serialPorts[port].rx_ipending = true;
                    }
                    break;
                case (int)serialInttypes.SER_INT_TXHOLD:
                    if (serialPorts[port].int_enable.txhold_enable)
                    {
                        serialPorts[port].tx_interrupt = true;
                        gen_int = true;
                    }
                    break;
                case (int)serialInttypes.SER_INT_RXLSTAT:
                    if (serialPorts[port].int_enable.rxlstat_enable)
                    {
                        serialPorts[port].ls_interrupt = true;
                        gen_int = true;
                    }
                    else
                    {
                        serialPorts[port].ls_ipending = true;
                    }
                    break;
                case (int)serialInttypes.SER_INT_MODSTAT:
                    if ((serialPorts[port].ms_ipending) &&
                        (serialPorts[port].int_enable.modstat_enable))
                    {
                        serialPorts[port].ms_interrupt = true;
                        serialPorts[port].ms_ipending = false;
                        gen_int = true;
                    }
                    break;
                case (int)serialInttypes.SER_INT_FIFO:
                    if (serialPorts[port].int_enable.rxdata_enable)
                    {
                        serialPorts[port].fifo_interrupt = true;
                        gen_int = true;
                    }
                    else
                    {
                        serialPorts[port].fifo_ipending = true;
                    }
                    break;
            }
            if (gen_int && serialPorts[port].modem_cntl.out2)
            {
                irq_handler(true, serialPorts[port].IRQ);
            }
        }
        public void rx_fifo_enq(byte port, byte data)
        {
            bool gen_int = false;

            if (serialPorts[port].fifo_cntl.enable)
            {
                if (serialPorts[port].rx_fifo_end == 16)
                {
                    //ERROR(("com%d: receive FIFO overflow", port+1));
                    serialPorts[port].line_status.overrun_error = true;
                    raise_interrupt(port, (int)serialInttypes.SER_INT_RXLSTAT);
                }
                else
                {
                    serialPorts[port].rx_fifo[serialPorts[port].rx_fifo_end++] = data;
                    switch (serialPorts[port].fifo_cntl.rxtrigger)
                    {
                        case 1:
                            if (serialPorts[port].rx_fifo_end == 4) gen_int = true;
                            break;
                        case 2:
                            if (serialPorts[port].rx_fifo_end == 8) gen_int = true;
                            break;
                        case 3:
                            if (serialPorts[port].rx_fifo_end == 14) gen_int = true;
                            break;
                        default:
                            gen_int = true;
                            break;
                    }
                    if (gen_int)
                    {
                        serialPorts[port].rx_timer.Dispose();
                        //Old was
                        //pc_system.deactivate_timer(serialPorts[port].fifo_timer_index);
                        serialPorts[port].line_status.rxdata_ready = true;
                        raise_interrupt(port, (int)serialInttypes.SER_INT_RXDATA);
                    }
                    else
                    {
                        while (serialPorts[port].in_fifo_callback)
                            Thread.Sleep(1);
                        serialPorts[port].fifo_timer.Change(50, 0);
                        //OLD WAS
                        //pc_system.activate_timer(serialPorts[port].fifo_timer_index,
                        //                            (int) (1000000.0 / serialPorts[port].baudrate *
                        //                            (serialPorts[port].line_cntl.wordlen_sel + 5) * 16),
                        //                            0); /* not continuous */
                    }
                }
            }
            else
            {
                if (serialPorts[port].line_status.rxdata_ready == true)
                {
                    //ERROR(("com%d: overrun error", port+1));
                    serialPorts[port].line_status.overrun_error = true;
                    raise_interrupt(port, (int)serialInttypes.SER_INT_RXLSTAT);
                }
                serialPorts[port].rxbuffer = data;
                serialPorts[port].line_status.rxdata_ready = true;
                raise_interrupt(port, (int)serialInttypes.SER_INT_RXDATA);
            }
        }
        void tx_timer(Object stateInfo)
        {
            bool gen_int = false;
            byte port = (byte)stateInfo;
            serialPorts[port].tx_timer.Change(Timeout.Infinite, Timeout.Infinite);
            serialPorts[port].in_tx_callback = true;
            if (serialPorts[port].modem_cntl.local_loopback)
            {
                rx_fifo_enq(port, serialPorts[port].tsrbuffer);
            }
            else
            {
                switch (serialPorts[port].io_mode)
                {
                    case SER_MODE_FILE:
                        //  fputc(serialPorts[port].tsrbuffer, serialPorts[port].output);
                        //  fflush(serialPorts[port].output);
                        mParent.mSystem.PrintDebugMsg(eDebuggieNames.Serial, "SERIAL: Sending " + serialPorts[port].tsrbuffer);
                        break;
                    case SER_MODE_TERM:
                        //        DEBUG(("com%d: write: '%c'", port+1, serialPorts[port].tsrbuffer));
                        //        if (serialPorts[port].tty_id >= 0) {
                        //          write(serialPorts[port].tty_id, (ptr_t) & serialPorts[port].tsrbuffer, 1);
                        //        }
                        //#endif
                        break;
                    case SER_MODE_RAW:
#if USE_RAW_SERIAL
        if (!serialPorts[port].raw->ready_transmit())
          PANIC(("com%d: not ready to transmit", port+1));
        serialPorts[port].raw->transmit(serialPorts[port].tsrbuffer);
#endif
                        break;
                    case SER_MODE_MOUSE:
                        //INFO(("com%d: write to mouse ignored: 0x%02x", port+1, serialPorts[port].tsrbuffer));
                        break;
                    //      case SER_MODE_SOCKET_CLIENT:
                    //      case SER_MODE_SOCKET_SERVER:
                    //        if (serialPorts[port].socket_id >= 0) {
                    //#ifdef WIN32
                    //          INFO(("attempting to write win32 : %c", serialPorts[port].tsrbuffer));
                    //          ::send(serialPorts[port].socket_id,
                    //                 (const char*) & serialPorts[port].tsrbuffer, 1, 0);
                    //#else
                    //          ::write(serialPorts[port].socket_id,
                    //                  (ptr_t) & serialPorts[port].tsrbuffer, 1);
                    //#endif
                    //}
                    //break;
                    //      case SER_MODE_PIPE_CLIENT:
                    //      case SER_MODE_PIPE_SERVER:
                    //#ifdef WIN32
                    //        if (serialPorts[port].pipe) {
                    //          DWORD written;
                    //          WriteFile(serialPorts[port].pipe, (ptr_t)& serialPorts[port].tsrbuffer, 1, &written, NULL);
                    //        }
                    //#endif
                    //        break;
                }
            }

            if (serialPorts[port].line_status.tsr_empty == true)
                if (serialPorts[port].fifo_cntl.enable && (serialPorts[port].tx_fifo_end > 0))
            {
                serialPorts[port].tsrbuffer = serialPorts[port].tx_fifo[0];
                serialPorts[port].line_status.tsr_empty = false;
                for (int cnt = 0; cnt < 15; cnt++)
                    serialPorts[port].tx_fifo[cnt] = serialPorts[port].tx_fifo[cnt + 1];
                gen_int = (--serialPorts[port].tx_fifo_end == 0);
            }
            else if (!serialPorts[port].line_status.thr_empty)
            {
                serialPorts[port].tsrbuffer = serialPorts[port].thrbuffer;
                serialPorts[port].line_status.tsr_empty = false;
                gen_int = true;
            }
            if (!serialPorts[port].line_status.tsr_empty)
            {
                if (gen_int)
                {
                    serialPorts[port].line_status.thr_empty = true;
                    raise_interrupt(port, (int)serialInttypes.SER_INT_TXHOLD);
                }
                serialPorts[port].tx_timer.Change(15, Timeout.Infinite);

                //OLD WAS
                //pc_system.activate_timer(serialPorts[port].tx_timer_index,
                //                          (int) (1000000.0 / serialPorts[port].baudrate *
                //                          (serialPorts[port].line_cntl.wordlen_sel + 5)),
                //                          0); /* not continuous */
            }
            else
                serialPorts[port].tx_timer.Change(15, Timeout.Infinite);
            serialPorts[port].in_tx_callback = false;
        }
        void rx_timer(Object stateInfo)
        {
            //#if HAVE_SELECT && defined(SERIAL_ENABLE)
            //  struct timeval tval;
            //  fd_set fds;
            //#endif
            byte port = (byte)stateInfo;

            serialPorts[port].in_rx_callback = true;
            bool data_ready = false;
            int bdrate = serialPorts[port].baudrate / (serialPorts[port].line_cntl.wordlen_sel + 5);
            byte chbuf = 0;

            serialPorts[port].rx_timer.Change(Timeout.Infinite, Timeout.Infinite);
            if (serialPorts[port].io_mode == SER_MODE_TERM)
            {
                //#if HAVE_SELECT && defined(SERIAL_ENABLE)
                //    tval.tv_sec  = 0;
                //    tval.tv_usec = 0;

                //// MacOS: I'm not sure what to do with this, since I don't know
                //// what an fd_set is or what FD_SET() or select() do. They aren't
                //// declared in the CodeWarrior standard library headers. I'm just
                //// leaving it commented out for the moment.

                //    FD_ZERO(&fds);
                //    if (serialPorts[port].tty_id >= 0) FD_SET(serialPorts[port].tty_id, &fds);
                //#endif
            }
            if ((serialPorts[port].line_status.rxdata_ready == false) ||
                (serialPorts[port].fifo_cntl.enable))
            {
                switch (serialPorts[port].io_mode)
                {
                    case SER_MODE_SOCKET_CLIENT:
                    case SER_MODE_SOCKET_SERVER:
                        //#if HAVE_SELECT && defined(SERIAL_ENABLE)
                        //        if (serialPorts[port].line_status.rxdata_ready == 0) {
                        //          tval.tv_sec  = 0;
                        //          tval.tv_usec = 0;
                        //          FD_ZERO(&fds);
                        //          SOCKET socketid = serialPorts[port].socket_id;
                        //          if (socketid >= 0) FD_SET(socketid, &fds);
                        //          if ((socketid >= 0) && (select(socketid+1, &fds, NULL, NULL, &tval) == 1)) {
                        //            ssize_t bytes = (ssize_t)
                        //#ifdef WIN32
                        //              ::recv(socketid, (char*) &chbuf, 1, 0);
                        //#else
                        //                read(socketid, &chbuf, 1);
                        //#endif
                        //            if (bytes > 0) {
                        //              INFO((" -- COM %d : read byte [%d]", port+1, chbuf));
                        //              data_ready = 1;
                        //            }
                        //          }
                        //        }
                        //#endif
                        break;
                    case SER_MODE_RAW:
#if USE_RAW_SERIAL
        int data;
        if ((data_ready = serialPorts[port].raw->ready_receive())) {
          data = serialPorts[port].raw->receive();
          if (data < 0) {
            data_ready = 0;
            switch (data) {
              case RAW_EVENT_BREAK:
                serialPorts[port].line_status.break_int = 1;
                raise_interrupt(port, SER_INT_RXLSTAT);
                break;
              case RAW_EVENT_FRAME:
                serialPorts[port].line_status.framing_error = 1;
                raise_interrupt(port, SER_INT_RXLSTAT);
                break;
              case RAW_EVENT_OVERRUN:
                serialPorts[port].line_status.overrun_error = 1;
                raise_interrupt(port, SER_INT_RXLSTAT);
                break;
              case RAW_EVENT_PARITY:
                serialPorts[port].line_status.parity_error = 1;
                raise_interrupt(port, SER_INT_RXLSTAT);
                break;
              case RAW_EVENT_CTS_ON:
              case RAW_EVENT_CTS_OFF:
              case RAW_EVENT_DSR_ON:
              case RAW_EVENT_DSR_OFF:
              case RAW_EVENT_RING_ON:
              case RAW_EVENT_RING_OFF:
              case RAW_EVENT_RLSD_ON:
              case RAW_EVENT_RLSD_OFF:
                raise_interrupt(port, SER_INT_MODSTAT);
                break;
            }
          }
        }
        if (data_ready) {
          chbuf = data;
        }
#endif
                        break;
                    case SER_MODE_TERM:
                        //#if HAVE_SELECT && defined(SERIAL_ENABLE)
                        //        if ((serialPorts[port].tty_id >= 0) && (select(serialPorts[port].tty_id + 1, &fds, NULL, NULL, &tval) == 1)) {
                        //          (void) read(serialPorts[port].tty_id, &chbuf, 1);
                        //          DEBUG(("com%d: read: '%c'", port+1, chbuf));
                        //          data_ready = 1;
                        //        }
                        //#endif
                        break;
                    case SER_MODE_MOUSE:
                        if (mouse_internal_buffer.num_elements > 0)
                        {
                            chbuf = (byte)mouse_internal_buffer.buffer[mouse_internal_buffer.head];
                            mouse_internal_buffer.head = (mouse_internal_buffer.head + 1) %
                              MOUSE_BUFF_SIZE;
                            mouse_internal_buffer.num_elements--;
                            data_ready = true;
                        }
                        break;
                    //      case SER_MODE_PIPE_CLIENT:
                    //      case SER_MODE_PIPE_SERVER:
                    //#ifdef WIN32
                    //        DWORD avail = 0;
                    //        if (serialPorts[port].pipe &&
                    //            PeekNamedPipe(serialPorts[port].pipe, NULL, 0, NULL, &avail, NULL) &&
                    //            avail > 0) {
                    //          ReadFile(serialPorts[port].pipe, &chbuf, 1, &avail, NULL);
                    //          data_ready = 1;
                    //        }
                    //#endif
                    //        break;
                }
                if (data_ready)
                {
                    if (!serialPorts[port].modem_cntl.local_loopback)
                    {
                        rx_fifo_enq(port, chbuf);
                    }
                }
                else
                {
                    if (!serialPorts[port].fifo_cntl.enable)
                    {
                        bdrate = (int)(1000000.0 / 10000); // Poll frequency is 100ms
                    }
                }
            }
            else
            {
                // Poll at 4x baud rate to see if the next-char can
                // be read
                bdrate *= 4;
            }
            serialPorts[port].rx_timer.Change(50, 0); //(int)(1000000.0 / bdrate));
            //Old was!!!
            //pc_system.activate_timer(serialPorts[port].rx_timer_index,
            //                            (int) (1000000.0 / bdrate), 0); /* not continuous */
            serialPorts[port].in_rx_callback = true;
        }
        void fifo_timer(Object stateInfo)
        {
            byte port = (byte)stateInfo;

            //serialPorts[port].in_fifo_callback = true;
            serialPorts[port].fifo_timer.Change(Timeout.Infinite, Timeout.Infinite);
            serialPorts[port].line_status.rxdata_ready = true;
            raise_interrupt(port, (int)serialInttypes.SER_INT_FIFO);
            //serialPorts[port].in_tx_callback = false;
        }
        #endregion

        #region cDevice Interface Related Methods
        public override void InitDevice()
        {
            base.InitDevice();
        }
        public override void ResetDevice()
        {
        }
        public override void HandleIO(sPortValue IO, eDataDirection Direction)
        {
            switch (Direction)
            {
                case eDataDirection.IO_In:
                    Handle_IN(IO);
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Serial, "\t8250: P=" + IO.Portnum.ToString("X4") + "\tD=" + Direction + " \tV=" + mParent.mSystem.mProc.ports.mPorts[IO.Portnum].ToString("X2"));
                    break;
                case eDataDirection.IO_Out:
                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Serial, "\t8250: P=" + IO.Portnum.ToString("X4") + "\tD=" + Direction + " \tV=" + IO.Value.ToString("X2"));
                    Handle_OUT(IO);
                    break;
            }
        }
        public void Handle_IN(sPortValue IO)
        {
            byte offset, val;
            byte port = 0;

            offset = (byte)(IO.Portnum & 0x07);
            switch (IO.Portnum & 0x03f8)
            {
                case 0x03f8: port = 0; break;
                case 0x02f8: port = 1; break;
                case 0x03e8: port = 2; break;
                case 0x02e8: port = 3; break;
            }

            switch (offset)
            {
                case SER_RBR: /* receive buffer, or divisor latch LSB if DLAB set */
                    if (serialPorts[port].line_cntl.dlab)
                    {
                        val = serialPorts[port].divisor_lsb;
                    }
                    else
                    {
                        if (serialPorts[port].fifo_cntl.enable)
                        {
                            val = serialPorts[port].rx_fifo[0];
                            if (serialPorts[port].rx_fifo_end > 0)
                            {
                                for (int cnt = 0; cnt < 15; cnt++)
                                    serialPorts[port].rx_fifo[cnt] = serialPorts[port].rx_fifo[cnt + 1];
                                serialPorts[port].rx_fifo_end--;
                            }
                            if (serialPorts[port].rx_fifo_end == 0)
                            {
                                serialPorts[port].line_status.rxdata_ready = false;
                                serialPorts[port].rx_interrupt = false;
                                serialPorts[port].rx_ipending = false;
                                serialPorts[port].fifo_interrupt = false;
                                serialPorts[port].fifo_ipending = false;
                                lower_interrupt(port);
                            }
                        }
                        else
                        {
                            val = serialPorts[port].rxbuffer;
                            serialPorts[port].line_status.rxdata_ready = false;
                            serialPorts[port].rx_interrupt = false;
                            serialPorts[port].rx_ipending = false;
                            lower_interrupt(port);
                        }
                    }
                    break;

                case SER_IER: /* interrupt enable register, or div. latch MSB */
                    if (serialPorts[port].line_cntl.dlab)
                    {
                        val = serialPorts[port].divisor_msb;
                    }
                    else
                    {
                        val = (byte)(System.Convert.ToByte(serialPorts[port].int_enable.rxdata_enable) |
                              (System.Convert.ToByte(serialPorts[port].int_enable.txhold_enable) << 1) |
                              (System.Convert.ToByte(serialPorts[port].int_enable.rxlstat_enable) << 2) |
                              (System.Convert.ToByte(serialPorts[port].int_enable.modstat_enable) << 3));
                    }
                    break;

                case SER_IIR: /* interrupt ID register */
                    /*
                     * Set the interrupt ID based on interrupt source
                     */
                    if (serialPorts[port].ls_interrupt)
                    {
                        serialPorts[port].int_ident.int_ID = 0x3;
                        serialPorts[port].int_ident.ipending = false;
                    }
                    else if (serialPorts[port].fifo_interrupt)
                    {
                        serialPorts[port].int_ident.int_ID = 0x6;
                        serialPorts[port].int_ident.ipending = false;
                    }
                    else if (serialPorts[port].rx_interrupt)
                    {
                        serialPorts[port].int_ident.int_ID = 0x2;
                        serialPorts[port].int_ident.ipending = false;
                    }
                    else if (serialPorts[port].tx_interrupt)
                    {
                        serialPorts[port].int_ident.int_ID = 0x1;
                        serialPorts[port].int_ident.ipending = false;
                    }
                    else if (serialPorts[port].ms_interrupt)
                    {
                        serialPorts[port].int_ident.int_ID = 0x0;
                        serialPorts[port].int_ident.ipending = false;
                    }
                    else
                    {
                        serialPorts[port].int_ident.int_ID = 0x0;
                        serialPorts[port].int_ident.ipending = true;
                    }
                    serialPorts[port].tx_interrupt = false;
                    lower_interrupt(port);

                    val = (byte)(System.Convert.ToByte(serialPorts[port].int_ident.ipending) |   //XXX
                          (System.Convert.ToByte(serialPorts[port].int_ident.int_ID) << 1) |
                          (System.Convert.ToByte(serialPorts[port].fifo_cntl.enable) == 1 ? 0xc0 : 0x00));
                    break;

                case SER_LCR: /* Line control register */
                    val = (byte)(System.Convert.ToByte(serialPorts[port].line_cntl.wordlen_sel) |
                          (System.Convert.ToByte(serialPorts[port].line_cntl.stopbits) << 2) |
                          (System.Convert.ToByte(serialPorts[port].line_cntl.parity_enable) << 3) |
                          (System.Convert.ToByte(serialPorts[port].line_cntl.evenparity_sel) << 4) |
                          (System.Convert.ToByte(serialPorts[port].line_cntl.stick_parity) << 5) |
                          (System.Convert.ToByte(serialPorts[port].line_cntl.break_cntl) << 6) |
                          (System.Convert.ToByte(serialPorts[port].line_cntl.dlab) << 7));
                    break;

                case SER_MCR: /* MODEM control register */
                    val = (byte)(System.Convert.ToByte(serialPorts[port].modem_cntl.dtr) |
                          (System.Convert.ToByte(serialPorts[port].modem_cntl.rts) << 1) |
                          (System.Convert.ToByte(serialPorts[port].modem_cntl.out1) << 2) |
                          (System.Convert.ToByte(serialPorts[port].modem_cntl.out2) << 3) |
                          (System.Convert.ToByte(serialPorts[port].modem_cntl.local_loopback) << 4));
                    break;

                case SER_LSR: /* Line status register */
                    val = (byte)(System.Convert.ToByte(serialPorts[port].line_status.rxdata_ready) |
                          (System.Convert.ToByte(serialPorts[port].line_status.overrun_error) << 1) |
                          (System.Convert.ToByte(serialPorts[port].line_status.parity_error) << 2) |
                          (System.Convert.ToByte(serialPorts[port].line_status.framing_error) << 3) |
                          (System.Convert.ToByte(serialPorts[port].line_status.break_int) << 4) |
                          (System.Convert.ToByte(serialPorts[port].line_status.thr_empty) << 5) |
                          (System.Convert.ToByte(serialPorts[port].line_status.tsr_empty) << 6) |
                          (System.Convert.ToByte(serialPorts[port].line_status.fifo_error) << 7));
                    serialPorts[port].line_status.overrun_error = false;
                    serialPorts[port].line_status.framing_error = false;
                    serialPorts[port].line_status.break_int = false;
                    serialPorts[port].ls_interrupt = false;
                    serialPorts[port].ls_ipending = false;
                    lower_interrupt(port);
                    break;

                case SER_MSR: /* MODEM status register */
#if USE_RAW_SERIAL
      if (serialPorts[port].io_mode == SER_MODE_RAW) {
        bool prev_cts = serialPorts[port].modem_status.cts;
        bool prev_dsr = serialPorts[port].modem_status.dsr;
        bool prev_ri  = serialPorts[port].modem_status.ri;
        bool prev_dcd = serialPorts[port].modem_status.dcd;

        val = serialPorts[port].raw->get_modem_status();
        serialPorts[port].modem_status.cts = (val & 0x10) >> 4;
        serialPorts[port].modem_status.dsr = (val & 0x20) >> 5;
        serialPorts[port].modem_status.ri  = (val & 0x40) >> 6;
        serialPorts[port].modem_status.dcd = (val & 0x80) >> 7;
        if (serialPorts[port].modem_status.cts != prev_cts) {
          serialPorts[port].modem_status.delta_cts = 1;
        }
        if (serialPorts[port].modem_status.dsr != prev_dsr) {
          serialPorts[port].modem_status.delta_dsr = 1;
        }
        if ((serialPorts[port].modem_status.ri == 0) && (prev_ri == 1))
          serialPorts[port].modem_status.ri_trailedge = 1;
        if (serialPorts[port].modem_status.dcd != prev_dcd) {
          serialPorts[port].modem_status.delta_dcd = 1;
        }
      }
#endif
                    val = (byte)(System.Convert.ToByte(serialPorts[port].modem_status.delta_cts) |
                          (System.Convert.ToByte(serialPorts[port].modem_status.delta_dsr) << 1) |
                          (System.Convert.ToByte(serialPorts[port].modem_status.ri_trailedge) << 2) |
                          (System.Convert.ToByte(serialPorts[port].modem_status.delta_dcd) << 3) |
                          (System.Convert.ToByte(serialPorts[port].modem_status.cts) << 4) |
                          (System.Convert.ToByte(serialPorts[port].modem_status.dsr) << 5) |
                          (System.Convert.ToByte(serialPorts[port].modem_status.ri) << 6) |
                          (System.Convert.ToByte(serialPorts[port].modem_status.dcd) << 7));
                    serialPorts[port].modem_status.delta_cts = false;
                    serialPorts[port].modem_status.delta_dsr = false;
                    serialPorts[port].modem_status.ri_trailedge = false;
                    serialPorts[port].modem_status.delta_dcd = false;
                    serialPorts[port].ms_interrupt = false;
                    serialPorts[port].ms_ipending = false;
                    lower_interrupt(port);
                    break;

                case SER_SCR: /* scratch register */
                    val = serialPorts[port].scratch;
                    break;

                default:
                    val = 0; // keep compiler happy
                    //      PANIC(("unsupported io read from address=0x%04x!", address));
                    break;
            }

            //  DEBUG(("com%d register read from address: 0x%04x = 0x%02x", port+1, address, val));
            mParent.mSystem.mProc.ports.mPorts[IO.Portnum] = val;
        }

        public void Handle_OUT(sPortValue IO)
        {
            byte port = 0;
            bool gen_int = false;
            byte offset, new_wordlen;

            offset = (byte)(IO.Portnum & 0x07);
            switch (IO.Portnum & 0x03f8)
            {
                case 0x03f8: port = 0; break;
                case 0x02f8: port = 1; break;
                case 0x03e8: port = 2; break;
                case 0x02e8: port = 3; break;
            }

            //  DEBUG(("com%d register write to  address: 0x%04x = 0x%02x", port+1, address, value));

            bool new_b0 = (IO.Value & 0x01) == 1;
            bool new_b1 = ((IO.Value & 0x02) >> 1) == 1;
            bool new_b2 = ((IO.Value & 0x04) >> 2) == 1;
            bool new_b3 = ((IO.Value & 0x08) >> 3) == 1;
            bool new_b4 = ((IO.Value & 0x10) >> 4) == 1;
            bool new_b5 = ((IO.Value & 0x20) >> 5) == 1;
            bool new_b6 = ((IO.Value & 0x40) >> 6) == 1;
            bool new_b7 = ((IO.Value & 0x80) >> 7) == 1;

            switch (offset)
            {
                case SER_THR: /* transmit buffer, or divisor latch LSB if DLAB set */
                    if (serialPorts[port].line_cntl.dlab)
                    {
                        serialPorts[port].divisor_lsb = (byte)IO.Value;

                        if ((IO.Value != 0) || (serialPorts[port].divisor_msb != 0))
                        {
                            serialPorts[port].baudrate = (int)(PC_CLOCK_XTL /
                                                           (16 * ((serialPorts[port].divisor_msb << 8) |
                                                           serialPorts[port].divisor_lsb)));
                        }
                    }
                    else
                    {
                        byte bitmask = (byte)(0xff >> (3 - serialPorts[port].line_cntl.wordlen_sel));
                        if (serialPorts[port].line_status.thr_empty)
                        {
                            if (serialPorts[port].fifo_cntl.enable)
                            {
                                serialPorts[port].tx_fifo[serialPorts[port].tx_fifo_end++] = (byte)(IO.Value & bitmask);
                            }
                            else
                            {
                                serialPorts[port].thrbuffer = (byte)(IO.Value & bitmask);
                            }
                            mSetTHRFalse = true;
                            if (serialPorts[port].line_status.tsr_empty)
                            {
                                if (serialPorts[port].fifo_cntl.enable)
                                {
                                    serialPorts[port].tsrbuffer = serialPorts[port].tx_fifo[0];
                                    for (int cnt = 0; cnt < 15; cnt++)
                                        serialPorts[port].tx_fifo[cnt] = serialPorts[port].tx_fifo[cnt + 1];
                                    serialPorts[port].line_status.thr_empty = (--serialPorts[port].tx_fifo_end == 0);
                                }
                                else
                                {
                                    serialPorts[port].tsrbuffer = serialPorts[port].thrbuffer;
                                    serialPorts[port].line_status.thr_empty = true;
                                }
                                serialPorts[port].line_status.tsr_empty = false;
                                raise_interrupt(port, (int)serialInttypes.SER_INT_TXHOLD);
                                while (serialPorts[port].in_tx_callback)
                                    Thread.Sleep(1);
                                serialPorts[port].tx_timer.Change(100, Timeout.Infinite);
                                //pc_system.activate_timer(serialPorts[port].tx_timer_index,
                                //                          (int) (1000000.0 / serialPorts[port].baudrate *
                                //                          (serialPorts[port].line_cntl.wordlen_sel + 5)),
                                //                          0); /* not continuous */
                            }
                            else
                            {
                                serialPorts[port].tx_interrupt = false;
                                lower_interrupt(port);
                            }
                        }
                        else
                        {
                            if (serialPorts[port].fifo_cntl.enable)
                            {
                                if (serialPorts[port].tx_fifo_end < 16)
                                {
                                    serialPorts[port].tx_fifo[serialPorts[port].tx_fifo_end++] = (byte)(IO.Value & bitmask);
                                }
                                else
                                {
                                    //              ERROR(("com%d: transmit FIFO overflow", port+1));
                                    mParent.mSystem.PrintDebugMsg(eDebuggieNames.Serial, "COM" + (port + 1).ToString("x") + ": transmit FIFO overflow");
                                }
                            }
                            else
                            {
                                //ERROR(("com%d: write to tx hold register when not empty", port+1));
                                mParent.mSystem.PrintDebugMsg(eDebuggieNames.Serial, "COM" + (port + 1).ToString("x") + ": write to txholdregisterwhennotempty");
                            }
                        }
                    }
                    break;

                case SER_IER: /* interrupt enable register, or div. latch MSB */
                    if (serialPorts[port].line_cntl.dlab)
                    {
                        serialPorts[port].divisor_msb = (byte)IO.Value;

                        if ((IO.Value != 0) || (serialPorts[port].divisor_lsb != 0))
                        {
                            serialPorts[port].baudrate = (int)(PC_CLOCK_XTL /
                                                           (16 * ((serialPorts[port].divisor_msb << 8) |
                                                           serialPorts[port].divisor_lsb)));
                        }
                    }
                    else
                    {
                        if (new_b3 != serialPorts[port].int_enable.modstat_enable)
                        {
                            serialPorts[port].int_enable.modstat_enable = new_b3;
                            if (serialPorts[port].int_enable.modstat_enable == true)
                            {
                                if (serialPorts[port].ms_ipending == true)
                                {
                                    serialPorts[port].ms_interrupt = true;
                                    serialPorts[port].ms_ipending = false;
                                    gen_int = true;
                                }
                            }
                            else
                            {
                                if (serialPorts[port].ms_interrupt == true)
                                {
                                    serialPorts[port].ms_interrupt = false;
                                    serialPorts[port].ms_ipending = true;
                                    lower_interrupt(port);
                                }
                            }
                        }
                        if (new_b1 != serialPorts[port].int_enable.txhold_enable)
                        {
                            serialPorts[port].int_enable.txhold_enable = new_b1;
                            if (serialPorts[port].int_enable.txhold_enable == true)
                            {
                                serialPorts[port].tx_interrupt = serialPorts[port].line_status.thr_empty;
                                if (serialPorts[port].tx_interrupt) gen_int = true;
                            }
                            else
                            {
                                serialPorts[port].tx_interrupt = false;
                                lower_interrupt(port);
                            }
                        }
                        if (new_b0 != serialPorts[port].int_enable.rxdata_enable)
                        {
                            serialPorts[port].int_enable.rxdata_enable = new_b0;
                            if (serialPorts[port].int_enable.rxdata_enable == true)
                            {
                                if (serialPorts[port].fifo_ipending == true)
                                {
                                    serialPorts[port].fifo_interrupt = true;
                                    serialPorts[port].fifo_ipending = false;
                                    gen_int = true;
                                }
                                if (serialPorts[port].rx_ipending == true)
                                {
                                    serialPorts[port].rx_interrupt = true;
                                    serialPorts[port].rx_ipending = false;
                                    gen_int = true;
                                }
                            }
                            else
                            {
                                if (serialPorts[port].rx_interrupt == true)
                                {
                                    serialPorts[port].rx_interrupt = false;
                                    serialPorts[port].rx_ipending = true;
                                    lower_interrupt(port);
                                }
                                if (serialPorts[port].fifo_interrupt == true)
                                {
                                    serialPorts[port].fifo_interrupt = false;
                                    serialPorts[port].fifo_ipending = true;
                                    lower_interrupt(port);
                                }
                            }
                        }
                        if (new_b2 != serialPorts[port].int_enable.rxlstat_enable)
                        {
                            serialPorts[port].int_enable.rxlstat_enable = new_b2;
                            if (serialPorts[port].int_enable.rxlstat_enable == true)
                            {
                                if (serialPorts[port].ls_ipending == true)
                                {
                                    serialPorts[port].ls_interrupt = true;
                                    serialPorts[port].ls_ipending = false;
                                    gen_int = true;
                                }
                            }
                            else
                            {
                                if (serialPorts[port].ls_interrupt == true)
                                {
                                    serialPorts[port].ls_interrupt = false;
                                    serialPorts[port].ls_ipending = true;
                                    lower_interrupt(port);
                                }
                            }
                        }
                        if (gen_int) raise_interrupt(port, (int)serialInttypes.SER_INT_IER);
                    }
                    break;

                case SER_FCR: /* FIFO control register */
                    if (new_b0 && !serialPorts[port].fifo_cntl.enable)
                    {
                        //INFO(("com%d: FIFO enabled", port+1));
                        serialPorts[port].rx_fifo_end = 0;
                        serialPorts[port].tx_fifo_end = 0;
                    }
                    serialPorts[port].fifo_cntl.enable = new_b0;
                    if (new_b1)
                    {
                        serialPorts[port].rx_fifo_end = 0;
                    }
                    if (new_b2)
                    {
                        serialPorts[port].tx_fifo_end = 0;
                    }
                    serialPorts[port].fifo_cntl.rxtrigger = (byte)((IO.Value & 0xc0) >> 6);
                    break;

                case SER_LCR: /* Line control register */
                    new_wordlen = (byte)(IO.Value & 0x03);
#if USE_RAW_SERIAL
      if (serialPorts[port].io_mode == SER_MODE_RAW) {
        if (serialPorts[port].line_cntl.wordlen_sel != new_wordlen) {
          serialPorts[port].raw->set_data_bits(new_wordlen + 5);
        }
        if (new_b2 != serialPorts[port].line_cntl.stopbits) {
          serialPorts[port].raw->set_stop_bits(new_b2 ? 2 : 1);
        }
        if ((new_b3 != serialPorts[port].line_cntl.parity_enable) ||
            (new_b4 != serialPorts[port].line_cntl.evenparity_sel) ||
            (new_b5 != serialPorts[port].line_cntl.stick_parity)) {
          if (new_b3 == 0) {
            p_mode = P_NONE;
          } else {
            p_mode = ((value & 0x30) >> 4) + 1;
          }
          serialPorts[port].raw->set_parity_mode(p_mode);
        }
        if ((new_b6 != serialPorts[port].line_cntl.break_cntl) &&
            (!serialPorts[port].modem_cntl.local_loopback)) {
          serialPorts[port].raw->set_break(new_b6);
        }
      }
#endif // USE_RAW_SERIAL
                    serialPorts[port].line_cntl.wordlen_sel = new_wordlen;
                    /* These are ignored, but set them up so they can be read back */
                    serialPorts[port].line_cntl.stopbits = new_b2;
                    serialPorts[port].line_cntl.parity_enable = new_b3;
                    serialPorts[port].line_cntl.evenparity_sel = new_b4;
                    serialPorts[port].line_cntl.stick_parity = new_b5;
                    serialPorts[port].line_cntl.break_cntl = new_b6;
                    if (serialPorts[port].modem_cntl.local_loopback &&
                        serialPorts[port].line_cntl.break_cntl)
                    {
                        serialPorts[port].line_status.break_int = true;
                        serialPorts[port].line_status.framing_error = true;
                        rx_fifo_enq(port, 0x00);
                    }
                    /* used when doing future writes */
                    if (!new_b7 && serialPorts[port].line_cntl.dlab)
                    {
                        // Start the receive polling process if not already started
                        // and there is a valid baudrate.
                        if (serialPorts[port].rx_pollstate == SER_RXIDLE &&
                            serialPorts[port].baudrate != 0)
                        {
                            serialPorts[port].rx_pollstate = SER_RXPOLL;
                            while (serialPorts[port].in_rx_callback)
                                Thread.Sleep(1);
                            serialPorts[port].rx_timer.Change(50, 0);
                            //OLD WAS
                            //pc_system.activate_timer(serialPorts[port].rx_timer_index,
                            //                          (int) (1000000.0 / serialPorts[port].baudrate *
                            //                          (serialPorts[port].line_cntl.wordlen_sel + 5)),
                            //                          0); /* not continuous */
                        }
#if USE_RAW_SERIAL
        if (serialPorts[port].io_mode == SER_MODE_RAW) {
          serialPorts[port].raw->set_baudrate(serialPorts[port].baudrate);
        }
#endif // USE_RAW_SERIAL
                        //DEBUG(("com%d: baud rate set - %d", port+1, serialPorts[port].baudrate));
                    }
                    serialPorts[port].line_cntl.dlab = new_b7;
                    break;

                case SER_MCR: /* MODEM control register */
                    if ((serialPorts[port].io_mode == SER_MODE_MOUSE) &&
                        ((serialPorts[port].line_cntl.wordlen_sel == 2) ||
                         (serialPorts[port].line_cntl.wordlen_sel == 3)))
                    {
                        if (new_b0 && !new_b1) detect_mouse = 1;
                        if (new_b0 && new_b1 && (detect_mouse == 1)) detect_mouse = 2;
                    }
#if USE_RAW_SERIAL
      if (serialPorts[port].io_mode == SER_MODE_RAW) {
        mcr_changed = (serialPorts[port].modem_cntl.dtr != new_b0) |
                      (serialPorts[port].modem_cntl.rts != new_b1);
      }
#endif
                    serialPorts[port].modem_cntl.dtr = new_b0;
                    serialPorts[port].modem_cntl.rts = new_b1;
                    serialPorts[port].modem_cntl.out1 = new_b2;
                    serialPorts[port].modem_cntl.out2 = new_b3;

                    if (new_b4 != serialPorts[port].modem_cntl.local_loopback)
                    {
                        serialPorts[port].modem_cntl.local_loopback = new_b4;
                        if (serialPorts[port].modem_cntl.local_loopback)
                        {
                            /* transition to loopback mode */
#if USE_RAW_SERIAL
          if (serialPorts[port].io_mode == SER_MODE_RAW) {
            if (serialPorts[port].modem_cntl.dtr ||
                serialPorts[port].modem_cntl.rts) {
              serialPorts[port].raw->set_modem_control(0);
            }
          }
#endif
                            if (serialPorts[port].line_cntl.break_cntl)
                            {
#if USE_RAW_SERIAL
            if (serialPorts[port].io_mode == SER_MODE_RAW) {
              serialPorts[port].raw->set_break(0);
            }
#endif
                                serialPorts[port].line_status.break_int = true;
                                serialPorts[port].line_status.framing_error = true;
                                rx_fifo_enq(port, 0x00);
                            }
                        }
                        else
                        {
                            /* transition to normal mode */
#if USE_RAW_SERIAL
          if (serialPorts[port].io_mode == SER_MODE_RAW) {
            mcr_changed = 1;
            if (serialPorts[port].line_cntl.break_cntl) {
              serialPorts[port].raw->set_break(0);
            }
          }
#endif
                        }
                    }

                    if (serialPorts[port].modem_cntl.local_loopback)
                    {
                        bool prev_cts = serialPorts[port].modem_status.cts;
                        bool prev_dsr = serialPorts[port].modem_status.dsr;
                        bool prev_ri = serialPorts[port].modem_status.ri;
                        bool prev_dcd = serialPorts[port].modem_status.dcd;
                        serialPorts[port].modem_status.cts = serialPorts[port].modem_cntl.rts;
                        serialPorts[port].modem_status.dsr = serialPorts[port].modem_cntl.dtr;
                        serialPorts[port].modem_status.ri = serialPorts[port].modem_cntl.out1;
                        serialPorts[port].modem_status.dcd = serialPorts[port].modem_cntl.out2;
                        if (serialPorts[port].modem_status.cts != prev_cts)
                        {
                            serialPorts[port].modem_status.delta_cts = true;
                            serialPorts[port].ms_ipending = true;
                        }
                        if (serialPorts[port].modem_status.dsr != prev_dsr)
                        {
                            serialPorts[port].modem_status.delta_dsr = true;
                            serialPorts[port].ms_ipending = true;
                        }
                        if (serialPorts[port].modem_status.ri != prev_ri)
                            serialPorts[port].ms_ipending = true;
                        if ((serialPorts[port].modem_status.ri == false) && (prev_ri == true))
                            serialPorts[port].modem_status.ri_trailedge = true;
                        if (serialPorts[port].modem_status.dcd != prev_dcd)
                        {
                            serialPorts[port].modem_status.delta_dcd = true;
                            serialPorts[port].ms_ipending = true;
                        }
                        raise_interrupt(port, (int)serialInttypes.SER_INT_MODSTAT);
                    }
                    else
                    {
                        if (serialPorts[port].io_mode == SER_MODE_MOUSE)
                        {
                            if (detect_mouse == 2)
                            {
                                if ((mouse_type == mouseTypes.MOUSE_TYPE_SERIAL) ||
                                    (mouse_type == mouseTypes.MOUSE_TYPE_SERIAL_MSYS))
                                {
                                    mouse_internal_buffer.head = 0;
                                    mouse_internal_buffer.num_elements = 1;
                                    mouse_internal_buffer.buffer[0] = 'M';
                                }
                                else if (mouse_type == mouseTypes.MOUSE_TYPE_SERIAL_WHEEL)
                                {
                                    mouse_internal_buffer.head = 0;
                                    mouse_internal_buffer.num_elements = 6;
                                    mouse_internal_buffer.buffer[0] = 'M';
                                    mouse_internal_buffer.buffer[1] = 'Z';
                                    mouse_internal_buffer.buffer[2] = '@';
                                    mouse_internal_buffer.buffer[3] = '\0';
                                    mouse_internal_buffer.buffer[4] = '\0';
                                    mouse_internal_buffer.buffer[5] = '\0';
                                }
                                detect_mouse = 0;
                            }
                        }

                        if (serialPorts[port].io_mode == SER_MODE_RAW)
                        {
#if USE_RAW_SERIAL
          if (mcr_changed) {
            serialPorts[port].raw->set_modem_control(value & 0x03);
          }
#endif
                        }
                        else
                        {
                            /* simulate device connected */
                            serialPorts[port].modem_status.cts = true;
                            serialPorts[port].modem_status.dsr = true;
                            serialPorts[port].modem_status.ri = false;
                            serialPorts[port].modem_status.dcd = false;
                        }
                    }
                    break;

                case SER_LSR: /* Line status register */
                    //      ERROR(("com%d: write to line status register ignored", port+1));
                    break;

                case SER_MSR: /* MODEM status register */
                    //      ERROR(("com%d: write to MODEM status register ignored", port+1));
                    break;

                case SER_SCR: /* scratch register */
                    //      serialPorts[port].scratch = value;
                    break;

                default:
                    //      PANIC(("unsupported io write to address=0x%04x, value = 0x%02x!",
                    //        (unsigned) address, (unsigned) value));
                    break;
            }
            if (mSetTHRFalse)
            {
                serialPorts[port].line_status.thr_empty = false;
                mSetTHRFalse = false;
            }
        }
        public override void DeviceThread()
        {
            DeviceThreadActive = false;
            return;
            while (1 == 1)
            {
                Thread.Sleep(DEVICE_THREAD_SLEEP_TIMEOUT);
                if (mShutdownRequested)
                    break;
            }
        }
        #endregion

        public struct serial_t
        {
            /*
             * UART internal state
             */
            public bool ls_interrupt;
            public bool ms_interrupt;
            public bool rx_interrupt;
            public bool tx_interrupt;
            public bool fifo_interrupt;
            public bool ls_ipending;
            public bool ms_ipending;
            public bool rx_ipending;
            public bool fifo_ipending;

            public byte IRQ;

            public byte rx_fifo_end;
            public byte tx_fifo_end;

            public int baudrate;
            public int tx_timer_index;

            public int rx_pollstate;
            public int rx_timer_index;
            public int fifo_timer_index;

            public int io_mode;
            public int tty_id;
            /*
               * Register definitions
               */
            public byte rxbuffer;     /* receiver buffer register (r/o) */
            public byte thrbuffer;    /* transmit holding register (w/o) */
            /* Interrupt Enable Register */

            public byte scratch;       /* Scratch Register (r/w) */
            public byte tsrbuffer;     /* transmit shift register (internal) */
            public byte[] rx_fifo;   /* receive FIFO (internal) */ /*16*/
            public byte[] tx_fifo;   /* transmit FIFO (internal) */ /*16*/
            public byte divisor_lsb;   /* Divisor latch, least-sig. byte */
            public byte divisor_msb;   /* Divisor latch, most-sig. byte */
            public s_int_enable int_enable;
            public s_int_ident int_ident;
            public s_fifo_cntl fifo_cntl;
            public s_line_cntl line_cntl;
            public s_modem_cntl modem_cntl;
            public s_line_status line_status;
            public s_modem_status modem_status;
            public System.Threading.Timer rx_timer, tx_timer, fifo_timer;
            public TimerCallback rx_tcb, tx_tcb, fifo_tcb;
            public bool in_tx_callback, in_rx_callback, in_fifo_callback;

            public serial_t(int dummy)
            {
                int_enable = new s_int_enable();
                int_ident = new s_int_ident();
                fifo_cntl = new s_fifo_cntl();
                line_cntl = new s_line_cntl();
                modem_cntl = new s_modem_cntl();
                line_status = new s_line_status();
                modem_status = new s_modem_status();
                scratch = 0;
                tsrbuffer = 0;
                rx_fifo = new byte[16];
                tx_fifo = new byte[16];
                divisor_lsb = 0;
                divisor_msb = 0;
                ls_interrupt = false;
                ms_interrupt = false;
                rx_interrupt = false;
                tx_interrupt = false;
                fifo_interrupt = false;
                ls_ipending = false;
                ms_ipending = false;
                rx_ipending = false;
                fifo_ipending = false;
                IRQ = 0;
                rx_fifo_end = 0;
                tx_fifo_end = 0;
                baudrate = 0;
                tx_timer_index = 0;
                rx_pollstate = 0;
                rx_timer_index = 0;
                fifo_timer_index = 0;
                io_mode = 0;
                tty_id = 0;
                rxbuffer = 0;
                thrbuffer = 0;
                rx_tcb = null; // new TimerCallback(DummyCallback);
                tx_tcb = null; // new TimerCallback(DummyCallback);
                fifo_tcb = null; // new TimerCallback(DummyCallback);
                rx_timer = null;
                tx_timer = null;
                fifo_timer = null;
                in_tx_callback = in_rx_callback = in_fifo_callback = false;

            }
            private static void DummyCallback(Object stateInfo)
            {
                return;
            }


        }
        struct mouse_buffer
        {
            public int num_elements;
            public char[] buffer; //MOUSE_BUFF_SIZE
            public int head;
            public mouse_buffer(int dummy)
            {
                buffer = new char[MOUSE_BUFF_SIZE];
                head = 0;
                num_elements = 0;
            }
        }
        public struct s_int_enable
        {
            public bool rxdata_enable;      /* 1=enable receive data interrupts */
            public bool txhold_enable;      /* 1=enable tx. holding reg. empty ints */
            public bool rxlstat_enable;     /* 1=enable rx line status interrupts */
            public bool modstat_enable;     /* 1=enable modem status interrupts */
        };
        /* Interrupt Identification Register (r/o) */
        public struct s_int_ident
        {
            public bool ipending;           /* 0=interrupt pending */
            public byte int_ID;             /* 3-bit interrupt ID */
        }
        public struct s_fifo_cntl
        {
            public bool enable;             /* 1=enable tx and rx FIFOs */
            public byte rxtrigger;          /* 2-bit code for rx fifo trigger level */
        }
        /* Line Control Register (r/w) */
        public struct s_line_cntl
        {
            public byte wordlen_sel;        /* 2-bit code for char length */
            public bool stopbits;           /* select stop bit len */
            public bool parity_enable;      /* ... */
            public bool evenparity_sel;     /* ... */
            public bool stick_parity;       /* ... */
            public bool break_cntl;         /* 1=send break signal */
            public bool dlab;               /* divisor latch access bit */
        }
        /* MODEM Control Register (r/w) */
        public struct s_modem_cntl
        {
            public bool dtr;                /* DTR output value */
            public bool rts;                /* RTS output value */
            public bool out1;               /* OUTPUT1 value */
            public bool out2;               /* OUTPUT2 value */
            public bool local_loopback;     /* 1=loopback mode */
        }
        /* Line Status Register (r/w) */
        public struct s_line_status
        {
            public bool rxdata_ready;       /* 1=receiver data ready */
            public bool overrun_error;      /* 1=receive overrun detected */
            public bool parity_error;       /* 1=rx char has a bad parity bit */
            public bool framing_error;      /* 1=no stop bit detected for rx char */
            public bool break_int;          /* 1=break signal detected */
            public bool thr_empty;          /* 1=tx hold register (or fifo) is empty */
            public bool tsr_empty;          /* 1=shift reg and hold reg empty */
            public bool fifo_error;         /* 1=at least 1 err condition in fifo */
        }
        /* Modem Status Register (r/w) */
        public struct s_modem_status
        {
            public bool delta_cts;          /* 1=CTS changed since last read */
            public bool delta_dsr;          /* 1=DSR changed since last read */
            public bool ri_trailedge;       /* 1=RI moved from low->high */
            public bool delta_dcd;          /* 1=CD changed since last read */
            public bool cts;                /* CTS input value */
            public bool dsr;                /* DSR input value */
            public bool ri;                 /* RI input value */
            public bool dcd;                /* DCD input value */
        }
    }
}
