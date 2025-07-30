using System.ComponentModel;

namespace CK.IO.User.PreferredCulture;

public interface ICreateUserCommand : UserProfile.ICreateUserCommand
{
    [DefaultValue( "en" )]
    public string PreferredCultureName { get; set; }
}
