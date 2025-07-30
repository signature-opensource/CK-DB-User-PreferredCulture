using CK.Core;
using CK.Cris;

namespace CK.IO.User.UserProfile;

public class IncomingValidators : IRealObject
{
    [IncomingValidator]
    public virtual void ValidateCreateUserCommand( ICreateUserCommand cmd, UserMessageCollector collector )
    {
        if( string.IsNullOrWhiteSpace( cmd.UserName ) )
        {
            collector.Error( "UserName cannot be null or whitespace.", "User.InvalidUserName" );
        }
    }

    [IncomingValidator]
    public virtual void ValidateDestroyUserCommand( IDestroyUserCommand cmd, UserMessageCollector collector )
    {
        if( cmd.UserId <= 0 )
        {
            collector.Error( "UserId must be greater than 0.", "User.InvalidUserId" );
        }
    }

    [IncomingValidator]
    public virtual void ValidateUpdateUserCommand( IUpdateUserCommand cmd, UserMessageCollector collector )
    {
        if( cmd.UserId <= 0 )
        {
            collector.Error( "UserId must be greater than 0.", "User.InvalidUserId" );
        }

        if( string.IsNullOrWhiteSpace( cmd.UserName ) )
        {
            collector.Error( "UserName cannot be null or whitespace.", "User.InvalidUserName" );
        }
    }
}
