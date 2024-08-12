Imports System.IO
Imports NHunspell
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Threading
Imports System.Threading.Tasks



Module ValidationSpellCheck

    Private arrDictionary As New ArrayList
    Private arrCorrectWords As New ArrayList

    Function ProcessSpellChecker(ByVal SourceFile As String) As List(Of SpellCheckLog)
        Dim intLine As Integer = 0
        Dim errSpell As New List(Of SpellCheckLog)


        arrCorrectWords.Add("text/xml")

        Using engine As New SpellEngine()

            Try

                engine.AddLanguage(DictionaryLanguage(dictionaryFolder & "\en_US.dic", dictionaryFolder & "\en_US.aff", "en-us"))
                engine.AddLanguage(DictionaryLanguage(dictionaryFolder & "\en_GB.dic", dictionaryFolder & "\en_GB.aff", "en-gb"))
                engine.AddLanguage(DictionaryLanguage(dictionaryFolder & "\en_CA.dic", dictionaryFolder & "\en_CA.aff", "en-ca"))
                engine.AddLanguage(DictionaryLanguage(dictionaryFolder & "\en_AU.dic", dictionaryFolder & "\en_AU.aff", "en-au"))
                engine.AddLanguage(DictionaryLanguage(dictionaryFolder & "\en_USNames.dic", dictionaryFolder & "\en_USNames.aff", "en-names"))

                Dim sr As StreamReader = New StreamReader(SourceFile)
                Do While sr.Peek > 0
                    Dim strLine As String = sr.ReadLine()

                    intLine += 1

                    Form1.lblProcessSubText.Text = String.Format("spell checking ... line#: {0}", intLine) : Form1.lblProcessSubText.Refresh()

                    strLine = Regex.Replace(strLine, "\<[^\>]+\>", " ")

                    If Not String.IsNullOrEmpty(strLine) Then
                        If Not Regex.IsMatch(strLine, "^\>$") Then
                            strLine = Regex.Replace(strLine, "<(.*?)$", "")
                        End If
                        If Not Regex.IsMatch(strLine, "^\<") Then
                            strLine = Regex.Replace(strLine, "^(.*?)[^\>]+>", "")
                        End If
                    End If


                    If Not String.IsNullOrEmpty(strLine) Then
                        Try

                            For Each fWord As Match In Regex.Matches(strLine, "\S+")

                                Dim word As String = RemoveUnnecessaryCharacter(fWord.Value.Trim)

                                If String.IsNullOrEmpty(word) Then
                                    Continue For
                                End If

                                If Regex.IsMatch(word, "\d+") Or word.Length = 1 Then
                                    word = String.Empty
                                End If

                                If Not String.IsNullOrEmpty(word) Then

                                    If word = word.ToUpper Or word = "ss" Then
                                        Continue For
                                    End If

                                    If Not arrCorrectWords.Contains(word.ToLower) Then
                                        If arrDictionary.Contains(word) Or
                                               engine("en-us").Spell(word) Or
                                               engine("en-ca").Spell(word) Or
                                               engine("en-gb").Spell(word) Or
                                               engine("en-au").Spell(word) Or
                                               engine("en-names").Spell(word) Then

                                            arrCorrectWords.Add(word.ToLower)

                                        Else
                                            errSpell.Add(New SpellCheckLog With {.LineNo = intLine, .Data = fWord.Value.Trim})
                                        End If
                                    End If
                                End If
                            Next

                        Catch ex As Exception

                        End Try
                    End If
                Loop

            Catch ex As Exception
                errSpell.Add(New SpellCheckLog With {.LineNo = intLine, .Data = String.Concat(ex.Message, vbCrLf, ex.StackTrace)})
            End Try

        End Using


        Return errSpell
    End Function


    Private Function DictionaryLanguage(ByVal SourceDic As String, ByVal SourceAff As String, ByVal DictName As String) As LanguageConfig
        Dim dicConfig As New LanguageConfig()
        dicConfig.LanguageCode = DictName
        dicConfig.HunspellAffFile = SourceAff
        dicConfig.HunspellDictFile = SourceDic
        Return dicConfig
    End Function

    Private Function RemoveUnnecessaryCharacter(ByVal FWord As String) As String
        FWord = FWord.Replace(ChrW(8220), "").Replace(ChrW(8221), "").Replace("’", "").Replace("‘", "")

        FWord = Regex.Replace(FWord, "(?is)\([A-Z]{1,3}\)", "")

        If Not String.IsNullOrEmpty(FWord) Then

            If Regex.IsMatch(FWord, "\&(\#x2019|rdquo|rsquo|quot|lsquo|\#x201C|\#x201D)\;s?$") Then
                FWord = Regex.Replace(FWord, "\&(\#x2019|rdquo|rsquo|quot|lsquo|\#x201C|\#x201D)\;s?$", String.Empty)
            End If

            If Regex.IsMatch(FWord, "^\&(\#x2018|ldquo|lsquo|quot|\#x201C|\#x201D)\;") Then
                FWord = Regex.Replace(FWord, "^\&(\#x2018|ldquo|lsquo|quot|\#x201C|\#x201D)\;", String.Empty)
            End If

            FWord = FWord.Replace("""", String.Empty)

            Do While Regex.IsMatch(FWord, "^(\p{Ps}|\p{Pe}|\!|\?|\.|,|:|\')")
                FWord = FWord.Substring(1).Trim
            Loop

            Do While Regex.IsMatch(FWord, "(\p{Pe}|\!|\?|\.|,|:|\'|\)|\]|…)$")
                FWord = FWord.Substring(0, FWord.Length - 1).Trim
            Loop

            If Regex.IsMatch(FWord, "\&(\#x2019|rdquo|rsquo|quot|lsquo|ldquo|\#x201C|\#x201D)\;(\p{Pe}|\!|\?|\.|,|:|\'|…|\)|\]|;)?$") Then
                FWord = Regex.Replace(FWord, "\&(\#x2019|rdquo|rsquo|quot|lsquo|ldquo|\#x201C|\#x201D)\;(\p{Pe}|\!|\?|\.|,|:|\'|…|;)?$", String.Empty)
            End If

            If Regex.IsMatch(FWord, "^\&(\#x2018|ldquo|lsquo|quot|rsquo|\#x201C|\#x201D)\;") Then
                FWord = Regex.Replace(FWord, "^\&(\#x2018|ldquo|lsquo|quot|rsquo|\#x201C|\#x201D)\;", String.Empty)
            End If

            If Regex.IsMatch(FWord, "\;$") Then
                If Not Regex.IsMatch(FWord, "\&[^\;]+\;$") Then
                    FWord = FWord.Substring(0, FWord.Length - 1).Trim
                End If
                Do While Regex.IsMatch(FWord, "(\p{Pe}|\!|\?|\.|,|:|\'|\)|\]|…)$")
                    FWord = FWord.Substring(0, FWord.Length - 1).Trim
                Loop
                If Regex.IsMatch(FWord, "\&(\#x2019|rdquo|rsquo|quot|lsquo|ldquo|\#x201C|\#x201D)\;(\p{Pe}|\!|\?|\.|,|:|\'|…|\)|\]|;)?$") Then
                    FWord = Regex.Replace(FWord, "\&(\#x2019|rdquo|rsquo|quot|lsquo|ldquo|\#x201C|\#x201D)\;(\p{Pe}|\!|\?|\.|,|:|\'|…|;)?$", String.Empty)
                End If
            End If

            FWord = FWord.Replace("&mldr;", "")
            FWord = HttpUtility.HtmlDecode(FWord)
        End If


        Return FWord
    End Function

End Module

Public Class SpellCheckLog
    Property LineNo As Integer
    Property Data As String

    Sub New()
        LineNo = 0
        Data = String.Empty
    End Sub
End Class

