namespace CK.IO.User.PreferredCulture;

public interface IUserProfile : Actor.IUserProfile
{
    public string PreferredCultureName { get; set; }
}
