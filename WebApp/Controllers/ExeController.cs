using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Threading;

// API2
using System.Threading.Tasks;

// RestSharp
using RestSharp;

// JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using WebApp.Models;

namespace WebApp.Controllers
{
    public class ExeController : Controller
    {
        private String TOKEN = String.Empty;
        private int exitcode;
        private String GetResponse = "";

        // GET: Exe
        [HttpGet]
        public ActionResult Index(String id)
        {
            try
            {
                RunAsyncGetName(id).Wait();

                ViewBag.Result = GetResponse;
            }
            catch (Exception ex)
            {
                ViewBag.Result = ex.Message;
            }
            return View();
        }

        private async Task RunAsyncGetName(String id)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            RestClient client = new RestClient("http://localhost:3293/api/token");
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("postman-token", "7b69394f-bd17-e5be-ea62-fa46a435be6f");
            request.AddHeader("cache-control", "no-cache");
            request.AddParameter("application/x-www-form-urlencoded", "grant_type=password&username=esarearthur&password=123456", ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token).ConfigureAwait(false);

            TOKEN = JsonGetKey(FormatJson(response.Content), "access_token");

            Process process = new Process();
            process.StartInfo.FileName = @"C:/Users/Ronald Arthur/Desktop/FingerPrint/ScanAPIDemo/bin/x64/Debug/ScanAPIDemo.exe";
            process.StartInfo.Arguments = id + " 0 " + TOKEN;
            process.Start();
            process.WaitForExit();
            exitcode = process.ExitCode;

            var PrimaryKey = new UserDetails()
            {
                PK = exitcode,
                NAME = "DUMMY"
            };

            client = new RestClient("http://localhost:3293/api/UserDetails");
            request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "ac68cd3a-db3a-5340-438f-f190ddd53bde");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "bearer " + TOKEN);
            request.AddParameter("application/json", JsonConvert.SerializeObject(PrimaryKey, Formatting.Indented), ParameterType.RequestBody);
            response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token).ConfigureAwait(false);
            GetResponse = response.Content.Replace("\"","");
        }

        private String FormatJson(String json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        private String JsonGetKey(String json, String key)
        {
            JObject Json_Key = JObject.Parse(json);
            return (String)Json_Key[key];
        }
    }
}