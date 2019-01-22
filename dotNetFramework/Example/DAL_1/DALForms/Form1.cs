using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DALForms {
   using System.Configuration;
   using BO;

   public partial class Form1 : Form {
      private BindingSource foreclosureBindingSource = new BindingSource();
      public Form1() {
         InitializeComponent();
      }

      private void Form1_Load(object sender, EventArgs e) {
         dataGridView1.DataSource = BOL.DA.ForeclosureSales;
         foreclosureBindingSource.DataSource = typeof(IForeclosureSaleHeader);
         textBox1.DataBindings.Add("Text", foreclosureBindingSource, "FileNumber");
         textBox2.DataBindings.Add("Text", foreclosureBindingSource, "City");
         textBox3.DataBindings.Add("Text", foreclosureBindingSource, "Active");
      }

      private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
         DataGridView dgv = (DataGridView)sender;
         IForeclosureSaleHeader selected = (IForeclosureSaleHeader)dgv.Rows[e.RowIndex].DataBoundItem;
         BOL.DA.ForeclosureSales.Open(selected);
         foreclosureBindingSource.DataSource = BOL.DA.CurrentForeclosureSale;
         dataGridView2.DataSource = BOL.DA.CurrentForeclosureSale.Sales;
      }

   }
}
