using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualProcessor
{
    public class cTLB
    {
        const int MAX_ENTRIES = 5000;

        public UInt32[] mLogicalAddr, mPhysAddr;
        public UInt16[] mType;
        public int mCurrEntries = 0;
        public UInt64 mMisses, mHits, mFlushes;
        private static object mSyncRoot = new object();
        public cTLB()
        {
            mLogicalAddr = new UInt32[MAX_ENTRIES];
            mPhysAddr = new UInt32[MAX_ENTRIES];
            mType = new UInt16[MAX_ENTRIES];
        }

        public UInt32 Translate(Processor_80x86 mProc, ref sInstruction sIns, UInt32 inAddr, bool Writing, ePrivLvl pCPL)
        {
            UInt32 lPage;
            int lEntry = 0;
            UInt32 lDirEntry = 0, lPageEntry = 0;
            UInt32 lTemp;

#if PAGING_USE_LAST_PASSED_ADDRESS_LOGIC
            if ((LastAddress & 0xFFFFF000) == (inAddr & 0xFFFFF000))
                return (LastReply & 0xFFFFF000) | (inAddr & 0x00000FFF);
#endif
            for (int cnt = 0; cnt < mCurrEntries; cnt++)
            {
                lTemp = mLogicalAddr[cnt];
                if (lTemp <= inAddr && lTemp + 0xFFF >= inAddr)
                {
                    if (pCPL == ePrivLvl.App_Level_3)
                    {
                        if (pCPL == ePrivLvl.App_Level_3 && Writing && (mType[cnt] & Misc.PAGING_PAGE_READ_WRITE_MASK) != Misc.PAGING_PAGE_READ_WRITE_MASK)
                        {
                            {
                                break;  //Make the code look up the entry again to see if it has changed.
                            }
                        }
                        else if ((mType[cnt] & Misc.PAGING_USER_MASK) != Misc.PAGING_USER_MASK)
                        {
                            break; //07/19/2013 - making this also do a look-up again
                        }
                    }
                    //Can't use this right now because kernel code changes paging entry from r/o to rw but we miss it here
                    else if (Writing &&
                    (((mType[cnt] & Misc.PAGING_PAGE_READ_WRITE_MASK) != Misc.PAGING_PAGE_READ_WRITE_MASK)
                        || ((mType[cnt] & Misc.PAGING_PAGE_READ_WRITE_MASK) != Misc.PAGING_PAGE_READ_WRITE_MASK))
                    && (mProc.regs.CR0 & Misc.CR0_WP_BIT_MASK) == Misc.CR0_WP_BIT_MASK
                    && mProc.mSystem.mCR0_WP_Honor_In_Sup_Mode)
                    {
                        break;  //Make the code look up the entry again to see if it has changed.
                    }
                    mHits++;
                    return mPhysAddr[cnt] | (inAddr & 0x00000FFF);
                }
            }
            //If we got here, we know there's no existing entry
            lPage = mProc.mem.GetPageTableEntry(mProc, ref sIns, inAddr, ref lDirEntry, ref lPageEntry, Writing);
            if (sIns.ExceptionThrown)
                return 0xF0F0F0F0;

            if (mCurrEntries + 1 < MAX_ENTRIES)
            {
                lEntry = mCurrEntries++;
            }
            else
            {
                //We have too many entries so get rid of the oldest (smallest #)
                //This is slow but it should hardly if EVER happen
                for (int c = 1; c < MAX_ENTRIES; c++)
                {
                    mPhysAddr[c - 1] = mPhysAddr[c];
                    mLogicalAddr[c - 1] = mLogicalAddr[c];
                    mType[c - 1] = mType[c];
                }
                lEntry = mCurrEntries;
            }
            mLogicalAddr[lEntry] = inAddr & 0xFFFFF000;
            mPhysAddr[lEntry] = lPage & 0xFFFFF000;
            mType[lEntry] = (UInt16)(lPage & 0x00000FFF);
            // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
            // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
            if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                mType[lEntry] &= 0xFFB;
            if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                mType[lEntry] &= 0xFFD;
            mMisses++;

            return mPhysAddr[lEntry] | (inAddr & 0x00000FFF);
        }

        public UInt32 ShallowTranslate(Processor_80x86 mProc, ref sInstruction sIns, UInt32 inAddr, bool Writing, ePrivLvl pCPL)
        {
            UInt32 lTemp;
            for (int cnt = 0; cnt < mCurrEntries; cnt++)
            {
                lTemp = mLogicalAddr[cnt];
                if (lTemp <= inAddr && lTemp + 0xFFF >= inAddr)
                {
                    if (pCPL == ePrivLvl.App_Level_3)
                    {
                        if (pCPL == ePrivLvl.App_Level_3 && Writing && (mType[cnt] & Misc.PAGING_PAGE_READ_WRITE_MASK) != Misc.PAGING_PAGE_READ_WRITE_MASK)
                        {
                            {
                                break;  //Make the code look up the entry again to see if it has changed.
                            }
                        }
                        else if ((mType[cnt] & Misc.PAGING_USER_MASK) != Misc.PAGING_USER_MASK)
                        {
                            break; //07/19/2013 - making this also do a look-up again
                        }
                    }
                    //Can't use this right now because kernel code changes paging entry from r/o to rw but we miss it here
                    else if (Writing &&
                    (((mType[cnt] & Misc.PAGING_PAGE_READ_WRITE_MASK) != Misc.PAGING_PAGE_READ_WRITE_MASK)
                        || ((mType[cnt] & Misc.PAGING_PAGE_READ_WRITE_MASK) != Misc.PAGING_PAGE_READ_WRITE_MASK))
                    && (mProc.regs.CR0 & Misc.CR0_WP_BIT_MASK) == Misc.CR0_WP_BIT_MASK
                    && mProc.mSystem.mCR0_WP_Honor_In_Sup_Mode)
                    {
                        break;  //Make the code look up the entry again to see if it has changed.
                    }
                    mHits++;
                    return mPhysAddr[cnt] | (inAddr & 0x00000FFF);
                }
            }
            sIns.ExceptionErrorCode = 0xfAfafafa;
            return 0xf1f1f1f1;
        }

        public void Flush(Processor_80x86 mProc)
        {
            if (mProc.mSystem.Debuggies.DebugMemPaging)
                mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "TLB has been flushing, has " + mCurrEntries + " entries.");
            //if the PGE (Page Global Enable) flag of CR4 is set, honor global pages
            if ((mProc.regs.CR4 & 0x80) == 0x80)
            {
                UInt16[] lIndexes = new UInt16[MAX_ENTRIES];
                //TODO: Add global page handling
                for (int c = 0; c < MAX_ENTRIES; c++)
                    if ((mType[c] & 0x100) == 0x100)
                        lIndexes[c] = 1;
                    else
                        lIndexes[c] = 0;

                mCurrEntries = 0;
                for (int c = 0; c < MAX_ENTRIES; c++)
                    if (lIndexes[c] == 1)
                    {
                        mLogicalAddr[mCurrEntries] = mLogicalAddr[c];
                        mPhysAddr[mCurrEntries] = mLogicalAddr[c];
                        mType[mCurrEntries++] = mType[c];
                    }
                    else
                    {
                        mLogicalAddr[c] = 0;
                        mPhysAddr[c] = 0;
                        mType[c] = 0;
                    }
            }
            else
            {
                //mLogicalAddr = new uint[MAX_ENTRIES];
                //mPhysAddr = new uint[MAX_ENTRIES];
                //mType = new ushort[MAX_ENTRIES];
                //mProc.mem.mLastAddressWrite = false;
                //mProc.mem.mLastLogicalAddress = 0xFFFFFFFF;
                //mProc.mem.mLastPhysicalAddress = 0xFFFFFFFF;
            }
            mCurrEntries = 0;
            mFlushes++;
        }

        internal void InvalidatePage(Processor_80x86 mProc, UInt32 Address)
        {
            for (int cnt = 0; cnt < mCurrEntries; cnt++)
                if (mLogicalAddr[cnt] <= Address && mLogicalAddr[cnt] + 0xFFF >= Address)
                {
                    if (mProc.mSystem.Debuggies.DebugMemPaging)
                        mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - TLB Entry = logical page " + mLogicalAddr[cnt].ToString("X8") + " --> " + mPhysAddr[cnt].ToString("X8"));
                    mLogicalAddr[cnt] = 0xFFFFFEEE;   //This is how we will invalidate the page
                    return;
                }
            if (mProc.mSystem.Debuggies.DebugMemPaging)
                mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - no TLB entry found!");
        }

    }
}
