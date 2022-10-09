using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CreateCMOSImage
{
    class Program
    {
        static Encoding en = Encoding.GetEncoding("utf-8");
        //Byte 19 currently set to 00 = no hard drives installed
        //Bytes 24 & 25 currently set to 0 (low/high bytes) = no extended memory
        //Byte 26 currently set to 0 = hd 0 extended type = 0
        //Bytes 28 - 36 set to 0 = undefined c drive
        static byte[] cmos = new byte[128]  {
        /*00*/              0x29, 0x2b, 0x30, 0x2b, 0x16, 0x0b, 0x00, 0x01, 0x01, 0x96,  //RTC values
        /*0A*/              0x00, 0x02, 0x50, 0x80,  //Status registers
        /*0E*/              0x00,  //Diagnostic Status
        /*0F*/              0x00,  //Shutdown Status
        /*10*/              0x44,  //Floppy Types - was 40 for just a=1.44, 44 for a+b=1.44, 34 for a=720k, b=1.44
        /*11*/              0xf,  //System config settings - was 8f - removed 8 to disable mouse (clr 03/10/2014)
//        /*12*/              0x00,  //HD types
        /*12*/              0xFF,  //HD types
        /*13*/              0xc0,  //Typematic parameters
        /*14*/              0x7F,  //Installed equipment - was 0x0f, added 40 for 2nd floppy, then to 70 to include mono setting
        /*15*/              0x80, 0x02,   //Base memory
        /*17*/              0x00, 0x20,   //Extended Memory - changed from 0,0 to 0,4 for 1 mb extended memory
//        /*19*/              0x00,         //HD0 Extended Type
                            //User defined type 47
        /*19*/              0x2F,         //HD0 Extended Type
        /*1A*/              0x00,         //HD1 Extended Type

        //Original values
        ///*1B*/              0x1e, 0x00, 0x04, 0xff, 0xff, 0x11, 0x00, 0x00, 0x00,     //User Defined C:
        ///*24*/              0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x08, 0x03, 0x03,     //User Defined D:

//        /*1B*/              0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     //User Defined C:

                            //306 cyl (0x132), 4 heads, 17 (0x11) sects
                          // ccc   CCC    HH                                SEC
        /*1B*/              0xc3, 0x04, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3f,     //User Defined C:
        /*24*/              0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     //User Defined D:
       // /*2D*/              0x63,         //System Operational Flags (original value 0x03), then from 0x63 to 0x73
       //set at 0x03 BIOS will ask which HD to boot from, 0x23 will make it boot from floppy
       /*2D*/              0x03,         //System Operational Flags (original value 0x03), then from 0x63 to 0x73 - back to 03 to boot from HD
        /*2E*/              0x05, 0xc5,   //CMOS Checksum
        /*30*/              0x00, 0x20,   //Actual Extended Memory - changed from 0,3c to 0,4 1 gb extended
        /*32*/              0x00,         //Century (BCD)
        /*33*/              0x00,         //POST Flags
        
        //Next two bytes should be shadow ... but it appears that they are used by bochs for extended memory amt
        0x00,0x00,
        // /*34*/              0x00,         //BIOS and Shadow Option Flags
        // /*35*/              0xf0,         //BIOS and Shadow Option Flags 2
        /*36*/              0x00,         //Chipset specific info
        /*37*/              0x10,         //Password seed & Color Option
        //38 - 3d was originally Encrypted password
        //changing 0x3D for Eltoro boot so to be first boot device, from 0x00 to 0x01=first floppy
        /*38*/              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,       //byte # 0x39 is translation parameter
        /*3E*/              0x00, 0x21,   //Extended CMOS Checksum
        /*40*/              0x00,         //Model number
        /*41*/              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,       //Serial # Bytes
        /*47*/              0x00,         //CRC Byte
        /*48*/              0x00,         //Century byte
        /*49*/              0x00,         //Date Alarm
        /*4A*/              0x00, 0x00,   //Extended Control Registers (4a & 4b)
        /*4C*/              0x00,         //Reserved
        /*4D*/              0x00,         //RTC Address 2
        /*4E*/              0x00,         //RTC Address 3
        /*4F*/              0x00, 0x00,   //Extended Ram Address (LSB/MSB)
        /*51*/              0x00,         //Reserved
        /*52*/              0x00,         //Extended RAM Data Port
        /*53*/              0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00,   //Reserved
        /*5d*/              0x00, 0x00, 0x00,
        /*60*/              0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  // 60
        /*68*/              0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        /*70*/              0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  // 70
        /*78*/              0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 //7F
        };

        static void Main(string[] args)
        {
            //Int16 a = (Int16)0xb401;
            //Int16 b = (Int16)0xfa65;
            string lCMOSFileName = @"I:\virtual8086-temp_backup\3\Virtual8086\cmos.bin";
            StreamWriter sw = new StreamWriter(lCMOSFileName, false);
            BinaryWriter bw = new BinaryWriter(sw.BaseStream);

            checksum_cmos();
            bw.Write(cmos);
            Console.WriteLine("Wrote CMOS to " + lCMOSFileName);
            sw.Close();
            Console.ReadLine();
        }
        static void checksum_cmos()
        {
            UInt16 sum = 0;
            for (int i = 0x10; i <= 0x2d; i++)
                sum += cmos[i];
            byte Hi = 0, Lo = 0;
            Hi = (byte)((sum >> 8) & 0xff);
            Lo = (byte)((sum & 0xff));
            cmos[0x2e] = Hi;
            cmos[0x2f] = Lo;
            Console.WriteLine("Checksum: " + sum.ToString("x").PadLeft(4, '0'));
            Console.WriteLine("Hi Byte: " + Hi.ToString("x").PadLeft(2, '0'));
            Console.WriteLine("Lo Byte: " + Lo.ToString("x").PadLeft(2, '0'));
        }
    }
}
