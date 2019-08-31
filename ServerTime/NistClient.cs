using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BicUtil.ServerTime
{
    public class NistClient
    {
        private Action<bool, DateTime> callback;
        
        public DateTime GetDummyDate()
        {
            return new DateTime(1000, 1, 1); //to check if we have an online date or not.
        }

        public async void GetNetworkTime(int _timeout, Action<bool, DateTime> _callback)
        {
            callback = _callback;

            System.Random ran = new System.Random(DateTime.Now.Millisecond);
            DateTime date = GetDummyDate();
            string serverResponse = string.Empty;

            // Represents the list of NIST servers
            string[] servers = new string[] {
                "time.bora.net",
                "nist1-ny.ustiming.org",
                "time-a.nist.gov",
                "nist1-chi.ustiming.org",
                "time.nist.gov",
                "ntp-nist.ldsbc.edu",
                "nist1-la.ustiming.org",                         
            };

            servers.Shuffle();

            // Try each server in random order to avoid blocked requests due to too frequent request
            bool _isSuccess = false;
            for (int i = 0; i < servers.Length; i++)
            {
                try
                {
                    Debug.Log("Connect " + servers[i]);
                    date = await getDate(servers[i], _timeout * 1000);
                    _isSuccess = true;
                    // Exit the loop
                    break;
                }
                catch (Exception)
                {
                    /* Do Nothing...try the next server */
                }
            }

            callback(_isSuccess, date);
        }

        private async Task<DateTime> getDate(string _server, int _timeout)
        {
            var _tcpClient = new System.Net.Sockets.TcpClient();
            _tcpClient.ReceiveTimeout = _timeout;
            _tcpClient.SendTimeout = _timeout;
            await _tcpClient.ConnectAsync(_server, 13);
            var _stream = _tcpClient.GetStream();
        
            StreamReader _reader = new StreamReader(_stream);
            var _serverResponse = await _reader.ReadToEndAsync();
            _tcpClient.Close();
            _stream.Close();
            _reader.Close();
            
            // Check to see that the signature is there
            if (_serverResponse.Length > 47 && _serverResponse.Substring(38, 9).Equals("UTC(NIST)"))
            {
                // Parse the date
                int jd = int.Parse(_serverResponse.Substring(1, 5));
                int yr = int.Parse(_serverResponse.Substring(7, 2));
                int mo = int.Parse(_serverResponse.Substring(10, 2));
                int dy = int.Parse(_serverResponse.Substring(13, 2));
                int hr = int.Parse(_serverResponse.Substring(16, 2));
                int mm = int.Parse(_serverResponse.Substring(19, 2));
                int sc = int.Parse(_serverResponse.Substring(22, 2));

                if (jd > 51544)
                    yr += 2000;
                else
                    yr += 1999;

                return new DateTime(yr, mo, dy, hr, mm, sc, DateTimeKind.Utc).ToLocalTime();

            }

            throw new SystemException("error");
        }
    }

    static class MyExtensions
    {
        private static System.Random rng = new System.Random();  

        public static void Shuffle<T>(this IList<T> list)  
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
    }
}