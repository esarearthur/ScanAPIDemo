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

        private class FP_DATA
        {
            public int FP_ID { get; set; } // or whatever
            public string FP_NAME { get; set; }  // or whatever
            public byte[] FP_BLOB { get; set; }  // or whatever
        }

        [HttpPost]
        [ResponseType(typeof(FingerPrintDetails))]
        public async Task<IHttpActionResult> PostFingerPrintDetails(FingerPrintDetails fingerPrintDetails)
        {
            FP_DATA fp = new FP_DATA();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Convert scanned byte to image - source
            // --------------------------------------

            // Convert byte to image
            Image img_src = byteArrayToImage(fingerPrintDetails.FP_BLOB01);

            // Extract features from finger print image
            Fingerprint fp1 = new Fingerprint();
            fp1.AsBitmap = new Bitmap(img_src);
            Person SourcePerson = new Person();
            SourcePerson.Fingerprints.Add(fp1);
            Afis.Extract(SourcePerson);

            // Get finger print image from database
            var it = db.FingerPrintDetails.AsQueryable();
            var items = db.FingerPrintDetails.Select(f => new FP_DATA
            {
                FP_ID = f.FP_ID,
                FP_NAME = f.FP_NAME,
                FP_BLOB = f.FP_BLOB01 // Finger print image (BLOB)
            });

            // Convert byte to image
            Image img_cmp = byteArrayToImage(fp.FP_BLOB);

            // Extract features from finger print image
            Fingerprint fp2 = new Fingerprint();
            fp1.AsBitmap = new Bitmap(img_cmp);
            Person DatabasePerson = new Person();
            DatabasePerson.Fingerprints.Add(fp2);
            Afis.Extract(DatabasePerson);

            float Score = Afis.Verify(SourcePerson, DatabasePerson);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return CreatedAtRoute("DefaultApi", new { id = fingerPrintDetails.Id }, fingerPrintDetails);
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