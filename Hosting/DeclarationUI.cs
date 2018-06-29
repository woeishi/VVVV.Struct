using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.ManagedVCL;

using VVVV.Struct.Core;

namespace VVVV.Struct.Hosting
{
    public static class DeclarationUIExtension
    {
        public static void SetByDeclaration(this DeclarationUI ui, Declaration declaration)
        {
            ui.Name = declaration.Name;
            ui.FSelection.SelectedItem = declaration;

            ui.ClearBody();
            foreach (var l in declaration.Lines)
            {
                if ((l as Comment) != null)
                    ui.CurrentColor = Color.DimGray;
                else if ((l as Field).ContainerType == "Null")
                    ui.CurrentColor = Color.Red;
                else
                    ui.CurrentColor = Color.Black;

                ui.AppendLine(l.ToString());
            }
            ui.CurrentColor = Color.Black;
            ui.KeyIsDirty = false;
            ui.BodyIsDirty = false;
        }
    }

    public class DeclarationString
    {
        public string Name { get;}
        public string Fields { get; }
        public DeclarationString(string name, string fields)
        {
            Name = name;
            Fields = fields;
        }
    }

    public class DeclarationUI : TopControl
    {
        INode FNode;
        IHDEHost FHDE;
        Panel FMenuPanel;
        public ComboBox FSelection;
        Button FSave;
        Button FNew;
        Panel FPadPanel;
        RichTextBox FBody;
        Label FError;
        public Color CurrentColor { set { FBody.SelectionColor = value; } }

        ToolTip FTooltip;

        bool FKeyIsDirty;
        bool FBodyIsDirty;

        public event EventHandler<DeclarationString> UpdateDeclaration;
        public event EventHandler<DeclarationString> CreateDeclaration;
        public event EventHandler<Declaration> SelectionChanged;

        public DeclarationUI(INode node, IHDEHost hde, BindingList<Declaration> binding)
        {
            FNode = node;
            FHDE = hde;

            var font = new Font("Verdana", 7.5F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            var fontBold = new Font("Verdana", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

            FTooltip = new ToolTip();

            FSelection = new ComboBox();
            FSelection.TabIndex = 0;
            FSelection.FlatStyle = FlatStyle.Flat;
            FSelection.BackColor = Color.Silver;
            FSelection.Dock = DockStyle.Top;
            FSelection.Font = fontBold;
            FSelection.DataSource = binding;
            FSelection.DisplayMember = "Name";
            FSelection.ValueMember = "Name";
            FSelection.TextChanged += (s,e) => KeyIsDirty = true;
            FSelection.SelectedValueChanged += FSelection_SelectedValueChanged;

            FSave = new Button();
            FSave.TabIndex = 2;
            FSave.FlatStyle = FlatStyle.Flat;
            FSave.AutoSize = true;
            FSave.Dock = DockStyle.Right;
            FSave.Font = font;
            FSave.Text = "Save";
            FSave.Click += (o, e) => Save();
            FSave.Enabled = false;

            FNew = new Button();
            FNew.TabIndex = 3;
            FNew.FlatStyle = FlatStyle.Flat;
            FNew.AutoSize = true;
            FNew.Dock = DockStyle.Right;
            FNew.Font = font;
            FNew.Text = "New";
            FNew.Click += (o, e) => New();
            FNew.Enabled = false;

            FMenuPanel = new Panel();
            FMenuPanel.Dock = DockStyle.Top;
            FMenuPanel.AutoSize = true;
            FMenuPanel.Controls.Add(FSelection);
            FMenuPanel.Controls.Add(FSave);
            FMenuPanel.Controls.Add(FNew);

            FPadPanel = new Panel();
            FPadPanel.Dock = DockStyle.Fill;
            FPadPanel.BorderStyle = BorderStyle.None;
            FPadPanel.Padding = new Padding(2);
            FPadPanel.BackColor = Color.Silver;

            FBody = new RichTextBox();
            FBody.TabIndex = 1;
            FBody.BackColor = Color.Silver;
            FBody.BorderStyle = BorderStyle.None;
            FBody.Dock = DockStyle.Fill;
            FBody.Font = font;
            FBody.Multiline = true;
            FBody.TextChanged += (s,e) => BodyIsDirty = true;

            FPadPanel.Controls.Add(FBody);

            FError = new Label();
            FError.BorderStyle = BorderStyle.None;
            FError.AutoSize = true;
            FError.Dock = DockStyle.Bottom;
            FError.Font = font;
            FError.Text = string.Empty;
            FError.Visible = false;
            FError.TextChanged += (o, e) =>
            {
                if (string.IsNullOrWhiteSpace(FError.Text))
                    FError.Visible = false;
                else
                    FError.Visible = true;
            };

            FTooltip.SetToolTip(FBody, "type name [= default]" + Environment.NewLine +
                "available shorthands:" + Environment.NewLine +
                "bool, int, float, double, string, color, matrix, vectorN, stream, struct, mouse, keyboard, touch, gesture;" + Environment.NewLine +
                "if DX11 pack present: dx11layer, dx11geometry, dx11textureNd, dx11buffer, dx11renderstate, dx11samplerstates, dx11rendersemantic, dx11objectvalidator" + Environment.NewLine +
                "not listed types need to be specified by the fully qualified name (with namespace), e.g. System.IO.FileInfo");

            this.Controls.Add(FPadPanel);
            this.Controls.Add(FMenuPanel);
            this.Controls.Add(FError);
            this.VisibleChanged += VisibilityChanged;

            this.GotFocus += (s, e) => FHDE.DisableShortCuts();
            this.LostFocus += (s, e) => FHDE.EnableShortCuts();
        }

        public bool KeyIsDirty
        {
            get { return FKeyIsDirty; }
            set { FKeyIsDirty = value; SetDirtyState(); }
        }

        public bool BodyIsDirty
        {
            get { return FBodyIsDirty; }
            set { FBodyIsDirty = value; SetDirtyState(); }
        }

        void SetDirtyState()
        {
            if (KeyIsDirty && FSelection.Text == this.Name)
                KeyIsDirty = false;

            FSave.Enabled = false;
            FNew.Enabled = false;
            if (FSelection.Text.ToLowerInvariant() != "template" && (!string.IsNullOrWhiteSpace(FSelection.Text)))
            {

                if ((BodyIsDirty || KeyIsDirty) && (this.Name != "Template"))
                    FSave.Enabled = true;
                if (FKeyIsDirty)
                    FNew.Enabled = true;
            }
            if (FNode.Window != null)
                FNode.Window.Caption = this.Name + ((KeyIsDirty || FBodyIsDirty)?" *":"");
        }

        //handle caret movement and selection
        protected override bool ProcessKeyPreview(ref Message m)
        {
            KeyEventArgs ke = new KeyEventArgs((Keys)m.WParam.ToInt32() | ModifierKeys);
            if (FSave.Enabled && ke.Control && ke.KeyCode == Keys.S)
                Save();
            else if (FNew.Enabled && ke.Control && ke.KeyCode == Keys.D)
                New();

            //return base.ProcessKeyPreview(ref m); messed up all the shortcuts
            return false;
        }

        public void ClearBody() => FBody.Clear();

        public void AppendLine(string line) => FBody.AppendText(line + Environment.NewLine);

        public void AppendError(string line) => FError.Text += line + Environment.NewLine;

        void FSelection_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FSelection.SelectedValue.ToString())) //selection might have not changed 
            {
                var d = FSelection.SelectedItem as Declaration;
                if (this.Name != d.Name)
                {
                    this.Name = d.Name;
                    SelectionChanged?.Invoke(this, FSelection.SelectedItem as Declaration);
                }
            }
        }

        void Save()
        {
            KeyIsDirty = false;
            BodyIsDirty = false;
            FError.Text = string.Empty;
            UpdateDeclaration?.Invoke(this, new DeclarationString(FSelection.Text.Trim(), FBody.Text.Trim()));
        }

        void New()
        {
            KeyIsDirty = false;
            BodyIsDirty = false;
            FError.Text = string.Empty;
            CreateDeclaration?.Invoke(this, new DeclarationString(FSelection.Text.Trim(), FBody.Text.Trim()));
        }

        void VisibilityChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
            {
                FNode.Window.Caption = this.Name;
                FSelection.Select(0, 0);
                FBody.Focus();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    FSelection.Dispose();
                    FSelection = null;
                    FSave.Dispose();
                    FSave = null;
                    FMenuPanel.Dispose();
                    FMenuPanel = null;
                    FBody.Dispose();
                    FBody = null;
                    this.Controls.Clear();
                }
            }
        }
    }
}