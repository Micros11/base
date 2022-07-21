using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.HRManagement.DocumentApprovalTask;

namespace DirRX.HRManagement.Server
{
  partial class DocumentApprovalTaskRouteHandlers
  {

    public virtual void Script8Execute()
    {
      _obj.Abort();
    }

    public virtual void StartBlock6(DirRX.HRManagement.Server.DocumentReworkAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = DocumentApprovalTasks.Resources.ReworkApprovalTaskSubjectFormat(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault().Name);
    }

    public virtual void StartBlock5(DirRX.HRManagement.Server.DocumentApprovalAssignmentArguments e)
    {
      var approvers = _obj.Approvers.Select(a => a.Approver);
      foreach (var approver in approvers)
        e.Block.Performers.Add(approver);
      e.Block.ShowInSelfServiceOffice = true;
      e.Block.RelativeDeadlineDays = _obj.ApproveDeadline;
      var isParallel = _obj.IsParallel.HasValue && _obj.IsParallel.Value;
      e.Block.IsParallel = isParallel;
      if (!isParallel)
        e.Block.StopResult = DirRX.HRManagement.DocumentApprovalAssignment.Result.ForRework;
      e.Block.Subject = DocumentApprovalTasks.Resources.DocumentApprovalTaskSubjectFormat(_obj.DocumentGroup.InternalDocumentBases.FirstOrDefault().Name);
      // Для отправки оповещений о новых заданиях раскрыть согласующих до сотрудников.
      var employees = DirRX.HRLite.PublicFunctions.Module.GetNonSystemActiveRecipients(approvers.ToList());
      foreach (var employee in employees)
        EssPlatform.PublicFunctions.Module.SendNewNotification(employee, false);
    }

  }
}