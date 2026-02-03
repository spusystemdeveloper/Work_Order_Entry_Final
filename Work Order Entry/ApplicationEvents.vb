Namespace My

    Partial Friend Class MyApplication

        Private Sub AppStart(ByVal sender As Object,
                  ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
            AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf ResolveAssemblies
        End Sub

        Private Function ResolveAssemblies(ByVal sender As Object, ByVal e As System.ResolveEventArgs) As Reflection.Assembly
            Dim desiredAssembly = New Reflection.AssemblyName(e.Name)

            If desiredAssembly.Name = "ExcelDataReader" Then
                Return Reflection.Assembly.Load(My.Resources.ExcelDataReader)
            ElseIf desiredAssembly.Name = "ExcelDataReader.DataSet" Then
                Return Reflection.Assembly.Load(My.Resources.ExcelDataReader_DataSet)
            ElseIf desiredAssembly.Name = "ICSharpCode.SharpZipLib" Then
                Return Reflection.Assembly.Load(My.Resources.ICSharpCode_SharpZipLib)

            Else
                Return Nothing
            End If
        End Function
    End Class

End Namespace

