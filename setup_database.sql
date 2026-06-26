-- =============================================================
-- Cybersecurity Awareness Chatbot – Part 3
-- Run this script once in MySQL Workbench or mysql CLI to
-- create the database and the tasks table.
--
-- Usage:
--   mysql -u root -p < setup_database.sql
-- =============================================================

CREATE DATABASE IF NOT EXISTS cybersecurity_bot
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE cybersecurity_bot;

CREATE TABLE IF NOT EXISTS cyber_tasks (
    Id          INT AUTO_INCREMENT PRIMARY KEY,
    Title       VARCHAR(200)    NOT NULL,
    Description TEXT            NOT NULL DEFAULT '',
    ReminderDate DATETIME       NULL,
    IsCompleted TINYINT(1)      NOT NULL DEFAULT 0,
    CreatedAt   DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP
);
