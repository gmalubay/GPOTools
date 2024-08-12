Imports System.IO
Imports System.Text.RegularExpressions

Module UtilityPresidentialDocs



    Sub ProcessUtilityPresDocs(ByVal InputPath As String)


        OutputPath = New DirectoryInfo(Path.Combine(InputPath, "OUTPUT"))

        If OutputPath.Exists = False Then
            OutputPath.Create()
        End If

        Form1.lblProcessText.Text = "updating presidentialDocs..."

        For Each xFile In Directory.GetFiles(InputPath, "*.xml")
            UpdatePresidentialDocs(xFile)
        Next

    End Sub


    Sub UpdatePresidentialDocs(ByVal SourceFile As String)

        Dim xmlFname As String = Path.GetFileName(SourceFile)

        Form1.lblProcessText.Text = String.Format("processing ... ", xmlFname)
        Form1.lblProcessText.Refresh()

        Dim blnPresStart As Boolean = False
        Dim strPresData As String = String.Empty


        Using sw As New StreamWriter(String.Concat(OutputPath.FullName, "\", xmlFname), False, System.Text.Encoding.UTF8)
            Using sr As New StreamReader(SourceFile)
                Do While sr.Peek > 0
                    Dim strLine As String = String.Empty
                    strLine = sr.ReadLine

                    If Regex.IsMatch(strLine, Regex.Escape("<presidentialDoc>")) Then
                        blnPresStart = True
                        strPresData = String.Empty
                    End If

                    If blnPresStart = True Then
                        strPresData += vbCrLf & strLine

                        If Regex.IsMatch(strLine, "<main>") Then
                            blnPresStart = False

                            Dim strMeta As String = String.Empty
                            Dim strPreface As String = String.Empty
                            Dim strMain As String = String.Empty

                            Dim strDCTitle As String = String.Empty
                            Dim strDCType As String = String.Empty
                            Dim strDCCreator As String = String.Empty
                            Dim strLongtitle As String = String.Empty

                            Dim metaMatch As Match = Regex.Match(strPresData, "<meta>(.+)</meta>", RegexOptions.Singleline)

                            Dim prefMatch As Match = Regex.Match(strPresData, "<preface>(.+)</preface>", RegexOptions.Singleline)

                            strPreface = prefMatch.Value
                            strMeta = metaMatch.Value

                            'get dc:title, dc:type, dc:creator  from <preface>
                            strDCTitle = Regex.Match(strPreface, "<dc:title>(.+)</dc:title>").Value
                            strDCType = Regex.Match(strPreface, "<dc:type>(.+)</dc:type>").Value
                            strDCCreator = Regex.Match(strPreface, "<dc:creator>(.+)</dc:creator>").Value

                            strPreface = strPreface.Replace(strDCTitle, "")
                            strPreface = strPreface.Replace(strDCCreator, "")
                            strPreface = strPreface.Replace(strDCType, "")
                            strPreface = strPreface.Replace(vbCrLf, "")


                            If strMeta.Contains("<dc:title>") = False Then
                                strMeta = strMeta.Replace("</meta>", String.Concat(strDCTitle, "</meta>"))
                            End If

                            If strMeta.Contains("<processedBy>") = False Then
                                strMeta = strMeta.Replace("</meta>", String.Concat("<processedBy>Digitization Vendor</processedBy>", "</meta>"))
                            End If

                            If strMeta.Contains("<dc:creator>") = False Then
                                strMeta = strMeta.Replace("</meta>", String.Concat(strDCCreator, "</meta>"))
                            End If

                            If strMeta.Contains("<dc:type>") = False Then
                                strMeta = strMeta.Replace("</meta>", String.Concat(strDCType, "</meta>"))
                            End If

                            strMeta = strMeta.Replace("><", ">" & vbCrLf & "<")


                            strLongtitle = String.Format("{0}<longTitle>{0}{1}{0}{2}{0}{3}{0}</longTitle>", vbCrLf,
                                        strDCTitle.Replace("dc:title>", "officialTitle>"),
                                        strDCCreator.Replace("dc:creator>", "authority>"),
                                        strDCType.Replace("dc:type>", "docTitle>"))


                            strPresData = strPresData.Replace(prefMatch.Value, strPreface)
                            strPresData = strPresData.Replace(metaMatch.Value, strMeta)

                            strPresData = strPresData.Replace("<main>", "<main>" & strLongtitle)
                            strLine = strPresData.Trim
                        Else
                            strLine = String.Empty
                        End If
                    End If

                    If Not String.IsNullOrEmpty(strLine) Then

                        sw.WriteLine(strLine)
                    End If
                Loop
            End Using
        End Using


    End Sub




    Sub UpdatePresDocs(ByVal SourceFile As String)

        Dim xmlFname As String = Path.GetFileName(SourceFile)

        Form1.lblProcessText.Text = String.Format("processing ... ", xmlFname)
        Form1.lblProcessText.Refresh()

        Dim strContent As String = String.Empty
        Dim strMeta As String = String.Empty
        Dim strPreface As String = String.Empty
        Dim strMain As String = String.Empty

        Dim strDCTitle As String = String.Empty
        Dim strDCType As String = String.Empty
        Dim strDCCreator As String = String.Empty


        strContent = File.ReadAllText(SourceFile, System.Text.Encoding.UTF8)

        Dim metaMatch As Match = Regex.Match(strContent, "<meta>(.+)</meta>", RegexOptions.Singleline)

        Dim prefMatch As Match = Regex.Match(strContent, "<preface>(.+)</preface>", RegexOptions.Singleline)

        strPreface = prefMatch.Value
        strMeta = metaMatch.Value

        strMain = Regex.Match(strContent, "<main>(.+)</main>", RegexOptions.Singleline).Value


        'get dc:title, dc:type, dc:creator  from <preface>
        strDCTitle = Regex.Match(strPreface, "<dc:title>(.+)</dc:title>").Value
        strDCType = Regex.Match(strPreface, "<dc:type>(.+)</dc:type>").Value
        strDCCreator = Regex.Match(strPreface, "<dc:creator>(.+)</dc:creator>").Value


        strPreface = strPreface.Replace(strDCTitle, "")
        strPreface = strPreface.Replace(strDCCreator, "")
        strPreface = strPreface.Replace(strDCType, "")
        strPreface = strPreface.Replace(vbCrLf, "")


        If strMeta.Contains("<dc:title>") = False Then
            strMeta = strMeta.Replace("</meta>", String.Concat(strDCTitle, "</meta>"))
        End If

        If strMeta.Contains("<processedBy>") = False Then
            strMeta = strMeta.Replace("</meta>", String.Concat("<processedBy>Digitization Vendor</processedBy>", "</meta>"))
        End If

        If strMeta.Contains("<dc:creator>") = False Then
            strMeta = strMeta.Replace("</meta>", String.Concat(strDCCreator, "</meta>"))
        End If

        If strMeta.Contains("<dc:type>") = False Then
            strMeta = strMeta.Replace("</meta>", String.Concat(strDCType, "</meta>"))
        End If

        strMeta = strMeta.Replace("><", ">" & vbCrLf & "<")


        If strMain.Contains("<longTitle>") = False Then
            Dim strLongtitle As String = String.Empty

            strLongtitle = String.Format("<longTitle>{0}{1}{0}{2}{0}{3}{0}</longTitle>", vbCrLf,
                                        strDCTitle.Replace("dc:title>", "officialTitle>"),
                                        strDCCreator.Replace("dc:creator>", "authority>"),
                                        strDCType.Replace("dc:type>", "docTitle>"))

            strContent = strContent.Replace("<main>", String.Concat("<main>", vbCrLf, strLongtitle))
        End If


        strContent = strContent.Replace(prefMatch.Value, strPreface)
        strContent = strContent.Replace(metaMatch.Value, strMeta)


        Using sw As New StreamWriter(String.Concat(OutputPath.FullName, "\", xmlFname), False, System.Text.Encoding.UTF8)
            sw.WriteLine(strContent)
        End Using


        File.Copy(SourceFile, String.Concat(OutputPath.FullName, "\", xmlFname, ".orig"), True)
        Try
            File.Delete(SourceFile)
        Catch ex As Exception
        End Try
    End Sub






End Module
