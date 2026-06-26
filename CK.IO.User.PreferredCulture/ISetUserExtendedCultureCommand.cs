using CK.Auth;
using CK.Cris;

namespace CK.IO.User.PreferredCulture;

public interface ISetUserExtendedCultureCommand : ICommand<ICrisBasicCommandResult>, ICommandAuthNormal
{
    public int UserId { get; set; }
    public int ExtendedCultureId { get; set; }
}
