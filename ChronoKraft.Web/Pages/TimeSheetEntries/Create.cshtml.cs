using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ChronoKraft.Domain.Entities;
using ChronoKraft.Infrastructure;

namespace ChronoKraft.Web.Pages_TimeSheetEntries
{
    public class CreateModel : PageModel
    {
        private readonly ChronoKraft.Infrastructure.CKDbContext _context;

        public CreateModel(ChronoKraft.Infrastructure.CKDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["WorkerSkillId"] = new SelectList(_context.WorkerSkills, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public TimeSheetEntry TimeSheetEntry { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.TimeSheetEntries.Add(TimeSheetEntry);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
