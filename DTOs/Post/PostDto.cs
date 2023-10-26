namespace KozoskodoAPI.DTOs.Post
{
    public class PostDto
    {
        public int PersonalPostId { get; set; }
        public string FullName { get; set; }
        public string AuthorAvatar { get; set; }
        public int AuthorId { get; set; }
        public int PostId { get; set; }
        public DateTime DateOfPost { get; set; }
        public string PostContent { get; set; }
        public List<CommentDto> PostComments { get; set; }
    }

    public class CommentDto
    {
        public int CommentId { get; set; }
        public int AuthorId { get; set; }
        public string CommenterFullName { get; set; }
        public string CommenterAvatar { get; set; }
        public DateTime CommentDate { get; set; }
        public string CommentText { get; set; }
    }
}
