Public Class clsInfo : Inherits clsMsg

    Protected sLabel As String
    Protected sLUN As String
    Protected sStart As String
    Protected sSectors As String

    Protected sCOMPort As String
    Protected sFileName As String
    Protected sDirName As String

    Protected iLUN As Byte
    Protected iStart As UInt32
    Protected iSectors As UInt32

    Private Const scHiddenLUN3 As String = "2048"
    Private Const scHiddenLUN6 As String = "1024"

    Protected ReadOnly Property DirName() As String

        Get
            Return sDirName & "\"
        End Get

    End Property

    ' Used for labeling Hidden Partitions: Hiddden_#

    Protected WriteOnly Property Label(ByVal iHidCnt As UInt32) As String

        Set(value As String)
            sLabel = value & iHidCnt.ToString
        End Set

    End Property

    ' Used for converting Hidden Partition Sectors to String

    Protected WriteOnly Property Sectors() As UInt32

        Set(value As UInt32)
            sSectors = value.ToString
        End Set

    End Property

    Protected WriteOnly Property Start() As UInt32

        Set(value As UInt32)
            sStart = value.ToString
        End Set

    End Property

    Protected WriteOnly Property LUN() As Byte

        Set(value As Byte)
            sLUN = value.ToString
        End Set

    End Property

    Protected ReadOnly Property iSize() As UInt32

        Get
            Return iStart + iSectors
        End Get

    End Property

    Protected Sub ResetInfo()

        sLabel = "" : sLUN = "" : sStart = "" : sSectors = ""
        iLUN = 0 : iStart = 0 : iSectors = 0

    End Sub

    Protected Sub SetHiddenLUN(ByVal iCurLUN As Byte)

        sLUN = iCurLUN.ToString

        Select Case iCurLUN
            Case 3 : sSectors = scHiddenLUN3
            Case 6 : sSectors = scHiddenLUN6
        End Select

    End Sub

    ' There's no way to know the exact number of sectors contained within the hidden LUNs, 
    ' as their sizes can change depending on the phone model, OS version & build number.

    ' This is why I've decided to let the user enter this info manualy, but I still can
    ' help by suggesting a possible number based on this guide:
    ' https://forum.xda-developers.com/t/tutorial-full-flash-backup-and-restore.4362809/

    Protected Function SuggestSectors(ByVal iCurLUN As Byte) As String

        Select Case iCurLUN
            Case 3 : Return scHiddenLUN3
            Case 6 : Return scHiddenLUN6
        End Select

        Return "4096"

    End Function

    Protected Sub ResetBackupDate()
        sDirName = "Backup-" & Format(System.DateTime.Now, "yyyy-MM-dd-hhmmss")
    End Sub

End Class
