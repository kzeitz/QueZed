using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ContactsForms {

    using BO;
    using System.ComponentModel;
    
    class Program : Karlton.Utility.Program.Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) { new Program().Run(args); }
        protected override void main(string[] args) {
            BOL bol = new BOL(Program.DefaultConnectionString);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public class TreeViewBinding<TDataItem> where TDataItem : class {
        private TreeView treeView;
        private TreeNodeCollection treeNodeCollection;
        private BindingSource bindingSource;
        private Func<object, TDataItem> getDataItemFunc;
        private Func<TDataItem, TreeNode> addTreeNodeFunc;
        private Action<TDataItem, TreeNode> updateTreeNodeAction;
        private TreeNode currentAddItem;
        private TreeNode parentTreeNode;

        public TreeViewBinding(TreeView treeView, BindingSource bindingSource, Func<object, TDataItem> getDataItemFunc, Func<TDataItem, TreeNode> addTreeNodeFunc, Action<TDataItem, TreeNode> updateTreeNodeAction) : this(treeView, treeView.Nodes, null, bindingSource, getDataItemFunc, addTreeNodeFunc, updateTreeNodeAction) { }
        public TreeViewBinding(TreeNode parentTreeNode, BindingSource bindingSource, Func<object, TDataItem> getDataItemFunc, Func<TDataItem, TreeNode> addTreeNodeFunc, Action<TDataItem, TreeNode> updateTreeNodeAction) : this(parentTreeNode.TreeView, parentTreeNode.Nodes, parentTreeNode, bindingSource, getDataItemFunc, addTreeNodeFunc, updateTreeNodeAction) { }
        private TreeViewBinding(TreeView treeView, TreeNodeCollection treeNodeCollection, TreeNode parentTreeNode, BindingSource bindingSource, Func<object, TDataItem> getDataItemFunc, Func<TDataItem, TreeNode> addTreeNodeFunc, Action<TDataItem, TreeNode> updateTreeNodeAction) {
            if (treeView == null) throw new ArgumentNullException("treeView");
            if (treeNodeCollection == null) throw new ArgumentNullException("treeNodeCollection ");
            if (bindingSource == null) throw new ArgumentNullException("bindingSource");
            if (bindingSource == null) throw new ArgumentNullException("bindingSource");
            if (getDataItemFunc == null) throw new ArgumentNullException("getDataItemFunc");
            if (addTreeNodeFunc == null) throw new ArgumentNullException("addTreeNodeFunc");
            if (updateTreeNodeAction == null) throw new ArgumentNullException("updateTreeNodeAction");
            this.treeView = treeView;
            this.treeNodeCollection = treeNodeCollection;
            this.parentTreeNode = parentTreeNode; // may be null.
            this.bindingSource = bindingSource;
            this.getDataItemFunc = getDataItemFunc;
            this.addTreeNodeFunc = addTreeNodeFunc;
            this.updateTreeNodeAction = updateTreeNodeAction;
            // sync to binding source's current items and selection.
            addExistingItems();
            bindingSource.ListChanged += (s, e) => {
                switch (e.ListChangedType) {
                case ListChangedType.ItemAdded: addItem(e.NewIndex); selectItem(); break;
                case ListChangedType.ItemChanged: updateItem(); break;
                case ListChangedType.ItemDeleted: deleteItem(e.NewIndex); break;
                case ListChangedType.ItemMoved: moveItem(e.OldIndex, e.NewIndex); break;
                case ListChangedType.PropertyDescriptorAdded:
                case ListChangedType.PropertyDescriptorChanged:
                case ListChangedType.PropertyDescriptorDeleted: break;
                case ListChangedType.Reset: addExistingItems(); selectItem(); break;
                default: throw new NotImplementedException("...");
                }
            };
            bindingSource.PositionChanged += (s, e) => selectItem();
            treeView.AfterSelect += afterNodeSelect;
            selectItem();
        }

        private void afterNodeSelect(object sender, TreeViewEventArgs e) {
            var treeNode = e.Node;
            // Skip, if the TreeNode belongs to a foreign collection.
            if (treeNode.Parent != parentTreeNode) return;
            bindingSource.Position = treeNode.Index;
        }

        private void addExistingItems() {
            foreach (var listItem in bindingSource.List) {
                TDataItem dataItem = getDataItemFunc(listItem);
                TreeNode treeNode = addTreeNodeFunc(dataItem);
                if (treeNode == null) continue;
                updateTreeNodeAction(dataItem, treeNode);
                treeNodeCollection.Add(treeNode);
            }
        }

        private void addItem(int newIndex) {
            TDataItem dataItem = getDataItemFunc(bindingSource.Current);
            if (currentAddItem == null) {
                TreeNode treeNode = addTreeNodeFunc(dataItem);
                if (treeNode == null) return;
                treeNodeCollection.Insert(newIndex, treeNode);
                currentAddItem = treeNode;
                return;
            }
            updateTreeNodeAction(dataItem, currentAddItem);
            currentAddItem = null;
        }

        private void updateItem() {
            if (bindingSource.Current == null) return;
            TDataItem dataItem = getDataItemFunc(bindingSource.Current);
            var treeNode = treeNodeCollection[bindingSource.Position];
            updateTreeNodeAction(dataItem, treeNode);
        }

        private void deleteItem(int index) {
            treeNodeCollection.RemoveAt(index);
            currentAddItem = null;
        }

        private void moveItem(int oldIndex, int newIndex) {
            var treeNode = treeNodeCollection[bindingSource.Position];
            treeNodeCollection.RemoveAt(oldIndex);
            treeNodeCollection.Insert(newIndex, treeNode);
        }

        private void selectItem() {
            if (bindingSource.Position < 0) return;
            if (treeNodeCollection.Count <= bindingSource.Position) return;
            var treeNode = treeNodeCollection[bindingSource.Position];
            treeNode.EnsureVisible();
            treeView.SelectedNode = treeNode;
        }
    }
}
