using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.EssPlatform.Server
{
  public class ModuleAsyncHandlers
  {

    public virtual void SynchronizeEmployee(DirRX.EssPlatform.Server.AsyncHandlerInvokeArgs.SynchronizeEmployeeInvokeArgs args)
    {
      int employeeId = args.employeeId;
      
      Logger.DebugFormat("EssPlatform.AsyncHandlers.SynchronizeEmployee started, iteration: {0}, employeeId: {1}", args.RetryIteration, employeeId);
      
      if (args.RetryIteration > Constants.Module.SynchronizeEmployee)
      {
        Logger.ErrorFormat("Essplatform.AsyncHandlers.SynchronizeEmployee didn`t update Employee, iteration: {0}, employeeId: {1}", args.RetryIteration, employeeId);
        args.Retry = false;
        return;
      }
      
      var employee = EssPlatformSolution.Employees.Get(employeeId);
      var personalAccountStatus = new Sungero.Core.Enumeration(args.personalAccountStatus);
      var personalPhone = args.personalPhone;
      var smsNotification = args.smsNotification;
      var viberNotification = args.viberNotification;
      var emailNotification = args.emailNotification;
      var messagesEmail = args.messagesEmail;
      
      try
      {
        if (employee.PersonalAccountStatusDirRX != personalAccountStatus)
          employee.PersonalAccountStatusDirRX = personalAccountStatus;
        if (employee.PersonalPhoneDirRX != personalPhone)
          employee.PersonalPhoneDirRX = personalPhone;
        if (employee.SmsNotifDirRX != smsNotification)
          employee.SmsNotifDirRX = smsNotification;
        if (employee.ViberNotifDirRX != viberNotification)
          employee.ViberNotifDirRX = viberNotification;
        if (employee.EmailNotifDirRX != emailNotification)
          employee.EmailNotifDirRX = emailNotification;
        if (employee.MessagesEmailDirRX != messagesEmail)
          employee.MessagesEmailDirRX = messagesEmail;
        if (employee.State.IsChanged)
          employee.Save();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.AsyncHandlers.SynchronizeEmployee: could not update Employee. EmployeeId: {0}. {1}. {2}", employeeId, ex.Message, ex.StackTrace);
        args.Retry = true;
      }
    }

    public virtual void SendMessageToViber(DirRX.EssPlatform.Server.AsyncHandlerInvokeArgs.SendMessageToViberInvokeArgs args)
    {
      Logger.DebugFormat("Essplatform.AsyncHandlers.SendMessageToViber started, iteration: {0}", args.RetryIteration);
      var employee = EssPlatformSolution.Employees.Get(args.userid);
      // После допустимого количества попыток отправить сообщение отправляем уведомление в RX администратору, в логи пишем ошибку и заканчиваем попытки.
      if (args.RetryIteration >= Constants.Module.SendMessagePossibleRetryCount)
      {
        Logger.ErrorFormat("Essplatform.AsyncHandlers.SendMessageToViber didn`t send message, iteration {0}", args.RetryIteration);
        args.Retry = false;
        var task = Sungero.Workflow.SimpleTasks.CreateWithNotices(DirRX.EssPlatform.Resources.SendMessagesErrorSubject, Roles.Administrators);
        task.ActiveText = DirRX.EssPlatform.Resources.SendMessageToViberNoticeErrorFormat(employee.Name, employee.Id);
        task.Start();
        return;
      }
      
      EssPlatform.PublicFunctions.Module.SendMessageToViber(employee.PersonalPhoneDirRX, args.messageText);
    }

    public virtual void SendEmail(DirRX.EssPlatform.Server.AsyncHandlerInvokeArgs.SendEmailInvokeArgs args)
    {
      Logger.DebugFormat("Essplatform.AsyncHandlers.SendEmail started, iteration: {0}", args.RetryIteration);
      var employee = EssPlatformSolution.Employees.Get(args.userid);
      // После допустимого количества попыток отправить сообщение отправляем уведомление в RX администратору, в логи пишем ошибку и заканчиваем попытки.
      if (args.RetryIteration >= Constants.Module.SendMessagePossibleRetryCount)
      {
        Logger.ErrorFormat("Essplatform.AsyncHandlers.SendEmail didn`t send message, iteration {0}", args.RetryIteration);
        args.Retry = false;
        var task = Sungero.Workflow.SimpleTasks.CreateWithNotices(DirRX.EssPlatform.Resources.SendMessagesErrorSubject, Roles.Administrators);
        task.ActiveText = DirRX.EssPlatform.Resources.SendEmailNoticeErrorFormat(employee.Name, employee.Id);
        task.Start();
        return;
      }
      
      EssPlatform.PublicFunctions.Module.SendEMail(employee.MessagesEmailDirRX, args.messageSubject, args.messageText);
    }

    public virtual void SendSms(DirRX.EssPlatform.Server.AsyncHandlerInvokeArgs.SendSmsInvokeArgs args)
    {
      Logger.DebugFormat("Essplatform.AsyncHandlers.SendSms started, iteration: {0}", args.RetryIteration);
      var employee = EssPlatformSolution.Employees.Get(args.userid);
      // После допустимого количества попыток отправить сообщение отправляем уведомление в RX администратору, в логи пишем ошибку и заканчиваем попытки.
      if (args.RetryIteration >= Constants.Module.SendMessagePossibleRetryCount)
      {
        Logger.ErrorFormat("Essplatform.AsyncHandlers.SendSms didn`t send message, iteration {0}", args.RetryIteration);
        args.Retry = false;
        var task = Sungero.Workflow.SimpleTasks.CreateWithNotices(DirRX.EssPlatform.Resources.SendMessagesErrorSubject, Roles.Administrators);
        task.ActiveText = DirRX.EssPlatform.Resources.SendSmsNoticeErrorFormat(employee.Name, employee.Id);
        task.Start();
        return;
      }
      
      EssPlatform.PublicFunctions.Module.SendSMS(employee.PersonalPhoneDirRX, args.messageText);
    }
    
    /// <summary>
    /// Добавить пользователей в личный кабинет.
    /// </summary>
    /// <param name="args">Параметры вызова асинхронного обработчика.
    /// "args.businessUnitIds" - ИД наших организаций.
    /// "args.departmentIds" - ИД подразделений.
    /// "args.employeeIds" - ИД сотрудников.
    /// </param>
    public virtual void ActivateESSUsers(DirRX.EssPlatform.Server.AsyncHandlerInvokeArgs.ActivateESSUsersInvokeArgs args)
    {
      Logger.DebugFormat("Essplatform.AsyncHandlers.ActivateEssUsers started, iteration: {0}", args.RetryIteration);
      
      // После допустимого количества попыток отправить приглашения в ЛК отправляем уведомление в RX, в логи пишем ошибку и заканчиваем попытки.
      if (args.RetryIteration > Constants.Module.ActivateEssUsersPossibleRetryCount)
      {
        Logger.ErrorFormat(DirRX.EssPlatform.Resources.ActivateEssUsersMaxRetryCount, args.RetryIteration);
        args.Retry = false;
        DirRX.EssPlatform.PublicFunctions.Module.SendNoticeAboutInvite(args.userId, Resources.ErrorESSInvites);
        return;
      }
      
      var businessUnitIds = args.businessUnitIds.Split(',').Where(bu => !string.IsNullOrEmpty(bu)).ToList();
      var departmentIds = args.departmentIds.Split(',').Where(dep => !string.IsNullOrEmpty(dep)).ToList();
      var employeeIds = args.employeeIds.Split(',').Where(emp => !string.IsNullOrEmpty(emp)).ToList();
      var result = DirRX.EssPlatform.PublicFunctions.Module.ActivateESSUsers(businessUnitIds, departmentIds, employeeIds);
      
      var totalUsersProcessed = result.AlreadyAcceptedUsersCount + result.AlreadyInvitedUsersCount + result.InvitedUsersCount + result.WithoutPhoneUsersCount + result.CatchErrorUsersCount;
      var message = string.Empty;
      message = DirRX.EssPlatform.Resources.InvitesSendFormat(totalUsersProcessed, result.InvitedUsersCount, result.WithoutPhoneUsersCount, result.AlreadyInvitedUsersCount, result.AlreadyAcceptedUsersCount);
      
      if (!string.IsNullOrEmpty(result.Error))
      {
        message += Environment.NewLine;
        message += DirRX.EssPlatform.Resources.InviteSendWithErrorsFormat(result.CatchErrorUsersCount);
      }
      
      DirRX.EssPlatform.PublicFunctions.Module.SendNoticeAboutInvite(args.userId, message);
    }

    /// <summary>
    /// Изменить статус сотрудника.
    /// </summary>
    /// <param name="args">Параметры вызова асинхронного обработчика.</param>
    public virtual void ChangeEmployeeEssStatus(DirRX.EssPlatform.Server.AsyncHandlerInvokeArgs.ChangeEmployeeEssStatusInvokeArgs args)
    {
      Logger.DebugFormat("EssPlatform.AsyncHandlers.ChangeEmployeeEssStatus started, iteration: {0}", args.RetryIteration);
      int employeeId = args.EmployeeId;
      var employee = EssPlatformSolution.Employees.Get(employeeId);
      var partTimeEmployees = EssPlatformSolution.Employees.GetAll(e => e.Status == Sungero.Company.Employee.Status.Active && e.Person.Equals(employee.Person));
      var statusString = args.EssStatus;
      var status = new Sungero.Core.Enumeration(statusString);

      foreach (var partTimeEmployee in partTimeEmployees)
      {
        try
        {
          if (statusString == EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteSent.ToString()
              && partTimeEmployee.PersonalAccountStatusDirRX == EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted)
            continue;
          
          partTimeEmployee.PersonalAccountStatusDirRX = status;
          partTimeEmployee.Save();
        }
        catch
        {
          Logger.ErrorFormat("EssPlatform.AsyncHandlers.ChangeEmployeeEssStatus: could not update EssStatus. Employee id: {0}.", partTimeEmployee.Id);
          args.Retry = true;
        }
      }
    }
    
  }
}