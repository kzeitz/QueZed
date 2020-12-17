using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Karlton.Utility.WindowsForms {
    using System.Runtime.InteropServices;
    using System.Collections;
    using System.Drawing.Design;

    public partial class DataTreeView : TreeView {
        const int SB_HORZ = 0;

        private object dataSource;
        private string dataMember;
        private CurrencyManager listManager;

        private string parentKeyPropertyName;
        private string keyPropertyName;
        private string namePropertyName;
        private string valuePropertyName;
        private string isLeafPropertyName;

        private PropertyDescriptor parentKeyProperty;
        private PropertyDescriptor keyProperty;
        private PropertyDescriptor nameProperty;
        private PropertyDescriptor valueProperty;
        private PropertyDescriptor isLeafProperty;

        private TypeConverter valueConverter;

        private SortedList items_Positions;
        private SortedList items_Identifiers;

        private bool selectionChanging;
        public DataTreeView() {
            InitializeComponent();

            parentKeyPropertyName = string.Empty;
            keyPropertyName = string.Empty;
            namePropertyName = string.Empty;

            items_Positions = new SortedList();
            items_Identifiers = new SortedList();
            selectionChanging = false;

            FullRowSelect = true;
            HideSelection = false;
            HotTracking = true;
            AfterSelect += new TreeViewEventHandler(dataTreeView_AfterSelect);
            BindingContextChanged += new EventHandler(dataTreeView_BindingContextChanged);
            AfterLabelEdit += new NodeLabelEditEventHandler(dataTreeView_AfterLabelEdit);
            BeforeExpand += new TreeViewCancelEventHandler(dataTreeView_BeforeExpand);
            BeforeCollapse += new TreeViewCancelEventHandler(dataTreeView_BeforeCollapse);
        }

        [DllImport("User32.dll")]
        static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        public class DataTreeViewNode : TreeNode {
            private int position;
            private object parentKey;
            private bool isLeaf;

            public DataTreeViewNode(int position) { this.position = position; }
            public object Key { get { return Tag; } set { Tag = value; } }
            public object ParentKey { get { return parentKey; } set { parentKey = value; } }
            public int Position { get { return position; } set { position = value; } }
            public bool IsLeaf { 
                get { return isLeaf; } 
                set { 
                    isLeaf = value;
                    if (isLeaf) { ImageIndex = 2; SelectedImageIndex = 3; } else ImageIndex = SelectedImageIndex = 0;
                } 
            }
        }

        [
        DefaultValue((string)null), TypeConverter("Design.DataSourceConverter, System.Design"),
        Editor("Design.DataSourceListEditor, System.Design", typeof(UITypeEditor)),
        RefreshProperties(RefreshProperties.Repaint),
        Category("Data"),
        Description("Data source of the tree.")
        ]
        public object DataSource { get { return dataSource; } set { if (dataSource != value) dataSource = value; ResetData(); } }

        [
        DefaultValue(""),
        Editor("Design.DataMemberListEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)),
        RefreshProperties(RefreshProperties.Repaint),
        Category("Data"),
        Description("Data member of the tree.")
        ]
        public string DataMember { get { return dataMember; } set { if (dataMember != value) dataMember = value; ResetData(); } }

        [
        DefaultValue(""),
        Editor(typeof(FieldTypeEditor), typeof(UITypeEditor)),
        Category("Data"),
        Description("Identifier of the parent. Note: this member must have the same type as identifier column.")
        ]
        public string ParentKeyColumn {
            get { return parentKeyPropertyName; }
            set {
                if (parentKeyPropertyName != value) {
                    parentKeyPropertyName = value;
                    parentKeyProperty = null;
                    ResetData();
                }
            }
        }

        [
        DefaultValue(""),
        Editor(typeof(FieldTypeEditor), typeof(UITypeEditor)),
        Category("Data"),
        Description("Identifier member, in most cases this is primary column of the table.")
        ]
        public string KeyColumn {
            get { return keyPropertyName; }
            set {
                if (keyPropertyName != value) {
                    keyPropertyName = value;
                    keyProperty = null;
                    if (valuePropertyName == null || valuePropertyName.Length == 0) ValueColumn = keyPropertyName;
                    ResetData();
                }
            }
        }

        [
        DefaultValue(""),
        Editor(typeof(FieldTypeEditor), typeof(UITypeEditor)),
        Category("Data"),
        Description("Name member. Note: editing of this column available only with types that support converting from string.")
        ]
        public string NameColumn {
            get { return namePropertyName; }
            set {
                if (namePropertyName != value) {
                    namePropertyName = value;
                    nameProperty = null;
                    ResetData();
                }
            }
        }

        [
        DefaultValue(""),
        Editor(typeof(FieldTypeEditor), typeof(UITypeEditor)),
        Category("Data"),
        Description("IsLeaf member. Note: setting this to true will cause node to be created with index 2 and index 3 of image list.")
        ]
        public string IsLeafColumn {
            get { return isLeafPropertyName; }
            set {
                if (isLeafPropertyName != value) {
                    isLeafPropertyName = value;
                    isLeafProperty = null;
                    ResetData();
                }
            }
        }

        [
        DefaultValue(""),
        Editor(typeof(FieldTypeEditor), typeof(UITypeEditor)),
        Category("Data"),
        Description("Value member. Form this column value will be taken.")
        ]
        public string ValueColumn {
            get { return valuePropertyName; }
            set {
                if (valuePropertyName != value) {
                    valuePropertyName = value;
                    valueProperty = null;
                    valueConverter = null;
                }
            }
        }

        [
        Category("Data"),
        Description("Get value current selected item.")
        ]
        public object Value {
            get {
                if (SelectedNode != null) {
                    DataTreeViewNode node = SelectedNode as DataTreeViewNode;
                    if (node != null && prepareValueDescriptor()) return valueProperty.GetValue(listManager.List[node.Position]);
                }
                return null;
            }
        }

        private void dataTreeView_AfterSelect(object sender, TreeViewEventArgs e) {
            beginSelectionChanging();
            DataTreeViewNode node = e.Node as DataTreeViewNode;
            if (node != null) listManager.Position = node.Position;
            endSelectionChanging();
        }

        private void dataTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
            DataTreeViewNode node = e.Node as DataTreeViewNode;
            if (node != null) {
                if (prepareValueConvertor() && valueConverter.IsValid(e.Label)) {
                    nameProperty.SetValue(listManager.List[node.Position], valueConverter.ConvertFromString(e.Label));
                    listManager.EndCurrentEdit();
                    return;
                }
            }
            e.CancelEdit = true;
        }

        private void dataTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
            DataTreeViewNode node = e.Node as DataTreeViewNode;
            if (node != null && !node.IsLeaf) e.Node.ImageIndex = e.Node.SelectedImageIndex = 1;
        }

        private void dataTreeView_BeforeCollapse(object sender, TreeViewCancelEventArgs e) {
            DataTreeViewNode node = e.Node as DataTreeViewNode;
            if (node != null && !node.IsLeaf) e.Node.ImageIndex = e.Node.SelectedImageIndex = 0;
        }

        private void dataTreeView_BindingContextChanged(object sender, System.EventArgs e) {
            ResetData();
        }

        private void listManager_PositionChanged(object sender, EventArgs e) { synchronizeSelection(); }

        private void dataTreeView_ListChanged(object sender, ListChangedEventArgs e) {
            switch (e.ListChangedType) {
                case ListChangedType.ItemAdded:
                if (!tryAddNode(createNode(listManager, e.NewIndex))) throw new ApplicationException("Item are not added.");
                break;
                case ListChangedType.ItemChanged:
                    DataTreeViewNode chnagedNode = items_Positions[e.NewIndex] as DataTreeViewNode;
                    if (chnagedNode != null) {
                        refreshData(chnagedNode);
                        changeParent(chnagedNode);
                    } else throw new ApplicationException("Item not found or wrong type.");
                break;
                case ListChangedType.ItemMoved:
                    DataTreeViewNode movedNode = items_Positions[e.OldIndex] as DataTreeViewNode;
                    if (movedNode != null) {
                        items_Positions.Remove(e.OldIndex);
                        items_Positions.Add(e.NewIndex, movedNode);
                    } else throw new ApplicationException("Item not found or wrong type.");
                break;
                case ListChangedType.ItemDeleted:
                    DataTreeViewNode deletedNode = items_Positions[e.OldIndex] as DataTreeViewNode;
                    if (deletedNode != null) {
                        items_Positions.Remove(e.OldIndex);
                        items_Identifiers.Remove(deletedNode.Key);
                        deletedNode.Remove();
                    } else throw new ApplicationException("Item not found or wrong type.");
                break;
                case ListChangedType.Reset:
                    ResetData();
                break;
            }
        }

        private void clear() {
            items_Positions.Clear();
            items_Identifiers.Clear();
            Nodes.Clear();
        }

        private bool prepareDataSource() {
            if (BindingContext != null) {
                if (dataSource != null) {
                    listManager = BindingContext[dataSource, dataMember] as CurrencyManager;
                    return true;
                } else {
                    listManager = null;
                    clear();
                }
            }
            return false;
        }

        private bool PrepareDescriptors() {
            if (keyPropertyName.Length != 0 && namePropertyName.Length != 0 && parentKeyPropertyName.Length != 0) {
                if (keyProperty == null) keyProperty = listManager.GetItemProperties()[keyPropertyName];
                if (nameProperty == null) nameProperty = listManager.GetItemProperties()[namePropertyName];
                if (parentKeyProperty == null) parentKeyProperty = listManager.GetItemProperties()[parentKeyPropertyName];
                if (isLeafProperty == null) isLeafProperty = listManager.GetItemProperties()[isLeafPropertyName];
            }
            return (keyProperty != null && nameProperty != null && parentKeyProperty != null);
        }

        private bool prepareValueDescriptor() {
            if (valueProperty == null) {
                if (valuePropertyName == string.Empty) valuePropertyName = keyPropertyName;
                valueProperty = listManager.GetItemProperties()[valuePropertyName];
            }
            return (valueProperty != null);
        }

        private bool prepareValueConvertor() {
            if (valueConverter == null) valueConverter = TypeDescriptor.GetConverter(nameProperty.PropertyType) as TypeConverter;
            return (valueConverter != null && valueConverter.CanConvertFrom(typeof(string)));
        }

        private void connectDataSource() {
            listManager.PositionChanged += new EventHandler(listManager_PositionChanged);
            ((IBindingList)listManager.List).ListChanged += new ListChangedEventHandler(dataTreeView_ListChanged);
        }

        private void ResetData() {
            BeginUpdate();
            clear();
            if (prepareDataSource()) {
                connectDataSource();
                if (PrepareDescriptors()) {
                    ArrayList unsortedNodes = new ArrayList();
                    for (int i = 0; i < listManager.Count; ++i) unsortedNodes.Add(createNode(listManager, i));
                    int startCount;
                    while (unsortedNodes.Count > 0) {
                        startCount = unsortedNodes.Count;
                        for (int i = unsortedNodes.Count - 1; i >= 0; --i) if (tryAddNode((DataTreeViewNode)unsortedNodes[i])) unsortedNodes.RemoveAt(i);
                        if (startCount == unsortedNodes.Count) throw new ApplicationException("Tree view confused when try to make your data hierarchical.");
                    }
                }
            }
            EndUpdate();
        }

        private bool tryAddNode(DataTreeViewNode node) {
            if (IsIDNull(node.ParentKey)) {
                addNode(Nodes, node);
                return true;
            } else {
                if (items_Identifiers.ContainsKey(node.ParentKey)) {
                    TreeNode parentNode = items_Identifiers[node.ParentKey] as TreeNode;
                    if (parentNode != null) {
                        addNode(parentNode.Nodes, node);
                        return true;
                    }
                }
            }
            return false;
        }

        private void addNode(TreeNodeCollection nodes, DataTreeViewNode node) {
            items_Positions.Add(node.Position, node);
            items_Identifiers.Add(node.Key, node);
            nodes.Add(node);
        }

        private void changeParent(DataTreeViewNode node) {
            object dataParentID = parentKeyProperty.GetValue(listManager.List[node.Position]);
            if (node.ParentKey != dataParentID) {
                DataTreeViewNode newParentNode = items_Identifiers[dataParentID] as DataTreeViewNode;
                if (newParentNode != null) {
                    node.Remove();
                    newParentNode.Nodes.Add(node);
                } else throw new ApplicationException("Item not found or wrong type.");
            }
        }

        private void synchronizeSelection() {
            if (!selectionChanging) {
                DataTreeViewNode node = items_Positions[listManager.Position] as DataTreeViewNode;
                if (node != null) SelectedNode = node;
            }
        }

        private void refreshData(DataTreeViewNode node) {
            int position = node.Position;
            node.Key = keyProperty.GetValue(listManager.List[position]);
            node.Text = (string)nameProperty.GetValue(listManager.List[position]);
            node.ParentKey = parentKeyProperty.GetValue(listManager.List[position]);
            node.IsLeaf = (bool)isLeafProperty.GetValue(listManager.List[position]);
        }

        private DataTreeViewNode createNode(CurrencyManager currencyManager, int position) {
            DataTreeViewNode node = new DataTreeViewNode(position);
            refreshData(node);
            return node;
        }
        
        private object getPerentKey(CurrencyManager currencyManager, int position) { return parentKeyProperty.GetValue(currencyManager.List[position]); }

        private bool IsIDNull(object id) {
            if (id == null || Convert.IsDBNull(id)) return true;
            else {
                if (id.GetType() == typeof(string)) return (((string)id).Length == 0);
                if (id.GetType() == typeof(Guid)) return ((Guid)id == Guid.Empty);
            }
            return false;
        }

        protected override void InitLayout() {
            base.InitLayout();
            ShowScrollBar(Handle, SB_HORZ, false);
        }

        private void beginSelectionChanging() { selectionChanging = true; }

        private void endSelectionChanging() { selectionChanging = false; }

    }
}
