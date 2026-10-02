using Microsoft.AspNetCore.Mvc;
using WebApplication3.Application.Orders.CreateOrder;
using WebApplication3.Application.Orders.GetOrder;
using WebApplication3.Web.Extensions;
using WebApplication3.Web.Models;

namespace WebApplication3.Web.Controllers
{




    public class OrdersController : Controller
    {
        private readonly CreateOrderUseCase _createOrder;
        private readonly GetOrderUseCase _getOrder;   // ← НОВОЕ

        public OrdersController(
            CreateOrderUseCase createOrder,
            GetOrderUseCase getOrder)                  // ← НОВОЕ
        {
            _createOrder = createOrder;
            _getOrder = getOrder;
        }

        [HttpGet]
        public IActionResult Create() => View(new CreateOrderViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // ViewModel → Command
            var command = new CreateOrderCommand(
                vm.CustomerEmail,
                vm.Items.Select(i => new CreateOrderItem(i.ProductId, i.Quantity)).ToList());

            // Use Case
            var result = await _createOrder.ExecuteAsync(command, ct);

 
            if (result.IsSuccess)
                return RedirectToAction(nameof(Details), new { id = result.Value });


            ModelState.AddModelError(string.Empty, result.Error!.Message);
            return View(vm);
        }

        [HttpGet]
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken ct)
        {
            var result = await _getOrder.ExecuteAsync(new GetOrderQuery(id), ct);

            // ✅ Отладка
            Console.WriteLine($"[DETAILS] id={id}, IsSuccess={result.IsSuccess}");
            if (!result.IsSuccess)
            {
                Console.WriteLine($"[DETAILS] Error: {result.Error?.Code} - {result.Error?.Message}");
                return NotFound();
            }

            var dto = result.Value!;
            Console.WriteLine($"[DETAILS] Order loaded: {dto.Id}, Items count = {dto.Items.Count}");

            var vm = new OrderDetailsViewModel
            {
                Id = dto.Id,
                CustomerEmail = dto.CustomerEmail,
                Status = "Created",
                Total = dto.Total,
                CreatedAtUtc = DateTime.UtcNow,
                CanBeCancelled = true,
                Items = dto.Items.Select(i => new OrderItemViewModel
                {
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return View(vm);
        }
    }
}
