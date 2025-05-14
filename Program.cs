using System;
using System.IO;
using System.Net.NetworkInformation;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

class ConfigManager
{
    public static string GetToken()
    {
        try
        {
            var json = File.ReadAllText("config.json");
            var config = JObject.Parse(json);
            return config["TelegramToken"]?.ToString() ?? "Token tidak ditemukan";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error membaca token: {ex.Message}");
            return string.Empty;
        }
    }
}

class Program
{
    static async System.Threading.Tasks.Task Main()
    {
        string token = ConfigManager.GetToken();
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Token tidak valid. Periksa config.json!");
            return;
        }

        var botClient = new TelegramBotClient(token);
        botClient.StartReceiving(UpdateHandler, ErrorHandler);
        Console.WriteLine("Bot sedang berjalan...");
        await System.Threading.Tasks.Task.Delay(-1);
    }

    private static async System.Threading.Tasks.Task UpdateHandler(ITelegramBotClient botClient, Update update, System.Threading.CancellationToken cancellationToken)
    {
        if (update.Message?.Text == "/getip")
        {
            string ipAddress = GetLocalIPAddress();
            await botClient.SendMessage(update.Message.Chat.Id, $"IP Address WLAN: {ipAddress}");
        }
    }

    private static string GetLocalIPAddress()
    {
        string ip = "Tidak dapat menemukan IP";
        foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && netInterface.OperationalStatus == OperationalStatus.Up)
            {
                foreach (var addr in netInterface.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ip = addr.Address.ToString();
                        break;
                    }
                }
            }
        }
        return ip;
    }

    private static System.Threading.Tasks.Task ErrorHandler(ITelegramBotClient botClient, Exception exception, System.Threading.CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
