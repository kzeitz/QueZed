using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo {
   using BO;

   public partial class Form1 : Form {
      private BindingSource nameBindingSource = new BindingSource();

      public Form1() {
         InitializeComponent();
      }

      private void Form1_Load(object sender, EventArgs e) {
         nameBindingSource.DataSource = typeof(IName);
         tbFirstName.DataBindings.Add("Text", nameBindingSource, "FirstName");
         tbLastName.DataBindings.Add("Text", nameBindingSource, "LastName");

         dataGridView1.DataSource = BOL.DA.Names;
         dataGridView1.Columns["FullName"].HeaderText = "Full Name";
         dataGridView1.Columns["FirstName"].Visible = false;
         dataGridView1.Columns["LastName"].Visible = false;
      }

      private void btSave_Click(object sender, EventArgs e) {
         if (0 == dataGridView1.SelectedRows.Count) return;
         var selected = dataGridView1.SelectedRows[0].DataBoundItem;
         if (null == selected) {
            int currentRowIndex = dataGridView1.NewRowIndex;
            BOL.DA.Names.Add(tbFirstName.Text, tbLastName.Text);
            dataGridView1.Rows[currentRowIndex].Selected = true;

         } else {
            int currentRowIndex = dataGridView1.SelectedRows[0].Index;
            BOL.DA.Names.Update(selected);
            BOL.DA.Names.ResetItem(currentRowIndex);
         }
      }

      private void dataGridView1_SelectionChanged(object sender, EventArgs e) {
         DataGridView dgv = (DataGridView)sender;
         IName selectedItem = (IName)dgv.SelectedRows[0].DataBoundItem;
         if (selectedItem != null) nameBindingSource.DataSource = selectedItem;
         else nameBindingSource.DataSource = typeof(IName);
      }
   }
}
