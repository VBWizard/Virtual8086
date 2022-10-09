using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualProcessor
{
    class cInstructionCache : System.Collections.CollectionBase
    {
        UInt64[,] mCachedAddresses;
        int mCachedAddressCount = 0;

        public cInstructionCache()
        {
            this.Capacity = MAX_CACHED_INSTRS;
            mCachedAddresses = new UInt64[MAX_CACHED_INSTRS,2];
            mCachedAddressCount = 0;
        }

        public const int MAX_CACHED_INSTRS = 8;
        public Instruct this[UInt64 InstructionAddress]
        {
            get
            {
                object lTemp;

                if (mCachedAddressCount == 0)
                    return null;
                //int a = 0;
                //if (InstructionAddress == 0xfe077)
                //    a = a + 1 - 1;
                for (int c = mCachedAddressCount - 1; c >= 0; c--)
                    if (mCachedAddresses[c, 0] == InstructionAddress)
                    {
                        //if (((int)mCachedAddresses[c, 1] < mCachedAddressCount - MAX_CACHED_INSTRS / 10))
                        //{
                        //    lTemp = this.InnerList[(int)mCachedAddresses[c, 1]];
                        //    MakeMRU((int)mCachedAddresses[c, 1]);
                        //    return (Instruct)lTemp;
                        //}
                        return (Instruct)this.InnerList[(int)mCachedAddresses[c, 1]];
                    }
                return null;
            }
        }
        public void Add(Instruct i)
        {
            if (MAX_CACHED_INSTRS == 0)
                return;

            int lIdx = 0;
            int lAddressArrayAddIdx = 0;
            if (this.Count >= MAX_CACHED_INSTRS)
            {
                MyRemoveAt(0);
                lAddressArrayAddIdx = MAX_CACHED_INSTRS-1;
            }
            else
                lAddressArrayAddIdx = this.Count;
            //int a = 0;
            //if (i.DecodedInstruction.InstructionAddress == 0xfe077)
            //    a = a + 1 - 1;
            lIdx =  this.InnerList.Add(i);
            //0 = Address
            //1 = Array Index
            mCachedAddresses[lAddressArrayAddIdx, 0] = i.DecodedInstruction.InstructionAddress;
            mCachedAddresses[lAddressArrayAddIdx, 1] = (UInt64)lIdx;
            mCachedAddressCount++;

        }

        public void MakeMRU(int Index)
        {
            Instruct lTemp = (Instruct)this.InnerList[(int)mCachedAddresses[Index, 1]];
            MyRemoveAt(Index);
            Add(lTemp);
        }
        public void Invalidate(UInt64 InstructionAddress)
        {
                for (int c = 0; c < mCachedAddressCount; c++)
                    //Assuming max bytes per instruction of 12
                    if (InstructionAddress >= mCachedAddresses[c, 0] && InstructionAddress <= mCachedAddresses[c,0] + 12)
                    {
                        Instruct temp = ((Instruct)(this.InnerList[(int)mCachedAddresses[c, 1]]));
                        if (InstructionAddress <= mCachedAddresses[c,0] + temp.DecodedInstruction.BytesUsed)
                            RemoveAt(c);
                    }
        }
        private void MyRemoveAt(int Index)
        {
            this.InnerList.RemoveAt(Index);
            for (int c = Index; c <= this.Count - 1; c++)
            {
                mCachedAddresses[c, 0] = ((Instruct)(this.InnerList[c])).DecodedInstruction.InstructionAddress;
                mCachedAddresses[c, 1] = (UInt64)c;
            }
            mCachedAddressCount--;
        }
        public void Flush()
        {
            for (int c = 0; c < mCachedAddressCount; c++)
            {
                mCachedAddresses[c, 0] = 0xFFFFFFFF;
                mCachedAddresses[c, 1] = 0;
            }
            mCachedAddressCount = 0;
            this.InnerList.Clear();
        }
    }
}
