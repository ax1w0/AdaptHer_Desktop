using AdaptHER.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptHER.Class
{
    public static class AuthorizationGuard
    {
        public static bool CanAccessResource(int currentUserId, int resourceUserId)
        {
            return currentUserId == resourceUserId;
        }

        public static bool CanAccessResource(int currentUserId, int resourceUserId, int[] allowedRoles)
        {
            if (currentUserId == resourceUserId) return true;

            using (var db = new ApplicationDbContext())
            {
                var user = db.Users
                    .Include(u => u.Persons)
                    .FirstOrDefault(u => u.Id == currentUserId);

                if (user?.Persons != null)
                {
                    foreach (var person in user.Persons)
                    {
                        if (person.RoleId.HasValue && allowedRoles.Contains(person.RoleId.Value))
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
