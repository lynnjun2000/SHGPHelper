using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net;

namespace ImgServer.Net
{
    #region 枚举定义
    // Enum to define the set of values used to indicate the type of table returned by  
    // calls made to the function 'GetExtendedTcpTable'. 
    public enum TcpTableClass
    {
        TCP_TABLE_BASIC_LISTENER,
        TCP_TABLE_BASIC_CONNECTIONS,
        TCP_TABLE_BASIC_ALL,
        TCP_TABLE_OWNER_PID_LISTENER,
        TCP_TABLE_OWNER_PID_CONNECTIONS,
        TCP_TABLE_OWNER_PID_ALL,
        TCP_TABLE_OWNER_MODULE_LISTENER,
        TCP_TABLE_OWNER_MODULE_CONNECTIONS,
        TCP_TABLE_OWNER_MODULE_ALL
    }

    // Enum to define the set of values used to indicate the type of table returned by calls 
    // made to the function GetExtendedUdpTable. 
    public enum UdpTableClass
    {
        UDP_TABLE_BASIC,
        UDP_TABLE_OWNER_PID,
        UDP_TABLE_OWNER_MODULE
    }

    // Enum for different possible states of TCP connection 
    public enum MibTcpState
    {
        CLOSED = 1,
        LISTENING = 2,
        SYN_SENT = 3,
        SYN_RCVD = 4,
        ESTABLISHED = 5,
        FIN_WAIT1 = 6,
        FIN_WAIT2 = 7,
        CLOSE_WAIT = 8,
        CLOSING = 9,
        LAST_ACK = 10,
        TIME_WAIT = 11,
        DELETE_TCB = 12,
        NONE = 0
    }

    #endregion

    #region  TCP记录定义
    /// <summary> 
    /// The structure contains information that describes an IPv4 TCP connection with 
    /// IPv4 addresses, ports used by the TCP connection, and the specific process ID 
    /// (PID) associated with connection. 
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPROW_OWNER_PID
    {
        public MibTcpState state;
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;
        public uint remoteAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] remotePort;
        public int owningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPROW
    {
        public MibTcpState state;
        public int localAddr;
        public int localPort;
        public int remoteAddr;
        public int remotePort;
    }

    /// <summary> 
    /// The structure contains a table of process IDs (PIDs) and the IPv4 TCP links that 
    /// are context bound to these PIDs. 
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
            SizeConst = 1)]
        public MIB_TCPROW_OWNER_PID[] table;
    }

    /// <summary> 
    /// This class provides access an IPv4 TCP connection addresses and ports and its 
    /// associated Process IDs and names. 
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    public class TcpProcessRecord
    {
        public IPAddress LocalAddress { get; set; }
        public ushort LocalPort { get; set; }
        public IPAddress RemoteAddress { get; set; }
        public ushort RemotePort { get; set; }
        public MibTcpState State { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        public TcpProcessRecord(IPAddress localIp, IPAddress remoteIp, ushort localPort,
            ushort remotePort, int pId, MibTcpState state)
        {
            LocalAddress = localIp;
            RemoteAddress = remoteIp;
            LocalPort = localPort;
            RemotePort = remotePort;
            State = state;
            ProcessId = pId;
            // Getting the process name associated with a process id. 
            //if (Process.GetProcesses().Any(process => process.Id == pId))
            //{
            //    ProcessName = Process.GetProcessById(ProcessId).ProcessName;
            //}
        }
    }

    #endregion

    #region UDP定义
    /// <summary> 
    /// The structure contains an entry from the User Datagram Protocol (UDP) listener 
    /// table for IPv4 on the local computer. The entry also includes the process ID 
    /// (PID) that issued the call to the bind function for the UDP endpoint. 
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPROW_OWNER_PID
    {
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;
        public int owningPid;
    }

    /// <summary> 
    /// The structure contains the User Datagram Protocol (UDP) listener table for IPv4 
    /// on the local computer. The table also includes the process ID (PID) that issued 
    /// the call to the bind function for each UDP endpoint. 
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
            SizeConst = 1)]
        public UdpProcessRecord[] table;
    }

    /// <summary> 
    /// This class provides access an IPv4 UDP connection addresses and ports and its 
    /// associated Process IDs and names. 
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    public class UdpProcessRecord
    {
        public IPAddress LocalAddress { get; set; }
        public uint LocalPort { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        public UdpProcessRecord(IPAddress localAddress, uint localPort, int pId)
        {
            LocalAddress = localAddress;
            LocalPort = localPort;
            ProcessId = pId;
            //if (Process.GetProcesses().Any(process => process.Id == pId))
            //    ProcessName = Process.GetProcessById(ProcessId).ProcessName;
        }
    }

    #endregion

    class TCPConnHelper
    {
        private const int AF_INET = 2;

        #region 系统API函数导入
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)] 
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize, 
            bool bOrder, int ulAf, TcpTableClass tableClass, uint reserved );

        [DllImport("iphlpapi.dll")]
        private static extern int GetTcpTable(IntPtr pTcpTable, ref int pdwSize, bool bOrder);


        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)] 
        private static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int pdwSize, 
            bool bOrder, int ulAf, UdpTableClass tableClass, uint reserved );

        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SetTcpEntry([In]IntPtr pTcprow);

        //Convert 16-bit value from network to host byte order 
        [DllImport("wsock32.dll")]
        private static extern int ntohs(int netshort);

        //Convert 16-bit value back again 
        [DllImport("wsock32.dll")]
        private static extern int htons(int netshort);

        const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        [DllImport("Kernel32.dll")]
        private static extern int FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId,
        [Out]StringBuilder lpBuffer, uint nSize, IntPtr arguments);
        #endregion

        private static IntPtr GetPtrFromTCPROW(MIB_TCPROW obj)
        {
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj));
            Marshal.StructureToPtr(obj, ptr, false);
            return ptr;
        }
        private static void ReleasePtrForTCPROW(IntPtr ptr)
        {
            Marshal.FreeCoTaskMem(ptr);
        }

        /// <summary> 
        /// This function reads and parses the active TCP socket connections available 
        /// and stores them in a list. 
        /// </summary> 
        /// <returns> 
        /// It returns the current set of TCP socket connections which are active. 
        /// </returns> 
        /// <exception cref="OutOfMemoryException"> 
        /// This exception may be thrown by the function Marshal.AllocHGlobal when there 
        /// is insufficient memory to satisfy the request. 
        /// </exception> 
        private static List<TcpProcessRecord> GetAllTcpConnections()
        {
            int bufferSize = 0;
            List<TcpProcessRecord> tcpTableRecords = new List<TcpProcessRecord>();

            // Getting the size of TCP table, that is returned in 'bufferSize' variable. 
            uint result = GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET,
                TcpTableClass.TCP_TABLE_OWNER_PID_ALL,0);

            // Allocating memory from the unmanaged memory of the process by using the 
            // specified number of bytes in 'bufferSize' variable. 
            IntPtr tcpTableRecordsPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // The size of the table returned in 'bufferSize' variable in previous 
                // call must be used in this subsequent call to 'GetExtendedTcpTable' 
                // function in order to successfully retrieve the table. 
                result = GetExtendedTcpTable(tcpTableRecordsPtr, ref bufferSize, true,
                    AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL,0);

                // Non-zero value represent the function 'GetExtendedTcpTable' failed, 
                // hence empty list is returned to the caller function. 
                if (result != 0)
                    return new List<TcpProcessRecord>();

                // Marshals data from an unmanaged block of memory to a newly allocated 
                // managed object 'tcpRecordsTable' of type 'MIB_TCPTABLE_OWNER_PID' 
                // to get number of entries of the specified TCP table structure. 
                MIB_TCPTABLE_OWNER_PID tcpRecordsTable = (MIB_TCPTABLE_OWNER_PID)
                                        Marshal.PtrToStructure(tcpTableRecordsPtr,
                                        typeof(MIB_TCPTABLE_OWNER_PID));
                IntPtr tableRowPtr = (IntPtr)((long)tcpTableRecordsPtr +
                                        Marshal.SizeOf(tcpRecordsTable.dwNumEntries));

                // Reading and parsing the TCP records one by one from the table and 
                // storing them in a list of 'TcpProcessRecord' structure type objects. 
                for (int row = 0; row < tcpRecordsTable.dwNumEntries; row++)
                {
                    MIB_TCPROW_OWNER_PID tcpRow = (MIB_TCPROW_OWNER_PID)Marshal.
                        PtrToStructure(tableRowPtr, typeof(MIB_TCPROW_OWNER_PID));
                    tcpTableRecords.Add(new TcpProcessRecord(
                                          new IPAddress(tcpRow.localAddr),
                                          new IPAddress(tcpRow.remoteAddr),
                                          BitConverter.ToUInt16(new byte[2] { 
                                              tcpRow.localPort[1], 
                                              tcpRow.localPort[0] }, 0),
                                          BitConverter.ToUInt16(new byte[2] { 
                                              tcpRow.remotePort[1], 
                                              tcpRow.remotePort[0] }, 0),
                                          tcpRow.owningPid, tcpRow.state));
                    tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(tcpRow));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                Console.WriteLine("Out Of Memory when get tcp connections");
            }
            catch (Exception exception)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(exception.Message).Append(" when get tcp connections");
                Console.WriteLine(sb.ToString());
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTableRecordsPtr);
            }
            return tcpTableRecords != null ? tcpTableRecords.Distinct()
                .ToList<TcpProcessRecord>() : new List<TcpProcessRecord>();
        }


        /// <summary> 
        /// This function reads and parses the active UDP socket connections available 
        /// and stores them in a list. 
        /// </summary> 
        /// <returns> 
        /// It returns the current set of UDP socket connections which are active. 
        /// </returns> 
        /// <exception cref="OutOfMemoryException"> 
        /// This exception may be thrown by the function Marshal.AllocHGlobal when there 
        /// is insufficient memory to satisfy the request. 
        /// </exception> 
        private static List<UdpProcessRecord> GetAllUdpConnections()
        {
            int bufferSize = 0;
            List<UdpProcessRecord> udpTableRecords = new List<UdpProcessRecord>();


            uint result = GetExtendedUdpTable(IntPtr.Zero, ref bufferSize, true,
                AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID,0);

            // Allocating memory from the unmanaged memory of the process by using the 
            // specified number of bytes in 'bufferSize' variable. 
            IntPtr udpTableRecordPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // The size of the table returned in 'bufferSize' variable in previous 
                // call must be used in this subsequent call to 'GetExtendedUdpTable' 
                // function in order to successfully retrieve the table. 
                result = GetExtendedUdpTable(udpTableRecordPtr, ref bufferSize, true,
                    AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID,0);

                // Non-zero value represent the function 'GetExtendedUdpTable' failed, 
                // hence empty list is returned to the caller function. 
                if (result != 0)
                    return new List<UdpProcessRecord>();

                // Marshals data from an unmanaged block of memory to a newly allocated 
                // managed object 'udpRecordsTable' of type 'MIB_UDPTABLE_OWNER_PID' 
                // to get number of entries of the specified TCP table structure. 
                MIB_UDPTABLE_OWNER_PID udpRecordsTable = (MIB_UDPTABLE_OWNER_PID)
                    Marshal.PtrToStructure(udpTableRecordPtr, typeof(MIB_UDPTABLE_OWNER_PID));
                IntPtr tableRowPtr = (IntPtr)((long)udpTableRecordPtr +
                    Marshal.SizeOf(udpRecordsTable.dwNumEntries));

                // Reading and parsing the UDP records one by one from the table and 
                // storing them in a list of 'UdpProcessRecord' structure type objects. 
                for (int i = 0; i < udpRecordsTable.dwNumEntries; i++)
                {
                    MIB_UDPROW_OWNER_PID udpRow = (MIB_UDPROW_OWNER_PID)
                        Marshal.PtrToStructure(tableRowPtr, typeof(MIB_UDPROW_OWNER_PID));
                    udpTableRecords.Add(new UdpProcessRecord(new IPAddress(udpRow.localAddr),
                        BitConverter.ToUInt16(new byte[2] { udpRow.localPort[1], 
                            udpRow.localPort[0] }, 0), udpRow.owningPid));
                    tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(udpRow));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                Console.WriteLine("Out Of Memory when get udp connections");
            }
            catch (Exception exception)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(exception.Message).Append(" when get tcp connections");
                Console.WriteLine(sb.ToString());
            }
            finally
            {
                Marshal.FreeHGlobal(udpTableRecordPtr);
            }
            return udpTableRecords != null ? udpTableRecords.Distinct()
                .ToList<UdpProcessRecord>() : new List<UdpProcessRecord>();
        }

        public static string getTCPServerInfo(ushort serverPort)
        {
            string serverInfo = "";
            List<TcpProcessRecord> tcpClientList = GetAllTcpConnections();
            foreach (TcpProcessRecord tcpRecord in tcpClientList)
            {
                if (tcpRecord.RemotePort == serverPort)
                {
                    serverInfo = tcpRecord.RemoteAddress + ":" + Convert.ToString(tcpRecord.RemotePort);
                    //Console.WriteLine(serverInfo);
                    break;
                }
            }
            return serverInfo;
        }

        private static uint IpToInt(string ip)
        {
            char[] separator = new char[] { '.' };
            string[] items = ip.Split(separator);
            long v= long.Parse(items[0]) << 24
                    | long.Parse(items[1]) << 16
                    | long.Parse(items[2]) << 8
                    | long.Parse(items[3]);
            return Convert.ToUInt32(v);
        }

        //The function that fills the MIB_TCPROW array with connectioninfos 
        private static MIB_TCPROW[] getTcpTable()
        {
            IntPtr buffer = IntPtr.Zero; bool allocated = false;
            try
            {
                int iBytes = 0;
                GetTcpTable(IntPtr.Zero, ref iBytes, false); //Getting size of return data 
                buffer = Marshal.AllocCoTaskMem(iBytes); //allocating the datasize 

                allocated = true;

                GetTcpTable(buffer, ref iBytes, false); //Run it again to fill the memory with the data 

                int structCount = Marshal.ReadInt32(buffer); // Get the number of structures 

                IntPtr buffSubPointer = buffer; //Making a pointer that will point into the buffer 
                buffSubPointer = (IntPtr)((int)buffer + 4); //Move to the first data (ignoring dwNumEntries from the original MIB_TCPTABLE struct) 

                MIB_TCPROW[] tcpRows = new MIB_TCPROW[structCount]; //Declaring the array 

                //Get the struct size 
                MIB_TCPROW tmp = new MIB_TCPROW();
                int sizeOfTCPROW = Marshal.SizeOf(tmp);

                //Fill the array 1 by 1 
                for (int i = 0; i < structCount; i++)
                {
                    tcpRows[i] = (MIB_TCPROW)Marshal.PtrToStructure(buffSubPointer, typeof(MIB_TCPROW)); //copy struct data 
                    buffSubPointer = (IntPtr)((int)buffSubPointer + sizeOfTCPROW); //move to next structdata 
                }

                return tcpRows;

            }
            catch (Exception ex)
            {
                throw new Exception("getTcpTable failed! [" + ex.GetType().ToString() + "," + ex.Message + "]");
            }
            finally
            {
                if (allocated) Marshal.FreeCoTaskMem(buffer); //Free the allocated memory 
            }
        }

        public static void CloseRemotePort(int port)
        {
            MIB_TCPROW[] rows = getTcpTable();
            for (int i = 0; i < rows.Length; i++)
            {
                if (port == ntohs(rows[i].remotePort))
                {
                    MIB_TCPROW tcpRecord = rows[i];
                    tcpRecord.state = MibTcpState.DELETE_TCB;
                    IntPtr ptr = GetPtrFromTCPROW(tcpRecord);
                    //Console.WriteLine(String.Format("localaddr:{0},localport:{1},remoteaddr:{2},remoteport:{3}",
                    //    tcpRecord.localAddr, tcpRecord.localPort, tcpRecord.remoteAddr, tcpRecord.remotePort));
                    int ret = SetTcpEntry(ptr);
                    ReleasePtrForTCPROW(ptr);
                    if (ret != 0)
                    {
                        uint dwFlags = FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS;
                        StringBuilder lpBuffer = new StringBuilder(260);    //声明StringBuilder的初始大小
                        int count = FormatMessage(dwFlags, IntPtr.Zero, 1439, 0, lpBuffer, 260, IntPtr.Zero);
                        Console.WriteLine("disconnect fail, ret = " + ret + " info:" + lpBuffer.ToString());
                    }

                }
            }
        }

        public static void disconnect8300()
        {

            List<TcpProcessRecord> tcpClientList = GetAllTcpConnections();
            foreach (TcpProcessRecord tcpRecord in tcpClientList)
            {
                if (tcpRecord.RemotePort == 8300)
                {
                    MIB_TCPROW connToDelete = new MIB_TCPROW();
                    connToDelete.state = MibTcpState.DELETE_TCB;
                    connToDelete.localAddr = (int)tcpRecord.LocalAddress.Address;
                    connToDelete.localPort = htons(tcpRecord.LocalPort);
                    connToDelete.remoteAddr = (int)tcpRecord.RemoteAddress.Address;
                    connToDelete.remotePort = htons(tcpRecord.RemotePort);
                    IntPtr tcprowPtr = GetPtrFromTCPROW(connToDelete);
                    int ret = SetTcpEntry(tcprowPtr);
                    ReleasePtrForTCPROW(tcprowPtr);
                    if (ret != 0)
                    {
                        uint dwFlags = FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS;
                        StringBuilder lpBuffer = new StringBuilder(260);    //声明StringBuilder的初始大小
                        int count = FormatMessage(dwFlags, IntPtr.Zero, 1439, 0, lpBuffer, 260, IntPtr.Zero);
                        Console.WriteLine("disconnect fail, ret = " + ret + " info:" + lpBuffer.ToString());
                    }
                    //Console.WriteLine(String.Format("localaddr:{0},localport:{1},remoteaddr:{2},remoteport:{3}",
                    //    connToDelete.localAddr, connToDelete.localPort, connToDelete.remoteAddr, connToDelete.remotePort));

                    break;
                }
            }

        }


    }
}
