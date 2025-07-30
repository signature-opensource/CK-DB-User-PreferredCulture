using CK.Core;
using CK.Cris;
using CK.IO.User.UserProfile;
using CK.SqlServer;

namespace CK.DB.User.UserProfile;

public partial class Package
{
    [CommandHandler]
    public async Task<ICreateUserCommandResult> HandleCreateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, ICreateUserCommand cmd )
    {
        using( ctx.Monitor.OpenInfo( $"Handling ICreateUserCommand. (ActorId: {cmd.ActorId})" ) )
        {
            var res = cmd.CreateResult();
            try
            {
                var userId = await CreateUserAsync( ctx, collector, cmd.ActorId.GetValueOrDefault(), cmd.UserName );
                res.CreatedUserId = userId;
                collector.Info( $"User successfully created. (UserId: {userId})", "User.UserCreated" );
            }
            catch( SqlDetailedException ex ) when( ex.InnerSqlException is not null )
            {
                ctx.Monitor.Error( $"Error while handling ICreateUserCommand: {ex.Message}", ex );
                collector.Error( ex );
            }
            catch( Exception ex )
            {
                ctx.Monitor.Error( $"Error while handling ICreateUserCommand: {ex.Message}", ex );
                collector.Error( "An error occurred while creating the user.", "User.UserCreationFailed" );
            }

            res.SetUserMessages( collector );
            return res;
        }
    }

    [CommandHandler]
    public async Task<IUserProfile?> HandleGetUserProfileAsync( ISqlCallContext ctx, IGetUserProfileQCommand cmd )
    {
        try
        {
            return await GetUserProfileAsync( ctx, cmd.ActorId.GetValueOrDefault(), cmd.UserId );
        }
        catch( Exception ex )
        {
            ctx.Monitor.Error( $"Error while handling IGetUserProfileQCommand: {ex.Message}", ex );
            return null;
        }
    }

    [CommandHandler]
    public async Task<ICrisBasicCommandResult> HandleUpdateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, IUpdateUserCommand cmd )
    {
        using( ctx.Monitor.OpenInfo( $"Handling IUpdateUserCommand. (ActorId: {cmd.ActorId})" ) )
        {
            var res = cmd.CreateResult();
            try
            {
                await UpdateUserAsync( ctx, collector, cmd.ActorId.GetValueOrDefault(), cmd.UserId, cmd.UserName );
                collector.Info( $"User successfully updated. (UserId: {cmd.UserId})", "User.UserUpdated" );
            }
            catch( SqlDetailedException ex ) when( ex.InnerSqlException is not null )
            {
                ctx.Monitor.Error( $"Error while handling IUpdateUserCommand: {ex.Message}", ex );
                collector.Error( ex );
            }
            catch( Exception ex )
            {
                ctx.Monitor.Error( $"Error while handling IUpdateUserCommand: {ex.Message}", ex );
                collector.Error( "An error occurred while updating the user.", "User.UserUpdateFailed" );
            }

            res.SetUserMessages( collector );
            return res;
        }
    }

    [CommandHandler]
    public async Task<ICrisBasicCommandResult> HandleDestroyUserAsync( ISqlCallContext ctx, UserMessageCollector collector, IDestroyUserCommand cmd )
    {
        using( ctx.Monitor.OpenInfo( $"Handling IDestroyUserCommand. (ActorId: {cmd.ActorId})" ) )
        {
            var res = cmd.CreateResult();
            try
            {
                await DestroyUserAsync( ctx, collector, cmd.ActorId.GetValueOrDefault(), cmd.UserId );
                collector.Info( $"User successfully destroyed. (UserId: {cmd.UserId})", "User.UserDestroyed" );
            }
            catch( SqlDetailedException ex ) when( ex.InnerSqlException is not null )
            {
                ctx.Monitor.Error( $"Error while handling IDestroyUserCommand: {ex.Message}", ex );
                collector.Error( ex );
            }
            catch( Exception ex )
            {
                ctx.Monitor.Error( $"Error while handling IDestroyUserCommand: {ex.Message}", ex );
                collector.Error( "An error occurred while destroying the user.", "User.UserDestroyFailed" );
            }

            res.SetUserMessages( collector );
            return res;
        }
    }
}
