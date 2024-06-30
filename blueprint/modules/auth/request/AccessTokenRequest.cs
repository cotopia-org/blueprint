using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace blueprint.modules.auth.request
{
    public class AccessTokenRequest
    {
        [Required]
        [DefaultValue("")]
        public string refreshToken { get; set; }
    }
}
