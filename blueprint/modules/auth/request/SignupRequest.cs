
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace blueprint.modules.auth.request
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
