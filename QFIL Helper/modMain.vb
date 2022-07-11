Imports System.IO
Imports Microsoft.VisualBasic

'QFIL Helper - Partition/LUN backup automation utility for LG v50/G8
' Copyright (C) 2022  Beliathal

'    This program is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.

'    This program is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.

'    You should have received a copy of the GNU General Public License
'    along with this program.  If not, see <https://www.gnu.org/licenses/>.

Module modMain

    Sub Main(args As String())

        Console.Title = My.Application.Info.Title
        Console.ForegroundColor = ConsoleColor.Red
        Console.OutputEncoding = System.Text.Encoding.UTF8



        Dim oPProcessor As New clsLUNs
        Dim cCurKey As Char

        If Not oPProcessor.ValidateLanguage(args) OrElse _
            Not oPProcessor.ValidateFiles() Then
            oPProcessor = Nothing
            args = Nothing
            Exit Sub
        End If

        Do

            Select Case cCurKey

                Case ""

                    Console.Clear()
                    Console.WriteLine(oPProcessor.ID2Msg(1) & vbCrLf)
                    Console.WriteLine(oPProcessor.ID2Msg(2))
                    Console.WriteLine(oPProcessor.ID2Msg(3))
                    Console.WriteLine(oPProcessor.ID2Msg(4))
                    Console.WriteLine(oPProcessor.ID2Msg(5))
                    Console.WriteLine(oPProcessor.ID2Msg(6) & vbCrLf)

                    cCurKey = Console.ReadKey(True).KeyChar

                Case "1"
                    Console.Clear()
                    oPProcessor.BackupPartitions()
                    cCurKey = ""

                Case "2"
                    Console.Clear()
                    oPProcessor.FindHiddenParts()
                    cCurKey = ""

                Case "3"
                    Console.Clear()
                    oPProcessor.BackupLUNs()
                    cCurKey = ""

                Case "4"
                    Console.Clear()
                    oPProcessor.BackupHiddenLUNs()
                    cCurKey = ""

                Case "Q", "q"
                    'do nothing

                Case Else

                    Console.WriteLine(oPProcessor.ID2Msg(7))
                    cCurKey = Console.ReadKey(True).KeyChar

            End Select

        Loop Until cCurKey = "Q" OrElse cCurKey = "q"

        oPProcessor = Nothing

    End Sub

End Module
