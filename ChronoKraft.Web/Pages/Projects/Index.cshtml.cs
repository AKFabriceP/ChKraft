using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChronoKraft.Domain.Entities;
using ChronoKraft.Infrastructure;

namespace ChronoKraft.Web.Pages_Projects
{
    public class IndexModel : PageModel
    {
        private readonly ChronoKraft.Infrastructure.CKDbContext _context;

        public IndexModel(ChronoKraft.Infrastructure.CKDbContext context)
        {
            _context = context;
        }

        public IList<Project> Project { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Project = await _context.Projects
                .Include(p => p.Manager).ToListAsync();
        }
    }
}
