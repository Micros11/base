using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HiringTask;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Server
{
  partial class HiringTaskFunctions
  {

    /// <summary>
    /// Создать сотрудника из персоны.
    /// </summary>
    [Remote]
    public void CreateEmployee()
    {
      var employee = Sungero.Company.Employees.Create();
      employee.Person = _obj.Person;
      employee.Department = _obj.Department;
      employee.JobTitle = _obj.JobTitle;
      employee.NeedNotifyExpiredAssignments = false;
      employee.NeedNotifyNewAssignments = false;
      employee.Save();
      
      _obj.Employee = employee;
      _obj.EmployeeAttachmentGroup.Employees.Add(employee);
    }

    /// <summary>
    /// Получить трудовой договор сотрудника.
    /// </summary>
    /// <returns>Трудовой договор.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHRDocument GetEmployeeContract()
    {
      if (_obj.EmploymentContract != null)
        return _obj.EmploymentContract;
      else
        return Functions.Module.GetEmployeeContract(_obj.Employee);
    }
    
    /// <summary>
    /// Получить приказ о приеме сотрудника.
    /// </summary>
    /// <returns>Приказ.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHROrder GetEmployeeOrder()
    {
      return _obj.Order;
    }
    
    /// <summary>
    /// Получить приложение к трудовому договору.
    /// </summary>
    /// <returns>Приложение к трудовому договору.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHRDocument GetContractAddendum()
    {
      return _obj.ContractAddendum;
    }
    
    /// <summary>
    /// Создать приказ на прием.
    /// </summary>
    /// <param name="baseDoc">Документ-основание.</param>
    /// <returns>Документ.</returns>
    public IHROrder CreateHiringOrder(Sungero.Docflow.IOfficialDocument baseDoc)
    {
      var document = Functions.Module.CreateOrder(DocKind.HiringOrderKind, _obj.Employee, baseDoc);
      document.EventDate = _obj.EmploymentDate;
      document.Save();
      return document;
    }
    
    /// <summary>
    /// Переформировать приказ на прием.
    /// </summary>
    /// <param name="order">Приказ.</param>
    [Remote]
    public void ReCreateHiringOrder(IHROrder order)
    {
      Functions.Module.RegenerateOrder(order);
    }
  }
}