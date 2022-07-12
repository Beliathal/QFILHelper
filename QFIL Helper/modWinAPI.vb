Imports System.Runtime.InteropServices

Module modWinAPI

    <StructLayout(LayoutKind.Sequential)>
    Public Structure COORD
        Public X As Short
        Public Y As Short

        Public Sub New(X As Short, Y As Short)
            Me.X = X
            Me.Y = Y
        End Sub
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure CONSOLE_FONT_INFO_EX
        Public cbSize As UInteger
        Public nFont As UInteger
        Public dwFontSize As COORD
        Public FontFamily As UShort
        Public FontWeight As UShort

        Public face0, face1, face2, face3, face4, face5, face6, face7 As UInt64
    End Structure


    <DllImport("kernel32.dll")>
    Public Function SetCurrentConsoleFontEx( _
                                           ConsoleOutput As IntPtr, _
                                           MaximumWindow As Boolean, _
                                           ConsoleCurrentFontEx As CONSOLE_FONT_INFO_EX) As Boolean
    End Function

End Module
