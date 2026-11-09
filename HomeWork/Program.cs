using System;
using System.Threading;
using FastPekar.Core.DataAccess;
using FastPekar.Core.Services;
using FastPekar.Infrastructure.DataAccess;
using FastPekar.TelegramBot;
using Otus.ToDoList.ConsoleBot;

namespace FastPekar
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            IUserRepository userRepo = new InMemoryUserRepository();
            IToDoRepository todoRepo = new InMemoryToDoRepository();

            IUserService userService = new UserService(userRepo);
            IToDoService todoService = new ToDoService(todoRepo, maxTaskCount: 10, maxTaskLength: 100);
            IToDoReportService reportService = new ToDoReportService(todoRepo);

            var handler = new UpdateHandler(userService, todoService, reportService);
            var botClient = new ConsoleBotClient();

            botClient.StartReceiving(handler, cts.Token);
        }
    }
}