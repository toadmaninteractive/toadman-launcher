using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using NLog;
using System.Diagnostics;

namespace Toadman.GameLauncher.Core
{
    public static class Utils
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// From PasswordBox
        /// </summary>
        public static string ConvertToUnsecureString(SecureString secureString)
        {
            if (secureString == null)
                return string.Empty;

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static bool SecureStringEqual(SecureString secureString1, SecureString secureString2)
        {
            if (secureString1 == null)
                throw new ArgumentNullException("s1");
            if (secureString2 == null)
                throw new ArgumentNullException("s2");

            if (secureString1.Length != secureString2.Length)
                return false;

            IntPtr ss_bstr1_ptr = IntPtr.Zero;
            IntPtr ss_bstr2_ptr = IntPtr.Zero;

            try
            {
                ss_bstr1_ptr = Marshal.SecureStringToBSTR(secureString1);
                ss_bstr2_ptr = Marshal.SecureStringToBSTR(secureString2);
             
                string str1 = Marshal.PtrToStringBSTR(ss_bstr1_ptr);
                string str2 = Marshal.PtrToStringBSTR(ss_bstr2_ptr);

                return str1.Equals(str2);
            }
            finally
            {
                if (ss_bstr1_ptr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(ss_bstr1_ptr);

                if (ss_bstr2_ptr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(ss_bstr2_ptr);
            }
        }

        public static string GetOsInfo()
        {
            OperatingSystem os_info = Environment.OSVersion;
            string version =
                os_info.Version.Major.ToString() + "." +
                os_info.Version.Minor.ToString();

            switch (version)
            {
                case "10.0": return "10/Server 2016";
                case "6.3": return "8.1/Server 2012 R2";
                case "6.2": return "8/Server 2012";
                case "6.1": return "7/Server 2008 R2";
                case "6.0": return "Server 2008/Vista";
                case "5.2": return "Server 2003 R2/Server 2003/XP 64-Bit Edition";
                case "5.1": return "XP";
                case "5.0": return "2000";
            }

            return "Unknown";
        }

        public static bool CheckEdgeBrowser()
        {
            using (var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\PackageRepository\Packages\"))
                if (key != null)
                    foreach (var subkey in key.GetSubKeyNames())
                        if (subkey.StartsWith("Microsoft.MicrosoftEdge_"))
                            return true;

            return false;
        }

        public static T Deserialize<T>(Stream stream) where T : class
        {
            var writerSettings = new XmlReaderSettings();
            var serializer = new XmlSerializer(typeof(T));

            using (var reader = XmlReader.Create(stream, writerSettings))
                return (T)serializer.Deserialize(reader);
        }

        public static void SerializeAndSave<T>(string filePath, object serializeObject) where T : class
        {
            var writerSettings = new XmlWriterSettings();
            writerSettings.NewLineChars = "\r\n";
            writerSettings.Indent = true;
            writerSettings.IndentChars = "\t";
            writerSettings.Encoding = new UTF8Encoding(false);

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var writer = XmlWriter.Create(filePath, writerSettings))
                    serializer.Serialize(writer, serializeObject);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        /// <summary>
        /// Load exist or create new Config file
        /// </summary>
        public static T LoadConfig<T>() where T : class
        {
            T result = null;

            try
            {
                if (File.Exists(FolderNames.ConfigXmlFilePath))
                    using (FileStream fsSource = new FileStream(FolderNames.ConfigXmlFilePath, FileMode.Open, FileAccess.Read))
                        result = Deserialize<T>(fsSource);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }

            if (result == null)
            {
                result = (T)Activator.CreateInstance(typeof(T));
                SerializeAndSave<T>(FolderNames.ConfigXmlFilePath, result);
            }

            return result;
        }

        public static Library LoadLibrary()
        {
            Library result = null;

            try
            {
                if (File.Exists(FolderNames.LibraryXmlFilePath))
                    using (FileStream fsSource = new FileStream(FolderNames.LibraryXmlFilePath, FileMode.Open, FileAccess.Read))
                        result = Deserialize<Library>(fsSource);
            }
            catch (Exception)
            {
                //TODO logger
            }

            if (result == null)
            {
                result = new Library();
                SerializeAndSave<Library>(FolderNames.LibraryXmlFilePath, result);
            }

            return result;
        }

        public static long GetFilesSize(string directoryPath, string filter = "*.*")
        {
            string[] a = Directory.GetFiles(directoryPath, filter, SearchOption.AllDirectories);
            long b = 0;
            foreach (string name in a)
            {
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }

            return b;
        }

        public static InterruptedProcessLib LoadInterruptedProcess()
        {
            InterruptedProcessLib result = null;

            try
            {
                if (File.Exists(FolderNames.InterruptedProcessXmlFilePath))
                    using (FileStream fsSource = new FileStream(FolderNames.InterruptedProcessXmlFilePath, FileMode.Open, FileAccess.Read))
                        result = Deserialize<InterruptedProcessLib>(fsSource);
            }
            catch (Exception)
            {
                //TODO logger
            }

            if (result == null)
            {
                result = new InterruptedProcessLib();
                SerializeAndSave<InterruptedProcessLib>(FolderNames.InterruptedProcessXmlFilePath, result);
            }

            return result;
        }

        public static string UrlEncode(string urlOrigin)
        {
            var urlDecode = new List<string>();

            foreach (var urlPart in urlOrigin.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
                urlDecode.Add(HttpUtility.UrlEncode(urlPart));

            return string.Join("/", urlDecode);
        }

        /// <summary>
        /// Read current Launcher revision from local file
        /// </summary>
        public static string GetCurrentRevision()
        {
            var revFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\rev.conf";
            return File.Exists(revFilePath) ? File.ReadAllText(revFilePath).Trim() : null;
        }

        public static bool CheckFileArchive(string localCompressedName, long compressedSize)
        {
            if (File.Exists(localCompressedName))
            {
                var info = new FileInfo(localCompressedName);
                return compressedSize == info.Length;
            }

            return false;
        }

        public static string GetAppVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = FileVersionInfo.GetVersionInfo(assembly.Location);
            return $"{version.FileMajorPart}.{version.FileMinorPart}.{version.FileBuildPart}";
        }

        public static async Task<bool> CheckForInternetConnectionSafeAsync()
        {
            var result = await Task.Run(() =>
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create($"{Constants.ApiUrl}api/auth/client/status");
                    request.Timeout = 1000; //Timeout after 1000 ms
                    using (var stream = request.GetResponse().GetResponseStream())
                        return true;
                }
                catch
                {
                    return false;
                }
            });

            return result;
        }
    }
}