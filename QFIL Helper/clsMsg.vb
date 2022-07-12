Imports System.IO
Imports System.Reflection
Imports Microsoft.VisualBasic

Public Class clsMsg

    Private Enum Language As Byte
        EN_Lang = 0
        RU_Lang = 1
    End Enum

    Private sUIMsg() As String
    Private eCurLang As Language

    Public Function ParseArguments(ByRef sCurLang() As String) As Boolean

        If sCurLang.Length = 0 Then _
            ReDim sCurLang(0 To 0)

        Select Case sCurLang(0)

            Case "-en", "-EN", ""
                eCurLang = Language.EN_Lang
                LoadMessages(Language.EN_Lang)

            Case "-ru", "-RU"
                eCurLang = Language.RU_Lang
                LoadMessages(Language.RU_Lang)

            Case Else

                Console.WriteLine("Ivalid arguments")
                Console.ReadKey(True)
                Return False

        End Select

        Return True

    End Function

    ' I've studied russian at the univercity, it's not my native language, 
    ' I'm not sure if I've translated eveything correctly :)

    Private Overloads Function LoadMessages() As Boolean

        Select Case eCurLang
            Case Language.EN_Lang : sUIMsg = File.ReadAllLines("english.msg")
            Case Language.RU_Lang : sUIMsg = File.ReadAllLines("russian.msg")
        End Select

        If sUIMsg.Length > 23 Then Return True

        Select Case eCurLang

            Case Language.EN_Lang
                Console.WriteLine("Critical error: File english.msg is corrupt!")
                Console.ReadKey(True)
                Return False

            Case Language.RU_Lang
                Console.WriteLine("Критическая ошибка: Файл russian.msg повреджен!")
                Console.ReadKey(True)
                Return False

        End Select

    End Function

    Private Overloads Sub LoadMessages(ByVal eLang As Language)

        'Dim sMsgList As String() = _
        'System.Reflection.Assembly.GetExecutingAssembly.GetManifestResourceNames()

        Dim ioSourceFile As StreamReader

        With Assembly.GetExecutingAssembly

            Select Case eLang

                Case Language.EN_Lang
                    ioSourceFile = New StreamReader(.GetManifestResourceStream("QFIL_Helper.english.msg"))
                    sUIMsg = ioSourceFile.ReadToEnd().Split(vbCrLf)

                Case Language.RU_Lang
                    ioSourceFile = New StreamReader(.GetManifestResourceStream("QFIL_Helper.russian.msg"))
                    sUIMsg = ioSourceFile.ReadToEnd().Split(vbCrLf)

            End Select

        End With

        ioSourceFile.Close()
        ioSourceFile.Dispose()
        ioSourceFile = Nothing

    End Sub

    Public Function ValidateLanguage(ByRef sCurLang() As String) As Boolean

        If sCurLang.Length = 0 Then _
            ReDim sCurLang(0 To 0)

        Select Case sCurLang(0)

            Case "-en", "-EN", ""

                If Not File.Exists("english.msg") Then
                    Console.WriteLine("Critical error: english.msg file is missing!")
                    Console.ReadKey(True)
                    Return False
                End If

                eCurLang = Language.EN_Lang

            Case "-ru", "-RU"

                If Not File.Exists("russian.msg") Then
                    Console.WriteLine("Критическая ошибка: Не найден файл russian.msg!")
                    Console.ReadKey(True)
                    Return False
                End If

                eCurLang = Language.RU_Lang

            Case Else

                Console.WriteLine("Ivalid arguments")
                Console.ReadKey(True)
                Return False

        End Select

        Return LoadMessages()

    End Function

    Public ReadOnly Property ID2Msg(ByVal iCurID As Byte) As String

        Get
            Return sUIMsg(iCurID - 1)
        End Get

    End Property

End Class
