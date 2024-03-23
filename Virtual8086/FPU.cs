using System;
using System.Collections;
using System.Linq;
using Word = System.UInt16;
using System.Threading;
using System.Runtime.InteropServices;

namespace VirtualProcessor
{
    public struct FPUStatus
    {
        public bool Busy, InvalidOp, DenormOp, ZeroDivide, Overflow, Underflow, Precision, StackFault, ErrSummaryStat, CC0, CC1, CC2, CC3;
        public int Top;

        public void Parse(Word Value)
        {
            InvalidOp = (Value & 0x01) == 0x01;
            DenormOp = (Value & 0x02) == 0x02;
            ZeroDivide = (Value & 0x04) == 0x04;
            Overflow = (Value & 0x08) == 0x08;
            Underflow = (Value & 0x10) == 0x10;
            Precision = (Value & 0x20) == 0x20;
            StackFault = (Value & 0x40) == 0x40;
            ErrSummaryStat = (Value & 0x80) == 0x80;
            CC0 = (Value & 0x100) == 0x100;
            CC1 = (Value & 0x200) == 0x200;
            CC2 = (Value & 0x400) == 0x400;
            Top = (byte)((Value & 0x3800) >> 0x0b);
            CC3 = (Value & 0x4000) == 0x4000;
            Busy = (Value & 0x8000) == 0x8000;
        }
        public Word Value
        {
            get
            {
                Word lValue = (Word)((((Busy == true) ? 1 : 0) << 15)
                    | (((CC3 == true) ? 1 : 0) << 14)
                    | (Top << 11)
                    | (((CC2 == true) ? 1 : 0) << 10)
                    | (((CC1 == true) ? 1 : 0) << 9)
                    | (((CC0 == true) ? 1 : 0) << 8)
                    | (((ErrSummaryStat == true) ? 1 : 0) << 7)
                    | (((StackFault == true) ? 1 : 0) << 6)
                    | (((Precision == true) ? 1 : 0) << 5)
                    | (((Underflow == true) ? 1 : 0) << 4)
                    | (((Overflow == true) ? 1 : 0) << 3)
                    | (((ZeroDivide == true) ? 1 : 0) << 2)
                    | (((DenormOp == true) ? 1 : 0) << 1)
                    | (((InvalidOp == true) ? 1 : 0))
                    );

                return lValue;
            }
            set
            {
                Parse(value);
            }
        }
    }

    struct FPUControl
    {
        public bool Infinity, PrecisionMask, UnderflowMask, OverflowMask, ZeroDivideMask, DenormalOpMask, InvalidOpMask;
        public byte Rounding, Precision, Bit15, Bit14, Bit13, Bit7, Bit6;

        public void Parse(Word Value)
        {
            InvalidOpMask = (Value & 0x01) == 0x01;
            DenormalOpMask = (Value & 0x02) == 0x02;
            ZeroDivideMask = (Value & 0x04) == 0x04;
            OverflowMask = (Value & 0x08) == 0x08;
            UnderflowMask = (Value & 0x10) == 0x10;
            PrecisionMask = (Value & 0x20) == 0x20;
            Bit6 = (Byte)((Value >> 0x06) & 0x01);
            Bit7 = (Byte)((Value >> 0x07) & 0x01);
            Precision = (byte)((Value & 0x300) >> 0x8);
            Rounding = (byte)((Value & 0xC00) >> 0x0a);
            Infinity = (Value & 0x1000) == 0x1000;
            Bit13 = (Byte)((Value >> 0x0d) & 0x01);
            Bit14 = (Byte)((Value >> 0x0e) & 0x01);
            Bit15 = (Byte)((Value >> 0x0f) & 0x01);
        }
        public Word Value
        {
            get
            {
                Word lValue = (Word)( 
                    ( (Bit15 & 0x01) << 0x0f)
                    | ((Bit14 & 0x01) << 0x0e)
                    | ((Bit13 & 0x01) << 0x0d)
                    | (( (Infinity==true) ? 1 : 0) << 0x0c) 
                    | (Rounding << 0x0a)
                    | (Precision << 0x08)
                    | ((Bit7 & 0x01) << 0x07)
                    | ((Bit6 & 0x01) << 0x06)
                    | (((PrecisionMask == true) ? 1 : 0) << 5)
                    | (((UnderflowMask == true) ? 1 : 0) << 4)
                    | (((OverflowMask == true) ? 1 : 0) << 3)
                    | (((ZeroDivideMask == true) ? 1 : 0) << 2)
                    | (((DenormalOpMask == true) ? 1 : 0) << 1)
                    | (((InvalidOpMask == true) ? 1 : 0)) 
                    ) ;

                return lValue;
            }
        }
    }

    struct FPUTagReg
    {
        internal byte[] TagRegister;

        public Word Value
        {
            get
            {
                Word lValue = 0;
                for (int c = 0; c < 8; c++)
                    lValue |= (Word)( (TagRegister[c] & 0x02) << c);
                return lValue;
            }
        }
        public byte this[int Idx]
        {
            get { return TagRegister[Idx]; }
            set { TagRegister[Idx] = value; }
        }
        public FPUTagReg(int Dummy)
        {
            TagRegister = new byte[8];
        }

    }

    public class FPU
    {
        internal static AutoResetEvent mNewCalc = new AutoResetEvent(false);
        public const int FPU_DEST_INTERNAL_SOURCE_EXTERNAL = 0xF0;
        internal const int TAG_REGISTER_COUNT = 8, DATA_REGISTER_COUNT = TAG_REGISTER_COUNT;
        internal Double[] mDataReg = new Double[8];
        public Double[] DataReg
        { get { return mDataReg; } set { mDataReg = value; } }
        //http://upload.wikimedia.org/wikipedia/commons/d/d2/Float_example.svg
        public Single GetDataRegSingle(int index)
        {
            double d = GetDataRegDouble(index);
            return System.Convert.ToSingle(d);
        }
        //http://upload.wikimedia.org/wikipedia/commons/a/a9/IEEE_754_Double_Floating_Point_Format.svg
        public Double GetDataRegDouble(int index)
        {
            //int lNewIndex = index;
            //int ptr = mStatusReg.Top + lNewIndex;
            //if (ptr > 7)
            //{
            //    ptr = ptr - 7;
            //    ptr = 0 + ptr - 1;
            //}
            //return mDataReg[ptr];
            return mDataReg[index];
        }
        internal FPUControl mControlReg;
        internal FPUTagReg mTagReg;
        internal FPUStatus mStatusReg;
        public FPUStatus StatusReg
        {
            get { return mStatusReg; }
        }
        internal UInt64 mLastData;
        internal UInt32 mLastOpCode, mLastOperandPtr, mLastOperandSegSel, mLastIP, mLastSegSel;
        internal bool m_shutDown = false;
        internal readonly Processor_80x86 mParent;
        internal System.Collections.ArrayList mInstructQueue;
        System.Threading.ThreadStart FPUThreadStart;
        System.Threading.Thread FPUProcessingThread;

        public FPU(Processor_80x86 mProc)
        {
            mParent = mProc;
            PowerOnReset();
            mStatusReg.Value = 0;
            FPUThreadStart = new ThreadStart(ExecutionThread);
            FPUProcessingThread = new Thread(FPUThreadStart);
            FPUProcessingThread.Name = "FPUThread";
            FPUProcessingThread.Start();
        }
        /// <summary>
        /// Converts the passed "logical" register number to the physical register number
        /// </summary>
        /// <param name="Index">Logical register number to convert</param>
        /// <returns>The physical register number pointed at by the logical number</returns>
        public int ST(int Index)
        {
            int lZero = mStatusReg.Top;

            for (int cnt = 0; cnt < Index; cnt++)
            {
                lZero += 1;
                if (lZero == 8)
                    lZero = 0;
            }
            return lZero;
        }
        public void PowerOnReset()
        {
            mDataReg = new Double[8];
            mControlReg = new FPUControl();
            mControlReg.Parse(0x0040);
            mStatusReg = new FPUStatus();
            mTagReg = new FPUTagReg(0);
            for (int c = 0; c < TAG_REGISTER_COUNT; c++)
            {
                mTagReg[c] = 0x01;
                DataReg[c] = new Double();
            }
            mInstructQueue = new ArrayList();
        }
        public void Init()
        {
            System.Threading.Thread thisThread = Thread.CurrentThread;
            thisThread.Priority = ThreadPriority.Highest;
            mControlReg.Parse(0x037F);
            mStatusReg.Parse(0x0000);
            for (int c = 0; c < TAG_REGISTER_COUNT; c++)
                mTagReg[c] = 0x03;
            mInstructQueue = new ArrayList();
            mStatusReg.Top = 8;
        }
        public void QueueInstruct(Instruct i)
        {
            mInstructQueue.Add(i);
        }
        public void ShutDown()
        {
            m_shutDown = true;
        }
        public void ExecutionThread()
        {
            sInstruction lCurrInstruct;
            while (!m_shutDown)
            {
                Thread.Sleep(1);
                while (mInstructQueue.Count > 0)
                {
                    mParent.Signals.BUSY = true;
                    lock (mInstructQueue)
                    {
                        lCurrInstruct = (sInstruction)mInstructQueue[0];
                        mInstructQueue.RemoveAt(0);
                            lCurrInstruct.mChosenInstruction.Impl(ref lCurrInstruct);
                        lCurrInstruct.mChosenInstruction.UsageCount++;
                    }
                }
                mParent.Signals.BUSY = false;
                if (mParent.PowerOff)
                    return;
            }
        }

        public void PushInt(Int16 Value)
        {
            double lValue = System.Convert.ToDouble(Value);
            Push(lValue);
        }
        public void PushInt(Int32 Value)
        {
            double lValue = System.Convert.ToDouble(Value);
            Push(lValue);
        }
        public void PushInt(Int64 Value)
        {
            double lValue = System.Convert.ToDouble(Value);
            Push(lValue);
        }
        
        public void Push(double Value)
        {
            mStatusReg.Top -= 1;
            if (mStatusReg.Top == -1)
                mStatusReg.Top = 7;
            DataReg[mStatusReg.Top] = Value;
        }
        public double Pop()
        {
            double lTemp = GetDataRegDouble(ST(0));
            mStatusReg.Top += 1;
            if (mStatusReg.Top == 8)
                mStatusReg.Top = 0;
            return lTemp;
        }


        #region Math
        public void Add(double Dest, double Source, int OperandCount, bool IntegerAdd, bool DoPop)
        {
            double lDest = Dest;
            double lSource = Source;

            switch (OperandCount)
            {
                case 0:
                    lDest = GetDataRegDouble(ST(0));
                    lSource = GetDataRegDouble( ST(mStatusReg.Top + 1));
                    break;
                case 1:
                    lSource = GetDataRegDouble(ST(0));
                    break;
                case 2: //For 2 operands, op#s are in the source/dest parameters
                    lDest = GetDataRegDouble(ST((int)Dest));
                    lSource = GetDataRegDouble(ST((int)Source));
                    break;
                case FPU_DEST_INTERNAL_SOURCE_EXTERNAL:
                    lDest = GetDataRegDouble(ST((int)Dest));
                    lSource = Source;
                    break;
            }

            if (IntegerAdd)
            {
                lSource = System.Convert.ToInt64(lSource);
                lDest = System.Convert.ToInt64(lDest);
            }
            lDest = lDest + lSource;

            switch (OperandCount)
            {
                case 0:
                    
                    DataReg[ST(1)] = lDest;
                    break;
                case 1:
                case FPU_DEST_INTERNAL_SOURCE_EXTERNAL:
                    DataReg[ST(0)] = lDest;
                    break;
                case 2:
                    DataReg[ST((int)Dest)] = lDest;
                    break;
            }
            if (DoPop)
                Pop();

        }
        public void Sub(double Dest, double Source, int OperandCount, bool IntegerSub, bool DoPop, bool ReverseSubtraction)
        {
            double lDest = Dest;
            double lSource = Source;

            switch (OperandCount)
            {
                case 0:
                    lDest = GetDataRegDouble(ST(0));
                    lSource = GetDataRegDouble(ST(mStatusReg.Top + 1));
                    break;
                case 1:
                    lSource = GetDataRegDouble(ST(0));
                    break;
                case 2: //For 2 operands, op#s are in the source/dest parameters
                    lDest = GetDataRegDouble(ST((int)Dest));
                    lSource = GetDataRegDouble(ST((int)Source));
                    break;
                case FPU_DEST_INTERNAL_SOURCE_EXTERNAL:
                    lDest = GetDataRegDouble(ST((int)Dest));
                    lSource = Source;
                    break;
            }

            if (IntegerSub)
            {
                lSource = System.Convert.ToInt64(lSource);
                lDest = System.Convert.ToInt64(lDest);
            }
            if (ReverseSubtraction)
                lDest = lSource - lDest;
            else
                lDest = lDest - lSource;

            switch (OperandCount)
            {
                case 0:

                    DataReg[ST(1)] = lDest;
                    break;
                case 1:
                case FPU_DEST_INTERNAL_SOURCE_EXTERNAL:
                    DataReg[ST(0)] = lDest;
                    break;
                case 2:
                    DataReg[ST((int)Dest)] = lDest;
                    break;
            }
            if (DoPop)
                Pop();

        }
        public void Multiply(double Dest, double Source, int OperandCount, bool IntegerMul, bool DoPop)
        {
            double lDest = Dest;
            double lSource = Source;
            switch (OperandCount)
            {
                case 0:
                    lDest = GetDataRegDouble(ST(0));
                    lSource = GetDataRegDouble(ST(mStatusReg.Top + 1));
                    break;
                case 1:
                    lSource = GetDataRegDouble(ST(0));
                    break;
                case 2: //For 2 operands, op#s are in the source/dest parameters
                    lDest = GetDataRegDouble(ST((int)Dest));
                    lSource = GetDataRegDouble(ST((int)Source));
                    break;
                case FPU_DEST_INTERNAL_SOURCE_EXTERNAL:
                    lDest = GetDataRegDouble(ST((int)Dest));
                    lSource = Source;
                    break;
            }

            if (IntegerMul)
            {
                lSource = System.Convert.ToInt64(lSource);
                lDest = System.Convert.ToInt64(lDest);
            }

            lDest = lDest * lSource;

            switch (OperandCount)
            {
                case 0:

                    DataReg[ST(1)] = lDest;
                    break;
                case 1:
                case FPU_DEST_INTERNAL_SOURCE_EXTERNAL:
                    DataReg[ST((int)Dest)] = lDest;
                    break;
                case 2:
                    DataReg[ST((int)Dest)] = lDest;
                    break;
            }
            if (DoPop)
                Pop();
        }
        public void Divide(double Dest, double Source, int OperandCount, bool IntegerDiv, bool DoPop)
        {
            double lDest = Dest;
            double lSource = Source;

            switch (OperandCount)
            {
                case 0:
                    lDest = GetDataRegDouble(ST(0));
                    lSource = GetDataRegDouble(ST(mStatusReg.Top + 1));
                    break;
                case 1:
                    lSource = GetDataRegDouble(ST(0));
                    break;
                case 2: //For 2 operands, op#s are in the source/dest parameters
                    lDest = GetDataRegDouble(ST((int)Dest));
                    lSource = GetDataRegDouble(ST((int)Source));
                    break;
                case FPU_DEST_INTERNAL_SOURCE_EXTERNAL:
                    lDest = GetDataRegDouble(ST((int)Dest));
                    lSource = Source;
                    break;
            }

            if (IntegerDiv)
            {
                lSource = System.Convert.ToInt64(lSource);
                lDest = System.Convert.ToInt64(lDest);
            }

            if (lSource == 0)
            {
                //mParent.mSystem.DeviceBlock.mPIC.RaiseIRQ(13);
                //mParent.ExceptionNumber = 0x10;
                //mParent.ExceptionErrorCode = 0x5a;
                this.mStatusReg.ZeroDivide = true;
                return;
            }

            lDest = lDest / lSource;

            switch (OperandCount)
            {
                case 0:

                    DataReg[ST(1)] = lDest;
                    break;
                case 1:
                case FPU_DEST_INTERNAL_SOURCE_EXTERNAL:
                    DataReg[ST(0)] = lDest;
                    break;
                case 2:
                    DataReg[ST((int)Dest)] = lDest;
                    break;
            }
            if (DoPop)
                Pop();
        }
        #endregion
    }

}
