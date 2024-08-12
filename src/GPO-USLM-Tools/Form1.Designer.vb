<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits MetroFramework.Forms.MetroForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.chkValidationOutputGen = New MetroFramework.Controls.MetroCheckBox()
        Me.chkParse = New MetroFramework.Controls.MetroCheckBox()
        Me.chkMerge = New MetroFramework.Controls.MetroCheckBox()
        Me.MetroPanel1 = New MetroFramework.Controls.MetroPanel()
        Me.MetroComboBox1 = New MetroFramework.Controls.MetroComboBox()
        Me.chkUtilQuotedtext = New MetroFramework.Controls.MetroCheckBox()
        Me.cmbViewer = New MetroFramework.Controls.MetroComboBox()
        Me.chkViewer = New MetroFramework.Controls.MetroCheckBox()
        Me.chkUtilRemoveHeader = New MetroFramework.Controls.MetroCheckBox()
        Me.chkUtilPresDoc = New MetroFramework.Controls.MetroCheckBox()
        Me.cmbSchemaVer = New MetroFramework.Controls.MetroComboBox()
        Me.chkValidation = New MetroFramework.Controls.MetroCheckBox()
        Me.chkOutputGen = New MetroFramework.Controls.MetroCheckBox()
        Me.MetroPanel2 = New MetroFramework.Controls.MetroPanel()
        Me.btnBrowse = New MetroFramework.Controls.MetroButton()
        Me.lblProcessSubText = New MetroFramework.Controls.MetroLabel()
        Me.lblProcessText = New MetroFramework.Controls.MetroLabel()
        Me.btnProcess = New MetroFramework.Controls.MetroButton()
        Me.txtInputPath = New MetroFramework.Controls.MetroTextBox()
        Me.lblProgramVersion = New MetroFramework.Controls.MetroLabel()
        Me.MetroPanel1.SuspendLayout()
        Me.MetroPanel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'chkValidationOutputGen
        '
        Me.chkValidationOutputGen.AutoSize = True
        Me.chkValidationOutputGen.FontSize = MetroFramework.MetroCheckBoxSize.Medium
        Me.chkValidationOutputGen.FontWeight = MetroFramework.MetroCheckBoxWeight.Bold
        Me.chkValidationOutputGen.Location = New System.Drawing.Point(15, 31)
        Me.chkValidationOutputGen.Name = "chkValidationOutputGen"
        Me.chkValidationOutputGen.Size = New System.Drawing.Size(232, 19)
        Me.chkValidationOutputGen.TabIndex = 0
        Me.chkValidationOutputGen.Text = "Validation and Output Creation"
        Me.chkValidationOutputGen.Theme = MetroFramework.MetroThemeStyle.Light
        Me.chkValidationOutputGen.UseSelectable = True
        '
        'chkParse
        '
        Me.chkParse.AutoSize = True
        Me.chkParse.FontSize = MetroFramework.MetroCheckBoxSize.Medium
        Me.chkParse.FontWeight = MetroFramework.MetroCheckBoxWeight.Bold
        Me.chkParse.Location = New System.Drawing.Point(15, 177)
        Me.chkParse.Name = "chkParse"
        Me.chkParse.Size = New System.Drawing.Size(101, 19)
        Me.chkParse.TabIndex = 1
        Me.chkParse.Text = "Parse XMLs"
        Me.chkParse.Theme = MetroFramework.MetroThemeStyle.Light
        Me.chkParse.UseSelectable = True
        '
        'chkMerge
        '
        Me.chkMerge.AutoSize = True
        Me.chkMerge.FontSize = MetroFramework.MetroCheckBoxSize.Medium
        Me.chkMerge.FontWeight = MetroFramework.MetroCheckBoxWeight.Bold
        Me.chkMerge.Location = New System.Drawing.Point(15, 139)
        Me.chkMerge.Name = "chkMerge"
        Me.chkMerge.Size = New System.Drawing.Size(108, 19)
        Me.chkMerge.TabIndex = 2
        Me.chkMerge.Text = "Merge XMLs"
        Me.chkMerge.Theme = MetroFramework.MetroThemeStyle.Light
        Me.chkMerge.UseSelectable = True
        '
        'MetroPanel1
        '
        Me.MetroPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.MetroPanel1.Controls.Add(Me.MetroComboBox1)
        Me.MetroPanel1.Controls.Add(Me.chkUtilQuotedtext)
        Me.MetroPanel1.Controls.Add(Me.cmbViewer)
        Me.MetroPanel1.Controls.Add(Me.chkViewer)
        Me.MetroPanel1.Controls.Add(Me.chkUtilRemoveHeader)
        Me.MetroPanel1.Controls.Add(Me.chkUtilPresDoc)
        Me.MetroPanel1.Controls.Add(Me.cmbSchemaVer)
        Me.MetroPanel1.Controls.Add(Me.chkValidation)
        Me.MetroPanel1.Controls.Add(Me.chkOutputGen)
        Me.MetroPanel1.Controls.Add(Me.chkMerge)
        Me.MetroPanel1.Controls.Add(Me.chkParse)
        Me.MetroPanel1.Controls.Add(Me.chkValidationOutputGen)
        Me.MetroPanel1.Dock = System.Windows.Forms.DockStyle.Left
        Me.MetroPanel1.HorizontalScrollbarBarColor = True
        Me.MetroPanel1.HorizontalScrollbarHighlightOnWheel = False
        Me.MetroPanel1.HorizontalScrollbarSize = 10
        Me.MetroPanel1.Location = New System.Drawing.Point(20, 60)
        Me.MetroPanel1.Name = "MetroPanel1"
        Me.MetroPanel1.Size = New System.Drawing.Size(279, 423)
        Me.MetroPanel1.TabIndex = 3
        Me.MetroPanel1.Theme = MetroFramework.MetroThemeStyle.Light
        Me.MetroPanel1.VerticalScrollbarBarColor = True
        Me.MetroPanel1.VerticalScrollbarHighlightOnWheel = False
        Me.MetroPanel1.VerticalScrollbarSize = 10
        '
        'MetroComboBox1
        '
        Me.MetroComboBox1.FontSize = MetroFramework.MetroComboBoxSize.Small
        Me.MetroComboBox1.FormattingEnabled = True
        Me.MetroComboBox1.ItemHeight = 19
        Me.MetroComboBox1.Items.AddRange(New Object() {"new", "old"})
        Me.MetroComboBox1.Location = New System.Drawing.Point(121, 63)
        Me.MetroComboBox1.Name = "MetroComboBox1"
        Me.MetroComboBox1.Size = New System.Drawing.Size(141, 25)
        Me.MetroComboBox1.TabIndex = 11
        Me.MetroComboBox1.Theme = MetroFramework.MetroThemeStyle.Light
        Me.MetroComboBox1.UseSelectable = True
        Me.MetroComboBox1.Visible = False
        '
        'chkUtilQuotedtext
        '
        Me.chkUtilQuotedtext.AutoSize = True
        Me.chkUtilQuotedtext.FontSize = MetroFramework.MetroCheckBoxSize.Medium
        Me.chkUtilQuotedtext.FontWeight = MetroFramework.MetroCheckBoxWeight.Bold
        Me.chkUtilQuotedtext.Location = New System.Drawing.Point(15, 334)
        Me.chkUtilQuotedtext.Name = "chkUtilQuotedtext"
        Me.chkUtilQuotedtext.Size = New System.Drawing.Size(157, 19)
        Me.chkUtilQuotedtext.TabIndex = 10
        Me.chkUtilQuotedtext.Text = "Utility - QuotedText"
        Me.chkUtilQuotedtext.Theme = MetroFramework.MetroThemeStyle.Light
        Me.chkUtilQuotedtext.UseSelectable = True
        '
        'cmbViewer
        '
        Me.cmbViewer.FontSize = MetroFramework.MetroComboBoxSize.Small
        Me.cmbViewer.FormattingEnabled = True
        Me.cmbViewer.ItemHeight = 19
        Me.cmbViewer.Items.AddRange(New Object() {"new", "old"})
        Me.cmbViewer.Location = New System.Drawing.Point(121, 299)
        Me.cmbViewer.Name = "cmbViewer"
        Me.cmbViewer.Size = New System.Drawing.Size(141, 25)
        Me.cmbViewer.TabIndex = 9
        Me.cmbViewer.Theme = MetroFramework.MetroThemeStyle.Light
        Me.cmbViewer.UseSelectable = True
        '
        'chkViewer
        '
        Me.chkViewer.AutoSize = True
        Me.chkViewer.FontSize = MetroFramework.MetroCheckBoxSize.Medium
        Me.chkViewer.FontWeight = MetroFramework.MetroCheckBoxWeight.Bold
        Me.chkViewer.Location = New System.Drawing.Point(15, 299)
        Me.chkViewer.Name = "chkViewer"
        Me.chkViewer.Size = New System.Drawing.Size(71, 19)
        Me.chkViewer.TabIndex = 8
        Me.chkViewer.Text = "Viewer"
        Me.chkViewer.Theme = MetroFramework.MetroThemeStyle.Light
        Me.chkViewer.UseSelectable = True
        '
        'chkUtilRemoveHeader
        '
        Me.chkUtilRemoveHeader.AutoSize = True
        Me.chkUtilRemoveHeader.FontSize = MetroFramework.MetroCheckBoxSize.Medium
        Me.chkUtilRemoveHeader.FontWeight = MetroFramework.MetroCheckBoxWeight.Bold
        Me.chkUtilRemoveHeader.Location = New System.Drawing.Point(15, 264)
        Me.chkUtilRemoveHeader.Name = "chkUtilRemoveHeader"
        Me.chkUtilRemoveHeader.Size = New System.Drawing.Size(185, 19)
        Me.chkUtilRemoveHeader.TabIndex = 7
        Me.chkUtilRemoveHeader.Text = "Utility - remove Header"
        Me.chkUtilRemoveHeader.Theme = MetroFramework.MetroThemeStyle.Light
        Me.chkUtilRemoveHeader.UseSelectable = True
        '
        'chkUtilPresDoc
        '
        Me.chkUtilPresDoc.AutoSize = True
        Me.chkUtilPresDoc.FontSize = MetroFramework.MetroCheckBoxSize.Medium
        Me.chkUtilPresDoc.FontWeight = MetroFramework.MetroCheckBoxWeight.Bold
        Me.chkUtilPresDoc.Location = New System.Drawing.Point(15, 223)
        Me.chkUtilPresDoc.Name = "chkUtilPresDoc"
        Me.chkUtilPresDoc.Size = New System.Drawing.Size(190, 19)
        Me.chkUtilPresDoc.TabIndex = 6
        Me.chkUtilPresDoc.Text = "Utility - presidentialDocs"
        Me.chkUtilPresDoc.Theme = MetroFramework.MetroThemeStyle.Light
        Me.chkUtilPresDoc.UseSelectable = True
        '
        'cmbSchemaVer
        '
        Me.cmbSchemaVer.FontSize = MetroFramework.MetroComboBoxSize.Small
        Me.cmbSchemaVer.FormattingEnabled = True
        Me.cmbSchemaVer.ItemHeight = 19
        Me.cmbSchemaVer.Items.AddRange(New Object() {"USLM 2.0.10", "USLM 2.0.13"})
        Me.cmbSchemaVer.Location = New System.Drawing.Point(121, 175)
        Me.cmbSchemaVer.Name = "cmbSchemaVer"
        Me.cmbSchemaVer.Size = New System.Drawing.Size(141, 25)
        Me.cmbSchemaVer.TabIndex = 5
        Me.cmbSchemaVer.Theme = MetroFramework.MetroThemeStyle.Light
        Me.cmbSchemaVer.UseSelectable = True
        '
        'chkValidation
        '
        Me.chkValidation.AutoSize = True
        Me.chkValidation.FontSize = MetroFramework.MetroCheckBoxSize.Medium
        Me.chkValidation.FontWeight = MetroFramework.MetroCheckBoxWeight.Bold
        Me.chkValidation.Location = New System.Drawing.Point(15, 66)
        Me.chkValidation.Name = "chkValidation"
        Me.chkValidation.Size = New System.Drawing.Size(92, 19)
        Me.chkValidation.TabIndex = 4
        Me.chkValidation.Text = "Validation"
        Me.chkValidation.Theme = MetroFramework.MetroThemeStyle.Light
        Me.chkValidation.UseSelectable = True
        '
        'chkOutputGen
        '
        Me.chkOutputGen.AutoSize = True
        Me.chkOutputGen.FontSize = MetroFramework.MetroCheckBoxSize.Medium
        Me.chkOutputGen.FontWeight = MetroFramework.MetroCheckBoxWeight.Bold
        Me.chkOutputGen.Location = New System.Drawing.Point(15, 102)
        Me.chkOutputGen.Name = "chkOutputGen"
        Me.chkOutputGen.Size = New System.Drawing.Size(167, 19)
        Me.chkOutputGen.TabIndex = 3
        Me.chkOutputGen.Text = "Output Creation Only"
        Me.chkOutputGen.Theme = MetroFramework.MetroThemeStyle.Light
        Me.chkOutputGen.UseSelectable = True
        '
        'MetroPanel2
        '
        Me.MetroPanel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.MetroPanel2.Controls.Add(Me.btnBrowse)
        Me.MetroPanel2.Controls.Add(Me.lblProcessSubText)
        Me.MetroPanel2.Controls.Add(Me.lblProcessText)
        Me.MetroPanel2.Controls.Add(Me.btnProcess)
        Me.MetroPanel2.Controls.Add(Me.txtInputPath)
        Me.MetroPanel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.MetroPanel2.HorizontalScrollbarBarColor = True
        Me.MetroPanel2.HorizontalScrollbarHighlightOnWheel = False
        Me.MetroPanel2.HorizontalScrollbarSize = 10
        Me.MetroPanel2.Location = New System.Drawing.Point(299, 60)
        Me.MetroPanel2.Name = "MetroPanel2"
        Me.MetroPanel2.Size = New System.Drawing.Size(715, 423)
        Me.MetroPanel2.TabIndex = 4
        Me.MetroPanel2.Theme = MetroFramework.MetroThemeStyle.Light
        Me.MetroPanel2.VerticalScrollbarBarColor = True
        Me.MetroPanel2.VerticalScrollbarHighlightOnWheel = False
        Me.MetroPanel2.VerticalScrollbarSize = 10
        '
        'btnBrowse
        '
        Me.btnBrowse.FontSize = MetroFramework.MetroButtonSize.Medium
        Me.btnBrowse.Location = New System.Drawing.Point(17, 59)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(143, 29)
        Me.btnBrowse.TabIndex = 7
        Me.btnBrowse.Text = "Browse Path"
        Me.btnBrowse.UseSelectable = True
        '
        'lblProcessSubText
        '
        Me.lblProcessSubText.Location = New System.Drawing.Point(19, 172)
        Me.lblProcessSubText.Name = "lblProcessSubText"
        Me.lblProcessSubText.Size = New System.Drawing.Size(478, 23)
        Me.lblProcessSubText.TabIndex = 6
        Me.lblProcessSubText.Text = ". . ."
        Me.lblProcessSubText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.lblProcessSubText.Theme = MetroFramework.MetroThemeStyle.Light
        '
        'lblProcessText
        '
        Me.lblProcessText.Location = New System.Drawing.Point(19, 140)
        Me.lblProcessText.Name = "lblProcessText"
        Me.lblProcessText.Size = New System.Drawing.Size(478, 23)
        Me.lblProcessText.TabIndex = 5
        Me.lblProcessText.Text = ". . ."
        Me.lblProcessText.Theme = MetroFramework.MetroThemeStyle.Light
        '
        'btnProcess
        '
        Me.btnProcess.FontSize = MetroFramework.MetroButtonSize.Medium
        Me.btnProcess.Location = New System.Drawing.Point(549, 140)
        Me.btnProcess.Name = "btnProcess"
        Me.btnProcess.Size = New System.Drawing.Size(142, 41)
        Me.btnProcess.TabIndex = 4
        Me.btnProcess.Text = "start"
        Me.btnProcess.UseSelectable = True
        '
        'txtInputPath
        '
        Me.txtInputPath.AllowDrop = True
        '
        '
        '
        Me.txtInputPath.CustomButton.Image = Nothing
        Me.txtInputPath.CustomButton.Location = New System.Drawing.Point(644, 2)
        Me.txtInputPath.CustomButton.Name = ""
        Me.txtInputPath.CustomButton.Size = New System.Drawing.Size(27, 27)
        Me.txtInputPath.CustomButton.Style = MetroFramework.MetroColorStyle.Blue
        Me.txtInputPath.CustomButton.TabIndex = 1
        Me.txtInputPath.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light
        Me.txtInputPath.CustomButton.UseSelectable = True
        Me.txtInputPath.CustomButton.Visible = False
        Me.txtInputPath.FontSize = MetroFramework.MetroTextBoxSize.Medium
        Me.txtInputPath.Lines = New String(-1) {}
        Me.txtInputPath.Location = New System.Drawing.Point(17, 97)
        Me.txtInputPath.MaxLength = 32767
        Me.txtInputPath.Name = "txtInputPath"
        Me.txtInputPath.PasswordChar = Global.Microsoft.VisualBasic.ChrW(0)
        Me.txtInputPath.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.txtInputPath.SelectedText = ""
        Me.txtInputPath.SelectionLength = 0
        Me.txtInputPath.SelectionStart = 0
        Me.txtInputPath.ShortcutsEnabled = True
        Me.txtInputPath.Size = New System.Drawing.Size(674, 32)
        Me.txtInputPath.TabIndex = 3
        Me.txtInputPath.Theme = MetroFramework.MetroThemeStyle.Light
        Me.txtInputPath.UseSelectable = True
        Me.txtInputPath.WaterMarkColor = System.Drawing.Color.FromArgb(CType(CType(109, Byte), Integer), CType(CType(109, Byte), Integer), CType(CType(109, Byte), Integer))
        Me.txtInputPath.WaterMarkFont = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel)
        '
        'lblProgramVersion
        '
        Me.lblProgramVersion.AutoSize = True
        Me.lblProgramVersion.Location = New System.Drawing.Point(866, 37)
        Me.lblProgramVersion.Name = "lblProgramVersion"
        Me.lblProgramVersion.Size = New System.Drawing.Size(145, 19)
        Me.lblProgramVersion.TabIndex = 5
        Me.lblProgramVersion.Text = "V01.00.00 - Mar.18.2023"
        Me.lblProgramVersion.Theme = MetroFramework.MetroThemeStyle.Light
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1034, 503)
        Me.Controls.Add(Me.lblProgramVersion)
        Me.Controls.Add(Me.MetroPanel2)
        Me.Controls.Add(Me.MetroPanel1)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Form1"
        Me.Style = MetroFramework.MetroColorStyle.Lime
        Me.Text = "GPO USLM TOOLS"
        Me.MetroPanel1.ResumeLayout(False)
        Me.MetroPanel1.PerformLayout()
        Me.MetroPanel2.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents chkValidationOutputGen As MetroFramework.Controls.MetroCheckBox
    Friend WithEvents chkParse As MetroFramework.Controls.MetroCheckBox
    Friend WithEvents chkMerge As MetroFramework.Controls.MetroCheckBox
    Friend WithEvents MetroPanel1 As MetroFramework.Controls.MetroPanel
    Friend WithEvents MetroPanel2 As MetroFramework.Controls.MetroPanel
    Friend WithEvents lblProcessText As MetroFramework.Controls.MetroLabel
    Friend WithEvents btnProcess As MetroFramework.Controls.MetroButton
    Friend WithEvents txtInputPath As MetroFramework.Controls.MetroTextBox
    Friend WithEvents lblProcessSubText As MetroFramework.Controls.MetroLabel
    Friend WithEvents chkValidation As MetroFramework.Controls.MetroCheckBox
    Friend WithEvents chkOutputGen As MetroFramework.Controls.MetroCheckBox
    Friend WithEvents lblProgramVersion As MetroFramework.Controls.MetroLabel
    Friend WithEvents cmbSchemaVer As MetroFramework.Controls.MetroComboBox
    Friend WithEvents btnBrowse As MetroFramework.Controls.MetroButton
    Friend WithEvents chkUtilPresDoc As MetroFramework.Controls.MetroCheckBox
    Friend WithEvents chkUtilRemoveHeader As MetroFramework.Controls.MetroCheckBox
    Friend WithEvents chkViewer As MetroFramework.Controls.MetroCheckBox
    Friend WithEvents cmbViewer As MetroFramework.Controls.MetroComboBox
    Friend WithEvents chkUtilQuotedtext As MetroFramework.Controls.MetroCheckBox
    Friend WithEvents MetroComboBox1 As MetroFramework.Controls.MetroComboBox
End Class
