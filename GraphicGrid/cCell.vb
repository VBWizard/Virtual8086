'========================================================================
' Grid Control Collection for .NET
' Matthew Hazlett (Jan 14, 2004)
'
' Description:
' This holds info about the different cells in the grid
'
' Change Log:
' Jan 14th: Created
'========================================================================
Public Class cCell
    Inherits System.Collections.CollectionBase

    ' A few vars for the properties of the collction
    '
    Public cellColor As Color
    Public cellbitmap As Bitmap = Nothing
    Public cellPoint As Point

    ' Empty new sub
    '
    Public Sub New()
    End Sub

    ' Color new sub
    '
    Public Sub New(ByVal cell As Point, ByVal myColor As Color)
        cellPoint = cell
        cellColor = myColor
    End Sub

    ' Bitmap new sub
    '
    Public Sub New(ByVal cell As Point, ByVal myBitmap As Bitmap)
        cellPoint = cell
        cellbitmap = myBitmap
    End Sub

    ' Add a color to the list
    '
    Public Overloads Sub add(ByVal cellPoint As Point, ByVal myColor As Color)
        list.Add(New cCell(cellPoint, myColor))
    End Sub

    ' Add a bitmap to the list
    '
    Public Overloads Sub add(ByVal cellPoint As Point, ByVal myBitmap As Bitmap)
        list.Add(New cCell(cellPoint, myBitmap))
    End Sub

    ' What item is this in the list?
    '
    Public Property item(ByVal number As Integer) As cCell
        Get
            Return list.Item(number)
        End Get
        Set(ByVal Value As cCell)
            list.Item(number) = Value
        End Set
    End Property

    ' Remove an item from the list.
    '
    Public Sub remove(ByVal cellPoint As Point)
        Dim x As Integer

        For x = list.Count - 1 To 0 Step -1
            Dim tmpcell As cCell = item(x)
            If tmpcell.cellPoint.Equals(cellPoint) Then
                If Not tmpcell.cellbitmap Is Nothing Then
                    tmpcell.cellbitmap.Dispose()
                End If
                list.RemoveAt(x)
            End If
        Next
    End Sub

    ' Override the default clear event and make sure we dispose any bitmap objects
    '
    Protected Overrides Sub OnClear()
        Dim x As Integer
        For x = list.Count - 1 To 0 Step -1
            If Not CType(list(x), cCell).cellbitmap Is Nothing Then
                CType(list(x), cCell).cellbitmap.Dispose()
            End If
            list.RemoveAt(x)
        Next
    End Sub

End Class
