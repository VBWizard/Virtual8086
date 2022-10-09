using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        //byte[] lTemp = new byte[Program.mSystem.mTotalMemory / 4096];
        Point p;
        Color[] memValues = new Color[Program.mSystem.mTotalMemory / 4096 + 1];
        Color gNewColor;
        int gCellCount;
        int gCellsPerSide;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            p = new Point();
            ggMemory.Cells = new Size(64,64);
            ggMemory_Click(this, new EventArgs());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Focus();

        }

        private void ggMemory_Click(object sender, EventArgs e)
        {
            if (Program.mSystem == null)
                return;
            int lCellCount = (int)Program.mSystem.mTotalMemory / 4096;
            int lCellsPerSide = (int)Math.Sqrt(lCellCount);

            Point[] p = new Point[lCellCount];

            int x, y;
            x = y = 0;

            for (int cnt = 0; cnt < lCellCount; cnt++)
            {
                x += 1;
                if (x > (Math.Sqrt(lCellCount)))
                {
                    x = 0;
                    y++;
                }
                p[cnt] = new Point(x, y);
            }
            ggMemory.setCell(p, Color.Green);
            ggMemory.GridColor = Color.AliceBlue;
            ggMemory.ShowGrid = true;
            ggMemory.Cells = new Size(lCellsPerSide, lCellsPerSide);
        }


        private void timer1_Tick(object sender, EventArgs e)
        {

            //ggMemory.Hide();
#if CALCULATE_PAGE_MEMORY_USAGE
            timer1.Stop();
            gCellCount =  (int)System.Math.Ceiling((double)(Program.mSystem.mTotalMemory / 4096));
            gCellsPerSide = (int)Math.Sqrt(gCellCount);
            //lTemp = Program.mSystem.mProc.mem.mCHangedBlocks;
            for (int cnt = 0; cnt < gCellCount; cnt++)
            {
                p.X = (int)Math.Ceiling((double)(cnt % gCellsPerSide));
                p.Y = (int)Math.Ceiling((double)(cnt / gCellsPerSide));
                gNewColor = Color.Black;
                switch (Program.mSystem.mProc.mem.mCHangedBlocks[cnt])
                {
                    case 1:
                        gNewColor = Color.Red;
                        break;
                    case 2:
                        gNewColor = Color.PaleVioletRed;
                        break;
                    case 4:
                        gNewColor = Color.Blue;
                        break;
                    case 5:
                        gNewColor = Color.LightBlue;
                        break;
                    default:
                        gNewColor = Color.Black;
                        break;
                }
                if (memValues[cnt] != gNewColor)
                {
                    memValues[cnt] = gNewColor;
                    ggMemory.setCell(p, gNewColor);
                }
            }
            //ggMemory.Show();
            timer1.Start();
#endif
        }

        private void ggMemory_gridClick(object sender, Point GridPoint)
        {

            int lCellCount = (int)Program.mSystem.mTotalMemory / 4096;
            int lCellsPerSide = (int)Math.Sqrt(lCellCount);
            UInt32 lOffset = (UInt32)(((GridPoint.Y * lCellsPerSide) + GridPoint.X + GridPoint.Y)*4096);
            string lContents = "";
            
            this.Text = lOffset.ToString("X8");
            byte[] lArry = Program.mSystem.mProc.mem.ChunkPhysical(Program.mSystem.mProc,0,lOffset,4096);
            for (int cnt = 0; cnt < 4096;cnt++ )
            {
                if (lArry[cnt] > 20 && lArry[cnt] < 127)
                    lContents += (char)lArry[cnt];
                else if (lArry[cnt] == 0)
                    lContents += "0";
                else
                    lContents += ".";
            }
#if DOTRIES
            MessageBox.Show("Contents: " + lContents);
#endif
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

    }
}
