Imports Microsoft.Reporting.WinForms
Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Drawing.Printing
Imports System.IO
Imports System.Runtime.CompilerServices

Module LocalReportExtensions
    <Extension()>
    Public Sub Print(ByVal report As LocalReport)
        Dim pageSettings = New PageSettings()

        Dim Size As New PaperSize()

        With Size

            .Height = 600
            .Width = 850

        End With

        With pageSettings
            .Margins.Top = 0.1
            .Margins.Bottom = 0.1
            .Margins.Left = 0.1
            .Margins.Right = 0.1
            .PaperSize = Size
        End With

        Print(report, pageSettings)
    End Sub

    Private m_currentPageIndex As Integer
    Private m_streams As IList(Of Stream)

    Public Function CreateStream(ByVal name As String, ByVal fileNameExtension As String, ByVal encoding As System.Text.Encoding, ByVal mimeType As String, ByVal willSeek As Boolean) As Stream
        Dim stream As Stream = New MemoryStream()
        m_streams.Add(stream)
        Return stream
    End Function

    <Extension()>
    Public Sub Print(ByVal report As LocalReport, ByVal pageSettings As PageSettings)
        Dim deviceInfo As String = "<DeviceInfo>" +
                  "<OutputFormat>EMF</OutputFormat>" +
                  "<PageWidth>8.5in</PageWidth>" +
                  "<PageHeight>6in</PageHeight>" +
                  "<MarginTop>0.1in</MarginTop>" +
                  "<MarginLeft>0.1in</MarginLeft>" +
                  "<MarginRight>0.1in</MarginRight>" +
                  "<MarginBottom>0.1in</MarginBottom>" +
                    "</DeviceInfo>"
        'new comment
        'Dim warnings As Warning()
        '' Dim streams = New List(Of Stream)()
        'm_streams = New List(Of Stream)
        'Dim currentPageIndex = 0
        'report.Render("Image", deviceInfo, AddressOf CreateStream, warnings)

        'report.Render("Image", deviceInfo, Function(name, fileNameExtension, encoding, mimeType, willSeek)
        '                                       Dim stream = New MemoryStream()
        '                                       streams.Add(stream)
        '                                       Return stream
        '                                   End Function, warnings)
        Dim warnings As Warning() = New Warning() {} ' Initialize as an empty array
        m_streams = New List(Of Stream)
        Dim currentPageIndex = 0
        report.Render("Image", deviceInfo, AddressOf CreateStream, warnings)

        Dim var = m_streams.Where(Function(X) X.Length <= 408).ToList

        For Each ms As MemoryStream In var
            m_streams.Remove(ms)
        Next

        For Each stream As Stream In m_streams
            stream.Position = 0
        Next

        If m_streams Is Nothing OrElse m_streams.Count = 0 Then Throw New Exception("Error: no stream to print.")
        Dim printDocument = New PrintDocument()
        printDocument.DefaultPageSettings = pageSettings
        printDocument.DefaultPageSettings.PaperSize = New System.Drawing.Printing.PaperSize("HalfInvoice", 850, 600)

        If Not printDocument.PrinterSettings.IsValid Then
            Throw New Exception("Error: cannot find the default printer.")
        Else
            AddHandler printDocument.PrintPage, Sub(s, e)
                                                    Dim pageImage As Metafile = New Metafile(m_streams(currentPageIndex))
                                                    Dim adjustedRect As Rectangle = New Rectangle(e.PageBounds.Left - CInt(e.PageSettings.HardMarginX), e.PageBounds.Top - CInt(e.PageSettings.HardMarginY), e.PageBounds.Width, e.PageBounds.Height)
                                                    e.Graphics.FillRectangle(Brushes.White, adjustedRect)
                                                    e.Graphics.DrawImage(pageImage, adjustedRect)
                                                    currentPageIndex += 1
                                                    e.HasMorePages = (currentPageIndex < m_streams.Count)
                                                    e.Graphics.DrawRectangle(Pens.White, adjustedRect)
                                                End Sub

            AddHandler printDocument.EndPrint, Sub(s, e)

                                                   If m_streams IsNot Nothing Then

                                                       For Each stream As Stream In m_streams
                                                           stream.Close()
                                                       Next

                                                       m_streams = Nothing
                                                   End If
                                               End Sub

            printDocument.Print()

        End If
    End Sub

End Module
