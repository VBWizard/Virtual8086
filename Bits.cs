using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtual8086
{
    [global::System.AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    sealed class BitfieldLengthAttribute : Attribute
    {
        ushort length;

        public BitfieldLengthAttribute(ushort length)
        {
            this.length = length;
        }

        public ushort Length { get { return length; } }
    }

    static class PrimitiveConversion
    {
        public static long ToLong<T>(T t) where T : struct
        {
            long r = 0;
            int offset = 0;

            // For every field suitably attributed with a BitfieldLength
            foreach (System.Reflection.FieldInfo f in t.GetType().GetFields())
            {
                object[] attrs = f.GetCustomAttributes(typeof(BitfieldLengthAttribute), false);
                if (attrs.Length == 1)
                {
                    ushort fieldLength = ((BitfieldLengthAttribute)attrs[0]).Length;

                    // Calculate a bitmask of the desired length
                    long mask = 0;
                    for (int i = 0; i < fieldLength; i++)
                        mask |= 1 << i;

                    r |= ((UInt16)f.GetValue(t) & mask) << offset;

                    offset += (int)fieldLength;
                }
            }

            return r;
        }
    }

/*    struct PESHeader
    {
        [BitfieldLength(2)]
        public ushort reserved;
        [BitfieldLength(2)]
        public ushort scrambling_control;
        [BitfieldLength(1)]
        public ushort priority;
        [BitfieldLength(1)]
        public ushort data_alignment_indicator;
        [BitfieldLength(1)]
        public ushort copyright;
        [BitfieldLength(1)]
        public ushort original_or_copy;
    }
*/
  }
