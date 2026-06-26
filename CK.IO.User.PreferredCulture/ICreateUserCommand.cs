using System.ComponentModel;

namespace CK.IO.User.PreferredCulture;

public interface ICreateUserCommand : Actor.ICreateUserCommand
{
    [DefaultValue( 210327884 )]
    public int ExtendedCultureId { get; set; }
}
