-- Создание базы данных
CREATE DATABASE IF NOT EXISTS mwsw_db;
USE mwsw_db;

-- Таблица пользователей
CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    birth_date DATE,
    registration_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    avatar_color VARCHAR(7) DEFAULT '#3498db'
);

-- Таблица планет
CREATE TABLE IF NOT EXISTS planets (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL,
    short_description TEXT,
    description TEXT,
    image_url VARCHAR(255),
    price DECIMAL(15,2),
    mass VARCHAR(50),
    diameter VARCHAR(50),
    distance_from_sun VARCHAR(50),
    type VARCHAR(50),
    moons INT,
    orbital_period VARCHAR(50),
    sold_out BOOLEAN DEFAULT FALSE
);

-- Таблица корзины
CREATE TABLE IF NOT EXISTS cart (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT,
    planet_id INT NOT NULL,
    quantity INT DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (planet_id) REFERENCES planets(id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_planet (user_id, planet_id)
);

-- Таблица заказов
CREATE TABLE IF NOT EXISTS orders (
    id INT AUTO_INCREMENT PRIMARY KEY,
    order_number VARCHAR(50) UNIQUE NOT NULL,
    user_id INT,
    order_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    status ENUM('processing', 'shipped', 'delivered') DEFAULT 'processing',
    total_amount DECIMAL(15,2),
    delivery_cost DECIMAL(15,2) DEFAULT 500000000,
    tax_amount DECIMAL(15,2),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL
);

-- Таблица элементов заказа
CREATE TABLE IF NOT EXISTS order_items (
    id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT NOT NULL,
    planet_id INT NOT NULL,
    quantity INT DEFAULT 1,
    price_at_time DECIMAL(15,2),
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    FOREIGN KEY (planet_id) REFERENCES planets(id) ON DELETE CASCADE
);

-- Таблица новостей
CREATE TABLE IF NOT EXISTS news (
    id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    content TEXT,
    excerpt TEXT,
    image_url VARCHAR(255),
    category VARCHAR(100),
    author VARCHAR(100),
    publish_date DATE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица избранного
CREATE TABLE IF NOT EXISTS favorites (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    planet_id INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (planet_id) REFERENCES planets(id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_planet_favorite (user_id, planet_id)
);

-- Заполнение таблицы планет данными
INSERT INTO planets (name, short_description, description, image_url, price, mass, diameter, distance_from_sun, type, moons, orbital_period, sold_out) VALUES
('Меркурий', 'Самая маленькая и ближайшая к Солнцу планета.', 'Меркурий - самая маленькая планета Солнечной системы и ближайшая к Солнцу. Из-за близкого расположения к звезде, температура на поверхности Меркурия колеблется от -180°C ночью до +430°C днем. Планета не имеет атмосферы и спутников.', 'https://i.pinimg.com/736x/eb/76/be/eb76be10595f3b3dedcaf5244b3334c3.jpg', 1500000000, '3.3×10^23 кг', '4,879 км', '57.9 млн км', 'Каменистая', 0, '88 земных дней', FALSE),
('Венера', 'Вторая планета от Солнца, известная как "утренняя звезда".', 'Венера - вторая планета от Солнца, часто называемая "утренней звездой" или "вечерней звездой". Имеет плотную атмосферу, состоящую преимущественно из углекислого газа, что создает сильный парниковый эффект. Температура на поверхности достигает 470°C.', 'https://i.pinimg.com/1200x/6c/e1/c1/6ce1c11e55d9177c780aa55490637dc9.jpg', 2800000000, '4.87×10^24 кг', '12,104 км', '108.2 млн км', 'Каменистая', 0, '225 земных дней', FALSE),
('Земля', 'Наша родная планета, единственная известная обитаемая планета.', 'Земля - третья планета от Солнца и единственная известная планета, на которой существует жизнь. Имеет разнообразные климатические зоны, богатую биосферу и единственный естественный спутник - Луну.', 'https://i.pinimg.com/736x/20/5e/e5/205ee5630c216cc8b7395688b1158b6e.jpg', 0, '5.97×10^24 кг', '12,756 км', '149.6 млн км', 'Каменистая', 1, '365.25 земных дней', TRUE),
('Марс', 'Красная планета, потенциальный кандидат для колонизации.', 'Марс - четвертая планета от Солнца, известная как "Красная планета" из-за оксида железа на поверхности. Имеет тонкую атмосферу, состоящую преимущественно из углекислого газа, и два небольших спутника - Фобос и Деймос.', 'https://i.pinimg.com/736x/e0/be/a0/e0bea09fd3261b3c7c8287c29c8a54f0.jpg', 3500000000, '6.42×10^23 кг', '6,792 км', '227.9 млн км', 'Каменистая', 2, '687 земных дней', FALSE),
('Юпитер', 'Крупнейшая планета Солнечной системы, газовый гигант.', 'Юпитер - пятая планета от Солнца и крупнейшая в Солнечной системе. Это газовый гигант, состоящий преимущественно из водорода и гелия. Имеет мощное магнитное поле и знаменитое Большое Красное Пятно - гигантский шторм, бушующий сотни лет.', 'https://i.pinimg.com/736x/3c/75/7a/3c757a1fecac15cfe661f31d16ad3c83.jpg', 12000000000, '1.90×10^27 кг', '142,984 км', '778.5 млн км', 'Газовая', 79, '11.9 земных лет', FALSE),
('Сатурн', 'Планета с знаменитой системой колец.', 'Сатурн - шестая планета от Солнца, известная своей впечатляющей системой колец, состоящих из частиц льда и камня. Как и Юпитер, это газовый гигант, состоящий преимущественно из водорода и гелия.', 'https://i.pinimg.com/736x/4b/9a/23/4b9a23467c2a88b5b0b9f6a02c615028.jpg', 10500000000, '5.68×10^26 кг', '120,536 км', '1.43 млрд км', 'Газовая', 82, '29.5 земных лет', FALSE),
('Уран', 'Ледяной гигант с уникальным наклоном оси.', 'Уран - седьмая планета от Солнца, ледяной гигант, состоящий преимущественно из воды, аммиака и метана в различных состояниях агрегации. Уникальной особенностью Урана является его ось вращения, наклоненная почти на 98 градусов.', 'https://i.pinimg.com/736x/3d/8f/de/3d8fdee2cfd8f61686457d585f63d728.jpg', 8700000000, '8.68×10^25 кг', '51,118 км', '2.87 млрд км', 'Ледяной гигант', 27, '84 земных года', FALSE),
('Нептун', 'Самый ветреный мир Солнечной системы.', 'Нептун - восьмая и самая дальняя от Солнца планета. Это ледяной гигант, подобный Урану, с самой сильной ветровой системой в Солнечной системе - скорости ветра могут достигать 2100 км/ч.', 'https://i.pinimg.com/736x/a7/c9/0f/a7c90ff08b97074a4bb3bcff61279d96.jpg', 9200000000, '1.02×10^26 кг', '49,528 км', '4.5 млрд км', 'Ледяной гигант', 14, '165 земных лет', FALSE),
('Плутон', 'Карликовая планета в поясе Койпера.', 'Плутон - карликовая планета в поясе Койпера, ранее считавшаяся девятой планетой Солнечной системы. Имеет пять известных спутников, крупнейший из которых - Харон.', 'https://i.pinimg.com/1200x/23/b3/7e/23b37eeb43cabcd470d7c3f27d481592.jpg', 2100000000, '1.31×10^22 кг', '2,377 км', '5.91 млрд км', 'Карликовая планета', 5, '248 земных лет', TRUE);

-- Добавление тестового пользователя (пароль: 1234567)
-- Хэш пароля для "1234567" через SHA256
INSERT INTO users (full_name, email, password_hash, phone, birth_date, avatar_color) VALUES
(CONVERT(0xD098D0B2D0B0D0BD20D098D0B2D0B0D0BDD0BED0B2 USING utf8mb4), 'ivan@example.com', TO_BASE64(UNHEX(SHA2('1234567', 256))), '+7 (999) 123-45-67', '1985-01-15', '#3498db');

-- Добавление тестового пользователя для регистрации (пароль: 1234567)
INSERT INTO users (full_name, email, password_hash, phone, birth_date, avatar_color) VALUES
('Тестовый Пользователь', 'test@example.com', TO_BASE64(UNHEX(SHA2('1234567', 256))), '+7 (888) 888-88-88', '1990-01-01', '#2ecc71');

-- Добавление тестового заказа
INSERT INTO orders (order_number, user_id, status, total_amount, delivery_cost, tax_amount) VALUES
('MW-2023-001', 1, 'delivered', 8060000000, 500000000, 1260000000);

INSERT INTO order_items (order_id, planet_id, quantity, price_at_time) VALUES
(1, 4, 1, 3500000000),
(1, 2, 1, 2800000000);

-- Добавление новостей
INSERT INTO news (title, content, excerpt, image_url, category, author, publish_date) VALUES
('Открыта новая экзопланета в зоне обитаемости', 'Астрономы обнаружили планету, похожую на Землю, в зоне обитаемости звезды. Это открытие может иметь фундаментальное значение для поиска жизни за пределами нашей Солнечной системы...', 'Астрономы обнаружили планету, похожую на Землю, в зоне обитаемости звезды...', 'https://i.pinimg.com/736x/2c/71/8d/2c718d6c9e7c501a6019f2a414faf030.jpg', 'Научные открытия', 'Доктор Астрономов', '2023-03-15'),
('Марсианская колония: новые перспективы', 'Ученые представили новые технологии для создания устойчивой среды на Марсе...', 'Ученые представили новые технологии для создания устойчивой среды на Марсе...', 'https://i.pinimg.com/736x/f6/6a/52/f66a521fff24ba140876f01cbc1463d8.jpg', 'Космические технологии', 'Инженер Космонавтов', '2023-03-10'),
('Тайны Большого Красного Пятна Юпитера', 'Новые исследования раскрывают секреты гигантского шторма на Юпитере...', 'Новые исследования раскрывают секреты гигантского шторма на Юпитере...', 'https://i.pinimg.com/736x/32/46/1d/32461d3c1841f4ec987f307bab502d19.jpg', 'Научные открытия', 'Астрофизик Звездный', '2023-03-05');

-- Добавление тестовых избранных для пользователя 1
INSERT INTO favorites (user_id, planet_id) VALUES 
(1, 4),  -- Марс
(1, 5),  -- Юпитер
(1, 2);  -- Венера
