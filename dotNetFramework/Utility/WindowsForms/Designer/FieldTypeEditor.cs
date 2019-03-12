﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karlton.Utility.WindowsForms {
    using System.Windows.Forms;
    using System.ComponentModel;
    public class FieldTypeEditor : DropDownTypeEditor {
        protected override void FillListBox(ListBox listBox, ITypeDescriptorContext context, object value) {
            string selectedFiled = (string)value;
            if (selectedFiled == null) selectedFiled = string.Empty;

            PropertyDescriptor dataSourceDescriptor = TypeDescriptor.GetProperties(context.Instance)["DataSource"];
            PropertyDescriptor dataMemberDescriptor = TypeDescriptor.GetProperties(context.Instance)["DataMember"];
            if (dataSourceDescriptor == null || dataMemberDescriptor == null) return;
            object dataSource = dataSourceDescriptor.GetValue(context.Instance);
            string dataMember = (string)dataMemberDescriptor.GetValue(context.Instance);
            if (dataSource != null) {
                CurrencyManager currencyManager = new BindingContext()[dataSource, dataMember] as CurrencyManager;
                if (currencyManager != null) {
                    int lastIndex;
                    foreach (PropertyDescriptor descriptor in currencyManager.GetItemProperties()) {
                        lastIndex = listBox.Items.Add(descriptor.Name);
                        if (string.Compare(descriptor.Name, selectedFiled) == 0) listBox.SelectedIndex = lastIndex;
                    }
                }
            }
        }
    }
}