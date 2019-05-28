
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiskInfoAPI.Models;
using DiskInfoAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

internal class NotificationService : IHostedService, IDisposable
{
    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly DiskService _diskService = new DiskService();

    private Timer _timer;
    
    public NotificationService(ILogger<NotificationService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[NotificationService] Starting.");

        _timer = new Timer(SendNotification, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        return Task.CompletedTask;
    }

    private void SendNotification(object state)
    {
        bool send = false;
        string bodyText = "";

        _logger.LogInformation("[NotificationService] Start sending a notification.");

        List<Disk> disks = _diskService.Get();

        foreach (Disk d in disks)
        {
            double value = ((double)d.TotalFreeSpace / (double)d.TotalSize) * 100;
            int percentage = Convert.ToInt32(Math.Round(100 - value, 0));

            if (percentage >= 90)
            {
                send = true;
                bodyText = "One or more of your disks are at or above 90%";
            }
        }

        var payload = new 
        {
            to = "/topics/all",
            priority = "high",
            content_available = true,
            notification = new {
                title = "Disk capacity warning",
                body = bodyText,
                sound = "default"
            },
            data = new {
                click_action = "FLUTTER_NOTIFICATION_CLICK",
                id = "1",
                status = "done"
            },
        };
        
        string postBody = JsonConvert.SerializeObject(payload).ToString();
        
        Byte[] byteArray = Encoding.UTF8.GetBytes(postBody);

        if (send)
        {
            try
            {
                string server_api_key = _config["Google:FirebaseApiKey"];
                string sender_id = _config["Google:SenderId"];

                WebRequest tRequets = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequets.Method = "post";
                tRequets.ContentType = "application/json";
                tRequets.Headers.Add($"Authorization: key={server_api_key}");
                tRequets.Headers.Add($"Sender: id={sender_id}");

                tRequets.ContentLength = byteArray.Length;
                Stream dataStream = tRequets.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse tResponse = tRequets.GetResponse();
                dataStream = tResponse.GetResponseStream();
                StreamReader tReader = new StreamReader(dataStream);

                string sResponseFromServer = tReader.ReadToEnd();

                if (sResponseFromServer != null)
                {
                    _logger.LogInformation("[NotificationService] Notification is send.");
                }

                tReader.Close();
                dataStream.Close();
                tResponse.Close();
            }
            catch (System.Exception e)
            {
                _logger.LogInformation("[NotificationService] Error sending notification. Status Code: {0}", e);
            }
        }
        else
        {
            _logger.LogInformation("[NotificationService] No notification send because there are no drives above 90%");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[NotificationService] Stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}