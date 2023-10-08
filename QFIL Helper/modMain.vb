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
    Private oHelper As New clsDebug

    ' Possible args: -advanced -hidden -utf8 -ru -narrow -red

    Sub Main(args As String())

        Console.Title = My.Application.Info.Title
        Console.CursorVisible = False

        If Debugger.IsAttached Then _
            Console.ForegroundColor = ConsoleColor.Red

        goSpeaker = New clsMsg

        If goSpeaker.ParseArguments(args) Then _
                If goSpeaker.gbAdvEnabled Then MainMenuFull() _
                Else MainMenuSimple()

        goSpeaker = Nothing
        args = Nothing

    End Sub

    Private Sub MainMenuSimple()

        Dim cCurKey As Char

        Do

            Select Case cCurKey

                Case ""
                    cCurKey = BuildMenu(0, False)

                Case "1"
                    Console.Clear()
                    oHelper.BackupLUNs()
                    cCurKey = ""

                Case "2"
                    Console.Clear()
                    oHelper.BackupPartitions()
                    cCurKey = ""

                Case "3"
                    Console.Clear()
                    oHelper.BackupUserData()
                    cCurKey = ""

                Case "4"
                    Console.Clear()
                    oHelper.QueryCOMPorts()
                    cCurKey = ""

                Case "5"
                    Console.Clear()
                    oHelper.FlashFirmware()
                    cCurKey = ""

                Case Else
                    cCurKey = BuildMenu(0, True)

            End Select

        Loop Until cCurKey = "Q" OrElse cCurKey = "q"

        oHelper = Nothing

    End Sub

    Private Sub MainMenuFull()

        Dim cCurKey As Char

        Do

            Select Case cCurKey

                Case ""
                    cCurKey = BuildMenu(0, False)

                Case "1"
                    Console.Clear()
                    oHelper.BackupLUNs()
                    cCurKey = ""

                Case "2"
                    Console.Clear()
                    oHelper.BackupPartitions()
                    cCurKey = ""

                Case "3"
                    Console.Clear()
                    QuickMenuLUNs()
                    cCurKey = ""

                Case "4"
                    Console.Clear()
                    QuickMenuPartitons()
                    cCurKey = ""

                Case "5"
                    Console.Clear()           
                    oHelper.BackupUserData()
                    cCurKey = ""

                Case "6"
                    Console.Clear()
                    QuickMenuErase()
                    cCurKey = ""

                Case "7"
                    Console.Clear()
                    oHelper.QueryCOMPorts()
                    cCurKey = ""

                Case "8"
                    Console.Clear()
                    oHelper.FlashFirmware()
                    cCurKey = ""

                Case "d"
                    Console.Clear()
                    oHelper.DebugerTester()
                    cCurKey = ""

                Case Else
                    cCurKey = BuildMenu(0, True)

            End Select

        Loop Until cCurKey = "Q" OrElse cCurKey = "q"

        oHelper = Nothing

    End Sub

    Private Sub QuickMenuPartitons()

        Dim cCurKey As Char

        Do

            Select Case cCurKey

                Case ""
                    cCurKey = BuildMenu(2, False)

                Case "1"
                    Console.Clear()
                    oHelper.SPBackup(clsLUNs.OPCode.ABL)
                    cCurKey = ""

                Case "2"
                    Console.Clear()
                    oHelper.SPBackup(clsLUNs.OPCode.FTM)
                    cCurKey = ""

                Case "3"
                    Console.Clear()
                    oHelper.SPBackup(clsLUNs.OPCode.SYSA)
                    cCurKey = ""

                Case "4"
                    Console.Clear()
                    oHelper.SPBackup(clsLUNs.OPCode.SYSB)
                    cCurKey = ""

                Case "5"
                    Console.Clear()
                    oHelper.DumpGTP()
                    cCurKey = ""

                Case "6"
                    Console.Clear()
                    oHelper.ManualBackup()
                    cCurKey = ""

                Case Else
                    cCurKey = BuildMenu(2, True)

            End Select

        Loop Until cCurKey = "Q" OrElse cCurKey = "q"

    End Sub

    Private Sub QuickMenuLUNs()

        Dim cCurKey As Char

        Do

            Select Case cCurKey

                Case ""
                    cCurKey = BuildMenu(3, False)

                Case "1"
                    Console.Clear()
                    oHelper.SLBackup(clsLUNs.OPCode.LUN16)
                    cCurKey = ""

                Case "2"
                    Console.Clear()
                    oHelper.BackupSelLUNs({"lun0"})
                    cCurKey = ""

                Case "3"
                    Console.Clear()
                    oHelper.BackupSelLUNs({"lun4"})
                    cCurKey = ""

                Case "4"
                    Console.Clear()
                    oHelper.ManualBackup()
                    cCurKey = ""

                Case Else
                    cCurKey = BuildMenu(3, True)

            End Select

        Loop Until cCurKey = "Q" OrElse cCurKey = "q"

    End Sub

    Private Sub QuickMenuErase()

        Dim cCurKey As Char

        Do

            Select Case cCurKey

                Case ""
                    cCurKey = BuildMenu(4, False)

                Case "1"
                    Console.Clear()
                    oHelper.SPErase(clsLUNs.OPCode.SID)
                    cCurKey = ""

                Case "2"
                    Console.Clear()
                    oHelper.ManualErase()
                    cCurKey = ""

                Case Else
                    cCurKey = BuildMenu(4, True)

            End Select

        Loop Until cCurKey = "Q" OrElse cCurKey = "q"

    End Sub

    Private Sub SelectiveMenu()

        Dim cCurKey As Char

        Do

            Select Case cCurKey

                Case ""
                    cCurKey = BuildMenu(2, False)

                Case "1"
                    Console.Clear()
                    oHelper.BackupSelPartitions(oHelper.LoadQuickList(1))
                    cCurKey = ""

                Case "2"
                    Console.Clear()
                    oHelper.BackupSelPartitions(oHelper.LoadQuickList(0))
                    cCurKey = ""

                Case "3"
                    Console.Clear()
                    oHelper.BackupSelPartitions(oHelper.LoadQuickList(2))
                    cCurKey = ""

                Case "4"
                    Console.Clear()
                    oHelper.BackupSelPartitions(oHelper.LoadQuickList(3))
                    cCurKey = ""

                Case "5"
                    Console.Clear()
                    oHelper.BackupSelLUNs(oHelper.LoadQuickList(5))
                    cCurKey = ""

                Case "6"
                    Console.Clear()
                    oHelper.BackupSelLUNs({"lun0"})
                    cCurKey = ""

                Case "7"
                    Console.Clear()
                    oHelper.BackupSelLUNs({"lun4"})
                    cCurKey = ""

                Case "8"
                    Console.Clear()
                    cCurKey = ""

                Case "9"
                    Console.Clear()
                    cCurKey = ""

                Case Else
                    cCurKey = BuildMenu(2, True)

            End Select

        Loop Until cCurKey = "Q" OrElse cCurKey = "q"

    End Sub

    Private Function BuildMenu(ByVal iMenu As Byte, ByVal isError As Boolean) As Char

        Console.Clear()
        Console.WriteLine(goSpeaker.getTitleLine & vbCrLf)

        For iCnt As Byte = 1 To goSpeaker.getMenuCount(iMenu)

            Console.WriteLine( _
                goSpeaker.ID2Menu(iMenu, iCnt) & IIf( _
                    goSpeaker.gbNarEnabled, "", vbCrLf))

        Next

        If isError Then Console.Write(goSpeaker.ID2Msg(25))
        BuildMenu = Console.ReadKey(True).KeyChar

    End Function

End Module
