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

    Public goUILang As clsMsg ' Parsing command line arguments, loading language related strings

    Sub Main(args As String())

        Console.Title = My.Application.Info.Title
        Console.CursorVisible = False

        If Debugger.IsAttached Then _
            Console.ForegroundColor = ConsoleColor.Red

        goUILang = New clsMsg

        If goUILang.ParseArguments(args) Then MainMenu()

        goUILang = Nothing
        args = Nothing

        End

    End Sub

    Private Sub MainMenu()

        Dim oPProcessor As New clsFlash
        Dim cCurKey As Char

        Do

            Select Case cCurKey

                Case ""

                    Console.Clear()
                    Console.WriteLine(goUILang.ID2Msg(1) & vbCrLf)
                    Console.WriteLine(goUILang.ID2Msg(2))
                    Console.WriteLine(goUILang.ID2Msg(3))
                    Console.WriteLine(goUILang.ID2Msg(4))
                    Console.WriteLine(goUILang.ID2Msg(5))
                    Console.WriteLine(goUILang.ID2Msg(25))
                    Console.WriteLine(goUILang.ID2Msg(35))
                    Console.WriteLine(goUILang.ID2Msg(26))
                    Console.WriteLine(goUILang.ID2Msg(36))
                    Console.WriteLine(goUILang.ID2Msg(6) & vbCrLf)

                    cCurKey = Console.ReadKey(True).KeyChar

                Case "1"
                    Console.Clear()
                    oPProcessor.BackupPartitions()
                    cCurKey = ""

                Case "2"
                    Console.Clear()
                    oPProcessor.BackupLUNs()
                    cCurKey = ""

                Case "3"
                    Console.Clear()
                    oPProcessor.FindHiddenParts()
                    cCurKey = ""

                Case "4"
                    Console.Clear()
                    oPProcessor.BackupHiddenLUNs()
                    cCurKey = ""

                Case "5"
                    Console.Clear()
                    oPProcessor.BackupBootParts()
                    cCurKey = ""

                Case "6"
                    Console.Clear()
                    oPProcessor.BackupIMEIParts()
                    cCurKey = ""

                Case "7"
                    Console.Clear()
                    oPProcessor.QueryCOMPorts()
                    cCurKey = ""

                Case "8"
                    Console.Clear()
                    oPProcessor.FlashFirmware()
                    cCurKey = ""

                Case "Q", "q"
                    'do nothing

                Case Else

                    Console.WriteLine(goUILang.ID2Msg(7))
                    cCurKey = Console.ReadKey(True).KeyChar

            End Select

        Loop Until cCurKey = "Q" OrElse cCurKey = "q"

        oPProcessor = Nothing

    End Sub

End Module
