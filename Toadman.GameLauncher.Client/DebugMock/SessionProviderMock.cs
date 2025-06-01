using System;
using System.Security;
using System.Threading.Tasks;
using Toadman.GameLauncher.Core;
using WebProtocol;

namespace Toadman.GameLauncher.Client
{
    public class SessionProviderMock : ISessionProvider
    {
        //status
        public async Task<ClientStatusResponse> VerificationSessionId(string sessionId)
        {
            await Task.Delay(2000);
            return sessionId == "sessionId"
                ? new ClientStatusResponse { Email = "vladimir@mail.com", LoggedIn = true, UserId = 111, Username = "vladimir" }
                : new ClientStatusResponse { LoggedIn = false };
        }

        //login
        public async Task<ClientLoginResponse> GetSessionIdByCredential(string userName, SecureString securePassword)
        {
            await Task.Delay(1000);
            var pass = Core.Utils.ConvertToUnsecureString(securePassword);

            if (userName == "vladimir" && (pass == "zaq" || pass == "z"))
                return new ClientLoginResponse { Result = true, SessionId = "sessionId" };
            else
            if (userName == "vladimir" && pass == "q")
                return new ClientLoginResponse { Result = false, Error = ClientLoginError.AccountIsBlocked};
            else
                return new ClientLoginResponse { Result = false, Error = ClientLoginError.Failure };
        }

        //logout
        public async Task Logout(string sessionId)
        {
            await Task.Delay(100);
            return;
        }

        // register
        public async Task<ClientRegisterResponse> AccountRegistration(string login, string email, SecureString securePassword, string captchaAnswer, string captchaKey)
        {
            await Task.Delay(1000);

            var response = new ClientRegisterResponse { Result = false, Error = null };

            if (string.IsNullOrWhiteSpace(login))
                response.Error = ClientRegistrationError.InvalidUsername;
            else if (string.IsNullOrWhiteSpace(email))
                response.Error = ClientRegistrationError.InvalidEmail;
            else if (securePassword.Length <= 0)
                response.Error = ClientRegistrationError.InvalidPassword;
            else
                response.Result = true;

            return response;
        }

        //register_confirm
        public async Task<bool> AccountActivationConfirm(string username, string code)
        {
            await Task.Delay(1000);
            return username == "vladimir" && code == "1234";
        }

        public async Task<ClientPasswordResetResponse> ResetPassword(string username, SecureString securePassword, string captchaAnswer, string captchaKey)
        {
            await Task.Delay(1500);
            return string.IsNullOrEmpty(username)
                ? new ClientPasswordResetResponse { Error = ClientPasswordResetError.Failure }
                : new ClientPasswordResetResponse { Result = true };
        }

        /// <summary>
        /// Forget
        /// </summary>
        public async Task<bool> ResetPasswordConfirm(string username, string code)
        {
            await Task.Delay(1500);
            return code == "1234";
        }

        public async Task<ClientPasswordChangeResponse> ChangePassword(string sessionId, SecureString currentPassword, SecureString securePassword)
        {
            await Task.Delay(1500);

            var pass = Core.Utils.ConvertToUnsecureString(currentPassword);

            return sessionId == "sessionId" && pass == "zaq"
                ? new ClientPasswordChangeResponse { Result = true }
                : new ClientPasswordChangeResponse { Error = ClientPasswordChangeError.NotLoggedIn };
        }

        public async Task<GameItemList> DownloadGameInfoList(string sessionId)
        {
            await Task.Delay(1000);
            return FileServerMock.GetGameListMock();
        }

        public async Task<GameManifest> GetGameManifestAsync(string sessionId, string gameGuid, string branchName)
        {
            await Task.Delay(1000);
            return new GameManifest()
            {
                Files = branchName == "master" ? FileServerMock.GetFilesMasterMock() : FileServerMock.GetFilesBetaMock()
            };
        }

        public async Task<GenericResponse> ResendRegistrationCode(string username)
        {
            await Task.Delay(100);
            return new GenericResponse { Result = true };
        }

        public async Task<GenericResponse> UnlockGameBranch(string unlockBranchPassword, string gameGuid, string sessionId)
        {
            var result = new GenericResponse();
            await Task.Delay(1500);

            if (gameGuid == "immortal")
            {
                result.Result = sessionId == "sessionId" && unlockBranchPassword == "a123";
                if (result.Result)
                    FileServerMock.ImmortalBetaLocked = false;
            }
            else if (gameGuid == "chess")
            {
                result.Result = sessionId == "sessionId" && unlockBranchPassword == "b123";
                if (result.Result)
                    FileServerMock.ChessBetaLocked = false;
            }
            else
                result.Result = false;

            return result;
        }

        public Task<CaptchaResponse> RequestCaptcha()
        {
            return new Task<CaptchaResponse>(() => new CaptchaResponse() { Filename = "lol.png" });
        }

        public Task<ClientLoginResponse> GetSessionIdByCredential(string userName, byte[] securePassword)
        {
            throw new NotImplementedException();
        }
    }
}