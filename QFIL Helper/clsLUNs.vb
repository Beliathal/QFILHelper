Imports System.IO
Imports Microsoft.VisualBasic

Public Class clsLUNs : Inherits clsHParts

    Public Sub BackupLUNs()

        Dim ioLogFile As StreamWriter
        Dim saBuffer() As String
        Dim sCMDLine As String
        Dim iIndex As Integer

        CreateBackupFolder()
        ioLogFile = File.CreateText(DirName & "lun_backup.log")
        saBuffer = File.ReadAllLines(sFileName)
        sLabel = "LUN Backup"

        ' Skipping LUN 0 becasue it has userdata at the end
        ' Skipping LUN 3 because it's hidden
        ' If iIndex (same as iIndex = -1, implicit convertion of neg num to boolean = true)

        For iCnt As Byte = 0 To 5

            Select Case iCnt

                Case 0

                    iIndex = LocateUserdata(saBuffer)

                    If iIndex = -1 OrElse Not _
                        ParseXML(saBuffer(iIndex)) Then Continue For

                    sLUN = "0" : sSectors = iStart

                Case 1, 2, 4, 5

                    iIndex = LocateLUN(saBuffer, iCnt)

                    If iIndex = -1 OrElse Not _
                        ParseXML(saBuffer(iIndex)) Then Continue For

                    sLUN = iCnt.ToString
                    sSectors = iSize.ToString

                    'Case 3, 6 : SetHiddenLUN(iCnt)

            End Select

            sCMDLine = BuildCommand()

            If Not ExecuteCommand(sCMDLine) Then Exit For

            WriteLog(ioLogFile)

        Next

        ioLogFile.Close()
        ioLogFile.Dispose()
        ioLogFile = Nothing
        saBuffer = Nothing
        CleanUpBackupFolder()

    End Sub

    ' There's no way to know the exact number of sectors contained within them hidden LUNs, 
    ' as their sizes can change depending on the phone model, OS version & build number.

    ' This is why I've decided to let the user enter this info manualy, but I still can
    ' help by suggesting a possible number based on this guide:
    ' https://forum.xda-developers.com/t/tutorial-full-flash-backup-and-restore.4362809/

    Public Sub BackupHiddenLUNs()

        Dim sCurInput As String = String.Empty
        Dim iCurStep As SByte
        Dim iCurLUN As Byte
        Dim iCurSectors As UInt32

        CreateBackupFolder()

        Do

            Select Case iCurStep

                Case 0

                    Console.WriteLine(ID2Msg(22))
                    sCurInput = Console.ReadLine

                    If Not Byte.TryParse(sCurInput, iCurLUN) _
                        Then iCurStep = -2 Else iCurStep = 1

                Case 1

                    Console.WriteLine(vbCrLf & ID2Msg(23).Replace("@", SuggestSectors(iCurLUN)))
                    sCurInput = Console.ReadLine

                    If Not UInt32.TryParse(sCurInput, iCurSectors) _
                        Then iCurStep = -1 Else iCurStep = 2

                Case 2

                    LUN = iCurLUN
                    Sectors = iCurSectors
                    ExecuteCommand(BuildCommand())
                    iCurStep = 0

                Case -2 To -1

                    Console.WriteLine(ID2Msg(7) & vbCrLf)
                    iCurStep += 2

            End Select

        Loop Until sCurInput.ToLower = "q"

        CleanUpBackupFolder()

    End Sub

    Private Overloads Function ParseXML(ByRef sBuffer As String) As Boolean

        Dim iNumIndex As UInt16
        Dim iBegIndex As UInt16

        iBegIndex = sBuffer.IndexOf("start_sector")
        iNumIndex = sBuffer.IndexOf("num_partition")

        ResetInfo()

        If iBegIndex > -1 AndAlso iNumIndex > -1 Then

            iBegIndex += "start_sector=""".Length
            iNumIndex += "num_partition_sectors=""".Length

            Do

                sStart &= sBuffer(iBegIndex).ToString
                iBegIndex += 1

            Loop While IsNumeric(sBuffer(iBegIndex))

            Do

                sSectors &= sBuffer(iNumIndex).ToString
                iNumIndex += 1

            Loop While IsNumeric(sBuffer(iNumIndex))

            iStart = UInt32.Parse(sStart)
            iSectors = UInt32.Parse(sSectors)

        Else

            Console.WriteLine(ID2Msg(8) & sBuffer)
            Console.ReadKey()
            Return False

        End If

        Return True


    End Function

    Private Function LocateLUN( _
                              ByRef sBuffer() As String, _
                              ByVal iCnt As Byte) As Integer

        LocateLUN = Array.FindIndex(sBuffer, _
        Function(x As String) (x.Contains("""last_parti"" physical_partition_number=""" & iCnt)))

        If LocateLUN = -1 Then
            Console.WriteLine(ID2Msg(0) & iCnt)
            Console.ReadKey(True)
        End If

    End Function

    Private Function LocateUserdata(ByRef sBuffer() As String) As Integer

        LocateUserdata = Array.FindIndex(sBuffer, _
        Function(x As String) (x.Contains("""userdata"" physical_partition_number=""0")))

        If LocateUserdata = -1 Then
            Console.WriteLine(ID2Msg(0) & "0")
            Console.ReadKey(True)
        End If

    End Function

    Private Shadows Function BuildCommand() As String

        ' fh_loader.exe --port=\\.\COM --convertprogram2read --sendimage=LUN1_Complete.bin --start_sector=0 
        '--lun=1 --num_sectors=2048 --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs

        Dim sCurLabel As String = DirName & "lun" & sLUN & "_Complete.bin"

        BuildCommand = "--port=\\.\COM" & sCOMPort & _
                       " --convertprogram2read --sendimage=" & sCurLabel & _
                       " --start_sector=0" & _
                       " --lun=" & sLUN & _
                       " --num_sectors=" & sSectors & _
                       " --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs"

        Console.WriteLine(ID2Msg(24) & sLUN & "_Complete.bin" & _
                          " | Sectors: " & sSectors & vbNewLine)

    End Function

    Private Sub WriteLog(ByRef ioLogFile As StreamWriter)

        ioLogFile.WriteLine("LUN: " & sLUN & " Sectors: " & sSectors)

    End Sub

End Class
