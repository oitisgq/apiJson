using Classes;
using ProjectJson.Models.Project;
using ProjectJson.Models.User;
using System.Collections.Generic;


namespace ProjectJson.DAOs
{
    public class UserDAO
    {
        private Connection _connection;

        public UserDAO()
        {
            _connection = new Connection(Bancos.Sgq);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public IList<User> getUsers()
        {
            string sql = @"
                select 
	                login,
	                name,
	                email,
	                cpf,
	                '' as password
                from 
	                SGQ_Users
                ";

            var list = _connection.Executar<User>(sql);

            return list;
        }

        public User getUserByCpf(string login, string cpf)
        {
            string sql = @"
                select 
	                login,
	                name,
	                email,
	                cpf,
	                '' as password
                from 
	                SGQ_Users
                where 
	                login = '@login' and 
	                cpf = '@cpf'
	                --login = 'TR412061' and 
	                --cpf = '85812838534'
                ";

            sql = sql.Replace("@login", login);
            sql = sql.Replace("@cpf", cpf);

            var list = _connection.Executar<User>(sql);


            if (list.Count > 0)
                return list[0];
            else
                return null;
        }

        public User getUserByPassword(string login, string password)
        {
            string sql = @"
                select 
	                login,
	                name,
	                email,
	                cpf,
	                '' as password
                from 
	                SGQ_Users
                where 
	                login = '@login' and 
	                password = '@password'
	                --login = 'TR412061' and 
	                --password = ''
                ";

            sql = sql.Replace("@login", login);
            sql = sql.Replace("@password", password);

            var list = _connection.Executar<User>(sql);

            return list[0];
        }

    }
}