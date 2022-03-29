using CBRE.Common.Mediator;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Actions.MapObjects.Selection;
using CBRE.Editor.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CBRE.Editor.UI
{
    public partial class MapTreeWindow : HotkeyForm, IMediatorListener
    {
        public Document Document { get; set; }

        public MapTreeWindow(Document document)
        {
            InitializeComponent();
            Document = document;
        }

        protected override void OnLoad(EventArgs e)
        {
            Mediator.Subscribe(EditorMediator.DocumentActivated, this);
            Mediator.Subscribe(EditorMediator.SelectionChanged, this);
            RefreshNodes();
        }

        protected override void OnClosed(EventArgs e)
        {
            Mediator.UnsubscribeAll(this);
        }

        private void DocumentActivated(Document document)
        {
            Document = document;
            RefreshNodes();
        }

        private void SelectionChanged()
        {
            if (Document == null || Document.Selection.InFaceSelection || Document.Selection.IsEmpty()) return;
            MapObject first = Document.Selection.GetSelectedParents().First();
            TreeNode node = FindNodeWithTag(MapTree.Nodes.OfType<TreeNode>(), first);
            if (node != null) MapTree.SelectedNode = node;
        }

        private TreeNode FindNodeWithTag(IEnumerable<TreeNode> nodes, object tag)
        {
            foreach (TreeNode tn in nodes)
            {
                if (tn.Tag == tag) return tn;
                TreeNode recurse = FindNodeWithTag(tn.Nodes.OfType<TreeNode>(), tag);
                if (recurse != null) return recurse;
            }
            return null;
        }

        private void RefreshNodes()
        {
            MapTree.BeginUpdate();
            MapTree.Nodes.Clear();
            if (Document != null)
            {
                LoadMapNode(null, Document.Map.WorldSpawn);
            }
            MapTree.EndUpdate();
        }

        private void LoadMapNode(TreeNode parent, MapObject obj)
        {
            string text = GetNodeText(obj);
            TreeNode node = new TreeNode(obj.GetType().Name + text) { Tag = obj };
            if (obj is World)
            {
                World w = (World)obj;
                node.Nodes.AddRange(GetEntityNodes(w.EntityData).ToArray());
            }
            else if (obj is Entity)
            {
                Entity e = (Entity)obj;
                node.Nodes.AddRange(GetEntityNodes(e.EntityData).ToArray());
            }
            else if (obj is Solid)
            {
                Solid s = (Solid)obj;
                node.Nodes.AddRange(GetFaceNodes(s.Faces).ToArray());
            }
            foreach (MapObject mo in obj.GetChildren())
            {
                LoadMapNode(node, mo);
            }
            if (parent == null) MapTree.Nodes.Add(node);
            else parent.Nodes.Add(node);
        }

        private string GetNodeText(MapObject mo)
        {
            if (mo is Solid)
            {
                return " (" + ((Solid)mo).Faces.Count + " faces)";
            }
            if (mo is Group)
            {
                return " (" + mo.ChildCount + " children)";
            }
            EntityData ed = mo.GetEntityData();
            if (ed != null)
            {
                string targetName = ed.GetPropertyValue("targetname");
                return ": " + ed.Name + (String.IsNullOrWhiteSpace(targetName) ? "" : " (" + targetName + ")");
            }
            return "";
        }

        private IEnumerable<TreeNode> GetEntityNodes(EntityData data)
        {
            yield return new TreeNode("Flags: " + data.Flags);
        }

        private IEnumerable<TreeNode> GetFaceNodes(IEnumerable<Face> faces)
        {
            int c = 0;
            foreach (Face face in faces)
            {
                TreeNode fnode = new TreeNode("Face " + c);
                c++;
                TreeNode pnode = fnode.Nodes.Add("Plane: " + face.Plane.Normal + " * " + face.Plane.DistanceFromOrigin);
                pnode.Nodes.Add("Normal: " + face.Plane.Normal);
                pnode.Nodes.Add("Distance: " + face.Plane.DistanceFromOrigin);
                pnode.Nodes.Add("A: " + face.Plane.A);
                pnode.Nodes.Add("B: " + face.Plane.B);
                pnode.Nodes.Add("C: " + face.Plane.C);
                pnode.Nodes.Add("D: " + face.Plane.D);
                TreeNode tnode = fnode.Nodes.Add("Texture: " + face.Texture.Name);
                tnode.Nodes.Add("U Axis: " + face.Texture.UAxis);
                tnode.Nodes.Add("V Axis: " + face.Texture.VAxis);
                tnode.Nodes.Add(String.Format("Scale: X = {0}, Y = {1}", face.Texture.XScale, face.Texture.YScale));
                tnode.Nodes.Add(String.Format("Offset: X = {0}, Y = {1}", face.Texture.XShift, face.Texture.YShift));
                tnode.Nodes.Add("Rotation: " + face.Texture.Rotation);
                TreeNode vnode = fnode.Nodes.Add("Vertices: " + face.Vertices.Count);
                int d = 0;
                foreach (Vertex vertex in face.Vertices)
                {
                    TreeNode cnode = vnode.Nodes.Add("Vertex " + d + ": " + vertex.Location);
                    d++;
                    cnode.Nodes.Add("Texture U: " + vertex.TextureU);
                    cnode.Nodes.Add("Texture V: " + vertex.TextureV);
                }
                yield return fnode;
            }
        }

        public void Notify(string message, object data)
        {
            Mediator.ExecuteDefault(this, message, data);
        }

        private void TreeSelectionChanged(object sender, TreeViewEventArgs e)
        {
            RefreshSelectionProperties();
            if (MapTree.SelectedNode != null && MapTree.SelectedNode.Tag is MapObject && !(MapTree.SelectedNode.Tag is World) && Document != null && !Document.Selection.InFaceSelection)
            {
                Document.PerformAction("Select object", new ChangeSelection(((MapObject)MapTree.SelectedNode.Tag).FindAll(), Document.Selection.GetSelectedObjects()));
            }
        }

        private void RefreshSelectionProperties()
        {
            Properties.Items.Clear();
            if (MapTree.SelectedNode != null && MapTree.SelectedNode.Tag != null)
            {
                IEnumerable<Tuple<string, string>> list = GetTagProperties(MapTree.SelectedNode.Tag);
                foreach (Tuple<string, string> kv in list)
                {
                    Properties.Items.Add(new ListViewItem(new[] { kv.Item1, kv.Item2 }));
                }
                Properties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
        }

        private IEnumerable<Tuple<string, string>> GetTagProperties(object tag)
        {
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            if (tag is MapObject)
            {
                MapObject mo = (MapObject)tag;
                EntityData ed = mo.GetEntityData();
                if (ed != null)
                {
                    DataStructures.GameData.GameDataObject gd = Document.GameData.Classes.FirstOrDefault(x => String.Equals(x.Name, ed.Name, StringComparison.OrdinalIgnoreCase));
                    foreach (Property prop in ed.Properties)
                    {
                        DataStructures.GameData.Property gdp = gd != null ? gd.Properties.FirstOrDefault(x => String.Equals(x.Name, prop.Key, StringComparison.OrdinalIgnoreCase)) : null;
                        string key = gdp != null && !String.IsNullOrWhiteSpace(gdp.ShortDescription) ? gdp.ShortDescription : prop.Key;
                        list.Add(Tuple.Create(key, prop.Value));
                    }
                }
            }
            return list;
        }
    }
}
