Imports System.IO
Imports Microsoft.VisualBasic

Public Class clsVital : Inherits clsHLUNs

    Private Enum OPCode As Byte
        BOOT = 0
        VITAL = 1
    End Enum

    Public BackupBootParts As Action = _
        Sub() BackupVitalParts(OPCode.BOOT)

    Public BackupIMEIParts As Action = _
        Sub() BackupVitalParts(OPCode.VITAL)

    ' Backs-up ABL, Boot and LAF, XBL before unlocking the bootloader / nuking LAF
    ' Backs-up FTM, Modemst, FSG, FSC

    Private Sub BackupVitalParts(ByVal eOP As OPCode)

        If Not ValidateFiles() Then Exit Sub

        Dim saBuffer() As String
        Dim sCMDLine As String
        Dim iCurIndex As UInt16
        Dim saPLookUp() As String

        ResetInfo()
        CreateBackupFolder()
        LoadPartList(saPLookUp, eOP)

        saBuffer = File.ReadAllLines(gsFileName)

        'If saBuffer.Length = 0 Then Exit Sub - check later?

        For Each sCurPart As String In saPLookUp

            iCurIndex = LocatePartition(saBuffer, sCurPart)

            If iCurIndex > -1 AndAlso _
                ParseXML(saBuffer(iCurIndex)) Then

                sCMDLine = BuildCommand()

                If Not ExecuteCommand(sCMDLine) Then Exit For

            End If

        Next

        saBuffer = Nothing
        saPLookUp = Nothing

        CleanUpBackupFolder()
        ProcessCompletedMsg()

    End Sub

    Protected Function LocatePartition( _
                              ByRef sBuffer() As String, _
                              ByVal sPName As String) As Integer

        LocatePartition = Array.FindIndex(sBuffer, _
        Function(x As String) (x.Contains(sPName)))

        If LocatePartition = -1 Then
            Console.WriteLine(goUILang.ID2Msg(28) & sPName)
            Console.WriteLine(goUILang.ID2Msg(21))
            Console.ReadKey(True)
        End If

    End Function

    Private Sub LoadPartList( _
                             ByRef saPLookUp() As String, _
                             ByVal eOP As OPCode)

        If eOP = OPCode.BOOT Then

            ' vbCrLf.ToArray: Would return an array of 2 elements Cr, Lf

            saPLookUp = My.Resources.boot_partitons.Split( _
                vbCrLf.ToArray, StringSplitOptions.RemoveEmptyEntries)

        Else

            saPLookUp = My.Resources.vital_partitions.Split( _
                vbCrLf.ToArray, StringSplitOptions.RemoveEmptyEntries)

        End If

    End Sub

End Class
