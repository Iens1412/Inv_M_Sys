--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4 (Debian 17.4-1.pgdg120+2)
-- Dumped by pg_dump version 17.4 (Debian 17.4-1.pgdg120+2)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: categories; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.categories (
    id integer NOT NULL,
    name text NOT NULL
);


ALTER TABLE public.categories OWNER TO admin;

--
-- Name: categories_id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.categories_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.categories_id_seq OWNER TO admin;

--
-- Name: categories_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.categories_id_seq OWNED BY public.categories.id;


--
-- Name: customers; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.customers (
    id integer NOT NULL,
    companyname text,
    firstname text NOT NULL,
    lastname text NOT NULL,
    email text,
    phonenumber text,
    address text,
    notes text
);


ALTER TABLE public.customers OWNER TO admin;

--
-- Name: customers_id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.customers_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.customers_id_seq OWNER TO admin;

--
-- Name: customers_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.customers_id_seq OWNED BY public.customers.id;


--
-- Name: orderitems; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.orderitems (
    id integer NOT NULL,
    orderid integer,
    productid integer,
    quantity integer NOT NULL,
    price numeric NOT NULL,
    totalprice numeric NOT NULL
);


ALTER TABLE public.orderitems OWNER TO admin;

--
-- Name: orderitems_id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.orderitems_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.orderitems_id_seq OWNER TO admin;

--
-- Name: orderitems_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.orderitems_id_seq OWNED BY public.orderitems.id;


--
-- Name: orders; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.orders (
    id integer NOT NULL,
    customerid integer,
    deliverydate date NOT NULL,
    totalprice numeric NOT NULL,
    status text DEFAULT 'Pending'::text NOT NULL,
    isdeleted boolean DEFAULT false NOT NULL,
    deletedat timestamp without time zone,
    deletedby character varying(100)
);


ALTER TABLE public.orders OWNER TO admin;

--
-- Name: orders_id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.orders_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.orders_id_seq OWNER TO admin;

--
-- Name: orders_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.orders_id_seq OWNED BY public.orders.id;


--
-- Name: owner; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.owner (
    id integer NOT NULL,
    username text NOT NULL,
    password text NOT NULL,
    role text NOT NULL
);


ALTER TABLE public.owner OWNER TO admin;

--
-- Name: owner_id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.owner_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.owner_id_seq OWNER TO admin;

--
-- Name: owner_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.owner_id_seq OWNED BY public.owner.id;


--
-- Name: ownersessions; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.ownersessions (
    sessionid integer NOT NULL,
    userid integer NOT NULL,
    token text NOT NULL,
    expirydate timestamp without time zone NOT NULL
);


ALTER TABLE public.ownersessions OWNER TO admin;

--
-- Name: ownersessions_sessionid_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.ownersessions_sessionid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.ownersessions_sessionid_seq OWNER TO admin;

--
-- Name: ownersessions_sessionid_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.ownersessions_sessionid_seq OWNED BY public.ownersessions.sessionid;


--
-- Name: products; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.products (
    id integer NOT NULL,
    productname text NOT NULL,
    categoryid integer NOT NULL,
    quantity integer NOT NULL,
    price numeric NOT NULL,
    minquantity integer NOT NULL,
    supplier text NOT NULL,
    description text
);


ALTER TABLE public.products OWNER TO admin;

--
-- Name: products_id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.products_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.products_id_seq OWNER TO admin;

--
-- Name: products_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.products_id_seq OWNED BY public.products.id;


--
-- Name: reportdetails; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.reportdetails (
    id integer NOT NULL,
    reportid integer NOT NULL,
    orderid integer NOT NULL,
    customername text NOT NULL,
    deliverydate date NOT NULL,
    totalprice numeric NOT NULL,
    status text NOT NULL
);


ALTER TABLE public.reportdetails OWNER TO admin;

--
-- Name: reportdetails_id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.reportdetails_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.reportdetails_id_seq OWNER TO admin;

--
-- Name: reportdetails_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.reportdetails_id_seq OWNED BY public.reportdetails.id;


--
-- Name: reports; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.reports (
    id integer NOT NULL,
    reporttitle text NOT NULL,
    reporttype text NOT NULL,
    details text,
    startdate date,
    enddate date,
    status text,
    date timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.reports OWNER TO admin;

--
-- Name: reports_id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.reports_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.reports_id_seq OWNER TO admin;

--
-- Name: reports_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.reports_id_seq OWNED BY public.reports.id;


--
-- Name: schedule; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.schedule (
    id integer NOT NULL,
    employeeid integer NOT NULL,
    dayname text NOT NULL,
    starttime time without time zone NOT NULL,
    endtime time without time zone NOT NULL,
    reststarttime time without time zone,
    restendtime time without time zone,
    totalhours double precision NOT NULL,
    isrestday boolean DEFAULT false NOT NULL
);


ALTER TABLE public.schedule OWNER TO admin;

--
-- Name: schedule_id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.schedule_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.schedule_id_seq OWNER TO admin;

--
-- Name: schedule_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.schedule_id_seq OWNED BY public.schedule.id;


--
-- Name: users; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.users (
    id integer NOT NULL,
    firstname text NOT NULL,
    lastname text NOT NULL,
    email text NOT NULL,
    phonenumber text NOT NULL,
    username text NOT NULL,
    password text NOT NULL,
    address text,
    role text NOT NULL
);


ALTER TABLE public.users OWNER TO admin;

--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_id_seq OWNER TO admin;

--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: usersessions; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.usersessions (
    sessionid integer NOT NULL,
    userid integer NOT NULL,
    token text NOT NULL,
    expirydate timestamp without time zone NOT NULL
);


ALTER TABLE public.usersessions OWNER TO admin;

--
-- Name: usersessions_sessionid_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.usersessions_sessionid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.usersessions_sessionid_seq OWNER TO admin;

--
-- Name: usersessions_sessionid_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.usersessions_sessionid_seq OWNED BY public.usersessions.sessionid;


--
-- Name: categories id; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.categories ALTER COLUMN id SET DEFAULT nextval('public.categories_id_seq'::regclass);


--
-- Name: customers id; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.customers ALTER COLUMN id SET DEFAULT nextval('public.customers_id_seq'::regclass);


--
-- Name: orderitems id; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.orderitems ALTER COLUMN id SET DEFAULT nextval('public.orderitems_id_seq'::regclass);


--
-- Name: orders id; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.orders ALTER COLUMN id SET DEFAULT nextval('public.orders_id_seq'::regclass);


--
-- Name: owner id; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.owner ALTER COLUMN id SET DEFAULT nextval('public.owner_id_seq'::regclass);


--
-- Name: ownersessions sessionid; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.ownersessions ALTER COLUMN sessionid SET DEFAULT nextval('public.ownersessions_sessionid_seq'::regclass);


--
-- Name: products id; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.products ALTER COLUMN id SET DEFAULT nextval('public.products_id_seq'::regclass);


--
-- Name: reportdetails id; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.reportdetails ALTER COLUMN id SET DEFAULT nextval('public.reportdetails_id_seq'::regclass);


--
-- Name: reports id; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.reports ALTER COLUMN id SET DEFAULT nextval('public.reports_id_seq'::regclass);


--
-- Name: schedule id; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.schedule ALTER COLUMN id SET DEFAULT nextval('public.schedule_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Name: usersessions sessionid; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.usersessions ALTER COLUMN sessionid SET DEFAULT nextval('public.usersessions_sessionid_seq'::regclass);


--
-- Data for Name: categories; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.categories (id, name) FROM stdin;
1	Elektronik
2	Mobiltelefoner
3	Surfplattor
4	Datorer
5	B├ñrbara datorer
6	Skrivare
7	Sk├ñrmar
8	TV & Hemmabio
9	Kameror
10	Ljudutrustning
11	H├╢rlurar
12	Tillbeh├╢r elektronik
13	Kontorsmaterial
14	Pennor
15	Papper
16	Skrivblock
17	Mappar
18	Bl├ñckpatroner
19	M├╢bler
20	Skrivbord
21	Stolar
22	F├╢rvaringsl├╢sningar
23	Hyllor
24	Belysning
25	Hush├Ñllsartiklar
26	K├╢ksutrustning
27	Kastruller & Stekpannor
28	Bestick
29	Tallrikar
30	Koppar & Glas
31	Reng├╢ringsprodukter
32	Tv├ñttmedel
33	Verktyg
34	Handverktyg
35	Elverktyg
36	Skruv & Spik
37	Byggmaterial
38	Skyddsutrustning
39	Tr├ñdg├Ñrdsverktyg
40	VVS-artiklar
41	Elmaterial
42	Kl├ñder
43	Arbetskl├ñder
44	T-shirts
45	Byxor
46	Jackor
47	Skor
48	Regnkl├ñder
49	Barnkl├ñder
50	Livsmedel
51	Konserver
52	Snacks
53	Godis
54	Dryck
55	F├ñrskvaror
56	Frysvaror
57	Mejeriprodukter
58	Bakprodukter
59	K├╢tt & Fisk
60	Frukt & Gr├╢nt
61	Leksaker
62	Br├ñdspel
63	Byggsatser
64	Pedagogiska leksaker
65	Dockor & Figurer
66	Utomhusleksaker
67	Fordon & Radiostyrt
68	Sk├╢nhet & H├ñlsa
69	Hudv├Ñrd
70	H├Ñrv├Ñrd
71	Smink
72	Parfymer
73	Hygienprodukter
74	F├╢rsta hj├ñlpen
75	Medicin
76	Vitaminer
77	Sport & Fritid
78	Tr├ñningsutrustning
79	Cyklar
80	Sportkl├ñder
81	Bollar
82	Camping
83	Fiskeutrustning
84	Jaktutrustning
85	Resv├ñskor & V├ñskor
86	Bil & Fordon
87	Bildelar
88	Bilv├Ñrd
90	Oljor & V├ñtskor
91	Tillbeh├╢r till bilen
92	IT & N├ñtverk
93	Routrar
94	Switchar
95	N├ñtverkskablar
96	Datatillbeh├╢r
97	Programvara
98	S├ñkerhet
99	Kamerasystem
100	Larm
101	Brandskydd
102	L├Ñs & Nycklar
103	Tillbeh├╢r & ├ûvrigt
104	d├ñck
\.


--
-- Data for Name: customers; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.customers (id, companyname, firstname, lastname, email, phonenumber, address, notes) FROM stdin;
1	Karlsson & S├╢derberg AB	Eva	Nyman	jacobpersson@larsson.org	08-41 08 26	Furugatan 1, 40720 Karlskoga	Facere vitae quibusdam temporibus dicta nisi.
2	Mattsson AB	Ingeg├ñrd	Ekberg	kristina18@gmail.com	0756-085 58	Ekstigen 97, 93442 Falun	Perferendis natus tenetur cum laboriosam.
3	Larsson AB	Martin	Sundin	berggrengustav@andersson.net	+46 (0)8 399 322 91	Storgatan 4, 74245 V├ñxj├╢	Laborum facere possimus quam debitis molestias.
4	Pettersson & Persson AB	Ingemar	Johansson	ingemarlundberg@telia.com	+46 (0)8 575 562 54	Storv├ñgen 22, 45013 Sk├╢vde	Reiciendis sunt molestiae vel.
5	Lundin & Nilsson HB	Sigvard	Persson	kristinajakobsson@eriksson.com	+46 (0)8 851 632 26	Nyv├ñgen 80, 14001 Lund	Deleniti nam non fuga numquam laborum eum animi.
6	Lund Larsson HB	Viola	Fransson	anderssonerik@telia.com	08-552 16 59	Bj├╢rktorget 646, 49696 Eskilstuna	Aspernatur voluptate perferendis voluptatum veniam.
7	G├╢ransson & Nilsson HB	Fredrik	Andersson	berggrenalice@svensson.se	047-112 16 29	Parkgatan 53, 17921 Ume├Ñ	Expedita pariatur ea praesentium autem deleniti.
8	Sj├╢gren & Svensson HB	Daniel	Davidsson	lilianpalm@googlemail.com	08-74 95 01	Kvarntorget 319, 55365 Borl├ñnge	Consequuntur sunt placeat tempore odit qui.
9	Lundin AB	Lars	Hermansson	marieolsson@persson.org	0568-974 37	├àkerv├ñgen 464, 40550 Sk├╢vde	At repellat corrupti nobis laudantium voluptates.
10	S├╢derstr├╢m Carlsson AB	Bengt	Sv├ñrd	pkarlsson@yahoo.de	+46 (0)8 703 376 04	B├ñckv├ñgen 377, 12751 Sandviken	Earum ab laudantium fuga quasi incidunt ratione.
11	Andersson Ljung HB	Annika	Ericsson	magnussonulf@pettersson.org	+46 (0)514 499 34	Skolv├ñgen 112, 56094 Kalmar	Eveniet molestiae mollitia.
12	Larsson & Samuelsson AB	Ulrika	├àkesson	zsjoberg@johansson.se	0232-01 09 25	Kyrkstigen 481, 18493 J├╢nk├╢ping	Architecto culpa illum dignissimos repudiandae repellat.
13	Gustafsson & Olsson AB	Eva	Sv├ñrd	hakan10@jansson.se	08-41 46 50	B├ñckgatan 656, 81475 Lidk├╢ping	Illum praesentium ut.
14	Isaksson AB	├àke	Nilsson	claesolsson@sundstrom.com	+46 (0)91 31 82 85	├ängsgr├ñnd 756, 34357 Malm├╢	Possimus maxime perferendis asperiores impedit occaecati.
15	Str├╢m & Persson HB	G├╢ran	Andersson	nilssonaina@telia.com	08-21 36 31	Kyrkov├ñgen 209, 45986 Liding├╢	Praesentium rerum dolorem iusto sunt repellendus molestiae.
16	Ljungberg & Karlsson AB	Birgitta	Johansson	bertil96@larsson.net	0794-475 56	Tr├ñdg├Ñrdsgr├ñnd 768, 20420 Bor├Ñs	Magnam provident illo voluptates nostrum corporis.
17	Johnsson AB	Johan	Johnsson	pkarlsson@olsson.com	+46 (0)573 667 98	Industriv├ñgen 1, 74403 Karlstad	Perferendis incidunt facere quasi error labore.
18	Nilsson & Strandberg AB	Nils	Lundin	petterssonbarbro@live.se	0796-845 11	Industriv├ñgen 60, 29292 Lule├Ñ	Itaque nesciunt deserunt excepturi.
19	Larsson HB	Annika	H├Ñkansson	larssonkurt@lindkvist.org	0157-921 03	Tr├ñdg├Ñrdstorget 609, 11435 Halmstad	Rem dolorum odio vero.
20	Larsson & Holmqvist HB	Olivia	Andersson	elisabethjansson@goransson.com	0820-223 34	Parkgr├ñnd 8, 19258 ├ängelholm	Dicta consectetur dignissimos reprehenderit.
21	Pettersson & Svensson HB	Roger	M├Ñrtensson	hannajohansson@spray.se	08-49 62 54	Stationstorget 9, 22924 Malm├╢	Delectus deserunt omnis saepe cupiditate.
22	Andersson HB	Mona	Olofsson	margaretaholm@yahoo.de	+46 (0)41 12 30 95	Nystigen 07, 70038 Sk├╢vde	Facere perferendis in.
23	Sandberg HB	Edit	Svensson	soderbergnina@nilsson.net	08-788 919 65	├ängsgr├ñnd 429, 81553 Helsingborg	Eos officiis quo sed ut deserunt voluptate.
24	Persson & Johansson AB	Kerstin	Lundqvist	anders06@yahoo.de	+46 (0)8 340 539 52	Storv├ñgen 708, 57208 Karlstad	Perspiciatis sed omnis ratione corrupti facilis voluptatum.
25	Jonsson AB	Gunnar	Andersson	erik52@erlandsson.se	08-651 957 56	Stationsv├ñgen 80, 85635 Borl├ñnge	Sed similique quis fuga dicta.
26	Fransson Sundqvist HB	Magnus	Andersson	nilsaberg@swipnet.se	+46 (0)43 80 90 89	├ängsv├ñgen 501, 34924 M├╢lndal	Fugiat molestias aut perferendis cupiditate qui.
27	Engstr├╢m & Andersson HB	Maud	Larsson	linnealundberg@karlsson.net	0111-81 11 75	Furugr├ñnd 5, 96946 ├ûrebro	Tempora vero tempora delectus numquam fuga libero.
28	Nilsson Lind├⌐n HB	Anna	Andersson	wnyberg@spray.se	+46 (0)984 103 74	Parkgatan 37, 19218 Borl├ñnge	Molestiae ipsa qui fuga amet dignissimos.
29	Hedlund Ek HB	Per	Gunnarsson	plundkvist@gustafsson.org	+46 (0)8 098 749 32	Nystigen 400, 14219 Lund	Beatae atque adipisci exercitationem mollitia vero necessitatibus at.
30	├àberg AB	Pontus	Petersson	lindahlake@spray.se	058-029 58 18	Strandgatan 80, 34835 Sk├╢vde	Optio corrupti laboriosam facere autem doloribus eaque sed.
31	Danielsson Nilsson HB	Christina	Johansson	erikmalm@gmail.com	+46 (0)180 367 32	Ringstigen 053, 59073 Alings├Ñs	Praesentium quibusdam ipsum sed perferendis.
32	Karlsson & ├àberg HB	Elisabeth	Berglund	ingridlindgren@johansson.se	08-821 21 32	J├ñrnv├ñgsgatan 5, 70102 Skellefte├Ñ	Laborum excepturi voluptas earum rem accusantium illo.
33	Olsson & Nyberg HB	Lars	Berntsson	maria69@lindqvist.org	007-024 98 09	Furutorget 2, 15809 Trelleborg	Nobis minus quae fugit quas.
34	Holm HB	Malin	Johansson	ingegerdhansson@gmail.com	08-009 26 88	Genv├ñgen 45, 87338 V├ñxj├╢	Sapiente placeat culpa laudantium tenetur eum tempora.
35	Persson & Karlsson HB	Karin	Martinsson	andreasbjork@gmail.com	+46 (0)65 01 15 74	B├ñckstigen 5, 88217 Helsingborg	Doloremque quisquam quae at laudantium perferendis esse.
36	Nilsson Johnsson AB	Thomas	Nilsson	jgoransson@gmail.com	08-302 97 14	Parktorget 198, 14902 Borl├ñnge	Culpa laudantium non numquam laboriosam voluptatum accusantium.
37	Str├╢mberg Holm AB	Jan	Johansson	marie41@yahoo.de	08-46 15 69	├àkerstigen 2, 37590 Trelleborg	Totam cumque soluta fugit magnam doloribus.
38	Bostr├╢m HB	Carin	Persson	lundbergchrister@gmail.com	08-460 997 65	├ängsgr├ñnd 829, 39077 Halmstad	Ipsam porro dignissimos minus doloremque animi expedita quidem.
39	Jonsson AB	Inger	Gustafsson	zkarlsson@gmail.com	+46 (0)8 687 472 90	Kyrkgatan 3, 48163 M├╢lndal	Maxime vero in alias.
40	Olsson Svensson HB	Ulf	Hansson	mariaberggren@jansson.com	+46 (0)399 134 52	Ringstigen 349, 75411 ├ûrnsk├╢ldsvik	Ad fugiat eaque magnam.
41	Jonsson AB	Lennart	Lundgren	andreasnilsson@yahoo.de	0157-75 85 31	B├ñckstigen 26, 17038 Link├╢ping	Veniam quia perferendis odit ut unde.
42	Petersson Pettersson HB	Paul	Johansson	nsamuelsson@live.se	+46 (0)895 156 03	Furuv├ñgen 7, 95674 ├ûrebro	Architecto totam cum sed repudiandae ducimus.
43	Lindqvist & Johansson HB	Robert	Roos	per46@live.se	08-477 82 97	Ringgatan 86, 23913 G├╢teborg	Accusamus porro cupiditate optio rem autem est.
44	Andersson & J├╢nsson AB	Margareta	Johansson	pkarlsson@live.se	059-952 74 55	Industristigen 73, 26258 ├ûrnsk├╢ldsvik	Sapiente atque laborum ab voluptates.
45	Bengtsson AB	Susanne	Nilsson	anderssongerd@melin.se	+46 (0)46 64 14 39	B├ñckstigen 259, 12995 Lidk├╢ping	Explicabo assumenda impedit commodi provident.
46	Larsson HB	Ulrika	G├╢ransson	nybergkent@spray.se	+46 (0)12 31 12 08	Parktorget 1, 23778 Kristianstad	Eveniet corporis earum hic accusantium fuga.
47	Magnusson HB	Sofia	Carlsson	sebastian73@hansson.com	08-760 423 00	├àkergatan 32, 61984 Helsingborg	Temporibus mollitia iure officiis nihil earum cupiditate.
48	Nilsson HB	Patrik	Pettersson	danielfriberg@gmail.com	0174-288 33	Parkgr├ñnd 2, 24269 Karlskoga	Unde debitis animi earum.
49	Gunnarsson HB	Mikael	Johansson	elisabethpettersson@eriksson.se	006-458 35 78	B├ñckgatan 416, 98944 Alings├Ñs	Doloribus consequuntur consectetur corporis itaque nulla ab ipsam.
50	Svensson & Svensson HB	Ragnar	Samuelsson	karlssonviktor@swipnet.se	015-02 73 35	Villav├ñgen 2, 43503 Helsingborg	Laudantium necessitatibus sed placeat.
51	Berg Jonsson AB	Martin	Svensson	cnilsson@swipnet.se	032-13 51 90	├ängstorget 74, 74444 Varberg	Voluptatibus illum illo perspiciatis saepe sit.
52	Andersson Johnsson AB	Marcus	Norman	daniel97@wikstrom.com	+46 (0)31 36 02 07	Nygatan 291, 29733 Lidk├╢ping	Cum architecto cum excepturi quo deleniti neque repudiandae.
53	Jonasson & Vikstr├╢m HB	Inger	Ivarsson	emeliebergman@moller.com	042-93 81 73	Skolv├ñgen 6, 45905 G├╢teborg	Repellat dolorum neque eaque mollitia soluta.
54	Svensson AB	Ingemar	Andersson	psamuelsson@friberg.com	031-016 64 54	Ringtorget 0, 24710 Kristianstad	Provident nesciunt unde minus.
55	Johannesson Dahlgren HB	Pierre	Bergstr├╢m	samuelkarlsson@live.se	+46 (0)503 782 24	Idrottsgr├ñnd 1, 38760 Stockholm	Porro molestias ducimus.
56	Persson & Lundqvist AB	Torsten	Andersson	evelinacarlsson@swipnet.se	08-62 90 16	Parkstigen 4, 53370 Nyk├╢ping	Sed quisquam enim dolore.
57	Palm & Nyberg AB	Lars	Lindgren	gandersson@live.se	006-678 02 51	B├ñckgr├ñnd 20, 94853 Eskilstuna	Et placeat voluptate velit ipsum neque accusamus.
58	Olsson Jakobsson HB	Anna	Pettersson	mikael34@yahoo.de	0249-831 20	Villagr├ñnd 619, 24625 Stockholm	Deleniti officia nulla illo quis delectus mollitia eligendi.
59	Andersson HB	Karin	Karlsson	margareta45@telia.com	0234-710 93	Kyrkov├ñgen 08, 58240 Ume├Ñ	Cupiditate temporibus fuga.
60	Karlsson Moberg AB	Margaretha	S├╢derlund	tore53@danielsson.com	08-945 514 94	Villagatan 24, 98475 Trelleborg	Debitis delectus consequuntur vero voluptates in molestiae.
61	Engstr├╢m & Karlsson AB	Torbj├╢rn	Pettersson	johanssonmaria@live.se	+46 (0)929 285 11	Tr├ñdg├Ñrdsgatan 7, 57112 ├ängelholm	Numquam atque magni veritatis nisi repellat.
62	Larsson Wallin AB	Frida	Lundqvist	slarsson@swipnet.se	068-23 89 76	Fabriksgr├ñnd 555, 75910 Kristianstad	Ratione nihil accusamus quae vitae fugit.
63	Eriksson Melin AB	Ove	Andersson	gunilla81@norman.com	08-17 83 95	Industrigatan 255, 10725 Lund	Debitis libero numquam veniam.
64	Lindberg Bengtsson AB	Mats	Axelsson	thomas90@googlemail.com	006-089 85 16	Storstigen 1, 33987 Eskilstuna	Ex ut illo sed.
65	Pettersson ├àgren HB	Sandra	Johansson	hnilsson@johnsson.com	052-05 40 61	Grangatan 3, 74308 Helsingborg	Perspiciatis dicta debitis laboriosam ipsam in cumque nisi.
66	├àberg Carlsson AB	Eva	Bostr├╢m	olof91@gmail.com	+46 (0)8 936 998 72	Ringgatan 5, 85734 Lidk├╢ping	Maxime similique exercitationem accusamus.
67	Falk HB	Katarina	G├╢ransson	helen12@gmail.com	029-25 35 98	Skolv├ñgen 355, 34382 Helsingborg	At eius saepe vel natus magnam.
68	Eklund HB	Linnea	Sandberg	robertlindstrom@olsson.se	030-87 04 01	Skogsstigen 3, 17592 Kristianstad	Sit quas voluptatibus.
69	Johansson Johansson AB	Lennart	Andersson	aronssoncharlotta@yahoo.de	+46 (0)8 159 118 78	Skolstigen 88, 79140 Eskilstuna	Fugiat sunt eos ducimus impedit dicta.
70	Johansson Andersson AB	Helena	Lundberg	anderssonlena@engstrom.com	+46 (0)8 813 090 60	Skolgatan 12, 11225 Uddevalla	Ipsam libero impedit nam inventore optio quidem.
71	B├╢rjesson Lindell HB	Markus	Nordin	lindgrenylva@googlemail.com	0861-202 63	Parkv├ñgen 45, 14152 Kristianstad	Distinctio itaque ratione accusantium.
72	Karlsson Olsson HB	Maj	Jonsson	mariabengtsson@live.se	+46 (0)8 884 225 12	Backv├ñgen 564, 61983 Landskrona	Vel fuga placeat sequi a.
73	Carlsson Lund HB	Helene	Berglund	arvidpetersson@spray.se	+46 (0)718 527 82	Granv├ñgen 9, 84768 ├ûrnsk├╢ldsvik	Amet optio inventore ea quam sapiente.
75	Karlsson HB	Charlotte	Bj├╢rklund	polsson@lund.se	+46 (0)8 446 542 61	Parkgatan 07, 66201 Motala	Dolores quis dolor ullam.
76	Larsson Eklund AB	Anna	Karlsson	esterakesson@hellstrom.com	08-937 17 13	Industristigen 411, 48853 G├ñvle	Excepturi veniam ipsam nam.
77	Dahl AB	Kristina	Hagstr├╢m	hjohansson@gmail.com	022-14 55 78	├ängsv├ñgen 02, 39365 Norrk├╢ping	At minima officiis.
78	Jensen & Palm HB	Lars	Eriksson	dahlbergstefan@nilsson.net	08-628 148 88	Furugatan 23, 81806 Stockholm	Minus magni ex aliquid labore.
79	Str├╢m & Lundin HB	Jens	Lundstr├╢m	adam92@swipnet.se	+46 (0)36 42 95 94	Skolgr├ñnd 24, 14006 Alings├Ñs	Explicabo assumenda id amet tempora ab.
80	H├╢gberg & Axelsson AB	Ragnar	Olofsson	larssoningrid@andersson.se	0800-40 14 45	Strandv├ñgen 256, 80598 G├╢teborg	Sequi nam vitae a quae.
81	Olsson HB	Ann	Johansson	seriksson@lindberg.se	0198-71 64 05	Backv├ñgen 242, 63981 G├ñvle	Et reiciendis eaque.
82	Gustafsson Johnsson AB	Robert	L├╢fgren	doris78@telia.com	08-86 42 69	├àkerv├ñgen 612, 89271 V├ñster├Ñs	Beatae rem quia at illo quasi tempora necessitatibus.
83	Andersson & Johansson HB	Petra	Andersson	ljohansson@live.se	008-62 97 58	├ängsgr├ñnd 576, 31477 Liding├╢	Itaque esse quia labore.
84	Lindberg AB	Lena	Stenberg	larssonkarin@spray.se	+46 (0)79 37 25 53	Skolv├ñgen 450, 11905 S├╢dert├ñlje	Amet modi voluptas.
85	Persson & Hermansson AB	Daniel	Wahlstr├╢m	holmgrenola@yahoo.de	064-861 48 03	Skolgatan 5, 41534 Kristianstad	Saepe veniam sapiente numquam.
86	Engstr├╢m Lindholm AB	Lennart	B├ñckstr├╢m	kristinanordstrom@gmail.com	08-182 15 16	Aspgatan 90, 28417 Pite├Ñ	Doloribus dolore in recusandae.
87	Johansson & Lindkvist AB	Ingemar	Hedlund	mlarsson@spray.se	051-64 18 44	Parkv├ñgen 275, 67427 Trelleborg	Quibusdam consequuntur voluptas dolorum officia odit.
88	Nordin Fransson AB	Olivia	Andersson	holger92@karlsson.se	+46 (0)8 106 993 19	Nygatan 4, 53973 Eskilstuna	Alias debitis ipsam quos cupiditate quasi.
89	Johansson Larsson HB	Anders	Johansson	eviklund@jonsson.net	08-920 631 48	Fabriksgatan 0, 88894 Motala	Libero nisi optio aperiam.
90	Karlsson Nilsson AB	Gunilla	Larsson	ucarlsson@yahoo.de	0847-50 24 59	Industrigr├ñnd 65, 26102 Uddevalla	Architecto deleniti inventore a magnam sapiente sint.
91	Holmberg Nordstr├╢m AB	Fredrik	Gustafsson	nilssontherese@gmail.com	0929-695 95	Skolgatan 89, 46249 Karlskoga	Labore voluptas error ipsam rerum.
92	H├╢glund Olsson AB	Karolina	Johansson	margareta18@lund.net	08-920 750 94	Strandv├ñgen 85, 51174 Malm├╢	Vero quod minima ea.
93	Persson HB	Magdalena	Johansson	rune59@live.se	0853-72 36 07	Fabriksgatan 47, 14300 Nyk├╢ping	Ullam dolorum porro autem cum accusantium voluptates reprehenderit.
94	Eriksson HB	Ingemar	Karlsson	johan20@larsson.se	+46 (0)833 058 17	Kyrkgatan 9, 97720 Alings├Ñs	Mollitia at recusandae ducimus.
95	Svensson Eriksson AB	Per	Friberg	magnussonmargareta@live.se	08-52 01 56	Strandstigen 867, 43497 Pite├Ñ	Doloremque asperiores rem eius asperiores.
96	Bengtsson HB	Ingeg├ñrd	Gustafsson	olssonirene@andersson.com	0334-217 34	Skolstigen 4, 20428 Trollh├ñttan	Autem dignissimos exercitationem.
97	Eriksson AB	Johan	Carlsson	mattssonelvira@gmail.com	+46 (0)04 13 53 03	Kyrkogr├ñnd 90, 77593 S├╢dert├ñlje	Deserunt doloremque quasi veniam quaerat repudiandae facilis sequi.
98	Axelsson HB	Margareta	Johansson	ulladahlstrom@gmail.com	0712-01 66 25	Gengatan 9, 72363 Pite├Ñ	Cupiditate autem cum quod dolore.
99	Mattsson Hedberg AB	Anne	Larsson	susanna98@lund.se	+46 (0)19 09 49 13	Idrottsgr├ñnd 8, 21536 Stockholm	A inventore ea praesentium.
100	Eklund HB	Arne	Karlsson	rbergstrom@spray.se	+46 (0)43 67 39 89	Skolv├ñgen 1, 41151 G├ñvle	Nesciunt amet error tempora.
\.


--
-- Data for Name: orderitems; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.orderitems (id, orderid, productid, quantity, price, totalprice) FROM stdin;
307	1	61	6	1312.64	7875.84
308	1	64	3	769.56	2308.68
309	1	78	9	1146.96	10322.64
310	1	53	8	516.47	4131.76
311	1	80	2	317.74	635.48
312	2	44	6	173.8	1042.8
313	2	13	9	152.91	1376.19
314	2	56	2	842.08	1684.16
315	2	24	3	512.06	1536.18
316	3	78	5	602.64	3013.2
317	4	2	6	447.5	2685.0
318	4	63	3	1282.97	3848.91
319	4	46	3	922.75	2768.25
320	4	40	2	496.99	993.98
321	4	47	5	1366.23	6831.15
322	5	72	9	426.49	3838.41
323	5	27	6	1255.08	7530.48
324	5	80	1	288.2	288.2
325	6	20	7	1474.17	10319.19
326	6	34	10	639.41	6394.1
327	7	22	9	146.07	1314.63
328	7	62	9	495.43	4458.87
329	8	55	8	1383.09	11064.72
330	9	19	8	432.46	3459.68
331	9	67	1	216.99	216.99
332	10	80	10	605.67	6056.7
333	11	65	3	100.06	300.18
334	12	64	4	743.57	2974.28
335	13	70	9	1195.1	10755.9
336	13	38	5	1316.39	6581.95
337	13	15	2	1168.66	2337.32
338	13	80	10	453.55	4535.5
339	14	15	7	1481.99	10373.93
340	14	70	8	618.13	4945.04
341	14	33	1	526.01	526.01
342	15	63	10	274.91	2749.1
343	16	9	8	1212.78	9702.24
344	16	72	7	1132.91	7930.37
345	16	49	3	886.81	2660.43
346	17	32	7	298.02	2086.14
347	17	70	5	764.91	3824.55
348	17	80	3	230.36	691.08
349	18	15	4	351.97	1407.88
350	18	50	3	685.6	2056.8
351	18	53	6	920.05	5520.3
352	19	70	10	537.49	5374.9
353	19	52	3	753.32	2259.96
354	19	21	10	348.59	3485.9
355	19	64	8	1057.73	8461.84
356	19	15	6	174.71	1048.26
357	20	22	4	760.53	3042.12
358	20	4	9	260.14	2341.26
359	20	62	5	1064.05	5320.25
360	20	2	4	236.56	946.24
361	21	80	8	1363.3	10906.4
362	21	43	9	1231.04	11079.36
363	21	10	2	773.85	1547.7
364	21	61	7	179.43	1256.01
365	22	11	3	1299.71	3899.13
366	22	68	1	176.05	176.05
367	22	41	7	411.33	2879.31
368	23	79	10	603.77	6037.7
369	24	77	9	816.7	7350.3
370	24	63	4	427.82	1711.28
371	24	68	8	264.64	2117.12
372	24	51	1	593.7	593.7
373	24	19	4	1437.8	5751.2
374	25	50	10	757.86	7578.6
375	25	72	3	486.84	1460.52
376	25	67	2	228.14	456.28
377	25	74	1	84.28	84.28
378	25	7	10	806.95	8069.5
379	25	70	2	512.66	1025.32
380	25	14	5	697.59	3487.95
381	25	3	1	449.31	449.31
382	25	80	6	1440.94	8645.64
383	25	80	2	1310.45	2620.9
384	25	60	1	396.18	396.18
385	25	69	6	504.5	3027.0
386	25	60	8	142.74	1141.92
387	25	44	4	1011.4	4045.6
388	26	6	8	561.75	4494.0
389	27	38	8	1354.51	10836.08
390	27	60	6	1105.82	6634.92
391	27	76	10	865.95	8659.5
392	28	80	10	478.36	4783.6
393	29	45	3	594.72	1784.16
394	29	70	6	1010.69	6064.14
395	29	8	8	772.49	6179.92
396	29	13	10	282.76	2827.6
397	29	30	7	1479.82	10358.74
398	30	60	8	1490.0	11920.0
399	30	12	7	789.33	5525.31
400	30	31	2	1043.01	2086.02
401	30	70	2	1114.56	2229.12
402	31	67	1	661.12	661.12
403	31	40	7	738.64	5170.48
404	31	42	2	115.49	230.98
405	32	24	3	724.37	2173.11
406	32	44	2	1022.78	2045.56
407	32	6	10	107.96	1079.6
408	32	20	9	204.16	1837.44
409	33	56	6	861.49	5168.94
410	33	16	1	1186.16	1186.16
411	33	31	1	1398.56	1398.56
412	33	40	3	823.53	2470.59
413	34	50	9	302.67	2724.03
414	34	20	2	228.36	456.72
415	34	66	5	180.09	900.45
416	35	42	7	303.92	2127.44
417	35	50	4	600.58	2402.32
418	35	62	9	824.25	7418.25
419	36	8	9	385.6	3470.4
420	36	64	8	517.85	4142.8
421	37	17	7	270.34	1892.38
422	37	23	8	623.33	4986.64
423	38	70	9	295.32	2657.88
424	38	73	9	733.85	6604.65
425	39	56	8	1253.61	10028.88
426	39	8	5	1039.71	5198.55
427	40	62	3	988.74	2966.22
428	40	52	9	858.94	7730.46
429	40	48	1	925.85	925.85
433	42	10	7	1365.07	9555.49
434	42	47	5	131.87	659.35
435	42	3	5	1374.68	6873.4
436	42	13	7	721.67	5051.69
437	43	47	7	1294.32	9060.24
438	43	50	10	1214.31	12143.1
439	44	67	9	986.12	8875.08
440	44	45	6	494.73	2968.38
441	45	60	2	736.43	1472.86
442	45	43	1	436.43	436.43
443	45	1	3	296.1	888.3
444	46	30	9	640.28	5762.52
445	46	31	10	800.21	8002.1
446	46	28	9	597.46	5377.14
447	46	63	3	913.13	2739.39
448	47	20	4	558.41	2233.64
449	47	7	9	192.89	1736.01
450	47	68	4	338.32	1353.28
451	48	68	6	1402.21	8413.26
452	48	20	3	1098.08	3294.24
453	49	44	10	274.71	2747.1
454	49	59	10	1015.11	10151.1
455	49	61	6	145.87	875.22
456	50	64	5	774.62	3873.1
457	50	49	9	131.67	1185.03
458	50	79	10	211.57	2115.7
459	50	50	1	829.2	829.2
460	51	70	16	926.27	14820.32
461	51	53	3	1392.03	4176.09
462	51	51	10	823.78	8237.80
463	52	53	20	1392.03	27840.60
464	53	53	20	1392.03	27840.60
465	54	51	10	823.78	8237.80
466	54	53	10	1392.03	13920.30
467	54	70	10	926.27	9262.70
468	54	41	10	410.47	4104.70
469	54	36	10	1249.51	12495.10
\.


--
-- Data for Name: orders; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.orders (id, customerid, deliverydate, totalprice, status, isdeleted, deletedat, deletedby) FROM stdin;
1	77	2025-05-20	1281.34	Shipped	f	\N	\N
2	97	2025-05-06	2173.28	Cancelled	f	\N	\N
3	15	2025-04-27	2698.3	Shipped	f	\N	\N
4	14	2025-05-23	1853.25	Cancelled	f	\N	\N
5	85	2025-04-25	4482.29	Shipped	f	\N	\N
6	99	2025-05-14	1588.6	Delivered	f	\N	\N
7	94	2025-05-23	1406.56	Delivered	f	\N	\N
8	72	2025-04-28	3997.83	Pending	f	\N	\N
9	77	2025-04-25	4730.63	Shipped	f	\N	\N
10	88	2025-05-07	2152.52	Pending	f	\N	\N
11	34	2025-05-17	2752.17	Shipped	f	\N	\N
12	58	2025-05-11	1366.62	Cancelled	f	\N	\N
13	27	2025-04-28	1056.34	Shipped	f	\N	\N
15	94	2025-05-06	892.68	Pending	f	\N	\N
16	92	2025-05-05	3231.57	Shipped	f	\N	\N
17	59	2025-05-03	2382.74	Cancelled	f	\N	\N
18	19	2025-05-03	1808.36	Delivered	f	\N	\N
19	16	2025-04-30	658.81	Shipped	f	\N	\N
20	28	2025-04-28	1349.35	Pending	f	\N	\N
21	91	2025-05-05	1315.84	Cancelled	f	\N	\N
22	18	2025-05-18	1709.44	Shipped	f	\N	\N
23	42	2025-04-30	4294.71	Pending	f	\N	\N
24	58	2025-05-20	2359.75	Delivered	f	\N	\N
26	72	2025-04-28	4059.28	Shipped	f	\N	\N
27	89	2025-05-04	4746.55	Shipped	f	\N	\N
28	69	2025-05-11	1919.37	Shipped	f	\N	\N
29	73	2025-05-14	3062.27	Shipped	f	\N	\N
30	75	2025-05-06	1739.73	Shipped	f	\N	\N
31	21	2025-05-04	3636.93	Delivered	f	\N	\N
32	71	2025-04-30	2249.75	Shipped	f	\N	\N
33	59	2025-05-04	666.12	Pending	f	\N	\N
34	75	2025-05-09	2565.86	Delivered	f	\N	\N
35	65	2025-05-11	3305.9	Pending	f	\N	\N
36	7	2025-05-08	2727.15	Delivered	f	\N	\N
37	4	2025-04-29	1005.18	Pending	f	\N	\N
38	80	2025-05-22	1968.91	Cancelled	f	\N	\N
39	2	2025-05-13	576.62	Pending	f	\N	\N
40	83	2025-05-14	1015.83	Shipped	f	\N	\N
42	36	2025-05-15	3684.85	Delivered	f	\N	\N
43	13	2025-05-16	4526.93	Shipped	f	\N	\N
44	4	2025-05-18	2363.26	Delivered	f	\N	\N
45	53	2025-05-22	3349.2	Delivered	f	\N	\N
46	95	2025-05-16	4919.32	Delivered	f	\N	\N
47	39	2025-05-14	3836.23	Pending	f	\N	\N
48	41	2025-04-30	1996.17	Shipped	f	\N	\N
49	69	2025-05-13	3697.39	Delivered	f	\N	\N
50	11	2025-04-30	4306.1	Delivered	f	\N	\N
25	76	2025-05-24	4220.06	Delivered	f	\N	\N
51	4	2025-04-25	27234.21	Pending	f	\N	\N
53	25	2025-04-26	27840.60	Pending	f	\N	\N
54	4	2025-04-30	48020.60	Pending	t	2025-04-24 20:57:47.588457	test5
52	4	2025-04-25	27840.60	Pending	t	2025-04-24 20:35:45.38141	test
14	92	2025-04-27	1464.75	Delivered	f	\N	\N
\.


--
-- Data for Name: owner; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.owner (id, username, password, role) FROM stdin;
1	admin	240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9	SuperAdmin
\.


--
-- Data for Name: ownersessions; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.ownersessions (sessionid, userid, token, expirydate) FROM stdin;
1	1	khcfQ/2ecDDn4jpgTcinYRE21/sUBtEHbqoO0SLRZjg=	1970-01-01 00:00:00
\.


--
-- Data for Name: products; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.products (id, productname, categoryid, quantity, price, minquantity, supplier, description) FROM stdin;
1	Lim 382	5	175	1047.61	7	Ahlsell	Rostfri.
2	Skiftnyckel 786	7	189	26.08	3	Ahlsell	Bra kvalitet.
3	Bits 799	10	144	408.81	10	Clas Ohlson	L├ñtt och smidig.
4	Hammare 916	2	155	931.08	5	Byggmax	Ekonomipack.
5	Skruvmejsel 330	6	25	1020.91	8	Bauhaus	Ekonomipack.
6	S├ñkring 714	7	119	815.24	2	Bauhaus	Ekonomipack.
7	Skruvmejsel 513	4	95	674.15	10	Jula	L├ñtt och smidig.
8	Tumstock 216	10	132	1270.57	8	Bauhaus	Extra stark.
9	Ankarskena 578	8	74	732.41	4	Clas Ohlson	Bra kvalitet.
10	Hammare 126	9	183	1428.72	10	Bauhaus	Kompatibel med alla modeller.
11	Skruvmejsel 645	8	56	212.09	2	Bauhaus	Extra stark.
12	Tumstock 257	6	181	1145.98	6	Clas Ohlson	Kompatibel med alla modeller.
13	Ankarskena 642	5	38	853.98	6	Byggmax	Rostfri.
14	S├ñkring 865	2	184	1444.61	10	Jula	L├ñtt och smidig.
15	Skruv 654	1	116	39.98	9	Byggmax	Ergonomiskt handtag.
16	Bits 882	6	27	1194.58	8	Ahlsell	Kompatibel med alla modeller.
17	S├Ñg 127	6	139	249.54	10	Bauhaus	L├ñtt och smidig.
18	Hammare 832	10	178	1247.29	7	Byggmax	L├Ñng livsl├ñngd.
19	Lim 863	5	7	380.63	2	Bauhaus	L├Ñng livsl├ñngd.
20	Skruv 681	3	20	1234.45	8	Bauhaus	Standardverktyg.
21	Sp├ñnnrem 378	2	164	691.3	2	Byggmax	Ekonomipack.
22	S├ñkring 373	3	62	397.74	5	Byggmax	Ergonomiskt handtag.
23	Skruvmejsel 615	4	193	1247.88	2	Byggmax	Rostfri.
24	Skruvmejsel 985	7	118	110.28	8	Ahlsell	F├╢r professionellt bruk.
25	Tumstock 774	2	190	794.09	3	Bauhaus	Extra stark.
26	Skruv 770	2	32	1122.21	6	Ahlsell	F├╢r professionellt bruk.
27	Tumstock 134	3	81	1249.82	9	Byggmax	L├Ñng livsl├ñngd.
28	Mutter 105	8	150	108.63	4	Byggmax	Bra kvalitet.
29	Hammare 462	7	26	634.47	10	Byggmax	Extra stark.
30	Bult 750	9	155	690.31	9	Clas Ohlson	Bra kvalitet.
31	Bits 220	5	131	1112.37	4	Byggmax	Ekonomipack.
32	Hammare 463	3	150	1384.54	6	Byggmax	Ergonomiskt handtag.
33	Kabel 826	9	167	441.44	7	Ahlsell	Ekonomipack.
34	Skruvdragare 985	6	20	917.21	9	Byggmax	Ekonomipack.
35	Skruv 713	5	18	1309.21	6	Bauhaus	L├ñtt och smidig.
37	Bits 783	5	55	107.68	4	Jula	Kompatibel med alla modeller.
38	Tumstock 999	7	29	705.46	10	Ahlsell	L├ñtt och smidig.
39	Sandpapper 225	4	177	616.49	1	Clas Ohlson	Kompatibel med alla modeller.
40	Tumstock 243	5	188	49.84	1	Bauhaus	Extra stark.
42	Bult 650	5	162	1332.59	5	Byggmax	Extra stark.
43	Lim 231	8	113	1091.83	4	Ahlsell	Bra kvalitet.
44	S├Ñg 362	7	110	80.49	1	Jula	F├╢r professionellt bruk.
45	Skruv 764	10	44	803.98	1	Jula	Ekonomipack.
46	Spik 248	2	51	478.31	1	Byggmax	Rostfri.
47	S├Ñg 900	8	89	234.06	1	Ahlsell	L├ñtt och smidig.
48	Skiftnyckel 147	6	194	818.1	1	Clas Ohlson	Bra kvalitet.
49	Skruv 415	3	138	631.38	7	Clas Ohlson	Ekonomipack.
50	Tumstock 938	3	187	336.37	8	Byggmax	Ergonomiskt handtag.
52	Skiftnyckel 970	5	115	1472.45	8	Jula	Ergonomiskt handtag.
54	S├Ñg 337	2	39	1361.47	7	Ahlsell	Rostfri.
55	Bits 990	6	179	1164.27	6	Jula	L├Ñng livsl├ñngd.
56	Spik 164	5	33	305.99	10	Byggmax	Bra kvalitet.
57	S├Ñg 475	10	53	654.01	8	Ahlsell	Kompatibel med alla modeller.
58	Skruv 508	3	153	1492.02	8	Byggmax	Standardverktyg.
59	Bult 782	3	161	476.46	5	Byggmax	Standardverktyg.
60	Tumstock 568	7	123	1185.44	10	Ahlsell	Kompatibel med alla modeller.
61	Ankarskena 353	5	123	309.85	3	Byggmax	Standardverktyg.
62	Skiftnyckel 583	3	169	787.05	10	Byggmax	Kompatibel med alla modeller.
63	Ankarskena 175	8	125	469.25	9	Jula	Extra stark.
64	Hammare 643	8	59	754.51	7	Bauhaus	Rostfri.
65	Batteri 864	7	34	485.82	5	Clas Ohlson	Rostfri.
66	Skruv 420	10	138	69.15	9	Byggmax	Standardverktyg.
67	Sp├ñnnrem 603	6	23	1130.52	10	Byggmax	Ergonomiskt handtag.
68	S├ñkring 504	5	86	99.78	1	Byggmax	F├╢r professionellt bruk.
69	Skiftnyckel 430	4	82	257.57	2	Jula	Ergonomiskt handtag.
71	Kabel 572	4	43	849.79	2	Clas Ohlson	L├Ñng livsl├ñngd.
72	Sandpapper 516	4	45	135.76	8	Jula	Kompatibel med alla modeller.
73	Sandpapper 651	4	156	1386.97	6	Ahlsell	Extra stark.
74	Tumstock 457	8	62	478.4	2	Byggmax	L├ñtt och smidig.
75	Skruvmejsel 454	6	189	1022.72	2	Clas Ohlson	Bra kvalitet.
76	Ankarskena 326	7	188	446.15	10	Ahlsell	L├ñtt och smidig.
77	Borrmaskin 771	10	165	555.85	1	Bauhaus	Bra kvalitet.
78	Kabel 353	7	36	36.3	6	Bauhaus	F├╢r professionellt bruk.
79	Borrmaskin 249	8	45	89.56	1	Clas Ohlson	F├╢r professionellt bruk.
80	Tumstock 836	2	14	1005.84	10	Clas Ohlson	Standardverktyg.
51	Lim 926	1	120	823.78	3	Byggmax	F├╢r professionellt bruk.
53	Mutter 283	1	154	1392.03	4	Bauhaus	Kompatibel med alla modeller.
70	Tumstock 294	1	180	926.27	5	Ahlsell	Rostfri.
41	Kabel 577	1	36	410.47	1	Ahlsell	L├Ñng livsl├ñngd.
36	Skiftnyckel 956	1	152	1249.51	9	Byggmax	F├╢r professionellt bruk.
\.


--
-- Data for Name: reportdetails; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.reportdetails (id, reportid, orderid, customername, deliverydate, totalprice, status) FROM stdin;
1	1	37	Ingemar Johansson	2025-04-29	1005.18	Pending
2	1	50	Annika Ericsson	2025-04-30	4306.1	Delivered
3	1	3	G├╢ran Andersson	2025-04-27	2698.3	Shipped
4	1	19	Birgitta Johansson	2025-04-30	658.81	Shipped
5	1	13	Maud Larsson	2025-04-28	1056.34	Shipped
6	1	20	Anna Andersson	2025-04-28	1349.35	Pending
7	1	48	Lennart Lundgren	2025-04-30	1996.17	Shipped
8	1	23	Paul Johansson	2025-04-30	4294.71	Pending
9	1	32	Markus Nordin	2025-04-30	2249.75	Shipped
10	1	26	Maj Jonsson	2025-04-28	4059.28	Shipped
11	1	8	Maj Jonsson	2025-04-28	3997.83	Pending
12	1	9	Kristina Hagstr├╢m	2025-04-25	4730.63	Shipped
13	1	5	Daniel Wahlstr├╢m	2025-04-25	4482.29	Shipped
14	1	14	Karolina Johansson	2025-04-27	1464.75	Delivered
15	2	37	Ingemar Johansson	2025-04-29	1005.18	Pending
16	2	50	Annika Ericsson	2025-04-30	4306.1	Delivered
17	2	3	G├╢ran Andersson	2025-04-27	2698.3	Shipped
18	2	19	Birgitta Johansson	2025-04-30	658.81	Shipped
19	2	13	Maud Larsson	2025-04-28	1056.34	Shipped
20	2	20	Anna Andersson	2025-04-28	1349.35	Pending
21	2	48	Lennart Lundgren	2025-04-30	1996.17	Shipped
22	2	23	Paul Johansson	2025-04-30	4294.71	Pending
23	2	32	Markus Nordin	2025-04-30	2249.75	Shipped
24	2	26	Maj Jonsson	2025-04-28	4059.28	Shipped
25	2	8	Maj Jonsson	2025-04-28	3997.83	Pending
26	2	9	Kristina Hagstr├╢m	2025-04-25	4730.63	Shipped
27	2	5	Daniel Wahlstr├╢m	2025-04-25	4482.29	Shipped
28	2	14	Karolina Johansson	2025-04-27	1464.75	Delivered
29	3	1	Kristina Hagstr├╢m	2025-05-20	1281.34	Shipped
30	3	2	Johan Carlsson	2025-05-06	2173.28	Cancelled
31	3	3	G├╢ran Andersson	2025-04-27	2698.3	Shipped
32	3	4	├àke Nilsson	2025-05-23	1853.25	Cancelled
33	3	5	Daniel Wahlstr├╢m	2025-04-25	4482.29	Shipped
34	3	6	Anne Larsson	2025-05-14	1588.6	Delivered
35	3	7	Ingemar Karlsson	2025-05-23	1406.56	Delivered
36	3	8	Maj Jonsson	2025-04-28	3997.83	Pending
37	3	9	Kristina Hagstr├╢m	2025-04-25	4730.63	Shipped
38	3	10	Olivia Andersson	2025-05-07	2152.52	Pending
39	3	11	Malin Johansson	2025-05-17	2752.17	Shipped
40	3	12	Anna Pettersson	2025-05-11	1366.62	Cancelled
41	3	13	Maud Larsson	2025-04-28	1056.34	Shipped
42	3	14	Karolina Johansson	2025-04-27	1464.75	Delivered
43	3	15	Ingemar Karlsson	2025-05-06	892.68	Pending
44	3	16	Karolina Johansson	2025-05-05	3231.57	Shipped
45	3	17	Karin Karlsson	2025-05-03	2382.74	Cancelled
46	3	18	Annika H├Ñkansson	2025-05-03	1808.36	Delivered
47	3	19	Birgitta Johansson	2025-04-30	658.81	Shipped
48	3	20	Anna Andersson	2025-04-28	1349.35	Pending
49	3	21	Fredrik Gustafsson	2025-05-05	1315.84	Cancelled
50	3	22	Nils Lundin	2025-05-18	1709.44	Shipped
51	3	23	Paul Johansson	2025-04-30	4294.71	Pending
52	3	24	Anna Pettersson	2025-05-20	2359.75	Delivered
53	3	26	Maj Jonsson	2025-04-28	4059.28	Shipped
54	3	27	Anders Johansson	2025-05-04	4746.55	Shipped
55	3	28	Lennart Andersson	2025-05-11	1919.37	Shipped
56	3	29	Helene Berglund	2025-05-14	3062.27	Shipped
57	3	30	Charlotte Bj├╢rklund	2025-05-06	1739.73	Shipped
58	3	31	Roger M├Ñrtensson	2025-05-04	3636.93	Delivered
59	3	32	Markus Nordin	2025-04-30	2249.75	Shipped
60	3	33	Karin Karlsson	2025-05-04	666.12	Pending
61	3	34	Charlotte Bj├╢rklund	2025-05-09	2565.86	Delivered
62	3	35	Sandra Johansson	2025-05-11	3305.9	Pending
63	3	36	Fredrik Andersson	2025-05-08	2727.15	Delivered
64	3	37	Ingemar Johansson	2025-04-29	1005.18	Pending
65	3	38	Ragnar Olofsson	2025-05-22	1968.91	Cancelled
66	3	39	Ingeg├ñrd Ekberg	2025-05-13	576.62	Pending
67	3	40	Petra Andersson	2025-05-14	1015.83	Shipped
68	3	41	Pierre Karlsson	2025-05-24	880.6	Cancelled
69	3	42	Thomas Nilsson	2025-05-15	3684.85	Delivered
70	3	43	Eva Sv├ñrd	2025-05-16	4526.93	Shipped
71	3	44	Ingemar Johansson	2025-05-18	2363.26	Delivered
72	3	45	Inger Ivarsson	2025-05-22	3349.2	Delivered
73	3	46	Per Friberg	2025-05-16	4919.32	Delivered
74	3	47	Inger Gustafsson	2025-05-14	3836.23	Pending
75	3	48	Lennart Lundgren	2025-04-30	1996.17	Shipped
76	3	49	Lennart Andersson	2025-05-13	3697.39	Delivered
77	3	50	Annika Ericsson	2025-04-30	4306.1	Delivered
78	3	25	Anna Karlsson	2025-05-24	4220.06	Delivered
\.


--
-- Data for Name: reports; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.reports (id, reporttitle, reporttype, details, startdate, enddate, status, date) FROM stdin;
1	tset	Sales Report	14 orders included.	2025-04-01	2025-04-30	All	2025-04-24 11:44:19.626392
2	tset2	Sales Report	14 orders included.	2022-01-01	2025-04-30	All	2025-04-24 11:44:42.622629
3	tset3	Sales Report	50 orders included.	2022-01-01	2025-12-30	All	2025-04-24 11:45:04.513611
\.


--
-- Data for Name: schedule; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.schedule (id, employeeid, dayname, starttime, endtime, reststarttime, restendtime, totalhours, isrestday) FROM stdin;
1	2	Monday	08:00:00	17:00:00	12:00:00	13:15:00	7.75	f
2	2	Tuesday	08:00:00	17:00:00	12:00:00	13:00:00	8	f
3	2	Wednesday	08:00:00	17:00:00	12:00:00	13:00:00	8	f
4	2	Thursday	08:00:00	17:00:00	12:00:00	13:00:00	8	f
5	2	Friday	08:00:00	17:00:00	12:00:00	13:00:00	8	f
6	2	Saturday	00:00:00	00:00:00	\N	\N	0	t
7	2	Sunday	00:00:00	00:00:00	\N	\N	0	t
8	6	Monday	08:00:00	17:00:00	12:00:00	13:15:00	7.75	f
9	6	Tuesday	08:00:00	17:00:00	12:00:00	13:00:00	8	f
10	6	Wednesday	08:00:00	17:00:00	12:00:00	13:00:00	8	f
11	6	Thursday	08:00:00	17:00:00	12:00:00	13:00:00	8	f
12	6	Friday	08:00:00	17:00:00	12:00:00	13:00:00	8	f
13	6	Saturday	00:00:00	00:00:00	\N	\N	0	t
14	6	Sunday	00:00:00	00:00:00	\N	\N	0	t
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.users (id, firstname, lastname, email, phonenumber, username, password, address, role) FROM stdin;
1	test	test	test	test	test	9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08	test	Admin
2	test2	test2	test2	test2	test2	60303ae22b998861bce3b28f33eec1be758a213c86c93c076dbe9f558c11c752	test2	SellingStaff
3	test3	test3	test3	test3	test3	fd61a03af4f77d870fc21e05e7e80678095c92d808cfb3b5c279ee04c74aca13	test3	Admin
4	test4	test4	test4	test4	test4	a4e624d686e03ed2767c0abd85c14426b0b1157d2ce81d27bb4fe4f6f01d688a	test4	SellingStaff
5	test5	test5	test5	test5	test5	a140c0c1eda2def2b830363ba362aa4d7d255c262960544821f556e16661b6ff	test5	StockStaff
6	test6	test6	test6	test6	test6	ed0cb90bdfa4f93981a7d03cff99213a86aa96a6cbcf89ec5e8889871f088727	test6	StockStaff
\.


--
-- Data for Name: usersessions; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.usersessions (sessionid, userid, token, expirydate) FROM stdin;
3	2	1JBef+qJQTQ3AUFYM3HMxsBb0qgWQ6UZVZAXlKm770E=	1970-01-01 00:00:00
16	3	q0waXqMcI55QX4smXMJww3vov/UWqF9F3RlreTF5E9o=	1970-01-01 00:00:00
17	5	+NTSXWjga/U59dcSYXuB2D5fZ0nQvNTgVjodKkW6sWI=	1970-01-01 00:00:00
1	1	qkz7aqMSAXtHxdFN1sa8XI5vKh4f1qmFn89Vy/iQiyI=	1970-01-01 00:00:00
\.


--
-- Name: categories_id_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public.categories_id_seq', 104, true);


--
-- Name: customers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public.customers_id_seq', 101, true);


--
-- Name: orderitems_id_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public.orderitems_id_seq', 469, true);


--
-- Name: orders_id_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public.orders_id_seq', 54, true);


--
-- Name: owner_id_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public.owner_id_seq', 1, true);


--
-- Name: ownersessions_sessionid_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public.ownersessions_sessionid_seq', 14, true);


--
-- Name: products_id_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public.products_id_seq', 82, true);


--
-- Name: reportdetails_id_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public.reportdetails_id_seq', 78, true);


--
-- Name: reports_id_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public.reports_id_seq', 4, true);


--
-- Name: schedule_id_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public.schedule_id_seq', 14, true);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public.users_id_seq', 8, true);


--
-- Name: usersessions_sessionid_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public.usersessions_sessionid_seq', 21, true);


--
-- Name: categories categories_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_pkey PRIMARY KEY (id);


--
-- Name: customers customers_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT customers_pkey PRIMARY KEY (id);


--
-- Name: orderitems orderitems_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.orderitems
    ADD CONSTRAINT orderitems_pkey PRIMARY KEY (id);


--
-- Name: orders orders_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_pkey PRIMARY KEY (id);


--
-- Name: owner owner_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.owner
    ADD CONSTRAINT owner_pkey PRIMARY KEY (id);


--
-- Name: owner owner_username_key; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.owner
    ADD CONSTRAINT owner_username_key UNIQUE (username);


--
-- Name: ownersessions ownersessions_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.ownersessions
    ADD CONSTRAINT ownersessions_pkey PRIMARY KEY (sessionid);


--
-- Name: ownersessions ownersessions_token_key; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.ownersessions
    ADD CONSTRAINT ownersessions_token_key UNIQUE (token);


--
-- Name: ownersessions ownersessions_userid_key; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.ownersessions
    ADD CONSTRAINT ownersessions_userid_key UNIQUE (userid);


--
-- Name: products products_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_pkey PRIMARY KEY (id);


--
-- Name: reportdetails reportdetails_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.reportdetails
    ADD CONSTRAINT reportdetails_pkey PRIMARY KEY (id);


--
-- Name: reports reports_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.reports
    ADD CONSTRAINT reports_pkey PRIMARY KEY (id);


--
-- Name: schedule schedule_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.schedule
    ADD CONSTRAINT schedule_pkey PRIMARY KEY (id);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: users users_username_key; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);


--
-- Name: usersessions usersessions_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.usersessions
    ADD CONSTRAINT usersessions_pkey PRIMARY KEY (sessionid);


--
-- Name: usersessions usersessions_token_key; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.usersessions
    ADD CONSTRAINT usersessions_token_key UNIQUE (token);


--
-- Name: usersessions usersessions_userid_key; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.usersessions
    ADD CONSTRAINT usersessions_userid_key UNIQUE (userid);


--
-- Name: orderitems orderitems_orderid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.orderitems
    ADD CONSTRAINT orderitems_orderid_fkey FOREIGN KEY (orderid) REFERENCES public.orders(id) ON DELETE CASCADE;


--
-- Name: orderitems orderitems_productid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.orderitems
    ADD CONSTRAINT orderitems_productid_fkey FOREIGN KEY (productid) REFERENCES public.products(id) ON DELETE CASCADE;


--
-- Name: orders orders_customerid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_customerid_fkey FOREIGN KEY (customerid) REFERENCES public.customers(id) ON DELETE CASCADE;


--
-- Name: ownersessions ownersessions_userid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.ownersessions
    ADD CONSTRAINT ownersessions_userid_fkey FOREIGN KEY (userid) REFERENCES public.owner(id) ON DELETE CASCADE;


--
-- Name: products products_categoryid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_categoryid_fkey FOREIGN KEY (categoryid) REFERENCES public.categories(id);


--
-- Name: reportdetails reportdetails_reportid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.reportdetails
    ADD CONSTRAINT reportdetails_reportid_fkey FOREIGN KEY (reportid) REFERENCES public.reports(id) ON DELETE CASCADE;


--
-- Name: schedule schedule_employeeid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.schedule
    ADD CONSTRAINT schedule_employeeid_fkey FOREIGN KEY (employeeid) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: usersessions usersessions_userid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.usersessions
    ADD CONSTRAINT usersessions_userid_fkey FOREIGN KEY (userid) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

