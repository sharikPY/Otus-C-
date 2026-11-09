using FastPekar.Core.DataAccess;
using FastPekar.Infrastructure.DataAccess;
using FastPekar.Services;
using Otus.ToDoList.ConsoleBot;
using System;

namespace FastPekar
{
    class Program
    {
        public const int MinTaskCount = 1;
        public const int MaxTaskCount = 100;
        public const int MinTaskLength = 1;
        public const int MaxTaskLength = 100;

        static void Main(string[] args)
        {
            try
            {
                int maxTaskCount = ReadLimit(
                    "Введите максимально допустимое количество задач",
                    min: MinTaskCount, max: MaxTaskCount);

                int maxTaskLength = ReadLimit(
                    "Введите максимально допустимую длину задачи",
                    min: MinTaskLength, max: MaxTaskLength);

                IUserRepository userRepository = new InMemoryUserRepository();
                IToDoRepository toDoRepository = new InMemoryToDoRepository();
                IUserService userService = new UserService(userRepository);
                IToDoService toDoService = new ToDoService(toDoRepository, maxTaskCount, maxTaskLength);
                IToDoReportService reportService = new ToDoReportService(toDoRepository);
                var handler = new UpdateHandler(userService, toDoService, reportService); 
                var botClient = new ConsoleBotClient();
                botClient.StartReceiving(handler);
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


        static int ReadLimit(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                string? input = Console.ReadLine();
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

    }
}