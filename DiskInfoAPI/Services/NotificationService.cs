
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
using Serilog;

internal class NotificationService
{
    private readonly IConfiguration _config;
    private readonly DiskService _diskService = new DiskService();
    
    public NotificationService(IConfiguration config)
    {
        _config = config;
    }

    public void SendNotification()
    {
        bool send = false;
        string bodyText = "";

        Log.Information("[NotificationService][SendNotification] Called");

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
        try
        {
            if (send)
            {
                Log.Information("[NotificationService][SendNotification] Sending a notification");

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
                   Log.Information("[NotificationService][SendNotification] Nofication send");
                }

                tReader.Close();
                dataStream.Close();
                tResponse.Close();
            }
            else
            {
                Log.Fatal("[NotificationService][SendNotification] Failed to send notification becasue no disks above/at 90%");
            }
        }
        catch (System.Exception e) 
        {
            Log.Fatal("[NotificationService][SendNotification] Failed to send notification, status code: {0}", e);
        }
        
    }
}