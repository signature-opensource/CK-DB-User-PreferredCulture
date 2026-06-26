--[beginscript]

alter table CK.tUser add ExtendedCultureId int not null
    constraint FK_CK_tUser_ExtendedCultureId foreign key( ExtendedCultureId ) references CK.tCulture( CultureId )
    constraint DF_CK_tUser_ExtendedCultureId default( 210327884 );

--[endscript]
