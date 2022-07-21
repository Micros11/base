using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Sungero.Commons;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Parties;
using RequestApiIdentity = DirRX.EssPlatform.Constants.Module.RequestApiIdentity;
using Aspose.Cells;
using Sungero.Domain;
using Sungero.Domain.Shared;
using System.Reflection;

namespace DirRX.EssPlatform.Server
{
  public class ModuleFunctions
  {
    
    /// <summary>
    /// Проверить, является ли сотрудник персоны единственным действующим.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="oldStatus">Состояние.</param>
    /// <returns>True - если у персоны нет других действующих сотрудников, иначе - false.</returns>
    [Public, Remote]
    public bool IsSingleActivePersonEmployee(IEmployee employee, Enumeration? oldStatus)
    {
      if (oldStatus == Sungero.Company.Employee.Status.Closed)
        return false;
      
      var otherEmployees = Employees.GetAll(w => Equals(w.Person, employee.Person) && w.Status == Sungero.Company.Employee.Status.Active && w.Id != employee.Id);
      
      return !otherEmployees.Any();
    }
    
    /// <summary>
    /// Получить основного сотрудника персоны.
    /// </summary>
    /// <param name="personId">ИД персоны.</param>
    /// <param name="filterBusinessUnit">Фильтр по НОР.</param>
    /// <returns>Основной сотрудник персоны.</returns>
    [Public, Remote]
    public EssPlatformSolution.IEmployee GetPersonMainEmployee(int personId, IBusinessUnit filterBusinessUnit)
    {
      /*
      Определение основного сотрудника:
      Найти действующих сотрудников одной персоны (с фильтрацией по НОР, если нужно). В найденном списке найти сотрудников с логином.
      Если найденных сотрудников с логином больше одного, то из этих сотрудников выбрать сотрудника с наименьшим ИД.
      Если сотрудников с логином не найдено, то найти сотрудника без логина с наименьшим ИД.
       */
      var employees = EssPlatformSolution.Employees.GetAll(w => w.Person.Id == personId && w.Status == EssPlatformSolution.Employee.Status.Active);
      
      if (filterBusinessUnit != null)
        employees = employees.Where(w => Equals(w.BusinessUnitDirRX, filterBusinessUnit));
      
      if (employees.Any())
      {
        var employeesWithLogin = employees.Where(w => w.Login != null);

        if (employeesWithLogin.Count() > 0)
          return employeesWithLogin.OrderBy(o => o.Id).FirstOrDefault();
        else
          return employees.OrderBy(o => o.Id).FirstOrDefault();
      }
      
      return DirRX.EssPlatformSolution.Employees.Null;
    }

    /// <summary>
    /// Проверить номер телефона ЛК.
    /// </summary>
    /// <param name="phoneNumber">Номер телефона.</param>
    /// <param name="employee">Сотрудник.</param>
    [Public, Remote]
    public virtual List<string> CheckPersonalPhoneNumber(string phoneNumber, DirRX.EssPlatformSolution.IEmployee employee)
    {
      var errorsList = new List<string>();
      if (!string.IsNullOrEmpty(phoneNumber))
      {
        if (!Functions.Module.PhoneNumberLengthIsValid(phoneNumber, false))
        {
          errorsList.Add(DirRX.EssPlatformSolution.Employees.Resources.PersonalPhoneNumbersCountError);
        }
        else
        {
          var phone = EssPlatform.PublicFunctions.Module.PhoneNumberToUniversalFormat(phoneNumber);
          
          if (PublicFunctions.EssSetting.Remote.SettingsConnected())
          {
            // Проверить уникальность номера в базе личного кабинета.
            DirRX.EssPlatform.Structures.Module.IdSUserInfo resultSearch = null;
            try
            {
              resultSearch = EssPlatform.PublicFunctions.Module.FindIdsUser(phone);
            }
            catch (AppliedCodeException ex)
            {
              errorsList.Add(ex.Message);
            }
            
            if (!string.IsNullOrEmpty(resultSearch.Name) && resultSearch.Name != EssPlatform.PublicFunctions.Module.GetUidPerson(employee.Person))
              errorsList.Add(DirRX.EssPlatformSolution.Employees.Resources.PersonalPhoneNotUniqInESS);
          }
          
          // Проверить уникальность номера в персонах.
          var result = DirRX.EssPlatformSolution.Employees.GetAll(e => e.PersonalPhoneDirRX == phone && e.Person.Id != employee.Person.Id);
          if (result.Any())
            errorsList.Add(DirRX.EssPlatformSolution.Employees.Resources.PersonalPhoneNotUniq);
        }
      }
      return errorsList;
    }

    /// <summary>
    /// Найти роли модуля.
    /// </summary>
    /// <returns>Список ролей модуля.</returns>
    [Remote]
    public virtual List<IRole> GetEssRoles()
    {
      return Sungero.CoreEntities.Roles.GetAll(r => r.Sid == Constants.Module.PAUsersWithPasswordAuthentication).ToList();
    }

    #region Старт асинхронных обработчиков
    
    [Public]
    /// <summary>
    /// Создать асинхронное событие для изменения статуса личного кабинета для сотрудника на "Отправлено"
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    public void ChangePersonalAccountStatusToInviteSent(EssPlatformSolution.IEmployee employee)
    {
      var asyncChangeEssStatus = EssPlatform.AsyncHandlers.ChangeEmployeeEssStatus.Create();
      asyncChangeEssStatus.EmployeeId = employee.Id;
      asyncChangeEssStatus.EssStatus = EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteSent.ToString();
      asyncChangeEssStatus.ExecuteAsync();
    }
    #endregion
    
    #region Активация, изменение, удаление и поиск пользователя ЛК
    
    /// <summary>
    /// Получить уникальный идентификатор персоны в рамках тенанта.
    /// </summary>
    /// <param name="person">Персона</param>
    /// <returns>Идентификатор.</returns>
    [Public]
    public virtual string GetUidPerson(Sungero.Parties.IPerson person)
    {
      return Sungero.Core.TenantInfo.TenantId + "@" + person.Id;
    }
    
    /// <summary>
    /// Отправить запрос на активацию пользователя.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    [Public, Remote]
    public virtual void ActivateESSUser(EssPlatformSolution.IEmployee employee)
    {
      try
      {
        var personUid = GetUidPerson(employee.Person);
        Logger.DebugFormat("EssPlatform.ModuleServerFunctions.ActivateESSUser(). Activate Ess user {0}.", personUid);
        var baseUri = PublicFunctions.EssSetting.GetSettings().IdentityServiceAddress;
        var requestUri = String.Format(EssPlatform.Constants.Module.RequestApiIdentity.ActivateUser, EssPlatform.Constants.Module.Audiences.EssSite);
        var token = GetAnAuthenticationTokenCurrentSystem();
        // создать структуру для формирования json-контента
        var userEssJson = new Structures.Module.UserActivateJson();
        userEssJson.name = personUid;
        userEssJson.fullName = employee.Name;
        userEssJson.phoneNumber = employee.PersonalPhoneDirRX;
        // Если не заполнена галочка уведомлений в viber, то поставить по умолчанию галочку SMS
        employee.SmsNotifDirRX = employee.ViberNotifDirRX.Value ? false : true;
        // почта пока игнорируется
        userEssJson.email = string.Empty;
        // пока это единственный способ аутентификации
        var paUsersWithPasswordAuthentication = Roles.GetAll(w => w.Sid == Constants.Module.PAUsersWithPasswordAuthentication).FirstOrDefault();
        if (employee.IncludedIn(paUsersWithPasswordAuthentication))
          userEssJson.authenticationProvider = "password";
        var claim = Structures.Module.Claim.Create();
        claim.DirectumRX_PersonId = employee.Person.Id;
        userEssJson.claims = claim;
        var jsonString = SerializedToJson(userEssJson).Replace("DirectumRX_PersonId", "DirectumRX/PersonId");
        var result = RunPostRequest(baseUri, requestUri, jsonString, token);
        
        if (result.StatusCode == 204)
        {
          Logger.Debug("EssPlatform.ModuleServerFunctions.ActivateESSUser(). User activated.");
          return;
        }
        else
          throw new Exception(DirRX.EssPlatform.Resources.ActivateESSUserExceptionFormat(result.StatusCode, result.RequestMessage));
        
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("An error occurred while sending the request: {0}.", ex.Message);
        throw new Exception(ex.Message);
      }
    }

    /// <summary>
    /// Добавить пользователей в личный кабинет.
    /// </summary>
    /// <param name="businessUnitIds">ИД наших организаций.</param>
    /// <param name="departmentIds">ИД подразделений.</param>
    /// <param name="employeeIds">ИД сотрудников.</param>
    /// <returns>Структура, содержащая всю информацию по созданию личных кабинетов сотрудников.
    /// Error содержит информацию об ошибках, произошедших в результате создания личного кабинета.
    /// WithoutPhoneUsersCount содержит кол-во сотрудников без личного телефона.
    /// AlreadyInvitedUsersCount содержит кол-во сотрудников, у которых уже есть личный кабинет.
    /// InvitedUsersCount содержит кол-во сотркудников, которым высланы приглашения.</returns>
    [Public]
    public virtual DirRX.EssPlatform.Structures.Module.ICreateEssUsersResults ActivateESSUsers(List<string> businessUnitIds, List<string> departmentIds, List<string> employeeIds)
    {
      Logger.Debug("Process CreateEssUsers started");
      
      var result = DirRX.EssPlatform.Structures.Module.CreateEssUsersResults.Create();

      var employees = EssPlatformSolution.Employees.GetAll().Where(emp => emp.Status == Sungero.CoreEntities.DatabookEntry.Status.Active
                                                                   && emp.Department != null
                                                                   && emp.Department.Status == Sungero.CoreEntities.DatabookEntry.Status.Active
                                                                   && emp.Department.BusinessUnit != null
                                                                   && emp.Department.BusinessUnit.Status == Sungero.CoreEntities.DatabookEntry.Status.Active
                                                                   && EssPlatformSolution.BusinessUnits.As(emp.Department.BusinessUnit).UseESSDirRX.HasValue
                                                                   && EssPlatformSolution.BusinessUnits.As(emp.Department.BusinessUnit).UseESSDirRX.Value);
      if (employeeIds.Any())
        employees = employees.Where(emp => employeeIds.Contains(emp.Id.ToString()));
      else if (departmentIds.Any())
        employees = employees.Where(emp => departmentIds.Contains(emp.Department.Id.ToString()));
      else if (businessUnitIds.Any())
        employees = employees.Where(emp => businessUnitIds.Contains(emp.Department.BusinessUnit.Id.ToString()));

      
      var usersWithoutPhone = employees.ToList().Where(emp => string.IsNullOrEmpty(emp.PersonalPhoneDirRX)).ToList();
      if (usersWithoutPhone.Any())
      {
        foreach (var user in usersWithoutPhone)
        {
          Logger.DebugFormat("{0} hasn't personal phone.", user.Name);
        }
      }

      var usersWithInviteAccepted = employees.ToList().Where(emp => emp.PersonalAccountStatusDirRX == EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted).ToList();
      if (usersWithInviteAccepted.Any())
      {
        foreach (var user in usersWithInviteAccepted)
        {
          Logger.DebugFormat("{0} already has personal office.", user.Name);
        }
      }
      
      var usersWithInviteSent = employees.ToList().Where(emp => emp.PersonalAccountStatusDirRX == EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteSent).ToList();
      if (usersWithInviteSent.Any())
      {
        foreach (var user in usersWithInviteSent)
        {
          Logger.DebugFormat("{0} already sent invite to personal office.", user.Name);
        }
      }
      
      var usersToInvite = employees.ToList().Where(emp => !string.IsNullOrEmpty(emp.PersonalPhoneDirRX) && emp.PersonalAccountStatusDirRX == EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent).ToList();
      foreach(var empl in usersToInvite) {
        
        Logger.DebugFormat("{0} inviting...", empl.Name);
        try
        {
          ActivateESSUser(empl);
        }
        catch (Exception ex)
        {
          Logger.Error("Error: " + ex.Message);
          result.Error = ex.Message;
          result.CatchErrorUsersCount ++;
          continue;
        }
        
        if (empl.PersonalAccountStatusDirRX != EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted)
        {
          EssPlatform.PublicFunctions.Module.ChangePersonalAccountStatusToInviteSent(empl);
        }
      }
      
      result.InvitedUsersCount = usersToInvite.Count() - result.CatchErrorUsersCount;
      result.AlreadyInvitedUsersCount = usersWithInviteSent.Count();
      result.AlreadyAcceptedUsersCount = usersWithInviteAccepted.Count();
      result.WithoutPhoneUsersCount = usersWithoutPhone.Count();
      Logger.Debug("CreateEssUsers ended = = = = =");
      return result;
    }
    
    /// <summary>
    /// Изменить информацию о пользователе личного кабинета
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    [Public, Remote]
    public virtual void PatchEssUser(EssPlatformSolution.IEmployee employee)
    {
      try
      {
        var personUid = GetUidPerson(employee.Person);
        var idSUrl = PublicFunctions.EssSetting.GetSettings().IdentityServiceAddress;
        var token = GetAnAuthenticationTokenCurrentSystem();
        // Cоздать структуру для формирования json-контента.
        var userEssJson = new Structures.Module.UserPatchJson();
        userEssJson.fullName = employee.Name;
        userEssJson.phoneNumber = employee.PersonalPhoneDirRX;
        //TODO временно отключено - сервисы не воспринимают пустой e-mail
        //userEssJson.email = string.Empty;
        
        var requestResult = RunPatchRequest(idSUrl, String.Format(EssPlatform.Constants.Module.RequestApiIdentity.PatchEssUser, personUid), SerializedToJson(userEssJson), token);
        var statusCode = requestResult.StatusCode;
        if (statusCode == (int)HttpStatusCode.NoContent)
          return;
        // Ответы, отличные от 204 считать ошибками и пробрасывать дальше.
        throw new Exception(Resources.FindEssUserExceptionFormat(statusCode, requestResult.RequestMessage));
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.PatchEssUser(). An error occurred while sending the request: {0}.", ex.Message);
        throw ex;
      }
    }
    
    /// <summary>
    /// Удалить пользователя из личного кабинета.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    public virtual void DeleteESSUser(EssPlatformSolution.IEmployee employee)
    {
      try
      {
        var personUid = GetUidPerson(employee.Person);
        var token = GetAnAuthenticationTokenCurrentSystem();
        var baseUri = PublicFunctions.EssSetting.GetSettings().IdentityServiceAddress;
        var requestApi = string.Format(Constants.Module.RequestApiIdentity.DeleteEssUser, personUid);
        
        RunDeleteRequest(baseUri, requestApi, token);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.DeleteESSUser(). An error occurred while sending the request: {0}.", ex.Message);
        throw ex;
      }
    }

    /// <summary>
    /// Найти пользователя по имени (name) или номеру телефона сотрудника.
    /// </summary>
    /// <param name="searchParam">Номер телефона или Uid пользователя.</param>
    /// <returns>Струкутура со информацией о пользователе. Если пользователь не найден, то поля структуры будут пустые.</returns>
    [Public]
    public virtual Structures.Module.IdSUserInfo FindIdsUser(string searchParam)
    {
      try
      {
        var token = GetAnAuthenticationTokenCurrentSystem();
        var idSUrl = PublicFunctions.EssSetting.GetSettings().IdentityServiceAddress;
        // IdS обрабатывает номер без "+", удалить.
        var getRequestResult = RunGetRequest(idSUrl, string.Format(Constants.Module.RequestApiIdentity.FindEssUser, searchParam.Replace("+", "")), token);
        var statusCode = getRequestResult.StatusCode;
        if (statusCode == (int)HttpStatusCode.OK)
        {
          return JsonConvert.DeserializeObject<Structures.Module.IdSUserInfo>(getRequestResult.RequestMessage);
        }
        if (statusCode == (int)HttpStatusCode.NotFound)
        {
          return (Structures.Module.IdSUserInfo)Structures.Module.IdSUserInfo.Create(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }
        // Ответы, отличные от 200 и 404 считать ошибками и пробрасывать дальше.
        throw new Exception(Resources.FindEssUserExceptionFormat(statusCode, getRequestResult.RequestMessage));
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.IsExistsESSUser(). An error occurred while sending the find request: {0}.", ex.Message);
        throw ex;
      }
    }
    
    /// <summary>
    /// Отключить пользователя личного кабинета.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    [Public, Remote]
    public virtual void DisconnectEssUser(EssPlatformSolution.IEmployee employee)
    {
      var personUid = GetUidPerson(employee.Person);
      var businessUnit = EssPlatformSolution.BusinessUnits.As(employee.Department.BusinessUnit);
      Logger.DebugFormat("EssPlatform.ModuleServerFunctions.DisconnectESSUser(): user {0} disconnected started.", personUid);
      
      try
      {
        DeleteESSUser(employee);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.DisconnectESSUser(). An error occurred while diconnecting: {0}.", ex.Message);
        throw AppliedCodeException.Create(DirRX.EssPlatform.Resources.DisconnectEssUserError, ex);
      }
      
      Logger.DebugFormat("EssPlatform.ModuleServerFunctions.DisconnectESSUser(): user {0} disconnected.", personUid);
      
      Logger.DebugFormat("EssPlatform.ModuleServerFunctions.DisconnectESSUser(): send sms to user {0}.", personUid);
      
      try
      {
        Functions.Module.SendSMS(employee.PersonalPhoneDirRX, businessUnit.SmsAfterCloseDirRX);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.DisconnectESSUser(): could not send SMS to user {0}. Error: {1}.", personUid, ex.Message);
        throw AppliedCodeException.Create(DirRX.EssPlatform.Resources.DisconnectEssUserError, ex);
      }
      
      employee.PersonalAccountStatusDirRX = EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent;
    }
    
    /// <summary>
    /// Обновить пользователя личного кабинета.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <remarks>- Если необходимо поменять номер, но сотрудник не включен в НОР или НОР не подключена к ЛК, СМС отправлены не будут, информация о пользователе не обновится.
    /// - Если было необходимо поменять номер, но процедура отправки СМС прошла неуспешно, информация о пользователе не обновится.</remarks>
    [Public]
    public virtual void UpdateEssUser(EssPlatformSolution.IEmployee employee, string oldPhoneNumber)
    {
      var uid = Functions.Module.GetUidPerson(employee.Person);
      Logger.DebugFormat("EssPlatform.ModuleServerFunctions.UpdateEssUser: user {0} update started.", uid);
      
      // Если передан старый номер телефона, значит номер сотрудника изменился - запустить процедуру по смене номера.
      if (!string.IsNullOrEmpty(oldPhoneNumber))
      {
        if (employee.Department.BusinessUnit != null)
        {
          var businessUnit = EssPlatformSolution.BusinessUnits.As(employee.Department.BusinessUnit);
          if (businessUnit.UseESSDirRX.HasValue && businessUnit.UseESSDirRX.Value)
          {
            if (oldPhoneNumber != employee.PersonalPhoneDirRX)
            {
              Logger.DebugFormat("EssPlatform.ModuleServerFunctions.UpdateEssUser: user {0} phone change started.", uid);
              try
              {
                SendSMS(oldPhoneNumber, businessUnit.SmsToOldPhoneDirRX);
                SendSMS(employee.PersonalPhoneDirRX, businessUnit.SmsToNewPhoneDirRX);
              }
              catch (Exception ex)
              {
               
                Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.UpdateEssUser: could not send SMS to user {0}. Error: {1}.", uid, ex.Message);
                throw AppliedCodeException.Create(DirRX.EssPlatform.Resources.UpdateEssuserError);
              }
            }
          }
          else
          {
            Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.UpdateEssUser: could not send SMS to user {0}. Business unit of user in not connected to Self Service Office.", uid);
            return;
          }
        }
        else
        {
          Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.UpdateEssUser: could not send SMS to user {0}. Business unit of user is not defined.", uid);
          return;
        }
      }
      
      try
      {
        EssPlatform.PublicFunctions.Module.Remote.PatchEssUser(employee);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.UpdateEssUser: could not update EssUser. Employee id: {0}. Error: {1}.", employee.Id, ex.Message);
        throw AppliedCodeException.Create(DirRX.EssPlatform.Resources.UpdateEssuserError);
      }
    }
    
    /// <summary>
    /// Отправить уведомление - результат об отправке приглашений.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="text">Текст уведомления.</param>
    [Public]
    public virtual void SendNoticeAboutInvite(int userId, string text)
    {
      var recipients = new[] {Recipients.As(Users.Get(userId))};
      var task = Sungero.Workflow.SimpleTasks.CreateWithNotices(Resources.ESSNoticeSubject, recipients);
      task.ActiveText = text;
      task.Start();
    }
    
    #endregion

    #region Запросы к MessageBroker Service

    /// <summary>
    /// Отправить SMS пользователю
    /// </summary>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <param name="sms">Текст</param>
    [Public]
    public void SendSMS(string phoneNumber, string sms)
    {
      // Если отключена настройка "Подключить сервис идентификации", то отправка СМС выполняться не будет.
      if (!EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected())
        return;
      
      try
      {
        var token = GetAnAuthenticationTokenMessageBroker();
        var messageBrokerUrl = PublicFunctions.EssSetting.GetSettings().MessageBrokerAddress;
        var requestApi = String.Format(EssPlatform.Constants.Module.RequestApiMessageBroker.Sms, phoneNumber);
        var requestResult = RunPostRequest(messageBrokerUrl, requestApi, SerializedToJson(sms), token);
        var statusCode = requestResult.StatusCode;
        if (statusCode == (int)HttpStatusCode.OK)
          return;
        // Ответы, отличные от 200 считать ошибками и пробрасывать дальше.
        throw new Exception(DirRX.EssPlatform.Resources.SendSMSErrorFormat(statusCode, requestResult.RequestMessage));
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.SendSMS(). An error occurred while sending the request: {0}.", ex.Message);
        throw ex;
      }
    }
    
    /// <summary>
    /// Отправить e-mail пользователю
    /// </summary>
    /// <param name="email">E-mail</param>
    /// <param name="subject">Тема</param>
    /// <param name="text">Текст</param>
    [Public]
    public void SendEMail(string email, string subject, string text)
    {
      // Если отключена настройка "Подключить сервис идентификации", то отправка E-mail выполняться не будет.
      if (!EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected())
        return;
      
      try
      {
        var token = GetAnAuthenticationTokenMessageBroker();
        var messageBrokerUrl = PublicFunctions.EssSetting.GetSettings().MessageBrokerAddress;
        var requestApi = String.Format(EssPlatform.Constants.Module.RequestApiMessageBroker.Email, email, subject);
        var requestResult = RunPostRequest(messageBrokerUrl, requestApi, SerializedToJson(text), token);
        var statusCode = requestResult.StatusCode;
        if (statusCode == (int)HttpStatusCode.OK)
          return;
        // Ответы, отличные от 200 считать ошибками и пробрасывать дальше.
        throw new Exception(DirRX.EssPlatform.Resources.SendEMailErrorFormat(statusCode, requestResult.RequestMessage));
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.SendEMail(). An error occurred while sending the request: {0}.", ex.Message);
        throw ex;
      }
    }
    
    /// <summary>
    /// Отправить сообщение в Viber пользователю
    /// </summary>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <param name="message">Текст</param>
    [Public]
    public void SendMessageToViber(string phoneNumber, string message)
    {
      // Если отключена настройка "Подключить сервис идентификации", то отправка сообщения в Viber выполняться не будет.
      if (!EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected())
        return;
      
      try
      {
        var token = GetAnAuthenticationTokenMessageBroker();
        var messageBrokerUrl = PublicFunctions.EssSetting.GetSettings().MessageBrokerAddress;
        var requestApi = String.Format(EssPlatform.Constants.Module.RequestApiMessageBroker.Viber, phoneNumber);
        var requestResult = RunPostRequest(messageBrokerUrl, requestApi, SerializedToJson(message), token);
        var statusCode = requestResult.StatusCode;
        if (statusCode == (int)HttpStatusCode.OK)
          return;
        // Ответы, отличные от 200 считать ошибками и пробрасывать дальше.
        throw new Exception(DirRX.EssPlatform.Resources.SendViberErrorFormat(statusCode, requestResult.RequestMessage));
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.SendMessageToViber(). An error occurred while sending the request: {0}.", ex.Message);
        throw ex;
      }
    }
    
    /// <summary>
    /// Проверить подключение до ЛК.
    /// </summary>
    /// <param name="MBUrl">Адрес ЛК.</param>
    /// <returns>Если соединение установлено - пустая строка, иначе текст ошибки.</returns>
    [Public, Remote]
    public virtual string CheckESSConnection(string EssUrl)
    {
      try
      {
        var result = RunGetRequest(EssUrl, string.Empty, string.Empty);
        if (result.StatusCode == (int)HttpStatusCode.OK)
          return string.Empty;
        else
          return DirRX.EssPlatform.Resources.CheckConnectionErrorFormat(result.StatusCode);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("CheckEssConnection(). An error occurred while sending the request: {0}.", ex.Message);
        return ex.Message;
      }
    }
    
    /// <summary>
    /// Проверить подключение до MB.
    /// </summary>
    /// <param name="MBUrl">Адрес сервиса MessageBroker.</param>
    /// <returns>Если соединение установлено - пустая строка, иначе текст ошибки.</returns>
    [Public, Remote]
    public virtual string CheckMessageBrokerConnection(string MBUrl)
    {
      try
      {
        var result = RunGetRequest(MBUrl, Constants.Module.RequestApiMessageBroker.MessageBrokerHealth, string.Empty);
        if (result.StatusCode == (int)HttpStatusCode.OK)
          return string.Empty;
        else
          return DirRX.EssPlatform.Resources.CheckConnectionErrorFormat(result.StatusCode);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("CheckMBConnection(). An error occurred while sending the request: {0}.", ex.Message);
        return ex.Message;
      }
    }

    [Public]
    /// <summary>
    /// Подготовка структуры с описанием сообщения-колокольчика в ЛК
    /// </summary>
    /// <param name="employee">Сотрудник, которому надо отправить сообщение.</param>
    /// <returns>Структура с описанием сообщения</returns>
    public virtual EssPlatform.Structures.Module.IMessageBrokerNotification CreateEmptyEssNotification(EssPlatformSolution.IEmployee employee)
    {
      try
      {
        // Структура для уведомления
        var result = DirRX.EssPlatform.Structures.Module.MessageBrokerNotification.Create();
        result.Properties = new Dictionary<string, string>();
        result.Attachments = new List<DirRX.EssPlatform.Structures.Module.IMessageBrokerNotificationAttachment>();
        
        // сотрудник, которому отправляется сообщение
        var identity = DirRX.EssPlatform.Structures.Module.MessageBrokerNotificationIdentity.Create();
        identity.CredentialType = Constants.Module.RequestApiMessageBroker.CredentialType;
        identity.CredentialValue = GetUidPerson(employee.Person);
        result.Identity = identity;
        
        // дефолтные параметры доставки
        result.DeliveryMethod = 1;
        result.Priority = 1;

        return result;
      }
      catch (Exception ex)
      {
        Logger.Error(ex.Message);
        throw ex;

      }
    }

    /// <summary>
    /// Подготовка структуры с описанием вложения в сообщение-колокольчик в ЛК
    /// </summary>
    /// <param name="title">Название вложения.</param>
    /// <param name="id">Id документа с вложением.</param>
    /// <returns>Структура с описанием вложения. В качестве вложений поддерживаются только документы</returns>
    [Public]
    public virtual EssPlatform.Structures.Module.IMessageBrokerNotificationAttachment CreateAttachmentNotification(string title, int id)
    {
      var attachment = EssPlatform.Structures.Module.MessageBrokerNotificationAttachment.Create();
      attachment.Title = title;
      attachment.Url = $"EssBase://ElectronicDocument/{id}";
      return attachment;
    }

    /// <summary>
    /// Отправка сообщения в ЛК через MessageBroker.
    /// </summary>
    /// <param name="employee">Сотрудник, которому отправляется колокольчик.</param>
    /// <param name="title">Заголовок сообщения.</param>
    /// <param name="content">Содержимое сообщение.</param>
    /// <param name="workProcess">Имя типа задачи</param>
    /// <param name="workItem">Имя типа задания</param>
    /// <param name="document">Документ, который надо вложить в сообщение</param>
    [Public]
    public virtual void SendEssNotification(EssPlatformSolution.IEmployee employee, string title, string content,
                                            string workProcess, string workItem,
                                            Sungero.Content.IElectronicDocument document)
    {
      try
      {
        SendEssNotification(employee, title, content, workProcess, workItem, document, 1);
      }
      catch (Exception ex)
      {
        Logger.Error(ex.Message);
        throw ex;
      }
    }

    /// <summary>
    /// Отправка сообщения в ЛК через MessageBroker.
    /// </summary>
    /// <param name="employee">Сотрудник, которому отправляется колокольчик.</param>
    /// <param name="title">Заголовок сообщения.</param>
    /// <param name="content">Содержимое сообщение.</param>
    /// <param name="workProcess">Имя типа задачи</param>
    /// <param name="workItem">Имя типа задания</param>
    /// <param name="document">Документ, который надо вложить в сообщение</param>
    /// <param name="priority">Важность/срочность сообщения. 0 - высокая, 1 - обычная, 2 - низкая</param>
    [Public]
    public virtual void SendEssNotification(EssPlatformSolution.IEmployee employee, string title, string content,
                                            string workProcess, string workItem,
                                            Sungero.Content.IElectronicDocument document, int priority)
    {
      try
      {
        List<Sungero.Content.IElectronicDocument> docList = new List<Sungero.Content.IElectronicDocument>();
        docList.Add(document);
        SendEssNotification(employee, title, content, workProcess, workItem, docList, priority);
      }
      catch (Exception ex)
      {
        Logger.Error(ex.Message);
        throw ex;
      }
    }

    /// <summary>
    /// Отправка сообщения в ЛК через MessageBroker.
    /// </summary>
    /// <param name="employee">Сотрудник, которому отправляется колокольчик.</param>
    /// <param name="title">Заголовок сообщения.</param>
    /// <param name="content">Содержимое сообщение.</param>
    /// <param name="workProcess">Имя типа задачи</param>
    /// <param name="workItem">Имя типа задания</param>
    /// <param name="documents">Список документов (приведенных к ElectronicDocument), которые надо вложить в сообщение</param>
    [Public]
    public virtual void SendEssNotification(EssPlatformSolution.IEmployee employee, string title, string content,
                                            string workProcess, string workItem,
                                            List<Sungero.Content.IElectronicDocument> documents)
    {
      try
      {
        SendEssNotification(employee, title, content, workProcess, workItem, documents, 1);
      }
      catch (Exception ex)
      {
        Logger.Error(ex.Message);
        throw ex;
      }
    }

    /// <summary>
    /// Отправка сообщения в ЛК через MessageBroker.
    /// </summary>
    /// <param name="employee">Сотрудник, которому отправляется колокольчик.</param>
    /// <param name="title">Заголовок сообщения.</param>
    /// <param name="content">Содержимое сообщение.</param>
    /// <param name="workProcess">Имя типа задачи</param>
    /// <param name="workItem">Имя типа задания</param>
    /// <param name="documents">Список документов (приведенных к ElectronicDocument), которые надо вложить в сообщение</param>
    /// <param name="priority">Важность/срочность сообщения. 0 - высокая, 1 - обычная, 2 - низкая</param>
    [Public]
    public virtual void SendEssNotification(EssPlatformSolution.IEmployee employee, string title, string content, string workProcess, string workItem,
                                            List<Sungero.Content.IElectronicDocument> documents, int priority)
    {
      try
      {
        var properties = new System.Collections.Generic.Dictionary<string, string>();
        properties.Add("WorkProcess", workProcess);
        properties.Add("WorkItem", workItem);

        System.Collections.Generic.Dictionary<string, int> attachments = null;
        if (documents != null)
        {
          attachments = new System.Collections.Generic.Dictionary<string, int>();
          foreach(var d in documents)
            attachments.Add(d.Name, d.Id);
        }
        
        EssPlatform.PublicFunctions.Module.SendEssNotification(employee, title, content, priority, properties, attachments);
      }
      catch (Exception ex)
      {
        Logger.Error(ex.Message);
        throw ex;
      }
    }
    
    /// <summary>
    /// Отправка сообщения в ЛК через MessageBroker.
    /// </summary>
    /// <param name="employee">Сотрудник, которому отправляется колокольчик.</param>
    /// <param name="title">Заголовок сообщения.</param>
    /// <param name="content">Содержимое сообщение.</param>
    /// <param name="priority">Срочность.</param>
    /// <param name="properties">Словарь (имя_параметра, значение) с дополнительными параметрами сообщения.</param>
    /// <param name="attachments">Словарь (имя_вложения, ИД-документа) с описанием вложений. Поддерживаются только документы</param>
    /// <returns>Значение.</returns>
    [Public]
    public virtual void SendEssNotification(EssPlatformSolution.IEmployee employee, string title, string content, int priority,
                                            System.Collections.Generic.Dictionary<string, string> properties,
                                            System.Collections.Generic.Dictionary<string, int> attachments)
    {
      try
      {
        var messageStruct = EssPlatform.PublicFunctions.Module.CreateEmptyEssNotification(employee);
        messageStruct.Title = title;
        messageStruct.Content = content;
        messageStruct.Priority = priority;
        
        if (properties != null)
          foreach(var p in properties)
            messageStruct.Properties.Add(p.Key, p.Value);

        if (attachments != null)
          foreach(var a in attachments)
            messageStruct.Attachments.Add(EssPlatform.PublicFunctions.Module.CreateAttachmentNotification(a.Key, a.Value));
        
        EssPlatform.PublicFunctions.Module.SendEssNotification(messageStruct);
      }
      catch (Exception ex)
      {
        Logger.Error(ex.Message);
        throw ex;
      }
    }
    
    /// <summary>
    /// Отправка сообщения в ЛК через MessageBroker
    /// </summary>
    /// <param name="message">Структура с описанием сообщения.</param>
    [Public]
    public virtual void SendEssNotification(EssPlatform.Structures.Module.IMessageBrokerNotification message)
    {
      //Если отключена настройка "Подключить сервис идентификации", то последующие проверки выполняться не будут.
      if (!EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected())
        return;
      
      try
      {
        string jsonstring = SerializedToJson(message);
        var baseUri = PublicFunctions.EssSetting.GetSettings().MessageBrokerAddress;
        var requestApi = Constants.Module.RequestApiMessageBroker.MessageBrokerMessages;
        var token = GetAnAuthenticationTokenMessageBroker();
        RunPostRequest(baseUri, requestApi, jsonstring, token);
      }
      catch (Exception ex)
      {
        Logger.Error(ex.Message);
        throw ex;
      }
    }

    #endregion

    #region Работа с токенами

    /// <summary>
    /// Получить токен аутентификации от Ids.
    /// </summary>
    /// <param name="login">Логин.</param>
    /// <param name="password">Пароль.</param>.</param>
    /// <param name="serviceName">Имя сервиса для которого выдаётся токен.</param>
    /// <param name="idsUrl">Адрес сервиса Ids.</param>
    /// <returns>Токен, если аутентифкация пройдена. Исключение, если ошибка.</returns>
    [Public, Remote]
    public static string GetAnAuthenticationToken(string login, string password, string serviceName, string idsUrl)
    {
      try
      {
        var result = RunPostRequest(idsUrl, string.Format(EssPlatform.Constants.Module.RequestApiIdentity.Authentication, login, password, serviceName), String.Empty, String.Empty);
        if (result.StatusCode != (int)HttpStatusCode.OK)
          throw new Exception(DirRX.EssPlatform.Resources.GetAnAuthenticationTokenError);
        else
          return result.RequestMessage;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetAnAuthenticationToken(). An error occurred while sending the request: {0}.", ex.Message);
        throw ex;
      }
    }
    
    /// <summary>
    /// Получить токен аутентификации от Ids для текущей системы.
    /// </summary>
    /// <returns>Токен, если аутентифкация пройдена. Исключение, если ошибка.</returns>
    [Public, Remote]
    public static string GetAnAuthenticationTokenCurrentSystem()
    {
      try
      {
        return GetAnAuthenticationToken(EssPlatform.Constants.Module.Audiences.CurrentSystem);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetAnAuthenticationTokenCurrentSystem(). An error occurred while sending the request: {0}.", ex.Message);
        throw ex;
      }
    }

    /// <summary>
    /// Получить токен аутентификации от Ids для указанного сервиса.
    /// Параметры подключения к Identity Service будут получены из настроек.
    /// </summary>
    /// <param name="serviceName">Имя сервиса для которого выдаётся токен.</param>
    /// <returns>Токен, если аутентифкация пройдена. Исключение, если ошибка.</returns>
    [Public, Remote]
    public static string GetAnAuthenticationToken(string serviceName)
    {
      var settings = PublicFunctions.EssSetting.GetSettings();
      var password = string.Empty;
      var idSUrl = string.Empty;
      var login = string.Empty;
      if (!string.IsNullOrEmpty(settings.IdentityServiceLogin))
        login = settings.IdentityServiceLogin;
      else
        throw AppliedCodeException.Create(string.Format(DirRX.EssPlatform.Resources.EmptyIdentityServiceField, settings.Info.Properties.IdentityServiceLogin.LocalizedName));
      
      if (!string.IsNullOrEmpty(settings.IdentityServicePassword))
        password = Encryption.Decrypt(settings.IdentityServicePassword);
      else
        throw AppliedCodeException.Create(string.Format(DirRX.EssPlatform.Resources.EmptyIdentityServiceField, settings.Info.Properties.IdentityServicePassword.LocalizedName));
      
      if (!string.IsNullOrEmpty(settings.IdentityServiceAddress))
        idSUrl = settings.IdentityServiceAddress;
      else
        throw AppliedCodeException.Create(string.Format(DirRX.EssPlatform.Resources.EmptyIdentityServiceField, settings.Info.Properties.IdentityServiceAddress.LocalizedName));
      
      try
      {
        return GetAnAuthenticationToken(login, password, serviceName, idSUrl);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetAnAuthenticationToken(). An error occurred while sending the request: {0}.", ex.Message);
        throw ex;
      }
    }


    /// <summary>
    /// Получить токен аутентификации от Ids для MessageBroker
    /// </summary>
    /// <returns>Токен, если аутентифкация пройдена. Исключение, если ошибка.</returns>
    [Public, Remote]
    public static string GetAnAuthenticationTokenMessageBroker()
    {
      try
      {
        return GetAnAuthenticationToken(EssPlatform.Constants.Module.Audiences.MessageBroker);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetAnAuthenticationTokenCurrentSystem(). An error occurred while sending the request: {0}.", ex.Message);
        throw ex;
      }
    }
    
    #endregion
    
    #region Низкоуровневые http-запросы

    /// <summary>
    /// Сериализовать объект в Json.
    /// </summary>
    /// <param name="obj">Объект.</param>
    /// <returns>Строка в формате JSON.</returns>
    [Public]
    public string SerializedToJson(object obj)
    {
      return JsonConvert.SerializeObject(obj);
    }
    
    /// <summary>
    /// Отправить POST-запрос.
    /// </summary>
    /// <param name="baseUri">url сервиса</param>
    /// <param name="requestApi">добавочный url</param>
    /// <param name="jsonContent">Контент.</param>
    /// <param name="token">Токен</param>
    /// <returns>Структура с кодом результата запроса и сообщением либо исключение.
    /// Успешное выполнение запроса - статус 200 и сообщение ответа.
    /// Указан некорректный api-метод - статус 404 и пустое сообщение.
    /// Неавторизованный доступ (неправильный токен) - выброс исключения: UnauthorizedAccessException.
    /// У пользователя нет доступа (в токене нет нужной роли) - выброс исключения: UnauthorizedAccessException.
    /// Другие исключения пробрасываются выше по вызову с текстом сообщения исключения.</returns>
    [Public, Remote]
    public static Structures.Module.IHttpRequestResult RunPostRequest(string baseUri, string requestApi, string jsonContent, string token)
    {
      try
      {
        var result = Structures.Module.HttpRequestResult.Create();
        
        var httpClient = new HttpClient { BaseAddress = new Uri(baseUri) };
        if (!string.IsNullOrEmpty(token))
        {
          httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
          httpClient.DefaultRequestHeaders.Accept.Clear();
        }
        
        // Добавление контента.
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        
        var response = httpClient.PostAsync(requestApi, httpContent).Result;
        var statusCode = response.StatusCode;
        var requestMessage = response.Content.ReadAsStringAsync().Result;
        
        
        // Ошибки авторизации пробрасывать дальше как ошибку, остальные результаты будем трактовать в зависимости от контекста вызова.
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
          Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.RunGetRequest(). Authentication error {0} {1}.", (int)statusCode, requestMessage);
          throw new UnauthorizedAccessException(DirRX.EssPlatform.Resources.AuthenticationErrorText);
        }
        if (statusCode == HttpStatusCode.Forbidden)
        {
          Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.RunGetRequest(). Authentication error {0} {1}.", (int)statusCode, requestMessage);
          throw new UnauthorizedAccessException(DirRX.EssPlatform.Resources.AuthenticationRightsErrorText);
        }

        result.StatusCode = (int)response.StatusCode;
        result.RequestMessage = response.Content.ReadAsStringAsync().Result;
        return result;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("An error occurred while sending the request: {0}.", ex.Message);
        throw ex;
      }
    }
    
    /// <summary>
    /// Отправить PATCH-запрос.
    /// </summary>
    /// <param name="baseUri">Адрес сервиса.</param>
    /// <param name="requestUri">Текст запроса.</param>
    /// <param name="jsonContent">Контент.</param>
    /// <param name="token">Токен</param>
    /// <returns>Структура с кодом результата запроса и сообщением либо исключение.
    /// Успешное выполнение запроса - статус 200 и сообщение ответа.
    /// Указан несуществующий идентификатор пользователя IdS или указан некорректный api-метод - статус 404 и пустое сообщение.
    /// Неавторизованный доступ (неправильный токен) - выброс исключения: UnauthorizedAccessException.
    /// У пользователя нет доступа (в токене нет нужной роли) - выброс исключения: UnauthorizedAccessException.
    /// Другие исключения пробрасываются выше по вызову с текстом сообщения исключения.</returns>
    [Public, Remote]
    public static Structures.Module.IHttpRequestResult RunPatchRequest(string baseUri, string requestUri, string jsonContent, string token)
    {
      try
      {
        var method = new HttpMethod("PATCH");
        var httpClient = new HttpClient { BaseAddress = new Uri(baseUri) };
        if (!string.IsNullOrEmpty(token))
        {
          httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
          httpClient.DefaultRequestHeaders.Accept.Clear();
        }
        
        // Добавление контента.
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) {Content = httpContent};
        var response = httpClient.SendAsync(request).Result;
        var statusCode = response.StatusCode;
        var requestMessage = response.Content.ReadAsStringAsync().Result;
        // Ошибки авторизации пробрасывать дальше как ошибку, остальные результаты будем трактовать в зависимости от контекста вызова.
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
          Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.RunGetRequest(). Authentication error {0} {1}.", (int)statusCode, requestMessage);
          throw new UnauthorizedAccessException(DirRX.EssPlatform.Resources.AuthenticationErrorText);
        }
        if (statusCode == HttpStatusCode.Forbidden)
        {
          Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.RunGetRequest(). Authentication error {0} {1}.", (int)statusCode, requestMessage);
          throw new UnauthorizedAccessException(DirRX.EssPlatform.Resources.AuthenticationRightsErrorText);
        }
        var httpRequestResult = new Structures.Module.HttpRequestResult();
        httpRequestResult.StatusCode = (int)statusCode;
        httpRequestResult.RequestMessage = requestMessage;
        return httpRequestResult;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("An error occurred while sending the request: {0}.", ex.Message);
        throw ex;
      }
    }
    
    /// <summary>
    /// Отправить DELETE-запрос.
    /// </summary>
    /// <param name="baseUri">url сервиса</param>
    /// <param name="requestApi">добавочный url</param>
    /// <param name="token">Токен</param>
    /// <returns>Структура с кодом результата запроса и сообщением либо исключение.
    /// Успешное выполнение запроса - статус 200 и сообщение ответа.
    /// Указан несуществующий идентификатор пользователя IdS или указан некорректный api-метод - статус 404 и пустое сообщение.
    /// Неавторизованный доступ (неправильный токен) - выброс исключения: UnauthorizedAccessException.
    /// У пользователя нет доступа (в токене нет нужной роли) - выброс исключения: UnauthorizedAccessException.
    /// Другие исключения пробрасываются выше по вызову с текстом сообщения исключения.</returns>
    [Public, Remote]
    public static Structures.Module.IHttpRequestResult RunDeleteRequest(string baseUri, string requestApi, string token)
    {
      try
      {
        var result = Structures.Module.HttpRequestResult.Create();
        var httpClient = new HttpClient { BaseAddress = new Uri(baseUri) };
        if(!string.IsNullOrEmpty(token)) {
          httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
          httpClient.DefaultRequestHeaders.Accept.Clear();
        }
        var response = httpClient.DeleteAsync(requestApi).Result;
        var statusCode = response.StatusCode;
        var requestMessage = response.Content.ReadAsStringAsync().Result;
        result.StatusCode = (int)statusCode;
        
        // Ошибки авторизации пробрасывать дальше как ошибку, остальные результаты будем трактовать в зависимости от контекста вызова.
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
          Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.RunGetRequest(). Authentication error {0} {1}.", (int)statusCode, requestMessage);
          throw new UnauthorizedAccessException(DirRX.EssPlatform.Resources.AuthenticationErrorText);
        }
        if (statusCode == HttpStatusCode.Forbidden)
        {
          Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.RunGetRequest(). Authentication error {0} {1}.", (int)statusCode, requestMessage);
          throw new UnauthorizedAccessException(DirRX.EssPlatform.Resources.AuthenticationRightsErrorText);
        }
        
        result.RequestMessage = response.Content.ReadAsStringAsync().Result;
        return result;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("RunDeleteRequest(). An error occurred while sending the request: {0}.", ex.Message);
        throw ex;
      }
    }
    
    /// <summary>
    /// Отправить GET-запрос.
    /// </summary>
    /// <param name="baseUri">Адрес сервиса.</param>
    /// <param name="requestUri">Текст запроса.</param>
    /// <param name="token">Токен.</param>
    /// <returns>Структура с кодом результата запроса и сообщением либо исключение.
    /// Успешное выполнение запроса - статус 200 и сообщение ответа.
    /// Указан несуществующий идентификатор пользователя IdS или указан некорректный api-метод - статус 404 и пустое сообщение.
    /// Неавторизованный доступ (неправильный токен) - выброс исключения: UnauthorizedAccessException.
    /// У пользователя нет доступа (в токене нет нужной роли) - выброс исключения: UnauthorizedAccessException.
    /// Другие исключения пробрасываются выше по вызову с текстом сообщения исключения.</returns>
    [Public]
    public static Structures.Module.HttpRequestResult RunGetRequest(string baseUri, string requestUri, string token)
    {
      try
      {
        var httpClient = new HttpClient { BaseAddress = new Uri(baseUri) };
        if (!string.IsNullOrEmpty(token))
        {
          httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
          httpClient.DefaultRequestHeaders.Accept.Clear();
        }
        var response = httpClient.GetAsync(requestUri).Result;
        var statusCode = response.StatusCode;
        var requestMessage = response.Content.ReadAsStringAsync().Result;
        // Ошибки авторизации пробрасывать дальше как ошибку, остальные результаты будем трактовать в зависимости от контекста вызова.
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
          Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.RunGetRequest(). Authentication error {0} {1}.", (int)statusCode, requestMessage);
          throw new UnauthorizedAccessException(DirRX.EssPlatform.Resources.AuthenticationErrorText);
        }
        if (statusCode == HttpStatusCode.Forbidden)
        {
          Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.RunGetRequest(). Authentication error {0} {1}.", (int)statusCode, requestMessage);
          throw new UnauthorizedAccessException(DirRX.EssPlatform.Resources.AuthenticationRightsErrorText);
        }
        var httpRequestResult = new Structures.Module.HttpRequestResult();
        httpRequestResult.StatusCode = (int)statusCode;
        httpRequestResult.RequestMessage = requestMessage;
        return httpRequestResult;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.RunGetRequest(). An error occurred while sending the request: {0}.", ex.Message);
        throw ex;
      }
    }
    #endregion
    
    #region test_functions
    /// <summary>
    /// Mock-функция для тестирования виджета "Заявления"
    /// </summary>
    /// <returns>строка с json для виджета "Заявления".</returns>
    [Public, Remote]
    public static string GetJSON4WidgetStatementMock()
    {
      string jsonString = "";
      var jsonValue = Sungero.Docflow.PublicFunctions.Module.GetDocflowParamsValue("JSON4WidgetStatement");
      if (jsonValue != null)
        jsonString = jsonValue.ToString();
      return jsonString;
    }
    #endregion
    
    #region Функции для вызова в задачах HRLite

    /// <summary>
    /// Проверить, есть ли сотрудники, не подключенные к личному кабинету.
    /// </summary>
    /// <param name="employees">Список сотрудников.</param>
    /// <returns>True - если такие сотрудники есть, иначе false.</returns>
    [Public, Remote]
    public bool HasEmployeeNotInESS(List<IEmployee> employees)
    {
      return employees.Where(t => !HasEmployeePersonalAccountRegistred(t)).Any();
    }
    
    /// <summary>
    /// Определить зарегистрирован ли личный кабинет у сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>True - если ЛК зарегистрирован, False - если нет.</returns>
    [Public]
    public bool HasEmployeePersonalAccountRegistred(Sungero.Company.IEmployee employee)
    {
      var essPlatformEmployee = DirRX.EssPlatformSolution.Employees.As(employee);
      return essPlatformEmployee.PersonalAccountStatusDirRX == DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted;
    }
    
    /// <summary>
    /// Получить список сотрудников с зарегистрированным ЛК.
    /// </summary>
    /// <param name="employees">Список сотрудников.</param>
    /// <returns>Список сотрудников с ЛК.</returns>
    [Public]
    public virtual List<Sungero.Company.IEmployee> GetEmployeesWithPersonalAccountRegistred(List<Sungero.Company.IEmployee> employees)
    {
      return employees.Where(t => HasEmployeePersonalAccountRegistred(t)).ToList();
    }
    
    /// <summary>
    /// Получить список сотрудников без ЛК.
    /// </summary>
    /// <param name="employees">Список сотрудников.</param>
    /// <returns>Список сотрудников без ЛК.</returns>
    [Public]
    public List<Sungero.Company.IEmployee> GetEmployeesWithoutPersonalAccountRegistred(List<Sungero.Company.IEmployee> employees)
    {
      return employees.Where(t => !HasEmployeePersonalAccountRegistred(t)).ToList();
    }
    
    /// <summary>
    /// Отправить сообщение сотруднику о новом задании/уведомлении.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="isNotice">True - уведомление, false - задание.</param>
    [Public]
    public void SendNewNotification(Sungero.Company.IEmployee employee, bool isNotice)
    {
      var messageText = isNotice ? DirRX.EssPlatform.Resources.NewNoticeNotificationTextFormat(PublicFunctions.EssSetting.GetSettings().ESSAddress) :
        DirRX.EssPlatform.Resources.NewAssignmentNotificationTextFormat(PublicFunctions.EssSetting.GetSettings().ESSAddress);
      var employeeO = EssPlatformSolution.Employees.As(employee);
      if (employeeO.PersonalAccountStatusDirRX == EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted)
      {
        if (employeeO.SmsNotifDirRX.HasValue && employeeO.SmsNotifDirRX.Value)
        {
          var asyncHandler = AsyncHandlers.SendSms.Create();
          asyncHandler.userid = employee.Id;
          asyncHandler.messageText = messageText;
          asyncHandler.ExecuteAsync();
        }
        if (employeeO.EmailNotifDirRX.HasValue && employeeO.EmailNotifDirRX.Value)
        {
          var messageSubject = isNotice ? DirRX.EssPlatform.Resources.NewNoticeNotificationSubject : DirRX.EssPlatform.Resources.NewAssignmentNotificationSubject;
          var asyncHandler = AsyncHandlers.SendEmail.Create();
          asyncHandler.userid = employee.Id;
          asyncHandler.messageText = messageText;
          asyncHandler.messageSubject = messageSubject;
          asyncHandler.ExecuteAsync();
        }
        if (employeeO.ViberNotifDirRX.HasValue && employeeO.ViberNotifDirRX.Value)
        {
          var asyncHandler = AsyncHandlers.SendMessageToViber.Create();
          asyncHandler.userid = employee.Id;
          asyncHandler.messageText = messageText;
          asyncHandler.ExecuteAsync();
        }
      }
      return;
    }
    
    #endregion
    
    #region Сервис интеграции

    // TODO После изменений в сервисе ЛК удалить эту ф-ию и обработчик openingEvetHandler из прикладной конфигурации услуги SendStatement.xml.
    // Task 89951: Некорректная работа с типами свойств шага при передаче макросов (таска команды Core).
    /// <summary>
    /// Костыль для совместителей.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <returns>ИД инициатора.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public int GetInitiatorId(int userId)
    {
      return userId;
    }
    
    /// Получить ссылку на Соглашение между участниками ЭДО.
    /// </summary>
    /// <returns>Гиперссылка документа Соглашения между участниками ЭДО.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetAgreementUrl()
    {
      var setting = EssSettings.GetAll().FirstOrDefault();
      
      if (setting.IsUsedIdentityService != true)
        return string.Empty;
      
      return setting.AgreementUrl;
    }
    
    /// <summary>
    /// Добавить в задание ActiveText.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <param name="аctiveText">Текст задания.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void AddAssignmentActiveText(int assignmentId, string аctiveText)
    {
      var assignment = Sungero.Workflow.Assignments.Get(assignmentId);
      if (assignment != null)
      {
        assignment.ActiveText = аctiveText;
        assignment.Save();
      }
    }
    
    /// <summary>
    /// Вложить в задание/задачу документы.
    /// </summary>
    /// <param name="entityType">Тип сущности (задача/задание).</param>
    /// <param name="entityId">ИД сущности.</param>
    /// <param name="documents">Список приложений в формате json, в которой есть ИД и имя группы, в которую надо вкладывать.</param>
    /// <remarks>Пример структуры вложений: "documents": "[{\"EntityId\":\"260\",\"Group\":\"AddendaGroup\"}]".
    /// Сейчас функция добавляет вложение в группу не смотря на то, какой тип указан в этой группе.</remarks>
    [Remote,Public(WebApiRequestType = RequestType.Post)]
    public static void AttachDocuments(string entityType, int entityId, string documents)
    {
      if (!string.IsNullOrEmpty(entityType))
      {
        Sungero.Workflow.ITask entity = null;
        if (entityType.ToLower().Contains("task"))
          entity = Sungero.Workflow.Tasks.GetAll(task => task.Id == entityId).FirstOrDefault();
        else if (entityType.ToLower().Contains("assignment"))
        {
          var assignment = Sungero.Workflow.Assignments.GetAll(assingment => assingment.Id == entityId).FirstOrDefault();
          if (assignment != null)
            entity = assignment.Task;
        }
        
        if (entity != null)
        {
          var documentsList = JsonConvert.DeserializeObject<List<Structures.Module.AttachDocumentsInfo>>(documents);
          
          foreach (var document in documentsList)
          {
            var attachment = Sungero.Content.ElectronicDocuments.GetAll(x => x.Id == int.Parse(document.EntityId)).FirstOrDefault();
            // HACK: обходное решение для добавления документов в SimpleTask.
            if (!string.IsNullOrEmpty(document.Group))
            {
              var attachmentGroupePropertyInfo = entity.GetType().GetProperty(document.Group);
              var attachmentGroupeValue = attachmentGroupePropertyInfo?.GetValue(entity);
              var attachmentsTypePropertyInfo = attachmentGroupeValue?.GetType().GetProperty("All");
              var attachmentsTypeValue = attachmentsTypePropertyInfo?.GetValue(attachmentGroupeValue);
              var addAttachmentMethod = attachmentsTypePropertyInfo?.PropertyType.GetMethod("Add", new[] { attachment.GetType() });
              addAttachmentMethod?.Invoke(attachmentsTypeValue, new object[] { attachment });
            }
            else
            {
              entity.Attachments.Add(attachment);
            }
          }
          entity.Save();
        }
      }
    }
    
    /// <summary>
    /// Добавить подпись к документу
    /// </summary>
    /// <param name="documentId">ИД документа.</param>
    /// <param name="versionNumber">Версия документа.</param>
    /// <param name="signatureType">Тип подписи.</param>
    /// <param name="signedByEmployeeId">ИД подписанта.</param>
    /// <param name="base64Signature">Подпись в base64.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void AddSignature(int documentId, int versionNumber, string signatureType, int signedByEmployeeId, string base64Signature)
    {
      var document = Sungero.Content.ElectronicDocuments.GetAll(x => x.Id == documentId).FirstOrDefault();
      var signature = Convert.FromBase64String(base64Signature);
      var user = Users.GetAll(x => x.Id == signedByEmployeeId).FirstOrDefault();
      if (user != null)
      {
        var signedByEmployeeName = user.Name;
        Sungero.Content.IElectronicDocumentVersions version = null;

        if (versionNumber > 0)
        {
          version = document.Versions.SingleOrDefault(v => v.Number == versionNumber);
        }
        else
        {
          version = document.LastVersion;
        }
        
        var type = (SignatureType) Enum.Parse(typeof(SignatureType), signatureType);
        Signatures.Import(document, type, signedByEmployeeName, signature, version);
        document.Save();
      }
    }
    
    /// <summary>
    /// Стартовать задачу.
    /// </summary>
    /// <param name="taskId">ИД задачи.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void StartTask(int taskId)
    {
      var task = Sungero.Workflow.Tasks.GetAll(x => x.Id == taskId).FirstOrDefault();
      if (task != null)
        task.Start();
    }
    
    /// <summary>
    /// Прекратить выполнение задачи.
    /// </summary>
    /// <param name="taskId">ИД задачи.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void AbortTask(int taskId)
    {
      var task = Sungero.Workflow.Tasks.GetAll(x => x.Id == taskId).FirstOrDefault();
      if (task != null)
        task.Abort();
    }
    
    /// <summary>
    /// Получить информацию об исполнителе задачи/задания.
    /// </summary>
    /// <param name="employeeId">ИД задачи/задания.</param>
    /// <param name="type">Тип (Задание/задача).</param>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetPerformerInfo(int entityId, string entityType)
    {
      try
      {
        var performerInfo = Structures.Module.PerformerInfo.Create();
        if (!string.IsNullOrEmpty(entityType))
        {
          if (entityType.ToLower().Contains("task"))
          {
            var task = Sungero.Workflow.Tasks.GetAll(t => t.Id == entityId).FirstOrDefault();
            if (task != null)
            {
              var employee = Employees.As(task.Author);
              performerInfo.FullName = employee.Person.Name;
              performerInfo.Organization = employee.Department.BusinessUnit.Name;
              performerInfo.Department = employee.Department.Name;
              performerInfo.Position = employee.JobTitle.Name;
            }
          }
          else if (entityType.ToLower().Contains("assignment"))
          {
            var assignment = Sungero.Workflow.Assignments.GetAll(a => a.Id == entityId).FirstOrDefault();
            if (assignment != null)
            {
              var employee = Employees.As(assignment.Performer);
              performerInfo.FullName = employee.Person.Name;
              performerInfo.Organization = employee.Department.BusinessUnit.Name;
              performerInfo.Department = employee.Department.Name;
              performerInfo.Position = employee.JobTitle.Name;
            }
          }
        }
        return SerializedToJson(performerInfo);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetPerformerInfo(). {0}", ex.Message);
        return "[]";
      }
    }
    
    /// <summary>
    /// Получить список должностей персоны.
    /// </summary>
    /// <param name="personId">Идентификатор персоны.</param>
    /// <returns>Список должностей персоны.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetPostListByPerson(int personId)
    {
      try
      {
        var employees = EssPlatformSolution.Employees.GetAll(w => w.Status == Sungero.Company.Employee.Status.Active && w.Person != null && w.Person.Id == personId).OrderByDescending(w => w.EmploymentType);
        var postList = new List<Structures.Module.IPersonPostInfo>();
        
        if (employees.Count() > 1)
        {
          foreach (var employee in employees)
          {
            var post = Structures.Module.PersonPostInfo.Create();
            post.id = employee.Id;
            var department = employee.Department;
            post.organization = department?.BusinessUnit?.Name;
            post.department = department?.Name;
            post.title = employee.JobTitle?.Name;
            post.isPrimary = employee.EmploymentType == EssPlatformSolution.Employee.EmploymentType.MainPlace;
            
            postList.Add(post);
          }
        }
        
        var personPostList = Structures.Module.PersonPostListInfo.Create();
        personPostList.positions = postList;
        
        return SerializedToJson(personPostList);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetPostListByPerson(). {0}", ex.Message);
        return "[]";
      }
    }
    
    /// <summary>
    /// Возвращает список идентификаторов сотрудников, у которых настроены замещения.
    /// </summary>
    /// <param name="personId">Ид персоны.</param>
    /// <returns>Список идентификаторов сотрудников.
    /// Если у персоны нет замещений, возвращется список из одного идентификатора.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public List<int> GetPersonEmployeeIds(int personId)
    {
      try
      {
        var substitutions = Substitutions.GetAll(s => s.EndDate == null || s.EndDate.Value > Calendar.Now);
        var employees = Employees.GetAll(e => e.Person != null && e.Status == Sungero.Company.Employee.Status.Active && e.Person.Id.Equals(personId)).Where(e => substitutions.Any(s => Equals(Employees.As(s.User), e) || Equals(Employees.As(s.Substitute), e)));
        if (employees.Count() > 0)
        {
          return employees.Select(e => e.Id).ToList();
        }
        var result = new List<int>();
        result.Add(EssPlatformSolution.Employees.GetAll(w => w.Status == Sungero.Company.Employee.Status.Active && w.Person != null && w.Person.Id == personId)
                   .OrderByDescending(w => w.EmploymentType).Select(e => e.Id).FirstOrDefault());
        return result;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetPersonEmployeeIds(). {0}", ex.Message);
        return new List<int>();
      }
    }
    
    
    /// <summary>
    /// Выполнить задание.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <param name="performResult">Результат выполнения.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void CompleteAssignment(int assignmentId, string performResult)
    {
      var assignment = Sungero.Workflow.Assignments.GetAll(assingment => assingment.Id == assignmentId).FirstOrDefault();
      if (!string.IsNullOrEmpty(performResult))
      {
        var result = new Enumeration(performResult);
        if (assignment != null)
          assignment.Complete(result);
      }
      else assignment.Complete(null);
    }
    
    /// <summary>
    /// Получить информацию о подписях документа.
    /// </summary>
    /// <param name="documentId">ИД документа.</param>
    /// <returns>Список подписей.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetSignatures(int documentId)
    {
      var document = Sungero.Content.ElectronicDocuments.GetAll(doc => doc.Id == documentId).FirstOrDefault();
      if (document != null)
      {
        var signatures = Signatures.Get(document.LastVersion).Select(signature => Structures.Module.SignatureInfo.Create(
          signature.Signatory?.Id,
          signature.SignatoryFullName,
          signature.SigningDate,
          signature.SignatureType.ToString(),
          signature.Comment,
          signature.SignCertificate?.Thumbprint
         )).ToList();
        
        return this.SerializedToJson(signatures);
      }
      
      return "[]";
    }
    
    /// <summary>
    /// Получить вложения задачи/задания.
    /// </summary>
    /// <param name="entityType">Тип сущности.</param>
    /// <param name="entityId">ИД сущности.</param>
    /// <returns>Список вложений.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetAttachments(string entityType, int entityId)
    {
      if (!string.IsNullOrEmpty(entityType))
      {
        Sungero.Workflow.ITask entity = null;
        if (entityType.ToLower().Contains("task"))
          entity = Sungero.Workflow.Tasks.GetAll(task => task.Id == entityId).FirstOrDefault();
        else if (entityType.ToLower().Contains("assignment"))
        {
          var assignment = Sungero.Workflow.Assignments.GetAll(assingment => assingment.Id == entityId).FirstOrDefault();
          if (assignment != null)
            entity = assignment.Task;
        }
        
        if (entity != null)
        {
          var attachments = entity.AllAttachments.Where(a => Sungero.Content.ElectronicDocuments.Is(a)).Select(a => EssPlatform.Structures.Module.AttachmentInfo.Create(
            a.Id.ToString(),
            a.DisplayValue,
            "Document",
            "IElectronicDocuments",
            entity.AttachmentsInfo.FirstOrDefault(i => i.IsLinkedTo(a))?.GroupName,
            Sungero.Content.ElectronicDocuments.As(a)?.Modified,
            Sungero.Content.ElectronicDocuments.As(a)?.AssociatedApplication?.Extension,
            Sungero.Content.ElectronicDocuments.As(a)?.LastVersion?.Body.Size)).ToList();
          
          return SerializedToJson(attachments);
        }
        
        return "[]";
      }
      
      return "[]";
    }
    
    /// <summary>
    /// Получить идентификатор сотрудника по идентификатору пользователя.
    /// </summary>
    /// <param name="uniqueId">Глобальный уникальный идентификатор пользователя.</param>
    /// <returns>Идентификатор сотрудника по идентификатору пользователя.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public int GetEmployeeByUniqueId(string uniqueId)
    {
      try
      {
        var guid = new Guid(uniqueId.Split('@').Last());
        var employees = Sungero.Company.Employees.GetAll(e => e.Sid.Value.Equals(guid));
        return employees.FirstOrDefault()?.Id ?? -1;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetEmployeeByUniqueId(). {0}", ex.Message);
        return 0;
      }
    }
    
    /// <summary>
    /// Создать асинхронное событие для изменения статуса личного кабинета для сотрудника на "Принято"
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <description>Вызывается в Личном кабинете.</description>
    [Public(WebApiRequestType = RequestType.Post)]
    public void ChangePersonalAccountStatusToInviteAccepted(int employeeId)
    {
      var asyncChangeEssStatus = EssPlatform.AsyncHandlers.ChangeEmployeeEssStatus.Create();
      asyncChangeEssStatus.EmployeeId = employeeId;
      asyncChangeEssStatus.EssStatus = EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted.ToString();
      asyncChangeEssStatus.ExecuteAsync();
    }

    /// <summary>
    /// Добавить штамп с ЭП в последнюю версию документа.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <return>Возвращает признак успешности добавления штампа.</return>
    [Public(WebApiRequestType = RequestType.Post)]
    public bool AddStamp(int documentId)
    {
      try
      {
        var document = Sungero.Content.ElectronicDocuments.Get(documentId);
        var lastVersion = document.LastVersion;
        var versionSignature = Signatures.Get(lastVersion).Where(s => s.SignatureType == SignatureType.Approval && s.IsExternal == true).FirstOrDefault();
        if (versionSignature != null)
        {
          using (var lastVersionBodyStream = new System.IO.MemoryStream())
          {
            lastVersion.Body.Read().CopyTo(lastVersionBodyStream);
            var extension = lastVersion.BodyAssociatedApplication.Extension;
            var signatureMark = Sungero.Docflow.PublicFunctions.Module.GetSignatureMarkForCertificateAsHtml(versionSignature);
            var pdfConverter = new Sungero.AsposeExtensions.Converter();
            var pdfStream = pdfConverter.GeneratePdf(lastVersionBodyStream, extension);
            var pdfDocumentStream = pdfConverter.AddSignatureMark(pdfStream, extension, signatureMark, Sungero.Docflow.Resources.SignatureMarkAnchorSymbol, 5);
            lastVersion.PublicBody.Write(pdfDocumentStream);
            lastVersion.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");
            pdfStream.Close();
            pdfDocumentStream.Close();
          }
          document.Save();
        }
        return true;
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("AddStamp() error: {0}", ex.StackTrace);
        return false;
      }
    }
    
    /// <summary>
    /// Получить должность сотрудника.
    /// </summary>
    /// <param name="employeeId">Идентификатор сотрудника.</param>
    /// <param name="declensionCase">Падеж.</param>
    /// <returns>Должность сотрудника</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetEmployeePosition(int employeeId, string declensionCase)
    {
      try
      {
        var employee = Sungero.Company.Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
        if (Enum.TryParse<DeclensionCase>(declensionCase, out DeclensionCase resultCase))
        {
          return Sungero.Company.PublicFunctions.Employee.GetJobTitle(employee, resultCase);
        }
        return employee.JobTitle.DisplayValue;
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("GetEmployeePosition() error: {0}", ex.StackTrace);
        return string.Empty;
      }
    }
    
    #endregion
  }
}