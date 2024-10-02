using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace KozossegiAPI.Repo
{
    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        private readonly DBContext _context;

        public CommentRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Comment> GetByTokenAsync(string commentToken)
        {
            return await _context.Comment.FirstOrDefaultAsync(p => p.CommentToken == commentToken);
        }

        public async Task<List<CommentDto>> GetCommentsAsync(int postId)
        {
            var sortedItems = await _context.Comment
           .Where(p => p.PostId == postId)
           .OrderByDescending(_ => _.CommentDate)
           .AsNoTracking()
           .Include(p => p.AuthorPerson)
            .Select(p =>
                new CommentDto()
                {
                    CommentId = p.commentId,
                    CommentDate = p.CommentDate,
                    CommentToken = p.CommentToken,
                    CommentText = p.CommentText,
                    LastModified = p.LastModified,
                    CommentAuthor = new PersonalDto(p.AuthorPerson.id, p.AuthorPerson.firstName, p.AuthorPerson.middleName,
                   p.AuthorPerson.lastName, p.AuthorPerson.PlaceOfBirth, p.AuthorPerson.avatar,
                   p.AuthorPerson.DateOfBirth, p.AuthorPerson.PlaceOfBirth,
                   p.AuthorPerson.Profession, p.AuthorPerson.Workplace)
                })               
           .ToListAsync();

            return sortedItems;
        }

        public async Task<CommentDto> Create(int postId, int authorId, string message)
        {
            Comment newComment = new Comment
            {
                PostId = postId,
                FK_AuthorId = authorId,
                CommentToken = Guid.NewGuid().ToString(),
                CommentDate = DateTime.Now,
                CommentText = message
            };
            
            await InsertSaveAsync(newComment);
            CommentDto dto = new CommentDto(newComment);
            Personal author = await GetByIdAsync<Personal>(authorId);
            PersonalDto personalDto = new(authorId, author.firstName, author.middleName, author.lastName, author.PlaceOfResidence, author.avatar, author.DateOfBirth, author.PlaceOfBirth, author.Profession, author.Workplace);
            dto.CommentAuthor = personalDto;
            return dto;
        }
    }
}
