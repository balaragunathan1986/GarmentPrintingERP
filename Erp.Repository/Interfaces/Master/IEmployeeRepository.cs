using Erp.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Erp.Repository.Interfaces.Master
{
    public interface IEmployeeRepository
    {
        Task<List<EmployeeDTO>> GetEmployee();
        Task<EmployeeDTO> GetEmployee(Guid Employee_Id);
        Task<EmployeeDTO> Save(EmployeeDTO employee);
        Task<EmployeeDTO> Update(EmployeeDTO employee);
        Task<bool> Delete(Guid Employee_Id);
    }
}
