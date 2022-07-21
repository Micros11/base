using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.HRManagement.TransferServicesTask;

namespace DirRX.HRManagement.Server
{
  partial class TransferServicesTaskRouteHandlers
  {

    public virtual bool Decision9Result()
    {
      return _obj.IsTransfer.Value == true;
    }

    public virtual void StartAssignment10(DirRX.HRManagement.ITransferServicesAssignment assignment, DirRX.HRManagement.Server.TransferServicesAssignmentArguments e)
    {
      if (_obj.TransferDate != null)
        assignment.Deadline = Calendar.EndOfWorkingDay(Calendar.AddWorkingDays(_obj.TransferDate.Value, -1));
    }

    public virtual void StartBlock10(DirRX.HRManagement.Server.TransferServicesAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetAccounter(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferServicesTasks.Resources.PreparingAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartAssignment7(DirRX.HRManagement.ITransferServicesAssignment assignment, DirRX.HRManagement.Server.TransferServicesAssignmentArguments e)
    {
      if (_obj.TransferDate != null)
        assignment.Deadline = Calendar.EndOfWorkingDay(Calendar.AddWorkingDays(_obj.TransferDate.Value, -1));
    }

    public virtual void StartBlock7(DirRX.HRManagement.Server.TransferServicesAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetSupplyResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferServicesTasks.Resources.PreparingTransferAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartAssignment5(DirRX.HRManagement.ITransferServicesAssignment assignment, DirRX.HRManagement.Server.TransferServicesAssignmentArguments e)
    {
      if (_obj.TransferDate != null)
        assignment.Deadline = Calendar.EndOfWorkingDay(Calendar.AddWorkingDays(_obj.TransferDate.Value, -1));
    }

    public virtual void StartBlock5(DirRX.HRManagement.Server.TransferServicesAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetSecurityResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferServicesTasks.Resources.PreparingTransferAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartAssignment8(DirRX.HRManagement.ITransferServicesAssignment assignment, DirRX.HRManagement.Server.TransferServicesAssignmentArguments e)
    {
      if (_obj.TransferDate != null)
        assignment.Deadline = Calendar.EndOfWorkingDay(Calendar.AddWorkingDays(_obj.TransferDate.Value, -1));
    }

    public virtual void StartBlock8(DirRX.HRManagement.Server.TransferServicesAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetAccounter(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferServicesTasks.Resources.PreparingTransferAssignmentSubjectFormat(_obj.Employee.Name);
    }

    public virtual void StartAssignment6(DirRX.HRManagement.ITransferServicesAssignment assignment, DirRX.HRManagement.Server.TransferServicesAssignmentArguments e)
    {
      if (_obj.TransferDate != null)
        assignment.Deadline = Calendar.EndOfWorkingDay(Calendar.AddWorkingDays(_obj.TransferDate.Value, -1));
    }

    public virtual void StartBlock6(DirRX.HRManagement.Server.TransferServicesAssignmentArguments e)
    {
      var employee = _obj.Employee;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var perfomer = Functions.Module.GetITResponsible(businessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = TransferServicesTasks.Resources.PreparingTransferAssignmentSubjectFormat(_obj.Employee.Name);
    }

  }
}