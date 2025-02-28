--
-- PostgreSQL database dump
--

-- Dumped from database version 14.15 (Debian 14.15-1.pgdg120+1)
-- Dumped by pg_dump version 16.1

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: public; Type: SCHEMA; Schema: -; Owner: postgres
--

-- *not* creating schema, since initdb creates it


ALTER SCHEMA public OWNER TO postgres;

--
-- Name: uuid-ossp; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA public;


--
-- Name: EXTENSION "uuid-ossp"; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION "uuid-ossp" IS 'generate universally unique identifiers (UUIDs)';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: attachments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.attachments (
    id character varying(40) NOT NULL,
    table_name character varying(40),
    file_id character varying(40),
    file_type character varying(40),
    file_path text,
    created_at timestamp without time zone,
    updated_at timestamp without time zone,
    file_name text
);


ALTER TABLE public.attachments OWNER TO postgres;

--
-- Name: classifications; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.classifications (
    id character varying(40) NOT NULL,
    name character varying(50) NOT NULL,
    description text,
    table_name character varying(50),
    field_name character varying(50),
    created_at timestamp without time zone,
    updated_at timestamp without time zone
);


ALTER TABLE public.classifications OWNER TO postgres;

--
-- Name: customers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.customers (
    id character varying(40) NOT NULL,
    name character varying(50) NOT NULL,
    phone_number character varying(20),
    email character varying(20),
    member character varying(50),
    created_at timestamp without time zone,
    updated_at timestamp without time zone
);


ALTER TABLE public.customers OWNER TO postgres;

--
-- Name: discounts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.discounts (
    id character varying(40) NOT NULL,
    discount_value integer NOT NULL,
    start_at timestamp without time zone,
    expired_at timestamp without time zone,
    product_id character varying(40),
    created_at timestamp without time zone,
    updated_at timestamp without time zone
);


ALTER TABLE public.discounts OWNER TO postgres;

--
-- Name: notifications; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.notifications (
    id character varying(40) NOT NULL,
    title character varying(50) NOT NULL,
    description text,
    has_read boolean DEFAULT false,
    table_name character varying(40),
    path_id character varying(40),
    user_id character varying(40),
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone DEFAULT now(),
    created_by character varying(40)
);


ALTER TABLE public.notifications OWNER TO postgres;

--
-- Name: product_transactions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.product_transactions (
    product_id character varying(40) NOT NULL,
    transaction_id character varying(40) NOT NULL,
    quantity integer NOT NULL,
    price double precision NOT NULL,
    discount double precision
);


ALTER TABLE public.product_transactions OWNER TO postgres;

--
-- Name: products; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.products (
    id character varying(40) NOT NULL,
    name character varying(50) NOT NULL,
    category character varying(50),
    price double precision,
    stock integer,
    created_at timestamp without time zone,
    updated_at timestamp without time zone,
    main_image text
);


ALTER TABLE public.products OWNER TO postgres;

--
-- Name: transactions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.transactions (
    id character varying(40) NOT NULL,
    total_amount double precision NOT NULL,
    customer_id character varying(40),
    user_id character varying(40),
    created_at timestamp without time zone,
    updated_at timestamp without time zone
);


ALTER TABLE public.transactions OWNER TO postgres;

--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    id character varying(40) NOT NULL,
    name character varying(50) NOT NULL,
    username character varying(50) NOT NULL,
    password character varying(225) NOT NULL,
    role character varying(50),
    created_at timestamp without time zone,
    updated_at timestamp without time zone
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Data for Name: attachments; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.attachments (id, table_name, file_id, file_type, file_path, created_at, updated_at, file_name) FROM stdin;
\.


--
-- Data for Name: classifications; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.classifications (id, name, description, table_name, field_name, created_at, updated_at) FROM stdin;
31f1c4d6-6f24-468b-89fe-b01f0c2550d3	Beverages	Drinks, including soft drinks, juices, and alcoholic beverages	products	category	2025-01-09 05:59:58.735185	2025-01-09 05:59:58.735185
46d11070-e2b9-404d-8533-4529a0871d1d	Fresh Produce	Fruits, vegetables, and other perishable goods	products	category	2025-01-09 05:59:58.735185	2025-01-09 05:59:58.735185
a491b1a5-5ec3-4b6e-abee-7537298cc4e6	Dairy	Milk, cheese, yogurt, and other dairy products	products	category	2025-01-09 05:59:58.735185	2025-01-09 05:59:58.735185
cc1824cd-9ba8-4a81-ad2a-212edbb3dd3c	Frozen Foods	Items like frozen vegetables, meats, and ready-to-eat meals	products	category	2025-01-09 05:59:58.735185	2025-01-09 05:59:58.735185
5c28b23d-0382-4906-934f-d76ab8378f3e	Personal Care	Health and beauty products like shampoo, soap, and cosmetics	products	category	2025-01-09 05:59:58.735185	2025-01-09 05:59:58.735185
ee198fd1-52e4-4864-a282-8c38cdb36df9	Cleaning Supplies	Household cleaning products such as detergents, wipes, and disinfectants	products	category	2025-01-09 05:59:58.735185	2025-01-09 05:59:58.735185
8c33508b-0451-461f-9015-660af900f280	Snacks and Sweets	Chips, candies, cookies, and other snack foods	products	category	2025-01-09 05:59:58.735185	2025-01-09 05:59:58.735185
c7178dbb-ba23-42e0-b7f5-01cf693ca244	Household Goods	Items such as paper towels, batteries, and kitchenware	products	category	2025-01-09 05:59:58.735185	2025-01-09 05:59:58.735185
79a419fb-ce7a-4abe-bd38-90b05772eee2	Baby Products	Diapers, baby food, and related items	products	category	2025-01-09 05:59:58.735185	2025-01-09 05:59:58.735185
acc5edf9-661e-4625-8683-7cf3b2df7f17	Pet Products	Pet food and care items	products	category	2025-01-09 05:59:58.735185	2025-01-09 05:59:58.735185
c619b393-ced6-4fee-99be-bfb83a626321	Tobacco	Cigarettes and related products	products	category	2025-01-09 05:59:58.735185	2025-01-09 05:59:58.735185
2bb50b77-c7c2-4a5f-981b-be2e0dfa36da	Food	All types of edible items, including packaged foods, snacks, and beverages ok	products	category	2025-01-09 05:59:58.735185	2025-01-14 07:00:08.360164
\.


--
-- Data for Name: customers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.customers (id, name, phone_number, email, member, created_at, updated_at) FROM stdin;
d319f321-8530-4ad6-b65f-47cf54ae3100	Yani	098008	asas	Staff	2025-01-14 08:27:12.388223	2025-01-14 08:27:33.693887
7728af59-ae34-48a4-a199-41eb0cf14243	Sutejinag	089212329856	Sutejing		2025-02-04 03:09:42.289985	2025-02-04 03:09:42.289988
adf66616-f6b2-4d64-a242-c15b92d4e160	Bidu	089212329856	Sutejing		2025-02-06 17:00:06.221796	2025-02-06 17:00:06.221798
\.


--
-- Data for Name: discounts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.discounts (id, discount_value, start_at, expired_at, product_id, created_at, updated_at) FROM stdin;
5f16a15c-f2c9-478d-88ec-35113544f0b0	20	2025-02-22 13:18:00	2025-04-22 13:18:00	1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1l	2025-02-22 20:18:37.07525	2025-02-22 20:28:05.359867
68df1790-167b-425d-b7c0-c0bab124126f	20	2025-06-04 17:00:00	2025-07-04 17:00:00	1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1v	2025-02-07 02:54:04.550439	2025-02-23 03:18:58.53294
7b33546c-5bd7-4841-ad89-5ffaf689d7e8	44	2025-02-23 03:29:00	2025-02-28 03:29:00	1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1p	2025-02-23 03:29:35.396139	2025-02-23 03:29:35.396139
\.


--
-- Data for Name: notifications; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.notifications (id, title, description, has_read, table_name, path_id, user_id, created_at, updated_at, created_by) FROM stdin;
4d075f77-763b-4442-a206-f4903b635857	Transactions	You have new transaction	f	transactions	fa0fcc85-ce21-49df-8926-9c54f36c63bd	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-26 02:54:01.608234	2025-02-26 02:54:01.608238	07191ec0-1724-4df8-9189-e3be0e27610a
617c03be-1650-4f1f-9fff-4568043a13b5	Transactions	You have new transaction	f	transactions	fa0fcc85-ce21-49df-8926-9c54f36c63bd	8b5af87d-a9dd-4d8d-863b-e2e6b024b701	2025-02-26 02:54:01.627257	2025-02-26 02:54:01.62726	07191ec0-1724-4df8-9189-e3be0e27610a
023e8407-c854-4553-af94-a86d50a38056	Transactions	You have new transaction	f	transactions	b3d6c2d4-3701-4f63-8b4e-2703baadcd30	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-26 15:32:45.355035	2025-02-26 15:32:45.355041	07191ec0-1724-4df8-9189-e3be0e27610a
9b34d6e0-8dca-4a42-8786-f3e5fe03f171	Transactions	You have new transaction	f	transactions	b3d6c2d4-3701-4f63-8b4e-2703baadcd30	8b5af87d-a9dd-4d8d-863b-e2e6b024b701	2025-02-26 15:32:45.363997	2025-02-26 15:32:45.363999	07191ec0-1724-4df8-9189-e3be0e27610a
d8e6bf0d-9742-4e33-8053-bda90171412b	Transactions	You have new transaction	f	transactions	0cf05b98-e220-4601-8d99-9ebf944f8b87	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-26 15:32:58.352074	2025-02-26 15:32:58.352185	07191ec0-1724-4df8-9189-e3be0e27610a
affe6872-a949-4c9e-9b11-ec34ad09c23b	Transactions	You have new transaction	f	transactions	0cf05b98-e220-4601-8d99-9ebf944f8b87	8b5af87d-a9dd-4d8d-863b-e2e6b024b701	2025-02-26 15:32:58.379737	2025-02-26 15:32:58.379739	07191ec0-1724-4df8-9189-e3be0e27610a
2dc6aabd-755f-4528-972e-7310c6eccd58	Transactions	You have new transaction	f	transactions	c72baad0-5e46-4d17-b295-7295851cc0a9	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-26 15:33:16.572781	2025-02-26 15:33:16.572783	07191ec0-1724-4df8-9189-e3be0e27610a
a57b0b16-2cdd-4b6c-83e6-89683aeebc2f	Transactions	You have new transaction	f	transactions	c72baad0-5e46-4d17-b295-7295851cc0a9	8b5af87d-a9dd-4d8d-863b-e2e6b024b701	2025-02-26 15:33:16.581743	2025-02-26 15:33:16.581746	07191ec0-1724-4df8-9189-e3be0e27610a
4ca72417-491a-47e5-ac83-fd2fd173fee3	Transactions	You have new transaction	f	transactions	cfbce5af-487c-46cc-808b-14486eb77035	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-27 08:57:39.075531	2025-02-27 08:57:39.075576	07191ec0-1724-4df8-9189-e3be0e27610a
2a660b35-a702-4d89-895a-5c8df1f133bb	Transactions	You have new transaction	f	transactions	cfbce5af-487c-46cc-808b-14486eb77035	8b5af87d-a9dd-4d8d-863b-e2e6b024b701	2025-02-27 08:57:39.083805	2025-02-27 08:57:39.083808	07191ec0-1724-4df8-9189-e3be0e27610a
211d56f6-c68f-4b18-85ca-204ca8418684	Transactions	You have new transaction	f	transactions	f73ff1a9-f6aa-4ba4-bf4e-aef22149e8f5	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-27 08:57:49.210408	2025-02-27 08:57:49.21041	07191ec0-1724-4df8-9189-e3be0e27610a
19eb9200-9423-4594-8d68-7ccd80e6dac7	Transactions	You have new transaction	f	transactions	f73ff1a9-f6aa-4ba4-bf4e-aef22149e8f5	8b5af87d-a9dd-4d8d-863b-e2e6b024b701	2025-02-27 08:57:49.228552	2025-02-27 08:57:49.228554	07191ec0-1724-4df8-9189-e3be0e27610a
e85fdb4e-6774-4942-bf81-7bed520e1779	Transactions	You have new transaction	f	transactions	413c36cc-e823-4cd4-afa5-34e98edf9acf	8b5af87d-a9dd-4d8d-863b-e2e6b024b701	2025-02-27 08:57:57.002888	2025-02-27 08:57:57.00289	07191ec0-1724-4df8-9189-e3be0e27610a
f3c1b29c-8081-4e5e-bcfb-8f454da07316	Transactions	10 You have new transaction	f	transactions	49952797-6911-4f5f-8642-cbedd56d5dee	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-27 08:58:05.705421	2025-02-27 08:58:05.705422	07191ec0-1724-4df8-9189-e3be0e27610a
848ddbd6-c551-4c50-846a-87770fdb3887	Transactions	9 You have new transaction	f	transactions	49952797-6911-4f5f-8642-cbedd56d5dee	8b5af87d-a9dd-4d8d-863b-e2e6b024b701	2025-02-27 08:58:05.713915	2025-02-27 08:58:05.713917	07191ec0-1724-4df8-9189-e3be0e27610a
36d35c6d-d2ba-4f0a-a422-fa4b20ba4a60	Transactions	7 You have new transaction	f	transactions	0bc5fb53-c910-4731-ae64-c30bc747280d	8b5af87d-a9dd-4d8d-863b-e2e6b024b701	2025-02-27 08:58:15.735905	2025-02-27 08:58:15.735907	07191ec0-1724-4df8-9189-e3be0e27610a
2f8e0148-3685-4afb-924d-c7a4308365c9	Transactions	5 You have new transaction	f	transactions	985345ab-c125-469e-bc01-d4cf437258aa	8b5af87d-a9dd-4d8d-863b-e2e6b024b701	2025-02-27 08:58:25.626087	2025-02-27 08:58:25.626089	07191ec0-1724-4df8-9189-e3be0e27610a
3fd727d3-e076-481b-a9b7-b664615c45fd	Transactions	8 You have new transaction	t	transactions	0bc5fb53-c910-4731-ae64-c30bc747280d	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-27 08:58:15.727539	2025-02-27 13:41:41.671347	07191ec0-1724-4df8-9189-e3be0e27610a
c6def6a3-e1de-4312-851b-9c87edb094e7	Transactions	3 You have new transaction	f	transactions	9a4fb74e-8d81-44fe-bb15-3b52600b6dcd	8b5af87d-a9dd-4d8d-863b-e2e6b024b701	2025-02-27 08:58:32.533391	2025-02-27 08:58:32.533393	07191ec0-1724-4df8-9189-e3be0e27610a
97a9caf5-6356-48d5-b2ab-c17da8c69670	Transactions	1 You have new transaction	f	transactions	23965224-e01b-440c-a9c4-5d32a4c85e0a	8b5af87d-a9dd-4d8d-863b-e2e6b024b701	2025-02-27 08:58:40.81408	2025-02-27 08:58:40.814083	07191ec0-1724-4df8-9189-e3be0e27610a
ba5fcb3e-8ed6-41c3-bb44-7bf817024657	Transactions	4 You have new transaction	t	transactions	9a4fb74e-8d81-44fe-bb15-3b52600b6dcd	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-27 08:58:32.521473	2025-02-27 13:20:46.282315	07191ec0-1724-4df8-9189-e3be0e27610a
2e0d398e-3f99-4ac2-aaab-ebe2f855fc9b	Transactions	6 You have new transaction	t	transactions	985345ab-c125-469e-bc01-d4cf437258aa	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-27 08:58:25.617488	2025-02-27 13:41:48.609561	07191ec0-1724-4df8-9189-e3be0e27610a
9d2882bc-0b4b-414c-b785-9c59e01ffa66	Transactions	2 You have new transaction	t	transactions	23965224-e01b-440c-a9c4-5d32a4c85e0a	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-27 08:58:40.792763	2025-02-27 13:58:47.305038	07191ec0-1724-4df8-9189-e3be0e27610a
295138cd-4479-4a25-9d27-d24366ec61ce	Transactions	You have new transaction	t	transactions	413c36cc-e823-4cd4-afa5-34e98edf9acf	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-27 08:57:56.993384	2025-02-27 14:06:28.421331	07191ec0-1724-4df8-9189-e3be0e27610a
\.


--
-- Data for Name: product_transactions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.product_transactions (product_id, transaction_id, quantity, price, discount) FROM stdin;
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1v	6f8ff794-8043-4a0d-85b0-984e4dbe8a36	1	25000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1t	6f8ff794-8043-4a0d-85b0-984e4dbe8a36	1	80000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1t	fa0fcc85-ce21-49df-8926-9c54f36c63bd	1	80000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1l	fa0fcc85-ce21-49df-8926-9c54f36c63bd	1	30000	20
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1n	b3d6c2d4-3701-4f63-8b4e-2703baadcd30	2	15000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1h	0cf05b98-e220-4601-8d99-9ebf944f8b87	1	25000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1c	c72baad0-5e46-4d17-b295-7295851cc0a9	1	15000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1t	cfbce5af-487c-46cc-808b-14486eb77035	1	80000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1s	f73ff1a9-f6aa-4ba4-bf4e-aef22149e8f5	1	50000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1n	413c36cc-e823-4cd4-afa5-34e98edf9acf	1	15000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1i	49952797-6911-4f5f-8642-cbedd56d5dee	1	50000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1j	0bc5fb53-c910-4731-ae64-c30bc747280d	1	17500	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1v	985345ab-c125-469e-bc01-d4cf437258aa	1	25000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1k	9a4fb74e-8d81-44fe-bb15-3b52600b6dcd	1	20000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1i	23965224-e01b-440c-a9c4-5d32a4c85e0a	1	50000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1w	91712dc7-b959-4050-9d91-b2a6a168b3e4	1	40000	0
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1w	4c94709c-403e-470a-866f-cf6375ac0d1c	1	40000	0
\.


--
-- Data for Name: products; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.products (id, name, category, price, stock, created_at, updated_at, main_image) FROM stdin;
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1d	Ice Cream	cc1824cd-9ba8-4a81-ad2a-212edbb3dd3c	45000	60	2025-01-14 09:34:57.554057	2025-01-14 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1a	Potato Chips	8c33508b-0451-461f-9015-660af900f280	17500	120	2025-01-14 09:34:57.554057	2025-01-11 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1b	Chocolate Bar	8c33508b-0451-461f-9015-660af900f280	10000	200	2025-01-14 09:34:57.554057	2025-01-12 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1	Coca-Cola	31f1c4d6-6f24-468b-89fe-b01f0c2550d3	15000	100	2025-01-14 09:34:57.554057	2025-01-01 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o2	Orange Juice	31f1c4d6-6f24-468b-89fe-b01f0c2550d3	20000	50	2025-01-14 09:34:57.554057	2025-01-02 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o3	Apple	46d11070-e2b9-404d-8533-4529a0871d1d	8000	200	2025-01-14 09:34:57.554057	2025-01-03 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o4	Banana	46d11070-e2b9-404d-8533-4529a0871d1d	5000	150	2025-01-14 09:34:57.554057	2025-01-04 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o5	Cheddar Cheese	a491b1a5-5ec3-4b6e-abee-7537298cc4e6	30000	75	2025-01-14 09:34:57.554057	2025-01-05 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1n	Cookies	8c33508b-0451-461f-9015-660af900f280	15000	197	2025-01-14 09:34:57.554057	2025-02-04 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o7	Frozen Peas	cc1824cd-9ba8-4a81-ad2a-212edbb3dd3c	15000	60	2025-01-14 09:34:57.554057	2025-01-07 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o8	Frozen Pizza	cc1824cd-9ba8-4a81-ad2a-212edbb3dd3c	50000	40	2025-01-14 09:34:57.554057	2025-01-08 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o9	Shampoo	5c28b23d-0382-4906-934f-d76ab8378f3e	40000	90	2025-01-14 09:34:57.554057	2025-01-09 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o0	Dish Soap	ee198fd1-52e4-4864-a282-8c38cdb36df9	25000	100	2025-01-14 09:34:57.554057	2025-01-10 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1e	Toothpaste	5c28b23d-0382-4906-934f-d76ab8378f3e	20000	80	2025-01-14 09:34:57.554057	2025-01-15 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1f	Laundry Detergent	ee198fd1-52e4-4864-a282-8c38cdb36df9	35000	70	2025-01-14 09:34:57.554057	2025-01-16 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1g	Bread	2bb50b77-c7c2-4a5f-981b-be2e0dfa36da	12000	200	2025-01-14 09:34:57.554057	2025-01-17 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1j	Pasta	2bb50b77-c7c2-4a5f-981b-be2e0dfa36da	17500	119	2025-01-14 09:34:57.554057	2025-01-20 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1v	Paper Towels	c7178dbb-ba23-42e0-b7f5-01cf693ca244	25000	118	2025-01-14 09:34:57.554057	2025-02-14 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1m	Granola Bars	8c33508b-0451-461f-9015-660af900f280	25000	150	2025-01-14 09:34:57.554057	2025-02-03 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1k	Rice	2bb50b77-c7c2-4a5f-981b-be2e0dfa36da	20000	99	2025-01-14 09:34:57.554057	2025-02-01 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1p	Cat Food	acc5edf9-661e-4625-8683-7cf3b2df7f17	150000	60	2025-01-14 09:34:57.554057	2025-02-05 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1r	Diapers	79a419fb-ce7a-4abe-bd38-90b05772eee2	100000	100	2025-01-14 09:34:57.554057	2025-02-07 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1i	Chicken Breast	2bb50b77-c7c2-4a5f-981b-be2e0dfa36da	50000	48	2025-01-14 09:34:57.554057	2025-01-19 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1u	Household Cleaner	ee198fd1-52e4-4864-a282-8c38cdb36df9	30000	90	2025-01-14 09:34:57.554057	2025-02-10 09:34:57.554057	\N
22982557-1708-4a6e-9a84-1d68fb5d6417	aaa	46d11070-e2b9-404d-8533-4529a0871d1d	1	2	2025-02-20 01:30:13.088717	2025-02-20 01:30:13.088718	
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o6	Yogurt	a491b1a5-5ec3-4b6e-abee-7537298cc4e6	12000	80	2025-01-14 09:34:57.554057	2025-01-05 01:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1w	Trash Bags	c7178dbb-ba23-42e0-b7f5-01cf693ca244	40000	98	2025-01-14 09:34:57.554057	2025-02-12 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1l	Cereal	2bb50b77-c7c2-4a5f-981b-be2e0dfa36da	30000	79	2025-01-14 09:34:57.554057	2025-02-02 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1h	Butter	a491b1a5-5ec3-4b6e-abee-7537298cc4e6	25000	89	2025-01-14 09:34:57.554057	2025-01-18 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1c	Milk	a491b1a5-5ec3-4b6e-abee-7537298cc4e6	15000	149	2025-01-14 09:34:57.554057	2025-01-13 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1t	Pet Shampoo	5c28b23d-0382-4906-934f-d76ab8378f3e	80000	77	2025-01-14 09:34:57.554057	2025-02-09 09:34:57.554057	\N
1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o1s	Wipes	79a419fb-ce7a-4abe-bd38-90b05772eee2	50000	149	2025-01-14 09:34:57.554057	2025-02-08 09:34:57.554057	\N
\.


--
-- Data for Name: transactions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.transactions (id, total_amount, customer_id, user_id, created_at, updated_at) FROM stdin;
6f8ff794-8043-4a0d-85b0-984e4dbe8a36	105000	d319f321-8530-4ad6-b65f-47cf54ae3100	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-25 02:25:35.768051	2025-02-25 02:25:35.768053
fa0fcc85-ce21-49df-8926-9c54f36c63bd	110000	d319f321-8530-4ad6-b65f-47cf54ae3100	07191ec0-1724-4df8-9189-e3be0e27610a	2025-02-26 02:54:01.508329	2025-02-26 02:54:01.508331
b3d6c2d4-3701-4f63-8b4e-2703baadcd30	30000	adf66616-f6b2-4d64-a242-c15b92d4e160	07191ec0-1724-4df8-9189-e3be0e27610a	2025-02-26 15:32:45.286261	2025-02-26 15:32:45.286264
0cf05b98-e220-4601-8d99-9ebf944f8b87	25000	7728af59-ae34-48a4-a199-41eb0cf14243	07191ec0-1724-4df8-9189-e3be0e27610a	2025-02-26 15:32:58.323798	2025-02-26 15:32:58.3238
c72baad0-5e46-4d17-b295-7295851cc0a9	15000	adf66616-f6b2-4d64-a242-c15b92d4e160	07191ec0-1724-4df8-9189-e3be0e27610a	2025-02-26 15:33:16.541398	2025-02-26 15:33:16.541399
cfbce5af-487c-46cc-808b-14486eb77035	80000	7728af59-ae34-48a4-a199-41eb0cf14243	07191ec0-1724-4df8-9189-e3be0e27610a	2025-02-27 08:57:38.982052	2025-02-27 08:57:38.982055
f73ff1a9-f6aa-4ba4-bf4e-aef22149e8f5	50000	adf66616-f6b2-4d64-a242-c15b92d4e160	07191ec0-1724-4df8-9189-e3be0e27610a	2025-02-27 08:57:49.178362	2025-02-27 08:57:49.178363
413c36cc-e823-4cd4-afa5-34e98edf9acf	15000	7728af59-ae34-48a4-a199-41eb0cf14243	07191ec0-1724-4df8-9189-e3be0e27610a	2025-02-27 08:57:56.973573	2025-02-27 08:57:56.973574
49952797-6911-4f5f-8642-cbedd56d5dee	50000	7728af59-ae34-48a4-a199-41eb0cf14243	07191ec0-1724-4df8-9189-e3be0e27610a	2025-02-27 08:58:05.676434	2025-02-27 08:58:05.676435
0bc5fb53-c910-4731-ae64-c30bc747280d	17500	7728af59-ae34-48a4-a199-41eb0cf14243	07191ec0-1724-4df8-9189-e3be0e27610a	2025-02-27 08:58:15.699923	2025-02-27 08:58:15.699925
985345ab-c125-469e-bc01-d4cf437258aa	25000	7728af59-ae34-48a4-a199-41eb0cf14243	07191ec0-1724-4df8-9189-e3be0e27610a	2025-02-27 08:58:25.587354	2025-02-27 08:58:25.587355
9a4fb74e-8d81-44fe-bb15-3b52600b6dcd	20000	7728af59-ae34-48a4-a199-41eb0cf14243	07191ec0-1724-4df8-9189-e3be0e27610a	2025-02-27 08:58:32.498461	2025-02-27 08:58:32.498463
23965224-e01b-440c-a9c4-5d32a4c85e0a	50000	7728af59-ae34-48a4-a199-41eb0cf14243	07191ec0-1724-4df8-9189-e3be0e27610a	2025-02-27 08:58:40.737054	2025-02-27 08:58:40.737055
91712dc7-b959-4050-9d91-b2a6a168b3e4	40000	7728af59-ae34-48a4-a199-41eb0cf14243	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-27 14:13:51.71178	2025-02-27 14:13:51.711782
4c94709c-403e-470a-866f-cf6375ac0d1c	40000	7728af59-ae34-48a4-a199-41eb0cf14243	5ba5d906-2db7-4525-a719-5720d65d15fe	2025-02-27 14:15:22.93418	2025-02-27 14:15:22.934181
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (id, name, username, password, role, created_at, updated_at) FROM stdin;
07191ec0-1724-4df8-9189-e3be0e27610a	user	user	AQAAAAIAAYagAAAAEMCEtDckBvRSCD7yc6hDhra2W34lJHu1YOLND/eEodXpBLcXOz81bJhTI4SzMCEM0g==	user	2025-01-28 17:35:17.667361	2025-01-28 17:35:17.667362
5ba5d906-2db7-4525-a719-5720d65d15fe	admin	admin	AQAAAAIAAYagAAAAEKHp4UGDl5IW3XNMh7AfESO2oaOYan/mBVPbzkrO+/b9CU7Q+oQWB1cB7iS4kpklGw==	admin	2025-02-06 16:56:55.234733	2025-02-06 16:56:55.234735
8b5af87d-a9dd-4d8d-863b-e2e6b024b701	Trail	rudy	AQAAAAIAAYagAAAAEIh60x1tfoerMWIQGUw8hYGTVN1a77fvjQ1V70LmoyE54cMxtNuy9nQs9GpXJlxSGw==	admin	2025-02-17 10:09:29.09128	2025-02-17 10:09:29.091282
\.


--
-- Name: attachments attachments_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attachments
    ADD CONSTRAINT attachments_pkey PRIMARY KEY (id);


--
-- Name: classifications classifications_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.classifications
    ADD CONSTRAINT classifications_pkey PRIMARY KEY (id);


--
-- Name: customers customers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT customers_pkey PRIMARY KEY (id);


--
-- Name: discounts discounts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.discounts
    ADD CONSTRAINT discounts_pkey PRIMARY KEY (id);


--
-- Name: notifications notifications_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT notifications_pkey PRIMARY KEY (id);


--
-- Name: product_transactions product_transactions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.product_transactions
    ADD CONSTRAINT product_transactions_pkey PRIMARY KEY (product_id, transaction_id);


--
-- Name: products products_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_pkey PRIMARY KEY (id);


--
-- Name: transactions transactions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.transactions
    ADD CONSTRAINT transactions_pkey PRIMARY KEY (id);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: users users_username_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);


--
-- Name: discounts discounts_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.discounts
    ADD CONSTRAINT discounts_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: notifications notifications_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT notifications_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE RESTRICT;


--
-- Name: product_transactions product_transactions_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.product_transactions
    ADD CONSTRAINT product_transactions_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE CASCADE;


--
-- Name: product_transactions product_transactions_transaction_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.product_transactions
    ADD CONSTRAINT product_transactions_transaction_id_fkey FOREIGN KEY (transaction_id) REFERENCES public.transactions(id) ON DELETE CASCADE;


--
-- Name: transactions transactions_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.transactions
    ADD CONSTRAINT transactions_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customers(id) ON DELETE SET NULL;


--
-- Name: transactions transactions_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.transactions
    ADD CONSTRAINT transactions_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: SCHEMA public; Type: ACL; Schema: -; Owner: postgres
--

REVOKE USAGE ON SCHEMA public FROM PUBLIC;
GRANT ALL ON SCHEMA public TO PUBLIC;


--
-- PostgreSQL database dump complete
--

