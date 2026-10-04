using System;
using System.Collections.Generic;
using System.Linq;

namespace FastPekar
{
    class Program
    {
        // Список задач — доступен всем методам класса
        static List<string> tasks = new List<string>();

        // Лимиты, задаются при старте
        static int maxTaskCount;
        static int maxTaskLength;

        static void Main(string[] args)
        {
            // Инициализация лимитов ДО основного цикла —
            // её ошибки обрабатываются отдельным try/catch
            try
            {
                maxTaskCount = ReadLimit(
                    "Введите максимально допустимое количество задач",
                    min: 1, max: 100);

                maxTaskLength = ReadLimit(
                    "Введите максимально допустимую длину задачи",
                    min: 1, max: 100);
            }
            catch (ArgumentException ex)
            {
                // Если лимиты заданы неверно — работать дальше нельзя
                Console.WriteLine(ex.Message);
                return;
            }

            PrintWelcomeMessage();
            string userName = null;

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
                        case "/start": HandleStart(ref userName); break;
                        case "/help": HandleHelp(userName); break;
                        case "/info": HandleInfo(); break;
                        case "/echo": HandleEcho(ref userName, argument); break;
                        case "/addtask": HandleAddTask(); break;
                        case "/showtasks": HandleShowTasks(); break;
                        case "/removetask": HandleRemoveTask(); break;
                        case "/exit": HandleExit(userName); return;

                        default:
                            Console.WriteLine("Неизвестная команда. Введите /help для справки.");
                            break;
                    }
                }
                catch (ArgumentException ex)
                {
                    // Только сообщение — для ошибок валидации
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
                    // Глобальный перехват — печатаем всё, что есть
                    Console.WriteLine("Произошла непредвиденная ошибка:");
                    Console.WriteLine($"  Type:           {ex.GetType().FullName}");
                    Console.WriteLine($"  Message:        {ex.Message}");
                    Console.WriteLine($"  StackTrace:     {ex.StackTrace}");
                    Console.WriteLine($"  InnerException: {ex.InnerException?.Message ?? "нет"}");
                }
            }
        }

        // ================= ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =================

        /// <summary>
        /// Читает строку, парсит в int и проверяет диапазон [min, max].
        /// Бросает ArgumentException при любой ошибке.
        /// </summary>
        static int ParseAndValidateInt(string str, int min, int max)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Введено пустое значение. Ожидалось число.");
            }

            if (!int.TryParse(str.Trim(), out int value))
            {
                throw new ArgumentException($"Значение '{str}' не является числом.");
            }

            if (value < min || value > max)
            {
                throw new ArgumentException(
                    $"Значение {value} должно быть в диапазоне от {min} до {max}.");
            }

            return value;
        }

        /// <summary>
        /// Проверяет, что строка не null, не пустая и не состоит из пробелов.
        /// Бросает ArgumentException в противном случае.
        /// </summary>
        static void ValidateString(string str)
        {
            if (str == null)
            {
                throw new ArgumentException("Строка не может быть null.");
            }

            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Строка не может быть пустой или состоять из пробелов.");
            }
        }

        /// <summary>
        /// Универсальное чтение лимита с консоли через ParseAndValidateInt.
        /// </summary>
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
                    // повторяем ввод, пока пользователь не введёт корректное значение
                }
            }
        }

        // ================= ПРИВЕТСТВИЕ И СПРАВКА =================

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
            Console.WriteLine("  /showtasks  - показать все задачи");
            Console.WriteLine("  /removetask - удалить задачу по номеру");
            Console.WriteLine("  /exit       - выйти из пекарни Fast Пекарь");
            Console.WriteLine();
            Console.WriteLine("Для начала работы введите /start.");
            Console.WriteLine("============================================================");
        }

        static void HandleHelp(string userName)
        {
            string greeting = userName != null ? $"{userName}, " : "";

            Console.WriteLine($"{greeting}справка по использованию Fast Пекарь:");
            Console.WriteLine();
            Console.WriteLine("  /start      - ввести имя и кинуть салам пекарям");
            Console.WriteLine("  /help       - показать справочку по плюшкам");
            Console.WriteLine("  /info       - информация о пекарне");
            Console.WriteLine("  /addtask    - добавить новую задачу в список");
            Console.WriteLine("  /showtasks  - показать список всех задач");
            Console.WriteLine("  /removetask - удалить задачу по её номеру");
            Console.WriteLine("  /exit       - выйти из пекарни");
            if (userName == null)
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

        // ================= СТАРЫЕ КОМАНДЫ =================

        static void HandleStart(ref string userName)
        {
            userName = AskUserName();
            Console.WriteLine($"Приятно познакомиться, {userName}!");
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

        static void HandleEcho(ref string userName, string argument)
        {
            if (userName == null)
            {
                Console.WriteLine("Неприлично сразу что-то требовать. Давайте познакомимся.");

                userName = AskUserName();
                Console.WriteLine($"Приятно познакомиться, {userName}!");

                if (!string.IsNullOrWhiteSpace(argument))
                {
                    Console.WriteLine($"{userName}, ваш текст: {argument}");
                }
                else
                {
                    Console.WriteLine("Теперь вы можете использовать команду: /echo <текст>");
                    Console.WriteLine($"Например: /echo Привет, {userName}!");
                }
                return;
            }

            if (string.IsNullOrWhiteSpace(argument))
            {
                Console.WriteLine($"{userName}, Про плюшки то напиши. Пример: /echo вкусные лавашики");
                return;
            }

            Console.WriteLine($"{userName}, ваш текст: {argument}");
        }

        // ================= НОВЫЕ КОМАНДЫ =================

        static void HandleAddTask()
        {
            // 1. Проверка лимита количества
            if (tasks.Count >= maxTaskCount)
            {
                throw new TaskCountLimitException(maxTaskCount);
            }

            // 2. Чтение и валидация описания
            Console.Write("Введите описание задачи: ");
            string description = Console.ReadLine();

            ValidateString(description);           // бросит ArgumentException, если пусто
            string trimmed = description.Trim();

            // 3. Проверка длины
            if (trimmed.Length > maxTaskLength)
            {
                throw new TaskLengthLimitException(trimmed.Length, maxTaskLength);
            }

            // 4. Проверка на дубликат
            if (tasks.Contains(trimmed))
            {
                throw new DuplicateTaskException(trimmed);
            }

            tasks.Add(trimmed);
            Console.WriteLine($"Задача добавлена: \"{trimmed}\"");
        }

        static void HandleShowTasks()
        {
            if (tasks.Count == 0)
            {
                Console.WriteLine("Список задач пуст.");
                return;
            }

            Console.WriteLine("Список задач:");
            for (int i = 0; i < tasks.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {tasks[i]}");
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
                Console.WriteLine($"  {i + 1}. {tasks[i]}");
            }

            while (true)
            {
                Console.Write("Введите номер задачи для удаления (или 'отмена'): ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Ошибка: введите номер задачи.");
                    continue;
                }

                string trimmed = input.Trim();

                if (trimmed.ToLower() == "отмена")
                {
                    Console.WriteLine("Удаление отменено.");
                    return;
                }

                try
                {
                    int number = ParseAndValidateInt(trimmed, 1, tasks.Count);
                    string removed = tasks[number - 1];
                    tasks.RemoveAt(number - 1);
                    Console.WriteLine($"Задача удалена: \"{removed}\"");
                    return;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    // продолжаем цикл — снова просим номер
                }
            }
        }

        static void HandleExit(string userName)
        {
            if (userName != null)
            {
                Console.WriteLine($"До свидания, {userName}! Спасибо за использование Fast Пекарь.");
            }
            else
            {
                Console.WriteLine("До свидания, незнакомец! С радостью ждем знакомства в Fast Пекарь.");
            }
        }
    }
}