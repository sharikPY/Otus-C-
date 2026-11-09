using System;

namespace FastPekar.Core.Exceptions;

public class DuplicateTaskException : Exception
{
    public DuplicateTaskException(string task)
        : base($"Задача '{task}' уже существует")
    {
    }
}