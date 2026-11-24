Adds a preferred culture to the users: the `ExtendedCultureId` column of `CK.tUser`, a culture of CK.DB.Globalization
set at creation or later, and exposed by `CK.vUser` and the user profile.

The `Package` creates a user with its culture (`Package.CreateUserAsync`) and changes it
(`Package.SetExtendedCultureAsync`); it also handles the `ISetUserExtendedCultureCommand` of
CK.IO.User.PreferredCulture.
