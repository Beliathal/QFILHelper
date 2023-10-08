Imports System.IO
Imports System.Environment
Imports System.Management
Imports Microsoft.VisualBasic
Imports System.Reflection

Public Class clsBase

    Protected geFailed As ErrorType
    Protected galoLookUp(0 To 6) As List(Of stPInfo)

    Protected gsCOMPort As String
    Protected gsPXMLList As String
    Protected gsBackupDir As String
    Protected gsFlashDir As String = "Flash\"
    Protected gsFHLoader As String

    Protected Enum ErrorType As Byte
        ER_NOERR = 0
        ER_FAILD = 1
        ER_ABORT = 2
    End Enum

    Protected getProgramFiles As Func(Of String) = _
        Function() GetFolderPath(SpecialFolder.ProgramFilesX86) _
                   & "\Qualcomm\QPST\bin\fh_loader.exe"

    Protected getAppRoaming As Func(Of String) = _
        Function() GetFolderPath(SpecialFolder.ApplicationData) _
                    & "\Qualcomm\QFIL\" & gsCOMPort & "_PartitionsList.xml"


    ' For some reason QFIL refuses to use another drive and windows temp folder :(

    '     Protected getTempFolder As Func(Of String) = _
    '           Function() Path.GetTempPath & "QFilHelper\"

    Protected getTempFolder As Func(Of String) = _
        Function() "TMP\"

    ' LG uses 4k sector sizes, hence, number of sectors = partition size in bytes / 4096

    Protected Sectors2Bytes As Func(Of UInt64, UInt32) = _
        Function(iCurSectors As UInt32) iCurSectors * 4096

    Protected Bytes2Sectors As Func(Of UInt32, UInt64) = _
            Function(iCurBytes As UInt64) iCurBytes / 4096

    Public Sub QueryCOMPorts()

        Dim tCOMPort = New Timers.Timer

        tCOMPort.AutoReset = True
        tCOMPort.Interval = 500
        AddHandler tCOMPort.Elapsed, AddressOf WaitForCOMPort
        tCOMPort.Start()

        Console.WriteLine(goSpeaker.ID2Msg(30))
        Console.ReadKey(True)

        tCOMPort.Enabled = False
        tCOMPort.Stop()

    End Sub

    Private Sub WaitForCOMPort( _
                              ByVal sender As Object, _
                              ByVal e As System.Timers.ElapsedEventArgs)

        Static iBlinker As UInt16
        Dim eColor As System.ConsoleColor
        Dim sPortName As String

        sender.Enabled = False

        If Not FindCOMPort(sPortName) Then

            If iBlinker Mod 2 = 0 Then

                eColor = Console.ForegroundColor
                Console.ForegroundColor = ConsoleColor.Yellow
                Console.Write(goSpeaker.ID2Msg(31))
                Console.ForegroundColor = eColor
                Console.CursorTop -= 1

                iBlinker += IIf(iBlinker = UInt16.MaxValue, 0, 1)

            Else

                eColor = Console.ForegroundColor
                Console.ForegroundColor = Console.BackgroundColor
                Console.Write(goSpeaker.ID2Msg(31))
                Console.ForegroundColor = eColor
                Console.CursorTop -= 1

                iBlinker += IIf(iBlinker = UInt16.MaxValue, 0, 1)

            End If

            sender.Enabled = True

        Else

            sender.Enabled = False
            sender.Stop()

            eColor = Console.ForegroundColor
            Console.ForegroundColor = Console.BackgroundColor
            Console.Write(goSpeaker.ID2Msg(31))
            Console.ForegroundColor = eColor
            Console.CursorTop -= 1

            Console.ForegroundColor = ConsoleColor.Green
            Console.WriteLine(goSpeaker.ID2Msg(32) & sPortName)
            Console.ForegroundColor = eColor

        End If

    End Sub

    ' This function requires referrence to System.Management assembly. It would iterate thru all
    ' aviable devices in Windows, looking for "Qualcomm". There's yet another way to do this,
    ' using QPST assembly supplied with Qualcomm's USB drivers which can be refereced in the VS.

    Private Function FindCOMPort(ByRef sPortName As String) As Boolean

        If Ports.SerialPort.GetPortNames.Count = 0 Then Return False

        Dim oSearcher As Management.ManagementObjectSearcher
        Dim oCollection As Management.ManagementObjectCollection

        oSearcher = New Management.ManagementObjectSearcher("Select * from Win32_PnPEntity")
        oCollection = oSearcher.Get

        For Each oEntity As Management.ManagementObject In oCollection

            sPortName = oEntity.Properties.Item("Name").Value

            If sPortName = "" Then Continue For
            If sPortName.IndexOf("Qualcomm HS-USB QDLoader 9008") > -1 Then Return True

        Next

    End Function

    ' This function will extract COM port number from: "Qualcomm HS-USB QDLoader 9008 (COM?)"

    Private Function ParseCOMPortNumber(ByRef sPortName As String) As Boolean

        If sPortName.IndexOf("COM") < 0 Then Return False

        gsCOMPort = sPortName.Replace("(", "")
        gsCOMPort = sPortName.Replace(")", "")
        gsCOMPort = sPortName.Substring(sPortName.IndexOf("COM"))

        Return True

    End Function

    ' Test to see if COMPort detected, if QFIL is running and FHLoader is present: ValidateCQF

    Protected Function ValidateCQF(Optional ByVal isDebug As Boolean = False) As Boolean

        If isDebug Then Return True

        ' Is there a Qualcomm device connected?
        If Not FindCOMPort(gsCOMPort) Then

            Console.WriteLine(goSpeaker.ID2Msg(33))
            Console.WriteLine(goSpeaker.ID2Msg(34))
            Console.WriteLine(goSpeaker.ID2Msg(21))
            Console.ReadKey(True)
            Return False

        End If

        ' Attempt to extract COM port number
        If Not ParseCOMPortNumber(gsCOMPort) Then

            Console.WriteLine(goSpeaker.ID2Msg(27))
            Console.WriteLine(goSpeaker.ID2Msg(21))
            Console.ReadKey(True)
            Return False

        End If

        ' Has the user lunched QFIL.exe?
        If Process.GetProcessesByName("QFIL").Count = 0 Then

            Console.WriteLine(goSpeaker.ID2Msg(41))
            Console.WriteLine(goSpeaker.ID2Msg(42))
            Console.WriteLine(goSpeaker.ID2Msg(21))
            Console.ReadKey(True)
            Return False

        End If

        LocateFHLoader()
        CreateTempFolder()
        Return True

    End Function

    ' Added on: 2023-02-01
    ' Creates temp folder to store GPT headers for partition lookup during Flash, LUN and Erase ops

    Protected Sub CreateTempFolder()

        Dim sTmpDir As String = "TMP"

        If Directory.Exists(sTmpDir) Then _
            Directory.Delete(sTmpDir, True)

        Directory.CreateDirectory(sTmpDir)

    End Sub

    ' Creates new backup folder according to this pattern: 
    ' Backup-year-month-days-houes-minutes-seconds

    Protected Sub CreateBackupFolder(Optional ByVal isDebug As Boolean = False)

        If isDebug Then Exit Sub

        gsBackupDir = "Backup-" & _
            DateTime.Now.ToString("yyyy-MM-dd-HHmmss") & "\"

        Directory.CreateDirectory(gsBackupDir)

        '06-10-2022 FileSystem.FileCopy(gsPXMLList, gsBackupDir & gsPXMLList)

    End Sub

    Protected Function CreateDebugBackupFolder(Optional ByVal isDebug As Boolean = False) As Boolean

        Try

            If Not isDebug Then Return True

            gsBackupDir = "P:\Temp\Backup-" & _
                DateTime.Now.ToString("yyyy-MM-dd-HHmmss") & "\"

            Directory.CreateDirectory(gsBackupDir)

            Dim sFolder As String = _
                gsBackupDir.Substring(0, gsBackupDir.Length - 1).Replace("\", "\\")

            Dim sQuery As String = _
                String.Format("SELECT * FROM Win32_Directory WHERE Name='{0}'", sFolder)

            Dim oSearcher As New ManagementObjectSearcher(sQuery)

            For Each oItem As ManagementObject In oSearcher.Get()
                oItem.InvokeMethod("Compress", Nothing)
            Next

        Catch

            Console.WriteLine(goSpeaker.ID2Msg(5))
            Console.ReadKey(False)
            Return False

        End Try

    End Function

    ' If for some reason backup has failed, and there arern't any *.bin files within the backup folder, 
    ' then it should be deleted, to avoid creating garbage backup folders...

    Protected Sub CleanUpBackupFolder()

        If gsBackupDir = Nothing Then Exit Sub ' debug mode on?

        If Directory.GetFiles(gsBackupDir, "*.bin").Count > 0 Then Exit Sub

        ' True to force delete non empty Dir
        Directory.Delete(gsBackupDir, True)

    End Sub

    Protected Function NTFSCompression(Optional ByVal doCompress As Boolean = True) As Boolean

        Try

            If gsBackupDir = Nothing Then Return True ' Debug mode?
            If doCompress = False Then Return True ' Skip compression flag

            ' single slash has to be replaced with double slash in the folder path, 
            ' otherwise this poop doesn't function

            Dim sFolder As String = _
                Directory.GetCurrentDirectory.Replace("\", "\\") & "\\" & _
                gsBackupDir.Substring(0, gsBackupDir.Length - 1)

            Dim sQuery As String = _
                String.Format("SELECT * FROM Win32_Directory WHERE Name='{0}'", sFolder)

            Dim oSearcher As New ManagementObjectSearcher(sQuery)

            For Each oItem As ManagementObject In oSearcher.Get()
                oItem.InvokeMethod("Compress", Nothing)
            Next

            Return True

        Catch

            Console.WriteLine(goSpeaker.ID2Msg(5))
            Console.ReadKey(False)
            Return False

        End Try

        ' An entertaining conv with ChatGPT:

        ' The For Each loop in the code example loops through a collection of ManagementObject instances 
        ' returned by the ManagementObjectSearcher.Get method.

        ' In this specific case, the ManagementObjectSearcher is searching for a specific directory using 
        ' the query "SELECT * FROM Win32_Directory WHERE Name='C:\test'" and the Get method returns a 
        ' collection of all ManagementObject instances that match the query. 

        ' The For Each loop then iterates through each ManagementObject in the collection, allowing 
        ' you to perform actions on each object.

        ' ME: The code assumes there are multiple "C:\Test" directories, which is not possible!

        ' ChatGPT: No, the code does not assume there are multiple directories named "C:\Test". 

        ' The ManagementObjectSearcher.Get method returns a collection of all ManagementObject instances 
        ' that match the query. In this case, the query is "SELECT * FROM Win32_Directory WHERE Name='C:\test'", 
        ' which is searching for a directory named "C:\Test".

        ' If there is only one directory named "C:\Test" on the system, then the collection returned by 
        ' Get will contain only one ManagementObject instance representing that directory. 

        ' The For Each loop will still iterate through that one instance, allowing you to perform actions on it. 

        ' If there are multiple directories with the same name, the collection returned by Get will contain a 
        ' ManagementObject for each of those directories, and the For Each loop will iterate through each instance, 
        ' allowing you to perform actions on each one.

        ' ME: Face Palm


        'Console.WriteLine("NTFS compression applied to directory: " & sFolder)
        'Console.ReadLine()

    End Function

    ' Displays: Press any key to return to the main menu
    ' If the calling function hasn't executed even 1 command skip showning "Process completed"

    Protected Sub ProcessCompletedMsg(Optional ByVal isExec As Boolean = True)

        If geFailed = ErrorType.ER_FAILD Then

            Console.WriteLine(goSpeaker.ID2Msg(47))
            Console.WriteLine(goSpeaker.ID2Msg(17))
            Console.ReadKey(True)

            geFailed = ErrorType.ER_NOERR
            Exit Sub

        End If

        If Not isExec Then

            geFailed = ErrorType.ER_NOERR
            Exit Sub

        End If

        Console.WriteLine(goSpeaker.ID2Msg(48))
        Console.WriteLine(goSpeaker.ID2Msg(17))
        Console.ReadKey(True)

        geFailed = ErrorType.ER_NOERR

    End Sub

    Protected Sub LocateFHLoader()

        gsFHLoader = getProgramFiles()

        If File.Exists(gsFHLoader) Then Exit Sub

        ' IF fh_loader is missing in QFIL instalation folder

        If Not File.Exists("fh_loader.exe") Then
            File.WriteAllBytes("fh_loader.exe", My.Resources.fh_loader)
            gsFHLoader = "fh_loader.exe"
        End If

        ' The reason why I preffer not to use the attached fh_loader is because 
        ' the urser's build of the QFIL might differ from the one I'm using and 
        ' the attached fh_loader might not function correctly.

    End Sub

    Protected Function ExecuteCommand( _
                          ByRef sCMDLine As String) As Boolean

        Dim oProcess As New Process
        Dim sError, sBuffer As String

        oProcess.StartInfo.Arguments = sCMDLine
        oProcess.StartInfo.CreateNoWindow = False
        'oProcess.StartInfo.FileName = "fh_loader.exe"
        oProcess.StartInfo.FileName = gsFHLoader
        oProcess.StartInfo.UseShellExecute = False
        oProcess.StartInfo.RedirectStandardOutput = True
        oProcess.StartInfo.RedirectStandardError = True
        oProcess.Start()

        sBuffer = oProcess.StandardOutput.ReadToEnd()
        sError = oProcess.StandardError.ReadToEnd()

        oProcess.WaitForExit()
        oProcess.Close()
        oProcess.Dispose()
        oProcess = Nothing

        Console.WriteLine(sBuffer)
        Console.WriteLine(sError)

        If sBuffer.Contains("Failed to open com port") OrElse _
            sBuffer.Contains("ERROR: Could not write to") OrElse _
            sBuffer.Contains("SAHARA mode!!") Then
            Console.Clear()
            Console.WriteLine(goSpeaker.ID2Msg(11) & vbCrLf)
            Console.WriteLine(goSpeaker.ID2Msg(12))
            Console.WriteLine(goSpeaker.ID2Msg(13))
            Console.WriteLine(goSpeaker.ID2Msg(14))
            Console.WriteLine(goSpeaker.ID2Msg(15))
            Console.WriteLine(goSpeaker.ID2Msg(16) & vbCrLf)
            'Console.WriteLine(vbCrLf & goSpeaker.ID2Msg(17))
            'Console.ReadKey(True)
            geFailed = ErrorType.ER_FAILD
            OutputErrorInfo(sError, sBuffer)
        Else : Return True
        End If

    End Function

    Private Sub OutputErrorInfo(ByRef sError As String, _
                                ByRef sBuffer As String)

        Dim sFileName As String = "Errors\QFIL-Error-" & _
            DateTime.Now.ToString("yyyy-MM-dd-HHmmss")

        Dim ioTXTWriter As New StreamWriter( _
             File.Open(sFileName, FileMode.Create))

        ioTXTWriter.WriteLine(sError & " " & sBuffer)

        ioTXTWriter.Flush()
        ioTXTWriter.Close()
        ioTXTWriter.Dispose()
        ioTXTWriter = Nothing

    End Sub

    Protected Sub ResetInfo()

        ' Loop thru declared variables and reset them. 

        ' I know that using reflection without a good reason is lame... 
        ' I there are other methods of doings this.... 
        ' But I'm too lazy to implement them :)

        ' Added UInt64 support on 02-10-2022

        Dim oaPtr() As FieldInfo = _
            GetType(clsBase).GetFields(BindingFlags.NonPublic Or BindingFlags.Instance)

        ' Use Dim sName As string = oPtr.Name for getting the name of the variable
        ' Use TypeOf oPtr.GetValue(Me) Is Nothing for empty strings

        For Each oPtr As FieldInfo In oaPtr

            If TypeOf oPtr.GetValue(Me) Is String Then oPtr.SetValue(Me, String.Empty)
            If TypeOf oPtr.GetValue(Me) Is UInt32 Then oPtr.SetValue(Me, UInt32.MinValue)
            If TypeOf oPtr.GetValue(Me) Is UInt64 Then oPtr.SetValue(Me, UInt64.MinValue)
            If TypeOf oPtr.GetValue(Me) Is Boolean Then oPtr.SetValue(Me, False)

        Next

        Erase oaPtr

    End Sub

    ' Clears GPT LookUp list

    Protected Sub ResetLookUp()

        For iCnt As Byte = 0 To galoLookUp.Count - 1

            If galoLookUp(iCnt) IsNot Nothing Then
                galoLookUp(iCnt).Clear()
                galoLookUp(iCnt) = Nothing
            End If

            galoLookUp(iCnt) = New List(Of stPInfo)

        Next

    End Sub

    Protected Function LittleEndianConverter(ByRef iaBuffer() As Byte) As UInt64

        If Not BitConverter.IsLittleEndian Then Array.Reverse(iaBuffer)
        Return BitConverter.ToUInt64(iaBuffer, 0)

    End Function

    ' If any1 tells ya can't use Optional ByRef in VB.NET, 
    ' then that person is a closed-minded idiot :)

    Protected Function IsByte(ByRef sCurValue As String, _
                              Optional ByRef iTest As Byte = 0) As Boolean

        Return Byte.TryParse(sCurValue, iTest)

    End Function

    Protected Function IsUI16(ByRef sCurValue As String, _
                              Optional ByRef iTest As UInt16 = 0) As Boolean

        Return UInt16.TryParse(sCurValue, iTest)

    End Function

    Protected Function IsUI32(ByRef sCurValue As String, _
                              Optional ByRef iTest As UInt32 = 0) As Boolean

        Return UInt32.TryParse(sCurValue, iTest)

    End Function

    Protected Overrides Sub Finalize()

        Erase galoLookUp
        MyBase.Finalize()

    End Sub

End Class

Public Structure stPInfo

    Public iLUN As Nullable(Of Byte)
    Public iStart As UInt32
    Public iSectors As UInt32
    Private sName As String

    ' Number of Sectors = (Last Sector + 1) - First Sector

    Public Property iEnd() As UInt32

        Set(value As UInt32)
            iSectors = (value + 1) - iStart
        End Set

        Get
            Return iSectors
        End Get

    End Property

    Public ReadOnly Property iSize() As UInt32

        Get
            Return iStart + iSectors
        End Get

    End Property

    Public ReadOnly Property sSectors() As String

        Get
            Return If(iSectors = 0, "ALL", iSectors.ToString)
        End Get

    End Property

    Public Property sLabel() As String

        Set(value As String)
            sName = value
        End Set

        Get
            Return If(sName = "", "ALL", sName)
        End Get

    End Property

End Structure