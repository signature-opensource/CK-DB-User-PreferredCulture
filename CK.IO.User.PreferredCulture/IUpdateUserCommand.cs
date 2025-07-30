namespace CK.IO.User.PreferredCulture;

public interface IUpdateUserCommand : UserProfile.IUpdateUserCommand
{
    public string? PreferredCultureName { get; set; }
}
