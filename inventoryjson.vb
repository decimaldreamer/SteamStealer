Friend Class inventoryjson
    Private _contextID As String
    Private list1 As New List(Of String)()
    Private list2 As New List(Of String)()

    Private Function getItems() As List(Of String())
        Dim list As New List(Of String())()
        For i As Integer = 0 To Me.list1.Count - 1
            Dim input As String = Me.list1(i)
            Dim str2 As String = Me.getValue(input, "id")
            Dim str3 As String = Me.getValue(input, "classid")
            Dim str4 As String = String.Empty
            For j As Integer = 0 To Me.list2.Count - 1
                If Me.getValue(Me.list2(j), "classid") = str3 Then
                    str4 = Me.list2(j)
                    Exit For
                End If
            Next
            If str4 <> String.Empty Then
                Dim item As String() = New String() {Me.getValue(str4, "appid"), Me.getValue(input, "amount"), str2, Me.getValue(str4, "market_name"), Me.getValue(str4, "type").ToLower(), Me._contextID}
                If Not list.Contains(item) AndAlso (Me.getValue(str4, "tradable") = "1") Then
                    list.Add(item)
                End If
            End If
        Next
        Return list
    End Function

    Private Function getValue(input As String, name As String) As String
        Dim strArray As String() = input.Split(New Char() {""""c, ":"c}, StringSplitOptions.RemoveEmptyEntries)
        For i As Integer = 0 To strArray.Length - 1
            If strArray(i) = name Then
                Return strArray(i + 1).Replace(",", "")
            End If
        Next
        Return Nothing
    End Function

    Public Function Parse(input As String, contextid As String) As List(Of String())
        Me._contextID = contextid
        Dim strArray As String() = input.Split(New Char() {"{"c, "}"c})
        If (strArray.Length > 2) AndAlso (strArray(1).IndexOf("true") <> -1) Then
            Me.parserginventory(strArray)
            Me.parsergdescrptions(strArray)
            Return Me.getItems()
        End If
        Return New List(Of String())()
    End Function

    Private Sub parsergdescrptions(input As String())
        Dim num As Integer = -1
        For i As Integer = 0 To input.Length - 1
            If input(i).IndexOf("rgDescriptions") <> -1 Then
                num = i + 1
                Exit For
            End If
        Next
        For j As Integer = num To input.Length - 1
            Dim strArray As String() = input(j).Split(New Char() {""""c})
            If (strArray.Length > 2) AndAlso (strArray(1) = "appid") Then
                Me.list2.Add(input(j))
            End If
        Next
    End Sub

    Private Sub parserginventory(input As String())
        Dim num As Integer = -1
        Dim num2 As Integer = -1
        For i As Integer = 0 To input.Length - 1
            If input(i).IndexOf("rgInventory") <> -1 Then
                num = i + 1
            End If
            If (num <> -1) AndAlso (input(i) = "") Then
                num2 = i
                Exit For
            End If
        Next
        For j As Integer = num To num2 - 1
            Dim strArray As String() = input(j).Split(New Char() {""""c})
            If (strArray.Length > 2) AndAlso (strArray(1) = "id") Then
                Me.list1.Add(input(j))
            End If
        Next
    End Sub
End Class