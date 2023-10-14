using Web4.Models.Domain;

namespace Web4.Models.Dto
{
    public class SubjectPaginacs
    {
        public int statuscode { get; set; }
        public int current { get; set; }
        public int pageSize { get; set; }
        public long total { get; set; }
        public List<Subject> data { get; set; }
    }
}
