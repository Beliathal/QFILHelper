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

    Protected Overloads Function LocatePartition( _
                                            ByRef sBuffer() As String, _
                                            ByRef sPName As String) As Integer

        ' Caused an awefull bug where AOP_a would get overwritten by OP_a. 
        ' This is what happens when you try to code while sleepy and tired :((

        'LocatePartition = Array.FindIndex(sBuffer, _
        'Function(x As String) (x.Contains(sPName)))

        LocatePartition = -1

        For iCnt As UInt16 = 0 To sBuffer.Count - 1

            If sBuffer(iCnt).ToLower.IndexOf("partition label=""" & sPName.ToLower & """") > -1 Then
                LocatePartition = iCnt
                Exit For
            End If

        Next

        If LocatePartition = -1 Then
            Console.WriteLine(goSpeaker.ID2Msg(28) & sPName)
            Console.WriteLine(goSpeaker.ID2Msg(21))
            Console.ReadKey(True)
        End If

    End Function

    Protected Overloads Function LocatePartition( _
                                             ByRef sBuffer() As String, _
                                             ByRef sPName As String, _
                                             ByRef sLNum As String) As Integer

        'LocatePartition = Array.FindIndex(sBuffer, _
        'Function(x As String) (x.Contains(sPName)))

        For iCnt As UInt16 = 0 To sBuffer.Count

            LocatePartition = sBuffer(iCnt).IndexOf(sPName)

            If LocatePartition > -1 AndAlso _
                sBuffer(iCnt).IndexOf(sLNum) > -1 Then Exit Function

        Next

        Console.WriteLine(goSpeaker.ID2Msg(28) & sPName)
        Console.WriteLine(goSpeaker.ID2Msg(21))
        Console.ReadKey(True)

    End Function

    Private Sub LoadPartList( _
                             ByRef saPLookUp() As String, _
                             ByVal eOP As OPCode)

        If eOP = OPCode.BOOT Then

            ' vbCrLf.ToArray: Would return an array of 2 elements Cr, Lf

            saPLookUp = My.Resources.list_boot.Split( _
                vbCrLf.ToArray, StringSplitOptions.RemoveEmptyEntries)

        Else

            saPLookUp = My.Resources.list_vital.Split( _
                vbCrLf.ToArray, StringSplitOptions.RemoveEmptyEntries)

        End If

    End Sub

End Class
