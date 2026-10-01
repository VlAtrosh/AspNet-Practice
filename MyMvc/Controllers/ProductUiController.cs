using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models;
using MyMvcApp.Services;
using MyMvcApp.ViewModels;

namespace MyMvcApp.Controllers;

public class ProductUiController : Controller
{
    private readonly IProductRepository _repo;

    public ProductUiController(IProductRepository repo) => _repo = repo;

    // GET: /ProductUi/Create — показать форму
    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProductCreateViewModel());
    }

    // POST: /ProductUi/Create — обработать форму
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);   // вернуть форму с ошибками

        var product = new Product
        {
            Name = vm.Name.Trim(),
            Price = vm.Price,
            Category = vm.Category.Trim()
        };

        var created = await _repo.AddAsync(product, ct);

        // После создания — на страницу просмотра созданного товара
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    // GET: /ProductUi/Details/5 — просмотр одного товара
    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(id, ct);
        if (product is null)
            return NotFound();

        return View(product);
    }
}