﻿using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace KozoskodoAPI.Repo
{
    public interface IUserRepository<T> : IGenericRepository<T>
    {
        Task<user?> GetuserByIdAsync(int id);
        Task<user> GetByGuid(string id);
        Task<Personal?> GetPersonalWithSettingsAndUserAsync(int userId);
        Task<user?> GetUserByEmailOrPassword(string email = null, string password = null);
        Task<user?> GetUserByEmailAsync(string email, bool withPersonal = true);
    }
}
