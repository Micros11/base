using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.VacationSingleSchedulingTask;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Server
{
  partial class VacationSingleSchedulingTaskFunctions
  {
    /// <summary>
    /// Получить приказ об отпуске.
    /// </summary>
    /// <returns>Приказ.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHROrder GetEmployeeOrder()
    {
      return _obj.Order;
    }
    
    /// <summary>
    /// Получить заявление о переносе отпуска.
    /// </summary>
    /// <returns>Заявление.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHRDocument GetEmployeeStatement()
    {
      return _obj.Statement;
    }
    
    /// <summary>
    /// Создать заявление на отпуск.
    /// </summary>
    /// <returns>Заявление.</returns>
    public IHRDocument CreateVacationStatement()
    {
      var document = Functions.Module.CreateStatement(DocKind.VacationStatementKind, _obj.Employee, _obj.Started.Value, _obj.Year.Value);
      var vacationPartsText = new List<string>();
      foreach (var vacation in _obj.Vacations)
      {
        var vacationKindName = vacation.VacationKind.Name.ToLower();
        vacationPartsText.Add(Resources.VacationStatementBodyPartFormat(vacationKindName, vacation.Duration, vacation.DateBegin.Value.ToLongDateString()));
      }
      document.BodyText = string.Join(", ", vacationPartsText);
      document.Save();
      return document;
    }
    
    /// <summary>
    /// Создать приказ об отпуске.
    /// </summary>
    /// <returns>Приказ.</returns>
    [Remote]
    public IHROrder CreateVacationOrder()
    {
      var guid = DocKind.VacationOrderKind;
      var document = Functions.Module.CreateOrder(guid, _obj.Employee, _obj.Statement, _obj.Year.Value);
      var vacationPartsText = new List<string>();
      var totalDuration = 0;
      foreach (var vacation in _obj.Vacations)
      {
        var vacationKindName = vacation.VacationKind.Name.ToLower();
        vacationPartsText.Add(Resources.VacationOrderBodyPartFormat(vacationKindName, vacation.Duration, vacation.DateBegin.Value.ToLongDateString(), vacation.DateEnd.Value.ToLongDateString()));
        totalDuration += vacation.Duration.Value;
      }
      document.VacationYear = _obj.Year.Value.Year.ToString();
      document.VacationDays = Resources.VacationOrderTotalDurationTextFormat(totalDuration);
      document.BodyText = string.Join(";" + Environment.NewLine, vacationPartsText);
      document.Save();
      return document;
    }
    
    /// <summary>
    /// Переформировать приказ об отпуске.
    /// </summary>
    /// <param name="order">Приказ.</param>
    [Remote]
    public void ReCreateVacationOrder(IHROrder order)
    {
      Functions.Module.RegenerateOrder(order);
    }
    
    /// <summary>
    /// Перенести данные о датах отпуска из задачи в задание.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    public void FillVacationDataFromTaskToAssignment(ISingleEmployeeSchedulingAssignment assignment)
    {
      assignment.Vacations.Clear();
      foreach (var vacationRow in _obj.Vacations)
      {
        var assignmentVacationRow = assignment.Vacations.AddNew();
        assignmentVacationRow.VacationKind = vacationRow.VacationKind;
        assignmentVacationRow.DateBegin = vacationRow.DateBegin;
        assignmentVacationRow.DateEnd = vacationRow.DateEnd;
        assignmentVacationRow.Duration = vacationRow.Duration;
      }
    }
    
    /// <summary>
    /// Перенести данные о датах отпуска из задания в задачу.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    public void FillVacationDataFromAssignmentToTask(ISingleEmployeeSchedulingAssignment assignment)
    {
      _obj.Vacations.Clear();
      foreach (var vacationRow in assignment.Vacations)
      {
        var taskVacationRow = _obj.Vacations.AddNew();
        taskVacationRow.VacationKind = vacationRow.VacationKind;
        taskVacationRow.DateBegin = vacationRow.DateBegin;
        taskVacationRow.DateEnd = vacationRow.DateEnd;
        taskVacationRow.Duration = vacationRow.Duration;
      }
    }
  }
}