namespace Web4.Models.Dto
{
    public class TokenDto
    {
        public string access_token { get; set; }
        public string message { get; set; }
        public int statusCode { get; set; }
        public List<String> role { get; set; }
    }
}
