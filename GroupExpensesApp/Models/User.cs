using System;
using System.Collections.Generic;

namespace GroupExpensesApp.Models;

public class User
{
    protected readonly Guid Id = Guid.NewGuid();

    public string Name { get; set; }

    public List<Group> Groups { get; set; } = new();

}
