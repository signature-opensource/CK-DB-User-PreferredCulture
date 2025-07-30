using CK.Auth;
using CK.Cris;

namespace CK.IO.User.UserProfile;

public interface IGetUserProfileQCommand : ICommand<IUserProfile?>, ICommandAuthNormal
{
    public int UserId { get; set; }
}
