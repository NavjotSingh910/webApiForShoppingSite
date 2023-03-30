using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace WebApplication3.Helper
{
    public class MailHelper
    {
        public String Send(MimeMessage? email)
        {
            //now we want to make a connection with our stmpclient using mailKit
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate("navjotsandhu910@outlook.com", "ywkjiurmcgojqyjd");//user name ,password
            smtp.Send(email);//send mail by passing our mail variable
            smtp.Disconnect(true);//now disconnect to server
            return "Done";
        }
    }
}