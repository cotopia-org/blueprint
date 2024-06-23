
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace cotopia_server.modules.auth.request
{
    public class SignupRequest
    {
        [Required]
        public string firstName { get; set; }
        [Required]
        public string lastName { get; set; }
        [EmailAddress]
        [Required]
        [DefaultValue("example@gmail.com")]
        public string email { get; set; }
        [Required]
        [DefaultValue("")]
        public string password { get; set; }
    }
}
