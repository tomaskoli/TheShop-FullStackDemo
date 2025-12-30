// TheShop Neo4j Seed Data
// Manual setup in Neo4j Browser:
//   1. Connect to 'system' database and run: CREATE DATABASE theshop IF NOT EXISTS;
//   2. Switch to 'theshop' database: :use theshop
//   3. Run the rest of this script

// Clear existing data (use with caution in production)
MATCH (n) DETACH DELETE n;

// Create constraints for uniqueness
CREATE CONSTRAINT product_id IF NOT EXISTS FOR (p:Product) REQUIRE p.id IS UNIQUE;
CREATE CONSTRAINT brand_id IF NOT EXISTS FOR (b:Brand) REQUIRE b.id IS UNIQUE;
CREATE CONSTRAINT category_id IF NOT EXISTS FOR (c:Category) REQUIRE c.id IS UNIQUE;

// Create indexes for performance
CREATE INDEX product_name IF NOT EXISTS FOR (p:Product) ON (p.name);
CREATE INDEX product_available IF NOT EXISTS FOR (p:Product) ON (p.isAvailable);
CREATE INDEX brand_name IF NOT EXISTS FOR (b:Brand) ON (b.name);
CREATE INDEX category_name IF NOT EXISTS FOR (c:Category) ON (c.name);

// Categories
CREATE (:Category {id: '11111111-1111-1111-1111-111111111001', name: 'Smartphones', description: 'Mobile phones and accessories'});
CREATE (:Category {id: '11111111-1111-1111-1111-111111111002', name: 'Headsets', description: 'Audio headphones and earbuds'});
CREATE (:Category {id: '11111111-1111-1111-1111-111111111003', name: 'Laptops', description: 'Portable computers'});

// Brands
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222001', name: 'Apple'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222002', name: 'Samsung'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222003', name: 'Sony'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222004', name: 'Google'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222005', name: 'OnePlus'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222006', name: 'Xiaomi'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222007', name: 'Bose'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222008', name: 'JBL'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222009', name: 'Sennheiser'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222010', name: 'Dell'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222011', name: 'Lenovo'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222012', name: 'HP'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222013', name: 'Asus'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222014', name: 'Huawei'});
CREATE (:Brand {id: '22222222-2222-2222-2222-222222222015', name: 'Motorola'});

// Smartphones - Apple
MATCH (b:Brand {name: 'Apple'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000001', name: 'iPhone 15 Pro Max 256GB', description: 'Latest flagship with A17 Pro chip', price: 1199.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Apple'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000002', name: 'iPhone 15 Pro 128GB', description: 'Pro performance in compact size', price: 999.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Apple'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000003', name: 'iPhone 15 256GB', description: 'Dynamic Island for everyone', price: 899.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Apple'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000004', name: 'iPhone 15 Plus 256GB', description: 'Big screen experience', price: 999.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Apple'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000005', name: 'iPhone 14 128GB', description: 'Previous gen great value', price: 699.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Apple'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000007', name: 'iPhone SE 64GB', description: 'Compact powerhouse', price: 429.00, isAvailable: false})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Smartphones - Samsung
MATCH (b:Brand {name: 'Samsung'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000013', name: 'Galaxy S24 Ultra 256GB', description: 'AI-powered flagship', price: 1299.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Samsung'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000014', name: 'Galaxy S24+ 256GB', description: 'Premium large screen', price: 999.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Samsung'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000015', name: 'Galaxy S24 128GB', description: 'Compact flagship', price: 799.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Samsung'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000016', name: 'Galaxy Z Fold5 256GB', description: 'Foldable innovation', price: 1799.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Samsung'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000017', name: 'Galaxy Z Flip5 256GB', description: 'Compact foldable style', price: 999.00, isAvailable: false})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Smartphones - Google
MATCH (b:Brand {name: 'Google'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000025', name: 'Pixel 8 Pro 128GB', description: 'AI-first flagship', price: 999.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Google'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000026', name: 'Pixel 8 128GB', description: 'Pure Android experience', price: 699.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Smartphones - OnePlus
MATCH (b:Brand {name: 'OnePlus'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000033', name: 'OnePlus 12 256GB', description: 'Flagship killer returns', price: 799.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'OnePlus'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000034', name: 'OnePlus 12R 128GB', description: 'Value flagship', price: 499.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Smartphones - Xiaomi
MATCH (b:Brand {name: 'Xiaomi'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000041', name: 'Xiaomi 14 Ultra 512GB', description: 'Leica camera flagship', price: 1299.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Xiaomi'}), (c:Category {name: 'Smartphones'})
CREATE (p:Product {id: '33333333-0001-0001-0001-000000000042', name: 'Xiaomi 14 256GB', description: 'Compact flagship', price: 899.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Headsets - Sony
MATCH (b:Brand {name: 'Sony'}), (c:Category {name: 'Headsets'})
CREATE (p:Product {id: '33333333-0002-0002-0002-000000000001', name: 'Sony WH-1000XM5', description: 'Premium ANC over-ear', price: 399.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Sony'}), (c:Category {name: 'Headsets'})
CREATE (p:Product {id: '33333333-0002-0002-0002-000000000002', name: 'Sony WH-1000XM4', description: 'Previous gen ANC', price: 299.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Sony'}), (c:Category {name: 'Headsets'})
CREATE (p:Product {id: '33333333-0002-0002-0002-000000000003', name: 'Sony WF-1000XM5', description: 'Premium ANC earbuds', price: 299.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Headsets - Bose
MATCH (b:Brand {name: 'Bose'}), (c:Category {name: 'Headsets'})
CREATE (p:Product {id: '33333333-0002-0002-0002-000000000011', name: 'Bose QuietComfort Ultra', description: 'Premium ANC headphones', price: 429.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Bose'}), (c:Category {name: 'Headsets'})
CREATE (p:Product {id: '33333333-0002-0002-0002-000000000012', name: 'Bose QuietComfort 45', description: 'Classic ANC comfort', price: 329.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Headsets - Apple
MATCH (b:Brand {name: 'Apple'}), (c:Category {name: 'Headsets'})
CREATE (p:Product {id: '33333333-0002-0002-0002-000000000041', name: 'AirPods Pro 2nd Gen', description: 'Premium ANC earbuds', price: 249.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Apple'}), (c:Category {name: 'Headsets'})
CREATE (p:Product {id: '33333333-0002-0002-0002-000000000042', name: 'AirPods 3rd Gen', description: 'Spatial audio earbuds', price: 179.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Apple'}), (c:Category {name: 'Headsets'})
CREATE (p:Product {id: '33333333-0002-0002-0002-000000000044', name: 'AirPods Max Space Gray', description: 'Premium over-ear', price: 549.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Headsets - JBL
MATCH (b:Brand {name: 'JBL'}), (c:Category {name: 'Headsets'})
CREATE (p:Product {id: '33333333-0002-0002-0002-000000000021', name: 'JBL Tour One M2', description: 'Premium ANC over-ear', price: 349.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'JBL'}), (c:Category {name: 'Headsets'})
CREATE (p:Product {id: '33333333-0002-0002-0002-000000000024', name: 'JBL Tour Pro 2', description: 'Smart case earbuds', price: 249.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Headsets - Sennheiser
MATCH (b:Brand {name: 'Sennheiser'}), (c:Category {name: 'Headsets'})
CREATE (p:Product {id: '33333333-0002-0002-0002-000000000031', name: 'Sennheiser Momentum 4', description: 'Premium wireless', price: 349.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Sennheiser'}), (c:Category {name: 'Headsets'})
CREATE (p:Product {id: '33333333-0002-0002-0002-000000000032', name: 'Sennheiser HD 660S2', description: 'Audiophile open-back', price: 499.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Laptops - Apple
MATCH (b:Brand {name: 'Apple'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000001', name: 'MacBook Pro 14 M3 Pro', description: 'Pro chip laptop', price: 1999.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Apple'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000002', name: 'MacBook Pro 16 M3 Max', description: 'Maximum performance', price: 3499.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Apple'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000003', name: 'MacBook Air 13 M3', description: 'Thin and light M3', price: 1099.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Apple'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000004', name: 'MacBook Air 15 M3', description: 'Larger Air display', price: 1299.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Laptops - Dell
MATCH (b:Brand {name: 'Dell'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000007', name: 'Dell XPS 15 9530', description: 'Premium Windows laptop', price: 1799.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Dell'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000008', name: 'Dell XPS 13 Plus', description: 'Compact premium', price: 1399.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Dell'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000012', name: 'Dell G16 Gaming', description: 'Gaming laptop', price: 1499.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Laptops - Lenovo
MATCH (b:Brand {name: 'Lenovo'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000013', name: 'ThinkPad X1 Carbon Gen 11', description: 'Business flagship', price: 1849.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Lenovo'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000017', name: 'Legion Pro 7i', description: 'Gaming flagship', price: 2499.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Laptops - HP
MATCH (b:Brand {name: 'HP'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000019', name: 'HP Spectre x360 14', description: 'Premium convertible', price: 1649.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'HP'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000023', name: 'HP Omen 16', description: 'Gaming laptop', price: 1599.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

// Laptops - Asus
MATCH (b:Brand {name: 'Asus'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000025', name: 'ASUS ROG Zephyrus G14', description: 'Gaming ultrabook', price: 1649.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Asus'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000027', name: 'ASUS ZenBook 14 OLED', description: 'Premium ultrabook', price: 1099.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

MATCH (b:Brand {name: 'Asus'}), (c:Category {name: 'Laptops'})
CREATE (p:Product {id: '33333333-0003-0003-0003-000000000030', name: 'ASUS VivoBook S 15 OLED', description: 'Value OLED laptop', price: 799.00, isAvailable: true})-[:MADE_BY]->(b), (p)-[:BELONGS_TO]->(c);

