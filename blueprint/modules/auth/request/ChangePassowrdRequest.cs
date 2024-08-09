using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace blueprint.modules.auth.request
{
    public class ChangePasswordRequest
    {
        [Required]
        [DefaultValue("")]
        public string currentPassword { get; set; }
        //[Password]
        [Required]
        [DefaultValue("")]
        public string newPassword { get; set; }
    }
}
