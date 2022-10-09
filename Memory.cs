using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualProcessor.Devices;

namespace VirtualProcessor
{
    public class PhysicalMem
    {
        public byte[] mMemBytes;
        public bool RefreshSuspended = false;
        public bool mUsePaging = true;
        public UInt32[] mChangeAddress = new UInt32[1024 * 1000];
        public byte[] mChanges = new byte[1024 * 1000];
        public long mChangesPending = 0;
#if CALCULATE_PAGE_MEMORY_USAGE
        public byte[] mCHangedBlocks;
#endif
        UInt32 mPagedMemoryAddress, lMemAddr, lMemAddr3;
        internal UInt32 mLastLogicalAddress = 0;
        internal UInt32 mLastPhysicalAddress = 0;
        internal bool mLastAddressWrite = false;
        internal int mPrivCheckSize;
        //// The 'priv_check' array is used to decide if the current access
        //// has the proper paging permissions.  An index is formed, based
        //// on parameters such as the access type and level, the write protect
        //// flag and values cached in the TLB.  The format of the index into this
        //// array is:
        ////
        ////   |4 |3 |2 |1 |0 |
        ////   |wp|us|us|rw|rw|
        ////    |  |  |  |  |
        ////    |  |  |  |  +---> r/w of current access
        ////    |  |  +--+------> u/s,r/w combined of page dir & table (cached)
        ////    |  +------------> u/s of current access
        ////    +---------------> Current CR0.WP value
        static byte[] mPrivCheck;

        public PhysicalMem(Processor_80x86 mProc, UInt32 RAMSize)
        {
            //2k for registers
            mMemBytes = new byte[RAMSize + 2048];
#if CALCULATE_PAGE_MEMORY_USAGE
            mCHangedBlocks = new byte[RAMSize / 4006];
#endif
            //InitializeMem(0x69);
            if (mProc.ProcType <= eProcTypes.i80386)
                mPrivCheck = new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 0, 1, 1 };
            else
                mPrivCheck = new byte[32] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 0, 0, 0, 0, 1, 0, 1, 1 };
       }

        public byte pMemory(Processor_80x86 mProc, UInt32 MemAddr)
        {
            if (MemAddr > Size)
                throw new Exception("CS:IP " + mProc.regs.CS.Value.ToString("X8") + ":" + mProc.regs.EIP.ToString("X8") + " - Attempt to access memory address " + MemAddr.ToString("X") + " which is above the limit of " + mMemBytes.Length);
            else
                return mMemBytes[MemAddr];
        }
        internal void pMemory(Processor_80x86 mProc, UInt32 MemAddr, byte Value)
        {
            //if ((MemAddr >= 0x0008E000) && (MemAddr <= 0x0008E003))
            //{
            //    MemAddr = MemAddr - 23;
            //    MemAddr += 22;
            //    MemAddr += 1;
            //} 
            mMemBytes[MemAddr] = Value;
        }
        internal void pMemory(Processor_80x86 mProc, UInt32 MemAddr, UInt16 Value)
        {
            pMemory(mProc, MemAddr, (byte)(Value));
            pMemory(mProc, MemAddr + 1, (byte)(Value >> 8));
        }
        internal void pMemory(Processor_80x86 mProc, UInt32 MemAddr, UInt32 Value)
        {
            UInt16 lHiWord = Misc.GetHi(Value);
            UInt16 lLoWord = (UInt16)Value;

            pMemory(mProc, MemAddr, (byte)(lLoWord));
            pMemory(mProc, MemAddr + 1, Misc.GetHi(lLoWord));
            pMemory(mProc, MemAddr + 2, (byte)(lHiWord));
            pMemory(mProc, MemAddr + 3, Misc.GetHi(lHiWord));
            //}
        }
        internal void pMemory(Processor_80x86 mProc, UInt32 MemAddr, UInt64 Value)
        {
            pMemory(mProc, MemAddr, (UInt32)Misc.GetLo(Value));
            pMemory(mProc, MemAddr + 4, (UInt32)Misc.GetHi(Value));
        }


        static UInt32 page_fault(Processor_80x86 mProc, ref sInstruction sIns, PagingErrorType fault, UInt32 laddr, byte user, byte rw)
        {
            sIns.ExceptionErrorCode = (UInt32)((byte)fault | (user << 2) | (rw << 1));
            sIns.ExceptionNumber = 0x0e;
            sIns.ExceptionAddress = laddr;
            sIns.ExceptionThrown = true;
#if DEBUG
            mProc.mSystem.PagingExceptionBreakpoint = true;
#endif

            return sIns.ExceptionErrorCode;
            /*#if BX_CPU_LEVEL >= 6
              if (rw == BX_EXECUTE) {
                if (BX_CPU_THIS_PTR cr4.get_SMEP())
                  error_code |= ERROR_CODE_ACCESS; // I/D = 1
                if (BX_CPU_THIS_PTR cr4.get_PAE() && BX_CPU_THIS_PTR efer.get_NXE())
                  error_code |= ERROR_CODE_ACCESS;
              }
            #endif
             * */
        }

        public UInt32 GetPageTableEntry(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Address, ref UInt32 DirEntry, ref UInt32 PageEntry, bool Writing)
        {
            UInt32 mLocalExceptionErrorCode = 0xFF;

            mLocalExceptionErrorCode = 0xff;

            if (Address >= Processor_80x86.REGADDRBASE && (Address <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS) || (mProc.regs.CR0 & 0x80000000) != 0x80000000)
                return Address;

            UInt32 lCR3 = (mProc.regs.CR3 & 0xFFFFF000);
            UInt32 dirEntry, pageEntry, dirIdx;
            UInt32 lMemAddr = Address, lTemp;


            dirIdx = (UInt32)(((Address & 0xFFC00000) >> 22) * 4);
            dirIdx |= lCR3;

            //Get the directory table entry
            dirEntry = (UInt32)(mMemBytes[dirIdx + 3] << 24 | mMemBytes[dirIdx + 2] << 16 | mMemBytes[dirIdx + 1] << 8 | mMemBytes[dirIdx]);
            DirEntry = dirEntry;

            mLocalExceptionErrorCode = 0xff;

            byte user = (byte)((((int)mProc.regs.CPL & 3) == 3) ? 1 : 0);
            byte wp = 0;
            if (mProc.mSystem.mCR0_WP_Honor_In_Sup_Mode)
                wp = (byte)((mProc.regs.CR0 & Misc.CR0_WP_BIT_MASK) >> 0x10);
            byte combined_access = 6;
            combined_access &= (byte)(DirEntry & 6);
            byte isWrite = (byte)(Writing?1:0);
           
            if ((DirEntry&1)!=1)
            {
                mLocalExceptionErrorCode = page_fault(mProc, ref sIns, PagingErrorType.ERROR_NOT_PRESENT, Address, user, isWrite);
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "PAGING directory fault for address: " + Address.ToString("X8") + ", CR3=" + mProc.regs.CR3.ToString("X8") + ", DI=" + dirIdx.ToString("X4") + ", DE=" + dirEntry.ToString("X8") + ", PE=" + PageEntry + ", CPL=" + (int)mProc.regs.CPL + ", during " + mProc.mProcessorStatus + ", errorCode = " + sIns.ExceptionErrorCode.ToString("X2"));
                return 0xF0F0F0F0;
            }

            //31						4 3 2 1 0
            //-------------------------|---------|
            //                         |I R U W P|
            //                         |/ s / / r|
            //                         |O v S R e|
            //-------------------------|---------|
            //P	0 The fault was caused by a non-present page.
            //    1 The fault was caused by a page-level protection violation.
            //W/R	0 The access causing the fault was a read.
            //    1 The access causing the fault was a write.
            //US	0 The access causing the fault originated when the processor was executing in supervisor mode.
            //    1 The access causing the fault originated when the processor was executing in user mode.
            //RSVD 	0 The fault was not caused by reserved bit violation.
            //    1 The fault was caused by reserved bits set to 1 in a page directory.
            //I/D 	0 The fault was not caused by an instruction fetch.
            //    1 The fault was caused by an instruction fetch.


            lTemp = ((Address & 0x3FF000) >> 12) * 4;
            pageEntry = (UInt32)((DirEntry & 0xFFFFF000) | lTemp);
            PageEntry = (UInt32)(mMemBytes[pageEntry + 3] << 24 | mMemBytes[pageEntry + 2] << 16 | mMemBytes[pageEntry + 1] << 8 | mMemBytes[pageEntry]);

            if ((PageEntry & 1) != 1)
                mLocalExceptionErrorCode = page_fault(mProc, ref sIns, PagingErrorType.ERROR_NOT_PRESENT, Address, user, isWrite);
            else
            {
                combined_access &= (byte)(PageEntry & 6);
                byte priv_index = (byte)((wp << 4) |   // bit 4
                                (user << 3) |                             // bit 3
                                (combined_access | isWrite));            // bit 2,1,0
                if (mPrivCheck[priv_index] == 0)
                    mLocalExceptionErrorCode = page_fault(mProc, ref sIns, PagingErrorType.ERROR_PROTECTION, Address, user, isWrite);

                if (mLocalExceptionErrorCode != 0xFF && (combined_access & 0x6) == 0x6)
                    System.Diagnostics.Debugger.Break();
            }

            if (mLocalExceptionErrorCode != 0xFF)
            {
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "PAGING page fault for address: " + Address.ToString("X8") + ", CR3=" + mProc.regs.CR3.ToString("X8") + ", DI=" + dirIdx.ToString("X4") + ", DE=" + dirEntry.ToString("X8") + ", PI= " + pageEntry.ToString("X8") + ", PE=" + PageEntry.ToString("X8") + ", CPL=" + (int)mProc.regs.CPL + " - during " + mProc.mProcessorStatus + ", errorCode=" + sIns.ExceptionErrorCode.ToString("X2"));
                return 0xF0F0F0F0;
            }

            //Update PDE and PTE
            DirEntry |= (1 << 5);
            //pMemory(mProc, dirIdx, (UInt32)(DirEntry | (1 << 5)));
            mChangeAddress[mChangesPending] = dirIdx;
            mChanges[mChangesPending++] = (byte)dirEntry;
            mChangeAddress[mChangesPending] = dirIdx + 1;
            mChanges[mChangesPending++] = (byte)(dirEntry >> 8);
            mChangeAddress[mChangesPending] = dirIdx + 2;
            mChanges[mChangesPending++] = (byte)(dirEntry >> 16);
            mChangeAddress[mChangesPending] = dirIdx + 3;
            mChanges[mChangesPending++] = (byte)(dirEntry >> 24);

            
            PageEntry |= (1 << 5);
            if (Writing)
                PageEntry |= (1 << 6);
            //pMemory(mProc, pageEntry, (UInt32)(PageEntry));
            mChangeAddress[mChangesPending] = pageEntry;
            mChanges[mChangesPending++] = (byte)PageEntry;
            mChangeAddress[mChangesPending] = pageEntry+1;
            mChanges[mChangesPending++] = (byte)(PageEntry >> 8);
            mChangeAddress[mChangesPending] = pageEntry + 2;
            mChanges[mChangesPending++] = (byte)(PageEntry >> 16);
            mChangeAddress[mChangesPending] = pageEntry + 3;
            mChanges[mChangesPending++] = (byte)(PageEntry >> 24);

            
            if ((PageEntry & 0xFFFFF000) + (Address & 0x00000FFF) > mProc.mem.Size)
            {
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "PagedMemoryAddress - Invalid memory address:  " + (PageEntry + (Address & 0xFFF)).ToString("X8"));
                return mProc.mem.Size;
            }
            return PageEntry;
        }

        public UInt32 PagedMemoryAddress(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Address, bool Writing)
        {
            UInt32 lResult;

            if ((Address >= Processor_80x86.REGADDRBASE && (Address <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS)) || (mProc.regs.CR0 & 0x80000000) != 0x80000000)
                return Address;
            //if ((Address & 0xFFFFF000) == mLastLogicalAddress && (mLastAddressWrite || !Writing))
            if ((Address & 0xFFFFF000) == mLastLogicalAddress && (Writing == mLastAddressWrite || !Writing && mLastAddressWrite))
                return (mLastPhysicalAddress) | (Address & 0x00000FFF);
            lResult = mProc.mTLB.Translate(mProc, ref sIns, Address, Writing, mProc.regs.CPL);

            if (sIns.ExceptionThrown)
                return 0xF0f0f0f0;
            mLastLogicalAddress = Address & 0xFFFFF000;
            mLastPhysicalAddress = lResult & 0xFFFFF000;
            mLastAddressWrite = Writing;
            return lResult;
        }

        public bool PageAccessWillCausePF(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Address, bool Writing)
        {
            UInt32 lResult;

            ////Assume the address is a DWord
            //Address += 3;

            if ((Address >= Processor_80x86.REGADDRBASE && (Address <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS)) || (mProc.regs.CR0 & 0x80000000) != 0x80000000)
                return false;
            if ((Address & 0xFFFFF000) == mLastLogicalAddress && (Writing == mLastAddressWrite || !Writing && mLastAddressWrite))
                return false;
            lResult = mProc.mTLB.Translate(mProc, ref sIns, Address, Writing, mProc.regs.CPL);
            if (sIns.ExceptionThrown)
            {
                mLastLogicalAddress = Address & 0xFFFFF000;
                mLastPhysicalAddress = lResult & 0xFFFFF000;
                mLastAddressWrite = Writing;
            }
            return sIns.ExceptionThrown;
        }

        public void Rollback()
        {
            mChangesPending = 0;
        }

#if CALCULATE_PAGE_MEMORY_USAGE
        public void BlockChanged(UInt32 Address, bool Writing)
        {

                UInt32 block = Address / 4096;
            if (Writing)
            {
                if (mCHangedBlocks[block] == 2)
                    mCHangedBlocks[block] = 1;
                else
                    mCHangedBlocks[block] = 2;
            }
            else
            {
                if (mCHangedBlocks[block] == 4)
                    mCHangedBlocks[block] = 3;
                else
                    mCHangedBlocks[block] = 4;
            }
        }
#endif
        public void Commit()
        {
            for (int cnt = 0; cnt < mChangesPending; cnt++)
            {
                mMemBytes[mChangeAddress[cnt]] = mChanges[cnt];
#if CALCULATE_PAGE_MEMORY_USAGE
                 BlockChanged (mChangeAddress[cnt],true);
#endif
            }
            mChangesPending = 0;
        }

        private byte Memory(Processor_80x86 mProc, ref sInstruction sIns, UInt32 MemAddr)
        {
            bool mMHHit = false;
            byte mMHValue = 0;

            if (sIns.ExceptionThrown) return 0;
            if ((mProc.regs.CR0 & 0x80000000) == 0x80000000 /*&& mProc.regs.CS.DescriptorNum > 0*/) //07/13/2013 - removed, caused FreeDOS to fail
             {
                //CLR 06/10/2013 - DO we really need this?
                //if ((MemAddr >= 0xf0000 && MemAddr <= 0xfffff))
                //    return mMemBytes[MemAddr];
                mPagedMemoryAddress = PagedMemoryAddress(mProc, ref sIns, MemAddr, false);
                if (sIns.ExceptionThrown)
                    return 0xF3;
                if (MHCount > 0 && mPagedMemoryAddress >= MHMinAddress && mPagedMemoryAddress <= MHMaxAddress)
                {
                    mMHValue = CallHandlerIfHit(mPagedMemoryAddress, eDataDirection.Mem_Read, 0, ref mMHHit);
                    if (mMHHit)
                        return mMHValue;
                }
#if CALCULATE_PAGE_MEMORY_USAGE
                BlockChanged(lMemAddr, false);
#endif
                return mMemBytes[mPagedMemoryAddress];
            }
            else
            {
                if (MemAddr >= mMemBytes.Length)
                    return 0x0;
#if CALCULATE_PAGE_MEMORY_USAGE
                BlockChanged(MemAddr, false);
#endif
                if (MHCount > 0 && MemAddr >= MHMinAddress && MemAddr <= MHMaxAddress)
                {
                    mMHValue = CallHandlerIfHit(MemAddr, eDataDirection.Mem_Read, 0, ref mMHHit);
                    if (mMHHit)
                        return mMHValue;
                }
                //If we get here there was no memory handler hit
                // throw new Exception("CS:IP " + mProc.regs.CS.Value.ToString("X8") + ":" + mProc.regs.EIP.ToString("X8") + " - Attempt to access memory address " + MemAddr.ToString("X") + " which is above the limit of " + mMemBytes.Length);
                return mMemBytes[MemAddr];
            }
        }
        private void Memory(Processor_80x86 mProc, ref sInstruction sIns, UInt32 MemAddr, byte Value)
        {
            bool mMHHit = false;

            if (MemAddr == 0x217E)
            {
                MemAddr = MemAddr - 23;
                MemAddr += 22;
                MemAddr += 1;
            }

            if (sIns.ExceptionThrown) return;
            //If paging enabled
            if ((mProc.regs.CR0 & 0x80000000) == 0x80000000)
            {
                UInt32 mPME = PagedMemoryAddress(mProc, ref sIns, MemAddr, true);
                if (sIns.ExceptionThrown)
                    return;
                if (mPME >= mMemBytes.Length)
                    throw new Exception("CS:IP " + mProc.regs.CS.Value.ToString("X8") + ":" + mProc.regs.EIP.ToString("X8") + " - Attempt to access memory address " + MemAddr.ToString("X") + " which is above the limit of " + mMemBytes.Length);
                if ((mPME >= 0xC0000 && mPME <= 0xCAFFF) || (mPME >= 0xF0000 && mPME <= 0xFFFFF))
                    return;
                //Check for a memory handler for the address.  If so let the handler have the value
                if (MHCount > 0 && mPME >= MHMinAddress && mPME <= MHMaxAddress) 
                {
                    CallHandlerIfHit(mPME, eDataDirection.Mem_Write, Value, ref mMHHit);
                    if (mMHHit)
                        return;
                }
                //If we get here, there was no memory handler hit
                mChangeAddress[mChangesPending] = mPME;
                mChanges[mChangesPending++] = Value;
            }
            else
            {
                if ((MemAddr >= 0xC0000 && MemAddr <= 0xCAFFF) || (MemAddr >= 0xF0000 && MemAddr <= 0xFFFFF))
                    return;
#if CALCULATE_PAGE_MEMORY_USAGE
                BlockChanged(MemAddr, true);
#endif
                if (MHCount > 0 && MemAddr >= MHMinAddress && MemAddr <= MHMaxAddress)
                {
                    CallHandlerIfHit(MemAddr, eDataDirection.Mem_Write, Value, ref mMHHit);
                    if (mMHHit)
                        return;
                }
                //If we get here, there was no memory handler hit
                mMemBytes[MemAddr] = Value;
            }
        }
        private void Memory(Processor_80x86 mProc, ref sInstruction sIns, UInt32 MemAddr, UInt16 Value)
        {
            Memory(mProc, ref sIns, MemAddr, (byte)(Value));
            if (sIns.ExceptionThrown) return;
            Memory(mProc, ref sIns, MemAddr + 1, (byte)(Value >> 8));
        }
        private void Memory(Processor_80x86 mProc, ref sInstruction sIns, UInt32 MemAddr, UInt32 Value)
        {

#if MEM_PERF_ENHANCEMENTS
            this.SetDWordP(mProc, ref sIns, MemAddr, Value);
            return;
#else
            UInt16 lHiWord = Misc.GetHi(Value);
            UInt16 lLoWord = (UInt16)Value;
            UInt32 lExceptionErrorCode = 0;

            Memory(mProc, ref sIns, MemAddr, (byte)(lLoWord));
            if (sIns.ExceptionThrown) return;
            Memory(mProc, ref sIns, MemAddr + 1, Misc.GetHi(lLoWord));
            Memory(mProc, ref sIns, MemAddr + 2, (byte)(lHiWord));
            Memory(mProc, ref sIns, MemAddr + 3, Misc.GetHi(lHiWord));
#endif
        }
        private void Memory(Processor_80x86 mProc, ref sInstruction sIns, UInt32 MemAddr, UInt64 Value)
        {
            Memory(mProc, ref sIns, MemAddr, (UInt32)Misc.GetLo(Value));
            Memory(mProc, ref sIns, MemAddr + 4, (UInt32)Misc.GetHi(Value));
        }
        public UInt32 Size
        {
            get { return (UInt32)mMemBytes.Length; }
        }
        /// <summary>
        /// Initializes all of memory to a given value.  Note: Ignores memory mapped ports
        /// </summary>
        /// <param name="ValueToInit">Value to initialize each byte to</param>
        public void InitializeMem(byte ValueToInit)
        {
            for (int loc = 0; loc < 0xb8000; loc++)
                mMemBytes[loc] = ValueToInit;
            for (int loc = 0xb9000; loc <mMemBytes.Length - 1; loc++)
                mMemBytes[loc] = ValueToInit;
        }
        public void InitializeVideoMem()
        {
            for (int cnt = 0xB8FA0- 1; cnt >= 0xb8000; cnt -= 2)
            //for (int cnt = 0xb8000; cnt < 0xb8000 + 0x200; cnt += 2)
            {
                mMemBytes[cnt] = 0x20;
                mMemBytes[cnt + 1] = 0x07;
            }
        }
        public static UInt32 GetLocForSegOfs(Processor_80x86 mProc, ref sSegmentSelector WhichSegReg, UInt32 Offset)
        {
            //CLR 06/26/2013 - Added 2nd condition incase just switched to protected mode and haven't initialized segments with prot mode sel's yet
            if (mProc.OperatingMode == ProcessorMode.Protected)
                if (WhichSegReg.Selector.Number > 0)
                    //Changed to simply Segment + offset to Segment << 4 + offset (protected mode)
                    return WhichSegReg.Selector.Base + Offset;
                //Segment:Offset calc = Shift Segment Left by 4 and add offset to it
                return (UInt32)((WhichSegReg.mRealModeValue << 4) + Offset);
        }
        public static UInt32 GetLocForSegOfs(Processor_80x86 mProc, UInt32 Segment, UInt32 Offset)
        {
            if (mProc.OperatingMode == ProcessorMode.Protected)
                //Changed to simply Segment + offset to Segment << 4 + offset (protected mode)
                return Segment + Offset;
            else
                //Segment:Offset calc = Shift Segment Left by 4 and add offset to it
                return (UInt32)((Segment << 4) + Offset);
        }
        public static UInt16 GetSegForLoc(Processor_80x86 mProc, UInt32 MemLocation)
        {
            return (UInt16)(MemLocation >> 16);
        }
        public static UInt16 GetSegForLoc(Processor_80x86 mProc, UInt64 MemLocation)
        {
            return (UInt16)(MemLocation >> 32);
        }
        public static UInt32 GetOfsForLoc(Processor_80x86 proc, UInt32 MemLocation)
        {
            //if (proc.ProtectedModeActive)
            //    return MemLocation;
            //else
            return (UInt16)(MemLocation & 0x0FFFF);
        }

        #region Memory access methods
        public byte GetByte(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Location)
        {
            if (Location >= Processor_80x86.REGADDRBASE && Location <= Processor_80x86.REGADDRBASE + Processor_80x86.RDHOFS)
                return GetByteRegisterValue(mProc, Location);
            else
                return Memory(mProc, ref sIns, Location);
        }
        public void SetByte(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Location, byte Value)
        {
            if (Location >= Processor_80x86.REGADDRBASE && Location <= Processor_80x86.REGADDRBASE + Processor_80x86.RDHOFS)
            {
                SetByteRegisterValue(mProc, Location, Value);
                return;
            }
            else
                Memory(mProc, ref sIns, Location, Value);
            if (sIns.ExceptionThrown) return;
            if (mProc.mCurrInstructOpMode != ProcessorMode.Real)
                RefreshIfRequired(mProc, ref sIns, Location, (Byte)Value, TypeCode.Byte);
        }
        public UInt16 GetWord(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Location)
        {
            UInt16 lRetVal;
            if (Location >= Processor_80x86.REGADDRBASE && (Location <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS))
                return GetWordRegisterValue(mProc, Location);
            else
            {
                lRetVal = (UInt16)(Memory(mProc, ref sIns, Location + 1) << 8);
                if (sIns.ExceptionThrown) return 0xF0F0;
                return (UInt16)(lRetVal | (UInt16)(Memory(mProc, ref sIns, Location)));
            }
        }
        public void SetWord(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Location, UInt16 Value)
        {
            if (Location >= Processor_80x86.REGADDRBASE && (Location <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS))
            {
                SetWordRegisterValue(mProc, Location, Value);
                return;
            }
            else
                Memory(mProc, ref sIns, Location, Value);
            if (sIns.ExceptionThrown) return;
            if (mProc.mCurrInstructOpMode != ProcessorMode.Real)
                RefreshIfRequired(mProc, ref sIns, Location, Value, TypeCode.UInt16);
        }
        public UInt32 GetDWord(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Location)
        {
            UInt32 lRetVal;
            if (sIns.ExceptionThrown) return 0;
            if (Location >= Processor_80x86.REGADDRBASE && Location <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS)
                return GetDWordRegisterValue(mProc, Location);
            else
#if MEM_PERF_ENHANCEMENTS
                if ((Location & 0xFFF) >= 0xFFD)
#endif
                {
                    //Retrieving a DWord from the requested location will cross a page boundary, so do it the old fashioned slow way
                    //so that we get each byte from the proper page.
                    lRetVal = (UInt32)(Memory(mProc, ref sIns, Location + 3) << 24);
                    if (sIns.ExceptionThrown) return 0xF0F0F0F0;
                    lRetVal |= (UInt32)(Memory(mProc, ref sIns, Location + 2) << 16);
                    lRetVal |= (UInt32)(Memory(mProc, ref sIns, Location + 1) << 8);
                    return lRetVal | Memory(mProc, ref sIns, Location);
                }
#if MEM_PERF_ENHANCEMENTS
                else
                    return GetDWordP(mProc, ref sIns, Location);
#endif
        }
#if MEM_PERF_ENHANCEMENTS
        public UInt32 GetDWordP(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Location)
        {
            UInt32 lRetVal;

            if ((mProc.regs.CR0 & 0x80000000) == 0x80000000 /*&& mProc.regs.CS.DescriptorNum > 0*/) //07/13/2013 - removed, caused FreeDOS to fail
            {
                lMemAddr3 = PagedMemoryAddress(mProc, ref sIns, Location, false);
                if (sIns.ExceptionThrown)
                    return 0xF0F0F0F0;
            }
            else
                lMemAddr3 = Location;

            lRetVal = (UInt32)(mMemBytes[lMemAddr3 + 3] << 24);
            if (sIns.ExceptionThrown) return 0xF0F0F0F0;
            lRetVal |= (UInt32)(mMemBytes[lMemAddr3 + 2] << 16);
            lRetVal |= (UInt32)(mMemBytes[lMemAddr3 + 1] << 8);
            return lRetVal | (UInt32)mMemBytes[lMemAddr3];
        }
#endif
        public void SetDWord(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Location, UInt32 Value)
        {
            if (sIns.ExceptionThrown) return;
            if (Location >= Processor_80x86.REGADDRBASE && Location <= Processor_80x86.REGADDRBASE + Processor_80x86.RSSOFS)
            {
                SetDWordRegisterValue(mProc, Location, Value);
                return;
            }
            else
#if MEM_PERF_ENHANCEMENTS
                if ((Location & 0xFFF) >= 0xFFD)
                    //Setting a DWord at the requested location will cross a page boundary, so do it the old fashioned slow way
                    //so that we get each byte from the proper page.
#endif
                Memory(mProc, ref sIns, Location, Value);
#if MEM_PERF_ENHANCEMENTS
                else
                    SetDWordP(mProc, ref sIns, Location, Value);
#endif
            if (sIns.ExceptionThrown) return;
            if (mProc.mCurrInstructOpMode != ProcessorMode.Real)
                RefreshIfRequired(mProc, ref sIns, Location, Value, TypeCode.UInt32);
        }
        #if MEM_PERF_ENHANCEMENTS
        public void SetDWordP(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Location, UInt32 Value)
        {
            UInt32 lMemAddr3 = 0;
            if ((mProc.regs.CR0 & 0x80000000) == 0x80000000 /*&& mProc.regs.CS.DescriptorNum > 0*/) //07/13/2013 - removed, caused FreeDOS to fail
            {
                lMemAddr3 = PagedMemoryAddress(mProc, ref sIns, Location, true);
                if (sIns.ExceptionThrown)
                    return;
                for (UInt32 loc=lMemAddr3;loc<=lMemAddr3+3;loc++)
                {
                    mChangeAddress[mChangesPending] = loc;
                    mChanges[mChangesPending++] = (byte)(Value & 0xFF); Value >>= 8; 
                }
                return;
            }
            lMemAddr3 = Location;
            mMemBytes[lMemAddr3++] = (byte)(Value & 0xFF); Value >>= 8;
            mMemBytes[lMemAddr3++] = (byte)(Value & 0xFF); Value >>= 8;
            mMemBytes[lMemAddr3++] = (byte)(Value & 0xFF); Value >>= 8;
            mMemBytes[lMemAddr3++] = (byte)(Value & 0xFF);
       
        }
#endif
        public UInt64 GetQWord(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Location)
        {
            UInt16 LoLoWord = (UInt16)(Misc.SetHi((UInt16)0, Memory(mProc, ref sIns, Location + 1)) + Memory(mProc, ref sIns, Location));
            if (sIns.ExceptionThrown) return 0xF0F0F0F0;
            UInt16 LoHiWord = (UInt16)(Misc.SetHi((UInt16)0, Memory(mProc, ref sIns, Location + 3)) + Memory(mProc, ref sIns, Location + 2));
            if (sIns.ExceptionThrown) return 0xF0F0F0F0;
            UInt16 HiLoWord = (UInt16)(Misc.SetHi((UInt16)0, Memory(mProc, ref sIns, Location + 5)) + Memory(mProc, ref sIns, Location + 4));
            if (sIns.ExceptionThrown) return 0xF0F0F0F0;
            UInt16 HiHiWord = (UInt16)(Misc.SetHi((UInt16)0, Memory(mProc, ref sIns, Location + 7)) + Memory(mProc, ref sIns, Location + 6));
            UInt32 HiDWord = (UInt32)((HiHiWord << 16) + HiLoWord);
            UInt32 LoDWord = (UInt32)((LoHiWord << 16) + LoLoWord);
            return Misc.SetLo(Misc.SetHi((UInt32)0, HiDWord), LoDWord);
        }
        public void SetQWord(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Location, UInt64 Value)
        {
            Memory(mProc, ref sIns, Location, Value);
            if (mProc.mCurrInstructOpMode != ProcessorMode.Real)
                RefreshIfRequired(mProc, ref sIns, Location, Value, TypeCode.UInt32);
        }


        public void RefreshIfRequired(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Location, Object Value, TypeCode ValueType)
        {
            UInt32 lLoc = PagedMemoryAddress(mProc, ref sIns, Location, false);

            if (sIns.ExceptionThrown)
                return;

            if (RefreshSuspended || mProc.mCurrInstructOpMode != ProcessorMode.Protected)
                return;

            //GDT
            if (mProc.regs.GDTR.Value != 0 && lLoc >= mProc.regs.GDTR.mBase
            && lLoc <= (mProc.regs.GDTR.mBase) + mProc.regs.GDTR.Limit)
            {
                UInt32 lBase = mProc.regs.GDTR.mBase;
                switch (ValueType)
                {
                    case TypeCode.Byte:
                        if (mProc.regs.GDTR.lCache.Length >= lLoc - lBase)
                            if (mProc.regs.GDTR.lCache[lLoc - lBase] != (Byte)Value)
                                mProc.RefreshGDTCache();
                        break;
                    case TypeCode.UInt16:
                        if (mProc.regs.GDTR.lCache.Length >= lLoc - lBase + 1)
                            if ((mProc.regs.GDTR.lCache[lLoc - lBase + 1] << 8) + mProc.regs.GDTR.lCache[lLoc - lBase] != (UInt16)Value)
                                mProc.RefreshGDTCache();
                        break;
                    case TypeCode.UInt32:
                        if (mProc.regs.GDTR.lCache.Length >= lLoc - lBase + 3)
                            if ((mProc.regs.GDTR.lCache[lLoc - lBase + 3] << 24) + (mProc.regs.GDTR.lCache[lLoc - lBase + 2] << 16) + (mProc.regs.GDTR.lCache[lLoc - lBase + 1] << 8) + mProc.regs.GDTR.lCache[lLoc - lBase] != (UInt32)Value)
                                mProc.RefreshGDTCache();
                        break;
                }
            }

            //LDT
            if (mProc.regs.LDTR.Value != 0 && lLoc >= mProc.regs.LDTR.mBase
            && lLoc <= (mProc.regs.LDTR.mBase + mProc.regs.LDTR.Limit))
            {
                UInt32 lBase = mProc.regs.LDTR.mBase;
                switch (ValueType)
                {
                    case TypeCode.Byte:
                        if (mProc.regs.LDTR.lCache.Length >= lLoc - lBase)
                            if (mProc.regs.LDTR.lCache[lLoc - lBase] != (Byte)Value)
                                mProc.RefreshLDTCache();
                        break;
                    case TypeCode.UInt16:
                        if (mProc.regs.LDTR.lCache.Length >= lLoc - lBase + 1)
                            if ((mProc.regs.LDTR.lCache[lLoc - lBase + 1] << 8) + mProc.regs.LDTR.lCache[lLoc - lBase] != (UInt16)Value)
                                mProc.RefreshLDTCache();
                        break;
                    case TypeCode.UInt32:
                        if (mProc.regs.LDTR.lCache.Length >= lLoc - lBase + 3)
                            if ((mProc.regs.LDTR.lCache[lLoc - lBase + 3] << 24) + (mProc.regs.LDTR.lCache[lLoc - lBase + 2] << 16) + (mProc.regs.LDTR.lCache[lLoc - lBase + 1] << 8) + mProc.regs.LDTR.lCache[lLoc - lBase] != (UInt32)Value)
                                mProc.RefreshLDTCache();
                        break;
                }
            }

            //TSS
            //if (mProc.regs.TR.lCache.Length > 0)
            //{
            //    if (lLoc >= mProc.regs.TR.mBase
            //      && lLoc <= (mProc.regs.TR.mBase + 104 /*mProc.regs.TR.Limit*/)
            //        && mProc.regs.TR.lCache.Length > 0)
            //        switch (ValueType)
            //        {
            //            case TypeCode.Byte:
            //                if (mProc.regs.TR.lCache[lLoc - mProc.regs.TR.Base] != (Byte)Value)
            //                    mProc.mCurrTSS = new sTSS(mProc.mem.ChunkPhysical(mProc, 0, mProc.regs.TR.mBase, 104),
            //                        mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].access.SystemDescType == eSystemOrGateDescType.TSS_32_Av || mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].access.SystemDescType == eSystemOrGateDescType.TSS_32_Bu);
            //                break;
            //            case TypeCode.UInt16:
            //                if ((mProc.regs.TR.lCache[lLoc - mProc.regs.TR.Base + 1] << 8) + mProc.regs.TR.lCache[lLoc - mProc.regs.TR.Base] != (UInt16)Value)
            //                    mProc.mCurrTSS = new sTSS(mProc.mem.ChunkPhysical(mProc, 0, mProc.regs.TR.mBase, 104),
            //                        mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].access.SystemDescType == eSystemOrGateDescType.TSS_32_Av || mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].access.SystemDescType == eSystemOrGateDescType.TSS_32_Bu);
            //                break;
            //            case TypeCode.UInt32:
            //                if ((mProc.regs.TR.lCache[lLoc - mProc.regs.TR.Base + 3] << 24) + (mProc.regs.TR.lCache[lLoc - mProc.regs.TR.Base + 2] << 16) + (mProc.regs.TR.lCache[lLoc - mProc.regs.TR.Base + 1] << 8) + mProc.regs.TR.lCache[lLoc - mProc.regs.TR.Base] != (UInt32)Value)
            //                    mProc.mCurrTSS = new sTSS(mProc.mem.ChunkPhysical(mProc, 0, mProc.regs.TR.mBase, 104),
            //                        mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].access.SystemDescType == eSystemOrGateDescType.TSS_32_Av || mProc.mGDTCache[mProc.regs.TR.mSegSel >> 3].access.SystemDescType == eSystemOrGateDescType.TSS_32_Bu);
            //                break;
            //        }
            //}

            //IDT
            if (mProc.regs.IDTR.Value != 0 && lLoc >= mProc.regs.IDTR.mBase
            && lLoc <= (mProc.regs.IDTR.mBase + mProc.regs.IDTR.Limit))
            {
                UInt32 lBase = mProc.regs.IDTR.mBase;
                switch (ValueType)
                {
                    case TypeCode.Byte:
                        if (mProc.regs.IDTR.lCache.Length >= lLoc - lBase)
                            if (mProc.regs.IDTR.lCache[lLoc - lBase] != (Byte)Value)
                                mProc.RefreshIDTCache();
                        break;
                    case TypeCode.UInt16:
                        if (mProc.regs.IDTR.lCache.Length >= lLoc - lBase + 1)
                            if ((mProc.regs.IDTR.lCache[lLoc - lBase + 1] << 8) + mProc.regs.IDTR.lCache[lLoc - lBase] != (UInt16)Value)
                                mProc.RefreshIDTCache();
                        break;
                    case TypeCode.UInt32:
                        if (mProc.regs.IDTR.lCache.Length >= lLoc - lBase + 3)
                            if ((mProc.regs.IDTR.lCache[lLoc - lBase + 3] << 24) + (mProc.regs.IDTR.lCache[lLoc - lBase + 2] << 16) + (mProc.regs.IDTR.lCache[lLoc - lBase + 1] << 8) + mProc.regs.IDTR.lCache[lLoc - lBase] != (UInt32)Value)
                                mProc.RefreshIDTCache();
                        break;
                }
            }
        }

        public byte[] Chunk(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Segment, UInt32 Offset, UInt32 NumberOfBytes)
        {
            UInt32 lLocation = GetLocForSegOfs(mProc, Segment, Offset);
            byte[] lTemp = new byte[NumberOfBytes];

            for (int cnt = 0; cnt < NumberOfBytes; cnt++)
                lTemp[cnt] = GetByte(mProc, ref sIns, lLocation++);  //proc.mem.mMemBytes[lLocation++]; // GetByte(proc, lLocation++);
            return lTemp;
        }
        public byte[] ChunkPhysical(Processor_80x86 proc, UInt32 Segment, UInt32 Offset, UInt32 NumberOfBytes)
        {
            UInt32 lLocation = GetLocForSegOfs(proc, Segment, Offset);
            byte[] lTemp = new byte[NumberOfBytes];

            for (int cnt = 0; cnt < NumberOfBytes; cnt++)
                lTemp[cnt] = mMemBytes[lLocation++];//GetByte(proc, lLocation++);  //proc.mem.mMemBytes[lLocation++]; // GetByte(proc, lLocation++);
            return lTemp;
        }
        public UInt64[] QWChunk(Processor_80x86 mProc, ref sInstruction sIns, UInt32 Offset, UInt32 NumberOfEntries)
        {
            //Hackarooni ... to get around trinux booting problem
            //Offset = Offset & 0x00FFFFFF;

            UInt64[] lTemp = new UInt64[NumberOfEntries + 1];
            for (int entry = 0; entry <= NumberOfEntries; entry++)
            {
                lTemp[entry] = GetByte(mProc, ref sIns, Offset + 7);
                lTemp[entry] = (lTemp[entry] << 8) + GetByte(mProc, ref sIns, Offset + 6);  //mProc.mem.mMemBytes[Offset + 6]; //GetByte(mProc, Offset + 6);
                lTemp[entry] = (lTemp[entry] << 8) + GetByte(mProc, ref sIns, Offset + 5);
                lTemp[entry] = (lTemp[entry] << 8) + GetByte(mProc, ref sIns, Offset + 4);
                lTemp[entry] = (lTemp[entry] << 8) + GetByte(mProc, ref sIns, Offset + 3);
                lTemp[entry] = (lTemp[entry] << 8) + GetByte(mProc, ref sIns, Offset + 2);
                lTemp[entry] = (lTemp[entry] << 8) + GetByte(mProc, ref sIns, Offset + 1);
                lTemp[entry] = (lTemp[entry] << 8) + GetByte(mProc, ref sIns, Offset);
                Offset += 8;
            }
            return lTemp;
        }
        public UInt64[] QWChunkPhysical(byte[] mem, UInt32 NumberOfEntries)
        {
            int lOffset = 0;
            UInt64[] lTemp = new UInt64[NumberOfEntries + 1];
            for (int entry = 0; entry < NumberOfEntries; entry++)
            {
                lTemp[entry] = mem[lOffset + 7];
                lTemp[entry] = (lTemp[entry] << 8) + mem[lOffset + 6];
                lTemp[entry] = (lTemp[entry] << 8) + mem[lOffset + 5];
                lTemp[entry] = (lTemp[entry] << 8) + mem[lOffset + 4];
                lTemp[entry] = (lTemp[entry] << 8) + mem[lOffset + 3];
                lTemp[entry] = (lTemp[entry] << 8) + mem[lOffset + 2];
                lTemp[entry] = (lTemp[entry] << 8) + mem[lOffset + 1];
                lTemp[entry] = (lTemp[entry] << 8) + mem[lOffset];
                lOffset += 8;
            }
            return lTemp;
        }

        public byte GetByteRegisterValue(Processor_80x86 mProc, UInt32 Location)
        {
            switch (Location)
            {
                case Processor_80x86.RAH:
                    return mProc.regs.AH;
                    break;
                case Processor_80x86.RAL:
                    return mProc.regs.AL;
                    break;
                case Processor_80x86.RBH:
                    return mProc.regs.BH;
                    break;
                case Processor_80x86.RBL:
                    return mProc.regs.BL;
                    break;
                case Processor_80x86.RCH:
                    return mProc.regs.CH;
                    break;
                case Processor_80x86.RCLr:
                    return mProc.regs.CL;
                    break;
                case Processor_80x86.RDH:
                    return mProc.regs.DH;
                    break;
                case Processor_80x86.RDL:
                    return mProc.regs.DL;
                    break;
                default:
                    throw new Exception("GetByteRegisterValue: Unidentified byte register!");
                    break;
            }
            //if (Location == Processor_80x86.RAH)
            //    return mProc.regs.AH;
            //else if (Location == Processor_80x86.RAL)
            //    return mProc.regs.AL;
            //else if (Location == Processor_80x86.RCH)
            //    return mProc.regs.CH;
            //else if (Location == Processor_80x86.RCLr)
            //    return mProc.regs.CL;
            //else if (Location == Processor_80x86.RDH)
            //    return mProc.regs.DH;
            //else if (Location == Processor_80x86.RDL)
            //    return mProc.regs.DL;
            //else if (Location == Processor_80x86.RBH)
            //    return mProc.regs.BH;
            //else if (Location == Processor_80x86.RBL)
            //    return mProc.regs.BL;
            //else
            //    throw new Exception("GetByteRegisterValue: Unidentified byte register!");
        }
        public void SetByteRegisterValue(Processor_80x86 mProc, UInt32 Location, byte Value)
        {
            switch (Location)
            {
                case Processor_80x86.RAH:
                    mProc.regs.AH = Value;
                    break;
                case Processor_80x86.RAL:
                    mProc.regs.AL = Value;
                    break;
                case Processor_80x86.RBH:
                    mProc.regs.BH = Value;
                    break;
                case Processor_80x86.RBL:
                    mProc.regs.BL = Value;
                    break;
                case Processor_80x86.RCH:
                    mProc.regs.CH = Value;
                    break;
                case Processor_80x86.RCLr:
                    mProc.regs.CL = Value;
                    break;
                case Processor_80x86.RDH:
                    mProc.regs.DH = Value;
                    break;
                case Processor_80x86.RDL:
                    mProc.regs.DL = Value;
                    break;
                default:
                    throw new Exception("GetByteRegisterValue: Unidentified byte register!");
                    break;
            }
            //if (Location == Processor_80x86.RAH)
            //    mProc.regs.AH = Value;
            //else if (Location == Processor_80x86.RAL)
            //    mProc.regs.AL = Value;
            //else if (Location == Processor_80x86.RCH)
            //    mProc.regs.CH = Value;
            //else if (Location == Processor_80x86.RCLr)
            //    mProc.regs.CL = Value;
            //else if (Location == Processor_80x86.RDH)
            //    mProc.regs.DH = Value;
            //else if (Location == Processor_80x86.RDL)
            //    mProc.regs.DL = Value;
            //else if (Location == Processor_80x86.RBH)
            //    mProc.regs.BH = Value;
            //else if (Location == Processor_80x86.RBL)
            //    mProc.regs.BL = Value;
            //else
            //    throw new Exception("GetByteRegisterValue: Unidentified byte register!");
        }
        public UInt16 GetWordRegisterValue(Processor_80x86 mProc, UInt32 Location)
        {
            switch (Location)
            {
                case Processor_80x86.RAX:
                    return mProc.regs.AX;
                    break;
                case Processor_80x86.RBX:
                    return mProc.regs.BX;
                    break;
                case Processor_80x86.RCX:
                    return mProc.regs.CX;
                    break;
                case Processor_80x86.RDX:
                    return mProc.regs.DX;
                    break;
                case Processor_80x86.RSI:
                    return mProc.regs.SI;
                    break;
                case Processor_80x86.RDI:
                    return mProc.regs.DI;
                    break;
                case Processor_80x86.RBP:
                    return mProc.regs.BP;
                    break;
                case Processor_80x86.RSP:
                    return mProc.regs.SP;
                    break;
                case Processor_80x86.RIP:
                    return mProc.regs.IP;
                    break;
                case Processor_80x86.RFL:
                    return mProc.regs.FLAGS;
                    break;
                case Processor_80x86.RCS:
                    return (UInt16)mProc.regs.CS.Value;
                    break;
                case Processor_80x86.RDS:
                    return (UInt16)mProc.regs.DS.Value;
                    break;
                case Processor_80x86.RES:
                    return (UInt16)mProc.regs.ES.Value;
                    break;
                case Processor_80x86.RFS:
                    return (UInt16)mProc.regs.FS.Value;
                    break;
                case Processor_80x86.RGS:
                    return (UInt16)mProc.regs.GS.Value;
                    break;
                case Processor_80x86.RSS:
                    return (UInt16)mProc.regs.SS.Value;
                    break;
                default:
                    throw new Exception("GetWordRegisterValue: Unidentified Word register at location " + Location.ToString("X8"));
            }
            //if (Location == Processor_80x86.RAX)
            //    return mProc.regs.AX;
            //else if (Location == Processor_80x86.RCX)
            //    return mProc.regs.CX;
            //else if (Location == Processor_80x86.RDX)
            //    return mProc.regs.DX;
            //else if (Location == Processor_80x86.RSI)
            //    return mProc.regs.SI;
            //else if (Location == Processor_80x86.RDI)
            //    return mProc.regs.DI;
            //else if (Location == Processor_80x86.RBX)
            //    return mProc.regs.BX;
            //else if (Location == Processor_80x86.RBP)
            //    return mProc.regs.BP;
            //else if (Location == Processor_80x86.RSP)
            //    return mProc.regs.SP;
            //else if (Location == Processor_80x86.RIP)
            //    return mProc.regs.IP;
            //else if (Location == Processor_80x86.RFL)
            //    return mProc.regs.FLAGS;
            //else if (Location == mProc.regs.CS.MemAddr)
            //    return (UInt16)mProc.regs.CS.Value;
            //else if (Location == mProc.regs.DS.MemAddr)
            //    return (UInt16)mProc.regs.DS.Value;
            //else if (Location == mProc.regs.ES.MemAddr)
            //    return (UInt16)mProc.regs.ES.Value;
            //else if (Location == mProc.regs.FS.MemAddr)
            //    return (UInt16)mProc.regs.FS.Value;
            //else if (Location == mProc.regs.GS.MemAddr)
            //    return (UInt16)mProc.regs.GS.Value;
            //else if (Location == mProc.regs.SS.MemAddr)
            //    return (UInt16)mProc.regs.SS.Value;
            //else
            //    throw new Exception("GetWordRegisterValue: Unidentified Word register at location " + Location.ToString("X8"));
        }
        public void SetWordRegisterValue(Processor_80x86 mProc, UInt32 Location, UInt16 Value)
        {
            switch (Location)
            {
                case Processor_80x86.RAX:
                    mProc.regs.AX = Value;
                    break;
                case Processor_80x86.RBX:
                    mProc.regs.BX = Value;
                    break;
                case Processor_80x86.RCX:
                    mProc.regs.CX = Value;
                    break;
                case Processor_80x86.RDX:
                    mProc.regs.DX = Value;
                    break;
                case Processor_80x86.RSI:
                    mProc.regs.SI = Value;
                    break;
                case Processor_80x86.RDI:
                    mProc.regs.DI = Value;
                    break;
                case Processor_80x86.RBP:
                    mProc.regs.BP = Value;
                    break;
                case Processor_80x86.RSP:
                    mProc.regs.SP = Value;
                    break;
                case Processor_80x86.RIP:
                    mProc.regs.IP = Value;
                    break;
                case Processor_80x86.RFL:
                    mProc.regs.FLAGS = Value;
                    break;
                case Processor_80x86.RCS:
                    mProc.regs.CS.Value = Value;
                    break;
                case Processor_80x86.RDS:
                    mProc.regs.DS.Value = Value;
                    break;
                case Processor_80x86.RES:
                    mProc.regs.ES.Value = Value;
                    break;
                case Processor_80x86.RFS:
                    mProc.regs.FS.Value = Value;
                    break;
                case Processor_80x86.RGS:
                    mProc.regs.GS.Value = Value;
                    break;
                case Processor_80x86.RSS:
                    mProc.regs.SS.Value = Value;
                    break;
                default:
                    throw new Exception("GetWordRegisterValue: Unidentified Word register!");
            }
            //if (Location == Processor_80x86.RAX)
            //    mProc.regs.AX = Value;
            //else if (Location == Processor_80x86.RBX)
            //    mProc.regs.BX = Value;
            //else if (Location == Processor_80x86.RCX)
            //    mProc.regs.CX = Value;
            //else if (Location == Processor_80x86.RDX)
            //    mProc.regs.DX = Value;
            //else if (Location == Processor_80x86.RSI)
            //    mProc.regs.SI = Value;
            //else if (Location == Processor_80x86.RDI)
            //    mProc.regs.DI = Value;
            //else if (Location == Processor_80x86.RBP)
            //    mProc.regs.BP = Value;
            //else if (Location == Processor_80x86.RSP)
            //    mProc.regs.SP = Value;
            //else if (Location == Processor_80x86.RIP)
            //    mProc.regs.IP = Value;
            //else if (Location == Processor_80x86.RFL)
            //    mProc.regs.FLAGS = Value;
            //else if (Location == mProc.regs.CS.MemAddr)
            //    mProc.regs.CS.Value = Value;
            //else if (Location == mProc.regs.DS.MemAddr)
            //    mProc.regs.DS.Value = Value;
            //else if (Location == mProc.regs.ES.MemAddr)
            //    mProc.regs.ES.Value = Value;
            //else if (Location == mProc.regs.FS.MemAddr)
            //    mProc.regs.FS.Value = Value;
            //else if (Location == mProc.regs.GS.MemAddr)
            //    mProc.regs.GS.Value = Value;
            //else if (Location == mProc.regs.SS.MemAddr)
            //    mProc.regs.SS.Value = Value;
            //else
            //    throw new Exception("GetWordRegisterValue: Unidentified Word register!");
        }
        public UInt32 GetDWordRegisterValue(Processor_80x86 mProc, UInt32 Location)
        {
            switch (Location)
            {
                case Processor_80x86.REAX:
                    return mProc.regs.EAX;
                case Processor_80x86.REBX:
                    return mProc.regs.EBX;
                case Processor_80x86.RECX:
                    return mProc.regs.ECX;
                case Processor_80x86.REDX:
                    return mProc.regs.EDX;
                case Processor_80x86.RESI:
                    return mProc.regs.ESI;
                case Processor_80x86.REDI:
                    return mProc.regs.EDI;
                case Processor_80x86.REBP:
                    return mProc.regs.EBP;
                case Processor_80x86.RESP:
                    return mProc.regs.ESP;
                case Processor_80x86.REIP:
                    return mProc.regs.EIP;
                case Processor_80x86.REFL:
                    return mProc.regs.EFLAGS;
                case Processor_80x86.RCS:
                    return mProc.regs.CS.Value;
                case Processor_80x86.RDS:
                    return mProc.regs.DS.Value;
                case Processor_80x86.RES:
                    return mProc.regs.ES.Value;
                case Processor_80x86.RFS:
                    return mProc.regs.FS.Value;
                case Processor_80x86.RGS:
                    return mProc.regs.GS.Value;
                case Processor_80x86.RSS:
                    return mProc.regs.SS.Value;
                case Processor_80x86.RCR0:
                    return mProc.regs.CR0;
                case Processor_80x86.RCR2:
                    return mProc.regs.CR2;
                case Processor_80x86.RCR3:
                    return mProc.regs.CR3;
                case Processor_80x86.RCR4:
                    return mProc.regs.CR4;
                case Processor_80x86.RDR7:
                    return mProc.regs.DR7;
                default:
                    throw new Exception("GetDWordRegisterValue: Unidentified DWord register!" + Location.ToString("X8"));
            }
            //if (Location == Processor_80x86.REAX)
            //    return mProc.regs.EAX;
            //else if (Location == Processor_80x86.REBX)
            //    return mProc.regs.EBX;
            //else if (Location == Processor_80x86.RECX)
            //    return mProc.regs.ECX;
            //else if (Location == Processor_80x86.REDX)
            //    return mProc.regs.EDX;
            //else if (Location == Processor_80x86.RESI)
            //    return mProc.regs.ESI;
            //else if (Location == Processor_80x86.REDI)
            //    return mProc.regs.EDI;
            //else if (Location == Processor_80x86.REBP)
            //    return mProc.regs.EBP;
            //else if (Location == Processor_80x86.RESP)
            //    return mProc.regs.ESP;
            //else if (Location == Processor_80x86.REIP)
            //    return mProc.regs.EIP;
            //else if (Location == Processor_80x86.REFL)
            //    return mProc.regs.EFLAGS;
            //else if (Location == mProc.regs.CS.MemAddr)
            //    return mProc.regs.CS.Value;
            //else if (Location == mProc.regs.DS.MemAddr)
            //    return mProc.regs.DS.Value;
            //else if (Location == mProc.regs.ES.MemAddr)
            //    return mProc.regs.ES.Value;
            //else if (Location == mProc.regs.FS.MemAddr)
            //    return mProc.regs.FS.Value;
            //else if (Location == mProc.regs.GS.MemAddr)
            //    return mProc.regs.GS.Value;
            //else if (Location == mProc.regs.SS.MemAddr)
            //    return mProc.regs.SS.Value;
            //else if (Location == Processor_80x86.RCR0)
            //    return mProc.regs.CR0;
            //else if (Location == Processor_80x86.RCR2)
            //    return mProc.regs.CR2;
            //else if (Location == Processor_80x86.RCR3)
            //    return mProc.regs.CR3;
            //else if (Location == Processor_80x86.RCR4)
            //    return mProc.regs.CR4;
            //else if (Location == Processor_80x86.RDR7)
            //    return mProc.regs.DR7;
            //else
            //    throw new Exception("GetDWordRegisterValue: Unidentified DWord register!" + Location.ToString("X8"));
        }
        public void SetDWordRegisterValue(Processor_80x86 mProc, UInt32 Location, UInt32 Value)
        {

            switch (Location)
            {
                case Processor_80x86.REAX:
                    mProc.regs.EAX = Value;
                    break;
                case Processor_80x86.REBX:
                    mProc.regs.EBX = Value;
                    break;
                case Processor_80x86.RECX:
                    mProc.regs.ECX = Value;
                    break;
                case Processor_80x86.REDX:
                    mProc.regs.EDX = Value;
                    break;
                case Processor_80x86.RESI:
                    mProc.regs.ESI = Value;
                    break;
                case Processor_80x86.REDI:
                    mProc.regs.EDI = Value;
                    break;
                case Processor_80x86.REBP:
                    mProc.regs.EBP = Value;
                    break;
                case Processor_80x86.RESP:
                    mProc.regs.ESP = Value;
                    break;
                case Processor_80x86.REIP:
                    mProc.regs.EIP = Value;
                    break;
                case Processor_80x86.REFL:
                    mProc.regs.EFLAGS = Value;
                    break;
                case Processor_80x86.RCS:
                    mProc.regs.CS.Value = Value;
                    break;
                case Processor_80x86.RDS:
                    mProc.regs.DS.Value = Value;
                    break;
                case Processor_80x86.RES:
                    mProc.regs.ES.Value = Value;
                    break;
                case Processor_80x86.RFS:
                    mProc.regs.FS.Value = Value;
                    break;
                case Processor_80x86.RGS:
                    mProc.regs.GS.Value = Value;
                    break;
                case Processor_80x86.RSS:
                    mProc.regs.SS.Value = Value;
                    break;
                case Processor_80x86.RCR0:
                    mProc.regs.CR0 = Value;
                    break;
                case Processor_80x86.RCR2:
                    mProc.regs.CR2 = Value;
                    break;
                case Processor_80x86.RCR3:
                    mProc.regs.CR3 = Value;
                    break;
                case Processor_80x86.RCR4:
                    mProc.regs.CR4 = Value;
                    break;
                case Processor_80x86.RDR0:
                    mProc.regs.DR0 = Value;
                    break;
                case Processor_80x86.RDR1:
                    mProc.regs.DR1 = Value;
                    break;
                case Processor_80x86.RDR2:
                    mProc.regs.DR2 = Value;
                    break;
                case Processor_80x86.RDR3:
                    mProc.regs.DR3 = Value;
                    break;
                case Processor_80x86.RDR6:
                    mProc.regs.DR6 = Value;
                    break;
                case Processor_80x86.RDR7:
                    mProc.regs.DR7 = Value;
                    break;
                default:
                    throw new Exception("SetDWordRegisterValue: Unidentified DWord register = " + Location);
            }
            //if (Location == Processor_80x86.REAX)
            //    mProc.regs.EAX = Value;
            //else if (Location == Processor_80x86.REBX)
            //    mProc.regs.EBX = Value;
            //else if (Location == Processor_80x86.RECX)
            //    mProc.regs.ECX = Value;
            //else if (Location == Processor_80x86.REDX)
            //    mProc.regs.EDX = Value;
            //else if (Location == Processor_80x86.RESI)
            //    mProc.regs.ESI = Value;
            //else if (Location == Processor_80x86.REDI)
            //    mProc.regs.EDI = Value;
            //else if (Location == Processor_80x86.REBP)
            //    mProc.regs.EBP = Value;
            //else if (Location == Processor_80x86.RESP)
            //    mProc.regs.ESP = Value;
            //else if (Location == Processor_80x86.REIP)
            //    mProc.regs.EIP = Value;
            //else if (Location == Processor_80x86.REFL)
            //    mProc.regs.EFLAGS = Value;
            //else if (Location == mProc.regs.CS.MemAddr)
            //    mProc.regs.CS.Value = Value;
            //else if (Location == mProc.regs.DS.MemAddr)
            //    mProc.regs.DS.Value = Value;
            //else if (Location == mProc.regs.ES.MemAddr)
            //    mProc.regs.ES.Value = Value;
            //else if (Location == mProc.regs.FS.MemAddr)
            //    mProc.regs.FS.Value = Value;
            //else if (Location == mProc.regs.GS.MemAddr)
            //    mProc.regs.GS.Value = Value;
            //else if (Location == mProc.regs.SS.MemAddr)
            //    mProc.regs.SS.Value = Value;
            //else if (Location == Processor_80x86.RCR0)
            //    mProc.regs.CR0 = Value;
            //else if (Location == Processor_80x86.RCR2)
            //    mProc.regs.CR2 = Value;
            //else if (Location == Processor_80x86.RCR3)
            //{
            //    mProc.regs.CR3 = Value;
            //    mProc.mTLB.Flush(mProc);
            //}
            //else if (Location == Processor_80x86.RCR4)
            //    mProc.regs.CR4 = Value;
            //else if (Location == Processor_80x86.RDR7)
            //    mProc.regs.DR7 = Value;
            //else
            //    throw new Exception("SetDWordRegisterValue: Unidentified DWord register!");
        }
        #endregion

        #region Memory handler related
        public void RegisterMemoryHandler(UInt32 StartAddr, UInt32 EndAddr, Devices.eDataDirection Direction, Delegate Method)
        {

        }
        internal delegate byte MemoryHandlerDelegate(UInt32 Address, eDataDirection Direction, byte Value);
        internal struct sMemoryHandler
        {
            internal UInt32 mStartAddress, mEndAddress;
            internal MemoryHandlerDelegate mMethodToCall;
            internal Devices.eDataDirection mDirection;
            public sMemoryHandler(UInt32 StartAddress, UInt32 EndAddress, eDataDirection Direction, MemoryHandlerDelegate Method)
            {
                mStartAddress = StartAddress;
                mEndAddress = EndAddress;
                mMethodToCall = Method;
                mDirection = Direction;
            }
        }
        internal int MHCount;
        internal UInt32[] MHStart = new UInt32[1];
        internal UInt32[] MHEnd = new UInt32[1];
        internal eDataDirection[] MHDir = new eDataDirection[1];
        internal MemoryHandlerDelegate[] MHDelegate = new MemoryHandlerDelegate[1];
        internal UInt32 MHMinAddress = 0xFFFFFFFF, MHMaxAddress = 0;
        internal void AddMemHandler(sMemoryHandler Handler)
        {
            int Index = MHCount;
            bool lInsterted = false;
            if (Index > 0)
            {
                Array.Resize(ref MHStart, MHCount + 1);
                Array.Resize(ref MHEnd, MHCount + 1);
                Array.Resize(ref MHDir, MHCount + 1);
                Array.Resize(ref MHDelegate, MHCount + 1);
            }

            MHStart[MHCount] = Handler.mStartAddress;
            MHEnd[MHCount] = Handler.mEndAddress;
            MHDir[MHCount] = Handler.mDirection;
            MHDelegate[MHCount++] = Handler.mMethodToCall;

            if (Handler.mStartAddress < MHMinAddress)
                MHMinAddress = Handler.mStartAddress;
            if (Handler.mEndAddress > MHMaxAddress)
                MHMaxAddress = Handler.mEndAddress;
        }

        internal byte CallHandlerIfHit(UInt32 MemAddr, eDataDirection Direction, byte Value, ref bool Hit)
        {
            Hit = false;
            if (MemAddr < MHMinAddress || MemAddr > MHMaxAddress) return 0;

            for (int cnt = 0; cnt < MHCount; cnt++)
            {
                if (MHStart[cnt] < MemAddr && MHEnd[cnt] > MemAddr && (MHDir[cnt] == Direction || MHDir[cnt] == eDataDirection.Mem_ReadWrite))
                    return MHDelegate[cnt](MemAddr, Direction, Value);
            }
            return 0;
        }
        #endregion
        #region Removed
        //internal class cMemoryHandlerCollection : System.Collections.CollectionBase
        //{
        //    public void Add(sMemoryHandler Handler)
        //    {
        //        List.Add(Handler);
        //    }
        //    public void Remove(int Index)
        //    {
        //        if (Index > Count - 1 || Index < 0)
        //            throw new Exception("Memory handler collection does not contain the referenced index to be removed (" + Index);
        //        else
        //            List.RemoveAt(Index);
        //    }
        //    public sMemoryHandler Item(int Index)
        //    {
        //        return (sMemoryHandler)List[Index];
        //    }
        //    public Delegate CallHandlerIfHit(UInt32 MemAddr, eDataDirection Direction, UInt32 Value, TypeCode Type)
        //    {
        //        for (int cnt = 0; cnt < Count; cnt++)
        //        {
        //            sMemoryHandler lCurr = (sMemoryHandler)List[cnt];
        //            if (lCurr.mStartAddress <= MemAddr && lCurr.mEndAddress >= MemAddr && (lCurr.mDirection == lCurr.mDirection || lCurr.mDirection == eDataDirection.Mem_ReadWrite))
        //            {
        //                lCurr.mMethodToCall(MemAddr, Direction, Value);
        //            }
        //        }
        //        return null;
        //    }
        //}
        #endregion

    }
}