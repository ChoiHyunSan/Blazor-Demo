create table if not exists notes (
  id bigserial primary key,
  title text not null,
  created_at timestamptz not null default now()
);

create table if not exists notes_adv (
  id          bigserial primary key,
  title       text        not null,
  content     text        null,
  created_at  timestamptz not null default now(),
  updated_at  timestamptz not null default now(),
  archived    boolean     not null default false
);