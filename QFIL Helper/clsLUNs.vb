Imports System.IO
Imports Microsoft.VisualBasic

' Purpouse: Backup LUNs

Public Class clsLUNs : Inherits clsHParts

    Public Sub BackupLUNs()

        If Not ValidateFiles() Then Exit Sub

        Dim saBuffer() As String
        Dim sCMDLine As String
        Dim iIndex As Integer

        ResetInfo()
        CreateBackupFolder()

        saBuffer = File.ReadAllLines(gsFileName)
        gsLabel = "LUN Backup"

        ' Skipping LUN 0 becasue it has userdata at the end
        ' Skipping LUN 3 because it's hidden
        ' If iIndex (same as iIndex = -1, implicit convertion of neg num to boolean = true)

        For iCnt As Byte = 0 To 5

            Select Case iCnt

                Case 0

                    iIndex = LocateUserdata(saBuffer)

                    If iIndex = -1 OrElse Not _
                        ParseXML(saBuffer(iIndex)) Then Continue For

                    gsLUN = "0" : gsSectors = giStart

                Case 1, 2, 4, 5

                    iIndex = LocateLUN(saBuffer, iCnt)

                    If iIndex = -1 OrElse Not _
                        ParseXML(saBuffer(iIndex)) Then Continue For

                    gsLUN = iCnt.ToString
                    gsSectors = getSize.ToString

                    'Case 3, 6 : SetHiddenLUN(iCnt)

            End Select

            sCMDLine = BuildCommand()

            If Not ExecuteCommand(sCMDLine) Then Exit For

        Next

        saBuffer = Nothing

        CleanUpBackupFolder()
        ProcessCompletedMsg()

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

                gsStart &= sBuffer(iBegIndex).ToString
                iBegIndex += 1

            Loop While IsNumeric(sBuffer(iBegIndex))

            Do

                gsSectors &= sBuffer(iNumIndex).ToString
                iNumIndex += 1

            Loop While IsNumeric(sBuffer(iNumIndex))

            giStart = UInt32.Parse(gsStart)
            giSectors = UInt32.Parse(gsSectors)

        Else

            Console.WriteLine(goSpeaker.ID2Msg(26) & sBuffer)
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
            Console.WriteLine(goSpeaker.ID2Msg(35) & iCnt)
            Console.ReadKey(True)
        End If

    End Function

    Private Function LocateUserdata(ByRef sBuffer() As String) As Integer

        LocateUserdata = Array.FindIndex(sBuffer, _
        Function(x As String) (x.Contains("""userdata"" physical_partition_number=""0")))

        If LocateUserdata = -1 Then
            Console.WriteLine(goSpeaker.ID2Msg(35) & "0")
            Console.ReadKey(True)
        End If

    End Function

    Private Shadows Function BuildCommand() As String

        ' fh_loader.exe --port=\\.\COM --convertprogram2read --sendimage=lun1_complete.bin --start_sector=0 
        '--lun=1 --num_sectors=2048 --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs

        Dim sCurLabel As String = getDirWSlash & "lun" & gsLUN & "_complete.bin"

        BuildCommand = "--port=\\.\" & gsCOMPort & _
                       " --convertprogram2read --sendimage=" & sCurLabel & _
                       " --start_sector=0" & _
                       " --lun=" & gsLUN & _
                       " --num_sectors=" & gsSectors & _
                       " --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=ufs"

        Console.WriteLine(goSpeaker.ID2Msg(24) & gsLUN & "_complete.bin" & _
                          " | Sectors: " & gsSectors & vbNewLine)

    End Function

End Class
