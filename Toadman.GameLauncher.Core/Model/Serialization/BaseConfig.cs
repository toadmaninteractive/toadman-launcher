namespace Toadman.GameLauncher.Core
{
    public abstract class BaseConfig
    {
        public string SessionId { get; set; }
        public ApplicationUpdateChannel Channel { get; set; }

        public abstract void Save();

        public abstract void SessionOut();
    }

    public abstract class BaseConfig<T> : BaseConfig where T : BaseConfig
    {
        public override void Save() 
        {
            Utils.SerializeAndSave<T>(FolderNames.ConfigXmlFilePath, this);
        }

        public override void SessionOut()
        {
            SessionId = string.Empty;
            Utils.SerializeAndSave<T>(FolderNames.ConfigXmlFilePath, this);
        }
    }
}