using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.HRLite.InformingTask;

namespace DirRX.HRLite.Server
{
  partial class InformingTaskRouteHandlers
  {

    public virtual void StartNotice3(DirRX.HRLite.IInformingNotice notice, DirRX.HRLite.Server.InformingNoticeArguments e)
    {
      var emp = DirRX.EssPlatformSolution.Employees.As(e.Block.Performers.FirstOrDefault());
      // Если сотрудник подключен к ЛК, то отправить сообщение о новом задании
      if (emp.PersonalAccountStatusDirRX != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent)
      {
        var task = InformingTasks.As(_obj);
        var documents = task.AttachmentGroup.All.Select(d => Sungero.Content.ElectronicDocuments.As(d)).ToList();
        // FIXME Отключение "колоколов"
        // EssPlatform.PublicFunctions.Module.SendEssNotification(emp, notice.Subject, notice.ActiveText, "InformingTask", "InformingNotice", documents);
      }      
    }

    public virtual void StartBlock3(DirRX.HRLite.Server.InformingNoticeArguments e)
    {
      e.Block.ShowInSelfServiceOffice = true;
      var recipients = EssPlatform.PublicFunctions.Module.GetEmployeesWithPersonalAccountRegistred(Functions.InformingTask.GetRecipients(_obj));
      recipients.Where(r => HRSolution.Employees.As(r).ConsentDirRX.Value == HRSolution.Employee.ConsentDirRX.Signed).ToList();
      
      foreach (var recipient in recipients)
      {
        e.Block.Performers.Add(recipient);
        EssPlatform.PublicFunctions.Module.SendNewNotification(recipient, true);
      }
      e.Block.Subject = _obj.Subject;
    }

  }
}