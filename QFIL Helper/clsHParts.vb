Imports System.IO
Imports Microsoft.VisualBasic

Public Class clsHParts : Inherits clsVParts

    ' If there is a gap between previous partition and current then:
    ' Previous Partitions Start Sector + Sectors < Current Partition Start Sector

    Public Sub FindHiddenParts()

        Dim ioLogFile As StreamWriter
        Dim ioSourceFile As StreamReader
        Dim sBuffer As String
        Dim sCMDLine As String

        Dim iLastLUN As Nullable(Of Byte)
        Dim iLastSector As UInt32
        Dim iHidCnt As UInt32

        ioSourceFile = File.OpenText(sFileName)
        ioLogFile = File.CreateText(DirName & "hidden_partitions.log")

        While (Not ioSourceFile.EndOfStream)

            sBuffer = ioSourceFile.ReadLine()

            If sBuffer.StartsWith("  <partition label") Then
                If ParseXML(sBuffer) Then

                    ' Conditions to check:
                    ' 1. Skip checking 1st 6 sectors of every partition, those are GPT tables IMHO
                    ' 2. LUN:0, Partiton:0, Sector:?
                    ' 3. Current LUN > Previous LUN ?: Reached partition boundaries

                    If Not iLastLUN.HasValue OrElse iLUN > iLastLUN Then

                        iLastLUN = iLUN
                        iLastSector = iSize
                        Continue While

                    End If

                    If iStart > iLastSector Then

                        ' Hidden Start = Previous Start + Sectors [iLastSector]
                        ' Hidden Sectors = New Start - (Previous Start + Previous Sectors)

                        Label(iHidCnt) = "hidden_partition_"
                        Sectors = iStart - iLastSector
                        Start = iLastSector
                        sCMDLine = BuildCommand()
                        iHidCnt += 1

                        If Not ExecuteCommand(sCMDLine) Then Exit While
                        WriteLog(ioLogFile)

                    End If

                    iLastSector = iSize

                End If
            End If

        End While

        ioSourceFile.Close() : ioSourceFile.Dispose() : ioSourceFile = Nothing
        ioLogFile.Close() : ioLogFile.Dispose() : ioLogFile = Nothing

    End Sub

    Private Sub WriteLog(ByRef ioLogFile As StreamWriter)

        ioLogFile.WriteLine(sLabel & _
                            " LUN: " & sLUN & _
                            " Start: " & sStart & _
                            " Sectors: " & sSectors)

    End Sub

End Class
