using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationApprovalTask;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;

namespace DirRX.HRManagement
{
  partial class VacationApprovalTaskVacationKindPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> VacationKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (Functions.Module.IsVacationScheduleUsed(_obj.Employee))
        query = query.Where(l => l.Paid.Value == false);
      return query;
    }
  }

  partial class VacationApprovalTaskSubstitutePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> SubstituteFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(l => !l.Equals(_obj.Employee));
    }
  }

  partial class VacationApprovalTaskServerHandlers
  {
    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      var order = _obj.OrderAttachmentGroup.HROrders.FirstOrDefault();
      if (order != null && order.LastVersion != null
          && Signatures.Get(order.LastVersion).Any(w => w.SignatureType == SignatureType.Approval && Equals(w.Signatory, Users.As(order.OurSignatory))))
      {
        e.AddError(DirRX.HRManagement.VacationApprovalTasks.Resources.ApprovedOrderApprovalTaskAbortError);
        return;
      }
      
      if (_obj.State.Properties.Status.OriginalValue == Sungero.Workflow.Task.Status.Draft)
        return;

      Functions.Module.SendAbortationNotices(_obj, VacationApprovalTasks.Resources.AbortNoticeFormat(_obj.Employee.Name), _obj.AbortingReason, _obj.Author, _obj.ThreadSubject);
    }

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      var errorList = Functions.VacationApprovalTask.CheckAllVacationDates(_obj);
      
      foreach (var error in errorList)
        e.AddError(error);
      
      if (errorList.Any())
        return;
      
      if (_obj.VacationKind.Paid.Value == true)
      {
        var warningList = Functions.VacationApprovalTask.CheckNotCriticalVacationsDates(_obj, _obj.Employee, Structures.Vacation.VacationDates.Create(_obj.DateBegin.Value, _obj.DateEnd.Value,
                                                                                                                                                      _obj.Duration.Value, DateStatuses.New, _obj.VacationKind));
        foreach (var warning in warningList)
          e.AddWarning(warning);
      }
      
      _obj.FirstSubstituteValue = _obj.Substitute;
    }
    
    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      var employee = Sungero.Company.Employees.As(_obj.Author);
      _obj.ActiveText = VacationApprovalTasks.Resources.VacationApprovalTaskActiveText;
      _obj.Employee = employee;
      _obj.Year = Calendar.SqlMinValue;
      _obj.VacationKind = Functions.Module.GetVacationWithoutPayKind();
    }
  }
}