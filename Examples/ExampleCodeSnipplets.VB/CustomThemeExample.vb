Public Class CustomThemeExample

    Private _testColumn As String
    Private ReadOnly _styles As Dictionary(Of String, SharpMap.Styles.IStyle) = _
        New Dictionary(Of String, SharpMap.Styles.IStyle)
    Private _defaultStyle As SharpMap.Styles.IStyle

    Public Sub New()
        'TODO FILL in STYLES
    End Sub

    Protected Overrides Sub Finalize()

        For Each kvp as KeyValuePair(Of String, SharpMap.Styles.IStyle) in  _styles
            kvp.Value.Dispose()
        Next

    End Sub

    Public Sub CustomThemeExample()

        Dim map As New SharpMap.Map()
        Dim prv As New SharpMap.Data.Providers.OleDbPoint("", "points", "fid", "x", "y")
        Dim lyr As New SharpMap.Layers.VectorLayer("points", prv)

        _testColumn = "Population"
        _defaultStyle = lyr.Style

        lyr.Theme = New SharpMap.Rendering.Thematics.CustomTheme(AddressOf StyleForValue)
        map.Layers.Add(lyr)
        map.ZoomToExtents()

        Using bmp As Drawing.Bitmap = map.GetMap()
            bmp.Save("test.png")
        End Using

    End Sub

    Public Function StyleForValue(row As SharpMap.Data.FeatureDataRow) As SharpMap.Styles.IStyle

        Dim testValue As String = Convert.ToString(row(_testColumn))
        Dim result As SharpMap.Styles.IStyle = Nothing
        If (_styles.TryGetValue(testValue, result)) Then
            Return result
        End If
        Return _defaultStyle

    End Function

End Class
