using CK.Core;
using CK.IO.User.UserProfile;
using CK.SqlServer;

namespace CK.DB.User.UserProfile;

public interface IUserService : IAutoService
{
    Task<int> CreateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, string userName );
    public Task<IUserProfile> GetUserProfileAsync( ISqlCallContext ctx, int actorId, int userId );
    Task UpdateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, int userId, string? userName );
    Task DestroyUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, int userId );
}


// UserPassword -> UserProfile.IUserService
// Ng.UserProfile.UserPassword -> 
