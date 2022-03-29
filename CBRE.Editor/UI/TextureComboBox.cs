using CBRE.Editor.Documents;
using CBRE.Providers.Texture;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CBRE.Editor.UI
{
    [ToolboxBitmap(typeof(ComboBox))]
    public sealed class TextureComboBox : ComboBox
    {
        private class TextureComboBoxItem
        {
            public TextureItem Item { get; set; }
            public bool IsHistory { get; set; }
            public bool DrawBorder { get; set; }

            public override string ToString()
            {
                return Item.Name;
            }
        }

        private List<TexturePackage> _packages;
        private ITextureStreamSource _streamSource;
        private int _fontHeight;

        public TextureComboBox()
        {
            DrawMode = DrawMode.OwnerDrawVariable;
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawItem += OwnerDrawItem;
            MeasureItem += OwnerMeasureItem;
            DropDown += OpenStream;
            DropDownClosed += CloseStream;
            _packages = new List<TexturePackage>();

            _fontHeight = (int)Font.GetHeight();
            FontChanged += (s, e) => _fontHeight = (int)Font.GetHeight();
        }

        private void OpenStream(object sender, EventArgs e)
        {
            if (DocumentManager.CurrentDocument == null || !DocumentManager.CurrentDocument.TextureCollection.Packages.Any()) return;
            _streamSource = DocumentManager.CurrentDocument.TextureCollection.GetStreamSource(64, 64,
                _packages.Union(DocumentManager.CurrentDocument.TextureCollection.GetRecentTextures().Select(x => x.Package).Distinct()));
        }

        private void CloseStream(object sender, EventArgs e)
        {
            if (_streamSource == null) return;
            _streamSource.Dispose();
            _streamSource = null;
        }

        protected override void OnSelectionChangeCommitted(EventArgs e)
        {
            TextureComboBoxItem si = (TextureComboBoxItem)SelectedItem;
            SetHistory(si.Item);
            base.OnSelectionChangeCommitted(e);
        }

        private void SetHistory(TextureItem ti)
        {
            if (ti == null) return;
            TextureComboBoxItem rem = FindItem(ti.Name, true) ?? GetTexture(ti, true);
            Items.Remove(rem);
            Items.Insert(0, rem);
            FixHistoryBorder();
            SelectedItem = rem;
        }

        public TextureItem GetSelectedTexture()
        {
            TextureComboBoxItem si = SelectedItem as TextureComboBoxItem;
            return si == null ? null : si.Item;
        }

        public void SetSelectedTexture(TextureItem selection)
        {
            SetHistory(selection);
        }

        private TextureComboBoxItem FindItem(string name, bool historyOnly = false)
        {
            return Items.OfType<TextureComboBoxItem>().FirstOrDefault(x => x.Item.Name == name && (!historyOnly || x.IsHistory));
        }

        private void FixHistoryBorder()
        {
            foreach (TextureComboBoxItem db in Items.OfType<TextureComboBoxItem>().Where(x => x.DrawBorder))
            {
                db.DrawBorder = false;
            }

            if (DocumentManager.CurrentDocument == null) return;

            TextureItem lh = DocumentManager.CurrentDocument.TextureCollection.GetRecentTextures().LastOrDefault();
            if (lh == null) return;

            TextureComboBoxItem ot = Items.OfType<TextureComboBoxItem>().FirstOrDefault(x => x.Item.Name == lh.Name && x.IsHistory);
            if (ot != null) ot.DrawBorder = true;
        }

        public void Update(string package)
        {
            // todo: optimisation point 1

            if (DocumentManager.CurrentDocument == null)
            {
                Items.Clear();
                return;
            }
            List<TexturePackage> packs = DocumentManager.CurrentDocument.TextureCollection.Packages.Where(x => x.ToString() == package).ToList();
            if (!packs.Any()) packs.AddRange(DocumentManager.CurrentDocument.TextureCollection.Packages);

            _packages = packs;

            List<TextureComboBoxItem> itemsList = new List<TextureComboBoxItem>();

            TextureComboBoxItem selected = SelectedItem as TextureComboBoxItem;
            string selectedName = selected == null ? null : selected.Item.Name;
            TextureComboBoxItem reselect = null;
            List<TextureItem> history = DocumentManager.CurrentDocument.TextureCollection.GetRecentTextures().ToList();
            TextureItem last = history.LastOrDefault();
            if (selectedName != null)
            {
                TextureComboBoxItem item = GetTexture(selectedName, false);
                if (item != null && item.Item != null)
                {
                    itemsList.Add(item);
                    reselect = item;
                    if (last == null) item.DrawBorder = true;
                }
            }
            foreach (TextureItem hi in history)
            {
                TextureComboBoxItem item = GetTexture(hi.Name, true);
                if (item.Item == null) continue;
                if (hi == last) item.DrawBorder = true;
                itemsList.Add(item);
                if (reselect == null && selectedName == item.Item.Name) reselect = item;
            }
            IOrderedEnumerable<TextureItem> textures = _packages.SelectMany(x => x.Items).Select(x => x.Value).OrderBy(x => x.Name);
            foreach (TextureComboBoxItem item in textures.Select(ti => GetTexture(ti.Name, false)).Where(x => x.Item != null))
            {
                itemsList.Add(item);
                if (reselect == null && selectedName == item.Item.Name) reselect = item;
            }

            BeginUpdate();
            Items.Clear();
            Items.AddRange(itemsList.OfType<object>().ToArray());
            EndUpdate();

            SelectedItem = null;
        }

        private static TextureComboBoxItem GetTexture(string name, bool isHistory)
        {
            TextureItem item = DocumentManager.CurrentDocument == null ? null : DocumentManager.CurrentDocument.TextureCollection.GetItem(name);
            return new TextureComboBoxItem { DrawBorder = false, IsHistory = isHistory, Item = item };
        }

        private TextureComboBoxItem GetTexture(TextureItem item, bool isHistory)
        {
            return new TextureComboBoxItem { DrawBorder = false, IsHistory = isHistory, Item = item };
        }

        private void OwnerMeasureItem(object sender, MeasureItemEventArgs e)
        {
            const int maxSize = 64;

            TextureComboBoxItem item = (TextureComboBoxItem)Items[e.Index];

            int iw = item.Item.Width;
            int height = item.Item.Height;
            int minHeight = _fontHeight * 2;

            if (iw > maxSize && iw >= height) height = (int)Math.Floor(maxSize * (height / (float)iw));
            else if (height > maxSize) height = maxSize;

            e.ItemHeight = Math.Max(minHeight, height + _fontHeight) + 9;
        }

        private void OwnerDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            TextureComboBoxItem item = (TextureComboBoxItem)Items[e.Index];

            e.DrawBackground();

            if (_streamSource == null || e.Bounds.Height == ItemHeight)
            {
                // Drop down is closed or we're painting on the control (the selected item)
                using (SolidBrush brush = new SolidBrush(e.ForeColor))
                {
                    e.Graphics.DrawString(item.Item.Name, e.Font, brush, e.Bounds.X, e.Bounds.Y);
                }
            }
            else
            {
                // Drop down is open and we're painting in the drop down area
                BitmapRef bmp = _streamSource.GetImage(item.Item);
                OwnerDrawItem(e.Graphics, bmp.Bitmap, item.Item, e.Bounds, e.ForeColor, e.Font, item.DrawBorder);
            }

            e.DrawFocusRectangle();
        }

        private void OwnerDrawItem(System.Drawing.Graphics g, Image bmp, TextureItem ti, Rectangle bounds, Color textColour, Font font, bool drawBorder)
        {
            if (bmp == null) return;
            int lineHeight = (int)Font.GetHeight();
            int imageSize = bounds.Height - lineHeight - 9;

            int iw = bmp.Width;
            int ih = bmp.Height;
            if (iw > imageSize && iw >= ih)
            {
                ih = (int)Math.Floor(imageSize * (ih / (float)iw));
                iw = imageSize;
            }
            else if (ih > imageSize)
            {
                iw = (int)Math.Floor(imageSize * (iw / (float)ih));
                ih = imageSize;
            }

            using (SolidBrush brush = new SolidBrush(textColour))
            {
                g.DrawString(ti.Name, font, brush, bounds.X + 3, bounds.Y + 3);
                g.DrawString(ti.Width + " x " + ti.Height, font, brush, bounds.X + 6 + imageSize, bounds.Y + lineHeight + 6);
            }

            g.DrawImage(bmp, bounds.X + 3, bounds.Y + lineHeight + 6, iw, ih);

            if (drawBorder)
            {
                using (Pen pen = new Pen(ForeColor))
                {
                    int liney = bounds.Y + bounds.Height - 1;
                    g.DrawLine(pen, bounds.X, liney, bounds.X + bounds.Width, liney);
                }
            }
        }
    }
}
