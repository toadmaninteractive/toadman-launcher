namespace Toadman.GameLauncher.Core
{
    public enum GameInstallStatus
    {
        NotPurchased = 0,
        NotInstalled,
        Installed
    }

    public enum GameState
    {
        Non, //Game not installed
        Expired,
        InvalidPermission,
        ReadyToPlay,
        Launched
    }

    public enum GameProcessingPhase
    {
        ManifestUpdating,
        FilesIntegrityChecking,
        Downloading,
        Decompressing,
        Uninstalling,
        Idleness
    }

    public enum FileProcessingStage
    {
        Processing,
        Complete,
    }

    public enum AuthorizationScreen
    {
        Authorization,
        /// <summary>
        /// from login state
        /// </summary>
        ChangePassword
    }

    public enum ConfirmCodeType
    {
        AccountActivation, ResetPassword
    }

    public enum DiffKind
    {
        Valid,
        Add,
        Modify,
        Remove
    }

    public enum ApplicationUpdateChannel
    {
        Stable = 0,
        Beta
    }

    public enum ValidationError
    {
        Non = 0,
        UserNameInvalid,
        EmailInvalid,
        PasswordInvalid,
        RepeatPasswordInvalid,
        InvalidNewPassword,
        PasswordMismatch
    }

    public enum ErrorType
    {
        Disconnect
    }
}