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
using System.Text;

namespace Confer.CheddarGetter
{

    public enum PlanCodeEnum
    {
        Free = 1,
        Trial,
        Good,
        Better,
        Best,
        Other
    }

    public enum ProductItemCode
    {
        USER = 1,
        MB
    }

    public class PlanItem
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public double QuantityIncluded { get; set; }
        public bool IsPeriodic { get; set; }
        public float OverageAmount { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }

    public class SubscriptionPlan
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int TrialDays { get; set; }
        public string BillingFrequency { get; set; }
        public string BillingFrequencyPer { get; set; }
        public string BillingFrequencyUnit { get; set; }
        public string BillingFrequencyQuantity { get; set; }
        public string SetupChargeCode { get; set; }
        public float SetupChargeAmount { get; set; }
        public string RecurringChargeCode { get; set; }
        public float RecurringChargeAmount { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public List<PlanItem> PlanItems { get; set; }
    }

    public class Charge
    {
        public Guid? ID { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public float EachAmount { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }

    public class Invoice
    {
        public Guid ID { get; set; }
        public int Number { get; set; }
        public string Type { get; set; }
        public DateTime BillingDateTime { get; set; }
        public Guid? PaidTransactionId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public List<Charge> Charges { get; set; }
    }

    public class SubscriptionItem
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }

    public class Subscription
    {
        public Guid ID { get; set; }
        public List<SubscriptionPlan> SubscriptionsPlans { get; set; }
        public string GatewayToken { get; set; }
        public string CCFirstName { get; set; }
        public string CCLastName { get; set; }
        public string CCZip { get; set; }
        public string CCType { get; set; }
        public int? CCLastFour { get; set; }
        public DateTime? CCExpirationDate { get; set; }
        public DateTime? CanceledDateTime { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public List<SubscriptionItem> SubscriptionItems { get; set; }
        public List<Invoice> Invoices { get; set; }
    }

    public class Customers
    {
        public List<Customer> CustomerList { get; set; }
        public List<CGError> ErrorList { get; set; }
    }

    public class Customer
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string GatewayToken { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public List<Subscription> Subscriptions { get; set; }
    }

    public class CustomerPost
    {
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public PlanCodeEnum PlanCode { get; set; }
        public string CCFirstName { get; set; }
        public string CCLastName { get; set; }
        public string CCNumber { get; set; }
        public string CCExpiration { get; set; }
        public string CCExpMonth { get; set; } 
        public string CCExpYear { get; set; }
        public string CCCardCode { get; set; }
        public string CCZip { get; set; }
    }

    public class CustomChargePost
    {
        public string CustomerCode { get; set; }
        public string ItemCode { get; set; }
        public string ChargeCode { get; set; }
        public int Quantity { get; set; }
        public int EachAmount { get; set; }
        public string Description { get; set; }
    }

    public class PlanUpdatePost
    {
        public string CustomerCode { get; set; }
        public PlanCodeEnum PlanCode { get; set; }
    }

    public class CGError
    {
        public string ID { get; set; }
        public string Code { get; set; }
        public string AuxCode { get; set; }
        public string Message { get; set; }
    }
}
