using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChronoKraft.Domain.Entities;
using ChronoKraft.Infrastructure;

namespace ChronoKraft.Web.Pages_ResourceAssignments
{
    public class EditModel : PageModel
    {
        private readonly ChronoKraft.Infrastructure.CKDbContext _context;

        public EditModel(ChronoKraft.Infrastructure.CKDbContext context)
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

            var resourceassignment =  await _context.ResourceAssignments.FirstOrDefaultAsync(m => m.Id == id);
            if (resourceassignment == null)
            {
                return NotFound();
            }
            ResourceAssignment = resourceassignment;
           ViewData["ProjectTaskId"] = new SelectList(_context.ProjectTasks, "Id", "Name");
           ViewData["ResourceId"] = new SelectList(_context.Resources, "Id", "Id");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(ResourceAssignment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResourceAssignmentExists(ResourceAssignment.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ResourceAssignmentExists(int id)
        {
            return _context.ResourceAssignments.Any(e => e.Id == id);
        }
    }
}
