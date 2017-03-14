using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ProjectJson.Models;
using Classes;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Diagnostics;
using System.Net.Mail;
using ProjectJson.Models.eMail;
using System.Net.Mime;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace ProjectJson.Controllers
{
  public class eMailController : ApiController
  {
    [HttpPost]
    [Route("SendEmail")]
    public void SendEmail(Email email)
    {
        // GERAR IMAGEM

        object x = email;

        string outfolder = @"~/tmp/"; 
        string outfile = "report";

        string[] url = new string[] { email.url };

        string[] options = new string[] { "-p http://oi12949:DaniJOF5@10.32.150.40:82 ", " --crop-y 53 ", " --javascript-delay 5000 " };

        string imageFile = Classes.HtmlToImage.convert(outfolder, outfile, url, options);

        using (MailMessage message = new MailMessage())
        {
            //message.BodyEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
            message.BodyEncoding = System.Text.Encoding.Unicode;
            message.Priority = MailPriority.Normal;
            message.IsBodyHtml = true;

            message.From = new MailAddress(email.from);
            //message.From = new MailAddress("sgq@oi.net.br");

            message.To.Add(email.to);
            //message.To.Add("joao.frade@oi.net.br");

            //foreach (string i in email.to)
            //    if (i.ToString() != "") message.To.Add(i);

            //foreach (string i in email.cc)
            //    if (i.ToString() != "") message.CC.Add(i);

            message.Subject = email.subject;
            //message.Subject = "Teste Report";

            // var inlineImage = new LinkedResource(Server.MapPath(imageFile));
            var inlineImage = new LinkedResource(System.Web.Hosting.HostingEnvironment.MapPath(imageFile));
            inlineImage.ContentId = Guid.NewGuid().ToString();

            message.Body = string.Format(@"<img src=""cid:{0}"" />", inlineImage.ContentId);

            var view = AlternateView.CreateAlternateViewFromString(message.Body, null, "text/html");
            view.LinkedResources.Add(inlineImage);
            message.AlternateViews.Add(view);

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

      if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(imageFile)))
      {
          File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(imageFile));
      }

    }

    //[HttpPost]
    //[Route("SaveImageByHtml")]
    //public void SaveImageByHtml(Email email)
    //{
    //  var source = @"
    //    <!DOCTYPE html>
    //    <html>
    //      <body>
    //        <p>An image from W3Schools:</p>
    //        <img 
    //          src=""http://www.w3schools.com/images/w3schools_green.jpg"" 
    //          alt=""W3Schools.com"" 
    //          width=""104"" 
    //          height=""142"">
    //      </body>
    //    </html>";
    //  StartBrowser(email.body);
    //  Console.ReadLine();
    //}

    //[HttpGet]
    //[Route("HtmlToImage")]
    //public string HtmlToImage()
    //{
    //    // string outFolder, string outFilename, string[] urls,
    //    // string[] options = null, string HtmlToImageExPath = @"c:\Program Files\wkhtmltopdf\bin\wkhtmltoimage.exe

    //}

    private void StartBrowser(string source)
    {
      var th = new Thread(() =>
            {
              var webBrowser = new WebBrowser();
              webBrowser.ScrollBarsEnabled = false;
              webBrowser.DocumentCompleted += webBrowser_DocumentCompleted;
              webBrowser.DocumentText = source;
              Application.Run();
            });
      th.SetApartmentState(ApartmentState.STA);
      th.Start();
    }

    private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
    {
      var webBrowser = (WebBrowser)sender;
      using (Bitmap bitmap = new Bitmap(webBrowser.Width, webBrowser.Height))
      {
        webBrowser.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
        bitmap.Save(@"d:\tmp\filename.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
      }
    }
  }
}
