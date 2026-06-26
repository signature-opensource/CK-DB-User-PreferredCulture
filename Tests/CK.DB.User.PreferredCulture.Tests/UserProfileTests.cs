using CK.Core;
using CK.Cris;
using CK.DB.Actor;
using CK.IO.Actor;
using CK.SqlServer;
using CK.Testing;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace CK.DB.User.PreferredCulture.Tests;

[TestFixture]
public class UserProfileTests
{
    // Value of the DF_CK_tUser_ExtendedCultureId default constraint (the "fr" culture seeded by CK.DB.Globalization).
    const int DefaultExtendedCultureId = 210327884;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    AsyncServiceScope _scope;
    CrisExecutionContext _executor;
    PocoDirectory _pocoDir;
    UserTable _userTable;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _scope = SharedEngine.AutomaticServices.CreateAsyncScope();
        var services = _scope.ServiceProvider;

        _pocoDir = services.GetRequiredService<PocoDirectory>();
        _executor = services.GetRequiredService<CrisExecutionContext>();
        _userTable = services.GetRequiredService<UserTable>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _scope.DisposeAsync();
    }

    [Test]
    public async Task can_get_userProfile_Async()
    {
        var userId = 1;
        var cmd = _pocoDir.Create<IGetUserProfileQCommand>( cmd =>
        {
            cmd.ActorId = 1;
            cmd.UserId = userId;
        } );
        var execCmd = await _executor.ExecuteRootCommandAsync( cmd );

        var profile = execCmd.WithResult<IO.User.PreferredCulture.IUserProfile?>().Result.ShouldNotBeNull();
        profile.UserId.ShouldBe( userId );
        profile.UserName.ShouldBe( "System" );
        // The exact ExtendedCultureId of the System user depends on the install history (fresh 2.0.0 -> default 'fr',
        // migrated from 1.1.0 -> recovered from the former PreferredCultureName). The profile read must simply return
        // the value actually stored on the row.
        using var ctx = new SqlStandardCallContext();
        var stored = ctx[_userTable].QuerySingle<int>( "select ExtendedCultureId from CK.tUser where UserId = @UserId;", new { UserId = userId } );
        profile.ExtendedCultureId.ShouldBe( stored );
        profile.ExtendedCultureId.ShouldBeGreaterThan( 0 );
    }

    [Test]
    public async Task can_create_user_with_extendedCulture_Async()
    {
        var otherCultureId = PickAnotherCultureId();
        var createCmd = _pocoDir.Create<IO.User.PreferredCulture.ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = Guid.NewGuid().ToString();
            c.ExtendedCultureId = otherCultureId;
        } );
        var execCreateCmd = await _executor.ExecuteRootCommandAsync( (IAbstractCommand)createCmd );
        var createRes = execCreateCmd.WithResult<ICreateUserCommandResult>().Result.ShouldNotBeNull();
        createRes.ShouldNotBeAssignableTo<ICrisResultError>();
        createRes.Success.ShouldBeTrue();
        createRes.UserIdResult.ShouldBeGreaterThan( 2 );

        using var ctx = new SqlStandardCallContext();
        var profile = await _userTable.GetUserProfileAsync<IO.User.PreferredCulture.IUserProfile>( ctx, actorId: 1, userId: createRes.UserIdResult );
        profile.ShouldNotBeNull();
        profile.ExtendedCultureId.ShouldBe( otherCultureId );
    }

    [Test]
    public async Task can_set_user_extendedCulture_Async()
    {
        var otherCultureId = PickAnotherCultureId();
        var createCmd = _pocoDir.Create<IO.User.PreferredCulture.ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = Guid.NewGuid().ToString();
        } );
        var execCreateCmd = await _executor.ExecuteRootCommandAsync( (IAbstractCommand)createCmd );
        var createRes = execCreateCmd.WithResult<ICreateUserCommandResult>().Result;

        var cmd = _pocoDir.Create<IO.User.PreferredCulture.ISetUserExtendedCultureCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = createRes.UserIdResult;
            c.ExtendedCultureId = otherCultureId;
        } );
        var execSetCmd = await _executor.ExecuteRootCommandAsync( cmd );
        var res = execSetCmd.WithResult<ICrisBasicCommandResult>().Result;
        res.UserMessages.ShouldNotBeNull();
        using var ctx = new SqlStandardCallContext();
        var profile = await _userTable.GetUserProfileAsync<IO.User.PreferredCulture.IUserProfile>( ctx, actorId: 1, userId: createRes.UserIdResult );
        profile.ShouldNotBeNull();
        profile.ExtendedCultureId.ShouldBe( otherCultureId );
    }

    // Picks any valid culture other than the default one.
    int PickAnotherCultureId()
    {
        using var ctx = new SqlStandardCallContext();
        return ctx[_userTable].QuerySingle<int>(
            "select top 1 CultureId from CK.tCulture where CultureId <> @Cur order by CultureId;",
            new { Cur = DefaultExtendedCultureId } );
    }
}
