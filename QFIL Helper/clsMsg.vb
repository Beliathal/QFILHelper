Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic

Public Class clsMsg
    Private Enum Language As Byte
        EN_Lang = 0
        RU_UTF8 = 1
        RU_ANSI = 2
    End Enum

    Private gsaUIMsg() As String
    Private geCurLang As Language

    ' Set to true to test russian lang
    Private isRULangTest As Boolean = False

    Public Function ParseArguments(ByRef sCurLang() As String) As Boolean

        If sCurLang.Length = 0 Then

            geCurLang = Language.EN_Lang
            LoadMessages()
            Return True

        ElseIf sCurLang(0).ToLower = "-ru" Then

            If sCurLang.Length = 1 Then

                geCurLang = Language.RU_ANSI
                LoadMessages()
                Return True

            ElseIf sCurLang(1).ToLower = "-utf8" Then

                Console.OutputEncoding = Encoding.UTF8
                geCurLang = Language.RU_UTF8
                LoadMessages()
                Return True

            Else

                Console.WriteLine("Ivalid arguments")
                Console.ReadKey(True)
                Return False

            End If

        Else

            Console.WriteLine("Ivalid arguments")
            Console.ReadKey(True)
            Return False

        End If

    End Function

    Private Sub LoadMessages()

        If isRULangTest Then
            geCurLang = Language.RU_UTF8
            Console.OutputEncoding = Encoding.UTF8
        End If

        Select Case geCurLang

            Case Language.EN_Lang
                gsaUIMsg = My.Resources.english.Split(vbCrLf)

            Case Language.RU_UTF8, Language.RU_ANSI
                gsaUIMsg = My.Resources.russian.Split(vbCrLf)

        End Select

    End Sub

    Private Function ConvertToANSI(ByVal iCurID As Byte) As String

        Dim oUnicode As Encoding = Encoding.Unicode
        Dim oASCII As Encoding = Encoding.GetEncoding(1251)

        Dim iaUnicode As Byte() = oUnicode.GetBytes(gsaUIMsg(iCurID))
        Dim iaASCII As Byte() = Encoding.Convert(oUnicode, oASCII, iaUnicode)

        ConvertToANSI = oASCII.GetString(iaASCII)
        oUnicode = Nothing : oASCII = Nothing
        iaUnicode = Nothing : iaASCII = Nothing

    End Function

    Public ReadOnly Property ID2Msg(ByVal iCurID As Byte) As String

        Get
            Select Case geCurLang
                Case Language.EN_Lang : Return gsaUIMsg(iCurID - 1)
                Case Language.RU_UTF8 : Return gsaUIMsg(iCurID - 1)
                Case Language.RU_ANSI : Return ConvertToANSI(iCurID - 1)
            End Select
        End Get

    End Property

End Class
