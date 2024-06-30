using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace blueprint.modules.auth.request
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
