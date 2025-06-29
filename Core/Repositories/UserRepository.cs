using DormitoryManagement.Core.Database;
using DormitoryManagement.Core.Models;
using DormitoryManagement.Core.Utils;

namespace DormitoryManagement.Core.Repositories
{
    public class UserRepository
    {
        private DbConnection _dbConnection;
        
        public UserRepository()
        {
            _dbConnection = new DbConnection();
        }

        public User? GetByUsername(string username)
        {
            User? user = null;
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Username, PasswordHash, UserRole, PersonId FROM Users WHERE Username = $username";
                command.Parameters.AddWithValue("$username", username);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            PasswordHash = reader.GetString(2),
                            UserRole = reader.GetString(3),
                            PersonId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                        };
                    }
                }
            }
            return user; // <<< --- THIS IS THE MISSING LINE TO ADD ---
        }

        // --- REFACTORED METHOD ---
        public void Register(Person personDetails, string username, string password)
        {
            // Create a single PersonRepository instance that will share our connection
            var personRepository = new PersonRepository(useSharedConnection: true); 
            
            // Create the single connection that will be shared by all operations
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                // Start the single transaction
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Add the person using the SHARED connection and transaction
                        int personId = personRepository.Add(personDetails, connection, transaction);

                        // 2. Create the User account using the SAME connection and transaction
                        var userCommand = connection.CreateCommand();
                        userCommand.Transaction = transaction; // Assign the transaction
                        userCommand.CommandText = "INSERT INTO Users (Username, PasswordHash, UserRole, PersonId) VALUES ($username, $passwordHash, $userRole, $personId)";
                        userCommand.Parameters.AddWithValue("$username", username);
                        userCommand.Parameters.AddWithValue("$passwordHash", PasswordHasher.HashPassword(password));
                        userCommand.Parameters.AddWithValue("$userRole", "Student"); // Default role
                        userCommand.Parameters.AddWithValue("$personId", personId);
                        userCommand.ExecuteNonQuery();

                        // If both operations succeeded, commit the transaction to save the changes
                        transaction.Commit();
                    }
                    catch
                    {
                        // If any error occurred, roll back the transaction.
                        // This ensures that we don't end up with a partial registration (e.g., a Person without a User).
                        transaction.Rollback();
                        throw; // Re-throw the exception to notify the caller
                    }
                }
            }
        }
    }
}