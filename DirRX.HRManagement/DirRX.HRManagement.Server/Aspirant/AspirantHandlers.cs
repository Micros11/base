using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.Aspirant;

namespace DirRX.HRManagement
{

  partial class AspirantReportPropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> ReportFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(q => Equals(q.Aspirant, _obj));
    }
  }

  partial class AspirantServerHandlers
  {
    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.RecruitmentState = DirRX.HRManagement.Aspirant.RecruitmentState.Draft;
    }
  }

}