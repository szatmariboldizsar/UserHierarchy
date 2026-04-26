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
        Task<bool> InsertUserAsync(User user, UserHierarchy userHierarchy);

        Task<bool> MoveUserAsync(User user, UserHierarchy newUserHierarchy);

        Task<bool> UpdateUserAsync(User user);

        Task<bool> DeleteUserAsync(UserNode node);

        Task<UserNode> GetUserNodesAsync();

        Task<UserNode> GetUserNodeForUserAsync(User? user);

        bool IsUsernameUnique(User user);
    }
}
