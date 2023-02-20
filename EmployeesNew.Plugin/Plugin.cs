using Newtonsoft.Json.Linq;
using PhoneApp.Domain.Attributes;
using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EmployeesNew.Plugin
{
    [Author(Name = "Morozov Alexander")]
    public class Plugin : IPluggable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static string webValue { get; set; }
        public static string WebValue
        {
            get
            {
                if (webValue == null)
                    AsyncTask().Wait();
                return webValue;
            }
        }

        public static async Task AsyncTask()
        {
            HttpClient httpClient = new HttpClient();
            string request = "https://dummyjson.com/users";
            HttpResponseMessage response = (await httpClient.GetAsync(request)).EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            webValue = result;
        }

        public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
        {
            logger.Info("Loading new employees from API");
            var result = WebValue;
            JObject jObject = JObject.Parse(result);

            List<string> allNamesPhones = jObject["users"].Select(t => $"{t["firstName"]},{t["lastName"]},{t["phone"]}").ToList();
            List<EmployeesDTO> employeesList = new List<EmployeesDTO>();

            foreach (var emp in allNamesPhones)
            {
                EmployeesDTO employee = new EmployeesDTO();
                employee.Name = $"{emp.Split(',')[0]} {emp.Split(',')[1]}";
                employee.AddPhone(emp.Split(',')[2]);
                employeesList.Add(employee);
            }
            logger.Info($"Loaded {employeesList.Count()} employees");
            return employeesList.Cast<DataTransferObject>();
        }
    }
}
