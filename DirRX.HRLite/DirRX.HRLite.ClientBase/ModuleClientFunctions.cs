using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Commons;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Workflow;
using Aspose.Cells;

namespace DirRX.HRLite.Client
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Отправить сотрудникам на подписание согласие на КЭДО.
    /// </summary>
    public virtual void SendConsentToKEDO()
    {
      var dialog = Dialogs.CreateInputDialog(Resources.ConsentSending);
      // Выбор НОР.
      var businessUnitString = dialog.AddString(Resources.BusinessUnit, false);
      businessUnitString.IsEnabled = false;
      var selectBU = dialog.AddHyperlink(Resources.SelectBusinessUnits);
      // Выбор подразделений.
      var departmentString = dialog.AddString(Resources.Department, false);
      departmentString.IsEnabled = false;
      var selectDep = dialog.AddHyperlink(Resources.SelectDepartments);
      // Выбор сотрудников.
      var employeeString = dialog.AddString(Resources.Employee, false);
      employeeString.IsEnabled = false;
      var selectEmp = dialog.AddHyperlink(Resources.SelectEmployees);
      
      // Выбрать НОР.
      var selectbusinessUnits = new List<HRSolution.IBusinessUnit>();
      selectBU.SetOnExecute(
        () =>
        {
          var businessUnits = HRSolution.BusinessUnits.GetAll(bu => bu.Status == Sungero.CoreEntities.DatabookEntry.Status.Active)
            .Where(bu => bu.UseESSDirRX == true);
          selectbusinessUnits = businessUnits.ShowSelectMany().ToList();
          businessUnitString.Value = string.Join("; ", selectbusinessUnits.Select(bu => bu.Name));
        });
      
      // Выбрать Подразделения.
      var selectDepartments = new List<IDepartment>();
      selectDep.SetOnExecute(
        () =>
        {
          var departments = Departments.GetAll(dep => dep.Status == Sungero.CoreEntities.DatabookEntry.Status.Active
                                               && dep.BusinessUnit != null
                                               && dep.BusinessUnit.Status == Sungero.CoreEntities.DatabookEntry.Status.Active)
            .Where(dep => HRSolution.BusinessUnits.As(dep.BusinessUnit).UseESSDirRX == true).ToList();
          selectDepartments = departments.ShowSelectMany().ToList();
          departmentString.Value = string.Join("; ", selectDepartments.Select(dep => dep.Name));
        });
      
      // Выбрать сотрудников.
      var selectEmployees = new List<HRSolution.IEmployee>();
      selectEmp.SetOnExecute(
        () =>
        {
          var employees = HRSolution.Employees.GetAll(emp => emp.Status == Sungero.CoreEntities.DatabookEntry.Status.Active
                                                      && emp.Department != null
                                                      && emp.Department.Status == Sungero.CoreEntities.DatabookEntry.Status.Active
                                                      && emp.Department.BusinessUnit != null
                                                      && emp.Department.BusinessUnit.Status == Sungero.CoreEntities.DatabookEntry.Status.Active)
            .Where(emp => emp.PersonalAccountStatusDirRX.Value ==  HRSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted).ToList();
          selectEmployees = employees.ShowSelectMany().ToList();
          employeeString.Value = string.Join("; ", selectEmployees.Select(emp => emp.Name));
        });
      
      // Отправить приглашения основным сотрудникам персон в рамках НОР.
      if (dialog.Show() == DialogButtons.Ok)
      {
        var employeesForConsent = HRSolution.Employees.GetAll(w => w.Status == Sungero.Company.Employee.Status.Active
                                                              && (selectEmployees.Contains(w) || selectDepartments.Contains(w.Department)
                                                                  || selectbusinessUnits.Contains(HRSolution.BusinessUnits.As(w.Department.BusinessUnit))))
          .Where(emp => emp.PersonalAccountStatusDirRX.Value ==  HRSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted);
        
        var businessUnitPersons = new List<Structures.Module.BusinessUnitPerson>();
        
        foreach (var employee in employeesForConsent)
        {
          var person = employee.Person;
          var businessUnit = employee.BusinessUnitDirRX;
          
          var businessUnitPerson = Structures.Module.BusinessUnitPerson.Create();
          businessUnitPerson.BusinessUnit = businessUnit;
          businessUnitPerson.Person = person;
          
          if (!businessUnitPersons.Contains(businessUnitPerson))
            businessUnitPersons.Add(businessUnitPerson);
        }
        
        var mainEmployeesForConsentSending = new List<IEmployee>();
        
        foreach (var item in businessUnitPersons)
        {
          var businessUnitMainEmployee = DirRX.EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(item.Person.Id, item.BusinessUnit);
          if (businessUnitMainEmployee != null)
            mainEmployeesForConsentSending.Add(businessUnitMainEmployee);
        }
        
        var asyncHandler = AsyncHandlers.SendConsentToKEDO.Create();
        asyncHandler.employeeIds = string.Join(",", mainEmployeesForConsentSending.Select(s => s.Id));
        asyncHandler.authorId = Users.Current.Id;
        asyncHandler.ExecuteAsync();
        
        Dialogs.NotifyMessage(Resources.ConsentToKEDOSentToAsync);
      }
    }
    
    #region Заявления.
    /// <summary>
    /// Открыть список настроек заявлений.
    /// </summary>
    public virtual void ShowStatementsSettings()
    {
      var statementsSettings = Functions.Module.Remote.GetAllStatementsSettings();
      
      if (!statementsSettings.Any())
      {
        var dialog = Dialogs.CreateConfirmDialog(Resources.StatementsSettingsNotFound, Resources.DefaultSettingsCreatingConfirmationMessage);
        if (dialog.Show())
        {
          var creationErrors = Functions.Module.Remote.CreateDefaultStatementsSettings().Where(w => !string.IsNullOrEmpty(w));
          statementsSettings = Functions.Module.Remote.GetAllStatementsSettings();
          
          if (creationErrors.Any())
          {
            Dialogs.ShowMessage(string.Join(Environment.NewLine, creationErrors), string.Empty, MessageType.Information, DirRX.HRLite.Resources.StatementsSettingCheckMessage);
          }
        }
      }
      
      statementsSettings.Show();
    }
    
    /// <summary>
    /// Открыть список всех заявлений сотрудника.
    /// </summary>
    public virtual void MyStatements()
    {
      var currentUser = Users.Current;
      if (currentUser.IsSystem.HasValue && currentUser.IsSystem.Value)
      {
        Dialogs.ShowMessage(Resources.SystemUserAccessWarning);
      }
      else
      {
        var statements = Functions.Module.Remote.GetEmployeeStatements(currentUser);
        statements.Show();
      }
    }
    
    public virtual void CreateStatementDialog(Sungero.Company.IEmployee employee)
    {
      if (Functions.Module.Remote.CheckCurrentUserCertificates(employee))
      {
        var DocumentKinds = Functions.StatementDocument.Remote.GetStatementDocumentKinds();
        var selectedDocumentKind = DocumentKinds.ShowSelect(Resources.StatementCreation, string.Empty);
        if (selectedDocumentKind != null)
        {
          var statement = Functions.Module.Remote.CreateStatement(selectedDocumentKind, Users.As(employee));
          var task = Functions.Module.Remote.CreateStatementTask(statement);
          task.Show();
        }
      }
      else
      {
        Dialogs.ShowMessage(Resources.CurrentUserCertificatesNotFound);
      }
    }

    /// <summary>
    /// Создать заявление.
    /// </summary>
    public virtual void CreateStatement()
    {
      var currentUser = Users.Current;
      if (currentUser.IsSystem.HasValue && currentUser.IsSystem.Value)
      {
        Dialogs.ShowMessage(Resources.SystemUserAccessWarning);
      }
      else
      {
        var currentEmployee = Employees.Current;
        var employeeIds = HRLite.PublicFunctions.Module.Remote.GetPartTimeEmployeeIds(currentEmployee.Person.Id);
        if (employeeIds.Count() > 1)
        {
          var employeesList = EssPlatformSolution.Employees.GetAll(w => employeeIds.Contains(w.Id)).ToList();
          var defaultJobTitle = string.Empty;
          var defaultOrgName = string.Empty;
          var mainPlaceEmployee = employeesList.FirstOrDefault();
          var defaultEmployee = mainPlaceEmployee;
          
          if (mainPlaceEmployee.JobTitle != null)
            defaultJobTitle = mainPlaceEmployee.JobTitle.DisplayValue;
          
          if (mainPlaceEmployee.BusinessUnitDirRX != null)
            defaultOrgName = mainPlaceEmployee.BusinessUnitDirRX.DisplayValue;
          
          var dialog = Dialogs.CreateInputDialog(DirRX.HRLite.Resources.SelectEmployee);
          var selectedEmployee = dialog.AddSelect(DirRX.HRLite.Resources.EmployeeDialogField, true, defaultEmployee).From(employeesList);
          var jobTitle = dialog.AddString(DirRX.HRLite.Resources.JobTitle, false, defaultJobTitle);
          var orgName = dialog.AddString(DirRX.HRLite.Resources.OrgName, false, defaultOrgName);
          jobTitle.IsEnabled = false;
          orgName.IsEnabled = false;
          dialog.Buttons.AddOkCancel();
          selectedEmployee.SetOnValueChanged((x) =>
                                             {
                                               if (selectedEmployee.Value != null && selectedEmployee.Value.JobTitle != null)
                                                 jobTitle.Value = selectedEmployee.Value.JobTitle.DisplayValue;
                                               else
                                                 jobTitle.Value = string.Empty;
                                               if (selectedEmployee.Value != null && selectedEmployee.Value.BusinessUnitDirRX != null)
                                                 orgName.Value = selectedEmployee.Value.BusinessUnitDirRX.DisplayValue;
                                               else
                                                 orgName.Value = string.Empty;
                                             });
          
          if (dialog.Show() == DialogButtons.Ok)
          {
            this.CreateStatementDialog(selectedEmployee.Value);
          }
        }
        else
        {
          if (HRSolution.Employees.As(currentEmployee).ConsentDirRX == HRSolution.Employee.ConsentDirRX.Signed)
            this.CreateStatementDialog(currentEmployee);
          else
            Dialogs.ShowMessage(HRLite.Resources.ErrorConsentNotSigned);
        }
      }
    }
    #endregion

    #region Импорт оргструктуры.
    /// <summary>
    /// Импортировать Наши организации, Подразделения и Сотрудников из Excel.
    /// </summary>
    public virtual void ImportOrganizationalStructure()
    {
      // Если пользователь не входит в группу Администраторы, ему не доступен импорт оргструктуры.
      if (!Users.Current.IncludedIn(Roles.Administrators))
      {
        Dialogs.ShowMessage(DirRX.HRLite.Resources.UserNotAdministrator, MessageType.Information);
        return;
      }
      
      var dialog = Dialogs.CreateInputDialog(Resources.ImportDataDialogName);
      var importButton = dialog.Buttons.AddCustom(Resources.ButtonImport);
      var cancelButton = dialog.Buttons.AddCancel();
      
      var fileSelector = dialog.AddFileSelect(Resources.FileNameField, true).WithFilter("xls", "xls", new string[] { "xls", "xlsx" });
      var importResult = string.Empty;
      var importVisible = true;
      
      // Обновить диалог.
      Action<CommonLibrary.InputDialogRefreshEventArgs> refresh = (r) =>
      {
        dialog.Text = importResult;
        fileSelector.IsVisible = importVisible;
        importButton.IsVisible = importVisible;
        cancelButton.Name = Resources.ButtonClose;
      };
      
      // Обработать нажатие кнопки.
      Action<CommonLibrary.InputDialogButtonClickEventArgs> buttonClick = (h) =>
      {
        // По кнопке "Импорт" импортировать из выбранного файла.
        if (h.Button == importButton && h.IsValid)
        {
          h.CloseAfterExecute = false;
          var doc = Sungero.Docflow.SimpleDocuments.Create();
          doc.Name = string.Format(Resources.ImportOrgStructureDocument, System.IO.Path.GetFileNameWithoutExtension(fileSelector.Value.Name));
          using (var memory = new System.IO.MemoryStream(fileSelector.Value.Content))
            doc.CreateVersionFrom(memory, System.IO.Path.GetExtension(fileSelector.Value.Name));
          doc.Save();
          
          Logger.Debug("=== Start async import company structure ===");
          var asyncHandler = AsyncHandlers.ImportOrganizationStructure.Create();
          asyncHandler.docId = doc.Id;
          asyncHandler.userId = Users.Current.Id;
          asyncHandler.ExecuteAsync();
          importResult = Resources.OrgStructureImported;
          importVisible = false;
        }
      };
      
      dialog.SetOnRefresh(refresh);
      dialog.SetOnButtonClick(buttonClick);
      dialog.Show();
    }
    #endregion

    #region Информирование.
    /// <summary>
    /// Отправить уведомление.
    /// </summary>
    public virtual void CreateInformingTask()
    {
      if (Employees.Current != null)
      {
        var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
        users.Add(Users.Current);
        if (DirRX.HRManagement.PublicFunctions.Module.Remote.IsHRResponsible(users))
          Functions.Module.Remote.CreateInformingTask().ShowModal();
        else
          Dialogs.ShowMessage(DirRX.HRManagement.Resources.CanSendTaskByRoleFormat(DirRX.HRManagement.Resources.RoleNameHiringResponsible));
      }
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }
    #endregion

    #region Создание документов.
    
    /// <summary>
    /// Общий диалог создания документов.
    /// </summary>
    public virtual void CreateDocument()
    {
      Sungero.Docflow.OfficialDocuments.CreateDocumentWithCreationDialog(PersonnelDocuments.Info,
                                                                         BilaterialDocuments.Info,
                                                                         LocalRegulationDocuments.Info,
                                                                         SimpleDocuments.Info);
    }
    
    #endregion

    #region Администрирование.
    
    /// <summary>
    /// Показать исполнителей ролей сервиса.
    /// </summary>
    public virtual void ShowHRRoles()
    {
      Functions.Module.Remote.GetHRLiteRoles().ShowModal();
    }
    
    #endregion
    
    #region Подписание документов.
    /// <summary>
    /// Утвердить документ.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    /// <param name="documents">Список документов.</param>
    /// <param name="autoComment">Автоматический комментарий.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    /// <param name="needAdvancedSign">Признак необходимости подписывать утверждающей подписью.</param>
    public static void ApproveDocument(IAssignment assignment, List<IOfficialDocument> documents, string autoComment, Sungero.Domain.Client.ExecuteActionArgs eventArgs, bool needAdvancedSign)
    {
      // Если требуется усиленная подпись, то проверить, есть ли сертификаты для подписания документов
      if (needAdvancedSign && !Functions.Module.Remote.HasCertificates(documents))
      {
        eventArgs.AddError(Sungero.Docflow.ApprovalTasks.Resources.CertificateNeededToSign);
        return;
      }
      
      // Добавить автоматический комментарий в качестве комментария к ЭП, если пользователь ничего не указал в тексте задания.
      var activeText = string.IsNullOrWhiteSpace(assignment.ActiveText) ? autoComment : assignment.ActiveText;

      ApproveDocument(assignment.Performer, documents, activeText, eventArgs, needAdvancedSign);
      
    }
    
    /// <summary>
    /// Утвердить документ.
    /// </summary>
    /// <param name="performer">Подписывающий.</param>
    /// <param name="documents">Список документов.</param>
    /// <param name="comment">Комментарий к подписи.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    /// <param name="needAdvancedSign">Признак необходимости подписывать утверждающей подписью.</param>
    public static void ApproveDocument(IUser performer, List<IOfficialDocument> documents, string comment, Sungero.Domain.Client.ExecuteActionArgs eventArgs, bool needAdvancedSign)
    {
      var haveError = false;
      var canApprove = true;
      var validates = Functions.Module.Remote.ValidateBeforeSign(documents);

      var currentEmployee = Employees.Current;
      var employee = Employees.As(performer);
      
      foreach (var validate in validates)
      {
        foreach (var error in validate.Errors)
          eventArgs.AddError(error);
        haveError = validate.Errors.Any();
        if (!validate.CanApprove)
          canApprove = false;
      }
      
      if (haveError)
        return;

      // Подписание утверждающей подписью.
      if (canApprove)
      {
        try
        {
          foreach (var document in documents)
          {
            // Подписать.
            var signatories = new List<Sungero.Docflow.Structures.SignatureSetting.Signatory>();
            signatories = Functions.Module.GetSignatories(document);
            var signatory = signatories.Any(s => Equals(s.EmployeeId, employee.Id)) && signatories.Any(s => Equals(s.EmployeeId, currentEmployee.Id)) ? employee : currentEmployee;
            
            if (!Sungero.Docflow.PublicFunctions.Module.ApproveWithAddenda(document, new List<Sungero.Docflow.IOfficialDocument>(), null, null, false, needAdvancedSign, comment))
              eventArgs.AddError(ApprovalTasks.Resources.ToPerformNeedSignDocument);
          }
        }
        catch (CommonLibrary.Exceptions.PlatformException ex)
        {
          if (!ex.IsInternal)
            eventArgs.AddError(ex.Message.Trim().EndsWith(".") ? ex.Message : string.Format("{0}.", ex.Message));
          else
            throw;
        }
      }
      else if (documents.Where(d => !d.AccessRights.CanApprove()).Any())
        eventArgs.AddError(Sungero.Docflow.ApprovalSigningAssignments.Resources.NoRigthToApproveDocumentForSubstituteFormat(currentEmployee.Name, performer.Name));
      else
        eventArgs.AddError(Sungero.Docflow.Resources.NoRightsToApproveDocument);
    }
    
    /// <summary>
    /// Согласовать документ.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    /// <param name="documents">Список документов.</param>
    /// <param name="autoComment">Комментарий.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    public static void EndorseDocument(IAssignment assignment, List<IOfficialDocument> documents, string autoComment, Sungero.Domain.Client.ExecuteActionArgs eventArgs)
    {
      
      try
      {
        foreach (var document in documents)
        {
          // Добавить в комментарий ЭП результат выполнения задания, если пользователь ничего не указал.
          var activeText = string.IsNullOrWhiteSpace(assignment.ActiveText) ? autoComment : assignment.ActiveText;
          Sungero.Docflow.PublicFunctions.Module.EndorseWithAddenda(document, new List<Sungero.Content.IElectronicDocument>(), null, assignment.Performer, true, activeText);
        }
      }
      catch (CommonLibrary.Exceptions.PlatformException ex)
      {
        if (!ex.IsInternal)
          eventArgs.AddError(ex.Message.EndsWith(".") ? ex.Message : string.Format("{0}.", ex.Message));
        else
          throw;
      }
    }
    #endregion
    
  }
}