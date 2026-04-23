using DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTOs
{
    public class UserNode
    {
        public readonly User? Root;
        public List<UserNode> Children { get; set; }
        public bool IsExpanded { get; set; }
        public UserNode(User? root)
        {   
            Root = root;
            Children = new List<UserNode>();
            IsExpanded = true;
        }
    }
}
