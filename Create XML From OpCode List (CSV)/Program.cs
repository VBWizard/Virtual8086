using System;
 using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;


namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] source = new string[456];
            int idx = 0;

            //System.IO.StreamReader file = new StreamReader(@"/home/yogi/src/Virtual8086/Create XML From OpCode List (CSV)/OpCode_Operand_List.csv");
            System.IO.StreamReader file = new StreamReader(@"/home/yogi/src/Virtual8086/Create XML From OpCode List (CSV)/OpCode_Operand_List.csv");
            // Read into an array of strings.
            //string[] source = File.ReadAllLines("cust.csv");
            while (!file.EndOfStream)
            {
                source[idx++] = file.ReadLine();
            }

            XElement cust = new XElement("Root",
                from str in source
                let fields = str.Split(',')
                select new XElement("Op",
                    new XAttribute("OpCode", fields[0]),
                    new XElement("Instruction", fields[1]),
                    new XElement("Operand1", fields[2]),
                    new XElement("Operand2", fields[3]),
                    new XElement("Operand3", fields[4]),
                    new XElement("UseNewDecoder", fields[5])
                    )
                );
            System.IO.StreamWriter fileO = new StreamWriter(@"/home/yogi/src/Virtual8086/Create XML From OpCode List (CSV)/OpCode_Operand_List.xml");
            fileO.WriteLine(cust);
            fileO.Close();
            Console.WriteLine(cust);
            Console.ReadLine();
        }
    }
}
