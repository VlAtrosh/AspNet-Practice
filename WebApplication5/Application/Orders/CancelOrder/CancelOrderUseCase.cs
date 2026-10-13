using WebApplication3.Application.Common;
using WebApplication3.Application.Common.Abstractions;

namespace WebApplication3.Application.Orders.CancelOrder
{
    public sealed class CancelOrderUseCase
    {
        private readonly IOrderRepository _orders;
        private readonly IUnitOfWork _uow;

        public CancelOrderUseCase(IOrderRepository orders, IUnitOfWork uow)
        {
            _orders = orders;
            _uow = uow;
        }

        public async Task<Result<bool>> ExecuteAsync(CancelOrderCommand cmd, CancellationToken ct)
        {
            var order = await _orders.GetAsync(cmd.OrderId, ct);
            if (order is null)
                return Error.NotFound("order.not_found", "Заказ не найден");

            order.Cancel();
            await _uow.SaveChangesAsync(ct);

            return true;
        }
    }
}