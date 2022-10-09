'========================================================================
' Grid Control for .NET
' Matthew Hazlett (Jan 14, 2004)
'
' Description:
' Basicly what this does is take a picturebox and divide it into cells.
' You can then put bitmaps or colors in these cells.
'
' Change Log:
' Jan 14th: Created
'========================================================================
Public Class graphicgrid
    Inherits System.Windows.Forms.UserControl

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'UserControl1 overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents grid As System.Windows.Forms.PictureBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.grid = New System.Windows.Forms.PictureBox
        Me.SuspendLayout()
        '
        'grid
        '
        Me.grid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grid.Location = New System.Drawing.Point(0, 0)
        Me.grid.Name = "grid"
        Me.grid.Size = New System.Drawing.Size(128, 88)
        Me.grid.TabIndex = 0
        Me.grid.TabStop = False
        '
        'graphicgrid
        '
        Me.Controls.Add(Me.grid)
        Me.Name = "graphicgrid"
        Me.Size = New System.Drawing.Size(128, 88)
        Me.ResumeLayout(False)

    End Sub

#End Region

    ' Define Events, for descriptions see the event subs
    '
    Public Event gridClick(ByVal sender As Object, ByVal GridPoint As Point)
    Public Event gridDoubleClick(ByVal sender As Object, ByVal GridPoint As Point)
    Public Event gridMouseMove(ByVal sender As Object, ByVal GridPoint As Point)
    Public Event gridMouseUp(ByVal sender As Object, ByVal e As Windows.Forms.MouseEventArgs, ByVal GridPoint As Point)
    Public Event gridMouseDown(ByVal sender As Object, ByVal e As Windows.Forms.MouseEventArgs, ByVal GridPoint As Point)

    ' A few vars to hold cutom Properties
    '
    Public propShowGrid As Boolean = True
    Public propGridColor As Color = Color.White
    Public propCells As New Size(10, 10)
    Public CellSize As Size
    Public CellContains As New cCell

    ' Events, make sure to update the grid when these happen
    '
    Private Sub graphicgrid_BackColorChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.BackColorChanged
        setGrid()
    End Sub
    Private Sub graphicgrid_ForeColorChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.ForeColorChanged
        setGrid()
    End Sub
    Private Sub graphicgrid_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        setGrid()
    End Sub
    Private Sub graphicgrid_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.SizeChanged
        setGrid()
    End Sub
    Private Sub graphicgrid_Move(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Move
        setGrid()
    End Sub
    Private Sub graphicgrid_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        setGrid()
    End Sub

    ' Properties
    '
    Public Property ShowGrid() As Boolean
        Get
            Return propShowGrid
        End Get
        Set(ByVal Value As Boolean)
            propShowGrid = Value
            setGrid()
        End Set
    End Property
    Public Property GridColor() As Color
        Get
            Return propGridColor
        End Get
        Set(ByVal Value As Color)
            propGridColor = Value
            setGrid()
        End Set
    End Property
    Public Property Cells() As Size
        Get
            Return propCells
        End Get
        Set(ByVal Value As Size)
            propCells = Value
            setGrid()
        End Set
    End Property

    ' Sets the background image of the grid
    '
    Private Sub graphicgrid_BackgroundImageChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.BackgroundImageChanged
        grid.BackgroundImage = Me.BackgroundImage
    End Sub

    ' Resets or Applies new grid settings
    '
    Private Sub setGrid()
        grid.BackColor = Me.BackColor
        grid.ForeColor = Me.ForeColor

        CellSize.Width = Me.Width / propCells.Width
        CellSize.Height = Me.Height / propCells.Height

        grid.Invalidate()
    End Sub

    '  Sub: setCell
    '  Diz: Sets the contents of the cell by adding the attributes
    '       to the collection.  
    '  Use: object.setCell(new point(1, 1), color.red)
    '
    Public Overloads Sub setCell(ByVal Cell As Point, ByVal newColor As Color)
        CellContains.add(Cell, newColor)
        grid.Invalidate()
    End Sub

    '  Sub: setCell
    '  Diz: Sets the contents of a range of cells by adding the attributes
    '       to the collection.  
    '  Use: object.setCell({new point(1, 1), new point(1, 2)}, color.red)
    '
    Public Overloads Sub setCell(ByVal Cells As Point(), ByVal newColor As Color)
        Dim tmpPoint As Point
        For Each tmpPoint In Cells
            CellContains.add(tmpPoint, newColor)
        Next
        grid.Invalidate()
    End Sub

    '  Sub: setCell
    '  Diz: Sets the contents of the cell by adding the attributes
    '       to the collection.  
    '  Use: object.setCell(new point(1, 1), new bitmap("cell.bmp"))
    '
    Public Overloads Sub setCell(ByVal Cell As Point, ByVal newBitmap As Bitmap)
        CellContains.add(Cell, newBitmap)
        grid.Invalidate()
    End Sub

    '  Sub: setCell
    '  Diz: Sets the contents of a range of cells by adding the attributes
    '       to the collection.  
    '  Use: object.setCell({new point(1, 1), new point(1, 2)}, new bitmap("cell.bmp"))
    '
    Public Overloads Sub setCell(ByVal Cells As Point(), ByVal newBitmap As Bitmap)
        Dim tmpPoint As Point
        For Each tmpPoint In Cells
            CellContains.add(tmpPoint, newBitmap)
        Next
        grid.Invalidate()
    End Sub

    '  Sub: removeCell
    '  Diz: Removes a cell by deleting the attributes from the collection
    '  Use: object.removeCell(new point(1, 1))
    '
    Public Overloads Sub removeCell(ByVal Cell As Point)
        CellContains.remove(Cell)
        grid.Invalidate()
    End Sub

    '  Sub: removeCell
    '  Diz: Removes a range of cells by deleting the attributes from the collection
    '  Use: object.removeCell({new point(1, 1), new point(1, 2)})
    '
    Public Overloads Sub removeCell(ByVal Cells As Point())
        Dim tmpPoint As Point
        For Each tmpPoint In Cells
            CellContains.remove(tmpPoint)
        Next
        grid.Invalidate()
    End Sub

    '  Sub: clearCell
    '  Diz: Removes all the cells by deleting the attributes from the collection
    '  Use: object.clearCells
    '
    Public Sub clearCells()
        CellContains.Clear()
        grid.Invalidate()
    End Sub

    '  Event: grid_Paint
    '    Diz: Paints the grid
    '
    Private Sub grid_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles grid.Paint
        Dim contents As cCell
        Dim x As Integer
        Dim y As Integer

        ' If there are any custom attributes draw them out
        '
        For Each contents In CellContains
            If contents.cellbitmap Is Nothing Then
                e.Graphics.FillRectangle(New SolidBrush(contents.cellColor), contents.cellPoint.X * CellSize.Width, contents.cellPoint.Y * CellSize.Height, CellSize.Width, CellSize.Height)
            Else
                e.Graphics.DrawImage(contents.cellbitmap, contents.cellPoint.X * CellSize.Width, contents.cellPoint.Y * CellSize.Height, CellSize.Width, CellSize.Height)
            End If
        Next

        ' If they want the grid draw that as well
        '
        If ShowGrid = True Then
            For x = 0 To Cells.Width
                For y = 0 To Cells.Height
                    e.Graphics.DrawRectangle(New Pen(GridColor), x * CellSize.Width, y * CellSize.Height, CellSize.Width, CellSize.Height)
                Next
            Next
        End If
    End Sub

    '  Event: grid_Click
    '    Diz: When the user clicks inside the grid
    ' Return: A point structure indicating the grid's cell
    '
    Private Sub grid_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles grid.Click
        Dim clientPoint As System.Drawing.Point = Me.PointToClient(Me.MousePosition)
        RaiseEvent gridClick(Me, New Point(Int(clientPoint.X / CellSize.Width), Int(clientPoint.Y / CellSize.Height)))
    End Sub

    '  Event: grid_DoubleClick
    '    Diz: When the user clicks inside the grid
    ' Return: A point structure indicating the grid's cell
    '
    Private Sub grid_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles grid.DoubleClick
        Dim clientPoint As System.Drawing.Point = Me.PointToClient(Me.MousePosition)
        RaiseEvent gridDoubleClick(Me, New Point(Int(clientPoint.X / CellSize.Width), Int(clientPoint.Y / CellSize.Height)))
    End Sub

    '  Event: grid_MouseMove
    '    Diz: When the user moves the mouse inside the grid
    ' Return: A point structure indicating the grid's cell
    '
    Private Sub grid_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles grid.MouseMove
        Dim clientPoint As System.Drawing.Point = Me.PointToClient(Me.MousePosition)
        RaiseEvent gridMouseMove(Me, New Point(Int(clientPoint.X / CellSize.Width), Int(clientPoint.Y / CellSize.Height)))
    End Sub

    '  Event: grid_MouseUp
    '    Diz: When the user moves the mouse inside the grid
    ' Return: The mouse and A point structure indicating the grid's cell
    '
    Private Sub grid_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles grid.MouseUp
        Dim clientPoint As System.Drawing.Point = Me.PointToClient(Me.MousePosition)
        RaiseEvent gridMouseUp(Me, e, New Point(Int(clientPoint.X / CellSize.Width), Int(clientPoint.Y / CellSize.Height)))
    End Sub

    '  Event: grid_MouseDown
    '    Diz: When the user moves the mouse inside the grid
    ' Return: The mouse and A point structure indicating the grid's cell
    '
    Private Sub grid_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles grid.MouseDown
        Dim clientPoint As System.Drawing.Point = Me.PointToClient(Me.MousePosition)
        RaiseEvent gridMouseDown(Me, e, New Point(Int(clientPoint.X / CellSize.Width), Int(clientPoint.Y / CellSize.Height)))
    End Sub
End Class
