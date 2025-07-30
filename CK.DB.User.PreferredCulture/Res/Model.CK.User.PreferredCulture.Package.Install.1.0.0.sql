--[beginscript]

alter table CK.tUser add 
    PreferredCultureName nvarchar( 48 ) not null
        constraint DF_CK_tUser_PreferredCulture default( N'en' );

--[endscript]
