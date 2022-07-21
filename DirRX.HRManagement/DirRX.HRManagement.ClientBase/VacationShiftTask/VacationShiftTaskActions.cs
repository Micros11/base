using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.VacationShiftTask;

namespace DirRX.HRManagement.Client
{
  partial class VacationShiftTaskActions
  {

    public virtual bool CanShowChart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void ShowChart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.Module.VacationScheduleChart(_obj.Year.Value, _obj.Employee);
    }

    public virtual bool CanCheckVacationSchedule(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public override void Abort(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var abortReason = Functions.Module.GetReasonBeforeAbort();
      if (abortReason != string.Empty)
      {
        _obj.AbortingReason = abortReason;
        _obj.Save();
        
        // Подписать уведомление о начале отпуска, если задача на перенос стартована из задания на подтверждение дат отпуска.
        var currentEmployee = Employees.Current;
        // TODO при прекращении задачи не тем сотрудником, по которому стартована задача, подпись ставиться вообще не будет
        //      возможно стоит по другому обрабатывать эту ситуацию.
        if (currentEmployee != null && Equals(currentEmployee, _obj.Employee)) {
          // Выяснить, требуется ли усиленная подпись.
          // Имитируется ситуация, как если бы нажали "Прекратить" в задании на доработку.
          var needAdvancedSign = Functions.Module.Remote.NeedAdvancedSign(_obj.Employee, Constants.VacationShiftTask.EmployeeReWorkAssignmentBlockId,
                                                                          new List<string>(),
                                                                          new List<string>() { Constants.VacationShiftTask.EmployeeReWorkAssignmentBlockId });
          
          Functions.Module.SignVacationNotice(_obj, Users.Current, e, needAdvancedSign);
        }
        base.Abort(e);
      }
    }

    public override bool CanAbort(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanAbort(e);
    }

  }

}