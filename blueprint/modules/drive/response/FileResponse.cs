using System.ComponentModel.DataAnnotations;

namespace blueprint.modules.drive.response
{
    public class FileResponse
    {
        public string id { get; set; }
        public string title { get; set; }
        public string name { get; set; }
        public string extention { get; set; }
        [Url]
        public string url { get; set; }
        public DateTime dateTime { get; set; }
    }
}
