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

    'Public Sub CustomThemeExample()

    '    Dim map As New SharpMap.Map()
    '    Dim prv As New SharpMap.Data.Providers.DataTablePoint("", "points", "fid", "x", "y")
    '    Dim lyr As New SharpMap.Layers.VectorLayer("points", prv)

    '    _testColumn = "Population"
    '    _defaultStyle = lyr.Style

    '    lyr.Theme = New SharpMap.Rendering.Thematics.CustomTheme(AddressOf StyleForValue)
    '    map.Layers.Add(lyr)
    '    map.ZoomToExtents()

    '    Using bmp As Drawing.Bitmap = map.GetMap()
    '        bmp.Save("test.png")
    '    End Using

    'End Sub

    Public Function StyleForValue(row As SharpMap.Data.FeatureDataRow) As SharpMap.Styles.IStyle

        Dim testValue As String = Convert.ToString(row(_testColumn))
        Dim result As SharpMap.Styles.IStyle = Nothing
        If (_styles.TryGetValue(testValue, result)) Then
            Return result
        End If
        Return _defaultStyle

    End Function

End Class


public Class UniqueValuesThemeTest

 Private delObjektLabelBez As SharpMap.Layers.LabelLayer.GetLabelMethod
  Private delObjektLabelPos As SharpMap.Layers.LabelLayer.GetLocationMethod
  Private delObjektLabelStyle As SharpMap.Rendering.Thematics.CustomTheme.GetStyleMethod
  Private delObjektStyle As SharpMap.Rendering.Thematics.CustomTheme.GetStyleMethod
  '
    Private _map As SharpMap.Map
 
    public class OtherClassItem
        public Property Name  () As String
        Public Property Datasource() As string
        Public Property Table() As string
        Public Property Field() As string
    End Class

    public class OtherClass 
        Inherits System.Collections.Generic.List(Of OtherClassItem)
        public Property Name  () As String
    End Class

    Private _polygonTheme As SharpMap.Rendering.Thematics.UniqueValuesTheme(Of Int32) = CreatePolygonTheme()

    Private Shared Function CreatePolygonTheme() As SharpMap.Rendering.Thematics.Uniquevaluestheme(of Int32)

        Dim styleMap As new Dictionary(Of int32, SharpMap.Styles.IStyle)
        Dim s As SharpMap.Styles.VectorStyle
        '1
        s = New SharpMap.Styles.VectorStyle()
        s.Line = new Drawing.Pen(Drawing.Color.Black, 1)
        s.Fill = new Drawing.SolidBrush(Drawing.Color.Green)
        s.EnableOutline = True
        styleMap.Add(1, s)
        '2
        s = New SharpMap.Styles. VectorStyle()
        s.Outline=new Drawing.Pen(Drawing.Color.Black, 1)
        s.Fill = New Drawing.SolidBrush(Drawing.Color.Blue)
        s.EnableOutline = True
        styleMap.Add(2, s)
        '3
        s = New SharpMap.Styles.VectorStyle()
        s.Outline =new Drawing.Pen(Drawing.Color.Black, 1)
        s.Fill = New Drawing.SolidBrush(Drawing.Color.Red)
        s.EnableOutline = True
        styleMap.Add(2, s)


        Return new SharpMap.Rendering.Thematics.UniqueValuesTheme(Of Integer)("FilterColumn", styleMap, SharpMap.Styles.VectorStyle.CreateRandomPolygonalStyle())
    End Function

 Public Sub CreateLayers(ByVal config as OtherClass)
     '
     delObjektLabelBez = New SharpMap.Layers.LabelLayer.GetLabelMethod(AddressOf LabelObjektBezeichnung)
     delObjektLabelPos = New SharpMap.Layers.LabelLayer.GetLocationMethod(AddressOf LabelBezeichnungPos)
     delObjektStyle = New SharpMap.Rendering.Thematics.CustomTheme.GetStyleMethod(AddressOf GetObjektStyle)
     delObjektLabelStyle = New SharpMap.Rendering.Thematics.CustomTheme.GetStyleMethod(AddressOf GetObjektLabelStyle)
     '
     For each itm as OtherClassItem in config
        Select Case itm.Name
           Case "Name1"
              Dim smp as New SharpMap.Data.Providers.PostGIS(itm.Datasource, itm.Table, itm.Field)
              Dim vLay as New SharpMap.Layers.VectorLayer(itm.Name, smp)
              'Dim layTheme = New SharpMap.Rendering.Thematics.CustomTheme(delObjektStyle)
              vLay.Theme = Me._polygonTheme 'layTheme
              '
              Dim labLay As New SharpMap.Layers.LabelLayer("Lable: " & itm.Name)
              Dim labTheme as New SharpMap.Rendering.Thematics.CustomTheme(delObjektLabelStyle)
              labLay.DataSource = vLay.DataSource
          labLay.Theme = labTheme
              labLay.LabelStringDelegate = delObjektLabelBez
          labLay.LabelPositionDelegate = delObjektLabelPos
              '
              _map.Layers.Add(vLay)
              _Map.Layers.Add(labLay)
              '
           Case "Name2"
              Dim smp as New SharpMap.Data.Providers.PostGIS(itm.Datasource, itm.Table, itm.Field)
              Dim vLay as New SharpMap.Layers.VectorLayer(itm.Name, smp)
              '
              'other code
              '
              Dim labLay As New SharpMap.Layers.LabelLayer("Lable: " & itm.Name)
              Dim labTheme as New SharpMap.Rendering.Thematics.CustomTheme(delObjektLabelStyle)
              labLay.DataSource = vLay.DataSource
          labLay.Theme = labTheme
              labLay.LabelStringDelegate = delObjektLabelBez
          labLay.LabelPositionDelegate = delObjektLabelPos
              '
              _map.Layers.Add(vLay)
              _map.Layers.Add(labLay)
              '
        End Select
        '
     Next
     '
     'Other Code .....
     '
  End Sub
  
    Private Shared function LabelObjektBezeichnung(fdr As SharpMap.Data.FeatureDataRow) As String
        return "The quick brown fox ..."
    End function

    Private Shared Function LabelBezeichnungPos(fdr As SharpMap.Data.FeatureDataRow) As GeoAPI.Geometries.Coordinate
        return new GeoAPI.Geometries.Coordinate(0, 0)
    End Function
    
    Private shared function GetObjektLabelStyle(fdr As SharpMap.Data.FeatureDataRow) As SharpMap.Styles.LabelStyle

    End function
  Private shared Function GetObjektStyle(fdr As SharpMap.Data.FeatureDataRow) As SharpMap.Styles.VectorStyle
    '
    Dim style As New SharpMap.Styles.VectorStyle
    Dim c as Drawing.Color
    Dim p as Drawing.Pen
    Dim b as Drawing.Brush
    '
    Select Case fdr.Table.TableName
       Case "Name1"  '-- Polygon
          Select Case fdr("FilterColumn")
             Case 1
               c = Drawing.Color.Green
               p = New Drawing.Pen(Drawing.Color.Black, 1)
           b = New Drawing.SolidBrush(c)
             Case 2
               c = Drawing.Color.Blue
               p = New Drawing.Pen(Drawing.Color.Black, 1)
           b = New Drawing.SolidBrush(c)
             Case Else
               c = Drawing.Color.Red
               p = New Drawing.Pen(Drawing.Color.Black, 1)
           b = New Drawing.SolidBrush(c)
          End Select
          '
          style.Fill = b
      style.Outline = p
          '
       Case "Name2"' -- Lines
         '
         ' this Code will not Run
         '
       Case Else
    End Select
    '
    Return style  
    '
  End Function
  '

End Class
