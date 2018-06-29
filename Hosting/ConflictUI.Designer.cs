namespace VVVV.Struct.Hosting
{
    partial class ConflictUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FDeclarationsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.FExistingDecl = new System.Windows.Forms.Panel();
            this.FExistingBody = new System.Windows.Forms.RichTextBox();
            this.FExistingName = new System.Windows.Forms.TextBox();
            this.FNewDecl = new System.Windows.Forms.Panel();
            this.FNewBody = new System.Windows.Forms.RichTextBox();
            this.FNewName = new System.Windows.Forms.ComboBox();
            this.FUsersPanel = new System.Windows.Forms.TableLayoutPanel();
            this.FExistingUsers = new System.Windows.Forms.Label();
            this.FNewUsers = new System.Windows.Forms.Label();
            this.FDecisionPanel = new System.Windows.Forms.TableLayoutPanel();
            this.FUseExisting = new System.Windows.Forms.Button();
            this.FUseNew = new System.Windows.Forms.Button();
            this.FSplit = new System.Windows.Forms.Button();
            this.FDeclarationsPanel.SuspendLayout();
            this.FExistingDecl.SuspendLayout();
            this.FNewDecl.SuspendLayout();
            this.FUsersPanel.SuspendLayout();
            this.FDecisionPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // FDeclarationsPanel
            // 
            this.FDeclarationsPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.FDeclarationsPanel.ColumnCount = 2;
            this.FDeclarationsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.FDeclarationsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.FDeclarationsPanel.Controls.Add(this.FExistingDecl, 0, 0);
            this.FDeclarationsPanel.Controls.Add(this.FNewDecl, 1, 0);
            this.FDeclarationsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FDeclarationsPanel.Location = new System.Drawing.Point(0, 0);
            this.FDeclarationsPanel.Name = "FDeclarationsPanel";
            this.FDeclarationsPanel.RowCount = 1;
            this.FDeclarationsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.FDeclarationsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.FDeclarationsPanel.Size = new System.Drawing.Size(576, 265);
            this.FDeclarationsPanel.TabIndex = 3;
            // 
            // FExistingDecl
            // 
            this.FExistingDecl.Controls.Add(this.FExistingBody);
            this.FExistingDecl.Controls.Add(this.FExistingName);
            this.FExistingDecl.Dock = System.Windows.Forms.DockStyle.Top;
            this.FExistingDecl.Font = new System.Drawing.Font("Verdana", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FExistingDecl.Location = new System.Drawing.Point(1, 1);
            this.FExistingDecl.Margin = new System.Windows.Forms.Padding(0);
            this.FExistingDecl.Name = "FExistingDecl";
            this.FExistingDecl.Size = new System.Drawing.Size(286, 171);
            this.FExistingDecl.TabIndex = 2;
            // 
            // FExistingBody
            // 
            this.FExistingBody.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FExistingBody.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FExistingBody.Font = new System.Drawing.Font("Verdana", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FExistingBody.Location = new System.Drawing.Point(0, 32);
            this.FExistingBody.Margin = new System.Windows.Forms.Padding(0);
            this.FExistingBody.Multiline = true;
            this.FExistingBody.Name = "FExistingBody";
            this.FExistingBody.ReadOnly = true;
            this.FExistingBody.Size = new System.Drawing.Size(288, 144);
            this.FExistingBody.TabIndex = 1;
            this.FExistingBody.TabStop = false;
            // 
            // FExistingName
            // 
            this.FExistingName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FExistingName.Dock = System.Windows.Forms.DockStyle.Top;
            this.FExistingName.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FExistingName.Location = new System.Drawing.Point(0, 0);
            this.FExistingName.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.FExistingName.MinimumSize = new System.Drawing.Size(2, 32);
            this.FExistingName.Name = "FExistingName";
            this.FExistingName.ReadOnly = true;
            this.FExistingName.Size = new System.Drawing.Size(286, 31);
            this.FExistingName.TabIndex = 0;
            this.FExistingName.TabStop = false;
            // 
            // FNewDecl
            // 
            this.FNewDecl.Controls.Add(this.FNewBody);
            this.FNewDecl.Controls.Add(this.FNewName);
            this.FNewDecl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FNewDecl.Location = new System.Drawing.Point(288, 1);
            this.FNewDecl.Margin = new System.Windows.Forms.Padding(0);
            this.FNewDecl.Name = "FNewDecl";
            this.FNewDecl.Size = new System.Drawing.Size(287, 263);
            this.FNewDecl.TabIndex = 3;
            // 
            // FNewBody
            // 
            this.FNewBody.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FNewBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FNewBody.Font = new System.Drawing.Font("Verdana", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FNewBody.Location = new System.Drawing.Point(0, 31);
            this.FNewBody.Margin = new System.Windows.Forms.Padding(0);
            this.FNewBody.Multiline = true;
            this.FNewBody.Name = "FNewBody";
            this.FNewBody.ReadOnly = true;
            this.FNewBody.Size = new System.Drawing.Size(287, 232);
            this.FNewBody.TabIndex = 1;
            this.FNewBody.TabStop = false;
            // 
            // FNewName
            // 
            this.FNewName.Dock = System.Windows.Forms.DockStyle.Top;
            this.FNewName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FNewName.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FNewName.FormattingEnabled = true;
            this.FNewName.Location = new System.Drawing.Point(0, 0);
            this.FNewName.Margin = new System.Windows.Forms.Padding(0);
            this.FNewName.Name = "FNewName";
            this.FNewName.Size = new System.Drawing.Size(287, 31);
            this.FNewName.TabIndex = 0;
            this.FNewName.SelectedIndexChanged += new System.EventHandler(this.FNewName_SelectedIndexChanged);
            this.FNewName.TextChanged += new System.EventHandler(this.FNewName_TextChanged);
            // 
            // FUsersPanel
            // 
            this.FUsersPanel.AutoSize = true;
            this.FUsersPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.FUsersPanel.ColumnCount = 2;
            this.FUsersPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.FUsersPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.FUsersPanel.Controls.Add(this.FExistingUsers, 0, 0);
            this.FUsersPanel.Controls.Add(this.FNewUsers, 1, 0);
            this.FUsersPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.FUsersPanel.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.FUsersPanel.Location = new System.Drawing.Point(0, 265);
            this.FUsersPanel.Name = "FUsersPanel";
            this.FUsersPanel.RowCount = 1;
            this.FUsersPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.FUsersPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.FUsersPanel.Size = new System.Drawing.Size(576, 20);
            this.FUsersPanel.TabIndex = 2;
            // 
            // FExistingUsers
            // 
            this.FExistingUsers.AutoSize = true;
            this.FExistingUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FExistingUsers.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FExistingUsers.Location = new System.Drawing.Point(4, 1);
            this.FExistingUsers.Name = "FExistingUsers";
            this.FExistingUsers.Size = new System.Drawing.Size(280, 18);
            this.FExistingUsers.TabIndex = 0;
            // 
            // FNewUsers
            // 
            this.FNewUsers.AutoSize = true;
            this.FNewUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FNewUsers.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FNewUsers.Location = new System.Drawing.Point(291, 1);
            this.FNewUsers.Name = "FNewUsers";
            this.FNewUsers.Size = new System.Drawing.Size(281, 18);
            this.FNewUsers.TabIndex = 1;
            // 
            // FDecisionPanel
            // 
            this.FDecisionPanel.AutoSize = true;
            this.FDecisionPanel.ColumnCount = 3;
            this.FDecisionPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33113F));
            this.FDecisionPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33443F));
            this.FDecisionPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33443F));
            this.FDecisionPanel.Controls.Add(this.FUseExisting, 0, 0);
            this.FDecisionPanel.Controls.Add(this.FUseNew, 2, 0);
            this.FDecisionPanel.Controls.Add(this.FSplit, 1, 0);
            this.FDecisionPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.FDecisionPanel.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.FDecisionPanel.Location = new System.Drawing.Point(0, 285);
            this.FDecisionPanel.Name = "FDecisionPanel";
            this.FDecisionPanel.RowCount = 1;
            this.FDecisionPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.FDecisionPanel.Size = new System.Drawing.Size(576, 51);
            this.FDecisionPanel.TabIndex = 1;
            // 
            // FUseExisting
            // 
            this.FUseExisting.AutoSize = true;
            this.FUseExisting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FUseExisting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FUseExisting.Font = new System.Drawing.Font("Verdana", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FUseExisting.Location = new System.Drawing.Point(0, 0);
            this.FUseExisting.Margin = new System.Windows.Forms.Padding(0);
            this.FUseExisting.Name = "FUseExisting";
            this.FUseExisting.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.FUseExisting.Size = new System.Drawing.Size(191, 51);
            this.FUseExisting.TabIndex = 0;
            this.FUseExisting.Text = "Use Existing";
            this.FUseExisting.UseVisualStyleBackColor = true;
            this.FUseExisting.Click += new System.EventHandler(this.FUseExisting_Click);
            // 
            // FUseNew
            // 
            this.FUseNew.AutoSize = true;
            this.FUseNew.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FUseNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FUseNew.Font = new System.Drawing.Font("Verdana", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FUseNew.Location = new System.Drawing.Point(383, 0);
            this.FUseNew.Margin = new System.Windows.Forms.Padding(0);
            this.FUseNew.Name = "FUseNew";
            this.FUseNew.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.FUseNew.Size = new System.Drawing.Size(193, 51);
            this.FUseNew.TabIndex = 1;
            this.FUseNew.Text = "Use New For All";
            this.FUseNew.UseVisualStyleBackColor = true;
            this.FUseNew.Click += new System.EventHandler(this.FUseNew_Click);
            // 
            // FSplit
            // 
            this.FSplit.AutoSize = true;
            this.FSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FSplit.Enabled = false;
            this.FSplit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FSplit.Font = new System.Drawing.Font("Verdana", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FSplit.Location = new System.Drawing.Point(191, 0);
            this.FSplit.Margin = new System.Windows.Forms.Padding(0);
            this.FSplit.Name = "FSplit";
            this.FSplit.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.FSplit.Size = new System.Drawing.Size(192, 51);
            this.FSplit.TabIndex = 2;
            this.FSplit.Text = "Split";
            this.FSplit.UseVisualStyleBackColor = true;
            this.FSplit.Click += new System.EventHandler(this.FSplit_Click);
            // 
            // ConflictUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 336);
            this.ControlBox = false;
            this.Controls.Add(this.FDeclarationsPanel);
            this.Controls.Add(this.FUsersPanel);
            this.Controls.Add(this.FDecisionPanel);
            this.Name = "ConflictUI";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ConflictUI";
            this.TopMost = true;
            this.FDeclarationsPanel.ResumeLayout(false);
            this.FExistingDecl.ResumeLayout(false);
            this.FExistingDecl.PerformLayout();
            this.FNewDecl.ResumeLayout(false);
            this.FNewDecl.PerformLayout();
            this.FUsersPanel.ResumeLayout(false);
            this.FUsersPanel.PerformLayout();
            this.FDecisionPanel.ResumeLayout(false);
            this.FDecisionPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        

        #endregion
        private System.Windows.Forms.Button FUseNew;
        private System.Windows.Forms.Button FUseExisting;
        private System.Windows.Forms.Button FSplit;
        private System.Windows.Forms.TableLayoutPanel FDecisionPanel;
        private System.Windows.Forms.TableLayoutPanel FUsersPanel;
        private System.Windows.Forms.Label FExistingUsers;
        private System.Windows.Forms.Label FNewUsers;
        private System.Windows.Forms.TableLayoutPanel FDeclarationsPanel;
        private System.Windows.Forms.Panel FExistingDecl;
        private System.Windows.Forms.RichTextBox FExistingBody;
        private System.Windows.Forms.TextBox FExistingName;
        private System.Windows.Forms.Panel FNewDecl;
        private System.Windows.Forms.RichTextBox FNewBody;
        private System.Windows.Forms.ComboBox FNewName;
    }
}