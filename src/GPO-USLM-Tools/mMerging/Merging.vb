Imports System.IO
Imports System.Text.RegularExpressions

Module Merging

    Sub ProcessMergingMain(ByVal InputPath As String)

        OutputPath = New DirectoryInfo(Path.Combine(InputPath, "OUTPUT"))

        If OutputPath.Exists = False Then
            OutputPath.Create()
        End If

        Form1.lblProcessSubText.Text = "merging files..."
        Form1.lblProcessText.Text = "merging"


        Dim itemList As New List(Of String)
        itemList = Directory.GetFiles(InputPath, "*.xml").ToList()
        Try
            itemList.Sort(New NaturalComparer(True))
        Catch ex As Exception
        End Try

        Dim prevParent As String = String.Empty
        Dim OutContent As String = String.Empty

        For Each itm In itemList
            Form1.lblProcessText.Text = String.Format("merging .... {0}", Path.GetFileName(itm))
            Form1.lblProcessText.Refresh()

            Dim strContnt As String = File.ReadAllText(itm)
            Dim prntElem As String = Regex.Match(strContnt, "<[^>]+>").Value

            If Regex.IsMatch(prntElem, "<(publicLaws|privateLaws|concurrentResolutions|presidentialDocs role=""proclamations"")>") = False Then
                prntElem = prevParent
            End If

            If String.IsNullOrEmpty(prevParent) Then
                OutContent = String.Concat(OutContent, vbCrLf, strContnt).Trim
            Else
                If prntElem = prevParent Then
                    Dim prntEndTag As String = prntElem.Replace("<", "</").Replace(" role=""proclamations""", "").Trim
                    strContnt = strContnt.Replace(prntElem, "").Trim
                    strContnt = strContnt.Replace(prntEndTag, "").Trim

                    Dim lastIndex As Integer = OutContent.LastIndexOf(prntEndTag)
                    OutContent = OutContent.Remove(lastIndex, prntEndTag.Length).Insert(lastIndex, strContnt & vbCrLf & prntEndTag)


                    'OutContent = OutContent.Replace(prntEndTag, strContnt & vbCrLf & prntEndTag).Trim

                Else
                    OutContent = String.Concat(OutContent, vbCrLf, strContnt).Trim
                End If
            End If
            prevParent = prntElem
        Next


        Dim outFilename As String = String.Concat(Path.GetFileNameWithoutExtension(itemList(0)), "-merged.xml")

        Using sw As New StreamWriter(String.Concat(OutputPath.FullName, "\", outFilename))
            sw.WriteLine(OutContent)
        End Using

    End Sub

End Module
