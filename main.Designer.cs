namespace DatabaseView {
    partial class main {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.addBtn = new System.Windows.Forms.Button();
            this.editBtn = new System.Windows.Forms.Button();
            this.removeBtn = new System.Windows.Forms.Button();
            this.queryBox = new System.Windows.Forms.GroupBox();
            this.editBox = new System.Windows.Forms.GroupBox();
            this.keyLabel = new System.Windows.Forms.Label();
            this.selectedKey = new System.Windows.Forms.ComboBox();
            this.requestBox = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.queryBox.SuspendLayout();
            this.editBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dataGridView1.Location = new System.Drawing.Point(3, 49);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(855, 374);
            this.dataGridView1.TabIndex = 0;
            // 
            // addBtn
            // 
            this.addBtn.Location = new System.Drawing.Point(6, 19);
            this.addBtn.Name = "addBtn";
            this.addBtn.Size = new System.Drawing.Size(197, 23);
            this.addBtn.TabIndex = 3;
            this.addBtn.Text = "Add";
            this.addBtn.UseVisualStyleBackColor = true;
            this.addBtn.Click += new System.EventHandler(this.addBtn_Click);
            // 
            // editBtn
            // 
            this.editBtn.Location = new System.Drawing.Point(6, 99);
            this.editBtn.Name = "editBtn";
            this.editBtn.Size = new System.Drawing.Size(197, 23);
            this.editBtn.TabIndex = 3;
            this.editBtn.Text = "Edit";
            this.editBtn.UseVisualStyleBackColor = true;
            this.editBtn.Click += new System.EventHandler(this.editBtn_Click);
            // 
            // removeBtn
            // 
            this.removeBtn.Location = new System.Drawing.Point(6, 128);
            this.removeBtn.Name = "removeBtn";
            this.removeBtn.Size = new System.Drawing.Size(197, 23);
            this.removeBtn.TabIndex = 3;
            this.removeBtn.Text = "Remove";
            this.removeBtn.UseVisualStyleBackColor = true;
            this.removeBtn.Click += new System.EventHandler(this.removeBtn_Click);
            // 
            // queryBox
            // 
            this.queryBox.Controls.Add(this.dataGridView1);
            this.queryBox.Location = new System.Drawing.Point(12, 12);
            this.queryBox.Name = "queryBox";
            this.queryBox.Size = new System.Drawing.Size(861, 426);
            this.queryBox.TabIndex = 4;
            this.queryBox.TabStop = false;
            this.queryBox.Text = "Query";
            // 
            // editBox
            // 
            this.editBox.Controls.Add(this.keyLabel);
            this.editBox.Controls.Add(this.selectedKey);
            this.editBox.Controls.Add(this.addBtn);
            this.editBox.Controls.Add(this.editBtn);
            this.editBox.Controls.Add(this.removeBtn);
            this.editBox.Enabled = false;
            this.editBox.Location = new System.Drawing.Point(879, 12);
            this.editBox.Name = "editBox";
            this.editBox.Size = new System.Drawing.Size(209, 167);
            this.editBox.TabIndex = 5;
            this.editBox.TabStop = false;
            this.editBox.Text = "Edit";
            // 
            // keyLabel
            // 
            this.keyLabel.AutoSize = true;
            this.keyLabel.Location = new System.Drawing.Point(6, 56);
            this.keyLabel.Name = "keyLabel";
            this.keyLabel.Size = new System.Drawing.Size(27, 13);
            this.keyLabel.TabIndex = 5;
            this.keyLabel.Text = "key:";
            // 
            // selectedKey
            // 
            this.selectedKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selectedKey.FormattingEnabled = true;
            this.selectedKey.Location = new System.Drawing.Point(9, 72);
            this.selectedKey.Name = "selectedKey";
            this.selectedKey.Size = new System.Drawing.Size(194, 21);
            this.selectedKey.TabIndex = 4;
            // 
            // requestBox
            // 
            this.requestBox.Location = new System.Drawing.Point(876, 197);
            this.requestBox.Name = "requestBox";
            this.requestBox.Size = new System.Drawing.Size(209, 168);
            this.requestBox.TabIndex = 6;
            this.requestBox.TabStop = false;
            this.requestBox.Text = "Requests";
            // 
            // main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 450);
            this.Controls.Add(this.requestBox);
            this.Controls.Add(this.editBox);
            this.Controls.Add(this.queryBox);
            this.Name = "main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "main";
            this.Load += new System.EventHandler(this.main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.queryBox.ResumeLayout(false);
            this.editBox.ResumeLayout(false);
            this.editBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button addBtn;
        private System.Windows.Forms.Button editBtn;
        private System.Windows.Forms.Button removeBtn;
        private System.Windows.Forms.GroupBox queryBox;
        private System.Windows.Forms.GroupBox editBox;
        private System.Windows.Forms.ComboBox selectedKey;
        private System.Windows.Forms.GroupBox requestBox;
        private System.Windows.Forms.Label keyLabel;
    }
}