using FastPekar.Services;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Linq;

namespace FastPekar
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoReportService _reportService;

        public UpdateHandler(IUserService userService, IToDoService toDoService,
                     IToDoReportService reportService)
        {
            _userService = userService;
            _toDoService = toDoService;
            _reportService = reportService;
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            try
            {
                var message = update?.Message;
                if (message == null || string.IsNullOrWhiteSpace(message.Text))
                    return;

                long telegramUserId = message.From.Id;
                string? telegramUserName = message.From.Username;
                var chat = message.Chat;

                string trimmedInput = message.Text.Trim();
                string command;
                string argument;
                int spaceIndex = trimmedInput.IndexOf(' ');

                if (spaceIndex == -1)
                {
                    command = trimmedInput.ToLower();
                    argument = string.Empty;
                }
                else
                {
                    command = trimmedInput.Substring(0, spaceIndex).ToLower();
                    argument = trimmedInput.Substring(spaceIndex + 1).Trim();
                }

                var user = _userService.GetUser(telegramUserId);

                // Если пользователь не зарегистрирован — доступны только /help /info
                if (user == null && command != "/start" && command != "/help" && command != "/info")
                {
                    botClient.SendMessage(chat, "Сначала зарегистрируйтесь командой /start.");
                    return;
                }

                switch (command)
                {
                    case "/start":
                        HandleStart(botClient, chat, telegramUserId, telegramUserName);
                        break;
                    case "/help":
                        HandleHelp(botClient, chat, user);
                        break;
                    case "/info":
                        HandleInfo(botClient, chat);
                        break;
                    case "/addtask":
                        HandleAddTask(botClient, chat, user!, argument);
                        break;
                    case "/showtasks":
                        HandleShowTasks(botClient, chat, user!);
                        break;
                    case "/showalltasks":
                        HandleShowAllTasks(botClient, chat, user!);
                        break;
                    case "/completetask":
                        HandleCompleteTask(botClient, chat, argument);
                        break;
                    case "/removetask":
                        HandleRemoveTask(botClient, chat, argument);
                        break;
                    case "/report":
                        HandleReport(botClient, chat, user!);
                        break;
                    case "/find":
                        HandleFind(botClient, chat, user!, argument);
                        break;
                    case "/exit":
                        botClient.SendMessage(chat, user != null
                            ? $"До свидания, {user.TelegramUserName}!"
                            : "До свидания, незнакомец!");
                        break;
                    
                    default:
                        botClient.SendMessage(chat, "Неизвестная команда. Введите /help для справки.");
                        break;
                }
            }
            catch (Exception ex)
            {
                // Единый catch, как рекомендовано в задании
                // (в предыдущем задании разные catch были учебными)
                var chat = update?.Message?.Chat;
                if (chat != null)
                {
                    botClient.SendMessage(chat, ex.Message);
                }
            }
        }

        private void HandleStart(ITelegramBotClient botClient, Chat chat,
                                 long telegramUserId, string? telegramUserName)
        {
            var existing = _userService.GetUser(telegramUserId);
            if (existing != null)
            {
                botClient.SendMessage(chat,
                    $"Вы уже зарегистрированы, {existing.TelegramUserName}!");
                return;
            }

            var user = _userService.RegisterUser(
                telegramUserId,
                string.IsNullOrWhiteSpace(telegramUserName) ? "Незнакомец" : telegramUserName);

            botClient.SendMessage(chat,
                $"Приятно познакомиться, {user.TelegramUserName}!\n" +
                $"Ваш UserId: {user.UserId}\n" +
                $"Дата регистрации (UTC): {user.RegisteredAt:yyyy-MM-dd HH:mm:ss}");
        }

        private void HandleHelp(ITelegramBotClient botClient, Chat chat, ToDoUser? user)
        {
            string greeting = user != null ? $"{user.TelegramUserName}, " : "";
            var lines = new[]
            {
                $"{greeting}справка по использованию Fast Пекарь:",
                "",
                "  /start        - регистрация",
                "  /help         - справка",
                "  /info         - информация о пекарне",
                "  /addtask <имя> - добавить задачу (пример: /addtask Новая задача)",
                "  /showtasks    - показать активные задачи",
                "  /showalltasks - показать все задачи",
                "  /completetask <Id> - отметить задачу выполненной",
                "  /removetask <Id>   - удалить задачу по Id",
                "  /report       - статистика по задачам",
                "  /find <префикс> - найти задачи по началу имени",
                "  /exit         - выйти"
            };
            botClient.SendMessage(chat, string.Join("\n", lines));
        }

        private void HandleInfo(ITelegramBotClient botClient, Chat chat)
        {
            botClient.SendMessage(chat,
                "Приложение: Fast Пекарь\n" +
                "Описание:   Асинхронная система управления доставкой\n" +
                "            выпечки и горячей еды\n" +
                "Версия:     1.0.0\n" +
                "Дата создания: 26.09.2026");
        }

        private void HandleAddTask(ITelegramBotClient botClient, Chat chat,
                                   ToDoUser user, string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                botClient.SendMessage(chat,
                    "Укажите имя задачи. Пример: /addtask Новая задача");
                return;
            }

            var item = _toDoService.Add(user, argument);
            botClient.SendMessage(chat,
                $"Задача добавлена: \"{item.Name}\" (Id: {item.Id})");
        }

        private void HandleShowTasks(ITelegramBotClient botClient, Chat chat, ToDoUser user)
        {
            var activeTasks = _toDoService.GetActiveByUserId(user.UserId);

            if (activeTasks.Count == 0)
            {
                botClient.SendMessage(chat, "Список активных задач пуст.");
                return;
            }

            var lines = activeTasks.Select((item, i) =>
                $"  {i + 1}. {item.Name} - {item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}");

            botClient.SendMessage(chat,
                "Список активных задач:\n" + string.Join("\n", lines));
        }

        private void HandleShowAllTasks(ITelegramBotClient botClient, Chat chat, ToDoUser user)
        {
            var allTasks = _toDoService.GetAllByUserId(user.UserId);

            if (allTasks.Count == 0)
            {
                botClient.SendMessage(chat, "Список задач пуст.");
                return;
            }

            var lines = allTasks.Select((item, i) =>
                $"  {i + 1}. ({item.State}) {item.Name} - " +
                $"{item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}");

            botClient.SendMessage(chat,
                "Список всех задач:\n" + string.Join("\n", lines));
        }

        private void HandleCompleteTask(ITelegramBotClient botClient, Chat chat, string argument)
        {
            if (string.IsNullOrWhiteSpace(argument) || !Guid.TryParse(argument.Trim(), out Guid id))
            {
                botClient.SendMessage(chat,
                    "Укажите корректный Id задачи. " +
                    "Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e");
                return;
            }

            _toDoService.MarkCompleted(id);
            botClient.SendMessage(chat, $"Задача с Id {id} отмечена как выполненная (если была найдена).");
        }

        private void HandleRemoveTask(ITelegramBotClient botClient, Chat chat, string argument)
        {
            if (string.IsNullOrWhiteSpace(argument) || !Guid.TryParse(argument.Trim(), out Guid id))
            {
                botClient.SendMessage(chat,
                    "Укажите корректный Id задачи. " +
                    "Пример: /removetask ffbfe448-4b39-4778-98aa-1aed98f7eed8");
                return;
            }

            _toDoService.Delete(id);
            botClient.SendMessage(chat, $"Задача с Id {id} удалена (если была найдена).");
        }
        private void HandleReport(ITelegramBotClient botClient, Chat chat, ToDoUser user)
        {
            var (total, completed, active, generatedAt) = _reportService.GetUserStats(user.UserId);

            botClient.SendMessage(chat,
                $"Статистика по задачам на {generatedAt:dd.MM.yyyy HH:mm:ss}. " +
                $"Всего: {total}; Завершенных: {completed}; Активных: {active};");
        }
        private void HandleFind(ITelegramBotClient botClient, Chat chat, ToDoUser user, string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                botClient.SendMessage(chat, "Укажите префикс имени. Пример: /find Купить");
                return;
            }

            var found = _toDoService.Find(user, argument);

            if (found.Count == 0)
            {
                botClient.SendMessage(chat, $"Задачи, начинающиеся на \"{argument}\", не найдены.");
                return;
            }

            var lines = found.Select((item, i) =>
                $"  {i + 1}. {item.Name} - {item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}");

            botClient.SendMessage(chat,
                $"Найденные задачи по префиксу \"{argument}\":\n" + string.Join("\n", lines));
        }
    }
}