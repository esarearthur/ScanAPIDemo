using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;

namespace WebApp.Controllers
{
    public class ExeController : Controller
    {
        // GET: Exe
        public ActionResult Index(String id)
        {
            int exitcode;
            try
            {
                Process process = new Process();
                //process.StartInfo.FileName = Server.MapPath("~/Content/Finger/ScanAPIDemo.exe");
                process.StartInfo.FileName = @"C:/Users/Ronald Arthur/Desktop/FingerPrint/ScanAPIDemo/bin/x64/Debug/ScanAPIDemo.exe";
                process.StartInfo.Arguments = id + " 123";
                process.Start();
                process.WaitForExit();
                exitcode = process.ExitCode;


                ViewBag.Result = "Done";
            }
            catch(Exception ex)
            {
                ViewBag.Result = ex.Message;
            }
            return View();
        }
    }
}