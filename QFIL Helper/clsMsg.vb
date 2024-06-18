Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic
Imports System.Globalization

Public Class clsMsg
    Private Enum Language As Byte
        ZH_Lang = 0
        EN_Lang = 1
        RU_Lang = 2
        JA_Lang = 3
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
    '-advanced -NTFS -zh -utf8 - run in Chinese with advanced options

    Public Function ParseArguments(ByRef sCurLang() As String) As Boolean
        Dim lcid As Integer = CultureInfo.InstalledUICulture.LCID

        ' command line usage example for Chinese Simplified UTF8 with advanced settings:
        ' -advanced -NTFS -zh -utf8

        ' command line usage example for Russian UTF8 with advanced settings:
        ' -advanced -NTFS -ru -utf8

        ' command line usage example for Russian ASCII (codepage 1251) with advanced settings:
        ' -advanced -NTFS -ru

        ' command line usage example for Chinese Simplified ASCII (codepage 936) with advanced settings:
        ' -advanced -NTFS -zh

        ' NOTE: codepage 936 will work only after registering System.Text.Encoding.CodePages
        ' reference to: https://www.cnblogs.com/artech/p/encoding-registeration-4-net-core.html

        ' If no arguments provided, set default mode to english
        geCurLang = Language.EN_Lang

        For Each sCurArg As String In sCurLang

            Select Case sCurArg.ToLower

                ' 2024-01-06: -en option was missing, as result the app would run in 
                ' Chinese when no arguments supplied at the command line

                Case "-ru" : geCurLang = Language.RU_Lang
                Case "-zh" : geCurLang = Language.ZH_Lang
                Case "-ja" : geCurLang = Language.JA_Lang
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

        If isUTF8 Then
            Console.OutputEncoding = Encoding.UTF8
        Else

            Select Case geCurLang
                'Case Language.ZH_Lang : Console.OutputEncoding = Encoding.GetEncoding(936)
                Case Language.ZH_Lang : Console.OutputEncoding = Encoding.UTF8
                Case Language.JA_Lang : Console.OutputEncoding = Encoding.UTF8
                Case Language.RU_Lang : Console.OutputEncoding = Encoding.GetEncoding(1251)
            End Select

        End If

        LoadMessages()
        LoadMenus()
        Return True

    End Function

    Private Sub LoadMessages()

        Select Case geCurLang
            Case Language.ZH_Lang : gsaMsgData = My.Resources.msg_zh.Split(vbCrLf)
            Case Language.EN_Lang : gsaMsgData = My.Resources.msg_en.Split(vbCrLf)
            Case Language.RU_Lang : gsaMsgData = My.Resources.msg_ru.Split(vbCrLf)
            Case Language.JA_Lang : gsaMsgData = My.Resources.msg_ja.Split(vbCrLf)
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
            Case Language.JA_Lang : saTemp = My.Resources.menu_ja.Split("#")
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
        Dim oASCII As Encoding = Encoding.Default

        Select Case geCurLang
            Case Language.ZH_Lang : oASCII = Encoding.GetEncoding(936)
            Case Language.JA_Lang : oASCII = Encoding.GetEncoding(932)
            Case Language.RU_Lang : oASCII = Encoding.GetEncoding(1251)
        End Select

        Dim iaUnicode As Byte() = oUnicode.GetBytes(sCurText)
        Dim iaASCII As Byte() = Encoding.Convert(oUnicode, oASCII, iaUnicode)

        TextToANSI = oASCII.GetString(iaASCII)
        oUnicode = Nothing : oASCII = Nothing
        iaUnicode = Nothing : iaASCII = Nothing

    End Function

    Public Function ID2Msg(ByVal iCurID As Byte) As String

        Select Case geCurLang
            Case Language.ZH_Lang : Return TextToANSI(gsaMsgData(iCurID - 1))
            Case Language.EN_Lang : Return gsaMsgData(iCurID - 1)
            Case Language.RU_Lang : Return TextToANSI(gsaMsgData(iCurID - 1))
            Case Language.JA_Lang : Return TextToANSI(gsaMsgData(iCurID - 1))
        End Select

    End Function

    Public Function ID2Menu(ByVal iCurMenu As Byte, ByVal iCurID As Byte) As String

        Dim saTemp() = glsaMnuData.Item(iCurMenu)

        Select Case geCurLang
            Case Language.ZH_Lang : Return saTemp(iCurID - 1).Replace("@", iCurID)
            Case Language.EN_Lang : Return saTemp(iCurID - 1).Replace("@", iCurID)
            Case Language.RU_Lang : Return TextToANSI(saTemp(iCurID - 1)).Replace("@", iCurID)
            Case Language.JA_Lang : Return saTemp(iCurID - 1).Replace("@", iCurID)
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
