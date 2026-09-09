using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ChronoKraft.Domain.Entities;
using ChronoKraft.Infrastructure;

namespace ChronoKraft.Web.Pages_ResourceAssignments
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
        ViewData["ProjectTaskId"] = new SelectList(_context.ProjectTasks, "Id", "Name");
        ViewData["ResourceId"] = new SelectList(_context.Resources, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public ResourceAssignment ResourceAssignment { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.ResourceAssignments.Add(ResourceAssignment);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
