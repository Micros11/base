using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;

namespace DirRX.HRManagement
{
  partial class VacationAcquaintanceListServerHandlers
  {

    public virtual IQueryable<Sungero.Company.IEmployee> GetEmployees()
    {
      var membersList = new List<IEmployee>();
      var departmentMembers = VacationAcquaintanceList.Department.RecipientLinks.Where(x => x.Member.Status == Sungero.CoreEntities.Recipient.Status.Active).Select(l => l.Member);
      foreach (var member in departmentMembers)
        membersList.Add(Employees.As(member));
      return Sungero.Company.Employees.GetAll(l => membersList.Contains(l)).OrderBy(x => x.Name);
    }
  }

}