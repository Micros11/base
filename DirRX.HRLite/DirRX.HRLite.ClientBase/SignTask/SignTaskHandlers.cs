using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.SignTask;

namespace DirRX.HRLite
{
  partial class SignTaskClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Properties.SignatoryDeadline.IsEnabled = _obj.Signatory != null;
    }

    public virtual void ApproveDeadlineValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue <= 0)
        e.AddError(SignTasks.Resources.ApproveDeadlineDaysError);
      if (e.NewValue.HasValue && e.NewValue > Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline)
        e.AddError(Sungero.Docflow.ApprovalRuleBases.Resources.IncorrectMaxReworkDeadlineFormat(Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline));
    }

    public virtual void EmployeeDeadlineValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue <= 0)
        e.AddError(SignTasks.Resources.EmployeeDealineDaysError);
      if (e.NewValue.HasValue && e.NewValue > Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline)
        e.AddError(Sungero.Docflow.ApprovalRuleBases.Resources.IncorrectMaxReworkDeadlineFormat(Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline));
    }

    public virtual void SignatoryDeadlineValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue <= 0)
        e.AddError(SignTasks.Resources.SignatoryDealineDaysError);
      if (e.NewValue.HasValue && e.NewValue > Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline)
        e.AddError(Sungero.Docflow.ApprovalRuleBases.Resources.IncorrectMaxReworkDeadlineFormat(Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline));
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      var document = _obj.DocumentGroup.BilaterialDocuments.FirstOrDefault();
      if (document != null && (EssPlatform.PublicFunctions.Module.Remote.HasEmployeeNotInESS(document.Employees.Select(t => t.Employee).ToList()) || !Functions.Module.Remote.CheckEmployeeConsent(document.Employees.Select(t => t.Employee).ToList())))
        e.AddWarning(SignTasks.Resources.NotAllEmployeeAcquaintance, _obj.Info.Actions.ShowEmployeeNoESS);
      var existApprovers = _obj.Approvers.Count > 0;
      _obj.State.Properties.ApproveDeadline.IsEnabled = existApprovers;
      _obj.State.Properties.ApproveDeadline.IsRequired = existApprovers;
      _obj.State.Properties.ApprovalQueue.IsEnabled = existApprovers;
      _obj.State.Properties.ApprovalQueue.IsRequired = existApprovers;
      if (Functions.SignTask.IsAllDeadlinesFilled(_obj))
        e.AddInformation(Functions.SignTask.GenerateExpectedTaskDeadlineInfo(_obj));
    }

  }
}