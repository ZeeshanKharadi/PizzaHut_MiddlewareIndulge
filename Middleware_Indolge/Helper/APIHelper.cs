using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public static class ApiHelper
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static async Task<string> PostAsync(string url, object requestBody)
    {
        try
        {
            string jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            response.EnsureSuccessStatusCode(); // throws if not 2xx

            string responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        catch (Exception ex)
        {
            // You can log the exception here if needed
            return $"Error: {ex.Message}";
        }
    }

    public static async Task<string> DeleteWithBodyAsync(string url, object requestBody)
    {
        try
        {
            string jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url),
                Content = content
            };

            Console.WriteLine("Serialized Body:");
            Console.WriteLine(jsonContent);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public static async Task<string> PutAsync(string url, object requestBody)
    {
        try
        {
            string jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public static async Task<string> DeleteAsync(string url)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync(url);

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }


}
