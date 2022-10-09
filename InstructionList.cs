using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace VirtualProcessor
{
    /// <summary>
    /// List of instructions and all applicable Opcodes
    /// </summary>
    public class InstructionList : System.Collections.CollectionBase
    {
        public UInt32[] mIndex;
        public InstructionList()
        {
            mIndex = new UInt32[0xFFFF];
            for (int c = 0; c < 0xffff; c++)
                mIndex[c] = 0xffffffff;
        }

        public Instruct this[UInt32 OpCode]
        {
            get
            {
                //    return (Instruct)this.List[index]; 
                //}
                //set { this.List[index] = value; }
                //for (int c = 0; c < this.Count; c++)
                //{
                //    for (int c2 = 0; c2 < ((Instruct)(this.InnerList[c])).OpCodes.Count; c2++)
                //        if (((Instruct)(this.InnerList[c])).OpCodes[c2].OpCode == index)
                //            return (Instruct)this.InnerList[c];
                //}
                return (Instruct)this.InnerList[(int)mIndex[OpCode]];
            }
        }
        public Instruct this[string InstructName]
        {
            get
            {
                for (int c=0;c<this.Count;c++)
                    if (((Instruct)(this.InnerList[c])).Name == InstructName)
                        return (Instruct)this.InnerList[c];
                    return null;
            }
        }

        public void Add(Instruct item, Processor_80x86 m_parent)
        {
            int lIdx = 0;
            item.mProc = m_parent;  
            lIdx = this.List.Add(item);
        }

        /// <summary>
        /// Create an array of OpCodes ... each element of the array points to the associated Instruct in the list (if any)
        /// </summary>
        public void FixupIndex(Processor_80x86 m_parent)
        {
            //For each instruction
            for (uint cnt = 0; cnt < this.Count; cnt++)
                //For each opcode defined in the instruction
                for (int c = 0; c < ((Instruct)(this.InnerList[(int)cnt])).mOpCodes.Count; c++)
                {
                    //Set the index of the opcode to point at the instruction's index
                    if (mIndex[((Instruct)(this.InnerList[(int)cnt])).mOpCodes[c].OpCode] == 0xFFFFFFFF)
                    {
                        mIndex[((Instruct)(this.InnerList[(int)cnt])).mOpCodes[c].OpCode] = (UInt32)cnt;
                    }
                    else
                        throw new Exception("BLAH!");
                }

        }
    }
}
