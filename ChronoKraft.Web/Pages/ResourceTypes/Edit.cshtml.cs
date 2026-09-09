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

namespace ChronoKraft.Web.Pages_ResourceTypes
{
    public class EditModel : PageModel
    {
        private readonly ChronoKraft.Infrastructure.CKDbContext _context;

        public EditModel(ChronoKraft.Infrastructure.CKDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ResourceType ResourceType { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resourcetype =  await _context.ResourceTypes.FirstOrDefaultAsync(m => m.Id == id);
            if (resourcetype == null)
            {
                return NotFound();
            }
            ResourceType = resourcetype;
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

            _context.Attach(ResourceType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResourceTypeExists(ResourceType.Id))
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

        private bool ResourceTypeExists(int id)
        {
            return _context.ResourceTypes.Any(e => e.Id == id);
        }
    }
}
