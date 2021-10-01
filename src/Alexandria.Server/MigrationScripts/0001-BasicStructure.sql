create table Authors
(
   AuthorId binary(16) not null,
   Name nvarchar(255) COLLATE 'utf8_bin' not null,
   primary key (AuthorId)
)
COLLATE utf8_bin
;



create table Books
(
   BookId binary(16) not null,
   Title nvarchar(1024) COLLATE 'utf8_bin' not null,
   primary key (BookId)
)
COLLATE utf8_bin
;


create table BookAuthors
(
    AuthorId binary(16) not null,
    BookId   binary(16) not null
)
COLLATE utf8_bin
;