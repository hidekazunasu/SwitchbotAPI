using System.Text;
using System.Security.Cryptography;
using System.Text.Json;

namespace switchbotapi;

class switchbotapi
{
    public static async Task Main()
    {
        var token = string.Empty;
        var secret = string.Empty;

        using (var sr = new StreamReader("../token.txt"))
        {
            token = sr.ReadLine();
            secret = sr.ReadLine();
        }
        int delayInMilliseconds = 60000 * 5;
        using (var sw = new StreamWriter("../temperature.csv"))
        {
            while (true)
            {
                DateTime dt1970 = new DateTime(1970, 1, 1);
                DateTime current = DateTime.Now;
                TimeSpan span = current - dt1970;
                long time = Convert.ToInt64(span.TotalMilliseconds);
                string nonce = Guid.NewGuid().ToString();
                string data = token + time.ToString() + nonce;
                Encoding utf8 = Encoding.UTF8;
                HMACSHA256 hmac = new HMACSHA256(utf8.GetBytes(secret));
                string signature = Convert.ToBase64String(hmac.ComputeHash(utf8.GetBytes(data)));

                //Create http client
                HttpClient client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, @"https://api.switch-bot.com/v1.1/devices/E38348A4EC7D/status");
                request.Headers.TryAddWithoutValidation(@"Authorization", token);
                request.Headers.TryAddWithoutValidation(@"sign", signature);
                request.Headers.TryAddWithoutValidation(@"nonce", nonce);
                request.Headers.TryAddWithoutValidation(@"t", time.ToString());
                var response = await client.SendAsync(request);
                var str = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<Rootobject>(str);
                var now = DateTime.Now;
                var line = now.ToString("yyyyMMdd HHmmss") + "," + responseObject.body.temperature.ToString();
                Console.WriteLine(line);
                sw.WriteLine(line.ToString());
                sw.Flush();
                await Task.Delay(delayInMilliseconds);
            }
        }

    }
}


public class Rootobject
{
    public int? statusCode { get; set; }
    public Body? body { get; set; }
    public string? message { get; set; }
}

public class Body
{
    public string? deviceId { get; set; }
    public string? deviceType { get; set; }
    public string? hubDeviceId { get; set; }
    public int? humidity { get; set; }
    public float? temperature { get; set; }
    public string? version { get; set; }
    public int? battery { get; set; }
}
