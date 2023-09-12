using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CaseSimulatorBot.Configs
{   
    internal class ConfigContext
    {

        private IConfig _config;

        public ConfigContext(IConfig config)
        {
            _config = config;
        }

        public IConfig? GetInstance()
        {
            string fileName = _config.FileName;
            string template = _config.Template;
            bool fillRequired = _config.IsFillRequired;

            InitFile(fileName, template, fillRequired);
            string data = ReadFile(fileName);
            object config = JsonConvert.DeserializeObject(data, _config.GetType())!;
            return (IConfig?)config;
        }

        private void InitFile(string fileName, string data, bool fillRequired)
        {         
            if (!File.Exists(fileName))
            {
                using (StreamWriter sr = new StreamWriter(fileName))
                {
                    sr.WriteAsync(data);
                }
                if (fillRequired)
                    StopApplication(fileName);
            }
        }

        private void StopApplication(string fileName)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Stopping the app...");
            Console.ForegroundColor = ConsoleColor.White;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"REASON: Configuration file '{fileName}' needs to be filled out after creation");
            Console.ForegroundColor = ConsoleColor.White;

            Environment.Exit(0);
        }

        private string ReadFile(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName, new UTF8Encoding(false)))
            {
                var data = reader.ReadToEnd();
                return data;
            }
        }   
    }
}
