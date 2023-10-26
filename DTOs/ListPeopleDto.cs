namespace KozoskodoAPI.DTOs
{
    public class ListPagesDto<T>
    {
        public ListPagesDto(List<T> data, int items) 
        {
            this.data = data;
            this.totalItems = items;
        }
        public List<T> data { get; set; }
        public int totalItems { get; set; }
    }
}
