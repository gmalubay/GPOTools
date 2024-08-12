Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Net
Module OutputGenMain

    Dim errLogs As New List(Of OutputGenReportModel)
    Dim errSpellChks As List(Of SpellCheckLog)

    Dim strGrType As String = String.Empty
    Dim strDCType As String = String.Empty
    Dim strProcessDate As String = String.Empty
    Dim strCongress As String = String.Empty

    Dim intFnoteCount As Integer = 0
    Dim lastElemValue As String = String.Empty
    Dim lastElem As String = String.Empty

    Dim numPattern As String = "(?is)\b\(?(\d+|[A-Z]{1,2}|M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3}))\)?\b"
    Dim TempRXMLText As String = String.Empty

    Dim volumeTxt As String = String.Empty
    Dim prefaceDcType As String = String.Empty

    Sub ProcessOutputGenMain(ByVal InputPath As String, ByVal noErrorFiles As ArrayList, ByVal IsFromValidation As Boolean)
        Dim parsingError As String = String.Empty
        OutputPath = New DirectoryInfo(Path.Combine(InputPath, "OUTPUT"))

        If OutputPath.Exists = False Then
            OutputPath.Create()
        End If


        For Each txtFile As String In Directory.GetFiles(InputPath, "*.txt")
            Dim txtName As String = Path.GetFileNameWithoutExtension(txtFile)

            Form1.lblProcessText.Text = String.Format("processing ... {0}", txtName)
            Form1.lblProcessText.Refresh()

            errLogs = New List(Of OutputGenReportModel)
            errSpellChks = New List(Of SpellCheckLog)

            If IsFromValidation = True Then
                If noErrorFiles.Contains(txtFile) = False Then
                    Continue For
                End If
            End If


            Dim strContent As String = File.ReadAllText(txtFile)

            volumeTxt = String.Empty
            volumeTxt = Regex.Match(txtName, "STATUTE\-([^\-]+)").Groups(1).Value

            strGrType = String.Empty
            strDCType = String.Empty
            strCongress = String.Empty
            intFnoteCount = 0
            parsingError = String.Empty
            strProcessDate = String.Concat(Now.Year.ToString, "-", Now.Month.ToString.PadLeft(2, "0"), "-", Now.Day.ToString.PadLeft(2, "0"))



            Try
                If IsGranuleTypeExist(strContent) Then
                    strGrType = Regex.Match(strContent, "<granuleType>([^\<]+)<\/granuleType>").Groups(1).Value.Trim

                    If IsGranuleTypeAttCorrect(strGrType) Then
                        strDCType = GetDCType(strGrType)
                        strContent = strContent.Replace("<subSection", "<subsection").Replace("</subSection>", "</subsection>")

                        ConvertTextFile(strContent, txtName, strGrType)

                        If File.Exists(String.Concat(OutputPath.FullName, "\", txtName, ".xml")) Then
                            Form1.lblProcessSubText.Text = "validating ..."

                            errorReportList = New List(Of ValidationReportModel)
                            ValidateTxtOrXmlFile(String.Concat(OutputPath.FullName, "\", txtName, ".xml"), "", "XML")

                            Form1.lblProcessSubText.Text = "spell checking ..." : Form1.lblProcessSubText.Refresh()
                            errSpellChks = ProcessSpellChecker(txtFile)

                            Dim sequenceErr = (From q As ValidationReportModel In errorReportList Order By q.LineNo).ToList
                            GenerateReportValidation(OutputPath.FullName, Path.GetFileNameWithoutExtension(txtFile) & "-XML", sequenceErr, errSpellChks)
                        End If

                    Else
                        AddErrorMessage(0, "granuleType", "Incorrect grType value", strGrType)
                    End If
                Else
                    AddErrorMessage(0, "granuleType", "Missing granuleType tag", "")
                End If
            Catch ex As Exception
                Dim pos As String = Regex.Match(ex.Message, "position\s(\d+)").Groups(1).Value
                Dim etxt As String = String.Empty
                If Not String.IsNullOrEmpty(pos) Then
                    Dim spos As Integer = TempRXMLText.Substring(0, pos).LastIndexOf("<")
                    Try
                        etxt = TempRXMLText.Substring(spos, 250)
                    Catch x As Exception
                        etxt = TempRXMLText.Substring(spos)
                    End Try
                End If

                'If String.IsNullOrEmpty(etxt) Then
                '    etxt = lastElemValue
                'End If

                '                AddErrorMessage(0, lastElement, ex.Message, etxt)
                AddErrorMessage(0, lastElem, ex.Message, lastElemValue)


                parsingError = TempRXMLText
            End Try

            '**** generate log report
            GenerateReportOutputGen(OutputPath.FullName, txtName, errLogs)
            '************

            If Not String.IsNullOrEmpty(parsingError) Then
                Using sw As New StreamWriter(String.Concat(OutputPath.FullName, "\", txtName, ".error"))
                    sw.WriteLine(parsingError)
                End Using
            End If
        Next


        Form1.lblProcessText.Text = "..."
        Form1.lblProcessSubText.Text = "..."
    End Sub


    Sub ConvertTextFile(ByVal SourceText As String, ByVal Filename As String, ByVal grType As String)

        Dim RXML As String = String.Concat("<?xml version=""1.0""?>", vbCrLf, "<USLM>", vbCrLf, SourceText, vbCrLf, "</USLM>")
        RXML = Regex.Replace(RXML, "(</?dc)\:", "$1_")
        RXML = RXML.Replace("&#x", "&amp;#x")

        If IsValidFile(RXML) = False Then
            Exit Sub
        End If


        Dim elems = [Enum].GetNames(GetType(ElemToConvert))

        'For element: organizationNote, sidenote, coverText
        RXML = ProcessPerElementLine(RXML)

        For Each elem In elems

            Form1.lblProcessSubText.Text = String.Format("converting ... <{0}>", elem)
            Form1.lblProcessSubText.Refresh()

            'Try
            RXML = ProcessPerElement(RXML, elem)
            'Catch ex As Exception
            '    AddErrorMessage(0, elem, ex.Message, ex.StackTrace)
            'End Try
        Next

        '*** <longTitle>
        RXML = RXML.Replace("<docTitle>", "<longTitle><docTitle>")
        If Regex.IsMatch(RXML, "</officialTitle><sidenote>") Then
            RXML = Regex.Replace(RXML, "(</officialTitle><sidenote>(.*?)></sidenote>)", "$1</longTitle>")
        Else
            RXML = RXML.Replace("</officialTitle>", "</officialTitle></longTitle>")
        End If
        '************

        '*** <signatures>
        If Regex.IsMatch(RXML, "<signature>") Then
            RXML = RXML.Replace("</signature></signatures><signatures><signature>", "</signature><signature>")
        End If
        '*************

        '*** <component>
        If grType = "pl" Or grType = "pv" Then
            RXML = RXML.Replace("<USLM>", "<USLM><component><pLaw>").Replace("</USLM>", "</pLaw></component></USLM>")
        Else
            RXML = RXML.Replace("<resolution>", "<component><resolution>").Replace("</resolution>", "<resolution></component>")
            RXML = RXML.Replace("<presidentialDoc>", "<component><presidentialDoc>").Replace("<presidentialDoc>", "<presidentialDoc></component>")
        End If
        '************

        '*** root element
        If grType = "pl" Then
            RXML = RXML.Replace("<USLM>", "<publicLaws>").Replace("</USLM>", "</publicLaws>")
        ElseIf grType = "pv" Then
            RXML = RXML.Replace("<USLM>", "<privateLaws>").Replace("</USLM>", "</privateLaws>")
        ElseIf grType = "hc" Or grType = "sc" Then
            RXML = RXML.Replace("<USLM>", "<concurrentResolutions>").Replace("</USLM>", "</concurrentResolutions>")
        ElseIf grType = "pr" Then
            RXML = RXML.Replace("<USLM>", "<presidentialDocs role=""proclamations"">").Replace("</USLM>", "</presidentialDocs>")
        End If
        '************

        '*** clean-up
        RXML = Regex.Replace(RXML, "<granuleType>([^\<]+)<\/granuleType>", "")
        RXML = RXML.Replace("&amp;#x", "&#x").Replace("<dc_", "<dc:").Replace("</dc_", "</dc:")
        RXML = RXML.Replace("><", ">" & vbCrLf & "<")
        RXML = Regex.Replace(RXML, "(<(heading|num)[^>]*>)" & vbCrLf & "(<inline)", "$1$3")
        RXML = Regex.Replace(RXML, vbCrLf & "(</(heading|num)>)", "$1")

        For Each sdnote As Match In Regex.Matches(RXML, "<sidenote>(.*?)</sidenote>", RegexOptions.Singleline)
            Dim sdnotenew As String = sdnote.Value.Replace(vbCrLf, "")
            RXML = RXML.Replace(sdnote.Value, sdnotenew)
        Next
        For Each rle As Match In Regex.Matches(RXML, "(<role>(.*?)</role>|<name>(.*?)</name>)", RegexOptions.Singleline)
            Dim rlenew As String = rle.Value.Replace(vbCrLf, "")
            RXML = RXML.Replace(rle.Value, rlenew)
        Next
        '************

        If RXML.Contains("|") = False Then
            '**** generate output file
            Using sw As New StreamWriter(String.Concat(OutputPath.FullName, "\", Filename, ".xml"))
                sw.Write(RXML)
            End Using
            '************
        Else
            Dim arrLines() As String = RXML.Split(vbCrLf)
            Dim lineNo As Integer = 0
            For Each line In arrLines
                lineNo += 1
                If line.Contains("|") Then
                    AddErrorMessage(lineNo, "", "Unable to convert, concat is still present", line)
                End If
            Next

            Using sw As New StreamWriter(String.Concat(OutputPath.FullName, "\", Filename, ".error"))
                sw.Write(RXML)
            End Using

        End If

    End Sub

    Function IsValidFile(ByVal SourceText As String) As Boolean
        Dim bln As Boolean = True
        Dim allLines As List(Of String) = New List(Of String)
        Dim arr() As String = SourceText.Replace(vbLf, vbCrLf).Split(vbCrLf)

        For Each line In arr
            If Not String.IsNullOrEmpty(line.Trim) Then
                allLines.Add(line.Trim)
            End If
        Next

        Dim XMLEle As XElement = <XML></XML>

        Try
            XMLEle = XElement.Parse(SourceText, LoadOptions.None Or LoadOptions.SetLineInfo)
        Catch ex As Exception
            Dim pos As String = Regex.Match(ex.Message, "position\s(\d+)").Groups(1).Value
            Dim lineNumber As String = Regex.Match(ex.Message, "Line\s(\d+)").Groups(1).Value
            Dim strline As String = String.Empty
            Try
                strline = allLines(lineNumber - 1)
            Catch ex1 As Exception
                strline = allLines.Last
            End Try
            bln = False
            AddErrorMessage(lineNumber, "", ex.Message, strline)
        End Try

        Return bln
    End Function



    Function ProcessPerElementLine(ByVal SourceText As String) As String
        TempRXMLText = SourceText

        Dim XMLEle As XElement = <XML></XML>
        Dim tmpOutput As String = String.Empty
        XMLEle = XElement.Parse(SourceText, LoadOptions.None Or LoadOptions.SetLineInfo)
        tmpOutput = XMLEle.ToString(SaveOptions.DisableFormatting)
        tmpOutput = tmpOutput.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

        If Not IsNothing(XMLEle) Then
            Dim prevElem As String = String.Empty

            For Each elem In XMLEle.Descendants.ToList
                Dim textOutput As String = String.Empty
                Dim textRaw As String = elem.ToString
                Dim textElem As String = elem.ToString(SaveOptions.DisableFormatting)
                textElem = textElem.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

                Dim textValue As String = Regex.Replace(textElem, "^(<" & elem.Name.LocalName & "(.*?)>)", "")
                textValue = Regex.Replace(textValue, "(<\/" & elem.Name.LocalName & ">)$", "")


                Select Case elem.Name.LocalName
                    Case "preface"
                        prefaceDcType = Regex.Match(textRaw, "<dc_type>(.+)</dc_type>").Groups(1).Value

                    Case "organizationNote"
                        textOutput = ConvertOrganizationNote(textRaw)

                    Case "coverText"
                        textOutput = ConvertCoverText(textRaw)

                    Case "sidenote"
                        textOutput = ConvertSidenote(textRaw, prevElem)

                    Case "appropriations"
                        If Regex.IsMatch(textRaw.Trim, "^(<appropriations level=""(major|intermediate|small)"">)") Then
                        Else
                            textOutput = ConvertAppropriations(textValue)
                        End If

                    Case "date"
                        textOutput = ConvertDate(textValue)

                    Case "referenceItem"
                        textOutput = ConvertReferenceItem(textValue, textElem)

                End Select

                If Not String.IsNullOrEmpty(textOutput) Then
                    If textOutput.Trim <> textElem.Trim Then
                        tmpOutput = tmpOutput.Replace(textElem, textOutput.Trim)
                    End If
                End If

                prevElem = elem.Name.LocalName

                lastElemValue = textElem
                lastElem = elem.Name.LocalName
            Next
        End If

        Return tmpOutput

    End Function



    Function ProcessPerElement(ByVal SourceText As String, ByVal Element As String) As String
        Dim XMLEle As XElement = <XML></XML>
        Dim tmpOutput As String = String.Empty

        SourceText = SourceText.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")
        TempRXMLText = SourceText

        XMLEle = XElement.Parse(SourceText, LoadOptions.None Or LoadOptions.SetLineInfo)
        tmpOutput = XMLEle.ToString(SaveOptions.DisableFormatting)
        tmpOutput = tmpOutput.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

        If Not IsNothing(XMLEle) Then
            For Each elem In XMLEle.Descendants(Element).ToList
                Dim textRaw As String = String.Empty
                Dim textOutput As String = String.Empty
                Dim textElem As String = elem.ToString(SaveOptions.DisableFormatting)
                textElem = textElem.Replace(vbCr, "").Replace(vbLf, "").Replace(vbCrLf, "")

                textRaw = elem.ToString

                Dim textValue As String = Regex.Replace(textElem, "^(<" & Element & "(.*?)>)", "")
                textValue = Regex.Replace(textValue, "(<\/" & Element & ">)$", "")


                If elem.HasAttributes = True And textValue.Contains("|") = False Then
                    Continue For
                End If


                Select Case Element
                    Case "elided"
                        textOutput = "<elided>. . .</elided>"

                    Case "meta"
                        textOutput = ConvertMeta(textElem)

                    Case "preface"
                        textOutput = ConvertPreface(textElem)

                    Case "section"
                        textOutput = ConvertSection(textValue, SourceText)

                    Case "subsection", "subSection"
                        textOutput = ConvertSubSection(textElem, textValue)

                    Case "paragraph"
                        textOutput = ConvertParagraph(textValue, textElem)

                    Case "subparagraph"
                        textOutput = ConvertSubParagraph(textElem, textValue)

                    Case "note"
                        textOutput = ConvertNote(textElem, elem.Parent.Name.LocalName, elem)

                    Case "authority"
                        textOutput = ConvertAuthority(textElem)

                    Case "ref"
                        textOutput = ConvertRef(textElem, textValue, elem.Parent)

                    Case "shortTitle"
                        textOutput = ConvertShortTitle(textValue)

                    Case "amendingAction"
                        textOutput = ConvertAmendingAction(textValue)

                    Case "action"
                        textOutput = ConvertAction(textElem)

                    Case "legislativeHistory"
                        textOutput = ConvertLegislativeHistory(textValue)

                    Case "title"
                        textOutput = ConvertTitle(textValue)

                    Case "subtitle"
                        textOutput = ConvertSubTitle(textValue)

                    Case "clause"
                        textOutput = ConvertClause(textElem, textValue, False)

                    Case "subclause"
                        textOutput = ConvertSubClause(textElem, textValue, False)

                    Case "continuation"
                        textOutput = ConvertContinuation(textElem) 'ConvertContinuation(textValue)

                    Case "chapter"
                        textOutput = ConvertChapter(textValue)

                    Case "item"
                        textOutput = ConvertItem(textValue, textElem)

                    Case "subitem"
                        textOutput = ConvertSubItem(textValue, textElem)

                    Case "division"
                        textOutput = ConvertDivision(textValue)

                    Case "part"
                        textOutput = ConvertPart(textValue)

                    Case "subpart"
                        textOutput = ConvertSubPart(textValue)

                    Case "resolvingClause"
                        textOutput = ConvertResolvingClause(textElem) 'ConvertResolvingClause(textValue)

                    Case "recital"
                        textOutput = ConvertRecital(textElem, textValue)

                    Case "signature"
                        textOutput = ConvertSignature(textValue)

                    Case "level"
                        textOutput = ConvertLevel(textValue)

                    Case "article"
                        textOutput = ConvertArticle(textValue)

                    Case "mo"
                        textOutput = ConvertMathML(textValue)

                    Case "fillIn"
                        textOutput = ConvertFillin(textValue)

                    Case "footnote"
                        textOutput = ConvertFootnote(textValue)


                    Case "img"
                        textOutput = ConvertImage(textValue)

                    Case "page"
                        textOutput = ConvertPage(textElem, False)

                    Case "approvedDate"
                        If elem.Parent.Name <> "meta" Then
                            textOutput = ConvertApproveDate(textValue)
                        End If

                    Case "designator"
                        textOutput = ConvertDesignator(textValue)


                End Select


                If Not String.IsNullOrEmpty(textOutput) Then


                    If textOutput.Contains("Section 722 of the Public Health Service Act (42 U.S.C. 292r), as amended by section 2014(b)(1) of Public Law ") Then
                        Debug.WriteLine("wwwwww")
                    End If




                    tmpOutput = tmpOutput.Replace(textElem, textOutput)

                    Dim testParse As XElement = <XML></XML>

                    Try
                        testParse = XElement.Parse(tmpOutput, LoadOptions.None Or LoadOptions.SetLineInfo)
                    Catch ex As Exception

                        lastElemValue = textElem
                        lastElem = elem.Name.LocalName
                        Exit For
                    End Try

                End If
            Next

        End If

        Return tmpOutput
    End Function

    Function ConvertMeta(ByVal SourceText As String) As String
        Dim strMeta As String = SourceText

        '**** <dc_type>
        If strGrType = "pr" Then
            If Regex.IsMatch(strMeta, Regex.Escape("</dc_creator>")) Then
                strMeta = strMeta.Replace("</dc_creator>", String.Concat("</dc_creator>", vbCrLf, strDCType))
            Else
                strMeta = strMeta.Replace("</dc_title>", String.Concat("</dc_title>", vbCrLf, strDCType))
            End If
        Else
            strMeta = strMeta.Replace("</dc_title>", String.Concat("</dc_title>", vbCrLf, strDCType))
        End If

        '*** <approvedDate>
        Dim mtchApprveDte As Match = Regex.Match(strMeta, "<approvedDate>([^>]+)</approvedDate>", RegexOptions.Singleline)
        If mtchApprveDte.Success Then
            Dim strApprveDate As String = FormatApprveDateValue(mtchApprveDte.Groups(1).Value)
            If mtchApprveDte.Groups(1).Value <> strApprveDate Then
                strMeta = strMeta.Replace(mtchApprveDte.Value, String.Format("<approvedDate>{0}</approvedDate>", strApprveDate))
            End If
        End If

        '**** <dc_publisher>
        strMeta = strMeta.Replace("</approvedDate>", String.Concat("</approvedDate>", vbCrLf,
                                                                        "<dc_publisher>United States Government Publishing Office</dc_publisher>", vbCrLf,
                                                                        "<dc_format>text/xml</dc_format>", vbCrLf,
                                                                        "<dc_language>EN</dc_language>", vbCrLf,
                                                                        "<dc_rights>Pursuant to Title 17 Section 105 of the United States Code, this file is not subject to copyright protection and is in the public domain.</dc_rights>", vbCrLf,
                                                                        "<processedBy>Digitization Vendor</processedBy>", vbCrLf,
                                                                        String.Format("<processedDate>{0}</processedDate>", strProcessDate)))

        '**** <publicPrivate>
        Dim strPblcPrvt As String = String.Empty
        If strGrType = "pl" Then
            strPblcPrvt = "public"
        ElseIf strGrType = "pv" Then
            strPblcPrvt = "private"
        End If

        If Not String.IsNullOrEmpty(prefaceDcType) Then
            If Regex.IsMatch(prefaceDcType, "(Chap|Chapter)", RegexOptions.IgnoreCase) Then
                If Regex.IsMatch(strMeta, "<docNumber>") Then
                    strMeta = strMeta.Replace(strDCType, "")
                    strMeta = strMeta.Replace("</docNumber>", "</docNumber>" & "<dc_type>Chapter</dc_type>")
                End If
            End If
        End If


        If Not String.IsNullOrEmpty(strPblcPrvt) Then
            strMeta = strMeta.Replace("</meta>", String.Concat("<publicPrivate>", strPblcPrvt, "</publicPrivate>", vbCrLf, "</meta>"))
        End If

        strCongress = Regex.Match(strMeta, "<congress>([^<]+)</congress>").Groups(1).Value

        Return strMeta

    End Function

    Function ConvertPreface(ByVal SourceText As String) As String
        Dim strPrefix As String = SourceText


        Dim mtchPage As Match = Regex.Match(SourceText, "<page>([^>]+)<\/page>")
        If mtchPage.Success Then
            Dim strPage As String = ConvertPage(mtchPage.Value, False)
            strPrefix = strPrefix.Replace(mtchPage.Value, strPage)
        End If


        Dim mtchCongress As Match = Regex.Match(SourceText, "<congress>([^>]+)<\/congress>")
        If mtchCongress.Success Then
            Dim strCngresNo As String = mtchCongress.Groups(1).Value.Replace("Congress", "").Trim
            Dim strCongress As String = mtchCongress.Value

            strCngresNo = Regex.Match(mtchCongress.Groups(1).Value, "(\d+)(st|nd|rd|th)?\sCongress").Groups(1).Value

            If String.IsNullOrEmpty(strCngresNo) Then
                strCngresNo = mtchCongress.Groups(1).Value.Replace("Congress", "").Trim
                Try
                    strCngresNo = ParseWordsToNumber(strCngresNo)
                Catch ex As Exception

                End Try
            End If

            strCongress = strCongress.Replace("<congress>", String.Format("<congress value=""{0}"">", strCngresNo))

            strPrefix = strPrefix.Replace(mtchCongress.Value, strCongress)
        End If


        strPrefix = strPrefix.Replace("</page>", String.Concat("</page>", vbCrLf, strDCType))


        Return strPrefix
    End Function

    Function ConvertPage(ByVal SourceText As String, ByVal IsFromEdit As Boolean) As String
        Dim strPage As String = String.Empty
        '**** <page>
        Dim mtchPage As Match = Regex.Match(SourceText, "<page>([^>]+)</page>")

        If Not String.IsNullOrEmpty(mtchPage.Groups(1).Value) Then
            If mtchPage.Groups(1).Value.Contains("|") Then
                Dim tmpPage As String = mtchPage.Groups(1).Value
                If IsFromEdit = True And tmpPage.StartsWith("|") Then
                    tmpPage = Regex.Replace(tmpPage, "^\|", "")
                End If

                'Dim arr() As String = mtchPage.Groups(1).Value.Split("|")
                Dim arr() As String = tmpPage.Split("|")

                If arr.Length = 3 Then
                    strPage = String.Format("<page identifier=""/us/stat/{0}/{1}"">{2}</page>", arr(0), arr(1), arr(2))
                End If
            Else

                Dim pgMatch As Match = Regex.Match(mtchPage.Groups(1).Value, "([^\s]+)\sSTAT\.(.+)")
                If pgMatch.Success Then
                    strPage = String.Format("<page identifier=""/us/stat/{0}/{1}"">{2}</page>", pgMatch.Groups(1).Value.Trim, pgMatch.Groups(2).Value.Trim, mtchPage.Groups(1).Value.Trim)
                Else
                    strPage = SourceText
                End If
            End If
        Else
            strPage = SourceText
        End If

        Return strPage
    End Function

    Function ConvertNote(ByVal SourceText As String, ByVal ParentElem As String, ByVal Elem As XElement) As String
        Dim strNote As String = SourceText
        Dim strPClass As String = String.Empty


        If ParentElem = "legislativeHistory" Then
            If Regex.IsMatch(SourceText, "<heading>") = True And Regex.IsMatch(SourceText, "<subheading>") = True Then
                strPClass = "indent2 firstIndent-1"
            ElseIf Regex.IsMatch(SourceText, "<heading>") = True And Regex.IsMatch(SourceText, "<subheading>") = False Then
                strPClass = "indent4 firstIndent-1"
            End If
        End If


        If Not String.IsNullOrEmpty(strPClass) Then
            For Each selm In Elem.Descendants("p").ToList
                If selm.Parent.Name.LocalName = "note" Then
                    Dim pnote As String = selm.ToString(SaveOptions.DisableFormatting)
                    Dim newpnote As String = pnote.Replace("<p>", String.Format("<p class=""{0}"">", strPClass))
                    strNote = strNote.Replace(pnote, newpnote)
                End If
            Next

        Else
            strNote = SourceText
        End If


        Return strNote

    End Function

    Function ConvertOrganizationNote(ByVal SourceText As String)
        Dim strOrgNote As String = String.Empty
        Dim arrNotes As New ArrayList

        SourceText = Regex.Replace(SourceText, "<\/?organizationNote>", "")
        arrNotes = AddPTagPerLine(SourceText, "")

        strOrgNote = String.Format("<organizationNote>{0}</organizationNote>", String.Join(vbCrLf, arrNotes.ToArray))

        Return strOrgNote
    End Function

    Function ConvertAuthority(ByVal SourceText As String) As String
        Dim strAuthority As String = SourceText

        strAuthority = strAuthority.Replace("<authority>", "<authority>" & vbCrLf & "<p>")
        strAuthority = strAuthority.Replace("</authority>", "</p>" & vbCrLf & "</authority>")

        Return strAuthority
    End Function

    Function ConvertCoverText(ByVal CoverTextValue As String) As String
        Dim strCoverText As String = String.Empty
        Dim arrCvrTxt As New ArrayList

        CoverTextValue = Regex.Replace(CoverTextValue, "<\/?coverText>", "")

        arrCvrTxt = AddPTagPerLine(CoverTextValue, "")

        strCoverText = String.Format("<coverText>{0}</coverText>", String.Join(vbCrLf, arrCvrTxt.ToArray))

        Return strCoverText
    End Function


    Function ConvertSidenote(ByVal SidenoteValue As String, ByVal PrevElem As String) As String
        Dim strSidenote As String = String.Empty
        Dim arrSideNte As New ArrayList

        SidenoteValue = Regex.Replace(SidenoteValue, "<\/?sidenote>", "")

        If PrevElem = "officialTitle" Then
            arrSideNte = AddPTagPerLine(SidenoteValue, "class=""centered fontsize8""")
        Else

            If CInt(volumeTxt) <= 65 Then
                arrSideNte = AddPTagPerLine(SidenoteValue, "class=""firstIndent1 fontsize8""")
            Else
                arrSideNte = AddPTagPerLine(SidenoteValue, "class=""indent0 firstIndent0 fontsize8""")
            End If

        End If

        strSidenote = String.Format("<sidenote>{0}</sidenote>", String.Join(vbCrLf, arrSideNte.ToArray))

        Return strSidenote
    End Function


    Function ConvertRef(ByVal SourceText As String, ByVal refVal As String, ByVal ParentElem As XElement) As String
        Dim strRef As String = String.Empty
        Dim strhref As String = String.Empty
        Dim refValue As String = System.Net.WebUtility.HtmlDecode(refVal.Replace("&amp;", "&"))

        If refValue.Contains("H.R.") Then
            strhref = String.Format("/us/bill/{0}/hr/{1}", strCongress, Regex.Match(refValue, " \d+").Value.Trim)
        ElseIf refValue.Contains("H.J. Res.") Then
            strhref = String.Format("/us/bill/{0}/hjres/{1}", strCongress, Regex.Match(refValue, " \d+").Value.Trim)
        ElseIf refValue.Contains("Stat\.") Then
            Dim mtchUsc As Match = Regex.Match(refValue, "^(\d+)\sStat\.\s(\d+)")
            strhref = String.Format("/us/stat/{0}/{1}", mtchUsc.Groups(1).Value, mtchUsc.Groups(2).Value)

        ElseIf refValue.Contains("S.") Then
            strhref = String.Format("/us/bill/{0}/s/{1}", strCongress, Regex.Match(refValue, " \d+").Value.Trim)
        ElseIf refValue.Contains("USC") Or refValue.Contains("U.S.C.") Then
            Dim mtchUsc As Match = Regex.Match(refValue, "^(\d+)\s([^\s]+)\s([^\s]+)")
            strhref = String.Format("/us/usc/t{0}/s{1}", mtchUsc.Groups(1).Value, mtchUsc.Groups(3).Value)

        ElseIf Regex.IsMatch(refValue, "Section", RegexOptions.IgnoreCase) Then
            Dim abbrev As String = String.Empty
            If refValue.Contains("United States Code") Then
                abbrev = "usc"
            Else
                abbrev = "{not usc}"
            End If

            'Section 40117(l)(7) of title 49
            'Section 40117(l) of title 49
            Dim mtchSec As Match = Regex.Match(refValue, "(?is)^Section\s([^\(]+)\(([^\s]+)\s[^\d]+(\d+)\,")

            If mtchSec.Success = True Then
                strhref = String.Format("/us/{0}/t{1}/s{2}/{3}", abbrev, mtchSec.Groups(3).Value, mtchSec.Groups(1).Value, mtchSec.Groups(2).Value.Replace("(", "").Replace(")", "/"))
                If strhref.EndsWith("/") Then
                    strhref = Regex.Replace(strhref, "\/$", "")
                End If
            Else
                mtchSec = Regex.Match(refValue, "(?is)^Section\s([^\s]+)\s[^\d]+(\d+)\,")
                If mtchSec.Success = True Then
                    strhref = String.Format("/us/{0}/t{1}/s{2}", abbrev, mtchSec.Groups(2).Value, mtchSec.Groups(1).Value)
                End If
            End If

            'Dim mtchSec As Match = Regex.Match(refValue, "^Section\s(\d+)\(([^\)]+)\)\(([^\)]+)\)[^\d]+(\d+)\,")
            'If mtchSec.Success = True Then
            '    strhref = String.Format("/us/{0}/t{1}/s{2}/{3}/{4}", abbrev, mtchSec.Groups(4).Value, mtchSec.Groups(1).Value, mtchSec.Groups(2).Value, mtchSec.Groups(3).Value)
            'Else
            '    mtchSec = Regex.Match(refValue, "^Section\s(\d+)\(([^\)]+)\)[^\d]+(\d+)\,")
            '    strhref = String.Format("/us/{0}/t{1}/s{2}/{3}", abbrev, mtchSec.Groups(3).Value, mtchSec.Groups(1).Value, mtchSec.Groups(2).Value)
            'End If
        ElseIf Regex.IsMatch(refValue, "title ", RegexOptions.IgnoreCase) Then
            strhref = String.Format("/us/usc/t{0}", Regex.Match(refValue, "title (\d+)").Groups(1).Value.Trim)
        ElseIf Regex.IsMatch(refValue, "(Public Law|Pub\.? L\.?) ", RegexOptions.IgnoreCase) Then
            Dim mtchPl As Match = Regex.Match(refValue, "\s(\d+)(\-|\–)(\d+)")
            strhref = String.Format("/us/pl/{0}/{1}", mtchPl.Groups(1).Value, mtchPl.Groups(3).Value)

        Else
            Try
                If ParentElem.Name.LocalName = "note" Then
                    Dim strHeading As String = Regex.Match(ParentElem.ToString(), "<heading(Text)?>(.*?)</heading(Text)?>").Value
                    Dim rv As Match = Regex.Match(refValue, "(\d+)(–|\-|\—)(\d+)")

                    If Regex.IsMatch(strHeading, "HOUSE REPORT", RegexOptions.IgnoreCase) Then
                        strhref = String.Format("/us/hrpt/{0}/{1}", rv.Groups(1).Value, rv.Groups(3).Value)
                    ElseIf Regex.IsMatch(strHeading, "SENATE REPORT", RegexOptions.IgnoreCase) Then
                        strhref = String.Format("/us/srpt/{0}/{1}", rv.Groups(1).Value, rv.Groups(3).Value)
                    End If

                End If

            Catch ex As Exception

            End Try
        End If


        If Not String.IsNullOrEmpty(strhref) Then
            strhref = Regex.Replace(strhref, "<[^>]+>", "")
        End If



        strRef = SourceText.Replace("<ref>", String.Format("<ref href=""{0}"">", strhref))
        If strRef.Contains("[") Then
            strRef = strRef.Replace("[", "").Replace("]", "")
            strRef = String.Concat("[", strRef, "]")
        End If

        Return strRef
    End Function


    Function ConvertSection(ByVal sectionValue As String, ByVal sourceText As String) As String
        Dim strSection As String = String.Empty
        sectionValue = sectionValue.Replace(vbCrLf, "")

        If Regex.IsMatch(sectionValue, "^(<num [^>]+>(.*)</num>)(\s)?<heading>") Or
            Regex.IsMatch(sectionValue, "^(<heading>(.*)</heading>)") Then

            Dim strElem As String = Regex.Match(sourceText, "<section(.*?)>").Value

            strSection = String.Concat(strElem, sectionValue, "</section>")

        Else

            Dim matchP As Match = Regex.Match(sectionValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

            If matchP.Success = False Then
                matchP = Regex.Match(String.Concat(" |", sectionValue), "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")
            Else
                If Regex.IsMatch(matchP.Groups(3).Value, "<(content|paragraph|chapeau)>") = True Then
                    matchP = Regex.Match(String.Concat(" |", sectionValue), "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")
                ElseIf sectionValue.Trim.StartsWith("|") = False And sectionValue.StartsWith("1|") = False Then
                    matchP = Regex.Match(String.Concat(" |", sectionValue), "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")
                End If
            End If


            If matchP.Success = True Then
                If Not String.IsNullOrEmpty(matchP.Groups(1).Value.Trim) Then
                    strSection = "<section class=""inline"">"
                Else
                    strSection = "<section>"
                End If


                If Not String.IsNullOrEmpty(matchP.Groups(2).Value.Trim) Then
                    Dim tmpnum As String = GetNumValue(matchP.Groups(2).Value)

                    Dim strNum As String = Regex.Replace(matchP.Groups(2).Value, "</?b>", "")
                    strNum = Regex.Replace(strNum, "<inline class=""smallCaps"">([^<]+)</inline>", "$1")

                    strSection = String.Concat(strSection, String.Format("<num value=""{0}"">{1}</num>", tmpnum, strNum))
                End If

                If Not String.IsNullOrEmpty(matchP.Groups(3).Value.Trim) Then
                    Dim strHdng As String = Regex.Replace(matchP.Groups(3).Value, "</?b>", "")
                    strHdng = Regex.Replace(strHdng, "<inline class=""smallCaps"">([^<]+)</inline>", "$1")

                    strSection = String.Concat(strSection, String.Format("<heading>{0}</heading>", strHdng))
                End If


                If Not String.IsNullOrEmpty(matchP.Groups(4).Value.Trim) Then
                    Dim strSidenote As String = String.Empty
                    Dim strContent As String = matchP.Groups(4).Value
                    If strContent.StartsWith("<content><sidenote>") And strContent.Contains("<section>") = False Then
                        strSidenote = Regex.Match(strContent, "<sidenote>(.*?)</sidenote>").Value

                        If strSection.Contains("</num>") Then
                            strSection = strSection.Replace("</num>", String.Concat("</num>", vbCrLf, strSidenote))
                            strContent = Replace(strContent, strSidenote, "", 1, 1)
                        ElseIf strSection.Contains("</heading>") Then
                            strSection = strSection.Replace("</heading>", String.Concat("</heading>", vbCrLf, strSidenote))
                            strContent = Replace(strContent, strSidenote, "", 1, 1)
                        End If
                    End If

                    strSection = String.Concat(strSection, strContent)
                End If

                strSection = String.Concat(strSection, "</section>")
            Else
                strSection = String.Concat("<section>", sectionValue, "</section>")
            End If


        End If



        Return strSection
    End Function


    Function ConvertShortTitle(ByVal shortTitleValue As String) As String
        Dim strShortTitle As String = String.Empty

        Dim strRole As String = String.Empty

        Dim arr() As String = shortTitleValue.Split("|")

        Select Case arr(0)
            Case "A"
                strRole = "act"
            Case "T"
                strRole = "title"
            Case "S"
                strRole = "subtitle"
        End Select

        strShortTitle = String.Format("<shortTitle role=""{0}"">{1}</shortTitle>", strRole, arr(1))

        Return strShortTitle
    End Function

    Function ConvertSubSection(ByVal SourceText As String, ByVal SubSectionValue As String) As String

        Dim strSubSection As String = Regex.Match(SourceText, "<subsection(.*?)>").Value
        Dim strRole As String = String.Empty

        If Regex.IsMatch(SubSectionValue, "^(<num [^>]+>(.*)</num>)(\s)?<heading>") Or
            Regex.IsMatch(SubSectionValue, "^(<heading>(.*)</heading>)") Then

            strSubSection = String.Concat(strSubSection, SubSectionValue, vbCrLf, "</subsection>")


        Else
            '''role|class|num|heading|data
            ''Dim matchP As Match = Regex.Match(SubSectionValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

            '***05062024
            'role|num|heading|data
            Dim matchP As Match = Regex.Match(SubSectionValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

            If matchP.Success Then
                If Not String.IsNullOrEmpty(matchP.Groups(1).Value.Trim) Then
                    Select Case matchP.Groups(1).Value.Trim
                        Case "D"
                            strRole = "definitions"
                        Case "I"
                            strRole = "instruction"
                    End Select

                    strSubSection = strSubSection.Replace("<subsection", String.Format("<subsection role=""{0}""", strRole))
                End If

                ''If Not String.IsNullOrEmpty(matchP.Groups(2).Value.Trim) Then
                ''    strSubSection = strSubSection.Replace("<subsection>", "<subsection class=""inline"">")
                ''End If

                ''If Not String.IsNullOrEmpty(matchP.Groups(3).Value.Trim) Then
                If Not String.IsNullOrEmpty(matchP.Groups(2).Value.Trim) Then
                    Dim tmpnum As String = GetNumValue(matchP.Groups(2).Value)
                    strSubSection = String.Concat(strSubSection, String.Format("<num value=""{0}"">{1}</num>", tmpnum, matchP.Groups(2).Value))
                End If

                ''If Not String.IsNullOrEmpty(matchP.Groups(4).Value) Then
                If Not String.IsNullOrEmpty(matchP.Groups(3).Value) Then
                    strSubSection = String.Concat(strSubSection, String.Format("<heading>{0}</heading>", matchP.Groups(3).Value))
                End If

                ''strSubSection = String.Concat(strSubSection, matchP.Groups(5).Value, vbCrLf, "</subsection>")
                strSubSection = String.Concat(strSubSection, matchP.Groups(4).Value, vbCrLf, "</subsection>")
            Else
                strSubSection = SourceText
                ''AddErrorMessage(0, "subsection", "Incorrect # of concat s/b 4 : role|class|num|heading|data", SourceText)
                AddErrorMessage(0, "subsection", "Incorrect # of concat s/b 3 : role|num|heading|data", SourceText)
            End If


        End If



        Return strSubSection
    End Function

    Function ConvertAmendingAction(ByVal AmendingActionValue As String) As String
        Dim strAmendingAction As String = String.Empty
        Dim strType As String = String.Empty

        Dim arr() As String = AmendingActionValue.Split("|")

        If Not String.IsNullOrEmpty(arr(0)) Then
            Select Case arr(0)
                Case "A"
                    strType = "amend"
                Case "D"
                    strType = "delete"
                Case "I"
                    strType = "insert"
                Case "R"
                    strType = "redesignate"

            End Select
        End If

        strAmendingAction = String.Format("<amendingAction type=""{0}"">{1}</amendingAction>", strType, arr(1).Trim)

        Return strAmendingAction
    End Function

    Function ConvertParagraph(ByVal ParagraphValue As String, ByRef ParagraphElem As String) As String
        Dim tempPara As String = String.Empty
        Dim strParagraph As String = Regex.Match(ParagraphElem, "<paragraph(.*?)>").Value
        Dim strRole As String = String.Empty
        Dim strHeading As String = String.Empty
        Dim arrClass As New ArrayList
        Dim strNum As String = String.Empty


        Dim tempData As String = String.Empty
        If Regex.IsMatch(ParagraphValue, "^(<num [^>]+>(.*)</num>)(\s)?<heading>") Or
            Regex.IsMatch(ParagraphValue, "^(<heading>(.*)</heading>)") Then

            strParagraph = String.Concat(strParagraph, ParagraphValue, "</paragraph>")

        Else
            ''indent|firstindent|fontsize|role|Class|num|heading|data
            ''Dim matchP As Match = Regex.Match(ParagraphValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

            '***05062024
            'role|num|heading|data
            Dim matchP As Match = Regex.Match(ParagraphValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

            If matchP.Success Then

                ''tempPara = String.Concat("<paragraph>", matchP.Groups(1).Value, "|", matchP.Groups(2).Value, "|", matchP.Groups(3).Value, "|", matchP.Groups(4).Value, "|", matchP.Groups(5).Value, "|", matchP.Groups(6).Value, "|", matchP.Groups(7).Value, "|")
                ''If matchP.Groups(8).Value.Length > 1000 Then
                ''    tempData = matchP.Groups(8).Value.Substring(0, 1000)
                ''Else
                ''    tempData += matchP.Groups(8).Value
                ''End If

                tempPara = String.Concat(strParagraph, matchP.Groups(1).Value, "|", matchP.Groups(2).Value, "|", matchP.Groups(3).Value, "|")
                If matchP.Groups(4).Value.Length > 1000 Then
                    tempData = matchP.Groups(4).Value.Substring(0, 1000)
                Else
                    tempData += matchP.Groups(4).Value
                End If


                tempPara += tempData


                ''If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                ''    arrClass.Add(String.Format("indent{0}", matchP.Groups(1).Value))
                ''End If
                ''If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
                ''    arrClass.Add(String.Format("firstindent{0}", matchP.Groups(2).Value))
                ''End If
                ''If Not String.IsNullOrEmpty(matchP.Groups(3).Value) Then
                ''    arrClass.Add(String.Format("fontsize{0}", matchP.Groups(3).Value))
                ''End If

                ''If Not String.IsNullOrEmpty(matchP.Groups(4).Value) Then
                If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                    ''Select Case matchP.Groups(4).Value
                    Select Case matchP.Groups(1).Value
                        Case "D"
                            strRole = "definitions"
                        Case "I"
                            strRole = "instruction"
                    End Select
                    strRole = String.Format(" role=""{0}""", strRole)
                End If

                ''If Not String.IsNullOrEmpty(matchP.Groups(5).Value) Then
                ''    arrClass.Insert(0, String.Format("inline{0}", matchP.Groups(5).Value))
                ''End If

                ''strParagraph = String.Concat("<paragraph", strRole, String.Format(" class=""{0}""", String.Join(" ", arrClass.ToArray).Trim), ">")

                ''If Not String.IsNullOrEmpty(matchP.Groups(6).Value) Then
                If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
                    Dim tmpnum As String = GetNumValue(matchP.Groups(2).Value)
                    strParagraph = String.Concat(strParagraph, String.Format("<num value=""{0}"">{1}</num>", tmpnum.Trim, matchP.Groups(2).Value))
                End If

                ''If Not String.IsNullOrEmpty(matchP.Groups(7).Value) Then
                If Not String.IsNullOrEmpty(matchP.Groups(3).Value) Then
                    strParagraph = String.Concat(strParagraph, String.Format("<heading>{0}</heading>", matchP.Groups(3).Value))
                End If

                ''strParagraph = String.Concat(strParagraph, matchP.Groups(8).Value, vbCrLf, "</paragraph>")

                strParagraph = String.Concat(strParagraph, tempData)
                ParagraphElem = tempPara

            Else
                strParagraph = ParagraphElem
                ''AddErrorMessage("0", "paragraph", "Incomplete # of concat s/b 7 : indent|firstindent|fontsize|role|Class|num|heading|data", ParagraphValue)
                AddErrorMessage("0", "paragraph", "Incomplete # of concat s/b 3 : roles|num|heading|data", strParagraph)
            End If

        End If

        Return strParagraph
    End Function

    Function ConvertSubParagraph(ByVal SourceText As String, ByVal SubParagraphValue As String) As String
        Dim strSubParagraph As String = Regex.Match(SourceText, "<subparagraph(.*?)>").Value
        Dim strRole As String = String.Empty

        ''role|Class|num|heading|data
        ''Dim matchP As Match = Regex.Match(SubParagraphValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        '***05062024
        'role|num|heading|data
        Dim matchP As Match = Regex.Match(SubParagraphValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        If matchP.Success = True Then
            If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                Select Case matchP.Groups(1).Value
                    Case "D"
                        strRole = "definitions"
                    Case "I"
                        strRole = "instruction"
                End Select
                strSubParagraph = strSubParagraph.Replace(" class=", String.Format(" role=""{0}"" class=", strRole))
            End If

            ''If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
            ''    strSubParagraph = strSubParagraph.Replace(" class=""", String.Format(" class=""inline{0}", matchP.Groups(2).Value))
            ''End If

            ''If Not String.IsNullOrEmpty(matchP.Groups(3).Value) Then
            If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
                Dim tmpnum As String = GetNumValue(matchP.Groups(2).Value)
                strSubParagraph = String.Concat(strSubParagraph, String.Format("<num value=""{0}"">{1}</num>", tmpnum.Trim, matchP.Groups(2).Value))
            End If

            ''If Not String.IsNullOrEmpty(matchP.Groups(4).Value) Then
            If Not String.IsNullOrEmpty(matchP.Groups(3).Value) Then
                strSubParagraph = String.Concat(strSubParagraph, String.Format("<heading>{0}</heading>", matchP.Groups(3).Value))
            End If

            ''strSubParagraph = String.Concat(strSubParagraph, matchP.Groups(5).Value, vbCrLf, "</subparagraph>")
            strSubParagraph = String.Concat(strSubParagraph, matchP.Groups(4).Value, vbCrLf, "</subparagraph>")
        Else
            strSubParagraph = SourceText
            ''AddErrorMessage("0", "subparagraph", "Incomplete # of concat s/b 4 : role|Class|num|heading|data", SourceText)
            AddErrorMessage("0", "subparagraph", "Incomplete # of concat s/b 3 : role|num|heading|data", SourceText)
        End If






        Return strSubParagraph
    End Function


    Function ConvertAction(ByVal SourceText As String) As String
        Dim strAction As String = SourceText

        If SourceText.Contains("<actionDescription>") = False Then
            strAction = strAction.Replace("<action>", "<action><actionDescription>")
            strAction = strAction.Replace("</action>", "</actionDescription></action>")
        End If

        Return strAction
    End Function

    Function ConvertLegislativeHistory(ByVal LegislativeHistoryValue As String) As String
        Dim strLegislativeHistory As String = String.Empty

        Dim matchP As Match = Regex.Match(LegislativeHistoryValue, "^([^\|]*)\|(.+)")

        If matchP.Success = True Then
            strLegislativeHistory = "<legislativeHistory>"
            If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                strLegislativeHistory = String.Concat(strLegislativeHistory, String.Format("<heading>{0}</heading>", matchP.Groups(1).Value))
            End If
            strLegislativeHistory = String.Concat(strLegislativeHistory, matchP.Groups(2).Value, "</legislativeHistory>")
        End If

        Return strLegislativeHistory
    End Function

    Function ConvertTitle(ByVal TitleValue As String) As String
        Dim strTitle As String = ConvertNumHeadData(TitleValue)
        strTitle = String.Concat("<title>", strTitle, "</title>")
        Return strTitle
    End Function

    Function ConvertSubTitle(ByVal SubTitleValue As String) As String
        Dim strSubTitle As String = ConvertNumHeadData(SubTitleValue)
        strSubTitle = String.Concat("<subtitle>", strSubTitle, "</subtitle>")
        Return strSubTitle
    End Function

    Function ConvertClause(ByVal SourceText As String, ByVal ClauseValue As String, ByVal IsFromEdit As Boolean)
        Dim strClause As String = Regex.Match(SourceText, "<clause(.*?)>").Value
        'num|data

        Dim pattern As String = String.Empty
        If IsFromEdit = False Then
            pattern = "^([^\|]*)\|(.+)"
        Else
            pattern = "^\|([^\|]*)\|\|(.+)"
        End If

        'Dim matchP As Match = Regex.Match(ClauseValue, "^([^\|]*)\|(.+)")

        Dim matchP As Match = Regex.Match(ClauseValue, pattern)

        If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
            Dim tmpnum As String = GetNumValue(matchP.Groups(1).Value)
            strClause = String.Concat(strClause, String.Format("<num value=""{0}"">{1}</num>", tmpnum.Trim, matchP.Groups(1).Value))
        End If

        strClause = String.Concat(strClause, matchP.Groups(2).Value, "</clause>")

        Return strClause
    End Function

    Function ConvertSubClause(ByVal SourceText As String, ByVal SubClauseValue As String, ByVal IsFromEdit As Boolean) As String
        Dim strSubClause As String = Regex.Match(SourceText, "<subclause(.*?)>").Value
        'num|data

        Dim pattern As String = String.Empty
        If IsFromEdit = False Then
            pattern = "^([^\|]*)\|(.+)"
        Else
            pattern = "^\|([^\|]*)\|\|(.+)"
        End If

        'Dim matchP As Match = Regex.Match(SubClauseValue, "^([^\|]*)\|(.+)")
        Dim matchP As Match = Regex.Match(SubClauseValue, pattern)

        If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
            Dim tmpnum As String = GetNumValue(matchP.Groups(1).Value)
            strSubClause = String.Concat(strSubClause, String.Format("<num value=""{0}"">{1}</num>", tmpnum.Trim, matchP.Groups(1).Value))
        End If

        strSubClause = String.Concat(strSubClause, matchP.Groups(2).Value, "</subclause>")

        Return strSubClause
    End Function

    Function ConvertContinuation(ByVal ContinuationValue As String) As String
        Dim strContinuation As String = String.Empty
        '***05062024
        'No attribute value will be added.
        strContinuation = ContinuationValue


        ''Dim arrClass As New ArrayList

        '''indent|firstindent|fontsize|data
        ''Dim matchP As Match = Regex.Match(ContinuationValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        ''If matchP.Success = True Then
        ''    If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
        ''        arrClass.Add(String.Format("indent{0}", matchP.Groups(1).Value))
        ''    End If
        ''    If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
        ''        arrClass.Add(String.Format("firstindent{0}", matchP.Groups(2).Value))
        ''    End If
        ''    If Not String.IsNullOrEmpty(matchP.Groups(3).Value) Then
        ''        arrClass.Add(String.Format("fontsize{0}", matchP.Groups(3).Value))
        ''    End If

        ''    strContinuation = String.Format("<continuation class=""{0}"">", String.Join(" ", arrClass.ToArray))
        ''    strContinuation = String.Concat(strContinuation, matchP.Groups(4).Value, "</continuation>")
        ''Else
        ''    strContinuation = String.Concat("<continuation>", ContinuationValue, "</continuation>")
        ''End If

        Return strContinuation
    End Function


    Function ConvertChapter(ByVal ChapterValue As String) As String
        Dim strChapter As String = ConvertNumHeadData(ChapterValue)
        strChapter = String.Concat("<chapter>", strChapter, "</chapter>")
        Return strChapter
    End Function


    Function ConvertItem(ByVal ItemValue As String, ByVal SourceText As String) As String
        Dim strItem As String = Regex.Match(SourceText, "<item(.*?)>").Value
        Dim arrClass As New ArrayList

        ''indent|fontsize|inline|num|data
        ''Dim matchP As Match = Regex.Match(ItemValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        '***05062024
        'num|data
        Dim matchP As Match = Regex.Match(ItemValue, "^([^\|]*)\|(.+)")
        If matchP.Success Then
            ''If Not String.IsNullOrEmpty(matchP.Groups(3).Value.Trim) Then
            ''    arrClass.Add("inline")
            ''End If

            ''If Not String.IsNullOrEmpty(matchP.Groups(1).Value.Trim) Then
            ''    arrClass.Add(String.Format("indent{0}", matchP.Groups(1).Value))
            ''End If
            ''If Not String.IsNullOrEmpty(matchP.Groups(2).Value.Trim) Then
            ''    arrClass.Add(String.Format("fontsize{0}", matchP.Groups(3).Value))
            ''End If

            ''strItem = String.Format("<item class=""{0}"">", String.Join(" ", arrClass.ToArray).Trim)

            ''If Not String.IsNullOrEmpty(matchP.Groups(4).Value) Then
            If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                Dim tmpnum As String = GetNumValue(matchP.Groups(1).Value)
                strItem = String.Concat(strItem, String.Format("<num value=""{0}"">{1}</num>", tmpnum.Trim, matchP.Groups(1).Value))
            End If

            ''strItem = String.Concat(strItem, matchP.Groups(5).Value, "</item>")
            strItem = String.Concat(strItem, matchP.Groups(2).Value, "</item>")
        Else
            strItem = SourceText
            AddErrorMessage(0, "item", "Incorrect # of concat s/b 1 : num|data", strItem)
        End If

        Return strItem
    End Function

    Function ConvertSubItem(ByVal SubItemValue As String, ByVal SourceText As String) As String
        Dim strItem As String = Regex.Match(SourceText, "<subitem(.*?)>").Value
        Dim arrClass As New ArrayList

        ''indent|fontsize|inline|num|data
        ''Dim matchP As Match = Regex.Match(SubItemValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        '***05062024
        'num|data
        Dim matchP As Match = Regex.Match(SubItemValue, "^([^\|]*)\|(.+)")

        If matchP.Success = True Then
            ''If Not String.IsNullOrEmpty(matchP.Groups(3).Value) Then
            ''    arrClass.Add("inline")
            ''End If

            ''If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
            ''    arrClass.Add(String.Format("indent{0}", matchP.Groups(1).Value))
            ''End If
            ''If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
            ''    arrClass.Add(String.Format("fontsize{0}", matchP.Groups(3).Value))
            ''End If

            ''strItem = String.Format("<subitem class=""{0}"">", String.Join(" ", arrClass.ToArray).Trim)

            ''If Not String.IsNullOrEmpty(matchP.Groups(4).Value) Then
            If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                Dim tmpnum As String = GetNumValue(matchP.Groups(1).Value)
                strItem = String.Concat(strItem, String.Format("<num value=""{0}"">{1}</num>", tmpnum.Trim, matchP.Groups(1).Value))
            End If

            ''strItem = String.Concat(strItem, matchP.Groups(5).Value, "</subitem>")
            strItem = String.Concat(strItem, matchP.Groups(2).Value, "</subitem>")
        Else
            ''strItem = String.Concat("<subitem>", SubItemValue, "</subitem>")
            ''AddErrorMessage(0, "subitem", "Incorrect # of concat s/b 4 : indent|fontsize|inline|num|data", strItem)
            strItem = SourceText
            AddErrorMessage(0, "subitem", "Incorrect # of concat s/b 1 : num|data", strItem)
        End If


        Return strItem
    End Function

    Function ConvertDivision(ByVal DivisionValue As String) As String
        Dim strDivision As String = ConvertNumHeadData(DivisionValue)
        strDivision = String.Concat("<division>", strDivision, "</division>")
        Return strDivision
    End Function

    Function ConvertPart(ByVal PartValue As String) As String
        Dim strPart As String = ConvertNumHeadData(PartValue)
        strPart = String.Concat("<part>", strPart, "</part>")
        Return strPart
    End Function

    Function ConvertSubPart(ByVal SubPartValue As String) As String
        Dim strSubPart As String = ConvertNumHeadData(SubPartValue)
        strSubPart = String.Concat("<subpart>", strSubPart, "</subpart>")
        Return strSubPart
    End Function

    Function ConvertResolvingClause(ByVal ResolvingClauseValue As String) As String
        Dim strResolvingClause As String = String.Empty

        '***05062024
        'No attribute value will be added.
        strResolvingClause = ResolvingClauseValue

        ''Dim arrClass As New ArrayList

        '''indent|firstindent|fontsize|data
        ''Dim matchP As Match = Regex.Match(ResolvingClauseValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")
        ''If matchP.Success = True Then
        ''    If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
        ''        arrClass.Add(String.Format("indent{0}", matchP.Groups(1).Value))
        ''    End If
        ''    If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
        ''        arrClass.Add(String.Format("firstindent{0}", matchP.Groups(2).Value))
        ''    End If
        ''    If Not String.IsNullOrEmpty(matchP.Groups(3).Value) Then
        ''        arrClass.Add(String.Format("fontsize{0}", matchP.Groups(3).Value))
        ''    End If

        ''    If arrClass.Count = 0 Then
        ''        strResolvingClause = "<resolvingClause>"
        ''    Else
        ''        strResolvingClause = String.Format("<resolvingClause class=""{0}"">", String.Join(" ", arrClass.ToArray))
        ''    End If


        ''    strResolvingClause = String.Concat(strResolvingClause, matchP.Groups(4).Value, "</resolvingClause>")
        ''Else
        ''    strResolvingClause = String.Concat("<resolvingClause>", ResolvingClauseValue, "</resolvingClause>")
        ''    AddErrorMessage(0, "resolvingClause", "Incorrect # of concat s/b 3 : indent|firstindent|fontsize|data", strResolvingClause)
        ''End If


        Return strResolvingClause
    End Function


    Function ConvertRecital(ByVal SourceText As String, ByVal RecitalValue As String) As String
        Dim strRecital As String = String.Empty

        '***05062024
        'No attribute value will be added.
        strRecital = SourceText


        ''Dim arrClass As New ArrayList

        '''indent|firstindent|fontsize|data
        ''Dim matchP As Match = Regex.Match(RecitalValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")
        ''If matchP.Success = True Then
        ''    If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
        ''        arrClass.Add(String.Format("indent{0}", matchP.Groups(1).Value))
        ''    End If
        ''    If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
        ''        arrClass.Add(String.Format("firstindent{0}", matchP.Groups(2).Value))
        ''    End If
        ''    If Not String.IsNullOrEmpty(matchP.Groups(3).Value) Then
        ''        arrClass.Add(String.Format("fontsize{0}", matchP.Groups(3).Value))
        ''    End If

        ''    If arrClass.Count = 0 Then
        ''        strRecital = Regex.Match(SourceText, "<recital(.*?)>").Value
        ''    Else
        ''        strRecital = String.Format("<recital class=""{0}"">", String.Join(" ", arrClass.ToArray))
        ''    End If

        ''    strRecital = String.Concat(strRecital, matchP.Groups(4).Value, "</recital>")
        ''Else
        ''    strRecital = String.Concat(Regex.Match(SourceText, "<recital(.*?)>").Value, RecitalValue, "</recital>")
        ''    AddErrorMessage(0, "recital", "Incorrect # of concat s/b 3 : indent|firstindent|fontsize|data", strRecital)
        ''End If



        Return strRecital
    End Function


    Function ConvertSignature(ByVal SignatureValue As String) As String
        Dim strSignature As String = "<signatures><signature>"

        'name|role
        Dim matchP As Match = Regex.Match(SignatureValue, "^([^\|]+)\|(.+)")

        If matchP.Success = False Then
            matchP = Regex.Match(String.Concat(SignatureValue, "| "), "^([^\|]+)\|(.+)")
        End If


        If Not String.IsNullOrEmpty(matchP.Groups(1).Value.Trim) Then
            If matchP.Groups(1).Value.Contains("<name>") = False Then
                strSignature = String.Concat(strSignature, String.Format("<name>{0}</name>", matchP.Groups(1).Value.Trim))
            Else
                strSignature = String.Concat(strSignature, matchP.Groups(1).Value)
            End If
        End If

        If Not String.IsNullOrEmpty(matchP.Groups(2).Value.Trim) Then
            strSignature = String.Concat(strSignature, String.Format("<role>{0}</role>", matchP.Groups(2).Value.Trim))
        End If


        strSignature = String.Concat(strSignature, "</signature></signatures>")

        Return strSignature
    End Function

    Function ConvertLevel(ByVal LevelValue As String) As String
        Dim strLevel As String = String.Empty

        'class|num|heading|data
        Dim matchP As Match = Regex.Match(LevelValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")

        If matchP.Success = True Then
            If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                strLevel = String.Format("<level class=""{0}"">", "inline")
            Else
                strLevel = "<level>"
            End If

            If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
                Dim tmpnum As String = GetNumValue(matchP.Groups(2).Value)
                strLevel = String.Concat(strLevel, String.Format("<num value=""{0}"">{1}</num>", tmpnum.Trim, matchP.Groups(2).Value))
            End If

            If Not String.IsNullOrEmpty(matchP.Groups(3).Value) Then
                strLevel = String.Concat(strLevel, String.Format("<heading>{0}</heading>", matchP.Groups(2).Value))
            End If

            strLevel = String.Concat(strLevel, matchP.Groups(4).Value, vbCrLf, "</level>")
        Else
            strLevel = String.Concat("<level>", LevelValue, "</level>")
            AddErrorMessage(0, "level", "Incorrect # of concat s/b 3 : class|num|heading|data", strLevel)
        End If

        Return strLevel

    End Function

    Function ConvertArticle(ByVal ArticelValue As String) As String
        Dim strArticle As String = ConvertNumHeadData(ArticelValue)
        strArticle = String.Concat("<article>", strArticle, "</article>")
        Return strArticle
    End Function

    Function ConvertMathML(ByVal MathMLValue As String) As String
        Dim strMathML As String = String.Empty
        strMathML = String.Format("<mo xmlns=""http://www.w3.org/1998/Math/MathML"" stretchy=""true"">{0}</mo>", MathMLValue)
        Return strMathML
    End Function

    Function ConvertFillin(ByVal FillinValue As String) As String
        Dim strFillin As String = String.Empty

        Dim outFI As String = StrDup(CInt(FillinValue), vbTab)

        strFillin = String.Format("<text><fillIn style=""font-family:monospace"">{0}</fillIn></text>", outFI)
        Return strFillin
    End Function

    Function ConvertFootnote(ByVal FootnoteValue As String) As String
        Dim strFootnote As String = String.Empty

        intFnoteCount += 1

        Dim matchP As Match = Regex.Match(FootnoteValue, "^([^\|]+)\|(.+)")

        Dim tmpNum As String = Regex.Replace(matchP.Groups(1).Value, "<[^>]+>", "").Trim

        strFootnote = String.Format("<ref class=""footnoteRef"" idref=""fn{0}"">{1}</ref>", intFnoteCount.ToString.PadLeft(6, "0"), tmpNum)

        strFootnote = String.Concat(strFootnote,
                                    String.Format("<footnote id=""fn{0}""><num>{1}</num>", intFnoteCount.ToString.PadLeft(6, "0"), matchP.Groups(1).Value),
                                    matchP.Groups(2).Value.Trim,
                                    "</footnote>")
        Return strFootnote
    End Function


    Function ConvertAppropriations(ByVal AppropriationsValue As String) As String
        Dim strAppropriations As String = String.Empty

        'level|heading|data
        Dim matchP As Match = Regex.Match(AppropriationsValue, "^([^\|]+)\|([^\|]+)\|(.+)")

        Dim strLevel As String = String.Empty
        Select Case matchP.Groups(1).Value.Trim
            Case "M"
                strLevel = "major"
            Case "I"
                strLevel = "intermediate"
            Case "S"
                strLevel = "small"
            Case Else
                strLevel = String.Empty
        End Select







        strAppropriations = String.Concat(String.Format("<appropriations level=""{0}"">", strLevel),
                                          String.Format("<heading>{0}</heading>", matchP.Groups(2).Value),
                                          matchP.Groups(3).Value, "</appropriations>")

        Return strAppropriations
    End Function

    Function ConvertImage(ByVal ImageValue As String) As String
        Dim strImage As String = String.Format("<figure><img src=""{0}""/></figure>", ImageValue)
        Return strImage
    End Function

    Function ConvertReferenceItem(ByVal ReferenceItemValue As String, ByVal ReferenceItemElem As String) As String

        Dim strReferenceItem As String = String.Empty

        'role|data
        Dim matchP As Match = Regex.Match(ReferenceItemValue, "^([^\|]*)\|(.+)")


        Dim strRef As String = Regex.Match(ReferenceItemElem, "<referenceItem(.*?)>").Value


        If matchP.Success Then
            Dim strLevel As String = String.Empty
            Select Case matchP.Groups(1).Value.Trim
                Case "se"
                    strLevel = "section"
                Case "su"
                    strLevel = "subsection"
                Case "ti"
                    strLevel = "title"
                Case "st"
                    strLevel = "subtitle"
                Case "ch"
                    strLevel = "chapter"
                Case "sc"
                    strLevel = "subchapter"
                Case Else
                    strLevel = String.Empty
            End Select


            If Not String.IsNullOrEmpty(strLevel) Then
                strReferenceItem = String.Format("<referenceItem role=""{0}"">", strLevel)
            Else
                strReferenceItem = strRef
            End If

            strReferenceItem = String.Concat(strReferenceItem, matchP.Groups(2).Value, "</referenceItem>")
        Else
            strReferenceItem = ReferenceItemElem
        End If



        Return strReferenceItem

    End Function

    Function ConvertDesignator(ByVal DesignatorValue As String) As String
        Dim strDesignator As String = String.Empty
        Dim arrClass As New ArrayList

        Dim matchP As Match = Regex.Match(DesignatorValue, "^([^\|]*)\|([^\|]*)\|(.+)")

        If matchP.Success = True Then
            If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                arrClass.Add(String.Format("leaderChar=""{0}""", matchP.Groups(1).Value))
            End If

            If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
                arrClass.Add(String.Format("leaderAlign=""{0}""", matchP.Groups(2).Value))
            End If

            If arrClass.Count = 0 Then
                strDesignator = "<designator>"
            Else
                strDesignator = String.Format("<designator {0}>", String.Join(" ", arrClass.ToArray))
            End If


            strDesignator = String.Concat(strDesignator, matchP.Groups(3).Value, "</designator>")
        Else
            strDesignator = String.Format("<designator>{0}</designator>", DesignatorValue)
        End If


        Return strDesignator
    End Function

    Function ConvertP(ByVal PValue As String) As String
        Dim strP As String = String.Empty
        Dim arrClass As New ArrayList

        'indent|firstindent|fontsize|data
        Dim matchP As Match = Regex.Match(PValue, "^([^\|]*)\|([^\|]*)\|([^\|]*)\|(.+)")
        If matchP.Success Then
            If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                arrClass.Add("indent0")
            End If

            If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
                arrClass.Add(String.Format("firstIndent=""{0}""", matchP.Groups(2).Value))
            End If

            If Not String.IsNullOrEmpty(matchP.Groups(3).Value) Then
                arrClass.Add(String.Format("fontsize=""{0}""", matchP.Groups(3).Value))
            End If

            If arrClass.Count = 0 Then
                strP = "<designator>"
            Else
                strP = String.Format("<designator class=""{0}"">", String.Join(" ", arrClass.ToArray))
            End If

            strP = String.Concat(strP, matchP.Groups(4).Value, "</p>")

        Else
            strP = String.Format("<p>{0}</p>", PValue)

        End If

        Return strP
    End Function

    Function ConvertContent(ByVal ContentValue As String) As String
        Dim strContent As String = String.Empty
        'inline|data
        Dim matchP As Match = Regex.Match(ContentValue, "^([^\|]*)\|(.+)")

        If matchP.Success Then
            If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                strContent = "<content class=""inline"">"
            Else
                strContent = "<content>"
            End If

            strContent = String.Concat(strContent, matchP.Groups(2).Value, "</content>")
        Else
            strContent = String.Format("<content>{0}</content>", ContentValue)
        End If


        Return strContent
    End Function

    Function ConvertClauseEdit(ByVal SourceText As String, ByVal ClauseValue As String)
        Dim strClause As String = Regex.Match(SourceText, "<clause(.*?)>").Value
        'num|data

        Dim matchP As Match = Regex.Match(ClauseValue, "^\|([^\|]*)\|\|(.+)")

        If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
            Dim tmpnum As String = GetNumValue(matchP.Groups(1).Value)
            strClause = String.Concat(strClause, String.Format("<num value=""{0}"">{1}</num>", tmpnum.Trim, matchP.Groups(1).Value))
        End If

        strClause = String.Concat(strClause, matchP.Groups(2).Value, "</clause>")

        Return strClause
    End Function

    Function ConvertDate(ByVal DateValue As String) As String
        Dim strDate As String = String.Empty

        Dim strAtt As String = FormatApprveDateValue(DateValue)

        strDate = String.Format("<Date Date=""{0}"">{1}</Date>", strAtt, DateValue)

        Return strDate

    End Function


    Function ConvertNumHeadData(ByVal NumHeadData As String) As String
        Dim strValue As String = String.Empty

        If Regex.IsMatch(NumHeadData.Trim, "^(<num [^>]+>(.*)</num>)(\s)?<heading>") Or
            Regex.IsMatch(NumHeadData.Trim, "^(<heading>(.*)</heading>)") Then
            strValue = NumHeadData
        Else

            'num|heading|data
            Dim matchP As Match = Regex.Match(NumHeadData, "^([^\|]*)\|([^\|]*)\|(.+)")
            If matchP.Success = True Then
                If Not String.IsNullOrEmpty(matchP.Groups(1).Value) Then
                    Dim tmpnum As String = GetNumValue(matchP.Groups(1).Value)

                    strValue = String.Concat(strValue, String.Format("<num value=""{0}"">{1}</num>", tmpnum.Trim, matchP.Groups(1).Value))
                End If

                If Not String.IsNullOrEmpty(matchP.Groups(2).Value) Then
                    strValue = String.Concat(strValue, String.Format("<heading>{0}</heading>", matchP.Groups(2).Value))
                End If

                strValue = String.Concat(strValue, matchP.Groups(3).Value)

            Else
                strValue = NumHeadData
            End If



        End If




        Return strValue
    End Function

    Function GetNumValue(ByVal NumValue As String) As String
        Dim tmpnum As String = Regex.Replace(NumValue, "<[^>]+>", "").Replace(".", "").Trim
        tmpnum = Regex.Replace(tmpnum, "\&amp;\#x201C;", "", RegexOptions.IgnoreCase)
        tmpnum = tmpnum.Replace("""", "")

        Dim numMatches As MatchCollection
        numMatches = Regex.Matches(tmpnum, numPattern)

        For i As Integer = 0 To numMatches.Count - 1
            If Not String.IsNullOrEmpty(numMatches.Item(i).Value) Then
                tmpnum = numMatches.Item(i).Value
                Exit For
            End If
        Next

        Return tmpnum

    End Function


    Function AddPTagPerLine(ByVal SourceText As String, ByVal classAtt As String) As ArrayList
        Dim arrPLines As New ArrayList

        Dim arr() As String = SourceText.Split(vbCrLf)

        For Each onote In arr
            If Not String.IsNullOrEmpty(onote.Trim) Then
                If onote.Contains("<p") = False Then
                    If Not String.IsNullOrEmpty(classAtt) Then
                        arrPLines.Add(String.Format("<p {1}>{0}</p>", onote.Trim, classAtt))
                    Else
                        arrPLines.Add(String.Format("<p>{0}</p>", onote.Trim))
                    End If
                Else
                    arrPLines.Add(onote.Trim)
                End If

            End If
        Next

        Return arrPLines
    End Function


    Function ConvertApproveDate(ByVal ApprovedateValue As String) As String
        Dim strApproveDate As String = String.Empty

        Dim strApprveDate As String = FormatApprveDateValue(ApprovedateValue)
        strApproveDate = String.Format("<approvedDate date=""{0}"">{1}</approvedDate>", strApprveDate, ApprovedateValue)

        Return strApproveDate
    End Function

    Function GetDCType(ByVal granuleType As String) As String
        Dim dcType As String = String.Empty

        Select Case granuleType.ToLower
            Case "pl"
                dcType = "<dc_type>Public Law</dc_type>"
            Case "pv"
                dcType = "<dc_type>Private Law</dc_type>"
            Case "hc"
                dcType = "<dc_type>House Concurrent Resolution</dc_type>"
            Case "sc"
                dcType = "<dc_type>Senate Concurrent Resolution</dc_type>"
            Case "pr"
                dcType = "<dc_type>A Proclamation</dc_type>"
            Case Else
                dcType = String.Format("<dc_type>[{0}]</dc_type>", granuleType)

        End Select

        Return dcType
    End Function


    Function FormatApprveDateValue(ByVal DateValue As String) As String
        Dim dateresult As String = String.Empty
        If Regex.IsMatch(DateValue, "Sept\.?") Then
            DateValue = DateValue.Replace("Sept", "Sep")
        End If

        Try

            Dim time As DateTime = DateTime.Parse(DateValue)
            dateresult = String.Concat(time.Year, "-", time.Month.ToString.PadLeft(2, "0"), "-", time.Day.ToString.PadLeft(2, "0"))

        Catch ex As Exception
            Debug.WriteLine("wwww")

        End Try

        Return dateresult
    End Function

    Function CheckIfValidFile(ByVal SourceText As String) As String
        Dim XMLEle As XElement = <XML></XML>

        Dim RXML As String = String.Concat("<?xml version=""1.0""?>", vbCrLf, "<USLM>", vbCrLf, SourceText, vbCrLf, "</USLM>")

        RXML = Regex.Replace(RXML, "(</?dc)\:", "$1_")

        'Try
        XMLEle = XElement.Parse(RXML, LoadOptions.None Or LoadOptions.SetLineInfo)
        'Catch ex As Exception
        '    AddErrorMessage(0, "Parsing Error", ex.Message, ex.StackTrace)
        'End Try

        Return XMLEle.ToString()
    End Function


    Function IsGranuleTypeExist(ByVal SourceText As String) As Boolean
        'Validate if the text file/s have started in tag <granuleType>. If not, flag as error "Missing <granuleType> tag"
        Return Regex.IsMatch(SourceText, Regex.Escape("<granuleType>"))
    End Function

    Function IsGranuleTypeAttCorrect(ByVal grType As String) As Boolean
        'Check If <granuleType> tag has the format: <granuleType>[grtype]</granuleType>
        'grType should have any Of the following values. If Not, flag as error "incorrect grtype"

        Return Regex.IsMatch(grType, "^(fm|pl|pv|hc|sc|pr|co|ro|bm|sv|ot)$")
    End Function


    Sub AddErrorMessage(ByVal lineNo As Integer, ByVal Element As String, ByVal ErrorMessage As String, ByVal ErrorData As String)

        errLogs.Add(New OutputGenReportModel() With {.LineNo = lineNo,
                      .Element = Element,
                      .ErrorMessage = ErrorMessage.Replace("<", "&lt;").Replace(">", "&gt;"),
                      .ErrorData = ErrorData.Replace("<", "&lt;").Replace(">", "&gt;")})

    End Sub


End Module
