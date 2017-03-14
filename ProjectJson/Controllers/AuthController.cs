using ProjectJson.DAOs;
using ProjectJson.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class AuthController : ApiController
    {
        [HttpGet]
        [Route("users")]
        public IList<User> GetUsers()
        {
            var userDAO = new UserDAO();
            var user = userDAO.getUsers();
            userDAO.Dispose();
            return user;
        }

        [HttpGet]
        [Route("userByCpf/{login}/{cpf}")]
        public User GetUserByCpf(string login, string cpf) {
            var userDAO = new UserDAO();
            var user = userDAO.getUserByCpf(login, cpf);
            userDAO.Dispose();
            return user;
        }

        [HttpGet]
        [Route("userByPassword/{login}/{password}")]
        public User GetUserByPassword(string login, string password)
        {
            var userDAO = new UserDAO();
            var user = userDAO.getUserByPassword(login, password);
            userDAO.Dispose();
            return user;
        }
    }
}