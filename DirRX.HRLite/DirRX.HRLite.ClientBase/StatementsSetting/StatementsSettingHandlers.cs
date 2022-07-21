using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementsSetting;

namespace DirRX.HRLite
{
  partial class StatementsSettingPerformersClientHandlers
  {
    // HACK: Добавлены пустые вычисления для вызова события "Обновление формы" иначе не отрабатывает.
    public virtual void PerformersNumberValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      
    }

    public virtual void PerformersFilterValueInput(Sungero.Presentation.EnumerationValueInputEventArgs e)
    {
      if (e.NewValue == StatementsSettingPerformers.Filter.NeedFiltration && Sungero.Company.Employees.Is(_obj.Performer))
        e.AddError(Resources.StatementSettingsApprovalsAndPerformersFiltration);
    }
  }

  partial class StatementsSettingApproversClientHandlers
  {
    // HACK: Добавлены пустые вычисления для вызова события "Обновление формы" иначе не отрабатывает.
    public virtual void ApproversNumberValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      
    }

    public virtual void ApproversFilterValueInput(Sungero.Presentation.EnumerationValueInputEventArgs e)
    {
      if (e.NewValue == StatementsSettingApprovers.Filter.NeedFiltration && Sungero.Company.Employees.Is(_obj.Approver))
        e.AddError(Resources.StatementSettingsApprovalsAndPerformersFiltration);
    }
  }

  partial class StatementsSettingClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      if (_obj.State.Properties.Approvers.IsChanged)
      {
        var changedApprover = _obj.State.Properties.Approvers.Changed.FirstOrDefault();
        if (changedApprover != null && changedApprover.Number.HasValue &&
            (!Functions.StatementsSetting.CheckInputNumberUniqueness(_obj, changedApprover.Number.Value, true) || !Functions.StatementsSetting.CheckDuplicates(_obj, true)))
          e.AddWarning(HRLite.Resources.UniqueApproversNumber);
      }
      
      if (_obj.State.Properties.Performers.IsChanged)
      {
        var changedPerformer = _obj.State.Properties.Performers.Changed.FirstOrDefault();
        if (changedPerformer != null && changedPerformer.Number.HasValue &&
            (!Functions.StatementsSetting.CheckInputNumberUniqueness(_obj, changedPerformer.Number.Value, false) || !Functions.StatementsSetting.CheckDuplicates(_obj, false)))
          e.AddWarning(HRLite.Resources.UniquePerformersNumber);
      }
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Properties.Template.IsRequired = true;
      _obj.State.Properties.TemplateRX.IsRequired = true;
      _obj.State.Properties.ConfigSettings.IsRequired = true;
    }

  }
}