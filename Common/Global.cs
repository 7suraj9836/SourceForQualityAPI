using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Common
{
    public class Global
    {
        public static string ConnectionString { get; set; }
        public static string SmtpCredId { get; set; }
        public static string SmtpCredPassword { get; set; }
        public static string SmtpFromEmail { get; set; }
        public static string SmtpSenderEmail { get; set; }
        public static string SmtpHost { get; set; }
        public static int SmtpPort { get; set; }

        public static async Task LoadSmtpSettings()
        {
            //import json file 
            try
            {
                string fdata = System.IO.File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Common", @"SmtpSettings.json"));
                var data = JsonConvert.DeserializeObject<IDictionary<string, object>>(fdata);
                SmtpCredId = data["SmtpCredId"].ToString();
                SmtpCredPassword = data["SmtpCredPassword"].ToString();
                SmtpPort = Convert.ToInt32(data["SmtpPort"]);
                SmtpHost = data["SmtpHost"].ToString();
                SmtpFromEmail = data["SmtpFromEmail"].ToString();
                SmtpSenderEmail = data["SmtpSenderEmail"].ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
