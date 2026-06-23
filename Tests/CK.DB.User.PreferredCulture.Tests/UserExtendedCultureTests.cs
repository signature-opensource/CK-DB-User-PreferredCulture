using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using CK.Testing;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.User.PreferredCulture.Tests;

[TestFixture]
public class UserExtendedCultureTests
{
    // Value of the DF_CK_tUser_ExtendedCultureId default constraint (the "fr" culture seeded by CK.DB.Globalization).
    const int DefaultExtendedCultureId = 210327884;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    AsyncServiceScope _scope;
    Package _package;
    UserTable _userTable;
#pragma warning restore CS8618

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _scope = SharedEngine.AutomaticServices.CreateAsyncScope();
        var services = _scope.ServiceProvider;
        _package = services.GetRequiredService<Package>();
        _userTable = services.GetRequiredService<UserTable>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _scope.DisposeAsync();
    }

    [Test]
    public async Task new_user_has_the_default_extended_culture_Async()
    {
        using var ctx = new SqlStandardCallContext( TestHelper.Monitor );
        var userId = await _userTable.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );

        var cultureId = ReadExtendedCultureId( ctx, userId );
        cultureId.ShouldBe( DefaultExtendedCultureId );
    }

    [Test]
    public async Task set_extended_culture_updates_the_user_Async()
    {
        using var ctx = new SqlStandardCallContext( TestHelper.Monitor );
        var userId = await _userTable.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );

        // Picks any valid culture other than the current one.
        var otherCultureId = ctx[_userTable].QuerySingle<int>(
            "select top 1 CultureId from CK.tCulture where CultureId <> @Cur order by CultureId;",
            new { Cur = DefaultExtendedCultureId } );

        await _package.SetExtendedCultureAsync( ctx, 1, userId, otherCultureId );

        ReadExtendedCultureId( ctx, userId ).ShouldBe( otherCultureId );
    }

    [Test]
    public async Task set_extended_culture_to_an_unknown_culture_throws_Async()
    {
        using var ctx = new SqlStandardCallContext( TestHelper.Monitor );
        var userId = await _userTable.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );

        // -1 cannot exist in CK.tCulture.
        await Util.Invokable( () => _package.SetExtendedCultureAsync( ctx, 1, userId, -1 ) )
                  .ShouldThrowAsync<SqlDetailedException>();

        // The user has not been touched.
        ReadExtendedCultureId( ctx, userId ).ShouldBe( DefaultExtendedCultureId );
    }

    [Test]
    public async Task anonymous_cannot_set_extended_culture_Async()
    {
        using var ctx = new SqlStandardCallContext( TestHelper.Monitor );
        var userId = await _userTable.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );

        await Util.Invokable( () => _package.SetExtendedCultureAsync( ctx, 0, userId, DefaultExtendedCultureId ) )
                  .ShouldThrowAsync<SqlDetailedException>();
    }

    [Test]
    public async Task set_extended_culture_with_invalid_userId_throws_Async()
    {
        using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

        await Util.Invokable( () => _package.SetExtendedCultureAsync( ctx, 1, 0, DefaultExtendedCultureId ) )
                  .ShouldThrowAsync<SqlDetailedException>();
    }

    int ReadExtendedCultureId( ISqlCallContext ctx, int userId )
        => ctx[_userTable].QuerySingle<int>(
            "select ExtendedCultureId from CK.tUser where UserId = @UserId;",
            new { UserId = userId } );
}
