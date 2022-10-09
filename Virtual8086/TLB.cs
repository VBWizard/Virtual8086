using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualProcessor
{
    public class cTLB
    {

        public const int MAX_ENTRIES = 16;

        public UInt32 mLogicalAddr1, mLogicalAddr2, mLogicalAddr3, mLogicalAddr4, mLogicalAddr5, mLogicalAddr6, mLogicalAddr7, mLogicalAddr8, mLogicalAddr9, mLogicalAddr10,
            mLogicalAddr11, mLogicalAddr12, mLogicalAddr13, mLogicalAddr14, mLogicalAddr15, mLogicalAddr16, mLogicalAddr17, mLogicalAddr18, mLogicalAddr19, mLogicalAddr20,
            mLogicalAddr21, mLogicalAddr22, mLogicalAddr23, mLogicalAddr24, mLogicalAddr25, mLogicalAddr26, mLogicalAddr27, mLogicalAddr28, mLogicalAddr29, mLogicalAddr30,
            mLogicalAddr31, mLogicalAddr32, mLogicalAddr33, mLogicalAddr34, mLogicalAddr35, mLogicalAddr36;
        public UInt32 mPhysicalAddr1, mPhysicalAddr2, mPhysicalAddr3, mPhysicalAddr4, mPhysicalAddr5, mPhysicalAddr6, mPhysicalAddr7, mPhysicalAddr8, mPhysicalAddr9, mPhysicalAddr10,
            mPhysicalAddr11, mPhysicalAddr12, mPhysicalAddr13, mPhysicalAddr14, mPhysicalAddr15, mPhysicalAddr16, mPhysicalAddr17, mPhysicalAddr18, mPhysicalAddr19, mPhysicalAddr20,
            mPhysicalAddr21, mPhysicalAddr22, mPhysicalAddr23, mPhysicalAddr24, mPhysicalAddr25, mPhysicalAddr26, mPhysicalAddr27, mPhysicalAddr28, mPhysicalAddr29, mPhysicalAddr30,
            mPhysicalAddr31, mPhysicalAddr32, mPhysicalAddr33, mPhysicalAddr34, mPhysicalAddr35, mPhysicalAddr36;
        public UInt16 mType1, mType2, mType3, mType4, mType5, mType6, mType7, mType8, mType9, mType10,
            mType11, mType12, mType13, mType14, mType15, mType16, mType17, mType18, mType19, mType20,
            mType21, mType22, mType23, mType24, mType25, mType26, mType27, mType28, mType29, mType30,
            mType31, mType32, mType33, mType34, mType35, mType36;
        public bool mValid1, mValid2, mValid3, mValid4, mValid5, mValid6, mValid7, mValid8, mValid9, mValid10,
            mValid11, mValid12, mValid13, mValid14, mValid15, mValid16, mValid17, mValid18, mValid19, mValid20,
            mValid21, mValid22, mValid23, mValid24, mValid25, mValid26, mValid27, mValid28, mValid29, mValid30,
            mValid31, mValid32, mValid33, mValid34, mValid35, mValid36;
        public int mCurrEntryPtr = 0, mCurrPointer = 0;
        public UInt64 mMisses, mHits, mFlushes;
        public cTLB()
        {
            mCurrEntryPtr = 1;
        }

        public UInt32 Translate(Processor_80x86 mProc, ref sInstruction sIns, UInt32 inAddr, bool Writing, ePrivLvl pCPL)
        {
            UInt32 lPage;
            int lEntry = 0;
            UInt32 lDirEntry = 0, lPageEntry = 0, lFoundLogical = 0, lFoundPhys = 0;
            bool lFound = false;
            UInt16 lFoundType = 0;

#if PAGING_USE_LAST_PASSED_ADDRESS_LOGIC
            if ((LastAddress & 0xFFFFF000) == (inAddr & 0xFFFFF000))
                return (LastReply & 0xFFFFF000) | (inAddr & 0x00000FFF);
#endif
            if (mValid1 && mLogicalAddr1 <= inAddr && mLogicalAddr1 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr1;
                lFoundPhys = mPhysicalAddr1;
                lFoundType = mType1;
                lFound = true;
            }
            else if (mValid2 && mLogicalAddr2 <= inAddr && mLogicalAddr2 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr2;
                lFoundPhys = mPhysicalAddr2;
                lFoundType = mType2;
                lFound = true;
            }
            else if (mValid3 && mLogicalAddr3 <= inAddr && mLogicalAddr3 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr3;
                lFoundPhys = mPhysicalAddr3;
                lFoundType = mType3;
                lFound = true;
            }
            else if (mValid4 && mLogicalAddr4 <= inAddr && mLogicalAddr4 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr4;
                lFoundPhys = mPhysicalAddr4;
                lFoundType = mType4;
                lFound = true;
            }
            else if (mValid5 && mLogicalAddr5 <= inAddr && mLogicalAddr5 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr5;
                lFoundPhys = mPhysicalAddr5;
                lFoundType = mType5;
                lFound = true;
            }
            else if (mValid6 && mLogicalAddr6 <= inAddr && mLogicalAddr6 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr6;
                lFoundPhys = mPhysicalAddr6;
                lFoundType = mType6;
                lFound = true;
            }
            else if (mValid7 && mLogicalAddr7 <= inAddr && mLogicalAddr7 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr7;
                lFoundPhys = mPhysicalAddr7;
                lFoundType = mType7;
                lFound = true;
            }
            else if (mValid8 && mLogicalAddr8 <= inAddr && mLogicalAddr8 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr8;
                lFoundPhys = mPhysicalAddr8;
                lFoundType = mType8;
                lFound = true;
            }
            else if (mValid9 && mLogicalAddr9 <= inAddr && mLogicalAddr9 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr9;
                lFoundPhys = mPhysicalAddr9;
                lFoundType = mType9;
                lFound = true;
            }
            else if (mValid10 && mLogicalAddr10 <= inAddr && mLogicalAddr10 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr10;
                lFoundPhys = mPhysicalAddr10;
                lFoundType = mType10;
                lFound = true;
            }
            else if (mValid11 && mLogicalAddr11 <= inAddr && mLogicalAddr11 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr11;
                lFoundPhys = mPhysicalAddr11;
                lFoundType = mType11;
                lFound = true;
            }
            else if (mValid12 && mLogicalAddr12 <= inAddr && mLogicalAddr12 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr12;
                lFoundPhys = mPhysicalAddr12;
                lFoundType = mType12;
                lFound = true;
            }
            else if (mValid13 && mLogicalAddr13 <= inAddr && mLogicalAddr13 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr13;
                lFoundPhys = mPhysicalAddr13;
                lFoundType = mType13;
                lFound = true;
            }
            else if (mValid14 && mLogicalAddr14 <= inAddr && mLogicalAddr14 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr14;
                lFoundPhys = mPhysicalAddr14;
                lFoundType = mType14;
                lFound = true;
            }
            else if (mValid15 && mLogicalAddr15 <= inAddr && mLogicalAddr15 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr15;
                lFoundPhys = mPhysicalAddr15;
                lFoundType = mType15;
                lFound = true;
            }
            else if (mValid16 && mLogicalAddr16 <= inAddr && mLogicalAddr16 + 0xfff >= inAddr)
            {
                lFoundLogical = mLogicalAddr16;
                lFoundPhys = mPhysicalAddr16;
                lFoundType = mType16;
                lFound = true;
            }

            if (lFound)
            {
                if (pCPL == ePrivLvl.App_Level_3)
                {
                    if (Writing && (lFoundType & Misc.PAGING_PAGE_READ_WRITE_MASK) != Misc.PAGING_PAGE_READ_WRITE_MASK)
                    {
                        {
                            lFound = false;  //Make the code look up the entry again to see if it has changed.
                        }
                    }
                    else if ((lFoundType & Misc.PAGING_USER_MASK) != Misc.PAGING_USER_MASK)
                    {
                        lFound = false; //07/19/2013 - making this also do a look-up again
                    }
                }
                else if (Writing &&
                (((lFoundType & Misc.PAGING_PAGE_READ_WRITE_MASK) != Misc.PAGING_PAGE_READ_WRITE_MASK)
                    || ((lFoundType & Misc.PAGING_PAGE_READ_WRITE_MASK) != Misc.PAGING_PAGE_READ_WRITE_MASK))
                && (mProc.regs.CR0 & Misc.CR0_WP_BIT_MASK) == Misc.CR0_WP_BIT_MASK
                && mProc.mSystem.mCR0_WP_Honor_In_Sup_Mode)
                {
                    lFound = false;  //Make the code look up the entry again to see if it has changed.
                }
                if (lFound)
                {
                    mHits++;
                    return lFoundPhys | (inAddr & 0x00000FFF);
                }
            }

            //If we got here, we know there's no entry Entry in the TLB so we'll walk the page tables
            lPage = mProc.mem.GetPageTableEntry(mProc, ref sIns, inAddr, ref lDirEntry, ref lPageEntry, Writing);
            if (sIns.ExceptionThrown)
                return 0xF0F0F0F0;

            if (mCurrEntryPtr > MAX_ENTRIES)
                mCurrEntryPtr = 1;

            switch (mCurrEntryPtr++)
                {
                    case 1:
                        mLogicalAddr1 = inAddr & 0xFFFFF000;
                        mPhysicalAddr1 = lPage & 0xFFFFF000;
                        mType1 = (UInt16)(lPage & 0x00000FFF);
                        mValid1 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType1 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType1 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr1 | (inAddr & 0x00000FFF);
                    case 2:
                        mLogicalAddr2 = inAddr & 0xFFFFF000;
                        mPhysicalAddr2 = lPage & 0xFFFFF000;
                        mType2 = (UInt16)(lPage & 0x00000FFF);
                        mValid2 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType2 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType2 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr2 | (inAddr & 0x00000FFF);
                    case 3:
                        mLogicalAddr3 = inAddr & 0xFFFFF000;
                        mPhysicalAddr3 = lPage & 0xFFFFF000;
                        mType3 = (UInt16)(lPage & 0x00000FFF);
                        mValid3 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType3 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType3 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr3 | (inAddr & 0x00000FFF);
                    case 4:
                        mLogicalAddr4 = inAddr & 0xFFFFF000;
                        mPhysicalAddr4 = lPage & 0xFFFFF000;
                        mType4 = (UInt16)(lPage & 0x00000FFF);
                        mValid4 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType4 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType4 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr4 | (inAddr & 0x00000FFF);
                    case 5:
                        mLogicalAddr5 = inAddr & 0xFFFFF000;
                        mPhysicalAddr5 = lPage & 0xFFFFF000;
                        mType5 = (UInt16)(lPage & 0x00000FFF);
                        mValid5 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType5 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType5 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr5 | (inAddr & 0x00000FFF);
                    case 6:
                        mLogicalAddr6 = inAddr & 0xFFFFF000;
                        mPhysicalAddr6 = lPage & 0xFFFFF000;
                        mType6 = (UInt16)(lPage & 0x00000FFF);
                        mValid6 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType6 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType6 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr6 | (inAddr & 0x00000FFF);
                    case 7:
                        mLogicalAddr7 = inAddr & 0xFFFFF000;
                        mPhysicalAddr7 = lPage & 0xFFFFF000;
                        mType7 = (UInt16)(lPage & 0x00000FFF);
                        mValid7 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType7 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType7 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr7 | (inAddr & 0x00000FFF);
                    case 8:
                        mLogicalAddr8 = inAddr & 0xFFFFF000;
                        mPhysicalAddr8 = lPage & 0xFFFFF000;
                        mType8 = (UInt16)(lPage & 0x00000FFF);
                        mValid8 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType8 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType8 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr8 | (inAddr & 0x00000FFF);
                    case 9:
                        mLogicalAddr9 = inAddr & 0xFFFFF000;
                        mPhysicalAddr9 = lPage & 0xFFFFF000;
                        mType9 = (UInt16)(lPage & 0x00000FFF);
                        mValid9 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType9 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType9 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr9 | (inAddr & 0x00000FFF);
                    case 10:
                        mLogicalAddr10 = inAddr & 0xFFFFF000;
                        mPhysicalAddr10 = lPage & 0xFFFFF000;
                        mType10 = (UInt16)(lPage & 0x00000FFF);
                        mValid10 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType10 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType10 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr10 | (inAddr & 0x00000FFF);
                    case 11:
                        mLogicalAddr11 = inAddr & 0xFFFFF000;
                        mPhysicalAddr11 = lPage & 0xFFFFF000;
                        mType11 = (UInt16)(lPage & 0x00000FFF);
                        mValid11 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType11 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType11 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr11 | (inAddr & 0x00000FFF);
                    case 12:
                        mLogicalAddr12 = inAddr & 0xFFFFF000;
                        mPhysicalAddr12 = lPage & 0xFFFFF000;
                        mType12 = (UInt16)(lPage & 0x00000FFF);
                        mValid12 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType12 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType12 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr12 | (inAddr & 0x00000FFF);
                    case 13:
                        mLogicalAddr13 = inAddr & 0xFFFFF000;
                        mPhysicalAddr13 = lPage & 0xFFFFF000;
                        mType13 = (UInt16)(lPage & 0x00000FFF);
                        mValid13 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType13 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType13 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr13 | (inAddr & 0x00000FFF);
                    case 14:
                        mLogicalAddr14 = inAddr & 0xFFFFF000;
                        mPhysicalAddr14 = lPage & 0xFFFFF000;
                        mType14 = (UInt16)(lPage & 0x00000FFF);
                        mValid14 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType14 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType14 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr14 | (inAddr & 0x00000FFF);
                    case 15:
                        mLogicalAddr15 = inAddr & 0xFFFFF000;
                        mPhysicalAddr15 = lPage & 0xFFFFF000;
                        mType15 = (UInt16)(lPage & 0x00000FFF);
                        mValid15 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType15 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType15 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr15 | (inAddr & 0x00000FFF);
                    default:
                        mLogicalAddr16 = inAddr & 0xFFFFF000;
                        mPhysicalAddr16 = lPage & 0xFFFFF000;
                        mType16 = (UInt16)(lPage & 0x00000FFF);
                        mValid16 = true;
                        // For a page directory entry, the user bit controls access to all the pages referenced by the page directory entry. Therefore if you wish to make 
                        // a page a user page, you must set the user bit in the relevant page directory entry as well as the page table entry. 
                        if ((lDirEntry & 0x4) != 0x4 || (lPageEntry & 0x4) != 0x4)
                            mType16 &= 0xFFB;
                        if ((lDirEntry & 0x2) != 0x2 || (lPageEntry & 0x2) != 0x2)
                            mType16 &= 0xFFD;
                        mMisses++;
                        return mPhysicalAddr16 | (inAddr & 0x00000FFF);
                }
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
            if (mValid1 && mLogicalAddr1 <= inAddr && mLogicalAddr1 + 0xfff >= inAddr)
                return mPhysicalAddr1 | (inAddr & 0x00000FFF);
            else if (mValid2 && mLogicalAddr2 <= inAddr && mLogicalAddr2 + 0xfff >= inAddr)
                return mPhysicalAddr2 | (inAddr & 0x00000FFF);
            else if (mValid3 && mLogicalAddr3 <= inAddr && mLogicalAddr3 + 0xfff >= inAddr)
                return mPhysicalAddr3 | (inAddr & 0x00000FFF);
            else if (mValid4 && mLogicalAddr4 <= inAddr && mLogicalAddr4 + 0xfff >= inAddr)
                return mPhysicalAddr4 | (inAddr & 0x00000FFF);
            else if (mValid5 && mLogicalAddr5 <= inAddr && mLogicalAddr5 + 0xfff >= inAddr)
                return mPhysicalAddr5 | (inAddr & 0x00000FFF);
            else if (mValid6 && mLogicalAddr6 <= inAddr && mLogicalAddr6 + 0xfff >= inAddr)
                return mPhysicalAddr6 | (inAddr & 0x00000FFF);
            else if (mValid7 && mLogicalAddr7 <= inAddr && mLogicalAddr7 + 0xfff >= inAddr)
                return mPhysicalAddr7 | (inAddr & 0x00000FFF);
            else if (mValid8 && mLogicalAddr8 <= inAddr && mLogicalAddr8 + 0xfff >= inAddr)
                return mPhysicalAddr8 | (inAddr & 0x00000FFF);
            else if (mValid9 && mLogicalAddr9 <= inAddr && mLogicalAddr9 + 0xfff >= inAddr)
                return mPhysicalAddr9 | (inAddr & 0x00000FFF);
            else if (mValid10 && mLogicalAddr10 <= inAddr && mLogicalAddr10 + 0xfff >= inAddr)
                return mPhysicalAddr10 | (inAddr & 0x00000FFF);
            else if (mValid11 && mLogicalAddr11 <= inAddr && mLogicalAddr11 + 0xfff >= inAddr)
                return mPhysicalAddr11 | (inAddr & 0x00000FFF);
            else if (mValid12 && mLogicalAddr12 <= inAddr && mLogicalAddr12 + 0xfff >= inAddr)
                return mPhysicalAddr12 | (inAddr & 0x00000FFF);
            else if (mValid13 && mLogicalAddr13 <= inAddr && mLogicalAddr13 + 0xfff >= inAddr)
                return mPhysicalAddr13 | (inAddr & 0x00000FFF);
            else if (mValid14 && mLogicalAddr14 <= inAddr && mLogicalAddr14 + 0xfff >= inAddr)
                return mPhysicalAddr14 | (inAddr & 0x00000FFF);
            else if (mValid15 && mLogicalAddr15 <= inAddr && mLogicalAddr15 + 0xfff >= inAddr)
                return mPhysicalAddr15 | (inAddr & 0x00000FFF);
            else if (mValid16 && mLogicalAddr16 <= inAddr && mLogicalAddr16 + 0xfff >= inAddr)
                return mPhysicalAddr16 | (inAddr & 0x00000FFF);
            //If we got here, we know there's no entry Entry in the TLB so we'll walk the page tables
            sIns.ExceptionErrorCode = 0xfAfafafa;
            return 0xf1f1f1f1;
        }

        public void Flush(Processor_80x86 mProc)
        {
            if (mProc.mSystem.Debuggies.DebugMemPaging)
                mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "TLB has been flushing, has " + mCurrEntryPtr + " entries.");
            //if the PGE (Page Global Enable) flag of CR4 is set, honor global pages
            if ((mProc.regs.CR4 & 0x80) == 0x80)
            {
                if ((mType1 & 0x100) == 0x100)
                    mValid1 = false;
                if ((mType2 & 0x100) == 0x100)
                    mValid2 = false;
                if ((mType3 & 0x100) == 0x100)
                    mValid3 = false;
                if ((mType4 & 0x100) == 0x100)
                    mValid4 = false;
                if ((mType5 & 0x100) == 0x100)
                    mValid5 = false;
                if ((mType6 & 0x100) == 0x100)
                    mValid6 = false;
                if ((mType7 & 0x100) == 0x100)
                    mValid7 = false;
                if ((mType8 & 0x100) == 0x100)
                    mValid8 = false;
                if ((mType9 & 0x100) == 0x100)
                    mValid9 = false;
                if ((mType10 & 0x100) == 0x100)
                    mValid10 = false;
                if ((mType11 & 0x100) == 0x100)
                    mValid11 = false;
                if ((mType12 & 0x100) == 0x100)
                    mValid12 = false;
                if ((mType13 & 0x100) == 0x100)
                    mValid13 = false;
                if ((mType14 & 0x100) == 0x100)
                    mValid14 = false;
                if ((mType15 & 0x100) == 0x100)
                    mValid15 = false;
                if ((mType16 & 0x100) == 0x100)
                    mValid6 = false;
            }
            mProc.mem.mLastAddressWrite = true;
            mProc.mem.mLastLogicalAddress = 0xFFFFFFFF;
            mProc.mem.mLastPhysicalAddress = 0xFFFFFFFF;
            mFlushes++;
            //If global pages enabled, exit now so that we don't invalidate EVERYTHING or set the current entry pointer back to 1
            if ((mProc.regs.CR4 & 0x80) == 0x80)
                return;
            mValid1 = mValid2 = mValid3 = mValid4 = mValid5 = mValid6 = mValid7 = mValid8 = 
                mValid9 = mValid10 = mValid11 = mValid12 = mValid13 = mValid14 = mValid15 = mValid16 = false;
            mCurrEntryPtr = 1;
        }

        internal void InvalidatePage(Processor_80x86 mProc, UInt32 Address)
        {
            if (mLogicalAddr1 <= Address && mLogicalAddr1 + 0xfff >= Address)
                mValid1 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr1.ToString("X8") + " --> physical page " + mPhysicalAddr1.ToString("X8"));
            else if (mLogicalAddr2 <= Address && mLogicalAddr2 + 0xfff >= Address)
                mValid2 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr2.ToString("X8") + " --> physical page " + mPhysicalAddr2.ToString("X8"));
                else if (mLogicalAddr3 <= Address && mLogicalAddr3 + 0xfff >= Address)
                    mValid3 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr3.ToString("X8") + " --> physical page " + mPhysicalAddr3.ToString("X8"));
                else if (mLogicalAddr4 <= Address && mLogicalAddr4 + 0xfff >= Address)
                    mValid4 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr4.ToString("X8") + " --> physical page " + mPhysicalAddr4.ToString("X8"));
                else if (mLogicalAddr5 <= Address && mLogicalAddr5 + 0xfff >= Address)
                    mValid5 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr5.ToString("X8") + " --> physical page " + mPhysicalAddr5.ToString("X8"));
                else if (mLogicalAddr6 <= Address && mLogicalAddr6 + 0xfff >= Address)
                    mValid6 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr6.ToString("X8") + " --> physical page " + mPhysicalAddr6.ToString("X8"));
                else if (mLogicalAddr7 <= Address && mLogicalAddr7 + 0xfff >= Address)
                    mValid7 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr7.ToString("X8") + " --> physical page " + mPhysicalAddr7.ToString("X8"));
                else if (mLogicalAddr8 <= Address && mLogicalAddr8 + 0xfff >= Address)
                    mValid8 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr8.ToString("X8") + " --> physical page " + mPhysicalAddr8.ToString("X8"));
                else if (mLogicalAddr9 <= Address && mLogicalAddr9 + 0xfff >= Address)
                    mValid9 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr9.ToString("X8") + " --> physical page " + mPhysicalAddr9.ToString("X8"));
                else if (mLogicalAddr10 <= Address && mLogicalAddr10 + 0xfff >= Address)
                    mValid10 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr10.ToString("X8") + " --> physical page " + mPhysicalAddr10.ToString("X8"));
                else if (mLogicalAddr11 <= Address && mLogicalAddr11 + 0xfff >= Address)
                    mValid11 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr11.ToString("X8") + " --> physical page " + mPhysicalAddr11.ToString("X8"));
                else if (mLogicalAddr12 <= Address && mLogicalAddr12 + 0xfff >= Address)
                    mValid12 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr12.ToString("X8") + " --> physical page " + mPhysicalAddr12.ToString("X8"));
                else if (mLogicalAddr13 <= Address && mLogicalAddr13 + 0xfff >= Address)
                    mValid13 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr13.ToString("X8") + " --> physical page " + mPhysicalAddr13.ToString("X8"));
                else if (mLogicalAddr14 <= Address && mLogicalAddr14 + 0xfff >= Address)
                    mValid14 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr14.ToString("X8") + " --> physical page " + mPhysicalAddr14.ToString("X8"));
                else if (mLogicalAddr15 <= Address && mLogicalAddr15 + 0xfff >= Address)
                    mValid15 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr15.ToString("X8") + " --> physical page " + mPhysicalAddr15.ToString("X8"));
                else if (mLogicalAddr16 <= Address && mLogicalAddr16 + 0xfff >= Address)
                    mValid16 = false;
                if (mProc.mSystem.Debuggies.DebugMemPaging)
                    mProc.mSystem.PrintDebugMsg(eDebuggieNames.MemoryPaging, "INVLPG request, parameter = " + Address.ToString("X8") + " - Removing TLB Entry = logical page " + mLogicalAddr16.ToString("X8") + " --> physical page " + mPhysicalAddr16.ToString("X8"));
        }

    }
}
