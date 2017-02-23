using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using projectJson.Models;
using Classes;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Diagnostics;
using System.Net.Mail;
using projectJson.Models.eMail;
using System.Net.Mime;

namespace projectJson.Controllers
{
    public class eMailController : ApiController
    {
        [HttpPost]
        [Route("send")]
        public string send(Email email)
        {
            Email x = email;
            using (MailMessage message = new MailMessage())
            {
                //message.BodyEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
                message.BodyEncoding = System.Text.Encoding.Unicode;
                message.Priority = MailPriority.Normal;
                message.IsBodyHtml = true;

                message.From = new MailAddress(email.from);

                message.To.Add(email.to);

                //foreach (string i in email.to)
                //    if (i.ToString() != "") message.To.Add(i);

                //foreach (string i in email.cc)
                //    if (i.ToString() != "") message.CC.Add(i);

                message.Subject = email.subject;
                message.Body = email.body;

                // ContentType mimeType = new System.Net.Mime.ContentType("text/html");
                // AlternateView alternate = AlternateView.CreateAlternateViewFromString(email.body, mimeType);
                // message.AlternateViews.Add(alternate);


                //if (email.attachment.ContentLength > 0)
                //{
                //    string fileName = Path.GetFileName(email.attachment.FileName);
                //    message.Attachments.Add(new Attachment(email.attachment.InputStream, fileName));
                //}

                using (SmtpClient smtp = new SmtpClient("relayinterno.telemar"))
                {
                    smtp.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    //smtp.Port = 587;

                    // smtp.EnableSsl = true;
                    // smtp.NetworkCredential NetworkCred = new NetworkCredential(email.Email, email.Password);
                    // smtp.UseDefaultCredentials = true;
                    // smtp.Credentials = NetworkCred;
                    try
                    {
                        smtp.Send(message);
                    }
                    catch (SmtpException) { }
                }
            }
            return "xxxx";
        }
    }
}