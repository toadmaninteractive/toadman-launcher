using Microsoft.Win32;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    [XmlRoot(IsNullable = false)]
    public class Config : BaseConfig<Config>
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public string UserName { get; set; }
        public string Email { get; set; }
        public byte[] SecurePassword { get; set; }
        public bool AutoRun
        {
            get { return GetAutorun(); }
            set { RegistryUpdate(value); }
        }
        
        public void SaveCredential(string sessionId, string userName, byte[] securePassword)
        {
            SessionId = sessionId;
            UserName = userName;
            SecurePassword = securePassword;
            base.Save();
        }

        public override void SessionOut()
        {
            SecurePassword = null;
            base.SessionOut();
        }

        private void RegistryUpdate(bool value)
        {
            RegistryKey autoRunKey = Registry.CurrentUser.OpenSubKey(Constants.RegistryAutoRunPath, true);
            var autoRunKeyExist = autoRunKey.GetValueNames().Any(x => x == Constants.AppName);

#if DEBUG
            var exePath = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%"), "Toadman", "Launcher", $"{Constants.AppName}.exe");
#else
            var exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
#endif

            if (!autoRunKeyExist && value)
                autoRunKey.SetValue(Constants.AppName, $"\"{exePath}\" -silent");
            else if (autoRunKeyExist && !value)
                autoRunKey.DeleteValue(Constants.AppName);

            autoRunKey.Close();
        }

        private bool GetAutorun()
        {
            RegistryKey autoRunKey = Registry.CurrentUser.OpenSubKey(Constants.RegistryAutoRunPath, true);
            var autoRunKeyExist = autoRunKey.GetValueNames().Any(x => x == Constants.AppName);
            autoRunKey.Close();
            return autoRunKeyExist;
        }
    }
}