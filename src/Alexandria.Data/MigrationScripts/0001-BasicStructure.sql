create table Authors
(
   AuthorId binary(16) not null,
   Name nvarchar(255) COLLATE 'utf8_bin' not null,
   primary key (AuthorId)
)
COLLATE utf8_bin
;


create table Locations
(
    LocationId binary(16) not null,
    Name nvarchar(255) COLLATE 'utf8_bin' not null,
    primary key (LocationId)
);


create table Books
(
   BookId binary(16) not null,
   Title nvarchar(1024) COLLATE 'utf8_bin' not null,
   Year smallint unsigned null,
   InventoryLocationId binary(16) null,
   Note text null,
   primary key (BookId),
   constraint `fk_book_inventoryLocation`
        foreign key (InventoryLocationId) references Locations (LocationId)
        on delete restrict
        on update restrict
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