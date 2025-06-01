using System;
using System.Net.Http;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebProtocol;

namespace Toadman.GameLauncher.Core
{
    public class HeliosApi
    {
        //public static ISessionProvider Provider { get; } = new SessionProviderMock();
        public static ISessionProvider Provider { get; } = new SessionProvider();
    }

    public class SessionProvider : ISessionProvider
    {
        //status
        public async Task<ClientStatusResponse> VerificationSessionId(string sessionId)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                httpClient.DefaultRequestHeaders.Add(Constants.HeaderNameSession, sessionId);
                var api = new HeliosAuthService(httpClient);
                
                return await api.GetClientStatusAsync();
            }
        }

        //login
        public async Task<ClientLoginResponse> GetSessionIdByCredential(string userName, SecureString securePassword)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                var api = new HeliosAuthService(httpClient);

                ClientLoginRequest request = new ClientLoginRequest()
                {
                    Username = userName,
                    Password = Utils.ConvertToUnsecureString(securePassword)
                };
                return await api.LoginClientAsync(request);
            }
        }

        //login
        public async Task<ClientLoginResponse> GetSessionIdByCredential(string userName, byte[] securePassword)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                var api = new HeliosAuthService(httpClient);

                ClientLoginRequest request = new ClientLoginRequest()
                {
                    Username = userName,
                    Password = Encoding.Unicode.GetString(ProtectedData.Unprotect(securePassword, null, DataProtectionScope.CurrentUser))
                };
                return await api.LoginClientAsync(request);
            }
        }

        //logout
        public async Task Logout(string sessionId)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                httpClient.DefaultRequestHeaders.Add(Constants.HeaderNameSession, sessionId);
                var api = new HeliosAuthService(httpClient);
                var result = await api.LogoutPersonnelAsync(new Empty());
            }
        }

        // register
        public async Task<ClientRegisterResponse> AccountRegistration(string login, string email, SecureString securePassword, string captchaAnswer, string captchaKey)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                var api = new HeliosAuthService(httpClient);

                var registerRequest = new ClientRegisterRequest
                {
                    Username = login,
                    Email = email,
                    Password = Utils.ConvertToUnsecureString(securePassword),
                    CaptchaAnswer = captchaAnswer,
                    CaptchaKey = captchaKey

                };
                return await api.RegisterClientAsync(registerRequest);
            }
        }

        //register_confirm
        public async Task<bool> AccountActivationConfirm(string username, string code)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                var api = new HeliosAuthService(httpClient);
                var request = new ClientRegisterConfirmRequest()
                {
                    Username = username,
                    SecurityCode = code
                };
                var response = await api.ConfirmClientRegistrationAsync(request);
                return response.Result;
            }
        }

        public async Task<ClientPasswordResetResponse> ResetPassword(string username, SecureString securePassword, string captchaAnswer, string captchaKey)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                var api = new HeliosAuthService(httpClient);
                var request = new ClientPasswordResetRequest()
                {
                    Username = username,
                    NewPassword = Utils.ConvertToUnsecureString(securePassword),
                    CaptchaAnswer = captchaAnswer,
                    CaptchaKey = captchaKey
                };
                return await api.ResetClientPasswordAsync(request);
            }
        }

        /// <summary>
        /// Forget
        /// </summary>
        public async Task<bool> ResetPasswordConfirm(string username, string code)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                var api = new HeliosAuthService(httpClient);
                var request = new ClientPasswordResetConfirmRequest()
                {
                    Username = username,
                    SecurityCode = code
                };
                var response = await api.ConfirmClientPasswordResetAsync(request);
                return response.Result;
            }
        }

        public async Task<ClientPasswordChangeResponse> ChangePassword(string sessionId, SecureString currentPassword, SecureString securePassword)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                httpClient.DefaultRequestHeaders.Add(Constants.HeaderNameSession, sessionId);
                var api = new HeliosAuthService(httpClient);
                var request = new ClientPasswordChangeRequest()
                {
                    CurrentPassword = Utils.ConvertToUnsecureString(currentPassword),
                    NewPassword = Utils.ConvertToUnsecureString(securePassword)
                };
                return await api.ChangeClientPasswordAsync(request);
            }
        }

        public async Task<GameItemList> DownloadGameInfoList(string sessionId)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                httpClient.DefaultRequestHeaders.Add(Constants.HeaderNameSession, sessionId);
                var api = new HeliosClientService(httpClient);

                return await api.GetGameItemListAsync();
            }
        }

        public async Task<GameManifest> GetGameManifestAsync(string sessionId, string gameGuid, string branchName)
        {
            GameManifest manifest = null;
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                httpClient.DefaultRequestHeaders.Add(Constants.HeaderNameSession, sessionId);
                var api = new HeliosClientService(httpClient);

                manifest = await api.GetGameManifestAsync(gameGuid, branchName);
            }
            return manifest;
        }

        public async Task<GenericResponse> ResendRegistrationCode(string username)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                    var api = new HeliosAuthService(httpClient);

                    return await api.ResendRegistrationCodeAsync(new ClientResendRequest { Username = username });
                }
                catch (Exception)
                {
                    return new GenericResponse { Result = false };
                }
            }
        }

        public async Task<GenericResponse> UnlockGameBranch(string unlockBranchPassword, string gameGuid, string sessionId)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                httpClient.DefaultRequestHeaders.Add(Constants.HeaderNameSession, sessionId);
                var api = new HeliosClientService(httpClient);

                return await api.UnlockGameBranchAsync(new BranchUnlockRequest { Password = unlockBranchPassword }, gameGuid);
            }
        }
        
        public async Task<CaptchaResponse> RequestCaptcha()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Constants.ApiUrl);
                var api = new HeliosAuthService(httpClient);

                return await api.RequestCaptchaAsync();
            }
        }
    }
}