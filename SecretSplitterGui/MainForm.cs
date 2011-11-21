using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Moserware.Security.Cryptography;

// NOTE: This GUI is just a prototype of an idea and not meant to be a demonstration of good techniques.
//       Obviously a better designed UI would make use of resource strings, data binding, and enhanced 
//       layout techniques rather than using magic numbers for things like widths. This UI is just 
//       exploring some options that might be good to check for in a full UI.
namespace SecretSplitterWinForms {
    public partial class MainForm : Form {
        public const string DialogFilter = "Split Secret Files (*.splitsecret)|*.splitsecret|GNU Privacy Guard Files (*.gpg)|*.gpg|All Files (*.*)|*.*";
        public const string AllFilesFilter = "All Files (*.*)|*.*";
        private bool _HasAgreedToSafetyWarning;
        private Control _LastSelectedShareTypeControl;
        
        public MainForm() {
            InitializeComponent();
            Text += " " + Moserware.Security.Cryptography.Versioning.VersionInfo.CurrentVersion;
        }

        private void ChangedSecretType(object sender, System.EventArgs e) {
            UpdateSecretPanels();
        }

        private void UpdateSecretPanels() {
            // I should be using databinding and other fancy things.. oh well :)
            var selectedShareType = rdoHaveSecretFile.Checked ? rdoHaveSecretFile : rdoHaveSecretMessage;
            btnCreateShares.Text = rdoHaveSecretFile.Checked ? "&Save Encrypted File and Create Secret Pieces" : "&Split Message Into Pieces";
            if((_LastSelectedShareTypeControl != selectedShareType) && (selectedShareType == rdoHaveSecretFile)) {
                GenerateRandomFileKey();
            }

            _LastSelectedShareTypeControl = selectedShareType;
            pnlCreateMessage.Visible = rdoHaveSecretMessage.Checked;
            pnlCreateSecretFileAdvancedInfo.Visible = rdoHaveSecretFile.Checked && chkShowAdvancedFileOptions.Checked;
            pnlCreateSecretFileBasicInfo.Visible = rdoHaveSecretFile.Checked;

            // Set minimum window size
            // TODO: Should be smarter about this..
            int minHeight = 80 + tabCreate.Controls.Cast<Control>().Where(c => c.Visible).Select(c => c.Height).Sum();
            MinimumSize = new Size(MinimumSize.Width, minHeight);
            Size = MinimumSize;
        }

        private void GenerateRandomFileKey() {
            // BONUS: you can specify however many characters you want ;)
            var match = Regex.Match(cboKeySizes.Text, @"(?<charCount>[0-9]+)\s*characters\s*\((?<bitCount>[0-9]+)\s*bits\s*\)");
            if(!match.Success) {
                ShowError("Specify a valid key size", "Invalid Key Size");
                return;
            }

            var charSize = Int32.Parse(match.Groups["charCount"].Value);
            var bitSize = Int32.Parse(match.Groups["bitCount"].Value);
            
            // every char is 4 bits
            var charBitSize = charSize*4;

            // If there is disagreement, pick the bigger one
            var maxBitSize = Math.Max(charBitSize, bitSize);

            txtMasterKey.Text = HexadecimalPasswordGenerator.GeneratePasswordOfBitSize(maxBitSize);
        }

        private void btnCreateShares_Click(object sender, System.EventArgs e) {
            if(rdoHaveSecretMessage.Checked) {
                var shares = CreateSecretMessageShares();
                if(!String.IsNullOrEmpty(shares)) {
                    txtShares.Text = shares;
                }
            }
            else if(rdoHaveSecretFile.Checked) {
                CreateSecretFileShares();
            }
        }

        private void CreateSecretFileShares() {
            if (!TryVerifyThresholdValues()) {
                return;
            }

            if(!File.Exists(txtSecretFilePath.Text)) {
                ShowError("Secret file does not exist", "File Not Found");
                return;
            }

            if(String.IsNullOrEmpty(txtMasterKey.Text)) {
                ShowError("A valid key is required to encrypt a file.", "Invalid Key Length");
                return;
            }

            byte[] keyBytes;
            if(!SecretEncoder.TryParseHexString(txtMasterKey.Text, out keyBytes)) {
                ShowError("The key must contain all hexadecimal characters.", "Invalid Key Character");
                return;
            }

            SplitSecret splitSecret = SecretSplitter.SplitFile(keyBytes, (int)nudThreshold.Value);

            saveFileDialog.Filter = DialogFilter;
            saveFileDialog.Title = "Save Encrypted File";
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(txtSecretFilePath.Text);
            saveFileDialog.FileName = Path.GetFileName(Path.ChangeExtension(txtSecretFilePath.Text, ".splitsecret"));
            saveFileDialog.AddExtension = true;

            ShowInfo("You'll now have to specify where to save the encrypted file.", "Specify Encrypted File Path");
            if(saveFileDialog.ShowDialog() != DialogResult.OK) {
                return;
            }

            splitSecret.EncryptFile(txtSecretFilePath.Text, saveFileDialog.FileName);

            var sb = new StringBuilder();
            sb.AppendLine("Here are your secret pieces that form the decryption key for the encrypted file located at:");
            sb.AppendLine(saveFileDialog.FileName);
            sb.AppendLine();
            int shareWidth = saveFileDialog.FileName.Length;

            foreach(var currentShare in splitSecret.GetShares((int)nudShares.Value)) {
                string currentShareText = currentShare.ToString();
                sb.AppendLine(currentShareText);
                shareWidth = Math.Max(shareWidth, currentShareText.Length);
                sb.AppendLine();
            }

            SetWidthFromShareLength(shareWidth);
            
            sb.AppendLine("When distributing the secret pieces, remember to also make sure that you distribute the encrypted file. It's safe to email the encrypted file, but you should securely distribute the secret pieces (i.e. in person).");
            sb.AppendLine();
            sb.AppendLine("Make sure that each person knows that exactly " + ((int) nudThreshold.Value) + " pieces are required to reconstruct the file.");

            txtShares.Text = sb.ToString();
            tabs.SelectedTab = tabRecover;
        }

        private void SetWidthFromShareLength(int shareWidth) {
            var textSize = TextRenderer.MeasureText(new string('0', shareWidth), txtShares.Font);
            // HACK to get shares to to not wordwrap
            var calculatedWidthFromText = textSize.Width + 70;
            var screenBounds = Screen.FromControl(txtShares).Bounds;
            var screenLeft = Left - screenBounds.Left;
            var screenWidth = screenBounds.Width;
            Width = Math.Max(Math.Max(Math.Min(calculatedWidthFromText, screenWidth - screenLeft), MinimumSize.Width), Width);
        }

        private void ShowInfo(string text, string caption = null) {
            ShowMessage(MessageBoxIcon.Information, text, caption);
        }

        private void ShowError(string text, string caption = null) {
            ShowMessage(MessageBoxIcon.Error, text, caption);
        }

        private void ShowMessage(MessageBoxIcon icon, string text, string caption = null) {
            MessageBox.Show(this, text, caption, MessageBoxButtons.OK, icon);
        }

        private string CreateSecretMessageShares() {
            if (!TryVerifyThresholdValues()) {
                return null;
            }

            if(String.IsNullOrWhiteSpace(txtSecretMessage.Text)) {
                ShowError("Enter something for the secret message", "Empty secret");
                return null;
            }

            var sb = new StringBuilder();
            sb.AppendFormat("Your secret message has been split into the following {0} pieces:", nudShares.Value);
            sb.AppendLine();
            sb.AppendLine();
            int maxShareWidth = 0;
            foreach(var currentSplit in SecretSplitter.SplitMessage(txtSecretMessage.Text, (int)nudThreshold.Value, (int)nudShares.Value)) {
                sb.AppendLine(currentSplit);
                maxShareWidth = Math.Max(maxShareWidth, currentSplit.Length);
                sb.AppendLine();
            }

            SetWidthFromShareLength(maxShareWidth);

            sb.AppendFormat("To reconstruct your secret, you'll need to provide exactly {0} of the above pieces. Please remember to keep the pieces safe and give them only to people you trust.", nudThreshold.Value);
            sb.AppendLine();

            tabs.SelectedTab = tabRecover;
            return sb.ToString();
        }

        private bool TryVerifyThresholdValues() {
            if(nudThreshold.Value <= nudShares.Value) {
                return true;
            }

            ShowError("The number of required pieces must equal or exceed the number of pieces.", "Invalid minimum piece count");
            return false;
        }

        private void MainForm_Load(object sender, System.EventArgs e) {
            // HACK to get reasonable height
            tabs.SelectedTab = tabCreate;
            UpdateSecretPanels();
            cboKeySizes.SelectedIndex = 0;
            tabs.SelectedTab = tabRecover;
        }

        private void btnRecover_Click(object sender, EventArgs e) {
            if(String.IsNullOrWhiteSpace(txtShares.Text)) {
                if(MessageBox.Show(this, "No secret pieces were specified. Would you like to see an example?", "Example?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    SetRecoverTextToExample();
                }
                return;
            }
            CombinedSecret combinedSecret;
            
            try {
                combinedSecret = SecretCombiner.Combine(txtShares.Text);
            }
            catch(InvalidChecksumShareException invalidChecksumShareException) {
                // We can provide a little UI magic
                var badShareText = invalidChecksumShareException.InvalidShare;
                int ixBadShareStart = txtShares.Text.IndexOf(badShareText);
                txtShares.SelectionStart = ixBadShareStart;
                txtShares.SelectionLength = badShareText.Length;
                ShowError("The selected secret piece seems to have been typed incorrectly.");
                return;
            }
            catch(Exception exception) {
                if (MessageBox.Show(this, exception.Message + Environment.NewLine + Environment.NewLine + "Would you like to see an example? (If yes, the current textbox content will be replaced)", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes) {
                    SetRecoverTextToExample();
                }
                
                return;
            }
            
            if (combinedSecret.ShareType == SecretShareType.File) {
                openFileDialog.Filter = DialogFilter;
                openFileDialog.Title = "Open Encrypted File";
                openFileDialog.FileName = "";

                if (!_HasAgreedToSafetyWarning) {
                    ShowInfo("It seems like you're trying to recover a file. You'll now have to specify the encrypted file in order to decrypt it. This encrypted file should have been given to you by whoever gave you the secret pieces.", "Specify File to Open");
                }

                if(openFileDialog.ShowDialog(this) != DialogResult.OK) {
                    return;
                }

                try {
                    using (var encryptedFileStream = File.OpenRead(openFileDialog.FileName)) {
                        string originalFileName;
                        DateTime originalFileDate;
                        var decryptedStream = combinedSecret.Decrypt(encryptedFileStream, out originalFileName,
                                                                     out originalFileDate);

                        if (!_HasAgreedToSafetyWarning) {
                            if (
                                MessageBox.Show(this,
                                                "You'll now need to specify where to save the decrypted file. Decrypted secret files typically contain sensitive information. Will you save the decrypted file in a safe location (i.e. an encrypted hard drive or USB drive that you will destroy or securely erase)?",
                                                "Will You Keep the Decrypted Data Safe?", MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Question) != DialogResult.Yes) {
                                ShowError("Decryption aborted to protect privacy of contents.", "Unable to Decrypt");
                                return;
                            }
                            _HasAgreedToSafetyWarning = true;
                        }

                        saveFileDialog.DefaultExt = Path.GetExtension(originalFileName);
                        saveFileDialog.Filter = String.Format("{0} files (*.{1})|*.{1}", saveFileDialog.DefaultExt.ToUpperInvariant(), saveFileDialog.DefaultExt) + "|" + AllFilesFilter;
                        saveFileDialog.Title = "Save Decrypted File";
                        
                        saveFileDialog.FileName = Path.GetFileName(originalFileName);
                        
                        if (saveFileDialog.ShowDialog(this) != DialogResult.OK) {
                            return;
                        }

                        if (File.Exists(saveFileDialog.FileName)) {
                            File.Delete(saveFileDialog.FileName);
                        }

                        using (var decryptedFileStream = File.OpenWrite(saveFileDialog.FileName)) {
                            decryptedStream.CopyTo(decryptedFileStream);
                        }

                        File.SetLastWriteTimeUtc(saveFileDialog.FileName, originalFileDate.ToUniversalTime());

                        if(MessageBox.Show(this, "Would you like to open the decrypted file?", "Open Decrypted File?", MessageBoxButtons.YesNo, MessageBoxIcon.Question)== DialogResult.Yes) {
                            Process.Start(new ProcessStartInfo(saveFileDialog.FileName) {UseShellExecute = true});
                        }
                    }
                } 
                catch(ModificationDetectedException modificationDetectedException) {
                    ShowError("It looks like the file was tampered with or the given secret pieces were invalid.",
                              "Modification Detected");
                }
                catch(Exception exception) {
                    ShowError("There was an error while trying to decrypt the file. Please check to make sure that you only typed in the fewest number of secret pieces needed and that you selected the proper file to decrypt and that the file wasn't tampered.");
                }
            }
            else {
                rdoHaveSecretMessage.Checked = true;
                tabs.SelectedTab = tabCreate;
                var recoveredTextString = combinedSecret.RecoveredTextString;

                if(!recoveredTextString.Any(c => Char.IsControl(c) && !Char.IsWhiteSpace(c))) {
                    txtSecretMessage.Text = combinedSecret.RecoveredTextString;
                }
                else {
                    txtSecretMessage.Text = combinedSecret.RecoveredHexString;
                    ShowMessage(MessageBoxIcon.Warning, "The recovered message contains unprintable characters. This typically means that you specified too many or too few secret pieces. If you believe you have too many secret pieces, try removing one and see if that helps. Otherwise, try adding another secret piece. (Showing decoded message as a binary value)", "Unprintable characters detected");
                }
                
                txtSecretMessage.SelectAll();
            }
        }

        private void SetRecoverTextToExample() {
            txtShares.Text = @"Keep in mind that you should only type in the minimum number of secret pieces needed to recover the secret.

For example, only 2 of the below secret pieces are needed to recover the sample secret message:

[SAMPLE SECRET]
Delete one of the above lines and press ""Recover Secret""";
            txtShares.Text = txtShares.Text.Replace("[SAMPLE SECRET]", CreateSecretMessageShares());
        }

        private void btnBrowsePlaintext_Click(object sender, EventArgs e) {
            openFileDialog.Filter = AllFilesFilter;
            openFileDialog.Title = "Select file containing secret information";
            openFileDialog.CheckFileExists = true;
            if(openFileDialog.ShowDialog() == DialogResult.OK) {
                txtSecretFilePath.Text = openFileDialog.FileName;
            }
        }

        private void cboKeySizes_TextChanged(object sender, EventArgs e) {
            GenerateRandomFileKey();
        }

        private void chkHideMasterKey_CheckedChanged(object sender, EventArgs e) {
            txtMasterKey.PasswordChar = chkHideMasterKey.Checked ? '*' : '\0';
        }

        private void chkShowAdvancedFileOptions_CheckedChanged(object sender, EventArgs e) {
            UpdateSecretPanels();
        }
    }
}
