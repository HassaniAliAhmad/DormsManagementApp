using DormitoryManagement.Core.Utils; // For the PasswordHasher
using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace DormitoryManagement.Core.Database
{
    public static class DatabaseInitializer
    {
        // This is the only public method. It's called once when the application starts.
        public static void Initialize()
        {
            var dbConnection = new DbConnection();
            using (var connection = dbConnection.GetConnection())
            {
                connection.Open();

                // Check if the 'Users' table exists. This is a reliable way to see if the schema has been created.
                var command = connection.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Users';";

                // ExecuteScalar returns the first column of the first row, or null if no results.
                var result = command.ExecuteScalar();

                // If the result is null, the 'Users' table does not exist, so we create everything.
                if (result == null)
                {
                    Console.WriteLine("Database schema not found. Creating tables and seeding admin user...");
                    CreateSchemaAndSeedAdmin(connection);
                }
            }
        }

        // This private method contains all the logic for creating tables and the default admin.
        // It now accepts an open connection to ensure everything happens in one sequence.
        private static void CreateSchemaAndSeedAdmin(SqliteConnection connection)
        {
            var command = connection.CreateCommand();

            // This single string contains all table creation commands.
            command.CommandText = @"
                -- Persons Table (Base for all people)
                CREATE TABLE Persons (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    NationalId TEXT UNIQUE NOT NULL,
                    PhoneNumber TEXT,
                    Address TEXT
                );

                -- Dormitories Table
                CREATE TABLE Dormitories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT UNIQUE NOT NULL,
                    Address TEXT,
                    TotalCapacity INTEGER NOT NULL,
                    DormOfficialId INTEGER,
                    FOREIGN KEY (DormOfficialId) REFERENCES Persons(Id)
                );

                -- Blocks Table
                CREATE TABLE Blocks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    DormId INTEGER NOT NULL,
                    BlockOfficialId INTEGER,
                    FOREIGN KEY (DormId) REFERENCES Dormitories(Id) ON DELETE CASCADE,
                    FOREIGN KEY (BlockOfficialId) REFERENCES Persons(Id)
                );

                -- Rooms Table
                CREATE TABLE Rooms (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    RoomNumber TEXT NOT NULL,
                    Floor INTEGER NOT NULL,
                    Capacity INTEGER DEFAULT 6 NOT NULL,
                    BlockId INTEGER NOT NULL,
                    UNIQUE (RoomNumber, BlockId),
                    FOREIGN KEY (BlockId) REFERENCES Blocks(Id) ON DELETE CASCADE
                );

                -- Students Table (Inherits from Person)
                CREATE TABLE Students (
                    PersonId INTEGER PRIMARY KEY,
                    StudentId TEXT UNIQUE NOT NULL,
                    RoomId INTEGER,
                    FOREIGN KEY (PersonId) REFERENCES Persons(Id) ON DELETE CASCADE,
                    FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
                );

                -- Assets Table
                CREATE TABLE Assets (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AssetNumber TEXT UNIQUE NOT NULL,
                    Type TEXT NOT NULL CHECK (Type IN ('Bed', 'Wardrobe', 'Desk', 'Chair', 'Fridge', 'Other')),
                    PartNumber TEXT NOT NULL,
                    Status TEXT NOT NULL CHECK (Status IN ('Healthy', 'Faulty', 'UnderRepair')),
                    RoomId INTEGER,
                    OwningStudentId INTEGER,
                    FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
                    FOREIGN KEY (OwningStudentId) REFERENCES Persons(Id)
                );

                -- Users Table (For login)
                CREATE TABLE Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    UserRole TEXT NOT NULL CHECK (UserRole IN ('Admin', 'DormOfficial', 'Student')),
                    PersonId INTEGER UNIQUE,
                    FOREIGN KEY (PersonId) REFERENCES Persons(Id) ON DELETE CASCADE
                );

                -- Asset Maintenance Logs Table
                CREATE TABLE AssetMaintenanceLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AssetId INTEGER NOT NULL,
                    RequestDate TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Status TEXT NOT NULL CHECK (Status IN ('Requested', 'InProgress', 'Completed', 'Canceled')),
                    FOREIGN KEY (AssetId) REFERENCES Assets(Id) ON DELETE CASCADE
                );
            ";

            command.ExecuteNonQuery();
            Console.WriteLine("Database tables created successfully.");

            // --- ADMIN SEEDING LOGIC ---
            // This code now runs immediately after the tables are created.
            Console.WriteLine("Seeding default admin account...");
            
            // Use a transaction to ensure both inserts (Person and User) succeed or fail together
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // 1. Insert the Person record for the admin
                    command.CommandText = "INSERT INTO Persons (FirstName, LastName, NationalId) VALUES ('Admin', 'User', '0000000000')";
                    command.ExecuteNonQuery();

                    // 2. Get the ID of the new Person
                    command.CommandText = "SELECT last_insert_rowid()";
                    var personId = (long)command.ExecuteScalar();

                    // 3. Insert the User record with the 'Admin' role
                    command.Parameters.Clear(); // Clear any old parameters
                    command.CommandText = "INSERT INTO Users (Username, PasswordHash, UserRole, PersonId) VALUES ($username, $passwordHash, 'Admin', $personId)";
                    command.Parameters.AddWithValue("$username", "admin");
                    command.Parameters.AddWithValue("$passwordHash", PasswordHasher.HashPassword("admin")); // Hash the default password
                    command.Parameters.AddWithValue("$personId", personId);
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    Console.WriteLine("Default admin account (admin/admin) created.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding admin user: {ex.Message}");
                    transaction.Rollback();
                }
            }
        }
    }
}