// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("ysjszQraf3jA798yv9WDElHuVoXsp0vnn76Du8mUBCs7tihX8pm9L4xdFjhpyBwTk0Rwub+vOD1o46rfAFxZrcXdupOyMlGF5G5dJjIAgo+8Do2uvIGKhaYKxAp7gY2NjYmMj3TMQh3pMmtMRBvQQ7MNYc3Fkifu6yH5L2sRdApUS7ysPlzHAnhCjHTO+VMGRtXsXICcTv8523rCsisFFg6Ng4y8Do2Gjg6NjYwyN4Y75MectmTfrKLsCpVVSqtVGSZEpc9r9Em9NjIpIfZfuVunpY7Vks79svYD7k9qDbM24YtaGQAu/MmI7dic3q8lZW+WTXGcJZwgYBiLSA/1l0dERT2+hJXU9+rs703PXGN8kbyHZrSO8B25AAs9VMzKDY6PjYyN");
        private static int[] order = new int[] { 4,10,8,8,4,8,6,12,10,10,13,11,12,13,14 };
        private static int key = 140;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
