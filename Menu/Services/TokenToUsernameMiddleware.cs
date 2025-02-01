namespace Menu.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

public class TokenToUsernameMiddleware
{
    private readonly RequestDelegate _next;


    public TokenToUsernameMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Step 1: Extract token from Authorization header
        var token = context.Request.Headers["token"].ToString();

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 400; // Bad Request
            await context.Response.WriteAsync("Token is required.");
            return;
        }

        // Step 2: Call external service to convert token to username
        var username = await GetUsernameFromTokenAsync(token);

        if (string.IsNullOrEmpty(username))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Invalid token.");
            return;
        }

        // Step 3: Store username in HttpContext.Items so it's accessible in the controller
        context.Items["Username"] = username;

        // Step 4: Proceed to the next middleware or controller
        await _next(context);
    }

    private async Task<string> GetUsernameFromTokenAsync(string token)
    {
        var client = new HttpClient();
        var url = "http://3.107.99.30:4000/authentication/getUsername";
        try
        {
            var data = new {
                Token = token
            };
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();  // Throw an exception if the status code is not 2xx

            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Request error: " + e.Message);
            return "";
        }
        // Here you would implement the logic to fetch the username from the token, e.g., by calling an external service
          // For illustration
    }
}
