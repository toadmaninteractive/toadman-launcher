using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Toadman.GameLauncher.Core
{
    public class ChronosApi : IDisposable
    {
        public readonly string server;
        public readonly int port;

        public readonly string project = "chronos";
        public readonly string branch = "dev";
        public readonly string appType = "client";
        private readonly string logs = "logs";

        private Socket socket;
        private string chronosTag;
        private string[] logLevel = { "error", "trace", "warning", "info", "fatal", "debug" };

        private StringFast json = new StringFast(256);
        private StringFast metadata = new StringFast(256);
        private Encoder encoder = Encoding.UTF8.GetEncoder();
        private byte[] buffer = new byte[256];

        public static ChronosApi Instance;

        public static void Init(string project, string branch, string appType = "client", string server = "fluent.yourcompany.com", int port = 24224)
        {
            if (Instance == null)
                Instance = new ChronosApi(project, branch, appType, server, port);
        }

        ChronosApi(string project, string branch, string appType, string server, int port)
        {
            this.project = project;
            this.branch = branch;
            this.appType = project;

            this.server = server;
            this.port = port;

            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            chronosTag = new StringFast().Append(project).Append(".").Append(logs).Append(".").Append(appType).Append(".").Append(branch).ToString();

            socket = ConnectSocket(server, port);
        }

        Socket ConnectSocket(string server, int port)
        {
            IPHostEntry hostEntry;
            hostEntry = Dns.GetHostEntry(server);

            Socket sock = null;

            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, port);
                sock = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sock.Connect(ipe);

                if (sock.Connected)
                {
                    socket = sock;
                    break;
                }
                else
                {
                    continue;
                }
            }
            return sock;
        }

        public void Dispose()
        {
            if (socket != null)
            {
                socket.Disconnect(false);
                socket.Close();
            }
        }

        public static void Log(string message, LogLevel logLv = LogLevel.info)
        {
            Instance.metadata.Append("{}");
            Instance.SendLog(message, Instance.metadata, Instance.logLevel[(int)logLv]);
        }

        public static void Log(string message, Dictionary<string, string> metadata, LogLevel logLv = LogLevel.info)
        {
            Instance.metadata.Append("{");
            int i = 0;
            foreach (var keyValue in metadata)
            {
                Instance.metadata.Append("\\\"").Append(keyValue.Key).Append("\\\":\\\"").Append(keyValue.Value).Append("\\\"");
                if (++i < metadata.Count) Instance.metadata.Append(",");
            }
            Instance.metadata.Append("}");
            Instance.SendLog(message, Instance.metadata, Instance.logLevel[(int)logLv]);
        }

        void SendLog(string logString, string metadata, string logLevel)
        {
            Instance.metadata.Append(metadata);
            SendLog(logString, Instance.metadata, logLevel);
        }
        void SendLog(string logString, StringFast metadata, string logLevel)
        {
            double timeStamp = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            json.Append("[\"").Append(chronosTag).Append("\",")
                .Append((int)timeStamp)
                .Append(",{\"timestamp\":").Append(timeStamp)
                .Append(",\"level\":\"").Append(logLevel)
                .Append("\",\"metadata\":\"").Append(metadata)
                .Append("\",\"message\":\"").Append(logString).Append("\"}]");

            //string json = "[\"chronos.logs.client.dev\", 10000000, {\"timestamp\": 10000000.5, \"level\": \"info\", \"metadata\": {}, \"message\": \"this is the log message\"}]";

            if (buffer.Length < json.GetLenght())
                buffer = new byte[json.GetLenght()];

            byte[] array = json.GetBytes(encoder, buffer);

            if (socket != null && socket.Connected)
                socket.Send(array, json.GetLenght(), 0);

            json.Clear();
            metadata.Clear();
        }

        public enum LogLevel
        {
            error,
            trace,
            warning,
            info,
            fatal,
            debug
        }
    }
}