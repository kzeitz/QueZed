using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karlton.Utility.WindowsForms {
    using System.ComponentModel;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Drawing.Design;

    public abstract class DropDownTypeEditor : UITypeEditor {
        private IWindowsFormsEditorService editorService;

        private void ListBox_SelectedIndexChanged(object objSender, EventArgs eventArgs) { if (editorService != null)  editorService.CloseDropDown(); }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
            if (context != null && context.Instance != null) return UITypeEditorEditStyle.DropDown;
            return base.GetEditStyle(context);
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
            if (context != null && context.Instance != null && context.Container != null && provider != null) {
                editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                if (editorService != null) {
                    using (ListBox listBox = new ListBox()) {
                        listBox.BorderStyle = BorderStyle.None;
                        FillListBox(listBox, context, value);
                        listBox.SelectedIndexChanged += new EventHandler(ListBox_SelectedIndexChanged);
                        editorService.DropDownControl(listBox);
                        if (listBox.SelectedItem != null) value = GetValueFromListItem(context, listBox.SelectedItem);
                        listBox.SelectedIndexChanged -= new EventHandler(ListBox_SelectedIndexChanged);
                    }
                    editorService = null;
                }
            }
            return value;
        }

        protected abstract void FillListBox(ListBox listBox, ITypeDescriptorContext context, object value);

        protected virtual object GetValueFromListItem(ITypeDescriptorContext context, object value) { return value; }
    }
}
