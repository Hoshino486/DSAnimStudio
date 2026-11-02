using DSAnimStudio.Localization;
namespace DSAnimStudio.TaeEditor
{
    partial class TaeEditTaeHeaderForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">若应释放托管资源则为 true，否则为 false。</param>
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
        /// 设计器支持所必需的方法 —— 不要用代码编辑器修改
        /// 这个方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaeEditTaeHeaderForm));
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.buttonDiscardChanges = new System.Windows.Forms.Button();
            this.buttonSaveChanges = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // propertyGrid
            // 
            this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid.Location = new System.Drawing.Point(12, 12);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGrid.Size = new System.Drawing.Size(370, 252);
            this.propertyGrid.TabIndex = 0;
            this.propertyGrid.ToolbarVisible = false;
            // 
            // buttonDiscardChanges
            // 
            this.buttonDiscardChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDiscardChanges.Location = new System.Drawing.Point(268, 270);
            this.buttonDiscardChanges.Name = "buttonDiscardChanges";
            this.buttonDiscardChanges.Size = new System.Drawing.Size(114, 23);
            this.buttonDiscardChanges.TabIndex = 1;
            this.buttonDiscardChanges.Text = Loc.T("Discard Changes");
            this.buttonDiscardChanges.UseVisualStyleBackColor = true;
            this.buttonDiscardChanges.Click += new System.EventHandler(this.buttonDiscardChanges_Click);
            // 
            // buttonSaveChanges
            // 
            this.buttonSaveChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveChanges.Location = new System.Drawing.Point(157, 270);
            this.buttonSaveChanges.Name = "buttonSaveChanges";
            this.buttonSaveChanges.Size = new System.Drawing.Size(105, 23);
            this.buttonSaveChanges.TabIndex = 2;
            this.buttonSaveChanges.Text = Loc.T("Keep Changes");
            this.buttonSaveChanges.UseVisualStyleBackColor = true;
            this.buttonSaveChanges.Click += new System.EventHandler(this.buttonSaveChanges_Click);
            // 
            // TaeEditTaeHeaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(394, 299);
            this.ControlBox = false;
            this.Controls.Add(this.buttonSaveChanges);
            this.Controls.Add(this.buttonDiscardChanges);
            this.Controls.Add(this.propertyGrid);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(380, 200);
            this.Name = "TaeEditTaeHeaderForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = Loc.T("Edit TAE Header");
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TaeEditAnimPropertiesForm_FormClosing);
            this.Load += new System.EventHandler(this.TaeEditTaeHeaderForm_Load);
            this.Shown += new System.EventHandler(this.TaeEditAnimPropertiesForm_Shown);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button buttonDiscardChanges;
        private System.Windows.Forms.Button buttonSaveChanges;
        private System.Windows.Forms.PropertyGrid propertyGrid;
    }
}