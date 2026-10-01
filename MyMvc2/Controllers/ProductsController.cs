using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Services;
using MyMvcApp.ViewModels;

namespace MyMvcApp.Controllers;

[Route("api/products")]
public class ProductsController : Controller
{
    private readonly IProductRepository _repo;

    public ProductsController(IProductRepository repo) => _repo = repo;

    // GET /api/products?category=Мебель
    [HttpGet("")]
    public async Task<IActionResult> List([FromQuery] string? category, CancellationToken ct)
    {
        var items = string.IsNullOrWhiteSpace(category)
            ? await _repo.GetAllAsync(ct)
            : await _repo.GetByCategoryAsync(category, ct);

        var dtos = items.Select(ToDto);
        return Json(dtos);
    }

    // GET /api/products/3 — с ETag и If-None-Match
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(id, ct);
        if (product is null)
            return NotFound(new { error = "Product not found", id });

        var dto = ToDto(product);
        var etag = GenerateETag(dto);
        var ifNoneMatch = Request.Headers["If-None-Match"].ToString().Trim();

        var ifNoneMatchClean = ifNoneMatch.Trim('"');
        var etagClean = etag.Trim('"');

        if (!string.IsNullOrEmpty(ifNoneMatchClean) && ifNoneMatchClean == etagClean)
        {
            Response.Headers["ETag"] = etag;
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers["ETag"] = etag;
        return Json(dto);
    }

    // PUT /api/products/3 — с If-Match (оптимистичная блокировка)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ProductDto request,
        CancellationToken ct)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null)
            return NotFound(new { error = "Product not found", id });

        var currentDto = ToDto(existing);
        var currentEtag = GenerateETag(currentDto);

        var ifMatch = Request.Headers["If-Match"].ToString().Trim().Trim('"');
        var currentEtagClean = currentEtag.Trim('"');

        if (string.IsNullOrEmpty(ifMatch))
            return StatusCode(StatusCodes.Status428PreconditionRequired,
                new { error = "If-Match header is required for PUT" });

        if (ifMatch != currentEtagClean)
            return StatusCode(StatusCodes.Status412PreconditionFailed,
                new { error = "ETag mismatch", expected = currentEtag });

        var updated = new Models.Product
        {
            Id = id,
            Name = request.Name,
            Price = request.Price,
            Category = request.Category
        };

        var result = await _repo.UpdateAsync(updated, ct);
        if (result is null)
            return NotFound(new { error = "Product not found", id });

        var newDto = ToDto(result);
        var newEtag = GenerateETag(newDto);
        Response.Headers["ETag"] = newEtag;

        return Json(newDto);
    }

    // DELETE /api/products/3
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _repo.DeleteAsync(id, ct);
        if (!deleted)
            return NotFound(new { error = "Product not found", id });

        return NoContent();  // 204
    }

    // GET /api/products/throw
    [HttpGet("throw")]
    public IActionResult Throw()
    {
        throw new InvalidOperationException("Тестовое исключение для проверки GlobalExceptionHandler");
    }

    private static ProductDto ToDto(Models.Product p)
        => new(p.Id, p.Name, p.Price, p.Category);

    private static string GenerateETag(ProductDto dto)
    {
        var raw = $"{dto.Id}|{dto.Name}|{dto.Price}|{dto.Category}";
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(raw);
        var hash = sha.ComputeHash(bytes);

        return $"\"{Convert.ToHexString(hash)}\"";
    }
}