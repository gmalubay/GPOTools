Imports HtmlAgilityPack
Imports System.IO
Imports System.Text.RegularExpressions

Module ValidationMain
    Friend errorReportList As New List(Of ValidationReportModel)
    Dim granuleType As String
    Dim pageIdentifier As String
    Dim collPages As New ArrayList
    Dim lastPage As String = String.Empty
    Dim errSpellChks As List(Of SpellCheckLog)

    Sub ProcessValidationMain(ByVal InputPath As String, ByVal IsValidationOnly As Boolean)
        noErrorFiles = New ArrayList

        OutputPath = New DirectoryInfo(Path.Combine(InputPath, "OUTPUT"))

        If OutputPath.Exists = False Then
            OutputPath.Create()
        End If


        For Each txtFile In Directory.GetFiles(InputPath, "*.*")
            Dim inputType As String = String.Empty

            If Regex.IsMatch(txtFile, "(\.(txt|xml))$", RegexOptions.IgnoreCase) = False Then
                Continue For
            End If

            Form1.lblProcessText.Text = String.Format("processing ... {0}", Path.GetFileName(txtFile))
            Form1.lblProcessText.Refresh()

            Form1.lblProcessSubText.Text = "validating ..." : Form1.lblProcessSubText.Refresh()

            errorReportList = New List(Of ValidationReportModel)

            granuleType = String.Empty
            pageIdentifier = String.Empty
            collPages = New ArrayList
            errSpellChks = New List(Of SpellCheckLog)

            If Regex.IsMatch(txtFile, "(\.txt)$", RegexOptions.IgnoreCase) Then
                inputType = "TXT"
                ValidateTxtFile(txtFile)

                Form1.lblProcessSubText.Text = "validating ..." : Form1.lblProcessSubText.Refresh()
                ValidateTxtOrXmlFile(txtFile, granuleType, inputType)



            ElseIf Regex.IsMatch(txtFile, "(\.xml)$", RegexOptions.IgnoreCase) Then
                inputType = "XML"

                Form1.lblProcessSubText.Text = "validating ..." : Form1.lblProcessSubText.Refresh()
                ValidateTxtOrXmlFile(txtFile, granuleType, inputType)

                Dim lines() As String = File.ReadAllLines(txtFile)
                ValidatePerLine(lines, "XML")

            End If

            If IsValidationOnly = True Then
                Form1.lblProcessSubText.Text = "spell checking ..." : Form1.lblProcessSubText.Refresh()
                errSpellChks = ProcessSpellChecker(txtFile)
            End If


            Form1.lblProcessSubText.Text = "generating report ..." : Form1.lblProcessSubText.Refresh()

            Dim sequenceErr = (From q As ValidationReportModel In errorReportList Order By q.LineNo).ToList

            GenerateReportValidation(OutputPath.FullName, Path.GetFileNameWithoutExtension(txtFile) & "-" & inputType, sequenceErr, errSpellChks)

            If errorReportList.Count = 0 Then
                noErrorFiles.Add(txtFile)
            End If
        Next

        Form1.lblProcessText.Text = "..." : Form1.lblProcessText.Refresh()
        Form1.lblProcessSubText.Text = "..." : Form1.lblProcessSubText.Refresh()

    End Sub


    Sub ValidateTxtFile(ByVal SourceFile As String)

        Dim lines() As String = File.ReadAllLines(SourceFile)

        ValidategranuleType(lines)


        Dim txtData As String = File.ReadAllText(SourceFile)
        txtData = txtData.Replace("<meta>", "<metagpo>").Replace("</meta>", "</metagpo>")

        Dim hdoc As New HtmlDocument
        hdoc.OptionAutoCloseOnEnd = True
        hdoc.LoadHtml(txtData)

        Dim parseerrors = hdoc.ParseErrors

        If Not IsNothing(parseerrors) Then
            For Each p In parseerrors
                If p.Code <> 3 Then
                    errorReportList.Add(New ValidationReportModel() With {.Element = p.SourceText,
                            .LineNo = p.Line,
                            .ErrorMessage = p.Reason.Replace("<metagpo>", "<meta>").Replace("</metagpo>", "</meta>"),
                            .ErrorData = p.SourceText,
                            .Remarks = p.SourceText})
                End If
            Next
        End If


        ValidatePerLine(lines, "TXT")

        Dim elems = [Enum].GetNames(GetType(ElemToValidate))

        For Each elem In elems
            Form1.lblProcessSubText.Text = String.Format("validating - <{0}>", elem) : Form1.lblProcessSubText.Refresh()
            txtData = ValidatePerElementTXT(txtData, elem)
        Next


    End Sub

    Function ValidatePerElementTXT(ByVal SourceText As String, ByVal Element As String) As String

        Dim blnFound As Boolean = False
        Dim hdoc As New HtmlDocument
        'hdoc.OptionOutputAsXml = True
        hdoc.OptionOutputOriginalCase = True
        hdoc.LoadHtml(SourceText)

        If Element = "dc_title" Then
            Element = "dc:title"
        ElseIf Element = "dc_date" Then
            Element = "dc:date"
        ElseIf Element = "meta" Then
            Element = "metagpo"
        End If

        For Each elem As HtmlNode In hdoc.DocumentNode.Descendants(Element).ToList

            Select Case elem.OriginalName
                Case "main", "preface", "meta", "dc:title", "metagpo"
                    blnFound = True

                Case "page"
                    ValidatePage(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)






                    elem.InnerHtml = elem.InnerHtml.Replace("|", "")

                Case "coverTitle", "covertitle"
                    ValidateCoverTitle(elem.InnerText, Element, elem.Line, elem.OuterHtml)


                Case "authority"
                    ValidateAuthority(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "coverText"
                    ValidateCoverText(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "enrolledDateline"
                    ValidateEnrolledDateline(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "docNumber"
                    ValidateDocNumber(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "congress"
                    ValidateCongress(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)


                Case "section"
                    ValidateSection(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "content"
                    ValidateContent(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "shortTitle"
                    ValidateShortTitle(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "subsection"
                    ValidateSubSection(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "paragraph"
                    ValidateParagraph(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "subparagraph"
                    ValidateSubParagraph(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "legislativeHistory"
                    ValidateLegislativeHistory(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "title", "subtitle"
                    ValidateTitleSubTitle(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "clause", "subclause"
                    ValidateClauseSubClause(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "continuation"
                    ValidateContinuation(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "chapeau"
                    ValidateChapeau(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "quotedContent"
                    ValidateQuotedContent(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "action"
                    ValidateAction(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "chapter"
                    ValidateChapter(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "item", "subitem"
                    ValidateItemSubItem(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "division"
                    ValidateDivision(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "part"
                    ValidatePart(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "subpart"
                    ValidateSubPart(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "docPart"
                    ValidateDocPart(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "resolvingClause"
                    ValidateResolvingClause(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "preamble"
                    ValidatePreamble(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "recital"
                    ValidateRecital(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "signature"
                    ValidateSignature(elem.InnerText, elem.OriginalName, elem.Line, elem.OuterHtml)
                    elem.InnerHtml = elem.InnerHtml.Replace("|", "")

                Case "enactingFormula"
                    ValidateEnactingFormula(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "level"
                    ValidateLevel(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "article"
                    ValidateArticle(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "proviso"
                    ValidateProviso(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "fillIn"
                    ValidateFillin(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "block"
                    ValidateBlock(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "footnote"
                    ValidateFootnote(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "appropriations"
                    ValidateAppropriations(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "layout"
                    ValidateLayout(elem.InnerHtml, elem.OriginalName, elem.Line, elem.OuterHtml)

                Case "img"
                    ValidateImg(elem.InnerText, elem.OuterHtml, elem.Line, elem.OuterHtml)

                Case "figure", "term", "amendingAction"
                    'There must be no tag 
                    errorReportList.Add(New ValidationReportModel() With {.Element = elem.OriginalName,
                                             .LineNo = elem.Line,
                                             .ErrorMessage = String.Format("There must be no tag <{0}>", elem.OriginalName),
                                             .ErrorData = elem.OuterHtml,
                                             .Remarks = elem.OuterHtml})
            End Select
        Next


        If Regex.IsMatch(Element, "(main|preface|meta|dc\:title|dc_title|metagpo)") Then
            If blnFound = False Then
                errorReportList.Add(New ValidationReportModel() With {.Element = Element,
                         .LineNo = 0,
                         .ErrorMessage = String.Format("Missing <{0}></{0}> element", Element),
                         .ErrorData = "",
                         .Remarks = ""})

            End If
        End If

        Return hdoc.DocumentNode.OuterHtml



    End Function





    Sub ValidatePerLine(ByVal lines() As String, ByVal InputType As String)
        Dim intLine As Integer = 0
        Dim blnDash As Boolean = False
        Dim tempText As String = String.Empty
        Dim blnLongTitle As Boolean = False
        Dim blnRef As Boolean = False
        Dim blnlegislativeHist As Boolean = False
        Dim prevLine As String = String.Empty

        'Form1.lblProcessSubText.Text = String.Format("validating : line#: {0}", intLine) : Form1.lblProcessSubText.Refresh()


        Form1.lblProcessSubText.Text = "validating... please wait..." : Form1.lblProcessSubText.Refresh()



        For Each line In lines
            intLine += 1
            Form1.lblProcessSubText.Text = String.Format("validating : line#: {0}", intLine) : Form1.lblProcessSubText.Refresh()

            ValidateGarbageChar(line, intLine)

            If Regex.IsMatch(line, "<(toc|listOfBillsEnacted|listOfPublicLaws|listOfPrivateLaws|listOfConcurrentResolutions|listOfProclamations)[^>]*>") Then
                blnDash = True
            End If
            ValidateDash(line, intLine, blnDash)

            If Regex.IsMatch(line, "</(toc|listOfBillsEnacted|listOfPublicLaws|listOfPrivateLaws|listOfConcurrentResolutions|listOfProclamations)>") Then
                blnDash = False
            End If

            If line.Contains("<sidenote>") Then
                tempText = String.Empty
            End If
            tempText = tempText.Trim & line

            Dim collElems As MatchCollection = Regex.Matches(line, "<(?!\/)([^>]+)><(?!\/)([^>]+)>")
            For Each mElem As Match In collElems
                Dim el1 As String = mElem.Groups(1).Value.Split(" ").FirstOrDefault
                Dim el2 As String = mElem.Groups(2).Value.Split(" ").FirstOrDefault
                If el1 = el2 Then
                    AddErrorMessageValidation(intLine,
                          el1,
                          "Duplicate element",
                          mElem.Value,
                          line)
                End If
            Next




            If line.Contains("<legislativeHistory") Or line.Contains("<sidenote") Then
                blnRef = True
            End If

            If line.Contains("<ref ") Then
                If blnRef = False Then
                    AddErrorMessageValidation(intLine,
                          "ref",
                          "<ref> in body outside <sidenote> and <legislativeHistory>  is not allowed",
                          line,
                          line)
                End If
            End If
            If line.Contains("</legislativeHistory>") Or line.Contains("</sidenote>") Then
                blnRef = False
            End If

            If line.Contains("<legislativeHistory") Then
                blnlegislativeHist = True
            End If


            If Regex.IsMatch(line, "<heading>END\s?NOTE\:</heading>", RegexOptions.IgnoreCase) Then
                If blnlegislativeHist = True Then
                    errorReportList.Add(New ValidationReportModel() With {.Element = "heading",
                                 .LineNo = intLine,
                                 .ErrorMessage = "<heading> END NOTE is not allowed inside <legislativeHistory>.",
                                 .ErrorData = Regex.Match(line, "<heading>END\s?NOTE\:</heading>", RegexOptions.IgnoreCase).Value,
                                 .Remarks = line})
                End If

                Dim noteMatch As Match
                If line.Contains("<note") Then
                    noteMatch = Regex.Match(line, "<note([^>]*)>")
                Else
                    noteMatch = Regex.Match(prevLine, "<note([^>]*)>")
                End If

                If noteMatch.Success = True Then
                    If noteMatch.Groups(1).Value.Trim <> "type=""endnote""" Then
                        errorReportList.Add(New ValidationReportModel() With {.Element = "heading",
                                 .LineNo = intLine,
                                 .ErrorMessage = "<note> should have attribute : type=""endnote"".",
                                 .ErrorData = Regex.Match(line, "<heading>END\s?NOTE\:</heading>", RegexOptions.IgnoreCase).Value,
                                 .Remarks = line})
                    End If

                End If
            End If


            If line.Contains("</legislativeHistory>") Then
                blnlegislativeHist = False
            End If


            If line.Contains("<officialTitle>") Then
                If Regex.IsMatch(tempText, "</sidenote><officialTitle>") Then
                    errorReportList.Add(New ValidationReportModel() With {.Element = "sidenote",
                                 .LineNo = intLine,
                                 .ErrorMessage = "<sidenote> before <officialTitle> is not allowed.",
                                 .ErrorData = line,
                                 .Remarks = line})
                End If
            ElseIf line.Contains("<enactingFormula>") Then
                If Regex.IsMatch(tempText, "</sidenote><enactingFormula>") Then 'And Not Regex.IsMatch(tempText, "</officialTitle><sidenote>") Then
                    errorReportList.Add(New ValidationReportModel() With {.Element = "sidenote",
                                 .LineNo = intLine,
                                 .ErrorMessage = "<sidenote> before <enactingFormula> is not allowed.",
                                 .ErrorData = line,
                                 .Remarks = line})
                End If

            End If


            'If line.Contains("<page>") Then
            If Regex.IsMatch(line, "<page([^>]*)>") = True Then

                Dim pgColl As MatchCollection = Regex.Matches(line, "<page([^>]*)>([^<]+)</page>")

                If pgColl.Count = 0 Then
                Else
                    For Each pgMatch As Match In pgColl
                        If InputType = "XML" Then
                            Dim pgAtt As String = Regex.Match(pgMatch.Groups(1).Value, "\sidentifier=""([^""]+)""").Groups(1).Value
                            ValidatePageXML(pgMatch.Groups(2).Value, "page", intLine, pgMatch.Value, pgAtt)
                        End If

                        If pgMatch.Success Then
                            Dim pgDash As Match = Regex.Match(pgMatch.Groups(2).Value, "\w+(\-|\—|\–|\&\#x2013;|\&\#x2014;)\d+")

                            If pgDash.Success = True Then
                                If pgDash.Groups(1).Value = "–" Or pgDash.Groups(1).Value = "&#x2013;" Then
                                Else errorReportList.Add(New ValidationReportModel() With {.Element = "page",
                                 .LineNo = intLine,
                                 .ErrorMessage = "Check usage of dash at number range should be ""–"" or &#x2013;",
                                 .ErrorData = pgMatch.Groups(2).Value,
                                 .Remarks = pgMatch.Value})
                                End If
                            End If

                            ValidatePageSequnce(pgMatch.Groups(2).Value, "page", intLine, pgMatch.Value)
                        End If

                    Next
                End If

            End If



            If line.Contains("<quotedText>") Then
                Dim collQT As MatchCollection = Regex.Matches(line, "<?([^<]+)<quotedText>\S+(\s\S+)?")

                For Each qt As Match In collQT
                    If line.Contains("This Act may be cited as the " & ChrW(8221) & "<quotedText>") Then
                        errorReportList.Add(New ValidationReportModel() With {.Element = "quotedText",
                         .LineNo = intLine,
                         .ErrorMessage = String.Format("Invalid {0}<quotedText> after 'This Act may be cited as the'", ChrW(8220)),
                         .ErrorData = qt.Value,
                         .Remarks = line})

                    End If

                    If Regex.IsMatch(qt.Value, "\&\#x201C;<quotedText>") = False And
                    qt.Value.Contains(ChrW(8220) & "<quotedText>") = False Then
                        errorReportList.Add(New ValidationReportModel() With {.Element = "quotedText",
                                 .LineNo = intLine,
                                 .ErrorMessage = String.Format("Missing &#x201C; or {0} before <quotedText>", ChrW(8220)),
                                 .ErrorData = qt.Value,
                                 .Remarks = line})
                    End If



                    If Not Regex.IsMatch(qt.Value, "(?is)(amended|striking|inserting|redesignate|insert|delete|strike|amend|by adding|redesignate|inserting after|by adding at the end the following\:?)\s?" & ChrW(8220) & "<quotedText>") Then
                        errorReportList.Add(New ValidationReportModel() With {.Element = "quotedText",
                         .LineNo = intLine,
                         .ErrorMessage = String.Format("Invalid {0}<quotedText>", ChrW(8220)),
                         .ErrorData = qt.Value,
                         .Remarks = line})
                    End If
                Next
            End If

            If line.Contains("</quotedText>") Then
                Dim errData As String = Regex.Match(line, "(\S+\s)?\S+<\/quotedText>\s?\S+(\s\S+)?").Value

                If Regex.IsMatch(line, "<\/quotedText>\&\#x201D;") = False And
                    errData.Contains("</quotedText>" & ChrW(8221)) = False Then

                    errorReportList.Add(New ValidationReportModel() With {.Element = "quotedText",
                                 .LineNo = intLine,
                                 .ErrorMessage = String.Format("Missing &#x201D; or {0} after </quotedText>", ChrW(8221)),
                                 .ErrorData = errData,
                                 .Remarks = line})
                End If
            End If

            If line.Contains("<img>") Then
                Dim imgMatch As Match = Regex.Match(line, "<img>([^>]+)</img>")
                ValidateImg(imgMatch.Groups(1).Value, "img", intLine, imgMatch.Value)
            End If

            Dim ampColl As MatchCollection = Regex.Matches(line, "[^\S]+\s\&\s[^\S]*")
            If ampColl.Count = 0 Then
            Else
                For Each am As Match In ampColl
                    errorReportList.Add(New ValidationReportModel() With {.Element = "",
                                 .LineNo = intLine,
                                 .ErrorMessage = "Unconverted ampersand(&) should be &amp;",
                                 .ErrorData = am.Value,
                                 .Remarks = line})
                Next
            End If


            Dim hexColl As MatchCollection = Regex.Matches(line, "\&[^\;]+\;")
            If hexColl.Count = 0 Then
            Else
                For Each hx As Match In hexColl
                    If Not Regex.IsMatch(hx.Value, "\&(gt|lt|amp)\;") Then
                        If Regex.IsMatch(hx.Value, "\&\#x\w{4}\;") = False Then
                            errorReportList.Add(New ValidationReportModel() With {.Element = "",
                                 .LineNo = intLine,
                                 .ErrorMessage = "Invalid hexadecimal character, please check.",
                                 .ErrorData = hx.Value,
                                 .Remarks = line})
                        End If
                    End If
                Next
            End If


            Dim dateColl As MatchCollection = Regex.Matches(line, "(January|February|March|April|May|June|July|August|September|October|November|December)\s?\d{1,2}(\,|\.)\s?\d{4}")
            For Each dtc As Match In dateColl
                If Regex.IsMatch(dtc.Value, "(January|February|March|April|May|June|July|August|September|October|November|December)\s\d{1,2}\,\s\d{4}") Then
                Else
                    errorReportList.Add(New ValidationReportModel() With {.Element = "Date",
                         .LineNo = intLine,
                         .ErrorMessage = "Invalid date format, please check.",
                         .ErrorData = dtc.Value,
                         .Remarks = line})
                End If
            Next


            dateColl = Regex.Matches(line, "(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sept|Sep|Oct|Nov|Dec)\.?\s?\d{1,2}(\.|\,)\s?\d{4}")
            For Each dtc As Match In dateColl
                If Regex.IsMatch(dtc.Value, "(Jan\.|Feb\.|Mar\.|Apr\.|May|Jun\.?|Jul\.?|Aug\.|Sept\.|Sep\.|Oct\.|Nov\.|Dec\.)\s\d{1,2}\,\s\d{4}") Then
                Else
                    errorReportList.Add(New ValidationReportModel() With {.Element = "Date",
                         .LineNo = intLine,
                         .ErrorMessage = "Invalid date format, please check.",
                         .ErrorData = dtc.Value,
                         .Remarks = line})
                End If
            Next




            prevLine = line
        Next


    End Sub

    Sub ValidateGranuleType(ByVal lines() As String)

        Dim startLine As String = lines(0)

        '1. Validate if the text file/s have started in tag <granuleType>. If not, flag as error "Missing <granuleType> tag"
        If Not Regex.IsMatch(startLine, "<granuleType>") Then
            errorReportList.Add(New ValidationReportModel() With {.Element = "granuleType",
                    .LineNo = 1,
                    .ErrorMessage = "Missing <granuleType> tag",
                    .ErrorData = startLine,
                    .Remarks = startLine})
        End If

        '2. Check If <granuleType> tag has the format: <granuleType>[grtype]</granuleType>
        'grType should have any Of the following values. If Not, flag as error "incorrect grtype"

        Dim grMatch As Match = Regex.Match(startLine, "<granuleType>(fm|pl|pv|hc|sc|pr|co|ro|bm|sv|ot)</granuleType>")

        If grMatch.Success = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = "granuleType",
                    .LineNo = 1,
                    .ErrorMessage = "Incorrect grtype",
                    .ErrorData = startLine,
                    .Remarks = startLine})
        Else
            granuleType = grMatch.Groups(1).Value
        End If
    End Sub


    Sub ValidateDCDate(ByVal DCDateValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Data should be a valid date or year

        If Regex.IsMatch(DCDateValue, "^\d{4}") Then
        Else
            Dim _dcdate As Date
            If DateTime.TryParse(DCDateValue, _dcdate) = False Then
                If Regex.IsMatch(DCDateValue, "Sept\.?") Then
                    DCDateValue = DCDateValue.Replace("Sept", "Sep")
                End If

                If DateTime.TryParse(DCDateValue, _dcdate) = False Then
                    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                    .LineNo = LineNo,
                                    .ErrorMessage = "Data should be a valid Date Or year",
                                    .ErrorData = DCDateValue,
                                    .Remarks = elemOuterTxt})
                End If

                If Regex.IsMatch(DCDateValue, "^(\d{4}\-\d{2}\-\d{2})$") = False Then
                    If Regex.IsMatch(DCDateValue, "(January|February|March|April|May|June|July|August|September|October|November|December)\s\d{1,2}\,\s\d{4}") Or
                    Regex.IsMatch(DCDateValue, "(Jan\.|Feb\.|Mar\.|Apr\.|May|Jun\.?|Jul\.?|Aug\.|Sept\.|Sep\.|Oct\.|Nov\.|Dec\.)\s\d{1,2}\,\s\d{4}") Then
                    Else
                        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                        .LineNo = LineNo,
                                        .ErrorMessage = "Invalid date format",
                                        .ErrorData = DCDateValue,
                                        .Remarks = elemOuterTxt})
                    End If
                End If

            End If
        End If
    End Sub

    Sub ValidateCoverTitle(ByVal CoverTitleValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Check if data contains "UNITED STATES" "STATUES AT LARGE"

        If Regex.IsMatch(CoverTitleValue, "UNITED STATES") And Regex.IsMatch(CoverTitleValue, "STATUES AT LARGE") Then
        Else
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                    .LineNo = LineNo,
                                    .ErrorMessage = "Data must contains ""UNITED STATES"" ""STATUES AT LARGE""",
                                    .ErrorData = CoverTitleValue,
                                    .Remarks = elemOuterTxt})

        End If

    End Sub

    Sub ValidatePage(ByVal PageValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)

        If elemOuterTxt = "<page></page>" Then
            Exit Sub
        End If

        If PageValue.Contains("|") Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                    .LineNo = LineNo,
                                    .ErrorMessage = "Should not have concut",
                                    .ErrorData = PageValue,
                                    .Remarks = elemOuterTxt})
        Else

            Dim matchP As Match = Regex.Match(PageValue, "^(\d+)\s(STAT\.)\s(\d+)")
            If matchP.Success = True Then
                If String.IsNullOrEmpty(pageIdentifier) Then
                    pageIdentifier = matchP.Groups(1).Value
                Else
                    If matchP.Groups(1).Value <> pageIdentifier Then
                        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                            .LineNo = LineNo,
                            .ErrorMessage = "1st numeric: should match across all <page> tag in the same xml",
                            .ErrorData = matchP.Groups(1).Value,
                            .Remarks = elemOuterTxt})
                    End If
                End If
            Else
                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                    .LineNo = LineNo,
                                    .ErrorMessage = "Value should be [numeric] STAT. [numeric]",
                                    .ErrorData = PageValue,
                                    .Remarks = elemOuterTxt})
            End If
        End If


        ''commented 05-07-24
        ''Check the following:
        ''1. no. of concuts should be 2
        ''2. data before the first and second concut should be numeric"
        ''Dim matchP As Match = Regex.Match(PageValue, "^([^\|]*)\|([^\|]*)\|(.+)")
        ''If matchP.Success = True Then

        ''    If Not Regex.IsMatch(matchP.Groups(1).Value, "^\d+$") Then
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                .LineNo = LineNo,
        ''                .ErrorMessage = "1st concut: Data must be numeric",
        ''                .ErrorData = matchP.Groups(1).Value,
        ''                .Remarks = elemOuterTxt})
        ''    Else
        ''        If String.IsNullOrEmpty(PageIdentifier) Then
        ''            PageIdentifier = matchP.Groups(1).Value
        ''        Else
        ''            If matchP.Groups(1).Value <> PageIdentifier Then
        ''                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "1st concut: should match across all <page> tag in the same xml",
        ''                    .ErrorData = matchP.Groups(1).Value,
        ''                    .Remarks = elemOuterTxt})
        ''            End If
        ''        End If
        ''    End If
        ''    If Not Regex.IsMatch(matchP.Groups(2).Value, "^\d+$") Then
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                .LineNo = LineNo,
        ''                .ErrorMessage = "2nd concut: Data must be numeric",
        ''                .ErrorData = matchP.Groups(2).Value,
        ''                .Remarks = elemOuterTxt})
        ''    End If


        ''    '- <page>#1|#2|[data]</page>
        ''    '- where value of #1 should be the first  number before the data "STAT." and it should match across all <page> tag in the same xml,
        ''    'while the value of #2 should be the data after "STAT."
        ''    '- "STAT." Is default including the period. Flag as error if Not.

        ''    If matchP.Groups(3).Value.StartsWith(String.Concat(matchP.Groups(1).Value, " STAT. ")) = False Then
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                .LineNo = LineNo,
        ''                .ErrorMessage = "1st concut value should match the first number before the data 'STAT.'",
        ''                .ErrorData = matchP.Groups(2).Value,
        ''                .Remarks = elemOuterTxt})
        ''    End If

        ''    If matchP.Groups(3).Value.EndsWith(String.Concat(" STAT. ", matchP.Groups(2).Value)) = False Then
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                .LineNo = LineNo,
        ''                .ErrorMessage = "2nd concut value should match the second number after the data 'STAT.'",
        ''                .ErrorData = matchP.Groups(2).Value,
        ''                .Remarks = elemOuterTxt})
        ''    End If

        ''    If matchP.Groups(3).Value.Contains(" STAT. ") = False Then
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                .LineNo = LineNo,
        ''                .ErrorMessage = "3rd concut value should have 'STAT.'",
        ''                .ErrorData = matchP.Groups(2).Value,
        ''                .Remarks = elemOuterTxt})
        ''    End If

        ''Else
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                            .LineNo = LineNo,
        ''                            .ErrorMessage = "Should have 2 concuts",
        ''                            .ErrorData = PageValue,
        ''                            .Remarks = elemOuterTxt})
        ''End If
    End Sub

    Sub ValidateAuthority(ByVal AuthorityValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Check if data contains a word "Authority"

        If Regex.IsMatch(AuthorityValue, "Authority") Then
        Else
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                    .LineNo = LineNo,
                                    .ErrorMessage = "Data must contains ""Authority""",
                                    .ErrorData = AuthorityValue,
                                    .Remarks = elemOuterTxt})

        End If
    End Sub

    Sub ValidateCoverText(ByVal CoverTextValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Check if data contains a word "session" or "congress"

        If Regex.IsMatch(CoverTextValue, "(session|congress)", RegexOptions.IgnoreCase) Then
        Else
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                    .LineNo = LineNo,
                                    .ErrorMessage = "Data must contains ""session"" or ""congress""",
                                    .ErrorData = CoverTextValue,
                                    .Remarks = elemOuterTxt})

        End If
    End Sub

    Sub ValidateEnrolledDateline(ByVal EnrolledDatelineValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Check if data contains date
        Dim dateMatch As Match = Regex.Match(EnrolledDatelineValue, "\w+\s+\d+(\s|\,)*\d+")

        If dateMatch.Success = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                    .LineNo = LineNo,
                                    .ErrorMessage = "Invalid date",
                                    .ErrorData = EnrolledDatelineValue,
                                    .Remarks = elemOuterTxt})

        Else
            Dim _enrolledDateline As Date
            If DateTime.TryParse(dateMatch.Value, _enrolledDateline) = False Then
                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                    .LineNo = LineNo,
                                    .ErrorMessage = "Invalid date",
                                    .ErrorData = EnrolledDatelineValue,
                                    .Remarks = elemOuterTxt})
            End If

            If Regex.IsMatch(dateMatch.Value, "^(\d{4}\-\d{2}\-\d{2})$") = False Then
                If Regex.IsMatch(dateMatch.Value, "(January|February|March|April|May|June|July|August|September|October|November|December)\s\d{1,2}\,\s\d{4}") Or
                    Regex.IsMatch(dateMatch.Value, "(Jan\.|Feb\.|Mar\.|Apr\.|May|Jun\.?|Jul\.?|Aug\.|Sept\.|Sep\.|Oct\.|Nov\.|Dec\.)\s\d{1,2}\,\s\d{4}") Then
                Else
                    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                        .LineNo = LineNo,
                                        .ErrorMessage = "Invalid date format",
                                        .ErrorData = dateMatch.Value,
                                        .Remarks = elemOuterTxt})
                End If
            End If

        End If
    End Sub

    Sub ValidateDocNumber(ByVal DocNumberValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Check if data contains number

        If Regex.IsMatch(DocNumberValue, "^(\d+|\d+(\&\#x2013;|\-|\–|\–)\d+)$") = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                    .LineNo = LineNo,
                                    .ErrorMessage = "Invalid data",
                                    .ErrorData = DocNumberValue,
                                    .Remarks = elemOuterTxt})

        End If


    End Sub

    Sub ValidateApprovedDate(ByVal ApprovedDateValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Data should be a valid date or year

        Dim _approvedDate As Date

        If Regex.IsMatch(ApprovedDateValue, "^\d{4}$") Then
        Else
            If DateTime.TryParse(ApprovedDateValue, _approvedDate) = False Then
                If Regex.IsMatch(ApprovedDateValue, "Sept\.?") Then
                    ApprovedDateValue = ApprovedDateValue.Replace("Sept", "Sep")
                End If

                If DateTime.TryParse(ApprovedDateValue, _approvedDate) = False Then
                    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                        .LineNo = LineNo,
                                        .ErrorMessage = "Invalid date format",
                                        .ErrorData = ApprovedDateValue,
                                        .Remarks = elemOuterTxt})

                End If
            End If

            If Regex.IsMatch(ApprovedDateValue, "^(\d{4}\-\d{2}\-\d{2})$") = False Then
                If Regex.IsMatch(ApprovedDateValue, "(January|February|March|April|May|June|July|August|September|October|November|December)\s\d{1,2}\,\s\d{4}") Or
                    Regex.IsMatch(ApprovedDateValue, "(Jan\.|Feb\.|Mar\.|Apr\.|May|Jun\.?|Jul\.?|Aug\.|Sept\.|Sep\.|Oct\.|Nov\.|Dec\.)\s\d{1,2}\,\s\d{4}") Then
                Else
                    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                        .LineNo = LineNo,
                                        .ErrorMessage = "Invalid date format",
                                        .ErrorData = ApprovedDateValue,
                                        .Remarks = elemOuterTxt})
                End If
            End If
        End If

    End Sub

    Sub ValidateCongress(ByVal CongressValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Check if data contains number

        If Not Regex.IsMatch(CongressValue, "\d+") Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                 .LineNo = LineNo,
                                 .ErrorMessage = "Data must contain numbers",
                                 .ErrorData = CongressValue,
                                 .Remarks = elemOuterTxt})
        End If

    End Sub

    Sub ValidateSection(ByVal SectionValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)

        SectionValue = SectionValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

        '1. Number of concuts should be 2
        '2. If there is a data before and after the first concut, the following tags should not be present:
        '<b> or <inline Class=""smallCaps"">

        Dim matchP As Match = Regex.Match(SectionValue, "^([^\|]*)\|([^\|]*)\|(.+)")

        If matchP.Success Then
            If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                If matchP.Groups(1).Value.Contains("<b>") Or matchP.Groups(1).Value.Contains("<inline class=""smallCaps"">") Then
                    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                    .LineNo = LineNo,
                                    .ErrorMessage = "Data before 1st concut, the following tags should Not be present <b> Or <inline class=""smallCaps"">",
                                    .ErrorData = matchP.Groups(1).Value,
                                    .Remarks = elemOuterTxt})
                End If
            End If

            If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
                If matchP.Groups(2).Value.Contains("<b>") Or matchP.Groups(2).Value.Contains("<inline class=""smallCaps"">") Then
                    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                    .LineNo = LineNo,
                                    .ErrorMessage = "Data after 1st concut, the following tags should Not be present <b> Or <inline class=""smallCaps"">",
                                    .ErrorData = matchP.Groups(2).Value,
                                    .Remarks = elemOuterTxt})
                End If
            End If

        Else

            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                .LineNo = LineNo,
                .ErrorMessage = "There must be 2 concuts",
                .ErrorData = String.Format("<{0}> ", elementName),
                .Remarks = elemOuterTxt})

        End If



        ''Number of concuts should either be 2 or 3.
        ''Requirements if 2 concuts:
        ''1. If there is a data before and after the first concut, the following tags should not be present:
        ''<b> or <inline class=""smallCaps"">

        ''Requirements if 3 concuts:
        ''1. There must be a value ""1"" before the first concut.
        ''2. If there is a data before and after the second concut, the following tags should not be present:
        ''<b> or <inline class=""smallCaps"">"


        ''Dim matchP As Match = Regex.Match(SectionValue, "^(\w*)\|([^\|]*)\|([^\|]*)\|(.+)")
        ''If matchP.Success Then

        ''    If matchP.Groups(1).Value <> "1" Then
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "There must be a value ""1"" before the first concut",
        ''                        .ErrorData = matchP.Groups(1).Value,
        ''                        .Remarks = elemOuterTxt})
        ''    End If

        ''    If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
        ''        If matchP.Groups(2).Value.Contains("<b>") Or matchP.Groups(2).Value.Contains("<inline class=""smallCaps"">") Then
        ''            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                            .LineNo = LineNo,
        ''                            .ErrorMessage = "If there is a data before 2nd concut, the following tags should Not be present <b> or <inline class=""smallCaps"">",
        ''                            .ErrorData = matchP.Groups(2).Value,
        ''                            .Remarks = elemOuterTxt})
        ''        End If
        ''    End If

        ''    If Not String.IsNullOrEmpty(matchP.Groups(3).Value) Then
        ''        If matchP.Groups(3).Value.Contains("<b>") Or matchP.Groups(3).Value.Contains("<inline class=""smallCaps"">") Then
        ''            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                            .LineNo = LineNo,
        ''                            .ErrorMessage = "If there is a data after 2nd concut, the following tags should Not be present <b> or <inline class=""smallCaps"">",
        ''                            .ErrorData = matchP.Groups(3).Value,
        ''                            .Remarks = elemOuterTxt})
        ''        End If
        ''    End If

        ''Else
        ''    matchP = Regex.Match(SectionValue, "^([^\|]*)\|([^\|]*)\|(.+)")

        ''    If matchP.Success Then
        ''        If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
        ''            If matchP.Groups(1).Value.Contains("<b>") Or matchP.Groups(1).Value.Contains("<inline class=""smallCaps"">") Then
        ''                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                                .LineNo = LineNo,
        ''                                .ErrorMessage = "If there is a data before 2nd concut, the following tags should Not be present <b> or <inline class=""smallCaps"">",
        ''                                .ErrorData = matchP.Groups(1).Value,
        ''                                .Remarks = elemOuterTxt})
        ''            End If
        ''        End If

        ''        If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
        ''            If matchP.Groups(2).Value.Contains("<b>") Or matchP.Groups(2).Value.Contains("<inline class=""smallCaps"">") Then
        ''                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                                .LineNo = LineNo,
        ''                                .ErrorMessage = "If there is a data after 2nd concut, the following tags should Not be present <b> or <inline class=""smallCaps"">",
        ''                                .ErrorData = matchP.Groups(2).Value,
        ''                                .Remarks = elemOuterTxt})
        ''            End If
        ''        End If

        ''    Else

        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''            .LineNo = LineNo,
        ''            .ErrorMessage = "There must be 2 or 3 concut",
        ''            .ErrorData = String.Format("<{0}> ", elementName),
        ''            .Remarks = elemOuterTxt})

        ''    End If
        ''End If


    End Sub


    Sub ValidateContent(ByVal ContentValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)

        ContentValue = ContentValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

        'There should be no concut.
        Dim matchP As Match = Regex.Match(ContentValue, "^(\w*)\|(\w*)\|(\w*)\|(\w*)\|(.+)")

        If matchP.Success = True Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                            .LineNo = LineNo,
                            .ErrorMessage = "There should be no concut",
                            .ErrorData = ContentValue,
                            .Remarks = elemOuterTxt})
        Else
            matchP = Regex.Match(ContentValue, "^(\w*)\|(.+)")

            If matchP.Success = True Then
                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                            .LineNo = LineNo,
                            .ErrorMessage = "There should be no concut",
                            .ErrorData = ContentValue,
                            .Remarks = elemOuterTxt})
            End If
        End If


        '''Number of concuts should be 4 or 1 or none.
        '''Requirements If 4 concuts
        '''1. Value of data before the 1st concut should be I (if present).
        '''2. Value of data before the 2nd And 3rd concut should be any numbers from 0-5.
        '''3. Value of data before the 4th concut should be any numbers from 8-10.

        '''Requirement If 1 concut
        '''1. Value of data before the 1st concut should be I (if present).

        '''Dim matchP As Match = Regex.Match(ContentValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")


        ''Dim matchP As Match = Regex.Match(ContentValue, "^(\w*)\|(\w*)\|(\w*)\|(\w*)\|(.+)")

        ''If matchP.Success = True Then
        ''    If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
        ''        If Not Regex.IsMatch(matchP.Groups(1).Value, "^I$") Then
        ''            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                .LineNo = LineNo,
        ''                .ErrorMessage = "Value of data before the 1st concut should be I (if present)",
        ''                .ErrorData = matchP.Groups(1).Value,
        ''                .Remarks = elemOuterTxt})
        ''        End If
        ''    End If

        ''    Try
        ''        Dim SecondConcat As Integer = Integer.Parse(matchP.Groups(2).Value)
        ''        If SecondConcat >= 0 And SecondConcat <= 5 Then
        ''        Else
        ''            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 2nd concut should be any numbers from 0 to 5",
        ''                        .ErrorData = matchP.Groups(2).Value,
        ''                        .Remarks = elemOuterTxt})
        ''        End If
        ''                Catch ex As Exception
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 2nd concut should be any numbers from 0 to 5",
        ''                    .ErrorData = matchP.Groups(2).Value,
        ''                    .Remarks = elemOuterTxt})
        ''    End Try


        ''    Try
        ''        Dim ThirdConcat As Integer = Integer.Parse(matchP.Groups(3).Value)
        ''        If ThirdConcat >= 0 And ThirdConcat <= 5 Then
        ''        Else
        ''            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 3rd concut should be any numbers from 0 to 5",
        ''                        .ErrorData = matchP.Groups(3).Value,
        ''                        .Remarks = elemOuterTxt})

        ''        End If
        ''    Catch ex As Exception
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 3rd concut should be any numbers from 0 to 5",
        ''                    .ErrorData = matchP.Groups(3).Value,
        ''                    .Remarks = elemOuterTxt})
        ''    End Try

        ''    Try
        ''        Dim FourthConcat As Integer = Integer.Parse(matchP.Groups(4).Value)
        ''        If FourthConcat >= 8 And FourthConcat <= 10 Then
        ''        Else
        ''            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 4th concut should be any numbers from 8 to 10",
        ''                        .ErrorData = matchP.Groups(4).Value,
        ''                        .Remarks = elemOuterTxt})

        ''        End If
        ''    Catch ex As Exception
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 4th concut should be any numbers from 8 to 10",
        ''                    .ErrorData = matchP.Groups(4).Value,
        ''                    .Remarks = elemOuterTxt})
        ''    End Try


        ''Else
        ''    matchP = Regex.Match(ContentValue, "^(\w*)\|(.+)")

        ''    If matchP.Success = True Then
        ''        If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
        ''            If Not Regex.IsMatch(matchP.Groups(1).Value, "^I$") Then
        ''                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 1st concut should be I (if present)",
        ''                    .ErrorData = matchP.Groups(1).Value,
        ''                    .Remarks = elemOuterTxt})
        ''            End If
        ''        End If
        ''    End If

        ''End If

    End Sub



    Sub ValidateShortTitle(ByVal ShortTitleValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        '"There must be one concut.
        'The data before the concut should be any of the following only: A or T or S

        ShortTitleValue = ShortTitleValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")


        Dim matchP As Match = Regex.Match(ShortTitleValue, "^([^\|]*)\|(.+)")


        If matchP.Success = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "There must be 1 concut",
                    .ErrorData = String.Format("<{0}> ", elementName),
                    .Remarks = elemOuterTxt})

        End If



        If Regex.IsMatch(matchP.Groups(1).Value, "^(A|T|S)$") = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "Value of data before the 1st concut should either be A, T or S",
                    .ErrorData = matchP.Groups(1).Value,
                    .Remarks = elemOuterTxt})
        End If


    End Sub

    Sub ValidateSubSection(ByVal SubSectionValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)

        SubSectionValue = SubSectionValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

        '''Number of concuts should be 4.
        ''Dim matchP As Match = Regex.Match(SubSectionValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        'Number of concuts should be 3.
        'Value before the first concut should either be ""D"" or ""I"" or none."
        Dim matchP As Match = Regex.Match(SubSectionValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|\<(.+)")

        If matchP.Success = True Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "There must be 3 concuts",
                    .ErrorData = String.Format("<{0}> ", elementName),
                    .Remarks = elemOuterTxt})
        Else
            matchP = Regex.Match(SubSectionValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|\<(.+)")
            If matchP.Success = False Then
                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "There must be 3 concuts",
                    .ErrorData = String.Format("<{0}> ", elementName),
                    .Remarks = elemOuterTxt})
            End If
        End If



        If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
            If Not Regex.IsMatch(matchP.Groups(1).Value, "^(I|D)$") Then
                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                        .LineNo = LineNo,
                        .ErrorMessage = "Value of data before the 1st concut should either be I or D (if present)",
                        .ErrorData = matchP.Groups(3).Value,
                        .Remarks = elemOuterTxt})
            End If
        End If
    End Sub

    Sub ValidateQuotedText(ByVal QuotedText As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be "&#x201C;" before the start tag <quotedText> and "&#x201D;" after the end tag </quotedText>


    End Sub

    Sub ValidateParagraph(ByVal ParagraphValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)

        ParagraphValue = ParagraphValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

        'There must be 3 concuts.
        'Value of data before the 1st concut should either be I or D or no value.

        Dim matchP As Match = Regex.Match(ParagraphValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")
        If matchP.Success = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "There must be 3 concuts",
                    .ErrorData = String.Format("<{0}> ", elementName),
                    .Remarks = elemOuterTxt})
        End If

        Try
            If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                If Not Regex.IsMatch(matchP.Groups(1).Value, "^(I|D)$") Then
                    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                            .LineNo = LineNo,
                            .ErrorMessage = "Value of data before the 1st concut should either be I or D (if present)",
                            .ErrorData = matchP.Groups(4).Value,
                            .Remarks = elemOuterTxt})
                End If
            End If

        Catch ex As Exception

        End Try


        '''There must be 7 concuts.
        '''Value of data before the 1st and 2nd concut should be any numbers from 0-5.
        '''Value of data before the 3rd concut should be any numbers from 8-10.
        '''Value of data before the 4th concut should either be I or D (if present).
        '''Value of data before the 5th concut should be I (if present)."


        ''Dim matchP As Match = Regex.Match(ParagraphValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        ''If matchP.Success = False Then
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''            .LineNo = LineNo,
        ''            .ErrorMessage = "There must be 7 concut",
        ''            .ErrorData = String.Format("<{0}> ", elementName),
        ''            .Remarks = elemOuterTxt})
        ''End If


        ''Try
        ''    Dim FirstConcat As Integer = Integer.Parse(matchP.Groups(1).Value)

        ''    If FirstConcat >= 0 And FirstConcat <= 5 Then
        ''    Else
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 1st concut should be any numbers from 0 to 5",
        ''                        .ErrorData = matchP.Groups(1).Value,
        ''                        .Remarks = elemOuterTxt})
        ''    End If
        ''Catch ex As Exception
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 1st concut should be any numbers from 0 to 5",
        ''                    .ErrorData = matchP.Groups(1).Value,
        ''                    .Remarks = elemOuterTxt})
        ''End Try


        ''Try
        ''    Dim SecondConcat As Integer = Integer.Parse(matchP.Groups(2).Value)

        ''    If SecondConcat >= 0 And SecondConcat <= 5 Then
        ''    Else
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 2nd concut should be any numbers from 0 to 5",
        ''                        .ErrorData = matchP.Groups(2).Value,
        ''                        .Remarks = elemOuterTxt})

        ''    End If
        ''Catch ex As Exception
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 2nd concut should be any numbers from 0 to 5",
        ''                    .ErrorData = matchP.Groups(2).Value,
        ''                    .Remarks = elemOuterTxt})
        ''End Try

        ''Try
        ''    Dim ThirdConcat As Integer = Integer.Parse(matchP.Groups(3).Value)
        ''    If ThirdConcat >= 8 And ThirdConcat <= 10 Then
        ''    Else
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 3rd concut should be any numbers from 8 to 10",
        ''                        .ErrorData = matchP.Groups(3).Value,
        ''                        .Remarks = elemOuterTxt})

        ''    End If
        ''Catch ex As Exception
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 3rd concut should be any numbers from 8 to 10",
        ''                    .ErrorData = matchP.Groups(3).Value,
        ''                    .Remarks = elemOuterTxt})
        ''End Try

        ''If Not String.IsNullOrEmpty(matchP.Groups(4).Value) Then
        ''    If Not Regex.IsMatch(matchP.Groups(4).Value, "^(I|D)$") Then
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                .LineNo = LineNo,
        ''                .ErrorMessage = "Value of data before the 4th concut should either be I or D (if present)",
        ''                .ErrorData = matchP.Groups(4).Value,
        ''                .Remarks = elemOuterTxt})
        ''    End If
        ''End If

        ''If Not String.IsNullOrEmpty(matchP.Groups(5).Value) Then
        ''    If Not Regex.IsMatch(matchP.Groups(5).Value, "^I$") Then
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                .LineNo = LineNo,
        ''                .ErrorMessage = "Value of data before the 5th concut should be I (if present)",
        ''                .ErrorData = matchP.Groups(5).Value,
        ''                .Remarks = elemOuterTxt})
        ''    End If
        ''End If



    End Sub

    Sub ValidateChapeau(ByVal ChapeauValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Value must contain &#x2014; before the end tag </chapeau>

        If Not Regex.IsMatch(ChapeauValue.Trim, "(\&\#x2014;)$") Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "Value must contain ""&#x2014;"" before the end tag </chapeau>",
                                .ErrorData = ChapeauValue,
                                .Remarks = elemOuterTxt})
        End If

    End Sub

    Sub ValidateQuotedContent(ByVal QuotedContentValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be "&#x201C;" and "&#x201D;" inside <quotedContent>[data]</quotedContent>

        If QuotedContentValue.Contains("&#x201C;") And QuotedContentValue.Contains("&#x201D;") Then
        Else
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "There must be ""&#x201C;"" and ""&#x201D;"" inside <quotedContent>[data]</quotedContent>",
                                .ErrorData = QuotedContentValue,
                                .Remarks = elemOuterTxt})
        End If
    End Sub


    Sub ValidateSubParagraph(ByVal SubParagraphValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be an attribute ""class"" with value ""indent#"" fontsize#"".

        Dim strSubParagraph As String = Regex.Match(elemOuterTxt, "<subparagraph(.*?)>").Value

        If strSubParagraph.Contains("class") And strSubParagraph.Contains("indent") And strSubParagraph.Contains("fontsize") Then
        Else
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "There must be an attribute ""class"" with value ""indent#"" ""fontsize#""",
                                .ErrorData = strSubParagraph,
                                .Remarks = elemOuterTxt})
        End If

        SubParagraphValue = SubParagraphValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")


        '''There must be 4 concuts.
        '''Value of data before the 2nd concut should be I (if present)."
        ''Dim matchP As Match = Regex.Match(SubParagraphValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")


        'There must be 3 concuts.
        'Value of data before the 1st concut should either be I Or D (if present).

        Dim matchP As Match = Regex.Match(SubParagraphValue, "^(\w{0,1})\|([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")
        If matchP.Success = True Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "There must be 3 concuts",
                    .ErrorData = String.Format("<{0}> ", elementName),
                    .Remarks = elemOuterTxt})

        Else

            matchP = Regex.Match(SubParagraphValue, "^(\w{0,1})\|([^\|]*)\|([^\|]*)\|(.+)")

            If matchP.Success = False Then
                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "There must be 3 concuts",
                    .ErrorData = String.Format("<{0}> ", elementName),
                    .Remarks = elemOuterTxt})
            End If

        End If


        If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
            If Not Regex.IsMatch(matchP.Groups(1).Value, "^(I|D)$") Then
                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                        .LineNo = LineNo,
                        .ErrorMessage = "Value of data before the 1st concut should either be I or D (if present)",
                        .ErrorData = matchP.Groups(1).Value,
                        .Remarks = elemOuterTxt})
            End If
        End If

        ''If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
        ''    If Not Regex.IsMatch(matchP.Groups(2).Value, "^I$") Then
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                .LineNo = LineNo,
        ''                .ErrorMessage = "Value of data before the 2nd concut should be I (if present)",
        ''                .ErrorData = matchP.Groups(2).Value,
        ''                .Remarks = elemOuterTxt})
        ''    End If
        ''End If



    End Sub


    Sub ValidateAction(ByVal ActionValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be a word "Approved" in the data.

        If Regex.IsMatch(ActionValue, "Approved") = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "Data must contain ""Approved""",
                                .ErrorData = ActionValue,
                                .Remarks = elemOuterTxt})
        End If

    End Sub

    Sub ValidateLegislativeHistory(ByVal LegislativeHistory As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be 2 concuts.
        'Data before the 1st concut should have the word ""LEGISLATIVE HISTORY""

        LegislativeHistory = LegislativeHistory.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")


        Dim matchP As Match = Regex.Match(LegislativeHistory, "^([^\|]*)\|(.+)")
        If matchP.Success = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "There must be 1 concut",
                    .ErrorData = String.Format("<{0}> ", elementName),
                    .Remarks = elemOuterTxt})
        End If


        Dim heading As String = matchP.Groups(1).Value
        If String.IsNullOrEmpty(heading) Then
            heading = Regex.Match(LegislativeHistory, "<heading>(.*?)</heading>").Value
        End If

        If Not String.IsNullOrEmpty(heading) Then
            If Regex.IsMatch(heading, "LEGISLATIVE HISTORY") = False Then
                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "Data before the 1st concut should have the word ""LEGISLATIVE HISTORY""",
                                .ErrorData = matchP.Groups(1).Value,
                                .Remarks = elemOuterTxt})
            End If
        Else
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "Data before the 1st concut should have the word ""LEGISLATIVE HISTORY""",
                                .ErrorData = matchP.Groups(1).Value,
                                .Remarks = elemOuterTxt})


        End If



    End Sub


    Sub ValidateTitleSubTitle(ByVal TitleValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        '"There must be 2 concuts.
        'Data before the 1st concut should have the word ""TITLE"" for <title>
        'Data before the 1st concut should have the word ""SUBTITLE"" for <title>

        TitleValue = TitleValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")


        Dim matchP As Match = Regex.Match(TitleValue, "^([^\|]*)\|([^\|]*)\|(.+)")

        If matchP.Success = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "There must be 2 concut",
                                .ErrorData = String.Format("<{0}> ", elementName),
                                .Remarks = elemOuterTxt})
        End If

        If Not Regex.IsMatch(matchP.Groups(1).Value, elementName, RegexOptions.IgnoreCase) Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = String.Format("Data before the 1st concut should have the word {0}", elementName.ToUpper),
                                .ErrorData = matchP.Groups(1).Value,
                                .Remarks = elemOuterTxt})
        End If

    End Sub



    Sub ValidateClauseSubClause(ByVal ClauseValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        '"There must be 1 concut.
        'There must be an attribute ""class"" with value ""indent#"" fontsize#""."


        ClauseValue = ClauseValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

        Dim matchP As Match = Regex.Match(ClauseValue, "^([^\|]*)\|(.+)")

        If matchP.Success = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "There must be 1 concut",
                                .ErrorData = String.Format("<{0}> ", elementName),
                                .Remarks = elemOuterTxt})
        End If

        Dim elempatt As String = String.Format("<{0}(.*?)>", elementName)
        Dim strClause As String = Regex.Match(elemOuterTxt, elempatt).Value

        If strClause.Contains("class") And strClause.Contains("indent") And strClause.Contains("fontsize") Then
        Else
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "There must be an attribute ""class"" with value ""indent#"" ""fontsize#""",
                                .ErrorData = strClause,
                                .Remarks = elemOuterTxt})
        End If


    End Sub


    Sub ValidateContinuation(ByVal ContinuationValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)

        ContinuationValue = ContinuationValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

        'There must be no concut.
        Dim matchP As Match = Regex.Match(ContinuationValue, "^([^\|]*)\|(.+)")
        If matchP.Success = True Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "There must be no concut",
                                .ErrorData = String.Format("<{0}> ", elementName),
                                .Remarks = elemOuterTxt})
        End If


        ''Dim matchP As Match = Regex.Match(ContinuationValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        '''There must be 3 concuts.
        '''Value of data before the 1st And 2nd concut should be any numbers from 0-5.
        '''Value of data before the 3rd concut should be any numbers from 8-10.
        '''Dim matchP As Match = Regex.Match(ContinuationValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        ''If matchP.Success = False Then
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "There must be 3 concut",
        ''                        .ErrorData = String.Format("<{0}> ", elementName),
        ''                        .Remarks = elemOuterTxt})
        ''End If

        ''Try
        ''    Dim FirstConcat As Integer = Integer.Parse(matchP.Groups(1).Value)
        ''    If FirstConcat >= 0 And FirstConcat <= 5 Then
        ''    Else
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 1st concut should be any numbers from 0 to 5",
        ''                    .ErrorData = matchP.Groups(1).Value,
        ''                    .Remarks = elemOuterTxt})
        ''    End If
        ''Catch ex As Exception
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                .LineNo = LineNo,
        ''                .ErrorMessage = "Value of data before the 1st concut should be any numbers from 0 to 5",
        ''                .ErrorData = matchP.Groups(1).Value,
        ''                .Remarks = elemOuterTxt})
        ''End Try


        ''Try
        ''    Dim SecondConcat As Integer = Integer.Parse(matchP.Groups(2).Value)

        ''    If SecondConcat >= 0 And SecondConcat <= 5 Then
        ''    Else
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 2nd concut should be any numbers from 0 to 5",
        ''                    .ErrorData = matchP.Groups(2).Value,
        ''                    .Remarks = elemOuterTxt})

        ''    End If
        ''Catch ex As Exception
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                .LineNo = LineNo,
        ''                .ErrorMessage = "Value of data before the 2nd concut should be any numbers from 0 to 5",
        ''                .ErrorData = matchP.Groups(2).Value,
        ''                .Remarks = elemOuterTxt})
        ''End Try

        ''Try
        ''    Dim ThirdConcat As Integer = Integer.Parse(matchP.Groups(3).Value)
        ''    If ThirdConcat >= 8 And ThirdConcat <= 10 Then
        ''    Else
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 3rd concut should be any numbers from 8 to 10",
        ''                    .ErrorData = matchP.Groups(3).Value,
        ''                    .Remarks = elemOuterTxt})

        ''    End If
        ''Catch ex As Exception
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                .LineNo = LineNo,
        ''                .ErrorMessage = "Value of data before the 3rd concut should be any numbers from 8 to 10",
        ''                .ErrorData = matchP.Groups(3).Value,
        ''                .Remarks = elemOuterTxt})
        ''End Try


    End Sub


    Sub ValidateChapter(ByVal ChapterValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be 2 concuts.
        'Data before the 1st concut should have the word ""CHAPTER"" or ""CHAP""

        ChapterValue = ChapterValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")


        Dim matchP As Match = Regex.Match(ChapterValue, "^([^\|]*)\|([^\|]*)\|(.+)")
        If matchP.Success = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "There must be 2 concut",
                    .ErrorData = String.Format("<{0}> ", elementName),
                    .Remarks = elemOuterTxt})
        End If

        If Regex.IsMatch(matchP.Groups(1).Value, "(CHAPTER|CHAP)") = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "Data before the 1st concut should have the word ""CHAPTER"" or ""CHAP""",
                                .ErrorData = matchP.Groups(1).Value,
                                .Remarks = elemOuterTxt})
        End If

    End Sub

    Sub ValidateItemSubItem(ByVal SubItemValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be 1 concut

        SubItemValue = SubItemValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

        Dim matchP As Match = Regex.Match(SubItemValue, "^(\d{0,1})*\|(\d{0,2})*\|(\w)*\|(.+)")
        If matchP.Success = True Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "There must be 1 concut",
                    .ErrorData = String.Format("<{0}> ", elementName),
                    .Remarks = elemOuterTxt})

        Else
            matchP = Regex.Match(SubItemValue, "^([^\|]*)\|(.+)")

            If matchP.Success = False Then
                If Not Regex.IsMatch(SubItemValue, "^<(num|heading)[^>]*>") Then
                    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                  .LineNo = LineNo,
                  .ErrorMessage = "There must be 1 concut",
                  .ErrorData = String.Format("<{0}> ", elementName),
                  .Remarks = elemOuterTxt})
                End If


            End If

        End If



        '''There must be 4 concuts.
        '''Value of data before the 1st concut should be any numbers from 0-5.
        '''Value of data before the 2nd concut should be any numbers from 8-10.
        '''Value of data before the 3rd concut should be I (if present)."

        ''Dim matchP As Match = Regex.Match(SubItemValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")
        ''If matchP.Success = False Then
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''            .LineNo = LineNo,
        ''            .ErrorMessage = "There must be 4 concut",
        ''            .ErrorData = String.Format("<{0}> ", elementName),
        ''            .Remarks = elemOuterTxt})
        ''    matchP = Regex.Match(SubItemValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")
        ''Else
        ''    'check for excess concat
        ''    matchP = Regex.Match(SubItemValue, "^(\w*)\|(\w*)\|(\w*)\|([^\|]*)\|([^\|]*)\|(.+)")
        ''    If matchP.Success = True Then
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''            .LineNo = LineNo,
        ''            .ErrorMessage = "There must be 4 concut",
        ''            .ErrorData = String.Format("<{0}> ", elementName),
        ''            .Remarks = elemOuterTxt})
        ''    Else
        ''        matchP = Regex.Match(SubItemValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")
        ''    End If
        ''End If

        ''Try
        ''    Dim FirstConcat As Integer = Integer.Parse(matchP.Groups(1).Value)
        ''    If FirstConcat >= 0 And FirstConcat <= 5 Then
        ''    Else
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 1st concut should be any numbers from 0 to 5",
        ''                        .ErrorData = matchP.Groups(1).Value,
        ''                        .Remarks = elemOuterTxt})
        ''    End If

        ''Catch ex As Exception
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 1st concut should be any numbers from 0 to 5",
        ''                    .ErrorData = matchP.Groups(1).Value,
        ''                    .Remarks = elemOuterTxt})
        ''End Try


        ''Try
        ''    Dim SecondConcat As Integer = Integer.Parse(matchP.Groups(2).Value)
        ''    If SecondConcat >= 8 And SecondConcat <= 10 Then
        ''    Else
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 2nd concut should be any numbers from 8 to 10",
        ''                    .ErrorData = matchP.Groups(2).Value,
        ''                    .Remarks = elemOuterTxt})
        ''    End If
        ''Catch ex As Exception
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 2nd concut should be any numbers from 8 to 10",
        ''                    .ErrorData = matchP.Groups(2).Value,
        ''                    .Remarks = elemOuterTxt})

        ''End Try

        ''If Not String.IsNullOrEmpty(matchP.Groups(3).Value.Trim) Then
        ''    If matchP.Groups(3).Value <> "I" Then
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 3rd concut should be I",
        ''                        .ErrorData = matchP.Groups(2).Value,
        ''                        .Remarks = elemOuterTxt})
        ''    End If
        ''End If

    End Sub


    Sub ValidateDivision(ByVal DivisionValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be 2 concuts.
        'Data before the 1st concut should have the word ""DIV""

        DivisionValue = DivisionValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")


        Dim matchP As Match = Regex.Match(DivisionValue, "^([^\|]*)\|([^\|]*)\|(.+)")
        If matchP.Success = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "There must be 2 concut",
                    .ErrorData = String.Format("<{0}> ", elementName),
                    .Remarks = elemOuterTxt})
        End If

        If Regex.IsMatch(matchP.Groups(1).Value, "DIV") = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "Data before the 1st concut should have the word ""DIV""",
                                .ErrorData = matchP.Groups(1).Value,
                                .Remarks = elemOuterTxt})
        End If

    End Sub


    Sub ValidatePart(ByVal PartValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be 2 concuts.
        'Data before the 1st concut should have the word ""PART""

        PartValue = PartValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")


        Dim matchP As Match = Regex.Match(PartValue, "^([^\|]*)\|([^\|]*)\|(.+)")
        If matchP.Success = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "There must be 2 concut",
                    .ErrorData = String.Format("<{0}> ", elementName),
                    .Remarks = elemOuterTxt})
        End If

        If Regex.IsMatch(matchP.Groups(1).Value, "PART") = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "Data before the 1st concut should have the word ""PART""",
                                .ErrorData = matchP.Groups(1).Value,
                                .Remarks = elemOuterTxt})
        End If

    End Sub


    Sub ValidateSubPart(ByVal SubPartValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be 2 concuts.

        SubPartValue = SubPartValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")


        Dim matchP As Match = Regex.Match(SubPartValue, "^([^\|]*)\|([^\|]*)\|(.+)")
        If matchP.Success = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "There must be 2 concut",
                    .ErrorData = String.Format("<{0}> ", elementName),
                    .Remarks = elemOuterTxt})
        End If

    End Sub


    Sub ValidateDocPart(ByVal DocPartValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Value should be numeric.

        If Not Regex.IsMatch(DocPartValue, "^\d+$") Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "Data must contain numbers",
                                .ErrorData = DocPartValue,
                                .Remarks = elemOuterTxt})
        End If

    End Sub

    Sub ValidateResolvingClause(ByVal ResolvingClauseValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)

        ResolvingClauseValue = ResolvingClauseValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

        'There must be no concut.
        Dim matchP As Match = Regex.Match(ResolvingClauseValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        If matchP.Success = True Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "There must be no concut",
                                .ErrorData = String.Format("<{0}> ", elementName),
                                .Remarks = elemOuterTxt})
        Else
            If ResolvingClauseValue.Contains("|") Then
                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "There must be no concut",
                                .ErrorData = String.Format("<{0}> ", elementName),
                                .Remarks = elemOuterTxt})
            End If
        End If





        '''There must be 3 concuts.
        '''Data after the 3rd concut should have the word ""Resolved""
        '''Value of data before the 1st and 2nd concut should be any numbers from 0-5.
        '''Value of data before the 3rd concut should be any numbers from 8-10.

        ''Dim matchP As Match = Regex.Match(ResolvingClauseValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        ''If matchP.Success = False Then
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "There must be 3 concut",
        ''                        .ErrorData = String.Format("<{0}> ", elementName),
        ''                        .Remarks = elemOuterTxt})

        ''End If


        ''Try
        ''    If Regex.IsMatch(matchP.Groups(4).Value, "Resolved") = False Then
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Data after the 3rd concut should have the word ""Resolved""",
        ''                        .ErrorData = matchP.Groups(4).Value,
        ''                        .Remarks = elemOuterTxt})
        ''    End If
        ''Catch ex As Exception
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Data after the 3rd concut should have the word ""Resolved""",
        ''                        .ErrorData = matchP.Groups(4).Value,
        ''                        .Remarks = elemOuterTxt})
        ''End Try


        ''Try
        ''        Dim FirstConcat As Integer = Integer.Parse(matchP.Groups(1).Value)
        ''        If FirstConcat >= -3 And FirstConcat <= 5 Then
        ''        Else
        ''            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 1st concut should be any numbers from -3 to 5",
        ''                        .ErrorData = matchP.Groups(1).Value,
        ''                        .Remarks = elemOuterTxt})
        ''        End If
        ''    Catch ex As Exception
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 1st concut should be any numbers from -3 to 5",
        ''                    .ErrorData = matchP.Groups(1).Value,
        ''                    .Remarks = elemOuterTxt})
        ''    End Try


        ''    Try
        ''        Dim SecondConcat As Integer = Integer.Parse(matchP.Groups(2).Value)
        ''        If SecondConcat >= -3 And SecondConcat <= 5 Then
        ''        Else
        ''            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 2nd concut should be any numbers from -3 to 5",
        ''                        .ErrorData = matchP.Groups(2).Value,
        ''                        .Remarks = elemOuterTxt})

        ''        End If
        ''    Catch ex As Exception
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 2nd concut should be any numbers from -3 to 5",
        ''                        .ErrorData = matchP.Groups(2).Value,
        ''                        .Remarks = elemOuterTxt})

        ''    End Try

        ''    Try
        ''        Dim ThirdConcat As Integer = Integer.Parse(matchP.Groups(3).Value)
        ''        If ThirdConcat >= 8 And ThirdConcat <= 10 Then
        ''        Else
        ''            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 3rd concut should be any numbers from 8 to 10",
        ''                        .ErrorData = matchP.Groups(3).Value,
        ''                        .Remarks = elemOuterTxt})

        ''        End If
        ''    Catch ex As Exception
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 3rd concut should be any numbers from 8 to 10",
        ''                        .ErrorData = matchP.Groups(3).Value,
        ''                        .Remarks = elemOuterTxt})

        ''    End Try

        ''If Not Regex.IsMatch(elemOuterTxt, "\,\s?<\/resolvingClause>") Then
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''            .LineNo = LineNo,
        ''            .ErrorMessage = "Missing comma(,) before </resolvingClause>",
        ''            .ErrorData = ResolvingClauseValue,
        ''            .Remarks = elemOuterTxt})
        ''End If


    End Sub


    Sub ValidatePreamble(ByVal PreambleValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be an element/tag <recital> after <preamble> and </recital> before </preamble>

        If Regex.IsMatch(PreambleValue.Trim, "^(<recital([^>]+)?>)") And Regex.IsMatch(PreambleValue.Trim, "(</recital>)$") Then
        Else
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                        .LineNo = LineNo,
                        .ErrorMessage = "There must be an element/tag <recital> after <preamble> and </recital> before </preamble>",
                        .ErrorData = PreambleValue,
                        .Remarks = elemOuterTxt})
        End If

    End Sub

    Sub ValidateRecital(ByVal RecitalValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)


        RecitalValue = RecitalValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

        'There must be no concut.
        Dim matchP As Match = Regex.Match(RecitalValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        If matchP.Success = True Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "There must be no concut",
                                .ErrorData = RecitalValue,
                                .Remarks = elemOuterTxt})
        Else
            If RecitalValue.Contains("|") Then
                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "There must be no concut",
                                .ErrorData = RecitalValue,
                                .Remarks = elemOuterTxt})
            End If
        End If




        '''There must be 3 concuts.
        '''Value of data before the 1st And 2nd concut should be any numbers from -3 until 5.
        '''Value of data before the 2nd concut should be any numbers from 8-10.
        ''Dim matchP As Match = Regex.Match(RecitalValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        ''If matchP.Success = False Then
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "There must be 3 concut",
        ''                        .ErrorData = RecitalValue,
        ''                        .Remarks = elemOuterTxt})
        ''End If


        ''Try
        ''    Dim FirstConcat As Integer = Integer.Parse(matchP.Groups(1).Value)
        ''    If FirstConcat >= -3 And FirstConcat <= 5 Then
        ''    Else
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 1st concut should be any numbers from -3 to 5",
        ''                        .ErrorData = matchP.Groups(1).Value,
        ''                        .Remarks = elemOuterTxt})
        ''    End If
        ''Catch ex As Exception
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 1st concut should be any numbers from -3 to 5",
        ''                        .ErrorData = matchP.Groups(1).Value,
        ''                        .Remarks = elemOuterTxt})
        ''End Try

        ''Try
        ''    Dim SecondConcat As Integer = Integer.Parse(matchP.Groups(2).Value)
        ''    If SecondConcat >= -3 And SecondConcat <= 5 Then
        ''    Else
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 2nd concut should be any numbers from -3 to 5",
        ''                        .ErrorData = matchP.Groups(2).Value,
        ''                        .Remarks = elemOuterTxt})
        ''    End If
        ''Catch ex As Exception
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                        .LineNo = LineNo,
        ''                        .ErrorMessage = "Value of data before the 2nd concut should be any numbers from -3 to 5",
        ''                        .ErrorData = matchP.Groups(2).Value,
        ''                        .Remarks = elemOuterTxt})
        ''End Try



        ''Try
        ''    Dim ThirdConcat As Integer = Integer.Parse(matchP.Groups(3).Value)
        ''    If ThirdConcat >= 8 And ThirdConcat <= 10 Then
        ''    Else
        ''        errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 3rd concut should be any numbers from 8 to 10",
        ''                    .ErrorData = matchP.Groups(3).Value,
        ''                    .Remarks = elemOuterTxt})
        ''    End If
        ''Catch ex As Exception
        ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
        ''                    .LineNo = LineNo,
        ''                    .ErrorMessage = "Value of data before the 3rd concut should be any numbers from 8 to 10",
        ''                    .ErrorData = matchP.Groups(3).Value,
        ''                    .Remarks = elemOuterTxt})
        ''End Try

    End Sub

    Sub ValidateSignature(ByVal SignatureValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be 1 concut.

        SignatureValue = SignatureValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

        Dim matchP As Match = Regex.Match(SignatureValue, "^([^\|]+)\|(.+)")

        Dim concatCount As MatchCollection = Regex.Matches(SignatureValue, "\|")

        If concatCount.Count <> 1 Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "There must be 1 concut",
                                .ErrorData = SignatureValue,
                                .Remarks = elemOuterTxt})

        End If

    End Sub


    Sub ValidateEnactingFormula(ByVal EnactingFormulaValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Data should have a word "enacted" or "enactment".

        If Not Regex.IsMatch(EnactingFormulaValue, "(enacted|enactment)") Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "Data should have the word ""enacted"" or ""enactment""",
                    .ErrorData = EnactingFormulaValue,
                    .Remarks = elemOuterTxt})
        End If

        If Regex.IsMatch(elemOuterTxt, "\,\s?(<[^>]+>)?<\/enactingFormula>") Then
        Else

            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                    .LineNo = LineNo,
                    .ErrorMessage = "Missing comma(,) before </enactingFormula>",
                    .ErrorData = EnactingFormulaValue,
                    .Remarks = elemOuterTxt})
        End If



    End Sub

    Sub ValidateLevel(ByVal LevelValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be 3 concuts.
        'Value of data before the 1st concut should be I (if present).

        LevelValue = LevelValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")


        Dim matchP As Match = Regex.Match(LevelValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")
        If matchP.Success = True Then
            If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                If matchP.Groups(1).Value.Trim <> "I" Then
                    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                        .LineNo = LineNo,
                                        .ErrorMessage = "Value of data before the 1st concut should be I (if present)",
                                        .ErrorData = matchP.Groups(1).Value,
                                        .Remarks = elemOuterTxt})
                End If
            End If

            'check if concat is excess
            matchP = Regex.Match(LevelValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")
            If matchP.Success = True Then
                errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "There must be 3 concut",
                                .ErrorData = String.Format("<{0}> ", elementName),
                                .Remarks = elemOuterTxt})
            End If
        Else
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                            .LineNo = LineNo,
                            .ErrorMessage = "There must be 3 concut",
                            .ErrorData = String.Format("<{0}> ", elementName),
                            .Remarks = elemOuterTxt})
        End If

    End Sub

    Sub ValidateArticle(ByVal ArticleValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be 2 concuts.
        'Data before the 1st concut should have the word ""article"" Or ""art.""

        ArticleValue = ArticleValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")


        Dim matchP As Match = Regex.Match(ArticleValue, "^([^\|]*)\|([^\|]*)\|(.+)")
        If matchP.Success = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                     .LineNo = LineNo,
                     .ErrorMessage = "There must be 2 concut",
                     .ErrorData = ArticleValue,
                     .Remarks = elemOuterTxt})
        End If


        matchP = Regex.Match(ArticleValue, "^([^\|]*)\|(.+)")

        If Regex.IsMatch(matchP.Groups(1).Value, "(article|art\.)", RegexOptions.IgnoreCase) = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                 .LineNo = LineNo,
                 .ErrorMessage = "Data before the 1st concut should have the word ""article"" Or ""art.""",
                 .ErrorData = matchP.Groups(1).Value,
                 .Remarks = elemOuterTxt})
        End If


    End Sub


    Sub ValidateProviso(ByVal ProvisoValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Data should have a word "Provided further" or "provided".

        If Regex.IsMatch(ProvisoValue, "(Provided further|provided)", RegexOptions.IgnoreCase) = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "Data should have a word ""Provided further"" Or ""provided""",
                                .ErrorData = ProvisoValue,
                                .Remarks = elemOuterTxt})
        End If
    End Sub

    Sub ValidateFillin(ByVal FillinValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Value should be numeric.
        If Not Regex.IsMatch(FillinValue.Trim, "^\d+$") Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                     .LineNo = LineNo,
                     .ErrorMessage = "Data must contain numbers",
                     .ErrorData = FillinValue,
                     .Remarks = elemOuterTxt})
        End If
    End Sub

    Sub ValidateBlock(ByVal BlockValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be elements/tags inside <block>.

        If Regex.IsMatch(BlockValue, "<[^>]+>") = False Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                     .LineNo = LineNo,
                     .ErrorMessage = "There must be elements/tags inside <block>",
                     .ErrorData = BlockValue,
                     .Remarks = elemOuterTxt})
        End If

    End Sub

    Sub ValidateFootnote(ByVal FootnoteValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be one concut.

        FootnoteValue = FootnoteValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")


        Dim matchP As Match = Regex.Match(FootnoteValue, "^([^\|]+)\|(.+)")
        Dim concatCount As MatchCollection = Regex.Matches(FootnoteValue, "\|")

        If concatCount.Count <> 1 Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                .LineNo = LineNo,
                                .ErrorMessage = "There must be 1 concut",
                                .ErrorData = FootnoteValue,
                                .Remarks = elemOuterTxt})
        End If
    End Sub


    Sub ValidateAppropriations(ByVal AppropriationsValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'There must be 2 concuts.
        'Value before the 1st concut should by any of the following: M or I or S

        AppropriationsValue = AppropriationsValue.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")


        Dim matchP As Match = Regex.Match(AppropriationsValue, "^([A-Z])\|([^\|]+)\|\<(.+)")

        If matchP.Success = True Then
            '''check for excess concat
            ''matchP = Regex.Match(AppropriationsValue, "^([A-Z])\|([^\|]+)\|([^\|]+)\|\<(.+)")
            ''If matchP.Success = True Then
            ''    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
            ''         .LineNo = LineNo,
            ''         .ErrorMessage = "There must be 2 concut",
            ''         .ErrorData = AppropriationsValue,
            ''         .Remarks = elemOuterTxt})
            ''Else
            ''    matchP = Regex.Match(AppropriationsValue, "^([^\|]+)\|(.+)")
            ''    If matchP.Success = True Then
            ''        If matchP.Groups(2).Value.Contains("|") = False Then
            ''            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
            ''                .LineNo = LineNo,
            ''                .ErrorMessage = "There must be 2 concut",
            ''                .ErrorData = AppropriationsValue,
            ''                .Remarks = elemOuterTxt})
            ''        End If
            ''    End If

            ''End If
        Else
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                     .LineNo = LineNo,
                     .ErrorMessage = "There must be 2 concut",
                     .ErrorData = AppropriationsValue,
                     .Remarks = elemOuterTxt})
        End If


        matchP = Regex.Match(AppropriationsValue, "^([^\|]+)\|(.+)")
        If Not Regex.IsMatch(matchP.Groups(1).Value.Trim, "^(M|I|S)$") Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                 .LineNo = LineNo,
                 .ErrorMessage = "The data before the concut should be any of the following only: M, I, S",
                 .ErrorData = matchP.Groups(1).Value,
                 .Remarks = elemOuterTxt})
        Else
            Dim heading As String = matchP.Groups(2).Value
            If heading.Contains("|") Then
                heading = heading.Substring(0, heading.IndexOf("|"))
            End If

            '*** 1. <inline class="smallCaps"> in the <heading> of <appropriations> is not allowed.
            '2. Here are the proper casing of <heading> in <appropriations> per level. Flag as error if not.
            '  - M: casing should be in UPPERCASE
            '  - I: casing should be in UpperLower
            '  - S: casing should be in small


            If Not String.IsNullOrEmpty(heading) Then
                If heading.Contains("<inline class=""smallCaps"">") Then
                    errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                         .LineNo = LineNo,
                         .ErrorMessage = "<inline class=""smallCaps""> in the <heading> of <appropriations> is not allowed",
                         .ErrorData = heading,
                         .Remarks = elemOuterTxt})

                End If

                heading = Regex.Replace(heading, "<[^>]+>", "").Trim

                Select Case matchP.Groups(1).Value.Trim
                    Case "M"
                        If heading <> heading.ToUpper Then
                            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                     .LineNo = LineNo,
                                     .ErrorMessage = "Incorrect casing of <heading> in <appropriations> - M: casing should be in UPPERCASE",
                                     .ErrorData = heading,
                                     .Remarks = elemOuterTxt})
                        End If

                    Case "S"
                        If heading <> heading.ToLower Then
                            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                 .LineNo = LineNo,
                                 .ErrorMessage = "Incorrect casing of <heading> in <appropriations> - S: casing should be in small",
                                 .ErrorData = heading,
                                 .Remarks = elemOuterTxt})

                        End If
                    Case Else
                        If heading = heading.ToUpper Or heading = heading.ToLower Then
                            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                     .LineNo = LineNo,
                                     .ErrorMessage = "Incorrect casing of <heading> in <appropriations> - I: casing should be in UpperLower",
                                     .ErrorData = heading,
                                     .Remarks = elemOuterTxt})
                        End If

                End Select
            End If


        End If

    End Sub


    Sub ValidateLayout(ByVal LayoutValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)

        If Regex.IsMatch(elemOuterTxt, "<row([^>]*)>") And Regex.IsMatch(elemOuterTxt, "<column([^>]*)>") Then
        Else
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                 .LineNo = LineNo,
                                 .ErrorMessage = "Should contain <row> and <column> elements/tags",
                                 .ErrorData = String.Format("<{0}>", elementName),
                                 .Remarks = elemOuterTxt})
        End If

    End Sub


    Sub ValidateImg(ByVal ImgValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        'Data should contain ".jpg"

        If Not Regex.IsMatch(ImgValue, "\.jpg", RegexOptions.IgnoreCase) Then
            errorReportList.Add(New ValidationReportModel() With {.Element = elementName,
                                 .LineNo = LineNo,
                                 .ErrorMessage = "Data should contain .jpg",
                                 .ErrorData = ImgValue,
                                 .Remarks = elemOuterTxt})
        End If

    End Sub


    Sub ValidatePageSequnce(ByVal PageValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String)
        Dim pgMatch As Match = Regex.Match(PageValue, "(\d+)\sSTAT\.\s(.+)")
        If pgMatch.Success = True Then
            If collPages.Count = 0 Then
                'collPages.Add(pgMatch.Groups(2).Value)
            Else
                If collPages.Contains(PageValue) Then
                    AddErrorMessageValidation(LineNo,
                                                elementName,
                                                "Duplicate <page> value",
                                                PageValue,
                                                elemOuterTxt)
                Else
                    Try
                        Dim tmpLst As Integer
                        Dim tmpCur As Integer

                        If Regex.IsMatch(lastPage.Trim, "^\d+$") Then
                            tmpLst = CInt(lastPage)
                        Else
                            Dim pgLst As Match = Regex.Match(lastPage, "\w+(\-|\—|\–|\&\#x2013;|\&\#x2014;)(\d+)")
                            If pgLst.Success Then
                                tmpLst = CInt(pgLst.Groups(2).Value)
                            End If
                        End If

                        If Regex.IsMatch(pgMatch.Groups(2).Value.Trim, "^\d+$") Then
                            tmpCur = CInt(pgMatch.Groups(2).Value)
                        Else
                            Dim pgCur As Match = Regex.Match(pgMatch.Groups(2).Value.Trim, "\w+(\-|\—|\–|\&\#x2013;|\&\#x2014;)(\d+)")
                            If pgCur.Success Then
                                tmpCur = CInt(pgCur.Groups(2).Value)
                            End If
                        End If

                        If (tmpLst + 1) = tmpCur Then
                        Else
                            'check if tmpLst is odd
                            If tmpLst Mod 2 = 1 Then
                            Else

                            End If

                            AddErrorMessageValidation(LineNo,
                                                        elementName,
                                                        "Illogical page sequence. Last page is " & collPages.Item(collPages.Count - 1),
                                                        PageValue,
                                                        elemOuterTxt)


                        End If

                    Catch ex As Exception

                    End Try
                End If
            End If
            collPages.Add(PageValue)
            lastPage = pgMatch.Groups(2).Value.Trim
        End If

    End Sub

    Sub ValidatePageXML(ByVal PageValue As String, ByVal elementName As String, ByVal LineNo As Integer, ByVal elemOuterTxt As String, ByVal pgAtt As String)
        If String.IsNullOrEmpty(pgAtt) = True Then
            If String.IsNullOrEmpty(elemOuterTxt) = False Then
                AddErrorMessageValidation(LineNo,
                                          elementName,
                                          "Missing identifier attribute in <page>",
                                           "",
                                           elemOuterTxt)
            End If

        Else
            '*** value should have an attribute identifier="/us/stat/#1/#2"
            'where "us" and "stat" are default.
            'Value of #1 should be the 1st number before the text "STAT. and it should match across all <page> tag in the same xml,
            'while the value of #2 should be the text after "STAT."
            ' - "STAT." Is default including the period. Flag as error if Not.

            Dim pgMatch As Match = Regex.Match(PageValue, "(\d+)\sSTAT\.\s(.+)")
            Dim pg() As String = pgAtt.Split("/")
            If pgAtt.StartsWith("/us/stat/") = False Then
                AddErrorMessageValidation(LineNo,
                                            elementName,
                                            "Value of identifier attribute in <page> should starts with /us/stat/",
                                            pgAtt,
                                            elemOuterTxt)
            End If

            If String.IsNullOrEmpty(pageIdentifier) Then
                pageIdentifier = pgMatch.Groups(1).Value.Trim
            Else
                If pageIdentifier <> pgMatch.Groups(1).Value.Trim Then
                    AddErrorMessageValidation(LineNo,
                                            elementName,
                                            "Value 1st number before the text ""STAT."" should match across all <page> tag in the same xml",
                                            pageIdentifier,
                                            elemOuterTxt)
                End If
                If pgMatch.Groups(1).Value.Trim <> pg(3) Then
                    AddErrorMessageValidation(LineNo,
                                            elementName,
                                            "Value of 1st number should match with 1st number before the text ""STAT.""",
                                            pgAtt,
                                            elemOuterTxt)
                End If
                'If pgMatch.Groups(2).Value.Replace("–", "-").Replace("&#x2013;", "-").Replace("—", "-").Trim <> pg(4).Replace("–", "-").Replace("&#x2013;", "-").Trim Then
                If pgMatch.Groups(2).Value.Trim <> pg(4).Trim Then
                    AddErrorMessageValidation(LineNo,
                                            elementName,
                                            "Value of 2nd number should match with 2nd number before the text ""STAT.""",
                                            pgAtt,
                                            elemOuterTxt)
                End If

                If PageValue.Contains("STAT.") = False Then
                    AddErrorMessageValidation(LineNo,
                                            elementName,
                                            "Missing ""STAT."" text at <page> value",
                                            PageValue,
                                            elemOuterTxt)
                End If

            End If
        End If

    End Sub

    Sub ValidateDash(ByVal ElemValue As String, ByVal LineNo As Integer, ByVal blnWords As Boolean)
        Dim txtline As String = ElemValue

        Dim dashColl As MatchCollection = Regex.Matches(txtline, "\d{4}(\–|\—)\d{2}(\–|\—)\d{2}")
        '1. Dates with yyyy-mm-dd format.
        If dashColl.Count = 0 Then
        Else
            For Each ds As Match In dashColl
                AddErrorMessageValidation(LineNo,
                                        "DASH",
                                        "Check usage of dash at dates with yyyy-mm-dd format.",
                                        ds.Value,
                                        ElemValue)
            Next
        End If


        '2. In between two numbers except in dates with yyyy-mm-dd format.
        Dim tmp As String = Regex.Replace(txtline, "\d{4}\-\d{2}(\-\d{2})?", "")
        If Not String.IsNullOrEmpty(tmp) Then
            dashColl = Regex.Matches(tmp, "\d+(\-|\—|\–|\&\#x2013;|\&\#x2014;)\d+")
            If dashColl.Count = 0 Then
            Else
                For Each ds As Match In dashColl
                    If ds.Groups(1).Value = "–" Or ds.Groups(1).Value = "&#x2013;" Then
                    Else
                        AddErrorMessageValidation(LineNo,
                                        "DASH",
                                        "Check usage of dash at number range should be ""–"" or &#x2013;",
                                        ds.Value,
                                        ElemValue)
                    End If
                Next
            End If
        End If


        '1. Before end tag
        '2. After end tag
        '3. Before start tag
        '4. In between two words inside the following elements:
        '<toc>
        '<listOfBillsEnacted>
        '<listOfPublicLaws>
        '<listOfPrivateLaws>
        '<listOfConcurrentResolutions>
        '<listOfProclamations>
        dashColl = Regex.Matches(txtline, "\w*(\-|\—|\–|\&\#x2013;|\&\#x2014;)<[^>]+>")
        If dashColl.Count = 0 Then
        Else
            For Each ds As Match In dashColl
                If ds.Groups(1).Value = "—" Or ds.Groups(1).Value = "&#x2014;" Then
                Else
                    AddErrorMessageValidation(LineNo,
                                        "DASH",
                                        "Check usage of dash at data before tag should be ""—"" or &#x2014;",
                                        ds.Value,
                                        ElemValue)
                End If
            Next
        End If

        dashColl = Regex.Matches(txtline, "\w+<\/[^>]+>(\-|\—|\–|\&\#x2013;|\&\#x2014;)\w*")
        If dashColl.Count = 0 Then
        Else
            For Each ds As Match In dashColl
                If ds.Groups(1).Value = "—" Or ds.Groups(1).Value = "&#x2014;" Then
                Else
                    AddErrorMessageValidation(LineNo,
                                        "DASH",
                                        "Check usage of dash at data after end tag should be ""—"" or &#x2014;",
                                        ds.Value,
                                        ElemValue)
                End If
            Next
        End If

        If blnWords = True Then
            dashColl = Regex.Matches(txtline, "[a-z]+(\-|\—|\–|\&\#x2013;|\&\#x2014;)[a-z]+", RegexOptions.IgnoreCase)
            If dashColl.Count = 0 Then
            Else
                For Each ds As Match In dashColl
                    If ds.Groups(1).Value = "—" Or ds.Groups(1).Value = "&#x2014;" Then
                    Else
                        AddErrorMessageValidation(LineNo,
                                        "DASH",
                                        "Check usage of dash in between two words ""—"" or &#x2014;",
                                        ds.Value,
                                        ElemValue)
                    End If
                Next
            End If

        End If


    End Sub



    Sub ValidateGarbageChar(ByVal ElemValue As String, ByVal LineNo As Integer)

        Dim txtline As String = ElemValue

        txtline = Regex.Replace(txtline, "<[^>]+>", "")

        For intCharPos As Integer = 0 To (txtline.Length - 1)
            Dim strxChar As String = txtline.Substring(intCharPos, 1)

            If IsGarbageChar(strxChar) Then
                AddErrorMessageValidation(LineNo,
                                            "GarbageChar",
                                            String.Format("Garbage character found - {0}", strxChar),
                                            txtline,
                                            ElemValue)
            End If
        Next

    End Sub

    Private Function IsGarbageChar(ByVal xChar As String) As Boolean
        Dim bln As Boolean = False
        If Regex.IsMatch(xChar, "[^\x00-\x7F]", RegexOptions.IgnoreCase) = True Then

            Try
                Dim intDecVal As Integer = AscW(xChar)

                If (intDecVal >= 0 And intDecVal <= 1327) Or
                (intDecVal >= 8192 And intDecVal <= 8303) Or
                (intDecVal >= 8352 And intDecVal <= 8399) Or
                (intDecVal >= 8448 And intDecVal <= 8959) Or
                (intDecVal >= 9472 And intDecVal <= 11263) Then

                    bln = False
                Else
                    bln = True
                End If

            Catch ex As Exception
                bln = True

            End Try

        End If

        Return bln
    End Function


End Module





'Sub ValidatePerLine(ByVal lines() As String)
'    Dim intLine As Integer = 0
'    Dim IsResolution As Boolean = False
'    Dim IsPresidentialDoc As Boolean = False
'    Dim IsPlaw As Boolean = False
'    Dim dcDate As String = String.Empty
'    Dim dcTitle As String = String.Empty
'    Dim dcTitleMatch As Match = Nothing
'    Dim docNumberMeta As String = String.Empty
'    Dim docNumberPref As String = String.Empty
'    Dim prevLine As String = String.Empty
'    Dim intDcType As Integer = 0
'    Dim IsMeta As Boolean = False
'    Dim IsPreface As Boolean = False
'    Dim congressMeta As String = String.Empty
'    Dim congressPref As String = String.Empty



'    For Each line In lines
'        intLine += 1

'        If line.Contains("<pLaw>") Then
'            IsPlaw = True
'        End If
'        If line.Contains("<resolution>") Then
'            IsResolution = True
'        End If
'        If line.Contains("<meta>") Then
'            IsMeta = True
'            intDcType = 0 : docNumberMeta = String.Empty
'        End If
'        If line.Contains("<preface>") Then
'            IsPreface = True
'            intDcType = 0 : docNumberPref = String.Empty
'        End If
'        If line.Contains("<presidentialDoc") Then
'            IsPresidentialDoc = True
'        End If

'        If line.Contains("<dc:date>") Then
'            dcDate = Regex.Match(line, "<dc\:date>([^\<]+)<\/dc\:date>").Groups(1).Value
'        End If

'        '******DC:TITLE********
'        If line.Contains("<dc:title>") Then
'            dcTitle = Regex.Match(line, "<dc\:title>(.*?)<\/dc\:title>").Groups(1).Value

'            If IsPlaw = True Or granuleType = "pl" Or granuleType = "pv" Then
'                '1. <dc:title> should have the format "Public Law [nn]–[nn]: " at the beginning of the data.
'                dcTitleMatch = Regex.Match(dcTitle, "(Public Law \d+(\-|\&\#x2013;)\d+):(.+)")
'                If dcTitleMatch.Success = False Then
'                    errorReportList.Add(New ValidationReportModel() With {.Element = "dc:title",
'                                 .LineNo = intLine,
'                                 .ErrorMessage = "In <plaw>, <dc:title> should have the format 'Public Law [nn]–[nn]:' at the beginning of the data",
'                                 .ErrorData = line,
'                                 .Remarks = line})
'                End If

'            ElseIf IsResolution = True Or granuleType = "sc" Or granuleType = "hc" Then
'                '1. <dc:title> should have the format "H. Con. Res. [nn]: " or "S. Con. Res. [nn]: " at the beginning of the data.
'                dcTitleMatch = Regex.Match(dcTitle, "((H|S)\.\sCon\.\sRes\.\s\d+):(.+)")

'                If dcTitleMatch.Success = False Then
'                    errorReportList.Add(New ValidationReportModel() With {.Element = "dc:title",
'                                 .LineNo = intLine,
'                                 .ErrorMessage = "In <resolution>, <dc:title> should have the format 'H. Con. Res. [nn]:' or 'S. Con. Res. [nn]:' at the beginning of the data",
'                                 .ErrorData = line,
'                                 .Remarks = line})
'                End If
'            End If
'        End If
'        '*******************

'        If line.Contains("<docNumber>") Then
'            If IsMeta = True Then
'                docNumberMeta = Regex.Match(line, "<docNumber>(.*?)</docNumber>").Value
'            ElseIf IsPreface = True Then
'                docNumberPref = Regex.Match(line, "<docNumber>(.*?)</docNumber>").Value
'            End If
'        End If

'        '******CONGRESS********
'        If line.Contains("<congress>") Then
'            Dim tmpCongress As String = String.Empty
'            If IsMeta = True Then
'                'Value of <congress> within <meta> should be the same across the XML file and value should be numeric only.
'                If String.IsNullOrEmpty(congressMeta) Then
'                    congressMeta = Regex.Match(line, "<congress>([^\<]+)</congress>").Groups(1).Value.Trim
'                    If Regex.IsMatch(congressMeta, "^\d+$") = False Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "congress",
'                                 .LineNo = intLine,
'                                 .ErrorMessage = "<congress> in <meta> should be numeric only",
'                                 .ErrorData = line,
'                                 .Remarks = line})

'                    End If
'                Else
'                    tmpCongress = Regex.Match(line, "<congress>([^\<]+)</congress>").Groups(1).Value
'                    If congressMeta <> tmpCongress Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "congress",
'                                 .LineNo = intLine,
'                                 .ErrorMessage = "<congress> in <meta> should be the same across the XML file/s",
'                                 .ErrorData = line,
'                                 .Remarks = line})
'                    End If
'                End If

'            ElseIf IsPreface = True Then
'                'Value of <congress> in <preface> should be the same across the XML file
'                'and it should have an attribute "value" where data should be numeric
'                'and the same as the data in <meta> and the number in <congress> element.
'                If String.IsNullOrEmpty(congressPref) Then
'                    Dim congMatch As Match = Regex.Match(line, "<congress( value=""([^""]+)"")?>([^\<]+)</congress>")
'                    congressPref = congMatch.Value

'                    If String.IsNullOrEmpty(congMatch.Groups(1).Value) Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "congress",
'                                 .LineNo = intLine,
'                                 .ErrorMessage = "Misssing <congress> value attribute in <preface>",
'                                 .ErrorData = line,
'                                 .Remarks = line})
'                    Else
'                        If Regex.IsMatch(congMatch.Groups(1).Value, "^\d+$") = False Then
'                            errorReportList.Add(New ValidationReportModel() With {.Element = "congress",
'                                 .LineNo = intLine,
'                                 .ErrorMessage = "<congress> value attribute data in <preface> should be numeric only",
'                                 .ErrorData = line,
'                                 .Remarks = line})
'                        End If
'                    End If

'                    If Regex.IsMatch(congMatch.Groups(2).Value, congMatch.Groups(3).Value & "(th|st|nd|rd)") = False Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "congress",
'                                 .LineNo = intLine,
'                                 .ErrorMessage = "<congress> value attribute data in <preface> should be the same with <congress> data",
'                                 .ErrorData = line,
'                                 .Remarks = line})
'                    End If

'                    If congMatch.Groups(1).Value <> congressMeta Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "congress",
'                                 .LineNo = intLine,
'                                 .ErrorMessage = "<congress> value attribute data in <preface> should be the same with <congress> in <meta>",
'                                 .ErrorData = line,
'                                 .Remarks = line})

'                    End If

'                Else
'                    tmpCongress = Regex.Match(line, "<congress[^>]*>([^\<]+)</congress>").Value
'                    If tmpCongress <> congressPref Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "congress",
'                                 .LineNo = intLine,
'                                 .ErrorMessage = "<congress> in <preface> should be the same across the XML file/s",
'                                 .ErrorData = line,
'                                 .Remarks = line})
'                    End If
'                End If
'            End If

'            If IsPlaw = True Or granuleType = "pl" Or granuleType = "pv" Then
'                'In <pLaw>, value should be the same as the 1st number appear in <dc:title>
'                If Regex.IsMatch(dcTitle, "^(Public|Private)\sLaw\s" & congressMeta & "(\-|\&\#x2013;)\d+\:") = False Then
'                    errorReportList.Add(New ValidationReportModel() With {.Element = "docNumber",
'                             .LineNo = intLine,
'                             .ErrorMessage = "In <pLaw>, <congress> in <meta> should match in the 1st number in <dc:title> after the word 'Public Law' or 'Private Law'",
'                             .ErrorData = line,
'                             .Remarks = line})
'                End If
'            End If
'        End If
'        '****************

'        If line.Contains("<officialTitle>") Then
'            '3. Data in <dc:title> after the format "H. Con. Res. [nn]: " or "S. Con. Res. [nn]: " should be the same as the data in <officialTitle>.

'            Dim offTitle As String = Regex.Match(line, "<officialTitle>(.*?)</officialTitle>").Groups(1).Value
'            If Not IsNothing(dcTitleMatch) Then
'                If dcTitleMatch.Success = True Then
'                    If offTitle.Trim = dcTitleMatch.Groups(3).Value Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "officialTitle",
'                                 .LineNo = intLine,
'                                 .ErrorMessage = "Data in <dc:title> after : should be the same as the data in <officialTitle>",
'                                 .ErrorData = line,
'                                 .Remarks = line})
'                    End If
'                Else
'                    errorReportList.Add(New ValidationReportModel() With {.Element = "officialTitle",
'                                 .LineNo = intLine,
'                                 .ErrorMessage = "Unable to validate <officialTitle> against <dc:title>",
'                                 .ErrorData = line,
'                                 .Remarks = line})
'                End If
'            End If
'        End If


'        If line.Contains("</officialTitle>") Or prevLine.Contains("</officialTitle>") Then
'            If line.Contains("<sidenote>") Then
'                Dim sideNote As String = Regex.Match(line, "<sidenote>(.*?)</sidenote>").Value

'                If IsResolution = True Then
'                    If sideNote.Contains(dcDate) = False Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "sidenote",
'                                 .LineNo = intLine,
'                                 .ErrorMessage = "Value of <dc:date> in <resolution> should be the same as the date in <sidenote>",
'                                 .ErrorData = line,
'                                 .Remarks = line})
'                    End If
'                    If sideNote.Contains("</approvedDate>") Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "sidenote",
'                                 .LineNo = intLine,
'                                 .ErrorMessage = "In <resolution>, date in <sidenote> after </officialTitle> should not be tagged inside <approvedDate>",
'                                 .ErrorData = line,
'                                 .Remarks = line})
'                    End If
'                End If

'                If sideNote.Contains(dcTitleMatch.Groups(1).Value) = False Then
'                    '2. The beginning of the data in <dc:title> should be present in the <sidenote> after the <officialTitle>.
'                    errorReportList.Add(New ValidationReportModel() With {.Element = "sidenote",
'                             .LineNo = intLine,
'                             .ErrorMessage = "The beginning of the data in <dc:title> should be present in the <sidenote> after the <officialTitle>",
'                             .ErrorData = line,
'                             .Remarks = line})
'                End If

'            End If
'        End If

'        '*******DC:TYPE*********
'        '***1. There must be only one <dc:type> inside <meta> and inside <preface>.
'        '   <dc:type> is mandatory inside <meta> for <pLaw> and <resolution>,
'        '   while mandatory in <preface> for <presidentialDoc>.
'        If line.Contains("</meta>") Then
'            IsMeta = False
'            If IsPlaw = True Or IsResolution = True Then
'                If intDcType <> 1 Then
'                    errorReportList.Add(New ValidationReportModel() With {.Element = "dc:type",
'                         .LineNo = intLine,
'                         .ErrorMessage = "Only one <dc:type> is mandatory inside <meta> for <pLaw> and <resolution>",
'                         .ErrorData = line,
'                         .Remarks = line})
'                End If
'            End If
'            intDcType = 0
'        End If

'        If line.Contains("</preface>") Then
'            IsPreface = False
'            If IsPresidentialDoc = True Then
'                If intDcType <> 0 Then
'                    errorReportList.Add(New ValidationReportModel() With {.Element = "dc:type",
'                         .LineNo = intLine,
'                         .ErrorMessage = "Only one <dc:type> is mandatory inside <preface> for <presidentialDoc>",
'                         .ErrorData = line,
'                         .Remarks = line})
'                End If
'            End If
'            intDcType = 0
'        End If
'        '******
'        If line.Contains("<dc:type>") Then
'            Dim dcType As String = Regex.Match(line, "<dc:type>([^<]+)</dc:type>").Groups(1).Value

'            If IsPlaw = True Or granuleType = "pl" Or granuleType = "pv" Then
'                '2. In <pLaw>, <dc:type> should either be "Public Law" or "Private Law"
'                'and should match in the first data in the <dc:title>.

'                If Not Regex.IsMatch(dcType, "(Public|Private) Law") Then
'                    errorReportList.Add(New ValidationReportModel() With {.Element = "dc:type",
'                         .LineNo = intLine,
'                         .ErrorMessage = "In <pLaw>, <dc:type> should either be 'Public Law' or 'Private Law'",
'                         .ErrorData = line,
'                         .Remarks = line})
'                End If

'                If Regex.IsMatch(dcTitle, "^" & Regex.Escape(dcType) & "\s") = False Then
'                    errorReportList.Add(New ValidationReportModel() With {.Element = "dc:type",
'                         .LineNo = intLine,
'                         .ErrorMessage = "In <pLaw>, first data in <dc:title> should match value of <dc:type>",
'                         .ErrorData = line,
'                         .Remarks = line})
'                End If

'            ElseIf IsResolution = True Then
'                '3. In <resolution>, if the <dc:title> starts in "S. Con. Res.", value of <dc:type> should be "Senate Concurrent Resolution".
'                'However, if the value of <dc:title> starts in "H. Con. Res.", value of <dc:type> should be "House Concurrent Resolution".

'                If Regex.IsMatch(dcTitle, Regex.Escape("S. Con. Res.")) Then
'                    If dcType <> "Senate Concurrent Resolution" Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "dc:type",
'                             .LineNo = intLine,
'                             .ErrorMessage = " In <resolution>, if the <dc:title> starts in 'H. Con. Res.', <dc:type> value should be 'House Concurrent Resolution'",
'                             .ErrorData = line,
'                             .Remarks = line})
'                    End If
'                ElseIf Regex.IsMatch(dcTitle, "H\. Con\. Res\.") Then
'                    If dcType <> "House Concurrent Resolution" Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "dc:type",
'                             .LineNo = intLine,
'                             .ErrorMessage = " In <resolution>, if the <dc:title> starts in 'H. Con. Res.', <dc:type> value should be 'House Concurrent Resolution'",
'                             .ErrorData = line,
'                             .Remarks = line})
'                    End If
'                End If

'            ElseIf IsPresidentialDoc = True Or granuleType = "pr" Then
'                '4. In <presidentialDoc>, if the <docNumber> in <preface> has a word "Proclamation" at the beginning of the
'                'data, <dc:type> should be "A Proclamation".
'                If IsPreface = True Then
'                    If docNumberPref.StartsWith("Proclamation") = True Then
'                        If dcType <> "A Proclamation" Then
'                            errorReportList.Add(New ValidationReportModel() With {.Element = "dc:type",
'                                     .LineNo = intLine,
'                                     .ErrorMessage = "In <presidentialDoc>, If the <docNumber> in <preface> has a word 'Proclamation' at the beginning of the data, <dc: Type> should be 'A Proclamation'",
'                                     .ErrorData = line,
'                                     .Remarks = line})
'                        End If
'                    End If
'                End If
'            End If

'            intDcType += 1
'        End If

'        '*** In <presidentialDoc>, the date in <docNumber> should be tagged in element <date>
'        'with attributes.<date date = "yyyy-mm-dd" > [date]</Date> tag as error if Not.
'        If IsPresidentialDoc = True Then
'            If line.Contains("<docNumber> ") Then
'                Dim docNo As String = Regex.Match(line, "<docNumber>(.*?)</docNumber>").Value
'                If Regex.IsMatch(docNo, "(January|February|March|April|May|June|July|August|September|October|November|December)\s\d{1,2}\,?\s\d{4}") Or
'                    Regex.IsMatch(docNo, "(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sept|Sept|Oct|Nov|Dec)\.?\s\d{1,2}\,?\s\d{4}") Then
'                    If docNo.Contains("</date>") Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "docNumber",
'                             .LineNo = intLine,
'                             .ErrorMessage = "In <presidentialDoc>, the Date In <docNumber> should be tagged In element <Date> With attributes.",
'                             .ErrorData = line,
'                             .Remarks = line})
'                    End If
'                End If
'            End If
'        End If
'        '********

'        '*******DOCNUMBER*********
'        If line.Contains("<docNumber> ") Then
'            'Applicable in <pLaw> only (<granuleType> "pl" And "pv")
'            If IsPlaw = True Or granuleType = "pl" Or granuleType = "pv" Then
'                If IsMeta = True Then
'                    '1. <docNumber> in <meta> should match in the 2nd number in <dc:title>
'                    'after the word "Public Law" Or "Private Law", Or before colon  ( : )
'                    If Regex.IsMatch(dcTitle, "^(Public|Private)\sLaw\s\d+(\-|\&\#x2013;)" & docNumberMeta & "\:") = False Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "docNumber",
'                             .LineNo = intLine,
'                             .ErrorMessage = "In <pLaw>, <docNumber> in <meta> should match in the 2nd number in <dc:title> after the word 'Public Law' or 'Private Law', or before colon (:)",
'                             .ErrorData = line,
'                             .Remarks = line})
'                    End If

'                ElseIf IsPreface = True Then
'                    '2. <docNumber> in <preface> must be the 1st and 2nd number (including dash) in <dc:title>
'                    'after the word "Public Law" or "Private Law".
'                    If Regex.IsMatch(dcTitle, "^(Public|Private)\sLaw\s" & Regex.Escape(docNumberPref)) = False Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "docNumber",
'                             .LineNo = intLine,
'                             .ErrorMessage = "In <pLaw>, <docNumber> in <preface> must be the 1st and 2nd number (including dash) in <dc:title> after the word 'Public Law' or 'Private Law'",
'                             .ErrorData = line,
'                             .Remarks = line})
'                    End If
'                End If
'            End If

'            'Applicable in <presidentialDocs role="proclamations"> (<granuleType> "pr")
'            If IsPresidentialDoc = True Or granuleType = "pr" Then
'                '1. <docNumber> in <meta> should match in the <docNumber> in <preface>,
'                'after the word "Proclamation".
'                If Regex.IsMatch(docNumberPref, "Proclamation\s" & docNumberMeta) = False Then
'                    errorReportList.Add(New ValidationReportModel() With {.Element = "docNumber",
'                         .LineNo = intLine,
'                         .ErrorMessage = "In <presidentialDocs role=""proclamations"">, <docNumber> in <meta> should match in the <docNumber> in <preface> after the word 'Proclamation'",
'                         .ErrorData = line,
'                         .Remarks = line})
'                End If
'            End If

'            'Applicable in <resolution> (<granuleType> "hc" and "sc")
'            If IsResolution = True Or granuleType = "hc" Or granuleType = "sc" Then
'                '1. <docNumber> in <meta> should match in the number in <dc:title>
'                If Regex.IsMatch(dcTitle, "(H|S)\.\sCon\.\sRes\.\s" & docNumberMeta) = False Then
'                    errorReportList.Add(New ValidationReportModel() With {.Element = "docNumber",
'                         .LineNo = intLine,
'                         .ErrorMessage = "In <resolution>, <docNumber> in <meta> should match in the <docNumber> in <preface> after the word 'Proclamation'",
'                         .ErrorData = line,
'                         .Remarks = line})
'                End If
'            End If
'        End If
'        '********

'        If line.Contains("</resolution>") Then
'            IsResolution = False
'        End If

'        If line.Contains("</pLaw>") Then
'            IsPlaw = False
'        End If

'        If line.Contains("</presidentialDocs>") Then
'            IsPresidentialDoc = False
'        End If

'        If line.Contains("<quotedText>") Then
'            If Not Regex.IsMatch(line, "\&\#x201C;<quotedText>") Then
'                errorReportList.Add(New ValidationReportModel() With {.Element = "quotedText",
'                             .LineNo = intLine,
'                             .ErrorMessage = "Missing &#x201C; before <quotedText>",
'                             .ErrorData = line,
'                             .Remarks = line})
'            End If
'        End If

'        If line.Contains("</quotedText>") Then
'            If Not Regex.IsMatch(line, "<\/quotedText>\&\#x201D;") Then
'                errorReportList.Add(New ValidationReportModel() With {.Element = "quotedText",
'                             .LineNo = intLine,
'                             .ErrorMessage = "Missing &#x201D; after </quotedText>",
'                             .ErrorData = line,
'                             .Remarks = line})
'            End If
'        End If

'        Dim ampColl As MatchCollection = Regex.Matches(line, "[^\S]+\s\&\s[^\S]*")
'        If ampColl.Count = 0 Then
'        Else
'            For Each am As Match In ampColl
'                errorReportList.Add(New ValidationReportModel() With {.Element = "",
'                             .LineNo = intLine,
'                             .ErrorMessage = "Unconverted ampersand(&) should be &amp;",
'                             .ErrorData = am.Value,
'                             .Remarks = line})
'            Next
'        End If


'        Dim hexColl As MatchCollection = Regex.Matches(line, "\&[^\;]+\;")
'        If hexColl.Count = 0 Then
'        Else
'            For Each hx As Match In hexColl
'                If Not Regex.IsMatch(hx.Value, "\&(gt|lt|amp)\;") Then
'                    If Regex.IsMatch(hx.Value, "\&\#x\w{4}\;") = False Then
'                        errorReportList.Add(New ValidationReportModel() With {.Element = "",
'                             .LineNo = intLine,
'                             .ErrorMessage = "Invalid hexadecimal character, please check.",
'                             .ErrorData = hx.Value,
'                             .Remarks = line})
'                    End If
'                End If
'            Next
'        End If


'        Dim dashNumColl As MatchCollection = Regex.Matches(line, "\d+(—|–|\&\#x2013;\&\#x2014;)\d+")


'        prevLine = line
'    Next


'End Sub