Imports System.IO
Imports Microsoft.VisualBasic

Public Class clsHParts : Inherits clsParts

    ' Some partitions, like DDR, CDT (LUN3), DevInfo, Limits (LUN4), are hidden and neither being displayed 
    ' nor exported to partitionsList.xml

    ' QFIL Helper will attempt to locate those partitions by searching for gaps between the end sector and 
    ' the start sector of two neighboring partitions

    Public Sub FindHiddenParts()

        If Not ValidateFiles() Then Exit Sub

        Dim ioSourceFile As StreamReader
        Dim sBuffer As String
        Dim sCMDLine As String

        Dim iLastLUN As Nullable(Of Byte)
        Dim iLastSector As UInt32

        ResetInfo()
        CreateBackupFolder()

        ioSourceFile = File.OpenText(gsFileName)

        While (Not ioSourceFile.EndOfStream)

            sBuffer = ioSourceFile.ReadLine()

            If sBuffer.StartsWith("  <partition label") Then
                If ParseXML(sBuffer) Then

                    ' Conditions to check:
                    ' 1. Skip checking 1st 6 sectors of every partition, those are GPT tables IMHO
                    ' 2. LUN:0, Partiton:0, Sector:?
                    ' 3. Current LUN > Previous LUN ?: Reached partition boundaries

                    If Not iLastLUN.HasValue OrElse giLUN > iLastLUN Then

                        iLastLUN = giLUN
                        iLastSector = getSize
                        Continue While

                    End If

                    If giStart > iLastSector Then

                        ' Hidden Start = Previous Start + Sectors [iLastSector]
                        ' Hidden Sectors = New Start - (Previous Start + Previous Sectors)

                        setSectors = giStart - iLastSector
                        setStart = iLastSector
                        sCMDLine = BuildCommand()

                        If Not ExecuteCommand(sCMDLine) Then Exit While

                    End If

                    iLastSector = getSize

                End If
            End If

        End While

        ioSourceFile.Close()
        ioSourceFile.Dispose()
        ioSourceFile = Nothing

        CleanUpBackupFolder()
        ProcessCompletedMsg()

    End Sub

    Private Shadows Function BuildCommand() As String

        ' fh_loader.exe --port=\\.\COM? --convertprogram2read --sendimage=lun5_hidden_partiton_b-0_s-6.bin
        ' --start_sector=6 --lun=0  --num_sectors=8192 --noprompt --showpercentagecomplete --zlpawarehost=1 
        ' --memoryname=ufs

        Dim sCurLabel As String = getDirWSlash & "lun" & gsLUN & getHPString & ".bin"

        BuildCommand = "--port=\\.\" & gsCOMPort & _
                       " --convertprogram2read --sendimage=" & sCurLabel & _
                       " --start_sector=" & gsStart & _
                       " --lun=" & gsLUN & _
                       " --num_sectors=" & gsSectors & _
                       " --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs"

        Console.WriteLine(goSpeaker.ID2Msg(36) & "hidden partition at" & _
                          " LUN: " & gsLUN & _
                          " | Start: " & gsStart & _
                          " | Sectors: " & gsSectors & _
                          vbNewLine)

    End Function

End Class
