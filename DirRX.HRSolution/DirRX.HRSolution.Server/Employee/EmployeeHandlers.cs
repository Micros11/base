using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRSolution.Employee;

namespace DirRX.HRSolution
{
  partial class EmployeeCreatingFromServerHandler
  {

    public override void CreatingFrom(Sungero.Domain.CreatingFromEventArgs e)
    {
      base.CreatingFrom(e);
      
    }
  }



  partial class EmployeeServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      var currentEmployee = Employees.Current;
      var isNeedUpdateConsent = currentEmployee != null && _obj.State.Properties.ConsentDirRX.IsChanged;
      e.Params.AddOrUpdate(DirRX.HRSolution.Employees.Resources.IsNeedUpdateConsentFormat(_obj.Id), isNeedUpdateConsent);
      e.Params.AddOrUpdate(DirRX.HRSolution.Employees.Resources.IsNeedUpdateConsentStatusFormat(_obj.Id), _obj.State.Properties.ConsentDirRX.IsChanged);
    }

    public override void AfterSave(Sungero.Domain.AfterSaveEventArgs e)
    {
      base.AfterSave(e);
      
      var employees = HRSolution.Employees.GetAll(w => w.Status == HRSolution.Employee.Status.Active && w.Person.Id == _obj.Person.Id);
      if (employees.Where(w => w.Login != null).Any() && employees.Count() > 1)
        HRLite.PublicFunctions.Module.ManageSubstitutionOptions(_obj);
      
      // Если изменился Статус согласия на КЭДО, необходимо отразить это в истории.
      var isNeedUpdateConsentStatus = false;
      e.Params.TryGetValue(DirRX.HRSolution.Employees.Resources.IsNeedUpdateConsentStatusFormat(_obj.Id), out isNeedUpdateConsentStatus);
      if (isNeedUpdateConsentStatus)
      {
        if (_obj.ConsentDirRX.Value == DirRX.HRSolution.Employee.ConsentDirRX.NotSent || _obj.ConsentDirRX.Value == DirRX.HRSolution.Employee.ConsentDirRX.Signed)
        {
          var operation = new Enumeration(DirRX.HRLite.PublicConstants.Module.SignAgreement);
          var detail = _obj.ConsentDirRX.Value == DirRX.HRSolution.Employee.ConsentDirRX.NotSent ? Employees.Resources.ConcentNotSent : Employees.Resources.ConcentSigned;
          _obj.History.Write(operation, operation, detail);
        }
      }
      
      var isNeedUpdateConsent = false;
      e.Params.TryGetValue(DirRX.HRSolution.Employees.Resources.IsNeedUpdateConsentFormat(_obj.Id), out isNeedUpdateConsent);
      
      if (isNeedUpdateConsent)
      {
        var ids = Employees.GetAll(w => w.Status == Sungero.Company.Employee.Status.Active && w.Person.Id == _obj.Person.Id && w.Id != _obj.Id).Select(s => s.Id).ToList();
        ids.Insert(0, _obj.Id);
        
        var synchronizeConsent = HRManagement.AsyncHandlers.EmployeesConsentUpdate.Create();
        synchronizeConsent.EmployeesIds = string.Join(";", ids);
        synchronizeConsent.ExecuteAsync();
      }
      
      // При создании второго сотрудника у персоны в рамках одной НОР привязать Согласие на КЭДО к новому сотруднику.
      var otherEmployee = DirRX.HRSolution.Employees.GetAll(w => w.Id != _obj.Id && w.Status == Sungero.Company.Employee.Status.Active &&
                                                            Equals(_obj.Person, w.Person) && w.Department != null && w.Department.BusinessUnit != null &&
                                                            Equals(w.Department.BusinessUnit, _obj.Department.BusinessUnit)).FirstOrDefault();
      if (otherEmployee != null)
      {
        var employeeConcentLinks = DirRX.HRLite.EmployeeDocumentLinks.GetAll(l => Equals(l.Employee, otherEmployee) &&
                                                                             Equals(l.Document.DocumentKind, Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(HRLite.PublicConstants.Module.DocumentKind.DocumentManagementAgreementKind)));
        if (employeeConcentLinks.Any())
        {
          var document = new List<Sungero.Docflow.IInternalDocumentBase>() {Sungero.Docflow.InternalDocumentBases.As(employeeConcentLinks.OrderByDescending(l => l.Id).FirstOrDefault().Document)};
          HRLite.PublicFunctions.Module.CreateOrUpdateEmployeeDocumentLink(_obj, document, HRLite.EmployeeDocumentLink.Status.Active);
        }
      }
    }

    public override IDigestModel GetDigest(Sungero.Domain.GetDigestEventArgs e)
    {
      var digest = UserDigest.Create(_obj);
      if (_obj.BusinessUnitDirRX != null)
        digest.AddEntity(_obj.BusinessUnitDirRX);
      
      if (_obj.Department != null)
        digest.AddEntity(_obj.Department);
      
      if (_obj.JobTitle != null)
        digest.AddLabel(_obj.JobTitle.Name);
      
      if (!string.IsNullOrWhiteSpace(_obj.Phone))
        digest.AddLabel(string.Format("{0} {1}", Sungero.Company.Employees.Resources.PopupPhoneDescription, _obj.Phone));
      
      if (_obj.Department != null)
      {
        var manager = _obj.Department.Manager;
        if (manager == null && _obj.Department.HeadOffice != null)
          manager = _obj.Department.HeadOffice.Manager;
        
        if (manager != null && !Equals(manager, _obj))
          digest.AddEntity(manager, Sungero.Company.Employees.Resources.PopupManagerDescription);
      }
      return digest;
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      if (!_obj.State.IsCopied)
      {
        _obj.SignKindDirRX = DirRX.HRSolution.Employee.SignKindDirRX.AsDepartment;
        _obj.ConsentDirRX = DirRX.HRSolution.Employee.ConsentDirRX.NotSent;
      }
    }
  }

}