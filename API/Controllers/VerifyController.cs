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
    [Authorize]
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
                x => new { x.Id, x.FP_ID, x.FP_BLOB01} ).ToList();

            /*if (fpp.Count < 1)
                return Ok("-1 No finger print in db");*/

            foreach(var v in fpp)
            {
                database.Add(Enroll((Bitmap)byteArrayToImage(v.FP_BLOB01), v.Id.ToString()));
            }

            Afis.Threshold = 45;
            Console.WriteLine("Identifying {0} in database of {1} persons...", probe.Name, database.Count);
            MyPerson match = Afis.Identify(probe, database).FirstOrDefault() as MyPerson;

            int nn = 0;
            
            if (match == null)
                return Ok("Finger print not found");

            var fp = await db.FingerPrintDetails.FindAsync(int.Parse(match.Name));

            float score = Afis.Verify(probe, match);
            return Ok(fp.Id.ToString() + " " + fp.FP_NAME + " Score: " + score.ToString());
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