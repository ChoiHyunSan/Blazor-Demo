create table if not exists notes (
  id bigserial primary key,
  title text not null,
  created_at timestamptz not null default now()
);

create table if not exists notes_adv (
  id          bigserial   primary key,
  title       text        not null,
  content     text        null,
  created_at  timestamptz not null default now(),
  updated_at  timestamptz not null default now(),
  archived    boolean     not null default false
);

create table if not exists users (
	id 		serial 	primary key,
    name 	text 	not null
);

create table if not exists mails (
	id 			serial 		primary key,
    user_id 	int 		not null,
    title  		text		not null,
    body 		text 		not null,
    item_id 	integer 	not null,
    item_count	integer 	not null,
);