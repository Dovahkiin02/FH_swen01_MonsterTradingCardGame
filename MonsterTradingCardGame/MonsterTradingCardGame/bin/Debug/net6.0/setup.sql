drop table if exists stack;
drop table if exists deck;
drop table if exists store;
drop table if exists card;
drop table if exists player;

create extension if not exists pgcrypto;
create extension if not exists "uuid-ossp";

create table if not exists player (
	id       uuid primary key default uuid_generate_v4()
,	name     text not null unique
,	password text not null
,	coins    int  not null
,	role     int  not null
,	wins	 int  not null
,   defeats  int  not null
,   ties     int  not null
);

create table if not exists card (
	id      serial primary key
,	name    text   not null
,	element int    not null
,	damage  int    not null
,	type    int    not null
);

create table store (
	id          serial primary key
,	card        int  not null references card(id)
,   seller      uuid not null references player(id)
,	price_card  int           references card(id)
,	price_coins int
,	active      bool not null
);

create table if not exists stack (
	id serial primary key
,	player uuid not null references player(id)
,   card   int  not null references card(id)
);

create table if not exists deck (
	id serial primary key
,	player uuid not null references player(id)
,   card   int  not null references card(id)
);