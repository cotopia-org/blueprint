using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace cotopia_server.modules.auth.request
{
    public class SignoutRequest
    {
        [Required]
        public string sessionId { get; set; }
    }
}
