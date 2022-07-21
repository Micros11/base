using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DismissalTask;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Server
{
  partial class DismissalTaskFunctions
  {
    /// <summary>
    /// Получить повторного исполнителя этапа обходного листа.
    /// </summary>
    /// <param name="recipient">Исполнитель.</param>
    /// <returns>Переданный исполнитель, если он выполнил задание прошлого этапа с результатом  "К возврату", иначе null.</returns>
    public IRecipient GetGoRoundRecipient(IRecipient recipient)
    {
      var prevPerformers = DismissalGoRoundAssignments.GetAll(a => Equals(a.Task, _obj) &&
                                                              a.Result == DirRX.HRManagement.DismissalGoRoundAssignment.Result.ClaimDebts)
        .Select(a => a.Performer);
      return prevPerformers.Contains(recipient) ? recipient : Recipients.Null;
    }
    
    /// <summary>
    /// Получить задания, выполненные с результатом Есть долги.
    /// </summary>
    /// <returns>Список заданий.</returns>
    public IQueryable<IDismissalGoRoundAssignment> GetAssignmentWithClaimDebtsResult()
    {
      return DismissalGoRoundAssignments.GetAll(a => Equals(a.Task, _obj) && a.Result == DirRX.HRManagement.DismissalGoRoundAssignment.Result.ClaimDebts);
    }
    
    /// <summary>
    /// Получить инструкцию для обхода и обходного листа.
    /// </summary>
    /// <returns>Инструкция для обхода.</returns>
    public string GetGoRoundInstruction()
    {
      var instruction = string.Empty;
      var instrItems = new List<string>();
      var assignments = this.GetAssignmentWithClaimDebtsResult();
      var i = 1;
      foreach (var assignment in assignments)
      {
        var serviceName = string.Empty;
        switch (assignment.BlockUid)
        {
          case Constants.DismissalTask.SBServiceBlockId:
            serviceName = DismissalTasks.Resources.ServiceSBName;
            break;
          case Constants.DismissalTask.ITServiceBlockId:
            serviceName = DismissalTasks.Resources.ServiceITName;
            break;
          case Constants.DismissalTask.AHOServiceBlockId:
            serviceName = DismissalTasks.Resources.ServiceAHOName;
            break;
        }
        instrItems.Add(string.Format("{0}. {1} ({2}): {3}", i.ToString(), assignment.Performer.Name, serviceName, assignment.ActiveText));
        i++;
      }
      if (instrItems.Any())
        instruction = DismissalTasks.Resources.EmployeeGoRoundAssignmentIntructionFormat(string.Join(Environment.NewLine, instrItems.ToArray()));
      return instruction;
    }
    
    /// <summary>
    /// Получить заявление об увольнении сотрудника.
    /// </summary>
    /// <returns>Заявление.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHRDocument GetEmployeeStatement()
    {
      return _obj.Statement;
    }
    
    /// <summary>
    /// Получить приказ об увольнении сотрудника.
    /// </summary>
    /// <returns>Приказ.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHROrder GetEmployeeOrder()
    {
      return _obj.Order;
    }
    
    /// <summary>
    /// Создать приказ на увольнение.
    /// </summary>
    /// <returns>Приказ.</returns>
    [Remote]
    public IHROrder CreateDismissalOrder()
    {
      var document = Functions.Module.CreateOrder(DocKind.DismissalOrderKind, _obj.Employee, _obj.Statement);
      document.EventDate = _obj.DismissalDate;
      document.EmployeeContract = Functions.Module.GetEmployeeContract(_obj.Employee);
      document.Save();
      return document;
    }
    
    /// <summary>
    /// Переформировать приказ на увольнение.
    /// </summary>
    /// <param name="order">Приказ.</param>
    [Remote]
    public void ReCreateDismissalOrder(IHROrder order)
    {
      Functions.Module.RegenerateOrder(order);
    }
    
    /// <summary>
    /// Создать заявление на увольнение.
    /// </summary>
    /// <returns>Заявление.</returns>
    public IHRDocument CreateDismissalStatement()
    {
      var document = Functions.Module.CreateStatement(DocKind.DismissalStatementKind, _obj.Employee, _obj.Started.Value);
      document.DismissalDate = _obj.DismissalDate;
      document.Save();
      return document;
    }
  }
}