using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementsSetting;

namespace DirRX.HRLite
{
  partial class StatementsSettingPerformersSharedHandlers
  {

    public virtual void PerformersPerformerChanged(DirRX.HRLite.Shared.StatementsSettingPerformersPerformerChangedEventArgs e)
    {
      if (e.NewValue != null)
        _obj.Filter = Sungero.Company.Employees.Is(e.NewValue) ? StatementsSettingPerformers.Filter.NoFiltration : StatementsSettingPerformers.Filter.NeedFiltration;
    }
  }

  partial class StatementsSettingPerformersSharedCollectionHandlers
  {

    public virtual void PerformersAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {
      var maxNumber = _obj.Performers.Select(p => p.Number).Max();
      _added.Number = maxNumber != null ? maxNumber + 1 : 1;
    }
  }

  partial class StatementsSettingApproversSharedHandlers
  {

    public virtual void ApproversApproverChanged(DirRX.HRLite.Shared.StatementsSettingApproversApproverChangedEventArgs e)
    {
      if (e.NewValue != null)
        _obj.Filter = Sungero.Company.Employees.Is(e.NewValue) ? StatementsSettingApprovers.Filter.NoFiltration : StatementsSettingApprovers.Filter.NeedFiltration;
    }
  }

  partial class StatementsSettingApproversSharedCollectionHandlers
  {

    public virtual void ApproversAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {
      var maxNumber = _obj.Approvers.Select(s => s.Number).Max();
      _added.Number = maxNumber != null ? maxNumber + 1 : 1;
    }
  }

  partial class StatementsSettingSharedHandlers
  {

  }
}