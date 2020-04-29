namespace LibraryApi.Models
{
    public class GetABookResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public int NumberOfPages { get; set; }
    }
}
