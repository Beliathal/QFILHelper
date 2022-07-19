Imports System.IO
Imports Microsoft.VisualBasic

Public Class clsFlash : Inherits clsVital

    Private ExtractFileName As Func(Of String, String) = _
        Function(sCurName As String) sCurName.Replace(DirWithSlash, "").ToLower

    Public Sub FlashFirmware()

        ' If Not ValidateFiles_Debug() Then Exit Sub
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

            If Not gsLabel = "" Then

                ' It's a partition backup file... possibly :) hopefully :)
                iCurIndex = LocatePartition(saBuffer, gsLabel)

                If iCurIndex > -1 AndAlso _
                    ParseXML(saBuffer(iCurIndex)) Then

                    sCMDLine = BuildCommand(sCurFile)

                    If Not ExecuteCommand(sCMDLine) Then Exit For

                End If

            ElseIf Not gsLUN = "" Then

                ' I've flashed LUN4 with this code without any issues, 
                ' but this is too dangerous, someone can destroy their phone

                Continue For

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
            Console.WriteLine(goUILang.ID2Msg(37))
            Console.WriteLine(goUILang.ID2Msg(17))
            Console.ReadKey(True)
            Return False
        End If

        saFileList = Directory.GetFiles(gsDirName, "*.bin")

        If saFileList.Count = 0 Then
            Console.WriteLine(goUILang.ID2Msg(38))
            Console.WriteLine(goUILang.ID2Msg(17))
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

        Console.WriteLine(goUILang.ID2Msg(39) & vbCrLf)
        Console.WriteLine(sOutput.Substring(0, sOutput.Length - 2))
        Console.WriteLine(goUILang.ID2Msg(40))

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

        Dim sCurLabel As String = DirWithSlash & sCurFile

        BuildCommand = "--port=\\.\" & gsCOMPort & _
                       " --sendimage=" & sCurLabel & _
                       " --start_sector=" & gsStart & _
                       " --lun=" & gsLUN & _
                       " --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs"

        Console.WriteLine(goUILang.ID2Msg(29) & gsLabel & _
                          " | LUN: " & gsLUN & _
                          " | Start: " & gsStart & _
                          vbNewLine)

    End Function

    ' Let's see what we're dealing with. Is it a LUN backup or a partition backup? 

    ' QFIL Helper will accept only files named according to the following rules:

    ' lun0.bin - a LUN backup
    ' lun0_complete.bin - a LUN backup

    ' lun4_abl_a.bin - a partition
    ' abl_a.bin - a partition

    ' lun4_hidden_partition_b-000_s-0000 - not processing yet

    Private Function IsValidBackup(ByRef sCurFile As String) As Boolean

        ResetInfo()
        sCurFile = ExtractFileName(sCurFile)

        With sCurFile

            If .StartsWith("lun") AndAlso _
               .EndsWith("_complete.bin") AndAlso _
               .Length = "lun0_complete.bin".Length Then

                gsLUN = .Substring("lun".Length, 1)
                Return True

            ElseIf .StartsWith("lun") AndAlso _
                   .EndsWith(".bin") AndAlso _
                   .Length = "lun0.bin".Length Then

                gsLUN = .Substring("lun".Length, 1)
                Return True

            ElseIf .StartsWith("lun") AndAlso _
                   .EndsWith(".bin") AndAlso _
                   .Contains("hidden") Then

                ' no code yet
                Return False

            ElseIf .StartsWith("lun") AndAlso _
                   .EndsWith(".bin") AndAlso Not _
                   .Contains("hidden") Then

                Dim iLPart As Byte = "lun0_".Length
                Dim iRPart As Byte = ".bin".Length

                gsLUN = .Substring("lun".Length, 1)
                gsLabel = .Substring(iLPart, .Length - (iLPart + iRPart))
                Return True

            ElseIf .EndsWith(".bin") Then

                Dim iRPart As Byte = ".bin".Length
                gsLabel = .Substring(0, .Length - iRPart)
                Return True

            ElseIf .EndsWith(".img") Then

                ' Reserved for TWRP flashing with QFIL
                ' Not sure if can be done, need to test
                Return False

            End If

        End With

    End Function

End Class