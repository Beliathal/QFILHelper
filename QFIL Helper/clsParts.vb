Imports System.IO
Imports Microsoft.VisualBasic

' Purpouse: Backup all visible partitions

Public Class clsParts : Inherits clsInit

    Public Sub BackupPartitions()

        If Not ValidateFiles() Then Exit Sub

        Dim ioSourceFile As StreamReader
        Dim sBuffer As String
        Dim sCMDLine As String

        ResetInfo()
        CreateBackupFolder()

        ioSourceFile = File.OpenText(gsFileName)

        While (Not ioSourceFile.EndOfStream)

            sBuffer = ioSourceFile.ReadLine()

            If sBuffer.StartsWith("  <partition label") Then
                If ParseXML(sBuffer) Then

                    ' Skip Userdata partition
                    If gsLabel = "userdata" Then Continue While

                    sCMDLine = BuildCommand()

                    If Not ExecuteCommand(sCMDLine) Then Exit While

                End If
            End If

        End While

        ioSourceFile.Close()
        ioSourceFile.Dispose()
        ioSourceFile = Nothing

        CleanUpBackupFolder()
        ProcessCompletedMsg()

    End Sub

    Protected Function ParseXML(ByRef sCurLine As String) As Boolean

        ' <partition label="mpt" physical_partition_number="0" start_sector="6" num_partition_sectors="8192" 
        ' type="SKIP" guid="SKIP" />

        Dim saBySpaces() As String = sCurLine.Split(" ")

        For Each sElement As String In saBySpaces

            If sElement = "" Then Continue For

            Select Case True

                Case sElement.StartsWith("label")

                    gsLabel = sElement.Replace("label=", "")
                    gsLabel = gsLabel.Replace("""", "")
                    Exit Select

                Case sElement.StartsWith("physical")

                    gsLUN = sElement.Replace("physical_partition_number=", "")
                    gsLUN = gsLUN.Replace("""", "")
                    Exit Select

                Case sElement.StartsWith("start")

                    gsStart = sElement.Replace("start_sector=", "")
                    gsStart = gsStart.Replace("""", "")
                    Exit Select

                Case sElement.StartsWith("num_")

                    gsSectors = sElement.Replace("num_partition_sectors=", "")
                    gsSectors = gsSectors.Replace("""", "")
                    Exit Select

            End Select

        Next

        If Not UInt32.TryParse(gsLUN, giLUN) Or _
           Not UInt32.TryParse(gsStart, giStart) Or _
           Not UInt32.TryParse(gsSectors, giSectors) Then

            Console.WriteLine(goUILang.ID2Msg(8) & sCurLine)
            Console.ReadKey()

            Return False

        End If

        Return True

    End Function

    Protected Function BuildCommand() As String

        ' fh_loader.exe --port=\\.\COM? --convertprogram2read --sendimage=mpt.bin --start_sector=6 --lun=0 '
        ' --num_sectors=8192 --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs

        Dim sCurLabel As String = DirWithSlash & "lun" & gsLUN & "_" & gsLabel & ".bin"

        BuildCommand = "--port=\\.\" & gsCOMPort & _
                       " --convertprogram2read --sendimage=" & sCurLabel & _
                       " --start_sector=" & gsStart & _
                       " --lun=" & gsLUN & _
                       " --num_sectors=" & gsSectors & _
                       " --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs"

        Console.WriteLine(goUILang.ID2Msg(10) & gsLabel & _
                          ".bin | LUN: " & gsLUN & _
                          " | Start: " & gsStart & _
                          " | Sectors: " & gsSectors & _
                          vbNewLine)

    End Function

    Protected Function ExecuteCommand( _
                              ByRef sCMDLine As String) As Boolean

        Dim oProcess As New Process
        Dim sError, sBuffer As String

        oProcess.StartInfo.Arguments = sCMDLine
        oProcess.StartInfo.CreateNoWindow = False
        oProcess.StartInfo.FileName = "fh_loader.exe"
        oProcess.StartInfo.UseShellExecute = False
        oProcess.StartInfo.RedirectStandardOutput = True
        oProcess.StartInfo.RedirectStandardError = True
        oProcess.Start()

        sBuffer = oProcess.StandardOutput.ReadToEnd()
        sError = oProcess.StandardError.ReadToEnd()

        oProcess.WaitForExit()
        oProcess.Close()
        oProcess.Dispose()
        oProcess = Nothing

        Console.WriteLine(sBuffer)
        Console.WriteLine(sError)

        If sBuffer.Contains("Failed to open com port") OrElse _
            sBuffer.Contains("ERROR: Could not write to") OrElse _
            sBuffer.Contains("SAHARA mode!!") Then
            Console.Clear()
            Console.WriteLine(goUILang.ID2Msg(11) & vbCrLf)
            Console.WriteLine(goUILang.ID2Msg(12) & vbCrLf)
            Console.WriteLine(goUILang.ID2Msg(13))
            Console.WriteLine(goUILang.ID2Msg(14))
            Console.WriteLine(goUILang.ID2Msg(15))
            Console.WriteLine(goUILang.ID2Msg(16))
            Console.WriteLine(vbCrLf & goUILang.ID2Msg(17))
            Console.ReadKey(True)
            gbFailed = True
        Else : Return True
        End If

    End Function

End Class
