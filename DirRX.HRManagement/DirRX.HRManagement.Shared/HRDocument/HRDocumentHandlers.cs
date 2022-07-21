using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.HRDocument;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement
{
  partial class HRDocumentSharedHandlers
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

    public override void RegistrationDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      this.FillName();
      base.RegistrationDateChanged(e);
    }

    public override void BusinessUnitChanged(Sungero.Docflow.Shared.OfficialDocumentBusinessUnitChangedEventArgs e)
    {
      this.FillName();
      base.BusinessUnitChanged(e);
      if (e.NewValue != null && e.NewValue != e.OldValue)
        _obj.StaffChief = Employees.As(Functions.Module.Remote.GetStaffChief(e.NewValue));
    }

    public override void DocumentKindChanged(Sungero.Docflow.Shared.OfficialDocumentDocumentKindChangedEventArgs e)
    {
      this.FillName();
      base.DocumentKindChanged(e);
    }

    public virtual void EmploymentContractChanged(DirRX.HRManagement.Shared.HRDocumentEmploymentContractChangedEventArgs e)
    {
      if (e.NewValue != null && !Equals(e.NewValue, e.OldValue))
        _obj.Employee = _obj.EmploymentContract.Employee;
    }

    public override void EmployeeChanged(DirRX.HRManagement.Shared.HRDocumentBaseEmployeeChangedEventArgs e)
    {
      this.FillName();
      if (e.NewValue != null && !Equals(e.NewValue, e.OldValue))
      {
        _obj.Department = e.NewValue.Department;
        if (e.NewValue.Department.BusinessUnit != null)
          _obj.BusinessUnit = e.NewValue.Department.BusinessUnit;
        
        if (Equals(_obj.DocumentKind, Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(DocKind.TransferAdditionalAgreementKind)) ||
            Equals(_obj.DocumentKind, Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(DocKind.ChangeWorkConditionsAdditionalAgreementKind)))
          _obj.EmploymentContract = HRManagement.PublicFunctions.Module.Remote.GetEmployeeContract(e.NewValue);
      }
    }
  }

}