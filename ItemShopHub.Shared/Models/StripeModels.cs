using System.Runtime.Serialization;

namespace ItemShopHub.Shared.Models;

[DataContract]
public class CreateStripeSessionResponse
{
    [DataMember]
    public string SessionId { get; set; } = "";
    
    [DataMember]
    public string? Url { get; set; }
}

[DataContract]
public class CreateStripeSessionRequest
{
    [DataMember]
    public string BaseUrl { get; set; } = "";
    
    [DataMember]
    public string? ShippingAddress { get; set; }
    
    [DataMember]
    public string? BillingAddress { get; set; }
    
    [DataMember]
    public string? BillingStateCode { get; set; }
    
    [DataMember]
    public decimal ShippingAmount { get; set; }
    
    [DataMember]
    public string? Notes { get; set; }
}

[DataContract]
public class CreatePayPalSessionResponse
{
    [DataMember]
    public string SessionId { get; set; } = "";
    
    [DataMember]
    public string? Url { get; set; }
}

[DataContract]
public class CreatePayPalSessionRequest
{
    [DataMember]
    public string BaseUrl { get; set; } = "";
    
    [DataMember]
    public string? ShippingAddress { get; set; }
    
    [DataMember]
    public string? BillingAddress { get; set; }
    
    [DataMember]
    public string? BillingStateCode { get; set; }
    
    [DataMember]
    public decimal ShippingAmount { get; set; }
    
    [DataMember]
    public string? Notes { get; set; }
}

[DataContract]
public class CancelPaymentRequest
{
    [DataMember]
    public string StripeSessionId { get; set; } = "";
}
