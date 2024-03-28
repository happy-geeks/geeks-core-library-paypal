using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class AuthenticationResponseModel
{
    [JsonProperty("scope")]
    [JsonPropertyName("scope")]
    public string Scope { get; set; }
    [JsonProperty("nonce")]
    [JsonPropertyName("nonce")]
    public string Nonce { get; set; }
    [JsonProperty("access_token")]
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    [JsonProperty("token_type")]
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
    [JsonProperty("app_id")]
    [JsonPropertyName("app_id")]
    public string AppId { get; set; }
    [JsonProperty("expires_in")]
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [Newtonsoft.Json.JsonIgnore()]
    public DateTime ExpiresAt { get; set; }
    [JsonProperty("error")]
    [JsonPropertyName("error")]
    public string Error { get; set; }
    [JsonProperty("error_description")]
    [JsonPropertyName("error_description")]
    public string ErrorDescription { get; set; }
    
    
    public AuthenticationResponseModel()
    {
        ExpiresAt = DateTime.Now.AddSeconds(ExpiresIn);
    }
}