using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Text;
using System.Security.Cryptography;
using System.Net.Http;
using System.Text.Json;

namespace Switchbotapi;
[ApiController]
[Route("[Controller]")]

public class GetDataController : ControllerBase
{

    [HttpGet("GetDeviceList")]
    public async Task<IActionResult> GetDeviceList()
    {
        SwitchBotService services = new SwitchBotService();
        services.SetAuth();
        HttpClient client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, @"https://api.switch-bot.com/v1.1/devices");
        request.Headers.TryAddWithoutValidation(@"Authorization", services.token);
        request.Headers.TryAddWithoutValidation(@"sign", services.signature);
        request.Headers.TryAddWithoutValidation(@"nonce", services.nonce);
        request.Headers.TryAddWithoutValidation(@"t", services.time.ToString());

        var response = await client.SendAsync(request);
        string jsonResponse = await response.Content.ReadAsStringAsync();
        Rootobject Object = JsonSerializer.Deserialize<Rootobject>(jsonResponse);
        return Ok(Object);
    }

    [HttpGet("GetStatus")]
    public async Task<IActionResult> GetStatus()
    {
        SwitchBotService services = new SwitchBotService();
        services.SetAuth();
        HttpClient client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, @"https://api.switch-bot.com/v1.1/devices/E38348A4EC7D/status");
        request.Headers.TryAddWithoutValidation(@"Authorization", services.token);
        request.Headers.TryAddWithoutValidation(@"sign", services.signature);
        request.Headers.TryAddWithoutValidation(@"nonce", services.nonce);
        request.Headers.TryAddWithoutValidation(@"t", services.time.ToString());
        var response = await client.SendAsync(request);

        return Ok(response.Content.ReadAsStringAsync());

    }


}


public class SwitchBotService
{
    public string? token { get; set; }
    public string? signature { get; set; }
    public string? nonce { get; set; }
    public long? time { get; set; }

    public void SetAuth()
    {
        string[] lines = File.ReadAllLines("../token.txt");
        var _token = lines[0];
        var _Secret = lines[1];

        if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_Secret))
        {
            throw new InvalidOperationException("Missing SwitchBot token or user secret.");
        }
        DateTime dt1970 = new DateTime(1970, 1, 1);
        DateTime current = DateTime.Now;
        TimeSpan span = current - dt1970;
        time = Convert.ToInt64(span.TotalMilliseconds);
        nonce = Guid.NewGuid().ToString();
        string data = _token + time.ToString() + nonce;
        Encoding utf8 = Encoding.UTF8;
        HMACSHA256 hmac = new HMACSHA256(utf8.GetBytes(_Secret));
        signature = Convert.ToBase64String(hmac.ComputeHash(utf8.GetBytes(data)));
        token = _token;


    }
}

public class Rootobject
{
    public int statusCode { get; set; }
    public Body body { get; set; }
    public string message { get; set; }
}

public class Body
{
    public Devicelist[] deviceList { get; set; }
    public Infraredremotelist[] infraredRemoteList { get; set; }
}

public class Devicelist
{
    public string deviceId { get; set; }
    public string deviceName { get; set; }
    public string deviceType { get; set; }
    public bool enableCloudService { get; set; }
    public string hubDeviceId { get; set; }
}

public class Infraredremotelist
{
    public string deviceId { get; set; }
    public string deviceName { get; set; }
    public string remoteType { get; set; }
    public string hubDeviceId { get; set; }
}
