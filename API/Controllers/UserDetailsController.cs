using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

using API.Models;

namespace API.Controllers
{
    [Authorize]
    public class UserDetailsController : ApiController
    {
        private APIContext db = new APIContext();

        // POST: UserDetails
        [HttpPost]
        [ResponseType(typeof(UserDetails))]
        public async Task<IHttpActionResult> GetUserDetails(UserDetails UD)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            FingerPrintDetails fp;
            if (UD.PK > 0)
            {
                fp = await db.FingerPrintDetails.FindAsync(UD.PK);
                return Ok(fp.FP_NAME);
            }
            return Ok("Fingerprint not found");
        }
    }
}