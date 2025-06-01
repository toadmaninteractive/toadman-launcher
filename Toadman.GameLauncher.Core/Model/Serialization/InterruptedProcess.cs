using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Toadman.GameLauncher.Core
{
    [XmlRoot(IsNullable = false)]
    public class InterruptedProcessLib
    {
        [XmlArrayItem("InterruptedProcess", Type = typeof(InterruptedProcessItem))]
        public List<InterruptedProcessItem> InterruptedProcesses { get; set; }

        public InterruptedProcessLib()
        {
            if (InterruptedProcesses == null)
            InterruptedProcesses = 
                new List<InterruptedProcessItem>();
        }

        public void Clear()
        {
            InterruptedProcesses.Clear();
            Utils.SerializeAndSave<InterruptedProcessLib>(FolderNames.InterruptedProcessXmlFilePath, this);
        }

        public void Add(string gameGuid, string localPath)
        {
            if (InterruptedProcesses.Any(x => x.GameGuid == gameGuid))
                return;

            InterruptedProcesses.Add(
                new InterruptedProcessItem
                {
                    GameGuid = gameGuid,
                    LocalPath = localPath
                });
            Utils.SerializeAndSave<InterruptedProcessLib>(FolderNames.InterruptedProcessXmlFilePath, this);
        }

        public void Remove(string gameGuid)
        {
            var item = InterruptedProcesses.SingleOrDefault(x => x.GameGuid == gameGuid);
            if (item == null)
                return;

            InterruptedProcesses.Remove(item);
            Utils.SerializeAndSave<InterruptedProcessLib>(FolderNames.InterruptedProcessXmlFilePath, this);
        }
    }
}