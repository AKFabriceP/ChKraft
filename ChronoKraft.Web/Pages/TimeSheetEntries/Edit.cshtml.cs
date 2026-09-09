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

namespace ChronoKraft.Web.Pages_TimeSheetEntries
{
    public class EditModel : PageModel
    {
        private readonly ChronoKraft.Infrastructure.CKDbContext _context;

        public EditModel(ChronoKraft.Infrastructure.CKDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TimeSheetEntry TimeSheetEntry { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var timesheetentry =  await _context.TimeSheetEntries.FirstOrDefaultAsync(m => m.Id == id);
            if (timesheetentry == null)
            {
                return NotFound();
            }
            TimeSheetEntry = timesheetentry;
           ViewData["WorkerSkillId"] = new SelectList(_context.WorkerSkills, "Id", "Id");
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

            _context.Attach(TimeSheetEntry).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TimeSheetEntryExists(TimeSheetEntry.Id))
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

        private bool TimeSheetEntryExists(int id)
        {
            return _context.TimeSheetEntries.Any(e => e.Id == id);
        }
    }
}
