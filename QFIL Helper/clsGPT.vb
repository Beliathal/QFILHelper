Imports System.IO
Imports Microsoft.VisualBasic

Public Class clsGPT : Inherits clsHParts

    ' Every partition has GPT headers that start from sector 0 and end at sector 6.

    Public Sub BackupGPTHeaders()

        'If Not ValidateFiles_Debug() Then Exit Sub
        If Not ValidateFiles() Then Exit Sub

        Dim sCMDLine As String

        ResetInfo()
        CreateBackupFolder()

        For iCnt As Byte = 0 To 6


            setSectors = 0
            setStart = giStart
            setLUN = iCnt

            sCMDLine = BuildCommand()
            If Not ExecuteCommand(sCMDLine) Then Exit For

        Next


        CleanUpBackupFolder()
        ProcessCompletedMsg()

    End Sub

    Private Shadows Function BuildCommand() As String

        ' fh_loader.exe --port=\\.\COM? --convertprogram2read --sendimage=lun5_gpt_partiton_b-0_s-6.bin
        ' --start_sector=0 --lun=0  --num_sectors=6 --noprompt --showpercentagecomplete --zlpawarehost=1 
        ' --memoryname=ufs

        Dim sCurLabel As String = getDirWSlash & "lun" & gsLUN & getGPTString & ".bin"

        BuildCommand = "--port=\\.\" & gsCOMPort & _
                       " --convertprogram2read --sendimage=" & sCurLabel & _
                       " --start_sector=" & gsStart & _
                       " --lun=" & gsLUN & _
                       " --num_sectors=" & gsSectors & _
                       " --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs"

        Console.WriteLine(goSpeaker.ID2Msg(36) & "GPT header at" & _
                          " LUN: " & gsLUN & _
                          " | Start: " & gsStart & _
                          " | Sectors: " & gsSectors & _
                          vbNewLine)

    End Function

End Class
