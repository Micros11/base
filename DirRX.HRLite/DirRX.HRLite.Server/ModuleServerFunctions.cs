using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sungero.Commons;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Parties;
using Sungero.Workflow;
using Sungero.AsposeExtensions;
using Aspose.Pdf;
using Aspose.Words;
using Aspose.Words.Tables;
using Aspose.Cells;
using PdfConverter;
using PdfSharp.Pdf;
using PdfS = PdfSharp.Pdf;
using Newtonsoft.Json.Linq;
using SettingsConstants = DirRX.HRLite.Constants.Module.StatementsSetting;

namespace DirRX.HRLite.Server
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Обновить поле "Согласие на КЭДО" в карточке сотрудника асинхронно.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="consentStatus">Значение поля "Согласие на КЭДО".</param>
    public void ChangeConsentStatusAsync(HRSolution.IEmployee employee, string consentStatus)
    {
      var asyncChangeConsentStatus = HRLite.AsyncHandlers.ChangeUserConsentStatus.Create();
      asyncChangeConsentStatus.EmployeeId = employee.Id;
      asyncChangeConsentStatus.ConsentStatus = consentStatus;
      var oldConsentStatus = employee.ConsentDirRX;
      if (oldConsentStatus.HasValue)
        asyncChangeConsentStatus.OldConsentStatus = oldConsentStatus.Value.ToString();
      asyncChangeConsentStatus.ExecuteAsync();
    }
    
    #region Работа с типами документов.
    /// <summary>
    /// Проверить, является ли тип документа кадровым договорным.
    /// </summary>
    /// <param name="documentType">Тип документа.</param>
    /// <returns>True, если переданный тип - кадровй договорной документ, иначе false.</returns>
    [Public, Remote]
    public virtual bool IsBilaterialDocument(IDocumentType documentType)
    {
      return documentType != null && Equals(Guid.Parse(documentType.DocumentTypeGuid), BilaterialDocument.ClassTypeGuid);
    }
    
    /// <summary>
    /// Проверить, является ли тип документа кадровым.
    /// </summary>
    /// <param name="documentType">Тип документа.</param>
    /// <returns>True, если переданный тип - кадровй документ, иначе false.</returns>
    [Public, Remote]
    public virtual bool IsPersonnelDocument(IDocumentType documentType)
    {
      return documentType != null && Equals(Guid.Parse(documentType.DocumentTypeGuid), PersonnelDocument.ClassTypeGuid);
    }
    
    /// <summary>
    /// Проверить, является ли тип документа ЛНА.
    /// </summary>
    /// <param name="documentType">Тип документа.</param>
    /// <returns>True, если переданный тип - ЛНА, иначе false.</returns>
    [Public, Remote]
    public virtual bool IsLocalRegulationDocument(IDocumentType documentType)
    {
      return documentType != null && Equals(Guid.Parse(documentType.DocumentTypeGuid), LocalRegulationDocument.ClassTypeGuid);
    }
    
    /// <summary>
    /// Проверить, является ли тип документа заявлением.
    /// </summary>
    /// <param name="documentType">Тип документа.</param>
    /// <returns>True, если переданный тип - заявление, иначе false.</returns>
    [Public, Remote]
    public virtual bool IsStatement(IDocumentType documentType)
    {
      return documentType != null && Equals(Guid.Parse(documentType.DocumentTypeGuid), StatementDocument.ClassTypeGuid);
    }
    
    /// <summary>
    /// Проверить, является ли тип документа заявлением.
    /// </summary>
    /// <param name="documentTypeGuid">Guid типа документа.</param>
    /// <returns>True, если переданный тип - заявление, иначе false.</returns>
    [Public, Remote]
    public virtual bool IsStatement(Guid documentTypeGuid)
    {
      return documentTypeGuid != null && Equals(documentTypeGuid, StatementDocument.ClassTypeGuid);
    }
    #endregion
    
    #region Вспомогательные функции.
    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <param name="text">Текст инструкции.</param>
    /// <returns>Модель состояния.</returns>
    public StateView GetAssignmentStateView(string text)
    {
      var stateView = Sungero.Core.StateView.Create();
      var block = stateView.AddBlock();
      var content = block.AddContent();
      content.AddLabel(text);
      block.ShowBorder = false;
      return stateView;
    }
    
    /// <summary>
    /// Получить переданный текст с первым символом в нижнем регистре.
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <returns>Текст с первым символом в нижнем регистре.</returns>
    [Public]
    public string GetTextWithLowerCaseFirstSymbol(string text)
    {
      if (string.IsNullOrEmpty(text))
        return string.Empty;
      return text.Substring(0, 1).ToLower() + text.Substring(1);
    }
    
    /// <summary>
    /// Записать информацию в файл.
    /// </summary>
    /// <param name="content">Информация.</param>
    /// <returns>Имя файла.</returns>
    public string SaveByteArrayToTempFile(byte[] content)
    {
      var filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
      System.IO.File.WriteAllBytes(filePath, content);
      return filePath;
    }
    #endregion

    #region Проверка права подписи
    /// <summary>
    /// Получить действующие права подписи по виду документа и нашим организациям.
    /// </summary>
    /// <param name="businessUnit">Наши организации.</param>
    /// <param name="kind">Виды документа.</param>
    /// <returns>Действующие права подписи.</returns>
    public virtual IQueryable<ISignatureSetting> GetSignatureSettings(IBusinessUnit businessUnit, IDocumentKind kind)
    {
      var today = Calendar.UserToday;
      return SignatureSettings.GetAll()
        .Where(s => s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active)
        .Where(s => !s.BusinessUnits.Any() || s.BusinessUnits.Any(k => businessUnit.Equals(k.BusinessUnit)))
        .Where(s => !s.DocumentKinds.Any() || s.DocumentKinds.Any(k => kind.Equals(k.DocumentKind)))
        .Where(s => (!s.ValidFrom.HasValue || s.ValidFrom.Value <= today) &&
               (!s.ValidTill.HasValue || s.ValidTill.Value >= today));
    }
    
    /// <summary>
    /// Имеет ли право подписи сотрудник по виду документа и нашим организациям.
    /// </summary>
    /// <param name="businessUnit">Наши организации.</param>
    /// <param name="kind">Виды документа.</param>
    /// <param name="recipient">Субъект прав.</param>
    /// <returns>Признак права подписи.</returns>
    [Public]
    public virtual bool IsSignRight(IBusinessUnit businessUnit, IDocumentKind kind, IRecipient recipient)
    {
      var signRights = this.GetSignatureSettings(businessUnit, kind);
      return signRights.Where(s => s.Recipient == recipient || s.Recipient == Roles.AllUsers).Any();
    }
    #endregion
    
    #region Информирование.
    /// <summary>
    /// Создать задачу на информирование.
    /// </summary>
    /// <returns>Задача на информирование.</returns>
    [Remote]
    public virtual IInformingTask CreateInformingTask()
    {
      return InformingTasks.Create();
    }
    
    /// <summary>
    /// Отправить уведомление - результат об отправке согласия на КЭДО.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="text">Текст уведомления.</param>
    [Public]
    public virtual void SendNoticeAboutConsentSending(IUser user, string text)
    {
      var recipients = new[] {Recipients.As(user)};
      var task = Sungero.Workflow.SimpleTasks.CreateWithNotices(Resources.ConsentSendingResult, recipients);
      task.ActiveText = text;
      task.Start();
    }
    #endregion
    
    #region Администрирование.
    /// <summary>
    /// Найти роли модуля.
    /// </summary>
    /// <returns>Список ролей модуля.</returns>
    [Remote]
    public virtual List<IRole> GetHRLiteRoles()
    {
      return Sungero.CoreEntities.Roles.GetAll(r => Equals(r, GetRole(Constants.Module.Roles.HRSpecialist))).ToList();
    }
    
    /// <summary>
    /// Проверить, что пользователь включен в роль.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <param name="roleGuid">Guid роли.</param>
    /// <returns>True, если один из пользователей входит в роль.</returns>
    public bool IsIncludedInRole(List<IUser> users, Guid roleGuid)
    {
      var role = Functions.Module.GetRole(roleGuid);
      
      if (role == null)
        return false;
      return users.Any(v => v.IncludedIn(role));
    }
    
    /// <summary>
    /// Проверить, что пользователь включен в роль Специалист по работе с персоналом.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <returns>True, если один из пользователей входит в роль Специалист по работе с персоналом.</returns>
    [Remote(IsPure = true), Public]
    public virtual bool IsHRSpecialist(List<IUser> users)
    {
      return this.IsIncludedInRole(users, Constants.Module.Roles.HRSpecialist);
    }
    
    /// <summary>
    /// Проверить, что текущий пользователь включен в роль Специалист по работе с персоналом.
    /// </summary>
    /// <returns>True, если текущий пользователь входит в роль Специалист по работе с персоналом.</returns>
    [Remote(IsPure = true), Public]
    public virtual bool IsCurrentUserInHRSpecialist()
    {
      return this.IsHRSpecialist(new List<IUser>() { Users.Current });
    }
    
    /// <summary>
    /// Проверить, что текущий пользователь включен в роль Сотрудники, которые могут выполнять задания в веб-доступе.
    /// </summary>
    /// <returns>True, если текущий пользователь входит в роль Сотрудники, которые могут выполнять задания в веб-доступе.</returns>
    [Remote(IsPure = true), Public]
    public virtual bool IsCurrentUserInWebAccessEmployees()
    {
      // TODO временно убрали проверку на вхождение в спецроль. Пока всегда возвращать false.
      return false;
    }
    
    #endregion
    
    #region Связка документов с сотрудником.
    /// <summary>
    /// Создать или обновить связку между сотрудниками и документами.
    /// </summary>
    /// <param name="employees">Список сотрудников.</param>
    /// <param name="documents">Документы.</param>
    /// <param name="status">Статус.</param>
    public void CreateOrUpdateEmployeeDocumentLink(List<IEmployee> employees, List<IInternalDocumentBase> documents, Sungero.Core.Enumeration status)
    {
      foreach (var document in documents)
      {
        if (PersonnelDocuments.Is(document) || BilaterialDocuments.Is(document) || StatementDocuments.Is(document))
        {
          foreach (var employee in employees)
          {
            var links = EmployeeDocumentLinks.GetAll(l => Equals(l.Document, document) && Equals(l.Employee, employee));
            var link = EmployeeDocumentLinks.Null;
            if (links.Any())
              link = links.FirstOrDefault();
            else
            {
              link = EmployeeDocumentLinks.Create();
              link.Employee = employee;
              link.Document = document;
            }
            link.DateTime = Calendar.Now;
            link.Status = status;
            link.Save();
          }
        }
      }
    }
    
    /// <summary>
    /// Создать или обновить связку между сотрудниками и документами.
    /// </summary>
    /// <param name="recipients">Список исполнителей.</param>
    /// <param name="documents">Документы.</param>
    /// <param name="status">Статус.</param>
    public void CreateOrUpdateEmployeeDocumentLink(List<IRecipient> recipients, List<IInternalDocumentBase> documents, Sungero.Core.Enumeration status)
    {
      var employees = GetEmployeesFromRecipients(recipients);
      Functions.Module.CreateOrUpdateEmployeeDocumentLink(employees, documents, status);
    }
    
    /// <summary>
    /// Создать или обновить связку между сотрудником и документами.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="documents">Документы.</param>
    /// <param name="status">Статус.</param>
    [Public]
    public void CreateOrUpdateEmployeeDocumentLink(IEmployee employee, List<IInternalDocumentBase> documents, Sungero.Core.Enumeration status)
    {
      Functions.Module.CreateOrUpdateEmployeeDocumentLink(new List<IEmployee>() { employee }, documents, status);
    }
    
    /// <summary>
    /// Получить документы по сотруднику.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Документы по сотруднику.</returns>
    [Public, Remote]
    public List<IOfficialDocument> GetEmployeeDocuments(IEmployee employee)
    {
      var linkedDocuments = HRLite.EmployeeDocumentLinks.GetAll(l => Equals(l.Employee, employee)).Select(d => d.Document);
      return OfficialDocuments.GetAll(d => linkedDocuments.Contains(d)).ToList();
    }
    #endregion

    #region Работа с ролями и пользователями.
    /// <summary>
    /// Проверить подписано ли соглашение о КЭДО у сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    [Public, Remote]
    public bool CheckEmployeeConsent(HRSolution.IEmployee employee)
    {
      return employee.ConsentDirRX.Value == HRSolution.Employee.ConsentDirRX.Signed;
    }
    
    /// <summary>
    /// Проверить подписано ли соглашение о КЭДО у сотрудников.
    /// </summary>
    /// <param name="employees">Список сотрудников.</param>
    [Public, Remote]
    public bool CheckEmployeeConsent(List<Sungero.Company.IEmployee> employees)
    {
      return !employees.Any(e => HRSolution.Employees.As(e).ConsentDirRX.Value != HRSolution.Employee.ConsentDirRX.Signed);
    }
    
    /// <summary>
    /// Получить сотрудников по списку реципиентов с раскрытием групп и ролей.
    /// </summary>
    /// <param name="recipients">Реципиенты.</param>
    /// <returns>Раскрытый список реципиентов до действующих сотрудников.</returns>
    [Public, Remote(IsPure = true)]
    public static List<IEmployee> GetEmployeesFromRecipients(List<IRecipient> recipients)
    {
      var performers = new List<IEmployee>();
      foreach (var recipient in recipients)
      {
        if (Employees.Is(recipient))
          performers.Add(Employees.As(recipient));
        else if (Departments.Is(recipient))
          performers.AddRange(Employees.GetAll(e => Equals(e.Department, Departments.As(recipient))));
        else if (BusinessUnits.Is(recipient))
          performers.AddRange(Employees.GetAll(e => Equals(e.Department.BusinessUnit, BusinessUnits.As(recipient))));
        else if (Groups.Is(recipient))
        {
          var groupRecipient = Groups.GetAllUsersInGroup(Groups.As(recipient))
            .Where(r => Employees.Is(r)).Select(r => Employees.As(r));
          foreach (var employee in groupRecipient)
            performers.Add(employee);
        }
      }
      performers = performers.Where(p => p.Status == Sungero.CoreEntities.DatabookEntry.Status.Active).Distinct().ToList();
      return performers;
    }
    
    /// <summary>
    /// Получить несистемные активные записи исполнителей.
    /// </summary>
    /// <param name="recipients">Список участников уведомления.</param>
    /// <returns>Несистемные активные записи исполнителей.</returns>
    [Public]
    public static List<Sungero.Company.IEmployee> GetNonSystemActiveRecipients(List<IRecipient> recipients)
    {
      var activeRecipients = recipients.Where(x => x != null && x.Status == Sungero.CoreEntities.DatabookEntry.Status.Active).ToList();
      var nonSystemRecipients = Functions.Module.GetEmployeesFromRecipients(activeRecipients)
        .Where(x => x.IsSystem != true && x.Status == Sungero.CoreEntities.DatabookEntry.Status.Active)
        .Distinct()
        .ToList();
      
      return nonSystemRecipients;
    }
    
    /// <summary>
    /// Получить несистемные активные записи исполнителей.
    /// </summary>
    /// <param name="recipients">Список участников уведомления.</param>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Несистемные активные записи исполнителей из нашей организации.</returns>
    [Public]
    public static List<Sungero.Company.IEmployee> GetNonSystemActiveRecipients(List<IRecipient> recipients, IBusinessUnit businessUnit)
    {
      var activeRecipients = recipients.Where(x => x != null && x.Status == Sungero.CoreEntities.DatabookEntry.Status.Active).ToList();
      var nonSystemRecipients = Functions.Module.GetEmployeesFromRecipients(activeRecipients)
        .Where(x => x.IsSystem != true && x.Status == Sungero.CoreEntities.DatabookEntry.Status.Active && Equals(x.Department.BusinessUnit, businessUnit))
        .Distinct()
        .ToList();
      
      return nonSystemRecipients;
    }
    
    /// <summary>
    /// Получить роль.
    /// </summary>
    /// <param name="roleGuid">Guid роли.</param>
    /// <returns>Роль.</returns>
    [Remote]
    public static IRole GetRole(Guid roleGuid)
    {
      return Roles.GetAll(r => r.Sid == roleGuid).FirstOrDefault();
    }
    
    /// <summary>
    /// Проверить, что текущий пользователь входит в роль для отладки.
    /// </summary>
    /// <returns>True - если входит в роль, иначе false.</returns>
    public bool IsUserInTestRole()
    {
      var testRole = Roles.GetAll(r => Equals(r.Sid, Guid.Parse("0223670e-bfd2-4747-b7c0-851c8837a21d"))).FirstOrDefault();
      if (testRole != null)
        if (Users.Current.IncludedIn(testRole))
          return true;
      return false;
    }
    #endregion
    
    #region Функции для подписания.
    
    /// <summary>
    /// Получить утверждающую подпись версии документа для формирования штампа.
    /// </summary>
    /// <param name="versionId">ИД версии документа.</param>
    /// <returns>Утверждающая подпись версии документа.</returns>
    [Public]
    public virtual Sungero.Domain.Shared.ISignature GetApprovalSignatureForStamp(Sungero.Docflow.IOfficialDocument document, int versionId)
    {
      var version = document.Versions.FirstOrDefault(x => x.Id == versionId);
      if (version == null)
        return null;
      
      // Только утверждающие подписи.
      var versionSignatures = Signatures.Get(version)
        .Where(s => s.SignatureType == SignatureType.Approval)
        .ToList();
      if (!versionSignatures.Any())
        return null;
      
      // В приоритете подпись сотрудника из поля "Подписал". Квалифицированная ЭП приоритетнее простой.
      return versionSignatures
        .OrderByDescending(s => Equals(s.Signatory, document.OurSignatory))
        .ThenBy(s => s.SignCertificate == null)
        .ThenByDescending(s => s.SigningDate)
        .FirstOrDefault();
    }
    
    /// <summary>
    /// Проверить есть ли сертификаты для подписания документов.
    /// </summary>
    /// <param name="documents">Список документов.</param>
    /// <returns>True - если есть сертификаты для каждого документа, false - если для какого-то нет.</returns>
    [Remote]
    public bool HasCertificates(List<IOfficialDocument> documents)
    {
      foreach (var document in documents)
        if (!Sungero.Docflow.PublicFunctions.Module.Remote.GetCertificates(document).Any())
          return false;
      return true;
    }
    
    /// <summary>
    /// Получить все данные для валидации подписания одним запросом.
    /// </summary>
    /// <param name="documents">Список документов.</param>
    /// <returns>Список структур с данными валидации.</returns>
    [Remote(IsPure = true)]
    public List<Structures.Module.BeforeSign> ValidateBeforeSign(List<IOfficialDocument> documents)
    {
      var beforeSign = new List<Structures.Module.BeforeSign>();
      var currentEmployee = Sungero.Company.Employees.Current;
      foreach (var document in documents)
      {
        var signatories = new List<Sungero.Docflow.Structures.SignatureSetting.Signatory>();
        var errors = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetApprovalValidationErrors(document, false);
        signatories = Functions.Module.GetSignatories(document);
        var canApprove = document.AccessRights.CanApprove() && signatories.Any(s => Equals(s.EmployeeId, currentEmployee.Id));
        var bodyChanged = Sungero.Docflow.PublicFunctions.ApprovalTask.Remote.DocumentHasBodyUpdateAfterLastView(document);
        beforeSign.Add(Structures.Module.BeforeSign.Create(errors, canApprove, bodyChanged));
      }
      return beforeSign;
    }
    #endregion
    
    #region Работа с заявлениями сотрудника.
    /// <summary>
    /// Создать настройку заявления.
    /// </summary>
    /// <param name="priority">Порядок заявлений.</param>
    /// <param name="docKind">Имя вида документа.</param>
    /// <param name="templateName">Имя шаблона для ЛК.</param>
    /// <param name="templateRXName">Имя шаблона для RX.</param>
    /// <returns>Ошибки при поиске шаблонов.</returns>
    public string CreateDefaultStatementsSetting(int priority, string docKind, string templateName, string templateRXName)
    {
      var errors = new List<string>();
      var result = string.Empty;
      
      var templates = DocumentTemplates.GetAll();
      var roles = Roles.GetAll();
      var setting = StatementsSettings.Create();
      var documentKind = DocumentKinds.GetAll(w => w.DocumentType.Name == Resources.StatementDocumentTypename && w.Name == docKind).FirstOrDefault();
      setting.DocumentKind = documentKind;
      
      var template = templates.Where(w => w.Name == templateName).FirstOrDefault();
      if (template != null)
        setting.Template = template;
      else
        errors.Add(Resources.TemplateNotFoundError);
      
      var templateRX = templates.Where(w => w.Name == templateRXName).FirstOrDefault();
      if (templateRX != null)
        setting.TemplateRX = templateRX;
      else
        errors.Add(Resources.TemplateForRXNotFoundError);
      
      setting.Priority = priority;
      setting.Approvers.AddNew().Approver = roles.Where(w => w.Sid == Sungero.Docflow.Constants.Module.RoleGuid.DepartmentManagersRole).FirstOrDefault();
      setting.ApproveStartOrder = HRLite.StatementsSetting.ApproveStartOrder.Consistently;
      setting.Performers.AddNew().Performer = roles.Where(w => w.Sid == Constants.Module.Roles.HRSpecialist).FirstOrDefault();
      setting.PerformStartOrder = HRLite.StatementsSetting.PerformStartOrder.Consistently;
      setting.Status = HRLite.StatementsSetting.Status.Closed;
      setting.Save();
      
      if (errors.Any())
        result = string.Format("ИД {0}. {1}", setting.Id.ToString(), string.Join(" ", errors));
      
      return result;
    }
    
    /// <summary>
    /// Создать стандартные настройки заявлений.
    /// </summary>
    /// <returns>Список ошибок при поиске шаблонов.</returns>
    [Public, Remote]
    public List<string> CreateDefaultStatementsSettings()
    {
      var creationResults = new List<string>();
      // Прочее заявление.
      creationResults.Add(CreateDefaultStatementsSetting(1, Resources.OtherStatement,
                                                         SettingsConstants.OtherStatementRXKind.TemplateName,
                                                         SettingsConstants.OtherStatementRXKind.RXTemplateName));
      
      // Заявление на изменение персональных данных.
      creationResults.Add(CreateDefaultStatementsSetting(2, Resources.ChangePersonalDataStatement,
                                                         SettingsConstants.ChangePersonalDataStatementKind.TemplateName,
                                                         SettingsConstants.ChangePersonalDataStatementKind.RXTemplateName));
      
      // Заявление о переводе.
      creationResults.Add(CreateDefaultStatementsSetting(3, Resources.TransferStatement,
                                                         SettingsConstants.TransferStatementKind.TemplateName,
                                                         SettingsConstants.TransferStatementKind.RXTemplateName));
      
      // Заявление о переносе отпуска.
      creationResults.Add(CreateDefaultStatementsSetting(4, Resources.VacationChangePeriodStatement,
                                                         SettingsConstants.VacationChangePeriodStatementKind.TemplateName,
                                                         SettingsConstants.VacationChangePeriodStatementKind.RXTemplateName));
      
      // Заявление о предоставлении дополнительного дня отдыха в связи со сдачей крови.
      creationResults.Add(CreateDefaultStatementsSetting(5, Resources.DonorAdditionalRestDayStatement,
                                                         SettingsConstants.DonorAdditionalRestDayStatementKind.TemplateName,
                                                         SettingsConstants.DonorAdditionalRestDayStatementKind.RXTemplateName));
      
      // Заявление о предоставлении ежегодного оплачиваемого отпуска.
      creationResults.Add(CreateDefaultStatementsSetting(6, Resources.AnnualPaidVacationStatement,
                                                         SettingsConstants.AnnualPaidVacationStatementKind.TemplateName,
                                                         SettingsConstants.AnnualPaidVacationStatementKind.RXTemplateName));
      
      // Заявление о предоставлении отпуска без сохранения заработной платы.
      creationResults.Add(CreateDefaultStatementsSetting(7, Resources.VacationWithoutPayStatement,
                                                         SettingsConstants.VacationWithoutPayStatementKind.TemplateName,
                                                         SettingsConstants.VacationWithoutPayStatementKind.RXTemplateName));
      
      // Заявление о предоставлении отпуска по беременности и родам.
      creationResults.Add(CreateDefaultStatementsSetting(8, Resources.MaternityLeaveStatement,
                                                         SettingsConstants.MaternityLeaveStatementKind.TemplateName,
                                                         SettingsConstants.MaternityLeaveStatementKind.RXTemplateName));
      
      // Заявление о предоставлении учебного отпуска.
      creationResults.Add(CreateDefaultStatementsSetting(9, Resources.StudyLeaveStatement,
                                                         SettingsConstants.StudyLeaveStatementKind.TemplateName,
                                                         SettingsConstants.StudyLeaveStatementKind.RXTemplateName));
      
      // Заявление об оказании материальной помощи.
      creationResults.Add(CreateDefaultStatementsSetting(10, Resources.FinancialAssistanceStatement,
                                                         SettingsConstants.FinancialAssistanceStatementKind.TemplateName,
                                                         SettingsConstants.FinancialAssistanceStatementKind.RXTemplateName));
      
      // Заявление об увольнении.
      creationResults.Add(CreateDefaultStatementsSetting(11, Resources.DismissalStatement,
                                                         SettingsConstants.DismissalStatementKind.TemplateName,
                                                         SettingsConstants.DismissalStatementKind.RXTemplateName));
      
      return creationResults;
    }

    /// <summary>
    /// Получить все настройки заявлений.
    /// </summary>
    [Public, Remote]
    public List<IStatementsSetting> GetAllStatementsSettings()
    {
      return StatementsSettings.GetAll().ToList();
    }
    
    /// <summary>
    /// Получить все заявления текущего пользователя.
    /// </summary>
    /// <param name="author">Текущий пользователь.</param>
    /// <returns></returns>
    [Public, Remote]
    public List<HRLite.IStatementTask> GetEmployeeStatements(IUser author)
    {
      return StatementTasks.GetAll().Where(s => s.Author == Users.Current).ToList();
    }
    
    /// <summary>
    /// Создать заявление сотрудника.
    /// </summary>
    /// <param name="docKind">Вид документа.</param>
    /// <returns></returns>
    [Public, Remote]
    public virtual IStatementDocument CreateStatement(IDocumentKind docKind, IUser currentUser)
    {
      var setting = Functions.Module.GetStatementSetting(docKind);
      var statement = StatementDocuments.CreateFrom(setting.TemplateRX);
      statement.DocumentKind = docKind;
      statement.Author = currentUser;
      statement.Save();
      return statement;
    }
    
    /// <summary>
    /// Создать задачу для отправки заявления.
    /// </summary>
    /// <param name="statement">Заявление сотрудника.</param>
    /// <returns></returns>
    [Public, Remote]
    public virtual IStatementTask CreateStatementTask(IStatementDocument statement)
    {
      var setting = Functions.Module.GetStatementSetting(statement.DocumentKind);
      var task = StatementTasks.Create();
      task.DocumentGroup.StatementDocuments.Add(statement);
      foreach (var approver in setting.Approvers)
      {
        task.Approvers.AddNew().Approver = approver.Approver;
      }
      foreach (var performer in setting.Performers)
      {
        task.Performers.AddNew().Performer = performer.Performer;
      }
      task.Save();
      return task;
    }
    
    /// <summary>
    /// Получить настройку заявления по виду документа.
    /// </summary>
    /// <param name="documentKind">Вид документа.</param>
    /// <returns>Настройка заявления.</returns>
    public static IStatementsSetting GetStatementSetting(IDocumentKind documentKind)
    {
      var statementsSetting = StatementsSettings.GetAll(s => s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active && Equals(s.DocumentKind, documentKind));
      if (statementsSetting.Any())
        return statementsSetting.FirstOrDefault();
      else
        return null;
    }
    
    /// <summary>
    /// Получить настройку для заявления.
    /// </summary>
    /// <param name="statement">Заявление.</param>
    /// <returns></returns>
    [Public, Remote]
    public virtual IStatementsSetting GetStatementSetting(IStatementDocument statement)
    {
      return HRLite.StatementsSettings.GetAll().Where(s => s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active && s.DocumentKind == statement.DocumentKind).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить исполнителей по заявлению с учетом организации заявителя.
    /// </summary>
    /// <param name="documentKind">Вид документа.</param>
    /// <param name="author">Заявитель.</param>
    /// <returns>Несистемные действующие сотрудники-исполнители.</returns>
    public static List<Sungero.Company.IEmployee> GetStatementPerformers(IDocumentKind documentKind, IUser author)
    {
      var statementPerformers = new List<Sungero.Company.IEmployee>();
      var statementSetting = GetStatementSetting(documentKind);
      var departmentManagersRole = Roles.GetAll(g => g.Sid == Sungero.Docflow.PublicConstants.Module.RoleGuid.DepartmentManagersRole).FirstOrDefault();
      var businessUnit = Employees.As(author).Department.BusinessUnit;
      if (statementSetting != null)
      {
        foreach (var statementPerformer in statementSetting.Performers.OrderBy(p => p.Number))
        {
          var performer = statementPerformer.Performer;
          if (Equals(performer, departmentManagersRole))
          {
            var departmentManager = HRManagement.PublicFunctions.Module.Remote.GetManager(Employees.As(author));
            if (departmentManager != null)
              statementPerformers.Add(departmentManager);
          }
          else
          {
            if (Employees.Is(performer))
              statementPerformers.Add(Employees.As(performer));
            if (Departments.Is(performer))
            {
              var departmentPerformers = Employees.GetAll(e => Equals(e.Department, Departments.As(performer))).Where(e => e.IsSystem != true && e.Status == Sungero.CoreEntities.DatabookEntry.Status.Active).Distinct().ToList();
              if (Equals(statementPerformer.Filter, HRLite.StatementsSettingPerformers.Filter.NeedFiltration))
                departmentPerformers = departmentPerformers.Where(e => Equals(e.Department.BusinessUnit, businessUnit)).ToList();
              statementPerformers.AddRange(departmentPerformers);
            }
            else if (BusinessUnits.Is(performer))
            {
              var businessUnitPerformers = Employees.GetAll(e => Equals(e.Department.BusinessUnit, BusinessUnits.As(performer))).Where(e => e.IsSystem != true && e.Status == Sungero.CoreEntities.DatabookEntry.Status.Active).Distinct().ToList();
              if (Equals(statementPerformer.Filter, HRLite.StatementsSettingPerformers.Filter.NeedFiltration))
                businessUnitPerformers = businessUnitPerformers.Where(e => Equals(e.Department.BusinessUnit, businessUnit)).ToList();
              statementPerformers.AddRange(businessUnitPerformers);
            }
            else if (Groups.Is(performer))
            {
              var groupRecipient = Groups.GetAllUsersInGroup(Groups.As(performer)).Where(r => Employees.Is(r)).Distinct();
              foreach (var user in groupRecipient)
              {
                var employee = Employees.As(user);
                if (Equals(statementPerformer.Filter, HRLite.StatementsSettingPerformers.Filter.NeedFiltration))
                {
                  if (Equals(employee.Department.BusinessUnit, businessUnit))
                    statementPerformers.Add(employee);
                }
                else
                  statementPerformers.Add(employee);
              }
            }
          }
        }
        return statementPerformers;
      }
      else
        return null;
    }
    
    /// <summary>
    /// Получить согласующих по заявлению с учетом организации заявителя.
    /// </summary>
    /// <param name="documentKind">Вид документа.</param>
    /// <param name="author">Заявитель.</param>
    /// <returns>Несистемные действующие сотрудники-согласующие.</returns>
    public static List<Sungero.Company.IEmployee> GetStatementApprovers(IDocumentKind documentKind, IUser author)
    {
      var statementApprovers = new List<Sungero.Company.IEmployee>();
      var statementSetting = GetStatementSetting(documentKind);
      var departmentManagersRole = Roles.GetAll(g => g.Sid == Sungero.Docflow.PublicConstants.Module.RoleGuid.DepartmentManagersRole).FirstOrDefault();
      var businessUnit = Employees.As(author).Department.BusinessUnit;
      if (statementSetting != null)
      {
        foreach (var statementApprover in statementSetting.Approvers.OrderBy(p => p.Number))
        {
          var approver = statementApprover.Approver;
          if (Equals(approver, departmentManagersRole))
          {
            var departmentManager = HRManagement.PublicFunctions.Module.Remote.GetManager(Employees.As(author));
            if (departmentManager != null)
              statementApprovers.Add(departmentManager);
          }
          else
          {
            if (Employees.Is(approver))
              statementApprovers.Add(Employees.As(approver));
            if (Departments.Is(approver))
            {
              var departmentApprovers = Employees.GetAll(e => Equals(e.Department, Departments.As(approver))).Where(e => e.IsSystem != true && e.Status == Sungero.CoreEntities.DatabookEntry.Status.Active).Distinct().ToList();
              if (Equals(statementApprover.Filter, HRLite.StatementsSettingApprovers.Filter.NeedFiltration))
                departmentApprovers = departmentApprovers.Where(e => Equals(e.Department.BusinessUnit, businessUnit)).ToList();
              statementApprovers.AddRange(departmentApprovers);
            }
            else if (BusinessUnits.Is(approver))
            {
              var businessUnitApprovers = Employees.GetAll(e => Equals(e.Department.BusinessUnit, BusinessUnits.As(approver))).Where(e => e.IsSystem != true && e.Status == Sungero.CoreEntities.DatabookEntry.Status.Active).Distinct().ToList();
              if (Equals(statementApprover.Filter, HRLite.StatementsSettingApprovers.Filter.NeedFiltration))
                businessUnitApprovers = businessUnitApprovers.Where(e => Equals(e.Department.BusinessUnit, businessUnit)).ToList();
              statementApprovers.AddRange(businessUnitApprovers);
            }
            else if (Groups.Is(approver))
            {
              var groupRecipient = Groups.GetAllUsersInGroup(Groups.As(approver)).Where(r => Employees.Is(r)).Distinct();
              foreach (var user in groupRecipient)
              {
                var employee = Employees.As(user);
                if (Equals(statementApprover.Filter, HRLite.StatementsSettingApprovers.Filter.NeedFiltration))
                {
                  if (Equals(employee.Department.BusinessUnit, businessUnit))
                    statementApprovers.Add(employee);
                }
                else
                  statementApprovers.Add(employee);
              }
            }
          }
        }
        return statementApprovers;
      }
      else
        return null;
    }
    
    /// <summary>
    /// Занести файл pdf со штампом в качеcтве publiсbody конкретной версии документа.
    /// </summary>
    /// <param name="filePath">Путь до pdf файла.</param>
    /// <param name="version">Версия документа.</param>
    /// <param name="document">Документ.</param>
    /// <returns>Список ошибок.</returns>
    [Public, Remote]
    public virtual List<string> UploadPdfWithStampToDocumentVersion(string filePath, Sungero.Content.IElectronicDocumentVersions version, Sungero.Content.IElectronicDocument document)
    {
      var errorsList = new List<string>();
      try
      {
        MemoryStream ms = new MemoryStream();
        using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
          file.CopyTo(ms);
          version.PublicBody.Write(ms);
          version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");
          document.Save();
        }
        ms.Close();
        if (File.Exists(filePath))
          File.Delete(filePath);
      }
      catch (Exception ex)
      {
        errorsList.Add(ex.Message);
      }
      if (Locks.GetLockInfo(version.PublicBody).IsLocked)
        Locks.Unlock(version.PublicBody);
      
      return errorsList;
    }

    /// <summary>
    /// Проверить есть ли у текущего пользователя сертификаты в системе.
    /// </summary>
    [Public, Remote]
    public bool CheckCurrentUserCertificates(IEmployee employee)
    {
      return Sungero.Company.PublicFunctions.Module.Remote.GetCertificatesOfEmployee(employee).Where(c => c.Enabled == true).Any(c => !c.NotAfter.HasValue || c.NotAfter.Value > Calendar.Now);
    }

    /// <summary>
    /// Проверить необходимость добавления приложений в задачу по рассмотрению заявления.
    /// </summary>
    [Public, Remote]
    public virtual bool CheckSettingForStatementAttachments(IStatementsSetting setting)
    {
      var jsonString = setting.ConfigSettings;
      var needAttachments = false;
      JObject jsonObject = JObject.Parse(jsonString);
      try
      {
        needAttachments = (bool)jsonObject.SelectToken("$..parameters[?(@.type == 'FacilityParameterFile')].isRequired");
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("CheckSettingForStatementAttachments(). Error: {0}", ex.StackTrace);
      }
      return needAttachments;
    }

    /// <summary>
    /// Получить ФИО в указанном падеже.
    /// HACK копипаста Employee.GetFullNameInDeclension() т.к. та функция не публичная.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="declensionCase">Падеж.</param>
    /// <returns>ФИО в указанном падеже.</returns>
    public virtual string GetFullNameInDeclension(Sungero.Company.IEmployee employee, Sungero.Core.DeclensionCase declensionCase)
    {
      if (employee == null)
        return string.Empty;

      var fullName = CommonLibrary.PersonFullName.Create(employee.Person.LastName,
                                                         employee.Person.FirstName,
                                                         employee.Person.MiddleName);
      
      var gender = CommonLibrary.Gender.NotDefined;
      if (employee.Person.Sex != null)
        gender = employee.Person.Sex == Sungero.Parties.Person.Sex.Female ?
          CommonLibrary.Gender.Feminine :
          CommonLibrary.Gender.Masculine;
      
      // Для фамилий типа Ардо (Иванова) неправильно склоняется через API. Баг 32895.
      var fullNameInDeclension = CommonLibrary.Padeg.ConvertPersonFullNameToTargetDeclension(fullName,
                                                                                             (CommonLibrary.DeclensionCase)(int)declensionCase,
                                                                                             gender);
      
      var lastName = fullNameInDeclension.LastName;
      var firstName = fullNameInDeclension.FirstName;
      var middleName = string.IsNullOrWhiteSpace(employee.Person.MiddleName) ? string.Empty : fullNameInDeclension.MiddleName;
      
      if (!string.IsNullOrEmpty(middleName))
        return string.Join(" ", new string[] { lastName, firstName, middleName });
      else
        return string.Join(" ", new string[] { lastName, firstName });
    }
    
    /// <summary>
    /// Получить значение макропеременной для заявления.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="fieldName">Макропеременная.</param>
    /// <returns>Значение макропеременной.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetDocumentFieldValue(int employeeId, string fieldName)
    {
      var employee = Sungero.Company.Employees.Get(employeeId);
      var businessUnit = employee.Department.BusinessUnit;
      var manager = HRManagement.PublicFunctions.Module.Remote.GetManager(employee);
      switch (fieldName)
      {
        case "BUSINESS_UNIT_NAME":
          if (businessUnit != null)
            return businessUnit.Name;
          return string.Empty;
        case "CEO_JOB_TITLE_DATIVE":
          if (businessUnit != null)
            return Sungero.Company.PublicFunctions.Employee.GetJobTitle(businessUnit.CEO, DeclensionCase.Dative);
          return string.Empty;
        case "CEO_FULLNAME_DATIVE":
          if (businessUnit != null)
            return this.GetFullNameInDeclension(businessUnit.CEO, DeclensionCase.Dative);
          return string.Empty;
        case "CURRENT_USER_JOB_TITLE_GENITIVE":
          return Sungero.Company.PublicFunctions.Employee.GetJobTitle(employee, DeclensionCase.Genitive);
        case "MANAGER_JOB_TITLE_DATIVE":
          if (manager != null)
            return Sungero.Company.PublicFunctions.Employee.GetJobTitle(manager, DeclensionCase.Dative);
          return string.Empty;
        case "MANAGER_FULLNAME_DATIVE":
          if (manager != null)
            return this.GetFullNameInDeclension(manager, DeclensionCase.Dative);
          return string.Empty;
        default:
          return string.Empty;
      }
    }
    
    /// <summary>
    /// Сборка JSON для виджета "Заявления сотрудника".
    /// </summary>
    /// <param name="userId">Ид сотрудника.</param>
    /// <returns>Cтрока с json для услуг виджета "Заявления сотрудника".</returns>
    [Public, Remote]
    public virtual string GetJSONForStatementsWidget(int userId)
    {
      var employee = Sungero.Company.Employees.Get(userId);
      var businessUnitName = string.Empty;

      var employeeFullNameGenitive = string.Empty;
      var employeeJobTitleGenitive = string.Empty;
      var ceoJobTitleDative = string.Empty;
      var ceoFullNameDative = string.Empty;
      var managerFullNameDative = string.Empty;
      var managerJobTitleDative = string.Empty;

      // #CURRENT_USER_FULLNAME_GENETIVE
      employeeFullNameGenitive = this.GetFullNameInDeclension(employee, DeclensionCase.Genitive);

      // #CURRENT_USER_JOB_TITLE_GENETIVE
      employeeJobTitleGenitive = Sungero.Company.PublicFunctions.Employee.GetJobTitle(employee, DeclensionCase.Genitive);
      
      var businessUnit = employee.Department.BusinessUnit;
      if (businessUnit != null)
      {
        // #BUSINESS_UNIT_NAME
        businessUnitName = businessUnit.Name.Replace("\"", "\\\"");

        // #CEO_JOB_TITLE_DATIVE
        ceoJobTitleDative = Sungero.Company.PublicFunctions.Employee.GetJobTitle(businessUnit.CEO, DeclensionCase.Dative);
        
        // #CEO_FULLNAME_DATIVE
        ceoFullNameDative = this.GetFullNameInDeclension(businessUnit.CEO, DeclensionCase.Dative);
      }

      var manager = HRManagement.PublicFunctions.Module.Remote.GetManager(employee);
      // #MANAGER_JOB_TITLE_DATIVE
      managerJobTitleDative = Sungero.Company.PublicFunctions.Employee.GetJobTitle(manager, DeclensionCase.Dative);
      // #Manager_FULLNAME_DATIVE
      managerFullNameDative = this.GetFullNameInDeclension(manager, DeclensionCase.Dative);

      return "[" + string.Join(", ", StatementsSettings.GetAll(s => s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active).OrderBy(s => s.Priority).ThenBy(s => s.DocumentKind.Name)
                               .Select(s => s.ConfigSettings.Replace("#DOCUMENT_KIND_ID", s.DocumentKind.Id.ToString())
                                       .Replace("#TEMPLATE_ID", s.Template.Id.ToString())
                                       .Replace("#CURRENT_USER_FULLNAME_GENITIVE", employeeFullNameGenitive))
                               .ToList()) + "]";
    }
    
    /// <summary>
    /// Получить причину отказа в рассмотрении/исполнении заявления для картоки в ЛК.
    /// </summary>
    /// <param name="assignmentId">ИД уведомления.</param>
    /// <returns>Причина отказа.</returns>
    [Public, Remote]
    public string GetStatementDenyReason(int assignmentId)
    {
      var task = StatementNotices.GetAll(x => x.Id == assignmentId).FirstOrDefault().Task;
      var approveAssignments = StatementApproveAssignments.GetAll(x => Equals(x.Task, task) && x.BlockUid == Constants.StatementTask.StatementApproveAssignmentBlockId
                                                                  && x.Status == Sungero.Workflow.Assignment.Status.Completed && x.Result.HasValue
                                                                  && x.Result.Value == DirRX.HRLite.StatementApproveAssignment.Result.Refuse);
      if (approveAssignments.Any())
        return approveAssignments.OrderByDescending(x => x.Completed).FirstOrDefault().ActiveText;
      else
      {
        var performAssignments = StatementPerformAssignments.GetAll(x => Equals(x.Task, task) && x.BlockUid == Constants.StatementTask.StatementPerformAssignmentBlockId
                                                                    && x.Status == Sungero.Workflow.Assignment.Status.Completed && x.Result.HasValue
                                                                    && x.Result.Value == DirRX.HRLite.StatementPerformAssignment.Result.Refuse);
        if (performAssignments.Any())
          return performAssignments.OrderByDescending(x => x.Created).FirstOrDefault().ActiveText;
      }
      return string.Empty;
    }
    
    /// <summary>
    /// Получить ФИО отказавшего в рассмотрении/исполнении заявления для картоки в ЛК.
    /// </summary>
    /// <param name="assignmentId">ИД уведомления.</param>
    /// <returns>ФИО отказавшего.</returns>
    [Public, Remote]
    public string GetStatementDenyPerson(int assignmentId)
    {
      var task = StatementNotices.GetAll(x => x.Id == assignmentId).FirstOrDefault().Task;
      var approveAssignments = StatementApproveAssignments.GetAll(x => Equals(x.Task, task) && x.BlockUid == Constants.StatementTask.StatementApproveAssignmentBlockId
                                                                  && x.Status == Sungero.Workflow.Assignment.Status.Completed && x.Result.HasValue
                                                                  && x.Result.Value == DirRX.HRLite.StatementApproveAssignment.Result.Refuse);
      if (approveAssignments.Any())
        return approveAssignments.OrderByDescending(x => x.Completed).FirstOrDefault().Performer.DisplayValue;
      else
      {
        var performAssignments = StatementPerformAssignments.GetAll(x => Equals(x.Task, task) && x.BlockUid == Constants.StatementTask.StatementPerformAssignmentBlockId
                                                                    && x.Status == Sungero.Workflow.Assignment.Status.Completed && x.Result.HasValue
                                                                    && x.Result.Value == DirRX.HRLite.StatementPerformAssignment.Result.Refuse);
        if (performAssignments.Any())
          return performAssignments.OrderByDescending(x => x.Created).FirstOrDefault().Performer.DisplayValue;
      }
      return string.Empty;
    }
    
    /// <summary>
    /// Получить текст задачи на информирование сотрудников.
    /// </summary>
    /// <param name="assignmentId">ИД уведолмения.</param>
    /// <returns>Текст задачи.</returns>
    [Public, Remote]
    public string GetInformingNoticeActiveText(int assignmentId)
    {
      return DirRX.HRLite.InformingNotices.GetAll(x => x.Id == assignmentId).FirstOrDefault().Task.ActiveText;
    }
    
    #endregion
    
    #region Конвертация в Pdf, Pdf/a, проставление штампов подписи, склейка.
    
    /// <summary>
    /// Конвертировать тело документа в pdf/a.
    /// </summary>
    /// <param name="document">Документ для конвертации.</param>
    /// <returns>Результат конвертации.</returns>
    [Public]
    public virtual Structures.Module.IСonversionToPdfAResult ConvertToPdfa(Sungero.Docflow.IInternalDocumentBase document)
    {
      var info = Structures.Module.СonversionToPdfAResult.Create();
      info.IsConverted = false;
      
      // Проверить наличие версий.
      if (!document.HasVersions)
      {
        info.HasErrors = true;
        info.ErrorMessage = Sungero.Docflow.OfficialDocuments.Resources.NoVersionError;
        return info;
      }
      
      var version = document.LastVersion;
      var pdfaDocumentStream = new System.IO.MemoryStream();
      using (var inputStream = new System.IO.MemoryStream())
      {
        version.Body.Read().CopyTo(inputStream);
        inputStream.Seek((Int64)0, SeekOrigin.Begin);
        try
        {
          // Не конвертировать документ, если он уже в pdf/a.
          if (!this.ValidatePdfAFormat(inputStream, version.AssociatedApplication.Extension))
          {
            // Заполнить параметры конвертации: расширение исходного файла и формат pdf/a.
            var paramsConvert = new PdfConverter.Models.ConvertParameters();
            paramsConvert.FileExtension = version.AssociatedApplication.Extension;
            paramsConvert.TargetFormat = PdfConverter.Models.PdfFormat.PDF_A_1A;
            var converter = new PdfConverter.Converter();
            var logger = new PdfConvertLogger.Wrapper(Sungero.Core.Logger.Error, Sungero.Core.Logger.Debug);
            converter.ConvertToPdfA(inputStream, pdfaDocumentStream, paramsConvert, logger);
            info.IsConverted = true;
          }
          else
            Logger.Debug(Resources.ValidatePDFAFormatFormat(document.Id));
        }
        catch (Exception e)
        {
          var errorMesssage = string.Empty;
          if (e is PdfConverter.Exceptions.UnexpectedConverterException)
            errorMesssage = Resources.UnexpectedConverterException;
          else if (e is PdfConverter.Exceptions.PdfFormatNotSupportedException || e is PdfConverter.Exceptions.DataTypeNotSupportedException)
            errorMesssage = Resources.FormatNotSupportedException;
          else if (e is PdfConverter.Exceptions.FontNotFoundException)
            errorMesssage = Resources.FontNotFoundException;
          else
            errorMesssage = e.Message;
          
          Logger.Debug(string.Format("{0} {1}", Resources.PdfAConvertErrorFormat(document.Id), e.Message));
          info.HasErrors = true;
          info.ErrorMessage = errorMesssage;
        }
      }
      
      // Записать сконвертированный документ в новую версию документа, если не было ошибок при конвертации.
      if (!info.HasErrors && info.IsConverted)
      {
        document.CreateVersionFrom(pdfaDocumentStream, "pdf");
        pdfaDocumentStream.Close();
        
        try
        {
          document.Save();
        }
        catch (Exception e)
        {
          Logger.Error(e.Message);
          info.HasErrors = true;
          info.ErrorMessage = e.Message;
        }
      }
      
      return info;
    }
    
    /// <summary>
    /// Проверить документ на соответствие формату PDF/A.
    /// </summary>
    /// <param name="inputStream">Поток.</param>
    /// <param name="extension">Расширение версии.</param>
    /// <returns>True - если соответствует формату, иначе false. </returns>
    [Public]
    public bool ValidatePdfAFormat(System.IO.Stream inputStream, string extension)
    {
      if (extension.ToLower() != "pdf")
        return false;
      var opt1A = new PdfFormatConversionOptions(PdfFormat.PDF_A_1A);
      var opt1B = new PdfFormatConversionOptions(PdfFormat.PDF_A_1B);
      var opt2A = new PdfFormatConversionOptions(PdfFormat.PDF_A_2A);
      var opt2B = new PdfFormatConversionOptions(PdfFormat.PDF_A_2B);
      var opt3A = new PdfFormatConversionOptions(PdfFormat.PDF_A_3A);
      var opt3B = new PdfFormatConversionOptions(PdfFormat.PDF_A_3B);
      var pdfDoc = new Aspose.Pdf.Document(inputStream);
      return pdfDoc.Validate(opt1A) || pdfDoc.Validate(opt1B) || pdfDoc.Validate(opt2A) || pdfDoc.Validate(opt2B) || pdfDoc.Validate(opt3A) || pdfDoc.Validate(opt3B);
    }
    
    /// <summary>
    /// Преобразовать AsposeWords документ в PDF, записать файл и вернуть имя файла.
    /// </summary>
    /// <param name="asposeDoc">Aspose Words Документ для преобразования.</param>
    /// <returns>Имя файла.</returns>
    public string ConvertAsposeWordsToPdfFile(Aspose.Words.Document asposeDoc)
    {
      System.IO.MemoryStream pdfDocumentStream = null;
      using (var inputStream = new System.IO.MemoryStream())
      {
        asposeDoc.Save(inputStream, Aspose.Words.SaveFormat.Docx);
        try
        {
          var pdfConverter = new Sungero.AsposeExtensions.Converter();
          pdfDocumentStream = pdfConverter.GeneratePdf(inputStream, "docx");
          var filePath = this.SaveByteArrayToTempFile(pdfDocumentStream.ToArray());
          pdfDocumentStream.Close();
          return filePath;
        }
        catch (Exception e)
        {
          if (e is Sungero.AsposeExtensions.PdfConvertException)
            Logger.Error(DirRX.HRLite.HRAcquaintanceTasks.Resources.PDFConvertError, e.InnerException);
          else
            Logger.Error(DirRX.HRLite.HRAcquaintanceTasks.Resources.PDFConvertError, e);
          pdfDocumentStream.Close();
          return null;
        }
      }
    }
    
    /// <summary>
    /// Склеить несколько файлов PDF в один.
    /// </summary>
    /// <param name="inFiles">Список полных путей до pdf-файлов для склеивания.</param>
    /// <returns>Склеенный файл.</returns>
    [Public]
    public string MergePdf(List<string> inFiles)
    {
      var outFile = string.Empty;
      var emptyFilename = false;
      if (inFiles.Count > 1)
      {
        try
        {
          PdfS.PdfDocument outputDocument = new PdfS.PdfDocument();
          foreach (string file in inFiles)
          {
            if (!string.IsNullOrEmpty(file))
            {
              PdfS.PdfDocument inputDocument = PdfS.IO.PdfReader.Open(file, PdfS.IO.PdfDocumentOpenMode.Import);
              int count = inputDocument.PageCount;
              for (int i = 0; i < count; i++)
              {
                PdfS.PdfPage page = inputDocument.Pages[i];
                outputDocument.AddPage(page);
              }
            }
            else
              emptyFilename = true;
          }
          outFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
          outputDocument.Save(outFile);
          foreach (string file in inFiles)
            if (File.Exists(file))
              File.Delete(file);
        }
        catch (Exception e)
        {
          Logger.Error("MergingPDF " + e.Message);
        }
        if (emptyFilename)
          Logger.DebugFormat("MergingPDF. У одного или нескольких файлов не указаны имена.");
      }
      else
      {
        File.Copy(inFiles[0], outFile, true);
      }
      return outFile;
    }
    
    /// <summary>
    /// Сконвертировать Pdf/a в Pdf, проставить штамп с отметкой об ЭП, записать в файл и вернуть имя файла.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="versionId">ИД версии документа.</param>
    /// <returns>Имя файла.</returns>
    [Public, Remote]
    public virtual string GetPdfaDocumentInLocalPdfFileWithStamp(Sungero.Docflow.IOfficialDocument document, int versionId)
    {
      var version = document.Versions.Where(v => v.Id == versionId).FirstOrDefault();
      var docVersHasSignature = Signatures.Get(version).Where(x => x.SignatureType == SignatureType.Approval).Any();
      
      var pdfDocumentStream = new System.IO.MemoryStream();
      using (var inputStream = new System.IO.MemoryStream())
      {
        version.Body.Read().CopyTo(inputStream);
        try
        {
          // Преобразовать в Pdf для простановки подписи.
          var pdfDocument = new Aspose.Pdf.Document(inputStream);
          pdfDocument.RemovePdfaCompliance();
          pdfDocument.Save(pdfDocumentStream);
          
          if (docVersHasSignature)
          {
            var pdfConverter = new Sungero.AsposeExtensions.Converter();
            var extension = version.BodyAssociatedApplication.Extension;
            var signatureMark = this.GetSignatureMarkAsHtml(document, versionId);
            pdfDocumentStream = (System.IO.MemoryStream)pdfConverter.AddSignatureMark(pdfDocumentStream, extension, signatureMark, Sungero.Docflow.Resources.SignatureMarkAnchorSymbol,
                                                                                      Sungero.Docflow.Constants.Module.SearchablePagesLimit);
          }
          
          var filePath = Functions.Module.SaveByteArrayToTempFile(pdfDocumentStream.ToArray());
          pdfDocumentStream.Close();
          return filePath;
        }
        catch (Exception e)
        {
          if (e is Sungero.AsposeExtensions.PdfConvertException)
            Logger.Error(Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), e.InnerException);
          else
            Logger.Error(string.Format("{0} {1}", Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), e.Message));
          pdfDocumentStream.Close();
          return null;
        }
      }
    }
    
    /// <summary>
    /// Получить отметку об ЭП.
    /// </summary>
    /// <param name="document">Документ для преобразования.</param>
    /// <param name="versionId">Id версии, для генерации.</param>
    /// <returns>Изображение отметки об ЭП в виде html.</returns>
    /// <remarks>Скопирован метод Docflow получения отметки об ЭП для подписи GetSignatureMarkAsHtml. Добавлена дата подписания.</remarks>
    public virtual string GetSignatureMarkAsHtml(Sungero.Docflow.IOfficialDocument document, int versionId)
    {
      var signature = HRLite.PublicFunctions.Module.GetApprovalSignatureForStamp(document, versionId);
      if (signature == null)
        throw new Exception(OfficialDocuments.Resources.LastVersionNotApproved);
      
      // В случае квалифицированной ЭП информацию для отметки брать из атрибутов субъекта сертификата.
      if (signature.SignCertificate != null)
        return this.GetSignatureMarkForCertificateAsHtml(signature);
      
      // В случае простой ЭП информацию для отметки брать из атрибутов подписи.
      return this.GetSignatureMarkForSimpleSignatureAsHtml(signature);
    }
    
    /// <summary>
    /// Получить отметку об ЭП для подписи.
    /// </summary>
    /// <param name="signature">Подпись.</param>
    /// <returns>Изображение отметки об ЭП для подписи в виде html.</returns>
    /// <remarks>Скопирован метод Docflow получения отметки об ЭП для подписи GetSignatureMarkForSimpleSignatureAsHtml. Добавлена дата подписания.</remarks>
    public virtual string GetSignatureMarkForSimpleSignatureAsHtml(Sungero.Domain.Shared.ISignature signature)
    {
      if (signature == null)
        return string.Empty;
      
      var signatoryFullName = signature.SignatoryFullName;
      var signatoryId = signature.Signatory.Id;
      
      string html;

      using (Sungero.Core.CultureInfoExtensions.SwitchTo(TenantInfo.Culture))
      {
        html = Resources.HtmlStampTemplateForSignature;
        html = html.Replace("{SignatoryFullName}", signatoryFullName);
        html = html.Replace("{SignatoryId}", signatoryId.ToString());
        var signingDate = signature.SigningDate.FromUtcTime().ToString("dd.MM.yyyy HH:mm");
        html = html.Replace("{SigningDate}", signingDate);
      }
      return html;
    }
    
    /// <summary>
    /// Получить отметку об ЭП для сертификата из подписи.
    /// </summary>
    /// <param name="signature">Подпись.</param>
    /// <returns>Изображение отметки об ЭП для сертификата в виде html.</returns>
    /// <remarks>Скопирован метод Docflow получения отметки об ЭП для подписи GetSignatureMarkForCertificateAsHtml. Добавлена дата подписания.</remarks>
    public virtual string GetSignatureMarkForCertificateAsHtml(Sungero.Domain.Shared.ISignature signature)
    {
      if (signature == null)
        return string.Empty;
      
      var certificate = signature.SignCertificate;
      if (certificate == null)
        return string.Empty;
      
      var certificateSubject = Sungero.Docflow.PublicFunctions.Module.GetCertificateSubject(signature);
      
      var signatoryFullName = string.Format("{0} {1}", certificateSubject.Surname, certificateSubject.GivenName).Trim();
      if (string.IsNullOrEmpty(signatoryFullName))
        signatoryFullName = certificateSubject.CounterpartyName;
      
      string html;
      string validity;
      using (Sungero.Core.CultureInfoExtensions.SwitchTo(TenantInfo.Culture))
      {
        html = Resources.HtmlStampTemplateForCertificate;
        html = html.Replace("{SignatoryFullName}", signatoryFullName);
        html = html.Replace("{Thumbprint}", certificate.Thumbprint.ToLower());
        var signingDate = signature.SigningDate.FromUtcTime().ToString("dd.MM.yyyy HH:mm");
        html = html.Replace("{SigningDate}", signingDate);
        validity = string.Format("{0} {1} {2} {3}",
                                 Sungero.Company.Resources.From,
                                 certificate.NotBefore.Value.ToShortDateString(),
                                 Sungero.Company.Resources.To,
                                 certificate.NotAfter.Value.ToShortDateString());
        html = html.Replace("{Validity}", validity);
      }
      return html;
    }
    
    /// <summary>
    /// Очистить текст от абзацов по совпадающему id документа. Применяется после валидации файла после преобразования в pdf.
    /// </summary>
    /// <param name="convertErrorMessage">Текст со списком ошибок.</param>
    /// <param name="docId">Id документа.</param>
    /// <returns>Текст без абзацов с заданным Id.</returns>
    [Public]
    public string ClearConverError(string convertErrorMessage, int docId)
    {
      var exactString = string.Format(" ({0}) ", docId);
      var docErrors = convertErrorMessage.Split('\n').Where(a => !a.Contains(exactString)).ToArray();
      var resultErrorMessage = string.Join("\n", docErrors);
      return resultErrorMessage;
    }
    #endregion
    
    #region Работа с листом ознакомления.
    
    /// <summary>
    /// Добавить лист ознакомления и штамп подписи в public body документа в формате PDF.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="versionId">ИД версии документа, в которую нужно записать содержимое.</param>
    /// <returns>True - если добавление прошло успешно, иначе false.</returns>
    public virtual bool AddAcquaintanceListAndSignatureMarkInPublicBody(Sungero.Docflow.IOfficialDocument document, int versionId)
    {
      var isUpdated = true;
      var version = document.Versions.Where(v => v.Id == versionId).FirstOrDefault();
      if (version.PublicBody.Size == 0 || Locks.TryLock(version.PublicBody))
      {
        // Сформировать лист ознакомления в PDF.
        var acquaintanceListFileName = this.GetAcquaintanceListLocalFile(document, versionId);
        // Сформировать тело документа с отметкой об ЭП в PDF.
        var documentWithStampFileName = this.GetPdfaDocumentInLocalPdfFileWithStamp(document, versionId);
        
        // Склеить файлы.
        var pdfFileList = new List<string>();
        pdfFileList.Add(documentWithStampFileName);
        pdfFileList.Add(acquaintanceListFileName);
        var filePath = Functions.Module.MergePdf(pdfFileList);
        
        try
        {
          MemoryStream ms = new MemoryStream();
          using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
          {
            file.CopyTo(ms);
            version.PublicBody.Write(ms);
            version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");
            document.Save();
          }
          ms.Close();
          if (File.Exists(filePath))
            File.Delete(filePath);
        }
        catch (Exception e)
        {
          Logger.Error(e.Message);
          isUpdated = false;
        }
        Locks.Unlock(version.PublicBody);
      }
      else
        isUpdated = false;
      return isUpdated;
    }
    
    /// <summary>
    /// Сформировать лист ознакомления с версией документа в локальном файле.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="versionId">ИД версии документа.</param>
    /// <returns>Имя файла.</returns>
    public virtual string GetAcquaintanceListLocalFile(Sungero.Docflow.IOfficialDocument document, int versionId)
    {
      var versionAcquaintanceInfo = this.GetVersionAcquaintanceInfo(document, versionId);
      var businessUnit = document.BusinessUnit;
      var businessUnitName = businessUnit != null ? businessUnit.Name : string.Empty;
      var asposeDoc = this.CreateAsposeAcquaintanceList(versionAcquaintanceInfo, document.Name, businessUnitName);
      return Functions.Module.ConvertAsposeWordsToPdfFile(asposeDoc);
    }
    
    /// <summary>
    /// Получить статусы ознакомления сотрудников с версией документа.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="versionId">ИД версии документа.</param>
    /// <returns>Список данных об ознакомлении в структурах AcquaintanceAssigmentStatus.</returns>
    public virtual List<DirRX.HRLite.Structures.Module.AcquaintanceAssigmentStatus> GetVersionAcquaintanceInfo(Sungero.Docflow.IOfficialDocument document, int versionId)
    {
      var acquaintanceAssigmentStatusList = new List<DirRX.HRLite.Structures.Module.AcquaintanceAssigmentStatus>();
      var acquaintanceAssignments = HRAcquaintanceAssignments.GetAll(a => a.Result.HasValue &&
                                                                   DirRX.HRLite.HRAcquaintanceTasks.As(a.Task).DocumentsVersionIDs.Select(vt => vt.VersionID).Where(v => v == versionId).Any()).Select(a => Sungero.Workflow.Assignments.As(a));
      // Пройти по всем озникомившимся и взять их последний результат ознакомления.
      foreach (var performer in acquaintanceAssignments.Select(p => p.Performer).Distinct())
      {
        var performerAcquaintanceAssignment = acquaintanceAssignments.Where(a => Equals(a.Performer, performer)).OrderByDescending(a => a.Modified).FirstOrDefault();
        var performerInfo = DirRX.HRLite.Structures.Module.AcquaintanceAssigmentStatus.Create();
        var version = document.Versions.Where(v => v.Id == versionId).FirstOrDefault();
        var signature = Signatures.Get(version).Where(x => x.SignatureType == SignatureType.Endorsing && x.SignatoryFullName == performerAcquaintanceAssignment.Performer.Name).FirstOrDefault();
        if (signature != null && performerAcquaintanceAssignment.Result.Value == DirRX.HRLite.HRAcquaintanceAssignment.Result.Sign)
          performerInfo.SignDate = Sungero.Docflow.PublicFunctions.Module.ToTenantTime(signature.SigningDate).ToUserTime();
        else
          performerInfo.SignDate = Sungero.Docflow.PublicFunctions.Module.ToTenantTime(performerAcquaintanceAssignment.Modified.Value).ToUserTime();
        performerInfo.PerformerName = performerAcquaintanceAssignment.Performer.Name;
        performerInfo.Signed = performerAcquaintanceAssignment.Result.Value == DirRX.HRLite.HRAcquaintanceAssignment.Result.Sign ? true : false;
        performerInfo.Commentary = performerAcquaintanceAssignment.Result.Value == DirRX.HRLite.HRAcquaintanceAssignment.Result.Sign ? string.Empty : performerAcquaintanceAssignment.ActiveText;
        var employee = Sungero.Company.Employees.As(performer);
        performerInfo.DepartmentName = employee.Department.Name;
        performerInfo.JobTitle = employee.JobTitle == null ? string.Empty : employee.JobTitle.Name;
        acquaintanceAssigmentStatusList.Add(performerInfo);
      }
      return acquaintanceAssigmentStatusList;
    }
    
    /// <summary>
    /// Создать лист ознакомления в Aspose документе.
    /// </summary>
    /// <param name="assigmentInfo">Информация об ознакомлении сотрудников с документом.</param>
    /// <param name="documentName">Имя документа.</param>
    /// <returns>Aspose документ с таблицей ознакомления.</returns>
    public virtual Aspose.Words.Document CreateAsposeAcquaintanceList(List<DirRX.HRLite.Structures.Module.AcquaintanceAssigmentStatus> assigmentInfo, string documentName, string businessUnitName)
    {
      var asposeDoc = new Aspose.Words.Document();
      
      var builder = new DocumentBuilder(asposeDoc);
      builder.Font.Name = "Arial";
      
      builder.PageSetup.Orientation = Orientation.Landscape;
      
      builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
      
      builder.Font.Size = 16;
      builder.Font.Bold = true;
      builder.Writeln(DirRX.HRLite.HRAcquaintanceTasks.Resources.AcquaintanceListFormat(businessUnitName));
      if (!string.IsNullOrEmpty(businessUnitName))
        builder.Writeln(businessUnitName);

      builder.Writeln(string.Empty);
      builder.Font.Size = 14;
      builder.Font.Bold = false;
      builder.Writeln(documentName);
      builder.Writeln(string.Empty);
      
      builder.StartTable();
      var currentNumber = 1;
      var currentDepartment = string.Empty;
      
      foreach (var perf in assigmentInfo.OrderBy(t => t.DepartmentName).ThenBy(t => t.PerformerName))
      {
        if (currentDepartment != perf.DepartmentName)
        {
          builder.EndTable();
          currentDepartment = perf.DepartmentName;
          builder = this.StartNewTable(builder, currentDepartment);
          currentNumber = 1;
        }
        
        this.InsertNewEmployeeInList(builder, perf, currentNumber);
        currentNumber++;
      }
      builder.EndTable();
      
      return asposeDoc;
    }
    
    /// <summary>
    /// Начать новую таблицу по подразделению.
    /// </summary>
    /// <param name="builder">Aspose билдер, в который добавится таблица.</param>
    /// <param name="department">Имя подразделения.</param>
    /// <returns>Aspose билдер.</returns>
    public virtual Aspose.Words.DocumentBuilder StartNewTable(Aspose.Words.DocumentBuilder builder, string department)
    {
      builder.Writeln(string.Empty);
      builder.Font.Bold = false;
      builder.Font.Size = 12;
      builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
      
      var tempSpacing = builder.ParagraphFormat.LineSpacing;
      
      builder.ParagraphFormat.LineSpacing = 15;
      builder.Writeln(department);
      builder.ParagraphFormat.LineSpacing = tempSpacing;
      builder.Font.Bold = true;
      
      builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
      builder.StartTable();
      builder = this.InsertNewCell(builder, DirRX.HRLite.HRAcquaintanceTasks.Resources.AcquaintanceListIndexNumber, DirRX.HRLite.Constants.HRAcquaintanceTask.NumberWidth, 10, ParagraphAlignment.Center);
      builder = this.InsertNewCell(builder, DirRX.HRLite.HRAcquaintanceTasks.Resources.AcquaintanceListEmployeeName, DirRX.HRLite.Constants.HRAcquaintanceTask.NameWidth, 10, ParagraphAlignment.Center);
      builder = this.InsertNewCell(builder, DirRX.HRLite.HRAcquaintanceTasks.Resources.AcquaintanceListJobTitle, DirRX.HRLite.Constants.HRAcquaintanceTask.JobTitleWidth, 10, ParagraphAlignment.Center);
      builder = this.InsertNewCell(builder, DirRX.HRLite.HRAcquaintanceTasks.Resources.AcquaintanceListStatus, DirRX.HRLite.Constants.HRAcquaintanceTask.StatusWidth, 10, ParagraphAlignment.Center);
      builder = this.InsertNewCell(builder, DirRX.HRLite.HRAcquaintanceTasks.Resources.AcquaintanceListDate, DirRX.HRLite.Constants.HRAcquaintanceTask.DateWidth, 10, ParagraphAlignment.Center);
      builder = this.InsertNewCell(builder, DirRX.HRLite.HRAcquaintanceTasks.Resources.AcquaintanceListCommentary, DirRX.HRLite.Constants.HRAcquaintanceTask.CommentaryWidth, 10, ParagraphAlignment.Center);
      builder.Font.Bold = false;
      
      return builder;
    }
    
    /// <summary>
    /// Вставить новую ячейку с содержимым в таблицу.
    /// </summary>
    /// <param name="builder">AsposeBuilder с начатой таблицей.</param>
    /// <param name="content">Содержимое ячейки.</param>
    /// <param name="width">Ширина ячейки.</param>
    /// <param name="fontSize">Размер шрифта.</param>
    /// <param name="aligment">Центровка содержимого.</param>
    /// <returns>AsposeBuilder с добавленной ячейкой в таблицу.</returns>
    public virtual Aspose.Words.DocumentBuilder InsertNewCell(Aspose.Words.DocumentBuilder builder, string content, double width, int fontSize, Aspose.Words.ParagraphAlignment aligment)
    {
      builder.Font.Size = fontSize;
      builder.InsertCell();
      builder.CellFormat.PreferredWidth = PreferredWidth.FromPoints(width);
      builder.CellFormat.ClearFormatting();
      builder.ParagraphFormat.Alignment = aligment;
      builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
      builder.Write(content);
      
      return builder;
    }
    
    /// <summary>
    /// Вставить нового сотрудника в билдер листа ознакомления.
    /// </summary>
    /// <param name="builder">Билдер.</param>
    /// <param name="assigmentInfo">Информация по ознакомлению сотрудника.</param>
    /// <param name="currentNumber">Порядковый номер в листе.</param>
    public virtual void InsertNewEmployeeInList(Aspose.Words.DocumentBuilder builder, DirRX.HRLite.Structures.Module.AcquaintanceAssigmentStatus assigmentInfo, int currentNumber)
    {
      builder.EndRow();
      builder = this.InsertNewCell(builder, currentNumber.ToString(), DirRX.HRLite.Constants.HRAcquaintanceTask.NumberWidth, 9, ParagraphAlignment.Center);
      builder = this.InsertNewCell(builder, assigmentInfo.PerformerName, DirRX.HRLite.Constants.HRAcquaintanceTask.NameWidth, 9, ParagraphAlignment.Left);
      builder = this.InsertNewCell(builder, assigmentInfo.JobTitle, DirRX.HRLite.Constants.HRAcquaintanceTask.JobTitleWidth, 9, ParagraphAlignment.Left);
      builder = this.InsertNewCell(builder, assigmentInfo.Signed ? DirRX.HRLite.HRAcquaintanceTasks.Resources.EmployeeGotAcquainted : DirRX.HRLite.HRAcquaintanceTasks.Resources.EmployeeRefusedToAcquaintance, DirRX.HRLite.Constants.HRAcquaintanceTask.StatusWidth, 9, ParagraphAlignment.Center);
      builder = this.InsertNewCell(builder, assigmentInfo.SignDate.ToString("dd.MM.yyyy HH:mm"), DirRX.HRLite.Constants.HRAcquaintanceTask.DateWidth, 9, ParagraphAlignment.Center);
      builder = this.InsertNewCell(builder, assigmentInfo.Commentary, DirRX.HRLite.Constants.HRAcquaintanceTask.CommentaryWidth, 9, ParagraphAlignment.Center);
    }
    
    #endregion
    
    #region Работа с отчетом Корректность настройки модуля.
    /// <summary>
    /// Получить список некорректно настроенных пар НОР-Вид документа шаблонов заявлений.
    /// </summary>
    /// <returns>Список некорректно настроенных пар НОР-Вид документа шаблонов заявлений.</returns>
    public virtual List<Structures.Module.StatementTemplatesError> GetStatementTemplatesError()
    {
      var statementTemplatesErrorList = new List<Structures.Module.StatementTemplatesError>();
      foreach (var businessUnit in BusinessUnits.GetAll(b => b.Status == Sungero.Company.BusinessUnit.Status.Active))
      {
        foreach (var documentKind in DocumentKinds.GetAll().ToList().Where(k => Functions.Module.IsStatement(k.DocumentType)))
        {
          // Найти шаблоны, в которых явно указан НОР либо НОР не указан (тогда считаем, что шаблон создан для всех НОР), а также вид заявления из рассматриваемой пары.
          var templates = DocumentTemplates.GetAll(t => (t.BusinessUnits.Where(b => Equals(b.BusinessUnit, businessUnit)).Any() || !t.BusinessUnits.Any()) &&
                                                   t.DocumentKinds.Where(k => Equals(k.DocumentKind, documentKind)).Any());
          if (templates.Count() != 1)
          {
            var statementTemplatesError = Structures.Module.StatementTemplatesError.Create();
            statementTemplatesError.BusinessUnit = businessUnit;
            statementTemplatesError.DocumentKind = documentKind;
            statementTemplatesError.TemplateList = templates.Any() ? templates.ToList() : null;
            statementTemplatesErrorList.Add(statementTemplatesError);
          }
        }
      }
      return statementTemplatesErrorList;
    }
    #endregion
    
    #region Импорт оргструктуры из Excel.
    
    /// <summary>
    /// Импортировать Наши организации, Подразделения и Сотрудников из Excel.
    /// </summary>
    /// <param name="importFilePath">Путь до документа с данными для импорта.</param>
    /// <param name="docId">ИД документа.</param>
    /// <returns>Результаты импорта.</returns>
    [Public]
    public virtual string ImportOrganizationalStructure(string importFilePath, int docId)
    {
      var workbook = new Workbook(importFilePath);

      var importBusinessUnitsResult = Structures.Module.ImportWorksheetResult.Create();
      var businessUnitsWorksheet = this.FindWorksheetByName(workbook, Constants.Module.ExcelImportWorksheetNames.BusinessUnits);
      if (businessUnitsWorksheet == null)
        Logger.Debug("Worksheet for Business units not found.");
      else
        importBusinessUnitsResult = this.ImportBusinessUnits(businessUnitsWorksheet);
      
      var importDepartmentsResult = Structures.Module.ImportWorksheetResult.Create();
      var departmentsWorksheet = this.FindWorksheetByName(workbook, Constants.Module.ExcelImportWorksheetNames.Departments);
      if (departmentsWorksheet == null)
        Logger.Debug("Worksheet for Departments not found.");
      else
        importDepartmentsResult = this.ImportDepartments(departmentsWorksheet);
      
      var importEmployeesResult = Structures.Module.ImportWorksheetResult.Create();
      var employeesWorksheet = this.FindWorksheetByName(workbook, Constants.Module.ExcelImportWorksheetNames.Employees);
      if (employeesWorksheet == null)
        Logger.Debug("Worksheet for Employees not found.");
      else
        importEmployeesResult = this.ImportEmployees(employeesWorksheet);
      
      var businessUnitsRefillData = importBusinessUnitsResult.RefillData;
      var refillBusinessUnitsResult = Structures.Module.ImportWorksheetResult.Create();
      if (businessUnitsRefillData != null && businessUnitsRefillData.Count > 0)
        refillBusinessUnitsResult = this.RefillBusinessUnits(businessUnitsRefillData);
      
      var departmentsRefillData = importDepartmentsResult.RefillData;
      var refillDepartmentsResult = Structures.Module.ImportWorksheetResult.Create();
      if (departmentsRefillData != null && departmentsRefillData.Count > 0)
        refillDepartmentsResult = this.RefillDepartments(departmentsRefillData);
      
      workbook.Save(importFilePath);
      workbook.Dispose();
      this.ReplaceDocVersion(importFilePath, docId);

      return Resources.ImportFinishedFormat(importBusinessUnitsResult.ImportedCount, importBusinessUnitsResult.TotalCount,
                                            importDepartmentsResult.ImportedCount, importDepartmentsResult.TotalCount,
                                            importEmployeesResult.ImportedCount, importEmployeesResult.TotalCount,
                                            importDepartmentsResult.EmptyBusinessUnitCreatedCount + refillBusinessUnitsResult.EmptyBusinessUnitCreatedCount,
                                            importEmployeesResult.EmptyDepartmentCreatedCount + refillDepartmentsResult.EmptyDepartmentCreatedCount,
                                            importEmployeesResult.PersonCreatedCount, importEmployeesResult.JobTitleCreatedCount);
    }
    
    /// <summary>
    /// Заменить версию документа из файла.
    /// </summary>
    /// <param name="filepath">Путь до файла.</param>
    /// <param name="docId">ИД документа.</param>
    public virtual void ReplaceDocVersion(string filepath, int docId)
    {
      Sungero.Content.ElectronicDocuments.Get(docId).Import(filepath);
    }
    
    /// <summary>
    /// Отправить уведомление с приложением.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="text">Текст уведомления.</param>
    /// <param name="attachmentId">ИД прикрепляемого документа.</param>
    [Public]
    public virtual void SendNoticeWithAttachment(int userId, string text, int attachmentId)
    {
      var attachments = new[] { SimpleDocuments.Get(attachmentId) };
      var recipients = new[] { Recipients.As(Users.Get(userId)) };
      var task = Sungero.Workflow.SimpleTasks.CreateWithNotices(Resources.ImportOrgStructureNoticeSubject, recipients, attachments);
      task.ActiveText = string.Concat(Resources.ImportOrgNotice, "\r\n", text);
      task.Start();
    }
    
    /// <summary>
    /// Найти лист по имени в документе Excel.
    /// </summary>
    /// <param name="workbook">Документ Excel.</param>
    /// <param name="worksheetName">Наименование листа.</param>
    /// <returns>Лист Excel.</returns>
    public Aspose.Cells.Worksheet FindWorksheetByName(Aspose.Cells.Workbook workbook, string worksheetName)
    {
      return workbook.Worksheets.Where(w => w.Name == worksheetName).FirstOrDefault();
    }
    
    /// <summary>
    /// Импортировать Наши организации из Excel.
    /// </summary>
    /// <param name="worksheet">Лист с данными.</param>
    /// <returns>Результаты импорта.</returns>
    public Structures.Module.ImportWorksheetResult ImportBusinessUnits(Aspose.Cells.Worksheet worksheet)
    {
      Logger.Debug("=====Import Business units.=====");
      var importResults = Structures.Module.ImportWorksheetResult.Create();
      var businessUnitsCount = worksheet.Cells.Rows.Count - 1;
      importResults.TotalCount = businessUnitsCount;
      importResults.ImportedCount = 0;
      importResults.Errors = new List<string>();
      importResults.RefillData = new List<Structures.Module.ImportRefillData>();
      
      for (var row = 1; row <= businessUnitsCount; row++)
      {
        
        var importOneBUResult = this.ImportBusinessUnit(worksheet, row);
        var importOneBUError = importOneBUResult.Error;
        var importOneBUWarnings = importOneBUResult.Warnings;
        if (!string.IsNullOrEmpty(importOneBUError))
        {
          importResults.Errors.Add(importOneBUError);
          worksheet.Cells[row, 19].Value = "Not imported";
          worksheet.Cells[row, 21].Value = importOneBUError;
        }
        else if (!string.IsNullOrEmpty(importOneBUWarnings))
        {
          importResults.Errors.Add(importOneBUWarnings);
          worksheet.Cells[row, 19].Value = "Imported partially";
          worksheet.Cells[row, 21].Value = importOneBUWarnings;
          importResults.ImportedCount++;
          importResults.PartiallyImportedCount++;
        }
        else
        {
          importResults.ImportedCount++;
          if (importOneBUResult.RefillBusinessUnitName != null || importOneBUResult.RefillEmployeeName != null)
          {
            var refillData = Structures.Module.ImportRefillData.Create();
            refillData.Record = importOneBUResult.Record;
            refillData.BusinessUnitName = importOneBUResult.RefillBusinessUnitName;
            refillData.EmployeeName = importOneBUResult.RefillEmployeeName;
            importResults.RefillData.Add(refillData);
          }
          worksheet.Cells[row, 19].Value = "Imported";
        }
        worksheet.Cells[row, 20].Value = Calendar.Now.ToString();
      }
      return importResults;
    }
    
    /// <summary>
    /// Импортировать Нашу организацию из Excel.
    /// </summary>
    /// <param name="worksheet">Лист с данными.</param>
    /// <param name="row">Номер строки с данными.</param>
    /// <returns>Результаты импорта.</returns>
    public Structures.Module.ImportRecordResult ImportBusinessUnit(Aspose.Cells.Worksheet worksheet, int row)
    {
      Logger.Debug("=====Import Business unit.=====");
      var importResult = Structures.Module.ImportRecordResult.Create();
      var error = string.Empty;
      var warning = string.Empty;
      var warnings = string.Empty;
      
      var name = worksheet.Cells[row, 0].StringValue.Trim();
      if (string.IsNullOrEmpty(name))
      {
        error = "Business unit is not imported. Name of Business unit is empty.";
        Logger.Error(error);
        importResult.Error = error;
        return importResult;
      }
      
      var tin = worksheet.Cells[row, 4].StringValue.Trim();
      var psrn = worksheet.Cells[row, 6].StringValue.Trim();
      if (!string.IsNullOrEmpty(psrn) && psrn.Length != 13 && psrn.Length != 15)
      {
        error = string.Format("Business unit {0} is not imported. PSRN value {1} must be 13 or 15 numbers long.", name, psrn);
        Logger.Error(error);
        importResult.Error = error;
        return importResult;
      }
      var businessUnits = BusinessUnits.GetAll(b => b.Name == name || (!string.IsNullOrEmpty(tin) && b.TIN == tin) || (!string.IsNullOrEmpty(psrn) && b.PSRN == psrn));
      if (businessUnits.Any())
      {
        error = string.Format("Business unit {0} is not imported. There is a double by the requisites Name = {1}, TIN = {2} or PSRN = {3}.", name, name, tin, psrn);
        Logger.Error(error);
        importResult.Error = error;
        return importResult;
      }
      var trrc = worksheet.Cells[row, 5].StringValue.Trim();
      if (!string.IsNullOrEmpty(trrc) && trrc.Length != 9)
      {
        error = string.Format("Business unit {0} is not imported. TRRC value {1} must be 9 numbers long.", name, trrc);
        Logger.Error(error);
        importResult.Error = error;
        return importResult;
      }
      var nceo = worksheet.Cells[row, 7].StringValue.Trim();
      if (!string.IsNullOrEmpty(nceo) && nceo.Length != 8 && nceo.Length != 10)
      {
        error = string.Format("Business unit {0} is not imported. NCEO value {1} must be 8 or 10 numbers long.", name, nceo);
        Logger.Error(error);
        importResult.Error = error;
        return importResult;
      }
      var account = worksheet.Cells[row, 17].StringValue.Trim();
      if (!string.IsNullOrEmpty(account) && account.Length != 20)
      {
        error = string.Format("Business unit {0} is not imported. Account value {1} must be 20 numbers long.", name, account);
        Logger.Error(error);
        importResult.Error = error;
        return importResult;
      }
      
      var legalName = worksheet.Cells[row, 1].StringValue.Trim();
      var headCompanyName = worksheet.Cells[row, 2].StringValue.Trim();
      var headCompany = BusinessUnits.GetAll(b => b.Name == headCompanyName).FirstOrDefault();
      if (!string.IsNullOrEmpty(headCompanyName) && headCompany == null)
      {
        importResult.RefillBusinessUnitName = headCompanyName;
        Logger.Debug(string.Format("Not found Head business unit {0} for Business unit {1}.", headCompanyName, name));
      }
      var ceoName = worksheet.Cells[row, 3].StringValue.Trim();
      var ceo = Employees.GetAll(e => e.Name == ceoName).FirstOrDefault();
      if (!string.IsNullOrEmpty(ceoName) && ceo == null)
      {
        importResult.RefillEmployeeName = ceoName;
        Logger.Debug(string.Format("Not found CEO {0} for Business unit {1}.", ceoName, name));
      }
      var ncea = worksheet.Cells[row, 8].StringValue.Trim();
      var cityName = worksheet.Cells[row, 9].StringValue.Trim();
      var city = Cities.GetAll(c => c.Name == cityName).FirstOrDefault();
      if (!string.IsNullOrEmpty(cityName) && city == null)
      {
        warning = string.Format(string.Format("Not found City {0} for Business unit {1}.", cityName, name));
        Logger.Debug(warning);
        warnings = string.Format("{0} {1}", warnings, warning);
      }
      var regionName = worksheet.Cells[row, 10].StringValue.Trim();
      var region = Regions.GetAll(r => r.Name == regionName).FirstOrDefault();
      if (!string.IsNullOrEmpty(regionName) && region == null)
      {
        warning = string.Format(string.Format("Not found Region {0} for Business unit {1}.", regionName, name));
        Logger.Debug(warning);
        warnings = string.Format("{0} {1}", warnings, warning);
      }
      var legalAdress = worksheet.Cells[row, 11].StringValue.Trim();
      var postalAdress = worksheet.Cells[row, 12].StringValue.Trim();
      var phones = worksheet.Cells[row, 13].StringValue.Trim();
      var email = worksheet.Cells[row, 14].StringValue.Trim();
      var homepage = worksheet.Cells[row, 15].StringValue.Trim();
      var note = worksheet.Cells[row, 16].StringValue.Trim();
      var bankName = worksheet.Cells[row, 18].StringValue.Trim();
      var bank = Banks.GetAll(b => b.Name == bankName).FirstOrDefault();
      if (!string.IsNullOrEmpty(bankName) && bank == null)
      {
        warning = string.Format(string.Format("Not found Bank {0} for Business unit {1}.", bankName, bank));
        Logger.Debug(warning);
        warnings = string.Format("{0} {1}", warnings, warning);
      }
      try
      {
        var businessUnit = BusinessUnits.Create();
        businessUnit.Name = name;
        businessUnit.LegalName = legalName;
        businessUnit.HeadCompany = headCompany;
        businessUnit.CEO = ceo;
        businessUnit.TIN = tin;
        businessUnit.TRRC = trrc;
        businessUnit.PSRN = psrn;
        businessUnit.NCEO = nceo;
        businessUnit.NCEA = ncea;
        businessUnit.Region = region;
        businessUnit.City = city;
        businessUnit.LegalAddress = legalAdress;
        businessUnit.PostalAddress = postalAdress;
        businessUnit.Phones = phones;
        businessUnit.Email = email;
        businessUnit.Homepage = homepage;
        businessUnit.Note = note;
        businessUnit.Account = account;
        businessUnit.Bank = bank;
        businessUnit.Save();
        importResult.Record = businessUnit;
        Logger.Debug(string.Format("Business unit {0} is imported.", name));
      }
      catch (Exception ex)
      {
        error = string.Format("Business unit {0} is not imported. {1}", name, ex.Message);
        Logger.Error(error);
        importResult.Error = error;
        return importResult;
      }
      importResult.Error = error;
      importResult.Warnings = warnings.Trim();
      return importResult;
    }
    
    /// <summary>
    /// Дозаполнить Наши организации.
    /// </summary>
    /// <param name="refillDataList">Данные о Наших организациях, которые нужно дозаполнить.</param>
    /// <returns>Результаты импорта.</returns>
    public Structures.Module.ImportWorksheetResult RefillBusinessUnits(List<Structures.Module.ImportRefillData> refillDataList)
    {
      Logger.Debug("=====Refill Business units.=====");
      var importResults = Structures.Module.ImportWorksheetResult.Create();
      importResults.EmptyBusinessUnitCreatedCount = 0;
      importResults.Errors = new List<string>();
      
      foreach (var refillData in refillDataList)
      {
        var refillOneBUResult = this.RefillBusinessUnit(refillData);
        if (!string.IsNullOrEmpty(refillOneBUResult.Error))
          importResults.Errors.Add(refillOneBUResult.Error);
        else
        {
          if (refillOneBUResult.IsEmptyBusinessUnitCreated)
            importResults.EmptyBusinessUnitCreatedCount++;
        }
      }
      return importResults;
    }
    
    /// <summary>
    /// Дозаполнить Нашу организацию.
    /// </summary>
    /// <param name="refillData">Данные о Нашей организации, которые нужно дозаполнить.</param>
    /// <returns>Результаты импорта.</returns>
    public Structures.Module.ImportRecordResult RefillBusinessUnit(Structures.Module.ImportRefillData refillData)
    {
      Logger.Debug("=====Refill Business unit.=====");
      var importResult = Structures.Module.ImportRecordResult.Create();
      
      var businessUnit = BusinessUnits.As(refillData.Record);
      var headCompanyName = refillData.BusinessUnitName;
      if (!string.IsNullOrEmpty(headCompanyName))
      {
        var headCompany = BusinessUnits.GetAll(b => b.Name == headCompanyName).FirstOrDefault();
        // Предполагается, что НОР уже импортированы, если нужной нет в списке - создать с указанием только наименования и отметить это
        // в результатах для корректной статистики.
        if (headCompany == null)
        {
          try
          {
            headCompany = BusinessUnits.Create();
            headCompany.Name = headCompanyName;
            headCompany.Save();
            Logger.Debug(string.Format("New Business unit {0} is created.", headCompanyName));
            importResult.IsEmptyBusinessUnitCreated = true;
          }
          catch (Exception ex)
          {
            Logger.Debug(string.Format("Business unit {0} is not created. {1}", headCompanyName, ex.Message));
          }
        }
        businessUnit.HeadCompany = headCompany;
      }
      var ceoName = refillData.EmployeeName;
      var ceo = Employees.GetAll(e => e.Name == ceoName).FirstOrDefault();
      if (!string.IsNullOrEmpty(ceoName) && ceo == null)
        Logger.Debug(string.Format("Not found CEO {0} for Business unit {1}.", ceoName, businessUnit.Name));
      else
        businessUnit.CEO = ceo;
      businessUnit.Save();
      return importResult;
    }
    
    /// <summary>
    /// Импортировать Подразделения из Excel.
    /// </summary>
    /// <param name="worksheet">Лист с данными.</param>
    /// <returns>Результаты импорта.</returns>
    public Structures.Module.ImportWorksheetResult ImportDepartments(Aspose.Cells.Worksheet worksheet)
    {
      Logger.Debug("=====Import Departments.=====");
      var importResults = Structures.Module.ImportWorksheetResult.Create();
      var departmentsCount = worksheet.Cells.Rows.Count - 1;
      importResults.TotalCount = departmentsCount;
      importResults.ImportedCount = 0;
      importResults.EmptyBusinessUnitCreatedCount = 0;
      importResults.Errors = new List<string>();
      importResults.RefillData = new List<Structures.Module.ImportRefillData>();
      
      for (var row = 1; row <= departmentsCount; row++)
      {
        var importOneDepartmentResult = this.ImportDepartment(worksheet, row);
        var importOneDepartmentError = importOneDepartmentResult.Error;
        var importOneDepartmentWarnings = importOneDepartmentResult.Warnings;
        if (!string.IsNullOrEmpty(importOneDepartmentError))
        {
          importResults.Errors.Add(importOneDepartmentError);
          worksheet.Cells[row, 8].Value = "Not imported";
          worksheet.Cells[row, 10].Value = importOneDepartmentError;
        }
        else
        {
          importResults.ImportedCount++;
          if (importOneDepartmentResult.RefillDepartmentName != null || importOneDepartmentResult.RefillEmployeeName != null)
          {
            var refillData = Structures.Module.ImportRefillData.Create();
            refillData.Record = importOneDepartmentResult.Record;
            refillData.DepartmentName = importOneDepartmentResult.RefillDepartmentName;
            refillData.EmployeeName = importOneDepartmentResult.RefillEmployeeName;
            importResults.RefillData.Add(refillData);
          }
          worksheet.Cells[row, 8].Value = "Imported";
        }
        worksheet.Cells[row, 9].Value = Calendar.Now.ToString();
        if (importOneDepartmentResult.IsEmptyBusinessUnitCreated)
          importResults.EmptyBusinessUnitCreatedCount++;
      }
      return importResults;
    }
    
    /// <summary>
    /// Импортировать Подразделение из Excel.
    /// </summary>
    /// <param name="worksheet">Лист с данными.</param>
    /// <param name="row">Номер строки с данными.</param>
    /// <returns>Результаты импорта.</returns>
    public Structures.Module.ImportRecordResult ImportDepartment(Aspose.Cells.Worksheet worksheet, int row)
    {
      Logger.Debug("=====Import Department.=====");
      var importResult = Structures.Module.ImportRecordResult.Create();
      var error = string.Empty;
      
      var name = worksheet.Cells[row, 0].StringValue.Trim();
      if (string.IsNullOrEmpty(name))
      {
        error = "Department is not imported. Name of Department is empty.";
        Logger.Error(error);
        importResult.Error = error;
        return importResult;
      }
      var code = worksheet.Cells[row, 4].StringValue.Trim();
      if (!string.IsNullOrEmpty(code) && code.Length > 10)
      {
        error = string.Format("Department {0} is not imported. Code can't be longer than 10 characters.", name);
        Logger.Error(error);
        importResult.Error = error;
        return importResult;
      }
      var shortName = worksheet.Cells[row, 1].StringValue.Trim();
      
      var headOfficeName = worksheet.Cells[row, 2].StringValue.Trim();
      var headOffice = Departments.GetAll(d => d.Name == headOfficeName).FirstOrDefault();
      if (!string.IsNullOrEmpty(headOfficeName) && headOffice == null)
      {
        importResult.RefillDepartmentName = headOfficeName;
        Logger.Debug(string.Format("Not found Head office {0} for Department {1}.", headOfficeName, name));
      }
      
      var businessUnitName = worksheet.Cells[row, 3].StringValue.Trim();
      var businessUnit = BusinessUnits.GetAll(b => b.Name == businessUnitName).FirstOrDefault();
      // Предполагается, что НОР уже импортированы, если нужной нет в списке - создать с указанием только наименования и отметить это
      // в результатах для корректной статистики.
      if (!string.IsNullOrEmpty(businessUnitName) && businessUnit == null)
      {
        try
        {
          businessUnit = BusinessUnits.Create();
          businessUnit.Name = businessUnitName;
          businessUnit.Save();
          Logger.Debug(string.Format("New Business unit {0} is created.", businessUnitName));
          importResult.IsEmptyBusinessUnitCreated = true;
        }
        catch (Exception ex)
        {
          Logger.Error(string.Format("Business unit {0} is not created. {1}", businessUnitName, ex.Message));
        }
      }
      
      var departments = Departments.GetAll(b => b.Name == name && Equals(b.BusinessUnit, businessUnit));
      if (departments.Any())
      {
        error = string.Format("Department {0} is not imported. There is a double by the requisites Name = {1}, BusinessUnit = {2}.", name, name, businessUnitName);
        Logger.Error(error);
        importResult.Error = error;
        return importResult;
      }
      
      var managerName = worksheet.Cells[row, 5].StringValue.Trim();
      var manager = Employees.GetAll(e => e.Name == managerName).FirstOrDefault();
      if (!string.IsNullOrEmpty(managerName) && manager == null)
      {
        importResult.RefillEmployeeName = managerName;
        Logger.Debug(string.Format("Not found Manager {0} for Department {1}.", managerName, name));
      }
      
      var phone = worksheet.Cells[row, 6].StringValue.Trim();
      var note = worksheet.Cells[row, 7].StringValue.Trim();
      try
      {
        var department = Departments.Create();
        department.Name = name;
        department.ShortName = shortName;
        department.Code = code;
        department.BusinessUnit = businessUnit;
        department.HeadOffice = headOffice;
        department.Manager = manager;
        department.Phone = phone;
        department.Note = note;
        department.Save();
        importResult.Record = department;
        Logger.Debug(string.Format("Department {0} is imported.", name));
      }
      catch (Exception ex)
      {
        error = string.Format("Department {0} is not imported. {1}", name, ex.Message);
        Logger.Error(error);
      }
      importResult.Error = error;
      return importResult;
    }
    
    /// <summary>
    /// Дозаполнить Подразделения.
    /// </summary>
    /// <param name="refillDataList">Данные о Подразделениях, которые нужно дозаполнить.</param>
    /// <returns>Результаты импорта.</returns>
    public Structures.Module.ImportWorksheetResult RefillDepartments(List<Structures.Module.ImportRefillData> refillDataList)
    {
      Logger.Debug("=====Refill Departments.=====");
      var importResults = Structures.Module.ImportWorksheetResult.Create();
      
      importResults.EmptyDepartmentCreatedCount = 0;
      importResults.Errors = new List<string>();
      
      foreach (var refillData in refillDataList)
      {
        var refillOneDepartmentResult = this.RefillDepartment(refillData);
        if (!string.IsNullOrEmpty(refillOneDepartmentResult.Error))
          importResults.Errors.Add(refillOneDepartmentResult.Error);
        else
        {
          if (refillOneDepartmentResult.IsEmptyDepartmentCreated)
            importResults.EmptyDepartmentCreatedCount++;
        }
      }
      return importResults;
    }
    
    /// <summary>
    /// Дозаполнить Подразделение.
    /// </summary>
    /// <param name="refillData">Данные о Подразделении, которые нужно дозаполнить.</param>
    /// <returns>Результаты импорта.</returns>
    public Structures.Module.ImportRecordResult RefillDepartment(Structures.Module.ImportRefillData refillData)
    {
      Logger.Debug("=====Refill Department.=====");
      var importResult = Structures.Module.ImportRecordResult.Create();
      
      var department = Departments.As(refillData.Record);
      var headOfficeName = refillData.DepartmentName;
      if (!string.IsNullOrEmpty(headOfficeName))
      {
        var headOffice = Departments.GetAll(b => b.Name == headOfficeName).FirstOrDefault();
        // Предполагается, что Подразделения уже импортированы, если нужного нет в списке - создать с указанием только наименования и отметить это
        // в результатах для корректной статистики.
        if (headOffice == null)
        {
          try
          {
            headOffice = Departments.Create();
            headOffice.Name = headOfficeName;
            headOffice.Save();
            Logger.Debug(string.Format("New Department {0} is created.", headOfficeName));
            importResult.IsEmptyDepartmentCreated = true;
          }
          catch (Exception ex)
          {
            Logger.Debug(string.Format("Department {0} is not created. {1}", headOfficeName, ex.Message));
          }
        }
        department.HeadOffice = headOffice;
      }
      var managerName = refillData.EmployeeName;
      var manager = Employees.GetAll(e => e.Name == managerName).FirstOrDefault();
      if (!string.IsNullOrEmpty(managerName) && manager == null)
        Logger.Debug(string.Format("Not found Manager {0} for Department {1}.", managerName, department.Name));
      else
        department.Manager = manager;
      department.Save();
      return importResult;
    }
    
    /// <summary>
    /// Импортировать Сотрудников из Excel.
    /// </summary>
    /// <param name="worksheet">Лист с данными.</param>
    /// <returns>Результаты импорта.</returns>
    public Structures.Module.ImportWorksheetResult ImportEmployees(Aspose.Cells.Worksheet worksheet)
    {
      Logger.Debug("=====Import Employees.=====");
      var importResults = Structures.Module.ImportWorksheetResult.Create();
      var employeesCount = worksheet.Cells.Rows.Count - 1;
      importResults.TotalCount = employeesCount;
      importResults.ImportedCount = 0;
      importResults.PersonCreatedCount = 0;
      importResults.EmptyDepartmentCreatedCount = 0;
      importResults.JobTitleCreatedCount = 0;
      importResults.Errors = new List<string>();
      
      for (var row = 1; row <= employeesCount; row++)
      {
        var importOneEmployeeResult = this.ImportEmployee(worksheet, row);
        var importOneEmployeeError = importOneEmployeeResult.Error;
        var importOneEmployeeWarnings = importOneEmployeeResult.Warnings;
        if (!string.IsNullOrEmpty(importOneEmployeeError))
        {
          importResults.Errors.Add(importOneEmployeeError);
          worksheet.Cells[row, 23].Value = "Not imported";
          worksheet.Cells[row, 25].Value = importOneEmployeeError;
        }
        else if (!string.IsNullOrEmpty(importOneEmployeeWarnings))
        {
          importResults.Errors.Add(importOneEmployeeWarnings);
          worksheet.Cells[row, 23].Value = "Imported partially";
          worksheet.Cells[row, 25].Value = importOneEmployeeWarnings;
          importResults.ImportedCount++;
          importResults.PartiallyImportedCount++;
        }
        else
        {
          importResults.ImportedCount++;
          worksheet.Cells[row, 23].Value = "Imported";
        }
        worksheet.Cells[row, 24].Value = Calendar.Now.ToString();
        if (importOneEmployeeResult.IsPersonCreated)
          importResults.PersonCreatedCount++;
        if (importOneEmployeeResult.IsEmptyDepartmentCreated)
          importResults.EmptyDepartmentCreatedCount++;
        if (importOneEmployeeResult.IsJobTitleCreated)
          importResults.JobTitleCreatedCount++;
      }
      return importResults;
    }
    
    /// <summary>
    /// Импортировать Сотрудника из Excel.
    /// </summary>
    /// <param name="worksheet">Лист с данными.</param>
    /// <param name="row">Номер строки с данными.</param>
    /// <returns>Результаты импорта.</returns>
    public Structures.Module.ImportRecordResult ImportEmployee(Aspose.Cells.Worksheet worksheet, int row)
    {
      Logger.Debug("=====Import Employee.=====");
      var importResult = Structures.Module.ImportRecordResult.Create();
      var error = string.Empty;
      var warning = string.Empty;
      var warnings = string.Empty;
      
      var lastName = worksheet.Cells[row, 2].StringValue.Trim();
      if (string.IsNullOrEmpty(lastName))
      {
        error = "Employee is not imported. Surname is empty.";
        Logger.Error(error);
        return importResult;
      }
      var firstName = worksheet.Cells[row, 3].StringValue.Trim();
      if (string.IsNullOrEmpty(firstName))
      {
        error = "Employee is not imported. Name is empty.";
        Logger.Error(error);
        importResult.Error = error;
        return importResult;
      }
      var departmentName = worksheet.Cells[row, 0].StringValue.Trim();
      if (string.IsNullOrEmpty(departmentName))
      {
        error = "Employee is not imported. Department is empty.";
        Logger.Error(error);
        return importResult;
      }
      var middleName = worksheet.Cells[row, 4].StringValue.Trim();
      DateTime? dateOfBirth = null;
      if (worksheet.Cells[row, 6].StringValue.Trim() != string.Empty)
      {
        try
        {
          dateOfBirth = Convert.ToDateTime(worksheet.Cells[row, 6].StringValue.Trim());
        }
        catch (Exception)
        {
          throw;
        }
      }
      var tin = worksheet.Cells[row, 7].StringValue.Trim();
      var inila = worksheet.Cells[row, 8].StringValue.Trim();
      
      // Проверить, не создан ли уже сотрудник с такими данными, проверку сделать по Подразделению сотрудника и ФИО из его персоны,
      // если в файле указаны еще ДР, ИНН, СНИЛС - искать по ним в персоне сотрудника.
      var employeeFullName = string.Format("{0} {1} {2}", lastName, firstName, middleName).Trim();
      var employees = Employees.GetAll(e => e.Person.FirstName == firstName && e.Person.LastName == lastName &&
                                       e.Person.MiddleName == middleName && e.Department.Name == departmentName);
      if (dateOfBirth != null)
        employees = employees.Where(e => e.Person.DateOfBirth == dateOfBirth);
      if (tin != null)
        employees = employees.Where(e => e.Person.TIN == tin);
      if (inila != null)
        employees = employees.Where(e => e.Person.INILA == inila);
      if (employees.Any())
      {
        error = string.Format("Employee {0} is not imported. There is a double by the requisites Name = {1}, Department = {2}.", employeeFullName, employeeFullName, departmentName);
        Logger.Error(error);
        importResult.Error = error;
        return importResult;
      }
      
      // Персон с указанными ФИО может быть несколько, поэтому смотрим, не указаны ли в файле еще ДР, ИНН, СНИЛС, отфильтруем еще по ним.
      var persons = People.GetAll(p => p.FirstName == firstName && p.LastName == lastName && p.MiddleName == middleName);
      if (dateOfBirth != null)
        persons = persons.Where(p => p.DateOfBirth == dateOfBirth);
      if (tin != null)
        persons = persons.Where(p => p.TIN == tin);
      if (inila != null)
        persons = persons.Where(p => p.INILA == inila);
      var person = People.Null;
      // Если ничего не нашли - придется создавать новую персону.
      // Если все же нашли несколько - взять первую и написать в лог.
      if (!persons.Any())
      {
        var createdPerson = this.ImportPerson(worksheet, row);
        person = People.As(createdPerson.Record);
        if (person != null)
        {
          importResult.IsPersonCreated = true;
          warnings = createdPerson.Warnings;
        }
        else
        {
          importResult.Error = createdPerson.Error;
          return importResult;
        }
      }
      else if (persons.Count() > 1)
      {
        person = persons.First();
        warning = string.Format("The person with name {0} {1} {2} is not unic. Check imported data.", lastName, firstName, middleName);
        Logger.Debug(warning);
        warnings = string.Format("{0} {1}", warnings, warning);
      }
      else
        person = persons.First();
      var fullName = person.Name;
      
      // Предполагается, что Подразделения уже импортированы, если нужного нет в списке - создать с указанием только наименования
      // и отметить это в результатах для корректной статистики.
      var departments = Departments.GetAll(d => d.Name == departmentName);
      var department = Departments.Null;
      if (!departments.Any())
      {
        try
        {
          department = Departments.Create();
          department.Name = departmentName;
          department.Save();
          Logger.Debug(string.Format("New Department {0} is created.", departmentName));
          importResult.IsEmptyDepartmentCreated = true;
        }
        catch (Exception ex)
        {
          error = string.Format("Department {0} is not created. {1}", departmentName, ex.Message);
          Logger.Error(error);
          importResult.Error = error;
          return importResult;
        }
      }
      else if (departments.Count() > 1)
      {
        department = departments.First();
        warning = string.Format("The Department {0} is not unic. Check imported data.", departmentName);
        Logger.Debug(warning);
        warnings = string.Format("{0} {1}", warnings, warning);
      }
      else
        department = departments.First();
      
      // Должность сначала ищем в указанном подразделении, если нет - без указания подразделения.
      var jobTitleName = worksheet.Cells[row, 1].StringValue.Trim();
      var jobTitle = JobTitles.Null;
      if (!string.IsNullOrEmpty(jobTitleName))
      {
        var jobTitles = JobTitles.GetAll(j => j.Name == jobTitleName && Equals(j.Department, department));
        if (!jobTitles.Any())
          jobTitles = JobTitles.GetAll(j => j.Name == jobTitleName && j.Department == null);
        // Создать должность, если не нашли ничего в справочнике.
        if (!jobTitles.Any())
        {
          try
          {
            jobTitle = JobTitles.Create();
            jobTitle.Name = jobTitleName;
            jobTitle.Save();
            Logger.Debug(string.Format("New Job title {0} is created.", jobTitleName));
            importResult.IsJobTitleCreated = true;
          }
          catch (Exception ex)
          {
            warning = string.Format("Job title {0} is not created. {1}", jobTitleName, ex.Message);
            Logger.Error(warning);
            warnings = string.Format("{0} {1}", warnings, warning);
          }
        }
        else if (jobTitles.Count() > 1)
        {
          jobTitle = jobTitles.First();
          warning = string.Format("The Job title {0} is not unic. Check imported data.", jobTitleName);
          Logger.Debug(warning);
          warnings = string.Format("{0} {1}", warnings, warning);
        }
        else
          jobTitle = jobTitles.First();
      }
      
      var email = worksheet.Cells[row, 14].StringValue.Trim();
      var phone = worksheet.Cells[row, 13].StringValue.Trim();
      var note = worksheet.Cells[row, 18].StringValue.Trim();
      var personnelNumber = worksheet.Cells[row, 19].StringValue.Trim();
      var personnelPhone = worksheet.Cells[row, 20].StringValue.Trim();
      var messagesEmail = worksheet.Cells[row, 21].StringValue.Trim();
      var employmentType = worksheet.Cells[row, 22].StringValue.Trim();
      
      try
      {
        var employee = DirRX.EssPlatformSolution.Employees.Create();
        employee.Person = person;
        employee.Department = department;
        employee.JobTitle = jobTitle;
        employee.Email = email;
        employee.Phone = phone;
        employee.Note = note;
        employee.PersonnelNumber = personnelNumber;
        employee.PersonalPhoneDirRX = personnelPhone;
        employee.MessagesEmailDirRX = messagesEmail;
        employee.NeedNotifyExpiredAssignments = false;
        employee.NeedNotifyNewAssignments = false;
        if (employmentType == employee.Info.Properties.EmploymentType.GetLocalizedValue(HRSolution.Employee.EmploymentType.MainPlace))
        {
          employee.EmploymentType = HRSolution.Employee.EmploymentType.MainPlace;
        }
        else if (employmentType == employee.Info.Properties.EmploymentType.GetLocalizedValue(HRSolution.Employee.EmploymentType.InternalConcurr))
        {
          employee.EmploymentType = HRSolution.Employee.EmploymentType.InternalConcurr;
        }
        else if (employmentType == employee.Info.Properties.EmploymentType.GetLocalizedValue(HRSolution.Employee.EmploymentType.ExternalConcurr))
        {
          employee.EmploymentType = HRSolution.Employee.EmploymentType.ExternalConcurr;
        }
        employee.Save();
        importResult.Record = employee;
        Logger.Debug(string.Format("Employee {0} is imported.", fullName));
      }
      catch (Exception ex)
      {
        error = string.Format("Employee {0} is not imported. {1}", fullName, ex.Message);
        Logger.Error(error);
      }
      importResult.Error = error;
      importResult.Warnings = warnings.Trim();
      return importResult;
    }
    
    /// <summary>
    /// Импортировать Персону из Excel.
    /// </summary>
    /// <param name="worksheet">Лист с данными.</param>
    /// <param name="row">Номер строки с данными.</param>
    /// <returns>Результаты импорта.</returns>
    public Structures.Module.ImportRecordResult ImportPerson(Aspose.Cells.Worksheet worksheet, int row)
    {
      Logger.Debug("=====Import Person.=====");
      var importResult = Structures.Module.ImportRecordResult.Create();
      var error = string.Empty;
      var warning = string.Empty;
      var warnings = string.Empty;
      
      var lastName = worksheet.Cells[row, 2].StringValue.Trim();
      var firstName = worksheet.Cells[row, 3].StringValue.Trim();
      var middleName = worksheet.Cells[row, 4].StringValue.Trim();
      var personName = string.Format("{0} {1} {2}", lastName, firstName, middleName).Trim();
      var account = worksheet.Cells[row, 16].StringValue.Trim();
      if (!string.IsNullOrEmpty(account) && (account.Length < 8 || account.Length > 34))
      {
        error = string.Format("Person {0} is not imported. Account value {1} must be between 8 and 34 numbers long.", personName, account);
        Logger.Error(error);
        importResult.Error = error;
        return importResult;
      }
      var sex = worksheet.Cells[row, 5].StringValue.Trim();
      DateTime? dateOfBirth = null;
      if (!string.IsNullOrEmpty(worksheet.Cells[row, 6].StringValue.Trim()))
      {
        try
        {
          dateOfBirth = Convert.ToDateTime(worksheet.Cells[row, 6].StringValue.Trim());
        }
        catch (Exception ex)
        {
          warning = string.Format("Date of birth {0} for Person {1} is not filled. {2}", personName, ex.Message);
          Logger.Error(warning);
          warnings = string.Format("{0} {1}", warnings, warning);
        }
      }
      var tin = worksheet.Cells[row, 7].StringValue.Trim();
      var inila = worksheet.Cells[row, 8].StringValue.Trim();
      var cityName = worksheet.Cells[row, 9].StringValue.Trim();
      var city = Cities.GetAll(c => c.Name == cityName).FirstOrDefault();
      if (!string.IsNullOrEmpty(cityName) && city == null)
      {
        warning = string.Format("Not found City {0} for Person {1}.", cityName, personName);
        Logger.Debug(warning);
        warnings = string.Format("{0} {1}", warnings, warning);
      }
      var regionName = worksheet.Cells[row, 10].StringValue.Trim();
      var region = Regions.GetAll(r => r.Name == regionName).FirstOrDefault();
      if (!string.IsNullOrEmpty(regionName) && region == null)
      {
        warning = string.Format("Not found Region {0} for Person {1}.", regionName, personName);
        Logger.Debug(warning);
        warnings = string.Format("{0} {1}", warnings, warning);
      }
      var legalAdress = worksheet.Cells[row, 11].StringValue.Trim();
      var postalAdress = worksheet.Cells[row, 12].StringValue.Trim();
      var email = worksheet.Cells[row, 13].StringValue.Trim();
      var homepage = worksheet.Cells[row, 14].StringValue.Trim();
      var bankName = worksheet.Cells[row, 15].StringValue.Trim();
      var bank = Banks.GetAll(b => b.Name == bankName).FirstOrDefault();
      if (!string.IsNullOrEmpty(bankName) && bank == null)
      {
        warning = string.Format("Not found Bank {0} for Person {1}.", bankName, personName);
        Logger.Debug(warning);
        warnings = string.Format("{0} {1}", warnings, warning);
      }
      var note = worksheet.Cells[row, 17].StringValue.Trim();
      try
      {
        var person = Sungero.Parties.People.Create();
        person.FirstName = firstName;
        person.LastName = lastName;
        person.MiddleName = middleName;
        person.Sex = sex == "Женский" ? Sungero.Parties.Person.Sex.Female : Sungero.Parties.Person.Sex.Male;
        person.DateOfBirth = dateOfBirth;
        person.TIN = tin;
        person.INILA = inila;
        person.Region = region;
        person.City = city;
        person.LegalAddress = legalAdress;
        person.PostalAddress = postalAdress;
        person.Email = email;
        person.Homepage = homepage;
        person.Bank = bank;
        person.Account = account;
        person.Note = note;
        person.Save();
        importResult.Record = person;
        Logger.Debug(string.Format("Person {0} is imported.", person.Name));
      }
      catch (Exception ex)
      {
        error = string.Format("Person {0} is not imported. {1}", personName, ex.Message);
        Logger.Error(error);
      }
      importResult.Error = error;
      importResult.Warnings = warnings.Trim();
      return importResult;
    }
    
    #endregion
    
    #region Функции для работы с личным кабинетом через сервис интеграции.
    /// <summary>
    /// Получить причину отказа подписания кадровых договорных документов.
    /// </summary>
    /// <param name="assignmentId">Ид задания.</param>
    /// <returns>Причина отказа.</returns>
    [Public, Remote]
    public string GetDenySignReason(int assignmentId)
    {
      var task = RefuseManagerSignNotices.GetAll(x => x.Id == assignmentId).FirstOrDefault().Task;
      var assignments = SignAssignments.GetAll(x => Equals(x.Task, task) && x.Id != assignmentId && x.Result == DirRX.HRLite.SignAssignment.Result.Refuse);
      
      if (assignments.Any())
        return assignments.OrderByDescending(x => x.Created).FirstOrDefault().ActiveText;
      
      return string.Empty;
    }
    
    /// <summary>
    /// Получить отказавшего подписания кадровых договорных документов.
    /// </summary>
    /// <param name="assignmentId">Ид задания.</param>
    /// <returns>Причина отказа.</returns>
    [Public, Remote]
    public string GetDenySignPerson(int assignmentId)
    {
      var task = RefuseManagerSignNotices.GetAll(x => x.Id == assignmentId).FirstOrDefault().Task;
      var assignments = SignAssignments.GetAll(x => Equals(x.Task, task) && x.Id != assignmentId && x.Result == DirRX.HRLite.SignAssignment.Result.Refuse);
      
      if (assignments.Any())
        return assignments.OrderByDescending(x => x.Created).FirstOrDefault().Performer.DisplayValue;
      
      return string.Empty;
    }
    
    /// <summary>
    /// Получить текст задачи на подписание кадровых договорных документов.
    /// </summary>
    /// <param name="assignmentId">Ид задания.</param>
    /// <returns>Текст задачи.</returns>
    [Public, Remote]
    public string GetSignTaskActiveText(int assignmentId)
    {
      var activeText = EmployeeSignAssignments.GetAll(x => Equals(x.Id, assignmentId)).FirstOrDefault().Task.ActiveText;
      return string.IsNullOrEmpty(activeText) ? string.Empty : activeText;
    }
    
    /// <summary>
    /// Получить текст задачи на подписание кадровых документов.
    /// </summary>
    /// <param name="assignmentId">Ид задания.</param>
    /// <returns>Текст задачи.</returns>
    [Public, Remote]
    public string GetAcquaintanceTaskActiveText(int assignmentId)
    {
      var activeText = HRAcquaintanceAssignments.GetAll(x => Equals(x.Id, assignmentId)).FirstOrDefault().Task.ActiveText;
      return string.IsNullOrEmpty(activeText) ? string.Empty : activeText;
    }
    #endregion
    
    #region Отправка уведомлений о приближении срока задний на подписание и ознакомление.
    /// <summary>
    /// Отправить уведомления о приближении срока задний на подписание.
    /// </summary>
    /// <param name="daysCount">За сколько дней отправить уведомление.</param>
    [Public, Remote]
    public virtual void SendSignTaskDeadlineNotice(int daysCount)
    {
      // Уведомление рассылать разово по задачам в работе, по которым автор запросил уведомление.
      var notifyTasks = SignTasks.GetAll(t => t.Status == Sungero.Workflow.Task.Status.InProcess && t.NotifyAuthor.HasValue && t.NotifyAuthor.Value).ToList();
      // Взять только те задачи, где уведомление еще не рассылалось. Если задачу рестартовали, то учитываются данные только с момента рестарта.
      notifyTasks = notifyTasks.Where(t => !SignDeadlineNotices.GetAll(n => n.Task.Equals(t) && n.TaskStartId == t.StartId).Any()).ToList();
      // Выбрать задачи, которые уже ушли на подписание первым из участников и до окончания планового срока подписания указанное кол-во дней.
      notifyTasks = notifyTasks.Where(t =>
                                      // Первым подписывает сотрудник.
                                      (t.SignQueue == HRLite.SignTask.SignQueue.WorkerFirst &&
                                       // Задание уже ушло сотруднику. Если задачу рестартовали, то учитываются данные только с момента рестарта.
                                       EmployeeSignAssignments.GetAll(a => t.Equals(a.Task) && a.TaskStartId == t.StartId).Any() &&
                                       // Ожидаемый срок выполнения задачи, расчитанный от срата задания сотруднику + кол-во дней на согласование сотрудником + кол-во дней на согласование (без времени),
                                       // должен выйти через указанное кол-во дней или ранее.
                                       EmployeeSignAssignments.GetAll(a => t.Equals(a.Task) && a.TaskStartId == t.StartId).OrderByDescending(a => a.Created).FirstOrDefault()
                                       .Created.Value.AddWorkingDays((t.EmployeeDeadline == null ? 0 : t.EmployeeDeadline.Value) +
                                                                     (t.SignatoryDeadline == null ? 0 : t.SignatoryDeadline.Value)).BeginningOfDay() <= Calendar.Today.AddWorkingDays(daysCount)) ||
                                      // Первым подписывает руководитель.
                                      (t.SignQueue == HRLite.SignTask.SignQueue.EmployerFirst &&
                                       // Задание уже ушло руководителю. Если задачу рестартовали, то учитываются данные только с момента рестарта.
                                       SignAssignments.GetAll(a => t.Equals(a.Task) && a.TaskStartId == t.StartId).Any() &&
                                       // Ожидаемый срок выполнения задачи, расчитанный от срата задания руководителю + кол-во дней на согласование сотрудником + кол-во дней на согласование (без времени),
                                       // должен выйти через указанное кол-во дней или ранее.
                                       SignAssignments.GetAll(a => t.Equals(a.Task) && a.TaskStartId == t.StartId).OrderByDescending(a => a.Created).FirstOrDefault()
                                       .Created.Value.AddWorkingDays((t.EmployeeDeadline == null ? 0 : t.EmployeeDeadline.Value) +
                                                                     (t.SignatoryDeadline == null ? 0 : t.SignatoryDeadline.Value)).BeginningOfDay() <= Calendar.Today.AddWorkingDays(daysCount))).ToList();
      foreach (var task in notifyTasks)
      {
        var notice = SignDeadlineNotices.Create(task);
        var subject = DirRX.HRLite.SignDeadlineNotices.Resources.SignDeadlineNoticeSubjectFormat(task.DocumentGroup.BilaterialDocuments.FirstOrDefault().DisplayValue).ToString();
        notice.Performer = task.Author;
        notice.Subject = subject.Length > 250 ? subject.Substring(0, 250) : subject;
        notice.Save();
      }
    }

    /// <summary>
    /// Отправить уведомления о приближении срока задания на ознакомление с кадровыми документами и ЛНА.
    /// </summary>
    /// <param name="daysCount">За сколько дней отправить уведомление.</param>
    [Public, Remote]
    public virtual void SendAcquaintanceTaskDeadlineNotice(int daysCount)
    {
      // Уведомление рассылать разово по задачам в работе, по которым автор запросил уведомление.
      var notifyTasks = HRAcquaintanceTasks.GetAll(t => t.Status == Sungero.Workflow.Task.Status.InProcess && t.NotifyAuthor.HasValue && t.NotifyAuthor.Value).ToList();
      // Взять только те задачи, где уведомление еще не рассылалось. Если задачу рестартовали, то учитываются данные только с момента рестарта.
      notifyTasks = notifyTasks.Where(t => !AcquaintanceDeadlineNotices.GetAll(n => n.Task.Equals(t) && n.TaskStartId == t.StartId).Any()).ToList();
      // Выбрать задачи, которые уже ушли на ознакомление сотрудником и у которых срок подходит через указанное кол-во дней.
      notifyTasks = notifyTasks.Where(t =>
                                      // Процесс ознакомления еще идет (есть задания в работе). Если задачу рестартовали, то учитываются данные только с момента рестарта.
                                      HRAcquaintanceAssignments.GetAll(a => t.Equals(a.Task) && a.TaskStartId == t.StartId && a.Status == Sungero.Workflow.AssignmentBase.Status.InProcess).Any() &&
                                      // С учетом того, что после отказа может быть отправлено повторное ознакомление, взять последнее из заданий.
                                      // Его срок (без времени) должен выйти через указанное кол-во дней или ранее.
                                      HRAcquaintanceAssignments.GetAll(a => t.Equals(a.Task) && a.TaskStartId == t.StartId && a.Status == Sungero.Workflow.AssignmentBase.Status.InProcess)
                                      .OrderByDescending(a => a.Created).FirstOrDefault().Deadline.Value.BeginningOfDay() <= Calendar.Today.AddWorkingDays(daysCount)).ToList();
      foreach (var task in notifyTasks)
      {
        var notice = AcquaintanceDeadlineNotices.Create(task);
        var subject = DirRX.HRLite.AcquaintanceDeadlineNotices.Resources.AcquaintanceDeadlineNoticeSubjectFormat(task.DocumentGroup.InternalDocumentBases.FirstOrDefault().DisplayValue).ToString();
        notice.Performer = task.Author;
        notice.Subject = subject.Length > 250 ? subject.Substring(0, 250) : subject;
        notice.Save();
      }
    }
    #endregion
    
    #region Работа с замещениями
    
    /// <summary>
    /// Настроить замещения автоматически.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    [Public]
    public void ManageSubstitutionOptions(DirRX.HRSolution.IEmployee employee)
    {
      var person = employee.Person;
      if (person != null)
      {
        this.StartAutoSubstitutionAsync(person.Id);
      }
      return;
    }
    
    /// <summary>
    /// Проверить, больше ли одной учетной записи у персоны.
    /// </summary>
    /// <param name="person">Персона.</param>
    /// <returns>True, если больше одной учетной записи у персоны.
    /// False, если не больше одной учетной записи у персоны.</returns>
    public bool IsMultipleLoginsPerson(IPerson person)
    {
      var employeesWithLogin = Employees.GetAll(x => x.Person.Id == person.Id && x.Login != null).Count();
      return employeesWithLogin > 1;
    }
    
    /// <summary>
    /// Отправить уведомление СОК об автоматическом создании замещений по сотрудникам персоны.
    /// </summary>
    /// <param name="substitute">Замещающий.</param>
    /// <param name="substitution">Замещаемый.</param>
    public void StartAutoSubstitutionNotice(IEmployee substitute, List<HRSolution.IEmployee> substitutions)
    {
      if (substitute != null && substitutions.Any())
      {
        var subject = DirRX.HRLite.Resources.AutoSubstitutionSubjectFormat(substitute.Name);
        var performer = Roles.GetAll(x => x.Sid == HRLite.PublicConstants.Module.Roles.HRSpecialist).FirstOrDefault();
        
        var subtitutionNoticeDetailList = new List<string> ();
        foreach (var employee in substitutions)
          subtitutionNoticeDetailList.Add(Resources.SubtitutionNoticeDetailFormat(Hyperlinks.Get(substitute), substitute.Department.BusinessUnit.Name, substitute.Department.Name, substitute.JobTitle?.Name,
                                                                                  Hyperlinks.Get(employee), employee.Department.BusinessUnit.Name, employee.Department.Name, employee.JobTitle?.Name));
        var subtitutionNoticeDetailText = string.Empty;
        if (substitutions.Count == 1)
          subtitutionNoticeDetailText = Resources.OneSubtitutionNoticeFormat(subtitutionNoticeDetailList.FirstOrDefault());
        else
          subtitutionNoticeDetailText = Resources.SomeSubtitutionNoticeFormat(string.Join(Environment.NewLine + "- ", subtitutionNoticeDetailList));
        
        var notice = SimpleTasks.CreateWithNotices(subject, performer);
        notice.ActiveText = Resources.SubtitutionNoticeTextFormat(substitute.Name, subtitutionNoticeDetailText);
        notice.Attachments.Add(substitute);
        notice.Start();
      }
    }
    
    /// <summary>
    /// Запустить обработчик на автоматическое создание замещений.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    public void StartAutoSubstitutionAsync(int personId)
    {
      var task = AsyncHandlers.CreateSubstitutionsForPerson.Create();
      task.personId = personId;
      task.ExecuteAsync();
    }
    
    /// <summary>
    /// Отправить задачу с вложенными сотрудниками.
    /// </summary>
    /// <param name="performer">Исполнитель.</param>
    /// <param name="subject">Тема задачи.</param>
    /// <param name="text">Текст задачи.</param>
    /// <param name="employees">Список сотрудников.</param>
    [Public, Remote]
    public void SendTaskWithEmployees(IRecipient performer, string subject, string text, List<IEmployee> employees)
    {
      var recipients = new[] { performer };
      var task = Sungero.Workflow.SimpleTasks.Create(subject, recipients, employees);
      task.ActiveText = text;
      task.NeedsReview = false;
      task.Start();
    }
    
    /// <summary>
    /// Получить список из ИД сотрудников-совместителей персоны. Первый в списке - основной сотрудник.
    /// </summary>
    /// <param name="person">Персона.</param>
    /// <returns>Список ИД сотрудников персоны.</returns>
    [Public, Remote]
    public List<int> GetPartTimeEmployeeIds(int personId)
    {
      var result = new List<int>();
      var employees = HRSolution.Employees.GetAll(w => w.Person.Id == personId && w.Status == HRSolution.Employee.Status.Active && w.ConsentDirRX == HRSolution.Employee.ConsentDirRX.Signed);
      
      if (employees.Any())
      {
        var employeesWithLogin = employees.Where(w => w.Login != null);

        /*
        Определение основного сотрудника:
        Найти действующих сотрудников одной персоны. В найденном списке найти сотрудников с логином.
        Если найденных сотрудников с логином больше одного, то из этих сотрудников выбрать сотрудника с наименьшим ИД.
        Если сотрудников с логином не найдено, то найти сотрудника без логина с наименьшим ИД.
         */
        var mainEmployee = HRSolution.Employees.Null;
        if (employeesWithLogin.Count() > 0)
          mainEmployee = employeesWithLogin.OrderBy(o => o.Id).FirstOrDefault();
        else
          mainEmployee = employees.OrderBy(o => o.Id).FirstOrDefault();

        result.Add(mainEmployee.Id);
        result.AddRange(employees.Where(w => w.Id != mainEmployee.Id).OrderBy(o => o.BusinessUnitDirRX).Select(s => s.Id));
      }
      return result;
    }
    #endregion
    
  }
}