using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.Data;
using System.IO;
using System.Threading;
using Word = System.UInt16;
using DWord = System.UInt32;

namespace VirtualProcessor
{
    public struct sOpCodePointer
    {
        public UInt16 OpCodeNum;
        public String InstructionName;
        public sOpCode OpCode;
    }
    public enum eProcTypes
    {
        i8086,
        i8088,
        i80186,
        i80286,
        i80386,
        i80486,
        iPentium,
        iPentiumPro
    }
    public struct sSignals
    {
        public bool
            /*For CPU usage*/
            BUSY,
            /*LOCK: output indicates that other system bus masters are not to gain
            control of the system bus while LOCK is active LOW. The LOCK signal
            is activated by the ``LOCK'' prefix instruction and remains active until the
            completion of the next instruction. This signal is active LOW, and floats
            to 3-state OFF in ``hold acknowledge''.*/
            LOCK,
            READ,
            /*INTR: is a level triggered input which is sampled
            during the last clock cycle of each instruction to determine if the
            processor should enter into an interrupt acknowledge operation. A
            subroutine is vectored to via an interrupt vector lookup table located in
            system memory. It can be internally masked by software resetting the
            interrupt enable bit. INTR is internally synchronized. This signal is
            active HIGH.*/
            INTR,
            /*TEST: input is examined by the ``Wait'' instruction. If the TEST input is
            LOW execution continues, otherwise the processor waits in an ``Idle''
            state. This input is synchronized internally during each clock cycle on
            the leading edge of CLK*/
            TEST,
            /*NMI: (NON-MASKABLE INTERRUPT) an edge triggered input which causes
            a type 2 interrupt. A subroutine is vectored to via an interrupt vector
            lookup table located in system memory. NMI is not maskable internally
            by software. A transition from LOW to HIGH initiates the interrupt at the
            end of the current instruction. This input is internally synchronized*/
            NMI,
            /*RESET: causes the processor to immediately terminate its present
            activity. The signal must be active HIGH for at least four clock cycles. It
            restarts execution, as described in the Instruction SetC description, when
            RESET returns LOW. RESET is internally synchronized.*/
            RESET,
            /*CLOCK: provides the basic timing for the processor and bus controller.
            It is asymmetric with a 33% duty cycle to provide optimized internal
            timing.*/
            CLK,
            /*READY is the acknowledgement from the addressed memory or I/O device that 
            it will complete the data transfer*/
            READY,
            /*The 8086 has a pin called HOLD. This pin is used by external devices to gain control of the bus.*/
            HOLD,
            /*When the HOLD signal is activated by an external device, the 8086 stops executing instructions
            and stops using the busses. This would allow external devices to control the information on the 8086 */
            HLDA,
            TC;
    }

    public class Processor_80x86
    {
        #region variables/properties
        public const byte REPEAT_TILL_ZERO = 0, REPEAT_TILL_NOT_ZERO = 1, NOT_REPEAT = 0xFF;
        public PCSystem mSystem;
        public FPU mFPU;
        private eProcTypes mProcType;
        public eProcTypes ProcType
        {
            get { return mProcType; }
        }
        public RegStruct regs, savedRegs;
        public Ports ports;
        public PhysicalMem mem;
        internal bool[] DRQ = new bool[8] { false, false, false, false, false, false, false, false };
        internal bool[] DACK = new bool[8] { false, false, false, false, false, false, false, false };
        internal bool mTC = false, mSTICalled, mSTIAfterNextInstr;
        internal bool TC
        { get { return mTC; } set { mTC = value; } }
        public bool mCurrInstructOpSize16, mCurrInstructAddrSize16;
        public bool OpSize16
        {
            get
            {
                if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
                {
                    //if mOpSize16=false - override prefix was found so flip the value we would otherwise send back
                    //(note: we would have sent back ! so we'll "not send back not"
                    if (regs.CS.mDescriptorNum == 0)
                        return mOpSize16;
                    if (!mOpSize16)
                        return regs.CS.Selector.granularity.OpSize32;
                    else
                        return !regs.CS.Selector.granularity.OpSize32;
                    
                    //Would like to use this logic instead
                    //bool retVal = !regs.CS.Selector.granularity.OpSize32;
                    //if (mProcessorStatus != eProcessorStatus.Decoding && sCurrentDecode.OpSizePrefixFound)
                    //    return !retVal;
                    //else
                    //    return retVal;
                }
                //In real mode if operandsize override prefix is passed, mOpSize will be false, meaning 32 bit
                else
                    return mOpSize16;
            }
        }
        public bool AddrSize16
        {
            get
            {
                if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
                {
                    //if mAddrSize16=false - override prefix was found so flip the value we would otherwise send back
                    //(note: we would have sent back ! so we'll "not send back not"
                    if (regs.CS.mDescriptorNum == 0)
                        return mAddrSize16;
                    if ( mProcessorStatus != eProcessorStatus.Decoding && sCurrentDecode.AddrSizePrefixFound)
                        return regs.CS.Selector.granularity.OpSize32;
                    else
                        return !regs.CS.Selector.granularity.OpSize32;
                    //In real mode if operandsize override prefix is passed, mOpSize will be false, meaning 32 bit
                }
                else
                    //True in real mode = 16 bit
                    return mAddrSize16;
            }

        }
        public bool AddrSize16Stack
        {
            get
            {
                if (Processor_80x86.mCurrInstructOpMode == ProcessorMode.Protected)
                {
                    //if mAddrSize16=false - override prefix was found so flip the value we would otherwise send back
                    //(note: we would have sent back ! so we'll "not send back not"
                    if (regs.SS.mDescriptorNum == 0)
                        //07/23/2013 - mOpSize16 just ain't workin!
                        return mAddrSize16;
                    //return !regs.CS.Selector.granularity.OpSize32;
                    if (mOverrideStackSize)
                        return mOverriddenStackSizeIs16;
                     return (!regs.SS.Selector.granularity.OpSize32);
                }
                else
                    //CLR 07/30/2015 - Changed to just return the boolean value rather than evaluating it and then returning the same value statically
                    return mAddrSize16;
            }

        }
        public UInt64 TestInstructKillCount = 0;
        /// <summary>
        /// List of instructions, used only to get the right sOpCode for a given instruction
        /// </summary>
        public InstructionList<Instruct> Instructions = new InstructionList<Instruct>();
        public sOpCodePointer[] OpCodeIndexer = new sOpCodePointer[0xFFFF];
        internal sSignals Signals;
        internal static bool mProtectedModeActive = false;
        internal bool mIncrementedEIP = false;
        public bool mGenerateDecodeStrings = false, mCalculateInstructionTimings = false, mExternalPauseActive = false;
        public static ProcessorMode mCurrInstructOpMode;
        public ProcessorMode OperatingMode
        {
            get
            {
                return mProtectedModeActive ?
                    (((regs.EFLAGS & 0x20000) != 0x20000) ?
                        ProcessorMode.Protected
                        : ProcessorMode.Virtual8086)
                    : ProcessorMode.Real;
            }
        }
        public bool ProtectedModeActive
        {
            get
            {
                return (mProtectedModeActive/* && (regs.EFLAGS & 0x20000) != 0x20000)*/);
            }
        }
        //public sOperation mCurrentOperation;
        internal bool mOpSize16 = true, mAddrSize16 = true, mOverrideStackSize = false, mOverriddenStackSizeIs16 = false, mExceptionWhileExcepting;
        public UInt32 mSegmentOverride = 0;
        internal byte mBranchHint = 0;
        internal UInt64 mInsructionsExecuted, mInsructionsDecoded, mGDTCacheResetsExecuted;
        internal double mTimeInGDTRefresh = 0;
        public double TimeInGDTRefresh
        { get { return mTimeInGDTRefresh; } }
        public double mMSinTimedSection = 0;
        public UInt64 InstructionsExecuted
        { get { return mInsructionsExecuted; } }
        public UInt64 GDTCacheResetsExecuted
        { get { return mGDTCacheResetsExecuted; } }
        public UInt64 InstructionsDecoded
        { get { return mInsructionsDecoded; } }
        public DateTime StartedAt;
        public DateTime StoppedAt;
        public bool HaltProcessor = false, PowerOff = false, TrapNextInst = false, ServicingIRQ = false, mExportDebug = false;
        private int mExceptionNumber = Global.CPU_NO_EXCEPTION;
        public UInt32 mLastInstructionAddress = 0, mCurrentInstructionAddress = 0, mExceptionTransferAddress = 0, mLastEIP = 0, mLastESP = 0;
        public int mTimerTickMSec = 52; //Default is just over 18 times per second (18.222)
        public cGDTCache mGDTCache, mLDTCache;
        public cIDTCache mIDTCache;
        internal sTSS mCurrTSS = new sTSS();
        //Used when rep type prefixes are called
        //Used by commands like REP, set to true till condition is met or CX = 0
        //Instruction will set back to false if condition met/not met
        //Processor will set back to false if CX = 0
        private Decoder mDecoder;
        //internal cInstructionCache InstructionCache = new cInstructionCache();
//        internal cInstructCache InstructCache = new cInstructCache();
        public cTLB mTLB;
        public Int64 mCacheHits, mCacheMisses;
        public byte mRepeatCondition = NOT_REPEAT;
        public OpcodeLoader ocl = new OpcodeLoader();
        public DWord mFlagsBefore;
        public byte mLastIRQVector = 0;
        public eSwitchSource mSwitchSource;
        internal eProcessorStatus mProcessorStatus = eProcessorStatus.Decoding;
        public eProcessorStatus ProcessorStatus { get { return mProcessorStatus; } }
        public bool mSingleStep = false;
        CEAStartDebugging CAEStartDeb;
        public sInstruction sCurrentDecode;

        #endregion

        /// <summary>
        /// Default constructor - 8086 with 1 MB of memory
        /// </summary>
        public Processor_80x86(PCSystem System, UInt32 TotalMemory, eProcTypes ProcessorType)
        {
            mProcType = ProcessorType;
            mSystem = System;
            if (TotalMemory < 1024 * 1024)
                throw new Exception("TOtal memory must be at least " + (1024 * 1024).ToString("0,000"));
            mem = new PhysicalMem(this,TotalMemory);
            Init(TotalMemory, ProcessorType);
        }
        public void Init(UInt32 TotalMemory, eProcTypes ProcessorType)
        {
            //p.ProcessorAffinity = (IntPtr)0xE0;

            for (int cnt = 0; cnt < 0xFFFF; cnt++)
                OpCodeIndexer[cnt].OpCode.OpCode = 0xFFFF;
            mFPU = new FPU(this);
            //REGADDRBASE = 0xF0000000; //TotalMemory;
            InitRegLocations();
            regs = new RegStruct(this);
            savedRegs = new RegStruct(this);
            mProcType = ProcessorType;
            ports = new Ports(mSystem);
            regs.ResetFlags();
            if (Instructions == null || Instructions.Count == 0)
                InitInstructions(Instructions);
            if (OpCodeIndexer == null || OpCodeIndexer[0xea].OpCodeNum == 0)
            {
                ocl.AddOpCodesToInstructionList(ref Instructions, ref OpCodeIndexer);
                Instructions.FixupIndex(this);
            }
            mProtectedModeActive = false;
            mCurrInstructOpMode = ProcessorMode.Real;
            mOpSize16 = true;
            mAddrSize16 = true;
            Signals.CLK = false;
            Signals.INTR = false;
            Signals.LOCK = false;
            Signals.NMI = false;
            Signals.READ = false;
            Signals.RESET = false;
            Signals.TEST = false;
            Signals.HOLD = false;
            Signals.HLDA = false;
            Signals.BUSY = false;

            mLastInstructionAddress = 0;
            //top 16 bytes must be empty because base is bits 48-16, limit is bits 15-0
            //32 bit linear base and 16 bit limit
            regs.GDTR.Parse(this, Misc.GetPModeDescriptor(0, 0xffff));
            mDecoder = new Decoder(this, Instructions, OpCodeIndexer, true, true);
            mTLB = new cTLB();
            regs.EFLAGS = 0x0;
            regs.CS.Value = 0xf000;
            regs.EIP = 0x0000FFF0;
            if (mSystem.mCR0_WP_Honor_In_Sup_Mode)
                regs.CR0 = 0x60010000; //Old value = 0x60000010, changed to deactivate math co
            else
                regs.CR0 = 0x60000000; //Old value = 0x60000010, changed to deactivate math co
            if (mSystem.bFPUEnabled)
                regs.CR0 |= 0x30;
            else
                regs.CR0 |= 4;
            regs.CR2 = regs.CR3 = regs.CR4 = 0;
            regs.EDX = 0xf15;
            regs.EAX = regs.EBX = regs.ECX = regs.ESI = regs.EDI = regs.EBP = regs.ESP = 0;
            regs.SS.Value = 0xa000;
            regs.SI = 0xFFF0;
            regs.SP = 0xFFFE;
            regs.DS.Value = 0xF000;
        }
        internal void SetupInstructionLoop(string LoopCondition)
        {
        }
        public void StartExecution()
        {
            //SetC interrupt flag!
            regs.FLAGS ^= 0x0200;
#if DOTRIES
            try
            {
                ExecutionLoop();
            }
            catch (Exception e)
            {
                //throw e;
                mSystem.HandleException(this, e);
            }
            finally
            {
                StoppedAt = DateTime.Now;
            }
#else
            ExecutionLoop();
#endif
        }
        private void ExecutionLoop()
        {

            StartedAt = DateTime.Now;
            while (!PowerOff)
            {
            //Lets go execute ROM BASIC! :-)
            //if (regs.CS.Value == 0x0000 & regs.IP == 0x7c00)
            //{
            //    regs.CS.Value = 0xD000; regs.IP = 0x0000;
            //}
            Top:
                if (PowerOff)
                    return;
                mLastEIP = regs.EIP;
                mLastESP = regs.ESP;
                if (PhysicalMem.mChangesPending > 0)
                    PhysicalMem.Commit(this, mem.mChangeAddress, mem.mChanges);
                mCurrInstructOpMode = OperatingMode;
                mIncrementedEIP = false;
                if (!HaltProcessor && !mExternalPauseActive)
                {
                    if (mRepeatCondition == NOT_REPEAT)
                    {
                        mCurrentInstructionAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, regs.EIP);
                        mCacheMisses++;
                        if (mProcessorStatus != eProcessorStatus.Resetting && mProcessorStatus != eProcessorStatus.ShuttingDown)
                            mProcessorStatus = eProcessorStatus.Decoding;
                        if (((regs.CR0 & 0x80000000) == 0x80000000 && mCurrentInstructionAddress < Processor_80x86.REGADDRBASE) && mem.PageAccessWillCausePF(this, ref sCurrentDecode, mCurrentInstructionAddress, false))
                        {
                            mRepeatCondition = NOT_REPEAT;
                            mBranchHint = 0;
                            mOpSize16 = true;
                            mAddrSize16 = true;
                            mOverrideStackSize = false;
                            mSegmentOverride = 0;
                            //ExceptionErrorCode |= 0x10;
                            Signals.LOCK = false;
                            ExceptionHandler(this, ref sCurrentDecode);
                            goto Top;
                        }
                        if (mLastInstructionAddress != mCurrentInstructionAddress)
                        {
                            mDecoder.Decode(ref sCurrentDecode);
                            mLastInstructionAddress = mCurrentInstructionAddress;
                        }
                        if (sCurrentDecode.ExceptionThrown)
                        {
                            regs.EIP = mLastEIP;
                            regs.ESP = mLastESP;
                            mRepeatCondition = NOT_REPEAT;
                            mBranchHint = 0;
                            mOpSize16 = true;
                            mAddrSize16 = true;
                            mOverrideStackSize = false;
                            mSegmentOverride = 0;
                            //ExceptionErrorCode |= 0x10;
                            Signals.LOCK = false;
                            ExceptionHandler(this, ref sCurrentDecode);
                            goto Top;
                        }
                        if (sCurrentDecode.Lock)
                            Signals.LOCK = true;
                        mInsructionsDecoded++;
                        //InstructionCache.Add(mCurrentInstruction);

                        if (mProcessorStatus != eProcessorStatus.Resetting && mProcessorStatus != eProcessorStatus.ShuttingDown)
                            mProcessorStatus = eProcessorStatus.Setup;

                        mOpSize16 = !sCurrentDecode.OpSizePrefixFound;
                        mAddrSize16 = !sCurrentDecode.AddrSizePrefixFound;
                        mCurrInstructOpSize16 = OpSize16;
                        mCurrInstructAddrSize16 = AddrSize16;
                        if (sCurrentDecode.OverrideSegment != eGeneralRegister.NONE)
                        {
                            switch (sCurrentDecode.OverrideSegment)
                            {
                                case eGeneralRegister.CS: mSegmentOverride = RCS; break;
                                case eGeneralRegister.DS: mSegmentOverride = RDS; break;
                                case eGeneralRegister.ES: mSegmentOverride = RES; break;
                                case eGeneralRegister.FS: mSegmentOverride = RFS; break;
                                case eGeneralRegister.GS: mSegmentOverride = RGS; break;
                                case eGeneralRegister.SS: mSegmentOverride = RSS; break;
                            }
                        }
                        if (sCurrentDecode.RepNZ && sCurrentDecode.mChosenInstruction.REPAble)
                            mRepeatCondition = REPEAT_TILL_NOT_ZERO;
                        else if (sCurrentDecode.RepZ && sCurrentDecode.mChosenInstruction.REPAble)
                            mRepeatCondition = REPEAT_TILL_ZERO;
                        else
                            mRepeatCondition = NOT_REPEAT;
                    }
                    sCurrentDecode.mChosenInstruction.SetupExecution(this, ref sCurrentDecode);
                    if (sCurrentDecode.ExceptionThrown)
                    {
                        regs.EIP = mLastEIP;
                        regs.ESP = mLastESP;
                        mRepeatCondition = NOT_REPEAT;
                        mBranchHint = 0;
                        mOpSize16 = true;
                        mAddrSize16 = true;
                        mOverrideStackSize = false;
                        Signals.LOCK = false;
                        mSegmentOverride = 0;
                        ExceptionHandler(this, ref sCurrentDecode);
                        goto Top;
                    }


                    if (mExportDebug)
                        OnParseCompleteEvent(new CustomEventArgs(this));
                    if (mProcessorStatus != eProcessorStatus.Resetting && mProcessorStatus != eProcessorStatus.ShuttingDown)
                        mProcessorStatus = eProcessorStatus.Execution;
                    if ((regs.CR0 & 0x80000000) == 0x80000000)
                        SaveRegisters();
                    ExecuteInstruction();
                    if (sCurrentDecode.ExceptionThrown)
                    {
                        regs.EIP = mLastEIP;
                        if (PhysicalMem.mChangesPending > 0)
                            mem.Rollback();
                        RestoreSavedRegisters();
                        mRepeatCondition = NOT_REPEAT;
                        mBranchHint = 0;
                        mOpSize16 = true;
                        mAddrSize16 = true;
                        mOverrideStackSize = false;
                        Signals.LOCK = false;
                        mSegmentOverride = 0;
                        ExceptionHandler(this, ref sCurrentDecode);
                        goto Top;
                    }
                    if (PhysicalMem.mChangesPending > 0)
                        PhysicalMem.Commit(this, mem.mChangeAddress, mem.mChanges);
                    //TimeStart = DateTime.Now;
                    //mMSinTimedSection += (DateTime.Now - TimeStart).TotalMilliseconds;

                    #region Trap Handling (debug single step)
                    if (TrapNextInst)
                    {
                        sCurrentDecode.ExceptionNumber = 1;
                        ExceptionHandler(this, ref sCurrentDecode);
                        TrapNextInst = false;
                        regs.FLAGSB.SetValues(regs.FLAGS);
                    }
                    if (regs.FLAGSB.TF)
                        TrapNextInst = true;
                    #endregion
                    mInsructionsExecuted++;

#if PERFTEST
                    if (TestInstructKillCount > 0)
                        if (mInsructionsExecuted > TestInstructKillCount)
                            return;
#endif
                    sCurrentDecode.mChosenInstruction.UsageCount++;
                    if (PhysicalMem.mChangesPending > 0)
                        PhysicalMem.Commit(this, mem.mChangeAddress, mem.mChanges);
#if !RELEASE
                    if (mExportDebug)
                        OnInstructCompleteEvent(new CustomEventArgs(this));
#endif
                    #region Proctected Mode Switch Processing
                    if ((!mProtectedModeActive) && (regs.CR0 & 1) == 1)
                    {
#if DEBUG
                        mSystem.ModeBreakpoint = true;
#endif
                        ZeroDescriptors();
                        mProtectedModeActive = true;
                        mCurrInstructOpMode = OperatingMode;
                        //                        InstructCache.Flush();
                        //regs.CS.mDescriptorNum = 0;
                    }
                    else if ((mProtectedModeActive) && (regs.CR0 & 1) != 1)
                    {
#if DEBUG
                        mSystem.ModeBreakpoint = true;
#endif
                        mProtectedModeActive = false;
                        mCurrInstructOpMode = OperatingMode;
                        SetSegRegsFromDescriptors();
                        //                        InstructCache.Flush();
                        // Do I really need these?    regs.mCSDescriptorNum = regs.mDSDescriptorNum = regs.mESDescriptorNum = regs.mFSDescriptorNum = regs.mGSDescriptorNum = regs.mSSDescriptorNum = 0;
                    }
                    #endregion
                }
                else
                {
                    //mInsructionsExecuted++;
                    //Thread.Sleep(1);
                    //Thread.Yield();
                    //NVM: We're in HALT which means the halt instruction was executed, so keep executing it to keep our instruction count correct
                    //regs.EIP -= sCurrentDecode.BytesUsed;
                    //HaltProcessor = false;

                }
                #region STI Handling
                if (mSTIAfterNextInstr == true)
                {
                    mSTIAfterNextInstr = false;
                    regs.setFlagIF(true);
                }
                if (mSTICalled)
                {
                    mSTICalled = false;
                    mSTIAfterNextInstr = true;
                }
                #endregion
                #region HOLD Pin Processing
                if (Signals.HOLD)
                {
                    if (mSystem.Debuggies.DebugCPU)
                        mSystem.PrintDebugMsg(eDebuggieNames.CPU, "HOLD raised, raising HLDA and going to sleep now");
                    Signals.HLDA = true;
                    while (1 == 1)
                    {
                        if (!Signals.HOLD || this.HaltProcessor)
                            break;

                    }
                    mSystem.PrintDebugMsg(eDebuggieNames.CPU, "HOLD dropped, lowering HLDA and going back to work");
                    Signals.HLDA = false;
                }
                #endregion
                #region INTR Pin Processing
                //If the INTR pin is asserted AND interrupts are enabled (IF=1), service IRQ
                //07/18/2013: Locking for obvious reasons
                lock (mSystem.DeviceBlock.mPIC)
                {
                    if (Signals.INTR)
                    {
                        if ((regs.FLAGS & 0x200) == 0x200)
                        {
                            HaltProcessor = false;
                            HandleNewIRQServiceRequest();
                        }
                    }
                }
                #endregion

                while (Signals.BUSY)
                    Thread.Yield();

#if DEBUG
                    #region TEMP
                    //if (this.mCurrentInstruction.Name.Contains("CMPS"))
                    //{
                    //    if (this.mCurrentInstruction.Name.EndsWith("B"))
                    //    {
                    //        mCurrentInstruction.Operand1SValue = mem.GetByte(this, PhysicalMem.GetLocForSegOfs(this, regs.DS.Value, regs.SI)).ToString("X2");
                    //        mCurrentInstruction.Operand2SValue = mem.GetByte(this, PhysicalMem.GetLocForSegOfs(this, regs.ES.Value, regs.DI)).ToString("X2");
                    //    }
                    //    else if (this.mCurrentInstruction.Name.EndsWith("W") && OpSize16)
                    //    {
                    //        mCurrentInstruction.Operand1SValue = mem.GetWord(this, PhysicalMem.GetLocForSegOfs(this, regs.DS.Value, regs.SI)).ToString("X4");
                    //        mCurrentInstruction.Operand2SValue = mem.GetWord(this, PhysicalMem.GetLocForSegOfs(this, regs.ES.Value, regs.DI)).ToString("X4");
                    //    }
                    //    else
                    //    {
                    //        mCurrentInstruction.Operand1SValue = mem.GetDWord(this, PhysicalMem.GetLocForSegOfs(this, regs.DS.Value, regs.SI)).ToString("X8");
                    //        mCurrentInstruction.Operand2SValue = mem.GetDWord(this, PhysicalMem.GetLocForSegOfs(this, regs.ES.Value, regs.DI)).ToString("X8");
                    //    }
                    //}
                    //else if (this.mCurrentInstruction.Name.Contains("SCAS"))
                    //{
                    //    if (this.mCurrentInstruction.Name.EndsWith("B"))
                    //    {
                    //        mCurrentInstruction.Operand1SValue = mem.GetByte(this, PhysicalMem.GetLocForSegOfs(this, regs.ES.Value, regs.DI)).ToString("X2");
                    //        mCurrentInstruction.Operand2SValue = regs.AL.ToString("X2");
                    //    }
                    //    else if (this.mCurrentInstruction.Name.EndsWith("W") && OpSize16)
                    //    {
                    //        mCurrentInstruction.Operand1SValue = mem.GetWord(this, PhysicalMem.GetLocForSegOfs(this, regs.ES.Value, regs.DI)).ToString("X4");
                    //        mCurrentInstruction.Operand2SValue = regs.AL.ToString("X4");
                    //    }
                    //    else
                    //    {
                    //        mCurrentInstruction.Operand1SValue = mem.GetDWord(this, PhysicalMem.GetLocForSegOfs(this, regs.ES.Value, regs.DI)).ToString("X8");
                    //        mCurrentInstruction.Operand2SValue = regs.AL.ToString("X8");
                    //    }
                    //}
                    #endregion
                    //OnWatchExecutionAddress(new CEAStartDebugging(this));
#if DEBUG
#if DEBUGGER_FEATURE
                    bool lFound = false;
                    if (mSystem.AddressBreakpointCount() > 0 && !mSingleStep)
                    {
                        foreach (cBreakpoint b in mSystem.BreakpointInfo.Where(u => u.InterruptNum == 0 && u.Enabled))
                            if (b.CS == regs.CS.Value && (b.EIP == regs.EIP || b.EIP == 0xFFFFFFFF) && !(b.CS==0 && b.EIP == 0))
                            {
                                if (b.DisableOnHit)
                                    b.Enabled = false;
                                else if (b.RemoveOnHit)
                                    mSystem.RemoveAddressBreakpoint(b);
                                mExportDebug = b.DebugToFile;
                                mSingleStep = b.DebugToScreen;
                                lFound = true;
                                break;
                            }
                    }
                        //Current instruction = INT and Interrupt = bp int, and Funct = 0 or FUnct = AH/EAX (based on DosFunct flag)
                    if ((!lFound) &&  (sCurrentDecode.mChosenInstruction.Name == "INT" && mSystem.BreakpointInfo.Where(u => u.InterruptNum == sCurrentDecode.Op1Value.OpByte 
                        && (u.FunctNum == 0 || (u.FunctNum == regs.EAX && u.DOSFunct == false) 
                                            || (u.FunctNum == regs.AH && u.DOSFunct == true)) && u.Enabled).LongCount() > 0))
                    {
                        cBreakpoint b = mSystem.BreakpointInfo.Where(u => u.InterruptNum == sCurrentDecode.Op1Value.OpByte && (u.FunctNum == 0 || (u.FunctNum == regs.EAX && u.DOSFunct == false)
                                            || (u.FunctNum == regs.AH && u.DOSFunct == true)) && u.Enabled).First();
                        mExportDebug = b.DebugToFile;
                        mSingleStep = b.DebugToScreen;
                        lFound = true;
                        mSingleStep = true;
                    }
                    else if ((!lFound) &&  mSystem.mSwitchToTaskBreakpoint)
                    {
                        lFound = true;
                        mSingleStep = mSystem.BreakpointInfo.Where(u => u.TaskNum == (regs.TR.SegSel & 0xFFFF) >> 3).First().DebugToScreen;
                        mSystem.mSwitchToTaskBreakpoint = false;
                        mExportDebug = mSystem.BreakpointInfo.Where(u => u.TaskNum == (regs.TR.SegSel & 0xFFFF) >> 3).First().DebugToFile;
                        mSystem.BreakpointInfo.Where(u => u.TaskNum == (regs.TR.SegSel & 0xFFFF) >> 3).First().Enabled = false;
                    }
                    else if ((!lFound) &&  mSystem.BreakpointInfo.Where(u => u.TaskName == GlobalRoutines.GetLinuxCurrentTaskName(mSystem)).Count() > 0)
                    {
                        cBreakpoint b = mSystem.BreakpointInfo.Where(u => u.TaskName == GlobalRoutines.GetLinuxCurrentTaskName(mSystem)).First();
                        if (b.Enabled)
                        {
                            mSingleStep = b.DebugToScreen;
                            mExportDebug = b.DebugToFile;
                            if (b.DisableOnHit)
                                b.Enabled = false;
                            lFound = true;
                        }
                    }
                    if (lFound)
                    {
                        mSystem.DeviceBlock.mPIT.mPICTimers[0].Stop();
                        mSystem.mSoftIntBreakpoint = false;
                        CEAStartDebugging d = new CEAStartDebugging(this);
                        if (mExportDebug)
                            d.mProcessorFoundDebuggingStart = true;
                        OnStartDebugging(d);
                    }

#endif
#endif
                    if (mSystem.mModeBreakpoint || mSystem.mCPL0SwitchBreakpoint || mSystem.mCPL3SwitchBreakpoint || mSystem.mTaskSwitchBreakpoint || mSystem.mPagingExceptionBreakpoint)
                    {
                        mSystem.mModeBreakpoint = false;
                        mSystem.mCPL0SwitchBreakpoint = false;
                        mSystem.mCPL3SwitchBreakpoint = false;
                        mSystem.mTaskSwitchBreakpoint = false;
                        mSystem.mPagingExceptionBreakpoint = false;
                        mSingleStep = true;
                        CEAStartDebugging d = new CEAStartDebugging(this);
                        OnStartDebugging(d);
                    }
#endif
                if (mSingleStep)
                {
                    mSystem.DeviceBlock.mPIT.mPICTimers[0].Stop();
                    OnSingleStepEvent(new CustomEventArgs(this));
                    mSystem.DeviceBlock.mPIT.mPICTimers[0].Interval += mTimerTickMSec;
                    mSystem.DeviceBlock.mPIT.mPICTimers[0].Start();
                }
            }
            StoppedAt = DateTime.Now;
            if (mProcessorStatus != eProcessorStatus.Resetting && mProcessorStatus != eProcessorStatus.ShuttingDown)
                mProcessorStatus = eProcessorStatus.Resetting;
            mSystem.mProc.mFPU.ShutDown();
        }

        public void ExecuteInstruction()
        {
            regs.EIP += sCurrentDecode.BytesUsed;
            mIncrementedEIP = true;
            if (mCalculateInstructionTimings)
                sCurrentDecode.mChosenInstruction.mLastStart = DateTime.UtcNow;
            mFlagsBefore = regs.EFLAGS;

            //mSystem.PrintDebugMsg(eDebuggieNames.CPU, "Executing: " + regs.CS.Value.ToString("X8") + ":" + regs.EIP.ToString("X8"));

            //Send FPU instructions to the FPU, otherwise execute them
            //if (mCurrentInstruction.FPUInstruction)
            //{
            //    mFPU.QueueInstruct(mCurrentInstruction);
            //    FPU.mNewCalc.Set();
            //    //CLR 07/04/2013 - No need to delay, in fact it breaks the FPU
            //    //Thread.Sleep(20);  
            //}
            //else
            sCurrentDecode.mChosenInstruction.Impl(ref sCurrentDecode);
            if (sCurrentDecode.ExceptionThrown)
                return;
            if (mFlagsBefore != regs.EFLAGS)
                regs.FLAGSB.FlagsDirty = true;
            if (mCalculateInstructionTimings)
                sCurrentDecode.mChosenInstruction.TotalTimeInInstruct += (DateTime.UtcNow - sCurrentDecode.mChosenInstruction.mLastStart).TotalMilliseconds;

            if (!sCurrentDecode.mChosenInstruction.REPAble)
                mRepeatCondition = NOT_REPEAT;

            if (mRepeatCondition != NOT_REPEAT)
            {
                //Need to fix this for !OpSize16
                if (mCurrInstructOpSize16 && regs.CX == 0 || ((!mCurrInstructOpSize16) && regs.ECX == 0))
                    mRepeatCondition = NOT_REPEAT;
                if ((this.sCurrentDecode.mChosenInstruction.Name.Contains("SCAS") || sCurrentDecode.mChosenInstruction.Name.Contains("CMPS")))
                    if (mRepeatCondition == REPEAT_TILL_NOT_ZERO && regs.FLAGSB.ZF == true)
                        mRepeatCondition = NOT_REPEAT;
                    else if (mRepeatCondition == REPEAT_TILL_ZERO && regs.FLAGSB.ZF == false)
                        mRepeatCondition = NOT_REPEAT;
                if (mRepeatCondition != NOT_REPEAT && !sCurrentDecode.ExceptionThrown)
                {
                    regs.EIP -= sCurrentDecode.BytesUsed;
                }
            }


            if (sCurrentDecode.ExceptionThrown)
                ExceptionHandler(this, ref sCurrentDecode);

            if (mRepeatCondition == NOT_REPEAT)
            {
                mBranchHint = 0;
                mOpSize16 = true;
                mAddrSize16 = true;
                mOverrideStackSize = false;
                mSegmentOverride = 0;
                Signals.LOCK = false;
            }
        }
        public void ZeroDescriptors()
        {
            regs.CS.ResetOnEnterPM();
            regs.DS.ResetOnEnterPM();
            regs.ES.ResetOnEnterPM();
            regs.FS.ResetOnEnterPM();
            regs.GS.ResetOnEnterPM();
            regs.SS.ResetOnEnterPM();
        }

        public void SetSegRegsFromDescriptors()
        {
            if (regs.CS.mDescriptorNum > 0)
                regs.CS.Value = mGDTCache[(int)regs.CS.mDescriptorNum].Base >> 4;
            if (regs.DS.mDescriptorNum > 0)
                regs.DS.Value = mGDTCache[(int)regs.DS.mDescriptorNum].Base >> 4;
            if (regs.ES.mDescriptorNum > 0)
                regs.ES.Value = mGDTCache[(int)regs.ES.mDescriptorNum].Base >> 4;
            if (regs.FS.mDescriptorNum > 0)
                regs.FS.Value = mGDTCache[(int)regs.FS.mDescriptorNum].Base >> 4;
            if (regs.GS.mDescriptorNum > 0)
                regs.GS.Value = mGDTCache[(int)regs.GS.mDescriptorNum].Base >> 4;
            if (regs.SS.mDescriptorNum > 0)
                regs.SS.Value = mGDTCache[(int)regs.SS.mDescriptorNum].Base >> 4;
        }
        public bool NeedToInterruptLoop()
        {
            if (sCurrentDecode.ExceptionThrown || Signals.HOLD || (Signals.INTR && (regs.FLAGS & 0x200) == 0x200) )
            {
                if (PhysicalMem.mChangesPending > 0)
                    PhysicalMem.Commit(this, mem.mChangeAddress, mem.mChanges);
                return true;
            }
            mInsructionsExecuted++;
            //if (Signals.INTR && (regs.FLAGS & 0x200) == 0x200)
            //    return true;

            
            return false;
        }
        public void PrepareForJump(ref sInstruction CurrentDecode)
        {
            Instruct lPush = Instructions[0x6];
            sInstruction ins = CurrentDecode;
            ins.Op1Add = 0x0;

            lPush.mProc = this;
            ins.Operand1IsRef = false;
            ins.Operand2IsRef = false;
            ins.Op1Value.OpDWord = regs.EFLAGS;
            //lPush.SetupExecution();
            lPush.Impl(ref ins);
            if (ins.ExceptionThrown)
            {
                sCurrentDecode = ins;
                CurrentDecode = ins;
                return;
            }
            ins.Op1Value.OpDWord = regs.CS.Value;
            //lPush.SetupExecution();
            lPush.Impl(ref ins);
            if (ins.ExceptionThrown)
            {
                sCurrentDecode = ins;
                CurrentDecode = ins;
                return;
            }
            ins.Op1Value.OpDWord = regs.EIP;
            //lPush.SetupExecution();
            lPush.Impl(ref ins);
            if (ins.ExceptionThrown)
            {
                sCurrentDecode = ins;
                CurrentDecode = ins;
                return;
            }
            if (PhysicalMem.mChangesPending > 0)
                PhysicalMem.Commit(this, mem.mChangeAddress, mem.mChanges);
        }
        public void HandleNewIRQServiceRequest()
        {
            Word lOldCS = (Word)regs.CS.Value;
            DWord lOldEIP = regs.EIP;
            
            mBranchHint = 0;
            mOpSize16 = true;
            mAddrSize16 = true;
            mOverrideStackSize = false;
            mSegmentOverride = 0;

            if (mCurrInstructOpMode == ProcessorMode.Real)
            {
                PrepareForJump(ref sCurrentDecode);

                mLastIRQVector = mSystem.DeviceBlock.mPIC.PassIRQVectorToCPU();
                if (mLastIRQVector == 0x0)
                    return;

                regs.CS.Value = mem.GetWord(this, ref sCurrentDecode, (UInt16)(mLastIRQVector * 4 + 2));
                regs.IP = mem.GetWord(this, ref sCurrentDecode, (UInt16)(mLastIRQVector * 4));

                //Clear the repeat conditon, tis necessary so that the new instruction is executed
                //the loop condition will be re-evaluated when the IRQ handler finishes and POPs
                mRepeatCondition = NOT_REPEAT;
                if (mSystem.Debuggies.DebugPIC)
                {
                    StringBuilder lPrint = new StringBuilder('0');
                    lPrint.AppendFormat("Servicing IRQ, Int # {0}.  Transferring control from {1}:{2} to {3}:{4}",
                        mLastIRQVector.ToString("X4"),
                        lOldCS.ToString("X4"),
                        lOldEIP.ToString("X4"),
                        regs.CS.Value.ToString("X4"),
                        regs.IP.ToString("X4"));
                    mSystem.PrintDebugMsg(eDebuggieNames.CPU, lPrint.ToString());
                }

#if DEBUG
                if (mExportDebug)
                    OnServiceInterruptStart(new CustomEventArgs(this));
#endif
                //Before we leave, clear the INTR pin and set the appropriate flags
                //Signals.INTR = false;
                regs.setFlagIF(false);
                regs.setFlagTF(false);
                regs.setFlagAC(false);
            }
            else
            {
                byte lVector = mSystem.DeviceBlock.mPIC.PassIRQVectorToCPU();
                mLastIRQVector = lVector;
                if (mLastIRQVector == 0x0)
                    return;

                mRepeatCondition = NOT_REPEAT;
                if (mSystem.Debuggies.DebugPIC)
                {
                    StringBuilder lPrint = new StringBuilder('0');
                    lPrint.AppendFormat("Servicing PMode IRQ, Int # {0}.  Transferring control from {1}:{2}",
                        lVector.ToString("X4"),
                        lOldCS.ToString("X8"),
                        lOldEIP.ToString("X8"));
                    mSystem.PrintDebugMsg(eDebuggieNames.CPU, lPrint.ToString());
                }

                if (mExportDebug)
                    OnServiceInterruptStart(new CustomEventArgs(this));

                //Before we leave, clear the INTR pin and set the appropriate flags
                //Signals.INTR = false;
               //regs.setFlagIF(false);
                regs.setFlagTF(false);
                regs.setFlagAC(false);

                Instruct lINT = Instructions[0x00CD];
                ServicingIRQ = true;
                sCurrentDecode.Op1Value.OpQWord = lVector;
                lINT.mIntIsSoftware = false;
                lINT.Impl(ref sCurrentDecode);
                ServicingIRQ = false;
            }
        }
        internal void ExceptionHandler(Processor_80x86 mProc, ref sInstruction sIns)
        {
#if DEBUG
            if (mExportDebug)
                OnServiceInterruptStart(new CustomEventArgs(this));
#endif
            UInt32 ExceptionHappenedAtAddress;
            sInstruction ins = sIns;

            //if (sIns.ExceptionNumber == 0)
            //    System.Diagnostics.Debugger.Break();

            if (mCurrInstructOpMode == ProcessorMode.Real)
            {
                switch (sIns.ExceptionNumber)
                {
                    case 1:
                        ExceptionHappenedAtAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, regs.IP);
                        sIns.Op1Value.OpQWord = 0x01;
                        Instructions[0x00CD].mIntIsSoftware = false;
                        Instructions[0x00CD].Impl(ref sIns);
                        mExceptionTransferAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, regs.IP);
                        break;
                    case 6:
                        ExceptionHappenedAtAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, regs.IP);
                        //?    
                        //regs.EIP += mCurrentOperation.Length;
                        if (mem.GetDWord(mProc, ref sIns, 0x06 * 4) == 0 || mem.GetDWord(mProc, ref sIns, 0x06 * 4) == 0xf000ff53)
                            throw new Exception("Invalid instruction, no INT 06 handler defined");
                        Instruct lINT = Instructions[0x00CD];
                        sIns.Op1Value.OpQWord = 0x06;
                        lINT.mIntIsSoftware = false;
                        lINT.Impl(ref sIns);
                        //mCurrentInstruction = mCurrentInstruction = mDecoder.Decode(this);
                        mExceptionTransferAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, regs.IP);
                        break;
                }
            }
            else
            {
                //07/23/2013 - FreeDOS does NOT like this, so I'm going to try to do without it
                //if (ExceptionNumber == 0xD)
                //    regs.EIP -= sCurrentDecode.BytesUsed;

                ExceptionHappenedAtAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, regs.EIP);
                if (mSystem.Debuggies.DebugExceptions || (mSystem.Debuggies.DebugCPU  && sIns.ExceptionNumber != 14))
                {
                    StringBuilder lPrint = new StringBuilder('0');
                    lPrint.AppendFormat("PMode EXCEPTION # {0}.  Executed instruction was: + " + sIns.mChosenInstruction.mName + ", transferring control from {1}",
                        sIns.ExceptionNumber,
                        ExceptionHappenedAtAddress.ToString("X8"));
                    mSystem.PrintDebugMsg(eDebuggieNames.Exceptions, lPrint.ToString());
                }
                if (sIns.ExceptionNumber == 0xE)
                    regs.CR2 = sIns.ExceptionAddress;

                Instruct lINT = Instructions[0x00CD];
                ins.Op1Value.OpQWord = sIns.ExceptionNumber;
                lINT.mIntIsSoftware = false;
                mExceptionWhileExcepting = false;
                lINT.Impl(ref ins);
                if (mExceptionWhileExcepting)
                {
                    mSystem.PrintDebugMsg(eDebuggieNames.Exceptions, "Double exception detected (" + ins.ExceptionNumber + "), trying one more time.");
                    ins.Op1Value.OpQWord = ins.ExceptionNumber;
                    lINT.mIntIsSoftware = false;
                    lINT.Impl(ref sIns);
                    if (mExceptionWhileExcepting)
                    {
                        throw new Exception("Triple exception detected!  Final exception was: " + ins.ExceptionNumber.ToString("X"));
                    }
                }
                mExceptionTransferAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, regs.EIP);
                sIns.ExceptionThrown = false;
            }
            sIns.ExceptionNumber = Global.CPU_NO_EXCEPTION;
            sIns.ExceptionThrown = false;
            sIns.ExceptionErrorCode = 0;
            sIns.ExceptionAddress = 0;

            if (PhysicalMem.mChangesPending > 0)
                PhysicalMem.Commit(this, mem.mChangeAddress, mem.mChanges);
        }

        private void SaveRegisters()
        {
            savedRegs.EAX = regs.EAX;
            savedRegs.EBX = regs.EBX;
            savedRegs.ECX = regs.ECX;
            savedRegs.EDX = regs.EDX;
            savedRegs.EBP = regs.EBP;
            savedRegs.ESP = regs.ESP;
            savedRegs.EDI = regs.EDI;
            savedRegs.ESI = regs.ESI;
            savedRegs.ESP = regs.ESP;
            savedRegs.EFLAGS = regs.EFLAGS;

        }

        private void RestoreSavedRegisters()
        {
            regs.EAX = savedRegs.EAX;
            regs.EBX = savedRegs.EBX;
            regs.ECX = savedRegs.ECX;
            regs.EDX = savedRegs.EDX;
            regs.EBP = savedRegs.EBP;
            regs.ESP = savedRegs.ESP;
            regs.EFLAGS = savedRegs.EFLAGS;
            regs.FLAGSB.FlagsDirty = true;
            regs.EDI = savedRegs.EDI;
            regs.ESI = savedRegs.ESI;
            //CLR 08/04/2013 - Not sure if we need these so I removed them as they slow things down
            //regs.CS = savedRegs.CS;
            //regs.DS = savedRegs.DS;
            //regs.SS = savedRegs.SS;
            //regs.ES = savedRegs.ES;
            //regs.FS = savedRegs.FS;
            //regs.GS = savedRegs.GS;
        }

        private void ValidateLock()
        {
            //if (Signals.LOCK)
            //{
            //    bool lFound = false;
            //    foreach (String s in new String[18] { "ADD", " ADC", " AND", " BTC", " BTR", " BTS", " CMPXCHG", " CMPXCH8B", " DEC", " INC", " NEG", " NOT", " OR", " SBB", " SUB", " XOR", " XADD", " XCHG" })
            //        if (mCurrentInstruction.Name == s && !mCurrentOperation.Op1Val.mIsRegister)
            //        {
            //            lFound = true;
            //            break;
            //        }
            //    if (!lFound)
            //    {
            //        ExceptionHandler(0x06);
            //        OnInvalidOpCode(new CustomEventArgs(this));
            //    }
            //    Signals.LOCK = false;
            //}
        }

        public void RefreshGDTCache()
        {
            DateTime lStart = DateTime.UtcNow;

            mGDTCacheResetsExecuted++;
            if (PhysicalMem.mChangesPending > 0)
                PhysicalMem.Commit(this,mem.mChangeAddress, mem.mChanges);
            regs.GDTR.lCache = mem.ChunkPhysical(this, 0, regs.GDTR.Base, regs.GDTR.Limit + 1);
            //UInt64[] lCache = mem.QWChunkPhysical(regs.GDTR.lCache, (UInt16)((regs.GDTR.Limit + 1) / 8));
            mGDTCache = new cGDTCache(PhysicalMem.QWChunkPhysical(regs.GDTR.lCache, (UInt16)((regs.GDTR.Limit + 1) / 8)), (UInt16)regs.GDTR.Limit);
			//mTLB.Flush(this);
            mTimeInGDTRefresh += (DateTime.UtcNow - lStart).TotalMilliseconds;
        }

        public void RefreshLDTCache()
        {
            if (PhysicalMem.mChangesPending > 0)
                PhysicalMem.Commit(this, mem.mChangeAddress, mem.mChanges);
            regs.LDTR.lCache = mem.ChunkPhysical(this, 0, regs.LDTR.Base, regs.LDTR.Limit + 1);
            //UInt64[] lCache = mem.QWChunkPhysical(regs.LDTR.lCache, (UInt16)((regs.LDTR.Limit + 1) / 8));
            mLDTCache = new cGDTCache(PhysicalMem.QWChunkPhysical(regs.LDTR.lCache, (UInt16)((regs.LDTR.Limit + 1) / 8)), (UInt16)regs.LDTR.Limit);
			//mTLB.Flush(this);
        }

        public void RefreshIDTCache()
        {
            if (PhysicalMem.mChangesPending > 0)
                PhysicalMem.Commit(this, mem.mChangeAddress, mem.mChanges);
            regs.IDTR.lCache = mem.ChunkPhysical(this, 0, regs.IDTR.Base, regs.IDTR.Limit + 1);
            //UInt64[] lCache = mem.QWChunkPhysical(regs.IDTR.lCache, (UInt16)((regs.IDTR.Limit + 1) / 8));
            mIDTCache = new cIDTCache(PhysicalMem.QWChunkPhysical(regs.IDTR.lCache, (UInt16)((regs.IDTR.Limit + 1) / 8)), mGDTCache);
            //int a = 0;
            //if (regs.CS.Value != 0xf000)
            //    a = 1;
        }

        public void InitInstructions(InstructionList<Instruct> Instructions)
        {
            Instructions.Add(new MOV(), this);
            Instructions.Add(new MOVZX(), this);
            Instructions.Add(new CALL(), this);
            Instructions.Add(new ADD(), this);
            Instructions.Add(new PUSH(), this);
            Instructions.Add(new INC(), this);
            Instructions.Add(new POP(), this);
            Instructions.Add(new JMP(), this);
            Instructions.Add(new CMPXCHG(), this);
            Instructions.Add(new CMP(), this);
            Instructions.Add(new AND(), this);
            Instructions.Add(new JA(), this);
            Instructions.Add(new JB(), this);
            Instructions.Add(new BSF(), this);
            Instructions.Add(new BSR(), this);
            Instructions.Add(new BSWAP(), this);
            Instructions.Add(new BT(), this);
            Instructions.Add(new BTC(), this);
            Instructions.Add(new BTR(), this);
            Instructions.Add(new BTS(), this);
            Instructions.Add(new CALLF(), this);
            Instructions.Add(new CBW(), this);
            Instructions.Add(new CLC(), this);
            Instructions.Add(new CLD(), this);
            Instructions.Add(new CLI(), this);
            Instructions.Add(new CLTS(), this);
            Instructions.Add(new CMC(), this);
            Instructions.Add(new CMOVcc(), this);
            Instructions.Add(new CMPSB(), this);
            Instructions.Add(new CMPSD(), this);
            Instructions.Add(new CMPSW(), this);
            Instructions.Add(new CPUID(), this);
            Instructions.Add(new CWD(), this);
            Instructions.Add(new DAA(), this);
            Instructions.Add(new DAS(), this);
            Instructions.Add(new DEC(), this);
            Instructions.Add(new DIV(), this);
            Instructions.Add(new ENTER(), this);
            Instructions.Add(new FABS(), this);
            Instructions.Add(new FADD(), this);
            Instructions.Add(new FCOM(), this);
            Instructions.Add(new FCHS(), this);
            Instructions.Add(new FCLEX(), this);
            Instructions.Add(new FCMOV(), this);
            Instructions.Add(new FDIV(), this);
            Instructions.Add(new FDIVR(), this);
            Instructions.Add(new FINIT(), this);
            Instructions.Add(new FILD(), this);
            Instructions.Add(new FIST(), this);
            Instructions.Add(new FLDCW(), this);
            Instructions.Add(new FLD(), this);
            Instructions.Add(new FLD1(), this);
            Instructions.Add(new FLDZ(), this);
            Instructions.Add(new FIMUL(), this);
            Instructions.Add(new FRNDINT(), this);
            Instructions.Add(new FMUL(), this);
            Instructions.Add(new FSAVE(), this);
            Instructions.Add(new FSTCW(), this);
            Instructions.Add(new FSTENV(), this);
            Instructions.Add(new FSUB(), this);
            Instructions.Add(new FUCOM(), this);
            Instructions.Add(new FST(), this);
            Instructions.Add(new FSTSW(), this);
            Instructions.Add(new HLT(), this);
            Instructions.Add(new ICEBP(), this);
            Instructions.Add(new IDIV(), this);
            Instructions.Add(new IMUL(), this);
            Instructions.Add(new IN(), this);
            Instructions.Add(new INVLPG(), this);
            Instructions.Add(new INS(), this);
            Instructions.Add(new INT(), this);
            Instructions.Add(new INTO(), this);
            Instructions.Add(new IRET(), this);
            Instructions.Add(new JAE(), this);
            Instructions.Add(new JBE(), this);
            Instructions.Add(new JC(), this);
            Instructions.Add(new JCXZ(), this);
            Instructions.Add(new JE(), this);
            Instructions.Add(new JG(), this);
            Instructions.Add(new JGE(), this);
            Instructions.Add(new JL(), this);
            Instructions.Add(new JLE(), this);
            Instructions.Add(new JMPF(), this);
            Instructions.Add(new JNA(), this);
            Instructions.Add(new JNAE(), this);
            Instructions.Add(new JNB(), this);
            Instructions.Add(new JNBE(), this);
            Instructions.Add(new JNC(), this);
            Instructions.Add(new JNE(), this);
            Instructions.Add(new JNG(), this);
            Instructions.Add(new JNGE(), this);
            Instructions.Add(new JNLE(), this);
            Instructions.Add(new JNO(), this);
            Instructions.Add(new JNP(), this);
            Instructions.Add(new JNS(), this);
            Instructions.Add(new JNZ(), this);
            Instructions.Add(new JO(), this);
            Instructions.Add(new JP(), this);
            Instructions.Add(new JPE(), this);
            Instructions.Add(new JPO(), this);
            Instructions.Add(new JS(), this);
            Instructions.Add(new JZ(), this);
            Instructions.Add(new LAHF(), this);
            Instructions.Add(new LAR(), this);
            Instructions.Add(new LDS(), this);
            Instructions.Add(new LEA(), this);
            Instructions.Add(new LEAVE(), this);
            Instructions.Add(new LES(), this);
            Instructions.Add(new LFS(), this);
            Instructions.Add(new LGS(), this);
            Instructions.Add(new LIDT(), this);
            Instructions.Add(new LLDT(), this);
            Instructions.Add(new LGDT(), this);
            Instructions.Add(new LMSW(), this);
            Instructions.Add(new LTR(), this);
            /*Instructions.Add(new LOCK(),this);*/
            Instructions.Add(new LODSB(), this);
            Instructions.Add(new LODSD(), this);
            Instructions.Add(new LODSW(), this);
            Instructions.Add(new LOOP(), this);
            Instructions.Add(new LOOPE(), this);
            Instructions.Add(new LOOPNE(), this);
            Instructions.Add(new LOOPNZ(), this);
            Instructions.Add(new LOOPZ(), this);
            Instructions.Add(new LSS(), this);
            Instructions.Add(new MOVSX(), this);
            Instructions.Add(new MOVSB(), this);
            Instructions.Add(new MOVSD(), this);
            Instructions.Add(new MOVSW(), this);
            Instructions.Add(new MUL(), this);
            Instructions.Add(new NEG(), this);
            Instructions.Add(new NOP(), this);
            Instructions.Add(new NOT(), this);
            Instructions.Add(new OR(), this);
            Instructions.Add(new OUT(), this);
            Instructions.Add(new OUTS(), this);
            Instructions.Add(new POPA(), this);
            Instructions.Add(new POPF(), this);
            Instructions.Add(new PUSHA(), this);
            Instructions.Add(new PUSHAD(), this);
            Instructions.Add(new PUSHF(), this);
            Instructions.Add(new RCL(), this);
            Instructions.Add(new RCR(), this);
            Instructions.Add(new REP(), this);
            Instructions.Add(new REPZ(), this);
            Instructions.Add(new REPE(), this);
            Instructions.Add(new REPNE(), this);
            Instructions.Add(new REPNZ(), this);
            Instructions.Add(new RET(), this);
            Instructions.Add(new RETF(), this);
            Instructions.Add(new RDTSC(), this);
            Instructions.Add(new ROL(), this);
            Instructions.Add(new ROR(), this);
            Instructions.Add(new SAHF(), this);
            Instructions.Add(new SBB(), this);
            Instructions.Add(new SCASB(), this);
            Instructions.Add(new SCASD(), this);
            Instructions.Add(new SCASW(), this);
            Instructions.Add(new SAL(), this);
            Instructions.Add(new SALC(), this);
            Instructions.Add(new SAR(), this);
            Instructions.Add(new SETcc(), this);
            Instructions.Add(new SGDT(), this);
            Instructions.Add(new SLDT(), this);
            Instructions.Add(new SHL(), this);
            Instructions.Add(new SIDT(), this);
            Instructions.Add(new SHLD(), this);
            Instructions.Add(new SMSW(), this);
            Instructions.Add(new SHR(), this);
            Instructions.Add(new SHRD(), this);
            Instructions.Add(new STC(), this);
            Instructions.Add(new STD(), this);
            Instructions.Add(new STI(), this);
            Instructions.Add(new STR(), this);
            Instructions.Add(new STOSB(), this);
            Instructions.Add(new STOSD(), this);
            Instructions.Add(new STOSW(), this);
            Instructions.Add(new SUB(), this);
            Instructions.Add(new TEST(), this);
            Instructions.Add(new FWAIT(), this);
            Instructions.Add(new FXCH(), this);
            Instructions.Add(new VERR(), this);
            Instructions.Add(new VERW(), this);
            Instructions.Add(new XADD(), this);
            Instructions.Add(new XCHG(), this);
            Instructions.Add(new XLAT(), this);
            Instructions.Add(new XLATB(), this);
            Instructions.Add(new XOR(), this);
            Instructions.Add(new GRP1(), this);
            Instructions.Add(new GRP2(), this);
            Instructions.Add(new GRP3a(), this);
            Instructions.Add(new GRP3b(), this);
            Instructions.Add(new GRP4(), this);
            Instructions.Add(new GRP5(), this);
            Instructions.Add(new GRP60(), this);
            Instructions.Add(new GRP61(), this);
            Instructions.Add(new GRP7(), this);
            Instructions.Add(new GRP8(), this);
            Instructions.Add(new GRPC0(), this);
            Instructions.Add(new GRPC1(), this);
            Instructions.Add(new GRPPUSH(), this);
            Instructions.Add(new AAA(), this);
            Instructions.Add(new AAD(), this);
            Instructions.Add(new AAM(), this);
            Instructions.Add(new AAS(), this);
            Instructions.Add(new ADC(), this);
            Instructions.Add(new ARPL(), this);
            Instructions.Add(new BOUND(), this);

        MOV = new MOV();
        ADD = new ADD();
        PUSH = new PUSH();
        POP = new POP();
        JMP = new JMP();
        CMPSD = new CMPSD();
        FSTENV = new FSTENV();
        FWAIT = new FWAIT();
        INT = new INT();
        LODSD = new LODSD();
        LOOPE = new LOOPE();
        LOOPNE = new LOOPNE();
        MOVSD = new MOVSD();
        POPF = new POPF();
        REPZ = new REPZ();
        REPNE = new REPNE();
        SCASD = new SCASD();
        SHL = new SHL();
        STOSD = new STOSD();
        MOV.mProc = this;
        ADD.mProc = this;
        PUSH.mProc = this;
        POP.mProc = this;
        JMP.mProc = this;
        CMPSD.mProc = this;
        FSTENV.mProc = this;
        FWAIT.mProc = this;
        INT.mProc = this;
        LODSD.mProc = this;
        LOOPE.mProc = this;
        LOOPNE.mProc = this;
        MOVSD.mProc = this;
        POPF.mProc = this;
        REPZ.mProc = this;
        REPNE.mProc = this;
        SCASD.mProc = this;
        SHL.mProc = this;
        STOSD.mProc = this;
        }
        internal static UInt32 GetRegAddrForRegEnum(eGeneralRegister Register)
        {
            switch (Register)
            {
                case eGeneralRegister.AX: return RAX;
                case eGeneralRegister.DX: return RDX;
                case eGeneralRegister.BX: return RBX;
                case eGeneralRegister.SP: return RSP;
                case eGeneralRegister.BP: return RBP;
                case eGeneralRegister.ECX: return RECX;
                case eGeneralRegister.EAX: return REAX;
                case eGeneralRegister.EBX: return REBX;
                case eGeneralRegister.AL: return RAL;
                case eGeneralRegister.CL: return RCLr;
                case eGeneralRegister.DL: return RDL;
                case eGeneralRegister.BL: return RBL;
                case eGeneralRegister.AH: return RAH;
                case eGeneralRegister.CH: return RCH;
                case eGeneralRegister.DH: return RDH;
                case eGeneralRegister.BH: return RBH;
                case eGeneralRegister.CX: return RCX;
                case eGeneralRegister.SI: return RSI;
                case eGeneralRegister.DI: return RDI;
                case eGeneralRegister.EDX: return REDX;
                case eGeneralRegister.ESP: return RESP;
                case eGeneralRegister.EBP: return REBP;
                case eGeneralRegister.ESI: return RESI;
                case eGeneralRegister.EDI: return REDI;
                case eGeneralRegister.CS: return RCS;
                case eGeneralRegister.DS: return RDS;
                case eGeneralRegister.ES: return RES;
                case eGeneralRegister.FS: return RFS;
                case eGeneralRegister.GS: return RGS;
                case eGeneralRegister.SS: return RSS;
                case eGeneralRegister.CR0: return RCR0;
                case eGeneralRegister.CR2: return RCR2;
                case eGeneralRegister.CR3: return RCR3;
                case eGeneralRegister.CR4: return RCR4;
                case eGeneralRegister.DR0: return RDR0;
                case eGeneralRegister.DR1: return RDR1;
                case eGeneralRegister.DR2: return RDR2;
                case eGeneralRegister.DR3: return RDR3;
                case eGeneralRegister.DR6: return RDR6;
                case eGeneralRegister.DR7: return RDR7;
            }
            throw new Exception("GetRegAddrForRegEnum - Register address not found for: " + Register);
        }

        internal byte GetByteRegValueForRegEnum(eGeneralRegister Register)
        {
            switch (Register)
            {
                case eGeneralRegister.AL: return regs.AL;
                case eGeneralRegister.CL: return regs.CL;
                case eGeneralRegister.DL: return regs.DL;
                case eGeneralRegister.BL: return regs.BL;
                case eGeneralRegister.AH: return regs.AH;
                case eGeneralRegister.CH: return regs.CH;
                case eGeneralRegister.DH: return regs.DH;
                case eGeneralRegister.BH: return regs.BH;
            }
            throw new Exception("GetByteRegValueForRegEnum - Register address not found for: " + Register);
        }

        internal Word GetWordRegValueForRegEnum(eGeneralRegister Register)
        {
            switch (Register)
            {
                case eGeneralRegister.AX: return regs.AX;
                case eGeneralRegister.CX: return regs.CX;
                case eGeneralRegister.DX: return regs.DX;
                case eGeneralRegister.BX: return regs.BX;
                case eGeneralRegister.SP: return regs.SP;
                case eGeneralRegister.BP: return regs.BP;
                case eGeneralRegister.SI: return regs.SI;
                case eGeneralRegister.DI: return regs.DI;
                case eGeneralRegister.ST0:
                case eGeneralRegister.ST1:
                case eGeneralRegister.ST2:
                case eGeneralRegister.ST3:
                case eGeneralRegister.ST4:
                case eGeneralRegister.ST5:
                case eGeneralRegister.ST6:
                case eGeneralRegister.ST7:
                    return 0;
            }
            throw new Exception("GetWordRegValueForRegEnum - Register address not found for: " + Register);
        }

        internal DWord GetDWordRegValueForRegEnum(eGeneralRegister Register)
        {
            switch (Register)
            {
                case eGeneralRegister.DS: return regs.DS.Value;
                case eGeneralRegister.SS: return regs.SS.Value;
                case eGeneralRegister.EAX: return regs.EAX;
                case eGeneralRegister.ECX: return regs.ECX;
                case eGeneralRegister.EDX: return regs.EDX;
                case eGeneralRegister.EBX: return regs.EBX;
                case eGeneralRegister.ESP: return regs.ESP;
                case eGeneralRegister.EBP: return regs.EBP;
                case eGeneralRegister.ESI: return regs.ESI;
                case eGeneralRegister.EDI: return regs.EDI;
                case eGeneralRegister.CS: return regs.CS.Value;
                case eGeneralRegister.ES: return regs.ES.Value;
                case eGeneralRegister.FS: return regs.FS.Value;
                case eGeneralRegister.GS: return regs.GS.Value;
                case eGeneralRegister.CR0: return regs.CR0;
                case eGeneralRegister.CR2: return regs.CR2;
                case eGeneralRegister.CR3: return regs.CR3;
                case eGeneralRegister.CR4: return regs.CR4;
                case eGeneralRegister.DR0: return regs.DR0;
                case eGeneralRegister.DR1: return regs.DR1;
                case eGeneralRegister.DR2: return regs.DR2;
                case eGeneralRegister.DR3: return regs.DR3;
                case eGeneralRegister.DR6: return regs.DR6;
                case eGeneralRegister.DR7: return regs.DR7;
            }
            throw new Exception("GetDWordRegValueForRegEnum - Register address not found for: " + Register);
        }

        internal UInt32 GetRegValueForRegEnum(eGeneralRegister Register)
        {
            switch (Register)
            {
                case eGeneralRegister.AL: return regs.AL;
                case eGeneralRegister.CL: return regs.CL;
                case eGeneralRegister.DL: return regs.DL;
                case eGeneralRegister.BL: return regs.BL;
                case eGeneralRegister.AH: return regs.AH;
                case eGeneralRegister.CH: return regs.CH;
                case eGeneralRegister.DH: return regs.DH;
                case eGeneralRegister.BH: return regs.BH;
                case eGeneralRegister.AX: return regs.AX;
                case eGeneralRegister.CX: return regs.CX;
                case eGeneralRegister.DX: return regs.DX;
                case eGeneralRegister.BX: return regs.BX;
                case eGeneralRegister.SP: return regs.SP;
                case eGeneralRegister.BP: return regs.BP;
                case eGeneralRegister.SI: return regs.SI;
                case eGeneralRegister.DI: return regs.DI;
                case eGeneralRegister.EAX: return regs.EAX;
                case eGeneralRegister.ECX: return regs.ECX;
                case eGeneralRegister.EDX: return regs.EDX;
                case eGeneralRegister.EBX: return regs.EBX;
                case eGeneralRegister.ESP: return regs.ESP;
                case eGeneralRegister.EBP: return regs.EBP;
                case eGeneralRegister.ESI: return regs.ESI;
                case eGeneralRegister.EDI: return regs.EDI;
                case eGeneralRegister.CS: return regs.CS.Value;
                case eGeneralRegister.DS: return regs.DS.Value;
                case eGeneralRegister.ES: return regs.ES.Value;
                case eGeneralRegister.FS: return regs.FS.Value;
                case eGeneralRegister.GS: return regs.GS.Value;
                case eGeneralRegister.SS: return regs.SS.Value;
                case eGeneralRegister.CR0: return regs.CR0;
                case eGeneralRegister.CR2: return regs.CR2;
                case eGeneralRegister.CR3: return regs.CR3;
                case eGeneralRegister.CR4: return regs.CR4;
                case eGeneralRegister.DR0: return regs.DR0;
                case eGeneralRegister.DR1: return regs.DR1;
                case eGeneralRegister.DR2: return regs.DR2;
                case eGeneralRegister.DR3: return regs.DR3;
                case eGeneralRegister.DR6: return regs.DR6;
                case eGeneralRegister.DR7: return regs.DR7;
            }
            throw new Exception("GetRegcValueForRegEnum - Register address not found for: " + Register);
        }

        internal UInt32 GetControlRegAddrForRegEnum(eGeneralRegister Register)
        {
            switch (Register)
            {
                case eGeneralRegister.DR0:
                    return RDR0;
                case eGeneralRegister.DR1:
                    return RDR1;
                case eGeneralRegister.DR2:
                    return RDR2;
                case eGeneralRegister.DR3:
                    return RDR3;
                case eGeneralRegister.DR6:
                    return RDR6;
                case eGeneralRegister.DR7:
                    return RDR7;
            }
            throw new Exception("Cannot identify Control Register: " + Register);
        }

        internal UInt32 GetDebugRegAddrForRegEnum(eGeneralRegister Register)
        {
            throw new Exception("GetDebugRegAddrForRegEnum: Incomplete!");
            switch (Register)
            {
                case eGeneralRegister.CR0:
                    return RCR0;
                case eGeneralRegister.CR2:
                    return RCR2;
                case eGeneralRegister.CR3:
                    return RCR3;
                case eGeneralRegister.CR4:
                    return RCR4;
            }
            throw new Exception("Cannot identify Debug Register: " + Register);
        }

        #region Register Related Stuff
        internal const UInt32 REGADDRBASE = 0xFFFFF000;
        internal const byte REGSIZE = 4;
        //Register offsets
        internal void InitRegLocations()
        {
        }
        //AX = 0
        public const int RAXOFS = 0; public const int RALOFS = RAXOFS; public const int RAHOFS = RAXOFS + 1; public const int REAXOFS = RAXOFS;
        //BX = 4
        public const int RBXOFS = RAXOFS + REGSIZE; public const int RBLOFS = RBXOFS; public const int RBHOFS = RBXOFS + 1; public const int REBXOFS = RBXOFS;
        //CX = 8
        public const int RCXOFS = RBXOFS + REGSIZE; public const int RCLOFS = RCXOFS; public const int RCHOFS = RCXOFS + 1; public const int RCAXOFS = RCXOFS;
        //DX = C
        public const int RDXOFS = RCXOFS + REGSIZE; public const int RDLOFS = RDXOFS; public const int RDHOFS = RDXOFS + 1; public const int REDXOFS = RDXOFS;

        //SI = 10; DI = 14; BP = 18; SP = 1C
        public const int RSIOFS = RDXOFS + REGSIZE; public const int RESIOFS = RSIOFS;
        public const int RDIOFS = RSIOFS + REGSIZE; public const int REDIOFS = RDIOFS;
        public const int RBPOFS = RDIOFS + REGSIZE; public const int REBPOFS = RBPOFS;
        public const int RSPOFS = RBPOFS + REGSIZE; public const int RESPOFS = RSPOFS;
        //IP = 20;
        public const int RIPOFS = RSPOFS + REGSIZE; public const int REIPOFS = RIPOFS;
        //Control Registers: CR0; CR2; CR3; CR4  24, 28, 2c, 30
        public const int RCR0OFS = RIPOFS + REGSIZE; public const int RCR2OFS = RCR0OFS + REGSIZE; public const int RCR3OFS = RCR2OFS + REGSIZE; public const int RCR4OFS = RCR3OFS + REGSIZE;
        //Debug Registers: DR0; DR1; DR2; DR3; DR6; DR7 = 34, 38, 3c, 40
        public const int RDR0OFS = RCR4OFS + REGSIZE; public const int RDR1OFS = RDR0OFS + REGSIZE; public const int RDR2OFS = RDR1OFS + REGSIZE; public const int RDR3OFS = RDR2OFS + REGSIZE;
        public const int RDR6OFS = RDR3OFS + REGSIZE; public const int RDR7OFS = RDR6OFS + REGSIZE;
        //Test Registers: TR4; TR5; TR6; TR7
        public const int RTR4OFS = RDR7OFS + REGSIZE; public const int RTR5OFS = RTR4OFS + REGSIZE; public const int RTR6OFS = RTR5OFS + REGSIZE; public const int RTR7OFS = RTR6OFS + REGSIZE;
        //FLAGS
        public const int RFLOFS = RTR7OFS + REGSIZE; public const int REFLOFS = RFLOFS;
        //IDTR
        public const int RIDTROFS = RFLOFS + REGSIZE;
        //GDTR
        public const int RGDTROFS = RIDTROFS + 64;
        //LDTR
        public const int RLDTROFS = RGDTROFS + 64;

        // CS = 36; DS = 40; ES = 44
        public const int RCSOFS = RLDTROFS + REGSIZE; public const int RDSOFS = RCSOFS + REGSIZE; public const int RESOFS = RDSOFS + REGSIZE;
        //FS; GS; SS
        public const int RFSOFS = RESOFS + REGSIZE; public const int RGSOFS = RFSOFS + REGSIZE; public const int RSSOFS = RGSOFS + REGSIZE;
        public const UInt32 RAL = (REGADDRBASE + RALOFS); public const UInt32 RAH = (UInt32)(REGADDRBASE + RAHOFS);
        public const UInt32 RBL = (REGADDRBASE + RBLOFS); public const UInt32 RBH = (UInt32)(REGADDRBASE + RBHOFS);
        public const UInt32 RCLr = (REGADDRBASE + RCLOFS); public const UInt32 RCH = (UInt32)(REGADDRBASE + RCHOFS);
        public const UInt32 RDL = (REGADDRBASE + RDLOFS); public const UInt32 RDH = (UInt32)(REGADDRBASE + RDHOFS);
        public const UInt32 RAX = (REGADDRBASE + RAXOFS);
        public const UInt32 RBX = (REGADDRBASE + RBXOFS);
        public const UInt32 RCX = (REGADDRBASE + RCXOFS);
        public const UInt32 RDX = (REGADDRBASE + RDXOFS);
        public const UInt32 RSI = (REGADDRBASE + RSIOFS);
        public const UInt32 RDI = (REGADDRBASE + RDIOFS);
        public const UInt32 RBP = (REGADDRBASE + RBPOFS);
        public const UInt32 RSP = (REGADDRBASE + RSPOFS);
        public const UInt32 RIP = (REGADDRBASE + RIPOFS);
        public const UInt32 RCS = (REGADDRBASE + RCSOFS);
        public const UInt32 RDS = (REGADDRBASE + RDSOFS);
        public const UInt32 RSS = (REGADDRBASE + RSSOFS);
        public const UInt32 RES = (REGADDRBASE + RESOFS);
        public const UInt32 RFS = (REGADDRBASE + RFSOFS);
        public const UInt32 RGS = (REGADDRBASE + RGSOFS);
        public const UInt32 RDR0 = (REGADDRBASE + RDR0OFS);
        public const UInt32 RDR1 = (REGADDRBASE + RDR1OFS);
        public const UInt32 RDR2 = (REGADDRBASE + RDR2OFS);
        public const UInt32 RDR3 = (REGADDRBASE + RDR3OFS);
        public const UInt32 RDR6 = (REGADDRBASE + RDR6OFS);
        public const UInt32 RDR7 = (REGADDRBASE + RDR7OFS);
        public const UInt32 RFL = (REGADDRBASE + RFLOFS);
        public const UInt32 REAX = RAX; public const UInt32 REBX = RBX; public const UInt32 RECX = RCX;
        public const UInt32 REDX = RDX; public const UInt32 RESI = RSI; public const UInt32 REDI = RDI; public const UInt32 REBP = RBP; public const UInt32 RESP = RSP; public const UInt32 REIP = RIP; public const UInt32 REFL = RFL;
        public const UInt32 RCR0 = (REGADDRBASE + RCR0OFS);
        public const UInt32 RCR2 = (REGADDRBASE + RCR2OFS);
        public const UInt32 RCR3 = (REGADDRBASE + RCR3OFS);
        public const UInt32 RCR4 = (REGADDRBASE + RCR4OFS);
        public const UInt32 RIDTR = (REGADDRBASE + RIDTROFS);
        public const UInt32 RGDTR = (REGADDRBASE + RGDTROFS);

        //SetC up register addresses
        //byte for H/L registers, UInt16 for 16 bit, UInt32 for extended
        #endregion

        #region Event Stuff
        // Define a class to hold custom event info
        public class CustomEventArgs : EventArgs
        {
            public CustomEventArgs(Processor_80x86 proc)
            {
                mProc = proc;
            }
            private Processor_80x86 mProc;
            public Processor_80x86 ProcInfo
            {
                get { return mProc; }
            }
        }

        public class CEAStartDebugging : EventArgs
        {
            public CEAStartDebugging(Processor_80x86 proc)
            {
                mCS = proc.regs.CS.Value;
                mEIP = proc.regs.EIP;
                mOperatingMode = proc.OperatingMode;
                mProcessorFoundDebuggingStart = false;
            }
            private UInt32 mCS, mEIP;
            private ProcessorMode mOperatingMode;
            internal bool mProcessorFoundDebuggingStart;
            public UInt32 CS
            {
                get { return mCS; }
            }
            public UInt32 EIP
            {
                get { return mEIP; }
            }
            public ProcessorMode OperatingMode
            {
                get { return mOperatingMode; }
            }
            public bool ProcessorFoundDebuggingStart
            {
                get { return mProcessorFoundDebuggingStart; }
            }
        }


        // Declare the event using EventHandler<T>
        public event EventHandler<CEAStartDebugging> StartDebugging;
        public virtual void OnStartDebugging(CEAStartDebugging e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CEAStartDebugging> handler = StartDebugging;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        public event EventHandler<CEAStartDebugging> WatchExecutionAddress;
        internal virtual void OnWatchExecutionAddress(CEAStartDebugging e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CEAStartDebugging> handler = WatchExecutionAddress;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        public event EventHandler<CustomEventArgs> ParseCompleteEvent;
        protected virtual void OnParseCompleteEvent(CustomEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CustomEventArgs> handler = ParseCompleteEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        public event EventHandler<CustomEventArgs> InstructCompleteEvent;
        protected virtual void OnInstructCompleteEvent(CustomEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CustomEventArgs> handler = InstructCompleteEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        public event EventHandler<CustomEventArgs> ServiceInterruptStart;
        protected virtual void OnServiceInterruptStart(CustomEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CustomEventArgs> handler = ServiceInterruptStart;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        public event EventHandler<CustomEventArgs> InvalidOpcodeStart;
        protected virtual void OnInvalidOpCode(CustomEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CustomEventArgs> handler = InvalidOpcodeStart;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        public event EventHandler<CustomEventArgs> SingleStepEvent;
        protected virtual void OnSingleStepEvent(CustomEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CustomEventArgs> handler = SingleStepEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        #endregion

        #region removed from executeinstruction
        //if (this.mCurrentInstruction.Name.Contains("CMPS"))
        //{
        //    if (this.mCurrentInstruction.Name.EndsWith("B"))
        //    {
        //        mCurrentOperation.Op1Vals = OpDecoder.ValueToHex(mem.GetByte(PhysicalMem.GetLocForSegOfs(this, regs.DS.Value, regs.SI)), 2);
        //        mCurrentOperation.Op2Vals = OpDecoder.ValueToHex(mem.GetByte(PhysicalMem.GetLocForSegOfs(this, regs.ES.Value, regs.DI)), 2);
        //    }
        //    else if (this.mCurrentInstruction.Name.EndsWith("W"))
        //    {
        //        mCurrentOperation.Op1Vals = OpDecoder.ValueToHex(mem.GetWord(PhysicalMem.GetLocForSegOfs(this, regs.DS.Value, regs.SI)), 4);
        //        mCurrentOperation.Op2Vals = OpDecoder.ValueToHex(mem.GetWord(PhysicalMem.GetLocForSegOfs(this, regs.ES.Value, regs.DI)), 4);
        //    }
        //    else
        //    {
        //        mCurrentOperation.Op1Vals = OpDecoder.ValueToHex(mem.GetDWord(PhysicalMem.GetLocForSegOfs(this, regs.DS.Value, regs.SI)), 8);
        //        mCurrentOperation.Op2Vals = OpDecoder.ValueToHex(mem.GetDWord(PhysicalMem.GetLocForSegOfs(this, regs.ES.Value, regs.DI)), 8);
        //    }
        //}
        //else if (this.mCurrentInstruction.Name.Contains("SCAS"))
        //{
        //    if (this.mCurrentInstruction.Name.EndsWith("B"))
        //    {
        //        mCurrentOperation.Op1Vals = OpDecoder.ValueToHex(mem.GetByte(PhysicalMem.GetLocForSegOfs(this, regs.ES.Value, regs.DI)), 2);
        //        mCurrentOperation.Op2Vals = OpDecoder.ValueToHex(regs.AL, 2);
        //    }
        //    else if (this.mCurrentInstruction.Name.EndsWith("W"))
        //    {
        //        mCurrentOperation.Op1Vals = OpDecoder.ValueToHex(mem.GetWord(PhysicalMem.GetLocForSegOfs(this, regs.ES.Value, regs.DI)), 4);
        //        mCurrentOperation.Op2Vals = OpDecoder.ValueToHex(regs.AL, 4);
        //    }
        //    else
        //    {
        //        mCurrentOperation.Op1Vals = OpDecoder.ValueToHex(mem.GetDWord(PhysicalMem.GetLocForSegOfs(this, regs.ES.Value, regs.DI)), 8);
        //        mCurrentOperation.Op2Vals = OpDecoder.ValueToHex(regs.AL, 8);
        //    }
        //}

        #endregion

        #region Removed
        //internal UInt32 GetRegWAddrForRegEnum(eGeneralRegister Register, byte wValue)
        //{
        //    if (OpSize16)
        //        switch (wValue)
        //        {
        //            case 0:
        //                #region Return w=0 versions of OpSize16 registers
        //                switch (Register)
        //                {
        //                    case eGeneralRegister.AL: return RAL;
        //                    case eGeneralRegister.CL: return RCL;
        //                    case eGeneralRegister.DL: return RDL;
        //                    case eGeneralRegister.BL: return RBL;
        //                    case eGeneralRegister.AH: return RAH;
        //                    case eGeneralRegister.CH: return RCH;
        //                    case eGeneralRegister.DH: return RDH;
        //                    case eGeneralRegister.BH: return RBH;
        //                    default: throw new ExceptionNumber("Cannot find register address for: " + Register);
        //                }
        //                break;
        //                #endregion
        //            case 1:
        //                #region Return w=1 versions of OpSize16 registers
        //                switch (Register)
        //                {
        //                    case eGeneralRegister.AL: return RAX;
        //                    case eGeneralRegister.CL: return RCX;
        //                    case eGeneralRegister.DL: return RDX;
        //                    case eGeneralRegister.BL: return RBX;
        //                    case eGeneralRegister.AH: return RSP;
        //                    case eGeneralRegister.CH: return RBP;
        //                    case eGeneralRegister.DH: return RSI;
        //                    case eGeneralRegister.BH: return RDI;
        //                    default: throw new ExceptionNumber("Cannot find register address for: " + Register);
        //                }
        //            #endregion
        //        }
        //    else
        //    {
        //        switch (wValue)
        //        {
        //            case 0:
        //                #region Return w=0 versions of OpSize 32 registers
        //                switch (Register)
        //                {
        //                    case eGeneralRegister.AL: return RAL;
        //                    case eGeneralRegister.CL: return RCL;
        //                    case eGeneralRegister.DL: return RDL;
        //                    case eGeneralRegister.BL: return RBL;
        //                    case eGeneralRegister.AH: return RAH;
        //                    case eGeneralRegister.CH: return RCH;
        //                    case eGeneralRegister.DH: return RDH;
        //                    case eGeneralRegister.BH: return RBH;
        //                    default: throw new ExceptionNumber("Cannot find register address for: " + Register);
        //                }
        //                #endregion
        //            case 1:
        //                #region Return w=1 versions of OpSize 32 registers
        //                switch (Register)
        //                {
        //                    case eGeneralRegister.AL: return REAX;
        //                    case eGeneralRegister.CL: return RECX;
        //                    case eGeneralRegister.DL: return REDX;
        //                    case eGeneralRegister.BL: return REBX;
        //                    case eGeneralRegister.AH: return RESP;
        //                    case eGeneralRegister.CH: return REBP;
        //                    case eGeneralRegister.DH: return RESI;
        //                    case eGeneralRegister.BH: return REDI;
        //                    default: throw new ExceptionNumber("Cannot find register address for: " + Register);
        //                }
        //                #endregion
        //        }
        //    }
        //    throw new ExceptionNumber("Cannot find register address for: " + Register);
        //}
        //internal UInt32 GetRegNonWAddrForRegEnum(eGeneralRegister Register)
        //{
        //    if (OpSize16)
        //    {
        //        switch (Register)
        //        {

        //            case eGeneralRegister.AL: return RAL;
        //            case eGeneralRegister.CL: return RCL;
        //            case eGeneralRegister.DL: return RDL;
        //            case eGeneralRegister.BL: return RBL;
        //            case eGeneralRegister.AH: return RAH;
        //            case eGeneralRegister.CH: return RCH;
        //            case eGeneralRegister.DH: return RDH;
        //            case eGeneralRegister.BH: return RBH;

        //            //In some places we actually use the correct register names
        //            case eGeneralRegister.AX: return RAX;
        //            case eGeneralRegister.CX: return RCX;
        //            case eGeneralRegister.DX: return RDX;
        //            case eGeneralRegister.BX: return RBX;
        //            case eGeneralRegister.SP: return RSP;
        //            case eGeneralRegister.BP: return RBP;
        //            case eGeneralRegister.SI: return RSI;
        //            case eGeneralRegister.DI: return RDI;
        //        }
        //    }
        //    else
        //    {
        //        switch (Register)
        //        {
        //            case eGeneralRegister.AL: return RAL;
        //            case eGeneralRegister.CL: return RCL;
        //            case eGeneralRegister.DL: return RDL;
        //            case eGeneralRegister.BL: return RBL;
        //            case eGeneralRegister.AH: return RAH;
        //            case eGeneralRegister.CH: return RCH;
        //            case eGeneralRegister.DH: return RDH;
        //            case eGeneralRegister.BH: return RBH;

        //            //In some places we actually use the correct register names
        //            case eGeneralRegister.AX: return REAX;
        //            case eGeneralRegister.CX: return RECX;
        //            case eGeneralRegister.DX: return REDX;
        //            case eGeneralRegister.BX: return REBX;
        //            case eGeneralRegister.SP: return RESP;
        //            case eGeneralRegister.BP: return REBP;
        //            case eGeneralRegister.SI: return RESI;
        //            case eGeneralRegister.DI: return REDI;
        //        }
        //    }
        //    throw new ExceptionNumber("Cannot find register address for: " + Register);
        //}
        #endregion

        internal Instruct MOV;
        internal Instruct CALL;
        internal Instruct ADD;
        internal Instruct PUSH;
        internal Instruct INC;
        internal Instruct POP;
        internal Instruct JMP;
        internal Instruct CMP;
        internal Instruct CMPXCHG;
        internal Instruct AND;
        internal Instruct JA;
        internal Instruct JB;
        internal Instruct BSF;
        internal Instruct BSR;
        internal Instruct BSWAP;
        internal Instruct BT;
        internal Instruct BTC;
        internal Instruct BTR;
        internal Instruct BTS;
        internal Instruct CALLF;
        internal Instruct CBW;
        internal Instruct CLC;
        internal Instruct CLD;
        internal Instruct CLI;
        internal Instruct CLTS;
        internal Instruct CMC;
        internal Instruct CMOVcc;
        internal Instruct CMPSB;
        internal Instruct CMPSD;
        internal Instruct CMPSW;
        internal Instruct CPUID;
        internal Instruct CWD;
        internal Instruct DAA;
        internal Instruct DAS;
        internal Instruct DEC;
        internal Instruct DIV;
        internal Instruct ENTER;
        internal Instruct FADD;
        internal Instruct FCLEX;
        internal Instruct FDIVR;
        internal Instruct FINIT;
        internal Instruct FILD;
        internal Instruct FIST;
        internal Instruct FLDCW;
        internal Instruct FLD1;
        internal Instruct FLDZ;
        internal Instruct FIMUL;
        internal Instruct FRNDINT;
        internal Instruct FMUL;
        internal Instruct FSTENV;
        internal Instruct FSTCW;
        internal Instruct FSTSW;
        internal Instruct HLT;
        internal Instruct ICEBP;
        internal Instruct IDIV;
        internal Instruct IMUL;
        internal Instruct IN;
        internal Instruct INS;
        internal Instruct INT;
        internal Instruct INTO;
        internal Instruct IRET;
        internal Instruct JAE;
        internal Instruct JBE;
        internal Instruct JC;
        internal Instruct JCXZ;
        internal Instruct JE;
        internal Instruct JG;
        internal Instruct JGE;
        internal Instruct JL;
        internal Instruct JLE;
        internal Instruct JMPF;
        internal Instruct JNA;
        internal Instruct JNAE;
        internal Instruct JNB;
        internal Instruct JNBE;
        internal Instruct JNC;
        internal Instruct JNE;
        internal Instruct JNG;
        internal Instruct JNGE;
        internal Instruct JNLE;
        internal Instruct JNO;
        internal Instruct JNP;
        internal Instruct JNS;
        internal Instruct JNZ;
        internal Instruct JO;
        internal Instruct JP;
        internal Instruct JPE;
        internal Instruct JPO;
        internal Instruct JS;
        internal Instruct JZ;
        internal Instruct LAHF;
        internal Instruct LDS;
        internal Instruct LEA;
        internal Instruct LEAVE;
        internal Instruct LES;
        internal Instruct LFS;
        internal Instruct LGS;
        internal Instruct LIDT;
        internal Instruct LLDT;
        internal Instruct LGDT;
        internal Instruct LMSW;
        internal Instruct LTR;
        /*internal Instruct LOCK = new LOCK(),this);*/
        internal Instruct LODSB;
        internal Instruct LODSD;
        internal Instruct LODSW;
        internal Instruct LOOP;
        internal Instruct LOOPE;
        internal Instruct LOOPNE;
        internal Instruct LOOPNZ;
        internal Instruct LOOPZ;
        internal Instruct LSS;
        internal Instruct MOVSX;
        internal Instruct MOVZX;
        internal Instruct MOVSB;
        internal Instruct MOVSD;
        internal Instruct MOVSW;
        internal Instruct MUL;
        internal Instruct NEG;
        internal Instruct NOP;
        internal Instruct NOT;
        internal Instruct OR;
        internal Instruct OUT;
        internal Instruct OUTS;
        internal Instruct POPA;
        internal Instruct POPF;
        internal Instruct PUSHA;
        internal Instruct PUSHAD;
        internal Instruct PUSHF;
        internal Instruct RCL;
        internal Instruct RCR;
        internal Instruct REP;
        internal Instruct REPZ;
        internal Instruct REPE;
        internal Instruct REPNE;
        internal Instruct REPNZ;
        internal Instruct RET;
        internal Instruct RETF;
        internal Instruct RDTSC;
        internal Instruct ROL;
        internal Instruct ROR;
        internal Instruct SAHF;
        internal Instruct SBB;
        internal Instruct SCASB;
        internal Instruct SCASD;
        internal Instruct SCASW;
        internal Instruct SAL;
        internal Instruct SALC;
        internal Instruct SAR;
        internal Instruct SETcc;
        internal Instruct SGDT;
        internal Instruct SHL;
        internal Instruct SIDT;
        internal Instruct SHLD;
        internal Instruct SMSW;
        internal Instruct SHR;
        internal Instruct SHRD;
        internal Instruct STC;
        internal Instruct STD;
        internal Instruct STI;
        internal Instruct STR;
        internal Instruct STOSB;
        internal Instruct STOSD;
        internal Instruct STOSW;
        internal Instruct SUB;
        internal Instruct TEST;
        internal Instruct FWAIT;
        internal Instruct VERR;
        internal Instruct VERW;
        internal Instruct XCHG;
        internal Instruct XLAT;
        internal Instruct XLATB;
        internal Instruct XOR;
        internal Instruct GRP1;
        internal Instruct GRP2;
        internal Instruct GRP3a;
        internal Instruct GRP3b;
        internal Instruct GRP4;
        internal Instruct GRP5;
        internal Instruct GRP60;
        internal Instruct GRP61;
        internal Instruct GRP7;
        internal Instruct GRP8;
        internal Instruct GRPC0;
        internal Instruct GRPC1;
        internal Instruct GRPPUSH;
        internal Instruct AAA;
        internal Instruct AAD;
        internal Instruct AAM;
        internal Instruct AAS;
        internal Instruct ADC;
        internal Instruct ARPL;
        internal Instruct BOUND;

    }

}