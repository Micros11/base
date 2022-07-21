using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationApprovalAssignment;

namespace DirRX.HRManagement
{
  partial class VacationApprovalAssignmentServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      // Выяснить, требуется ли усиленная подпись.
      var needAdvancedSign = Functions.Module.NeedAdvancedSign(Sungero.Company.Employees.As(_obj.Performer), _obj.BlockUid,
                                                                      new List<string>(),
                                                                      new List<string>() { Constants.VacationAlertTask.EmployeeConfirmationBlockId });
      _obj.SignatureType = needAdvancedSign ? SignatureType.Approval.ToString() : SignatureType.Endorsing.ToString();
    }

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      if (Equals(_obj.Result.Value, VacationApprovalAssignment.Result.Shift))
      {
        var shiftTask = VacationShiftTasks.As(_obj.ShiftTask);
        var alertTask = VacationAlertTasks.As(_obj.Task);
        var shiftedVacations = new List<IVacation>();
        shiftedVacations.Add(shiftTask.Vacation1);
        if (shiftTask.Vacation2 != null)
          shiftedVacations.Add(shiftTask.Vacation2);
        if (shiftTask.Vacation3 != null)
          shiftedVacations.Add(shiftTask.Vacation3);
        // Для всех отпусков, которые были в задаче на подтверждение, но не попали в перенос,
        //   сбросить дату отправки уведомления, чтобы по ним пришло отдельное задание на подтверждение
        var notShiftedVacations = alertTask.Vacations.Where(l => !shiftedVacations.Contains(l.Vacation));
        foreach (var notShiftedVacation in notShiftedVacations)
        {
          var vacation = notShiftedVacation.Vacation;
          var lockInfo = Locks.GetLockInfo(vacation);
          if (!lockInfo.IsLocked)
          {
            vacation.NoteDate = null;
            vacation.Save();
          }
        }
      }
    }
  }

  partial class VacationApprovalAssignmentSubstitutePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> SubstituteFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var employee = Sungero.Company.Employees.As(_obj.Performer);
      return query.Where(l => !l.Equals(employee));
    }
  }

}