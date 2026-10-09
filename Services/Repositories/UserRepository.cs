using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Services.Repositories
{
    public class UserRepository(RideMatchContext dbContext) : IUserRepository
    {
        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await dbContext.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await dbContext.Users.AddAsync(user, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
