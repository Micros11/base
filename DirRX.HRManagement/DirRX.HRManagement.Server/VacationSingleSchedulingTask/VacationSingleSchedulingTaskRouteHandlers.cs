using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using Sungero.Workflow;
using DirRX.HRManagement.VacationSingleSchedulingTask;

namespace DirRX.HRManagement.Server
{
  partial class VacationSingleSchedulingTaskRouteHandlers
  {

    #region Преобразовать в pdf с отметкой об ЭП
    public virtual void Script18Execute()
    {
      // Получить заявление в Pdf с подстановкой штампа сотрудника
      var statement = _obj.Statement;
      // Задать списки для сотрудника и якорей подписей
      var employees = new List<IEmployee>() { statement.Employee };
      var employeeAnchors = new List<string>() { HRManagement.Resources.EmployeeAnchor };
      PublicFunctions.Module.ConvertToPdfWithSignatureMarks(statement, employees, employeeAnchors);

      // Получить приказ в Pdf с подстановкой штампов
      var order = _obj.Order;
      // Задать списки для подписантов со стороны организации и якорей подписей
      var ourSignatories = new List<IEmployee>() { order.OurSignatory };
      var ourSignatoryAnchors = new List<string>() { HRManagement.Resources.ManagerAnchor };
      PublicFunctions.Module.ConvertToPdfWithSignatureMarks(order, employees, employeeAnchors, ourSignatories, ourSignatoryAnchors);
    }
    #endregion
    
    #region Планирование отпуска сотрудником
    public virtual void CompleteAssignment3(DirRX.HRManagement.ISingleEmployeeSchedulingAssignment assignment, DirRX.HRManagement.Server.SingleEmployeeSchedulingAssignmentArguments e)
    {
      Functions.VacationSingleSchedulingTask.FillVacationDataFromAssignmentToTask(_obj, assignment);
    }

    public virtual void StartAssignment3(DirRX.HRManagement.ISingleEmployeeSchedulingAssignment assignment, DirRX.HRManagement.Server.SingleEmployeeSchedulingAssignmentArguments e)
    {
      var employeeVacationDurations = Functions.Module.GetEmployeeUnScheduledDurations(_obj.Employee, _obj.Year.Value);
      foreach (var vacationDuration in employeeVacationDurations)
      {
        var assignmentDurationRow = assignment.VacationDurations.AddNew();
        var existedVacations = Vacations.GetAll(l => Equals(l.VacationKind, vacationDuration.VacationKind) &&
                                                Equals(l.Employee, _obj.Employee) &&
                                                l.Year.Value.Year == _obj.Year.Value.Year &&
                                                l.Status != DirRX.HRManagement.Vacation.Status.OnApproval &&
                                                l.Status != DirRX.HRManagement.Vacation.Status.Shifted &&
                                                l.Status != DirRX.HRManagement.Vacation.Status.Canceled);
        var existedDays = existedVacations.Any() ? existedVacations.Sum(x => x.VacationDuration.Value) : 0;
        assignmentDurationRow.VacationKind = vacationDuration.VacationKind;
        assignmentDurationRow.DaysCount = vacationDuration.DaysCount - existedDays;
      }
      Functions.SingleEmployeeSchedulingAssignment.FillInstruction(assignment);
      Functions.VacationSingleSchedulingTask.FillVacationDataFromTaskToAssignment(_obj, assignment);
    }

    public virtual void StartBlock3(DirRX.HRManagement.Server.SingleEmployeeSchedulingAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      if (_obj.Vacations.Any())
        e.Block.Subject = VacationSingleSchedulingTasks.Resources.EmployeeReworkSubject;
      else
        e.Block.Subject = VacationSingleSchedulingTasks.Resources.EmployeeSchedulingSubjectFormat(_obj.Year.Value.Year);
    }
    #endregion
    
    #region Внесение изменений в график отпусков
    public virtual void Script12Execute()
    {
      foreach (var vacation in _obj.Vacations)
      {
        var date = Structures.Vacation.VacationDates.Create(vacation.DateBegin.Value, vacation.DateEnd.Value, vacation.Duration.Value, DirRX.HRManagement.Constants.Module.DateStatus.New, vacation.VacationKind);
        Functions.Module.CreateVacation(date, _obj.Employee, _obj.Year.Value, HRManagement.Vacation.Status.Active);
      }
    }

    public virtual bool Decision15Result()
    {
      return Functions.Module.IsAllPaperVacationDoc(_obj.Employee.Department.BusinessUnit) || !Functions.Module.IsEmployeeUsingESS(_obj.Employee);
    }
    
    public virtual void StartBlock16(DirRX.HRManagement.Server.SinglePrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationSingleSchedulingTasks.Resources.SignAllPaperDocAssignmentSubjectFormat(employee.Name);
    }

    public virtual void StartBlock10(DirRX.HRManagement.Server.SingleSignDocsAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = VacationSingleSchedulingTasks.Resources.SignEmployeeAssignmentSubject;
    }

    public virtual void StartBlock8(DirRX.HRManagement.Server.SinglePrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationSingleSchedulingTasks.Resources.VacationDocsReWorkAssignmentSubjectFormat(employee.Name);
    }

    public virtual void StartBlock7(DirRX.HRManagement.Server.SingleSignDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetStaffChief(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationSingleSchedulingTasks.Resources.SignAssignmentSubjectFormat(employee.Name);
    }

    public virtual void CompleteAssignment6(DirRX.HRManagement.ISinglePrepareDocsAssignment assignment, DirRX.HRManagement.Server.SinglePrepareDocsAssignmentArguments e)
    {
      _obj.Order = assignment.Order;
    }

    public virtual void StartBlock6(DirRX.HRManagement.Server.SinglePrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationSingleSchedulingTasks.Resources.PrepareOrderAssignmentSubjectFormat(employee.Name);
      var order = Functions.VacationSingleSchedulingTask.CreateVacationOrder(_obj);
      e.Block.Order = order;
    }
    
    public virtual void StartBlock5(DirRX.HRManagement.Server.SingleSignDocsAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = VacationSingleSchedulingTasks.Resources.SignStatementAssignmentSubject;
      
      var document = Functions.VacationSingleSchedulingTask.CreateVacationStatement(_obj);
      _obj.Statement = document;
    }
    #endregion

    #region Согласование заявления
    public virtual void StartAssignment4(DirRX.HRManagement.ISingleEmployeeSchedulingAssignment assignment, DirRX.HRManagement.Server.SingleEmployeeSchedulingAssignmentArguments e)
    {
      Functions.VacationSingleSchedulingTask.FillVacationDataFromTaskToAssignment(_obj, assignment);
    }

    public virtual void StartBlock4(DirRX.HRManagement.Server.SingleEmployeeSchedulingAssignmentArguments e)
    {
      e.Block.Performers.Add(Functions.Module.GetManager(_obj.Employee));
      e.Block.Instruction = Functions.Module.TrimInstruction(VacationSingleSchedulingTasks.Resources.ManagerCheckInstruction);
      e.Block.Subject = VacationSingleSchedulingTasks.Resources.ManagerCheckSubjectFormat(_obj.Employee.Name);
    }
    #endregion

  }
}