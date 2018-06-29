using System;
using System.Collections.Generic;
using System.Windows.Forms;

using VVVV.Struct.Core;

namespace VVVV.Struct.Hosting
{
    public partial class ConflictUI : Form
    {
        public ConflictUI()
        {
            InitializeComponent();
        }

        public event EventHandler<string> SelectionChanged;
        public event EventHandler<string> SelectionTextChanged;

        public string NewDeclarationName => FNewName.Text;

        public string NewDeclarationBody => FNewBody.Text;

        public bool SplitEnabled
        {
            get { return FSplit.Enabled; }
            set { FSplit.Enabled = value; }
        }

        public bool NewBodyEnabled
        {
            get { return !FNewBody.ReadOnly; }
            set { FNewBody.ReadOnly = !value; }
        }

        public void SetExistingDeclaration(Declaration d)
        {
            FExistingName.Text = d.Name;
            FExistingBody.Text = string.Empty;
            //FExistingBody.Clear(); //clear seemingly doesn't do for readonly
            foreach (var l in d.Lines)
                FExistingBody.Text += l.ToString() + Environment.NewLine;
            FExistingBody.Text.TrimEnd();
        }

        public void SetNewSelection(ICollection<string> existing)
        {
            FNewName.Items.Clear();
            foreach (var e in existing)
                FNewName.Items.Add(e);

            FNewName.SelectedIndex = 0;
        }

        public void SetNewDeclaration(Declaration d)
        {
            FNewBody.Clear();
            foreach (var l in d.Lines)
                FNewBody.Text += l.ToString() + Environment.NewLine;
            FNewBody.Text.TrimEnd();
        }

        public void SetExistingUsers(IEnumerable<string> users)
        {
            FExistingUsers.Text = string.Empty;
            foreach (var l in users)
                FExistingUsers.Text += l + Environment.NewLine;
        }

        public void SetNewUsers(IEnumerable<string> users)
        {
            FNewUsers.Text = string.Empty;
            foreach (var l in users)
                FNewUsers.Text += l + Environment.NewLine;
        }

        public void ShowDiff()
        {
            var el = FExistingBody.Lines;
            var nl = FNewBody.Lines;
            int ei = 0;
            int ni = 0;

            for (int i = 0; i < Math.Max(el.Length, nl.Length); i++)
            {
                if (i < el.Length && i < nl.Length && el[i] != nl[i])
                {
                    FExistingBody.SelectionStart = ei;
                    FExistingBody.SelectionLength = el[i].Length;
                    FExistingBody.SelectionColor = System.Drawing.Color.Red;

                    FNewBody.SelectionStart = ni;
                    FNewBody.SelectionLength = nl[i].Length;
                    FNewBody.SelectionColor = System.Drawing.Color.Red;
                }
                if (i < el.Length)
                    ei += el[i].Length + 1;
                if (i < nl.Length)
                    ni += nl[i].Length + 1;
            }
        }

        public void ClearDiff()
        {
            FExistingBody.SelectionStart = 0;
            FExistingBody.SelectionLength = FExistingBody.Text.Length;
            FExistingBody.SelectionColor = System.Drawing.Color.Black;

            FNewBody.SelectionStart = 0;
            FNewBody.SelectionLength = FNewBody.Text.Length;
            FNewBody.SelectionColor = System.Drawing.Color.Black;
        }

        void FNewName_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectionChanged?.Invoke(sender, FNewName.SelectedItem.ToString());
        }

        void FNewName_TextChanged(object sender, EventArgs e)
        {
            SelectionTextChanged?.Invoke(sender, FNewName.Text);
        }

        void FUseExisting_Click(object sender, EventArgs e)
        {
            this.DialogResult = (DialogResult)ConflictManager.ConflictSolutionKind.Ignore;
            this.Close();
        }

        void FSplit_Click(object sender, EventArgs e)
        {
            this.DialogResult = (DialogResult)ConflictManager.ConflictSolutionKind.Split;
            this.Close();
        }

        void FUseNew_Click(object sender, EventArgs e)
        {
            this.DialogResult = (DialogResult)ConflictManager.ConflictSolutionKind.Overwrite;
            this.Close();
        }
    }
}
