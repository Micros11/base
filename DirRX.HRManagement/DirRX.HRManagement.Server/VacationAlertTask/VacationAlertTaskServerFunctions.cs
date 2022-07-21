using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.VacationAlertTask;

namespace DirRX.HRManagement.Server
{
  partial class VacationAlertTaskFunctions
  {
    /// <summary>
    /// Создать и запустить асинхронный процесс по удалению замещения.
    /// </summary>
    [Remote]
    public void DeleteSubstitutionAsync()
    {
      var asyncDelete = AsyncHandlers.DeleteSubstitution.Create();
      var vacation = _obj.Vacation;
      asyncDelete.SubstitutionID = _obj.Substitution.Id;
      asyncDelete.TaskID = _obj.Id;
      asyncDelete.ExecuteAsync();
    }
    
    /// <summary>
    /// Создать и запустить асинхронный процесс по созданию замещения.
    /// </summary>
    /// <param name="substitute">Замещающий.</param>
    [Remote]
    public void CreateSubstitutionAsync(IEmployee substitute)
    {
      var asyncCreate = AsyncHandlers.CreateSubstitution.Create();
      var vacation = _obj.Vacation;
      asyncCreate.EmployeeID = vacation.Employee.Id;
      asyncCreate.SubstituteID = substitute.Id;
      asyncCreate.From = vacation.StartDate.Value;
      asyncCreate.To = vacation.FinDate.Value;
      asyncCreate.Comment = VacationAlertTasks.Resources.Vacation;
      asyncCreate.TaskID = _obj.Id;
      asyncCreate.ExecuteAsync();
    }

    /// <summary>
    /// Создать и запустить асинхронный процесс по изменению замещения.
    /// </summary>
    /// <param name="newEmployee">Новый замещающий.</param>
    [Remote]
    public void ChangeSubstitutionAsync(IEmployee newEmployee)
    {
      var asyncSubstitutionChange = AsyncHandlers.ChangeSubstitution.Create();
      asyncSubstitutionChange.SubstitutionID = _obj.Substitution.Id;
      asyncSubstitutionChange.NewSubstituteID = newEmployee.Id;
      asyncSubstitutionChange.TaskID = _obj.Id;
      asyncSubstitutionChange.ExecuteAsync();
    }
    
    /// <summary>
    /// Получить уведомление о начале отпуска.
    /// </summary>
    /// <returns>Уведомление.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHRDocument GetEmployeeNotice()
    {
      return _obj.Notification;
    }

  }
}