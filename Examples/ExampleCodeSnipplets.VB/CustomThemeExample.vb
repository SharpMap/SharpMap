Public Class CustomThemeExample

    Private testColumn As String
    Private styles As System.Collections.Generic.Dictionary(Of String, SharpMap.Styles.IStyle) = _
        New Dictionary(Of String, SharpMap.Styles.IStyle)
    Private defaultStyle As SharpMap.Styles.IStyle

    Public Sub New()
        'TODO FILL in STYLES
    End Sub

    Protected Overrides Sub Finalize()

        For Each kvp In Me.styles
            Dim style = kvp.Value
            If (TypeOf style Is IDisposable) Then
                DirectCast(style, IDisposable).Dispose()
            End If
        Next

    End Sub

    Public Sub CustomThemeExample()

        Dim map As New SharpMap.Map()
        Dim prv As New SharpMap.Data.Providers.OleDbPoint("", "points", "fid", "x", "y")
        Dim lyr As New SharpMap.Layers.VectorLayer("points", prv)

        testColumn = "Population"
        defaultStyle = lyr.Style

        lyr.Theme = New SharpMap.Rendering.Thematics.CustomTheme(AddressOf StyleForValue)
        map.Layers.Add(lyr)
        map.ZoomToExtents()

        Using bmp As System.Drawing.Bitmap = map.GetMap()
            bmp.Save("test.png")
        End Using

    End Sub

    Public Function StyleForValue(row As SharpMap.Data.FeatureDataRow) As SharpMap.Styles.IStyle

        Dim testValue As String = Convert.ToString(row(Me.testColumn))
        Dim result As SharpMap.Styles.IStyle = Nothing
        If (Me.styles.TryGetValue(testValue, result)) Then
            Return result
        End If
        Return Me.defaultStyle

    End Function

End Class
