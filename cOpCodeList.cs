using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualProcessor
{
    internal class cOpCodeList : System.Collections.CollectionBase
    {

        public cOpCodeList()
        { }

        public sOpCode this[int index]
        {
            get { return (sOpCode)InnerList[index]; }
            set { InnerList[index] = value; }
        }

        public sOpCode this[UInt16 OpCode]
        {
            get
            {
                for (int cnt = 0; cnt < List.Count;cnt++ )
                    if (((sOpCode)(List[cnt])).OpCode == OpCode)
                        return (sOpCode)(List[cnt]);
                        //foreach (sOpCode i in List)
                        //    if (i.OpCode == OpCode)
                        //        return (sOpCode)i;
                return new sOpCode();
            }
        }

        public int Add(sOpCode item)
        {
            return this.List.Add(item);
        }

/*        public int Count()
        {
            return List.Count;
        }
*/    }
}
