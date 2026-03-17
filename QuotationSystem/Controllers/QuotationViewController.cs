using Microsoft.AspNetCore.Mvc;
using QuotationSystem.Services;

public class QuotationView : Controller
{
    private readonly IQuotationService _service;
    public QuotationView(IQuotationService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        var data = await _service.GetAllQuotationsAsync();
        return View(data);
    }
}