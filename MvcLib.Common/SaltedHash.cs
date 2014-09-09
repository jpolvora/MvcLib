using System;
using System.Security.Cryptography;
using System.Text;

namespace MvcLib.Common
{
    public interface IHashProvider
    {
        void GetHashAndSalt(byte[] data, out byte[] hash, out byte[] salt);
        void GetHashAndSaltString(string data, out string hash, out string salt);
        bool VerifyHash(byte[] data, byte[] hash, byte[] salt);
        bool VerifyHashString(string data, string hash, string salt);
    }

    /// <summary>
    /// Thank you Martijn
    /// http://www.dijksterhuis.org/creating-salted-hash-values-in-c/
    /// </summary>
    public class SaltedHash : IHashProvider
    {
        static readonly SaltedHash ShInstance = new SaltedHash();
        public Encoding Encoding { get; set; }

        public static SaltedHash Instance
        {
            get { return ShInstance; }
        }

        readonly HashAlgorithm _hashProvider;
        readonly int _salthLength;

        private SaltedHash() : this(new SHA256Managed(), 4) { }

        private SaltedHash(HashAlgorithm hashAlgorithm, int theSaltLength)
        {
            _hashProvider = hashAlgorithm;
            _salthLength = theSaltLength;

            Encoding = Encoding.UTF8; //configuração
        }

        private byte[] ComputeHash(byte[] data, byte[] salt)
        {
            var dataAndSalt = new byte[data.Length + _salthLength];
            Array.Copy(data, dataAndSalt, data.Length);
            Array.Copy(salt, 0, dataAndSalt, data.Length, _salthLength);

            return _hashProvider.ComputeHash(dataAndSalt);
        }

        public void GetHashAndSalt(byte[] data, out byte[] hash, out byte[] salt)
        {
            salt = new byte[_salthLength];

            var random = new RNGCryptoServiceProvider();
            random.GetNonZeroBytes(salt);

            hash = ComputeHash(data, salt);
        }

        public void GetHashAndSaltString(string data, out string hash, out string salt)
        {
            byte[] hashOut;
            byte[] saltOut;

            GetHashAndSalt(Encoding.GetBytes(data), out hashOut, out saltOut);

            hash = Convert.ToBase64String(hashOut);
            salt = Convert.ToBase64String(saltOut);
        }

        /// <summary>
        /// Hash, Salt as string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Tuple<string, string> GetHashAndSaltString(string data)
        {
            byte[] hashOut;
            byte[] saltOut;

            GetHashAndSalt(Encoding.GetBytes(data), out hashOut, out saltOut);
            return new Tuple<string, string>(Convert.ToBase64String(hashOut), Convert.ToBase64String(saltOut));
        }

        public bool VerifyHash(byte[] data, byte[] hash, byte[] salt)
        {
            var newHash = ComputeHash(data, salt);

            if (newHash.Length != hash.Length) return false;

            for (int lp = 0; lp < hash.Length; lp++)
                if (!hash[lp].Equals(newHash[lp]))
                    return false;

            return true;
        }

        public bool VerifyHashString(string input, string storedHash, string storedSalt)
        {
            byte[] hashToVerify = Convert.FromBase64String(storedHash);
            byte[] saltToVerify = Convert.FromBase64String(storedSalt);
            byte[] dataToVerify = Encoding.GetBytes(input);
            return VerifyHash(dataToVerify, hashToVerify, saltToVerify);
        }

        #region helper

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();  // SHA1.Create()
            return algorithm.ComputeHash(Instance.Encoding.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        #endregion
    }
}