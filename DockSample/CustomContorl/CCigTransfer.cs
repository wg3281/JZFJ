﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BusinessLogic.Common;
using BusinessLogic.INTASKS;
using BusinessLogic.SortingTask;
using DevComponents.DotNetBar;

namespace MonitorMain.CustomContorl
{
    public partial class CCigTransfer : UserControl
    {
        public CCigTransfer()
        {
            InitializeComponent();
        }

        private void CCigTransfer_Load(object sender, EventArgs e)
        {
            DataGridViewTranslation.LoadMainColHeader(dgvnewlinebox);
            DataGridViewTranslation.LoadMainColHeader(dgvoldlinebox);
        	dgvoldlinebox.DataSource = SortingLineBoxList.GetBindLineBoxList();
            dgvnewlinebox.DataSource = SortingLineBoxList.GetEmptyLineBoxList();
        }

        private void customButton3_Click(object sender, EventArgs e)
        {
            if (dgvnewlinebox.SelectedRows.Count > 0 && dgvoldlinebox.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("是否转换烟仓？请确认烟仓拨烟数相同。", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (dgvoldlinebox.SelectedRows[0].Cells["oldPutNum"].Value.ToString() ==
                        dgvnewlinebox.SelectedRows[0].Cells["newPutNum"].Value.ToString())
                    {
                        SortingLineBox oldsortingLineBox = new SortingLineBox();
                        SortingLineBox newsortingLineBox = new SortingLineBox();


                        oldsortingLineBox.LineBoxCode =
                            dgvoldlinebox.SelectedRows[0].Cells["oldlineBoxCode"].Value.ToString();
                        oldsortingLineBox.LineBoxName =
                            dgvoldlinebox.SelectedRows[0].Cells["oldlineBoxName"].Value.ToString();
                        oldsortingLineBox.PlcAddress =
                            dgvoldlinebox.SelectedRows[0].Cells["oldplcAddress"].Value.ToString();

                  



                        newsortingLineBox.LineBoxCode =
                            dgvnewlinebox.SelectedRows[0].Cells["newLineBoxCode"].Value.ToString();
                        newsortingLineBox.LineBoxName =
                            dgvnewlinebox.SelectedRows[0].Cells["newLineBoxName"].Value.ToString();
                        newsortingLineBox.PlcAddress =
                            dgvnewlinebox.SelectedRows[0].Cells["newPlcAddress"].Value.ToString();

                        





                        SortingLineBoxList.TransactCigBox(oldsortingLineBox, newsortingLineBox);

                        dgvoldlinebox.DataSource = SortingLineBoxList.GetBindLineBoxList();
                        dgvnewlinebox.DataSource = SortingLineBoxList.GetEmptyLineBoxList();
                        MessageBox.Show("烟仓转移完成，如转移烟仓为立式烟仓请重新生成LED数据并发送！");

                    }
                    else
                    {
                        MessageBox.Show("需要转移的烟道拨烟数不相符！");
                    }
                }
            }
            else
            {
                MessageBox.Show("未选择转移的烟道！");
            }
        }
    }
}
