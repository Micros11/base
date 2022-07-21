using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TaskSetting;

namespace DirRX.HRManagement
{
  partial class TaskSettingClientHandlers
  {

    public virtual void EmployeeDealineDaysValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue <= 0)
        e.AddError(TaskSettings.Resources.EmployeeDealineDaysError);
      if (e.NewValue.HasValue && e.NewValue > Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline)
        e.AddError(Sungero.Docflow.ApprovalRuleBases.Resources.IncorrectMaxReworkDeadlineFormat(Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline));
    }

    public virtual void SignerDealineDaysValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue <= 0)
        e.AddError(TaskSettings.Resources.SignerDealineDaysError);
      if (e.NewValue.HasValue && e.NewValue > Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline)
        e.AddError(Sungero.Docflow.ApprovalRuleBases.Resources.IncorrectMaxReworkDeadlineFormat(Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline));
    }

    public virtual void ApproveDeadlineDaysValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue <= 0)
        e.AddError(TaskSettings.Resources.ApproveDeadlineDaysError);
      if (e.NewValue.HasValue && e.NewValue > Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline)
        e.AddError(Sungero.Docflow.ApprovalRuleBases.Resources.IncorrectMaxReworkDeadlineFormat(Sungero.Docflow.Constants.ApprovalRuleBase.MaxReworkDeadline));
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      var isEnabledApproveFields = _obj.Approvers.Count > 0;
      _obj.State.Properties.ApproveDeadlineDays.IsEnabled = isEnabledApproveFields;
      _obj.State.Properties.ApproveDeadlineDays.IsRequired = isEnabledApproveFields;
      _obj.State.Properties.ApprovalQueue.IsEnabled = isEnabledApproveFields;
      _obj.State.Properties.ApprovalQueue.IsRequired = isEnabledApproveFields;
      
      var enabledSignQueue = !string.IsNullOrEmpty(_obj.TaskType) && _obj.TaskType.Equals(HRLite.SignTasks.Info.Name);
      _obj.State.Properties.SignQueue.IsEnabled = enabledSignQueue;
      _obj.State.Properties.SignQueue.IsRequired = enabledSignQueue;
      _obj.State.Properties.SignQueue.IsVisible = enabledSignQueue;
    }

    public virtual void DocumentKindValueInput(DirRX.HRManagement.Client.TaskSettingDocumentKindValueInputEventArgs e)
    {
      if (e.NewValue != null)
      {
        if (HRLite.PublicFunctions.Module.Remote.IsPersonnelDocument(e.NewValue.DocumentType) || HRLite.PublicFunctions.Module.Remote.IsLocalRegulationDocument(e.NewValue.DocumentType))
        {
          _obj.TaskType = HRLite.HRAcquaintanceTasks.Info.Name;
          _obj.Name = HRLite.HRAcquaintanceTasks.Info.LocalizedName;
        }
        else if (HRLite.PublicFunctions.Module.Remote.IsBilaterialDocument(e.NewValue.DocumentType))
        {
          _obj.TaskType = HRLite.SignTasks.Info.Name;
          _obj.Name = HRLite.SignTasks.Info.LocalizedName;
        }
      }
    }

  }
}