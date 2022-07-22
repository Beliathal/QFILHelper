Imports System.IO
Imports Microsoft.VisualBasic

Public Class clsFlash : Inherits clsVital

    Private ExtractFileName As Func(Of String, String) = _
        Function(sCurName As String) sCurName.Replace(getDirWSlash, "").ToLower

    Public Sub FlashFirmware()

        'If Not ValidateFiles_Debug() Then Exit Sub
        If Not ValidateFiles() Then Exit Sub

        Dim saFileList() As String
        Dim saBuffer() As String
        Dim sCMDLine As String
        Dim iCurIndex As Int16

        gsDirName = "Flash"

        If Not LoadFlashList(saFileList) OrElse _
            Not DisplayWarning(saFileList) Then
            saFileList = Nothing
            Exit Sub
        End If

        saBuffer = File.ReadAllLines(gsFileName)

        For Each sCurFile As String In saFileList

            ' User placed unknown files in the flash folder?
            If Not IsValidBackup(sCurFile) Then Continue For

            If gsLabel = "hidden" Then

                sCMDLine = BuildCommand(sCurFile)

                If Not ExecuteCommand(sCMDLine) Then Exit For

            ElseIf Not gsLabel = "" Then

                ' It's a partition backup file... possibly :) hopefully :)
                iCurIndex = LocatePartition(saBuffer, gsLabel)

                If iCurIndex > -1 AndAlso _
                    ParseXML(saBuffer(iCurIndex)) Then

                    sCMDLine = BuildCommand(sCurFile)

                    If Not ExecuteCommand(sCMDLine) Then Exit For

                End If

            ElseIf Not gsLUN = "" _
                AndAlso goSpeaker.gbAdvEnabled Then

                ' I've flashed LUN4 with this code without any issues, 
                ' but this is too dangerous for users that don't understand what they'r doing.
                ' Use -advanced parameter withing the command line to enable this code.

                'This code will work with hidden LUN backup as well, there's no need to add anything

                gsLabel = "LUN Recovery"
                gsStart = 0

                sCMDLine = BuildCommand(sCurFile)

                If Not ExecuteCommand(sCMDLine) Then Exit For

            End If

        Next

        saFileList = Nothing
        saBuffer = Nothing

        ProcessCompletedMsg()

    End Sub

    Private Function LoadFlashList(ByRef saFileList() As String) As Boolean

        If Not Directory.Exists(gsDirName) Then
            Console.WriteLine(goSpeaker.ID2Msg(37))
            Console.WriteLine(goSpeaker.ID2Msg(17))
            Console.ReadKey(True)
            Return False
        End If

        saFileList = Directory.GetFiles(gsDirName, "*.bin")

        If saFileList.Count = 0 Then
            Console.WriteLine(goSpeaker.ID2Msg(38))
            Console.WriteLine(goSpeaker.ID2Msg(17))
            Console.ReadKey(True)
            Return False
        End If

        Return True

    End Function

    ' Will show the list of files faound in the flash dir 
    ' and warn user their about to flash firmware

    Private Function DisplayWarning(ByRef saFileList() As String) As Boolean

        Dim sOutput As String

        For Each sFileName As String In saFileList

            sOutput += ExtractFileName(sFileName) & ", "

        Next

        Console.WriteLine(goSpeaker.ID2Msg(39) & vbCrLf)
        Console.WriteLine(sOutput.Substring(0, sOutput.Length - 2))
        Console.WriteLine(goSpeaker.ID2Msg(40))

        If Console.ReadKey(True).Key = ConsoleKey.Y Then
            Console.Clear()
            Return True
        End If

    End Function

    Private Overloads Function BuildCommand(ByRef sCurFile As String) As String

        'fh_loader.exe --port=\\.\COM? --sendimage=ftm.bin --start_sector=23816 --lun=0 --noprompt 
        '--showpercentagecomplete --zlpawarehost=1 --memoryname=ufs

        'fh_loader.exe --port=\\.\COM? --sendimage=LUN4_complete.bin --start_sector=0 --lun=4 --noprompt 
        ' --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs

        Dim sCurLabel As String = getDirWSlash & sCurFile

        BuildCommand = "--port=\\.\" & gsCOMPort & _
                       " --sendimage=" & sCurLabel & _
                       " --start_sector=" & gsStart & _
                       " --lun=" & gsLUN & _
                       " --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs"

        Console.WriteLine(goSpeaker.ID2Msg(29) & gsLabel & _
                          " | LUN: " & gsLUN & _
                          " | Start: " & gsStart & _
                          vbNewLine)

    End Function

    ' Split those by underscore for any possible variation:

    ' lun3_hidden_s -2048.bin
    ' lun4_hidden_partition_b-417181_s-1.bin
    ' lun4_abl_a.bin
    ' lun0_ftm.bin
    ' lun4.bin
    ' abl_a.bin
    ' ftm.bin

    Private Function IsValidBackup(ByRef sCurFile As String) As Boolean

        ResetInfo()
        sCurFile = ExtractFileName(sCurFile)

        Dim saSplitByUnder() As String = sCurFile.Split("_")

        If saSplitByUnder.Count = 1 Then

            If sCurFile.StartsWith("lun") Then

                ' This is a LUN image
                ' Example: lun4.bin

                Return ParseLUN(saSplitByUnder)

            End If

            ' This is a partition image
            ' Example: [0] ftm.bin

            Return ParseShortPart(saSplitByUnder)

        End If


        If saSplitByUnder(0).StartsWith("lun") Then

            If saSplitByUnder(1).StartsWith("hidden") Then

                If saSplitByUnder(2).StartsWith("partition") Then

                    ' This is a hidden partition image
                    ' Example: [0] lun4 [1] hidden [2] partition [3] b-417181 [4] s-1.bin

                    Return ParseHiddenPart(saSplitByUnder)

                End If

                ' This is a hidden LUN image
                ' Example: [0] lun3 [1] hidden [2] s-2048.bin

                Return ParseHiddenLUN(saSplitByUnder)

            ElseIf saSplitByUnder(1).StartsWith("complete") Then

                ' This is a LUN image
                ' Example: lun4_complete.bin

                Return ParseLUN(saSplitByUnder)

            End If

            ' This is a partition image
            ' Example: [0] lun0 [1] ftm.bin
            ' Example: [0] lun4 [1] abl [2] a.bin

            Return ParseFullPart(saSplitByUnder)

        End If

        ' This is a partition image
        ' Example: [0] ALIGN [1] TO [2] 128K [3] 2
        ' Example: [0] raw [1] resources [2] b

        Return ParseShortPart(saSplitByUnder)

    End Function

    Private Function ParseHiddenPart(ByRef saSplitByUnder() As String) As Boolean

        ' This is a hidden partition image
        ' Example: [0] lun4 [1] hidden [2] partition [3] b-417181 [4] s-1.bin

        If saSplitByUnder(0).Length <> 4 Then Return False

        Dim saPBeg() As String = saSplitByUnder(3).Split("-")
        Dim saPTot() As String = saSplitByUnder(4).Split("-")
        Dim iTest As UInt32

        If saPBeg.Count <> 2 OrElse _
            saPTot.Count <> 2 Then Return False

        gsLabel = "hidden"
        gsStart = saPBeg(1)
        gsSectors = saPTot(1).Split(".")(0)
        gsLUN = saSplitByUnder(0).Substring(3, 1)

        If Not UInt32.TryParse(gsLUN, iTest) AndAlso _
           Not UInt32.TryParse(gsStart, iTest) AndAlso _
           Not UInt32.TryParse(gsSectors, iTest) Then Return False

        Return True

    End Function

    Private Function ParseHiddenLUN(ByRef saSplitByUnder() As String) As Boolean

        ' This is a hidden LUN image
        ' Example: [0] lun3 [1] hidden [2] s-2048.bin

        If saSplitByUnder(0).Length <> 4 Then Return False

        Dim saLTot() As String = saSplitByUnder(2).Split("-")
        Dim iTest As UInt32

        If saLTot.Count <> 2 Then Return False

        gsSectors = saLTot(1).Split(".")(0)
        gsLUN = saSplitByUnder(0).Substring(3, 1)

        If Not UInt32.TryParse(gsLUN, iTest) AndAlso _
           Not UInt32.TryParse(gsSectors, iTest) Then Return False

        Return True

    End Function

    Private Function ParseLUN(ByRef saSplitByUnder() As String) As Boolean

        ' This is a LUN image
        '  Example: [0] lun4.bin

        If saSplitByUnder(0).Length <> 8 Then Return False

        Dim iTest As Byte
        gsLUN = saSplitByUnder(0).Substring(3, 1)

        If Not Byte.TryParse(gsLUN, iTest) Then Return False

        Return True

    End Function

    Private Function ParseFullPart(ByRef saSplitByUnder()) As String

        If saSplitByUnder(0).Length <> 4 Then Return False

        ' This is a partition image
        ' Example: [0] lun4 [1] abl [2] a.bin
        ' Example: [0] lun0 [1] ftm.bin

        ' Example: [0] lun5 [1] ALIGN [2] TO [3] 128K [4] 2
        ' Example: [0] lun4 [1] raw [2] resources [3] b
        ' Example: [0] lun4 [1] abl [2] a.bin
        ' Example: [0] lun0 [1] ftm.bin

        Dim iCnt As Byte
        gsLUN = saSplitByUnder(0).Substring(3, 1)

        If Not Byte.TryParse(gsLUN, iCnt) Then Return False

        Try

            For iCnt = 1 To saSplitByUnder.Count - 2
                gsLabel &= saSplitByUnder(iCnt) & "_"
            Next

            gsLabel &= saSplitByUnder(iCnt).Split(".")(0)

            Return True

        Catch

            Return False

        End Try

    End Function


    Private Function ParseShortPart(ByRef saSplitByUnder() As String) As Boolean

        ' This is a partition image
        ' Example: [0] ALIGN [1] TO [2] 128K [3] 2
        ' Example: [0] raw [1] resources [2] b
        ' Example: [0] abl [1] a.bin
        ' Example: [0] ftm.bin

        Try

            Dim iCnt As Byte

            For iCnt = 0 To saSplitByUnder.Count - 2
                gsLabel &= saSplitByUnder(iCnt) & "_"
            Next

            gsLabel &= saSplitByUnder(iCnt).Split(".")(0)

            Return True

        Catch

            Return False

        End Try


    End Function

End Class