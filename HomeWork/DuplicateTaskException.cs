using System;

namespace FastPekar
{
    public class DuplicateTaskException : Exception
    {
        public DuplicateTaskException(string task)
            : base($"Задача '{task}' уже существует")
        {
        }
    }
}