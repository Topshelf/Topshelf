namespace Topshelf.Configuration
{
    using System.ComponentModel;

    partial class ServiceConsole
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly IContainer _components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (_components != null))
            {
                _components.Dispose();
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
            this.viewServices = new System.Windows.Forms.ListView();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colType = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // viewServices
            // 
            this.viewServices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colType,
            this.colStatus});
            this.viewServices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewServices.Location = new System.Drawing.Point(0, 0);
            this.viewServices.Name = "viewServices";
            this.viewServices.Size = new System.Drawing.Size(715, 383);
            this.viewServices.TabIndex = 0;
            this.viewServices.UseCompatibleStateImageBehavior = false;
            this.viewServices.View = System.Windows.Forms.View.Details;
            // 
            // colName
            // 
            this.colName.Text = "Name";
            // 
            // colType
            // 
            this.colType.Text = "Type";
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            // 
            // ServiceConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 383);
            this.Controls.Add(this.viewServices);
            this.Name = "ServiceConsole";
            this.Text = "ServiceConsole";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView viewServices;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colStatus;
    }
}