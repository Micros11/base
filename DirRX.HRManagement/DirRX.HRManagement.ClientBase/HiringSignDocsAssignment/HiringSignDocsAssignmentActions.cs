using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRManagement.HiringSignDocsAssignment;

namespace DirRX.HRManagement.Client
{
  partial class HiringSignDocsAssignmentActions
  {
    public virtual void ReWork(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(HiringTasks.Resources.ErrorTextReWorkNoActiveText);
    }

    public virtual bool CanReWork(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.HiringTask.SignDocBySeoBlockId;
    }

    public virtual void Sign(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // Подписание трудового договора и приказа.
      var task = HiringTasks.As(_obj.Task);
      var documents = new List<IOfficialDocument>();
      documents.Add(Functions.HiringTask.Remote.GetEmployeeOrder(task));
      documents.Add(Functions.HiringTask.Remote.GetEmployeeContract(task));
      var perfomer = Sungero.Company.Employees.As(_obj.Performer);
      if (perfomer.Equals(task.Employee))
      {
        var contractAddendum = Functions.HiringTask.Remote.GetContractAddendum(task);
        if (contractAddendum != null)
          documents.Add(contractAddendum);
      }
      // Выяснить, требуется ли усиленная подпись.
      var needAdvancedSign = Functions.Module.Remote.NeedAdvancedSign(Sungero.Company.Employees.As(_obj.Performer), _obj.BlockUid,
                                                                      new List<string>() { Constants.HiringTask.SignDocBySeoBlockId },
                                                                      new List<string>() { Constants.HiringTask.SignDocByEmployeeBlockId });
      // Подписать документ.
      Functions.Module.ApproveDocument(Sungero.Workflow.Assignments.As(_obj), documents, e.Action.LocalizedName, e, needAdvancedSign);
    }

    public virtual bool CanSign(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}