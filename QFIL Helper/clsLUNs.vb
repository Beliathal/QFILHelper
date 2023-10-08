Imports System.IO
Imports Microsoft.VisualBasic
Imports System.Text.RegularExpressions

' Purpouse: Backup LUNs

Public Class clsLUNs : Inherits clsParts

    Public Enum OPCode As Byte
        FTM = 0
        ABL = 1
        SYSA = 2
        SYSB = 3
        SID = 4
        LUN16 = 5
        LUN0 = 6
        LUN4 = 7
    End Enum

    Public Sub BackupLUNs()

        Dim isDebug As Boolean = False
        Dim sCMDLine As String

        If Not ValidateCQF(isDebug) Then Exit Sub

        Dim obPInfo As stPInfo
        Dim isExec As Boolean ' Did we execute even 1 command?

        ResetLookUp()
        CreateBackupFolder(isDebug)

        If Not NTFSCompression(goSpeaker.gbDoCompress) Then GoTo EXT
        If Not ReadGPTHeaders(True, False, isDebug) Then GoTo EXT

        For iCnt As Byte = 0 To galoLookUp.Count - 1

            obPInfo = Nothing
            obPInfo.iStart = 0
            obPInfo.sLabel = "complete"
            obPInfo.iLUN = iCnt

            If Not CalcBounds(obPInfo) Then Exit For

            sCMDLine = BuildCommand(obPInfo, False)

            If isDebug Then Continue For
            If Not ExecuteCommand(sCMDLine) Then Exit For

            isExec = True

        Next

EXT:
        CleanUpBackupFolder()
        ProcessCompletedMsg(isExec Or isDebug)

        obPInfo = Nothing

    End Sub

    ' lun - matches the characters lun literally (case sensitive)
    ' \d  - matches a digit (equivalent to [0-9])
    ' {1} - matches the previous token exactly one time (limits to 1 digit)
    '$    - specifies position at the end of a line (limits sting length to 4 in below case)

    Public Function BackupSelLUNs(ByRef saLookUp() As String) As Boolean

        Dim isDebug As Boolean = False
        Dim sCMDLine As String

        If saLookUp.Count = 0 Then Return False
        If Not ValidateCQF(isDebug) Then Return False

        Dim obPInfo As stPInfo
        Dim isExec As Boolean ' Did we execute even 1 command?

        ResetLookUp()
        CreateBackupFolder(isDebug)

        If Not NTFSCompression(goSpeaker.gbDoCompress) Then GoTo EXT
        If Not ReadGPTHeaders(True, False, isDebug) Then GoTo EXT

        For iCnt As Byte = 0 To saLookUp.Count - 1

            If Not Regex.IsMatch(saLookUp(iCnt).ToLower, "lun\d{1}$") Then Continue For

            obPInfo = Nothing
            obPInfo.iLUN = 0
            obPInfo.iStart = 0
            obPInfo.sLabel = "complete"

            sCMDLine = saLookUp(iCnt).Substring(3, 1)

            If Not ValidateLUNNumber(sCMDLine, obPInfo.iLUN) Then _
                If geFailed = ErrorType.ER_ABORT Then _
                GoTo EXT _
                Else Continue For

            If Not CalcBounds(obPInfo) Then Exit For

            sCMDLine = BuildCommand(obPInfo, False)

            If isDebug Then Continue For
            If Not ExecuteCommand(sCMDLine) Then Exit For

            ' Remove the item fromLookUp  array in case partition backup is run later on the same array...
            ' Could happen in a mixed selective backup request

            saLookUp(iCnt) = ""
            isExec = True

        Next

        BackupSelLUNs = True

EXT:
        CleanUpBackupFolder()
        ProcessCompletedMsg(isExec Or isDebug)

        obPInfo = Nothing

    End Function

    Private Function CalcBounds(ByRef obPInfo As stPInfo) As Boolean

        Try

            If obPInfo.iLUN = 0 Then

                ' galoLookUp[0][UpperBound-0].sLabel = "grow"
                ' galoLookUp[0][UpperBound-1].sLabel = "userdata"
                ' galoLookUp[0][UpperBound-2].sLabel = "OP_b"
                ' galoLookUp[0][UpperBound-2] = galoLookUp(0).Count - 3
                ' iB4User: Before Userdata

                ' grow partition in LUN0, comes after userdata. 
                ' It's the last partition in LUN0.
                ' Its size is only 1 sector.

                Dim iB4User As UInt16 = galoLookUp(0).Count - 3

                obPInfo.iSectors = galoLookUp(0)(iB4User).iSize

                Return True

            Else

                Dim iCnt As Byte = obPInfo.iLUN
                Dim iLast As UInt16 = galoLookUp(iCnt).Count - 1

                obPInfo.iSectors = galoLookUp(iCnt)(iLast).iSize

                Return True

            End If

        Catch

            Console.WriteLine(goSpeaker.ID2Msg(35) & obPInfo.iLUN)
            'Console.ReadKey(True)

            geFailed = ErrorType.ER_FAILD

        End Try

    End Function

    Private Function ValidateLUNNumber( _
                                      ByRef sLUNNum As String, _
                                      ByRef iLUNNum As Byte) As Boolean

        If Not Byte.TryParse(sLUNNum, iLUNNum) Then

            Console.WriteLine(goSpeaker.ID2Msg(22) & sLUNNum)
            Console.WriteLine(goSpeaker.ID2Msg(10))

            If Console.ReadKey(True).Key = ConsoleKey.A Then _
                geFailed = ErrorType.ER_ABORT ' ByPass ProcessCompletedMsg

            'Console.Clear()
            Return False

        ElseIf iLUNNum > galoLookUp.Count - 1 Then
            Console.WriteLine(goSpeaker.ID2Msg(23).Replace("$", iLUNNum) & galoLookUp.Count - 1)
            Console.WriteLine(goSpeaker.ID2Msg(10))

            If Console.ReadKey(True).Key = ConsoleKey.A Then _
                geFailed = ErrorType.ER_ABORT ' ByPass ProcessCompletedMsg

            'Console.Clear()
            Return False

        End If

        Return True

    End Function

    Public Function LoadQuickList(ByVal iID As Byte) As String()

        Dim saTemp() As String

        ' Split entires by "#", each split would respresnt a set of partion labels
        ' Split by carriage return and remove empty lines (aka double carriage issue)
        ' vbCrLf.ToArray: Would return an array of 2 elements Cr, Lf

        saTemp = My.Resources.list_vital.Split("#")
        Return saTemp(iID).Split(vbCrLf.ToCharArray, StringSplitOptions.RemoveEmptyEntries)

    End Function

    Public Sub SLBackup(ByVal eCode As OPCode)

        Dim saLookUp() As String = LoadQuickList(eCode)

        BackupSelLUNs(saLookUp)
        Erase saLookUp

    End Sub

    Public Sub SPBackup(ByVal eCode As OPCode)

        Dim saLookUp() As String = LoadQuickList(eCode)

        BackupSelPartitions(saLookUp)
        Erase saLookUp

    End Sub

    Public Sub ManualBackup()

        Console.Clear()
        Console.WriteLine(goSpeaker.ID2Msg(49))
        Console.WriteLine(goSpeaker.ID2Msg(50))
        Console.WriteLine(goSpeaker.ID2Msg(51) & vbCr)
        Console.WriteLine(goSpeaker.ID2Msg(52) & vbCrLf)

        Dim sInput As String
        Dim saParams() As String

        Console.CursorVisible = True
        sInput = Console.ReadLine()
        Console.CursorVisible = False

        If sInput = Nothing Then Exit Sub

        saParams = sInput.ToLower.Split(";")

        If BackupSelLUNs(saParams) Then _
           BackupSelPartitions(saParams)

        Erase saParams

    End Sub

End Class
