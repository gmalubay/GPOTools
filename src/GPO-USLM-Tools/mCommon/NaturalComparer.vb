Imports System.Text.RegularExpressions

Public Class NaturalComparer
    Implements IComparer(Of String)

    Private _pos As Integer
    Private ReadOnly _order As Integer

    Public Sub New(Optional Ascending As Boolean = True)
        _order = If(Ascending, 1, -1)
    End Sub

    Private Shared Function RegexSplit(ByVal s As String) As String()
        Return Regex.Split(s, "(\d+)", RegexOptions.IgnoreCase)
    End Function

    Private Shared Function GetEmptyStrings() As Predicate(Of String)
        Return Function(s) String.IsNullOrEmpty(s)
    End Function

    Public Function Compare(x As String, y As String) As Integer Implements IComparer(Of String).Compare
        Dim left As New List(Of String)(RegexSplit(x))
        Dim right As New List(Of String)(RegexSplit(y))

        left.RemoveAll(GetEmptyStrings())
        right.RemoveAll(GetEmptyStrings())

        _pos = 0
        For Each x In left
            If y.Count > _pos Then
                If Not Decimal.TryParse(x, Nothing) AndAlso Not Decimal.TryParse(right(_pos), Nothing) Then
                    Dim result As Integer = String.Compare(x, right(_pos), True)
                    If result <> 0 Then
                        Return result * _order
                    Else
                        _pos += 1
                    End If
                ElseIf Decimal.TryParse(x, Nothing) AndAlso Not Decimal.TryParse(right(_pos), Nothing) Then
                    Return -1 * _order
                ElseIf Not Decimal.TryParse(x, Nothing) AndAlso Decimal.TryParse(right(_pos), Nothing) Then
                    Return 1 * _order
                Else
                    Dim result = Decimal.Compare(Decimal.Parse(x), Decimal.Parse(right(_pos)))
                    If result = 0 Then
                        _pos += 1
                    Else
                        Return result * _order
                    End If
                End If
            Else
                Return -1 * _order
            End If
        Next

        Return _order
    End Function
End Class
