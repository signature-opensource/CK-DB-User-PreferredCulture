create view CK.vUserProfile
as
    select u.UserId,
           u.UserName
    from CK.tUser u;
