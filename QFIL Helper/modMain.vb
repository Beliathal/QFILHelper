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

    Public goSpeaker As clsMsg ' CMD arguments, errors, warnings, language related settings

    Sub Main(args As String())

        Console.Title = My.Application.Info.Title
        Console.CursorVisible = False

        If Debugger.IsAttached Then _
            Console.ForegroundColor = ConsoleColor.Red

        goSpeaker = New clsMsg

        If goSpeaker.ParseArguments(args) Then _
                If goSpeaker.gbHdnEnabled Then MainMenuFull() _
                Else MainMenuSimple()

        goSpeaker = Nothing
        args = Nothing

    End Sub

    Private Sub MainMenuSimple()

        Dim oHelper As New clsFlash
        Dim cCurKey As Char

        Do

            Select Case cCurKey

                Case ""
                    cCurKey = BuildMenu(False)

                Case "1"
                    Console.Clear()
                    oHelper.BackupPartitions()
                    cCurKey = ""

                Case "2"
                    Console.Clear()
                    oHelper.BackupLUNs()
                    cCurKey = ""

                Case "3"
                    Console.Clear()
                    oHelper.BackupBootParts()
                    cCurKey = ""

                Case "4"
                    Console.Clear()
                    oHelper.BackupIMEIParts()
                    cCurKey = ""

                Case "5"
                    Console.Clear()
                    oHelper.QueryCOMPorts()
                    cCurKey = ""

                Case "6"
                    Console.Clear()
                    oHelper.FlashFirmware()
                    cCurKey = ""

                Case Else
                    cCurKey = BuildMenu(True)

            End Select

        Loop Until cCurKey = "Q" OrElse cCurKey = "q"

        oHelper = Nothing

    End Sub

    Private Sub MainMenuFull()

        Dim oHelper As New clsFlash
        Dim cCurKey As Char

        Do

            Select Case cCurKey

                Case ""
                    cCurKey = BuildMenu(False)

                Case "1"
                    Console.Clear()
                    oHelper.BackupPartitions()
                    cCurKey = ""

                Case "2"
                    Console.Clear()
                    oHelper.BackupLUNs()
                    cCurKey = ""

                Case "3"
                    Console.Clear()
                    oHelper.FindHiddenParts()
                    cCurKey = ""

                Case "4"
                    Console.Clear()
                    oHelper.BackupHiddenLUNs()
                    cCurKey = ""

                Case "5"
                    Console.Clear()
                    oHelper.BackupBootParts()
                    cCurKey = ""

                Case "6"
                    Console.Clear()
                    oHelper.BackupIMEIParts()
                    cCurKey = ""

                Case "7"
                    Console.Clear()
                    oHelper.QueryCOMPorts()
                    cCurKey = ""

                Case "8"
                    Console.Clear()
                    oHelper.FlashFirmware()
                    cCurKey = ""

                Case Else
                    cCurKey = BuildMenu(True)

            End Select

        Loop Until cCurKey = "Q" OrElse cCurKey = "q"

        oHelper = Nothing

    End Sub

    Private Function BuildMenu(ByVal isError As Boolean) As Char

        Console.Clear()
        Console.WriteLine(goSpeaker.getTitleLine & vbCrLf)

        For iCnt As Byte = 1 To goSpeaker.getMenuCount
            'Console.WriteLine(goSpeaker.ID2Menu(iCnt) & vbCrLf)
            Console.WriteLine(goSpeaker.ID2Menu(iCnt) & IIf(goSpeaker.gbNarEnabled, "", vbCrLf))
        Next

        If isError Then Console.Write(goSpeaker.ID2Msg(25))
        BuildMenu = Console.ReadKey(True).KeyChar

    End Function

End Module
