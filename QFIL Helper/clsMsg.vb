Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic
Imports System.Globalization

Public Class clsMsg
    Private Enum Language As Byte
        ZH_Lang = 0
        EN_Lang = 1
        RU_Lang = 2
    End Enum

    Private gsaMsgData() As String
    Private glsaMnuData As List(Of String())
    Private geCurLang As Language
    Private isUTF8 As Boolean

    Public gbAdvEnabled As Boolean ' Enable advanced options
    Public gbNarEnabled As Boolean ' Enable narrow menus
    Public gbDoCompress As Boolean
    Public gbDoDebug As Boolean

    ' Run programm with arguments:
    '-ru - sets language to Russian ANSI
    '-ru -utf8 - sets language to Russian UTF-8
    '-advanced - enables flashing of entire LUN

    Public Function ParseArguments(ByRef sCurLang() As String) As Boolean
        Dim lcid As Integer = CultureInfo.InstalledUICulture.LCID


        If lcid = 2052 Then
            geCurLang = Language.ZH_Lang   '中文简体
            Console.OutputEncoding = Encoding.UTF8
        ElseIf lcid = 1049 Then
            geCurLang = Language.RU_Lang   '俄语
            Console.OutputEncoding = Encoding.UTF8
        Else : geCurLang = Language.EN_Lang
            Console.OutputEncoding = Encoding.GetEncoding(1251)
        End If
        Debug.WriteLine(“Language code：” & lcid)



        For Each sCurArg As String In sCurLang

            Select Case sCurArg.ToLower

                Case "-ru" : geCurLang = Language.RU_Lang
                Case "-utf8" : isUTF8 = True
                Case "-advanced" : gbAdvEnabled = True
                Case "-narrow" : gbNarEnabled = True
                Case "-debug" : gbDoDebug = True
                Case "-ntfs" : gbDoCompress = True
                Case "-red" : Console.ForegroundColor = ConsoleColor.Red

                Case Else

                    Console.WriteLine("Ivalid arguments")
                    Console.ReadKey(True)
                    Return False

            End Select

        Next

        LoadMessages()
        LoadMenus()
        Return True

    End Function

    Private Sub LoadMessages()

        Select Case geCurLang
            Case Language.ZH_Lang : gsaMsgData = My.Resources.msg_zh.Split(vbCrLf)
            Case Language.EN_Lang : gsaMsgData = My.Resources.msg_en.Split(vbCrLf)
            Case Language.RU_Lang : gsaMsgData = My.Resources.msg_ru.Split(vbCrLf)
        End Select

    End Sub

    Private Sub LoadMenus()

        Dim saTemp() As String
        glsaMnuData = New List(Of String())

        ' Split menu entires by "#" (1: advanced menu, 2: simple menu)
        ' Split by carriage return and remove empty lines (aka double carriage issue)

        Select Case geCurLang
            Case Language.ZH_Lang : saTemp = My.Resources.menu_zh.Split("#")
            Case Language.EN_Lang : saTemp = My.Resources.menu_en.Split("#")
            Case Language.RU_Lang : saTemp = My.Resources.menu_ru.Split("#")
        End Select

        Select Case gbAdvEnabled

            Case True
                glsaMnuData.Add( _
                    saTemp(0).Split(vbCrLf.ToCharArray, _
                                    StringSplitOptions.RemoveEmptyEntries))
            Case False
                glsaMnuData.Add( _
                    saTemp(1).Split(vbCrLf.ToCharArray, _
                                    StringSplitOptions.RemoveEmptyEntries))
        End Select

        ' sub menus

        For iCnt As Byte = 1 To saTemp.Count - 1

            glsaMnuData.Add( _
                saTemp(iCnt).Split( _
                    vbCrLf.ToCharArray, _
                    StringSplitOptions.RemoveEmptyEntries))

        Next

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
            Case Language.ZH_Lang : Return gsaMsgData(iCurID - 1)
            Case Language.EN_Lang : Return gsaMsgData(iCurID - 1)
            Case Language.RU_Lang : Return TextToANSI(gsaMsgData(iCurID - 1))
        End Select

    End Function

    Public Function ID2Menu(ByVal iCurMenu As Byte, ByVal iCurID As Byte) As String

        Dim saTemp() = glsaMnuData.Item(iCurMenu)

        Select Case geCurLang
            Case Language.ZH_Lang : Return saTemp(iCurID - 1).Replace("@", iCurID)
            Case Language.EN_Lang : Return saTemp(iCurID - 1).Replace("@", iCurID)
            Case Language.RU_Lang : Return TextToANSI(saTemp(iCurID - 1)).Replace("@", iCurID)
        End Select

    End Function

    Public ReadOnly Property getMenuCount(ByVal iCurMenu As Byte) As Byte
        Get
            Return glsaMnuData.Item(iCurMenu).Count
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

    Protected Overrides Sub Finalize()

        glsaMnuData = Nothing
        Erase gsaMsgData
        MyBase.Finalize()

    End Sub

End Class
