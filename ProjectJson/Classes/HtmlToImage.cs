using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Hosting;

namespace Classes
{
    public static class HtmlToImage
    {
        public static string convert(
            string outFolder, 
            string outFilename, 
            string[] urls, 
            string[] options = null, 
            string HtmlToImageExPath = @"c:\Program Files\wkhtmltopdf\bin\wkhtmltoimage.exe")
        {
            string urlsSeparatedBySpaces = string.Empty;
            try
            {
                if ((urls == null) || (urls.Length == 0))
                    throw new Exception("Informe a URL para o HtmlToJpg");
                else
                    urlsSeparatedBySpaces = String.Join(" ", urls);

                outFilename = outFilename + "_" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff") + ".jpg";

                var p = new System.Diagnostics.Process()
                {
                    StartInfo =
                    {
                        FileName = HtmlToImageExPath,
                        Arguments = ((options == null) ? "" : String.Join(" ", options)) + " -p http://oi12949:DaniJOF5@10.32.150.40:82 " + urlsSeparatedBySpaces + " " + outFilename,
                        UseShellExecute = false, // needs to be false in order to redirect output
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true, // redirect all 3, as it should be all 3 or none
                        WorkingDirectory = HttpContext.Current.Server.MapPath(outFolder)
                    }
                };

                p.Start();

                // read the output here...
                var output = p.StandardOutput.ReadToEnd();
                var errorOutput = p.StandardError.ReadToEnd();

                // ...then wait n milliseconds for exit (as after exit, it can't read the output)
                p.WaitForExit(60000);

                // read the exit code, close process
                int returnCode = p.ExitCode;
                p.Close();

                // if 0 or 2, it worked so return path of pdf
                //if ((returnCode == 0) || (returnCode == 2))
                //    return outFolder + outFilename;
                //else
                //    throw new Exception(errorOutput);

                return outFolder + outFilename;
            }
            catch (Exception exc)
            {
                throw new Exception("Problem generating PDF from HTML, URLs: " + urlsSeparatedBySpaces + ", outputFilename: " + outFilename, exc);
            }
        }
    }
}