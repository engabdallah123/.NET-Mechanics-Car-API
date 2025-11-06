using Data;
using Domain.DTO.Part;
using Domain.Model;
using Services.IReposetory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Repository
{
    public class PartRepo : IPartRepo
    {
        private readonly DataContext db;

        public PartRepo(DataContext db)
        {
            this.db = db;
        }
        public async Task AddPart(AddPartDTO partDTO,int taskId)
        {
            var part = new Part
            {

                Name = partDTO.Name ?? string.Empty,
                Quantity = partDTO.Quantity ?? 1,
                UnitPrice = partDTO.UnitPrice ?? 0,
                TaskId = taskId
                
            };
            await db.Parts.AddAsync(part);
            await db.SaveChangesAsync();
        }
        public async Task DeletePart(int partId)
        {
            var part = await db.Parts.FindAsync(partId);
            if (part == null)
                throw new KeyNotFoundException($"Part With id '{partId}' not found");
            db.Parts.Remove(part);
            await db.SaveChangesAsync();
        }
    }
}
