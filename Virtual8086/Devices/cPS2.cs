using System;
using System.Linq;

namespace VirtualProcessor.Devices
{

    public class cPS2 : cDevice, iDevice
    {

        #region Private Variables & Constants
        byte mSystemControlPortA;
        #endregion

        public cPS2(cDeviceBlock DevBlock)
        {
            mParent = DevBlock;
            mDeviceId = "49b0fba1-a365-42d1-85a6-ebe69a568189";
            mDeviceClass = eDeviceClass.PS2;
            mName = "PS2 (Control Port only)";
        }

        #region Device Methods
        #endregion

        #region cDevice Interface Related Methods
        public override void InitDevice()
        {
            //SetC up IO handlers and PIC Registration and then call parent InitDevice which will register IO handlers
            mIOHandlers = new sIOHandler[1];
            mIOHandlers[0].Device = this; mIOHandlers[0].PortNum = 0x92; mIOHandlers[0].Direction = eDataDirection.IO_InOut;
            //mParent.mPIC.RegisterIRQ(this, 6);
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
                    break;
                case eDataDirection.IO_Out:
                    Handle_OUT(IO);
                    break;
            }
        }
        public void Handle_IN(sPortValue IO)
        {
            mParent.mProc.ports.mPorts[0x92] = mSystemControlPortA;
        }
        public void Handle_OUT(sPortValue IO)
        {
            mSystemControlPortA = (byte)IO.Value;
        }
        public override void DeviceThread()
        {
            DeviceThreadActive = false;
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
