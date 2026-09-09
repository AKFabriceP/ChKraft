using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChronoKraft.Domain.Entities;
using ChronoKraft.Infrastructure;

namespace ChronoKraft.Web.Pages_ProjectTasks
{
    public class DeleteModel : PageModel
    {
        private readonly ChronoKraft.Infrastructure.CKDbContext _context;

        public DeleteModel(ChronoKraft.Infrastructure.CKDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ProjectTask ProjectTask { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projecttask = await _context.ProjectTasks.FirstOrDefaultAsync(m => m.Id == id);

            if (projecttask is not null)
            {
                ProjectTask = projecttask;

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

            var projecttask = await _context.ProjectTasks.FindAsync(id);
            if (projecttask != null)
            {
                ProjectTask = projecttask;
                _context.ProjectTasks.Remove(ProjectTask);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
