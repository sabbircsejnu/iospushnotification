using Newtonsoft.Json.Linq;
using PushSharp.Apple;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace iospushnotification.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult SendNotification()
        {
            try
            {
                string deviceId = Request["deviceToken"];
                string certificateFilePwd = Request["certificateFilePwd"];
                var file = Request.Files["p12File"];

                //Upload file into a directory
                string path = Path.Combine(Server.MapPath("~/Certificates/"), "CertificateName.p12");
                file.SaveAs(path);

                //Get Certificate
                //You will get this certificate from apple account
                var appleCert = System.IO.File.ReadAllBytes(HostingEnvironment.MapPath("~/Certificates/CertificateName.p12"));


                // Configuration
                var config = new PushSharp.Apple.ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Production, appleCert, certificateFilePwd);
                config.ValidateServerCertificate = false;

                // Create a new broker
                var apnsBroker = new ApnsServiceBroker(config);

                //Wire up events
                apnsBroker.OnNotificationFailed += (notification, aggregateEx) =>
                {
                    aggregateEx.Handle(ex =>
                    {
                        if (ex is ApnsNotificationException)
                        {
                            var notificationException = (ApnsNotificationException)ex;

                            // Deal with the failed notification
                            var apnsNotification = notificationException.Notification;
                            var statusCode = notificationException.ErrorStatusCode;
                            string desc = $"Notification Failed: ID={apnsNotification.Identifier},Code={statusCode}";
                            Console.WriteLine(desc);
                        }
                        else
                        {
                            string desc = $"Notification Failed for some unknown reason: ID={ex.InnerException}";
                            Console.WriteLine(desc);
                        }
                        return true;
                    });
                    ViewBag.Message = "Notification sent successfully.";
                };

                apnsBroker.OnNotificationSucceeded += (notification) =>
                {
                    ViewBag.Message = "Notification sent failed.";
                };

                var fbs = new FeedbackService(config);
                fbs.FeedbackReceived += (string deviceToken, DateTime timestamp) =>
                {

                };

                //All apns configuration done
                //Now start the apns broker
                apnsBroker.Start();

                if (!string.IsNullOrEmpty(deviceId))
                {
                    apnsBroker.QueueNotification(new ApnsNotification
                    {
                        DeviceToken = deviceId,
                        Payload = JObject.Parse(("{\"aps\": {\"alert\": {\"title\":\"Notification Title\",\"body\" : \"" + "Message Body" + "\"},\"badge\": \"" + 0 + "\",\"content-available\": \"1\",\"sound\": \"default\"},\"notification_details\": {}} "))
                    });

                }
                apnsBroker.Stop();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return RedirectToAction("Index");
    }
    }
}