using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;

namespace DirRX.HRLite
{
  partial class AcquaintanceControlReportServerHandlers
  {

    public virtual IQueryable<Sungero.Company.IBusinessUnit> GetBusinessUnit()
    {
      var department = AcquaintanceControlReport.Departments.FirstOrDefault();
      var businessUnitId = 0;
      
      if (department != null)
      {
        var businessUnit = department.BusinessUnit;
        
        if (businessUnit != null)
          businessUnitId = department.BusinessUnit.Id;
      }
      
      return Sungero.Company.BusinessUnits.GetAll(w => w.Id == businessUnitId);
    }

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      var reportSessionId = AcquaintanceControlReport.ReportSessionId;
      // Данные о документах и приложениях
      AcquaintanceControlReport.Document = AcquaintanceControlReport.AcquaintanceTask.DocumentGroup.InternalDocumentBases.FirstOrDefault().Name;
      AcquaintanceControlReport.AddendaList = AcquaintanceControlReport.AcquaintanceTask.AddendaGroup.InternalDocumentBases.Any() ?
        DirRX.HRLite.Reports.Resources.AcquaintanceControlReport.AddendaSubTitile + Environment.NewLine +
        string.Join(Environment.NewLine, AcquaintanceControlReport.AcquaintanceTask.AddendaGroup.InternalDocumentBases.Select(d => " - " + d.Name).ToList()) : string.Empty;
      // Данные об исполнителях
      var dataTable = new List<Structures.AcquaintanceControlReport.TableLine>();
      var assignments = HRAcquaintanceAssignments.GetAll(a => Equals(a.Task, AcquaintanceControlReport.AcquaintanceTask));
      var employees = assignments.Select(a => Employees.As(a.Performer)).Distinct().ToList();
      var deprtments = AcquaintanceControlReport.Departments.ToList();
      if (deprtments.Any())
        employees = employees.Where(p => deprtments.Any(d => d.Equals(p.Department))).ToList();
      foreach (var employee in employees)
      {
        // Статус взять по последнему из заданий сотрудника для корректной обработки возвратов на повторное ознакомление.
        var assignmentResult = assignments.Where(a => employee.Equals(Employees.As(a.Performer))).OrderByDescending(d => d.Created).Select(r => r.Result).FirstOrDefault();
        // Исключить подписавших, если это необходимо.
        if (!(AcquaintanceControlReport.ExcludeSigned.Value && assignmentResult.HasValue && assignmentResult.Value == HRAcquaintanceAssignment.Result.Sign))
        {
          var newLine = Structures.AcquaintanceControlReport.TableLine.Create();
          newLine.ReportSessionId = reportSessionId;
          newLine.Department = employee.Department.DisplayValue;
          newLine.Performer = employee.DisplayValue;
          newLine.JobTitle = employee.JobTitle == null ? string.Empty : employee.JobTitle.DisplayValue;
          // Порядок сортировки: сначала не выполнившие задания, потом отказавшиеся, потом подписавшие.
          if (!assignmentResult.HasValue)
          {
            newLine.Result = Reports.Resources.AcquaintanceControlReport.AcquaintanceNotPerformed;
            newLine.ResultOrder = 1;
          }
          else if (assignmentResult.Value == HRAcquaintanceAssignment.Result.Refuse)
          {
            newLine.Result = Reports.Resources.AcquaintanceControlReport.AcquaintanceRefused;
            newLine.ResultOrder = 2;
          }
          else
          {
            newLine.Result = Reports.Resources.AcquaintanceControlReport.AcquaintanceSigned;
            newLine.ResultOrder = 3;
          }
          dataTable.Add(newLine);
        }
      }
      Sungero.Docflow.PublicFunctions.Module.WriteStructuresToTable(Constants.AcquaintanceControlReport.SourceTableName, dataTable);
    }

    public override void AfterExecute(Sungero.Reporting.Server.AfterExecuteEventArgs e)
    {
      Sungero.Docflow.PublicFunctions.Module.DeleteReportData(Constants.AcquaintanceControlReport.SourceTableName, AcquaintanceControlReport.ReportSessionId);
    }
  }

}