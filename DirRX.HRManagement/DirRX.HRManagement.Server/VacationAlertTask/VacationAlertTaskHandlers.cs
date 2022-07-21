using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationAlertTask;
using DocKinds = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement
{
  partial class VacationAlertTaskServerHandlers
  {

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      var employee = _obj.EmployeeAttachmentGroup.Employees.FirstOrDefault();
      var vacationNotice = Functions.Module.CreateStatement(DocKinds.ScheduledVacationNoticeKind, employee, Calendar.Now, _obj.Vacations.FirstOrDefault().Vacation.Year.Value);
      var vacationTexts = new List<string>();
      foreach (var vacationRow in _obj.Vacations)
      {
        var vacation = vacationRow.Vacation;
        vacationTexts.Add(Resources.AlertStatmentVacationTextFormat(vacation.VacationKind.Name.ToLower(),
                                                                    vacation.VacationDuration.Value,
                                                                    vacation.StartDate.Value.ToLongDateString()));
      }
      vacationNotice.BodyText = string.Join(", ", vacationTexts);
      vacationNotice.WorkDate = Calendar.NextWorkingDay(_obj.Vacations.Max(l => l.Vacation.FinDate.Value));
      vacationNotice.Save();
      _obj.Manager = Functions.Module.GetManager(employee);
      _obj.Notification = vacationNotice;
    }
  }
}