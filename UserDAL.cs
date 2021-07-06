using System;
using System.Text;
using System.Data.SqlClient;

namespace dotnet_chat_server
{
    public class UserDAL
    {
        public enum InsertResult
        {
            SUCCESS,
            GENERIC_ERROR,
            LOGIN_UNIQUE_KEY_ERROR,
        };

        public enum LoginResult
        {
            SUCCESS,
            GENERIC_ERROR,
            USER_NOT_FOUND,
        };

        public InsertResult Insert(User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Data Source=localhost;Initial Catalog=chat;Integrated Security=True"))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand();

                    command.Connection = connection;

                    command.CommandText = @"
					INSERT INTO [tb-user]
					([login], [name], [password])
					VALUES (@login, @name, @password)
					";

                    command.Parameters.Add(new SqlParameter("login", user.Login));
                    command.Parameters.Add(new SqlParameter("name", user.Name));
                    command.Parameters.Add(new SqlParameter("password", user.Password));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        return InsertResult.SUCCESS; //Sucesso
                    }

                    return InsertResult.GENERIC_ERROR; //Falha desconhecida
                }
            }
            catch (SqlException ex0)
            {
                Console.WriteLine(ex0.StackTrace); //Log de erro

                if (ex0.Number == 2627)
                {
                    return InsertResult.LOGIN_UNIQUE_KEY_ERROR; //Login já cadastrado
                }

                return InsertResult.GENERIC_ERROR; //Falha desconhecida
            }
            catch (Exception ex1)
            {
                Console.WriteLine(ex1.StackTrace); //Log de erro

                return InsertResult.GENERIC_ERROR; //Falha desconhecida
            }
        }

        public LoginResult GetByLoginPassword(User user, StringBuilder userName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Data Source=localhost;Initial Catalog=chat;Integrated Security=True"))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand();

                    command.Connection = connection;

                    command.CommandText = @"
					SELECT 
					[id],
					[login],
					[name],
					[password]
					FROM [tb-user]
					WHERE [login] = @login
					AND [password] = @password
					";

                    command.Parameters.Add(new SqlParameter("login", user.Login));
                    command.Parameters.Add(new SqlParameter("password", user.Password));

                    SqlDataReader dataReader = command.ExecuteReader();

                    if (dataReader.Read())
                    {
                        userName.Append(dataReader.GetString(2));

                        return LoginResult.SUCCESS;
                    }

                    return LoginResult.USER_NOT_FOUND;
                }
            }
            catch
            {
                return LoginResult.GENERIC_ERROR;
            }
        }
    }
}
