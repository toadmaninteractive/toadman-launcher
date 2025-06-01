using System.Security;
using System.Threading.Tasks;
using WebProtocol;

namespace Toadman.GameLauncher.Core
{
    public interface ISessionProvider
    {
        Task<ClientStatusResponse> VerificationSessionId(string sessionId);

        //login
        Task<ClientLoginResponse> GetSessionIdByCredential(string userName, SecureString securePassword);
        //login
        Task<ClientLoginResponse> GetSessionIdByCredential(string userName, byte[] securePassword);

        //logout
        Task Logout(string sessionId);

        // register
        Task<ClientRegisterResponse> AccountRegistration(string login, string email, SecureString securePassword, string captchaAnswer, string captchaKey);

        //register_confirm
        Task<bool> AccountActivationConfirm(string username, string code);

        Task<ClientPasswordResetResponse> ResetPassword(string username, SecureString securePassword, string captchaAnswer, string captchaKey);

        /// <summary>
        /// Forget
        /// </summary>
        Task<bool> ResetPasswordConfirm(string username, string code);

        Task<ClientPasswordChangeResponse> ChangePassword(string sessionId, SecureString currentPassword, SecureString securePassword);
        Task<GameItemList> DownloadGameInfoList(string sessionId);
        Task<GameManifest> GetGameManifestAsync(string sessionId, string gameGuid, string branchName);
        Task<GenericResponse> ResendRegistrationCode(string username);
        Task<GenericResponse> UnlockGameBranch(string unlockBranchPassword, string gameGuid, string sessionId);
        Task<CaptchaResponse> RequestCaptcha();
    }
}