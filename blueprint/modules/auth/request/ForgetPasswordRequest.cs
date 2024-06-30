using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace blueprint.modules.auth.request
{
    public class ForgetPasswordRequest
    {
        [EmailAddress]
        [Required]
        [DefaultValue("example@gmail.com")]
        public string email { get; set; }
    }
}
