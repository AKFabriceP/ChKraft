using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChronoKraft.Domain.Entities;
using ChronoKraft.Infrastructure;

namespace ChronoKraft.Web.Pages_TimeSheetReviews
{
    public class DeleteModel : PageModel
    {
        private readonly ChronoKraft.Infrastructure.CKDbContext _context;

        public DeleteModel(ChronoKraft.Infrastructure.CKDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TimeSheetReview TimeSheetReview { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var timesheetreview = await _context.TimeSheetReviews.FirstOrDefaultAsync(m => m.Id == id);

            if (timesheetreview is not null)
            {
                TimeSheetReview = timesheetreview;

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

            var timesheetreview = await _context.TimeSheetReviews.FindAsync(id);
            if (timesheetreview != null)
            {
                TimeSheetReview = timesheetreview;
                _context.TimeSheetReviews.Remove(TimeSheetReview);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
