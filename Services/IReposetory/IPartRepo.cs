using Domain.DTO.Part;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IReposetory
{
    public interface IPartRepo
    {
        Task AddPart(AddPartDTO partDTO,int taskId);
        Task DeletePart(int partId);
    }
} 
