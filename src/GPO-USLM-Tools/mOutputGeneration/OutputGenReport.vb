Module OutputGenReport

    Sub GenerateReportOutputGen(ByVal ReportPath As String, ByVal Filename As String, ByVal errLogs As List(Of OutputGenReportModel))
        Dim dteNow As DateTime = DateTime.Now

        Dim bodyContent As String = String.Empty

        For Each cm As OutputGenReportModel In errLogs
            bodyContent += String.Format("{3}<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>",
                                            cm.Element, cm.ErrorMessage, cm.ErrorData, vbCrLf)
        Next

        If String.IsNullOrEmpty(bodyContent) Then
            bodyContent = String.Format("{3}<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>",
                                           "", "", "No error.", vbCrLf)
        End If

        Dim strBody As String = String.Empty
        strBody = My.Resources.Report_Template
        strBody = strBody.Replace("[DateGenerated]", dteNow.ToString)
        strBody = strBody.Replace("[XML-Filename]", Filename)
        strBody = strBody.Replace("[report-body]", bodyContent)
        strBody = strBody.Replace("[ProgVersion]", String.Concat(gProgramVersionNo, " - ", gProgramVersionDate))

        Using sw As New IO.StreamWriter(String.Concat(ReportPath, "\", Filename.Replace(".xml", ""), "-GPO-OutputXML-Report.html"))
            sw.WriteLine(strBody)
        End Using

    End Sub



End Module
