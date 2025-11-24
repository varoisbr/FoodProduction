using FoodProduction.Data;
using FoodProduction.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FoodProduction.Pages.Admin;

public class LabelTemplatesAdminModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public LabelTemplatesAdminModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<LabelTemplate> Templates { get; set; } = [];

    [BindProperty]
    public int EditingTemplateId { get; set; }

    [BindProperty]
    public string? TemplateName { get; set; }

    [BindProperty]
    public string? TemplateContent { get; set; }

    public async Task OnGetAsync()
    {
        await LoadData();
    }

    public async Task<IActionResult> OnPostAsync(string action)
    {
        await LoadData();

        try
        {
            if (action == "save")
            {
                if (string.IsNullOrWhiteSpace(TemplateName))
                {
                    ModelState.AddModelError(string.Empty, "Template name is required.");
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(TemplateContent))
                {
                    ModelState.AddModelError(string.Empty, "Template content is required.");
                    return Page();
                }

                if (EditingTemplateId > 0)
                {
                    // Update existing template
                    var template = await _context.LabelTemplates.FindAsync(EditingTemplateId);
                    if (template == null)
                    {
                        ModelState.AddModelError(string.Empty, "Template not found.");
                        return Page();
                    }

                    template.Name = TemplateName;
                    template.Content = TemplateContent;
                    template.UpdatedAt = DateTime.UtcNow;

                    _context.LabelTemplates.Update(template);
                }
                else
                {
                    // Create new template
                    var template = new LabelTemplate
                    {
                        Name = TemplateName,
                        Content = TemplateContent,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.LabelTemplates.Add(template);
                }

                await _context.SaveChangesAsync();

                TemplateName = null;
                TemplateContent = null;
                EditingTemplateId = 0;
            }
            else if (action == "cancel")
            {
                EditingTemplateId = 0;
                TemplateName = null;
                TemplateContent = null;
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostEditAsync(int id)
    {
        var template = await _context.LabelTemplates.FindAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        EditingTemplateId = template.Id;
        TemplateName = template.Name;
        TemplateContent = template.Content;

        await LoadData();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var template = await _context.LabelTemplates.FindAsync(id);
        if (template != null)
        {
            _context.LabelTemplates.Remove(template);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    private async Task LoadData()
    {
        Templates = await _context.LabelTemplates
            .Include(t => t.Products)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}
