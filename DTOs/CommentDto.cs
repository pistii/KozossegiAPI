using KozossegiAPI.Models;
using NuGet.Common;
using System.ComponentModel.DataAnnotations;

namespace KozossegiAPI.DTOs
{
    public class CommentDto
    {
        public CommentDto() { }
        public CommentDto(Comment comment)
        {
            this.Comment = new(false, comment.PublicId, comment.CommentDate, comment.CommentText, comment.LastModified);
        }

        public CommentDto(Comment comment, Personal author)
        {
            this.Comment = new(
                comment.AuthorPerson.users.PublicId == author.users.PublicId,
                comment.PublicId,
                comment.CommentDate,
                comment.CommentText,
                comment.LastModified,
                comment.CommentReactions.Count(c => c.ReactionTypeId == 1),
                comment.CommentReactions.Count(c => c.ReactionTypeId == 2));

            this.CommentAuthor = new PostAuthor(
                author.avatar, 
                author.firstName, 
                author.middleName, 
                author.lastName,
                author.users.PublicId);
        }

        public GetComment Comment { get; set; }
        public PostAuthor CommentAuthor { get; set; }
    }

    public class CreateCommentDto
    {
        public CreateCommentDto(string token, string message)
        {
            this.PostToken = token;
            this.Message = message;
        }
        public string PostToken { get; set; }
        public string Message { get; set; }
    }

    public class UpdateCommentDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "CommentToken field is required")]
        public string? CommentToken { get; set; }
        [Required(ErrorMessage = "CommenterId field is required")]
        public int commenterId { get; set; }
        public string? CommentTxt { get; set; }
    }

    public class GetComment
    {
        public GetComment(bool isAuthor, string commentToken, DateTime createdAt, string message, DateTime? lastModified, int likes = 0, int dislikes = 0)
        {
            this.IsAuthor = isAuthor;
            this.CommentToken = commentToken;
            this.CommentDate = createdAt;
            this.CommentText = message;
            this.LastModified = lastModified;
            Likes = likes;
            Dislikes = dislikes;
        }

        public bool IsAuthor { get; set; }
        public DateTime CommentDate { get; set; }
        [StringLength(36)]
        public string CommentToken { get; set; }
        public string CommentText { get; set; }
        public DateTime? LastModified { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }
}
