﻿using KozoskodoAPI.Models;

namespace KozoskodoAPI.Repo
{
    public interface IPersonalRepository<T> : IGenericRepository<T>
    {
        Task<Personal> Get(int id);
        IQueryable<Personal> FilterPersons(int userId);
    }
}