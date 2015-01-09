using System;
using System.IO;
using System.Net;
using System.Text;

namespace FreshdeskTest
{
    class CreateTicketWithAttachment
    {
        private const string _APIKey = "YOUR_API_KEY";
        private const string _Url = "http://YOUR_DOMAIN.freshdesk.com/contacts.json"; // verify if you have to use http or https for your account
        
        private static void writeCRLF(Stream o)
        {
            byte[] crLf = Encoding.ASCII.GetBytes("\r\n");
            o.Write(crLf, 0, crLf.Length);
        }
        
        private static void writeBoundaryBytes(Stream o, string b, bool isFinalBoundary)
        {
            string boundary = isFinalBoundary == true ? "--" + b + "--" : "--" + b + "\r\n";
            byte[] d = Encoding.ASCII.GetBytes(boundary);
            o.Write(d, 0, d.Length);
        }
        
        private static void writeContentDispositionFormDataHeader(Stream o, string name)
        {
            string data = "Content-Disposition: form-data; name=\"" + name + "\"\r\n\r\n";
            byte[] b = Encoding.ASCII.GetBytes(data);
            o.Write(b, 0, b.Length);
        }
        
        private static void writeContentDispositionFileHeader(Stream o, string name, string fileName, string contentType)
        {
            string data = "Content-Disposition: form-data; name=\"" + name + "\"; filename=\"" + fileName + "\"\r\n";
            data += "Content-Type: " + contentType + "\r\n\r\n";
            byte[] b = Encoding.ASCII.GetBytes(data);
            o.Write(b, 0, b.Length);
        }
        
        private static void writeString(Stream o, string data)
        {
            byte[] b = Encoding.ASCII.GetBytes(data);
            o.Write(b, 0, b.Length);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Application starting...");

            // Define boundary:
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            // Web Request:
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(_Url);

            wr.Headers.Clear();
            
            // Method and headers:
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            
            // Basic auth:
            string login = _APIKey + ":X";
            string credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(login));
            wr.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            
            // Body:
            using (var rs = wr.GetRequestStream())
            {
                // Email:
                writeBoundaryBytes(rs, boundary, false);
                writeContentDispositionFormDataHeader(rs, "user[email]");
                writeString(rs, "example@example.com");
                writeCRLF(rs);
                
                // Name:
                writeBoundaryBytes(rs, boundary, false);
                writeContentDispositionFormDataHeader(rs, "user[name]");
                writeString(rs, "Super Man");
                writeCRLF(rs);
                
                
                // Attachment:
                writeBoundaryBytes(rs, boundary, false);
                writeContentDispositionFileHeader(rs, "user[avatar_attributes][content]", "x.png", "image/png");
                FileStream fs = new FileStream("x.png", FileMode.Open, FileAccess.Read);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();
                rs.Write(data, 0, data.Length);
                writeCRLF(rs);
                
                // End marker:
                writeBoundaryBytes(rs, boundary, true);
                
                rs.Close();
            }
            
            // Response processing:
            try
            {
                Console.WriteLine("Submitting Request");
                var response = (HttpWebResponse)wr.GetResponse();
                Stream resStream = response.GetResponseStream();
                string resJson = new StreamReader(resStream, Encoding.ASCII).ReadToEnd();
                Console.WriteLine(resJson);
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(Environment.NewLine);
            }

        }

    }
}
