using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.IO;
using Line.Messaging;
using Line.Messaging.Webhooks;

string channelAccessToken = "";
using (var sr = new StreamReader("../LINEToken.txt"))
{
    channelAccessToken = sr.ReadLine().ToString();
}

HttpClient httpClient = new HttpClient();
string url = "https://api.line.me/v2/bot/message/narrowcast";
var path = "../temperature.csv";
string lastLine = null;
using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
{
    byte[] buffer = new byte[1];
    long filePos = fs.Length;

    while (filePos > 0)
    {
        fs.Seek(--filePos, SeekOrigin.Begin);
        fs.Read(buffer, 0, 1);

        if (buffer[0] == '\n' && filePos < fs.Length - 1)
        {
            break;
        }
    }

    if (filePos != fs.Length - 1)
    {
        filePos++;
    }

    fs.Seek(filePos, SeekOrigin.Begin);
    using (StreamReader sr = new StreamReader(fs))
    {
        lastLine = sr.ReadLine();
    }
}

if (lastLine != null)
{
    Console.WriteLine("Last line is: " + lastLine);
}
else
{
    Console.WriteLine("File is empty or does not exist.");
}
var data = lastLine.Split(',');
// メッセージを構築
var message = new
{
    messages = new object[]
    {
        new
        {
            type = "text",
            text = $"現在の室温は{data[1]}℃です"
        }
    }
};

// JSONにシリアライズ
var json = JsonSerializer.Serialize(message);

// リクエストの構築
var request = new HttpRequestMessage(HttpMethod.Post, url)
{
    Content = new StringContent(json, Encoding.UTF8, "application/json")
};
request.Headers.Add("Authorization", $"Bearer {channelAccessToken}");

// リクエストの送信
var response = await httpClient.SendAsync(request);

// 応答のチェック
if (response.IsSuccessStatusCode)
{
    Console.WriteLine("Message sent successfully!");
}
else
{
    Console.WriteLine($"Error sending message: {response.StatusCode}");
}
