using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Security;
using System.Windows.Media.Animation;
using WebProtocol;
using NLog;
using Toadman.GameLauncher.Core;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using ValidationError = Toadman.GameLauncher.Core.ValidationError;
using System.Security.Cryptography;
using System.Text;

namespace Toadman.GameLauncher.Client
{
    /// <summary>
    /// Interaction logic for AuthorizationView.xaml
    /// </summary>
    public partial class AuthorizationView : MetroWindow
    {
        public AuthorizationViewModel AuthorizationVM { get; set; }
        public RegistrationViewModel RegistrationVM { get; set; }
        public ResetPasswordViewModel ResetPasswordVM { get; set; }
        public ChangePasswordViewModel ChangePasswordVM { get; set; }
        public CodeConfirmViewModel CodeConfirmVM { get; set; }
        
        private enum GridNames { AuthorizationGrid, RegistrationGrid, ChangePasswordGrid, CodeConfirmGrid, ResetPasswordGrid }


        private Dictionary<ValidationError, string> ValidationErrorTexts = new Dictionary<ValidationError, string>();

        private Dictionary<GridNames, string> gridNames = new Dictionary<GridNames, string>
        {
            { GridNames.AuthorizationGrid, "AuthorizationGrid" },
            { GridNames.ChangePasswordGrid, "ChangePasswordGrid" },
            { GridNames.CodeConfirmGrid, "CodeConfirmGrid" },
            { GridNames.RegistrationGrid, "RegistrationGrid" },
            { GridNames.ResetPasswordGrid, "ResetPasswordGrid" }
        };

        // +30
        private Dictionary<GridNames, double> gridHeights = new Dictionary<GridNames, double>
        {
            { GridNames.AuthorizationGrid, 420 },
            { GridNames.RegistrationGrid, 615 },
            { GridNames.ChangePasswordGrid, 420 },
            { GridNames.ResetPasswordGrid, 580 },
            { GridNames.CodeConfirmGrid, 310 }
        };

        private Dictionary<GridNames, SingUpBaseViewModel> gridModels = new Dictionary<GridNames, SingUpBaseViewModel>();
        private Logger logger = LogManager.GetCurrentClassLogger();
        private GridNames currentGrid;
        
        public AuthorizationView(AuthorizationScreen screen)
        {
            InitializeComponent();

            AuthorizationVM = new AuthorizationViewModel
            {
                ToRegistrationCommand = new RelayCommand(() => NavigationTo(GridNames.RegistrationGrid)),
                ForgotPasswordCommand = new RelayCommand(() => NavigationTo(GridNames.ResetPasswordGrid)),
                LoginCommand = new AsyncCommand(Authorization)
            };

            RegistrationVM = new RegistrationViewModel
            {
                RegistrationCommand = new AsyncCommand(Registration),
                ToBackCommand = new RelayCommand(() => NavigationTo(GridNames.AuthorizationGrid))
            };

            ResetPasswordVM = new ResetPasswordViewModel
            {
                ResetPasswordCommand = new AsyncCommand(ResetPassword),
                ToBackCommand = new RelayCommand(() => NavigationTo(GridNames.AuthorizationGrid))
            };

            ChangePasswordVM = new ChangePasswordViewModel
            {
                ChangePasswordCommand = new AsyncCommand(ChangePassword)
            };

            CodeConfirmVM = new CodeConfirmViewModel
            {
                CheckCodeCommand = new AsyncCommand(CodeConfirm),
                ResendCodeCommand = new AsyncCommand(RequestNewCode)
            };

            gridModels[GridNames.AuthorizationGrid] = AuthorizationVM;
            gridModels[GridNames.RegistrationGrid] = RegistrationVM;
            gridModels[GridNames.ResetPasswordGrid] = ResetPasswordVM;
            gridModels[GridNames.ChangePasswordGrid] = ChangePasswordVM;
            gridModels[GridNames.CodeConfirmGrid] = CodeConfirmVM;

            ValidationErrorTexts[ValidationError.UserNameInvalid] = (string)App.Languages["error_InvalidUsername"];
            ValidationErrorTexts[ValidationError.EmailInvalid] = (string)App.Languages["error_InvalidEmail"];
            ValidationErrorTexts[ValidationError.PasswordInvalid] = (string)App.Languages["error_InvalidPassword"];
            ValidationErrorTexts[ValidationError.RepeatPasswordInvalid] = (string)App.Languages["error_InvalidRepeatPassword"];
            ValidationErrorTexts[ValidationError.PasswordMismatch] = (string)App.Languages["error_PasswordMismatch"];
            ValidationErrorTexts[ValidationError.InvalidNewPassword] = (string)App.Languages["error_InvalidNewPassword"];

            switch (screen)
            {
                case AuthorizationScreen.Authorization:
                    currentGrid = GridNames.AuthorizationGrid;
                    UpdateShowGridAnimation(GridNames.AuthorizationGrid).Begin();
                    break;
                case AuthorizationScreen.ChangePassword:
                    currentGrid = GridNames.ChangePasswordGrid;
                    ChangePasswordVM.Email = Core.Utils.LoadConfig<Config>().Email;
                    UpdateShowGridAnimation(GridNames.ChangePasswordGrid).Begin();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DataContext = this;
        } 

        private async Task Authorization()
        {
            var vmStatus = AuthorizationVM.Validation(UserPassword.SecurePassword);
            if (vmStatus != ValidationError.Non)
            {
                AuthorizationVM.ErrorText = ValidationErrorTexts[vmStatus];
                return;
            }

            var response = await CheckAndSaveNewSession(AuthorizationVM.UserName, UserPassword.SecurePassword);
            if (response.Result)
                DialogResult = true;
            else if (response.Error.HasValue)
            {
                switch (response.Error.Value)
                {
                    case ClientLoginError.Failure:
                        AuthorizationVM.ErrorText = (string)App.Languages["error_Failure"];
                        break;
                    case ClientLoginError.AlreadyLoggedIn:
                        AuthorizationVM.ErrorText = (string)App.Languages["error_AlreadyLoggedIn"];
                        break;
                    case ClientLoginError.AccountNotExists:
                        AuthorizationVM.ErrorText = (string)App.Languages["error_AccountNotExists"];
                        break;
                    case ClientLoginError.AccountNotActivated:
                        DisableGui(true);
                        var resendResponse = await HeliosApi.Provider.ResendRegistrationCode(AuthorizationVM.UserName);
                        DisableGui(false);

                        if (resendResponse.Result)
                        {
                            CodeConfirmVM.CodeType = ConfirmCodeType.AccountActivation;
                            CodeConfirmVM.UserName = AuthorizationVM.UserName;
                            CodeConfirmVM.SecurePassword = UserPassword.SecurePassword;
                            CodeConfirmVM.ButtonCaption = (string)App.Languages["header_btn_ConfirmAccount"];
                            NavigationTo(GridNames.CodeConfirmGrid);
                        }
                        else
                            AuthorizationVM.ErrorText = (string)App.Languages["error_ServerNotAvailable"];

                        break;
                    case ClientLoginError.AccountIsBlocked:
                        AuthorizationVM.ErrorText = (string)App.Languages["error_AccountIsBlocked"];
                        break;
                    case ClientLoginError.AccountIsDeleted:
                        AuthorizationVM.ErrorText = (string)App.Languages["error_AccountIsDeleted"];
                        break;
                    case ClientLoginError.InvalidPassword:
                        AuthorizationVM.ErrorText = (string)App.Languages["error_InvalidPassword"];
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task Registration()
        {
            var vmStatus = RegistrationVM.Validation(
                UserRegisterPassword.SecurePassword,
                UserRepeatRegisterPassword.SecurePassword);

            if (vmStatus != ValidationError.Non)
            {
                RegistrationVM.ErrorText = ValidationErrorTexts[vmStatus];
                return;
            }
            
            DisableGui(true);
            var registrationResult = await HeliosApi.Provider.AccountRegistration(
                RegistrationVM.UserName,
                RegistrationVM.Email,
                RegistrationVM.SecurePassword,
                CaptchaFrame.CaptchaAnswer,
                CaptchaFrame.CaptchaResponse.CaptchaKey
            );
            DisableGui(false);

            if (registrationResult.Result)
            {
                CodeConfirmVM.CodeType = ConfirmCodeType.AccountActivation;
                CodeConfirmVM.UserName = RegistrationVM.UserName;
                CodeConfirmVM.SecurePassword = UserRegisterPassword.SecurePassword;
                CodeConfirmVM.ButtonCaption = (string)App.Languages["header_btn_ConfirmAccount"];
                NavigationTo(GridNames.CodeConfirmGrid);
            }
            else if (registrationResult.Error.HasValue)
            {
                switch (registrationResult.Error.Value)
                {
                    case ClientRegistrationError.Failure:
                        RegistrationVM.ErrorText = (string)App.Languages["error_Failure"];
                        break;
                    case ClientRegistrationError.InvalidEmail:
                        RegistrationVM.ErrorText = (string)App.Languages["error_InvalidEmail"];
                        break;
                    case ClientRegistrationError.InvalidUsername:
                        RegistrationVM.ErrorText = (string)App.Languages["error_InvalidUsername"];
                        break;
                    case ClientRegistrationError.InvalidPassword:
                        RegistrationVM.ErrorText = (string)App.Languages["error_InvalidPassword"];
                        break;
                    case ClientRegistrationError.EmailAlreadyRegistered:
                        RegistrationVM.ErrorText = (string)App.Languages["error_EmailAlreadyRegistered"];
                        break;
                    case ClientRegistrationError.UsernameAlreadyRegistered:
                        RegistrationVM.ErrorText = (string)App.Languages["error_UsernameAlreadyRegistered"];
                        break;
                    case ClientRegistrationError.AlreadyLoggedIn:
                        RegistrationVM.ErrorText = (string)App.Languages["error_AlreadyLoggedIn"];
                        break;
                    case ClientRegistrationError.CaptchaExpired:
                        RegistrationVM.ErrorText = (string)App.Languages["error_CaptchaExpired"];
                        break;
                    case ClientRegistrationError.InvalidCaptchaKey:
                        RegistrationVM.ErrorText = (string)App.Languages["error_InvalidCaptchaKey"];
                        break;
                    case ClientRegistrationError.InvalidCaptchaResponse:
                        RegistrationVM.ErrorText = (string)App.Languages["error_InvalidCaptchaResponse"];
                        break;
                }

                CaptchaFrame.ResetCaptcha();
            }
        }

        private async Task ResetPassword()
        {
            var vmStatus = ResetPasswordVM.Validation(pswResetPassword.SecurePassword, pswRepeatResetPassword.SecurePassword);

            if (vmStatus != ValidationError.Non)
            {
                ResetPasswordVM.ErrorText = ValidationErrorTexts[vmStatus];
                return;
            }

            DisableGui(true);
            var status = await HeliosApi.Provider.ResetPassword(
                ResetPasswordVM.UserName, 
                pswResetPassword.SecurePassword,
                CaptchaFrame.CaptchaAnswer,
                CaptchaFrame.CaptchaResponse.CaptchaKey);
            DisableGui(false);
            
            if (status.Result) 
            {
                CodeConfirmVM.CodeType = ConfirmCodeType.ResetPassword;
                CodeConfirmVM.UserName = ResetPasswordVM.UserName;
                CodeConfirmVM.SecurePassword = pswResetPassword.SecurePassword;
                CodeConfirmVM.ButtonCaption = (string)App.Languages["header_btn_ResetPassword"];
                NavigationTo(GridNames.CodeConfirmGrid);
            }
            else if (status.Error.HasValue)
            {
                switch (status.Error.Value)
                {
                    case ClientPasswordResetError.Failure:
                        ResetPasswordVM.ErrorText = (string)App.Languages["error_Failure"];
                        break;
                    case ClientPasswordResetError.InvalidNewPassword:
                        ResetPasswordVM.ErrorText = (string)App.Languages["error_InvalidNewPassword"];
                        break;
                    case ClientPasswordResetError.CaptchaExpired:
                        ResetPasswordVM.ErrorText = (string)App.Languages["error_CaptchaExpired"];
                        break;
                    case ClientPasswordResetError.InvalidCaptchaKey:
                        ResetPasswordVM.ErrorText = (string)App.Languages["error_InvalidCaptchaKey"];
                        break;
                    case ClientPasswordResetError.InvalidCaptchaResponse:
                        ResetPasswordVM.ErrorText = (string)App.Languages["error_InvalidCaptchaResponse"];
                        break;
                }

                CaptchaFrame.ResetCaptcha();
            }
        }

        private async Task ChangePassword()
        {
            var vmStatus = ChangePasswordVM.Validation(pswChangePassword.SecurePassword, pswRepeatChangePassword.SecurePassword);

            if (vmStatus != ValidationError.Non)
            {
                ChangePasswordVM.ErrorText = ValidationErrorTexts[vmStatus];
                return;
            }

            DisableGui(true);

            var status = await HeliosApi.Provider.ChangePassword(
                Core.Utils.LoadConfig<Config>().SessionId,
                pswCurrentChangePassword.SecurePassword,
                pswChangePassword.SecurePassword
            );

            DisableGui(false);

            if (status.Result)
                DialogResult = true;
            else if (status.Error.HasValue)
            {
                switch (status.Error.Value)
                {
                    case ClientPasswordChangeError.Failure:
                        ChangePasswordVM.ErrorText = (string)App.Languages["error_Failure"];
                        break;
                    case ClientPasswordChangeError.InvalidCurrentPassword:
                        ChangePasswordVM.ErrorText = (string)App.Languages["error_InvalidCurrentPassword"];
                        break;
                    case ClientPasswordChangeError.InvalidNewPassword:
                        ChangePasswordVM.ErrorText = (string)App.Languages["error_InvalidNewPassword"];
                        break;
                    case ClientPasswordChangeError.NotLoggedIn:
                        ChangePasswordVM.ErrorText = (string)App.Languages["error_NotLoggedIn"];
                        break;
                }
            }
        }

        private async Task CodeConfirm()
        {
            if (string.IsNullOrWhiteSpace(CodeConfirmVM.Code))
            {
                ChangePasswordVM.ErrorText = (string)App.Languages["error_IncorrectCredential"];
                return;
            }

            var userName = CodeConfirmVM.UserName ?? Core.Utils.LoadConfig<Config>().UserName;

            if (string.IsNullOrWhiteSpace(userName))
            {
                logger.Error("Username is null or empty");
                throw new InvalidOperationException();
            }
                
            var codeIsValid = false;

            DisableGui(true);
            switch (CodeConfirmVM.CodeType)
            {
                case ConfirmCodeType.AccountActivation:
                    codeIsValid = await HeliosApi.Provider.AccountActivationConfirm(userName, CodeConfirmVM.Code);
                    break;
                case ConfirmCodeType.ResetPassword:
                    codeIsValid = await HeliosApi.Provider.ResetPasswordConfirm(userName, CodeConfirmVM.Code);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            DisableGui(false);

            if (codeIsValid)
            {
                await CheckAndSaveNewSession(userName, CodeConfirmVM.SecurePassword);
                DialogResult = true;
            }
            else
            {
                CodeConfirmVM.ResendCodeVisibility = Visibility.Visible;
                CodeConfirmVM.ErrorText = (string)App.Languages["error_IncorrectCode"];
            }
        }

        /// <summary>
        /// Request new code from form "CodeConfirm"
        /// </summary>
        /// <returns></returns>
        private async Task RequestNewCode()
        {
            CodeConfirmVM.ErrorText = string.Empty;
            CodeConfirmVM.ResendCodeVisibility = Visibility.Collapsed;
            DisableGui(true);
            switch (CodeConfirmVM.CodeType)
            {
                case ConfirmCodeType.AccountActivation:
                    var resendResponse = await HeliosApi.Provider.ResendRegistrationCode(CodeConfirmVM.UserName);
                    break;
                case ConfirmCodeType.ResetPassword:
                    var status = await HeliosApi.Provider.ResetPassword(CodeConfirmVM.UserName, pswResetPassword.SecurePassword, CaptchaFrame.CaptchaAnswer, CaptchaFrame.CaptchaResponse.CaptchaKey);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            DisableGui(false);
        }

        private void DisableGui(bool isDisable)
        {
            if (isDisable)
            {
                DockPanelContent.Opacity = 0.5;
                DockPanelContent.IsEnabled = false;

                ProgressIndicator.Visibility = Visibility.Visible;
                ProgressIndicator.IsActive = true;
            }
            else
            {
                DockPanelContent.Opacity = 1;
                DockPanelContent.IsEnabled = true;

                ProgressIndicator.Visibility = Visibility.Collapsed;
                ProgressIndicator.IsActive = false;
            }
        }

        private void NavigationTo(GridNames targetGrid)
        {
            gridModels[targetGrid].ClearValues();
            UpdateHideGridAnimation(currentGrid).Begin();
            UpdateShowGridAnimation(targetGrid).Begin();
            currentGrid = targetGrid;

            switch (targetGrid)
            {
                case GridNames.AuthorizationGrid:
                    break;
                case GridNames.RegistrationGrid:
                    if (RegistrationGrid.Children.IndexOf(CaptchaFrame) == -1)
                    {
                        ((Grid)CaptchaFrame.Parent).Children.Remove(CaptchaFrame);
                        RegistrationGrid.Children.Add(CaptchaFrame);
                        CaptchaFrame.SetValue(Grid.RowProperty, 4);
                        CaptchaFrame.ResetCaptcha();
                    }
                    break;
                case GridNames.ChangePasswordGrid:
                    break;
                case GridNames.CodeConfirmGrid:
                    break;
                case GridNames.ResetPasswordGrid:
                    if (ResetPasswordGrid.Children.IndexOf(CaptchaFrame) == -1)
                    {
                        ((Grid)CaptchaFrame.Parent).Children.Remove(CaptchaFrame);
                        ResetPasswordGrid.Children.Add(CaptchaFrame);
                        CaptchaFrame.SetValue(Grid.RowProperty, 3);
                        CaptchaFrame.ResetCaptcha();
                    }
                    ResetPasswordVM.UserName = Core.Utils.LoadConfig<Config>().UserName;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Storyboard UpdateShowGridAnimation(GridNames targetGridNameKey)
        {
            var targetGridName = gridNames[targetGridNameKey];
            var targetHeight = gridHeights[targetGridNameKey];

            var sbShowGrid = FindResource("showGrid") as Storyboard;
            foreach (var animation in sbShowGrid.Children)
            {
                var targetPath = Storyboard.GetTargetProperty(animation).Path;
                var targetName = Storyboard.GetTargetName(animation);
                if (targetPath == nameof(Height))
                    ((DoubleAnimation)animation).To = targetHeight;
                else
                    Storyboard.SetTargetName(animation, targetGridName);
            }
            return sbShowGrid;
        }

        private Storyboard UpdateHideGridAnimation(GridNames targetGridNameKey)
        {
            var targetGridName = gridNames[targetGridNameKey];
            var sbShowGrid = FindResource("hideGrid") as Storyboard;
            foreach (var animation in sbShowGrid.Children)
            {
                var targetName = Storyboard.GetTargetName(animation);
                Storyboard.SetTargetName(animation, targetGridName);
            }
            return sbShowGrid;
        }

        /// <summary>
        /// Get session from Helios server and save it to local file (if session is valid)
        /// </summary>
        /// <param name="securePassword">for check credential on Helios server</param>
        /// <returns></returns>
        private async Task<ClientLoginResponse> CheckAndSaveNewSession(string userName, SecureString securePassword)
        {
            DisableGui(true);
            var response = await HeliosApi.Provider.GetSessionIdByCredential(userName, securePassword);
            DisableGui(false);

            if (response.Result)
            {
                var passwordHash = ProtectedData.Protect(
                    Encoding.Unicode.GetBytes(Core.Utils.ConvertToUnsecureString(securePassword)), 
                    null, 
                    DataProtectionScope.CurrentUser);
                Core.Utils.LoadConfig<Config>().SaveCredential(response.SessionId, userName, passwordHash);
            }
            
            return response;
        }
    }
}