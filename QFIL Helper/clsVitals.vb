Imports System.IO
Imports Microsoft.VisualBasic

Public Class clsVitals : Inherits clsParts

    ' Backs-up ABL, Boot and LAF, XBL before unlocking the bootloader / nuking LAF
    ' Backs-up FTM, Modemst, FSG, FSC

    Protected Enum OPCode As Byte
        BOOT = 0
        VITAL = 1
        NUKE = 2
    End Enum

    Public BackupBootParts As Action = _
        Sub() BackupVitalParts(OPCode.BOOT)

    Public BackupIMEIParts As Action = _
        Sub() BackupVitalParts(OPCode.VITAL)

    Private Sub BackupVitalParts(ByVal eOP As OPCode)

        If Not ValidateCQF() Then Exit Sub

        Dim saPLookUp() As String
        Dim obPInfo As stPInfo

        ResetLookUp()
        CreateBackupFolder()
        LoadPartList(saPLookUp, eOP)

        If Not ReadGPTHeaders(False, True) Then GoTo EXT

        For Each sLabel As String In saPLookUp

            obPInfo = Nothing
            obPInfo.sLabel = sLabel

            ' If partition info not found: 
            ' Continue proccessing other files or exit?

            If Not LookUpNames(obPInfo) Then _
                If Not DisplayNotFound(obPInfo) Then Exit For _
                Else Continue For

            If Not ExecuteCommand(BuildCommand(obPInfo, False)) Then Exit For

        Next

EXT:
        CleanUpBackupFolder()
        ProcessCompletedMsg()

        obPInfo = Nothing
        Erase saPLookUp

    End Sub

    Protected Sub LoadPartListOld( _
                             ByRef saPLookUp() As String, _
                             ByVal eOP As OPCode)

        ' vbCrLf.ToArray: Would return an array of 2 elements Cr, Lf

        Select Case eOP

            Case OPCode.BOOT
                ' saPLookUp = My.Resources.list_boot.Split( _
                ' vbCrLf.ToArray, StringSplitOptions.RemoveEmptyEntries)

            Case OPCode.VITAL
                saPLookUp = My.Resources.list_vital.Split( _
                    vbCrLf.ToArray, StringSplitOptions.RemoveEmptyEntries)

            Case OPCode.NUKE
                saPLookUp = My.Resources.lst_erase.Split( _
                    vbCrLf.ToArray, StringSplitOptions.RemoveEmptyEntries)

        End Select

    End Sub

    Protected Function DisplayNotFound(ByRef obPInfo As stPInfo) As Boolean

        Console.WriteLine(goSpeaker.ID2Msg(29) & obPInfo.sLabel)
        Console.WriteLine(goSpeaker.ID2Msg(40))

        If Console.ReadKey(True).Key = ConsoleKey.A Then

            ' ByPass ProcessCompletedMsg
            geFailed = ErrorType.ER_ABORT

            'Console.Clear()
            Return False

        End If

        Return True

    End Function

End Class
