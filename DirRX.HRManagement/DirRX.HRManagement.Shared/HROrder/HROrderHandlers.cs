using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.HROrder;

namespace DirRX.HRManagement
{
  partial class HROrderSharedHandlers
  {

    public override void SubjectChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      
    }

    public override void NameChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      base.NameChanged(e);
      // Записать имя документа в Содержание для корректного отображения в окне со списком документов при выгрузке.
      if (e.NewValue != null)
        _obj.Subject = e.NewValue;
    }

    public override void BusinessUnitChanged(Sungero.Docflow.Shared.OfficialDocumentBusinessUnitChangedEventArgs e)
    {
      var businessUnit = e.NewValue;
      if (businessUnit != e.OldValue && _obj.Department != null && !Equals(businessUnit, _obj.Department.BusinessUnit))
        _obj.Department = null;
    }

    public override void DepartmentChanged(Sungero.Docflow.Shared.OfficialDocumentDepartmentChangedEventArgs e)
    {
      var department = e.NewValue;
      if (department != null && department.BusinessUnit != null)
        _obj.BusinessUnit = department.BusinessUnit;
    }
    
    public virtual void EmployeeChanged(DirRX.HRManagement.Shared.HROrderEmployeeChangedEventArgs e)
    {
      this.FillName();
      if (e.NewValue != null && !Equals(e.NewValue, e.OldValue))
      {
        _obj.Department = e.NewValue.Department;
        var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(e.NewValue);
        _obj.StaffChief = Employees.As(Functions.Module.Remote.GetStaffChief(businessUnit));
        _obj.PreparedBy = Employees.As(Functions.Module.Remote.GetHRResponsible(businessUnit));
      }
    }
  }
}