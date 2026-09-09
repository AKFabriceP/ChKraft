using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChronoKraft.Domain.Entities;
using ChronoKraft.Infrastructure;

namespace ChronoKraft.Web.Pages_ResourceAssignments
{
    public class DeleteModel : PageModel
    {
        private readonly ChronoKraft.Infrastructure.CKDbContext _context;

        public DeleteModel(ChronoKraft.Infrastructure.CKDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ResourceAssignment ResourceAssignment { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resourceassignment = await _context.ResourceAssignments.FirstOrDefaultAsync(m => m.Id == id);

            if (resourceassignment is not null)
            {
                ResourceAssignment = resourceassignment;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resourceassignment = await _context.ResourceAssignments.FindAsync(id);
            if (resourceassignment != null)
            {
                ResourceAssignment = resourceassignment;
                _context.ResourceAssignments.Remove(ResourceAssignment);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
