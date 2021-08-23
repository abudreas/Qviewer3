Imports System.ComponentModel
Imports System.Data
Public Class MainWindow
    Const maxTable As Integer = 5
    Dim WithEvents OpenFileDialog1 As Microsoft.Win32.OpenFileDialog

    Enum Qselection As Integer
        onlyUnattempeted = 0
        attempetedWrong = 1
        both = 3
        all = 4
    End Enum
    Dim WithEvents qForm As New Questions
    Dim infoStrings As New List(Of String)
    Dim tablesName As New List(Of String)
    Dim shoWconter As Integer
    Dim filePath As String = ""
    Dim settings As String
    Dim combo2Indx As Integer = 0
    Dim tableLoaded As Boolean
    Dim stats(maxTable)(,) As String
    Dim cateogary(maxTable)() As String
    Dim mask As String
    <CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")>
    Sub prepareDB()
        tableLoaded = False
        Dim sql As String
        Try
            Dim cmd As New OleDb.OleDbCommand


            cmd.Connection = qForm.con
            qForm.con.Open()
            Dim restrictions(3) As String
            restrictions(3) = "TABLE"
            Dim res = qForm.con.GetSchema("Tables", restrictions)
            comboBox2.Items.Clear()
            tablesName.Clear()
            Dim i As Integer = 0
            Do While i < res.Rows.Count And i < maxTable
                tablesName.Add(res.Rows(i)(2))
                sql = "SELECT TableInfo FROM " & res.Rows(i)(2)
                cmd.CommandText = sql
                Dim s = cmd.ExecuteScalar()
                Dim temp() As String = loadCatg(tablesName(i))
                ' ReDim Preserve cateogary(i)(UBound(temp))
                cateogary(i) = temp
                stats(i) = loadStats(tablesName(i), cateogary(i))
                comboBox2.Items.Add(prossecInfo(s, "info"))
                i += 1
            Loop




        Catch ex As Exception
            MsgBox(ex.Message)
        Finally
            qForm.con.Close()

        End Try
        Dim s2 As String = prossecInfo(settings, "table")
        If s2 <> "" AndAlso Integer.Parse(s2) < comboBox2.Items.Count Then
            comboBox2.SelectedIndex = Integer.Parse(s2)
        Else
            settings = "*path:" & filePath & "*table:0*"
            comboBox2.SelectedIndex = 0

        End If
        combo2Indx = comboBox2.SelectedIndex
    End Sub
    Sub prepareTable()
        Try
            qForm.con.Open()
            For i As Integer = 0 To tablesName.Count - 1
                stats(i) = loadStats(tablesName(i), cateogary(i))
            Next
        Catch ex As Exception
            MsgBox(ex.Message)
        Finally
            qForm.con.Close()
        End Try


    End Sub
    Sub drawTable(ByVal arr As String(,))
        tableGrid.Children.Clear()
        tableGrid.RowDefinitions.Clear()
        tableGrid.ColumnDefinitions.Clear()

        For x As Integer = 0 To UBound(arr, 2)
            If tableGrid.RowDefinitions.Count <= x Then
                Dim row As New RowDefinition
                row.Height = GridLength.Auto
                tableGrid.RowDefinitions.Add(row)
            End If
            For i As Integer = 0 To UBound(arr, 1)
                If x = 0 And tableGrid.ColumnDefinitions.Count <= i Then
                    Dim cl As New ColumnDefinition
                    cl.Width = GridLength.Auto
                    'cl.MaxWidth = 150
                    tableGrid.ColumnDefinitions.Add(cl)
                End If
                Dim t As New TextBlock
                Dim r As New Rectangle()
                If x = 0 Then
                    r.Fill = Brushes.Gray
                ElseIf x = UBound(arr, 2) Then
                    r.Fill = Brushes.LightGray
                Else
                    r.Fill = Brushes.Ivory
                End If

                r.Stroke = Brushes.Black
                tableGrid.Children.Add(r)
                Grid.SetColumn(r, i)
                Grid.SetRow(r, x)
                t.TextWrapping = TextWrapping.Wrap
                t.MaxWidth = 250
                t.Text = addSpace(arr(i, x))
                tableGrid.Children.Add(t)
                Grid.SetColumn(t, i)
                Grid.SetRow(t, x)

            Next
        Next
    End Sub
    <CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")>
    Function loadStats(ByVal myTable As String, ByVal catg() As String) As String(,)
        Dim sql As String
        Dim x, t As Double
        Dim arrTable(3, catg.Length + 1) As String
        arrTable(0, 0) = "category"
        arrTable(1, 0) = "total"
        arrTable(2, 0) = "attempted"
        arrTable(3, 0) = "score"

        Dim cmd As New OleDb.OleDbCommand
        cmd.Connection = qForm.con

        For i As Integer = 0 To UBound(catg)
            arrTable(0, i + 1) = catg(i)
            sql = "SELECT count(ID) FROM " & myTable & " WHERE catg ='" & catg(i) & "'"
            cmd.CommandText = sql
            arrTable(1, i + 1) = cmd.ExecuteScalar
            sql += " AND solved <> '0'"
            cmd.CommandText = sql
            arrTable(2, i + 1) = cmd.ExecuteScalar
            sql = sql.Replace(" <> '0'", " = '2'")
            cmd.CommandText = sql
            x = cmd.ExecuteScalar
            t = Convert.ToInt32(arrTable(2, i + 1))
            If t <> 0 Then
                x = (x / t)
            End If

            arrTable(3, i + 1) = Math.Round(x, 2) * 100 & " %"
        Next
        sql = "SELECT count(ID) FROM " & myTable
        cmd.CommandText = sql
        arrTable(1, UBound(arrTable, 2)) = cmd.ExecuteScalar
        sql += " WHERE solved <> '0'"
        cmd.CommandText = sql
        arrTable(2, UBound(arrTable, 2)) = cmd.ExecuteScalar
        sql = sql.Replace(" <> '0'", " = '2'")
        cmd.CommandText = sql
        x = cmd.ExecuteScalar
        t = Convert.ToInt32(arrTable(2, UBound(arrTable, 2)))
        If t <> 0 Then
            x = (x / t)
        End If
        arrTable(3, UBound(arrTable, 2)) = Math.Round(x, 2) * 100 & " %"
        arrTable(0, UBound(arrTable, 2)) = "total"

        Return arrTable
    End Function
    Function ProcessIdString(ByVal TableN As String, ByVal catogary As String, ByVal selectoption As Qselection) As String
        Dim sql As String = "SELECT * FROM " + TableN

        Select Case selectoption
            Case Qselection.onlyUnattempeted
                sql += " WHERE (solved = '0' or solved is null)"
            Case Qselection.attempetedWrong
                sql += " WHERE solved = '1'"
            Case Qselection.both
                sql += " WHERE (solved = '0' or solved = '1')"
        End Select
        If catogary IsNot Nothing And catogary <> "All" Then
            If selectoption = Qselection.all Then
                sql += " WHERE catg = '" + catogary + "'"
            Else
                sql += " and catg = '" + catogary + "'"
            End If
        End If

        Return sql
    End Function
    <CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")>
    Function loadQ(ByVal Idstring As String)
        If MaskedTextBox1.Text = "" Or Not IsNumeric(MaskedTextBox1.Text) Then
            MaskedTextBox1.Text = Str(50)
        End If
        Dim newResult As New List(Of Object())
        shoWconter = 0
        Try
            Dim idList As New List(Of Object())
            Dim reader As OleDb.OleDbDataReader
            Dim cmd As New OleDb.OleDbCommand
            cmd.Connection = qForm.con
            cmd.CommandText = Idstring
            qForm.con.Open()
            reader = cmd.ExecuteReader()
            While reader.Read()
                Dim n(13) As Object
                reader.GetValues(n)
                idList.Add(n)
            End While
            reader.Close()


            If radioButton2.IsChecked Then
                cmd.CommandText = "SELECT TableInfo FROM " & tablesName(comboBox2.SelectedIndex)
                Dim catg As String
                If comboBox1.SelectedIndex < 0 Then
                    catg = "All"
                Else
                    catg = comboBox1.SelectedItem.ToString

                End If
                catg = prossecInfo(cmd.ExecuteScalar, catg)
                If catg = "" Or checkBox1.IsChecked Or checkBox2.IsChecked Then
                    shoWconter = 0
                Else
                    shoWconter = Convert.ToInt32(catg)
                End If
                newResult = idList
            Else
                Dim arr() As Integer = randomizeId(idList.Count, MaskedTextBox1.Text)

                For Each i As Integer In arr
                    newResult.Add(idList.Item(i))
                Next
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        Finally
            qForm.con.Close()
        End Try
        Return newResult
    End Function

    Private Sub StartBtn_Click(sender As Object, e As EventArgs) Handles StartBtn.Click
        Dim opt As Qselection
        If checkBox1.IsChecked And checkBox2.IsChecked Then
            opt = Qselection.both
        ElseIf checkBox1.IsChecked Then
            opt = Qselection.onlyUnattempeted
        ElseIf checkBox2.IsChecked Then
            opt = Qselection.attempetedWrong
        Else
            opt = Qselection.all
        End If
        qForm.result = loadQ(ProcessIdString(tablesName(ComboBox2.SelectedIndex), ComboBox1.SelectedItem, opt))
        Dim catg As String
        If ComboBox1.SelectedIndex < 0 Then
            catg = "All"
        Else
            catg = ComboBox1.SelectedItem.ToString
        End If

        qForm.init(radioButton1.IsChecked, tablesName(comboBox2.SelectedIndex), radioButton3.IsChecked, catg, shoWconter)

    End Sub
    Function randomizeId(ByVal count As Integer, ByVal size As Integer) As Integer()
        Randomize()
        Dim cbylist, idlist As New List(Of Integer)
        For i As Integer = 0 To count - 1
            idlist.Add(i)
            cbylist.Add(i)
        Next
        For Each item As Integer In idlist
            cbylist.Remove(item)
            cbylist.Insert(Convert.ToInt32((count - 2) * Rnd()), item)
        Next item
        If size < count Then
            cbylist.RemoveRange(size, count - size)
        End If

        Return cbylist.ToArray()
    End Function

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        initComp()
        OpenFileDialog1.InitialDirectory = Environment.CurrentDirectory

        OpenFileDialog1.FileName = ""
        qForm.initConnection(fetchFiles())

        prepareDB()
        mask = MaskedTextBox1.Text
        radioButton1.ToolTip = "Solve a random set of questions acoording to the selected criteria." & vbNewLine & "you can't skip questions"
        radioButton2.ToolTip = "Questions are not random." & vbNewLine & "solve and skip as you like." & vbNewLine & "your progress will be saved"
        radioButton3.ToolTip = "Same as Test Mode , with added timer" & vbNewLine & "30 sec for one question"
        button1.ToolTip = "Reset and delet All your records in this Q bank" & vbNewLine & "No question will be deleted"
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")>
    Function loadCatg(ByVal myTable As String) As String()
        Dim sql As String
        Dim li As New List(Of String)

        '  Try
        Dim cmd As New OleDb.OleDbCommand
        Dim reader As OleDb.OleDbDataReader
        cmd.Connection = qForm.con
        sql = "Select Distinct catg FROM " & myTable
        'qForm.con.Open()
        cmd.CommandText = sql
        reader = cmd.ExecuteReader()

        While reader.Read()
            Dim n(0) As Object
            reader.GetValues(n)
            li.Add(n(0).ToString)
            'comboBox1.Items.Add()
        End While
        ' Catch ex As Exception
        ' MsgBox(ex.Message)
        ' Finally
        ' qForm.con.Close()
        ' End Try
        Return li.ToArray
    End Function
    Public Shared Function prossecInfo(ByVal info As String, ByVal theOPt As String, Optional ByVal setValue As String = "") As String
        Dim cnt As Integer = 0
        Dim opt(1) As String
        Dim found As Boolean = False
        Dim s As String = ""
        For i As Integer = 0 To info.Length - 1
            If info.Chars(i) = "*" And Not found Then
                cnt = i + 1
            ElseIf info.Chars(i) = ":" Then
                If info.Substring(cnt, i - cnt) = theOPt Then
                    found = True
                End If
            ElseIf info.Chars(i) = "*" And found Then
                If setValue = "" Then
                    s = info.Substring(cnt + theOPt.Length + 1, i - (cnt + theOPt.Length + 1))
                    found = False
                Else
                    s = info.Remove(cnt + theOPt.Length + 1, i - (cnt + theOPt.Length + 1))
                    s = s.Insert(cnt + theOPt.Length + 1, setValue)
                End If

            End If
        Next
        Return s
    End Function
    <CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")>
    Sub reset()
        Dim sql As String


        Try
            Dim cmd As New OleDb.OleDbCommand

            cmd.Connection = qForm.con
            sql = "UPDATE " & tablesName(comboBox2.SelectedIndex) & " SET solved = '0' "
            qForm.con.Open()
            cmd.CommandText = sql
            cmd.ExecuteNonQuery()
            sql = "UPDATE " & tablesName(comboBox2.SelectedIndex) & " SET TableInfo = '*info:" & comboBox2.SelectedItem.ToString & "*' WHERE ID = 1"
            cmd.CommandText = sql
            cmd.ExecuteNonQuery()
        Catch ex As Exception
            MsgBox(ex.Message)
        Finally
            qForm.con.Close()
        End Try
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If MsgBox("Are Sure you want to reset all records for '" & ComboBox2.SelectedItem.ToString & "' Q Bank ?", MsgBoxStyle.OkCancel, "Reset Record") = MsgBoxResult.Ok Then
            reset()
            prepareTable()
            drawTable(stats(comboBox2.SelectedIndex))
        End If

    End Sub

    Private Sub qForm_Closing(sender As Object, e As CancelEventArgs) Handles qForm.Closing
        e.Cancel = True
        prepareTable()
        drawTable(stats(comboBox2.SelectedIndex))

    End Sub
    Function fetchFiles() As String

        Dim str As IO.StreamReader
        Try

            If IO.File.Exists("Settings") Then
                str = IO.File.OpenText("Settings")
                settings = str.ReadToEnd
                filePath = prossecInfo(settings, "path")
                str.Close()
            Else
                settings = ""
                Dim wr = IO.File.CreateText("Settings")
                wr.Close()
            End If
            If Not IO.File.Exists(filePath) Then
                OpenFileDialog1.ShowDialog()
                filePath = OpenFileDialog1.FileNames(0)
                Dim x As Integer = 0
                If filePath = "" Or filePath Is Nothing Then


                    End
                    Exit Function

                End If

                Dim wrt As IO.StreamWriter = IO.File.CreateText("Settings")
                If settings = "" Then
                    settings = "*path:t*table:0*"
                    settings = prossecInfo(settings, "path", filePath)
                End If
                wrt.Write(settings)
                wrt.Close()
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
            Me.Close()
            End
        End Try
        Return filePath
    End Function

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles radioButton2.Checked

        MaskedTextBox1.IsEnabled = False

    End Sub
    Private Sub radioButton2_Unchecked(sender As Object, e As RoutedEventArgs) Handles radioButton2.Unchecked
        MaskedTextBox1.IsEnabled = True
    End Sub

    Private Sub OpenDBFileToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenDBFileToolStripMenuItem.Click
        Dim newfile As String
        OpenFileDialog1.InitialDirectory = IO.Path.GetDirectoryName(filePath)
        OpenFileDialog1.ShowDialog()
        newfile = OpenFileDialog1.FileName
        If newfile = filePath Or newfile = "" Or newfile Is Nothing Then
            Exit Sub
        Else
            filePath = newfile
            Dim wrt As IO.StreamWriter = IO.File.CreateText("Settings")
            If settings = "" Then
                settings = "*path:t*table:0*"
            End If
            settings = prossecInfo(settings, "path", filePath)
            wrt.Write(settings)
            wrt.Close()
            qForm.initConnection(filePath)
            prepareDB()
            'prepareCateogary()
            'prepareTable()
            'loadStats()
        End If
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        '  MsgBox("Qviewer 3.0 by Abudreas . 21 AUG 2021 ")
        Windows.MessageBox.Show("Qviewer 3.0 by Abudreas . 21 AUG 2021 ")
    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles comboBox2.SelectionChanged
        If (comboBox2.SelectedIndex <> combo2Indx Or Not tableLoaded) And Not comboBox2.Items.Count = 0 Then
            Dim wrt As IO.StreamWriter = IO.File.CreateText("Settings")
            If settings = "" Then
                settings = "*path:" & filePath & "*table:0*"
            End If
            settings = prossecInfo(settings, "table", comboBox2.SelectedIndex)
            wrt.Write(settings)
            wrt.Close()
            'prepareCateogary()
            'prepareTable()
            drawTable(stats(comboBox2.SelectedIndex))
            drawCatg(cateogary(comboBox2.SelectedIndex))
            tableLoaded = True
            combo2Indx = comboBox2.SelectedIndex
        End If

    End Sub

    Sub initComp()
        OpenFileDialog1 = New Microsoft.Win32.OpenFileDialog()
        OpenFileDialog1.FileName = "OpenFileDialog1"
        OpenFileDialog1.Filter = "Access file |*.accdb"
    End Sub

    Sub drawCatg(ByVal catg() As String)
        comboBox1.Items.Clear()
        comboBox1.SelectedIndex = -1
        comboBox1.Text = "Select Category"
        comboBox1.Items.Add("All")
        For i As Integer = 0 To UBound(catg)
            comboBox1.Items.Add(catg(i))
        Next
    End Sub
    Function addSpace(ByVal t As Object) As String
        Return " " & t.ToString & " "
    End Function

    Private Sub MaskedTBchanged(sender As Object, e As TextChangedEventArgs) Handles MaskedTextBox1.TextChanged
        If (IsNumeric(MaskedTextBox1.Text) AndAlso Int(MaskedTextBox1.Text) <= 50) Or MaskedTextBox1.Text = "" Then
            mask = MaskedTextBox1.Text
        Else
            MaskedTextBox1.Text = mask
        End If
    End Sub
End Class
