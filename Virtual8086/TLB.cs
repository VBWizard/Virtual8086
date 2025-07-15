using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualProcessor
{
    public class cTLB
    {

        public const int MAX_ENTRIES = 16;

        public struct TLBEntry
        {
            public UInt32 LogicalAddr;
            public UInt32 PhysicalAddr;
            public UInt16 Type;
            public bool Valid;
        }

        private static TLBEntry[] Entries = new TLBEntry[MAX_ENTRIES];
        private static int mCurrEntryPtr = 0;
        public static UInt64 mMisses, mHits, mFlushes;
        public cTLB()
        {
            mCurrEntryPtr = 0;
        }

        public static UInt32 Translate(Processor_80x86 mProc, ref sInstruction sIns, UInt32 inAddr, bool Writing, ePrivLvl pCPL)
        {
            UInt32 lDirEntry = 0, lPageEntry = 0;
            UInt32 LastAddress = 0;
            UInt32 LastReply = 0;

#if PAGING_USE_LAST_PASSED_ADDRESS_LOGIC
            if ((LastAddress & 0xFFFFF000) == (inAddr & 0xFFFFF000))
                return (LastReply & 0xFFFFF000) | (inAddr & 0x00000FFF);
#endif

            for (int i = 0; i < MAX_ENTRIES; i++)
            {
                var entry = Entries[i];
                if (!entry.Valid)
                    continue;
                if (entry.LogicalAddr <= inAddr && entry.LogicalAddr + 0xFFF >= inAddr)
                {
                    if (pCPL == ePrivLvl.App_Level_3)
                    {
                        if (Writing && (entry.Type & Misc.PAGING_PAGE_READ_WRITE_MASK) != Misc.PAGING_PAGE_READ_WRITE_MASK)
                            break;
                        if ((entry.Type & Misc.PAGING_USER_MASK) != Misc.PAGING_USER_MASK)
                            break;
                    }
                    else if (Writing &&
                        ((entry.Type & Misc.PAGING_PAGE_READ_WRITE_MASK) != Misc.PAGING_PAGE_READ_WRITE_MASK) &&
                        (mProc.regs.CR0 & Misc.CR0_WP_BIT_MASK) == Misc.CR0_WP_BIT_MASK &&
                        mProc.mSystem.mCR0_WP_Honor_In_Sup_Mode)
                    {
                        break;
                    }

                    mHits++;
                    return entry.PhysicalAddr | (inAddr & 0x00000FFF);
                }
            }

            // Not found, walk page tables
            UInt32 lPage = PhysicalMem.GetPageTableEntry(mProc, ref sIns, inAddr, ref lDirEntry, ref lPageEntry, Writing);
            if (sIns.ExceptionThrown)
                return 0xF0F0F0F0;

            int entryIndex = mCurrEntryPtr;
            Entries[entryIndex].LogicalAddr = inAddr & 0xFFFFF000;
            Entries[entryIndex].PhysicalAddr = lPage & 0xFFFFF000;
            Entries[entryIndex].Type = (UInt16)(lPage & 0x00000FFF);
            Entries[entryIndex].Valid = true;

            if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                Entries[entryIndex].Type &= 0xFFB;
            if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                Entries[entryIndex].Type &= 0xFFD;

            mCurrEntryPtr = (mCurrEntryPtr + 1) % MAX_ENTRIES;
            mMisses++;

            return Entries[entryIndex].PhysicalAddr | (inAddr & 0x00000FFF);
        }



        /// <summary>
        /// This method is a shallow version of Translate, which will not go to the page tables, but will instead return a fake exception (triggered in the passed sIns) if 
        /// the address is not found in the TLB.  This is necessary because if the GUI calls Translate when it is attempting to access system memoroy, it can trigger paging
        /// exceptions in the executing code which would be bad.
        /// </summary>
        /// <param name="mProc"></param>
        /// <param name="sIns"></param>
        /// <param name="inAddr"></param>
        /// <param name="Writing"></param>
        /// <param name="pCPL"></param>
        /// <returns></returns>
        public UInt32 ShallowTranslate(Processor_80x86 mProc, ref sInstruction sIns, UInt32 inAddr, bool Writing, ePrivLvl pCPL)
        {
            for (int i = 0; i < MAX_ENTRIES; i++)
            {
                var entry = Entries[i];
                if (!entry.Valid)
                    continue;
                if (entry.LogicalAddr <= inAddr && entry.LogicalAddr + 0xFFF >= inAddr)
                {
                    if (pCPL == ePrivLvl.App_Level_3)
                    {
                        if (Writing && (entry.Type & Misc.PAGING_PAGE_READ_WRITE_MASK) != Misc.PAGING_PAGE_READ_WRITE_MASK)
                            break;
                        if ((entry.Type & Misc.PAGING_USER_MASK) != Misc.PAGING_USER_MASK)
                            break;
                    }
                    else if (Writing &&
                        ((entry.Type & Misc.PAGING_PAGE_READ_WRITE_MASK) != Misc.PAGING_PAGE_READ_WRITE_MASK) &&
                        (mProc.regs.CR0 & Misc.CR0_WP_BIT_MASK) == Misc.CR0_WP_BIT_MASK &&
                        mProc.mSystem.mCR0_WP_Honor_In_Sup_Mode)
                    {
                        break;
                    }
                    mHits++;
                    return entry.PhysicalAddr | (inAddr & 0x00000FFF);
                }
            }
            sIns.ExceptionErrorCode = 0xfAfafafa;
            return 0xf1f1f1f1;
        }

        public static void Flush(Processor_80x86 mProc)
        {
            if (mProc.mSystem.Debuggies.DebugMemPaging)
                mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "TLB has been flushing, has " + MAX_ENTRIES + " entries.");

            if ((mProc.regs.CR4 & 0x80) == 0x80)
            {
                for (int i = 0; i < MAX_ENTRIES; i++)
                    if ((Entries[i].Type & 0x100) == 0x100)
                        Entries[i].Valid = false;
            }
            else
            {
                for (int i = 0; i < MAX_ENTRIES; i++)
                    Entries[i].Valid = false;
                mCurrEntryPtr = 0;
            }

            PhysicalMem.mLastAddressWrite = true;
            PhysicalMem.mLastLogicalAddress = 0xFFFFFFFF;
            PhysicalMem.mLastPhysicalAddress = 0xFFFFFFFF;
            mFlushes++;
        }

        internal static void InvalidatePage(Processor_80x86 mProc, UInt32 Address)
        {
            for (int i = 0; i < MAX_ENTRIES; i++)
            {
                var entry = Entries[i];
                if (!entry.Valid)
                    continue;
                if (entry.LogicalAddr <= Address && entry.LogicalAddr + 0xFFF >= Address)
                {
                    Entries[i].Valid = false;
                    if (mProc.mSystem.Debuggies.DebugMemPaging)
                        mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + entry.LogicalAddr.ToString("X8") + " --> physical page " + entry.PhysicalAddr.ToString("X8"));
                    return;
                }
            }
            if (mProc.mSystem.Debuggies.DebugMemPaging)
                mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - no TLB entry found!");
        }

    }
}
