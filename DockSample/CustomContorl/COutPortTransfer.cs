﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BusinessLogic.Common;
using DevComponents.DotNetBar;

namespace MonitorMain.CustomContorl
{
    public partial class COutPortTransfer : UserControl
    {
        public COutPortTransfer()
        {
            InitializeComponent();
        }

        private void COutPortTransfer_Load(object sender, EventArgs e)
        {
            DataGridViewTranslation.LoadMainColHeader(dgvfirstout);
            DataGridViewTranslation.LoadMainColHeader(dgvsecout);
            dgvfirstout.DataSource = OutPortTransfer.GetOutPortTaskList("1");
            dgvsecout.DataSource = OutPortTransfer.GetOutPortTaskList("2");
            btndisfirst.Pulse(10);
            btndissec.Pulse(10);
            
            
        }

        

        private void btndisfirst_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否将当前出口的卷烟转移动另一出口？", "提示", MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {

                OutPortTransfer.SetDisOutPort("1", "2");
                OutPortTransfer.SetServerDisOutPort("2");
                //OutPortTransfer.SetServerDisOutPort("1", "2");
                dgvfirstout.DataSource = OutPortTransfer.GetOutPortTaskList("1");
                dgvsecout.DataSource = OutPortTransfer.GetOutPortTaskList("2");
                FJMainForm.Instance.SetOutPort();
            }
            
        }

        private void btndissec_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否将当前出口的卷烟转移动另一出口？", "提示", MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {
                OutPortTransfer.SetDisOutPort("2", "1");
                OutPortTransfer.SetServerDisOutPort("1");
                //OutPortTransfer.SetServerDisOutPort("2", "1");
                dgvfirstout.DataSource = OutPortTransfer.GetOutPortTaskList("1");
                dgvsecout.DataSource = OutPortTransfer.GetOutPortTaskList("2");
                FJMainForm.Instance.SetOutPort();
            }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否平均分配出口？", "提示", MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {
                OutPortTransfer.SetDisOutPort();
                OutPortTransfer.SetServerDisOutPort();
                //OutPortTransfer.SetServerDisOutPort("2", "1");
                dgvfirstout.DataSource = OutPortTransfer.GetOutPortTaskList("1");
                dgvsecout.DataSource = OutPortTransfer.GetOutPortTaskList("2");
                FJMainForm.Instance.SetOutPort();
            }
        }
    }
}
