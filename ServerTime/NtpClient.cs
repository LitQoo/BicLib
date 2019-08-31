using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BicUtil.ServerTime
{
    public class NtpClient
    {
        public const string NTPORG = "pool.ntp.org";
        public const string WINDOSCOM = "time.windows.com";

        private Socket socket;
        private IPEndPoint ipEndPoint;
        private Action<bool, DateTime> callback;
        private int timeout = 3;

        public async void GetNetworkTime(string _ntpTimeServer, int _timeout, Action<bool, DateTime> _callback)
        {
            Debug.Log("Connect to " + _ntpTimeServer);
            callback = _callback;
            //default Windows time server
            string ntpServer = _ntpTimeServer;
            timeout = _timeout * 1000;
            
            try{
                var addresses = await Dns.GetHostAddressesAsync(ntpServer);
                //The UDP port number assigned to NTP is 123
                ipEndPoint = new IPEndPoint(addresses[0], 123);
                
            }catch{
                callback(false, DateTime.Now);
                return;
            }
            
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            SocketAsyncEventArgs sArgsConnect = new SocketAsyncEventArgs() { RemoteEndPoint = ipEndPoint };

            sArgsConnect.Completed += new EventHandler<SocketAsyncEventArgs>(onCompleteConnect);

            socket.ConnectAsync(sArgsConnect);
        }

        private void onCompleteConnect(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                SocketAsyncEventArgs sArgsSend = new SocketAsyncEventArgs() { RemoteEndPoint = ipEndPoint };
                socket.ReceiveTimeout = timeout;   

                sArgsSend.Completed += new EventHandler<SocketAsyncEventArgs>(onCompleteSend);

                // NTP message size - 16 bytes of the digest (RFC 2030)
                var ntpData = new byte[48];
                //Setting the Leap Indicator, Version Number and Mode values
                ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)
                sArgsSend.SetBuffer(ntpData, 0, ntpData.Length);
                socket.SendAsync(sArgsSend);
            }else{
                callback(false, DateTime.Now);
                socket.Close();
            }
        }

        private void onCompleteSend(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                SocketAsyncEventArgs sArgsRecive = new SocketAsyncEventArgs() { RemoteEndPoint = ipEndPoint };
                sArgsRecive.Completed += new EventHandler<SocketAsyncEventArgs>(onCompleteRecive);
                sArgsRecive.SetBuffer(new byte[48], 0, 48);
                socket.ReceiveAsync(sArgsRecive);
            }else{
                callback(false, DateTime.Now);
                socket.Close();
            }
        }

        private void onCompleteRecive(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                const byte serverReplyTime = 40;
                //Get the seconds part
                ulong intPart = BitConverter.ToUInt32(e.Buffer, serverReplyTime);

                //Get the seconds fraction
                ulong fractPart = BitConverter.ToUInt32(e.Buffer, serverReplyTime + 4);

                //Convert From big-endian to little-endian
                intPart = SwapEndianness(intPart);
                fractPart = SwapEndianness(fractPart);

                var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                //**UTC** time
                var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);
                callback(true, networkDateTime.ToLocalTime());
                socket.Close();
            }else{
                callback(false, DateTime.Now.ToLocalTime());
                socket.Close();
            }
        }

        static uint SwapEndianness(ulong x)
        {
            return (uint) (((x & 0x000000ff) << 24) +
                        ((x & 0x0000ff00) << 8) +
                        ((x & 0x00ff0000) >> 8) +
                        ((x & 0xff000000) >> 24));
        }
    }
}