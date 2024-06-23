using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace cotopia_server.modules.auth.request
{
    public class ForgetPasswordRequest
    {
        [EmailAddress]
        [Required]
        [DefaultValue("example@gmail.com")]
        public string email { get; set; }
    }
}
