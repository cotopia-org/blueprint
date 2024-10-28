using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace blueprint.modules.auth.request
{
    public class SigninRequest
    {
        [Required]
        [DefaultValue("")]
        public string email { get; set; }
        [Required]
        [DefaultValue("")]
        public string password { get; set; }
        [DefaultValue("default-session")]
        public string sessionName { get; set; }
    }
}
