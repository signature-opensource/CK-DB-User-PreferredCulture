using CK.Auth;
using CK.Cris;

namespace CK.IO.User.UserProfile;

public interface IUpdateUserCommand : ICommand<ICrisBasicCommandResult>, ICommandCurrentCulture, ICommandAuthNormal
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
}
