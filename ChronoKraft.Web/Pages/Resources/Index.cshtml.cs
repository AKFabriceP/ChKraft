using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChronoKraft.Domain.Entities;
using ChronoKraft.Infrastructure;

namespace ChronoKraft.Web.Pages_Resources
{
    public class IndexModel : PageModel
    {
        private readonly ChronoKraft.Infrastructure.CKDbContext _context;

        public IndexModel(ChronoKraft.Infrastructure.CKDbContext context)
        {
            _context = context;
        }

        public IList<Resource> Resource { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Resource = await _context.Resources
                .Include(r => r.Contractor)
                .Include(r => r.ResourceType).ToListAsync();
        }
    }
}
