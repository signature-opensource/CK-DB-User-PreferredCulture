using CK.Core;
using CK.Cris;
using CK.IO.User.PreferredCulture;
using CK.SqlServer;
using Microsoft.Data.SqlClient;

namespace CK.DB.User.PreferredCulture;

public partial class Package
{
    [CommandHandler]
    public async Task<IO.User.UserProfile.ICreateUserCommandResult> HandleCreateUserAsync( ISqlTransactionCallContext ctx, UserMessageCollector collector, ICreateUserCommand cmd )
    {
        using( ctx.Monitor.OpenInfo( $"Handling ICreateUserCommand. (ActorId: {cmd.ActorId})" ) )
        {
            var res = cmd.CreateResult();
            try
            {
                using( var transaction = ctx[_userProfilePackage].BeginTransaction() )
                {
                    var userId = await CreateUserAsync( ctx,
                                                        collector,
                                                        cmd.ActorId.GetValueOrDefault(),
                                                        cmd.UserName,
                                                        cmd.PreferredCultureName );
                    transaction.Commit();
                    res.CreatedUserId = userId;
                    collector.Info( $"User successfully created. (UserId: {userId})", "User.UserCreated" );
                }
            }
            catch( SqlDetailedException ex ) when( ex.InnerSqlException is SqlException sqlEx )
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
    public async Task<ICrisBasicCommandResult> HandleUpdateUserAsync( ISqlTransactionCallContext ctx, UserMessageCollector collector, IUpdateUserCommand cmd )
    {
        using( ctx.Monitor.OpenInfo( $"Handling IUpdateUserCommand. (ActorId: {cmd.ActorId})" ) )
        {
            var res = cmd.CreateResult();
            try
            {
                using( var transaction = ctx[_userProfilePackage].BeginTransaction() )
                {
                    await UpdateUserAsync( ctx, collector, cmd.ActorId.GetValueOrDefault(), cmd.UserId, cmd.UserName, cmd.PreferredCultureName );
                    transaction.Commit();
                    collector.Info( $"User successfully updated. (UserId: {cmd.UserId})", "User.UserUpdated" );
                }
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
}
