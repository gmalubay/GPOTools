Public Class ValidationReportModel
    Private _rmrks As String

    Property LineNo As Integer
    Property Element As String
    Property ErrorMessage As String
    Property ErrorData As String
    Property Remarks As String
        Get
            Return _rmrks
        End Get
        Set(value As String)
            _rmrks = value
            If Not String.IsNullOrEmpty(_rmrks) Then
                If _rmrks.Length > 1000 Then
                    _rmrks = value.Substring(0, 1000)
                End If
            End If

        End Set
    End Property

    Sub New()
        LineNo = 0
        Element = String.Empty
        ErrorMessage = String.Empty
        ErrorData = String.Empty
        Remarks = String.Empty
        _rmrks = String.Empty
    End Sub

End Class
