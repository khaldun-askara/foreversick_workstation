
namespace foreversick_workstation
{
    partial class mainMenu_form
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer_main_menu = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main_menu)).BeginInit();
            this.splitContainer_main_menu.Panel1.SuspendLayout();
            this.splitContainer_main_menu.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_main_menu
            // 
            this.splitContainer_main_menu.BackColor = System.Drawing.Color.White;
            this.splitContainer_main_menu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main_menu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.splitContainer_main_menu.IsSplitterFixed = true;
            this.splitContainer_main_menu.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_main_menu.Name = "splitContainer_main_menu";
            // 
            // splitContainer_main_menu.Panel1
            // 
            this.splitContainer_main_menu.Panel1.AutoScroll = true;
            this.splitContainer_main_menu.Panel1.BackColor = System.Drawing.Color.White;
            this.splitContainer_main_menu.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer_main_menu.Panel1.Padding = new System.Windows.Forms.Padding(25);
            // 
            // splitContainer_main_menu.Panel2
            // 
            this.splitContainer_main_menu.Panel2.BackColor = System.Drawing.Color.White;
            this.splitContainer_main_menu.Size = new System.Drawing.Size(1209, 706);
            this.splitContainer_main_menu.SplitterDistance = 499;
            this.splitContainer_main_menu.SplitterWidth = 20;
            this.splitContainer_main_menu.TabIndex = 0;
            this.splitContainer_main_menu.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Roboto Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox1.Location = new System.Drawing.Point(25, 25);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(449, 656);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Предложения пользователей";
            // 
            // mainMenu_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1209, 706);
            this.Controls.Add(this.splitContainer_main_menu);
            this.Font = new System.Drawing.Font("Roboto Light", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(1000, 700);
            this.Name = "mainMenu_form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Предложения пользователей";
            this.splitContainer_main_menu.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main_menu)).EndInit();
            this.splitContainer_main_menu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_main_menu;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

