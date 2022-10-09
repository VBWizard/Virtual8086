using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualProcessor
{
    class cInstructCache
    {
        public const int MAX_INSTRUCTS = 23;

        internal Instruct[] mInstructs;
        internal int mCachedInstructions = 0;

        public cInstructCache()
        {
            mInstructs = new Instruct[MAX_INSTRUCTS];
            for (int cnt = 0; cnt < MAX_INSTRUCTS; cnt++)
            {
                mInstructs[cnt] = new Instruct();
                mInstructs[cnt].DecodedInstruction.InstructionAddress = 0;
            }
        }

        public Instruct Get(UInt64 InstructionAddress)
        {
            for (int c = 0; c < mCachedInstructions; c++)
                if (mInstructs[c].DecodedInstruction.InstructionAddress == InstructionAddress/* && mInstructs[c].valid*/)
                {
                    MRUInstruct(c);
                    return mInstructs[0];
                }

            //if (/*mCachedInstructions > 0 && */InstructionAddress == mInstructs[0].DecodedInstruction.InstructionAddress)
            //    return mInstructs[0];
            //if (/*mCachedInstructions > 1 && */InstructionAddress == mInstructs[1].DecodedInstruction.InstructionAddress)
            //    return mInstructs[1];
            //if (/*mCachedInstructions > 2 && */InstructionAddress == mInstructs[2].DecodedInstruction.InstructionAddress)
            //    return mInstructs[2];
            //if (/*mCachedInstructions > 3 && */InstructionAddress == mInstructs[3].DecodedInstruction.InstructionAddress)
            //    return mInstructs[3];
            //if (/*mCachedInstructions > 4 && */InstructionAddress == mInstructs[4].DecodedInstruction.InstructionAddress)
            //    return mInstructs[4];
            //if (/*mCachedInstructions > 5 && */InstructionAddress == mInstructs[5].DecodedInstruction.InstructionAddress)
            //    return mInstructs[5];
            //if (/*mCachedInstructions > 6 && */InstructionAddress == mInstructs[6].DecodedInstruction.InstructionAddress)
            //    return mInstructs[6];
            //if (/*mCachedInstructions > 7 && */InstructionAddress == mInstructs[7].DecodedInstruction.InstructionAddress)
            //    return mInstructs[7];
            //if (/*mCachedInstructions > 8 && */InstructionAddress == mInstructs[8].DecodedInstruction.InstructionAddress)
            //    return mInstructs[8];
            //if (/*mCachedInstructions > 9 && */InstructionAddress == mInstructs[9].DecodedInstruction.InstructionAddress)
            //    return mInstructs[9];
            //if (/*mCachedInstructions > 10 && */InstructionAddress == mInstructs[10].DecodedInstruction.InstructionAddress)
            //    return mInstructs[10];
            //if (/*mCachedInstructions > 11 && */InstructionAddress == mInstructs[11].DecodedInstruction.InstructionAddress)
            //    return mInstructs[11];
            //if (/*mCachedInstructions > 12 && */InstructionAddress == mInstructs[12].DecodedInstruction.InstructionAddress)
            //    return mInstructs[12];
            //if (/*mCachedInstructions > 13 && */InstructionAddress == mInstructs[13].DecodedInstruction.InstructionAddress)
            //    return mInstructs[13];
            //if (/*mCachedInstructions >= 14 && */InstructionAddress == mInstructs[14].DecodedInstruction.InstructionAddress)
            //    return mInstructs[14];
            //if (/*mCachedInstructions > 5 && */InstructionAddress == mInstructs[15].DecodedInstruction.InstructionAddress)
            //    return mInstructs[15];
            //if (/*mCachedInstructions > 6 && */InstructionAddress == mInstructs[16].DecodedInstruction.InstructionAddress)
            //    return mInstructs[16];
            //if (/*mCachedInstructions > 7 && */InstructionAddress == mInstructs[17].DecodedInstruction.InstructionAddress)
            //    return mInstructs[17];
            //if (/*mCachedInstructions > 8 && */InstructionAddress == mInstructs[18].DecodedInstruction.InstructionAddress)
            //    return mInstructs[18];
            //if (/*mCachedInstructions > 9 && */InstructionAddress == mInstructs[19].DecodedInstruction.InstructionAddress)
            //    return mInstructs[19];

            return null;
        }

        public void MRUInstruct(int InstructCurrNumber)
        {
            Instruct temp = mInstructs[InstructCurrNumber];
            for (int cnt = InstructCurrNumber; cnt > 0; cnt--)
            {
                mInstructs[cnt] = mInstructs[cnt - 1];
            }
            mInstructs[0] = temp;
        }

        public void Add(Instruct Instruction)
        {
            if (mCachedInstructions == MAX_INSTRUCTS)
            {
                for (int c = 0; c < MAX_INSTRUCTS-1; c++)
                    mInstructs[c] = mInstructs[c + 1];
                mInstructs[MAX_INSTRUCTS-1] = Instruction;
            }
            else
            {
                mInstructs[mCachedInstructions++] = Instruction;
            }
        }

        public void Flush()
        {
            return;
            mInstructs = new Instruct[MAX_INSTRUCTS];
            mCachedInstructions = 0;
        }

    }

}
