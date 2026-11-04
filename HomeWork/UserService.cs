using System.Collections.Generic;
using System.Linq;

namespace FastPekar
{
    public class UserService : IUserService
    {
        private readonly List<ToDoUser> _users = new();

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var user = new ToDoUser(telegramUserId, telegramUserName);
            _users.Add(user);
            return user;
        }

        public ToDoUser? GetUser(long telegramUserId)
        {
            return _users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
        }
    }
}