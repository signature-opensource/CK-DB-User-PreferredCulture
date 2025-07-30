create procedure CK.sUserPreferredCultureNameSet
(
    @ActorId int,
    @UserId int,
    @PreferredCultureName nvarchar( 255 )
)
as
begin
	if @ActorId <= 0 throw 50000, 'Argument.InvalidActorId', 1;
	if @UserId <= 0 throw 50000, 'Argument.InvalidUserId', 1;

    declare @Error nvarchar( 255 ) = 'The targeted user could not be found. (UserId: ' + CAST(@UserId AS nvarchar( 11 )) + ')||User.UnknownUser';
    if not exists ( select 1 from CK.tUser where UserId = @UserId )
        throw 50000, @Error, 1;

    --<PreUpdate revert />

    update CK.tUser
    set PreferredCultureName = @PreferredCultureName
    where UserId = @UserId;

    --<PostUpdate revert />
end
