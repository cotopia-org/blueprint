namespace blueprint.modules.processlog.response
{
    public class Log
    {
        public string id { get; set; }
        public string process_id { get; set; }
        public string blueprint_id { get; set; }
        public string type { get; set; }
        public string message { get; set; }
        public DateTime createDateTime { get; set; }
    }
}
