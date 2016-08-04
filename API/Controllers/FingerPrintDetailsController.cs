using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using API.Models;

namespace API.Controllers
{
    [Authorize]
    public class FingerPrintDetailsController : ApiController
    {
        private APIContext db = new APIContext();

        // GET: api/FingerPrintDetails
        public IQueryable<FingerPrintDetails> GetFingerPrintDetails()
        {
            return db.FingerPrintDetails;
        }

        // GET: api/FingerPrintDetails/5
        [ResponseType(typeof(FingerPrintDetails))]
        public async Task<IHttpActionResult> GetFingerPrintDetails(int id)
        {
            FingerPrintDetails fingerPrintDetails = await db.FingerPrintDetails.FindAsync(id);

            if (fingerPrintDetails == null)
            {
                return NotFound();
            }

            return Ok(fingerPrintDetails);
        }

        // PUT: api/FingerPrintDetails/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutFingerPrintDetails(int id, FingerPrintDetails fingerPrintDetails)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != fingerPrintDetails.Id)
            {
                return BadRequest();
            }

            db.Entry(fingerPrintDetails).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FingerPrintDetailsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/FingerPrintDetails
        [ResponseType(typeof(FingerPrintDetails))]
        public async Task<IHttpActionResult> PostFingerPrintDetails(FingerPrintDetails fingerPrintDetails)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.FingerPrintDetails.Add(fingerPrintDetails);
            var items = db.FingerPrintDetails.Select(f => new {f.FP_ID, f.FP_BLOB01}).Distinct();

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (FingerPrintDetailsExists(fingerPrintDetails.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = fingerPrintDetails.Id }, fingerPrintDetails);
        }

        // DELETE: api/FingerPrintDetails/5
        [ResponseType(typeof(FingerPrintDetails))]
        public async Task<IHttpActionResult> DeleteFingerPrintDetails(int id)
        {
            FingerPrintDetails fingerPrintDetails = await db.FingerPrintDetails.FindAsync(id);
            if (fingerPrintDetails == null)
            {
                return NotFound();
            }

            db.FingerPrintDetails.Remove(fingerPrintDetails);
            await db.SaveChangesAsync();

            return Ok(fingerPrintDetails);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool FingerPrintDetailsExists(int id)
        {
            return db.FingerPrintDetails.Count(e => e.Id == id) > 0;
        }
    }
}