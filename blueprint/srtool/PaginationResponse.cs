using System.Collections.Generic;

namespace srtool
{
    public class PaginationResponse<Item>
    {
        public int page { get; set; }
        public int perPage { get; set; }
        public long total { get; set; }
        public List<Item> items { get; set; }
    }
}
