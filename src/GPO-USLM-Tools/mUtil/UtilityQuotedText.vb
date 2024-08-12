Imports System.IO
Imports System.Text.RegularExpressions

Module UtilityQuotedText


    Sub ProcessUtilityQuotedText(ByVal InputPath As String)


        OutputPath = New DirectoryInfo(Path.Combine(InputPath, "OUTPUT"))

        If OutputPath.Exists = False Then
            OutputPath.Create()
        End If

        Form1.lblProcessText.Text = "validating quotedText..."

        For Each xFile In Directory.GetFiles(InputPath, "*.xml")
            RemoveQuotedText(xFile)
        Next

    End Sub

    Sub RemoveQuotedText(ByVal SourceFile As String)
        Dim xmlFname As String = Path.GetFileName(SourceFile)

        Form1.lblProcessText.Text = String.Format("processing ... ", xmlFname)
        Form1.lblProcessText.Refresh()

        Dim sbcontent As New System.Text.StringBuilder

        Using sr As New StreamReader(SourceFile)
            Do While sr.Peek > 0
                Dim strLine As String = String.Empty
                strLine = sr.ReadLine

                Dim qtColl As MatchCollection = Regex.Matches(strLine, "<quotedText>(.*?)</quotedText>")

                If qtColl.Count = 0 Then
                Else
                    For i As Integer = (qtColl.Count - 1) To 0 Step -1
                        Dim currQt As Match = qtColl.Item(i)
                        Dim strQT As String = String.Empty
                        Dim strPrvQt As String = String.Empty
                        If i = 0 Then
                            strQT = strLine.Substring(strLine.IndexOf(">"), ((currQt.Index + currQt.Length) - strLine.IndexOf(">")))
                        Else
                            Dim prvQt As Match = qtColl.Item(i - 1)
                            strPrvQt = prvQt.Value
                            strQT = strLine.Substring((prvQt.Index + prvQt.Length + 1), (currQt.Index - (prvQt.Index + prvQt.Length + 1) + currQt.Length))

                        End If


                        Dim strQTnew As String = String.Empty


                        If Regex.IsMatch(strQT, "This Act may be cited as the " & ChrW(8220) & "<quotedText>") Then
                            strQTnew = strQT.Replace("<quotedText>", "<shortTitle role=""act"">").Replace("</quotedText>", "</shortTitle>")

                        ElseIf Not Regex.IsMatch(strQT, "(amended|striking|inserting|redesignate|insert|delete|strike|amend|by adding|redesignate|inserting after|by adding at the end the following\:?) " & ChrW(8220) & "<quotedText>") Then

                            If Not String.IsNullOrEmpty(strPrvQt) Then
                                If Not Regex.IsMatch(strLine, Regex.Escape(String.Concat(strPrvQt, ChrW(8221), " the following ", ChrW(8220), strQT))) Then
                                    strQTnew = strQT.Replace("<quotedText>", "").Replace("</quotedText>", "")
                                End If
                            Else
                                strQTnew = strQT.Replace("<quotedText>", "").Replace("</quotedText>", "")
                            End If

                        End If

                        If Not String.IsNullOrEmpty(strQTnew) Then
                            strLine = strLine.Replace(strQT, strQTnew)
                        End If
                    Next
                End If


                sbcontent.AppendLine(strLine)

            Loop
        End Using


        Using sw As New StreamWriter(String.Concat(OutputPath.FullName, "\", xmlFname), False, System.Text.Encoding.UTF8)
            sw.WriteLine(sbcontent.ToString)
        End Using


        File.Copy(SourceFile, String.Concat(OutputPath.FullName, "\", xmlFname, ".orig"), True)
        Try
            File.Delete(SourceFile)
        Catch ex As Exception
        End Try



    End Sub


End Module
