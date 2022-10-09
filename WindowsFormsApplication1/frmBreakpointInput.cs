using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class frmBreakpointInput : Form
    {
        public frmBreakpointInput()
        {
            InitializeComponent();
        }

        private void txtEIP_TextChanged(object sender, EventArgs e)
        {
            if (txtEIP.Text.ToUpper() == "FFFFFFFF")
            {
                ckRemoveOnHit.Checked = true;
                ckRemoveOnHit.Enabled = false;
            }
            else
            {
                ckRemoveOnHit.Checked = false;
                ckRemoveOnHit.Enabled = true;
            }
            Textboxes_TextChanged(sender, e);
        }

        private void Textboxes_TextChanged(object sender, EventArgs e)
        {
            cmdOk.Enabled = false;
            if (txtCS.Text != "" || txtEIP.Text != "")
            {
                txtIntNum.Enabled = false;
                txtFunction.Enabled = false;
                ckDOSFunct.Enabled = false;
                txtTaskNumber.Enabled = false;
                txtProcessName.Enabled = false;
                txtCS.Enabled = true;
                txtEIP.Enabled = true;
            }
            else if (txtTaskNumber.Text != "")
            {
                txtCS.Enabled = false; txtEIP.Enabled = false; 
                txtIntNum.Enabled = false;
                txtFunction.Enabled = false;
                ckDOSFunct.Enabled = false;
                cmdOk.Enabled = true;
                txtProcessName.Enabled = false;
                txtTaskNumber.Enabled = true;
            }
            else if (txtIntNum.Text != "" || txtFunction.Text != "0")
            {
                txtCS.Enabled = false; txtEIP.Enabled = false; ckRemoveOnHit.Enabled = false; txtTaskNumber.Enabled = false;
                txtProcessName.Enabled = false;
                txtIntNum.Enabled = true;
                txtFunction.Enabled = true;
                cmdOk.Enabled = true;
            }
            else if (txtProcessName.Text != "")
            {
                txtCS.Enabled = false; txtEIP.Enabled = false;
                txtIntNum.Enabled = false;
                txtFunction.Enabled = false;
                ckDOSFunct.Enabled = false;
                cmdOk.Enabled = true;
                txtTaskNumber.Enabled = false;
                txtProcessName.Enabled = true;
            }
            else
            {
                txtCS.Enabled = txtEIP.Enabled = txtIntNum.Enabled = txtFunction.Enabled = ckDOSFunct.Enabled = ckRemoveOnHit.Enabled = txtTaskNumber.Enabled = txtProcessName.Enabled = true;
                cmdOk.Enabled = false;
            }
            if (cmdOk.Enabled == false && txtCS.Text != "" && txtEIP.Text != "")
                cmdOk.Enabled = true;
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {

        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {

        }

        private void txtFunction_TextChanged(object sender, EventArgs e)
        {
            if (txtFunction.Text == "")
                txtFunction.Text = "0";
            Textboxes_TextChanged(sender, e);
        }
    }
}
