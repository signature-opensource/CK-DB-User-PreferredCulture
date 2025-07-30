using CK.Core;
using CK.Cris;
using CK.IO.User.UserProfile;
using CK.SqlServer;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.User.UserProfile.Tests;

[TestFixture]
public class UserProfileTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    AutomaticServices _automaticServices;
    AsyncServiceScope _scope;
    CrisBackgroundExecutor _backgroundExecutor;
    PocoDirectory _pocoDir;
    Package _package;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        var configuration = TestHelper.CreateDefaultEngineConfiguration();
        configuration.FirstBinPath.Path = TestHelper.BinFolder;
        configuration.FirstBinPath.Assemblies.Add( "CK.DB.User.UserProfile" );
        configuration.FirstBinPath.Types.Add( typeof( CrisBackgroundExecutorService ),
                                              typeof( CrisBackgroundExecutor ) );
        configuration.EnsureSqlServerConfigurationAspect();

        var r = await configuration.RunSuccessfullyAsync();
        _automaticServices = r.CreateAutomaticServices();
        _scope = _automaticServices.Services.CreateAsyncScope();
        var services = _scope.ServiceProvider;

        _pocoDir = services.GetRequiredService<PocoDirectory>();
        _backgroundExecutor = services.GetRequiredService<CrisBackgroundExecutor>();
        _package = services.GetRequiredService<Package>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _scope.DisposeAsync();
        await _automaticServices.DisposeAsync();
    }

    [Test]
    public async Task can_get_userProfile_through_Cris_Async()
    {
        var userId = 1;
        var cmd = _pocoDir.Create<IGetUserProfileQCommand>( cmd =>
        {
            cmd.ActorId = 1;
            cmd.UserId = userId;
        } );
        var executingCmd = _backgroundExecutor.Submit( TestHelper.Monitor, cmd, incomingValidationCheck: false )
                                              .WithResult<IUserProfile?>();

        var profile = await executingCmd.Result;
        profile.ShouldNotBeNull();
        profile.UserId.ShouldBe( userId );
        profile.UserName.ShouldBe( "System" );
    }

    [Test]
    public async Task can_create_user_through_Cris_Async()
    {
        var userName = Guid.NewGuid().ToString();
        var cmd = _pocoDir.Create<ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = userName;
        } );
        var executingCmd = _backgroundExecutor.Submit( TestHelper.Monitor, cmd, incomingValidationCheck: false )
                                              .WithResult<ICreateUserCommandResult>();
        var res = await executingCmd.Result;
        res.CreatedUserId.ShouldBeGreaterThan( 1 );
        res.UserMessages.ShouldNotBeNull();
    }

    [Test]
    public async Task cannot_create_user_with_invalid_username_Async()
    {
        var cmd = _pocoDir.Create<ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = string.Empty;
        } );
        var executingCmd = _backgroundExecutor.Submit( TestHelper.Monitor, cmd )
                                              .WithResult<ICreateUserCommandResult>();

        await Util.Invokable( () => executingCmd.Result )
            .ShouldThrowAsync<Exception>( "Username could not be null, empty nor whitespace" );

        cmd = _pocoDir.Create<ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = " ";
        } );
        executingCmd = _backgroundExecutor.Submit( TestHelper.Monitor, cmd )
                                          .WithResult<ICreateUserCommandResult>();

        await Util.Invokable( () => executingCmd.Result )
            .ShouldThrowAsync<Exception>( "Username could not be null, empty nor whitespace" );
    }

    [Test]
    public async Task can_update_user_through_Cris_Async()
    {
        var createCmd = _pocoDir.Create<ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = Guid.NewGuid().ToString();
        } );
        var executingCreateCmd = _backgroundExecutor.Submit( TestHelper.Monitor, createCmd, incomingValidationCheck: false )
                                                    .WithResult<ICreateUserCommandResult>();
        var createRes = await executingCreateCmd.Result;

        var newName = Guid.NewGuid().ToString();
        var cmd = _pocoDir.Create<IUpdateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = createRes.CreatedUserId;
            c.UserName = newName;
        } );
        var executingCmd = _backgroundExecutor.Submit( TestHelper.Monitor, cmd, incomingValidationCheck: false )
                                              .WithResult<ICrisBasicCommandResult>();
        var res = await executingCmd.Result;
        res.UserMessages.ShouldNotBeNull();
        var profile = await _package.GetUserProfileAsync( new SqlStandardCallContext(), 1, createRes.CreatedUserId );
        profile.ShouldNotBeNull();
        profile.UserName.ShouldBe( newName );
    }

    [Test]
    public async Task cannot_update_user_with_invalid_username_Async()
    {
        var createCmd = _pocoDir.Create<ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = Guid.NewGuid().ToString();
        } );
        var executingCreateCmd = _backgroundExecutor.Submit( TestHelper.Monitor, createCmd, incomingValidationCheck: false )
                                                    .WithResult<ICreateUserCommandResult>();

        var createRes = await executingCreateCmd.Result;
        var ucmd = _pocoDir.Create<IUpdateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = 0;
        } );
        var executingUCmd = _backgroundExecutor.Submit( TestHelper.Monitor, ucmd )
                                              .WithResult<ICrisBasicCommandResult>();
        await Util.Invokable( () => executingUCmd.Result )
            .ShouldThrowAsync<Exception>( "UserId must be greater than 0" );

        var cmd = _pocoDir.Create<IUpdateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = createRes.CreatedUserId;
            c.UserName = string.Empty;
        } );
        var executingCmd = _backgroundExecutor.Submit( TestHelper.Monitor, cmd )
                                              .WithResult<ICrisBasicCommandResult>();
        await Util.Invokable( () => executingCmd.Result )
            .ShouldThrowAsync<Exception>( "Username could not be null, empty nor whitespace" );

        cmd = _pocoDir.Create<IUpdateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = createRes.CreatedUserId;
            c.UserName = " ";
        } );
        executingCmd = _backgroundExecutor.Submit( TestHelper.Monitor, cmd )
                                              .WithResult<ICrisBasicCommandResult>();
        await Util.Invokable( () => executingCmd.Result )
            .ShouldThrowAsync<Exception>( "Username could not be null, empty nor whitespace" );
    }

    [Test]
    public async Task can_destroy_user_through_Cris_Async()
    {
        var createCmd = _pocoDir.Create<ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = Guid.NewGuid().ToString();
        } );
        var executingCreateCmd = _backgroundExecutor.Submit( TestHelper.Monitor, createCmd, incomingValidationCheck: false )
                                                    .WithResult<ICreateUserCommandResult>();
        var createRes = await executingCreateCmd.Result;
        var cmd = _pocoDir.Create<IDestroyUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = createRes.CreatedUserId;
        } );
        var executingCmd = _backgroundExecutor.Submit( TestHelper.Monitor, cmd, incomingValidationCheck: false )
                                              .WithResult<ICrisBasicCommandResult>();
        var res = await executingCmd.Result;
        res.UserMessages.Where( um => um.Level == UserMessageLevel.Error ).ShouldBeEmpty();

        var getcmd = _pocoDir.Create<IGetUserProfileQCommand>( cmd =>
        {
            cmd.ActorId = 1;
            cmd.UserId = createRes.CreatedUserId;
        } );
        var executingGetCmd = _backgroundExecutor.Submit( TestHelper.Monitor, getcmd, incomingValidationCheck: false )
                                                 .WithResult<IUserProfile?>();

        var profile = await executingGetCmd.Result;
        profile.ShouldBeNull();
    }
}
