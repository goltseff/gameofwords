using System.Security.Cryptography;
using System.Text;

namespace gameofwords.common.tools
{
    public static class PasswordHash
    {
        public static string GetPasswordHash(string password, string solt )
            => Convert.ToHexString( SHA1.HashData( Encoding.UTF8.GetBytes( password+solt ) ) );
    }
}
