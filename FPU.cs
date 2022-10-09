using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Word = System.UInt16;
using DWord = System.UInt32;
using System.Threading;
using System.Runtime.InteropServices;

namespace VirtualProcessor
{
    struct FPUStatus
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
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct EPDouble
    {
        [FieldOffset(0)]
        internal Word mHigh16;
        [FieldOffset(2)]
        internal UInt64 mSignificand;
        public EPDouble(double Value)
        {
            FPUDouble dbl = new FPUDouble(Value);
            mSignificand = dbl.mSignificand;
            mHigh16 = dbl.MemHigh16;
        }
    }

    public struct FPUDouble
    {
        public double mValue;
        public Word MemHigh16;
        public byte mSign;
        public Word mExponent;
        public UInt64 mSignificand;

        public FPUDouble(double Value)
        {
            mValue = Value;
            mSign = (byte)Extreme.FloatingPoint.FloatingPoint.SignBit(Value);
            mSignificand = (UInt64)Extreme.FloatingPoint.FloatingPoint.Significand(Value);
            mExponent = (Word)Extreme.FloatingPoint.FloatingPoint.Exponent(Value);
            MemHigh16 = (UInt16)((mSign << 0x0f) | (mExponent & 0x7FFF));
        }
    }

    struct FPUControl
    {
        public bool Infinity, PrecisionMask, UnderflowMask, OverflowMask, ZeroDivideMask, DenormalOpMask, InvalidOpMask;
        public byte Rounding, Precision, Bit15, Bit14, Bit13, Bit7, Bit6;
        private Word mValue;

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
        internal EPDouble[] mDataReg = new EPDouble[8];
        public EPDouble[] DataReg
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
            Double d = new Double();
            d = mDataReg[index].mHigh16 << 48;
            d += mDataReg[index].mSignificand & 0xFFFFFFFFFFFFF;
            return d;
        }
        internal FPUControl mControlReg;
        internal double mInternalResultRegister;
        internal FPUTagReg mTagReg;
        internal FPUStatus mStatusReg;
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
            //FPUThreadStart = new ThreadStart(ExecutionThread);
            //FPUProcessingThread = new Thread(FPUThreadStart);
            //FPUProcessingThread.Name = "FPUThread";
            //FPUProcessingThread.Start();
        }
        /// <summary>
        /// Converts the passed "logical" register number to the physical register number
        /// </summary>
        /// <param name="Index">Logical register number to convert</param>
        /// <returns>The physical register number pointed at by the logical number</returns>
        public int ST(int Index)
        {
            int lZero = mStatusReg.Top;

            for (int cnt = 0; cnt <= Index; cnt++)
            {
                lZero -= 1;
                if (lZero == -1)
                    lZero = 7;
            }
            return lZero;
        }
        public void PowerOnReset()
        {
            mDataReg = new EPDouble[8];
            mControlReg = new FPUControl();
            mControlReg.Parse(0x0040);
            mStatusReg = new FPUStatus();
            mTagReg = new FPUTagReg(0);
            for (int c = 0; c < TAG_REGISTER_COUNT; c++)
            {
                mTagReg[c] = 0x01;
                DataReg[c] = new EPDouble(0);
            }
            mInstructQueue = new ArrayList();
        }
        public void Init()
        {
            mControlReg.Parse(0x037F);
            mStatusReg.Parse(0x0000);
            for (int c = 0; c < TAG_REGISTER_COUNT; c++)
                mTagReg[c] = 0x03;
            mInstructQueue = new ArrayList();
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
            Instruct lCurrInstruct;
            while (!m_shutDown)
            {
                mNewCalc.WaitOne();
                while (mInstructQueue.Count > 0)
                {
                    while (mParent.Signals.LOCK)
                        Thread.Sleep(0);
                    mParent.Signals.BUSY = true;
                    lock (mInstructQueue)
                    {
                        lCurrInstruct = (Instruct)mInstructQueue[0];
                        mInstructQueue.RemoveAt(0);
                        lCurrInstruct.Impl();
                        lCurrInstruct.UsageCount++;
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
        public void PushEPDouble(EPDouble Value)
        {
            mStatusReg.Top -= 1;
            if (mStatusReg.Top == -1)
                mStatusReg.Top = 7;
            DataReg[mStatusReg.Top] = Value;
        }
        private void Push(double Value)
        {
            EPDouble dbl = new EPDouble(Value);
            PushEPDouble(dbl);
        }
        public double Pop()
        {
            double lTemp = GetDataRegDouble(ST(0));
            mStatusReg.Top += 1;
            if (mStatusReg.Top == 8)
                mStatusReg.Top = 0;
            return lTemp;
        }

        public EPDouble PopEP()
        {
            EPDouble lTemp = mDataReg[mStatusReg.Top];
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
                lDest = System.Convert.ToInt64(lSource);
            }
            lDest = lDest + lSource;

            switch (OperandCount)
            {
                case 0:
                    
                    DataReg[ST(1)] = new EPDouble(lDest);
                    break;
                case 1:
                case FPU_DEST_INTERNAL_SOURCE_EXTERNAL:
                    DataReg[ST(0)] = new EPDouble(lDest);
                    break;
                case 2:
                    DataReg[ST((int)Dest)] = new EPDouble(lDest);
                    break;
            }
            if (DoPop)
                Pop();

        }
        public void Sub(double Dest, double Source, int OperandCount, bool IntegerSub, bool DoPop)
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
                lDest = System.Convert.ToInt64(lSource);
            }
            lDest = lDest - lSource;

            switch (OperandCount)
            {
                case 0:

                    DataReg[ST(1)] = new EPDouble(lDest);
                    break;
                case 1:
                case FPU_DEST_INTERNAL_SOURCE_EXTERNAL:
                    DataReg[ST(0)] = new EPDouble(lDest);
                    break;
                case 2:
                    DataReg[ST((int)Dest)] = new EPDouble(lDest);
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
                lSource = System.Convert.ToInt64(lSource);

            lDest = lDest * lSource;

            switch (OperandCount)
            {
                case 0:

                    DataReg[ST(1)] = new EPDouble(lDest);
                    break;
                case 1:
                case FPU_DEST_INTERNAL_SOURCE_EXTERNAL:
                    DataReg[ST(0)] = new EPDouble(lDest);
                    break;
                case 2:
                    DataReg[ST((int)Dest)] = new EPDouble(lDest);
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
            if (lDest == 0)
            {
                //mParent.mSystem.DeviceBlock.mPIC.RaiseIRQ(13);
                //mParent.ExceptionNumber = 0x10;
                //mParent.ExceptionErrorCode = 0x5a;
                this.mStatusReg.ZeroDivide = true;
                return;
            }

            if (IntegerDiv)
                lSource = System.Convert.ToInt64(lSource);

            lDest = lDest / lSource;

            switch (OperandCount)
            {
                case 0:

                    DataReg[ST(1)] = new EPDouble(lDest);
                    break;
                case 1:
                case FPU_DEST_INTERNAL_SOURCE_EXTERNAL:
                    DataReg[ST(0)] = new EPDouble(lDest);
                    break;
                case 2:
                    DataReg[ST((int)Dest)] = new EPDouble(lDest);
                    break;
            }
            if (DoPop)
                Pop();
        }
        #endregion
    }

}
