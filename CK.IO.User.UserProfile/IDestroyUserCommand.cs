using CK.Auth;
using CK.Cris;

namespace CK.IO.User.UserProfile;

public interface IDestroyUserCommand : ICommand<ICrisBasicCommandResult>, ICommandCurrentCulture, ICommandAuthNormal
{
    public int UserId { get; set; }
}
