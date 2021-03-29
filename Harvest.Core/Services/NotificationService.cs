﻿using Harvest.Core.Data;
using Harvest.Core.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Harvest.Core.Services
{
    public interface INotificationService
    {
        Task SendSampleNotificationMessage(string email, string body);
    }

    public class NotificationService : INotificationService
    {
        private readonly SmtpClient _client;
        private readonly AppDbContext _dbContext;
        private readonly SparkpostSettings _emailSettings;

        public NotificationService(AppDbContext dbContext, IOptions<SparkpostSettings> emailSettings)
        {
            _dbContext = dbContext;
            _emailSettings = emailSettings.Value;
            _client = new SmtpClient(_emailSettings.Host, _emailSettings.Port) { Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password), EnableSsl = true };
        }
        public async Task SendSampleNotificationMessage(string email, string body)
        {
            if(_emailSettings.DisableSend.Equals("Yes", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            using (var message = new MailMessage { From = new MailAddress("harvest@notify.ucdavis.edu", "Harvest Notification"), Subject = "Harvest Notification" })
            {
                message.To.Add(new MailAddress(email, email));

                // body is our fallback text and we'll add an HTML view as an alternate.
                message.Body = "Sample Email Text";

                var htmlView = AlternateView.CreateAlternateViewFromString(body, new ContentType(MediaTypeNames.Text.Html));
                message.AlternateViews.Add(htmlView);

                await _client.SendMailAsync(message);
            }
        }
    }
}
