using System.ComponentModel;

namespace CK.IO.User.PreferredCulture;

public interface ICreateUserCommand : Actor.ICreateUserCommand
{
    [DefaultValue( "en" )]
    public string PreferredCultureName { get; set; }
}
