﻿using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace blueprint.modules.scheduler.database
{
    public class SchedulerResponse
    {
        public string id { get; set; }
        public string? key { get; set; }
        public string category { get; set; }
        public string payload { get; set; }
        public bool repeat { get; set; }
        public DateTime createDateTime { get; set; }
        public DateTime invokeTime { get; set; }
    }

}
