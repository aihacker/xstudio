using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;

namespace xstudio
{
    public class RSA
    {
/*
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCBXjdBBo7uZ2gjOTYy3RXd6FMnw3i4C4AY0/ui7YX2ooLy8+NfWW0xXGqDCiu2L7RQKgNdkMcytS7lYjaVXzsbBhB2e7Tvb+IlkG0hO5+7wS80vQ8D8nhiogOSP9rEQAVhgcD2a/1HLG+pHKZveLOmalyhtKvsH16NVexfCfinUwIDAQAB
MIICdwIBADANBgkqhkiG9w0BAQEFAASCAmEwggJdAgEAAoGBAIFeN0EGju5naCM5NjLdFd3oUyfDeLgLgBjT+6LthfaigvLz419ZbTFcaoMKK7YvtFAqA12QxzK1LuViNpVfOxsGEHZ7tO9v4iWQbSE7n7vBLzS9DwPyeGKiA5I/2sRABWGBwPZr/Ucsb6kcpm94s6ZqXKG0q+wfXo1V7F8J+KdTAgMBAAECgYAmxEv8gXGdgYFUZNWYAmaGHBOnK81mIZQeXI/gsBrf4K0rDujI7uxoyU/lusuEieEX0K83f6YhzOejt32x31q/fNQXsfMfmh+uPWlSZQy229TPcPrzLJDqazQrAOcIOjKifOWkiKxOVyMgCYzG5iosR4q7xpaxXmSfGCqmjtHQ6QJBANZxDAi5vGh0uRL1ekKEXFghvTIpiFRNI58HWaerytsYrpC+yUYc2Pzm5THVWpRWud/Hja+qpWTmmtcxfjVgH0UCQQCacHgLXbnS3Y/GgoNebcUaxFk2UYEZVpNch67S14F3cxV33rBODbOocYi9xOnxCEezaJ+s6Apft42XdhTjI2m3AkEAlgcTT0t7CG2ZSi1aMw1def9o2Z57Fde+MzW2QPuM+gpjnzsLoDTwjseP1HSbYarnciuv8hXmjxhTfnjO/tLYLQJAQ5qX8eHFRhjWpv7aoqtKbL0mkDB9YqoTN53tWT4c3jzyWNaSNpio3ENWqDtabLhDKrXRr86jO+MNiA+YdRU7YQJBALwylrue17cPHpeu5lK88mGivef2E0XS6mkSVk5imKTSTc+D8iWP5ybh+nBPrkxkupuFCp2gGRDIEOo3RRMR7Hs=
*/
        private static AsymmetricKeyParameter _privateKeyParameter;
        public static AsymmetricKeyParameter PrivateKeyParameter
        {
            get
            {
                if (_privateKeyParameter == null)
                {
                    lock (typeof (RSA))
                    {
                        if (_privateKeyParameter == null)
                        {
                            _privateKeyParameter =
                                PrivateKeyFactory.CreateKey(
                                    Convert.FromBase64String(
                                        "MIICdwIBADANBgkqhkiG9w0BAQEFAASCAmEwggJdAgEAAoGBAIFeN0EGju5naCM5NjLdFd3oUyfDeLgLgBjT+6LthfaigvLz419ZbTFcaoMKK7YvtFAqA12QxzK1LuViNpVfOxsGEHZ7tO9v4iWQbSE7n7vBLzS9DwPyeGKiA5I/2sRABWGBwPZr/Ucsb6kcpm94s6ZqXKG0q+wfXo1V7F8J+KdTAgMBAAECgYAmxEv8gXGdgYFUZNWYAmaGHBOnK81mIZQeXI/gsBrf4K0rDujI7uxoyU/lusuEieEX0K83f6YhzOejt32x31q/fNQXsfMfmh+uPWlSZQy229TPcPrzLJDqazQrAOcIOjKifOWkiKxOVyMgCYzG5iosR4q7xpaxXmSfGCqmjtHQ6QJBANZxDAi5vGh0uRL1ekKEXFghvTIpiFRNI58HWaerytsYrpC+yUYc2Pzm5THVWpRWud/Hja+qpWTmmtcxfjVgH0UCQQCacHgLXbnS3Y/GgoNebcUaxFk2UYEZVpNch67S14F3cxV33rBODbOocYi9xOnxCEezaJ+s6Apft42XdhTjI2m3AkEAlgcTT0t7CG2ZSi1aMw1def9o2Z57Fde+MzW2QPuM+gpjnzsLoDTwjseP1HSbYarnciuv8hXmjxhTfnjO/tLYLQJAQ5qX8eHFRhjWpv7aoqtKbL0mkDB9YqoTN53tWT4c3jzyWNaSNpio3ENWqDtabLhDKrXRr86jO+MNiA+YdRU7YQJBALwylrue17cPHpeu5lK88mGivef2E0XS6mkSVk5imKTSTc+D8iWP5ybh+nBPrkxkupuFCp2gGRDIEOo3RRMR7Hs="));
                        }
                    }
                }
                return _privateKeyParameter;
            }
        }
        public static byte[] Encrypt(byte[] content)
        {
            IAsymmetricBlockCipher engine = new Pkcs1Encoding(new RsaEngine());
            engine.Init(true, PrivateKeyParameter);

            var length = content.Length;
            var offset = 0;
            using (var stream = new MemoryStream())
            {
                while (length - offset > 0)
                {
                    var bytes = length - offset > 117
                        ? engine.ProcessBlock(content, offset, 117)
                        : engine.ProcessBlock(content, offset, length - offset);
                    offset += 117;
                    stream.Write(bytes, 0, bytes.Length);
                }

                return stream.ToArray();
            }
        }

        public static string Encrypt(string content)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(content)));
        }

        public static byte[] Decrypt(byte[] content)
        {
            IAsymmetricBlockCipher engine = new Pkcs1Encoding(new RsaEngine());
            engine.Init(false, PrivateKeyParameter);

            var length = content.Length;
            var offset = 0;
            using (var stream = new MemoryStream())
            {
                while (length - offset > 0)
                {
                    var bytes = length - offset > 128
                        ? engine.ProcessBlock(content, offset, 128)
                        : engine.ProcessBlock(content, offset, length - offset);
                    offset += 128;
                    stream.Write(bytes, 0, bytes.Length);
                }
                return stream.ToArray();
            }
        }

        public static string Decrypt(string content)
        {
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(content)));
        }
    }
}