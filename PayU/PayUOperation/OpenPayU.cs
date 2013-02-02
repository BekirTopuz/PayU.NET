using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PayUOperation
{
    public class OpenPayU
    {
        /*
         * Coded by Bilgehan PALALIOĞLU
         * LinkedIn: http://www.linkedin.com/in/bilgehanpalalioglu
         */

        public static XDocument GenerateListToXML(List<Product> productList,Dictionary<string,string> contentDic)
        {
            /*
             * Generate XML from Product List
             */
            decimal totalAmount = 0M;
            foreach (Product product in productList)
            {

                totalAmount += product.UnitPrice * product.Quantity;
            }
            try
            {
                XNamespace nameSpace = StaticString.NameSpace;
                XDocument PayUDocument = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                     new XElement(nameSpace + StaticString.OpenPayU,
                        new XElement(nameSpace + StaticString.OrderCreateRequest,
                            new XElement(nameSpace + StaticString.ReqId, Guid.NewGuid()),
                            new XElement(nameSpace + StaticString.CustomerIp, contentDic["IP"]),
                            new XElement(nameSpace + StaticString.ExtOrderId, StaticString.ExtOrderIdValue),
                            new XElement(nameSpace + StaticString.MerchantPosId, StaticString.MerchantPosIdValue),
                            new XElement(nameSpace + StaticString.Description, contentDic["Description"]),
                            new XElement(nameSpace + StaticString.CurrencyCode, StaticString.CurrencyCodeValue),
                            new XElement(nameSpace + StaticString.TotalAmount, totalAmount),
                            new XElement(nameSpace + StaticString.Buyer,
                                new XElement(nameSpace + StaticString.FirstName, contentDic["FirstName"]),
                                new XElement(nameSpace + StaticString.LastName, contentDic["LastName"]),
                                new XElement(nameSpace + StaticString.CountryCode, StaticString.CountryCodeValue),
                                new XElement(nameSpace + StaticString.Email, contentDic["Email"]),
                                new XElement(nameSpace + StaticString.PhoneNumber, contentDic["Phone"]),
                                new XElement(nameSpace + StaticString.Language, StaticString.LanguageValue)),
                            new XElement(nameSpace + StaticString.Products,
                                     from item in productList
                                     select new XElement(nameSpace + StaticString.Product,
                                     new XElement(nameSpace + StaticString.Name, item.Name),
                                     new XElement(nameSpace + StaticString.UnitPrice, item.UnitPrice),
                                     new XElement(nameSpace + StaticString.Quantity, item.Quantity))),
                            new XElement(nameSpace + StaticString.PayMethod, StaticString.Default)
                                     ))
                        );
                return PayUDocument;
            }
            catch (Exception )
            {
                
                return new XDocument();
            }
            
           
        }
        
    }
    public class Product
    {
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
