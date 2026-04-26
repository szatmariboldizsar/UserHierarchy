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
        public async Task<bool> InsertUserAsync(User user, UserHierarchy userHierarchy)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                List<UserHierarchy> usersToMove = await _dbContext.UserHierarchy.Where(uh => uh.ParentId == userHierarchy.ParentId && uh.SortOrder >= userHierarchy.SortOrder).ToListAsync();
                foreach (UserHierarchy userToMove in usersToMove)
                {
                    userToMove.SortOrder++;
                }

                user.CreatedAt = DateTime.Now;
                _dbContext.User.Add(user);
                await _dbContext.SaveChangesAsync();
                userHierarchy.UserId = user.Id;
                _dbContext.UserHierarchy.Add(userHierarchy);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Moves a user to a new position in the hierarchy and updates the sort order of affected users accordingly.
        public async Task<bool> MoveUserAsync(User user, UserHierarchy newUserHierarchy)
        {
            try
            {
                if (user.Id == newUserHierarchy.ParentId)
                {
                    return false;
                }


                UserHierarchy? oldUserHierarchy = await _dbContext.UserHierarchy.Where(uh => uh.UserId == user.Id).FirstOrDefaultAsync();
                if (oldUserHierarchy == null)
                {
                    return false;
                }

                if (oldUserHierarchy.ParentId == newUserHierarchy.ParentId)
                {
                    // Moving within the same parent, just update sort order, depending on whether we're moving up or down

                    if (newUserHierarchy.SortOrder > oldUserHierarchy.SortOrder)
                    {
                        // Move up users that are between the current position and the new position
                        List<UserHierarchy> usersToMove = await _dbContext.UserHierarchy.Where(uh => uh.ParentId == oldUserHierarchy.ParentId && uh.SortOrder > oldUserHierarchy.SortOrder && uh.SortOrder <= newUserHierarchy.SortOrder).ToListAsync();
                        foreach (UserHierarchy userToMove in usersToMove)
                        {
                            userToMove.SortOrder--;
                        }
                    }
                    else
                    {
                        // Move down users that are between the new position and the current position
                        List<UserHierarchy> usersToMove = await _dbContext.UserHierarchy.Where(uh => uh.ParentId == newUserHierarchy.ParentId && uh.SortOrder < oldUserHierarchy.SortOrder && uh.SortOrder >= newUserHierarchy.SortOrder).ToListAsync();
                        foreach (UserHierarchy userToMove in usersToMove)
                        {
                            userToMove.SortOrder++;
                        }
                    }
                }
                else
                {
                    // Moving to a different parent, need to update sort order for both old and new branches

                    // Update sort order for old parent branch, move up users that were below the current user
                    List<UserHierarchy> usersToMove = await _dbContext.UserHierarchy.Where(uh => uh.ParentId == oldUserHierarchy.ParentId && uh.SortOrder > oldUserHierarchy.SortOrder).ToListAsync();
                    foreach (UserHierarchy userToMove in usersToMove)
                    {
                        userToMove.SortOrder--;
                    }

                    // Update sort order for new parent branch, move down users that are at or below the new position
                    usersToMove = await _dbContext.UserHierarchy.Where(uh => uh.ParentId == newUserHierarchy.ParentId && uh.SortOrder >= newUserHierarchy.SortOrder).ToListAsync();
                    foreach (UserHierarchy userToMove in usersToMove)
                    {
                        userToMove.SortOrder++;
                    }
                }
                oldUserHierarchy.ParentId = newUserHierarchy.ParentId;
                oldUserHierarchy.SortOrder = newUserHierarchy.SortOrder;
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

        public async Task<bool> DeleteUserAsync(UserNode node)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                UserHierarchy? userHierarchy = await _dbContext.UserHierarchy.Where(uh => uh.UserId == node.Root.Id).FirstOrDefaultAsync();
                if (userHierarchy is null)
                {
                    return false;
                }

                int sortOrder = userHierarchy.SortOrder;

                foreach (UserNode child in node.Children)
                {
                    await MoveUserAsync(child.Root, new UserHierarchy { UserId = child.Root.Id, ParentId = userHierarchy.ParentId, SortOrder = sortOrder });
                    sortOrder++;
                }

                _dbContext.User.Remove(node.Root);
                _dbContext.UserHierarchy.Remove(userHierarchy);

                List<UserHierarchy> usersToMove = await _dbContext.UserHierarchy.Where(uh => uh.ParentId == userHierarchy.ParentId && uh.SortOrder > userHierarchy.SortOrder).ToListAsync();
                foreach (UserHierarchy userToMove in usersToMove)
                {
                    userToMove.SortOrder--;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
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
                userNode.Children.Add(childNode);
            }
            return userNode;
        }

        // Checks if a given username is unique in the database.
        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            return !await _dbContext.User.Where(u => u.Username == username).AnyAsync();
        }
    }
}