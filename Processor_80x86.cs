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
                if (OperatingMode == ProcessorMode.Protected)
                {
                    //if mOpSize16=false - override prefix was found so flip the value we would otherwise send back
                    //(note: we would have sent back ! so we'll "not send back not"
                    if (regs.CS.mDescriptorNum == 0)
                        return mOpSize16;
                    if (!mOpSize16)
                        return regs.CS.Selector.granularity.OpSize32;
                    else
                        return !regs.CS.Selector.granularity.OpSize32;
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
                if (OperatingMode == ProcessorMode.Protected)
                {
                    //if mAddrSize16=false - override prefix was found so flip the value we would otherwise send back
                    //(note: we would have sent back ! so we'll "not send back not"
                    if (regs.CS.mDescriptorNum == 0)
                        return mAddrSize16;
                    bool retVal = !regs.CS.Selector.granularity.OpSize32;
                    if ( mProcessorStatus != eProcessorStatus.Decoding && mCurrentInstruction.DecodedInstruction.AddrSizePrefixFound)
                        return !retVal;
                    else
                        return retVal;
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
                if (OperatingMode == ProcessorMode.Protected)
                {
                    //if mAddrSize16=false - override prefix was found so flip the value we would otherwise send back
                    //(note: we would have sent back ! so we'll "not send back not"
                    if (regs.SS.mDescriptorNum == 0)
                        //07/23/2013 - mOpSize16 just ain't workin!
                        return mOpSize16;
                    //return !regs.CS.Selector.granularity.OpSize32;
                    if (mOverrideStackSize)
                        return mOverriddenStackSizeIs16;
                     return (!regs.SS.Selector.granularity.OpSize32);
                    //else if (!mAddrSize16)
                    //    return false;
                    //else return !regs.SS.Selector.granularity.OpSize32;
                }
                else
                    //True in real mode = 16 bit
                    if (mAddrSize16)
                        return true;
                    //False in real mode = 32 bit
                    else
                        return false;
            }

        }
        public UInt64 TestInstructKillCount = 0;
        /// <summary>
        /// List of instructions, used only to get the right sOpCode for a given instruction
        /// </summary>
        public InstructionList Instructions = new InstructionList();
        public sOpCodePointer[] OpCodeIndexer = new sOpCodePointer[0xFFFF];
        internal sSignals Signals;
        internal static bool mProtectedModeActive = false;
        internal bool mIncrementedEIP = false;
        public bool mGenerateDecodeStrings = false, mCalculateInstructionTimings = false;
        public ProcessorMode mCurrInstructOpMode;
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
        public Instruct mCurrentInstruction;
        //public sOperation mCurrentOperation;
        internal bool mOpSize16 = true, mAddrSize16 = true, mOverrideStackSize = false, mOverriddenStackSizeIs16 = false, mExceptionWhileExcepting;
        public UInt32 mSegmentOverride = 0;
        internal byte mBranchHint = 0;
        internal UInt64 mInsructionsExecuted, mInsructionsDecoded;
        public double mMSinTimedSection = 0;
        public UInt64 InstructionsExecuted
        { get { return mInsructionsExecuted; } }
        public UInt64 InstructionsDecoded
        { get { return mInsructionsDecoded; } }
        public DateTime StartedAt;
        public DateTime StoppedAt;
        public bool HaltProcessor = false, PowerOff = false, TrapNextInst = false, ServicingIRQ = false, mExportDebug = false;
        private int mExceptionNumber = Global.CPU_NO_EXCEPTION;
        public UInt32 mLastInstructionAddress = 0, mCurrentInstructionAddress = 0, mExceptionTransferAddress = 0, mLastEIP = 0, mLastESP = 0;
        public int mTimerTickMSec = 2; //Default is just over 18 times per second (18.222)
        public cGDTCache mGDTCache, mLDTCache;
        public cIDTCache mIDTCache;
        internal sTSS mCurrTSS = new sTSS();
        //Used when rep type prefixes are called
        //Used by commands like REP, set to true till condition is met or CX = 0
        //Instruction will set back to false if condition met/not met
        //Processor will set back to false if CX = 0
        private Decoder mDecoder;
        //internal cInstructionCache InstructionCache = new cInstructionCache();
        internal cInstructCache InstructCache = new cInstructCache();
        public cTLB mTLB;
        public Int64 mCacheHits, mCacheMisses;
        public byte mRepeatCondition = NOT_REPEAT;
        public OpcodeLoader ocl = new OpcodeLoader();
        public DWord mFlagsBefore;
        public byte mLastIRQVector = 0;
        public eSwitchSource mSwitchSource;
        DateTime TimeStart, TimeEnd;
        internal eProcessorStatus mProcessorStatus = eProcessorStatus.Decoding;
        public eProcessorStatus ProcessorStatus { get { return mProcessorStatus; } }
        public bool mSingleStep = false;
        CEAStartDebugging CAEStartDeb;

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
                regs.CR0 = 0x60010030; //Old value = 0x60000010, changed to deactivate math co
            else
                regs.CR0 = 0x60000030; //Old value = 0x60000010, changed to deactivate math co
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
            UInt32 lExceptionErrorCode = 0;

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
                if (mem.mChangesPending > 0)
                    mem.Commit();
                mCurrInstructOpMode = OperatingMode;
                mIncrementedEIP = false;
                if (!HaltProcessor)
                {
                    if (mRepeatCondition == NOT_REPEAT)
                    {
                        //mCurrentInstruction = null;
                        mCurrentInstructionAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, (UInt32)regs.EIP);
                        //    //mCurrentInstruction = InstructCache.Get(mCurrentInstructionAddress);
                        //    mCurrentInstruction = InstructionCache[mCurrentInstructionAddress];
                        //if (mCurrentInstruction == null)
                        {
                            mCacheMisses++;
                            if (mProcessorStatus != eProcessorStatus.Resetting && mProcessorStatus != eProcessorStatus.ShuttingDown)
                                mProcessorStatus = eProcessorStatus.Decoding;
                            if (((regs.CR0 & 0x80000000) == 0x80000000 && mCurrentInstructionAddress < Processor_80x86.REGADDRBASE) && mem.PageAccessWillCausePF(this, ref mCurrentInstruction.DecodedInstruction, mCurrentInstructionAddress, false))
                            {
                                mRepeatCondition = NOT_REPEAT;
                                mBranchHint = 0;
                                mOpSize16 = true;
                                mAddrSize16 = true;
                                mOverrideStackSize = false;
                                mSegmentOverride = 0;
                                //ExceptionErrorCode |= 0x10;
                                Signals.LOCK = false;
                                ExceptionHandler(this, ref mCurrentInstruction.DecodedInstruction);
                                goto Top;
                            }
                            if (mLastInstructionAddress != mCurrentInstructionAddress)
                                mCurrentInstruction = mDecoder.Decode();
                            if (mCurrentInstruction.DecodedInstruction.ExceptionThrown)
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
                                ExceptionHandler(this, ref mCurrentInstruction.DecodedInstruction);
                                goto Top;
                            }
                            if (mCurrentInstruction.DecodedInstruction.Lock)
                                Signals.LOCK = true;
                            mInsructionsDecoded++;
                            //InstructionCache.Add(mCurrentInstruction);
                            //if (OperatingMode == ProcessorMode.Protected && mCurrentInstruction.Name == "INT") 
                            //    mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "Int 0x" + mCurrentInstruction.DecodedInstruction.Op1.Imm8.ToString("X4") + ", EIP=" + regs.EIP.ToString("X8") + ", EAX=" + regs.EAX.ToString("X8") + ", CPL=" + regs.CPL);

                            if (mProcessorStatus != eProcessorStatus.Resetting && mProcessorStatus != eProcessorStatus.ShuttingDown)
                                mProcessorStatus = eProcessorStatus.Setup;

                            //catch (Exception e)
                            //{
                            //    throw e;
                            //}
                        }
                        //else
                        //    mCacheHits++;
                        mLastInstructionAddress = mCurrentInstructionAddress;
                        mOpSize16 = !mCurrentInstruction.DecodedInstruction.OpSizePrefixFound;
                        mAddrSize16 = !mCurrentInstruction.DecodedInstruction.AddrSizePrefixFound;
                        mCurrInstructOpSize16 = OpSize16;
                        mCurrInstructAddrSize16 = AddrSize16;
                        if (mCurrentInstruction.DecodedInstruction.OverrideSegment != eGeneralRegister.NONE)
                        {
                            switch (mCurrentInstruction.DecodedInstruction.OverrideSegment)
                            {
                                case eGeneralRegister.CS: mSegmentOverride = RCS; break;
                                case eGeneralRegister.DS: mSegmentOverride = RDS; break;
                                case eGeneralRegister.ES: mSegmentOverride = RES; break;
                                case eGeneralRegister.FS: mSegmentOverride = RFS; break;
                                case eGeneralRegister.GS: mSegmentOverride = RGS; break;
                                case eGeneralRegister.SS: mSegmentOverride = RSS; break;
                            }
                        }
                        if (mCurrentInstruction.DecodedInstruction.RepNZ)
                            mRepeatCondition = REPEAT_TILL_NOT_ZERO;
                        else if (mCurrentInstruction.DecodedInstruction.RepZ)
                            mRepeatCondition = REPEAT_TILL_ZERO;
                    }
                    mCurrentInstruction.SetupExecution(this);
                    if (mCurrentInstruction.DecodedInstruction.ExceptionThrown)
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
                        ExceptionHandler(this, ref mCurrentInstruction.DecodedInstruction);
                        goto Top;
                    }

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
                    if (mExportDebug)
                        OnParseCompleteEvent(new CustomEventArgs(this));
#if DEBUGGER_FEATURE
                    bool lFound = false;
                    if (mSystem.AddressBreakpointCount() > 0 && !mSingleStep)
                    {
                        foreach (cBreakpoint b in mSystem.BreakpointInfo.Where(u => u.InterruptNum == 0 && u.Enabled))
                            if (b.CS == regs.CS.Value && (b.EIP == regs.EIP || b.EIP == 0xFFFFFFFF))
                            {
                                if (b.DisableOnHit)
                                    b.Enabled = false;
                                else if (b.RemoveOnHit)
                                    mSystem.RemoveAddressBreakpoint(b);
                                mExportDebug = b.DebugToFile;
                                mSingleStep = b.DebugToStreen;
                                lFound = true;
                                break;
                            }
                    }
                        //Current instruction = INT and Interrupt = bp int, and Funct = 0 or FUnct = AH/EAX (based on DosFunct flag)
                    else if (mCurrentInstruction.Name == "INT" && mSystem.BreakpointInfo.Where(u => u.InterruptNum == mCurrentInstruction.Op1Value.OpByte 
                        && (u.FunctNum == 0 || (u.FunctNum == regs.EAX && u.DOSFunct == false) 
                                            || (u.FunctNum == regs.AH && u.DOSFunct == true)) && u.Enabled).LongCount() > 0)
                    {
                        cBreakpoint b = mSystem.BreakpointInfo.Where(u => u.InterruptNum == mCurrentInstruction.Op1Value.OpByte && (u.FunctNum == 0 || (u.FunctNum == regs.EAX && u.DOSFunct == false)
                                            || (u.FunctNum == regs.AH && u.DOSFunct == true)) && u.Enabled).First();
                        mExportDebug = b.DebugToFile;
                        mSingleStep = b.DebugToStreen;
                        lFound = true;
                        mSingleStep = true;
                    }
                    else if (mSystem.mSwitchToTaskBreakpoint)
                    {
                        lFound = true;
                        mSingleStep = mSystem.BreakpointInfo.Where(u => u.TaskNum == (regs.TR.SegSel & 0xFFFF) >> 3).First().DebugToStreen;
                        mSystem.mSwitchToTaskBreakpoint = false;
                        mExportDebug = mSystem.BreakpointInfo.Where(u => u.TaskNum == (regs.TR.SegSel & 0xFFFF) >> 3).First().DebugToFile;
                        mSystem.BreakpointInfo.Where(u => u.TaskNum == (regs.TR.SegSel & 0xFFFF) >> 3).First().Enabled = false;
                    }
                    if (lFound)
                    {
                        mSystem.DeviceBlock.mPIT.mPICTimers[0].Stop();
                        mSystem.mSoftIntBreakpoint = false;
                        CEAStartDebugging d = new CEAStartDebugging(this);
                        OnStartDebugging(d);
                    }

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
                    if (mProcessorStatus != eProcessorStatus.Resetting && mProcessorStatus != eProcessorStatus.ShuttingDown)
                        mProcessorStatus = eProcessorStatus.Execution;
                    if ((regs.CR0 & 0x80000000) == 0x80000000) 
                        SaveRegisters();
                    ExecuteInstruction();
                    if (mCurrentInstruction.DecodedInstruction.ExceptionThrown)
                    {
                        regs.EIP = mLastEIP;
                        if (mem.mChangesPending > 0)
                            mem.Rollback();
                        RestoreSavedRegisters();
                        mRepeatCondition = NOT_REPEAT;
                        mBranchHint = 0;
                        mOpSize16 = true;
                        mAddrSize16 = true;
                        mOverrideStackSize = false;
                        Signals.LOCK = false;
                        mSegmentOverride = 0;
                        ExceptionHandler(this, ref mCurrentInstruction.DecodedInstruction);
                        goto Top;
                    }
                    if (mem.mChangesPending > 0)
                        mem.Commit();
                    //TimeStart = DateTime.Now;
                    //mMSinTimedSection += (DateTime.Now - TimeStart).TotalMilliseconds;

                    #region Trap Handling (debug single step)
                    if (TrapNextInst)
                    {
                        mCurrentInstruction.DecodedInstruction.ExceptionNumber = 1;
                        ExceptionHandler(this, ref mCurrentInstruction.DecodedInstruction);
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
                    mCurrentInstruction.UsageCount++;
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
                        InstructCache.Flush();
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
                        InstructCache.Flush();
                        // Do I really need these?    regs.mCSDescriptorNum = regs.mDSDescriptorNum = regs.mESDescriptorNum = regs.mFSDescriptorNum = regs.mGSDescriptorNum = regs.mSSDescriptorNum = 0;
                    }
                    #endregion
                }
                else
                {
                    mInsructionsExecuted++;
                    //Thread.Sleep(1);
                    //Thread.Yield();
                    //NVM: We're in HALT which means the halt instruction was executed, so keep executing it to keep our instruction count correct
                    //regs.EIP -= mCurrentInstruction.DecodedInstruction.BytesUsed;
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
            regs.EIP += mCurrentInstruction.DecodedInstruction.BytesUsed;
            mIncrementedEIP = true;
            if (mCalculateInstructionTimings)
                mCurrentInstruction.mLastStart = DateTime.Now;
            //mFlagsBefore = regs.EFLAGS;

            //Send FPU instructions to the FPU, otherwise execute them
            if (mCurrentInstruction.FPUInstruction)
            {
                mFPU.QueueInstruct(mCurrentInstruction);
                FPU.mNewCalc.Set();
                //CLR 07/04/2013 - No need to delay, in fact it breaks the FPU
                //Thread.Sleep(20);  
            }
            else
                mCurrentInstruction.Impl();

            if (mCurrentInstruction.DecodedInstruction.ExceptionThrown)
                return;
            //if (mFlagsBefore != regs.EFLAGS)
            //    regs.FLAGSB.FlagsDirty = true;
            if (mCalculateInstructionTimings)
                mCurrentInstruction.TotalTimeInInstruct += (DateTime.Now - mCurrentInstruction.mLastStart).TotalMilliseconds;

            if (!mCurrentInstruction.REPAble)
                mRepeatCondition = NOT_REPEAT;

            if (mRepeatCondition != NOT_REPEAT)
            {
                //Need to fix this for !OpSize16
                if (mCurrInstructOpSize16 && regs.CX == 0 || ((!mCurrInstructOpSize16) && regs.ECX == 0))
                    mRepeatCondition = NOT_REPEAT;
                if ((this.mCurrentInstruction.Name.Contains("SCAS") || mCurrentInstruction.Name.Contains("CMPS")))
                    if (mRepeatCondition == REPEAT_TILL_NOT_ZERO && regs.FLAGSB.ZF == true)
                        mRepeatCondition = NOT_REPEAT;
                    else if (mRepeatCondition == REPEAT_TILL_ZERO && regs.FLAGSB.ZF == false)
                        mRepeatCondition = NOT_REPEAT;
                if (mRepeatCondition != NOT_REPEAT && !mCurrentInstruction.DecodedInstruction.ExceptionThrown)
                {
                    regs.EIP -= mCurrentInstruction.DecodedInstruction.BytesUsed;
                }
            }


            if (mCurrentInstruction.DecodedInstruction.ExceptionThrown)
                ExceptionHandler(this, ref mCurrentInstruction.DecodedInstruction);

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
            if (mCurrentInstruction.DecodedInstruction.ExceptionThrown || Signals.HOLD || (Signals.INTR && (regs.FLAGS & 0x200) == 0x200) )
            {
                if (mem.mChangesPending > 0)
                    mem.Commit();
                return true;
            }
            mInsructionsExecuted++;
            //if (Signals.INTR && (regs.FLAGS & 0x200) == 0x200)
            //    return true;

            
            return false;
        }
        public void PrepareForJump()
        {
            Instruct lPush = Instructions["PUSH"];
            lPush.DecodedInstruction = this.mCurrentInstruction.DecodedInstruction;
            lPush.mProc = this;
            lPush.Operand1IsRef = false;
            lPush.Operand2IsRef = false;
            lPush.Op1Value.OpDWord = regs.EFLAGS;
            //lPush.SetupExecution();
            lPush.Impl();
            if (lPush.DecodedInstruction.ExceptionThrown)
            {
                mCurrentInstruction.DecodedInstruction = lPush.DecodedInstruction;
                return;
            }
            lPush.Op1Value.OpDWord = regs.CS.Value;
            //lPush.SetupExecution();
            lPush.Impl();
            if (lPush.DecodedInstruction.ExceptionThrown)
            {
                mCurrentInstruction.DecodedInstruction = lPush.DecodedInstruction;
                return;
            }
            lPush.Op1Value.OpDWord = (UInt32)regs.EIP;
            //lPush.SetupExecution();
            lPush.Impl();
            if (lPush.DecodedInstruction.ExceptionThrown)
            {
                mCurrentInstruction.DecodedInstruction = lPush.DecodedInstruction;
                return;
            }
            if (mem.mChangesPending > 0)
                mem.Commit();
        }
        public void HandleNewIRQServiceRequest()
        {

            mBranchHint = 0;
            mOpSize16 = true;
            mAddrSize16 = true;
            mOverrideStackSize = false;
            mSegmentOverride = 0;

            Word lOldCS = (Word)regs.CS.Value;
            DWord lOldEIP = regs.EIP;

            if (mCurrInstructOpMode == ProcessorMode.Real)
            {
                PrepareForJump();

                mLastIRQVector = mSystem.DeviceBlock.mPIC.PassIRQVectorToCPU();
                if (mLastIRQVector == 0x0)
                    return;

                regs.CS.Value = mem.GetWord(this, ref mCurrentInstruction.DecodedInstruction, (UInt16)(mLastIRQVector * 4 + 2));
                regs.IP = mem.GetWord(this, ref mCurrentInstruction.DecodedInstruction, (UInt16)(mLastIRQVector * 4));

                //Clear the repeat conditon, tis necessary so that the new instruction is executed
                //the loop condition will be re-evaluated when the IRQ handler finishes and POPs
                mRepeatCondition = NOT_REPEAT;
                if (mSystem.Debuggies.DebugCPU)
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
                if (mSystem.Debuggies.DebugCPU)
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

                ServicingIRQ = true;
                Instruct lINT = Instructions["INT"];
                lINT.DecodedInstruction = mCurrentInstruction.DecodedInstruction;
                lINT.Op1Value.OpQWord = lVector;
                lINT.mIntIsSoftware = false;
                lINT.Impl();
                ServicingIRQ = false;
                mCurrentInstruction.DecodedInstruction = lINT.DecodedInstruction;
                //mCurrentInstruction = mDecoder.Decode(this);
            }
        }
        internal void ExceptionHandler(Processor_80x86 mProc, ref sInstruction sIns)
        {
#if DEBUG
            if (mExportDebug)
                OnServiceInterruptStart(new CustomEventArgs(this));
#endif
            UInt32 ExceptionHappenedAtAddress;

            if (sIns.ExceptionNumber == 0)
                System.Diagnostics.Debugger.Break();

            if (mCurrInstructOpMode == ProcessorMode.Real)
            {
                switch (sIns.ExceptionNumber)
                {
                    case 1:
                        ExceptionHappenedAtAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, regs.IP);
                        Instructions["INT"].Op1Value.OpQWord = 0x01;
                        Instructions["INT"].mIntIsSoftware = false;
                        Instructions["INT"].Impl();
                        mExceptionTransferAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, regs.IP);
                        break;
                    case 6:
                        ExceptionHappenedAtAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, regs.IP);
                        //?    
                        //regs.EIP += mCurrentOperation.Length;
                        if (mem.GetDWord(mProc, ref mCurrentInstruction.DecodedInstruction, 0x06 * 4) == 0 || mem.GetDWord(mProc, ref mCurrentInstruction.DecodedInstruction, 0x06 * 4) == 0xf000ff53)
                            throw new Exception("Invalid instruction, no INT 06 handler defined");
                        Instruct lINT = Instructions["INT"];
                        lINT.DecodedInstruction = mCurrentInstruction.DecodedInstruction;
                        lINT.Op1Value.OpQWord = 0x06;
                        lINT.mIntIsSoftware = false;
                        lINT.Impl();
                        //mCurrentInstruction = mCurrentInstruction = mDecoder.Decode(this);
                        mExceptionTransferAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, regs.IP);
                        break;
                }
            }
            else
            {
                //07/23/2013 - FreeDOS does NOT like this, so I'm going to try to do without it
                //if (ExceptionNumber == 0xD)
                //    regs.EIP -= mCurrentInstruction.DecodedInstruction.BytesUsed;

                ExceptionHappenedAtAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, (UInt32)regs.EIP);
                if (mSystem.Debuggies.DebugExceptions || mSystem.Debuggies.DebugCPU)
                {
                    StringBuilder lPrint = new StringBuilder('0');
                    lPrint.AppendFormat("PMode EXCEPTION # {0}.  Executed instruction was: + " + mCurrentInstruction.mName + ", transferring control from {1}",
                        sIns.ExceptionNumber,
                        ExceptionHappenedAtAddress.ToString("X8"));
                    mSystem.PrintDebugMsg(eDebuggieNames.Exceptions, lPrint.ToString());
                }
                //if (mCurrentInstruction.DecodedInstruction.ExceptionThrown == true)
                //    mCurrentInstruction.DecodedInstruction.ExceptionThrown = false;
                if (sIns.ExceptionNumber == 0xE)
                    regs.CR2 = sIns.ExceptionAddress;
                
                Instruct lINT = Instructions["INT"];
                if (mCurrentInstruction != null)
                    lINT.DecodedInstruction = mCurrentInstruction.DecodedInstruction;
                lINT.Op1Value.OpQWord = (UInt32)sIns.ExceptionNumber;
                lINT.mIntIsSoftware = false;
                mExceptionWhileExcepting = false;
                lINT.Impl();
                if (mExceptionWhileExcepting)
                {
                    mSystem.PrintDebugMsg(eDebuggieNames.Exceptions, "Double exception detected, trying one more time.");
                    lINT.Op1Value.OpQWord = (UInt32)sIns.ExceptionNumber;
                    lINT.mIntIsSoftware = false;
                    lINT.Impl();
                    if (mExceptionWhileExcepting)
                    {
                        throw new Exception("Triple exception detected!  Final exception was: " + sIns.ExceptionNumber.ToString("X"));
                    }
                }
                mExceptionTransferAddress = PhysicalMem.GetLocForSegOfs(this, regs.CS.Value, (UInt32)regs.EIP);
                lINT.DecodedInstruction.ExceptionThrown = false;
            }
            sIns.ExceptionNumber = Global.CPU_NO_EXCEPTION;
            sIns.ExceptionThrown = false;
            sIns.ExceptionErrorCode = 0;
            sIns.ExceptionAddress = 0;

            if (mem.mChangesPending > 0)
                mem.Commit();
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
            if (mem.mChangesPending > 0)
                mem.Commit();
            regs.GDTR.lCache = mem.ChunkPhysical(this, 0, regs.GDTR.Base, regs.GDTR.Limit + 1);
            UInt64[] lCache = mem.QWChunkPhysical(regs.GDTR.lCache, (UInt16)((regs.GDTR.Limit + 1) / 8));
            mGDTCache = new cGDTCache(lCache, (UInt16)regs.GDTR.Limit);
			//mTLB.Flush(this);
        }

        public void RefreshLDTCache()
        {
            if (mem.mChangesPending > 0)
                mem.Commit();
            regs.LDTR.lCache = mem.ChunkPhysical(this, 0, regs.LDTR.Base, regs.LDTR.Limit + 1);
            UInt64[] lCache = mem.QWChunkPhysical(regs.LDTR.lCache, (UInt16)((regs.LDTR.Limit + 1) / 8));
            mLDTCache = new cGDTCache(lCache, (UInt16)regs.LDTR.Limit);
			//mTLB.Flush(this);
        }

        public void RefreshIDTCache()
        {
            if (mem.mChangesPending > 0)
                mem.Commit();
            regs.IDTR.lCache = mem.ChunkPhysical(this, 0, regs.IDTR.Base, regs.IDTR.Limit + 1);
            UInt64[] lCache = mem.QWChunkPhysical(regs.IDTR.lCache, (UInt16)((regs.IDTR.Limit + 1) / 8));
            mIDTCache = new cIDTCache(lCache, mGDTCache);
            //int a = 0;
            //if (regs.CS.Value != 0xf000)
            //    a = 1;
        }

        public void InitInstructions(InstructionList Instructions)
        {
            Instructions.Add(new MOV(), this);
            Instructions.Add(new MOVZX(), this);
            Instructions.Add(new CALL(), this);
            Instructions.Add(new ADD(), this);
            Instructions.Add(new PUSH(), this);
            Instructions.Add(new INC(), this);
            Instructions.Add(new POP(), this);
            Instructions.Add(new JMP(), this);
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
            Instructions.Add(new FMUL(), this);
            Instructions.Add(new FSAVE(), this);
            Instructions.Add(new FSTCW(), this);
            Instructions.Add(new FSTENV(), this);
            Instructions.Add(new FSTP(), this);
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

            foreach (Instruct i in Instructions)
                i.mProc = this;

        }
        internal UInt32 GetRegAddrForRegEnum(eGeneralRegister Register)
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
                case eGeneralRegister.EAX: return regs.EAX;
                case eGeneralRegister.ECX: return regs.ECX;
                case eGeneralRegister.EDX: return regs.EDX;
                case eGeneralRegister.EBX: return regs.EBX;
                case eGeneralRegister.ESP: return regs.ESP;
                case eGeneralRegister.EBP: return regs.EBP;
                case eGeneralRegister.ESI: return regs.ESI;
                case eGeneralRegister.EDI: return regs.EDI;
                //case eGeneralRegister.CS: return mem.PagedMemoryAddress(this, regs.CS.Value);
                //case eGeneralRegister.DS: return mem.PagedMemoryAddress(this, regs.DS.Value);
                //case eGeneralRegister.ES: return mem.PagedMemoryAddress(this, regs.ES.Value);
                //case eGeneralRegister.FS: return mem.PagedMemoryAddress(this, regs.FS.Value);
                //case eGeneralRegister.GS: return mem.PagedMemoryAddress(this, regs.GS.Value);
                //case eGeneralRegister.SS: return mem.PagedMemoryAddress(this, regs.SS.Value);
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
            throw new Exception("GetDWordRegValueForRegEnum - Register address not found for: " + Register);
        }

        //internal cValue GetRegcValueForRegEnum(eGeneralRegister Register)
        //{
        //    cValue lValue;
        //    switch (Register)
        //    {
        //        case eGeneralRegister.AL: lValue = new cValue(regs.AL); lValue.mIsRegister=true; lValue.mRegisterIsByte=true; return lValue;
        //        case eGeneralRegister.CL: lValue = new cValue(regs.CL); lValue.mIsRegister = true; lValue.mRegisterIsByte = true; return lValue;
        //        case eGeneralRegister.DL: lValue = new cValue(regs.DL); lValue.mIsRegister = true; lValue.mRegisterIsByte = true; return lValue;
        //        case eGeneralRegister.BL: lValue = new cValue(regs.BL); lValue.mIsRegister = true; lValue.mRegisterIsByte = true; return lValue;
        //        case eGeneralRegister.AH: lValue = new cValue(regs.AH); lValue.mIsRegister = true; lValue.mRegisterIsByte = true; return lValue;
        //        case eGeneralRegister.CH: lValue = new cValue(regs.CH); lValue.mIsRegister = true; lValue.mRegisterIsByte = true; return lValue;
        //        case eGeneralRegister.DH: lValue = new cValue(regs.DH); lValue.mIsRegister = true; lValue.mRegisterIsByte = true; return lValue;
        //        case eGeneralRegister.BH: lValue = new cValue(regs.BH); lValue.mIsRegister = true; lValue.mRegisterIsByte = true; return lValue;
        //        case eGeneralRegister.AX: lValue = new cValue(regs.AX); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.CX: lValue = new cValue(regs.CX); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.DX: lValue = new cValue(regs.DX); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.BX: lValue = new cValue(regs.BX); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.SP: lValue = new cValue(regs.SP); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.BP: lValue = new cValue(regs.BP); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.SI: lValue = new cValue(regs.SI); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.DI: lValue = new cValue(regs.DI); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.EAX: lValue = new cValue(regs.EAX); lValue.mIsRegister = true; lValue.mRegisterIsDWord = true; return lValue;
        //        case eGeneralRegister.ECX: lValue = new cValue(regs.ECX); lValue.mIsRegister = true; lValue.mRegisterIsDWord = true; return lValue;
        //        case eGeneralRegister.EDX: lValue = new cValue(regs.EDX); lValue.mIsRegister = true; lValue.mRegisterIsDWord = true; return lValue;
        //        case eGeneralRegister.EBX: lValue = new cValue(regs.EBX); lValue.mIsRegister = true; lValue.mRegisterIsDWord = true; return lValue;
        //        case eGeneralRegister.ESP: lValue = new cValue(regs.ESP); lValue.mIsRegister = true; lValue.mRegisterIsDWord = true; return lValue;
        //        case eGeneralRegister.EBP: lValue = new cValue(regs.EBP); lValue.mIsRegister = true; lValue.mRegisterIsDWord = true; return lValue;
        //        case eGeneralRegister.ESI: lValue = new cValue(regs.ESI); lValue.mIsRegister = true; lValue.mRegisterIsDWord = true; return lValue;
        //        case eGeneralRegister.EDI: lValue = new cValue(regs.EDI); lValue.mIsRegister = true; lValue.mRegisterIsDWord = true; return lValue;
        //        //case eGeneralRegister.CS: lValue = new cValue((DWord)mem.PagedMemoryAddress(this, regs.CS.Value)); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        //case eGeneralRegister.DS: lValue = new cValue((DWord)mem.PagedMemoryAddress(this, regs.DS.Value)); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        //case eGeneralRegister.ES: lValue = new cValue((DWord)mem.PagedMemoryAddress(this, regs.ES.Value)); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        //case eGeneralRegister.FS: lValue = new cValue((DWord)mem.PagedMemoryAddress(this, regs.FS.Value)); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        //case eGeneralRegister.GS: lValue = new cValue((DWord)mem.PagedMemoryAddress(this, regs.GS.Value)); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        //case eGeneralRegister.SS: lValue = new cValue((DWord)mem.PagedMemoryAddress(this, regs.SS.Value)); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.CS: lValue = new cValue((DWord)regs.CS.Value); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.DS: lValue = new cValue((DWord)regs.DS.Value); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.ES: lValue = new cValue((DWord)regs.ES.Value); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.FS: lValue = new cValue((DWord)regs.FS.Value); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.GS: lValue = new cValue((DWord)regs.GS.Value); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.SS: lValue = new cValue((DWord)regs.SS.Value); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.CR0: lValue = new cValue((DWord)regs.CR0); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.CR2: lValue = new cValue((DWord)regs.CR2); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.CR3: lValue = new cValue((DWord)regs.CR3); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.CR4: lValue = new cValue((DWord)regs.CR4); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.DR0: lValue = new cValue((DWord)regs.DR0); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.DR1: lValue = new cValue((DWord)regs.DR1); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.DR2: lValue = new cValue((DWord)regs.DR2); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.DR3: lValue = new cValue((DWord)regs.DR3); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.DR6: lValue = new cValue((DWord)regs.DR6); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //        case eGeneralRegister.DR7: lValue = new cValue((DWord)regs.DR7); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
        //    }
        //    throw new Exception("GetRegcValueForRegEnum - Register address not found for: " + Register);
        //}
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
                //case eGeneralRegister.CS: lValue = new cValue((DWord)mem.PagedMemoryAddress(this, regs.CS.Value)); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
                //case eGeneralRegister.DS: lValue = new cValue((DWord)mem.PagedMemoryAddress(this, regs.DS.Value)); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
                //case eGeneralRegister.ES: lValue = new cValue((DWord)mem.PagedMemoryAddress(this, regs.ES.Value)); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
                //case eGeneralRegister.FS: lValue = new cValue((DWord)mem.PagedMemoryAddress(this, regs.FS.Value)); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
                //case eGeneralRegister.GS: lValue = new cValue((DWord)mem.PagedMemoryAddress(this, regs.GS.Value)); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
                //case eGeneralRegister.SS: lValue = new cValue((DWord)mem.PagedMemoryAddress(this, regs.SS.Value)); lValue.mIsRegister = true; lValue.mRegisterIsWord = true; return lValue;
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
        public const UInt32 RAL = (UInt32)(REGADDRBASE + RALOFS); public const UInt32 RAH = (UInt32)(REGADDRBASE + RAHOFS);
        public const UInt32 RBL = (UInt32)(REGADDRBASE + RBLOFS); public const UInt32 RBH = (UInt32)(REGADDRBASE + RBHOFS);
        public const UInt32 RCLr = (UInt32)(REGADDRBASE + RCLOFS); public const UInt32 RCH = (UInt32)(REGADDRBASE + RCHOFS);
        public const UInt32 RDL = (UInt32)(REGADDRBASE + RDLOFS); public const UInt32 RDH = (UInt32)(REGADDRBASE + RDHOFS);
        public const UInt32 RAX = (UInt32)(REGADDRBASE + RAXOFS);
        public const UInt32 RBX = (UInt32)(REGADDRBASE + RBXOFS);
        public const UInt32 RCX = (UInt32)(REGADDRBASE + RCXOFS);
        public const UInt32 RDX = (UInt32)(REGADDRBASE + RDXOFS);
        public const UInt32 RSI = (UInt32)(REGADDRBASE + RSIOFS);
        public const UInt32 RDI = (UInt32)(REGADDRBASE + RDIOFS);
        public const UInt32 RBP = (UInt32)(REGADDRBASE + RBPOFS);
        public const UInt32 RSP = (UInt32)(REGADDRBASE + RSPOFS);
        public const UInt32 RIP = (UInt32)(REGADDRBASE + RIPOFS);
        public const UInt32 RCS = (UInt32)(REGADDRBASE + RCSOFS);
        public const UInt32 RDS = (UInt32)(REGADDRBASE + RDSOFS);
        public const UInt32 RSS = (UInt32)(REGADDRBASE + RSSOFS);
        public const UInt32 RES = (UInt32)(REGADDRBASE + RESOFS);
        public const UInt32 RFS = (UInt32)(REGADDRBASE + RFSOFS);
        public const UInt32 RGS = (UInt32)(REGADDRBASE + RGSOFS);
        public const UInt32 RDR0 = (UInt32)(REGADDRBASE + RDR0OFS);
        public const UInt32 RDR1 = (UInt32)(REGADDRBASE + RDR1OFS);
        public const UInt32 RDR2 = (UInt32)(REGADDRBASE + RDR2OFS);
        public const UInt32 RDR3 = (UInt32)(REGADDRBASE + RDR3OFS);
        public const UInt32 RDR6 = (UInt32)(REGADDRBASE + RDR6OFS);
        public const UInt32 RDR7 = (UInt32)(REGADDRBASE + RDR7OFS);
        public const UInt32 RFL = (UInt32)(REGADDRBASE + RFLOFS);
        public const UInt32 REAX = RAX; public const UInt32 REBX = RBX; public const UInt32 RECX = RCX;
        public const UInt32 REDX = RDX; public const UInt32 RESI = RSI; public const UInt32 REDI = RDI; public const UInt32 REBP = RBP; public const UInt32 RESP = RSP; public const UInt32 REIP = RIP; public const UInt32 REFL = RFL;
        public const UInt32 RCR0 = (UInt32)(REGADDRBASE + RCR0OFS);
        public const UInt32 RCR2 = (UInt32)(REGADDRBASE + RCR2OFS);
        public const UInt32 RCR3 = (UInt32)(REGADDRBASE + RCR3OFS);
        public const UInt32 RCR4 = (UInt32)(REGADDRBASE + RCR4OFS);
        public const UInt32 RIDTR = (UInt32)(REGADDRBASE + RIDTROFS);
        public const UInt32 RGDTR = (UInt32)(REGADDRBASE + RGDTROFS);

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
            }
            private UInt32 mCS, mEIP;
            private ProcessorMode mOperatingMode;
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
        }


        // Declare the event using EventHandler<T>
        public event EventHandler<CEAStartDebugging> StartDebugging;
        internal virtual void OnStartDebugging(CEAStartDebugging e)
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

        internal Instruct MOV = new MOV();
        internal Instruct CALL = new CALL();
        internal Instruct ADD = new ADD();
        internal Instruct PUSH = new PUSH();
        internal Instruct INC = new INC();
        internal Instruct POP = new POP();
        internal Instruct JMP = new JMP();
        internal Instruct CMP = new CMP();
        internal Instruct CMPXCHG = new CMPXCHG();
        internal Instruct AND = new AND();
        internal Instruct JA = new JA();
        internal Instruct JB = new JB();
        internal Instruct BSF = new BSF();
        internal Instruct BSR = new BSR();
        internal Instruct BSWAP = new BSWAP();
        internal Instruct BT = new BT();
        internal Instruct BTC = new BTC();
        internal Instruct BTR = new BTR();
        internal Instruct BTS = new BTS();
        internal Instruct CALLF = new CALLF();
        internal Instruct CBW = new CBW();
        internal Instruct CLC = new CLC();
        internal Instruct CLD = new CLD();
        internal Instruct CLI = new CLI();
        internal Instruct CLTS = new CLTS();
        internal Instruct CMC = new CMC();
        internal Instruct CMOVcc = new CMOVcc();
        internal Instruct CMPSB = new CMPSB();
        internal Instruct CMPSD = new CMPSD();
        internal Instruct CMPSW = new CMPSW();
        internal Instruct CPUID = new CPUID();
        internal Instruct CWD = new CWD();
        internal Instruct DAA = new DAA();
        internal Instruct DAS = new DAS();
        internal Instruct DEC = new DEC();
        internal Instruct DIV = new DIV();
        internal Instruct ENTER = new ENTER();
        internal Instruct FCLEX = new FCLEX();
        internal Instruct FDIVR = new FDIVR();
        internal Instruct FINIT = new FINIT();
        internal Instruct FILD = new FILD();
        internal Instruct FIST = new FIST();
        internal Instruct FLDCW = new FLDCW();
        internal Instruct FLD1 = new FLD1();
        internal Instruct FLDZ = new FLDZ();
        internal Instruct FMUL = new FMUL();
        internal Instruct FSTENV = new FSTENV();
        internal Instruct FSTCW = new FSTCW();
        internal Instruct FSTSW = new FSTSW();
        internal Instruct HLT = new HLT();
        internal Instruct ICEBP = new ICEBP();
        internal Instruct IDIV = new IDIV();
        internal Instruct IMUL = new IMUL();
        internal Instruct IN = new IN();
        internal Instruct INS = new INS();
        internal Instruct INT = new INT();
        internal Instruct INTO = new INTO();
        internal Instruct IRET = new IRET();
        internal Instruct JAE = new JAE();
        internal Instruct JBE = new JBE();
        internal Instruct JC = new JC();
        internal Instruct JCXZ = new JCXZ();
        internal Instruct JE = new JE();
        internal Instruct JG = new JG();
        internal Instruct JGE = new JGE();
        internal Instruct JL = new JL();
        internal Instruct JLE = new JLE();
        internal Instruct JMPF = new JMPF();
        internal Instruct JNA = new JNA();
        internal Instruct JNAE = new JNAE();
        internal Instruct JNB = new JNB();
        internal Instruct JNBE = new JNBE();
        internal Instruct JNC = new JNC();
        internal Instruct JNE = new JNE();
        internal Instruct JNG = new JNG();
        internal Instruct JNGE = new JNGE();
        internal Instruct JNLE = new JNLE();
        internal Instruct JNO = new JNO();
        internal Instruct JNP = new JNP();
        internal Instruct JNS = new JNS();
        internal Instruct JNZ = new JNZ();
        internal Instruct JO = new JO();
        internal Instruct JP = new JP();
        internal Instruct JPE = new JPE();
        internal Instruct JPO = new JPO();
        internal Instruct JS = new JS();
        internal Instruct JZ = new JZ();
        internal Instruct LAHF = new LAHF();
        internal Instruct LDS = new LDS();
        internal Instruct LEA = new LEA();
        internal Instruct LEAVE = new LEAVE();
        internal Instruct LES = new LES();
        internal Instruct LFS = new LFS();
        internal Instruct LGS = new LGS();
        internal Instruct LIDT = new LIDT();
        internal Instruct LLDT = new LLDT();
        internal Instruct LGDT = new LGDT();
        internal Instruct LMSW = new LMSW();
        internal Instruct LTR = new LTR();
        /*internal Instruct LOCK = new LOCK(),this);*/
        internal Instruct LODSB = new LODSB();
        internal Instruct LODSD = new LODSD();
        internal Instruct LODSW = new LODSW();
        internal Instruct LOOP = new LOOP();
        internal Instruct LOOPE = new LOOPE();
        internal Instruct LOOPNE = new LOOPNE();
        internal Instruct LOOPNZ = new LOOPNZ();
        internal Instruct LOOPZ = new LOOPZ();
        internal Instruct LSS = new LSS();
        internal Instruct MOVSX = new MOVSX();
        internal Instruct MOVZX = new MOVZX();
        internal Instruct MOVSB = new MOVSB();
        internal Instruct MOVSD = new MOVSD();
        internal Instruct MOVSW = new MOVSW();
        internal Instruct MUL = new MUL();
        internal Instruct NEG = new NEG();
        internal Instruct NOP = new NOP();
        internal Instruct NOT = new NOT();
        internal Instruct OR = new OR();
        internal Instruct OUT = new OUT();
        internal Instruct OUTS = new OUTS();
        internal Instruct POPA = new POPA();
        internal Instruct POPF = new POPF();
        internal Instruct PUSHA = new PUSHA();
        internal Instruct PUSHAD = new PUSHAD();
        internal Instruct PUSHF = new PUSHF();
        internal Instruct RCL = new RCL();
        internal Instruct RCR = new RCR();
        internal Instruct REP = new REP();
        internal Instruct REPZ = new REPZ();
        internal Instruct REPE = new REPE();
        internal Instruct REPNE = new REPNE();
        internal Instruct REPNZ = new REPNZ();
        internal Instruct RET = new RET();
        internal Instruct RETF = new RETF();
        internal Instruct RDTSC = new RDTSC();
        internal Instruct ROL = new ROL();
        internal Instruct ROR = new ROR();
        internal Instruct SAHF = new SAHF();
        internal Instruct SBB = new SBB();
        internal Instruct SCASB = new SCASB();
        internal Instruct SCASD = new SCASD();
        internal Instruct SCASW = new SCASW();
        internal Instruct SAL = new SAL();
        internal Instruct SALC = new SALC();
        internal Instruct SAR = new SAR();
        internal Instruct SETcc = new SETcc();
        internal Instruct SGDT = new SGDT();
        internal Instruct SHL = new SHL();
        internal Instruct SIDT = new SIDT();
        internal Instruct SHLD = new SHLD();
        internal Instruct SMSW = new SMSW();
        internal Instruct SHR = new SHR();
        internal Instruct SHRD = new SHRD();
        internal Instruct STC = new STC();
        internal Instruct STD = new STD();
        internal Instruct STI = new STI();
        internal Instruct STR = new STR();
        internal Instruct STOSB = new STOSB();
        internal Instruct STOSD = new STOSD();
        internal Instruct STOSW = new STOSW();
        internal Instruct SUB = new SUB();
        internal Instruct TEST = new TEST();
        internal Instruct FWAIT = new FWAIT();
        internal Instruct VERR = new VERR();
        internal Instruct VERW = new VERW();
        internal Instruct XCHG = new XCHG();
        internal Instruct XLAT = new XLAT();
        internal Instruct XLATB = new XLATB();
        internal Instruct XOR = new XOR();
        internal Instruct GRP1 = new GRP1();
        internal Instruct GRP2 = new GRP2();
        internal Instruct GRP3a = new GRP3a();
        internal Instruct GRP3b = new GRP3b();
        internal Instruct GRP4 = new GRP4();
        internal Instruct GRP5 = new GRP5();
        internal Instruct GRP60 = new GRP60();
        internal Instruct GRP61 = new GRP61();
        internal Instruct GRP7 = new GRP7();
        internal Instruct GRP8 = new GRP8();
        internal Instruct GRPC0 = new GRPC0();
        internal Instruct GRPC1 = new GRPC1();
        internal Instruct GRPPUSH = new GRPPUSH();
        internal Instruct AAA = new AAA();
        internal Instruct AAD = new AAD();
        internal Instruct AAM = new AAM();
        internal Instruct AAS = new AAS();
        internal Instruct ADC = new ADC();
        internal Instruct ARPL = new ARPL();
        internal Instruct BOUND = new BOUND();

    }

}