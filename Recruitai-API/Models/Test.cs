using System;
using System.Collections.Generic;

namespace Recruitai_API.Models;

public partial class Test
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;
}
