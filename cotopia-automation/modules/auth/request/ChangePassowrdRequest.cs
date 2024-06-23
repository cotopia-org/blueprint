using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace cotopia_server.modules.auth.request
{
    public class ChangePassowrdRequest
    {
        [Required]
        [DefaultValue("")]
        public string currentPassword { get; set; }
        //[Password]
        [Required]
        [DefaultValue("")]
        public string newPassowrd { get; set; }
    }
}
