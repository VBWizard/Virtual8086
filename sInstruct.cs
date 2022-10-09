using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualProcessor
{

    interface IInstruct
    {
        void Impl();
    }

    public struct sMOV : IInstruct
    {
        Processor_80x86 mProc;

        public sMOV(Processor_80x86 pProc)
        {
            mProc = pProc;
        }

        public void Impl()
        {

        }
    }

}
