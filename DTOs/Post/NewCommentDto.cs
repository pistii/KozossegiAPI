namespace KozoskodoAPI.DTOs.Post
{

    public class NewCommentDto
    {
        public int postId { get; set; }
        public int commenterId { get; set; }
        public string commentTxt { get; set; }
    }
}
