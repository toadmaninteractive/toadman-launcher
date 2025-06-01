using Protocol;
using System.Xml.Serialization;
using Toadman.GameLauncher.Core;

namespace Toadman.Bloodties.Launcher
{
    [XmlRoot(IsNullable = false)]
    public class Config : BaseConfig<Config>
    {
        public Locale Language { get; set; }
    }
}