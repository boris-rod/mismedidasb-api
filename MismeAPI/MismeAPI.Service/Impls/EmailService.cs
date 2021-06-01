using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MismeAPI.Common;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace MismeAPI.Services.Impls
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _uow;

        public EmailService(IConfiguration configuration, IUnitOfWork uow)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task StoreEmailAync(string subject, string message, IEnumerable<string> emails, string filePath = "")
        {
            var stringEmail = EmailsToString(emails);
            var email = new ScheduledEmail
            {
                Subject = subject,
                Message = message,
                Emails = stringEmail,
                Filepath = filePath,
                Sent = false,
                RetryCount = 0,
                ExceptionMessage = "",
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            await _uow.ScheduledEmailsRepository.AddAsync(email);
            await _uow.CommitAsync();

            // Expire the cache when a new email gets into the database
            var cachePrefix = _configuration.GetSection("AWS")["CachePrefix"];
            QueryCacheManager.ExpireTag(cachePrefix + CacheEntries.ALL_SCHEDULED_EMAILS);
        }

        public async Task ProcessScheduledEmailAsync(ScheduledEmail scheduledEmail)
        {
            if (scheduledEmail.Sent == false)
            {
                var emails = StringToEmails(scheduledEmail.Emails);

                foreach (var email in emails)
                {
                    try
                    {
                        // Send emails one by one to protect user email to be seen by other users
                        // and also following AWS SES best practices
                        var to = new List<string> { email };

                        if (string.IsNullOrEmpty(scheduledEmail.Filepath))
                        {
                            await SendEmailResponseAsync(scheduledEmail.Subject, scheduledEmail.Message, to);
                        }
                        else
                        {
                            await SendEmailResponseWithAttachmentAsync(scheduledEmail.Subject, scheduledEmail.Message, to, scheduledEmail.Filepath);
                        }

                        // Make sure that no more than one AWS-SES request per second is made
                        Thread.Sleep(1000);
                    }
                    catch (Exception e)
                    {
                        if (scheduledEmail.RetryCount > 0)
                        {
                            scheduledEmail.Sent = true;
                        }
                        else
                        {
                            scheduledEmail.RetryCount++;
                        }

                        scheduledEmail.ExceptionMessage = e.Message;
                        scheduledEmail.ModifiedAt = DateTime.UtcNow;

                        await _uow.ScheduledEmailsRepository.UpdateAsync(scheduledEmail, scheduledEmail.Id);
                        await _uow.CommitAsync();

                        Thread.Sleep(1000);

                        throw e;
                    }
                }

                scheduledEmail.Sent = true;
                scheduledEmail.ModifiedAt = DateTime.UtcNow;

                await _uow.ScheduledEmailsRepository.UpdateAsync(scheduledEmail, scheduledEmail.Id);
                await _uow.CommitAsync();
            }
        }

        public async Task SendEmailResponseAsync(string subject, string message, IEnumerable<string> emails)
        {
            // Stop sending emails to multiple receipts
            if (emails.Count() < 2)
            {
                try
                {
                    if (emails.Count() == 1)
                    {
                        var cancelEmails = await GetUseCancelEmailNotificationsAsync(emails.FirstOrDefault());
                        if (cancelEmails)
                            return;

                        message = await ReplaceFooterPersonalDataAsync(message, emails.FirstOrDefault());
                    }

                    using (var client = new SmtpClient())
                    {
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                        await client.ConnectAsync(_configuration.GetValue<string>("SMTP:Server"), int.Parse(_configuration.GetValue<string>("SMTP:Port")), false);

                        await client.AuthenticateAsync(_configuration.GetValue<string>("SMTP:User"), _configuration.GetValue<string>("SMTP:Password"));

                        var mess = new MimeMessage();

                        var from = _configuration.GetValue<string>("SMTP:From");
                        var fromName = _configuration.GetValue<string>("SMTP:FromName");

                        mess.From.Add(new MailboxAddress(fromName, from));

                        foreach (var email in emails)
                        {
                            mess.To.Add(new MailboxAddress(email));
                        }

                        mess.Subject = subject;
                        mess.Body = new TextPart("html")
                        {
                            Text = message
                        };

                        // Different approach to send html
                        //var builder = new BodyBuilder();
                        //builder.HtmlBody = message;

                        //mess.Body = builder.ToMessageBody();

                        await client.SendAsync(mess);
                        client.Disconnect(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                await StoreEmailAync(subject, message, emails);
            }
        }

        public async Task SendEmailResponseWithAttachmentAsync(string subject, string message, IEnumerable<string> emails, string filePath)
        {
            // Stop sending emails to multiple receipts
            if (emails.Count() < 2)
            {
                try
                {
                    using (var client = new SmtpClient())
                    {
                        if (emails.Count() == 1)
                        {
                            var cancelEmails = await GetUseCancelEmailNotificationsAsync(emails.FirstOrDefault());
                            if (cancelEmails)
                                return;

                            message = await ReplaceFooterPersonalDataAsync(message, emails.FirstOrDefault());
                        }

                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                        await client.ConnectAsync(_configuration.GetValue<string>("SMTP:Server"), int.Parse(_configuration.GetValue<string>("SMTP:Port")), false);

                        await client.AuthenticateAsync(_configuration.GetValue<string>("SMTP:User"), _configuration.GetValue<string>("SMTP:Password"));

                        var mess = new MimeMessage();

                        var from = _configuration.GetValue<string>("SMTP:From");
                        var fromName = _configuration.GetValue<string>("SMTP:FromName");

                        mess.From.Add(new MailboxAddress(fromName, from));

                        foreach (var email in emails)
                        {
                            mess.To.Add(new MailboxAddress(email));
                        }
                        mess.Subject = subject;

                        var builder = new BodyBuilder();

                        builder.HtmlBody = message;

                        builder.Attachments.Add(filePath);

                        mess.Body = builder.ToMessageBody();

                        await client.SendAsync(mess);
                        client.Disconnect(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                await StoreEmailAync(subject, message, emails, filePath);
            }
        }

        private string EmailsToString(IEnumerable<string> emails)
        {
            var result = "";
            foreach (var email in emails)
            {
                result += email + " ";
            }

            return result.Trim();
        }

        private IEnumerable<string> StringToEmails(string text)
        {
            var emails = text.Split(" ");

            return emails.ToList();
        }

        private async Task<string> GetTokenAsync(string email)
        {
            var user = await _uow.UserRepository.GetAll()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();

            return user?.GuidId;
        }

        private async Task<string> ReplaceFooterPersonalDataAsync(string message, string email)
        {
            var url = _configuration.GetSection("CustomSetting")["AdminUrl"];
            var token = await GetTokenAsync(email);

            url += "/email-unsuscribe/" + token;

            return message.Replace("#UNSUSCRIBEURL#", url);
        }

        private async Task<bool> GetUseCancelEmailNotificationsAsync(string email)
        {
            var user = await _uow.UserRepository.GetAll()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();

            if (user == null)
                return false;

            var setting = await _uow.SettingRepository.GetAll().Where(s => s.Name == SettingsConstants.EMAIL_NOTIFICATION).FirstOrDefaultAsync();
            if (setting != null)
            {
                var us = await _uow.UserSettingRepository.GetAll().Where(us => us.SettingId == setting.Id && us.UserId == user.Id).FirstOrDefaultAsync();
                if (us != null && !string.IsNullOrWhiteSpace(us.Value))
                {
                    return us.Value == "false";
                }
            }

            return false;
        }
    }
}
