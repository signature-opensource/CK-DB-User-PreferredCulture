namespace CK.IO.User.PreferredCulture;

public interface IUserProfile : Actor.IUserProfile
{
    public int ExtendedCultureId { get; set; }
}
