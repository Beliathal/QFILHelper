Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic
Imports System.Text.RegularExpressions

Public Class clsParts : Inherits clsBase

    Protected Enum ProgressType As Byte
        PR_BKP = 0
        PR_FLH = 1
        PR_ERS = 2
    End Enum

    ' To run the sub in debug, place 7 GPT Headers in then temp folder: %TEMP%:\QFILHelper
    ' Run ReadGPTHeaders with: isTemp = true, isDebug = True, it will load the headers from 
    ' the TMP folder and bypass executing FHLoader

    Public Sub BackupPartitions()

        Dim isDebug As Boolean = False
        Dim sCMDLine As String
        Dim isExec As Boolean ' Did we execute even 1 command?

        If Not ValidateCQF(isDebug) Then Exit Sub

        ResetLookUp()
        CreateBackupFolder(isDebug)

        If Not NTFSCompression(goSpeaker.gbDoCompress) Then GoTo EXT
        If Not ReadGPTHeaders(isDebug, False, isDebug) Then GoTo EXT

        For iCnt As Byte = 0 To galoLookUp.Count - 1

            For Each obPInfo As stPInfo In galoLookUp(iCnt)

                ' Skip Userdata partition
                If obPInfo.sLabel = "userdata" Then Continue For

                sCMDLine = BuildCommand(obPInfo, False)

                If isDebug Then Continue For
                If Not ExecuteCommand(sCMDLine) Then GoTo EXT

                isExec = True

            Next

        Next

EXT:
        CleanUpBackupFolder()
        ProcessCompletedMsg(isExec Or isDebug)

    End Sub

    Public Function BackupSelPartitions(ByRef saLookUp() As String) As Boolean

        Dim isDebug As Boolean = False
        Dim sCMDLine As String

        If saLookUp.Count = 0 Then Return False
        If Not ValidateCQF(isDebug) Then Return False

        Dim obPInfo As stPInfo
        Dim isExec As Boolean ' Did we execute even 1 command?

        ResetLookUp()
        CreateBackupFolder(isDebug)

        If Not NTFSCompression(goSpeaker.gbDoCompress) Then GoTo EXT
        If Not ReadGPTHeaders(isDebug, True, isDebug) Then GoTo EXT

        For Each sLabel As String In saLookUp

            If Not Regex.IsMatch(sLabel.ToLower, "\w+") Then Continue For
            If sLabel.ToLower.Contains("userdata") Then Continue For

            obPInfo = Nothing
            obPInfo.sLabel = sLabel.ToLower

            If Not ValidatePartLabel(obPInfo) Then _
                If geFailed = ErrorType.ER_ABORT Then _
                GoTo EXT _
                Else Continue For

            sCMDLine = BuildCommand(obPInfo, False)

            If isDebug Then Continue For
            If Not ExecuteCommand(sCMDLine) Then GoTo EXT

            isExec = True

        Next

        BackupSelPartitions = True

EXT:
        CleanUpBackupFolder()
        ProcessCompletedMsg(isExec Or isDebug)

        obPInfo = Nothing

    End Function

    Public Sub BackupUserData()

        Dim isDebug As Boolean = False
        Dim sCMDLine As String

        If Not ValidateCQF(isDebug) Then Exit Sub

        Dim obPInfo As stPInfo
        Dim isExec As Boolean ' Did we execute even 1 command?

        ResetLookUp()
        CreateBackupFolder(isDebug)

        ' NTFS compressing stuff is ain't no good for them cheap bufferless QLC SSDs
        ' It's an ugly solution, but some users requested this feature,
        ' so here we go

        ' just in case: If Not NTFSCompression(goSpeaker.gbDoCompress) Then GoTo EXT

        If Not NTFSCompression() Then GoTo EXT
        If Not ReadGPTHeaders(isDebug, True, isDebug) Then GoTo EXT

        obPInfo = Nothing
        obPInfo.sLabel = "userdata"

        If Not UserDataWarn(obPInfo) Then GoTo EXT

        sCMDLine = BuildCommand(obPInfo, False)

        If isDebug Then GoTo EXT
        If Not ExecuteCommand(sCMDLine) Then GoTo EXT

        isExec = True

EXT:
        CleanUpBackupFolder()
        ProcessCompletedMsg(isExec Or isDebug)

        obPInfo = Nothing

    End Sub

    Public Sub DumpGTP()

        Dim isDebug As Boolean = False

        If Not ValidateCQF() Then Exit Sub

        ResetLookUp()
        CreateBackupFolder(isDebug)

        If Not ReadGPTHeaders(isDebug:=isDebug) Then GoTo EXT

        Dim ioTXTWriter As New StreamWriter( _
             File.Open(gsBackupDir & "\GPTData.txt", FileMode.Create))

        Dim sOutBuf As String

        For iCnt = 0 To galoLookUp.Count - 1

            For Each obPInfo As stPInfo In galoLookUp(iCnt)

                sOutBuf = _
                    "LUN: " & iCnt & " | " & _
                    "Label: " & obPInfo.sLabel & " | " & _
                    "Start: " & obPInfo.iStart & " | " & _
                    "Size: " & obPInfo.iSectors

                ioTXTWriter.WriteLine(sOutBuf)

            Next

        Next

        ioTXTWriter.Flush()
        ioTXTWriter.Close()
        ioTXTWriter.Dispose()
        ioTXTWriter = Nothing

EXT:
        CleanUpBackupFolder()
        ProcessCompletedMsg()

    End Sub

    Protected Function ReadGPTHeaders( _
                                   Optional ByVal isTemp As Boolean = False, _
                                   Optional ByVal isSort As Boolean = False, _
                                   Optional ByVal isDebug As Boolean = False) As Boolean

        Dim sCMDLine As String
        Dim obPInfo As stPInfo

        For iCnt As Byte = 0 To galoLookUp.Count - 1

            obPInfo.iLUN = iCnt
            obPInfo.sLabel = "gpt_header"
            obPInfo.iSectors = 6

            If isDebug Then GoTo SKIP ' Put GPT headers in \TMP folder to simulate reading

            sCMDLine = BuildCommand(obPInfo, isTemp)

            If isDebug Then GoTo SKIP
            If Not ExecuteCommand(sCMDLine) Then Return False

SKIP:
            LoadGPTData(obPInfo, isTemp, isSort)

        Next

        'Console.Clear()
        Return True

    End Function

    ' This function will read the GPT Headers from the current backup folder, parse partition info 
    ' and load the data to the RAM for fasert Look-ups, to allow locating hidden partitions and LUNs.

    ' GPT Partition entries structure (LBA 2–33) starts at offset 2000h (LG specific imho)

    ' Offset 	    Length 	    Contents
    ' 0 (0x00) 	    16 bytes 	Partition type GUID (mixed endian[7])
    ' 16 (0x10) 	16 bytes 	Unique partition GUID (mixed endian)
    ' 32 (0x20) 	8 bytes 	First LBA (little endian)
    ' 40 (0x28) 	8 bytes 	Last LBA (little endian)
    ' 48 (0x30) 	8 bytes 	Attribute flags (e.g. bit 60 denotes read-only)
    ' 56 (0x38) 	72 bytes 	Partition name (36 UTF-16LE code units) 

    ' We are after: 
    ' 1st LBA > Partition start sector
    ' Partition name

    ' Note: LG uses 4k sector sizes, hence, number of sectors = partition size in bytes / 4096
    ' https://en.wikipedia.org/wiki/GUID_Partition_Table#Partition_entries_(LBA_2%E2%80%9333)

    Protected Sub LoadGPTData( _
                             ByRef obPInfo As stPInfo, _
                             ByVal isTemp As Boolean, _
                             Optional ByVal isSort As Boolean = False)

        Dim iFileLength As UInt32
        Dim iaBuffer() As Byte

        Dim sFileName As String = BuildFileName(obPInfo, isTemp)
        Dim ioBINReader As New BinaryReader(File.OpenRead(sFileName))

        ' GPT partition data offset + 32 byte 1st LBA offset
        ioBINReader.BaseStream.Position = &H2020UI
        iFileLength = ioBINReader.BaseStream.Length

        While ioBINReader.BaseStream.Position <= iFileLength

            ' Read first LBA sector number (Little endian)
            iaBuffer = ioBINReader.ReadBytes(8)
            obPInfo.iStart = LittleEndianConverter(iaBuffer)

            ' Read last LBA sector number (Little endian)
            ' Number of Sectors = (Last Sector + 1) - First Sector
            iaBuffer = ioBINReader.ReadBytes(8)
            obPInfo.iEnd = LittleEndianConverter(iaBuffer)

            ioBINReader.BaseStream.Position += 8 ' Skip attributes

            ' Read partition name data, remove NULL characters (must)
            iaBuffer = ioBINReader.ReadBytes(72)
            obPInfo.sLabel = Encoding.Unicode.GetString(iaBuffer)
            obPInfo.sLabel = obPInfo.sLabel.Replace(vbNullChar, "").ToLower
            'obPInfo.iLabel = Ascii2Int(obPInfo.sLabel)

            If obPInfo.iStart = 0 OrElse _
                obPInfo.sLabel = "" Then Exit While

            galoLookUp(obPInfo.iLUN).Add(obPInfo)
            ioBINReader.BaseStream.Position += 32 ' 1st LBA offset

        End While

        ' Using LINQ query to sort the list ascendingly, for later use in binary serach

        'If isSort Then LINQSortAscii(galoLookUp(obPInfo.iLUN))
        If isSort Then LINQSort(galoLookUp(obPInfo.iLUN))
        'If isSort Then LambdaSort(galoLookUp(obPInfo.iLUN))

        ioBINReader.Close()
        ioBINReader.Dispose()
        ioBINReader = Nothing
        Erase iaBuffer

    End Sub

    ' fh_loader.exe --port=\\.\COM? --convertprogram2read --sendimage=mpt.bin --start_sector=6 --lun=0 '
    ' --num_sectors=8192 --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs

    Protected Function BuildCommand( _
                                   ByRef obPInfo As stPInfo, _
                                   ByVal isTemp As Boolean) As String

        Dim sFileName As String = BuildFileName(obPInfo, isTemp)
        Dim saMerge() As String = { _
                    " --port=\\.\$", _
                    " --convertprogram2read --sendimage=$", _
                    " --start_sector=#", _
                    " --lun=#", _
                    " --num_sectors=#", _
                    " --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs"}

        saMerge(0) = saMerge(0).Replace("$", gsCOMPort)
        saMerge(1) = saMerge(1).Replace("$", sFileName)
        saMerge(2) = saMerge(2).Replace("#", obPInfo.iStart)
        saMerge(3) = saMerge(3).Replace("#", obPInfo.iLUN)
        saMerge(4) = saMerge(4).Replace("#", obPInfo.iSectors)
        sFileName = ""

        For iCnt As Byte = 0 To saMerge.Count - 1
            sFileName &= saMerge(iCnt)
        Next

        ShowProgress(obPInfo, ProgressType.PR_BKP)

        Erase saMerge
        Return sFileName

    End Function

    Protected Function BuildFileName( _
                                    ByRef obPInfo As stPInfo, _
                                    ByVal isTemp As Boolean, _
                                    Optional ByVal isFlash As Boolean = False) As String

        Dim sFileName, sDir As String
        Dim saMerge() As String = {"$", "lun#", "_$.bin"}

        Select Case True
            Case isTemp : sDir = getTempFolder()
            Case isFlash : sDir = gsFlashDir
            Case Else : sDir = gsBackupDir
        End Select

        saMerge(0) = saMerge(0).Replace("$", sDir)
        saMerge(1) = saMerge(1).Replace("#", obPInfo.iLUN)
        saMerge(2) = saMerge(2).Replace("$", obPInfo.sLabel)

        For iCnt As Byte = 0 To saMerge.Count - 1
            sFileName &= saMerge(iCnt)
        Next

        Erase saMerge
        Return sFileName

    End Function

    Protected Sub ShowProgress(ByRef obPInfo As stPInfo, ByVal eOP As ProgressType)

        Dim sOutput As String = goSpeaker.ID2Msg(46)

        Select Case eOP
            Case ProgressType.PR_BKP : sOutput = goSpeaker.ID2Msg(46)
            Case ProgressType.PR_FLH : sOutput = goSpeaker.ID2Msg(45)
            Case ProgressType.PR_ERS : sOutput = goSpeaker.ID2Msg(24)
        End Select

        sOutput = sOutput.Replace("$", obPInfo.iLUN)
        sOutput = sOutput.Replace("@", obPInfo.sLabel)
        sOutput = sOutput.Replace("#", obPInfo.iStart)
        sOutput = sOutput.Replace("?", obPInfo.sSectors)

        Console.WriteLine(sOutput, vbNewLine)

    End Sub

    Private Sub LINQSort(ByRef lsPInfo As List(Of stPInfo))

        Dim lsSorted = _
            From obTInfo As stPInfo In lsPInfo _
            Order By obTInfo.sLabel Ascending Select obTInfo

        lsPInfo = lsSorted.ToList()
        lsSorted = Nothing

    End Sub

    Private Sub LambdaSort(ByRef lsPInfo As List(Of stPInfo))

        lsPInfo = lsPInfo.OrderBy( _
            Function(obTInfo) obTInfo.sLabel).ToList()

    End Sub

    Protected Function LookUpNames(ByRef obPInfo As stPInfo) As Boolean

        ' Updated on: 08/01/2024
        Try

            Dim iLBound, iUBound As Byte

            Dim sLabel As String = _
                obPInfo.sLabel.ToLower

            If obPInfo.iLUN.HasValue Then

                iLBound = obPInfo.iLUN
                iUBound = obPInfo.iLUN

            Else

                iLBound = 0
                iUBound = galoLookUp.Count - 1

            End If

            For iCnt As Byte = iLBound To iUBound

                For iCnt2 As UInt16 = 0 To galoLookUp(iCnt).Count - 1

                    If sLabel = galoLookUp(iCnt)(iCnt2).sLabel Then

                        obPInfo = galoLookUp(iCnt)(iCnt2)
                        Return True

                    End If

                Next

            Next

            Return False

        Catch

            ' s4704 reported a crash with BufferOverFlow in this function. 
            ' I've double checked it with my phone and with s4704's GPT headers from Motorola Edge X30
            ' in debug/emulation mode... Didn't get any issues :( Decided to add this piece of code
            ' to further investigate this matter.

            Dim ioOutputFile As StreamWriter
            ioOutputFile = File.AppendText("errors.txt")

            ioOutputFile.WriteLine("Error occured in LookUpNames:" & Err.Description)
            ioOutputFile.WriteLine("Switching to fallback function...")
            ioOutputFile.Close()
            ioOutputFile = Nothing

            ' Return false on error
            Return FallBackBinary(obPInfo)

        End Try


    End Function

    ' Added on: 08/01/2024
    ' For testing Motorola Issue

    Private Function FallBackBinary(ByRef obPInfo As stPInfo) As Boolean

        Try

            Dim iLBound, iUBound As Byte
            Dim iLow, iMid, iHigh As Int16

            Dim sLookUp As String

            Dim sLabel As String = _
                obPInfo.sLabel.ToLower

            If obPInfo.iLUN.HasValue Then

                iLBound = obPInfo.iLUN
                iUBound = obPInfo.iLUN

            Else

                iLBound = 0
                iUBound = galoLookUp.Count - 1

            End If

            For iCnt As Byte = iLBound To iUBound

                iLow = 0
                iMid = 0
                iHigh = galoLookUp(iCnt).Count - 1

                Do Until iLow > iHigh

                    iMid = (iHigh + iLow) / 2

                    sLookUp = galoLookUp(iCnt)(iMid).sLabel.ToLower

                    If sLabel = sLookUp Then

                        obPInfo = galoLookUp(iCnt)(iMid) : Return True

                    ElseIf sLabel > sLookUp Then : iLow = iMid + 1
                    Else : iHigh = iMid - 1

                    End If

                Loop

            Next

            Return False

        Catch

            Dim ioOutputFile As StreamWriter
            ioOutputFile = File.AppendText("errors.txt")

            ioOutputFile.WriteLine("Error occured in FallBackLookUpNames:" & Err.Description)
            ioOutputFile.Close()
            ioOutputFile = Nothing

            ' Return false on error
            Return False

        End Try


    End Function

    Protected Function ValidatePartLabel(ByRef obPInfo As stPInfo) As Boolean

        If Not LookUpNames(obPInfo) Then

            Console.WriteLine(goSpeaker.ID2Msg(29) & obPInfo.sLabel)
            Console.WriteLine(goSpeaker.ID2Msg(10))

            If Console.ReadKey(True).Key = ConsoleKey.A Then _
                geFailed = ErrorType.ER_ABORT ' ByPass ProcessCompletedMsg

            'Console.Clear()
            Return False

        End If

        Return True

    End Function

    Private Function UserDataWarn(ByRef obPInfo As stPInfo) As Boolean

        ' Just in case the user have erased every single partiton in the QFIL and now's trying to 
        ' make userdata backup for some reason... The world is full of em 12 o'clock flashers, 
        ' and you never know what to expect of em

        If Not LookUpNames(obPInfo) Then

            Console.WriteLine(goSpeaker.ID2Msg(29) & obPInfo.sLabel)
            Console.WriteLine(goSpeaker.ID2Msg(10))

            If Console.ReadKey(True).Key = ConsoleKey.A Then _
                geFailed = ErrorType.ER_ABORT ' ByPass ProcessCompletedMsg

            Console.Clear()
            Return False

        End If

        Console.WriteLine(goSpeaker.ID2Msg(6))
        Console.WriteLine(goSpeaker.ID2Msg(7))
        Console.WriteLine(goSpeaker.ID2Msg(8))
        Console.WriteLine(goSpeaker.ID2Msg(9))

        If Console.ReadKey(True).Key = ConsoleKey.A Then

            geFailed = ErrorType.ER_ABORT ' ByPass ProcessCompletedMsg

            'Console.Clear()
            Return False

        End If

        Return True

    End Function

End Class
