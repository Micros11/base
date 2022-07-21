using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.VacationManagerNotice;

namespace DirRX.HRManagement.Client
{
  partial class VacationManagerNoticeActions
  {
    public virtual void ChangeSubstitution(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var dialog = Dialogs.CreateInputDialog(VacationAlertTasks.Resources.SubstitutionChangeSubject);
      var alertTask = VacationAlertTasks.As(_obj.Task);
      var substitution = alertTask.Substitution;
      var currentSubstitute = Employees.Null;
      if (substitution != null)
        currentSubstitute = Employees.As(alertTask.Substitution.Substitute);
      var employee = alertTask.Vacation.Employee;
      var newEmployee = dialog.AddSelect(VacationAlertTasks.Resources.EmployeeFieldName, false, currentSubstitute).Where(l => !l.Equals(employee));
      if (dialog.Show() == DialogButtons.Ok && !Equals(currentSubstitute, newEmployee.Value))
      {
        if (substitution != null)
        {
          if (newEmployee.Value != null)
          {
            Functions.VacationAlertTask.Remote.ChangeSubstitutionAsync(alertTask, newEmployee.Value);
            e.AddInformation(VacationAlertTasks.Resources.SubstitutionWillBeChanged);
          }
          else
          {
            Functions.VacationAlertTask.Remote.DeleteSubstitutionAsync(alertTask);
            e.AddInformation(VacationAlertTasks.Resources.SubstitutionWillBeDeleted);
          }
        }
        else
        {
          Functions.VacationAlertTask.Remote.CreateSubstitutionAsync(alertTask, newEmployee.Value);
          e.AddInformation(VacationAlertTasks.Resources.SubstitutionWillBeCreated);
        }
      }
    }

    public virtual bool CanChangeSubstitution(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      var task = VacationAlertTasks.As(_obj.Task);
      return _obj.BlockUid == Constants.VacationAlertTask.ManagerSubstitutionNoticeBlockId;
    }
  }
}