using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public static class InternetTime
{
    public static async void GetTime(Action<bool, DateTime> _callback, int _timeout = 3){
        var _pages = new string[]{
            "https://www.google.com/robots.txt",
            "http://www.microsoft.com/robots.txt",
            "https://aws.amazon.com/robots.txt",
            "https://www.tmall.com/robots.txt"
        };

        _pages.Shuffle();

        for(int i = 0; i < _pages.Length; i++){
            try{
                var _date = await GetCurrentTime(_pages[i], _timeout);
                _callback(true, _date);
                return;
            }catch{

            }
        }

        _callback(false, DateTime.Now);

    }

    private static async Task<DateTime> GetCurrentTime(string _page, int _timeout)
    {
        Debug.Log("Connect " + _page);

        try{
            var _request = WebRequest.Create(_page);
            _request.Timeout = _timeout;
            using (var _response = await _request.GetResponseAsync()){
                var _date = DateTime.ParseExact(_response.Headers["date"],
                    "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                    CultureInfo.InvariantCulture.DateTimeFormat,
                    DateTimeStyles.AssumeUniversal).ToLocalTime();
                return _date;
            }
        }
        catch (WebException)
        {
        }

        throw new SystemException("not found time");

        
    }
}