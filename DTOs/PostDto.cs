using KozossegiAPI.Models;
using KozossegiAPI.Models.Cloud;

namespace KozossegiAPI.DTOs
{
    public class PostDto
    {
        public PostDto()
        {
            
        }
        public PostDto(Personal author, int postedToUser, Post post, int? commentsQty = 0)
        {
            Post = post;
            PostAuthor = new PostAuthor(author.avatar, author.firstName, author.middleName, author.lastName, author.id);
            PostedToUserId = postedToUser;
            CommentsQty = commentsQty;
        }

        public Post Post { get; set; }
        public PostAuthor PostAuthor { get; set; }
        public int PostedToUserId { get; set; }
        public int? CommentsQty { get; set; }
    }

    public class PostToUser
    {
        public PostToUser()
        {
            
        }

        public PostToUser(int Id)
        {
            this.Id = Id;
        }

        public int Id { get; set; }
    }

    public class PostAuthor
    {
        public PostAuthor()
        {
            
        }
        public PostAuthor(string avatar, string firstName, string middleName, string lastName, int authorId)
        {
            Avatar = avatar;
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            AuthorId = authorId;
        }

        public string? Avatar { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public int AuthorId { get; set; }

    }
    public class CreatePostDto
    {
        public CreatePostDto()
        {

        }
        public CreatePostDto(Personal author, int postedToUserId, Post post, FileUpload file)
        {
            post = new Post(post.Id, post.PostContent, post.Likes, post.Dislikes);
            PostAuthor = new PostAuthor(author.avatar, author.firstName, author.middleName, author.lastName, author.id);
            PostedToUserId = postedToUserId;
            FileUpload = new FileUpload(file.Name, file.Type, file.File, file.File.Length);
        }

        public FileUpload? FileUpload { get; set; }
        public Post post { get; set; }
        public PostAuthor PostAuthor { get; set; }
        public int PostedToUserId { get; set; }
    }
}
