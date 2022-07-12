Imports System.IO
Imports Microsoft.VisualBasic

Public Class clsVParts : Inherits clsInit

    Public Sub BackupPartitions()

        Dim ioSourceFile As StreamReader
        Dim sBuffer As String
        Dim sCMDLine As String

        ioSourceFile = File.OpenText(sFileName)
        CreateBackupFolder()

        While (Not ioSourceFile.EndOfStream)

            sBuffer = ioSourceFile.ReadLine()

            If sBuffer.StartsWith("  <partition label") Then
                If ParseXML(sBuffer) Then

                    ' Skip Userdata partition
                    If sLabel = "userdata" Then Continue While

                    sCMDLine = BuildCommand()

                    If Not ExecuteCommand(sCMDLine) Then Exit While

                End If
            End If

        End While

        ioSourceFile.Close()
        ioSourceFile.Dispose()
        ioSourceFile = Nothing
        CleanUpBackupFolder()

    End Sub

    Protected Function ParseXML(ByRef sCurLine As String) As Boolean

        ' <partition label="mpt" physical_partition_number="0" start_sector="6" num_partition_sectors="8192" 
        ' type="SKIP" guid="SKIP" />

        Dim saBySpaces() As String = sCurLine.Split(" ")

        For Each sElement As String In saBySpaces

            If sElement = "" Then Continue For

            Select Case True

                Case sElement.StartsWith("label")

                    sLabel = sElement.Replace("label=", "")
                    sLabel = sLabel.Replace("""", "")
                    Exit Select

                Case sElement.StartsWith("physical")

                    sLUN = sElement.Replace("physical_partition_number=", "")
                    sLUN = sLUN.Replace("""", "")
                    Exit Select

                Case sElement.StartsWith("start")

                    sStart = sElement.Replace("start_sector=", "")
                    sStart = sStart.Replace("""", "")
                    Exit Select

                Case sElement.StartsWith("num_")

                    sSectors = sElement.Replace("num_partition_sectors=", "")
                    sSectors = sSectors.Replace("""", "")
                    Exit Select

            End Select

        Next

        If Not UInt32.TryParse(sLUN, iLUN) Or _
           Not UInt32.TryParse(sStart, iStart) Or _
           Not UInt32.TryParse(sSectors, iSectors) Then

            Console.WriteLine(ID2Msg(8) & sCurLine)
            Console.ReadKey()

            Return False

        End If

        Return True

    End Function

    Protected Function BuildCommand() As String

        ' fh_loader.exe --port=\\.\COM? --convertprogram2read --sendimage=mpt.bin --start_sector=6 --lun=0 '
        ' --num_sectors=8192 --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs

        Dim sCurLabel As String = DirName & "lun" & sLUN & "_" & sLabel & ".bin"

        BuildCommand = "--port=\\.\COM" & sCOMPort & _
                       " --convertprogram2read --sendimage=" & sCurLabel & _
                       " --start_sector=" & sStart & _
                       " --lun=" & sLUN & _
                       " --num_sectors=" & sSectors & _
                       " --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs"

        Console.WriteLine(ID2Msg(10) & sLabel & _
                          ".bin | LUN: " & sLUN & _
                          " | Start: " & sStart & _
                          " | Sectors: " & sSectors & _
                          vbNewLine)

    End Function

    Protected Function ExecuteCommand( _
                              ByRef sCMDLine As String, _
                              Optional ByVal doSeparate As Boolean = False) As Boolean

        If doSeparate Then

            System.Diagnostics.Process.Start("fh_loader.exe", sCMDLine)

        Else

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
                Console.WriteLine(ID2Msg(11) & vbCrLf)
                Console.WriteLine(ID2Msg(12) & vbCrLf)
                Console.WriteLine(ID2Msg(13))
                Console.WriteLine(ID2Msg(14))
                Console.WriteLine(ID2Msg(15))
                Console.WriteLine(ID2Msg(16))
                Console.WriteLine(vbCrLf & ID2Msg(17))
                Console.ReadKey(True)
            Else : Return True
            End If

        End If

    End Function

End Class
