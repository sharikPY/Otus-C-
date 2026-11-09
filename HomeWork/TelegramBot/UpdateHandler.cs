using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastPekar.Core.Entities;    
using FastPekar.Core.Services;     
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;


namespace FastPekar.TelegramBot
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

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
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

            var user = await _userService.GetUserAsync(telegramUserId, ct);

            // Если пользователь не зарегистрирован — доступны только /help /info
            if (user == null && command != "/start" && command != "/help" && command != "/info")
            {
                await botClient.SendMessage(chat, "Сначала зарегистрируйтесь командой /start.", ct);
                return;
            }

            switch (command)
            {
                case "/start":
                    await HandleStart(botClient, chat, telegramUserId, telegramUserName, ct);
                    break;
                case "/help":
                    await HandleHelp(botClient, chat, user, ct);
                    break;
                case "/info":
                    await HandleInfo(botClient, chat, ct);
                    break;
                case "/addtask":
                    await HandleAddTask(botClient, chat, user!, argument, ct);
                    break;
                case "/showtasks":
                    await HandleShowTasks(botClient, chat, user!, ct);
                    break;
                case "/showalltasks":
                    await HandleShowAllTasks(botClient, chat, user!, ct);
                    break;
                case "/completetask":
                    await HandleCompleteTask(botClient, chat, argument, ct);
                    break;
                case "/removetask":
                    await HandleRemoveTask(botClient, chat, argument, ct);
                    break;
                case "/report":
                    await HandleReport(botClient, chat, user!, ct);
                    break;
                case "/find":
                    await HandleFind(botClient, chat, user!, argument, ct);
                    break;
                case "/exit":
                    await botClient.SendMessage(chat, user != null
                        ? $"До свидания, {user.TelegramUserName}!"
                        : "До свидания, незнакомец!", ct);
                    break;
                default:
                    await botClient.SendMessage(chat, "Неизвестная команда. Введите /help для справки.", ct);
                    break;
            }
        }

        private async Task HandleStart(ITelegramBotClient botClient, Chat chat,
                                       long telegramUserId, string? telegramUserName, CancellationToken ct)
        {
            var existing = await _userService.GetUserAsync(telegramUserId, ct);
            if (existing != null)
            {
                await botClient.SendMessage(chat,
                    $"Вы уже зарегистрированы, {existing.TelegramUserName}!", ct);
                return;
            }

            var user = await _userService.RegisterUserAsync(
                telegramUserId,
                string.IsNullOrWhiteSpace(telegramUserName) ? "Незнакомец" : telegramUserName, ct);

            await botClient.SendMessage(chat,
                $"Приятно познакомиться, {user.TelegramUserName}!\n" +
                $"Ваш UserId: {user.UserId}\n" +
                $"Дата регистрации (UTC): {user.RegisteredAt:yyyy-MM-dd HH:mm:ss}", ct);
        }

        private async Task HandleHelp(ITelegramBotClient botClient, Chat chat, ToDoUser? user, CancellationToken ct)
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
            await botClient.SendMessage(chat, string.Join("\n", lines), ct);
        }

        private async Task HandleInfo(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {
            await botClient.SendMessage(chat,
                "Приложение: Fast Пекарь\n" +
                "Описание:   Асинхронная система управления доставкой\n" +
                "            выпечки и горячей еды\n" +
                "Версия:     1.0.0\n" +
                "Дата создания: 26.09.2026", ct);
        }

        private async Task HandleAddTask(ITelegramBotClient botClient, Chat chat,
                                         ToDoUser user, string argument, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                await botClient.SendMessage(chat,
                    "Укажите имя задачи. Пример: /addtask Новая задача", ct);
                return;
            }

            var item = await _toDoService.AddAsync(user, argument, ct);
            await botClient.SendMessage(chat,
                $"Задача добавлена: \"{item.Name}\" (Id: {item.Id})", ct);
        }

        private async Task HandleShowTasks(ITelegramBotClient botClient, Chat chat, ToDoUser user, CancellationToken ct)
        {
            var activeTasks = await _toDoService.GetActiveByUserIdAsync(user.UserId, ct);

            if (activeTasks.Count == 0)
            {
                await botClient.SendMessage(chat, "Список активных задач пуст.", ct);
                return;
            }

            var lines = activeTasks.Select((item, i) =>
                $"  {i + 1}. {item.Name} - {item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}");

            await botClient.SendMessage(chat,
                "Список активных задач:\n" + string.Join("\n", lines), ct);
        }

        private async Task HandleShowAllTasks(ITelegramBotClient botClient, Chat chat, ToDoUser user, CancellationToken ct)
        {
            var allTasks = await _toDoService.GetAllByUserIdAsync(user.UserId, ct);

            if (allTasks.Count == 0)
            {
                await botClient.SendMessage(chat, "Список задач пуст.", ct);
                return;
            }

            var lines = allTasks.Select((item, i) =>
                $"  {i + 1}. ({item.State}) {item.Name} - " +
                $"{item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}");

            await botClient.SendMessage(chat,
                "Список всех задач:\n" + string.Join("\n", lines), ct);
        }

        private async Task HandleCompleteTask(ITelegramBotClient botClient, Chat chat, string argument, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(argument) || !Guid.TryParse(argument.Trim(), out Guid id))
            {
                await botClient.SendMessage(chat,
                    "Укажите корректный Id задачи. " +
                    "Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e", ct);
                return;
            }

            await _toDoService.MarkCompletedAsync(id, ct);
            await botClient.SendMessage(chat, $"Задача с Id {id} отмечена как выполненная (если была найдена).", ct);
        }

        private async Task HandleRemoveTask(ITelegramBotClient botClient, Chat chat, string argument, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(argument) || !Guid.TryParse(argument.Trim(), out Guid id))
            {
                await botClient.SendMessage(chat,
                    "Укажите корректный Id задачи. " +
                    "Пример: /removetask ffbfe448-4b39-4778-98aa-1aed98f7eed8", ct);
                return;
            }

            await _toDoService.DeleteAsync(id, ct);
            await botClient.SendMessage(chat, $"Задача с Id {id} удалена (если была найдена).", ct);
        }
        private async Task HandleReport(ITelegramBotClient botClient, Chat chat, ToDoUser user, CancellationToken ct)
        {
            var (total, completed, active, generatedAt) = await _reportService.GetUserStatsAsync(user.UserId, ct);

            await botClient.SendMessage(chat,
                $"Статистика по задачам на {generatedAt:dd.MM.yyyy HH:mm:ss}. " +
                $"Всего: {total}; Завершенных: {completed}; Активных: {active};", ct);
        }
        private async Task HandleFind(ITelegramBotClient botClient, Chat chat, ToDoUser user, string argument, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                await botClient.SendMessage(chat, "Укажите префикс имени. Пример: /find Купить", ct);
                return;
            }

            var found = await _toDoService.FindAsync(user, argument, ct);

            if (found.Count == 0)
            {
                await botClient.SendMessage(chat, $"Задачи, начинающиеся на \"{argument}\", не найдены.", ct);
                return;
            }

            var lines = found.Select((item, i) =>
                $"  {i + 1}. {item.Name} - {item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}");

            await botClient.SendMessage(chat,
                 $"Найденные задачи по префиксу \"{argument}\":\n" + string.Join("\n", lines), ct);
        }
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            Console.WriteLine("=== Ошибка при обработке обновления ===");
            Console.WriteLine($"Type:           {exception.GetType().FullName}");
            Console.WriteLine($"Message:        {exception.Message}");
            Console.WriteLine($"StackTrace:     {exception.StackTrace}");
            Console.WriteLine($"InnerException: {exception.InnerException?.Message ?? "нет"}");
            return Task.CompletedTask;
        }
    }
}