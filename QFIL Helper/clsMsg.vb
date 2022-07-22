Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic

Public Class clsMsg
    Private Enum Language As Byte
        EN_Lang = 0
        RU_Lang = 1
    End Enum

    Private gsaMsgData() As String
    Private gsaMnuData() As String
    Private geCurLang As Language
    Private isUTF8 As Boolean

    Public gbAdvEnabled As Boolean ' Enable advanced options
    Public gbHdnEnabled As Boolean ' Enable menu entries for hidden partitions & luns
    Public gbNarEnabled As Boolean ' Enable narrow menus

    ' Run programm with arguments:
    '-ru - sets language to Russian ANSI
    '-ru -utf8 - sets language to Russian UTF-8
    '-hidden - enables hidden partitions and hidden LUN backup functions
    '-advanced - enables flashing of entire LUN

    Public Function ParseArguments(ByRef sCurLang() As String) As Boolean

        If sCurLang.Length = 0 Then

            geCurLang = Language.EN_Lang
            LoadMessages()
            LoadMenus()
            Return True

        End If

        For Each sCurArg As String In sCurLang

            Select Case sCurArg

                Case "-ru" : geCurLang = Language.RU_Lang
                Case "-utf8" : isUTF8 = True
                Case "-advanced" : gbAdvEnabled = True
                Case "-hidden" : gbHdnEnabled = True
                Case "-narrow" : gbNarEnabled = True
                Case "-red" : Console.ForegroundColor = ConsoleColor.Red

                Case Else

                    Console.WriteLine("Ivalid arguments")
                    Console.ReadKey(True)
                    Return False

            End Select

        Next

        If geCurLang = Language.RU_Lang And isUTF8 Then _
             Console.OutputEncoding = Encoding.UTF8 _
        Else Console.OutputEncoding = Encoding.GetEncoding(1251)

        LoadMessages()
        LoadMenus()
        Return True

    End Function

    Private Sub LoadMessages()

        Select Case geCurLang
            Case Language.EN_Lang : gsaMsgData = My.Resources.msg_en.Split(vbCrLf)
            Case Language.RU_Lang : gsaMsgData = My.Resources.msg_ru.Split(vbCrLf)
        End Select

    End Sub

    Private Sub LoadMenus()

        Dim saTemp() As String

        Select Case geCurLang
            Case Language.EN_Lang : saTemp = My.Resources.menu_en.Split("#")
            Case Language.RU_Lang : saTemp = My.Resources.menu_ru.Split("#")
        End Select

        Select Case gbHdnEnabled
            Case True
                gsaMnuData = saTemp(0).Split(vbCrLf.ToCharArray, _
                                             StringSplitOptions.RemoveEmptyEntries)
            Case False
                gsaMnuData = saTemp(1).Split(vbCrLf.ToCharArray, _
                                             StringSplitOptions.RemoveEmptyEntries)
        End Select

        saTemp = Nothing

    End Sub

    Private Function TextToANSI(ByRef sCurText As String) As String

        If isUTF8 Then Return sCurText

        Dim oUnicode As Encoding = Encoding.Unicode
        Dim oASCII As Encoding = Encoding.GetEncoding(1251)

        Dim iaUnicode As Byte() = oUnicode.GetBytes(sCurText)
        Dim iaASCII As Byte() = Encoding.Convert(oUnicode, oASCII, iaUnicode)

        TextToANSI = oASCII.GetString(iaASCII)
        oUnicode = Nothing : oASCII = Nothing
        iaUnicode = Nothing : iaASCII = Nothing

    End Function

    Public Function ID2Msg(ByVal iCurID As Byte) As String

        Select Case geCurLang
            Case Language.EN_Lang : Return gsaMsgData(iCurID - 1)
            Case Language.RU_Lang : Return TextToANSI(gsaMsgData(iCurID - 1))
        End Select

    End Function

    Public Function ID2Menu(ByVal iCurID As Byte) As String

        Select Case geCurLang
            Case Language.EN_Lang : Return gsaMnuData(iCurID - 1).Replace("@", iCurID)
            Case Language.RU_Lang : Return TextToANSI(gsaMnuData(iCurID - 1)).Replace("@", iCurID)
        End Select

    End Function

    Public ReadOnly Property getMenuCount() As Byte
        Get
            Return gsaMnuData.Count
        End Get
    End Property

    Public ReadOnly Property getTitleLine() As String

        Get
            Select Case gbAdvEnabled
                Case True : getTitleLine = ID2Msg(2).Replace("@", My.Application.Info.Version.ToString)
                Case False : getTitleLine = ID2Msg(3).Replace("@", My.Application.Info.Version.ToString)
            End Select
        End Get

    End Property

End Class
