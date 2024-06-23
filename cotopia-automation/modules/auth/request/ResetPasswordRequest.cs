using blueprint.core.DataAnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace cotopia_server.modules.auth.request
{
    public class ResetPasswordRequest
    {
        [Required]
        [DefaultValue("")]
        public string code { get; set; }
        [Password]
        [Required]
        [DefaultValue("")]
        public string newPassowrd { get; set; }
    }
}
