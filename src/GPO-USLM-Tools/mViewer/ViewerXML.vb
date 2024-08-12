Imports System.IO
Module ViewerXML

    Dim viewerFolder As String = Path.Combine(Application.StartupPath, "Viewer")

    Sub ViewXML(ByVal SourcePath As String, ByVal cssVersion As String)

        Dim viewerTemp As String = String.Empty
        viewerTemp = My.Resources.Viewer_Template

        OutputPath = New DirectoryInfo(Path.Combine(viewerFolder, cssVersion))


        For Each XMLFile As String In Directory.GetFiles(SourcePath, "*.xml")
            Dim xmlName As String = Path.GetFileName(XMLFile)

            Form1.lblProcessText.Text = String.Format("processing ... {0}", xmlName)

            Dim content As String = IO.File.ReadAllText(XMLFile)

            Using sw As New StreamWriter(OutputPath.FullName & "\" & xmlName)
                sw.WriteLine(viewerTemp.Replace("[gpo-body-xml]", content))
            End Using

            Process.Start(OutputPath.FullName & "\" & xmlName)
        Next
    End Sub


End Module
