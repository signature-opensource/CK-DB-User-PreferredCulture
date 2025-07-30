using CK.Auth;
using CK.Cris;

namespace CK.IO.User.UserProfile;

public interface ICreateUserCommand : ICommand<ICreateUserCommandResult>, ICommandCurrentCulture, ICommandAuthNormal
{
    public string UserName { get; set; }
}

public interface ICreateUserCommandResult : ICrisBasicCommandResult
{
    public int CreatedUserId { get; set; }
}
