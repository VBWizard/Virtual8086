using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Be.Windows.Forms;
using VirtualProcessor;

namespace WindowsFormsApplication1
{
    public partial class frmMemDisplay : Form
    {
        sInstruction sIns = new sInstruction();
        public frmMemDisplay()
        {
            InitializeComponent();
            txtPhysAddr.Focus();
        }

        public void cmdGo_Click(object sender, EventArgs e)
        {
            UInt32 lAddress = 0, lTranslatedAddress = 0;
            byte[] lMemArray = new byte[65536];
            VirtualProcessor.sInstruction sIns = new VirtualProcessor.sInstruction();

            if (UInt32.TryParse(txtPhysAddr.Text, System.Globalization.NumberStyles.HexNumber, System.Threading.Thread.CurrentThread.CurrentCulture, out lAddress))
            {

                //if ((Program.mSystem.mProc.regs.CR0 & 0x80000000) == 0x80000000)
                //{
                //    lTranslatedAddress = Program.mSystem.mProc.mTLB.ShallowTranslate(Program.mSystem.mProc, ref sIns, lAddress, false, ePrivLvl.Kernel_Ring_0);
                //    lMemArray = Program.mSystem.mProc.mem.Chunk(Program.mSystem.mProc, ref sIns, 0, lAddress, (uint)lMemArray.Length);
                //}
                //else
                try { lMemArray = Program.mSystem.mProc.mem.ChunkPhysical(Program.mSystem.mProc, 0, lAddress, (uint)lMemArray.Length); }
                catch { }

                this.Text = this.Text + " - Address=" + lTranslatedAddress.ToString("X8");

                Be.Windows.Forms.DynamicByteProvider bp = new Be.Windows.Forms.DynamicByteProvider(lMemArray);
                //if (sIns.ExceptionThrown)
                //     return;

                var converter = (object)"{ANSI (Default)}";
                hbMemory.ByteCharConverter = converter as IByteCharConverter;

                hbMemory.StringViewVisible = true;
                //DefaultByteCharConverter defConverter = new DefaultByteCharConverter();
                //hbMemory.ByteCharConverter = defConverter;

                hbMemory.ByteProvider = bp;
                hbMemory.BytesPerLine = 8;
                hbMemory.LineInfoVisible = true;
                hbMemory.LineInfoOffset = lAddress;
                hbMemory.GroupSeparatorVisible = false;
                hbMemory.GroupSeparatorVisible = true;
                hbMemory.GroupSize = 4;
            }
        }

        private void txtPhysAddr_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                cmdGo_Click(this, new EventArgs());
                txtPhysAddr.SelectAll();
                frmMemDisplay_Resize(this, new EventArgs());
            }
        }

        private void frmMemDisplay_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
                hbMemory.BytesPerLine = 32;
            else
                hbMemory.BytesPerLine = 8;
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void hbmemory_CopiedHex(object sender, EventArgs e)
        {
            hbMemory.CopyHex();
        }

        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            sInstruction test = new sInstruction();
            UInt32 lAddress;
            byte lValue;
            Byte.TryParse(txtValue.Text, System.Globalization.NumberStyles.HexNumber, System.Threading.Thread.CurrentThread.CurrentCulture, out lValue);
            UInt32.TryParse(txtPhysAddr.Text, System.Globalization.NumberStyles.HexNumber, System.Threading.Thread.CurrentThread.CurrentCulture, out lAddress);
            PhysicalMem.mMemBytes[lAddress] = lValue;
        }
    }
}
