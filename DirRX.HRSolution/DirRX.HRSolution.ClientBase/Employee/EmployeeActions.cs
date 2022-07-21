using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRSolution.Employee;

namespace DirRX.HRSolution.Client
{
  partial class EmployeeActions
  {

    public virtual void SendConsent(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      // Сотрудника без НОР или из закрытой НОР к КЭДО подключить нельзя.
      // Отправлять Соглашение на КЭДО можно только сотруднику, организация которого подключена к ЛК.
      var canSendConsent = false;
      var businessUnit = _obj.Department.BusinessUnit;
      if (businessUnit != null && businessUnit.Status.Value == Sungero.Company.BusinessUnit.Status.Active)
      {
        var businessUnitO = HRSolution.BusinessUnits.As(businessUnit);
        canSendConsent = businessUnitO.UseESSDirRX.HasValue && businessUnitO.UseESSDirRX.Value;
      }
      if (!canSendConsent)
      {
        e.AddError(DirRX.HRSolution.Employees.Resources.SendConsentError);
        return;
      }
      // Отправить Соглашение.
      var task = HRLite.ConsentTasks.Create();
      task.Employee = _obj;
      task.Start();
      Dialogs.NotifyMessage(DirRX.HRSolution.Employees.Resources.EDFAgreementSentNotify);
    }

    public virtual bool CanSendConsent(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      var consentTasks = HRLite.ConsentTasks.GetAll(t => t.Status == Sungero.Workflow.Task.Status.InProcess).Where(t => t.Employee.Equals(_obj));
      // Действие доступно для СОК и администраторов HR, если сотрудник подключен к ЛК и согласие на КЭДО не подписано и задача на подписание согласия КЭДО
      // ещё не отправлена.
      var canSendConsent = !_obj.State.IsInserted && _obj.Status.Value == Sungero.Company.Employee.Status.Active &&
        _obj.PersonalAccountStatusDirRX == Employee.PersonalAccountStatusDirRX.InviteAccepted &&
        _obj.ConsentDirRX == Employee.ConsentDirRX.NotSent &&
        (Users.Current.IncludedIn(HRLite.PublicConstants.Module.Roles.HRSpecialist) ||
         Users.Current.IncludedIn(HRManagement.PublicConstants.Module.HRRoles.HRAdministrator)) &&
        !consentTasks.Any();
      return canSendConsent;      
    }

    public virtual void ShowLinkedDocsDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var employeeDocuments = DirRX.HRLite.PublicFunctions.Module.Remote.GetEmployeeDocuments(_obj);
      employeeDocuments.ShowModal();
    }

    public virtual bool CanShowLinkedDocsDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }


}