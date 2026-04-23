using DAL.DTOs;
using DAL.Models;
using DAL.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Inserts a new user into the database and updates the sort order of existing users in the same hierarchy level.
        public async Task<bool> InsertUser(User user, UserHierarchy userHierarchy)
        {
            try
            {
                List<UserHierarchy> usersToMove = await _dbContext.UserHierarchy.Where(uh => uh.ParentId == userHierarchy.ParentId && uh.SortOrder >= userHierarchy.SortOrder).ToListAsync();
                foreach (UserHierarchy userToMove in usersToMove)
                {
                    userToMove.SortOrder++;
                }

                _dbContext.User.Add(user);
                userHierarchy.UserId = user.Id;
                _dbContext.UserHierarchy.Add(userHierarchy);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Moves a user to a new position in the hierarchy and updates the sort order of affected users accordingly.
        public async Task<bool> MoveUser(User user, UserHierarchy newUserHierarchy)
        {
            try
            {
                if (user.Id == newUserHierarchy.ParentId)
                {
                    return false;
                }

                UserHierarchy? currentUserHierarchy = await _dbContext.UserHierarchy.Where(uh => uh.UserId == user.Id).FirstOrDefaultAsync();
                if (currentUserHierarchy == null)
                {
                    return false;
                }

                List<UserHierarchy> usersToMove = await _dbContext.UserHierarchy.Where(uh => uh.ParentId == currentUserHierarchy.ParentId && uh.SortOrder > newUserHierarchy.SortOrder).ToListAsync();
                foreach (UserHierarchy userToMove in usersToMove)
                {
                    userToMove.SortOrder--;
                }

                usersToMove = await _dbContext.UserHierarchy.Where(uh => uh.ParentId == newUserHierarchy.ParentId && uh.SortOrder >= newUserHierarchy.SortOrder).ToListAsync();
                foreach (UserHierarchy userToMove in usersToMove)
                {
                    userToMove.SortOrder++;
                }
                currentUserHierarchy.ParentId = newUserHierarchy.ParentId;
                currentUserHierarchy.SortOrder = newUserHierarchy.SortOrder;
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Updates an existing user in the database.
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _dbContext.User.Update(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Retrieves the entire user hierarchy starting from the root nodes (users with no parent).
        public async Task<UserNode> GetUserNodesAsync()
        {
            return await GetUserNodeForUserAsync(null);
        }

        // Recursively builds a UserNode tree for a given user, including all of its children.
        public async Task<UserNode> GetUserNodeForUserAsync(User? user)
        {
            UserNode userNode = new UserNode(user);
            long? parentId = user?.Id;
            List<UserHierarchy> userHierarchies = await _dbContext.UserHierarchy.Where(uh => uh.ParentId == parentId).OrderBy(uh => uh.SortOrder).ToListAsync();
            foreach (UserHierarchy userHierarchy in userHierarchies)
            {
                User child = await _dbContext.User.FindAsync(userHierarchy.UserId);
                UserNode childNode = await GetUserNodeForUserAsync(child);
            }
            return userNode;
        }

        // Checks if a given username is unique in the database.
        public async Task<bool> IsUsernameUnique(string username)
        {
            return !await _dbContext.User.Where(u => u.Username == username).AnyAsync();
        }
    }
}
