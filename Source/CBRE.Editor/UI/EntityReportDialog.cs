using CBRE.Common.Mediator;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Actions.MapObjects.Operations;
using CBRE.Editor.Actions.MapObjects.Selection;
using CBRE.Editor.Documents;
using CBRE.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace CBRE.Editor.UI
{
    public partial class EntityReportDialog : HotkeyForm, IMediatorListener
    {
        private class ColumnComparer : IComparer
        {
            public int Column { get; set; }
            public SortOrder SortOrder { get; set; }

            public ColumnComparer(int column)
            {
                Column = column;
                SortOrder = SortOrder.Ascending;
            }

            public int Compare(object x, object y)
            {
                ListViewItem i1 = (ListViewItem)x;
                ListViewItem i2 = (ListViewItem)y;
                int compare = String.CompareOrdinal(i1.SubItems[Column].Text, i2.SubItems[Column].Text);
                return SortOrder == SortOrder.Descending ? -compare : compare;
            }
        }

        private readonly ColumnComparer _sorter;

        public EntityReportDialog()
        {
            InitializeComponent();
            _sorter = new ColumnComparer(0);
            EntityList.ListViewItemSorter = _sorter;
        }

        protected override void OnLoad(EventArgs e)
        {
            Mediator.Subscribe(EditorMediator.SelectionChanged, this);
            Mediator.Subscribe(EditorMediator.DocumentActivated, this);
            Mediator.Subscribe(EditorMediator.DocumentTreeStructureChanged, this);
            Mediator.Subscribe(EditorMediator.DocumentTreeObjectsChanged, this);
            Mediator.Subscribe(EditorMediator.EntityDataChanged, this);
            ResetFilters(null, null);
            base.OnLoad(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            Mediator.UnsubscribeAll(this);
            base.OnClosed(e);
        }

        public void Notify(string message, object data)
        {
            Mediator.ExecuteDefault(this, message, data);
        }

        public void DocumentActivated()
        {
            FiltersChanged(null, null);
        }

        public void SelectionChanged()
        {
            if (!FollowSelection.Checked) return;
            MapObject selection = DocumentManager.CurrentDocument.Selection.GetSelectedObjects().LastOrDefault(x => x is Entity);
            SetSelected(selection);
        }

        private void DocumentTreeStructureChanged()
        {
            FiltersChanged(null, null);
        }

        private void DocumentTreeObjectsChanged(IEnumerable<MapObject> objects)
        {
            if (objects.Any(x => x is Entity))
            {
                FiltersChanged(null, null);
            }
        }

        private void EntityDataChanged(IEnumerable<MapObject> objects)
        {
            FiltersChanged(null, null);
        }

        private Entity GetSelected()
        {
            return EntityList.SelectedItems.Count == 0 ? null : (Entity)EntityList.SelectedItems[0].Tag;
        }

        private void SetSelected(MapObject selection)
        {
            if (selection == null) return;

            ListViewItem item = EntityList.Items.OfType<ListViewItem>().FirstOrDefault(x => x.Tag == selection);
            if (item == null) return;

            item.Selected = true;
            EntityList.EnsureVisible(EntityList.Items.IndexOf(item));
        }

        private void FiltersChanged(object sender, EventArgs e)
        {
            EntityList.BeginUpdate();
            Entity selected = GetSelected();
            EntityList.ListViewItemSorter = null;
            EntityList.Items.Clear();

            ListViewItem[] items = DocumentManager.CurrentDocument.Map.WorldSpawn
                .Find(x => x is Entity)
                .OfType<Entity>()
                .Where(DoFilters)
                .Select(GetListItem)
                .ToArray();
            EntityList.Items.AddRange(items);

            EntityList.ListViewItemSorter = _sorter;
            EntityList.Sort();
            SetSelected(selected);
            EntityList.EndUpdate();
        }

        private ListViewItem GetListItem(Entity entity)
        {
            Property targetname = entity.EntityData.Properties.FirstOrDefault(x => x.Key.ToLower() == "targetname");
            return new ListViewItem(new[]
                                        {
                                            entity.EntityData.Name,
                                            targetname == null ? "" : targetname.Value
                                        })
            { Tag = entity };
        }

        private bool DoFilters(Entity ent)
        {
            bool hasChildren = ent.HasChildren;

            if (hasChildren && TypePoint.Checked) return false;
            if (!hasChildren && TypeBrush.Checked) return false;
            if (!IncludeHidden.Checked && ent.IsVisgroupHidden) return false;

            string classFilter = FilterClass.Text.ToUpperInvariant();
            bool exactClass = FilterClassExact.Checked;
            string keyFilter = FilterKey.Text.ToUpperInvariant();
            string valueFilter = FilterValue.Text.ToUpperInvariant();
            bool exactKeyValue = FilterKeyValueExact.Checked;

            if (!String.IsNullOrWhiteSpace(classFilter))
            {
                string name = (ent.EntityData.Name ?? "").ToUpperInvariant();
                if (exactClass && name != classFilter) return false;
                if (!exactClass && !name.Contains(classFilter)) return false;
            }

            if (!String.IsNullOrWhiteSpace(keyFilter))
            {
                Property prop = ent.EntityData.Properties.FirstOrDefault(x => x.Key.ToUpperInvariant() == keyFilter);
                if (prop == null) return false;
                string val = prop.Value.ToUpperInvariant();
                if (exactKeyValue && val != valueFilter) return false;
                if (!exactKeyValue && !val.Contains(valueFilter)) return false;
            }

            return true;
        }

        private void ResetFilters(object sender, EventArgs e)
        {
            TypeAll.Checked = true;
            IncludeHidden.Checked = true;
            FilterKeyValueExact.Checked = false;
            FilterClassExact.Checked = false;
            FilterKey.Text = "";
            FilterValue.Text = "";
            FilterClass.Text = "";
            FiltersChanged(null, null);
        }

        private void SortByColumn(object sender, ColumnClickEventArgs e)
        {
            if (_sorter.Column == e.Column)
            {
                _sorter.SortOrder = _sorter.SortOrder == SortOrder.Descending
                                        ? SortOrder.Ascending
                                        : SortOrder.Descending;
            }
            else
            {
                _sorter.Column = e.Column;
                _sorter.SortOrder = SortOrder.Ascending;
            }
            EntityList.Sort();
            SetSelected(GetSelected()); // Reset the scroll value
        }

        private void SelectEntity(Entity sel)
        {
            IEnumerable<MapObject> currentSelection = DocumentManager.CurrentDocument.Selection.GetSelectedObjects();
            ChangeSelection change = new ChangeSelection(sel.FindAll(), currentSelection);
            DocumentManager.CurrentDocument.PerformAction("Select entity", change);
        }

        private void GoToSelectedEntity(object sender, EventArgs e)
        {
            Entity selected = GetSelected();
            if (selected == null) return;
            SelectEntity(selected);
            Mediator.Publish(HotkeysMediator.CenterAllViewsOnSelection);
        }

        private void DeleteSelectedEntity(object sender, EventArgs e)
        {
            Entity selected = GetSelected();
            if (selected == null) return;
            DocumentManager.CurrentDocument.PerformAction("Delete entity", new Delete(new[] { selected.ID }));
        }

        private void OpenEntityProperties(object sender, EventArgs e)
        {
            Entity selected = GetSelected();
            if (selected == null) return;
            SelectEntity(selected);
            Mediator.Publish(HotkeysMediator.ObjectProperties);
        }

        private void CloseButtonClicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}
