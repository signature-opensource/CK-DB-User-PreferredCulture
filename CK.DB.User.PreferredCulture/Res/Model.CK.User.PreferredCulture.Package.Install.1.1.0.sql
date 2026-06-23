--[beginscript]

alter table CK.tUser add
    PreferredCultureName nvarchar( 48 ) not null
        constraint DF_CK_tUser_PreferredCulture default( N'en' );

alter table CK.tUser add ExtendedCultureId int not null
    constraint FK_CK_tUser_ExtendedCultureId foreign key( ExtendedCultureId ) references CK.tCulture( CultureId )
    constraint DF_CK_tUser_ExtendedCultureId default( 210327884 );

--[endscript]
