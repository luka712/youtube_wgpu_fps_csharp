using System.Runtime.InteropServices;

namespace FPSGame.Extensions
{
    internal static class StringExtensions
    {
        internal static unsafe byte* ToBytePtr(this string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return null;
            }

            return (byte*)Marshal.StringToHGlobalAnsi(str);
        }
    }
}
