using CK.Core;
using CK.SqlServer;
using System.Diagnostics.CodeAnalysis;

namespace CK.DB.User.PreferredCulture;

[SqlPackage( FullName = "CK.User.PreferredCulture.Package", Schema = "CK", ResourcePath = "Res" )]
[Versions( "1.0.0" )]
[SqlObjectItem( "transform:sUserUserProfileRead, transform:vUserProfile, transform:sUserCreate" )]
public abstract partial class Package : SqlPackage, IUserService
{
    [AllowNull]
    UserProfile.Package _userProfilePackage;

    void StObjConstruct( UserProfile.Package userProfilePackage )
    {
        _userProfilePackage = userProfilePackage;
    }

    [SqlProcedure( "sUserPreferredCultureNameSet" )]
    public abstract Task SetPreferredCultureNameAsync( ISqlCallContext ctx, int actorId, int userId, string preferredCultureName );

    public async Task<int> CreateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, string userName, string preferredCultureName = "en" )
    {
        var userId = await _userProfilePackage.CreateUserAsync( ctx, collector, actorId, userName );
        await SetPreferredCultureNameAsync( ctx, actorId, userId, preferredCultureName );
        ctx.Monitor.Info( $"User's preferred culture successfully set. (ActorId: {actorId}, UserId: {userId}, PreferredCulture: {preferredCultureName})" );

        return userId;
    }

    public Task<int> CreateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, string userName )
        => _userProfilePackage.CreateUserAsync( ctx, collector, actorId, userName );

    public async Task<IO.User.PreferredCulture.IUserProfile> GetUserProfileAsync( ISqlCallContext ctx, int actorId, int userId )
        => (IO.User.PreferredCulture.IUserProfile)await _userProfilePackage.ReadUserProfileAsync( ctx, actorId, userId );

    Task<IO.User.UserProfile.IUserProfile> UserProfile.IUserService.GetUserProfileAsync( ISqlCallContext ctx, int actorId, int userId )
        => _userProfilePackage.ReadUserProfileAsync( ctx, actorId, userId );

    public async Task UpdateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, int userId, string? userName, string? preferredCultureName )
    {
        if( !string.IsNullOrWhiteSpace( userName ) )
        {
            await _userProfilePackage.UpdateUserAsync( ctx, collector, actorId, userId, userName );
            collector.Info( $"User's username successfully updated. (UserId: {userId})", "User.UserUserNameUpdated" );
        }
        if( !string.IsNullOrWhiteSpace( preferredCultureName ) )
        {
            await SetPreferredCultureNameAsync( ctx, actorId, userId, preferredCultureName );
            ctx.Monitor.Info( $"User's preferred culture name successfully updated. (UserId: {userId}, PreferredCulture: {preferredCultureName})" );
            collector.Info( $"User's preferred culture name successfully updated. (UserId: {userId})", "User.UserPreferredCultureNameUpdated" );
        }
    }

    public Task UpdateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, int userId, string? userName )
        => _userProfilePackage.UpdateUserAsync( ctx, collector, actorId, userId, userName );

    public Task DestroyUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, int userId )
        => _userProfilePackage.DestroyUserAsync( ctx, collector, actorId, userId );
}
