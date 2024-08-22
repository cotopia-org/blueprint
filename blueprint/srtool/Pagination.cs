namespace srtool
{
    public class Pagination
    {
        public int Page { get; set; }
        public int PerPage { get; set; }

        public int Skip { get; set; }
        public int Take { get; set; }
        public int Total { get; set; }
        public Pagination(int page, int perPage)
        {
            if (perPage < 1)
                perPage = 1;

            if (page < 1)
                page = 1;

            Page = page;
            PerPage = perPage;

            Skip = (page - 1) * perPage;
            Take = perPage;
        }
        public override string ToString()
        {
            return $"page:{Page},perPage:{PerPage}";
        }
    }
}