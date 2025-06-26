using Microsoft.AspNetCore.Mvc;
using BlogShare.Web.Helpers;

namespace BlogShare.Web.Controllers
{
    public class QrController : Controller
    {
        [HttpGet("/qr")]
        public IActionResult GetQr(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("Thiếu URL");

            var qrBytes = QrHelper.GenerateQr(url);
            return File(qrBytes, "image/png");
        }
    }
}
