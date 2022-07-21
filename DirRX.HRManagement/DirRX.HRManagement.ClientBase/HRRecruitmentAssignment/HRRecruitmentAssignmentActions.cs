using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HRRecruitmentAssignment;

namespace DirRX.HRManagement.Client
{
  partial class HRRecruitmentAssignmentActions
  {
    public virtual void Delay(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      RecruitmentTasks.As(_obj.Task).Aspirant.RecruitmentState = Aspirant.RecruitmentState.Delayed;
    }

    public virtual bool CanDelay(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.RecruitmentTask.HiringBlockId;
    }

    public virtual void Hire(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var task = Functions.Module.Remote.CreateNewHiringTask();
      task.Aspirant = RecruitmentTasks.As(_obj.Task).Aspirant;
      task.ShowModal();
      if (task.Started == null)
        e.Cancel();
      task.Aspirant.RecruitmentState = Aspirant.RecruitmentState.Approved;
    }

    public virtual bool CanHire(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.RecruitmentTask.HiringBlockId;
    }

    public virtual void Finish(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      RecruitmentTasks.As(_obj.Task).Aspirant.RecruitmentState = Aspirant.RecruitmentState.Refused;
      _obj.State.Properties.Managers.IsRequired = false;
    }

    public virtual bool CanFinish(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.RecruitmentTask.HiringBlockId;
    }

  }

}