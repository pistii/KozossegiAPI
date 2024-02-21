namespace KozoskodoAPI.DTOs
{
    public class ContentDto<T>
    {
        public ContentDto(List<T>? data, int totalPages)
        {
            Data = data;
            TotalPages = totalPages;
        }

        public List<T>? Data { get; set; }
        public int TotalPages { get; set; }
    }
}
