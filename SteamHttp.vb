Imports System.IO
Imports System.Net

Public Class SteamHttp
    Public Shared Function ObtainsessionID(cookie As CookieContainer) As Boolean
        Dim response As HttpWebResponse = Nothing
        Dim reader As StreamReader = Nothing
        Dim request As HttpWebRequest = Nothing
        Try
            request = DirectCast(WebRequest.Create("http://steamcommunity.com"), HttpWebRequest)
            request.Method = "GET"
            request.Headers("Origin") = "http://steamcommunity.com"
            request.Referer = "http://steamcommunity.com"
            request.Headers("Accept-Encoding") = "gzip,deflate"
            request.Headers("Accept-Language") = "en-us,en"
            request.Headers("Accept-Charset") = "iso-8859-1,*,utf-8"
            request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.3; en-US; Valve Steam Client/1393366296; ) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.166 Safari/535.19"
            request.CookieContainer = cookie
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
            request.AutomaticDecompression = DecompressionMethods.Deflate Or DecompressionMethods.GZip
            response = DirectCast(request.GetResponse(), HttpWebResponse)
            If response.StatusCode = HttpStatusCode.OK Then
                reader = New StreamReader(response.GetResponseStream())
                Dim str As String = reader.ReadToEnd()
                Return Not (reader.ReadToEnd().Contains("g_steamID = false;") OrElse (response.Cookies("sessionid") Is Nothing))
            End If
        Catch generatedExceptionName As Exception
        Finally
            If reader IsNot Nothing Then
                reader.Close()
            End If
            If response IsNot Nothing Then
                response.Close()
            End If
        End Try
        Return False
    End Function

    Public Shared Function SteamWebRequest(cookie As CookieContainer, url As String, Optional data As String = Nothing, Optional lasturl As String = "") As String
        Dim response As HttpWebResponse = Nothing
        Dim reader As StreamReader = Nothing
        Dim request As HttpWebRequest = Nothing
        Try
            request = Nothing
            If url.StartsWith("http") Then
                request = DirectCast(WebRequest.Create(url), HttpWebRequest)
            Else
                request = DirectCast(WebRequest.Create(Convert.ToString("http" + (If((data IsNot Nothing), "s", "")) + "://steamcommunity.com/") & url), HttpWebRequest)
            End If
            request.Referer = Convert.ToString("http://steamcommunity.com/") & lasturl
            request.Accept = If((data IsNot Nothing), "*/*", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8")
            request.Method = If((data IsNot Nothing), "POST", "GET")
            If data IsNot Nothing Then
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8"
            End If
            request.Headers("Accept-Charset") = "iso-8859-1,*,utf-8"
            request.Headers("Origin") = "https://steamcommunity.com"
            request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.3; en-US; Valve Steam Client/1393366296; ) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.166 Safari/535.19"
            request.Headers("Accept-Language") = "en-us,en"
            request.Headers("Accept-Encoding") = "gzip,deflate"
            request.CookieContainer = cookie
            request.AutomaticDecompression = DecompressionMethods.Deflate Or DecompressionMethods.GZip
            If data IsNot Nothing Then
                Using writer As New StreamWriter(request.GetRequestStream())
                    writer.Write(data)
                End Using
            End If
            response = DirectCast(request.GetResponse(), HttpWebResponse)
            If response.StatusCode = HttpStatusCode.OK Then
                reader = New StreamReader(response.GetResponseStream())
                Return reader.ReadToEnd()
            End If
        Catch exception As WebException
            If exception.Response IsNot Nothing Then
                Dim response2 As HttpWebResponse = DirectCast(exception.Response, HttpWebResponse)
                Return New StreamReader(response2.GetResponseStream()).ReadToEnd()
            End If
        Finally
            If reader IsNot Nothing Then
                reader.Close()
            End If
            If response IsNot Nothing Then
                response.Close()
            End If
        End Try
        Return Nothing
    End Function
End Class