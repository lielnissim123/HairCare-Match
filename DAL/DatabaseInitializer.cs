using System.Data;
using MySql.Data.MySqlClient;

namespace DAL
{
    public static class DatabaseInitializer
    {
        private const string DatabaseName = "HairCareMatchDB";

        public static async Task InitializeAsync(string serverConnectionString)
        {
            await CreateDatabaseAsync(serverConnectionString);

            string dbConnectionString = $"{serverConnectionString}Database={DatabaseName};";

            await CreateTablesAsync(dbConnectionString);
            await SeedRolesAsync(dbConnectionString);
        }

        private static async Task CreateDatabaseAsync(string serverConnectionString)
        {
            string sql = $"CREATE DATABASE IF NOT EXISTS `{DatabaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";

            using var connection = new MySqlConnection(serverConnectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

        private static async Task CreateTablesAsync(string dbConnectionString)
        {
            string createTablesSql = @"
                CREATE TABLE IF NOT EXISTS Roles (
                    RoleID INT AUTO_INCREMENT PRIMARY KEY,
                    RoleName VARCHAR(30) NOT NULL UNIQUE
                ) ENGINE=InnoDB;

                CREATE TABLE IF NOT EXISTS Users (
                    UserID INT AUTO_INCREMENT PRIMARY KEY,
                    Username VARCHAR(50) NOT NULL UNIQUE,
                    Email VARCHAR(100) NOT NULL UNIQUE,
                    PasswordHash VARCHAR(255) NOT NULL,
                    RoleID INT NOT NULL,
                    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID) ON DELETE RESTRICT ON UPDATE CASCADE
                ) ENGINE=InnoDB;

                CREATE TABLE IF NOT EXISTS HairProfiles (
                    ProfileID INT AUTO_INCREMENT PRIMARY KEY,
                    UserID INT NOT NULL UNIQUE,
                    HairType VARCHAR(50) NOT NULL,
                    Thickness VARCHAR(50) NOT NULL,
                    ScalpCondition VARCHAR(50) NOT NULL,
                    TargetIssue VARCHAR(50) NOT NULL,
                    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE ON UPDATE CASCADE
                ) ENGINE=InnoDB;

                CREATE TABLE IF NOT EXISTS Treatments (
                    TreatmentID INT AUTO_INCREMENT PRIMARY KEY,
                    Title VARCHAR(150) NOT NULL,
                    Description TEXT NOT NULL,
                    Ingredients TEXT NOT NULL,
                    Instructions TEXT NOT NULL,
                    DifficultyLevel VARCHAR(20) NOT NULL,
                    TargetIssue VARCHAR(50) NOT NULL,
                    IsApproved BOOLEAN NOT NULL DEFAULT FALSE,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                ) ENGINE=InnoDB;

                CREATE TABLE IF NOT EXISTS UserTreatments (
                    UserTreatmentID INT AUTO_INCREMENT PRIMARY KEY,
                    UserID INT NOT NULL,
                    TreatmentID INT NOT NULL,
                    IsTried BOOLEAN NOT NULL DEFAULT FALSE,
                    SavedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE ON UPDATE CASCADE,
                    FOREIGN KEY (TreatmentID) REFERENCES Treatments(TreatmentID) ON DELETE CASCADE ON UPDATE CASCADE
                ) ENGINE=InnoDB;

                CREATE TABLE IF NOT EXISTS Reviews (
                    ReviewID INT AUTO_INCREMENT PRIMARY KEY,
                    TreatmentID INT NOT NULL,
                    UserID INT NOT NULL,
                    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
                    Comment TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (TreatmentID) REFERENCES Treatments(TreatmentID) ON DELETE CASCADE ON UPDATE CASCADE,
                    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE ON UPDATE CASCADE
                ) ENGINE=InnoDB;

                CREATE TABLE IF NOT EXISTS TreatmentHairTypes (
                    TreatmentID INT NOT NULL,
                    HairType VARCHAR(50) NOT NULL,
                    PRIMARY KEY (TreatmentID, HairType),
                    FOREIGN KEY (TreatmentID) REFERENCES Treatments(TreatmentID) ON DELETE CASCADE ON UPDATE CASCADE
                ) ENGINE=InnoDB;
            ";

            using var connection = new MySqlConnection(dbConnectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(createTablesSql, connection);
            await command.ExecuteNonQueryAsync();
        }

        private static async Task SeedRolesAsync(string dbConnectionString)
        {
            using var connection = new MySqlConnection(dbConnectionString);
            await connection.OpenAsync();

            string checkSql = "SELECT COUNT(*) FROM Roles;";
            using var checkCmd = new MySqlCommand(checkSql, connection);

            long count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());

            if (count == 0)
            {
                string insertRolesSql = @"
                    INSERT INTO Roles (RoleName) VALUES
                    ('SuperAdmin'),
                    ('ContentManager'),
                    ('User'),
                    ('Guest');
                ";
                using var insertCmd = new MySqlCommand(insertRolesSql, connection);
                await insertCmd.ExecuteNonQueryAsync();
            }
        }
    }
}