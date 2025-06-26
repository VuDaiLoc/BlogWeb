using System.ComponentModel.DataAnnotations.Schema;

namespace BlogShare.Web.Models
{

    [NotMapped]
    public class QrLoginRequest
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public string AuthToken { get; set; }
    }
}
