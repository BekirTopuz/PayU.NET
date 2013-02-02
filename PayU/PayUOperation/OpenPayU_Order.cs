using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Web;
using System.Xml;
using System.Security.Cryptography;
using System.Net;
namespace PayUOperation
{
   public  class OpenPayU_Order
    {
        /*
          * Coded by Bilgehan PALALIOĞLU
          * LinkedIn: http://www.linkedin.com/in/bilgehanpalalioglu
          */

        public string Create(XDocument docx)
        {
            XNamespace nameSpace = StaticString.NameSpace;
            string merchantPostId = string.Empty;
            try
            {
                var result =from item in docx.Descendants(nameSpace + StaticString.OpenPayU).Elements(nameSpace + StaticString.OrderCreateRequest)
                select item.Element(nameSpace + StaticString.MerchantPosId).Value;
                merchantPostId = result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            if (string.IsNullOrEmpty(merchantPostId))
            {
                return ErrorContent.MerchantPostId;
            }
            /* 
             * Generated xml from OpenPayU class,
             * Assigned  value from ocr.aspx.cs file (SignatureKey),
             * Add order.xml to the end of the Environment 
             * sent Generated xml, SignatureKey and Environment to the SendOpenPayuDocumentAuth function
            */
            return SendOpenPayuDocumentAuth(docx, merchantPostId, StaticString.SignatureKey, StaticString.Environment + "order.xml");
        }
        public static string SendOpenPayuDocumentAuth(XDocument doc, string merchantPostId, string secretKey, string openPayuEndPointUrl)
        {
            if (string.IsNullOrEmpty(merchantPostId))
            {
                return ErrorContent.MerchantPostId;
            }
            if (string.IsNullOrEmpty(secretKey))
            {
                return ErrorContent.SecretKey;
            }
            if (string.IsNullOrEmpty(openPayuEndPointUrl))
            {
                return ErrorContent.OpenPayuEndPointUrl;
            }
            //doc , converting to the string type
            StringWriter sw = new Utf8StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);
            doc.WriteTo(tx);
            string signature = ComputeHash(sw.ToString() + StaticString.SignatureKey, new SHA256CryptoServiceProvider());
            string authData = "sender=" + merchantPostId + ";signature=" + signature + ";algorithm=SHA256;content=DOCUMENT";
            return SendDataAuth(openPayuEndPointUrl, "DOCUMENT=" + HttpContext.Current.Server.UrlEncode(sw.ToString()), authData);
        }
        public static string ComputeHash(string input, HashAlgorithm algorithm)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
       public static string  SendDataAuth(string openPayuEndPointUrl,string doc,string authData)
       {
           //Sending request to the Payu server and getting response from there 
           string responseData = string.Empty;

           HttpWebRequest request = (HttpWebRequest)WebRequest.Create(openPayuEndPointUrl);
           request.Method = "POST";
           request.Headers.Add("OpenPayu-Signature:" + authData);
           request.ContentType = "application/x-www-form-urlencoded";

           byte[] postByteArray = Encoding.UTF8.GetBytes(doc);
           request.ContentLength = postByteArray.Length;
           Stream postStream = request.GetRequestStream();
           postStream.Write(postByteArray, 0, postByteArray.Length);
           postStream.Close();

           try
           {
               HttpWebResponse response = (HttpWebResponse)request.GetResponse();
               if (response.StatusCode == System.Net.HttpStatusCode.OK)
               {
                   Stream responseStream = response.GetResponseStream();
                   StreamReader myStreamReader = new StreamReader(responseStream);
                   responseData = myStreamReader.ReadToEnd();
               }
               response.Close();
           }
           catch (Exception ex)
           {
               return ex.Message;
           }
           
           
           return responseData;
       }

    }
   public class Utf8StringWriter : StringWriter
   {
       public override Encoding Encoding
       {
           get
           {
               return Encoding.UTF8;
           }
       }
   }
}
