using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using mktng.Controllers;
using Newtonsoft.Json;
using System.IO;

namespace mktng.Controllers
{
    public enum NotificationType
    {
        success,
        error,
        warning,
        info
    }

    public class BaseController : Controller
    {
        public void Notify(string message, string title = "Notification", NotificationType notificationType = NotificationType.success, bool isActive = false)
        {
            var msg = new
            {
                message = message,
                title = title,
                icon = notificationType.ToString(),
                type = notificationType.ToString(),
                provider = GetProvider()
            };
            TempData["Message"] = JsonConvert.SerializeObject(msg);
        }

        private string GetProvider()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddEnvironmentVariables();
            IConfigurationRoot configuration = builder.Build();
            var value = configuration["NotificationProvider"];
            return value;
        }
    }
}
//// Notify success
//Notify("Client added successfully!", "Success", NotificationType.success);
//// Notify error
//Notify("Unauthorized access.", "Error", NotificationType.error);