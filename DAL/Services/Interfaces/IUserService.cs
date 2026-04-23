using DAL.DTOs;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> InsertUser(User user, UserHierarchy userHierarchy);

        Task<bool> MoveUser(User user, UserHierarchy newUserHierarchy);

        Task<UserNode> GetUserNodesAsync();

        Task<UserNode> GetUserNodeForUserAsync(User? user);

        Task<bool> IsUsernameUnique(string username);
    }
}
