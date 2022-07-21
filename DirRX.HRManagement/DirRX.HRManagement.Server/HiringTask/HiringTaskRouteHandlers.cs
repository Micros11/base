using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.HRManagement.HiringTask;

namespace DirRX.HRManagement.Server
{
  partial class HiringTaskRouteHandlers
  {
    #region Преобразование в Pdf.

    public virtual void Script81Execute()
    {
      // Получить приказ в Pdf с подстановкой штампов
      var order = _obj.Order;
      // Задать списки для сотрудника и якорей подписей 
      var employees = new List<IEmployee>() { order.Employee };
      var employeeAnchors = new List<string>() { HRManagement.Resources.EmployeeAnchor };
      
      // Задать списки для подписантов со стороны организации и якорей подписей 
      var ourSignatories = new List<IEmployee>() { order.OurSignatory };
      var ourSignatoryAnchors = new List<string>() { HRManagement.Resources.ManagerAnchor };
      
      PublicFunctions.Module.ConvertToPdfWithSignatureMarks(order, employees, employeeAnchors, ourSignatories, ourSignatoryAnchors);
      
      // Трудовой договор и лист ознакомления преобразовать только в том случае, если они не предполагаются в бумажном виде
      if (!Functions.Module.IsOnlyBilStaffDocPaper(_obj.Department.BusinessUnit))
      {
        // Получить трудовой договор в Pdf с подстановкой штампов
        var employmentContract = _obj.EmploymentContract;
        PublicFunctions.Module.ConvertToPdfWithSignatureMarks(employmentContract, employees, employeeAnchors, ourSignatories, ourSignatoryAnchors);
        // Получить лист ознакомления в Pdf с подстановкой штампов
        var contractAddendum = Functions.HiringTask.GetContractAddendum(_obj);
        if (contractAddendum != null)
          PublicFunctions.Module.ConvertToPdfWithSignatureMarks(contractAddendum, employees, employeeAnchors);
      }     
    }
    
    #endregion

    #region Подготовительный этап - задания в ответственные службы.
    
    public virtual void StartBlock4(DirRX.HRManagement.Server.HiringAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetSecurityResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = HiringTasks.Resources.PreparingAssignmentSubjectFormat(_obj.Person.Name);
    }
    
    public virtual void StartAssignment4(DirRX.HRManagement.IHiringAssignment assignment, DirRX.HRManagement.Server.HiringAssignmentArguments e)
    {
      assignment.Deadline = _obj.EmploymentDate.Value;
    }
    
    public virtual void StartBlock19(DirRX.HRManagement.Server.HiringAssignmentArguments e)
    {
      if (_obj.Department != null && _obj.Department.Manager != null)
        e.Block.Performers.Add(_obj.Department.Manager);
      e.Block.Subject = HiringTasks.Resources.PreparingAssignmentSubjectFormat(_obj.Person.Name);
    }

    public virtual void StartAssignment19(DirRX.HRManagement.IHiringAssignment assignment, DirRX.HRManagement.Server.HiringAssignmentArguments e)
    {
      assignment.Deadline = _obj.EmploymentDate.Value;
    }
    
    public virtual void StartBlock48(DirRX.HRManagement.Server.HiringPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = HiringTasks.Resources.PreparingAssignmentSubjectFormat(_obj.Person.Name);
    }
    
    public virtual void StartAssignment48(DirRX.HRManagement.IHiringPrepareDocsAssignment assignment, DirRX.HRManagement.Server.HiringPrepareDocsAssignmentArguments e)
    {
      assignment.Deadline = _obj.EmploymentDate;
    }
    
    public virtual void CompleteAssignment48(DirRX.HRManagement.IHiringPrepareDocsAssignment assignment, DirRX.HRManagement.Server.HiringPrepareDocsAssignmentArguments e)
    {
      _obj.EmploymentContract = assignment.EmploymentContract;
      _obj.ProcPersonalDataAgreement = assignment.ProcPersonalDataAgreement;
      _obj.JoiningStatement = assignment.JoiningStatement;
      _obj.Order = assignment.Order;
      _obj.ContractAddendum = assignment.ContractAddendum;
    }
    
    public virtual void StartBlock54(DirRX.HRManagement.Server.HiringAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetITResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = HiringTasks.Resources.PreparingAssignmentSubjectFormat(_obj.Person.Name);
    }

    public virtual void StartAssignment54(DirRX.HRManagement.IHiringAssignment assignment, DirRX.HRManagement.Server.HiringAssignmentArguments e)
    {
      assignment.Deadline = _obj.EmploymentDate.Value;
    }
    
    public virtual void StartBlock56(DirRX.HRManagement.Server.HiringAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetSupplyResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = HiringTasks.Resources.PreparingAssignmentSubjectFormat(_obj.Person.Name);
    }

    public virtual void StartAssignment56(DirRX.HRManagement.IHiringAssignment assignment, DirRX.HRManagement.Server.HiringAssignmentArguments e)
    {
      assignment.Deadline = _obj.EmploymentDate.Value;
    }
    
    #endregion

    #region Подписание документов.
    
    public virtual bool Decision73Result()
    {
      return Functions.Module.IsOnlyBilStaffDocPaper(_obj.Department.BusinessUnit) && Functions.Module.IsEmployeeUsingESS(_obj.Employee);
    }

    public virtual bool Decision72Result()
    {
      return Functions.Module.IsAllElectronicStaffDoc(_obj.Department.BusinessUnit) && Functions.Module.IsEmployeeUsingESS(_obj.Employee);
    }
    
    public virtual void StartBlock28(DirRX.HRManagement.Server.HiringSignDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetStaffChief(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = HiringTasks.Resources.SignSeoEmploymentDocsSubjectFormat(_obj.Person.Name);
    }
    
    public virtual void StartBlock69(DirRX.HRManagement.Server.HiringPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = HiringTasks.Resources.ReWorkDocsAssignmentSubjectFormat(_obj.Person.Name);
    }
    
    public virtual void StartBlock27(DirRX.HRManagement.Server.HiringSignDocsAssignmentArguments e)
    {
      if (_obj.Employee != null)
      {
        e.Block.Performers.Add(_obj.Employee);
        e.Block.Subject = HiringTasks.Resources.SignEmploymentDocsSubject;
        var lnaDocList = Functions.Module.GetLNARecord(_obj.Employee);
        if (lnaDocList != null)
          foreach (var docLNA in lnaDocList.LNADocs)
            _obj.OtherAttachmentGroup.All.Add(docLNA.LNADoc);
      }
    }
    
    public virtual void StartBlock74(DirRX.HRManagement.Server.HiringPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = HiringTasks.Resources.SignAllPaperDocAssignmentSubjectFormat(_obj.Person.Name);
    }

    public virtual void StartBlock76(DirRX.HRManagement.Server.HiringPrepareDocsAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetHRResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = HiringTasks.Resources.SignPaperEmploymentContractAssignmentSubjectFormat(_obj.Person.Name);
    }
    
    #endregion
  }
}