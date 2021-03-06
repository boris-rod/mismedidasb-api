﻿using DeviceDetectorNET;
using DeviceDetectorNET.Cache;
using DeviceDetectorNET.Parser;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Service;
using MismeAPI.Service.Utils;
using rlcx.suid;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wangkanai.Detection;

namespace MismeAPI.Services.Impls
{
    public class AccountService : IAccountService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _uow;
        private readonly IDetection _detection;
        private readonly IFileService _fileService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IProfileHelthHelper _profileHelthHelper;

        public AccountService(IConfiguration configuration, IUnitOfWork uow, IDetection detection,
            IFileService fileService, ISubscriptionService subscriptionService, IProfileHelthHelper profileHelthHelper)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _detection = detection ?? throw new ArgumentNullException(nameof(detection));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
            _profileHelthHelper = profileHelthHelper ?? throw new ArgumentNullException(nameof(profileHelthHelper));
        }

        public async Task<(User user, string accessToken, string refreshToken, double kcal, double IMC, DateTime? firstHealtMeasured)> LoginAsync(LoginRequest loginRequest)
        {
            var hashedPass = GetSha256Hash(loginRequest.Password);

            var user = await _uow.UserRepository.FindBy(u => u.Email == loginRequest.Email)
                .Include(u => u.Group)
                .Include(u => u.UserSettings)
                    .ThenInclude(s => s.Setting)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }

            if (user.Password != hashedPass)
            {
                throw new UnauthorizedException(ExceptionConstants.UNAUTHORIZED);
            }

            if (user.Role == RoleEnum.GROUP_ADMIN && user.Group == null)
            {
                await ValidateGroupAdminFirstLoginAsync(user);
            }

            if (user.Status != StatusEnum.ACTIVE)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var dd = GetDeviceDetectorConfigured();

            var clientInfo = dd.GetClient();
            var osrInfo = dd.GetOs();
            var device1 = dd.GetDeviceName();
            var brand = dd.GetBrandName();
            var model = dd.GetModel();

            var claims = GetClaims(user);
            var token = GetToken(claims);
            var refreshToken = GetRefreshToken();
            var t = new UserToken();
            t.AccessToken = token;
            t.AccessTokenExpiresDateTime = DateTime.UtcNow.AddHours(int.Parse(_configuration.GetSection("BearerTokens")["AccessTokenExpirationHours"]));
            t.RefreshToken = refreshToken;
            t.RefreshTokenExpiresDateTime = DateTime.UtcNow.AddHours(int.Parse(_configuration.GetSection("BearerTokens")["RefreshTokenExpirationHours"]));
            t.UserId = user.Id;

            t.DeviceModel = model;
            t.DeviceBrand = brand;

            t.OS = osrInfo.Match?.Name;
            t.OSPlatform = osrInfo.Match?.Platform;
            t.OSVersion = osrInfo.Match?.Version;

            t.ClientName = clientInfo.Match?.Name;
            t.ClientType = clientInfo.Match?.Type;
            t.ClientVersion = clientInfo.Match?.Version;

            await _uow.UserTokenRepository.AddAsync(t);

            if (!string.IsNullOrEmpty(loginRequest.UserTimeZone))
            {
                user.TimeZone = loginRequest.UserTimeZone;
                user.TimeZoneOffset = loginRequest.UserTimeZoneOffset;
            }

            var kcal = 0.0;
            var imc = 0.0;
            DateTime? first = null;

            if (user.CurrentImc <= 0 || user.CurrentKcal <= 0 || user.FirtsHealthMeasured.HasValue == false)
            {
                var info = await _profileHelthHelper.GetCurrentKCalIMCFirstHeltMeasureAsync(user.Id);
                kcal = info.kcal;
                imc = info.imc;
                first = info.firstHealthMeasure;
                // patch for current users, not needed for new users or old users at second time
                user.CurrentImc = imc;
                user.CurrentKcal = kcal;
                user.FirtsHealthMeasured = first;
            }
            else
            {
                kcal = user.CurrentKcal;
                imc = user.CurrentImc;
                first = user.FirtsHealthMeasured;
            }

            await _uow.UserRepository.UpdateAsync(user, user.Id);

            await _uow.CommitAsync();

            return (user, token, refreshToken, kcal, imc, first);
        }

        public async Task<double> GetKCalAsync(int userId)
        {
            try
            {
                var concept = await _uow.ConceptRepository.GetAll().Where(c => c.Codename == CodeNamesConstants.HEALTH_MEASURES).FirstOrDefaultAsync();

                if (concept != null)
                {
                    var polls = _uow.PollRepository.GetAll().Where(p => p.ConceptId == concept.Id)
                          .Include(p => p.Questions)
                          .OrderBy(p => p.Order)
                          .ToList();
                    // this is hardcoded but is the way it is.

                    // poll 1- personal data
                    var questions = polls.ElementAt(0).Questions.OrderBy(q => q.Order);

                    var age = 0;
                    var weight = 0;
                    var height = 0;
                    var sex = 0;

                    var count = 0;
                    foreach (var q in questions)
                    {
                        var ua = _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id)
                            .Include(u => u.Answer)
                                .ThenInclude(a => a.Question)
                            .OrderByDescending(ua => ua.CreatedAt)
                            .FirstOrDefault();
                        //age
                        if (count == 0 && ua != null)
                        {
                            age = ua.Answer.Weight;
                        }
                        //weight
                        else if (count == 1 && ua != null)
                        {
                            weight = ua.Answer.Weight;
                        }
                        //height
                        else if (count == 2 && ua != null)
                        {
                            height = ua.Answer.Weight;
                        }
                        //sex
                        else
                        {
                            sex = ua.Answer.Weight;
                        }

                        count += 1;
                    }

                    //poll 2- Physical excersice
                    var physicalExercise = 0;
                    var physicalQuestion = polls.ElementAt(1).Questions.OrderBy(q => q.Order).FirstOrDefault();
                    if (physicalQuestion != null)
                    {
                        var ua = _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == physicalQuestion.Id)
                                .Include(u => u.Answer)
                                    .ThenInclude(a => a.Question)
                                .OrderByDescending(ua => ua.CreatedAt)
                                .FirstOrDefault();
                        if (ua != null)
                        {
                            physicalExercise = ua.Answer.Weight;
                        }
                    }
                    //poll 3- Diet
                    //var dietSummary = 0;
                    //questions = polls.ElementAt(2).Questions.OrderBy(q => q.Order);

                    //foreach (var q in questions)
                    //{
                    //    var ua = _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id)
                    //        .Include(u => u.Answer)
                    //            .ThenInclude(a => a.Question)
                    //        .OrderByDescending(ua => ua.CreatedAt)
                    //        .FirstOrDefault();

                    //    dietSummary += ua.Answer.Weight;
                    //}

                    // other values
                    //var IMC = Convert.ToDouble(weight) / ((Convert.ToDouble(height) / 100) * ((Convert.ToDouble(height) / 100)));
                    var TMB_PROV = 10 * Convert.ToDouble(weight) + 6.25 * Convert.ToDouble(height) - 5 * age;

                    var dailyKalDouble = 0.0;

                    if (sex == 1)
                    {
                        if (physicalExercise == 1)
                        {
                            dailyKalDouble = (TMB_PROV + 5) * 1.2;
                        }
                        else if (physicalExercise == 2)
                        {
                            dailyKalDouble = (TMB_PROV + 5) * 1.375;
                        }
                        else if (physicalExercise == 3)
                        {
                            dailyKalDouble = (TMB_PROV + 5) * 1.55;
                        }
                        else if (physicalExercise == 4)
                        {
                            dailyKalDouble = (TMB_PROV + 5) * 1.725;
                        }
                        else
                        {
                            dailyKalDouble = (TMB_PROV + 5) * 1.9;
                        }
                    }
                    else
                    {
                        if (physicalExercise == 1)
                        {
                            dailyKalDouble = (TMB_PROV - 161) * 1.2;
                        }
                        else if (physicalExercise == 2)
                        {
                            dailyKalDouble = (TMB_PROV - 161) * 1.375;
                        }
                        else if (physicalExercise == 3)
                        {
                            dailyKalDouble = (TMB_PROV - 161) * 1.55;
                        }
                        else if (physicalExercise == 4)
                        {
                            dailyKalDouble = (TMB_PROV - 161) * 1.725;
                        }
                        else
                        {
                            dailyKalDouble = (TMB_PROV - 161) * 1.9;
                        }
                    }
                    return dailyKalDouble;
                }
            }
            catch (Exception)
            {
                return 0.0;
            }
            return 0.0;
        }

        private DateTime? GetFirstHealthMeasured(int userId)
        {
            var concept = _uow.ConceptRepository.GetAll().Where(c => c.Codename == CodeNamesConstants.HEALTH_MEASURES).FirstOrDefault();

            if (concept != null)
            {
                var c = _uow.UserConceptRepository.GetAll().Where(c => c.UserId == userId && c.ConceptId == concept.Id).FirstOrDefault();
                if (c != null)
                {
                    return c.CompletedAt;
                }
            }

            return null;
        }

        public async Task<double> GetIMCAsync(int userId)
        {
            try
            {
                var concept = await _uow.ConceptRepository.GetAll().Where(c => c.Codename == CodeNamesConstants.HEALTH_MEASURES).FirstOrDefaultAsync();

                if (concept != null)
                {
                    var polls = _uow.PollRepository.GetAll().Where(p => p.ConceptId == concept.Id)
                          .Include(p => p.Questions)
                          .OrderBy(p => p.Order)
                          .ToList();
                    // this is hardcoded but is the way it is.

                    // poll 1- personal data
                    var questions = polls.ElementAt(0).Questions.OrderBy(q => q.Order);

                    var age = 0;
                    var weight = 0;
                    var height = 0;
                    var sex = 0;

                    var count = 0;
                    foreach (var q in questions)
                    {
                        var ua = _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id)
                            .Include(u => u.Answer)
                                .ThenInclude(a => a.Question)
                            .OrderByDescending(ua => ua.CreatedAt)
                            .FirstOrDefault();
                        //age
                        if (count == 0 && ua != null)
                        {
                            age = ua.Answer.Weight;
                        }
                        //weight
                        else if (count == 1 && ua != null)
                        {
                            weight = ua.Answer.Weight;
                        }
                        //height
                        else if (count == 2 && ua != null)
                        {
                            height = ua.Answer.Weight;
                        }
                        //sex
                        else
                        {
                            sex = ua.Answer.Weight;
                        }

                        count += 1;
                    }

                    // other values
                    var IMC = Convert.ToDouble(weight) / ((Convert.ToDouble(height) / 100) * ((Convert.ToDouble(height) / 100)));

                    return IMC;
                }
            }
            catch (Exception)
            {
                return 0.0;
            }
            return 0.0;
        }

        private string GetRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private string GetToken(IEnumerable<Claim> claims)
        {
            var issuer = _configuration.GetSection("BearerTokens")["Issuer"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("BearerTokens")["Key"]));

            var jwt = new JwtSecurityToken(issuer: issuer,
                audience: _configuration.GetSection("BearerTokens")["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(int.Parse(_configuration.GetSection("BearerTokens")["AccessTokenExpirationHours"])),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt); //the method is called WriteToken but returns a string
        }

        public async Task LogoutAsync(int userIdValue, string accessToken)
        {
            var tokens = await _uow.UserTokenRepository.FindByAsync(t => t.UserId == userIdValue && t.AccessToken == accessToken);

            foreach (var item in tokens)
            {
                _uow.UserTokenRepository.Delete(item);
            }
            await _uow.CommitAsync();
        }

        public async Task<User> SignUpAsync(SignUpRequest suRequest)
        {
            var emailExists = await _uow.UserRepository.FindAllAsync(u => u.Email == suRequest.Email);
            if (emailExists.Count > 0)
            {
                throw new MismeAPI.Common.Exceptions.InvalidDataException(ExceptionConstants.INVALID_DATA, "email.");
            }

            if (string.IsNullOrWhiteSpace(suRequest.Password) ||
                suRequest.Password.Length < 6
                //|| CheckStringWithoutSpecialChars(suRequest.Password)
                //|| !CheckStringWithUppercaseLetters(suRequest.Password)
                )
            {
                throw new InvalidDataException("password.");
            }

            if (suRequest.Password != suRequest.ConfirmationPassword)
            {
                throw new InvalidDataException("password.");
            }

            var passwordHash = GetSha256Hash(suRequest.Password);
            Random r = new Random();
            int number = r.Next(100000, 999999);

            var user = new User
            {
                Email = suRequest.Email,
                FullName = suRequest.FullName,
                Password = passwordHash,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                Username = await GetUserNameAsync(suRequest.Email),
                VerificationCode = number,
                GuidId = Guid.NewGuid().ToString(),

                Status = StatusEnum.PENDING
            };

            await _uow.UserRepository.AddAsync(user);

            var setting = _uow.SettingRepository.GetAll().Where(s => s.Name == SettingsConstants.LANGUAGE).FirstOrDefault();
            if (setting != null)
            {
                var uSett = new UserSetting();
                uSett.Setting = setting;
                uSett.User = user;
                var lang = string.IsNullOrWhiteSpace(suRequest.Language) ? "ES" :
                    ((suRequest.Language.ToLower() != "es" && suRequest.Language.ToLower() != "en" && suRequest.Language.ToLower() != "it") ? "ES" : suRequest.Language.ToUpper());
                uSett.Value = lang;

                await _uow.UserSettingRepository.AddAsync(uSett);
            }

            await _uow.CommitAsync();
            return user;
        }

        private async Task<string> GetUserNameAsync(string email)
        {
            var username = email.Split('@')[0];
            var exist = await _uow.UserRepository.FindByAsync(u => u.Username == username);
            if (exist.Count > 0)
            {
                var validationObject = await ValidateUsernameAsync(username, email, "");

                if (validationObject.Suggestions.Count() == 0)
                {
                    username = username + "_" + Suid.NewLettersOnlySuid();
                }
                else
                {
                    username = validationObject.Suggestions.FirstOrDefault();
                }
            }
            return username;
        }

        private bool CheckStringWithoutSpecialChars(string word)
        {
            var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
            return regexItem.IsMatch(word);
        }

        private bool CheckStringWithUppercaseLetters(string word)
        {
            var regexItem = new Regex("[A-Z]");
            return regexItem.IsMatch(word);
        }

        private string GetSha256Hash(string input)
        {
            using (var hashAlgorithm = new SHA256CryptoServiceProvider())
            {
                var byteValue = Encoding.UTF8.GetBytes(input);
                var byteHash = hashAlgorithm.ComputeHash(byteValue);
                return Convert.ToBase64String(byteHash);
            }
        }

        public Task<ClaimsPrincipal> GetPrincipalFromExpiredTokenAsync(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("BearerTokens")["Key"])),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new MismeAPI.Common.Exceptions.InvalidDataException("token.");

            return Task.FromResult(principal);
        }

        public async Task GetRefreshTokenAsync(RefreshTokenRequest refreshToken, string userId)
        {
            var refToken = await _uow.UserTokenRepository.FindBy(u => u.UserId == int.Parse(userId) && u.AccessToken == refreshToken.Token)
                .FirstOrDefaultAsync();
            if (refToken == null)
            {
                throw new NotFoundException(ExceptionConstants.INVALID_DATA, "refresh token.");
            }
            if (refToken.RefreshToken != refreshToken.RefreshToken)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "refresh token.");
            }
        }

        public async Task<(string accessToken, string refreshToken)> GenerateNewTokensAsync(string token, string refreshToken)
        {
            var oldToken = await _uow.UserTokenRepository.FindBy(u => u.AccessToken == token && u.RefreshToken == refreshToken)
                .Include(u => u.User)
                    .ThenInclude(u => u.Group)
                .FirstOrDefaultAsync();

            if (oldToken == null)
            {
                throw new UnauthorizedException(ExceptionConstants.UNAUTHORIZED);
            }

            var claims = GetClaims(oldToken.User);

            var newToken = GetToken(claims);
            var newRefreshToken = GetRefreshToken();

            oldToken.AccessToken = newToken;
            oldToken.AccessTokenExpiresDateTime = DateTime.UtcNow.AddHours(int.Parse(_configuration.GetSection("BearerTokens")["AccessTokenExpirationHours"]));
            oldToken.RefreshToken = newRefreshToken;
            oldToken.RefreshTokenExpiresDateTime = DateTime.UtcNow.AddHours(int.Parse(_configuration.GetSection("BearerTokens")["RefreshTokenExpirationHours"]));

            await _uow.UserTokenRepository.UpdateAsync(oldToken, oldToken.Id);
            await _uow.CommitAsync();
            return (newToken, newRefreshToken);
        }

        private List<Claim> GetClaims(User user)
        {
            var issuer = _configuration.GetSection("BearerTokens")["Issuer"];
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Email, user.Email, ClaimValueTypes.Email, issuer));
            claims.Add(new Claim(ClaimTypes.AuthenticationMethod, "bearer", ClaimValueTypes.String, issuer));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.FullName, ClaimValueTypes.String, issuer));
            claims.Add(new Claim(ClaimTypes.UserData, user.Id.ToString(), ClaimValueTypes.String, issuer));
            claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString(), ClaimValueTypes.String, issuer));
            claims.Add(new Claim(ClaimTypes.GroupSid, user.GroupId.HasValue ? user.GroupId.ToString() : "", ClaimValueTypes.String, issuer));
            return claims;
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest changePassword, int userId)
        {
            var user = await _uow.UserRepository.FindBy(u => u.Id == userId).FirstOrDefaultAsync();
            var passwordHash = GetSha256Hash(changePassword.OldPassword);

            if (passwordHash != user.Password)
            {
                throw new MismeAPI.Common.Exceptions.InvalidDataException(ExceptionConstants.INVALID_DATA, "Password");
            }

            if (changePassword.NewPassword != changePassword.ConfirmPassword)
            {
                throw new MismeAPI.Common.Exceptions.InvalidDataException(ExceptionConstants.INVALID_DATA, "Password");
            }

            if (string.IsNullOrWhiteSpace(changePassword.NewPassword) ||
                changePassword.NewPassword.Length < 6
                //|| CheckStringWithoutSpecialChars(changePassword.NewPassword)
                //|| !CheckStringWithUppercaseLetters(changePassword.NewPassword)
                )
            {
                throw new MismeAPI.Common.Exceptions.InvalidDataException(ExceptionConstants.INVALID_DATA, "Password");
            }

            var newPasswordHash = GetSha256Hash(changePassword.NewPassword);

            user.Password = newPasswordHash;
            user.ModifiedAt = DateTime.UtcNow;

            _uow.UserRepository.Update(user);

            await _uow.CommitAsync();
        }

        public async Task GlobalLogoutAsync(int userId)
        {
            var tokens = await _uow.UserTokenRepository.FindByAsync(t => t.UserId == userId);

            foreach (var item in tokens)
            {
                _uow.UserTokenRepository.Delete(item);
            }
            await _uow.CommitAsync();
        }

        private DeviceDetector GetDeviceDetectorConfigured()
        {
            var ua = _detection.UserAgent;

            DeviceDetector.SetVersionTruncation(VersionTruncation.VERSION_TRUNCATION_NONE);

            var dd = new DeviceDetector(ua.ToString());

            // OPTIONAL: Set caching method By default static cache is used, which works best within
            // one php process (memory array caching) To cache across requests use caching in files
            // or memcache add using DeviceDetectorNET.Cache;
            dd.SetCache(new DictionaryCache());

            // OPTIONAL: If called, GetBot() will only return true if a bot was detected (speeds up
            // detection a bit)
            dd.DiscardBotInformation();

            // OPTIONAL: If called, bot detection will completely be skipped (bots will be detected as
            // regular devices then)
            dd.SkipBotDetection();
            dd.Parse();
            return dd;
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            var isValid = true;

            var validator = new JwtSecurityTokenHandler();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = _configuration.GetSection("BearerTokens")["Audience"],
                ValidateAudience = true,
                ValidIssuer = _configuration.GetSection("BearerTokens")["Issuer"],
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("BearerTokens")["Key"])),
                ValidateLifetime = true
            };

            if (validator.CanReadToken(token))
            {
                try
                {
                    SecurityToken securityToken;
                    var principal = validator.ValidateToken(token, tokenValidationParameters, out securityToken);
                }
                catch (Exception)
                {
                    isValid = false;
                }
            }
            else
            {
                isValid = false;
            }

            return Task.FromResult(isValid);
        }

        public async Task<User> GetUserAsync(int userId)
        {
            var user = await _uow.UserRepository.GetAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User.");
            }

            return user;
        }

        public async Task<User> ChangeAccountStatusAsync(ChangeAccountStatusRequest changeAccountStatus, int userId)
        {
            var user = await _uow.UserRepository.FindBy(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }

            if (user.Id == userId && changeAccountStatus.Active == false)
            {
                throw new MismeAPI.Common.Exceptions.InvalidDataException(ExceptionConstants.INVALID_DATA, "Account");
            }

            if (changeAccountStatus.Active == false)
            {
                user.Status = StatusEnum.INACTIVE;
            }
            else
            {
                user.Status = StatusEnum.ACTIVE;
            }

            user.ModifiedAt = DateTime.UtcNow;

            _uow.UserRepository.Update(user);

            await _uow.CommitAsync();
            return user;
        }

        public async Task<User> UploadAvatarAsync(IFormFile file, int userId)
        {
            var user = await _uow.UserRepository.FindBy(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null) { throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User"); }

            string guid = Guid.NewGuid().ToString();

            if (!string.IsNullOrWhiteSpace(user.Avatar))
            {
                await _fileService.DeleteFileAsync(user.Avatar);
            }

            //upload the new one and update user avatar's properties await
            await _fileService.UploadFileAsync(file, guid);

            user.Avatar = guid;
            user.AvatarMimeType = file.ContentType;

            await _uow.UserRepository.UpdateAsync(user, userId);
            await _uow.CommitAsync();

            return user;
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _uow.UserRepository.FindBy(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }

            var generator = new PasswordGenerator(6, 6, 0, 0, 0, 0);

            string newPass = generator.Generate();

            var newPasswordHash = GetSha256Hash(newPass);

            user.Password = newPasswordHash;
            user.ModifiedAt = DateTime.UtcNow;
            await _uow.UserRepository.UpdateAsync(user, user.Id);
            await _uow.CommitAsync();
            return newPass;
        }

        public async Task<User> ActivationAccountAsync(ActivationAccountRequest activation)
        {
            var user = await _uow.UserRepository.FindBy(u => u.Email == activation.Email && u.VerificationCode == activation.Code).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }
            user.Status = StatusEnum.ACTIVE;
            user.VerificationCode = 0;
            user.ActivatedAt = DateTime.UtcNow;

            await _uow.UserRepository.UpdateAsync(user, user.Id);
            await _subscriptionService.GetOrInitSubscriptionAsync(user, SubscriptionEnum.VIRTUAL_ASESSOR);

            await _uow.CommitAsync();

            return user;
        }

        public async Task<int> ResendVerificationCodeAsync(string email)
        {
            Random r = new Random();
            int number = r.Next(100000, 999999);

            var user = await _uow.UserRepository.FindBy(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }

            user.VerificationCode = number;
            await _uow.UserRepository.UpdateAsync(user, user.Id);
            await _uow.CommitAsync();
            return number;
        }

        public async Task<User> RemoveAvatarAsync(int loggedUser)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (!string.IsNullOrWhiteSpace(user.Avatar))
            {
                await _fileService.DeleteFileAsync(user.Avatar);
            }
            user.Avatar = "";
            user.AvatarMimeType = "";
            _uow.UserRepository.Update(user);
            await _uow.CommitAsync();
            return user;
        }

        public async Task<IEnumerable<UserSetting>> GetUserSettingsAsync(int loggedUser)
        {
            return await _uow.UserSettingRepository.GetAll().Where(u => u.UserId == loggedUser)
                .Include(u => u.Setting).ToListAsync();
        }

        public async Task UpdateUserSettingsAsync(int loggedUser, List<UpdateSettingRequest> request)
        {
            foreach (var item in request)
            {
                var set = await _uow.SettingRepository.GetAsync(item.SettingId);
                if (set != null)
                {
                    var uSet = await _uow.UserSettingRepository.GetAll().Where(us => us.SettingId == item.SettingId && us.UserId == loggedUser).FirstOrDefaultAsync();
                    if (uSet != null)
                    {
                        uSet.Value = item.Value;
                        _uow.UserSettingRepository.Update(uSet);
                    }
                    else
                    {
                        uSet = new UserSetting();
                        uSet.UserId = loggedUser;
                        uSet.SettingId = item.SettingId;
                        uSet.Value = item.Value;
                        await _uow.UserSettingRepository.AddAsync(uSet);
                    }
                }
            }
            await _uow.CommitAsync();
        }

        public async Task DisableAccountAsync(int loggedUser, bool softDeletion)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }
            if (softDeletion == true)
            {
                user.DisabledAt = DateTime.UtcNow;
                user.MarkedForDeletion = true;
                user.Status = StatusEnum.INACTIVE;
                _uow.UserRepository.Update(user);
            }
            else
            {
                _uow.UserRepository.Delete(user);
            }
            await _uow.CommitAsync();
        }

        public async Task UpdateProfileAsync(int loggedUser, UpdateUserProfileRequest userProfileRequest)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }

            var existUsername = await _uow.UserRepository.FindAsync(u => u.Username == userProfileRequest.Username && u.Id != loggedUser);
            if (existUsername != null)
            {
                throw new AlreadyExistsException("Username is already in use.");
            }

            user.Username = userProfileRequest.Username;
            user.FullName = userProfileRequest.FullName;
            user.Phone = userProfileRequest.Phone;
            _uow.UserRepository.Update(user);
            await _uow.CommitAsync();
        }

        public async Task<(bool IsValid, ICollection<string> Suggestions)> ValidateUsernameAsync(string username, string email, string fullName, int userId = -1)
        {
            var isValid = false;
            var suggestions = new List<string>();

            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Email");
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Username");
            }

            var emailName = email.Split("@")[0];
            emailName = emailName.Replace(".", "");
            fullName = fullName.Replace(" ", "");
            fullName = fullName.ToLower();

            if (string.IsNullOrEmpty(fullName))
            {
                fullName = emailName.Substring(0, 4);
            }

            //Store the first possible name.
            var possibleUsername = string.Format("{0}{1}", fullName, emailName.Substring(0, 1));

            //Don't hit the database N times, instead get all the possible names in one shot.
            var existingUsers = await _uow.UserRepository.FindAllAsync(u => u.Username.StartsWith(possibleUsername));
            var existUsername = await _uow.UserRepository.FindAsync(u => u.Username == username && u.Id != userId);
            isValid = existUsername == null;

            //Find the first possible open username.
            if (existingUsers.Count == 0)
            {
                //Create the user since the username is open.
                suggestions.Add(possibleUsername);
            }
            else
            {
                //Iterate through all the possible usernames and create it when a spot is open.
                for (var i = 1; i < existingUsers.Count; i++)
                {
                    possibleUsername = string.Format("{0}{1}", fullName, emailName.Substring(0, i));
                    if (existingUsers.FirstOrDefault(u => u.Username == possibleUsername) == null)
                    {
                        //Create the user since the user name is open.
                        suggestions.Add(possibleUsername);
                    }

                    if (suggestions.Count > 3)
                        break;
                }
            }

            return (isValid, suggestions);
        }

        public async Task<int> GetHeightAsync(int userId)
        {
            try
            {
                var concept = await _uow.ConceptRepository.GetAll().Where(c => c.Codename == CodeNamesConstants.HEALTH_MEASURES).FirstOrDefaultAsync();

                if (concept != null)
                {
                    var polls = _uow.PollRepository.GetAll().Where(p => p.ConceptId == concept.Id)
                          .Include(p => p.Questions)
                          .OrderBy(p => p.Order)
                          .ToList();
                    // this is hardcoded but is the way it is.

                    // poll 1- personal data
                    var questions = polls.ElementAt(0).Questions.OrderBy(q => q.Order);

                    var age = 0;
                    var weight = 0;
                    var height = 0;
                    var sex = 0;

                    var count = 0;
                    foreach (var q in questions)
                    {
                        var ua = _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id)
                            .Include(u => u.Answer)
                                .ThenInclude(a => a.Question)
                            .OrderByDescending(ua => ua.CreatedAt)
                            .FirstOrDefault();
                        //age
                        if (count == 0 && ua != null)
                        {
                            age = ua.Answer.Weight;
                        }
                        //weight
                        else if (count == 1 && ua != null)
                        {
                            weight = ua.Answer.Weight;
                        }
                        //height
                        else if (count == 2 && ua != null)
                        {
                            height = ua.Answer.Weight;
                        }
                        //sex
                        else
                        {
                            sex = ua.Answer.Weight;
                        }

                        count += 1;
                    }
                    return height;
                }
            }
            catch (Exception)
            {
                return 0;
            }
            return 0;
        }

        public async Task<int> GetSexAsync(int userId)
        {
            try
            {
                var concept = await _uow.ConceptRepository.GetAll().Where(c => c.Codename == CodeNamesConstants.HEALTH_MEASURES).FirstOrDefaultAsync();

                if (concept != null)
                {
                    var polls = _uow.PollRepository.GetAll().Where(p => p.ConceptId == concept.Id)
                          .Include(p => p.Questions)
                          .OrderBy(p => p.Order)
                          .ToList();
                    // this is hardcoded but is the way it is.

                    // poll 1- personal data
                    var questions = polls.ElementAt(0).Questions.OrderBy(q => q.Order);

                    var age = 0;
                    var weight = 0;
                    var height = 0;
                    var sex = 0;

                    var count = 0;
                    foreach (var q in questions)
                    {
                        var ua = _uow.UserAnswerRepository.GetAll().Where(u => u.UserId == userId && u.Answer.QuestionId == q.Id)
                            .Include(u => u.Answer)
                                .ThenInclude(a => a.Question)
                            .OrderByDescending(ua => ua.CreatedAt)
                            .FirstOrDefault();
                        //age
                        if (count == 0 && ua != null)
                        {
                            age = ua.Answer.Weight;
                        }
                        //weight
                        else if (count == 1 && ua != null)
                        {
                            weight = ua.Answer.Weight;
                        }
                        //height
                        else if (count == 2 && ua != null)
                        {
                            height = ua.Answer.Weight;
                        }
                        //sex
                        else
                        {
                            sex = ua.Answer.Weight;
                        }

                        count += 1;
                    }
                    return sex;
                }
            }
            catch (Exception)
            {
                return 0;
            }
            return 0;
        }

        public async Task<bool> ValidateGroupAdminFirstLoginAsync(User user)
        {
            var invitation = await _uow.GroupInvitationRepository.GetAll()
                .FirstOrDefaultAsync(gi => gi.UserId == user.Id && gi.Status == StatusInvitationEnum.PENDING);

            if (invitation != null)
            {
                var group = await _uow.GroupRepository.GetAll().FirstOrDefaultAsync(g => g.Id == invitation.GroupId.Value);
                invitation.Status = StatusInvitationEnum.ACCEPTED;
                invitation.ModifiedAt = DateTime.UtcNow;
                await _uow.GroupInvitationRepository.UpdateAsync(invitation, invitation.Id);

                user.ActivatedAt = DateTime.UtcNow;
                user.Status = StatusEnum.ACTIVE;
                user.Group = group;
                await _uow.UserRepository.UpdateAsync(user, user.Id);
                await _uow.CommitAsync();

                return true;
            }

            return false;
        }

        public async Task UnsubscribeEmailsAsync(string token)
        {
            var user = await _uow.UserRepository.GetAll()
                .Include(u => u.UserSettings)
                .Where(u => u.GuidId == token)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new NotFoundException("User");

            var setting = await _uow.SettingRepository.GetAll().Where(s => s.Name == SettingsConstants.EMAIL_NOTIFICATION).FirstOrDefaultAsync();
            if (setting != null)
            {
                var us = await _uow.UserSettingRepository.GetAll().Where(us => us.SettingId == setting.Id && us.UserId == user.Id).FirstOrDefaultAsync();

                if (us == null)
                {
                    var usNew = new UserSetting
                    {
                        SettingId = setting.Id,
                        UserId = user.Id,
                        Value = "false"
                    };

                    await _uow.UserSettingRepository.AddAsync(usNew);
                }
                else
                {
                    us.Value = "false";

                    await _uow.UserSettingRepository.UpdateAsync(us, us.Id);
                }

                await _uow.CommitAsync();
            }
        }
    }
}
