using AurSystem.Framework.Models;
using AurSystem.Framework.Models.Domain;

namespace OrderService.Api.Integrations.Courier.Activities;

public interface OrderLog
{
    Order Order { get; }
    public OrderStatus Status { get; }

}