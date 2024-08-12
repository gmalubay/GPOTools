Imports System.IO

Module GlobalVariable

    Public OutputPath As DirectoryInfo
    Public noErrorFiles As New ArrayList
    Public xsdFolder As String = Path.Combine(Application.StartupPath, "XSD")
    Public dictionaryFolder As String = Path.Combine(Application.StartupPath, "DICTIONARY")

End Module
