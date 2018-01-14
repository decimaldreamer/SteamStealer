Imports System.Runtime.InteropServices

Public Class WinApis
    <DllImport("kernel32.dll")>
    Public Shared Sub GetSystemInfo(ByRef input As SYSTEM_INFO)
    End Sub
    <DllImport("kernel32.dll")>
    Public Shared Function OpenProcess(ProcessAcces As UInteger, bInheritHandle As Boolean, processId As Integer) As IntPtr
    End Function
    <DllImport("kernel32.dll")>
    Public Shared Function ReadProcessMemory(handle As IntPtr, adress As IntPtr, <Out> buffer As Byte(), size As UInteger, ByRef numberofbytesread As IntPtr) As Boolean
    End Function
    <DllImport("kernel32.dll")>
    Public Shared Function VirtualQueryEx(handle As IntPtr, adress As IntPtr, ByRef processQuery As PROCESS_QUERY_INFORMATION, length As UInteger) As Integer
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure PROCESS_QUERY_INFORMATION
        Public BaseAdress As IntPtr
        Public AllocationBase As IntPtr
        Public AllocationProtect As UInteger
        Public RegionSize As UInteger
        Public State As UInteger
        Public Protect As UInteger
        Public Type As UInteger
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure SYSTEM_INFO
        Public processorArchitecture As UShort
        Private reserved As UShort
        Public pageSize As UInteger
        Public minimumApplicationAddress As IntPtr
        Public maximumApplicationAddress As IntPtr
        Public activeProcessorMask As IntPtr
        Public numberOfProcessors As UInteger
        Public processorType As UInteger
        Public allocationGranularity As UInteger
        Public processorLevel As UShort
        Public processorRevision As UShort
    End Structure
End Class
