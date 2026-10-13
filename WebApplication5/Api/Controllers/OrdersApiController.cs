using WebApplication3.Api.Contracts;
using WebApplication3.Application.Orders.CreateOrder;
using WebApplication3.Application.Orders.GetOrder;
using WebApplication3.Application.Orders.CancelOrder;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.Api.Extensions;
using System.Security.Cryptography;
using System.Text;

namespace WebApplication3.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersApiController : ControllerBase
    {
        private readonly CreateOrderUseCase _createOrder;
        private readonly GetOrderUseCase _getOrder;
        private readonly CancelOrderUseCase _cancelOrder;

        public OrdersApiController(
            CreateOrderUseCase createOrder,
            GetOrderUseCase getOrder,
            CancelOrderUseCase cancelOrder)
        {
            _createOrder = createOrder;
            _getOrder = getOrder;
            _cancelOrder = cancelOrder;
        }

        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        {
            var command = new CreateOrderCommand(
                request.CustomerEmail,
                request.Items.Select(i => new CreateOrderItem(i.ProductId, i.Quantity)).ToList());

            var result = await _createOrder.ExecuteAsync(command, ct);

            return result.ToApiResult(this, id =>
                CreatedAtAction(nameof(GetById), new { id }, new { id }));
        }

        // ✅ GET с ETag
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _getOrder.ExecuteAsync(new GetOrderQuery(id), ct);
            if (!result.IsSuccess)
                return NotFound();

            var dto = result.Value!;
            var etag = GenerateETag(dto);

            // Проверяем If-None-Match
            var ifNoneMatch = Request.Headers["If-None-Match"].ToString().Trim().Trim('"');
            var etagClean = etag.Trim('"');

            if (!string.IsNullOrEmpty(ifNoneMatch) && ifNoneMatch == etagClean)
            {
                Response.Headers["ETag"] = etag;
                return StatusCode(StatusCodes.Status304NotModified);
            }

            Response.Headers["ETag"] = etag;

            return Ok(new OrderResponse(
                dto.Id,
                dto.CustomerEmail,
                dto.Total,
                dto.Status,
                dto.Items.Select(i => new OrderItemResponse(i.ProductId, i.Name, i.Price, i.Quantity)).ToList()));
        }

        // ✅ Cancel с If-Match (оптимистичная блокировка)
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        {
            // 1. Загружаем текущий заказ
            var getResult = await _getOrder.ExecuteAsync(new GetOrderQuery(id), ct);
            if (!getResult.IsSuccess)
                return NotFound();

            var dto = getResult.Value!;
            var currentETag = GenerateETag(dto);
            var currentClean = currentETag.Trim('"');

            // 2. Проверяем If-Match
            var ifMatch = Request.Headers["If-Match"].ToString().Trim().Trim('"');

            if (string.IsNullOrEmpty(ifMatch))
            {
                return StatusCode(StatusCodes.Status428PreconditionRequired, new
                {
                    error = "Требуется заголовок If-Match",
                    currentETag
                });
            }

            if (ifMatch != currentClean)
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed, new
                {
                    error = "Документ был изменён другим клиентом",
                    currentETag,
                    yourETag = ifMatch
                });
            }

            // 3. Версии совпали — отменяем
            var result = await _cancelOrder.ExecuteAsync(new CancelOrderCommand(id), ct);

            Response.Headers["ETag"] = currentETag;
            return result.ToApiResult(this, _ => NoContent());
        }

        /// <summary>
        /// Генерирует ETag из данных заказа: Id + Status + Total + кол-во позиций.
        /// </summary>
        private static string GenerateETag(OrderDto dto)
        {
            var raw = $"{dto.Id}|{dto.Status}|{dto.Total}|{dto.Items.Count}";
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hash = sha.ComputeHash(bytes);
            return $"\"{Convert.ToHexString(hash)}\"";
        }
    }
}