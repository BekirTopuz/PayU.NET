using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
namespace PayUOperation
{
    public partial class ocr : System.Web.UI.Page
    {
        /*
         * Coded by Bilgehan PALALIOĞLU
         * LinkedIn: http://www.linkedin.com/in/bilgehanpalalioglu
         */

        protected void Page_Load(object sender, EventArgs e)
        {
            //Gets Data From Default.aspx
            Dictionary<string, string> contentDic = new Dictionary<string, string>();
            contentDic.Add("Description", Request.Form["Description"]);
            contentDic.Add("Email", Request.Form["Email"]);
            contentDic.Add("FirstName", Request.Form["FirstName"]);
            contentDic.Add("LastName", Request.Form["LastName"]);
            contentDic.Add("Phone", Request.Form["Phone"]);
            contentDic.Add("IP", Request.ServerVariables["REMOTE_ADDR"]);
            //3 items available in basket
            List<Product> proList = new List<Product>();

            proList.Add(new Product()
            {
                Name = "macbook pro",
                Quantity = 2,
                UnitPrice = 1M
            });
            proList.Add(new Product()
            {
                Name = "toshiba qosmio",
                Quantity = 1,
                UnitPrice = 2M
            }); 
            proList.Add(new Product()
            {
                Name = "imac",
                Quantity = 1,
                UnitPrice = 3M
            });
            
           
            OpenPayU_Order op = new OpenPayU_Order();
            string createdRequest = string.Empty;
            
            createdRequest = op.Create(OpenPayU.GenerateListToXML(proList, contentDic));
            

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(createdRequest);
            string jsonText = JsonConvert.SerializeXmlNode(doc);
        
            Response.ContentType = "application/json";
            Response.Write(jsonText);
            Response.End();
        }
        
    }
}