using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace blueprint.modules.drive.request
{
    public class SetFileRequest
    {
        [Required]
        [DefaultValue("")]
        public string fileId { get; set; }
    }
}
