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
    public class IndexModel : PageModel
    {
        private readonly ChronoKraft.Infrastructure.CKDbContext _context;

        public IndexModel(ChronoKraft.Infrastructure.CKDbContext context)
        {
            _context = context;
        }

        public IList<ResourceAssignment> ResourceAssignment { get;set; } = default!;

        public async Task OnGetAsync()
        {
            ResourceAssignment = await _context.ResourceAssignments
                .Include(r => r.ProjectTask)
                .Include(r => r.Resource).ToListAsync();
        }
    }
}
