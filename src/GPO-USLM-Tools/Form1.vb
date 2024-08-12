Public Class Form1
    Inherits MetroFramework.Forms.MetroForm

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        chkValidationOutputGen.Checked = True
        cmbViewer.SelectedIndex = 0
    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.lblProgramVersion.Text = String.Concat(gProgramVersionNo, " - ", gProgramVersionDate)

        If Not String.IsNullOrEmpty(My.Settings.LAST_DIR) Then
            If IO.Directory.Exists(My.Settings.LAST_DIR) Then
                Me.txtInputPath.Text = My.Settings.LAST_DIR
            End If
        End If

        LoadSchemaVersion()
    End Sub


    Private Sub btnProcess_Click(sender As Object, e As EventArgs) Handles btnProcess.Click

        If String.IsNullOrEmpty(Me.txtInputPath.Text) Then
            MessageBox.Show("please specify input path", gProgramName)
            Exit Sub
        End If

        Me.btnProcess.Enabled = False

        My.Settings.LAST_DIR = Me.txtInputPath.Text
        My.Settings.Save()



        If chkValidationOutputGen.Checked = True Then

            ProcessValidationMain(Me.txtInputPath.Text, False)
            ProcessOutputGenMain(Me.txtInputPath.Text, noErrorFiles, True)

        ElseIf chkValidation.Checked = True Then

            ProcessValidationMain(Me.txtInputPath.Text, True)

        ElseIf chkOutputGen.Checked = True Then

            ProcessOutputGenMain(Me.txtInputPath.Text, noErrorFiles, False)

        ElseIf chkMerge.Checked = True Then

            ProcessMergingMain(Me.txtInputPath.Text)

        ElseIf chkParse.Checked = True Then

            ProcessMainParseXML(Me.txtInputPath.Text, cmbSchemaVer.Text)

        ElseIf chkUtilPresDoc.Checked = True Then
            ProcessUtilityPresDocs(Me.txtInputPath.Text)

        ElseIf chkUtilRemoveHeader.Checked = True Then
            ProcessUtilityRemoveHeader(Me.txtInputPath.Text)

        ElseIf chkViewer.Checked = True Then
            ViewXML(Me.txtInputPath.Text, Me.cmbViewer.Text)

        ElseIf chkUtilQuotedtext.Checked = True Then
            ProcessUtilityQuotedText(Me.txtInputPath.Text)

        End If


        Me.lblProcessText.Text = "..." : Me.lblProcessText.Refresh()
        Me.lblProcessSubText.Text = "..." : Me.lblProcessSubText.Refresh()
        Me.btnProcess.Enabled = True

        MessageBox.Show(String.Format("processing done. see report and output files @ {0}", OutputPath.FullName), gProgramName)
        Process.Start(OutputPath.FullName)

    End Sub

    Private Sub chkValidationOutputGen_CheckedChanged(sender As Object, e As EventArgs) Handles chkValidationOutputGen.CheckedChanged

        If chkValidationOutputGen.Checked = True Then
            CheckUncheckedCheckbox(chkValidationOutputGen)
        End If

    End Sub

    Private Sub chkValidation_CheckedChanged(sender As Object, e As EventArgs) Handles chkValidation.CheckedChanged
        If chkValidation.Checked = True Then
            CheckUncheckedCheckbox(chkValidation)
        End If
    End Sub

    Private Sub chkOutputGen_CheckedChanged(sender As Object, e As EventArgs) Handles chkOutputGen.CheckedChanged
        If chkOutputGen.Checked = True Then
            CheckUncheckedCheckbox(chkOutputGen)
        End If
    End Sub

    Private Sub chkMerge_CheckedChanged(sender As Object, e As EventArgs) Handles chkMerge.CheckedChanged
        If chkMerge.Checked = True Then
            CheckUncheckedCheckbox(chkMerge)
        End If
    End Sub

    Private Sub chkParse_CheckedChanged(sender As Object, e As EventArgs) Handles chkParse.CheckedChanged
        If chkParse.Checked = True Then
            CheckUncheckedCheckbox(chkParse)
        End If
    End Sub

    Private Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
        Using FD As New FolderSelectDialog
            FD.Title = "Select Folder"

            If FD.ShowDialog = DialogResult.Cancel Then

            Else
                txtInputPath.Text = FD.FolderName
            End If

        End Using
    End Sub

    Private Sub chkUtilPresDoc_CheckedChanged(sender As Object, e As EventArgs) Handles chkUtilPresDoc.CheckedChanged
        If chkUtilPresDoc.Checked = True Then
            CheckUncheckedCheckbox(chkUtilPresDoc)
        End If

    End Sub

    Private Sub chkUtilRemoveHeader_CheckedChanged(sender As Object, e As EventArgs) Handles chkUtilRemoveHeader.CheckedChanged
        If chkUtilRemoveHeader.Checked = True Then
            CheckUncheckedCheckbox(chkUtilRemoveHeader)
        End If
    End Sub

    Private Sub chkViewer_CheckedChanged(sender As Object, e As EventArgs) Handles chkViewer.CheckedChanged
        If chkViewer.Checked = True Then

            CheckUncheckedCheckbox(chkViewer)
        End If
    End Sub


    Private Sub chkUtilQuotedtext_CheckedChanged(sender As Object, e As EventArgs) Handles chkUtilQuotedtext.CheckedChanged
        If chkUtilQuotedtext.Checked = True Then
            CheckUncheckedCheckbox(chkUtilQuotedtext)
        End If
    End Sub

    Sub CheckUncheckedCheckbox(ByVal chk As CheckBox)

        For Each cc As Control In Me.MetroPanel1.Controls
            If TypeOf cc Is CheckBox Then
                If DirectCast(cc, CheckBox).Name <> chk.Name Then
                    DirectCast(cc, CheckBox).Checked = False
                End If
            End If
        Next


    End Sub

    Sub LoadSchemaVersion()


        If IO.File.Exists(String.Concat(xsdFolder, "\", "SCHEMA.lst")) Then
            cmbSchemaVer.Items.Clear()

            Dim arr() As String = IO.File.ReadAllLines(String.Concat(xsdFolder, "\", "SCHEMA.lst"))
            cmbSchemaVer.Items.AddRange(arr)

        End If

        cmbSchemaVer.SelectedIndex = 0

    End Sub



End Class
