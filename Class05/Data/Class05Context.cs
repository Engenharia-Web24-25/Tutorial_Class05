using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Class05.Models;

namespace Class05.Data
{
    public class Class05Context : DbContext
    {
        public Class05Context (DbContextOptions<Class05Context> options)
            : base(options)
        {
        }

        public DbSet<Class05.Models.Book> Book { get; set; } = default!;
    }
}
