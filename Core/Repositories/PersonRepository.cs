using DormitoryManagement.Core.Database;
using DormitoryManagement.Core.Models;
using Microsoft.Data.Sqlite; // Add this
using System.Collections.Generic;

namespace DormitoryManagement.Core.Repositories
{
    public class PersonRepository
    {
        private DbConnection? _dbConnection; // Make this nullable

        // Constructor for when we create our own connection
        public PersonRepository()
        {
            _dbConnection = new DbConnection();
        }

        // --- NEW ---
        // Constructor for when we use a shared connection (no-op, we'll pass connection directly)
        public PersonRepository(bool useSharedConnection = false)
        {
            if (!useSharedConnection)
            {
                _dbConnection = new DbConnection();
            }
        }
        
        // This is the key method. It allows an operation to be part of a larger transaction.
        public int Add(Person person, SqliteConnection connection, SqliteTransaction transaction)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction; // Use the existing transaction
            
            command.CommandText = "INSERT INTO Persons (FirstName, LastName, NationalId, PhoneNumber, Address) VALUES ($firstName, $lastName, $nationalId, $phone, $address)";
            command.Parameters.AddWithValue("$firstName", person.FirstName);
            command.Parameters.AddWithValue("$lastName", person.LastName);
            command.Parameters.AddWithValue("$nationalId", person.NationalId);
            command.Parameters.AddWithValue("$phone", person.PhoneNumber ?? (object)System.DBNull.Value);
            command.Parameters.AddWithValue("$address", person.Address ?? (object)System.DBNull.Value);
            command.ExecuteNonQuery();

            // Get the Id of the person we just inserted
            var lastIdCommand = connection.CreateCommand();
            lastIdCommand.Transaction = transaction;
            lastIdCommand.CommandText = "SELECT last_insert_rowid()";
            return (int)(long)lastIdCommand.ExecuteScalar();
        }

        // The old Add method for single, non-transactional adds (if needed elsewhere)
        public int Add(Person person)
        {
            using (var conn = _dbConnection!.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var newId = Add(person, conn, trans);
                    trans.Commit();
                    return newId;
                }
            }
        }

        // The GetAll method is a read-only operation, so it's fine as is.
        public List<Person> GetAll()
        {
            var people = new List<Person>();
            using (var connection = _dbConnection!.GetConnection())
            {
                // ... (GetAll method is unchanged) ...
            }
            return people;
        }
    }
}