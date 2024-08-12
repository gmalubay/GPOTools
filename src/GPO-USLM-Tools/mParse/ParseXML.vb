Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Xml
Imports System.Xml.Schema

Module ParseXML

    Sub ProcessMainParseXML(ByVal InputPath As String, ByVal SchemaVer As String)

        OutputPath = New DirectoryInfo(InputPath)

        If OutputPath.Exists = False Then
            OutputPath.Create()
        End If


        Dim schemaName As String = String.Concat("parser-", SchemaVer.Replace(" ", "_").Replace(".", "-"), ".bat")

        For Each xmlFile As String In Directory.GetFiles(InputPath, "*.xml")
            Dim xmlName As String = Path.GetFileName(xmlFile)
            Dim logName As String = Regex.Replace(xmlName, "\.xml", ".log", RegexOptions.IgnoreCase)

            Form1.lblProcessText.Text = String.Format("please wait. validating {0} ...", xmlName)
            Form1.lblProcessText.Refresh()

            Dim tmpXMLFile As String = Path.Combine(xsdFolder, xmlName)
            Dim tmpLOGFile As String = Path.Combine(xsdFolder, logName)

            InsertDocType(xmlFile, tmpXMLFile, SchemaVer.Replace(" ", "-"))


            ChDir(xsdFolder)


            If File.Exists(String.Concat(xsdFolder & "\uslm-2.0.14\", SchemaVer.Replace(" ", "-"), ".xsd")) Then

                ValidateXML(String.Concat(xsdFolder & "\uslm-2.0.14\", SchemaVer.Replace(" ", "-"), ".xsd"), tmpXMLFile, tmpLOGFile)


            ElseIf SchemaVer = "USLM 2.0.14" Then
                ValidateXML(String.Concat(xsdFolder & "\uslm-2.0.14\uslm-2.0.14.xsd"), tmpXMLFile, tmpLOGFile)
            Else
                Shell(schemaName, AppWinStyle.Hide, True)
            End If


            If File.Exists(tmpLOGFile) Then
                File.Copy(tmpLOGFile, Path.Combine(OutputPath.FullName, logName), True)
                File.Delete(tmpLOGFile)
            End If
            Try
                File.Delete(tmpXMLFile)
            Catch ex As Exception
            End Try
        Next

    End Sub


    Sub InsertDocType(ByVal SourceFilePath As String, ByVal OutputFilePath As String, ByVal SchemaVersion As String)
        Dim content As String = File.ReadAllText(SourceFilePath)

        If content.Contains("</statutesAtLarge>") = False Then
            content = String.Concat("<?xml version=""1.0"" encoding=""UTF-8""?>",
                    "<?xml-stylesheet type=""text/css"" href=""uslm.css""?>",
                    "<statutesAtLarge xmlns=""http://schemas.gpo.gov/xml/uslm"" xmlns:dc=""http://purl.org/dc/elements/1.1/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:dcterms=""http://purl.org/dc/terms/"" xml:lang=""en"" xsi:schemaLocation=""http://schemas.gpo.gov/xml/uslm https://www.govinfo.gov/schemas/xml/uslm/" & SchemaVersion & ".xsd"">",
                    "<meta>",
                    "<dc:publisher>United States Government Publishing Office</dc:publisher>",
                    "<dc:format>text/xml</dc:format>",
                    "<dc:language>EN</dc:language>",
                    "<dc:rights>Pursuant to Title 17 Section 105 of the United States Code, this file is not subject to copyright protection and is in the public domain.</dc:rights>",
                    "<processedBy>Digitization Vendor</processedBy>",
                    "<processedDate>2024-01-09</processedDate>",
                    "<congress>106</congress><session>2</session>",
                    "<dc:date>2000</dc:date>",
                    "<volume>114</volume>",
                    "</meta>",
                    "<main><collection role=""statutesParts"">",
                    "<component role=""statutesPart""><meta><docPart>1</docPart></meta>",
                    "<preface>",
                    "<page />",
                    "</preface>",
                    content,
                    "</component>",
                    "</collection>",
                    "</main>",
                    "</statutesAtLarge>")

            Using sw As New StreamWriter(OutputFilePath)
                sw.WriteLine(content)
            End Using
        Else
            File.Copy(SourceFilePath, OutputFilePath, True)

        End If
    End Sub


    Dim collErrors As New ArrayList
    Sub ValidateXML(ByVal SchemaFile As String, ByVal XMLFile As String, ByVal TempLogFile As String)


        Dim settings As XmlReaderSettings = New XmlReaderSettings()
        settings.Schemas.Add(Nothing, SchemaFile)
        settings.ValidationType = ValidationType.Schema

        collErrors = New ArrayList
        AddHandler settings.ValidationEventHandler, AddressOf ValidationEventHandler


        Using reader As XmlReader = XmlReader.Create(XMLFile, settings)

            Try

                While reader.Read()
                End While

            Catch e As XmlException
                collErrors.Add(e.Message & vbCrLf)
                collErrors.Add("The XML document is NOT valid.")
            End Try
        End Using


        If collErrors.Count = 0 Then
            collErrors.Add("The XML document is valid.")
        End If


        Using sw As New StreamWriter(TempLogFile)
            sw.WriteLine("XML schema (XSD) validation with XmlSchemaSet")
            sw.WriteLine("Version 01.00.00")
            sw.WriteLine("")
            sw.WriteLine(String.Format("Schema File: {0}", Path.GetFileName(SchemaFile)))
            sw.WriteLine("")
            sw.WriteLine(String.Format("validating {0}", Path.GetFileName(XMLFile)))
            sw.WriteLine("")
            sw.WriteLine(String.Join(vbCrLf, collErrors.ToArray()))

            sw.WriteLine("********")
        End Using


    End Sub

    Private Sub ValidationEventHandler(ByVal sender As Object, ByVal e As ValidationEventArgs)

        If e.Severity = XmlSeverityType.Warning Then
            collErrors.Add(e.Exception.LineNumber & vbTab & "WARNING: " & e.Message & vbCrLf)
        ElseIf e.Severity = XmlSeverityType.[Error] Then
            collErrors.Add(e.Exception.LineNumber & vbTab & "ERROR: " & e.Message & vbCrLf)
        End If
    End Sub



End Module
