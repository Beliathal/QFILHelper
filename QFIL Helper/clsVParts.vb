Imports System.IO
Imports Microsoft.VisualBasic

Public Class clsVParts : Inherits clsInfo

    Public Sub BackupPartitions()

        Dim ioSourceFile As StreamReader
        Dim sBuffer As String
        Dim sCMDLine As String

        ioSourceFile = File.OpenText(sFileName)

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

    Public Function ValidateFiles() As Boolean

        ' Get all files in current folder

        Dim saFileList() As String = _
            Directory.GetFiles(Directory.GetCurrentDirectory)

        Dim isPortNumber As Boolean
        Dim isPartitionList As Boolean
        Dim isFHLoader As Boolean

        For iCnt As UInt16 = 0 To saFileList.Length - 1

            If saFileList(iCnt).IndexOf("_PartitionsList.xml") > -1 And _
               saFileList(iCnt).IndexOf("COM") > -1 Then

                isPartitionList = True
                sFileName = Path.GetFileName(saFileList(iCnt))
                isPortNumber = ParseCOMPortNumber()

            ElseIf saFileList(iCnt).IndexOf("fh_loader.exe") > -1 Then
                isFHLoader = True
            End If

            ' Found both the fh_loader.exe and the PartitionsList.xml
            If isFHLoader And isPortNumber Then Exit For

        Next

        If Not isFHLoader Then
            Console.WriteLine(ID2Msg(18))
            Console.WriteLine(ID2Msg(21))
            Console.ReadKey()
            Return False

        ElseIf Not isPartitionList Then
            Console.WriteLine(ID2Msg(19))
            Console.WriteLine(ID2Msg(21))
            Console.ReadKey()
            Return False

        ElseIf Not isPortNumber Then
            Console.WriteLine(ID2Msg(20))
            Console.WriteLine(ID2Msg(21))
            Console.ReadKey()
            Return False

        End If

        Directory.CreateDirectory(sDirName)
        FileSystem.FileCopy(sFileName, DirName & sFileName)
        Return True

    End Function

    Private Function ParseCOMPortNumber() As Boolean

        ' File Name Example: COM13_PartitionsList.xml
        ' Start immidiately after COM and continue until _ sign
        ' Everything in-between should be numbers

        Dim iPortNumber As UInt16

        For iCnt As UInt16 = _
            sFileName.IndexOf("COM") + 3 To sFileName.Length - 1

            If UInt16.TryParse(sFileName(iCnt), iPortNumber) Then
                sCOMPort &= sFileName(iCnt)
                ParseCOMPortNumber = True

            ElseIf sFileName(iCnt) = "_" Then
                Exit For
            End If

        Next

    End Function

End Class
