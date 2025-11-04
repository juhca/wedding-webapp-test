using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Common.Interfaces;

public interface IUserRepository
{
	Task<User?> GetByIdAsync(Guid id);
	Task<IEnumerable<User>> GetAllAsync();
	Task AddAsync(User user);
}
