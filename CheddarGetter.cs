/*  CheddarGetter C# API Wrappper
 * 
 *  Version: 1.0
 *  Author: John Siladie <john [at] getconfer [dot] com>
 *  
 *  Copyright (c) 2010, Confer
 *  All rights reserved.
 *  
 *  
 *  Redistribution and use in source and binary forms, with or without modification, are 
 *  permitted provided that the following conditions are met:
 *  
 *  - Redistributions of source code must retain the above copyright notice, this list 
 *    of conditions and the following disclaimer.
 *  - Redistributions in binary form must reproduce the above copyright notice, this list 
 *    of conditions and the following disclaimer in the documentation and/or other 
 *    materials provided with the distribution.
 *  - Neither the name of the Confer nor the names of its contributors may be 
 *    used to endorse or promote products derived from this software without specific 
 *    prior written permission.
 * 
 *  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
 *  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
 *  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
 *  IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
 *  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
 *  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
 *  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 *  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 *  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 *  POSSIBILITY OF SUCH DAMAGE.
 *  
 *  For more information please visit our blog at http://blog.getconfer.com/
 *  
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Configuration;
using System.Web;

namespace Confer.CheddarGetter
{
    public static class CheddarGetter
    {
        private static string _Username = "";
        private static string _Password = "";
        private static string _ProductCode = "";

        /// <summary>
        /// The constructor will set the username and password for the CheddarGetter account
        /// </summary>
        static CheddarGetter()
        {
            try
            {
                _Username = ConfigurationManager.AppSettings["CheddarGetterUser"];
                _Password = ConfigurationManager.AppSettings["CheddarGetterPassword"];
                _ProductCode = ConfigurationManager.AppSettings["CheddarGetterProductCode"];
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Get all of the subscription plans for your product code
        /// </summary>
        /// <returns>A list of SubscriptionPlan objects</returns>
        public static List<SubscriptionPlan> GetSubscriptionPlans()
        {
            List<SubscriptionPlan> subscriptionPlansList = new List<SubscriptionPlan>();

            try
            {
                string urlBase = "https://cheddargetter.com/xml";
                string urlPath = string.Format("/plans/get/productCode/{0}", _ProductCode);

                string result = getRequest(urlBase, urlPath);
                XDocument plansXML = XDocument.Parse(result);

                subscriptionPlansList = (from p in plansXML.Descendants("plan")
                                         select new SubscriptionPlan
                                             {
                                                 ID = (Guid)p.Attribute("id"),
                                                 Code = (string)p.Attribute("code"),
                                                 Name = (string)p.Element("name"),
                                                 Description = (string)p.Element("description"),
                                                 IsActive = (bool)p.Element("isActive"),
                                                 TrialDays = (int)p.Element("trialDays"),
                                                 BillingFrequency = (string)p.Element("billingFrequency"),
                                                 BillingFrequencyPer = (string)p.Element("billingFrequencyPer"),
                                                 BillingFrequencyUnit = (string)p.Element("billingFrequencyUnit"),
                                                 BillingFrequencyQuantity = (string)p.Element("billingFrequencyQuantity"),
                                                 SetupChargeCode = (string)p.Element("setupChargeCode"),
                                                 SetupChargeAmount = (float)p.Element("setupChargeAmount"),
                                                 RecurringChargeCode = (string)p.Element("recurringChargeCode"),
                                                 RecurringChargeAmount = (float)p.Element("recurringChargeAmount"),
                                                 CreatedDateTime = (DateTime)p.Element("createdDatetime"),
                                                 PlanItems = (from i in p.Element("items").Descendants("item")
                                                              select new PlanItem
                                                              {
                                                                  ID = (Guid)i.Attribute("id"),
                                                                  Code = (string)i.Attribute("code"),
                                                                  Name = (string)i.Element("name"),
                                                                  QuantityIncluded = (double)i.Element("quantityIncluded"),
                                                                  IsPeriodic = (bool)i.Element("isPeriodic"),
                                                                  OverageAmount = (float)i.Element("overageAmount"),
                                                                  CreatedDateTime = (DateTime)i.Element("createdDatetime")
                                                              }).ToList()
                                             }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return subscriptionPlansList;
        }

        /// <summary>
        /// Get a list of all customers for your product code
        /// </summary>
        /// <returns>A list of Customer objects</returns>
        public static List<Customer> GetCustomers()
        {
            Customers customers = new Customers();

            try
            {
                string urlBase = "https://cheddargetter.com/xml";
                string urlPath = string.Format("/customers/get/productCode/{0}", _ProductCode);

                string result = getRequest(urlBase, urlPath);
                XDocument customersXML = XDocument.Parse(result);

                customers = getCustomerList(customersXML);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return customers.CustomerList;
        }

        /// <summary>
        /// Get a particular customer based on a passed in customer code and your product code
        /// </summary>
        /// <param name="customerCode">A string representing a customer's code in CG </param>
        /// <returns>A associated Customer object for the passed in customer code</returns>
        public static Customer GetCustomer(string customerCode)
        {
            Customers customers = new Customers();
            Customer customer = new Customer();

            string urlBase = "https://cheddargetter.com/xml";
            string urlPath = string.Format("/customers/get/productCode/{0}/code/{1}", _ProductCode, customerCode);
            try
            {
                string result = getRequest(urlBase, urlPath);

                XDocument customersXML = XDocument.Parse(result);

                customers = getCustomerList(customersXML);

                if (customers.CustomerList.Count > 0)
                {
                    customer = customers.CustomerList[0];
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return customer;
        }

        /// <summary>
        /// Create a new customer based on the passed in CustomerPost object
        /// </summary>
        /// <param name="customer">A CustomerPost object that represents a customer to be created</param>
        /// <returns>A newly created Customer object</returns>
        public static Customer CreateCustomer(CustomerPost customer)
        {
            Customers customers = new Customers();
            Customer newCustomer = new Customer();

            try
            {
                string urlBase = "https://cheddargetter.com/xml";
                string urlPath = string.Format("/customers/new/productCode/{0}", _ProductCode);
                string postParams = string.Format(
                    "code={0}" +
                    "&firstName={1}" +
                    "&lastName={2}" +
                    "&email={3}" +
                    "&company={4}" +
                    "&subscription[planCode]={5}" +
                    "&subscription[ccFirstName]={6}" +
                    "&subscription[ccLastName]={7}" +
                    "&subscription[ccNumber]={8}" +
                    "&subscription[ccExpiration]={9}" +
                    "&subscription[ccCardCode]={10}" +
                    "&subscription[ccZip]={11}",
                    HttpUtility.UrlEncode(customer.Code),
                    HttpUtility.UrlEncode(customer.FirstName),
                    HttpUtility.UrlEncode(customer.LastName),
                    HttpUtility.UrlEncode(customer.Email),
                    HttpUtility.UrlEncode(customer.Company),
                    HttpUtility.UrlEncode(customer.PlanCode.ToString().ToUpper()),
                    HttpUtility.UrlEncode(customer.CCFirstName),
                    HttpUtility.UrlEncode(customer.CCLastName),
                    HttpUtility.UrlEncode(customer.CCNumber),
                    HttpUtility.UrlEncode(customer.CCExpiration),
                    HttpUtility.UrlEncode(customer.CCCardCode),
                    HttpUtility.UrlEncode(customer.CCZip));

                string result = postRequest(urlBase, urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    newCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return newCustomer;
        }

        /// <summary>
        /// Update a customer and their subscription
        /// </summary>
        /// <param name="customer">A CustomerPost object that represents the changes to be updated</param>
        /// <returns>An updated Customer object with the changes applied</returns>
        public static Customer UpdateCustomerAndSubscription(CustomerPost customer)
        {
            Customers customers = new Customers();
            Customer updatedCustomer = new Customer();

            try
            {
                // Create the web request  
                string urlBase = "https://cheddargetter.com/xml";
                string urlPath = string.Format("/customers/edit/productCode/{0}/code/{1}", _ProductCode, customer.Code);
                string postParams = string.Format(
                    "firstName={0}" +
                    "&lastName={1}" +
                    "&email={2}" +
                    "&company={3}" +
                    "&subscription[planCode]={4}" +
                    "&subscription[ccFirstName]={5}" +
                    "&subscription[ccLastName]={6}" +
                    "&subscription[ccNumber]={7}" +
                    "&subscription[ccExpiration]={8}" +
                    "&subscription[ccCardCode]={9}" +
                    "&subscription[ccZip]={10}",
                    HttpUtility.UrlEncode(customer.FirstName),
                    HttpUtility.UrlEncode(customer.LastName),
                    HttpUtility.UrlEncode(customer.Email),
                    HttpUtility.UrlEncode(customer.Company),
                    HttpUtility.UrlEncode(customer.PlanCode.ToString().ToUpper()),
                    HttpUtility.UrlEncode(customer.CCFirstName),
                    HttpUtility.UrlEncode(customer.CCLastName),
                    HttpUtility.UrlEncode(customer.CCNumber),
                    HttpUtility.UrlEncode(string.Format("{0}/{1}", formatMonth(customer.CCExpMonth), customer.CCExpYear)),
                    HttpUtility.UrlEncode(customer.CCCardCode),
                    HttpUtility.UrlEncode(customer.CCZip));

                string result = postRequest(urlBase, urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    updatedCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return updatedCustomer;
        }

        /// <summary>
        /// Update a customer
        /// </summary>
        /// <param name="customer">A CustomerPost object that represents the changes to be updated</param>
        /// <returns>An updated Customer object with the changes applied</returns>
        public static Customer UpdateCustomer(CustomerPost customer)
        {
            Customers customers = new Customers();
            Customer updatedCustomer = new Customer();

            try
            {
                // Create the web request  
                string urlBase = "https://cheddargetter.com/xml";
                string urlPath = string.Format("/customers/edit-customer/productCode/{0}/code/{1}", _ProductCode, customer.Code);
                string postParams = string.Format(
                    "firstName={0}" +
                    "&lastName={1}" +
                    "&email={2}" +
                    "&company={3}",
                    HttpUtility.UrlEncode(customer.FirstName),
                    HttpUtility.UrlEncode(customer.LastName),
                    HttpUtility.UrlEncode(customer.Email),
                    HttpUtility.UrlEncode(customer.Company));

                string result = postRequest(urlBase, urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    updatedCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return updatedCustomer;
        }

        /// <summary>
        /// Update a customer's subscription
        /// </summary>
        /// <param name="customer">A CustomerPost object with the subscription details to update</param>
        /// <returns>A Customer object with the applied changes</returns>
        public static Customer UpdateSubscription(CustomerPost customer)
        {
            Customers customers = new Customers();
            Customer updatedCustomer = new Customer();

            try
            {
                // Create the web request  
                string urlBase = "https://cheddargetter.com/xml";
                string urlPath = string.Format("/customers/edit-subscription/productCode/{0}/code/{1}", _ProductCode, customer.Code);

                //note: expiration date must be in MM/YYYY format
                StringBuilder postParamsSB = new StringBuilder();
                postParamsSB.Append(string.Format("planCode={0}", HttpUtility.UrlEncode(customer.PlanCode.ToString().ToUpper())));
                postParamsSB.Append(string.Format("&ccFirstName={0}", HttpUtility.UrlEncode(customer.CCFirstName)));
                postParamsSB.Append(string.Format("&ccLastName={0}", HttpUtility.UrlEncode(customer.CCLastName)));

                if (!string.IsNullOrEmpty(customer.CCNumber))
                {
                    postParamsSB.Append(string.Format("&ccNumber={0}", HttpUtility.UrlEncode(customer.CCNumber)));
                }

                postParamsSB.Append(string.Format("&ccExpiration={0}", HttpUtility.UrlEncode(string.Format("{0}/{1}", formatMonth(customer.CCExpMonth), customer.CCExpYear))));

                if (!string.IsNullOrEmpty(customer.CCCardCode))
                {
                    postParamsSB.Append(string.Format("&ccCardCode={0}", HttpUtility.UrlEncode(customer.CCCardCode)));
                }

                postParamsSB.Append(string.Format("&ccZip={0}", HttpUtility.UrlEncode(customer.CCZip)));

                string result = postRequest(urlBase, urlPath, postParamsSB.ToString());
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    updatedCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return updatedCustomer;
        }

        /// <summary>
        /// Update a customer's subscription with only partial subscription information
        /// </summary>
        /// <param name="customer">A CustomerPost object with the changes that are to be updated</param>
        /// <returns>A Customer object with the applied changes</returns>
        public static Customer UpdateSubscriptionPartial(CustomerPost customer)
        {
            Customers customers = new Customers();
            Customer updatedCustomer = new Customer();

            try
            {
                // Create the web request  
                string urlBase = "https://cheddargetter.com/xml";
                string urlPath = string.Format("/customers/edit-subscription/productCode/{0}/code/{1}", _ProductCode, customer.Code);

                //note: expiration date must be in MM/YYYY format
                string postParams = string.Format(
                    "planCode={0}" +
                    "&ccFirstName={1}" +
                    "&ccLastName={2}" +
                    "&ccZip={3}",
                    HttpUtility.UrlEncode(customer.PlanCode.ToString().ToUpper()),
                    HttpUtility.UrlEncode(customer.CCFirstName),
                    HttpUtility.UrlEncode(customer.CCLastName),
                    HttpUtility.UrlEncode(customer.CCZip));

                string result = postRequest(urlBase, urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    updatedCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return updatedCustomer;
        }

        /// <summary>
        /// Update a customer's subscription plan
        /// </summary>
        /// <param name="customerCode">The customer's code of the customer to be updated</param>
        /// <param name="newPlan">The plan to set the customer to</param>
        /// <returns>A Customer object with the updated changes applied</returns>
        public static Customer UpdateSubscriptionPlanOnly(string customerCode, PlanCodeEnum newPlan)
        {
            Customers customers = new Customers();
            Customer updatedCustomer = new Customer();

            try
            {
                // Create the web request  
                string urlBase = "https://cheddargetter.com/xml";
                string urlPath = string.Format("/customers/edit-subscription/productCode/{0}/code/{1}", _ProductCode, customerCode);

                //note: expiration date must be in MM/YYYY format
                string postParams = string.Format("planCode={0}", HttpUtility.UrlEncode(newPlan.ToString().ToUpper()));

                string result = postRequest(urlBase, urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    updatedCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return updatedCustomer;
        }

        /// <summary>
        /// Cancel a customer's subscription
        /// </summary>
        /// <param name="customerCode">The customer code of the customer to cancel</param>
        /// <returns>A bool representing the success of the cancel</returns>
        public static bool CancelSubscription(string customerCode)
        {
            Customers customers = new Customers();
            Customer editCustomer = new Customer();
            bool canceled = false;

            try
            {
                string urlBase = "https://cheddargetter.com/xml";
                string urlPath = string.Format("/customers/cancel/productCode/{0}/code/{1}", _ProductCode, customerCode);

                string result = getRequest(urlBase, urlPath);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    editCustomer = customers.CustomerList[0];
                }

                canceled = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return canceled;
        }

        /// <summary>
        /// Add an item and set the quantity for a customer
        /// Note: if no quantity is specified then it will increment by 1 by default
        /// </summary>
        /// <param name="customerCode">The customer's code to associate the item with</param>
        /// <param name="itemCode">The item code of the item which we are adding</param>
        /// <param name="quantityToAdd">The number of units to add of this item</param>
        /// <returns>A Customer object reflecting the updated item and quantity</returns>
        public static Customer AddItem(string customerCode, string itemCode, int quantityToAdd)
        {
            Customers customers = new Customers();
            Customer editCustomer = new Customer();

            try
            {
                string urlBase = "https://cheddargetter.com/xml";
                string urlPath = string.Format("/customers/add-item-quantity/productCode/{0}/code/{1}/itemCode/{2}", _ProductCode, customerCode, itemCode);
                string postParams = "";

                if (quantityToAdd > 1)
                {
                    postParams = string.Format("quantity={0}", HttpUtility.UrlEncode(quantityToAdd.ToString()));
                }

                string result = postRequest(urlBase, urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    editCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return editCustomer;
        }

        /// <summary>
        /// Remove an item and set the quantity for a customer
        /// Note: if no quantity is specified then it will decrement by 1 by default
        /// </summary>
        /// <param name="customerCode">The customer's code to associate the item with</param>
        /// <param name="itemCode">The item code of the item which we are removing</param>
        /// <param name="quantityToRemove">The number of units to remove of this item</param>
        /// <returns>A Customer object reflecting the updated item and quantity</returns>
        public static Customer RemoveItem(string customerCode, string itemCode, int quantityToRemove)
        {
            Customers customers = new Customers();
            Customer editCustomer = new Customer();

            try
            {
                string urlBase = "https://cheddargetter.com/xml";
                string urlPath = string.Format("/customers/remove-item-quantity/productCode/{0}/code/{1}/itemCode/{2}", _ProductCode, customerCode, itemCode);
                string postParams = "";

                if (quantityToRemove > 1)
                {
                    postParams = string.Format("quantity={0}", HttpUtility.UrlEncode(quantityToRemove.ToString()));
                }

                string result = postRequest(urlBase, urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    editCustomer = customers.CustomerList[0];
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return editCustomer;
        }

        /// <summary>
        /// Set an item count to a specific quantity
        /// </summary>
        /// <param name="customerCode">The customer's code of the customer that will be updated </param>
        /// <param name="itemCode">The code of the item that will be updated</param>
        /// <param name="quantityToSet">The quantity to set for the item</param>
        /// <returns>A Customer object reflecting the updated item and quantity count</returns>
        public static Customer SetItem(string customerCode, ProductItemCode itemCode, int quantityToSet)
        {
            Customers customers = new Customers();
            Customer editCustomer = new Customer();

            try
            {
                string urlBase = "https://cheddargetter.com/xml";
                string urlPath = string.Format("/customers/set-item-quantity/productCode/{0}/code/{1}/itemCode/{2}", _ProductCode, customerCode, itemCode.ToString());
                string postParams = string.Format("quantity={0}", HttpUtility.UrlEncode(quantityToSet.ToString()));

                string result = postRequest(urlBase, urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    editCustomer = customers.CustomerList[0];
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return editCustomer;
        }

        /// <summary>
        /// Add a customer charge for a customer
        /// </summary>
        /// <param name="customCharge">A CustomerChargePost object with the customer charge and customer code</param>
        /// <returns>A Customer object with the reflected custom charge</returns>
        public static Customer AddCustomCharge(CustomChargePost customCharge)
        {
            Customers customers = new Customers();
            Customer editCustomer = new Customer();

            try
            {
                string urlBase = "https://cheddargetter.com/xml";
                string urlPath = string.Format("/customers/set-item-quantity/productCode/{0}/code/{1}/itemCode/{2}", _ProductCode, customCharge.CustomerCode, customCharge.ItemCode);
                string postParams = string.Format(
                  "chargeCode={0}" +
                  "&quantity={1}" +
                  "&eachAmount={2}" +
                  "&description={3}",
                  HttpUtility.UrlEncode(customCharge.ChargeCode),
                  HttpUtility.UrlEncode(customCharge.Quantity.ToString()),
                  HttpUtility.UrlEncode(customCharge.EachAmount.ToString()),
                  HttpUtility.UrlEncode(customCharge.Description));

                string result = postRequest(urlBase, urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    editCustomer = customers.CustomerList[0];
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return editCustomer;
        }

        /// <summary>
        /// Handles the GET requests
        /// </summary>
        /// <param name="urlBase">The base url of the request</param>
        /// <param name="urlPath">The rest of the url for the request</param>
        /// <returns>A string of XML data that is returned for the request</returns>
        private static string getRequest(string urlBase, string urlPath)
        {
            string result = "";

            try
            {
                HttpWebRequest request = WebRequest.Create(urlBase + urlPath) as HttpWebRequest;

                //Add authentication
                request.Credentials = new NetworkCredential(_Username, _Password);

                // Get response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    result = reader.ReadToEnd();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// Handles the POST request
        /// </summary>
        /// <param name="urlBase">The base URL of the request</param>
        /// <param name="urlPath">The rest of the URL for the request</param>
        /// <param name="postParams">Any additional parameters for the POST are added here</param>
        /// <returns>A string of XML data that is returned for the request</returns>
        private static string postRequest(string urlBase, string urlPath, string postParams)
        {
            string result = "";

            try
            {
                HttpWebRequest request = WebRequest.Create(urlBase + urlPath) as HttpWebRequest;

                //Add authentication
                request.Credentials = new NetworkCredential(_Username, _Password);

                //make into a post
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";

                byte[] bytes = Encoding.UTF8.GetBytes(postParams);
                request.ContentLength = bytes.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);

                    using (WebResponse response = request.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// Get the customer list and any associated CG errors from a XDocument (customer XML)
        /// </summary>
        /// <param name="customersXML">A XDocument that contains customer XML data</param>
        /// <returns>A Customers object (which is a list of customers and any associated GC errors) 
        /// that is built from the parsed XDocument</returns>
        private static Customers getCustomerList(XDocument customersXML)
        {
            Customers customers = new Customers();
            List<Customer> customerList = new List<Customer>();
            List<CGError> errorList = new List<CGError>();

            try
            {
                customerList = (from c in customersXML.Descendants("customer")
                                select new Customer
                                {
                                    ID = (Guid)c.Attribute("id"),
                                    Code = (string)c.Attribute("code"),
                                    FirstName = (string)c.Element("firstName"),
                                    LastName = (string)c.Element("lastName"),
                                    Company = (string)c.Element("company"),
                                    Email = (string)c.Element("email"),
                                    GatewayToken = (string)c.Element("gatewayToken"),
                                    CreatedDateTime = (DateTime)c.Element("createdDatetime"),
                                    ModifiedDateTime = (DateTime)c.Element("modifiedDatetime"),
                                    Subscriptions = getSubscriptionList(c.Element("subscriptions")),
                                }).ToList();

                errorList = (from e in customersXML.Descendants("errors")
                             select new CGError
                             {
                                 ID = (string)e.Attribute("id"),
                                 Code = (string)e.Attribute("code"),
                                 AuxCode = (string)e.Attribute("auxCode"),
                                 Message = (string)e.Element("error")
                             }).ToList();

                customers.CustomerList = customerList;
                customers.ErrorList = errorList;

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return customers;
        }

        /// <summary>
        /// Get a list of SubscriptionItem objects based on an item XML node in a XElement object
        /// </summary>
        /// <param name="item">A XElement object representing a node of XML items</param>
        /// <returns>A list of SubscriptionItem objects</returns>
        private static List<SubscriptionItem> getSubscriptionItems(XElement item)
        {
            List<SubscriptionItem> subscriptionItemList = new List<SubscriptionItem>();

            try
            {
                if (item != null && item.Descendants("item") != null)
                {
                    subscriptionItemList = (from si in item.Descendants("item")
                                            select new SubscriptionItem
                                            {
                                                ID = (Guid)si.Attribute("id"),
                                                Code = (string)si.Attribute("code"),
                                                Name = (string)si.Element("name"),
                                                Quantity = (int)si.Element("quantity"),
                                                CreatedDateTime = si.Element("createdDatetime") == null ? (DateTime?)null : (DateTime?)si.Element("createdDatetime"),
                                                ModifiedDateTime = (DateTime?)si.Element("modifiedDatetime")
                                            }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return subscriptionItemList;
        }

        /// <summary>
        /// Get a list of charges based on a charge XML node in a XElement object
        /// </summary>
        /// <param name="charges">A XElement object representing a node of XML charges</param>
        /// <returns>A list of Charge objects</returns>
        private static List<Charge> getCharges(XElement charges)
        {
            List<Charge> chargeList = new List<Charge>();

            try
            {
                if (charges != null && charges.Descendants("charge") != null)
                {
                    chargeList = (from ch in charges.Descendants("charge")
                                  select new Charge
                                  {
                                      ID = string.IsNullOrEmpty(ch.Attribute("id").Value) ? (Guid?)null : (Guid?)ch.Attribute("id"),
                                      Code = (string)ch.Attribute("code"),
                                      Type = (string)ch.Element("type"),
                                      Quantity = (int)ch.Element("quantity"),
                                      EachAmount = (float)ch.Element("eachAmount"),
                                      Description = (string)ch.Element("description"),
                                      CreatedDateTime = (DateTime)ch.Element("createdDatetime")
                                  }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return chargeList;
        }

        /// <summary>
        /// Get a list of invoices based on an invoice XML node in a XElement object
        /// </summary>
        /// <param name="invoices">A XElement object representing a node of XML invoices</param>
        /// <returns>A list of Invoice objects</returns>
        private static List<Invoice> getInvoiceList(XElement invoices)
        {
            List<Invoice> invoiceList = new List<Invoice>();

            try
            {
                if (invoices != null && invoices.Descendants("invoice") != null)
                {
                    invoiceList = (from i in invoices.Descendants("invoice")
                                   select new Invoice
                                   {
                                       ID = (Guid)i.Attribute("id"),
                                       Number = (int)i.Element("number"),
                                       Type = (string)i.Element("type"),
                                       BillingDateTime = (DateTime)i.Element("billingDatetime"),
                                       PaidTransactionId = string.IsNullOrEmpty(i.Element("paidTransactionId").Value) ? (Guid?)null : (Guid?)i.Element("paidTransactionId"),
                                       CreatedDateTime = (DateTime)i.Element("createdDatetime"),
                                       Charges = getCharges(i.Element("charges"))
                                   }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return invoiceList;
        }

        /// <summary>
        /// Get a list of plan items based on an items XML node in a XElement object
        /// </summary>
        /// <param name="items">A XElement object representing a node of XML items</param>
        /// <returns>A list of PlanItem objects</returns>
        private static List<PlanItem> getPlanItemsList(XElement items)
        {
            List<PlanItem> planItemList = new List<PlanItem>();

            try
            {
                if (items != null && items.Descendants("item") != null)
                {
                    planItemList = (from pi in items.Descendants("item")
                                    select new PlanItem
                                    {
                                        ID = (Guid)pi.Attribute("id"),
                                        Code = (string)pi.Attribute("code"),
                                        Name = (string)pi.Element("name"),
                                        QuantityIncluded = (int)pi.Element("quantityIncluded"),
                                        IsPeriodic = (bool)pi.Element("isPeriodic"),
                                        OverageAmount = (float)pi.Element("overageAmount"),
                                        CreatedDateTime = (DateTime)pi.Element("createdDatetime")
                                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return planItemList;
        }

        /// <summary>
        /// Get a list of subscription plans based on a plan XML node in a XElement object
        /// </summary>
        /// <param name="plans">A XElement object representing a node of XML plans</param>
        /// <returns>A list of SubscriptionPlan objects</returns>
        private static List<SubscriptionPlan> getSubscriptionPlanList(XElement plans)
        {
            List<SubscriptionPlan> subscriptionPlanList = new List<SubscriptionPlan>();

            try
            {
                if (plans != null && plans.Descendants("plan") != null)
                {
                    subscriptionPlanList = (from sp in plans.Descendants("plan")
                                            select new SubscriptionPlan
                                            {
                                                ID = (Guid)sp.Attribute("id"),
                                                Code = (string)sp.Attribute("code"),
                                                Name = (string)sp.Element("name"),
                                                Description = (string)sp.Element("description"),
                                                IsActive = (bool)sp.Element("isActive"),
                                                TrialDays = (int)sp.Element("trialDays"),
                                                BillingFrequency = (string)sp.Element("billingFrequency"),
                                                BillingFrequencyPer = (string)sp.Element("billingFrequencyPer"),
                                                BillingFrequencyUnit = (string)sp.Element("billingFrequencyUnit"),
                                                BillingFrequencyQuantity = (string)sp.Element("billingFrequencyQuantity"),
                                                SetupChargeCode = (string)sp.Element("setupChargeCode"),
                                                SetupChargeAmount = (float)sp.Element("setupChargeAmount"),
                                                RecurringChargeCode = (string)sp.Element("recurringChargeCode"),
                                                RecurringChargeAmount = (float)sp.Element("recurringChargeAmount"),
                                                CreatedDateTime = (DateTime)sp.Element("createdDatetime"),
                                                PlanItems = getPlanItemsList(sp.Element("items"))
                                            }).ToList();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return subscriptionPlanList;
        }

        /// <summary>
        /// Get a list of subscriptions based on a subscriptions XML node in a XElement object
        /// </summary>
        /// <param name="subscriptions">A XElement object representing a node of XML subscriptions</param>
        /// <returns>A list of Subscription objects</returns>
        private static List<Subscription> getSubscriptionList(XElement subscriptions)
        {
            List<Subscription> subscriptionList = new List<Subscription>();

            try
            {
                if (subscriptions != null && subscriptions.Descendants("subscriptions") != null)
                {
                    subscriptionList = (from s in subscriptions.Descendants("subscription")
                                        select new Subscription
                                        {
                                            ID = (Guid)s.Attribute("id"),
                                            SubscriptionsPlans = getSubscriptionPlanList(s.Element("plans")),
                                            GatewayToken = (string)s.Element("gatewayToken"),
                                            CCFirstName = (string)s.Element("ccFirstName"),
                                            CCLastName = (string)s.Element("ccLastName"),
                                            CCZip = (string)s.Element("ccZip"),
                                            CCType = (string)s.Element("ccType"),
                                            CCLastFour = string.IsNullOrEmpty(s.Element("ccLastFour").Value) ? (int?)null : (int?)s.Element("ccLastFour"),
                                            CCExpirationDate = string.IsNullOrEmpty(s.Element("ccExpirationDate").Value) ? (DateTime?)null : (DateTime?)s.Element("ccExpirationDate"),
                                            CanceledDateTime = string.IsNullOrEmpty(s.Element("canceledDatetime").Value) ? (DateTime?)null : (DateTime?)s.Element("canceledDatetime"),
                                            CreatedDateTime = (DateTime)s.Element("createdDatetime"),
                                            SubscriptionItems = getSubscriptionItems(s.Element("items")),
                                            Invoices = getInvoiceList(s.Element("invoices")),
                                        }).ToList();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return subscriptionList;
        }

        /// <summary>
        /// Will format the string into a acceptable month date format for CG
        /// </summary>
        /// <param name="ccMonth">The month in a string to be parsed</param>
        /// <returns>A formatted month string</returns>
        private static string formatMonth(string ccMonth)
        {
            string returnMonth = ccMonth;

            if (ccMonth.Length == 1)
            {
                returnMonth = "0" + ccMonth;
            }

            return returnMonth;
        }
    }
}
