using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using WebApplication3.Dto;

namespace WebApplication3.Helper
{
    public class MailHelper
    {
        public async Task<String> Send(Email mail)
        {
             // Create a new email message and set its details
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse($"{mail.From}"));
            email.To.Add(MailboxAddress.Parse($"{mail.To}"));
            email.Subject = $"{mail.Subject}";
            email.Body = new TextPart(TextFormat.Html) { Text = $"{mail.Body}" };

            //now we want to make a connection with our stmpclient using mailKit
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate("navjotsandhu910@outlook.com", "yemeltsyxzyueqrp");//user name ,password
            smtp.Send(email);//send mail by passing our mail variable
            smtp.Disconnect(true);//now disconnect to server
            return "Done";
        }

    }
}