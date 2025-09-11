using CK.Core;
using CK.Cris;
using CK.IO.User.PreferredCulture;
using CK.SqlServer;
using System.Diagnostics.CodeAnalysis;

namespace CK.DB.User.PreferredCulture;

[SqlPackage( FullName = "CK.User.PreferredCulture.Package", Schema = "CK", ResourcePath = "Res" )]
[Versions( "1.0.0" )]
[SqlObjectItem( "transform:sUserUserProfileRead, transform:vUser" )]
public abstract partial class Package : SqlPackage
{
    [AllowNull]
    Actor.Package _actorPackage;

    void StObjConstruct( Actor.Package actorPackage )
    {
        _actorPackage = actorPackage;
    }

    [SqlProcedure( "transform:sUserCreate" )]
    public abstract Task<int> CreateUserAsync( ISqlCallContext ctx,
                                               int actorId,
                                               string userName,
                                               string preferredCultureName );

    //[CommandHandler]
    //[SqlProcedure( "transform:sUserCreate" )]
    //public abstract Task<IO.Actor.ICreateUserCommandResult> CreateUserAsync( ISqlCallContext ctx, [ParameterSource] ICreateUserCommand command );

    [SqlProcedure( "CK.sUserPreferredCultureNameSet" )]
    public abstract Task SetPreferredCultureNameAsync( ISqlCallContext ctx, int actorId, int userId, string preferredCultureName );

    [CommandHandler]
    [SqlProcedure( "CK.sUserPreferredCultureNameSet" )]
    public abstract Task<ICrisBasicCommandResult> SetPreferredCultureNameAsync( ISqlCallContext ctx, [ParameterSource] ISetUserPreferredCultureCommand command );
}
