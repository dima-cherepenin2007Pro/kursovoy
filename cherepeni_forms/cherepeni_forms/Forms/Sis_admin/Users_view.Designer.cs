namespace cherepeni_forms.Forms.Sis_admin
{
    partial class Users_view
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
            this.AddUserButton = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.RefreshButton = new System.Windows.Forms.Button();
            this.txtCount = new System.Windows.Forms.Label();
            this.SortButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SortCombo = new System.Windows.Forms.ComboBox();
            this.FilterButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.FilterCombo = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // AddUserButton
            // 
            this.AddUserButton.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.AddUserButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.AddUserButton.Location = new System.Drawing.Point(804, 421);
            this.AddUserButton.Name = "AddUserButton";
            this.AddUserButton.Size = new System.Drawing.Size(166, 66);
            this.AddUserButton.TabIndex = 26;
            this.AddUserButton.Text = "Добавить";
            this.AddUserButton.UseVisualStyleBackColor = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(1, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(980, 357);
            this.dataGridView1.TabIndex = 19;
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            // 
            // RefreshButton
            // 
            this.RefreshButton.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.RefreshButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.RefreshButton.Location = new System.Drawing.Point(804, 365);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(166, 50);
            this.RefreshButton.TabIndex = 31;
            this.RefreshButton.Text = "Обновить";
            this.RefreshButton.UseVisualStyleBackColor = false;
            // 
            // txtCount
            // 
            this.txtCount.AutoSize = true;
            this.txtCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.txtCount.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.txtCount.Location = new System.Drawing.Point(12, 372);
            this.txtCount.Name = "txtCount";
            this.txtCount.Size = new System.Drawing.Size(140, 20);
            this.txtCount.TabIndex = 37;
            this.txtCount.Text = "Кол-во записей";
            // 
            // SortButton
            // 
            this.SortButton.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.SortButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SortButton.Location = new System.Drawing.Point(244, 568);
            this.SortButton.Name = "SortButton";
            this.SortButton.Size = new System.Drawing.Size(222, 50);
            this.SortButton.TabIndex = 30;
            this.SortButton.Text = "Сортировать";
            this.SortButton.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.label1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label1.Location = new System.Drawing.Point(297, 508);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 20);
            this.label1.TabIndex = 32;
            this.label1.Text = "СОРТИРОВКА";
            // 
            // SortCombo
            // 
            this.SortCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SortCombo.FormattingEnabled = true;
            this.SortCombo.Location = new System.Drawing.Point(244, 533);
            this.SortCombo.Name = "SortCombo";
            this.SortCombo.Size = new System.Drawing.Size(222, 21);
            this.SortCombo.TabIndex = 33;
            // 
            // FilterButton
            // 
            this.FilterButton.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.FilterButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FilterButton.Location = new System.Drawing.Point(495, 568);
            this.FilterButton.Name = "FilterButton";
            this.FilterButton.Size = new System.Drawing.Size(222, 50);
            this.FilterButton.TabIndex = 34;
            this.FilterButton.Text = "Фильтровать";
            this.FilterButton.UseVisualStyleBackColor = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.label3.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label3.Location = new System.Drawing.Point(545, 508);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(132, 20);
            this.label3.TabIndex = 35;
            this.label3.Text = "ФИЛЬТРАЦИЯ";
            // 
            // FilterCombo
            // 
            this.FilterCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FilterCombo.FormattingEnabled = true;
            this.FilterCombo.Location = new System.Drawing.Point(495, 533);
            this.FilterCombo.Name = "FilterCombo";
            this.FilterCombo.Size = new System.Drawing.Size(222, 21);
            this.FilterCombo.TabIndex = 36;
            // 
            // Users_view
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(982, 499);
            this.Controls.Add(this.txtCount);
            this.Controls.Add(this.FilterCombo);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.FilterButton);
            this.Controls.Add(this.SortCombo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RefreshButton);
            this.Controls.Add(this.SortButton);
            this.Controls.Add(this.AddUserButton);
            this.Controls.Add(this.dataGridView1);
            this.Name = "Users_view";
            this.Text = "Пользователи";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button AddUserButton;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.Label txtCount;
        private System.Windows.Forms.Button SortButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox SortCombo;
        private System.Windows.Forms.Button FilterButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox FilterCombo;
    }
}