namespace CK.IO.User.PreferredCulture;

public interface IUserProfile : UserProfile.IUserProfile
{
    public string PreferredCultureName { get; set; }
}
