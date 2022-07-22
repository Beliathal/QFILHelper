Imports System.IO
Imports Microsoft.VisualBasic

Public Class clsHLUNs : Inherits clsLUNs

    Private ReturnPossibleSects As Func(Of Byte, String) = _
    Function(iCurLUN As Byte) goSpeaker.ID2Msg(23).Replace("@", SuggestSectors(iCurLUN))


    ' There's no way to know the exact number of sectors contained within them hidden LUNs, 
    ' as their sizes can change depending on the phone model, OS version & build number.

    ' This is why I've decided to let the user enter this info manualy, but I still can
    ' help by suggesting a possible number based on this guide:
    ' https://forum.xda-developers.com/t/tutorial-full-flash-backup-and-restore.4362809/

    Public Sub BackupHiddenLUNs()

        If Not ValidateFiles() Then Exit Sub

        Dim sCurInput As String = String.Empty
        Dim sCMDLine As String
        Dim iCurStep As SByte
        Dim iCurLUN As Byte
        Dim iCurSectors As UInt32

        ResetInfo()
        CreateBackupFolder()
        Console.CursorVisible = True

        Do

            Select Case iCurStep

                Case 0

                    Console.Clear()
                    Console.WriteLine(goSpeaker.ID2Msg(22))
                    sCurInput = Console.ReadLine

                    If Not Byte.TryParse(sCurInput, iCurLUN) _
                        Then iCurStep = -2 Else iCurStep = 1

                Case 1

                    Console.WriteLine(vbCrLf & ReturnPossibleSects(iCurLUN))
                    sCurInput = Console.ReadLine

                    If Not UInt32.TryParse(sCurInput, iCurSectors) _
                        Then iCurStep = -1 Else iCurStep = 2

                Case 2

                    setLUN = iCurLUN
                    setSectors = iCurSectors
                    sCMDLine = BuildCommand()
                    iCurStep = 0

                    If Not ExecuteCommand(sCMDLine) Then Exit Do

                Case -2 To -1

                    Console.WriteLine(goSpeaker.ID2Msg(25) & vbCrLf)
                    iCurStep += 2

            End Select

        Loop Until sCurInput.ToLower = "q"

        Console.CursorVisible = False

        CleanUpBackupFolder()
        'ProcessCompletedMsg()

    End Sub

    Private Shadows Function BuildCommand() As String

        ' fh_loader.exe --port=\\.\COM --convertprogram2read --sendimage=lun3_hidden_s-6.bin --start_sector=0 
        '--lun=1 --num_sectors=2048 --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs

        Dim sCurLabel As String = getDirWSlash & "lun" & gsLUN & getHLString & ".bin"

        BuildCommand = "--port=\\.\" & gsCOMPort & _
                       " --convertprogram2read --sendimage=" & sCurLabel & _
                       " --start_sector=0" & _
                       " --lun=" & gsLUN & _
                       " --num_sectors=" & gsSectors & _
                       " --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs"

        Console.WriteLine(goSpeaker.ID2Msg(24) & "hidden LUN: " & gsLUN & _
                          " | Sectors: " & gsSectors & vbNewLine)

    End Function

End Class
