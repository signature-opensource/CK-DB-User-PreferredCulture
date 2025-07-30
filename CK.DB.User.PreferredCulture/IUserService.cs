using CK.Core;
using CK.SqlServer;

namespace CK.DB.User.PreferredCulture;

public interface IUserService : UserProfile.IUserService
{
    Task<int> CreateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, string userName, string preferredCultureName = "en" );
    new Task<IO.User.PreferredCulture.IUserProfile> GetUserProfileAsync( ISqlCallContext ctx, int actorId, int userId );
    Task UpdateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, int userId, string? userName, string? preferredCultureName );
}
