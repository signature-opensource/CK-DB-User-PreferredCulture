using CK.Core;

namespace CK.IO.User.UserProfile;

public interface IUserProfile : IPoco
{
    public int UserId { get; set; }
    public string UserName { get; set; }
}
