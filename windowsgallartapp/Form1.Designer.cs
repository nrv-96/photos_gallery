namespace windowsgallartapp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
        /// Method required for Designer support — do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.flowLayoutPanelGallery = new System.Windows.Forms.FlowLayoutPanel();
            this.panelSidebar = new System.Windows.Forms.Panel();
            this.btnAllPhotos = new System.Windows.Forms.Button();
            this.btnAlbums = new System.Windows.Forms.Button();
            this.btnAddDirectory = new System.Windows.Forms.Button(); // Add Directory button
            this.btnCopy = new System.Windows.Forms.Button(); // Copy button
            this.btnMove = new System.Windows.Forms.Button(); // Move button
            this.listBoxAlbums = new System.Windows.Forms.ListBox();
            this.panelSidebar.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanelGallery
            // 
            this.flowLayoutPanelGallery.AutoScroll = true;
            this.flowLayoutPanelGallery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelGallery.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelGallery.Name = "flowLayoutPanelGallery";
            this.flowLayoutPanelGallery.Size = new System.Drawing.Size(784, 561);
            this.flowLayoutPanelGallery.TabIndex = 0;
            // 
            // panelSidebar
            // 
            this.panelSidebar.Controls.Add(this.listBoxAlbums);
            this.panelSidebar.Controls.Add(this.btnMove); // Add Move button
            this.panelSidebar.Controls.Add(this.btnCopy); // Add Copy button
            this.panelSidebar.Controls.Add(this.btnAddDirectory); // Add Directory button
            this.panelSidebar.Controls.Add(this.btnAllPhotos);
            this.panelSidebar.Controls.Add(this.btnAlbums);
            this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelSidebar.Location = new System.Drawing.Point(784, 0);
            this.panelSidebar.Name = "panelSidebar";
            this.panelSidebar.Size = new System.Drawing.Size(200, 561);
            this.panelSidebar.TabIndex = 1;
            // 
            // btnAlbums
            // 
            this.btnAlbums.Location = new System.Drawing.Point(12, 12);
            this.btnAlbums.Name = "btnAlbums";
            this.btnAlbums.Size = new System.Drawing.Size(175, 35);
            this.btnAlbums.TabIndex = 0;
            this.btnAlbums.Text = "Albums";
            this.btnAlbums.UseVisualStyleBackColor = true;
            this.btnAlbums.Click += new System.EventHandler(this.btnAlbums_Click);
            // 
            // btnAllPhotos
            // 
            this.btnAllPhotos.Location = new System.Drawing.Point(12, 53);
            this.btnAllPhotos.Name = "btnAllPhotos";
            this.btnAllPhotos.Size = new System.Drawing.Size(175, 35);
            this.btnAllPhotos.TabIndex = 1;
            this.btnAllPhotos.Text = "All Photos";
            this.btnAllPhotos.UseVisualStyleBackColor = true;
            this.btnAllPhotos.Click += new System.EventHandler(this.btnAllPhotos_Click);
            // 
            // btnAddDirectory
            // 
            this.btnAddDirectory.Location = new System.Drawing.Point(12, 94); // Position above Copy and Move
            this.btnAddDirectory.Name = "btnAddDirectory";
            this.btnAddDirectory.Size = new System.Drawing.Size(175, 35);
            this.btnAddDirectory.TabIndex = 2;
            this.btnAddDirectory.Text = "Add Directory";
            this.btnAddDirectory.UseVisualStyleBackColor = true;
            this.btnAddDirectory.Click += new System.EventHandler(this.btnAddDirectory_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(12, 135); // Position below Add Directory
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(175, 35);
            this.btnCopy.TabIndex = 3;
            this.btnCopy.Text = "Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Visible = false; // Hide by default
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnMove
            // 
            this.btnMove.Location = new System.Drawing.Point(12, 176); // Position below Copy
            this.btnMove.Name = "btnMove";
            this.btnMove.Size = new System.Drawing.Size(175, 35);
            this.btnMove.TabIndex = 4;
            this.btnMove.Text = "Move";
            this.btnMove.UseVisualStyleBackColor = true;
            this.btnMove.Visible = false; // Hide by default
            this.btnMove.Click += new System.EventHandler(this.btnMove_Click);
            // 
            // listBoxAlbums
            // 
            this.listBoxAlbums.FormattingEnabled = true;
            this.listBoxAlbums.ItemHeight = 16;
            this.listBoxAlbums.Location = new System.Drawing.Point(12, 217); // Position below Move
            this.listBoxAlbums.Name = "listBoxAlbums";
            this.listBoxAlbums.Size = new System.Drawing.Size(175, 372);
            this.listBoxAlbums.TabIndex = 5;
            this.listBoxAlbums.SelectedIndexChanged += new System.EventHandler(this.listBoxAlbums_SelectedIndexChanged);
            this.listBoxAlbums.Visible = false;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(984, 561);
            this.Controls.Add(this.flowLayoutPanelGallery);
            this.Controls.Add(this.panelSidebar);
            this.Name = "Form1";
            this.Text = "Photo Gallery App";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panelSidebar.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelGallery;
        private System.Windows.Forms.Panel panelSidebar;
        private System.Windows.Forms.Button btnAllPhotos;
        private System.Windows.Forms.Button btnAlbums;
        private System.Windows.Forms.ListBox listBoxAlbums;
        private System.Windows.Forms.Button btnMove;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnAddDirectory;
        
    }
}