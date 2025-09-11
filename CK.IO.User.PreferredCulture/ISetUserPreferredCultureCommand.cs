using CK.Auth;
using CK.Cris;

namespace CK.IO.User.PreferredCulture;

public interface ISetUserPreferredCultureCommand : ICommand<ICrisBasicCommandResult>, ICommandAuthNormal
{
    public int UserId { get; set; }
    public string PreferredCultureName { get; set; }
}
