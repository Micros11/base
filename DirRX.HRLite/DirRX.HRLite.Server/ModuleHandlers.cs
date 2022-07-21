using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.HRLite.Server
{
  partial class ActiveTasksFolderHandlers
  {

    public virtual bool IsActiveTasksVisible()
    {
      return Functions.Module.IsCurrentUserInHRSpecialist();
    }

    public virtual IQueryable<Sungero.Workflow.ITask> ActiveTasksDataQuery(IQueryable<Sungero.Workflow.ITask> query)
    {
      // HACK используется репозиторий в качестве источника данных вместо query чтобы получить задачи всех пользователей.
      var allHRTasks = Sungero.Workflow.Tasks.GetAll(t => DirRX.HRLite.HRAcquaintanceTasks.Is(t) || DirRX.HRLite.InformingTasks.Is(t) || DirRX.HRLite.SignTasks.Is(t))
        .Where(t => Sungero.Workflow.Tasks.As(t).Status == Sungero.Workflow.Task.Status.InProcess || Sungero.Workflow.Tasks.As(t).Status == Sungero.Workflow.Task.Status.Completed);
      
      if (_filter == null)
        return allHRTasks;
      
      if (_filter.AcquaintanceTask || _filter.SignTask || _filter.InformingTask)
      {
        // Задача на ознакомление.
        if (!_filter.AcquaintanceTask)
          allHRTasks = allHRTasks.Where(t => !DirRX.HRLite.HRAcquaintanceTasks.Is(t));
        
        // Задача на подписание.
        if (!_filter.SignTask)
          allHRTasks = allHRTasks.Where(t => !DirRX.HRLite.SignTasks.Is(t));
        
        // Задача на информирование.
        if (!_filter.InformingTask)
          allHRTasks = allHRTasks.Where(t => !DirRX.HRLite.InformingTasks.Is(t));
      }
      
      if (_filter.InProcess || _filter.Completed)
      {
        // В работе.
        if (!_filter.InProcess)
          allHRTasks = allHRTasks.Where(t => t.Status != Sungero.Workflow.Task.Status.InProcess);
        
        // Завершена.
        if (!_filter.Completed)
          allHRTasks = allHRTasks.Where(t => t.Status != Sungero.Workflow.Task.Status.Completed);
      }
      
      // Задачи, отправленные сегодня.
      if (_filter.Today)
        allHRTasks = allHRTasks.Where(t => t.Created.Value.Date == Calendar.Today.Date);
      
      // Задачи, отправленные за последние 30 дней.
      if (_filter.ThirtyDays)
        allHRTasks = allHRTasks.Where(t => Calendar.Today.Date.AddDays(-30) < t.Created.Value.Date);
      
      // Задачи, отправленные за последние 90 дней.
      if (_filter.NinetyDays)
        allHRTasks = allHRTasks.Where(t => Calendar.Today.Date.AddDays(-90) < t.Created.Value.Date);
      
      // Задачи, отправленные за последние 180 дней.
      if (_filter.OneHundretEightyDays)
        allHRTasks = allHRTasks.Where(t => Calendar.Today.Date.AddDays(-180) < t.Created.Value.Date);
      
      // Задачи, запущенные текущим пользователем.
      if (_filter.MyTasks)
        allHRTasks = allHRTasks.Where(t => t.Author == Sungero.CoreEntities.Users.Current);
      
      return allHRTasks;
    }
  }

  partial class OnControlFolderHandlers
  {

    public virtual IQueryable<Sungero.Workflow.INotice> OnControlDataQuery(IQueryable<Sungero.Workflow.INotice> query)
    {
      return query.Where(a => AcquaintanceDeadlineNotices.Is(a) || SignDeadlineNotices.Is(a));
    }

    public virtual bool IsOnControlVisible()
    {
      return Functions.Module.IsCurrentUserInHRSpecialist();
    }
  }

  partial class HRLiteHandlers
  {
  }
}