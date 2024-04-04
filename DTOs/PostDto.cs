using KozoskodoAPI.Controllers.Cloud;
using KozoskodoAPI.Models;

namespace KozoskodoAPI.DTOs
{
    public class PostDto
    {
        public int PersonalPostId { get; set; }
        public string FullName { get; set; }
        public string AuthorAvatar { get; set; }
        public int AuthorId { get; set; }
        public int PostId { get; set; }
        public int? Likes { get; set; }
        public int? Dislikes { get; set; }
        public DateTime DateOfPost { get; set; } = DateTime.UtcNow;
        public string PostContent { get; set; }
        public List<CommentDto> PostComments { get; set; }
        public List<MediaContent> MediaContents { get; set; }

        public string? userReaction { get; set; } = string.Empty;
    }

    public class CommentDto
    {
        public int CommentId { get; set; }
        public int AuthorId { get; set; }
        public string CommenterFirstName { get; set; }
        public string CommenterMiddleName { get; set; } = string.Empty;
        public string CommenterLastName { get; set; }
        public string CommenterAvatar { get; set; }
        public DateTime CommentDate { get; set; } = DateTime.UtcNow;
        public string CommentText { get; set; }
    }
    public class NewCommentDto
    {
        public int postId { get; set; }
        public int commenterId { get; set; }
        public string commentTxt { get; set; }
    }

    public class Like_DislikeDto
    {
        public int postId { get; set; }
        public int UserId { get; set; }
        public string actionType { get; set; }
        //public bool isIncrement { get; set; }
    }

    public class CreatePostDto : FileUpload
    {
        public CreatePostDto(string name, string type, IFormFile file) : base(name, type, file)
        {
        }

        public int SourceId { get; set; }
        public int userId { get; set; }
        public string postContent { get; set; }
    }
}
