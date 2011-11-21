namespace SecretSplitterWinForms {
    partial class MainForm {
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
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabRecover = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtShares = new System.Windows.Forms.TextBox();
            this.btnRecover = new System.Windows.Forms.Button();
            this.tabCreate = new System.Windows.Forms.TabPage();
            this.pnlCreateSharesInfo = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.lblThresholdDescriptionSuffix = new System.Windows.Forms.Label();
            this.nudShares = new System.Windows.Forms.NumericUpDown();
            this.nudThreshold = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.pnlCreateSecretFileAdvancedInfo = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.cboKeySizes = new System.Windows.Forms.ComboBox();
            this.txtMasterKey = new System.Windows.Forms.TextBox();
            this.chkHideMasterKey = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.pnlCreateMessage = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtSecretMessage = new System.Windows.Forms.TextBox();
            this.pnlCreateSecretFileBasicInfo = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.btnBrowsePlaintext = new System.Windows.Forms.Button();
            this.txtSecretFilePath = new System.Windows.Forms.TextBox();
            this.chkShowAdvancedFileOptions = new System.Windows.Forms.CheckBox();
            this.pnlCreateHeader = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.rdoHaveSecretFile = new System.Windows.Forms.RadioButton();
            this.rdoHaveSecretMessage = new System.Windows.Forms.RadioButton();
            this.btnCreateShares = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tabs.SuspendLayout();
            this.tabRecover.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabCreate.SuspendLayout();
            this.pnlCreateSharesInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudShares)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreshold)).BeginInit();
            this.pnlCreateSecretFileAdvancedInfo.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.pnlCreateMessage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.pnlCreateSecretFileBasicInfo.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.pnlCreateHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.tabRecover);
            this.tabs.Controls.Add(this.tabCreate);
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Location = new System.Drawing.Point(0, 0);
            this.tabs.Margin = new System.Windows.Forms.Padding(2);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(760, 478);
            this.tabs.TabIndex = 0;
            // 
            // tabRecover
            // 
            this.tabRecover.Controls.Add(this.groupBox1);
            this.tabRecover.Controls.Add(this.btnRecover);
            this.tabRecover.Location = new System.Drawing.Point(4, 22);
            this.tabRecover.Name = "tabRecover";
            this.tabRecover.Padding = new System.Windows.Forms.Padding(3);
            this.tabRecover.Size = new System.Drawing.Size(752, 452);
            this.tabRecover.TabIndex = 3;
            this.tabRecover.Text = "Recover";
            this.tabRecover.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtShares);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(746, 403);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Enter enough secret pieces to recover the secret (separate each piece by a new li" +
    "ne):";
            // 
            // txtShares
            // 
            this.txtShares.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtShares.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtShares.HideSelection = false;
            this.txtShares.Location = new System.Drawing.Point(3, 16);
            this.txtShares.Multiline = true;
            this.txtShares.Name = "txtShares";
            this.txtShares.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtShares.Size = new System.Drawing.Size(740, 384);
            this.txtShares.TabIndex = 1;
            // 
            // btnRecover
            // 
            this.btnRecover.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnRecover.Location = new System.Drawing.Point(3, 406);
            this.btnRecover.Name = "btnRecover";
            this.btnRecover.Size = new System.Drawing.Size(746, 43);
            this.btnRecover.TabIndex = 2;
            this.btnRecover.Text = "&Recover Secret";
            this.btnRecover.UseVisualStyleBackColor = true;
            this.btnRecover.Click += new System.EventHandler(this.btnRecover_Click);
            // 
            // tabCreate
            // 
            this.tabCreate.Controls.Add(this.pnlCreateSharesInfo);
            this.tabCreate.Controls.Add(this.pnlCreateSecretFileAdvancedInfo);
            this.tabCreate.Controls.Add(this.pnlCreateMessage);
            this.tabCreate.Controls.Add(this.pnlCreateSecretFileBasicInfo);
            this.tabCreate.Controls.Add(this.pnlCreateHeader);
            this.tabCreate.Controls.Add(this.btnCreateShares);
            this.tabCreate.Location = new System.Drawing.Point(4, 22);
            this.tabCreate.Name = "tabCreate";
            this.tabCreate.Padding = new System.Windows.Forms.Padding(3);
            this.tabCreate.Size = new System.Drawing.Size(752, 452);
            this.tabCreate.TabIndex = 4;
            this.tabCreate.Text = "Create";
            this.tabCreate.UseVisualStyleBackColor = true;
            // 
            // pnlCreateSharesInfo
            // 
            this.pnlCreateSharesInfo.Controls.Add(this.label3);
            this.pnlCreateSharesInfo.Controls.Add(this.lblThresholdDescriptionSuffix);
            this.pnlCreateSharesInfo.Controls.Add(this.nudShares);
            this.pnlCreateSharesInfo.Controls.Add(this.nudThreshold);
            this.pnlCreateSharesInfo.Controls.Add(this.label4);
            this.pnlCreateSharesInfo.Controls.Add(this.label5);
            this.pnlCreateSharesInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCreateSharesInfo.Location = new System.Drawing.Point(3, 336);
            this.pnlCreateSharesInfo.Name = "pnlCreateSharesInfo";
            this.pnlCreateSharesInfo.Size = new System.Drawing.Size(746, 63);
            this.pnlCreateSharesInfo.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(166, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "I would like to split the secret into ";
            // 
            // lblThresholdDescriptionSuffix
            // 
            this.lblThresholdDescriptionSuffix.AutoSize = true;
            this.lblThresholdDescriptionSuffix.Location = new System.Drawing.Point(115, 43);
            this.lblThresholdDescriptionSuffix.Name = "lblThresholdDescriptionSuffix";
            this.lblThresholdDescriptionSuffix.Size = new System.Drawing.Size(241, 13);
            this.lblThresholdDescriptionSuffix.TabIndex = 15;
            this.lblThresholdDescriptionSuffix.Text = "of those pieces can be used to restore the secret.";
            // 
            // nudShares
            // 
            this.nudShares.Location = new System.Drawing.Point(171, 8);
            this.nudShares.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.nudShares.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudShares.Name = "nudShares";
            this.nudShares.Size = new System.Drawing.Size(49, 20);
            this.nudShares.TabIndex = 11;
            this.nudShares.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // nudThreshold
            // 
            this.nudThreshold.Location = new System.Drawing.Point(69, 39);
            this.nudThreshold.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.nudThreshold.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudThreshold.Name = "nudThreshold";
            this.nudThreshold.Size = new System.Drawing.Size(45, 20);
            this.nudThreshold.TabIndex = 14;
            this.nudThreshold.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(226, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "pieces";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "so that any";
            // 
            // pnlCreateSecretFileAdvancedInfo
            // 
            this.pnlCreateSecretFileAdvancedInfo.Controls.Add(this.groupBox3);
            this.pnlCreateSecretFileAdvancedInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCreateSecretFileAdvancedInfo.Location = new System.Drawing.Point(3, 232);
            this.pnlCreateSecretFileAdvancedInfo.Name = "pnlCreateSecretFileAdvancedInfo";
            this.pnlCreateSecretFileAdvancedInfo.Size = new System.Drawing.Size(746, 104);
            this.pnlCreateSecretFileAdvancedInfo.TabIndex = 3;
            this.pnlCreateSecretFileAdvancedInfo.Visible = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel1);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(746, 104);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Advanced Options";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cboKeySizes, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtMasterKey, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkHideMasterKey, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(740, 85);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(51, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "Key Size:";
            // 
            // cboKeySizes
            // 
            this.cboKeySizes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboKeySizes.FormattingEnabled = true;
            this.cboKeySizes.Items.AddRange(new object[] {
            "Good - 32 characters (128 bits)",
            "Very Strong - 48 characters (192 bits)",
            "Paranoid - 64 characters (256 bits)"});
            this.cboKeySizes.Location = new System.Drawing.Point(73, 3);
            this.cboKeySizes.Name = "cboKeySizes";
            this.cboKeySizes.Size = new System.Drawing.Size(664, 21);
            this.cboKeySizes.TabIndex = 6;
            this.cboKeySizes.TextChanged += new System.EventHandler(this.cboKeySizes_TextChanged);
            // 
            // txtMasterKey
            // 
            this.txtMasterKey.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMasterKey.Location = new System.Drawing.Point(73, 34);
            this.txtMasterKey.Name = "txtMasterKey";
            this.txtMasterKey.PasswordChar = '*';
            this.txtMasterKey.Size = new System.Drawing.Size(664, 20);
            this.txtMasterKey.TabIndex = 8;
            // 
            // chkHideMasterKey
            // 
            this.chkHideMasterKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkHideMasterKey.AutoSize = true;
            this.chkHideMasterKey.Checked = true;
            this.chkHideMasterKey.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHideMasterKey.Location = new System.Drawing.Point(668, 65);
            this.chkHideMasterKey.Name = "chkHideMasterKey";
            this.chkHideMasterKey.Size = new System.Drawing.Size(69, 17);
            this.chkHideMasterKey.TabIndex = 9;
            this.chkHideMasterKey.Text = "&Hide Key";
            this.chkHideMasterKey.UseVisualStyleBackColor = true;
            this.chkHideMasterKey.CheckedChanged += new System.EventHandler(this.chkHideMasterKey_CheckedChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 31);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(63, 13);
            this.label9.TabIndex = 7;
            this.label9.Text = "Master Key:";
            // 
            // pnlCreateMessage
            // 
            this.pnlCreateMessage.Controls.Add(this.groupBox2);
            this.pnlCreateMessage.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCreateMessage.Location = new System.Drawing.Point(3, 114);
            this.pnlCreateMessage.Name = "pnlCreateMessage";
            this.pnlCreateMessage.Size = new System.Drawing.Size(746, 118);
            this.pnlCreateMessage.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtSecretMessage);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(746, 118);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Secret Message";
            // 
            // txtSecretMessage
            // 
            this.txtSecretMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSecretMessage.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSecretMessage.HideSelection = false;
            this.txtSecretMessage.Location = new System.Drawing.Point(3, 16);
            this.txtSecretMessage.Multiline = true;
            this.txtSecretMessage.Name = "txtSecretMessage";
            this.txtSecretMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSecretMessage.Size = new System.Drawing.Size(740, 99);
            this.txtSecretMessage.TabIndex = 0;
            this.txtSecretMessage.Text = "Hello Secret World!";
            // 
            // pnlCreateSecretFileBasicInfo
            // 
            this.pnlCreateSecretFileBasicInfo.Controls.Add(this.tableLayoutPanel2);
            this.pnlCreateSecretFileBasicInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCreateSecretFileBasicInfo.Location = new System.Drawing.Point(3, 52);
            this.pnlCreateSecretFileBasicInfo.Name = "pnlCreateSecretFileBasicInfo";
            this.pnlCreateSecretFileBasicInfo.Size = new System.Drawing.Size(746, 62);
            this.pnlCreateSecretFileBasicInfo.TabIndex = 13;
            this.pnlCreateSecretFileBasicInfo.Visible = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.chkShowAdvancedFileOptions, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(746, 62);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnBrowsePlaintext, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.txtSecretFilePath, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(740, 29);
            this.tableLayoutPanel3.TabIndex = 8;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(88, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "that is located at:";
            // 
            // btnBrowsePlaintext
            // 
            this.btnBrowsePlaintext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBrowsePlaintext.Location = new System.Drawing.Point(643, 3);
            this.btnBrowsePlaintext.Name = "btnBrowsePlaintext";
            this.btnBrowsePlaintext.Size = new System.Drawing.Size(94, 23);
            this.btnBrowsePlaintext.TabIndex = 10;
            this.btnBrowsePlaintext.Text = "&Browse...";
            this.btnBrowsePlaintext.UseVisualStyleBackColor = true;
            this.btnBrowsePlaintext.Click += new System.EventHandler(this.btnBrowsePlaintext_Click);
            // 
            // txtSecretFilePath
            // 
            this.txtSecretFilePath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSecretFilePath.Location = new System.Drawing.Point(103, 3);
            this.txtSecretFilePath.Name = "txtSecretFilePath";
            this.txtSecretFilePath.Size = new System.Drawing.Size(534, 20);
            this.txtSecretFilePath.TabIndex = 9;
            // 
            // chkShowAdvancedFileOptions
            // 
            this.chkShowAdvancedFileOptions.AutoSize = true;
            this.chkShowAdvancedFileOptions.Location = new System.Drawing.Point(3, 38);
            this.chkShowAdvancedFileOptions.Name = "chkShowAdvancedFileOptions";
            this.chkShowAdvancedFileOptions.Size = new System.Drawing.Size(144, 17);
            this.chkShowAdvancedFileOptions.TabIndex = 7;
            this.chkShowAdvancedFileOptions.Text = "Show &Advanced Options";
            this.chkShowAdvancedFileOptions.UseVisualStyleBackColor = true;
            this.chkShowAdvancedFileOptions.CheckedChanged += new System.EventHandler(this.chkShowAdvancedFileOptions_CheckedChanged);
            // 
            // pnlCreateHeader
            // 
            this.pnlCreateHeader.Controls.Add(this.label1);
            this.pnlCreateHeader.Controls.Add(this.label2);
            this.pnlCreateHeader.Controls.Add(this.rdoHaveSecretFile);
            this.pnlCreateHeader.Controls.Add(this.rdoHaveSecretMessage);
            this.pnlCreateHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCreateHeader.Location = new System.Drawing.Point(3, 3);
            this.pnlCreateHeader.Name = "pnlCreateHeader";
            this.pnlCreateHeader.Size = new System.Drawing.Size(746, 49);
            this.pnlCreateHeader.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(294, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "In order to split up your secret, please describe your situation:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "I have a secret:";
            // 
            // rdoHaveSecretFile
            // 
            this.rdoHaveSecretFile.AutoSize = true;
            this.rdoHaveSecretFile.Location = new System.Drawing.Point(164, 27);
            this.rdoHaveSecretFile.Name = "rdoHaveSecretFile";
            this.rdoHaveSecretFile.Size = new System.Drawing.Size(38, 17);
            this.rdoHaveSecretFile.TabIndex = 3;
            this.rdoHaveSecretFile.Text = "file";
            this.rdoHaveSecretFile.UseVisualStyleBackColor = true;
            this.rdoHaveSecretFile.CheckedChanged += new System.EventHandler(this.ChangedSecretType);
            // 
            // rdoHaveSecretMessage
            // 
            this.rdoHaveSecretMessage.AutoSize = true;
            this.rdoHaveSecretMessage.Checked = true;
            this.rdoHaveSecretMessage.Location = new System.Drawing.Point(91, 27);
            this.rdoHaveSecretMessage.Name = "rdoHaveSecretMessage";
            this.rdoHaveSecretMessage.Size = new System.Drawing.Size(67, 17);
            this.rdoHaveSecretMessage.TabIndex = 2;
            this.rdoHaveSecretMessage.TabStop = true;
            this.rdoHaveSecretMessage.Text = "message";
            this.rdoHaveSecretMessage.UseVisualStyleBackColor = true;
            this.rdoHaveSecretMessage.CheckedChanged += new System.EventHandler(this.ChangedSecretType);
            // 
            // btnCreateShares
            // 
            this.btnCreateShares.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnCreateShares.Location = new System.Drawing.Point(3, 404);
            this.btnCreateShares.Name = "btnCreateShares";
            this.btnCreateShares.Size = new System.Drawing.Size(746, 45);
            this.btnCreateShares.TabIndex = 16;
            this.btnCreateShares.Text = "&Create Secret Pieces";
            this.btnCreateShares.UseVisualStyleBackColor = true;
            this.btnCreateShares.Click += new System.EventHandler(this.btnCreateShares_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(760, 478);
            this.Controls.Add(this.tabs);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(575, 350);
            this.Name = "MainForm";
            this.Text = "Secret Splitter";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabs.ResumeLayout(false);
            this.tabRecover.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabCreate.ResumeLayout(false);
            this.pnlCreateSharesInfo.ResumeLayout(false);
            this.pnlCreateSharesInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudShares)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreshold)).EndInit();
            this.pnlCreateSecretFileAdvancedInfo.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.pnlCreateMessage.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.pnlCreateSecretFileBasicInfo.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.pnlCreateHeader.ResumeLayout(false);
            this.pnlCreateHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.TabPage tabRecover;
        private System.Windows.Forms.Button btnRecover;
        private System.Windows.Forms.TabPage tabCreate;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtShares;
        private System.Windows.Forms.Button btnCreateShares;
        private System.Windows.Forms.Panel pnlCreateSharesInfo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblThresholdDescriptionSuffix;
        private System.Windows.Forms.NumericUpDown nudShares;
        private System.Windows.Forms.NumericUpDown nudThreshold;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel pnlCreateSecretFileAdvancedInfo;
        private System.Windows.Forms.Panel pnlCreateSecretFileBasicInfo;
        private System.Windows.Forms.Panel pnlCreateMessage;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtSecretMessage;
        private System.Windows.Forms.Panel pnlCreateHeader;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton rdoHaveSecretFile;
        private System.Windows.Forms.RadioButton rdoHaveSecretMessage;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cboKeySizes;
        private System.Windows.Forms.TextBox txtMasterKey;
        private System.Windows.Forms.CheckBox chkHideMasterKey;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.CheckBox chkShowAdvancedFileOptions;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnBrowsePlaintext;
        private System.Windows.Forms.TextBox txtSecretFilePath;
    }
}

