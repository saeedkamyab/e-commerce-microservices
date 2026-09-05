using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    public Guid UserId { get; }
}
