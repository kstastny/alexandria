create table Authors
(
   AuthorId binary(16) not null,
   Name nvarchar(255) not null,
   primary key (AuthorId)
);


create table Books
(
   BookId binary(16) not null,
   Title nvarchar(1024) not null,
   primary key (BookId)
);

create table BookAuthors
(
    AuthorId binary(16) not null,
    BookId   binary(16) not null
);
