using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Drawing;

using SourceAFIS.Simple;

using API.Models;

namespace API.Controllers
{
    public class VerifyController : ApiController
    {
        private APIContext db = new APIContext();
        private AfisEngine Afis = new AfisEngine();

        [Serializable]
        class MyPerson : Person
        {
            public String Name;
        }

        private MyPerson Enroll(Bitmap fprint, String name)
        {
            Fingerprint fp = new Fingerprint();
            fp.AsBitmap = new Bitmap(fprint);

            MyPerson person = new MyPerson();
            person.Name = name;
            person.Fingerprints.Add(fp);

            Afis.Extract(person);

            return person;
        }

        /*private class FP_DATA
        {
            public int FP_ID { get; set; } // or whatever
            public string FP_NAME { get; set; }  // or whatever
            public byte[] FP_BLOB { get; set; }  // or whatever
        }*/

        [HttpPost]
        [ResponseType(typeof(FingerPrintDetails))]
        public async Task<IHttpActionResult> VerifyFingerPrintDetails(FingerPrintDetails fingerPrintDetails)
        {
            List<MyPerson> database = new List<MyPerson>();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(fingerPrintDetails.FP_BLOB01 == null)
            {
                return BadRequest();
            }

            Image img_src = byteArrayToImage(fingerPrintDetails.FP_BLOB01);

            MyPerson probe = Enroll((Bitmap) img_src, "Visitor #12345");

            await db.FingerPrintDetails.LoadAsync();
            var fpp = db.FingerPrintDetails.Local.Select(
                x => new { x.FP_NAME, x.FP_BLOB01} ).ToList();

            foreach(var v in fpp)
            {
                database.Add(Enroll((Bitmap)byteArrayToImage(v.FP_BLOB01), v.FP_NAME));
            }

            /*var fingerprintrec = from p in db.FingerPrintDetails.Local
                                 select p.FP_NAME;

            var it = db.FingerPrintDetails.SqlQuery("SELECT FP_ID, FP_NAME, FP_BLOB1 FROM dbo.FingerPrintDetails");

            foreach(var i in it)
            {
                database.Add(Enroll((Bitmap)byteArrayToImage(i.FP_BLOB01), i.FP_NAME));
            }*/

            Afis.Threshold = 10;
            Console.WriteLine("Identifying {0} in database of {1} persons...", probe.Name, database.Count);
            MyPerson match = Afis.Identify(probe, database).FirstOrDefault() as MyPerson;

            return Ok(match.Name);
        }

        private byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        private Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }
    }
}