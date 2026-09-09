using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChronoKraft.Domain.Entities;
using ChronoKraft.Infrastructure;

namespace ChronoKraft.Web.Pages_WorkerSkills
{
    public class DeleteModel : PageModel
    {
        private readonly ChronoKraft.Infrastructure.CKDbContext _context;

        public DeleteModel(ChronoKraft.Infrastructure.CKDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WorkerSkill WorkerSkill { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workerskill = await _context.WorkerSkills.FirstOrDefaultAsync(m => m.Id == id);

            if (workerskill is not null)
            {
                WorkerSkill = workerskill;

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

            var workerskill = await _context.WorkerSkills.FindAsync(id);
            if (workerskill != null)
            {
                WorkerSkill = workerskill;
                _context.WorkerSkills.Remove(WorkerSkill);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
