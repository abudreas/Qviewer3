﻿Imports System.Data
Imports System.ComponentModel

Public Class Questions
    Dim WithEvents timer1 As New System.Timers.Timer
    Public con As SQLite.SQLiteConnection
    Public result As List(Of Object())
    Dim radioBrush As Brush
    Dim magnification As Double = 1
    ' Public FilePath As String
    Enum res As Integer
        id = 0
        question = 1
        option1 = 2
        option2 = 3
        option3 = 4
        option4 = 5
        option5 = 6
        correct = 7
        explanation = 8
        category = 9
        solved = 10
        quimage = 11
        expImage = 12
    End Enum
    Dim showCounter, answeredCounter, correctCounter As Integer
    Dim answered() As Integer
    Dim answers() As String
    Dim testMode As Boolean
    Dim timeTrial As Boolean
    Dim tableName As String
    Dim category As String
    Dim timeCounter As Integer = 30

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()
        radioBrush = RadioButton1.Background.Clone

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Function loadImage(ByVal img As String) As BitmapImage

        '  Dim image As New Image
        Dim bit As New BitmapImage

        Dim bytes() As Byte = Convert.FromBase64String(img)
        Using ms As New IO.MemoryStream(bytes)
            bit.BeginInit()
            bit.StreamSource = ms
            bit.CacheOption = BitmapCacheOption.OnLoad
            bit.EndInit()
        End Using
        'image.Source = bit
        Return bit
    End Function
    Public Sub init(ByVal tMode As Boolean, ByVal tbname As String, ByVal timer As Boolean, ByVal catg As String, Optional counter As Integer = 0)

        timer1.Stop()
        testMode = tMode
        tableName = tbname
        category = catg
        Title = catg
        timeTrial = timer
        If result Is Nothing OrElse result.Count = 0 Then
            MsgBox("No Questions matching the selected criteria !", MsgBoxStyle.OkOnly, "Error !")
            ' Me.Visibility = Visibility.Hidden

            Exit Sub
        End If
        correctCounter = 0

        answeredCounter = 0
        ReDim answers(result.Count - 1)
        ReDim answered(result.Count - 1)
        unselectAll()
        label3.Text = ""
        label2.Text = ""
        ComboBox1.Items.Clear()
        For i As Integer = 1 To result.Count
            ComboBox1.Items.Add(i)
        Next

        ComboBox1.SelectedIndex = 0
        'I have to put down here because of the stupid change index Event
        showCounter = counter
        If testMode Or timer Then
            ComboBox1.Visibility = Visibility.Hidden
        Else
            ComboBox1.Visibility = Visibility.Visible
        End If

        Me.Show()


        showNextQuestion()
        timer1.Interval = 1000
        If timer Then
            timeCounter = 30
            timer1.Start()
            button2.Visibility = Visibility.Hidden
            label4.Visibility = Visibility.Visible
            label4.Foreground = Brushes.Black
            label4.Text = timeCounter
        Else
            button2.Visibility = Visibility.Visible
            label4.Visibility = Visibility.Hidden

        End If
    End Sub



    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles button1.Click
        nextq()


    End Sub

    Private Sub changeQ(ByVal forword As Integer)
        Dim radio As RadioButton = captureSelection()
        If showCounter < 0 Then Exit Sub


        If answered(showCounter) = 0 And forword = 1 Then

            If radio Is Nothing And testMode Then
                MsgBox("You have to select an answer to progress")
            ElseIf Not testMode And radio Is Nothing Then
                showCounter += forword
                unselectAll()
                If timeTrial Then
                    timeCounter = 30
                    label4.Foreground = Brushes.Black
                    label4.Text = timeCounter
                    timer1.Start()
                End If
                showNextQuestion()
            Else
                checkAnswer(radio)
                If timeTrial Then
                    timer1.Stop()
                End If
            End If

        ElseIf answered(showCounter) = 1 Or forword = -1 Then
            showCounter += forword
            unselectAll()

            showNextQuestion()
            If timeTrial Then
                timeCounter = 30
                label4.Foreground = Brushes.Black
                label4.Text = timeCounter
                timer1.Start()
            End If
            If RadioButton1.Content.text = answers(showCounter) Then
                RadioButton1.IsChecked = True
            ElseIf RadioButton2.Content.text = answers(showCounter) Then
                RadioButton2.IsChecked = True
            ElseIf RadioButton4.Content.text = answers(showCounter) Then
                RadioButton4.IsChecked = True
            ElseIf RadioButton3.Content.text = answers(showCounter) Then
                RadioButton3.IsChecked = True
            ElseIf RadioButton5.Content.text = answers(showCounter) Then
                RadioButton5.IsChecked = True
            End If
            radio = captureSelection()
            If radio IsNot Nothing Then
                checkAnswer(captureSelection())
                RichTextBox1.BringIntoView()
            End If
        Else
            unselectAll()

            showNextQuestion()
        End If

    End Sub
    Sub unselectAll()

        RadioButton1.IsChecked = False
        RadioButton2.IsChecked = False
        RadioButton3.IsChecked = False
        RadioButton4.IsChecked = False
        RadioButton5.IsChecked = False

        RadioButton1.Content.Background = radioBrush
        RadioButton2.Content.Background = radioBrush
        RadioButton3.Content.Background = radioBrush
        RadioButton4.Content.Background = radioBrush
        RadioButton5.Content.Background = radioBrush
    End Sub
    Sub checkAnswer(ByRef cradio As RadioButton)
        Dim iscorrect As Integer

        '////////////////////////////
        If cradio Is Nothing And testMode Then
            MsgBox("You have to select an answer to progress")

            Exit Sub
        End If
        '//////////////////////////
        hilightAnswer()

        If cradio.Content.text = result.Item(showCounter)(res.correct) Then
            label1.Foreground = Brushes.Green
            label1.Text = "Correct!"
            iscorrect = 1
        Else
            cradio.Content.Background = Brushes.Red
            label1.Foreground = Brushes.Red
            label1.Text = "Wrong !"
            iscorrect = 0
        End If


        If answered(showCounter) = 0 Then
            answers(showCounter) = cradio.Content.text
            answeredCounter += 1
            correctCounter += iscorrect

            label3.Text = Math.Round(correctCounter / answeredCounter, 2) * 100 & " % Correct"
        End If
        answered(showCounter) = 1
        RichTextBox2.Visibility = Visibility.Visible
        PictureBox1.Visibility = Visibility.Visible
        scroller.BringIntoView()

        'Panel1.Visibility = Visibility.Visible
    End Sub

    Private Sub hilightAnswer()
        If RadioButton1.Content.text = result.Item(showCounter)(res.correct) Then
            RadioButton1.Content.Background = Brushes.Green

        ElseIf RadioButton2.Content.text = result.Item(showCounter)(res.correct) Then
            RadioButton2.Content.Background = Brushes.Green

        ElseIf RadioButton3.Content.text = result.Item(showCounter)(res.correct) Then
            RadioButton3.Content.Background = Brushes.Green

        ElseIf RadioButton4.Content.text = result.Item(showCounter)(res.correct) Then
            RadioButton4.Content.Background = Brushes.Green

        ElseIf RadioButton5.Content.text = result.Item(showCounter)(res.correct) And RadioButton5.IsVisible Then
            RadioButton5.Content.Background = Brushes.Green
        End If
    End Sub

    Private Sub showNextQuestion()

        label2.Text = "Question " & showCounter + 1 & " Of " & result.Count
        If showCounter >= result.Count() Then
            showCounter = result.Count - 1
            If timeTrial Then
                MsgBox("you Finished !" & vbNewLine & " your score is " & Math.Round(correctCounter / answeredCounter, 2) * 100 & " %")
                timer1.Stop()
                Me.Hide()

            Else
                MsgBox("No more Questions")

            End If
            Exit Sub
        End If
        ComboBox1.SelectedIndex = showCounter
        Dim qu = result.Item(showCounter)
        label1.Text = ""
        '  RichTextBox1.Inlines.Clear()
        RichTextBox1.Text = (qu(res.question))

        ' RichTextBox1.Height = RichTextBox1.ClientRectangle.Height
        qu(res.correct) = qu(res.correct).Replace(vbCrLf, " ")
        RadioButton1.Content.text = qu(res.option1)
        RadioButton2.Content.text = qu(res.option2)
        RadioButton3.Content.text = qu(res.option3)
        RadioButton4.Content.text = qu(res.option4)
        RadioButton1.Content.text = RadioButton1.Content.text.Replace(vbCrLf, " ")
        RadioButton2.Content.text = RadioButton2.Content.text.Replace(vbCrLf, " ")
        RadioButton3.Content.text = RadioButton3.Content.text.Replace(vbCrLf, " ")
        RadioButton4.Content.text = RadioButton4.Content.text.Replace(vbCrLf, " ")
        If qu(res.option5) <> "" And qu(res.option5) <> " " And qu(res.option5) IsNot DBNull.Value Then
            RadioButton5.Visibility = Visibility.Visible
            RadioButton5.Content.text = qu(res.option5)
            RadioButton5.Content.text = RadioButton5.Content.text.Replace(vbCrLf, " ")
        Else
            RadioButton5.Visibility = Visibility.Hidden
        End If
        If answered(showCounter) = 0 Then
            RichTextBox2.Visibility = Visibility.Collapsed
            ' RichTextBox2.Text = ""
            PictureBox1.Visibility = Visibility.Collapsed
        Else
            RichTextBox2.Visibility = Visibility.Visible
            PictureBox1.Visibility = Visibility.Visible
            ' RichTextBox2.Text = qu(res.explanation)

        End If
        ' RichTextBox2.Inlines.Clear()
        ' RichTextBox2.Inlines.Add(qu(res.explanation))
        RichTextBox2.Text = qu(res.explanation)
        If qu(res.expImage) Is DBNull.Value OrElse qu(res.expImage) = "" Then
            PictureBox1.Source = Nothing
            PictureBox1.Width = 0
            PictureBox1.Height = 0
        Else

            PictureBox1.Source = loadImage(qu(res.expImage))
            PictureBox1.Stretch = Stretch.Uniform
            PictureBox1.Width = PictureBox1.Source.Width
            PictureBox1.Height = PictureBox1.Source.Height
        End If
        If qu(res.quimage) Is DBNull.Value OrElse qu(res.quimage) = "" Then
            PictureBox2.Source = Nothing
            PictureBox2.Width = 0
            PictureBox2.Height = 0
        Else
            '  PictureBox2.BeginInit()
            PictureBox2.Source = loadImage(qu(res.quimage))
            PictureBox2.Stretch = Stretch.Uniform
            ' PictureBox2.EndInit()
            PictureBox2.Width = PictureBox2.Source.Width
            PictureBox2.Height = PictureBox2.Source.Height
            If PictureBox2.Width > 400 Then
                PictureBox2.Height = (400 / PictureBox2.Width) * PictureBox2.Height
                PictureBox2.Width = 400

            End If
        End If
        RichTextBox1.BringIntoView()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles button2.Click

        prevq()
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectionChanged
        showCounter = ComboBox1.SelectedItem - 1
        changeQ(0)
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles timer1.Elapsed
        timeCounter -= 1
        If timeCounter < 10 And timeCounter > 0 Then
            label4.Foreground = Brushes.Red
        ElseIf timeCounter <= 0 Then
            timeCounter = 30
            timer1.Stop()
            changeQ(1)

        Else
            label4.Foreground = Brushes.Black
        End If
        label4.Text = timeCounter
    End Sub

    Private Sub Questions_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing

        e.Cancel = True
        If result IsNot Nothing Then
            saveProgress()
        End If
        result = Nothing
        timer1.Stop()
        GC.Collect()
        Me.Hide()

    End Sub


    Private Sub saveProgress()
        Dim sql As String = ""
        Dim sql2 As String = ""
        Dim x As Integer = 0
        For i As Integer = 0 To answered.Length - 1
            Dim answer As String = answers(i)
            If answers(i) Is Nothing Then
                Continue For

            ElseIf result.Item(i)(res.correct) = answer Then
                sql += Convert.ToString(result.Item(i)(res.id)) + " or ID = "
            ElseIf result.Item(i)(res.correct) <> answer And (IsDBNull(result.Item(i)(res.solved)) OrElse Convert.ToInt32(result.Item(i)(res.solved)) = 0) Then
                sql2 += Convert.ToString(result.Item(i)(res.id)) + " or ID = "
            End If
            If x > 100 Then
                x = 0
                runSaveCMD(sql, sql2)
                sql = ""
                sql2 = ""

            End If
            x += 1
        Next

        runSaveCMD(sql, sql2)

    End Sub

    Private Sub runSaveCMD(ByRef sql As String, ByRef sql2 As String)

        If sql <> "" Then
            sql = "UPDATE `" & tableName & "` SET solved = 2 WHERE ID = " + sql
            sql = sql.TrimEnd(" or ID = ".ToCharArray)

        End If

        If sql2 <> "" Then
            sql2 = "UPDATE `" & tableName & "` SET solved = 1 WHERE ID = " + sql2
            sql2 = sql2.TrimEnd(" or ID = ".ToCharArray)

        End If
        Dim cmd As New SQLite.SQLiteCommand
        Try

            cmd.Connection = con
            cmd.CommandText = sql
            con.Open()
            If sql <> "" Then
                cmd.ExecuteNonQuery()
            End If

            cmd.CommandText = sql2
            If sql2 <> "" Then
                cmd.ExecuteNonQuery()
            End If
            If Not timeTrial And Not testMode Then
                sql = "SELECT TableInfo FROM `" & tableName & "`"
                cmd.CommandText = sql
                Dim info As String = cmd.ExecuteScalar
                Dim processedinfo As String = MainWindow.prossecInfo(info, category, showCounter.ToString)
                If processedinfo = "" Then
                    sql = "UPDATE `" & tableName & "` SET TableInfo = '" & info & category & ":" & showCounter.ToString & "*' WHERE ID = 1"
                Else
                    sql = "UPDATE `" & tableName & "` SET TableInfo = '" & processedinfo & "' WHERE ID = 1"
                End If

                cmd.CommandText = sql
                cmd.ExecuteNonQuery()
            End If


        Catch ex As Exception

            MsgBox(ex.Message + cmd.CommandText)
        Finally
            con.Close()
        End Try
    End Sub

    Private Sub Window_KeyDown(sender As Object, e As KeyEventArgs) Handles dociewer.KeyDown
        If e.Key = Key.Right Then
            System.Threading.Thread.Sleep(100)
            nextq()
        ElseIf e.Key = Key.Left Then
            System.Threading.Thread.Sleep(100)
            prevq()
        End If

    End Sub

    Function captureSelection() As RadioButton
        Dim cradio As RadioButton
        If RadioButton1.IsChecked Then
            cradio = RadioButton1
        ElseIf RadioButton2.IsChecked Then
            cradio = RadioButton2
        ElseIf RadioButton3.IsChecked Then
            cradio = RadioButton3
        ElseIf RadioButton4.IsChecked Then
            cradio = RadioButton4
        ElseIf RadioButton5.IsChecked And RadioButton5.IsVisible Then
            cradio = RadioButton5
        End If
        Return cradio
    End Function
    Public Sub initConnection(ByVal file As String)
        Try
            con = New SQLite.SQLiteConnection("Data Source=" & file)

        Catch ex As Exception
            MsgBox("Data base engine couldn't initiate")
            End
        End Try

    End Sub

    Private Sub slider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles slider.ValueChanged
        Dim x As Integer
        ' Dim y As Double = 1
        x = slider.Value + 11
        RadioButton1.Content.fontsize = x
        RadioButton2.Content.fontsize = x
        RadioButton3.Content.fontsize = x
        RadioButton4.Content.fontsize = x
        RadioButton5.Content.fontsize = x
        RichTextBox1.FontSize = x
        RichTextBox2.FontSize = x


    End Sub

    Sub nextq()
        changeQ(1)
    End Sub

    Private Sub dociewer_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles dociewer.PreviewKeyDown
        If e.Key = Key.Right Then
            System.Threading.Thread.Sleep(100)
            nextq()
        ElseIf e.Key = Key.Left Then
            System.Threading.Thread.Sleep(100)
            prevq()
        End If
    End Sub

    Private Sub button3_Click(sender As Object, e As RoutedEventArgs) Handles button3.Click
        hilightAnswer()
        RichTextBox2.Visibility = Visibility.Visible
        PictureBox1.Visibility = Visibility.Visible
        If answered(showCounter) = 0 Then
            answers(showCounter) = "Wrong!!@#$%^&*()(*&^%$13"
            answeredCounter += 1


            label3.Text = Math.Round(correctCounter / answeredCounter, 2) * 100 & " % Correct"
        End If
        answered(showCounter) = 1
    End Sub

    Sub prevq()
        If showCounter <> 0 Then
            changeQ(-1)
        End If
    End Sub

    Private Sub Questions_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        'If magnification = 0 Then magnification = 0.39
        RichTextBox1.MaxWidth = 0.83 * Me.ActualWidth
        RichTextBox2.MaxWidth = 0.83 * Me.ActualWidth
        Resources("radmax") = 0.47 * Me.ActualWidth

    End Sub


End Class
