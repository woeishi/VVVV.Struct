using System;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;

using System.Windows.Forms;
using System.Drawing;

namespace VVVV.Struct
{
    #region PluginInfo
    [PluginInfo(Name = "DefinitionHelper", Category = "Struct", Help = "edit and paste defininitions via node selection", Author = "woei", AutoEvaluate = true)]
    #endregion PluginInfo
    public class DefinitionHelper : UserControl, IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        const string snippet = 
            "<PATCH>" +
            "<NODE id=\"1\" componentmode=\"InABox\" systemname=\"IOBox (String)\">" +
            "<BOUNDS type=\"Box\" left=\"0\" top=\"0\" width=\"1000\" height=\"HEIGHT\"></BOUNDS>" +
            "<PIN pinname=\"Input String\" slicecount=\"COUNT\" values=\"types\"></PIN>" +
            "<PIN pinname=\"Rows\" slicecount=\"1\" values=\"COUNT\"></PIN>" +
            "<PIN pinname=\"Show Grid\" slicecount=\"1\" values=\"1\"></PIN>" +
            "</NODE>" +
            "<NODE id=\"2\" componentmode=\"InABox\" systemname=\"IOBox (String)\">" +
            "<BOUNDS type=\"Box\" left=\"1150\" top=\"0\" width=\"1000\" height=\"HEIGHT\"></BOUNDS>" +
            "<PIN pinname=\"Input String\" slicecount=\"COUNT\" values=\"names\"></PIN>" +
            "<PIN pinname=\"Rows\" slicecount=\"1\" values=\"COUNT\"></PIN>" +
            "<PIN pinname=\"Show Grid\" slicecount=\"1\" values=\"1\"></PIN>" +
            "</NODE>"+
            "<NODE id =\"0\" componentmode=\"Hidden\" systemname=\"Definition (Struct)\">" +
            "<BOUNDS type=\"Node\" left=\"0\" top=\"TOP\"></BOUNDS>" +
            "</NODE>" +
            "<LINK srcnodeid=\"1\" srcpinname=\"Output String\" dstnodeid=\"0\" dstpinname=\"Pin Type\"></LINK>" +
            "<LINK srcnodeid=\"2\" srcpinname=\"Output String\" dstnodeid=\"0\" dstpinname=\"Pin Name\"></LINK>" +
            "</PATCH>";

        #region fields & pins
        [Import()]
        public IHDEHost FHDE;

        [Import()]
        public IPluginHost2 FHost;

        INode2[] FNodes;

        DataGridView FView;
        DataGridViewCellStyle FOutStyle;
        #endregion fields & pins

        protected override void Dispose(bool disposing)
        {
            FHDE.NodeSelectionChanged -= OnNodeSelectionChanged;

            this.Controls.Clear();
            FView.Dispose();

            base.Dispose(disposing);
        }

        void FView_ValueChanged(object sender, EventArgs e)
        {
            WriteClipboard();
        }

        private void FView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            WriteClipboard();
        }

        private void FView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            WriteClipboard();
        }

        #region dragdrop
        private Rectangle FDragBoxFromMouseDown;
        private int FDragSourceIndex;

        void FView_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        void FView_DragDrop(object sender, DragEventArgs e)
        {
            // The mouse locations are relative to the screen, so they must be
            // converted to client coordinates.
            Point clientPoint = FView.PointToClient(new Point(e.X, e.Y));

            // Get the row index of the item the mouse is below.
            var dragTargetIndex = FView.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            // If the drag operation was a move then remove and insert the row.
            if (e.Effect == DragDropEffects.Move)
            {
                int idx = FView.SelectedRows[0].Index;
                var source = FView.Rows[idx];

                FView.Rows.Remove(source);
                FView.Rows.Insert(dragTargetIndex, source);
            }
        }

        void FView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Get the index of the item the mouse is below.
            FDragSourceIndex = e.RowIndex;
            if (FDragSourceIndex != -1)
            {
                // Remember the point where the mouse down occurred.
                // The DragSize indicates the size that the mouse can move
                // before a drag event should be started.
                Size dragSize = SystemInformation.DragSize;

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                FDragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                                e.Y - (dragSize.Height / 2)),
                                                      dragSize);
            }
            else
                // Reset the rectangle if the mouse is not over an item in the ListBox.
                FDragBoxFromMouseDown = Rectangle.Empty;
        }

        void FView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //row dragging
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // If the mouse moves outside the rectangle, start the drag.
                if (FDragBoxFromMouseDown != Rectangle.Empty &&
                    !FDragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    // Proceed with the drag and drop, passing in the list item.
                    DragDropEffects dropEffect = FView.DoDragDrop(
                        FView.Rows[FDragSourceIndex],
                        DragDropEffects.Move);
                }
            }
        }
        #endregion dragdrop

        public void OnImportsSatisfied()
        {
            FOutStyle = new DataGridViewCellStyle();
            FOutStyle.BackColor = System.Drawing.Color.LightGray;

            FView = new DataGridView();
            FView.Dock = DockStyle.Fill;
            FView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            FView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            FView.ColumnHeadersHeight = 35;

            FView.AllowDrop = true;
            FView.AllowUserToAddRows = true;
            FView.AllowUserToDeleteRows = true;

            var typeCol = new DataGridViewTextBoxColumn();
            typeCol.HeaderText = "Type";
            FView.Columns.Add(typeCol);
            var nameCol = new DataGridViewTextBoxColumn();
            typeCol.HeaderText = "Name";
            FView.Columns.Add(nameCol);

            FView.CellValueChanged += FView_ValueChanged;
            FView.RowsAdded += FView_RowsAdded;
            FView.RowsRemoved += FView_RowsRemoved;

            FView.DragOver += FView_DragOver;
            FView.DragDrop += FView_DragDrop;
            FView.CellMouseDown += FView_CellMouseDown;
            FView.MouseMove += FView_MouseMove;

            this.Controls.Add(FView);

            FHDE.NodeSelectionChanged += OnNodeSelectionChanged;
        }

        void OnNodeSelectionChanged(object sender, NodeSelectionEventArgs e)
        {
            if ((FHost.Window != null) &&
                (e.Nodes.Length > 0) &&
                (e.Nodes[0].Name != FHost.GetNodeInfo().Systemname))
            {
                if (FNodes != null && e.Nodes.Length == FNodes.Length)
                {
                    bool changed = false;
                    for (int i = 0; i < FNodes.Length; i++)
                        if (e.Nodes[i].ID != FNodes[i].ID)
                            changed = true;
                    if (changed)
                        ParseNodes(e.Nodes);
                }
                else
                    ParseNodes(e.Nodes);
            }
        }

        void ParseNodes(INode2[] nodes)
        {
            FNodes = nodes;
            FView.Rows.Clear();

            foreach (var n in nodes)
                ParsePins(n);
        }

        void ParsePins(INode2 node)
        {
            foreach (var p in node.Pins)
            {
                if ((p.Direction != PinDirection.Configuration) &&
                    (p.Visibility != PinVisibility.False) &&
                    (p.Visibility != PinVisibility.OnlyInspector))
                {
                    var name = ParseName(p);
                    var id = FView.Rows.Add(new object[] { ParsePinType(p), name });
                    if (p.Direction == PinDirection.Output)
                            FView.Rows[id].DefaultCellStyle = FOutStyle;
                }
            }
        }

        string ParseName(IPin2 pin)
        {
            var desc = pin.ParentNode.LabelPin.Spread.Trim('|');

            if (pin.ParentNode.Name.Contains("IOBox") && (!string.IsNullOrEmpty(desc)))
                return desc;
            else if ((pin.ParentNode.NodeInfo.Type == NodeType.Module) || (pin.ParentNode.NodeInfo.Type == NodeType.Patch))
                return pin.NameByParent(pin.ParentNode);
            else
                return pin.Name;
        }

        string ParsePinType(IPin2 pin)
        {
            string type = pin.Type;
            if (type == "Value")
            {
                var typeInfo = pin.SubType.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var mainInfo = typeInfo[0].Trim();
                if (mainInfo == "Toggle" || mainInfo == "Bang")
                    return "Boolean";
                else if (mainInfo == "Endless")
                {
                    switch (typeInfo[1].Trim())
                    {
                        case "2":
                            return "Vector2D";
                        case "3":
                            return "Vector3D";
                        case "4":
                            return "Vector4D";
                        default:
                            return "Double";
                    }
                }
                else
                    return mainInfo;
            }
            else if (pin.SubType.Contains("Transform"))
            {
                return "Matrix";
            }
            else
                return type.Trim(new char[] { '-', '<', '>', ' ' });
        }

        void WriteClipboard()
        {
            var t = string.Empty;
            var n = string.Empty;
            int i = 0;

            foreach (DataGridViewRow r in FView.Rows)
            {
                if (!r.IsNewRow)
                {
                    var _t = r.Cells[0].Value.ToString();
                    if (_t.Contains(" "))
                        _t = "|" + _t + "|";
                    t += _t + ",";

                    var _n = r.Cells[1].Value.ToString();
                    if (_n.Contains(" "))
                        _n = "|" + _n + "|";
                    n += _n + ",";
                    i++;
                }
            }
            
            t = t.TrimEnd(new char[] { ',' });
            n = n.TrimEnd(new char[] { ',' });

            var text = snippet.Replace("COUNT", i.ToString())
                                .Replace("types", t)
                                .Replace("names", n)
                                .Replace("HEIGHT", (i * 200).ToString())
                                .Replace("TOP", (i * 200 + 250).ToString());
            Clipboard.SetText(text);
        }

        public void Evaluate(int SpreadMax) {}
    }
}
