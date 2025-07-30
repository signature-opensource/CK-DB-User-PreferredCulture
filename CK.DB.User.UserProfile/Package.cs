using CK.Core;
using CK.Cris;
using CK.DB.Actor;
using CK.IO.User.UserProfile;
using CK.SqlServer;
using System.Diagnostics.CodeAnalysis;

namespace CK.DB.User.UserProfile;

[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
[Versions( "1.0.0" )]
[SqlObjectItem( "vUserProfile" )]
public abstract partial class Package : SqlPackage, IUserService
{
    [AllowNull]
    UserTable _userTable;

    void StObjConstruct( Actor.UserTable userTable )
    {
        _userTable = userTable;
    }

    [SqlProcedure( "sUserUserProfileRead" )]
    public abstract Task<IUserProfile> ReadUserProfileAsync( ISqlCallContext ctx, int actorId, int userId );

    public async Task<int> CreateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, string userName )
    {
        var userId = await _userTable.CreateUserAsync( ctx, actorId, userName );
        if( userId <= 0 )
        {
            throw new InvalidOperationException( "Failed to create user." );
        }
        ctx.Monitor.Info( $"User successfully created. (ActorId: {actorId}, UserId: {userId})" );
        return userId;
    }

    public async Task<IUserProfile> GetUserProfileAsync( ISqlCallContext ctx, int actorId, int userId )
        => await ReadUserProfileAsync( ctx, actorId, userId );

    public async Task UpdateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, int userId, string? userName )
    {
        if( !string.IsNullOrWhiteSpace( userName ) )
        {
            await _userTable.UserNameSetAsync( ctx, actorId, userId, userName );
            ctx.Monitor.Info( $"User's username successfully updated. (UserId: {userId})" );
        }
    }

    public async Task DestroyUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, int userId )
    {
        await _userTable.DestroyUserAsync( ctx, actorId, userId );
        ctx.Monitor.Info( $"User successfully destroyed. (UserId: {userId})" );
    }
}
