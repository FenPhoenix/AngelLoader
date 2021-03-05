using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AngelLoader.Forms
{
    public partial class DarkListBox2_Test_Form : Form
    {
        public DarkListBox2_Test_Form()
        {
            InitializeComponent();

            darkListBox21.Rows.Add(new DataGridViewRow());
            darkListBox21.Rows[0].Cells[0].Value = "Test!";
            //darkListBox21.Columns[0].Selected = false;
            //darkListBox21.Rows[0].Selected = false;
            //darkListBox21.Rows[0].Cells[0].Selected = false;
            //darkListBox21.Rows[darkListBox21.SelectedRows[0].Index].Selected = false;

            listView1.Items.Add("test long item length sdffdsf ds fdsaf asdf dasf asdf asd");
            darkListBox21.ClearSelection();
            foreach (DataGridViewRow row in darkListBox21.Rows)
            {
                Trace.WriteLine(row.Selected);
            }
        }

        private void DarkListBox2_Test_Form_Shown(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in darkListBox21.Rows)
            {
                Trace.WriteLine(row.Selected);
            }
            //darkListBox21.Rows[0].Selected = false;

            Trace.WriteLine(darkListBox21.DefaultCellStyle.Font.Height);
        }

        private void DarkListBox2_Test_Form_Load(object sender, EventArgs e)
        {
            darkListBox21.ClearSelection();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //darkListBox21.Rows.Clear();
            darkListBox21.Rows.AddRange(new DataGridViewRow[]{new DataGridViewRow(),new DataGridViewRow()});
            darkListBox21.Rows[1].Cells[0].Value = "woop";
            darkListBox21.Rows[2].Cells[0].Value = "fweeeep";
        }
    }
}
