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

    class DeviceTemplate : cDevice, iDevice
    {

        #region Private Variables & Constants
        #endregion

        public DeviceTemplate(cDeviceBlock DevBlock)
        {
            mParent = DevBlock;
            mDeviceId = "X";
            mDeviceClass = eDeviceClass.Floppy;
            mName = "";
        }

        #region Device Methods
        #endregion

        #region cDevice Interface Related Methods
        public override void InitDevice()
        {
            //Set up IO handlers and PIC Registration and then call parent InitDevice which will register IO handlers
            //mIOHandlers = new sIOHandler[4];
            //mIOHandlers[0].Device = this; mIOHandlers[0].PortNum = CONFIG_PORT; mIOHandlers[0].Direction = ePortDirection.IO_InOut;
            //mParent.mPIC.RegisterIRQ(this, 6);
            base.InitDevice();
        }
        public override void ResetDevice()
        {
        }
        public override void HandleIO(sPortValue IO, ePortDirection Direction)
        {
            switch (Direction)
            {
                case ePortDirection.IO_In:
                    Handle_IN(IO);
                    break;
                case ePortDirection.IO_Out:
                    Handle_OUT(IO);
                    break;
            }
        }
        public void Handle_IN(sPortValue IO)
        {
        }
        public void Handle_OUT(sPortValue IO)
        {
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


    }
}
