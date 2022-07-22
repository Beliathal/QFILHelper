Module modToolBag

    Public Function IsNumber(ByRef oCrValue As Object) As Boolean

        Return _
           TypeOf oCrValue Is UInt16 OrElse _
           TypeOf oCrValue Is UInt32 OrElse _
           TypeOf oCrValue Is UInt64 OrElse _
           TypeOf oCrValue Is Byte OrElse _
           TypeOf oCrValue Is SByte OrElse _
           TypeOf oCrValue Is Int16 OrElse _
           TypeOf oCrValue Is Int32 OrElse _
           TypeOf oCrValue Is Int64

    End Function

    Public Function IsLiteral(ByRef oCrValue As Object) As Boolean

        Return _
            TypeOf oCrValue Is String OrElse _
            TypeOf oCrValue Is Char

    End Function

End Module
