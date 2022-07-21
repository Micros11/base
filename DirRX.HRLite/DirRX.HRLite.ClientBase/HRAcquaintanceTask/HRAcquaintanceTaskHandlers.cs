using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.HRAcquaintanceTask;

namespace DirRX.HRLite
{

  partial class HRAcquaintanceTaskClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Properties.SignatoryDeadline.IsEnabled = _obj.Signatory != null;
    }

    public virtual void ApproveDeadlineValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue <= 0)
        e.AddError(HRAcquaintanceTasks.Resources.ApproveDeadlineDaysError);
      if (e.NewValue.HasValue && e.NewValue > Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline)
        e.AddError(Sungero.Docflow.ApprovalRuleBases.Resources.IncorrectMaxReworkDeadlineFormat(Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline));
    }

    public virtual void EmployeeDeadlineValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue <= 0)
        e.AddError(HRAcquaintanceTasks.Resources.EmployeeDealineDaysError);
      if (e.NewValue.HasValue && e.NewValue > Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline)
        e.AddError(Sungero.Docflow.ApprovalRuleBases.Resources.IncorrectMaxReworkDeadlineFormat(Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline));
    }

    public virtual void SignatoryDeadlineValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue <= 0)
        e.AddError(HRAcquaintanceTasks.Resources.SignatoryDeadlineDaysError);
      if (e.NewValue.HasValue && e.NewValue > Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline)
        e.AddError(Sungero.Docflow.ApprovalRuleBases.Resources.IncorrectMaxReworkDeadlineFormat(Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline));
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {      
      var employees = Functions.HRAcquaintanceTask.Remote.GetParticipants(_obj);
      
      if (employees.Count > 0 && (EssPlatform.PublicFunctions.Module.Remote.HasEmployeeNotInESS(employees) || !Functions.Module.Remote.CheckEmployeeConsent(employees)))
        e.AddWarning(HRAcquaintanceTasks.Resources.NotAllEmployeeAcquaintance, _obj.Info.Actions.ShowEmployeeNoESS);
      var existApprovers = _obj.Approvers.Count > 0;
      _obj.State.Properties.ApproveDeadline.IsEnabled = existApprovers;
      _obj.State.Properties.ApproveDeadline.IsRequired = existApprovers;
      _obj.State.Properties.ApprovalQueue.IsEnabled = existApprovers;
      _obj.State.Properties.ApprovalQueue.IsRequired = existApprovers;
      if (Functions.HRAcquaintanceTask.IsAllDeadlinesFilled(_obj))
        e.AddInformation(Functions.HRAcquaintanceTask.GenerateExpectedTaskDeadlineInfo(_obj));

      if (_obj.Signatory == null)
        e.AddWarning(DirRX.HRLite.HRAcquaintanceTasks.Resources.SignatoryFieldIsEmpty);
    }
  }
}