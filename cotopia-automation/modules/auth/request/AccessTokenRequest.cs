using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace cotopia_server.modules.auth.request
{
    public class AccessTokenRequest
    {
        [Required]
        [DefaultValue("")]
        public string refreshToken { get; set; }
    }
}
