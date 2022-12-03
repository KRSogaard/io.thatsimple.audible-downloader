-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               8.0.30 - MySQL Community Server - GPL
-- Server OS:                    Linux
-- HeidiSQL Version:             12.1.0.6537
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- Dumping structure for table audible.authors
CREATE TABLE IF NOT EXISTS `authors` (
                                         `id` int NOT NULL AUTO_INCREMENT,
                                         `asin` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
                                         `link` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
                                         `name` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL,
                                         `created` int DEFAULT NULL,
                                         PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.authors_books
CREATE TABLE IF NOT EXISTS `authors_books` (
                                               `id` int NOT NULL AUTO_INCREMENT,
                                               `book_id` int NOT NULL,
                                               `author_id` int NOT NULL,
                                               `created` int NOT NULL,
                                               PRIMARY KEY (`id`),
                                               KEY `book_id` (`book_id`),
                                               KEY `author_id` (`author_id`),
                                               CONSTRAINT `authors_books_ibfk_1` FOREIGN KEY (`book_id`) REFERENCES `books` (`id`) ON DELETE CASCADE,
                                               CONSTRAINT `authors_books_ibfk_2` FOREIGN KEY (`author_id`) REFERENCES `authors` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.books
CREATE TABLE IF NOT EXISTS `books` (
                                       `id` int NOT NULL AUTO_INCREMENT,
                                       `asin` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL,
                                       `link` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
                                       `title` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
                                       `length` int DEFAULT NULL,
                                       `released` int DEFAULT NULL,
                                       `summary` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
                                       `last_updated` int NOT NULL,
                                       `created` int NOT NULL,
                                       `categories_cache` text COLLATE utf8mb4_unicode_ci,
                                       `tags_cache` text COLLATE utf8mb4_unicode_ci,
                                       `narrators_cache` text COLLATE utf8mb4_unicode_ci,
                                       `authors_cache` text COLLATE utf8mb4_unicode_ci,
                                       `should_download` tinyint(1) NOT NULL,
                                       `is_temp` tinyint(1) NOT NULL,
                                       PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.categories
CREATE TABLE IF NOT EXISTS `categories` (
                                            `id` int NOT NULL AUTO_INCREMENT,
                                            `name` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL,
                                            `link` varchar(512) COLLATE utf8mb4_unicode_ci NOT NULL,
                                            `created` int NOT NULL,
                                            PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.categories_books
CREATE TABLE IF NOT EXISTS `categories_books` (
                                                  `id` int NOT NULL AUTO_INCREMENT,
                                                  `book_id` int NOT NULL,
                                                  `category_id` int NOT NULL,
                                                  `created` int NOT NULL,
                                                  PRIMARY KEY (`id`),
                                                  KEY `book_id` (`book_id`),
                                                  KEY `category_id` (`category_id`),
                                                  CONSTRAINT `categories_books_ibfk_1` FOREIGN KEY (`book_id`) REFERENCES `books` (`id`) ON DELETE CASCADE,
                                                  CONSTRAINT `categories_books_ibfk_2` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=82 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.narrators
CREATE TABLE IF NOT EXISTS `narrators` (
                                           `id` int NOT NULL AUTO_INCREMENT,
                                           `name` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL,
                                           `created` int NOT NULL,
                                           PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.narrators_books
CREATE TABLE IF NOT EXISTS `narrators_books` (
                                                 `id` int NOT NULL AUTO_INCREMENT,
                                                 `book_id` int NOT NULL,
                                                 `narrator_id` int NOT NULL,
                                                 `created` int NOT NULL,
                                                 PRIMARY KEY (`id`),
                                                 KEY `book_id` (`book_id`),
                                                 KEY `narrator_id` (`narrator_id`),
                                                 CONSTRAINT `narrators_books_ibfk_1` FOREIGN KEY (`book_id`) REFERENCES `books` (`id`) ON DELETE CASCADE,
                                                 CONSTRAINT `narrators_books_ibfk_2` FOREIGN KEY (`narrator_id`) REFERENCES `narrators` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.series
CREATE TABLE IF NOT EXISTS `series` (
                                        `id` int NOT NULL AUTO_INCREMENT,
                                        `asin` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL,
                                        `link` varchar(512) COLLATE utf8mb4_unicode_ci NOT NULL,
                                        `name` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL,
                                        `last_updated` int NOT NULL,
                                        `created` int NOT NULL,
                                        `should_download` tinyint(1) NOT NULL,
                                        `last_checked` int NOT NULL,
                                        PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.series_books
CREATE TABLE IF NOT EXISTS `series_books` (
                                              `id` int NOT NULL AUTO_INCREMENT,
                                              `series_id` int NOT NULL,
                                              `book_id` int NOT NULL,
                                              `book_number` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
                                              `created` int NOT NULL,
                                              `sort` int DEFAULT NULL,
                                              PRIMARY KEY (`id`),
                                              KEY `book_id` (`book_id`),
                                              KEY `series_id` (`series_id`),
                                              CONSTRAINT `series_books_ibfk_1` FOREIGN KEY (`book_id`) REFERENCES `books` (`id`) ON DELETE CASCADE,
                                              CONSTRAINT `series_books_ibfk_2` FOREIGN KEY (`series_id`) REFERENCES `series` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=37 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.tags
CREATE TABLE IF NOT EXISTS `tags` (
                                      `id` int NOT NULL AUTO_INCREMENT,
                                      `tag` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL,
                                      `created` int NOT NULL,
                                      PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.tags_books
CREATE TABLE IF NOT EXISTS `tags_books` (
                                            `id` int NOT NULL AUTO_INCREMENT,
                                            `book_id` int NOT NULL,
                                            `tag_id` int NOT NULL,
                                            `created` int NOT NULL,
                                            PRIMARY KEY (`id`),
                                            KEY `book_id` (`book_id`),
                                            KEY `tag_id` (`tag_id`),
                                            CONSTRAINT `tags_books_ibfk_1` FOREIGN KEY (`book_id`) REFERENCES `books` (`id`) ON DELETE CASCADE,
                                            CONSTRAINT `tags_books_ibfk_2` FOREIGN KEY (`tag_id`) REFERENCES `tags` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=45 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.users
CREATE TABLE IF NOT EXISTS `users` (
                                       `id` int NOT NULL AUTO_INCREMENT,
                                       `username` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL,
                                       `password` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL,
                                       `password_salt` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
                                       `created` int NOT NULL,
                                       `email` varchar(256) COLLATE utf8mb4_unicode_ci NOT NULL,
                                       PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.users_archived_series
CREATE TABLE IF NOT EXISTS `users_archived_series` (
                                                       `id` int NOT NULL AUTO_INCREMENT,
                                                       `user_id` int NOT NULL,
                                                       `series_id` int NOT NULL,
                                                       `created` int NOT NULL,
                                                       PRIMARY KEY (`id`),
                                                       KEY `user_id` (`user_id`),
                                                       KEY `series_id` (`series_id`),
                                                       CONSTRAINT `users_archived_series_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
                                                       CONSTRAINT `users_archived_series_ibfk_2` FOREIGN KEY (`series_id`) REFERENCES `series` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.users_books
CREATE TABLE IF NOT EXISTS `users_books` (
                                             `id` int NOT NULL AUTO_INCREMENT,
                                             `user_id` int NOT NULL,
                                             `book_id` int NOT NULL,
                                             `created` int NOT NULL,
                                             PRIMARY KEY (`id`),
                                             KEY `user_id` (`user_id`),
                                             KEY `book_id` (`book_id`),
                                             CONSTRAINT `book_id` FOREIGN KEY (`book_id`) REFERENCES `books` (`id`) ON DELETE CASCADE,
                                             CONSTRAINT `user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.users_jobs
CREATE TABLE IF NOT EXISTS `users_jobs` (
                                            `id` int NOT NULL AUTO_INCREMENT,
                                            `user_id` int NOT NULL,
                                            `created` int NOT NULL,
                                            `type` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL,
                                            `payload` text COLLATE utf8mb4_unicode_ci NOT NULL,
                                            PRIMARY KEY (`id`),
                                            KEY `user_id` (`user_id`),
                                            CONSTRAINT `users_jobs_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table audible.users_tokens
CREATE TABLE IF NOT EXISTS `users_tokens` (
                                              `id` int NOT NULL AUTO_INCREMENT,
                                              `user_id` int NOT NULL,
                                              `token` varchar(128) COLLATE utf8mb4_unicode_ci NOT NULL,
                                              `created` int NOT NULL,
                                              `expires` int NOT NULL,
                                              PRIMARY KEY (`id`),
                                              KEY `user_id` (`user_id`),
                                              CONSTRAINT `users_tokens_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
