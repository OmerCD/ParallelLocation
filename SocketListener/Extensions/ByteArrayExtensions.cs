using System;
using System.Collections.Generic;

namespace SocketListener.Extensions
{
    public static class ByteArrayExtensions
    {
        public static unsafe int IndexOf(this byte[] haystack, byte[] needle, int startIndex = 0)
        {
            try
            {
                fixed (byte* h = haystack)
                fixed (byte* n = needle)
                {
                    int i = startIndex;
                    for (byte* hNext = h + startIndex, hEnd = h + haystack.LongLength; hNext < hEnd; i++, hNext++)
                    {
                        var found = true;
                        for (byte* hInc = hNext, nInc = n, nEnd = n + needle.LongLength;
                            found && nInc < nEnd;
                            found = *nInc == *hInc, nInc++, hInc++) ;
                        if (found) return i;
                    }

                    return -1;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static unsafe List<long> IndexesOf(this byte[] haystack, byte[] needle)
        {
            var indexes = new List<long>();
            fixed (byte* h = haystack) fixed (byte* n = needle)
            {
                long i = 0;
                for (byte* hNext = h, hEnd = h + haystack.LongLength; hNext < hEnd; i++, hNext++)
                {
                    var found = true;
                    for (byte* hInc = hNext, nInc = n, nEnd = n + needle.LongLength; found && nInc < nEnd; found = *nInc == *hInc, nInc++, hInc++) ;
                    if (found) indexes.Add(i);
                }
                return indexes;
            }
        }
    }
}