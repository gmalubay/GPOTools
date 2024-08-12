Imports System.Text.RegularExpressions
Module Words2Number
    Function ParseWordsToNumber(ByVal number As String) As Integer
        Dim words As String() = number.ToLower().Split(New Char() {" "c, "-"c, ","c}, StringSplitOptions.RemoveEmptyEntries)
        Dim ones As String() = {"one", "two", "three", "four", "five", "six", "seven", "eight", "nine"}
        Dim teens As String() = {"eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"}
        Dim tens As String() = {"ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"}
        Dim modifiers As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)() From {
            {"billion", 1000000000},
            {"million", 1000000},
            {"thousand", 1000},
            {"hundred", 100}
        }
        If number = "eleventy billion" Then Return Integer.MaxValue
        Dim result As Integer = 0
        Dim currentResult As Integer = 0
        Dim lastModifier As Integer = 1

        For Each word As String In words

            word = word.ToLower

            If word = "first" Then
                word = "one"
            ElseIf word = "second" Then
                word = "two"
            ElseIf word = "third" Then
                word = "three"
            ElseIf word = "fifth" Then
                word = "five"
            End If

            word = Regex.Replace(word, "th", "")


            If modifiers.ContainsKey(word) Then
                lastModifier = lastModifier * modifiers(word)
            Else
                Dim n As Integer

                If lastModifier > 1 Then
                    result += currentResult * lastModifier
                    lastModifier = 1
                    currentResult = 0
                End If

                If Array.IndexOf(ones, word) + 1 > 0 Then
                    n = Array.IndexOf(ones, word) + 1
                    currentResult += n
                ElseIf Array.IndexOf(teens, word) + 1 > 0 Then
                    n = Array.IndexOf(teens, word) + 1
                    currentResult += n + 10
                ElseIf Array.IndexOf(tens, word) + 1 > 0 Then
                    n = Array.IndexOf(tens, word) + 1
                    currentResult += n * 10

                ElseIf word <> "and" Then
                    Throw New ApplicationException("Unrecognized word: " & word)
                End If
            End If
        Next

        Return result + currentResult * lastModifier
    End Function
End Module
