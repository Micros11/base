using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TaskSetting;

namespace DirRX.HRManagement
{
  partial class TaskSettingApproversApproverPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ApproversApproverFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(a => Sungero.Company.Employees.Is(a) || a.Sid == Sungero.Docflow.Constants.Module.RoleGuid.DepartmentManagersRole);
    }
  }

  partial class TaskSettingDocumentKindPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> DocumentKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var allPossibleDocumentKinds = Sungero.Docflow.DocumentKinds.GetAll().ToList().Where(k => HRLite.PublicFunctions.Module.Remote.IsPersonnelDocument(k.DocumentType) ||
                                                                                           HRLite.PublicFunctions.Module.Remote.IsBilaterialDocument(k.DocumentType) ||
                                                                                           HRLite.PublicFunctions.Module.Remote.IsLocalRegulationDocument(k.DocumentType));
      return query.Where(k => allPossibleDocumentKinds.Contains(k) && k.Status == Sungero.Docflow.DocumentKind.Status.Active);
    }
  }

  partial class TaskSettingServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      // Не сохранять, если есть действующие настройки с параметрами равными текущей настройке.
      var sameTaskSetting = TaskSettings.GetAll(t => t.TaskType.Equals(_obj.TaskType) && t.DocumentKind.Equals(_obj.DocumentKind) && t.BusinessUnit.Equals(_obj.BusinessUnit)
                                                && t.Department.Equals(_obj.Department) && !t.Equals(_obj) && t.Status == HRManagement.TaskSetting.Status.Active);
      if (sameTaskSetting.Any())
        e.AddError(DirRX.HRManagement.TaskSettings.Resources.SaveSameTaskSettingError);
    }
  }

  partial class TaskSettingSignerPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> SignerFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var signatureAuthority = Sungero.Docflow.SignatureSettings.GetAll().Select(s => s.Recipient);
      var employees = query.Where(emp => signatureAuthority.Contains(emp));
      return employees;
    }
  }
}