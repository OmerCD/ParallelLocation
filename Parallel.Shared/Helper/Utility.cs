using System;
using System.Text;

namespace Parallel.Shared.Helper
{
    public static class Utility
    {
        public static string GetBasicAuthToken(string userName, string password)
        {
            return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"));
        }
    }
}