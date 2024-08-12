Imports HtmlAgilityPack
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Web

Module ValidationXML


    Public Sub ValidateTxtOrXmlFile(ByVal SourceFile As String, ByVal GranuleType As String, ByVal InputType As String)
        Dim outputType As String = String.Empty
        Dim dcTitle As String = String.Empty
        Dim dcDate As String = String.Empty
        Dim dcType As String = String.Empty
        Dim dcTypeMeta As String = String.Empty
        Dim congressMeta As String = String.Empty
        Dim congressPref As String = String.Empty
        Dim blnDocTitle As Boolean = False
        Dim blnOfficialTitle As Boolean = False
        Dim intDcTypeMeta As Integer = 0
        Dim intDcTypePref As Integer = 0
        Dim docNumberPref As String = String.Empty
        Dim docNumberMeta As String = String.Empty
        Dim dcTitleMatch As Match = Nothing
        Dim linePosLongTitle As Integer = 0
        Dim approvedDateMeta As String = String.Empty
        Dim casingNum As String = String.Empty
        Dim casingHeading As String = String.Empty
        Dim tempText As String = String.Empty
        Dim blnCongressStatAtlrge As Boolean = False
        Dim blnStatutes As Boolean = False
        Dim strVolume As String = String.Empty

        Dim strDcCreator As String = String.Empty

        Dim sourceText As String = File.ReadAllText(SourceFile)
        sourceText = sourceText.Replace("<meta>", "<metagpo>").Replace("</meta>", "</metagpo>")

        strVolume = Regex.Match(Path.GetFileName(SourceFile), "STATUTE\-(\d+)(\-|\.)").Groups(1).Value

        ValidateCaingPerElement(sourceText)

        Form1.lblProcessSubText.Text = "validating per element..." : Form1.lblProcessSubText.Refresh()



        Dim hdoc As New HtmlDocument
        hdoc.OptionOutputOriginalCase = True
        hdoc.LoadHtml(sourceText)


        For Each elem As HtmlNode In hdoc.DocumentNode.Descendants().ToList

            'Form1.lblProcessSubText.Text = String.Format("validating - <{0}>", elem.OriginalName) : Form1.lblProcessSubText.Refresh()

            Select Case elem.OriginalName
                Case "statutesAtLarge"
                    blnStatutes = True
                    blnCongressStatAtlrge = False
                Case "pLaw", "resolution", "presidentialDoc", "presidentialDocs"
                    outputType = elem.OriginalName

                    If linePosLongTitle <> 0 Then
                        If blnOfficialTitle = False Or blnDocTitle = False Then
                            AddErrorMessageValidation(linePosLongTitle,
                                      "longTitle",
                                      "There must be an elements <docTitle> and <officialTitle> inside <longTitle>",
                                      "",
                                      "")
                        End If
                    End If

                    intDcTypeMeta = 0
                    linePosLongTitle = 0

                    If elem.OriginalName = "presidentialDoc" Then
                        If CInt(strVolume) <= 63 Then
                            Dim strRole As String = elem.GetAttributeValue("role", "")
                            If String.IsNullOrEmpty(strRole) Then
                                AddErrorMessageValidation(elem.Line,
                                  "presidentialDoc",
                                  "<presidentialDoc> element should have attribute 'role'",
                                  "",
                                  "")
                            End If
                        End If
                    End If

                Case "volume"
                    If elem.ParentNode.OriginalName = "metagpo" Then
                        strVolume = elem.InnerText
                    End If

                Case "metagpo"
                    dcType = String.Empty
                    dcTitle = String.Empty
                    strDcCreator = String.Empty
                    dcDate = String.Empty
                    dcTypeMeta = String.Empty


                    If Not String.IsNullOrEmpty(outputType) Or Not String.IsNullOrEmpty(GranuleType) Then
                        If elem.InnerHtml.Contains("<docPart>") = False Then
                            If elem.InnerHtml.Contains("<dc:type>") = False Then
                                AddErrorMessageValidation(elem.Line,
                                      "dc:type",
                                      "There must be only one <dc:type> inside <meta>.",
                                      "",
                                      "")
                            End If
                        End If
                    End If
                    If outputType = "presidentialDocs" Or outputType = "presidentialDoc" Or GranuleType = "pr" Then
                        If elem.InnerHtml.Contains("<docNumber>") = False Then
                            AddErrorMessageValidation(elem.Line,
                                      "docNumber",
                                      "<docNumber> is mandatory inside <meta> for " & IIf(String.IsNullOrEmpty(outputType), GranuleType, outputType),
                                      "",
                                      "")
                        End If

                        If elem.InnerHtml.Contains("<processedBy>Digitization Vendor</processedBy>") = False Then
                            AddErrorMessageValidation(elem.Line,
                                      "processedBy",
                                      "<processedBy> is mandatory inside <meta> for " & IIf(String.IsNullOrEmpty(outputType), GranuleType, outputType),
                                      "",
                                      "")

                        End If

                        If elem.InnerHtml.Contains("<dc:creator>") Then

                        End If
                    End If

                    If CInt(strVolume) <= 63 Then
                        If elem.InnerHtml.Contains("<dc:type>") = False Then
                            AddErrorMessageValidation(elem.Line,
                                      "dc:type",
                                      "<dc:type> is mandatory inside <meta>",
                                      "",
                                      "")

                        End If

                        If elem.InnerHtml.Contains("<docNumber>") = False Then
                            If elem.InnerHtml.Contains("<citableAs>") = False Then
                                AddErrorMessageValidation(elem.Line,
                                          "citableAs",
                                          "<citableAs> is mandatory inside <meta> if there is no <docNumber>",
                                          "",
                                          "")
                            Else
                                Dim strCitable As String = Regex.Match(elem.InnerHtml, "<citableAs>(.*?)</citableAs>").Groups(1).Value
                                If Not Regex.IsMatch(strCitable, "\d+\s(STAT|Stat)\.\s\d+") Then
                                    AddErrorMessageValidation(elem.Line,
                                          "citableAs",
                                          "Value of <citableAs> inside <meta> should be '# STAT. #'",
                                          strCitable,
                                          "")
                                End If
                            End If
                        End If
                    End If


                Case "preface"

                    If outputType = "presidentialDoc" Or GranuleType = "pr" Then
                        If elem.InnerHtml.Contains("<docNumber>") = False Then
                            AddErrorMessageValidation(elem.Line,
                                      "docNumber",
                                      "<docNumber> is mandatory inside <preface> for " & IIf(String.IsNullOrEmpty(outputType), GranuleType, outputType),
                                      "",
                                      "")
                        End If

                        If elem.InnerHtml.Contains("<dc:type>") = True Then
                            AddErrorMessageValidation(elem.Line,
                                      "dc:type",
                                      "<dc:type> is not allowed inside <preface> for " & IIf(String.IsNullOrEmpty(outputType), GranuleType, outputType),
                                      "",
                                      "")
                        End If

                    End If



                Case "dc:date"

                    ValidateDCDate(elem.InnerText, "dc:date", elem.Line, elem.OuterHtml)
                    dcDate = elem.InnerText


                Case "dc:creator"
                    strDcCreator = elem.InnerText


                    If elem.ParentNode.OriginalName = "metagpo" Then
                        If Regex.IsMatch(dcTypeMeta, "(Treaty|Agreement|Convention|Executive Agreements)") Then
                            AddErrorMessageValidation(elem.Line,
                                                      elem.OriginalName,
                                                      "There should be no <dc:creator> element in <meta> if the <dc:type> is any of the ff: 'Treaty', 'Agreement', 'Convention', 'Executive Agreements'",
                                                      elem.InnerHtml,
                                                      elem.OuterHtml)
                        End If
                    End If

                Case "dc:title"
                    dcTitle = elem.InnerText

                    If Regex.IsMatch(elem.InnerHtml, "<[^>]+>") = True Then
                        '*** Emphasis and other tags in <dc:title> is not allowed. Flag as error if found.
                        AddErrorMessageValidation(elem.Line,
                                                      elem.OriginalName,
                                                      "Emphasis and other tags in <dc:title> is not allowed",
                                                      elem.InnerHtml,
                                                      elem.OuterHtml)
                    End If

                    If Regex.IsMatch(dcTitle, "Chapter", RegexOptions.IgnoreCase) Then
                        '*** There must be no word "Chapter" or "chapter" in <dc:title>
                        AddErrorMessageValidation(elem.Line,
                                                      elem.OriginalName,
                                                      "There must be no word 'Chapter' or 'chapter' in <dc:title>",
                                                      elem.InnerHtml,
                                                      elem.OuterHtml)

                    End If


                    If outputType = "pLaw" Or GranuleType = "pl" Or GranuleType = "pv" Then
                        '*** <dc:title> should have the format "Public Law [nn]–[nn]: " at the beginning of the data.
                        dcTitleMatch = Regex.Match(dcTitle, "^(Public\s?Law)\s\d+(\-|\&\#x2013;|\–|\–)\d+\:\s?(.+)", RegexOptions.IgnoreCase)
                        If dcTitleMatch.Success = False Then
                            AddErrorMessageValidation(elem.Line,
                                                      elem.OriginalName,
                                                      "In <pLaw>, <dc:title> should have the format ""Public Law [nn]–[nn]: "" at the beginning of the data.",
                                                      elem.InnerText,
                                                      elem.OuterHtml)
                        End If
                    ElseIf outputType = "resolution" Or GranuleType = "hc" Or GranuleType = "sc" Then
                        '*** <dc:title> should have the format "H. Con. Res. [nn]: " or "S. Con. Res. [nn]: " at the beginning of the data.
                        dcTitleMatch = Regex.Match(dcTitle, "^((H|S)\.\s?Con\.\s?Res\.\s?\d+)\:\s?(.+)", RegexOptions.IgnoreCase)
                        If dcTitleMatch.Success = False Then
                            AddErrorMessageValidation(elem.Line,
                                                      elem.OriginalName,
                                                      "In <resolution>, <dc:title> should have the format ""H. Con. Res. [nn]: "" or ""S. Con. Res. [nn]: "" at the beginning of the data.",
                                                      elem.InnerText,
                                                      elem.OuterHtml)
                        End If
                    Else
                        dcTitleMatch = Regex.Match(dcTitle, "(.+)")
                    End If


                Case "dc:type"

                    dcType = elem.InnerText

                    If elem.ParentNode.OriginalName = "metagpo" Then
                        intDcTypeMeta += 1
                        dcTypeMeta = elem.InnerText

                    ElseIf elem.ParentNode.OriginalName = "preface" Then
                        intDcTypePref += 1

                        If Not String.IsNullOrEmpty(dcTypeMeta) Then
                            If Regex.IsMatch(dcType, "(Chap|Chapter)", RegexOptions.IgnoreCase) Then
                                '*** If <dc:type> in <preface> has a word 'Chapter' or 'Chap' (uppercase or lowercase),
                                ''value of <dc:type> in <meta> should be 'Chapter' (case sensitive).
                                If dcTypeMeta <> "Chapter" Then
                                    AddErrorMessageValidation(elem.Line,
                                                      elem.OriginalName,
                                                      "Value of <dc:type> in <meta> should be 'Chapter' (case sensitive).",
                                                      dcTypeMeta,
                                                      elem.OuterHtml)

                                End If


                                'Also, there must be a space after the word ""Chapter"" Or ""Chap"" (any casing) in <preface>."
                                If Not Regex.IsMatch(dcType, "(Chap|Chapter)\s", RegexOptions.IgnoreCase) Then
                                    AddErrorMessageValidation(elem.Line,
                                                      elem.OriginalName,
                                                      "There must be a space after the word 'Chapter' Or 'Chap'",
                                                      dcType,
                                                      elem.OuterHtml)
                                End If

                            End If

                        End If
                    End If

                    If CInt(strVolume) <= 63 Then
                        If elem.ParentNode.OriginalName = "metagpo" Then
                            If Not Regex.IsMatch(dcType, "^(Public Law|Private Law|House Concurrent Resolution|Senate Concurrent Resolution|A Proclamation|Chapter|Treaty|Agreement|Memorandum|Protocol|Convention|Executive Agreements)$") Then
                                AddErrorMessageValidation(elem.Line,
                                                      elem.OriginalName,
                                                      "<dc:type> value should be any of the ff: 'Public Law', 'Private Law', 'House Concurrent Resolution', 'Senate Concurrent Resolution', 'A Proclamation', 'Chapter', 'Treaty', 'Agreement', 'Memorandum', 'Protocol', 'Convention', 'Executive Agreements'",
                                                      dcType,
                                                      elem.OuterHtml)
                            End If
                        End If
                    End If

                    If outputType = "pLaw" Then
                        '*** In <pLaw>, <dc:type> should either be "Public Law" or "Private Law"
                        'and should match in the first data in the <dc:title>.

                        '*** 04-12-24 In <pLaw>, the requirement "<dc:type> should either be "Public Law" or "Private Law"
                        'and should match in the first data in the <dc:title>." is not applicable
                        'sa lower volumes (<volume>64</volume> and below)

                        If String.IsNullOrEmpty(strVolume) = False Then
                            If CInt(strVolume) >= 64 Then
                                If Regex.IsMatch(dcType, "^((Public|Private) Law)$") = False Then
                                    AddErrorMessageValidation(elem.Line,
                                                      elem.OriginalName,
                                                      "In <pLaw>, <dc:type> should either be ""Public Law"" or ""Private Law""",
                                                      dcType,
                                                      elem.OuterHtml)
                                End If
                            End If
                        Else
                            If Regex.IsMatch(dcType, "^((Public|Private) Law)$") = False Then
                                AddErrorMessageValidation(elem.Line,
                                                      elem.OriginalName,
                                                      "In <pLaw>, <dc:type> should either be ""Public Law"" or ""Private Law""",
                                                      dcType,
                                                      elem.OuterHtml)
                            End If
                        End If



                        If String.IsNullOrEmpty(dcTitle) Then
                            AddErrorMessageValidation(elem.Line,
                                                      elem.OriginalName,
                                                      "Unable to validate. In <pLaw>, <dc:type> value should match in the first data in the <dc:title>",
                                                      dcTitle,
                                                      elem.OuterHtml)
                        Else
                            If dcTitle.StartsWith(dcType) = False Then
                                AddErrorMessageValidation(elem.Line,
                                                          elem.OriginalName,
                                                          "In <pLaw>, <dc:type> value should match in the first data in the <dc:title>",
                                                          dcTitle,
                                                          elem.OuterHtml)
                            End If
                        End If

                    ElseIf outputType = "resolution" Then
                        '*** In <resolution>, if the <dc:title> starts in "S. Con. Res.", value of <dc:type> should be "Senate Concurrent Resolution".
                        ' However, if the value of <dc:title> starts in "H. Con. Res.", value of <dc:type> should be "House Concurrent Resolution"
                        If String.IsNullOrEmpty(dcTitle) Then
                            AddErrorMessageValidation(elem.Line,
                                                              elem.OriginalName,
                                                              "Unable to validate. In <resolution>, if the <dc:title> starts in ""S. Con. Res."" or ""H. Con. Res."", value of <dc:type> should be ""Senate Concurrent Resolution"" or ""House Concurrent Resolution""",
                                                              dcTitle,
                                                              elem.OuterHtml)
                        Else
                            If dcTitle.StartsWith("S. Con. Res.") Then
                                If dcType <> "Senate Concurrent Resolution" Then
                                    AddErrorMessageValidation(elem.Line,
                                                              elem.OriginalName,
                                                              "In <resolution>, if the <dc:title> starts in ""S. Con. Res."", value of <dc:type> should be ""Senate Concurrent Resolution""",
                                                              dcTitle,
                                                              elem.OuterHtml)
                                End If
                            ElseIf dcTitle.StartsWith("H. Con. Res.") Then
                                If dcType <> "House Concurrent Resolution" Then
                                    AddErrorMessageValidation(elem.Line,
                                                              elem.OriginalName,
                                                              "In <resolution>, if the <dc:title> starts in ""H. Con. Res."", value of <dc:type> should be ""House Concurrent Resolution""",
                                                              dcTitle,
                                                              elem.OuterHtml)
                                End If
                            End If
                        End If

                    ElseIf outputType = "presidentialDoc" Then
                        '*** In <presidentialDoc>, if the <docNumber> in <preface> has a word "Proclamation"
                        'at the beginning of the data, <dc:type> should be "A Proclamation".
                        If String.IsNullOrEmpty(docNumberPref) Then
                            AddErrorMessageValidation(elem.Line,
                                                      elem.OriginalName,
                                                      "Unable to validate. In <presidentialDoc>, if the <docNumber> in <preface> has a word ""Proclamation"" at the beginning of the data, <dc:type> should be ""A Proclamation""",
                                                      docNumberPref,
                                                      elem.OuterHtml)
                        Else
                            If docNumberPref.StartsWith("Proclamation") Then
                                If dcType <> "A Proclamation" Then
                                    AddErrorMessageValidation(elem.Line,
                                                              elem.OriginalName,
                                                              "In <presidentialDoc>, if the <docNumber> in <preface> has a word ""Proclamation"" at the beginning of the data, <dc:type> should be ""A Proclamation""",
                                                              docNumberPref,
                                                              elem.OuterHtml)
                                End If
                            End If
                        End If
                    End If


                Case "docNumber"
                    If elem.ParentNode.OriginalName = "metagpo" Then
                        docNumberMeta = elem.InnerText

                        If outputType = "pLaw" Or GranuleType = "pl" Or GranuleType = "pv" Then
                            '*** <docNumber> in <meta> should match in the 2nd number in <dc:title>
                            'after the word "Public Law" or "Private Law", or before colon  ( : )

                            If String.IsNullOrEmpty(dcTitle) Then
                                AddErrorMessageValidation(elem.Line,
                                                              elem.OriginalName,
                                                              "Unabel to validate. In <pLaw>, <docNumber> in <meta> should match in the 2nd number in <dc:title> after the word ""Public Law"" or ""Private Law"", or before colon  ( : ).",
                                                              dcTitle,
                                                              elem.OuterHtml)
                            Else
                                If Regex.IsMatch(dcTitle, "(\-|\&\#x2013;|\–|\–)" & docNumberMeta & "\:") = False Then
                                    AddErrorMessageValidation(elem.Line,
                                                              elem.OriginalName,
                                                              "In <pLaw>, <docNumber> in <meta> should match in the 2nd number in <dc:title> after the word ""Public Law"" or ""Private Law"", or before colon  ( : ).",
                                                              dcTitle,
                                                              elem.OuterHtml)
                                End If
                            End If

                        ElseIf outputType = "resolution" Or GranuleType = "hc" Or GranuleType = "sc" Then
                            '*** <docNumber> in <meta> should match in the number in <dc:title>
                            If String.IsNullOrEmpty(dcTitle) Then
                                AddErrorMessageValidation(elem.Line,
                                                    elem.OriginalName,
                                                    "Unable to validate. In <resolution>, <docNumber> in <meta> should match in the number in <dc:title>",
                                                    dcTitle,
                                                    elem.OuterHtml)
                            Else
                                If Regex.IsMatch(dcTitle, "(H|S)\.\sCon\.\sRes\.\s" & Regex.Escape(docNumberMeta) & "\:") = False Then
                                    AddErrorMessageValidation(elem.Line,
                                                    elem.OriginalName,
                                                    "In <resolution>, <docNumber> in <meta> should match in the number in <dc:title>",
                                                    dcTitle,
                                                    elem.OuterHtml)
                                End If
                            End If
                        End If

                    ElseIf elem.ParentNode.OriginalName = "preface" Then

                        docNumberPref = elem.InnerText

                        If docNumberMeta <> docNumberPref Then
                            '*** generic value in <docNumber> inside <preface> should be the same as the value of <docNumber> inside <meta>
                            AddErrorMessageValidation(elem.Line,
                                                          elem.OriginalName,
                                                          "<docNumber> inside <preface> should be the same as the value of <docNumber> inside <meta>.",
                                                          elem.InnerHtml,
                                                          "<meta>.<docNumber> : " & docNumberMeta)

                        End If



                        If outputType = "presidentialDoc" Then
                            '*** In <presidentialDoc>, the date in <docNumber> should be tagged in
                            'element <date> with attributes.<date date = "yyyy-mm-dd"> [date]</Date> Tag as error if Not.

                            If elem.InnerHtml.Contains("<date ") = False Then
                                AddErrorMessageValidation(elem.Line,
                                                          elem.OriginalName,
                                                          "In <presidentialDoc>, the date in <docNumber> should be tagged in element <date> with attributes.",
                                                          elem.InnerHtml,
                                                          elem.OuterHtml)
                            End If

                        ElseIf outputType = "pLaw" Or GranuleType = "pl" Or GranuleType = "pv" Then
                            '*** <docNumber> in <preface> must be the 1st and 2nd number (including dash)
                            'in <dc:title> after the word "Public Law" or "Private Law".

                            If String.IsNullOrEmpty(dcTitle) Then
                                AddErrorMessageValidation(elem.Line,
                                                        elem.OriginalName,
                                                        "Unable to validate. In <pLaw>, <docNumber> in <preface> must be the 1st and 2nd number (including dash) in <dc:title> after the word ""Public Law"" or ""Private Law""",
                                                        dcTitle,
                                                        elem.OuterHtml)
                            Else

                                If String.IsNullOrEmpty(strVolume) = False Then
                                    If CInt(strVolume) >= 64 Then
                                        If Regex.IsMatch(dcTitle, "(Private|Public)\sLaw\s" & Regex.Escape(docNumberPref)) = False Then
                                            AddErrorMessageValidation(elem.Line,
                                                              elem.OriginalName,
                                                              "In <pLaw>, <docNumber> in <preface> must be the 1st and 2nd number (including dash) in <dc:title> after the word ""Public Law"" or ""Private Law""",
                                                              dcTitle,
                                                              elem.OuterHtml)
                                        End If
                                    Else
                                        'If Regex.IsMatch(dcTitle, "\s" & Regex.Escape(docNumberPref) & "\.?\s?\:") = False Then
                                        '    AddErrorMessageValidation(elem.Line,
                                        '                      elem.OriginalName,
                                        '                      "In <pLaw>, <docNumber> in <preface> must be the 1st and 2nd number (including dash) in <dc:title> after the word ""Public Law"" or ""Private Law""",
                                        '                      dcTitle,
                                        '                      elem.OuterHtml)
                                        'End If
                                    End If
                                Else
                                    If Regex.IsMatch(dcTitle, "(Private|Public)\sLaw\s" & Regex.Escape(docNumberPref)) = False Then
                                        AddErrorMessageValidation(elem.Line,
                                                              elem.OriginalName,
                                                              "In <pLaw>, <docNumber> in <preface> must be the 1st and 2nd number (including dash) in <dc:title> after the word ""Public Law"" or ""Private Law""",
                                                              dcTitle,
                                                              elem.OuterHtml)
                                    End If
                                End If

                            End If

                        ElseIf outputType = "presidentialDocs" Or GranuleType = "pr" Then
                            '*** <docNumber> in <meta> should match in the <docNumber> in <preface>,
                            'after the word "Proclamation".
                            If Regex.IsMatch(docNumberPref, "Proclamation " & Regex.Escape(docNumberMeta)) = False Then
                                AddErrorMessageValidation(elem.Line,
                                                          elem.OriginalName,
                                                          "In <presidentialDocs role=""proclamations"">, <docNumber> in <meta> should match in the <docNumber> in <preface>, after the word ""Proclamation""",
                                                          docNumberMeta,
                                                          elem.OuterHtml)
                            End If
                        End If

                    End If


                Case "congress"
                    If elem.ParentNode.OriginalName = "metagpo" And elem.ParentNode.ParentNode.OriginalName = "statutesAtLarge" Then
                        blnCongressStatAtlrge = True
                    End If

                    If elem.ParentNode.OriginalName = "metagpo" Then
                        '*** Value of <congress> within <meta> should be the same across the
                        'XML file and value should be numeric only.

                        If String.IsNullOrEmpty(congressMeta) Then
                            congressMeta = elem.InnerText
                        Else
                            If elem.InnerText <> congressMeta Then
                                AddErrorMessageValidation(elem.Line,
                                                          elem.OriginalName,
                                                          "Value of <congress> within <meta> should be the same across the XML file",
                                                          congressMeta,
                                                          elem.OuterHtml)
                            End If
                        End If

                        If Regex.IsMatch(elem.InnerText, "^\d+$") = False Then
                            AddErrorMessageValidation(elem.Line,
                                                      elem.OriginalName,
                                                      "Value of <congress> should be numeric only",
                                                      elem.InnerText,
                                                      elem.OuterHtml)
                        End If

                        If outputType = "pLaw" Then
                            '*** In <pLaw>, value should be the same as the 1st number appear in <dc:title>
                            If String.IsNullOrEmpty(dcTitle) Then
                                AddErrorMessageValidation(elem.Line,
                                                          elem.OriginalName,
                                                          "In <pLaw>, <congress> in <meta> should match in the 1st number in <dc:title> after the word ""Public Law"" or ""Private Law""",
                                                          dcTitle,
                                                          elem.OuterHtml)
                            Else
                                If Regex.IsMatch(dcTitle, "\s" & congressMeta & "(\-|\&\#x2013;|\–|\–)") = False Then
                                    AddErrorMessageValidation(elem.Line,
                                                              elem.OriginalName,
                                                              "In <pLaw>, <congress> in <meta> should match in the 1st number in <dc:title> after the word ""Public Law"" or ""Private Law""",
                                                              dcTitle,
                                                              elem.OuterHtml)
                                End If
                            End If
                        End If

                    ElseIf elem.ParentNode.OriginalName = "preface" Then
                        '*** Value of <congress> in <preface> should be the same across the XML file
                        'and it should have an attribute "value" where data should be numeric and the same as the data in <meta> and the number in <congress> element.
                        If String.IsNullOrEmpty(congressPref) Then
                            congressPref = elem.OuterHtml
                        Else
                            If elem.OuterHtml <> congressPref Then
                                AddErrorMessageValidation(elem.Line,
                                                          elem.OriginalName,
                                                          "Value of <congress> within <preface> should be the same across the XML file",
                                                          congressPref,
                                                          elem.OuterHtml)
                            End If
                        End If

                        Dim congValue As String = elem.GetAttributeValue("value", "")
                        If String.IsNullOrEmpty(congValue) Then
                            AddErrorMessageValidation(elem.Line,
                                            elem.OriginalName,
                                            "Missing attribute 'value' of <congress> within <preface>",
                                            elem.OuterHtml,
                                            elem.OuterHtml)
                        Else
                            If Regex.IsMatch(congValue, "^\d+$") = False Or
                                congValue <> congressMeta Or
                                elem.InnerText.Contains(congValue) = False Then

                                AddErrorMessageValidation(elem.Line,
                                                          elem.OriginalName,
                                                          "Value of attribute 'value' of <congress> within <preface> should be numeric and the same as the data in <meta> and the number in <congress> element.",
                                                          congValue,
                                                          elem.OuterHtml)
                            End If
                        End If

                    End If

                Case "main"

                    If outputType = "presidentialDocs" Or outputType = "presidentialDoc" Or GranuleType = "pr" Then
                        If Not String.IsNullOrEmpty(strDcCreator) Then

                            Dim strMainData As String = Regex.Match(elem.OuterHtml, "(.*?)<(content|p|section|paragraph|title|num)([^>]*)>", RegexOptions.Singleline).Value
                            If String.IsNullOrEmpty(strMainData) Then
                                If elem.OuterHtml.Length > 1500 Then
                                    strMainData = strMainData.Substring(0, 1500)
                                Else
                                    strMainData = elem.OuterHtml
                                End If
                            End If

                            If strMainData.Contains("<authority>") = False Then
                                AddErrorMessageValidation(elem.Line,
                                        "authority",
                                        String.Format("In {0}, the <dc:creator> in the <meta> is present, <authority> is required in <main>.", IIf(String.IsNullOrEmpty(outputType), GranuleType, outputType)),
                                        strDcCreator,
                                        strMainData)

                            ElseIf strMainData.Contains(String.Format("<authority>{0}</authority>", strDcCreator)) = False Then
                                AddErrorMessageValidation(elem.Line,
                                        "authority",
                                        String.Format("In {0}, the <dc:creator> in the <meta> value, should be the same with <authority> in <main>.", IIf(String.IsNullOrEmpty(outputType), GranuleType, outputType)),
                                        strDcCreator,
                                        strMainData)
                            End If
                        End If
                    End If


                Case "sidenote"

                    If elem.PreviousSibling Is Nothing = False Then
                        'If elem.PreviousSibling.OriginalName = "officialTitle" Then
                        If tempText.Contains("<officialTitle>") Then
                            If outputType = "resolution" Then
                                '*** Value of <dc:date> in <resolution> should be the same as the
                                'data/date in <sidenote> after the </officialTitle>. Flag as error if not.
                                If elem.InnerText.Contains(dcDate) = False Then
                                    AddErrorMessageValidation(elem.Line,
                                                              elem.OriginalName,
                                                              "In <resolution>, Value of <dc:date> in <resolution> should be the same as the data/date in <sidenote> after the </officialTitle>",
                                                              dcDate,
                                                              elem.OuterHtml)
                                End If

                                '*** In <resolution>, the date in <sidenote> after the </officialTitle>
                                'should not be tagged inside <approvedDate>
                                If elem.InnerHtml.Contains("<approvedDate") Then
                                    Dim tappdate As String = Regex.Match(elem.InnerHtml, "<approvedDate[^>]*>(.*?)</approvedDate>").Value
                                    AddErrorMessageValidation(elem.Line,
                                                              elem.OriginalName,
                                                              "In <resolution>, the date in <sidenote> after the </officialTitle> should not be tagged inside <approvedDate>",
                                                              tappdate,
                                                              elem.OuterHtml)
                                End If

                                If IsNothing(dcTitleMatch) = False Then
                                    If elem.InnerText.Contains(dcTitleMatch.Groups(1).Value) = False Then
                                        AddErrorMessageValidation(elem.Line,
                                                                  elem.OriginalName,
                                                                  "In <resolution>, The beginning of the data in <dc:title> should be present in the <sidenote> after the <officialTitle>",
                                                                  dcTitleMatch.Groups(1).Value,
                                                                  elem.OuterHtml)
                                    End If
                                End If
                            ElseIf outputType = "pLaw" Then
                                '*** In <pLaw>, the <approvedDate> in the <meta>, should match the <approvedDate> in the sidenote after the </officialTitle>.
                                If elem.InnerHtml.Contains("<approvedDate") = False Then
                                    AddErrorMessageValidation(elem.Line,
                                                                  elem.OriginalName,
                                                                  "In <pLaw>, the <approvedDate> in the <meta>, should match the <approvedDate> in the sidenote after the </officialTitle>.",
                                                                  approvedDateMeta,
                                                                  elem.OuterHtml)

                                Else
                                    Dim apprvdateMatch As Match = Regex.Match(elem.InnerHtml, "<approvedDate([^>]*)>(.*?)</approvedDate>")
                                    If String.Format(" date=""{0}""", approvedDateMeta) <> apprvdateMatch.Groups(1).Value Then
                                        AddErrorMessageValidation(elem.Line,
                                                                  elem.OriginalName,
                                                                  "In <pLaw>, the <approvedDate> in the <meta>, should match the <approvedDate> in the sidenote after the </officialTitle>.",
                                                                  approvedDateMeta,
                                                                  elem.OuterHtml)
                                    End If
                                End If

                            End If
                        End If
                        tempText = String.Empty
                    End If


                Case "longTitle"
                    linePosLongTitle = elem.Line


                Case "docTitle"
                    If elem.ParentNode.OriginalName = "longTitle" Then
                        blnDocTitle = True
                    End If

                    If outputType = "presidentialDocs" Or outputType = "presidentialDoc" Or GranuleType = "pr" Then
                        If elem.InnerText <> dcType Then
                            AddErrorMessageValidation(elem.Line,
                                        elem.OriginalName,
                                        String.Format("In {0}, the <dc:type> in the <meta>, should match the <docTitle> in <main>.", IIf(String.IsNullOrEmpty(outputType), GranuleType, outputType)),
                                        dcType,
                                        elem.OuterHtml)
                        End If
                    End If


                    If Regex.IsMatch(elem.InnerText, "^(An Act|Joint Resolution|A proclamation)$") = False Then
                        AddErrorMessageValidation(elem.Line,
                                        elem.OriginalName,
                                        "Data of <docTitle> should be any of the following only - 'An Act', 'Joint Resolution', 'A Proclamation'",
                                        elem.InnerText,
                                        elem.OuterHtml)
                    End If


                Case "officialTitle"
                    tempText = String.Empty
                    If elem.ParentNode.OriginalName = "longTitle" Then
                        blnOfficialTitle = True
                    End If

                    Dim offtitle As String = String.Empty
                    offtitle = Regex.Replace(elem.InnerText, "<[>]+>", "").Trim


                    If outputType = "presidentialDocs" Or outputType = "presidentialDoc" Or GranuleType = "pr" Then
                        If offtitle <> dcTitle Then
                            AddErrorMessageValidation(elem.Line,
                                                              elem.OriginalName,
                                                              "Data in <officialTitle> should be the same with data in <dc:title>",
                                                              dcTitle,
                                                              elem.OuterHtml)
                        End If
                    Else
                        '*** Data in <officialTitle> should be the same with data in <dc:title> after the colon(:)
                        If IsNothing(dcTitleMatch) = False Then
                            If dcTitleMatch.Success = True Then
                                If offtitle <> dcTitleMatch.Groups(3).Value.Trim And offtitle <> dcTitleMatch.Value Then
                                    AddErrorMessageValidation(elem.Line,
                                                          elem.OriginalName,
                                                          "Data in <officialTitle> should be the same with data in <dc:title> after the colon(:)",
                                                          dcTitleMatch.Value,
                                                          elem.OuterHtml)
                                End If
                            Else
                                Dim tmpTitle As String = String.Empty
                                If dcTitle.Contains(":") Then
                                    tmpTitle = dcTitle.Substring(dcTitle.IndexOf(":") + 1).Trim
                                End If

                                If elem.InnerText <> tmpTitle.Trim Then
                                    AddErrorMessageValidation(elem.Line,
                                                              elem.OriginalName,
                                                              "Data in <officialTitle> should be the same with data in <dc:title> after the colon(:)",
                                                              tmpTitle,
                                                              elem.OuterHtml)
                                End If
                            End If
                        End If

                    End If

                    '*** <sidenote> before and inside <officialTitle> and <enactingFormula> is not allowed. 
                    If elem.InnerHtml.Contains("<sidenote>") Then
                        AddErrorMessageValidation(elem.Line,
                                                            elem.OriginalName,
                                                             String.Format("<sidenote> inside <{0}> is not allowed.", elem.OriginalName),
                                                            elem.InnerHtml,
                                                            elem.OuterHtml)
                    End If


                Case "enactingFormula"
                    ValidateEndComma(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)

                    '*** <sidenote> before and inside <officialTitle> and <enactingFormula> is not allowed. 
                    If elem.InnerHtml.Contains("<sidenote>") Then
                        AddErrorMessageValidation(elem.Line,
                                                            elem.OriginalName,
                                                             String.Format("<sidenote> inside <{0}> is not allowed.", elem.OriginalName),
                                                            elem.InnerHtml,
                                                            elem.OuterHtml)
                    End If


                Case "resolvingClause"
                    ValidateEndComma(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)


                Case "label"
                    ValidateLabel(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)


                Case "approvedDate"

                    ValidateApprovedDate(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)


                    If elem.ParentNode.OriginalName = "metagpo" Then
                        approvedDateMeta = elem.InnerText
                    ElseIf elem.ParentNode.OriginalName = "sidenote" Or elem.ParentNode.ParentNode.OriginalName = "sidenote" Then


                        If outputType <> "resolution" Then
                            If FormatApprveDateValue(elem.InnerText) <> approvedDateMeta Then
                                errorReportList.Add(New ValidationReportModel() With {.Element = elem.OriginalName,
                                                .LineNo = elem.Line,
                                                 .ErrorMessage = "Value of <approvedDate> in <sidenote> should be the same with <approvedDate> in <meta>",
                                                 .ErrorData = approvedDateMeta,
                                                 .Remarks = elem.OuterHtml})
                            End If
                        End If

                        Dim attAppDate As String = String.Empty
                        Try
                            attAppDate = elem.GetAttributeValue("date", "")
                            If Not String.IsNullOrEmpty(attAppDate) Then
                                If attAppDate <> approvedDateMeta Then
                                    errorReportList.Add(New ValidationReportModel() With {.Element = elem.OriginalName,
                                                .LineNo = elem.Line,
                                                 .ErrorMessage = "Value of <approvedDate> attribute in <sidenote> should be the same with <approvedDate> in <meta>",
                                                 .ErrorData = approvedDateMeta,
                                                 .Remarks = elem.OuterHtml})
                                End If

                                If FormatApprveDateValue(elem.InnerText) <> attAppDate Then
                                    errorReportList.Add(New ValidationReportModel() With {.Element = elem.OriginalName,
                                                .LineNo = elem.Line,
                                                 .ErrorMessage = "Value of <approvedDate> in <sidenote> should be matched with <approvedDate> attribute value",
                                                 .ErrorData = elem.OuterHtml,
                                                 .Remarks = elem.OuterHtml})
                                End If

                            End If
                        Catch ex As Exception
                        End Try


                    End If
                Case "legislativeHistory"
                    Dim collHd As MatchCollection = Regex.Matches(elem.InnerHtml, "<(heading|headingText)>(.*?)</(heading|headingText)>")

                    For Each hd As Match In collHd
                        If hd.Groups(2).Value.Contains(":") Then
                            If hd.Groups(2).Value.EndsWith(":") = False Then
                                AddErrorMessageValidation(elem.Line,
                                         elem.OriginalName,
                                         "Colon ':' inside <heading> under <legislativeHistory> should followed by </heading>",
                                         hd.Value,
                                         hd.Value)
                            End If
                        End If
                    Next

                Case "heading", "headingText"
                    If elem.ParentNode.OriginalName = "appropriations" Then
                        If elem.InnerHtml.Contains("<inline class=""smallCaps"">") Then
                            AddErrorMessageValidation(elem.Line,
                                                  elem.OriginalName,
                                                  "<inline class=""smallCaps""> in the <heading> of <appropriations> is not allowed",
                                                  elem.InnerHtml,
                                                  elem.OuterHtml)
                        End If
                    End If

                    If elem.InnerHtml.Contains("<num") Then
                        Dim strNum As String = Regex.Match(elem.InnerHtml, "<num(\s[^>]+)?>(.*?)</num>").Value

                        AddErrorMessageValidation(elem.Line,
                                                  elem.OriginalName,
                                                  String.Format("<num> should not be a children of <{0}>", elem.OriginalName),
                                                  strNum,
                                                  elem.OuterHtml)
                    End If


                Case "title", "chapter", "part", "division"
                    Dim tmp As String = elem.InnerHtml.Replace(vbCrLf, "").Trim

                    If elem.InnerHtml.Trim.StartsWith("<appropriations ") Or
                        Regex.IsMatch(tmp, "<" & elem.OriginalName & "(\s[^>]+)?><appropriations ") Then
                        AddErrorMessageValidation(elem.Line,
                                                  elem.OriginalName,
                                                  String.Format("<appropriations> is not allowed after <{0}>", elem.OriginalName),
                                                  elem.InnerHtml,
                                                  elem.OuterHtml)

                    End If

                'Case "list", "content", "chapeau", "continuation"
                '    If elem.ParentNode.OriginalName = "p" Then
                '        AddErrorMessageValidation(elem.Line,
                '                                  elem.OriginalName,
                '                                  String.Format("<{0}> should not be a children of <p>", elem.OriginalName),
                '                                  elem.InnerHtml,
                '                                  elem.OuterHtml)

                '    End If

                Case "p"
                    If elem.InnerHtml.Contains("<list>") Then
                        AddErrorMessageValidation(elem.Line,
                                                  elem.OriginalName,
                                                  String.Format("<{0}> should not be a children of <p>", "list"),
                                                  elem.InnerHtml,
                                                  elem.OuterHtml)
                    End If
                    If elem.InnerHtml.Contains("<content>") Then
                        AddErrorMessageValidation(elem.Line,
                                                  elem.OriginalName,
                                                  String.Format("<{0}> should not be a children of <p>", "content"),
                                                  elem.InnerHtml,
                                                  elem.OuterHtml)
                    End If
                    If elem.InnerHtml.Contains("<chapeau>") Then
                        AddErrorMessageValidation(elem.Line,
                                                  elem.OriginalName,
                                                  String.Format("<{0}> should not be a children of <p>", "chapeau"),
                                                  elem.InnerHtml,
                                                  elem.OuterHtml)

                    End If
                    If elem.InnerHtml.Contains("<continuation>") Or elem.InnerHtml.Contains("<continuation ") Then
                        AddErrorMessageValidation(elem.Line,
                                                  elem.OriginalName,
                                                  String.Format("<{0}> should not be a children of <p>", "continuation"),
                                                  elem.InnerHtml,
                                                  elem.OuterHtml)

                    End If
                    If elem.InnerHtml.Contains("<num>") Or elem.InnerHtml.Contains("<num ") Then
                        AddErrorMessageValidation(elem.Line,
                                                  elem.OriginalName,
                                                  String.Format("<{0}> should not be a children of <p>", "num"),
                                                  elem.InnerHtml,
                                                  elem.OuterHtml)

                    End If


                Case "num"
                    If Regex.IsMatch(elem.ParentNode.OriginalName, "^(heading|subheading)$") = True Then
                        AddErrorMessageValidation(elem.Line,
                                                  elem.OriginalName,
                                                  String.Format("<{0}> should not be a children of <{1}>", elem.OriginalName, elem.ParentNode.OriginalName),
                                                  elem.InnerHtml,
                                                  elem.OuterHtml)
                    End If

                Case "section"
                    If elem.InnerHtml.Contains("<resolvingClause") Then
                        Dim strRc As String = Regex.Match(elem.InnerHtml, "<resolvingClause(.*?)</resolvingClause>").Value

                        AddErrorMessageValidation(elem.Line,
                                  elem.OriginalName,
                                  "<resolvingClause> should not be inside <section> element.",
                                  strRc,
                                  strRc)

                    End If

                Case "content"

                    If elem.InnerHtml.Trim.StartsWith("<p>") Then
                        Dim tmp As String = Regex.Match(elem.OuterHtml.Replace(vbCrLf, ""), "(.*?)<\/p>").Value
                        AddErrorMessageValidation(elem.Line,
                                  elem.OriginalName,
                                  "<content> element should not be immediately followed by <p> element.",
                                  tmp,
                                  tmp)

                    End If


            End Select

            'If Not Regex.IsMatch(elem.OriginalName, "(component|presedentialDocs|pLaw|resolution|presidentialDoc|section|subsection|)", RegexOptions.IgnoreCase) Then
            If Regex.IsMatch(elem.OriginalName, "(officialTitle|sidenote)", RegexOptions.IgnoreCase) Then
                If Not String.IsNullOrEmpty(elem.InnerText.Trim) And Not elem.OriginalName.StartsWith("#") Then
                    tempText = elem.OuterHtml
                End If
            End If

        Next



        '*** There must be only one <dc:type> inside <meta> and inside <preface>.
        '<dc:type> is mandatory inside <meta> for <pLaw> and <resolution>,
        'while mandatory in <preface> for <presidentialDoc>.
        'If outputType = "pLaw" Or outputType = "resolution" Then
        '    If intDcTypeMeta = 1 Then
        '    Else
        '        AddErrorMessageValidation(0,
        '                                  "dc:type",
        '                                  "There must be only one <dc:type> inside <meta>. It is mandatory for <pLaw> and <resolution>",
        '                                  "",
        '                                  "")
        '    End If
        'ElseIf outputType = "presidentialDoc" Then
        '    If intDcTypePref = 1 Then
        '    Else
        '        AddErrorMessageValidation(0,
        '                                 "dc:type",
        '                                 "There must be only one <dc:type> inside <preface>. It is mandatory for <presidentialDoc>",
        '                                 "",
        '                                 "")
        '    End If
        'End If

        '*** There must be an elements <docTitle> and <officialTitle> inside <longTitle>.
        'If anyone of these are missing, there must be no <longTitle> tag. Flag as error if there is.
        If linePosLongTitle <> 0 Then
            If blnOfficialTitle = False Or blnDocTitle = False Then
                AddErrorMessageValidation(linePosLongTitle,
                                      "longTitle",
                                      "There must be an elements <docTitle> and <officialTitle> inside <longTitle>",
                                      "",
                                      "")
            End If
        End If



        If blnCongressStatAtlrge = False And blnStatutes = True Then
            AddErrorMessageValidation(0,
                                      "congress",
                                      "Missing <congress> inside <meta> after <statutesAtLarge>",
                                      "",
                                      "")
        End If



    End Sub


    Sub ValidateCaingPerElement(ByVal SourceText As String)


        Dim chkElems = [Enum].GetNames(GetType(ElemCasingChecks))
        Dim hdoc As New HtmlDocument
        hdoc.OptionOutputOriginalCase = True
        hdoc.LoadHtml(SourceText)


        For Each chkEl In chkElems
            Form1.lblProcessSubText.Text = "validating ... casing of " & chkEl : Form1.lblProcessSubText.Refresh()

            Dim casingNum As String = String.Empty
            Dim casingHeading As String = String.Empty


            Dim txtnum As String = String.Empty
            Dim txthead As String = String.Empty


            For Each elem As HtmlNode In hdoc.DocumentNode.Descendants(chkEl.Trim).ToList
                Dim num As HtmlNode = elem.SelectSingleNode("num")
                If IsNothing(num) = False Then
                    Dim cleanNum As String = num.InnerHtml
                    cleanNum = Regex.Replace(cleanNum, "<ref>(.*?)</ref>", "")
                    cleanNum = Regex.Replace(cleanNum, "<sidenote>(.*?)</sidenote>", "")
                    cleanNum = Regex.Replace(cleanNum, "<page[^>]+>(.*?)</page>", "")
                    cleanNum = Regex.Replace(cleanNum, "</?inline([^>]*)>", "")
                    cleanNum = HttpUtility.HtmlDecode(cleanNum)


                    If cleanNum.Contains("CHAPTER 202") Then
                        Debug.WriteLine("WWWW")
                    End If


                    If String.IsNullOrEmpty(casingNum) Then
                        If cleanNum = cleanNum.ToUpper Then
                            casingNum = "uppercase"
                        ElseIf cleanNum = cleanNum.ToLower Then
                            casingNum = "lowercase"
                        Else
                            casingNum = "titlecase"
                        End If
                        txtnum = num.InnerText
                    Else
                        ValidateCasing(cleanNum, num.OriginalName, num.Line, num.OuterHtml, casingNum, txtnum)
                    End If
                End If


                Dim headng As HtmlNode = elem.SelectSingleNode("heading")
                If IsNothing(headng) = False Then
                    Dim cleanHead As String = headng.InnerHtml
                    cleanHead = Regex.Replace(cleanHead, "<ref>(.*?)</ref>", "")
                    cleanHead = Regex.Replace(cleanHead, "<sidenote>(.*?)</sidenote>", "")
                    cleanHead = Regex.Replace(cleanHead, "</?inline([^>]*)>", "")
                    cleanHead = Regex.Replace(cleanHead, "<page[^>]+>(.*?)</page>", "")
                    cleanHead = HttpUtility.HtmlDecode(cleanHead)

                    If String.IsNullOrEmpty(casingHeading) Then
                        If cleanHead = cleanHead.ToUpper Then
                            casingHeading = "uppercase"
                        ElseIf cleanHead = cleanHead.ToLower Then
                            casingHeading = "lowercase"
                        Else
                            casingHeading = "titlecase"
                        End If
                        txthead = headng.InnerText
                    Else
                        ValidateCasing(cleanHead, headng.OriginalName, headng.Line, headng.OuterHtml, casingHeading, txthead)
                    End If
                End If
            Next


        Next








        'If SourceText.Contains("<component>") Then
        '    For Each comp As HtmlNode In hdoc.DocumentNode.Descendants("component").ToList
        '        casingNum = String.Empty
        '        casingHeading = String.Empty

        '        For Each elem As HtmlNode In comp.Descendants(Element).ToList
        '            Dim num As HtmlNode = elem.SelectSingleNode("num")
        '            If IsNothing(num) = False Then
        '                Dim cleanNum As String = num.InnerHtml
        '                cleanNum = Regex.Replace(cleanNum, "<ref>(.*?)</ref>", "")
        '                cleanNum = Regex.Replace(cleanNum, "<sidenote>(.*?)</sidenote>", "")
        '                cleanNum = Regex.Replace(cleanNum, "</?inline([^>]*)>", "")

        '                If String.IsNullOrEmpty(casingNum) Then
        '                    If cleanNum = cleanNum.ToUpper Then
        '                        casingNum = "uppercase"
        '                    ElseIf cleanNum = cleanNum.ToLower Then
        '                        casingNum = "lowercase"
        '                    Else
        '                        casingNum = "titlecase"
        '                    End If
        '                    txtnum = cleanNum
        '                Else
        '                    ValidateCasing(cleanNum, num.OriginalName, num.Line, num.OuterHtml, casingNum, txtnum)
        '                End If
        '            End If


        '            Dim headng As HtmlNode = elem.SelectSingleNode("heading")
        '            If IsNothing(headng) = False Then
        '                Dim cleanHead As String = headng.InnerHtml
        '                cleanHead = Regex.Replace(cleanHead, "<ref>(.*?)</ref>", "")
        '                cleanHead = Regex.Replace(cleanHead, "<sidenote>(.*?)</sidenote>", "")
        '                cleanHead = Regex.Replace(cleanHead, "</?inline([^>]*)>", "")

        '                If String.IsNullOrEmpty(casingHeading) Then
        '                    If cleanHead = cleanHead.ToUpper Then
        '                        casingHeading = "uppercase"
        '                    ElseIf cleanHead = cleanHead.ToLower Then
        '                        casingHeading = "lowercase"
        '                    Else
        '                        casingHeading = "titlecase"
        '                    End If
        '                    txthead = cleanHead
        '                Else
        '                    ValidateCasing(cleanHead, headng.OriginalName, headng.Line, headng.OuterHtml, casingHeading, txthead)
        '                End If
        '            End If
        '        Next
        '    Next

        'Else


        'End If


    End Sub





    Sub ValidateLabel(ByVal LabelValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Check if data in <label> has a combination of characters and date.
        'Flag as error if yes.The Date should be captured as a separate <label>



        If Regex.IsMatch(LabelValue, "(January|February|March|April|May|June|July|August|September|October|November|December|Jan|Feb|Mar|Apr|Jun|Jul|Aug|Sep|Sept|Oct|Nov|Dec)\.?\s\d{1,2}\,?\s\d{4}") Then
            Dim tmpLbl As String = Regex.Replace(LabelValue, "(January|February|March|April|May|June|July|August|September|October|November|December|Jan|Feb|Mar|Apr|Jun|Jul|Aug|Sep|Sept|Oct|Nov|Dec)\.?\s\d{1,2}\,?\s\d{4}", "").Trim

            If Not String.IsNullOrEmpty(tmpLbl) Then
                AddErrorMessageValidation(LineNo,
                                           elementName,
                                           "The date should be captured as a separate <label>",
                                           LabelValue,
                                           elemOuterTxt)
            End If


        End If

    End Sub

    Sub ValidateEndComma(ByVal ElemValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)

        If Regex.IsMatch(ElemValue.Trim, "\,$") = False Then
            AddErrorMessageValidation(LineNo,
                                       elementName,
                                       String.Format("There must be a comma (,) before </{0}>", elementName),
                                       ElemValue,
                                       elemOuterTxt)
        End If


    End Sub

    Sub ValidateCasing(ByVal ElemValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String, ByVal CasingType As String, ByVal xn As String)
        If CasingType = "uppercase" Then
            If ElemValue <> ElemValue.ToUpper Then
                AddErrorMessageValidation(LineNo,
                                            elementName,
                                            String.Format("Check the capitalization of data in <{0}> should be the same across the XML file - {1}", elementName, xn),
                                            ElemValue,
                                            elemOuterTxt)
            End If
        ElseIf CasingType = "lowercase" Then
            If ElemValue <> ElemValue.ToLower Then
                AddErrorMessageValidation(LineNo,
                                            elementName,
                                            String.Format("Check the capitalization of data in <{0}> should be the same across the XML file - {1}", elementName, xn),
                                            ElemValue,
                                            elemOuterTxt)
            End If
        Else
            If ElemValue = ElemValue.ToUpper Or ElemValue = ElemValue.ToLower Then
                AddErrorMessageValidation(LineNo,
                                            elementName,
                                            String.Format("Check the capitalization of data in <{0}> should be the same across the XML file - {1}", elementName, xn),
                                            ElemValue,
                                            elemOuterTxt)
            End If
        End If

    End Sub

    Public Sub AddErrorMessageValidation(ByVal lineNo As Integer, ByVal Element As String, ByVal ErrorMessage As String, ByVal ErrorData As String, ByVal Remarks As String)

        errorReportList.Add(New ValidationReportModel() With {.LineNo = lineNo,
                      .Element = Element,
                      .ErrorMessage = ErrorMessage,
                      .ErrorData = ErrorData,
                      .Remarks = Remarks})

    End Sub

End Module
