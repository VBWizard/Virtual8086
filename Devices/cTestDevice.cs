using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VirtualProcessor.Devices
{
    class cTestDevice : cDevice
    {

        public cTestDevice(cDeviceBlock Parent)
        {
            mParent = Parent;
        }

        public override void InitDevice()
        {
            //mParent.mPIC.RegisterIRQ(this, 1);
        }

        public override void DeviceThread()
        {
            //Thread.Sleep(20000);
            //mParent.mPIC.mMasterIMR = 0xff;
            //mParent.mKeyboard.GenerateScanCode(0x40);
            //System.Diagnostics.Debug.WriteLine("Generated fake scancode!");
            return;
        }

    }
}
