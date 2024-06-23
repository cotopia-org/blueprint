using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace cotopia_server.modules.auth.request
{
    public class SigninRequest
    {
        [Required]
        [DefaultValue("")]
        public string email { get; set; }
        [Required]
        [DefaultValue("")]
        public string password { get; set; }
        [DefaultValue(false)]
        public bool rememberMe { get; set; }
        [DefaultValue("default-session")]
        public string sessionName { get; set; }
    }
}
