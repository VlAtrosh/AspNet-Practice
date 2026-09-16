using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AnketaApp.Models;

namespace AnketaApp.Pages
{
    public class AnketaModel : PageModel
    {
        [BindProperty]
        public Anketa Anketa { get; set; } = new();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Здесь можно сохранить данные в БД, отправить email и т.п.
            // А пока просто переходим на страницу успеха
            TempData["FirstName"] = Anketa.FirstName;
            TempData["LastName"] = Anketa.LastName;
            TempData["Age"] = Anketa.Age;
            TempData["Email"] = Anketa.Email;
            TempData["City"] = Anketa.City;
            TempData["About"] = Anketa.About;

            return RedirectToPage("/Success");
        }
    }
}