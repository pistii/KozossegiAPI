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


        public async Task<Comment?> GetCommentByTokenAsync(string token)
        {
            var comment = await _context.Comment
                .Include(c => c.CommentReactions)
                .FirstOrDefaultAsync(p => p.PublicId == token);
            return comment;
        }

        public async Task<ContentDto<CommentDto>> GetCommentsAsync(string userPublicId, int postId, int currentPage = 1, int itemPerPage = 20)
        {
            var sortedItems = await _context.Comment
                .Include(c => c.CommentReactions)
                .Include(p => p.AuthorPerson)
                .ThenInclude(u => u.users)
                .Where(p => p.PostId == postId)
                .OrderByDescending(_ => _.CommentDate)
                .AsNoTracking()
                .Select(p =>
                new CommentDto(p, p.AuthorPerson))
                .ToListAsync();

            var returnValue = Paginator(sortedItems, currentPage, itemPerPage).ToList();
            int totalPages = await GetTotalPages(sortedItems, itemPerPage);

            return new ContentDto<CommentDto>(returnValue, totalPages);
        }

        public async Task<CommentDto> Create(int postId, int authorId, string message)
        {
            Comment newComment = new Comment
            {
                PostId = postId,
                FK_AuthorId = authorId,
                PublicId = Guid.NewGuid().ToString(),
                CommentDate = DateTime.Now,
                CommentText = message
            };
            
            await InsertSaveAsync(newComment);
            CommentDto dto = new CommentDto(newComment);
            Personal author = await GetByIdAsync<Personal>(authorId);
            PostAuthor personalDto = new PostAuthor(author.avatar, author.firstName, author.middleName, author.lastName, author.users.PublicId);
            dto.CommentAuthor = personalDto;
            return dto;
        }
    }
}
