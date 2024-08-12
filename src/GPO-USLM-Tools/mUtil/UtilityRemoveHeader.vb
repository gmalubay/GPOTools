Imports System.IO
Imports System.Text.RegularExpressions


Module UtilityRemoveHeader

    Sub ProcessUtilityRemoveHeader(ByVal InputPath As String)


        OutputPath = New DirectoryInfo(Path.Combine(InputPath, "OUTPUT"))

        If OutputPath.Exists = False Then
            OutputPath.Create()
        End If

        Form1.lblProcessText.Text = "removing Header..."

        For Each xFile In Directory.GetFiles(InputPath, "*.xml")
            RemoveFileHeader(xFile)
        Next

    End Sub

    Sub RemoveFileHeader(ByVal SourceFile As String)
        Dim blnStart As Boolean = False
        Dim xmlFname As String = Path.GetFileName(SourceFile)
        Dim parentElem As String = String.Empty


        Form1.lblProcessText.Text = String.Format("processing ... ", xmlFname)
        Form1.lblProcessText.Refresh()


        Dim strContent = File.ReadAllText(SourceFile, System.Text.Encoding.UTF8)

        Dim startMatch As Match = Regex.Match(strContent, "<(publicLaws|privateLaws|concurrentResolutions|presidentialDocs role=""proclamations"")>")
        If startMatch.Success = True Then
            parentElem = startMatch.Groups(1).Value
        Else
            parentElem = "component"
        End If


        Using sw As New StreamWriter(String.Concat(OutputPath.FullName, "\", xmlFname), False, System.Text.Encoding.UTF8)
            Using sr As New StreamReader(SourceFile)
                Do While sr.Peek > 0
                    Dim strLine As String = String.Empty
                    strLine = sr.ReadLine

                    If Regex.IsMatch(strLine, "<" & parentElem & ">") Then
                        blnStart = True
                    End If

                    If blnStart = True Then
                        sw.WriteLine(strLine)
                    End If


                    If Regex.IsMatch(strLine, "<\/" & parentElem.Replace(" role=""proclamations""", "").Trim & ">") Then
                        blnStart = False
                    End If
                Loop
            End Using
        End Using


        File.Copy(SourceFile, String.Concat(OutputPath.FullName, "\", xmlFname, ".orig"), True)
        Try
            File.Delete(SourceFile)
        Catch ex As Exception
        End Try

    End Sub


End Module
