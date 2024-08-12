Imports System.IO
Imports OfficeOpenXml
Module ValidationReport

    Sub GenerateReportValidation(ByVal OutputPath As String, ByVal ReportFilename As String, ByVal errorReportLists As List(Of ValidationReportModel), ByVal errorSpellCheckList As List(Of SpellCheckLog))
        Dim errIden As String = IIf(errorReportLists.Count <= 1, "Error", "Errors")
        Dim reportOut As String = String.Concat(OutputPath, "\", ReportFilename, "_", errorReportLists.Count, errIden, ".xlsx")

        If File.Exists(reportOut) Then
            Dim arr() As String = Directory.GetFiles(OutputPath, String.Concat(ReportFilename, "*.xlsx"))
            Try
                File.Delete(reportOut)
            Catch ex As Exception
                reportOut = reportOut.Replace(".xlsx", "-" & (arr.Length + 1) & ".xlsx")
            End Try
        End If

        Try
            Dim newFile As New FileInfo(reportOut)
            Using package As New ExcelPackage(newFile)

                Dim ws As ExcelWorksheet = package.Workbook.Worksheets.Add("Validation")
                'ws.Cells.AutoFitColumns()
                ws.Column(1).Width = 10
                ws.Column(2).Width = 15
                ws.Column(3).Width = 80
                ws.Column(4).Width = 110
                ws.Column(5).Width = 110

                ws.Cells("A1").LoadFromCollection(errorReportLists, True, Table.TableStyles.Light8)

                Dim wo As ExcelWorksheet = package.Workbook.Worksheets.Add("Spell Check")
                wo.Cells("A1").LoadFromCollection(errorSpellCheckList, True, Table.TableStyles.Light8)
                wo.Cells.AutoFitColumns()


                package.Workbook.Properties.Title = "GPO USLM Validation"
                package.Save()

            End Using

        Catch ex As Exception

        End Try

    End Sub


End Module
