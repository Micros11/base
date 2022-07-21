using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.RecruitmentTask;

namespace DirRX.HRManagement
{
  partial class RecruitmentTaskDepartmentsDepartmentPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> DepartmentsDepartmentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = query.Where(d => d.Status == Sungero.CoreEntities.DatabookEntry.Status.Active);
      if (_root.BusinessUnit != null)
        query = query.Where(d => Equals(d.BusinessUnit, _root.BusinessUnit));
      return query;
    }
  }

  partial class RecruitmentTaskServerHandlers
  {

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      _obj.Aspirant = _obj.AspirantAttachmentGroup.Aspirants.FirstOrDefault();
    }

    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      if (_obj.State.Properties.Status.OriginalValue == Sungero.Workflow.Task.Status.Draft)
        return;
      
      Functions.Module.SendAbortationNotices(_obj, RecruitmentTasks.Resources.AbortNoticeFormat(_obj.Aspirant.Name), _obj.AbortingReason, _obj.Author, _obj.ThreadSubject);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      var employee = Sungero.Company.Employees.Current;
      _obj.BusinessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
      _obj.ActiveText = RecruitmentTasks.Resources.ActiveTextOnCreate;
    }
  }

}