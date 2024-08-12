Public Class OutputGenReportModel

    Property LineNo As Integer

    Property Element As String
    Property ErrorMessage As String

    Property ErrorData As String

    Sub New()
        LineNo = 0
        Element = String.Empty
        ErrorMessage = String.Empty
        ErrorData = String.Empty
    End Sub


End Class
