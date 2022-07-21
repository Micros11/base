using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationSchedulingTask;

namespace DirRX.HRManagement.Server
{
  partial class VacationSchedulingTaskFunctions
  {
    /// <summary>
    /// Прекратить все подзадачи по планированию в подразделениях.
    /// </summary>
    /// <param name="abortReason">Причина прекращения.</param>
    [Remote]
    public void AbortSubTasks(string abortReason)
    {
      var subTaskAbortAsync = AsyncHandlers.AbortDepartmentScheduling.Create();
      subTaskAbortAsync.MainTaskID = _obj.Id;
      subTaskAbortAsync.AbortReason = abortReason;
      subTaskAbortAsync.ExecuteAsync();
    }

    /// <summary>
    /// Найти все задачи на формирование графика отпусков в подразделениях, связанные с основной задачей на планирование.
    /// </summary>
    /// <returns>Задачи на формирование графика отпусков в подразделениях.</returns>
    [Remote]
    public virtual IQueryable<Sungero.Workflow.ITask> FindDepartmentSchedulingTasks()
    {
      return VacationDepartmentSchedulingTasks.GetAll(l => Equals(l.MainSchedulingTask, _obj));
    }
    
    /// <summary>
    /// Заполнить свойства задачи на формирование графика отпусков по подразделению.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <param name="department">Подразделение.</param>
    [Remote]
    public virtual void FillVacationDepartmentSchedulingTaskFields(Sungero.Workflow.ITask task, Sungero.Company.IDepartment department)
    {
      var deptask = VacationDepartmentSchedulingTasks.As(task);
      deptask.Department = department;
      deptask.MainSchedulingTask = _obj;
      deptask.Year = _obj.Year;
    }
  }
}