using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace blueprint.modules.auth.request
{
    public class SignoutRequest
    {
        [Required]
        public string sessionId { get; set; }
    }
}
