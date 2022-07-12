Imports System.IO
Imports System.Text
Imports System.Reflection
Imports Microsoft.VisualBasic

Public Class clsMsg

    Private Enum Language As Byte
        EN_Lang = 0
        RU_UTF8 = 1
        RU_ANSI = 2
    End Enum

    Private sUIMsg() As String
    Private eCurLang As Language

    Public Function ParseArguments(ByRef sCurLang() As String) As Boolean

        If sCurLang.Length = 0 Then

            eCurLang = Language.EN_Lang
            LoadMessages()
            Return True

        ElseIf sCurLang(0).ToLower = "-ru" Then

            If sCurLang.Length = 1 Then

                eCurLang = Language.RU_ANSI
                LoadMessages()
                Return True

            ElseIf sCurLang(1).ToLower = "-utf8" Then

                Console.OutputEncoding = Encoding.UTF8
                eCurLang = Language.RU_UTF8
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

        'Dim sMsgList As String() = _
        'System.Reflection.Assembly.GetExecutingAssembly.GetManifestResourceNames()

        Dim ioSourceFile As StreamReader

        With Assembly.GetExecutingAssembly

            Select Case eCurLang

                Case Language.EN_Lang
                    ioSourceFile = New StreamReader(.GetManifestResourceStream("QFIL_Helper.english.msg"))
                    sUIMsg = ioSourceFile.ReadToEnd().Split(vbCrLf)

                Case Language.RU_UTF8, Language.RU_ANSI
                    ioSourceFile = New StreamReader(.GetManifestResourceStream("QFIL_Helper.russian.msg"))
                    sUIMsg = ioSourceFile.ReadToEnd().Split(vbCrLf)

            End Select

        End With

        ioSourceFile.Close()
        ioSourceFile.Dispose()
        ioSourceFile = Nothing

    End Sub

    Private Function ConvertToANSI(ByVal iCurID As Byte) As String

        Dim oUnicode As Encoding = Encoding.Unicode
        Dim oASCII As Encoding = Encoding.GetEncoding(1251)

        Dim iaUnicode As Byte() = oUnicode.GetBytes(sUIMsg(iCurID))
        Dim iaASCII As Byte() = System.Text.Encoding.Convert(oUnicode, oASCII, iaUnicode)

        ConvertToANSI = oASCII.GetString(iaASCII)
        oUnicode = Nothing : oASCII = Nothing
        iaUnicode = Nothing : iaASCII = Nothing

    End Function

    Public ReadOnly Property ID2Msg(ByVal iCurID As Byte) As String

        Get
            Select Case eCurLang
                Case Language.EN_Lang : Return sUIMsg(iCurID - 1)
                Case Language.RU_UTF8 : Return sUIMsg(iCurID - 1)
                Case Language.RU_ANSI : Return ConvertToANSI(iCurID - 1)
            End Select
        End Get

    End Property

End Class
