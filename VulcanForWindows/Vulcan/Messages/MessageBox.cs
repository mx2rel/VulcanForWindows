using System;
using Vulcanova.Core.Data;

namespace Vulcanova.Features.Messages;

public class MessageBox
{
    public AccountEntityId Id { get; set; }
    public Guid GlobalKey { get; set; }
    public string Name { get; set; }
    public bool IsSelected { get; set; }
}