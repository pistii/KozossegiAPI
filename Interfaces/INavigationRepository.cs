
using KozossegiAPI.DTOs;
using KozossegiAPI.Models;

public interface INavigationRepository
    {
        public Task<List<RecommendedPerson>> SearchForPerson(string searchByValue, int page, int itemPerRequest);
        public Task<List<RecommendedPosts>> SearchForPost(int currentUser, string searchByValue, int page, int itemPerRequest);
    }