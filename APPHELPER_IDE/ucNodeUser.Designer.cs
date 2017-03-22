namespace TRP_APPHELPER_IDE
{
    partial class ucNodeUser
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel = new System.Windows.Forms.Panel();
            this.nodesControl = new TRP_APPHELPER_DLL.NodeEditorFile.NodesControl();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.gcQueryResult = new DevExpress.XtraGrid.GridControl();
            this.gvQuery = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.lblDosyaAd = new System.Windows.Forms.Label();
            this.txtNodeDosya = new System.Windows.Forms.TextBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonProcess = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcQueryResult)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvQuery)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(622, 485);
            this.splitContainer1.SplitterDistance = 378;
            this.splitContainer1.TabIndex = 0;
            // 
            // panel
            // 
            this.panel.AutoScroll = true;
            this.panel.Controls.Add(this.nodesControl);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(378, 485);
            this.panel.TabIndex = 0;
            // 
            // nodesControl
            // 
            this.nodesControl.BackgroundImage = global::TRP_APPHELPER_IDE.Properties.Resources.grid;
            this.nodesControl.Context = null;
            this.nodesControl.Location = new System.Drawing.Point(0, 0);
            this.nodesControl.Name = "nodesControl";
            this.nodesControl.Size = new System.Drawing.Size(5000, 5000);
            this.nodesControl.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.propertyGrid);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.gcQueryResult);
            this.splitContainer2.Panel2.Controls.Add(this.lblDosyaAd);
            this.splitContainer2.Panel2.Controls.Add(this.txtNodeDosya);
            this.splitContainer2.Panel2.Controls.Add(this.buttonSave);
            this.splitContainer2.Panel2.Controls.Add(this.buttonProcess);
            this.splitContainer2.Size = new System.Drawing.Size(240, 485);
            this.splitContainer2.SplitterDistance = 336;
            this.splitContainer2.TabIndex = 0;
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(240, 336);
            this.propertyGrid.TabIndex = 1;
            // 
            // gcQueryResult
            // 
            this.gcQueryResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcQueryResult.Location = new System.Drawing.Point(0, 23);
            this.gcQueryResult.MainView = this.gvQuery;
            this.gcQueryResult.Name = "gcQueryResult";
            this.gcQueryResult.Size = new System.Drawing.Size(240, 66);
            this.gcQueryResult.TabIndex = 4;
            this.gcQueryResult.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvQuery});
            // 
            // gvQuery
            // 
            this.gvQuery.GridControl = this.gcQueryResult;
            this.gvQuery.Name = "gvQuery";
            // 
            // lblDosyaAd
            // 
            this.lblDosyaAd.AutoSize = true;
            this.lblDosyaAd.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblDosyaAd.Location = new System.Drawing.Point(0, 89);
            this.lblDosyaAd.Name = "lblDosyaAd";
            this.lblDosyaAd.Size = new System.Drawing.Size(83, 13);
            this.lblDosyaAd.TabIndex = 3;
            this.lblDosyaAd.Text = "Dosya adı giriniz";
            // 
            // txtNodeDosya
            // 
            this.txtNodeDosya.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtNodeDosya.Location = new System.Drawing.Point(0, 102);
            this.txtNodeDosya.Name = "txtNodeDosya";
            this.txtNodeDosya.Size = new System.Drawing.Size(240, 20);
            this.txtNodeDosya.TabIndex = 2;
            // 
            // buttonSave
            // 
            this.buttonSave.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonSave.Location = new System.Drawing.Point(0, 122);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(240, 23);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonProcess
            // 
            this.buttonProcess.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonProcess.Location = new System.Drawing.Point(0, 0);
            this.buttonProcess.Name = "buttonProcess";
            this.buttonProcess.Size = new System.Drawing.Size(240, 23);
            this.buttonProcess.TabIndex = 0;
            this.buttonProcess.Text = "Process";
            this.buttonProcess.UseVisualStyleBackColor = true;
            this.buttonProcess.Click += new System.EventHandler(this.buttonProcess_Click);
            // 
            // ucNodeUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ucNodeUser";
            this.Size = new System.Drawing.Size(622, 485);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcQueryResult)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvQuery)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel;
        public TRP_APPHELPER_DLL.NodeEditorFile.NodesControl nodesControl;
        private System.Windows.Forms.SplitContainer splitContainer2;
        public System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.Button buttonProcess;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Label lblDosyaAd;
        public System.Windows.Forms.TextBox txtNodeDosya;
        public DevExpress.XtraGrid.GridControl gcQueryResult;
        public DevExpress.XtraGrid.Views.Grid.GridView gvQuery;
    }
    
}
