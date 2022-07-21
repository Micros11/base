using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DismissalGoRoundAssignment;

namespace DirRX.HRManagement.Client
{
  partial class DismissalGoRoundAssignmentActions
  {
    public virtual void NoClaimDebts(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanNoClaimDebts(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return DismissalTasks.As(_obj.Task).ReGoRound != true;
    }

    public virtual void ClaimDebts(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(DismissalTasks.Resources.ErrorTextGoRoundNoActiveText);
    }

    public virtual bool CanClaimDebts(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return DismissalTasks.As(_obj.Task).ReGoRound != true;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return DismissalTasks.As(_obj.Task).ReGoRound == true;
    }

  }

}