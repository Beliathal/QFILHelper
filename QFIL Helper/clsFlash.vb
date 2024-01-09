Imports System.IO
Imports Microsoft.VisualBasic
Imports System.Text.RegularExpressions

Public Class clsFlash : Inherits clsLUNs

    Protected Enum WarningType As Byte
        WR_LUN = 0
        WR_GPT = 1
        WR_PRT = 2
        WR_ERS = 3
    End Enum

    Private Enum ListType As Byte
        LS_PRT = 0
        LS_GPT = 1
        LS_LUN = 2
        LS_TMP = 3
    End Enum

    ' 2023-02-01: Bug removed: placed gsFlashDir.Length instead of gsFlashDir.Length+1

    Private getFileName As Func(Of String, String) = _
        Function(sFileName As String) sFileName.Substring(gsFlashDir.Length).ToLower

    Private getLabel As Func(Of String, String) = _
        Function(sFileName As String) sFileName.Substring(5, sFileName.Length - 9).ToLower

    Private getShortLabel As Func(Of String, String) = _
        Function(sFileName As String) sFileName.Substring(0, sFileName.Length - 4).ToLower

    Private getLUN As Func(Of String, String) = _
        Function(sFileName As String) sFileName.Substring(3, 1)

    Private getDummy As Func(Of String, String) = _
        Function(sFileName As String) sFileName

    ' lun - matches the characters lun literally (case sensitive)
    ' \d  - matches a digit (equivalent to [0-9])
    ' \w  - matches any word character (equivalent to [a-zA-Z0-9_])
    ' +   - matches the previous token between one and unlimited times, as many times as possible
    ' \.  - matches the character . with index 4610 (2E16 or 568) literally (case sensitive)
    ' bin - matches the characters bin literally (case sensitive)

    Public Sub FlashFirmware()

        Dim isDebug As Boolean = False
        Dim sCMDLine As String
        Dim isExec As Boolean ' Did we execute even 1 command?

        If Not ValidateCQF(isDebug) Then Exit Sub

        Dim lsaFileList As New List(Of String())
        Dim obPInfo As stPInfo
        Dim isShort As Boolean ' Short file name?

        ResetLookUp()

        If Not LoadFileList(lsaFileList) Then Exit Sub
        If Not FlashLUNs(lsaFileList(ListType.LS_LUN), isExec, isDebug) Then GoTo EXT
        If Not FlashGPTs(lsaFileList(ListType.LS_GPT), isExec, isDebug) Then GoTo EXT
        If Not ReadGPTHeaders(True, True, isDebug) Then GoTo EXT
        If Not DisplayWarning(lsaFileList(ListType.LS_PRT), WarningType.WR_PRT) Then GoTo EXT

        For Each sFileName As String In lsaFileList(ListType.LS_PRT)

            isShort = False
            obPInfo = Nothing
            sFileName = getFileName(sFileName)

            ' Updated on: 09/01/2024

            ' Changed: lun\d\w+\.bin to lun\d\S+\.bin
            ' Issue: was too seelpy, sould've used S+ to begin with :(

            If Regex.IsMatch(sFileName, "lun\d\S+\.bin") Then
                obPInfo.iLUN = getLUN(sFileName)
                obPInfo.sLabel = getLabel(sFileName)
            Else
                obPInfo.sLabel = getShortLabel(sFileName) : isShort = True
            End If

            If Not LookUpNames(obPInfo) Then _
                If Not DisplayNotFound(obPInfo) Then Exit For _
                Else Continue For

            Short2Long(obPInfo, isShort)
            sCMDLine = BuildCommand(obPInfo)

            If isDebug Then Continue For
            If Not ExecuteCommand(sCMDLine) Then Exit For

            isExec = True

        Next

EXT:
        lsaFileList.Clear()
        lsaFileList = Nothing
        obPInfo = Nothing

        ProcessCompletedMsg(isExec Or isDebug)

    End Sub

    Private Function FlashGPTs(ByRef saFileList() As String, _
                               ByRef isExec As Boolean, _
                               Optional ByVal isDebug As Boolean = False) As Boolean

        If saFileList.Count = 0 Then Return True
        If Not DisplayWarning(saFileList, WarningType.WR_GPT) Then Return True

        Dim sCMDLine As String
        Dim obPInfo As stPInfo

        For Each sFileName As String In saFileList

            obPInfo = Nothing
            sFileName = getFileName(sFileName)

            obPInfo.iStart = 0
            obPInfo.iSectors = 6
            obPInfo.iLUN = getLUN(sFileName)
            obPInfo.sLabel = getLabel(sFileName)
            sCMDLine = BuildCommand(obPInfo)

            If isDebug Then Continue For
            If Not ExecuteCommand(sCMDLine) Then Return False

            isExec = True

        Next

        'Console.Clear()
        Return True

    End Function

    Private Function FlashLUNs(ByRef saFileList() As String, _
                               ByRef isExec As Boolean, _
                               Optional ByVal isDebug As Boolean = False) As Boolean

        If saFileList.Count = 0 Then Return True
        If Not DisplayWarning(saFileList, WarningType.WR_LUN) Then Return True

        ' Return true, even if the user choses to Abort, because it could be a mixed backup, 
        ' where user has placed LUN, GPT and partition backups in the folder and wants to procceed 
        ' with them all. The only case when this function returns false is when there's an error 
        ' with FHLoader

        Dim sCMDLine As String
        Dim obPInfo As stPInfo

        For Each sFileName As String In saFileList

            obPInfo = Nothing
            sFileName = getFileName(sFileName)

            obPInfo.iLUN = getLUN(sFileName)
            obPInfo.sLabel = getLabel(sFileName)
            sCMDLine = BuildCommand(obPInfo)

            If isDebug Then Continue For
            If Not ExecuteCommand(sCMDLine) Then Return False

            isExec = True

        Next

        'Console.Clear()
        Return True

    End Function

    Protected Function DisplayWarning( _
                                   ByRef saFileList() As String, _
                                   ByVal eWarningType As WarningType) As Boolean

        If saFileList.Length = 0 Then Return False

        Dim sOutput As String
        Dim ptrFileName As System.Func(Of String, String)

        Select Case eWarningType
            Case WarningType.WR_LUN : sOutput = goSpeaker.ID2Msg(28) ' LUN
            Case WarningType.WR_GPT : sOutput = goSpeaker.ID2Msg(26) ' GPT
            Case WarningType.WR_PRT : sOutput = goSpeaker.ID2Msg(39) ' Partitions
            Case WarningType.WR_ERS : sOutput = goSpeaker.ID2Msg(36) ' Erase
        End Select

        sOutput = sOutput & vbCrLf & vbCrLf

        ' One of those moments when I wish that VB had actual C/C++ pointers :(

        If eWarningType = WarningType.WR_ERS Then _
            ptrFileName = getDummy Else _
            ptrFileName = getFileName

        For Each sFileName As String In saFileList
            sOutput += ptrFileName(sFileName) & ", "
        Next

        sOutput = sOutput.Substring(0, sOutput.Length - 2)

        Console.WriteLine(sOutput)
        Console.WriteLine(goSpeaker.ID2Msg(40))

        If Console.ReadKey(True).Key = ConsoleKey.A Then

            ' ByPass ProcessCompletedMsg
            geFailed = ErrorType.ER_ABORT
            Return False

        End If

        'Console.Clear()
        Return True

    End Function

    ' Naming Variations Reference

    '   1. lun3_hidden_s-2048.bin;
    '   2. lun4_abl_a_b-417181_s-1.bin; lun0_gpt_header_b-0_s-6.bin; lun4_hidden_partition_b-417181_s-1
    '   3. lun5_ALIGN_TO_128K_2; lun4_raw_resources_a; lun4_abl_a.bin; lun0_ftm.bin
    '   4. lun4.bin
    '   5. lun4_complete.bin
    '   6. abl_a.bin; ftm.bin

    Private Function LoadFileList(ByRef lsaFileList As List(Of String())) As Boolean

        If Not Directory.Exists(gsFlashDir) Then
            Console.WriteLine(goSpeaker.ID2Msg(37))
            Console.WriteLine(goSpeaker.ID2Msg(17))
            Console.ReadKey(True)
            Return False
        End If

        lsaFileList.Add(Directory.GetFiles(gsFlashDir, "*.bin")) ' 0

        If lsaFileList(0).Count = 0 Then
            Console.WriteLine(goSpeaker.ID2Msg(38))
            Console.WriteLine(goSpeaker.ID2Msg(17))
            Console.ReadKey(True)
            Return False
        End If

        lsaFileList.Add(Directory.GetFiles(gsFlashDir, "lun?_gpt_header.bin"))      '1
        lsaFileList.Add(Directory.GetFiles(gsFlashDir, "lun?_complete.bin"))        '2
        lsaFileList.Add(Directory.GetFiles(gsFlashDir, "lun?.bin"))                 '3

        ' If present, exclude GPT headers (lun?_gpt_header.bin) list from main list [0]
        ' If present, exclude LUN files (lun?_complete.bin) from from main list [0]
        ' If present, exclude LUN files (lun?.bin) from from main list [0]
        ' Whatever has benn left, if anything at all, will be partition list

        If lsaFileList(ListType.LS_GPT).Count > 0 Then _
            lsaFileList(ListType.LS_PRT) = _
            lsaFileList(ListType.LS_PRT).Except(lsaFileList(ListType.LS_GPT)).ToArray()

        If lsaFileList(ListType.LS_LUN).Count > 0 Then _
            lsaFileList(ListType.LS_PRT) = _
            lsaFileList(ListType.LS_PRT).Except(lsaFileList(ListType.LS_LUN)).ToArray()

        If lsaFileList(ListType.LS_TMP).Count > 0 Then _
            lsaFileList(ListType.LS_PRT) = _
            lsaFileList(ListType.LS_PRT).Except(lsaFileList(ListType.LS_TMP)).ToArray()

        ' Merge lun?_complete.bin and lun?.bin into one array

        lsaFileList(ListType.LS_LUN) = _
            lsaFileList(ListType.LS_LUN).Concat(lsaFileList(ListType.LS_TMP)).ToArray()

        lsaFileList.RemoveAt(ListType.LS_TMP)

        ' If user did something stupid like this: LUN1.bin, LUN1_complete.bin, it's their problem,
        ' not mine. I can't handle every possible scenario or install my brain onto someone else...

        Return True

    End Function

    'fh_loader.exe --port=\\.\COM? --sendimage=ftm.bin --start_sector=23816 --lun=0 --noprompt 
    '--showpercentagecomplete --zlpawarehost=1 --memoryname=ufs

    'fh_loader.exe --port=\\.\COM? --sendimage=LUN4_complete.bin --start_sector=0 --lun=4 --noprompt 
    ' --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs

    Private Overloads Function BuildCommand(ByRef obPInfo As stPInfo) As String

        Dim sFileName As String = BuildFileName(obPInfo, False, True)
        Dim saMerge() As String = { _
                    " --port=\\.\$", _
                    " --sendimage=$", _
                    " --start_sector=#", _
                    " --lun=#", _
                    " --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs"}

        saMerge(0) = saMerge(0).Replace("$", gsCOMPort)
        saMerge(1) = saMerge(1).Replace("$", sFileName)
        saMerge(1) = saMerge(1).Replace("$", obPInfo.sLabel)
        saMerge(2) = saMerge(2).Replace("#", obPInfo.iStart)
        saMerge(3) = saMerge(3).Replace("#", obPInfo.iLUN)
        sFileName = ""

        For iCnt As Byte = 0 To saMerge.Count - 1
            sFileName &= saMerge(iCnt)
        Next

        ShowProgress(obPInfo, ProgressType.PR_FLH)

        Erase saMerge
        Return sFileName

    End Function

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

    ' If user placed short labeled partition images, then rename them according to format that
    ' BuildFileName function expects to find.... It's dirty solution, but I just don't have time to
    ' implement a proper one

    Private Sub Short2Long(ByRef obPInfo As stPInfo, ByVal isShort As Boolean)

        If Not isShort Then Exit Sub

        Dim sOldName As String = gsFlashDir & obPInfo.sLabel & ".bin"
        Dim sNewName As String = gsFlashDir & "lun" & obPInfo.iLUN & "_" & obPInfo.sLabel & ".bin"

        File.Move(sOldName, sNewName)

    End Sub

End Class