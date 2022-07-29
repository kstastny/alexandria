alter table Authors
add column SortByName nvarchar(255) COLLATE 'utf8_bin' not null;

alter table Books
add column SortByTitle nvarchar(255) COLLATE 'utf8_bin' not null;


update Authors set SortByName = lower(Name);


update Books set SortByTitle = lower(Title);