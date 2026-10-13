using System;
using System.Collections.Generic;
using System.Linq;

namespace FastPekar
{
    class Program
    {
        public const int MinTaskCount = 1;
        public const int MaxTaskCount = 100;
        public const int MinTaskLength = 1;
        public const int MaxTaskLength = 100;
        static readonly List<ToDoItem> tasks = new List<ToDoItem>();
        static int maxTaskCount;
        static int maxTaskLength;

        static void Main(string[] args)
        {
            try
            {
                maxTaskCount = ReadLimit(
                    "Введите максимально допустимое количество задач",
                    min: MinTaskCount, max: MaxTaskCount);

                maxTaskLength = ReadLimit(
                    "Введите максимально допустимую длину задачи",
                    min: MinTaskCount, max: MaxTaskCount);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            PrintWelcomeMessage();
            ToDoUser user = null;

            while (true)
            {
                try
                {
                    Console.Write("\n> ");
                    string input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        Console.WriteLine("Пожалуйста, введите команду.");
                        continue;
                    }

                    string trimmedInput = input.Trim();
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

                    switch (command)
                    {
                        case "/start": HandleStart(ref user); break;
                        case "/help": HandleHelp(user); break;
                        case "/info": HandleInfo(); break;
                        case "/echo": HandleEcho(ref user, argument); break;
                        case "/addtask": HandleAddTask(user); break;
                        case "/showtasks": HandleShowTasks(); break;
                        case "/showalltasks": HandleShowAllTasks(); break;
                        case "/completetask": HandleCompleteTask(argument); break;
                        case "/removetask": HandleRemoveTask(); break;
                        case "/exit": HandleExit(user); return;

                        default:
                            Console.WriteLine("Неизвестная команда. Введите /help для справки.");
                            break;
                    }
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (TaskCountLimitException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (TaskLengthLimitException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (DuplicateTaskException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Произошла непредвиденная ошибка:");
                    Console.WriteLine($"  Type:           {ex.GetType().FullName}");
                    Console.WriteLine($"  Message:        {ex.Message}");
                    Console.WriteLine($"  StackTrace:     {ex.StackTrace}");
                    Console.WriteLine($"  InnerException: {ex.InnerException?.Message ?? "нет"}");
                }
            }
        }

        public static int ParseAndValidateInt(string str, int min, int max)
        {
            if (str == null)
            {
                throw new ArgumentException(
                    "Введено null-значение вместо числа. Ожидалась строка с целым числом.");
            }

            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException(
                    "Введено пустое значение или строка из пробелов. " +
                    $"Введите целое число в диапазоне от {min} до {max}.");
            }

            if (!int.TryParse(str.Trim(), out int value))
            {
                throw new ArgumentException(
                    $"Значение '{str.Trim()}' не является целым числом. " +
                    $"Введите целое число в диапазоне от {min} до {max}.");
            }

            if (value < min || value > max)
            {
                throw new ArgumentException(
                    $"Значение {value} выходит за допустимые границы. " +
                    $"Допустимый диапазон: от {min} до {max}.");
            }

            return value;
        }

        public static void ValidateString(string str)
        {
            if (str == null)
            {
                throw new ArgumentException(
                    "Передано null-значение. Ожидалась непустая строка.");
            }

            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException(
                    "Строка пуста или состоит только из пробелов. " +
                    "Введите осмысленное значение.");
            }
        }

        static int ReadLimit(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                string input = Console.ReadLine();
                try
                {
                    return ParseAndValidateInt(input, min, max);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }


        static void PrintWelcomeMessage()
        {
            Console.WriteLine("============================================================");
            Console.WriteLine("  Fast Пекарь");
            Console.WriteLine("  Асинхронная система управления доставкой выпечки");
            Console.WriteLine("  и горячей еды");
            Console.WriteLine("============================================================");
            Console.WriteLine();
            Console.WriteLine("Добро пожаловать в Fast Пекарь!");
            Console.WriteLine($"Максимум задач: {maxTaskCount}, максимальная длина задачи: {maxTaskLength}.");
            Console.WriteLine();
            Console.WriteLine("Доступные команды:");
            Console.WriteLine();
            Console.WriteLine("  /start      - войти в пекарню Fast Пекарь");
            Console.WriteLine("  /help       - справка по плюшкам");
            Console.WriteLine("  /info       - информация о пекарне");
            Console.WriteLine("  /addtask    - добавить новую задачу");
            Console.WriteLine("  /showtasks  - показать все активные задачи");
            Console.WriteLine("  /showalltasks - показать все задачи (любой State)");
            Console.WriteLine("  /completetask - отметить задачу выполненной по Id");
            Console.WriteLine("  /removetask - удалить задачу по номеру");
            Console.WriteLine("  /exit       - выйти из пекарни Fast Пекарь");
            Console.WriteLine();
            Console.WriteLine("Для начала работы введите /start.");
            Console.WriteLine("============================================================");
        }

        static void HandleHelp(ToDoUser user)
        {
            string greeting = user != null ? $"{user.TelegramUserName}, " : "";

            Console.WriteLine($"{greeting}справка по использованию Fast Пекарь:");
            Console.WriteLine();
            Console.WriteLine("  /start      - ввести имя и кинуть салам пекарям");
            Console.WriteLine("  /help       - показать справочку по плюшкам");
            Console.WriteLine("  /info       - информация о пекарне");
            Console.WriteLine("  /addtask    - добавить новую задачу в список");
            Console.WriteLine("  /showtasks  - показать список активных задач");
            Console.WriteLine("  /showalltasks - показать список всех задач");
            Console.WriteLine("  /completetask - отметить задачу выполненной по Id");
            Console.WriteLine("  /removetask - удалить задачу по её номеру");
            Console.WriteLine("  /exit       - выйти из пекарни");
            if (user == null)
            {
                Console.WriteLine();
                Console.WriteLine("Для того чтобы поздороваться с пекарями и разблочить команду /echo напишите /start.");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("  /echo <текст> - повторить введённый текст");
                Console.WriteLine("                  пример: /echo Hello");
            }
        }

        static void HandleInfo()
        {
            Console.WriteLine("Приложение: Fast Пекарь");
            Console.WriteLine("Описание:   Асинхронная система управления доставкой");
            Console.WriteLine("            выпечки и горячей еды");
            Console.WriteLine("Версия:     1.0.0");
            Console.WriteLine("Дата создания: 26.09.2026");
        }

        static void HandleStart(ref ToDoUser user)
        {
            string telegramUserName = AskUserName();
            user = new ToDoUser(telegramUserName);

            Console.WriteLine($"Приятно познакомиться, {user.TelegramUserName}!");
            Console.WriteLine($"Ваш UserId: {user.UserId}");
            Console.WriteLine($"Дата регистрации (UTC): {user.RegisteredAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("Теперь вам доступна команда /echo.");
        }

        static string AskUserName()
        {
            while (true)
            {
                Console.Write("Введите ваше имя: ");
                string input = Console.ReadLine();

                try
                {
                    ValidateString(input);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ошибка ввода: {ex.Message} Попробуйте снова.");
                    continue;
                }

                string trimmed = input.Trim();

                if (trimmed.Any(char.IsDigit))
                {
                    Console.WriteLine("Ошибка ввода: имя не должно содержать цифр. Попробуйте снова.");
                    continue;
                }

                return trimmed;
            }
        }

        static void HandleEcho(ref ToDoUser user, string argument)
        {
            if (user == null)
            {
                Console.WriteLine("Неприлично сразу что-то требовать. Давайте познакомимся.");

                string telegramUserName = AskUserName();
                user = new ToDoUser(telegramUserName);

                Console.WriteLine($"Приятно познакомиться, {user.TelegramUserName}!");

                if (!string.IsNullOrWhiteSpace(argument))
                {
                    Console.WriteLine($"{user.TelegramUserName}, ваш текст: {argument}");
                }
                else
                {
                    Console.WriteLine("Теперь вы можете использовать команду: /echo <текст>");
                    Console.WriteLine($"Например: /echo Привет, {user.TelegramUserName}!");
                }
                return;
            }

            if (string.IsNullOrWhiteSpace(argument))
            {
                Console.WriteLine($"{user.TelegramUserName}, Про плюшки то напиши. Пример: /echo вкусные лавашики");
                return;
            }

            Console.WriteLine($"{user.TelegramUserName}, ваш текст: {argument}");
        }


        static void HandleAddTask(ToDoUser user)
        {
            if (user == null)
            {
                Console.WriteLine("Сначала войдите в пекарню командой /start.");
                return;
            }

            if (tasks.Count >= maxTaskCount)
            {
                throw new TaskCountLimitException(maxTaskCount);
            }

            Console.Write("Введите описание задачи: ");
            string description = Console.ReadLine();

            ValidateString(description);           // бросит ArgumentException, если пусто
            string trimmed = description.Trim();

            if (trimmed.Length > maxTaskLength)
            {
                throw new TaskLengthLimitException(trimmed.Length, maxTaskLength);
            }

            if (tasks.Any(t => t.Name == trimmed))
            {
                throw new DuplicateTaskException(trimmed);
            }

            var item = new ToDoItem(user, trimmed);
            tasks.Add(item);
            Console.WriteLine($"Задача добавлена: \"{item.Name}\" (Id: {item.Id})");
        }

        static void HandleShowTasks()
        {
            var activeTasks = tasks.Where(t => t.State == ToDoItemState.Active).ToList();

            if (activeTasks.Count == 0)
            {
                Console.WriteLine("Список активных задач пуст.");
                return;
            }

            Console.WriteLine("Список активных задач:");
            for (int i = 0; i < activeTasks.Count; i++)
            {
                var item = activeTasks[i];
                Console.WriteLine(
                    $"  {i + 1}. {item.Name} - {item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}");
            }
        }

        static void HandleShowAllTasks()
        {
            if (tasks.Count == 0)
            {
                Console.WriteLine("Список задач пуст.");
                return;
            }

            Console.WriteLine("Список всех задач:");
            for (int i = 0; i < tasks.Count; i++)
            {
                var item = tasks[i];
                Console.WriteLine(
                    $"  {i + 1}. ({item.State}) {item.Name} - " +
                    $"{item.CreatedAt:dd.MM.yyyy HH:mm:ss} - {item.Id}");
            }
        }

        static void HandleCompleteTask(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                Console.WriteLine("Укажите Id задачи. Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e");
                return;
            }

            if (!Guid.TryParse(argument.Trim(), out Guid id))
            {
                Console.WriteLine($"'{argument.Trim()}' не является корректным Guid. " +
                                  "Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e");
                return;
            }

            var item = tasks.FirstOrDefault(t => t.Id == id);

            if (item == null)
            {
                Console.WriteLine($"Задача с Id {id} не найдена.");
                return;
            }

            if (item.State == ToDoItemState.Completed)
            {
                Console.WriteLine($"Задача \"{item.Name}\" уже выполнена ({item.StateChangedAt:dd.MM.yyyy HH:mm:ss}).");
                return;
            }

            item.State = ToDoItemState.Completed;
            item.StateChangedAt = DateTime.UtcNow;

            Console.WriteLine(
                $"Задача \"{item.Name}\" отмечена как выполненная. " +
                $"StateChangedAt: {item.StateChangedAt:dd.MM.yyyy HH:mm:ss} UTC");
        }

        static int AskTaskNumberForRemoval()
        {
            while (true)
            {
                Console.Write(
                    $"Введите номер задачи для удаления (1–{tasks.Count}, " +
                    "Esc — отмена, слово «отмена» — тоже отмена): ");

                var key = Console.ReadKey(intercept: true);

                // Выход по Esc
                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    Console.WriteLine("Удаление отменено.");
                    return -1;
                }

                // Собираем строку: уже прочитанный символ + остаток строки
                string input;
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    input = string.Empty;
                }
                else
                {
                    // Эхо-вывод первого символа и чтение остатка
                    Console.Write(key.KeyChar);
                    input = key.KeyChar + Console.ReadLine();
                }

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Ошибка: введите номер задачи.");
                    continue;
                }

                string trimmed = input.Trim();

                if (trimmed.Equals("отмена", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Удаление отменено.");
                    return -1;
                }

                try
                {
                    int number = ParseAndValidateInt(trimmed, 1, tasks.Count);
                    return number;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        static void HandleRemoveTask()
        {
            if (tasks.Count == 0)
            {
                Console.WriteLine("Список задач пуст. Нечего удалять.");
                return;
            }

            Console.WriteLine("Список задач:");
            for (int i = 0; i < tasks.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {tasks[i].Name}");
            }

            int number = AskTaskNumberForRemoval();
            if (number == -1)
            {
                return;
            }

            var removed = tasks[number - 1];
            tasks.RemoveAt(number - 1);
            Console.WriteLine($"Задача удалена: \"{removed.Name}\" (Id: {removed.Id})");
        }

        static void HandleExit(ToDoUser user)
        {
            if (user != null)
            {
                Console.WriteLine($"До свидания, {user.TelegramUserName}! Спасибо за использование Fast Пекарь.");
            }
            else
            {
                Console.WriteLine("До свидания, незнакомец! С радостью ждем знакомства в Fast Пекарь.");
            }
        }
    }
}