using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;


namespace VirtualProcessor
{
    public class OpcodeLoader
    {
        DataSet dsOpCodes;
        DataTable tbl;

        public OpcodeLoader()
        {
            dsOpCodes = new DataSet("OpCodes");
            string filePath = "J:\\Yogi\\src\\Virtual8086\\Virtual8086\\OpCode_Operand_List.xml";
            dsOpCodes.ReadXml(filePath);
            tbl = dsOpCodes.Tables[0];
        }

        internal DataTable GetDatTable()
        {
            return tbl;
        }

        public void AddOpCodesToInstructionList(ref InstructionList Instrs, ref sOpCodePointer[] OpCodeIndexer)
        {
            int lConvertTo = 16;
            foreach (DataRow dr in tbl.Rows)
            {
                sOpCode oc = new sOpCode();
                oc.Instruction = dr["Instruction"].ToString();
                oc.OpCode = System.Convert.ToUInt16(dr["OpCode"].ToString(),lConvertTo);
                //Define each potential Operand
                for (int cnt2 = 1; cnt2 <= 3; cnt2++)
                {
                    switch (cnt2)
                    {
                        case 1:
                            oc.Register1 = Misc.GetRegIDForRegName(dr["Operand1"].ToString());
                            if (oc.Register1 == eGeneralRegister.NONE)
                            {
                                oc.Op1AM = GetAddressingMethodForId(dr["Operand1"].ToString().Substring(0, 1), ref oc.ImmedOp1,ref oc.Op1UsesModRMRegSIB);
                                oc.Op1OT = GetOperandTypeForId(dr["Operand1"].ToString().Substring(1, dr["Operand1"].ToString().Length - 1));
                            }
                            else
                                oc.Op1AM = sOpCodeAddressingMethod.NamedRegister;
                            break;
                        case 2:
                            oc.Register2 = Misc.GetRegIDForRegName(dr["Operand2"].ToString());
                            if (oc.Register2 == eGeneralRegister.NONE)
                            {
                                oc.Op2AM = GetAddressingMethodForId(dr["Operand2"].ToString().Substring(0, 1), ref oc.ImmedOp2,ref oc.Op2UsesModRMRegSIB);
                                oc.Op2OT = GetOperandTypeForId(dr["Operand2"].ToString().Substring(1, dr["Operand2"].ToString().Length - 1));
                            }
                            else
                                oc.Op2AM = sOpCodeAddressingMethod.NamedRegister;
                            break;
                        case 3:
                            oc.Register3 = Misc.GetRegIDForRegName(dr["Operand3"].ToString());
                            if (oc.Register3 == eGeneralRegister.NONE)
                            {
                                oc.Op3AM = GetAddressingMethodForId(dr["Operand3"].ToString().Substring(0, 1), ref oc.ImmedOp3,ref oc.Op3UsesModRMRegSIB);
                                oc.Op3OT = GetOperandTypeForId(dr["Operand3"].ToString().Substring(1, dr["Operand3"].ToString().Length - 1));
                            }
                            else
                                oc.Op3AM = sOpCodeAddressingMethod.NamedRegister;
                            break;
                    }
                }
                //Process Addressing Methods
                try
                {
                    if (oc.Instruction.ToString() != " Instruction" && oc.Instruction.ToString() != "CS:" && oc.Instruction.ToString() != "SS:" && oc.Instruction.ToString() != "DS:" && oc.Instruction.ToString() != "ES:" && oc.Instruction.ToString() != "LOCK" /*&& oc.Instruction.ToString() != "JNL"*/)
                    {
                        Instrs[oc.Instruction].mOpCodes.Add(oc);
                        OpCodeIndexer[oc.OpCode].InstructionName = oc.Instruction;
                        OpCodeIndexer[oc.OpCode].OpCodeNum = oc.OpCode;
                        OpCodeIndexer[oc.OpCode].OpCode = oc;
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Write("Couldn't match instruction '" + dr["Instruction"].ToString() + "'");
                    System.Diagnostics.Debug.WriteLine(", error was: \n" + e.Message);
                }
            }
            tbl = null;
        }

        internal sOpCodeAddressingMethod GetAddressingMethodForId(String AM, ref bool HasImmediateData, ref bool UsesModRMSIB)
        {
            string lAMLeft1 = AM.Substring(0, 1);

            HasImmediateData = false;
            switch (AM)
            {
                case "-": return sOpCodeAddressingMethod.None;
                case "A": HasImmediateData = true; return sOpCodeAddressingMethod.DirectAddress;
                case "C": UsesModRMSIB = true; return sOpCodeAddressingMethod.RegControlReg;
                case "D": UsesModRMSIB = true; return sOpCodeAddressingMethod.RegDebugReg;
                case "E": UsesModRMSIB = true; return sOpCodeAddressingMethod.EType;
                case "F": return sOpCodeAddressingMethod.EFlags;
                case "G": UsesModRMSIB = true; return sOpCodeAddressingMethod.GenReg;
                case "I": HasImmediateData = true; return sOpCodeAddressingMethod.ImmedData;
                case "J": HasImmediateData = true; return sOpCodeAddressingMethod.JmpRelOffset;
                case "M": UsesModRMSIB = true; return sOpCodeAddressingMethod.MemoryOnly;
                case "O": HasImmediateData = true; return sOpCodeAddressingMethod.OpOffset;
                case "P": UsesModRMSIB = true; return sOpCodeAddressingMethod.MMXPkdQWord;
                case "Q": UsesModRMSIB = true; return sOpCodeAddressingMethod.QType;
                case "R": UsesModRMSIB = true; return sOpCodeAddressingMethod.ModGenReg;
                case "S": UsesModRMSIB = true; return sOpCodeAddressingMethod.RegSegReg;
                case "T": UsesModRMSIB = true; return sOpCodeAddressingMethod.RegTestReg;
                case "V": UsesModRMSIB = true; return sOpCodeAddressingMethod.XMM128Reg;
                case "W": UsesModRMSIB = true; return sOpCodeAddressingMethod.WType;
                case "X": return sOpCodeAddressingMethod.DSSIMem;
                case "Y": return sOpCodeAddressingMethod.ESDIMem;
                case "3": HasImmediateData = true; return sOpCodeAddressingMethod.Int3;
                case "1": HasImmediateData = true; return sOpCodeAddressingMethod.TheNumberOne;
                default:
                    throw new Exception("Addressing Mode ID is not defined: " + AM);

            }

        }
        internal sOpCodeOperandType GetOperandTypeForId(String OT)
        {
            switch (OT)
            {
                case "1": return sOpCodeOperandType.None;
                case "a": return sOpCodeOperandType.BoundType;
                case "b": return sOpCodeOperandType.Byte;
                case "c": return sOpCodeOperandType.ByteOrWord;
                case "d": return sOpCodeOperandType.DWord;
                case "dq": return sOpCodeOperandType.DQWord;
                case "p": return sOpCodeOperandType.Pointer;
                case "pi": return sOpCodeOperandType.QWMMXReg;
                case "ps": return sOpCodeOperandType.Packed128FP;
                case "q": return sOpCodeOperandType.QWord;
                case "s": return sOpCodeOperandType.PseudoDesc;
                case "ss": return sOpCodeOperandType.Scalar;
                case "si": return sOpCodeOperandType.DWordIntReg;
                case "v": return sOpCodeOperandType.WordOrDWord;
                case "w": return sOpCodeOperandType.Word;
                case "": return sOpCodeOperandType.None;
                default:
                    throw new Exception("Operand Type ID is not defined: " + OT);
            }
        }
    }
}
