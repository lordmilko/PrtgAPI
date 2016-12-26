using System;
using System.Threading.Tasks;

namespace PrtgAPI
{
    internal interface IWebClient
    {
        string DownloadString(string address);

        Task<string> DownloadStringTaskAsync(string address);
    }
}