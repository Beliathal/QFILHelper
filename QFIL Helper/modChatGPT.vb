Imports System.Runtime.InteropServices

Module modChatGPT

    ' Written with the help of ChatGPT ;)

    Public Const FILE_ATTRIBUTE_DIRECTORY As Integer = &H10
    Public Const FILE_ATTRIBUTE_COMPRESSED As Integer = &H800
    Public Const FO_DELETE As Integer = &H3
    Public Const FOF_ALLOWUNDO As Integer = &H40
    Public Const FOF_NOCONFIRMATION As Integer = &H10
    Public Const FOF_NOERRORUI As Integer = &H4000

    <DllImport("kernel32.dll", SetLastError:=True)>
    Public Function GetFileAttributes(ByVal lpFileName As String) As Integer
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Public Function SetFileAttributes(ByVal lpFileName As String, ByVal dwFileAttributes As Integer) As Boolean
    End Function

    <DllImport("shell32.dll", CharSet:=CharSet.Auto)>
    Public Function SHFileOperation(ByRef lpFileOp As SHFILEOPSTRUCT) As Integer
    End Function

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto, Pack:=1)>
    Public Structure SHFILEOPSTRUCT
        Public hWnd As IntPtr
        Public wFunc As Integer
        Public pFrom As String
        Public pTo As String
        Public fFlags As Short
        Public fAnyOperationsAborted As Boolean
        Public hNameMappings As IntPtr
        Public lpszProgressTitle As String
    End Structure

    Public Sub ApplyCompression(ByVal folderPath As String)
        Dim currentAttributes As Integer = GetFileAttributes(folderPath)

        If (currentAttributes And FILE_ATTRIBUTE_DIRECTORY) <> 0 Then
            currentAttributes = currentAttributes Or FILE_ATTRIBUTE_COMPRESSED
            SetFileAttributes(folderPath, currentAttributes)

            'Dim shf As SHFILEOPSTRUCT
            'shf.wFunc = FO_DELETE
            'shf.pFrom = folderPath + vbNullChar
            'shf.fFlags = FOF_ALLOWUNDO Or FOF_NOCONFIRMATION Or FOF_NOERRORUI
            'SHFileOperation(shf)
        End If
    End Sub

End Module
