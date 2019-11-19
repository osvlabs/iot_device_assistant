using System;
using System.Collections.Generic;
using System.IO; 
using System.Security.Cryptography;
using System.Text; 
using System.Diagnostics;
using System.IO.Compression;
using System.Configuration;
using System.Runtime.InteropServices;

namespace DownloadApp
{
    public static class Tool
    {
        static byte[] _iv = new byte[16];

        public static bool DecryptFile(string inputFile, string outputFile, string password, string hash) {

            try
            {
                System.Threading.Thread.Sleep(2000);
                try
                {
                    if (File.Exists(outputFile)) {
                        File.Delete(outputFile);
                    }
                }
                catch { }

                FileStream fsNew = File.Create(outputFile);
                
                using (FileStream fs = File.Open(inputFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    int trunkSize = 16 * (1000 + 1);
                    byte[] b = new byte[trunkSize];

                    fs.Read(_iv, 0, 16);

                    while (fs.Position < fs.Length)
                    {
                        var remainlength = fs.Length - fs.Position;
                        if (remainlength > trunkSize)
                        {
                            fs.Read(b, 0, trunkSize);
                        }else{
                            b = new byte[(int)remainlength];
                            fs.Read(b, 0, (int)remainlength);
                        }
                        byte[] tmp = DecryptBytes(b, password);
                        fsNew.Write(tmp, 0, tmp.Length);
                    }
                    
                    fsNew.Close();
                    fs.Close();
                    var downloadHash = GetMD5HashFromFile(outputFile);
                    if (downloadHash == hash)
                    {
                        try
                        {
                            File.Delete(inputFile);
                        }
                        catch { }

                        return true;
                    }
                    else {
                        try
                        {
                            File.Move(outputFile, outputFile + ".failed");
                        }
                        catch { }
                        return false;
                    } 
                }
            }
            catch(Exception ex)
            {
                var message = ex.Message;
                Tool.Log("解密失败，原文件："+inputFile+",目标文件："+outputFile);
                Tool.Log("解密失败原因：" + ex.Message + ",内部堆栈信息："+ex.StackTrace.ToString());
                return false;
            }
        }

        // static string _password = "3sc3RLrpd17";
        // Create secret IV
        
        // { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

        public static byte[] DecryptBytes(byte[] cipherBytes, string password)
        {
            // Create sha256 hash
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(password));

            byte[] iv = _iv;

            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;

            // Set key and IV
            byte[] aesKey = new byte[32];
            Array.Copy(key, 0, aesKey, 0, 32);
            encryptor.Key = aesKey;
            encryptor.IV = iv;

            //encryptor.Padding = PaddingMode.None;

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our Aes object
            ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);

            // Will contain decrypted plaintext
            // string plainText = String.Empty;

            try
            {
                // Convert the ciphertext string into a byte array
                // byte[] cipherBytes = Convert.FromBase64String(cipherText);

                // Decrypt the input ciphertext string
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                // Complete the decryption process
                cryptoStream.FlushFinalBlock();

                new MemoryStream(cipherBytes).Read(_iv, 0, 16);

                // Convert the decrypted data from a MemoryStream to a byte array
                byte[] plainBytes = memoryStream.ToArray();
                return plainBytes;
                // Convert the decrypted byte array to string
                // plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
            }
            catch
            {
                return new byte[0];
            }
            finally
            {
                // Close both the MemoryStream and the CryptoStream
                memoryStream.Close();
                cryptoStream.Close();
            }

            // Return the decrypted data as a string

        }

        public static byte[] EncryptString(byte[] plainBytes, string password)
        {
            // Create sha256 hash
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(password)); 

            byte[] iv = _iv;

            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;

            // Set key and IV
            byte[] aesKey = new byte[32];
            Array.Copy(key, 0, aesKey, 0, 32);
            encryptor.Key = aesKey;
            encryptor.IV = iv;

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our Aes object
            ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write); 

            // Encrypt the input plaintext string
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);

            // Complete the encryption process
            cryptoStream.FlushFinalBlock();

            // Convert the encrypted data from a MemoryStream to a byte array
            byte[] cipherBytes = memoryStream.ToArray();

            new MemoryStream(cipherBytes).Write(_iv, 0, 16);
            
            // Close both the MemoryStream and the CryptoStream
            memoryStream.Close();
            cryptoStream.Close(); 

            // Return the encrypted data as a string
            return cipherBytes;
        }


        public static void WriteFile(string fileName, string content)
        { 
            if (File.Exists(fileName)) {
                File.Delete(fileName);
            }
            FileStream fs = new FileStream(fileName, FileMode.Create);
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            fs.Write(bytes, 0, (int)bytes.Length);
            fs.Flush();
            fs.Close();
        }

        public static string ReadFile(string filename) {
            string result = "";
            if (File.Exists(filename)) { 
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] info = new byte[fs.Length];
                fs.Read(info, 0, info.Length);
                result = Encoding.UTF8.GetString(info); 
                fs.Flush();
                fs.Close();
            }
            return result;
        }

        public static string GetMd5Hash( string input )
        {
            MD5 md5Hash = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static IEnumerable<byte[]> ReadChunks(string path)
        {
            var lengthBytes = new byte[7296];

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int n = fs.Read(lengthBytes, 0, sizeof(int));  // Read block size.

                if (n == 0)      // End of file.
                    yield break;

                if (n != sizeof(int))
                    throw new InvalidOperationException("Invalid header");

                int blockLength = BitConverter.ToInt32(lengthBytes, 0);
                var buffer = new byte[blockLength];
                n = fs.Read(buffer, 0, blockLength);

                if (n != blockLength)
                    throw new InvalidOperationException("Missing data");

                yield return buffer;
            }
        }
        
        public static void Log(string message) { 
            try
            {
                string logPath = GlobalObj.logPath;
                string fileName = "Log-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                string pathString = Path.Combine(logPath, fileName);
                
                string txt1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : " + message + " \r\n";
                File.AppendAllText(pathString, txt1); 
            }
            catch
            {} 
        }

        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }

        public static bool Unzip(string zipFile, string unzipPath) {
            try {
                if ( Directory.Exists(unzipPath) ) {
                    Directory.Delete(unzipPath, true);
                }
                ZipFile.ExtractToDirectory(zipFile, unzipPath);
                return true;
            }
            catch {
                return false;
            }
        }


        /// <summary>
        /// 创建结构体用于返回捕获时间
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            /// <summary>
            /// 设置结构体块容量
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;

            /// <summary>
            /// 抓获的时间
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        /// <summary>
        /// 获取键盘和鼠标没有操作的时间
        /// </summary>
        /// <returns>用户上次使用系统到现在的时间间隔，单位为秒</returns>
        public static long GetLastInputTime()
        {
            LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
            if (!GetLastInputInfo(ref vLastInputInfo))
            {
                return 0;
            }
            else
            {
                var count = Environment.TickCount - (long)vLastInputInfo.dwTime;
                var icount = count / 1000;
                return icount;
            }
        }


    }
}
