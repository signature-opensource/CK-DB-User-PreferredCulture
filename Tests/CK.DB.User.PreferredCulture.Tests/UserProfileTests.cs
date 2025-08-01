using CK.Core;
using CK.Cris;
using CK.DB.Actor;
using CK.IO.Actor;
using CK.SqlServer;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace CK.DB.User.PreferredCulture.Tests;

[TestFixture]
public class UserProfileTests
{
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
        profile.PreferredCultureName.ShouldBe( "en" );
    }

    [Test]
    public async Task can_create_user_with_preferredCulture_Async()
    {
        var createCmd = _pocoDir.Create<IO.User.PreferredCulture.ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = Guid.NewGuid().ToString();
            c.PreferredCultureName = "en";
        } );
        var execCreateCmd = await _executor.ExecuteRootCommandAsync( (IAbstractCommand)createCmd );
        var createRes = execCreateCmd.WithResult<ICreateUserCommandResult>().Result.ShouldNotBeNull();
        createRes.ShouldNotBeAssignableTo<ICrisResultError>();
        createRes.Success.ShouldBeTrue();
        createRes.UserIdResult.ShouldBeGreaterThan( 2 );
    }

    [Test]
    public async Task can_set_user_preferredCulture_Async()
    {
        var createCmd = _pocoDir.Create<IO.User.PreferredCulture.ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = Guid.NewGuid().ToString();
            c.PreferredCultureName = "en";
        } );
        var execCreateCmd = await _executor.ExecuteRootCommandAsync( (IAbstractCommand)createCmd );
        var createRes = execCreateCmd.WithResult<ICreateUserCommandResult>().Result;

        var cmd = _pocoDir.Create<IO.User.PreferredCulture.ISetUserPreferredCultureCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = createRes.UserIdResult;
            c.PreferredCultureName = "fr";
        } );
        var execSetCmd = await _executor.ExecuteRootCommandAsync( cmd );
        var res = execSetCmd.WithResult<ICrisBasicCommandResult>().Result;
        res.UserMessages.ShouldNotBeNull();
        using var ctx = new SqlStandardCallContext();
        var profile = await _userTable.GetUserProfileAsync<IO.User.PreferredCulture.IUserProfile>( ctx, actorId: 1, userId: createRes.UserIdResult );
        profile.ShouldNotBeNull();
        profile.PreferredCultureName.ShouldBe( "fr" );
    }
}
