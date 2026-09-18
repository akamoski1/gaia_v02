namespace gaia_v02
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        /// <summary>
        /// Draws a 1px black border around the sender control's client area, regardless of the
        /// current Windows theme's default border color.
        /// </summary>
        private static void DrawBlackBorder(object? sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (sender is not System.Windows.Forms.Control control) return;
            var rect = new System.Drawing.Rectangle(0, 0, control.ClientSize.Width - 1, control.ClientSize.Height - 1);
            using var pen = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
            e.Graphics.DrawRectangle(pen, rect);
        }

        #region Windows Form Designer generated code

        private void InitializeComponentEx()
        {
            components = new System.ComponentModel.Container();

            grpParams = new System.Windows.Forms.GroupBox();
            btnRunExperiment01 = new System.Windows.Forms.Button();
            btnResetDefaults = new System.Windows.Forms.Button();
            btnReadme = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            lblSummaryTitle = new System.Windows.Forms.Label();
            pnlSummaryBorder = new System.Windows.Forms.Panel();
            txtSummary = new System.Windows.Forms.TextBox();
            lblCsvTitle = new System.Windows.Forms.Label();
            pnlCsvBorder = new System.Windows.Forms.Panel();
            txtCsv = new System.Windows.Forms.TextBox();


            lblStarCount = new System.Windows.Forms.Label();
            lblR0 = new System.Windows.Forms.Label();
            lblSigmaR = new System.Windows.Forms.Label();
            lblSigmaZ = new System.Windows.Forms.Label();
            lblSigmaPhi = new System.Windows.Forms.Label();
            lblVLSR = new System.Windows.Forms.Label();
            lblScaleHeightR = new System.Windows.Forms.Label();
            lblScaleHeightZ = new System.Windows.Forms.Label();
            lblExpectedRatio = new System.Windows.Forms.Label();
            lblRatioTolerance = new System.Windows.Forms.Label();
            lblCellDeltaR = new System.Windows.Forms.Label();
            lblCellDeltaZ = new System.Windows.Forms.Label();
            lblMinStars = new System.Windows.Forms.Label();
            lblTimeoutMinutes = new System.Windows.Forms.Label();

            nudStarCount = new System.Windows.Forms.NumericUpDown();
            nudR0 = new System.Windows.Forms.NumericUpDown();
            nudSigmaR = new System.Windows.Forms.NumericUpDown();
            nudSigmaZ = new System.Windows.Forms.NumericUpDown();
            nudSigmaPhi = new System.Windows.Forms.NumericUpDown();
            nudVLSR = new System.Windows.Forms.NumericUpDown();
            nudScaleHeightR = new System.Windows.Forms.NumericUpDown();
            nudScaleHeightZ = new System.Windows.Forms.NumericUpDown();
            nudExpectedRatio = new System.Windows.Forms.NumericUpDown();
            nudRatioTolerance = new System.Windows.Forms.NumericUpDown();
            nudCellDeltaR = new System.Windows.Forms.NumericUpDown();
            nudCellDeltaZ = new System.Windows.Forms.NumericUpDown();
            nudMinStars = new System.Windows.Forms.NumericUpDown();
            nudTimeoutMinutes = new System.Windows.Forms.NumericUpDown();

            SuspendLayout();

            // grpParams
            grpParams.Text = "Experiment Parameters";
            grpParams.Location = new System.Drawing.Point(12, 12);
            grpParams.Size = new System.Drawing.Size(1000, 270);

            // Layout constants (adjusted for 1024x768)
            const int labelWidth = 240;
            const int inputWidth = 120;
            int leftLabelX = 12, leftInputX = 260;
            int rightLabelX = 520, rightInputX = 780;

            int rowY0 = 28, rowGap = 30;

            // Row 0
            lblStarCount.Text = "Star Count:";
            lblStarCount.Location = new System.Drawing.Point(leftLabelX, rowY0);
            lblStarCount.Size = new System.Drawing.Size(labelWidth, 22);
            nudStarCount.Location = new System.Drawing.Point(leftInputX, rowY0);
            nudStarCount.Size = new System.Drawing.Size(inputWidth, 22);
            nudStarCount.Minimum = 100;
            nudStarCount.Maximum = 10000000;
            nudStarCount.Increment = 1000;
            nudStarCount.DecimalPlaces = 0;
            nudStarCount.Value = 50000;

            lblExpectedRatio.Text = "Expected σR/σZ Ratio:";
            lblExpectedRatio.Location = new System.Drawing.Point(rightLabelX, rowY0);
            lblExpectedRatio.Size = new System.Drawing.Size(labelWidth, 22);
            nudExpectedRatio.Location = new System.Drawing.Point(rightInputX, rowY0);
            nudExpectedRatio.Size = new System.Drawing.Size(inputWidth, 22);
            nudExpectedRatio.Minimum = 0.1m;
            nudExpectedRatio.Maximum = 10m;
            nudExpectedRatio.DecimalPlaces = 3;
            nudExpectedRatio.Value = 1.930m;

            // Row 1
            int row1 = rowY0 + rowGap;
            lblR0.Text = "R₀ – Solar Radius [kpc]:";
            lblR0.Location = new System.Drawing.Point(leftLabelX, row1);
            lblR0.Size = new System.Drawing.Size(labelWidth, 22);
            nudR0.Location = new System.Drawing.Point(leftInputX, row1);
            nudR0.Size = new System.Drawing.Size(inputWidth, 22);
            nudR0.Minimum = 0.1m; nudR0.Maximum = 50m; nudR0.DecimalPlaces = 2; nudR0.Value = 8.00m;

            lblRatioTolerance.Text = "Ratio Tolerance (±):";
            lblRatioTolerance.Location = new System.Drawing.Point(rightLabelX, row1);
            lblRatioTolerance.Size = new System.Drawing.Size(labelWidth, 22);
            nudRatioTolerance.Location = new System.Drawing.Point(rightInputX, row1);
            nudRatioTolerance.Size = new System.Drawing.Size(inputWidth, 22);
            nudRatioTolerance.Minimum = 0.01m; nudRatioTolerance.Maximum = 2m; nudRatioTolerance.DecimalPlaces = 3; nudRatioTolerance.Value = 0.150m;

            // Row 2
            int row2 = row1 + rowGap;
            lblSigmaR.Text = "σR – Radial Disp. [km/s]:";
            lblSigmaR.Location = new System.Drawing.Point(leftLabelX, row2);
            lblSigmaR.Size = new System.Drawing.Size(labelWidth, 22);
            nudSigmaR.Location = new System.Drawing.Point(leftInputX, row2);
            nudSigmaR.Size = new System.Drawing.Size(inputWidth, 22);
            nudSigmaR.Minimum = 0.1m; nudSigmaR.Maximum = 500m; nudSigmaR.DecimalPlaces = 1; nudSigmaR.Value = 38.0m;

            lblCellDeltaR.Text = "Cell ΔR [kpc]:";
            lblCellDeltaR.Location = new System.Drawing.Point(rightLabelX, row2);
            lblCellDeltaR.Size = new System.Drawing.Size(labelWidth, 22);
            nudCellDeltaR.Location = new System.Drawing.Point(rightInputX, row2);
            nudCellDeltaR.Size = new System.Drawing.Size(inputWidth, 22);
            nudCellDeltaR.Minimum = 0.01m; nudCellDeltaR.Maximum = 5m; nudCellDeltaR.DecimalPlaces = 3; nudCellDeltaR.Value = 0.500m;

            // Row 3
            int row3 = row2 + rowGap;
            lblSigmaZ.Text = "σZ – Vertical Disp. [km/s]:";
            lblSigmaZ.Location = new System.Drawing.Point(leftLabelX, row3);
            lblSigmaZ.Size = new System.Drawing.Size(labelWidth, 22);
            nudSigmaZ.Location = new System.Drawing.Point(leftInputX, row3);
            nudSigmaZ.Size = new System.Drawing.Size(inputWidth, 22);
            nudSigmaZ.Minimum = 0.1m; nudSigmaZ.Maximum = 500m; nudSigmaZ.DecimalPlaces = 1; nudSigmaZ.Value = 20.0m;

            lblCellDeltaZ.Text = "Cell ΔZ [kpc]:";
            lblCellDeltaZ.Location = new System.Drawing.Point(rightLabelX, row3);
            lblCellDeltaZ.Size = new System.Drawing.Size(labelWidth, 22);
            nudCellDeltaZ.Location = new System.Drawing.Point(rightInputX, row3);
            nudCellDeltaZ.Size = new System.Drawing.Size(inputWidth, 22);
            nudCellDeltaZ.Minimum = 0.01m; nudCellDeltaZ.Maximum = 2m; nudCellDeltaZ.DecimalPlaces = 3; nudCellDeltaZ.Value = 0.150m;

            // Row 4
            int row4 = row3 + rowGap;
            lblSigmaPhi.Text = "σΦ – Azimuth. Disp. [km/s]:";
            lblSigmaPhi.Location = new System.Drawing.Point(leftLabelX, row4);
            lblSigmaPhi.Size = new System.Drawing.Size(labelWidth, 22);
            nudSigmaPhi.Location = new System.Drawing.Point(leftInputX, row4);
            nudSigmaPhi.Size = new System.Drawing.Size(inputWidth, 22);
            nudSigmaPhi.Minimum = 0.1m; nudSigmaPhi.Maximum = 500m; nudSigmaPhi.DecimalPlaces = 1; nudSigmaPhi.Value = 28.0m;

            lblMinStars.Text = "Min Stars / Cell:";
            lblMinStars.Location = new System.Drawing.Point(rightLabelX, row4);
            lblMinStars.Size = new System.Drawing.Size(labelWidth, 22);
            nudMinStars.Location = new System.Drawing.Point(rightInputX, row4);
            nudMinStars.Size = new System.Drawing.Size(inputWidth, 22);
            nudMinStars.Minimum = 1; nudMinStars.Maximum = 1000; nudMinStars.DecimalPlaces = 0; nudMinStars.Value = 20;

            // Row 5
            int row5 = row4 + rowGap;
            lblVLSR.Text = "V_LSR [km/s]:";
            lblVLSR.Location = new System.Drawing.Point(leftLabelX, row5);
            lblVLSR.Size = new System.Drawing.Size(labelWidth, 22);
            nudVLSR.Location = new System.Drawing.Point(leftInputX, row5);
            nudVLSR.Size = new System.Drawing.Size(inputWidth, 22);
            nudVLSR.Minimum = 0m; nudVLSR.Maximum = 500m; nudVLSR.DecimalPlaces = 1; nudVLSR.Value = 220.0m;

            lblTimeoutMinutes.Text = "Timeout [minutes]:";
            lblTimeoutMinutes.Location = new System.Drawing.Point(rightLabelX, row5);
            lblTimeoutMinutes.Size = new System.Drawing.Size(labelWidth, 22);
            nudTimeoutMinutes.Location = new System.Drawing.Point(rightInputX, row5);
            nudTimeoutMinutes.Size = new System.Drawing.Size(inputWidth, 22);
            nudTimeoutMinutes.Minimum = 1; nudTimeoutMinutes.Maximum = 60; nudTimeoutMinutes.DecimalPlaces = 0; nudTimeoutMinutes.Value = 5;

            // Row 6
            int row6 = row5 + rowGap;
            lblScaleHeightR.Text = "Scale Height R [kpc]:";
            lblScaleHeightR.Location = new System.Drawing.Point(leftLabelX, row6);
            lblScaleHeightR.Size = new System.Drawing.Size(labelWidth, 22);
            nudScaleHeightR.Location = new System.Drawing.Point(leftInputX, row6);
            nudScaleHeightR.Size = new System.Drawing.Size(inputWidth, 22);
            nudScaleHeightR.Minimum = 0.01m; nudScaleHeightR.Maximum = 20m; nudScaleHeightR.DecimalPlaces = 2; nudScaleHeightR.Value = 2.50m;

            lblScaleHeightZ.Text = "Scale Height Z [kpc]:";
            lblScaleHeightZ.Location = new System.Drawing.Point(rightLabelX, row6);
            lblScaleHeightZ.Size = new System.Drawing.Size(labelWidth, 22);
            nudScaleHeightZ.Location = new System.Drawing.Point(rightInputX, row6);
            nudScaleHeightZ.Size = new System.Drawing.Size(inputWidth, 22);
            nudScaleHeightZ.Minimum = 0.01m; nudScaleHeightZ.Maximum = 5m; nudScaleHeightZ.DecimalPlaces = 3; nudScaleHeightZ.Value = 0.300m;

            // Add to group box
            grpParams.Controls.Add(lblStarCount);
            grpParams.Controls.Add(nudStarCount);
            grpParams.Controls.Add(lblExpectedRatio);
            grpParams.Controls.Add(nudExpectedRatio);
            grpParams.Controls.Add(lblR0);
            grpParams.Controls.Add(nudR0);
            grpParams.Controls.Add(lblRatioTolerance);
            grpParams.Controls.Add(nudRatioTolerance);
            grpParams.Controls.Add(lblSigmaR);
            grpParams.Controls.Add(nudSigmaR);
            grpParams.Controls.Add(lblCellDeltaR);
            grpParams.Controls.Add(nudCellDeltaR);
            grpParams.Controls.Add(lblSigmaZ);
            grpParams.Controls.Add(nudSigmaZ);
            grpParams.Controls.Add(lblCellDeltaZ);
            grpParams.Controls.Add(nudCellDeltaZ);
            grpParams.Controls.Add(lblSigmaPhi);
            grpParams.Controls.Add(nudSigmaPhi);
            grpParams.Controls.Add(lblMinStars);
            grpParams.Controls.Add(nudMinStars);
            grpParams.Controls.Add(lblVLSR);
            grpParams.Controls.Add(nudVLSR);
            grpParams.Controls.Add(lblTimeoutMinutes);
            grpParams.Controls.Add(nudTimeoutMinutes);
            grpParams.Controls.Add(lblScaleHeightR);
            grpParams.Controls.Add(nudScaleHeightR);
            grpParams.Controls.Add(lblScaleHeightZ);

            // Ensure group box anchors to top,left,right so it stretches horizontally
            grpParams.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpParams.Controls.Add(nudScaleHeightZ);

            // Action buttons
            btnRunExperiment01.Location = new System.Drawing.Point(12, grpParams.Bottom + 12);
            btnRunExperiment01.Size = new System.Drawing.Size(200, 34);
            btnRunExperiment01.Text = "▶  Run Experiment 01";
            btnRunExperiment01.Click += BtnRunExperiment01_Click;

            btnResetDefaults.Location = new System.Drawing.Point(222, grpParams.Bottom + 12);
            btnResetDefaults.Size = new System.Drawing.Size(160, 34);
            btnResetDefaults.Text = "↺  Reset Defaults";
            btnResetDefaults.Click += BtnResetDefaults_Click;

            btnReadme.Location = new System.Drawing.Point(392, grpParams.Bottom + 12);
            btnReadme.Size = new System.Drawing.Size(110, 34);
            btnReadme.Text = "README";
            btnReadme.Click += BtnReadme_Click;

            lblStatus.Location = new System.Drawing.Point(512, grpParams.Bottom + 18);
            lblStatus.Size = new System.Drawing.Size(372, 22);
            lblStatus.Text = "Ready. Set parameters above, then click Run.";
            lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            // Open output buttons
            btnOpenNotepad = new System.Windows.Forms.Button();
            btnOpenSpreadsheet = new System.Windows.Forms.Button();
            btnOpenNotepad.Location = new System.Drawing.Point(900, grpParams.Bottom + 12);
            btnOpenNotepad.Size = new System.Drawing.Size(110, 34);
            btnOpenNotepad.Text = "Notepad";
            btnOpenNotepad.Click += BtnOpenNotepad_Click;
            btnOpenNotepad.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            btnOpenSpreadsheet.Location = new System.Drawing.Point(768, grpParams.Bottom + 12);
            btnOpenSpreadsheet.Size = new System.Drawing.Size(120, 34);
            btnOpenSpreadsheet.Text = "Sheet";
            btnOpenSpreadsheet.Click += BtnOpenSpreadsheet_Click;
            btnOpenSpreadsheet.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // Summary box
            lblSummaryTitle.Location = new System.Drawing.Point(12, btnRunExperiment01.Bottom + 12);
            lblSummaryTitle.Size = new System.Drawing.Size(500, 20);
            lblSummaryTitle.Text = "Experiment Summary & Insights:";
            lblSummaryTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // pnlSummaryBorder hosts txtSummary and paints a 1px black border around it
            // (native TextBox controls don't reliably raise Paint, so a bordered host panel is used instead).
            pnlSummaryBorder.Location = new System.Drawing.Point(12, lblSummaryTitle.Bottom + 6);
            pnlSummaryBorder.Size = new System.Drawing.Size(1000, 180);
            pnlSummaryBorder.BackColor = System.Drawing.Color.Black;
            pnlSummaryBorder.Padding = new System.Windows.Forms.Padding(1);
            pnlSummaryBorder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            txtSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            txtSummary.Multiline = true;
            txtSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtSummary.ReadOnly = true;
            txtSummary.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pnlSummaryBorder.Controls.Add(txtSummary);


            // CSV box
            lblCsvTitle.Location = new System.Drawing.Point(12, pnlSummaryBorder.Bottom + 12);
            lblCsvTitle.Size = new System.Drawing.Size(500, 18);
            lblCsvTitle.Text = "CSV Output (also saved to file):";

            // pnlCsvBorder hosts txtCsv and paints a 1px black border around it.
            // Height reduced ~10% further (290 -> 261) so the label above is no longer clipped.
            pnlCsvBorder.Location = new System.Drawing.Point(12, lblCsvTitle.Bottom + 6);
            pnlCsvBorder.Size = new System.Drawing.Size(1126, 261);
            pnlCsvBorder.BackColor = System.Drawing.Color.Black;
            pnlCsvBorder.Padding = new System.Windows.Forms.Padding(1);
            pnlCsvBorder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            txtCsv.Dock = System.Windows.Forms.DockStyle.Fill;
            txtCsv.Multiline = true;
            txtCsv.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtCsv.ReadOnly = true;
            txtCsv.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pnlCsvBorder.Controls.Add(txtCsv);

            // Form
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1150, 912);
            MinimumSize = new System.Drawing.Size(900, 700);
            Text = "Gaia SR – Experiment Runner";

            Controls.Add(grpParams);
            Controls.Add(btnRunExperiment01);
            Controls.Add(btnResetDefaults);
            Controls.Add(btnReadme);
            Controls.Add(btnOpenNotepad);
            Controls.Add(btnOpenSpreadsheet);
            Controls.Add(lblStatus);
            Controls.Add(lblSummaryTitle);
            Controls.Add(pnlSummaryBorder);
            Controls.Add(lblCsvTitle);
            Controls.Add(pnlCsvBorder);

            // 1px black border to make the parameter group box stand out visually.
            // (txtSummary/txtCsv get their black borders from their host Panel's black background + padding.)
            grpParams.Paint += DrawBlackBorder;

            ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpParams;
        private System.Windows.Forms.Label lblStarCount, lblR0, lblSigmaR, lblSigmaZ, lblSigmaPhi;
        private System.Windows.Forms.Label lblVLSR, lblScaleHeightR, lblScaleHeightZ;
        private System.Windows.Forms.Label lblExpectedRatio, lblRatioTolerance;
        private System.Windows.Forms.Label lblCellDeltaR, lblCellDeltaZ, lblMinStars, lblTimeoutMinutes;
        private System.Windows.Forms.NumericUpDown nudStarCount, nudR0, nudSigmaR, nudSigmaZ, nudSigmaPhi;
        private System.Windows.Forms.NumericUpDown nudVLSR, nudScaleHeightR, nudScaleHeightZ;
        private System.Windows.Forms.NumericUpDown nudExpectedRatio, nudRatioTolerance;
        private System.Windows.Forms.NumericUpDown nudCellDeltaR, nudCellDeltaZ, nudMinStars, nudTimeoutMinutes;
        private System.Windows.Forms.Button btnRunExperiment01;
        private System.Windows.Forms.Button btnResetDefaults;
        private System.Windows.Forms.Button btnReadme;
        private System.Windows.Forms.Button btnOpenNotepad;
        private System.Windows.Forms.Button btnOpenSpreadsheet;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblSummaryTitle;
        private System.Windows.Forms.Panel pnlSummaryBorder;
        private System.Windows.Forms.TextBox txtSummary;
        private System.Windows.Forms.Label lblCsvTitle;
        private System.Windows.Forms.Panel pnlCsvBorder;
        private System.Windows.Forms.TextBox txtCsv;

        #endregion
    }
}
