using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.Vacation;
using HRRoles = DirRX.HRManagement.Constants.Module.HRRoles;

namespace DirRX.HRManagement.Client
{
  partial class VacationActions
  {

    public virtual void ShowAllBoundTasks(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var tasks = Functions.Vacation.Remote.GetAllTasksForVacation(_obj);
      tasks.ShowModal();
    }

    public virtual bool CanShowAllBoundTasks(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted;
    }

    public virtual void StartVacationAlertDate(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.Module.Remote.CreateAndStartAlertTask(_obj);
      _obj.NoteDate = Calendar.Today;
      _obj.Save();
      Dialogs.NotifyMessage(Vacations.Resources.СonfirmationTaskSentSuccessfully);
    }

    public virtual bool CanStartVacationAlertDate(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return Functions.Module.Remote.IsDebugEnabled() && !_obj.State.IsInserted && _obj.Status == DirRX.HRManagement.Vacation.Status.Active;
    }

  }

}