--[beginscript]

-- Récupère ExtendedCultureId depuis l'ancien PreferredCultureName avant de supprimer la colonne.
-- Seuls les users encore au défaut (210327884 = 'fr') sont mis à jour : les ExtendedCultureId déjà
-- fixés explicitement via CK.sUserExtendedCultureSet sont préservés.
-- Comparaison insensible à la casse : match exact du nom prioritaire, sinon racine de langue
-- (partie avant '-'). Les noms non résolus gardent le défaut.
update u
set u.ExtendedCultureId = resolved.CultureId
from CK.tUser u
cross apply
(
    select top 1 c.CultureId
    from CK.tCulture c
    where c.Name = u.PreferredCultureName collate Latin1_General_100_CI_AS
       or c.Name = left( u.PreferredCultureName,
                         case when charindex( '-', u.PreferredCultureName ) > 0
                              then charindex( '-', u.PreferredCultureName ) - 1
                              else len( u.PreferredCultureName ) end ) collate Latin1_General_100_CI_AS
    order by case when c.Name = u.PreferredCultureName collate Latin1_General_100_CI_AS then 0 else 1 end
) resolved
where u.ExtendedCultureId = 210327884
  and u.PreferredCultureName is not null
  and u.PreferredCultureName <> N'';

-- Suppression de la colonne et de sa contrainte de défaut.
alter table CK.tUser drop constraint DF_CK_tUser_PreferredCulture;
alter table CK.tUser drop column PreferredCultureName;

--[endscript]
