using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Workflow;
using ParamKeys = DirRX.HRManagement.Constants.Module.ParamKey;

namespace DirRX.HRManagement.Client
{
  public class ModuleFunctions
  {

    #region Выгрузка документов
    /// <summary>
    /// Показать диалог выгрузки документов.
    /// </summary>
    /// <param name="isVacationDocuments">True, если выгружать будем документы по отпускам, False для прочих.</param>
    public virtual void ShowExportDocumentsDialog(bool isVacationDocuments)
    {
      // Диалог запроса параметров и его свойства
      var exportDialog = Dialogs.CreateInputDialog(Resources.ExportDocuments);
      exportDialog.Height = 240;
      exportDialog.Text = Resources.ExportParamsDialogInstruction;
      var businessUnit = Employees.Current == null ? BusinessUnits.Null : Employees.Current.Department.BusinessUnit;
      var businessUnitSelect = exportDialog.AddSelect<IBusinessUnit>(Resources.BusinessUnitFieldName, true, businessUnit);
      var departmentSelect = exportDialog.AddSelect<IDepartment>(Resources.DeparmentFieldName, false, Departments.Null)
        .From(Functions.Module.Remote.GetFilteredDepartments(businessUnit, false).ToArray());
      var employeeSelect = exportDialog.AddSelect<IEmployee>(Resources.EmployeeFieldName, false, Employees.Null)
        .From(Functions.Module.Remote.GetFilteredEmployees(businessUnit, false).ToArray());
      var documentKindSelect = exportDialog.AddSelectMany<IDocumentKind>(Resources.DocumentKindFieldName, false, DocumentKinds.Null)
        .From(Functions.Module.Remote.GetHRDocumentKinds(isVacationDocuments));
      var beginPeriodSelect = exportDialog.AddDate(isVacationDocuments ? Resources.YearFromFieldName : Resources.PeriodFromFieldName, false);
      var endPeriodSelect = exportDialog.AddDate(isVacationDocuments ? Resources.YearToFieldName : Resources.PeriodToFieldName, false);
      if (isVacationDocuments)
      {
        // Отпускные документы выбирать по году, к которому они относятся
        beginPeriodSelect = beginPeriodSelect.AsYear();
        endPeriodSelect = endPeriodSelect.AsYear();
      }
      var exportFormatSelect = exportDialog.AddSelect(Resources.ExportFormatFieldName, true, Resources.ExportFormatElectronicDocuments)
        .From(Resources.ExportFormatElectronicDocuments, Resources.ExportFormatPaperDocuments);
      
      // Изменение значений свойств
      businessUnitSelect.SetOnValueChanged(arg =>
                                           {
                                             if (!Equals(arg.NewValue, arg.OldValue))
                                             {
                                               if (arg.NewValue == null || (departmentSelect.Value != null && !Equals(arg.NewValue, departmentSelect.Value.BusinessUnit)))
                                                 departmentSelect.Value = Departments.Null;
                                               if (arg.NewValue == null || (employeeSelect.Value != null && !Equals(arg.NewValue, employeeSelect.Value.Department.BusinessUnit)))
                                                 employeeSelect.Value = Employees.Null;
                                               departmentSelect.From(Functions.Module.Remote.GetFilteredDepartments(arg.NewValue, false));
                                               employeeSelect.From(Functions.Module.Remote.GetFilteredEmployees(arg.NewValue, false));
                                             }
                                           });

      departmentSelect.SetOnValueChanged(arg =>
                                         {
                                           if (!Equals(arg.NewValue, arg.OldValue))
                                           {
                                             var departmentList = new List<IDepartment>();
                                             if (arg.NewValue != null)
                                             {
                                               businessUnitSelect.Value = arg.NewValue.BusinessUnit;
                                               departmentList.Add(arg.NewValue);
                                               if (employeeSelect.Value != null && !Equals(arg.NewValue, employeeSelect.Value.Department))
                                                 employeeSelect.Value = Employees.Null;
                                             }
                                             else
                                               employeeSelect.Value = Employees.Null;
                                             employeeSelect.From(Functions.Module.Remote.GetFilteredEmployees(departmentList, false));
                                           }
                                         });
      
      employeeSelect.SetOnValueChanged(arg =>
                                       {
                                         if (!Equals(arg.NewValue, arg.OldValue) && arg.NewValue != null)
                                           departmentSelect.Value = arg.NewValue.Department;
                                       });
      
      // Кнопки
      var nextButton = exportDialog.Buttons.AddCustom(Resources.NextButton);
      var fileListButton = exportDialog.Buttons.AddCustom(Resources.FileListButton);
      fileListButton.IsVisible = false;
      var backButton = exportDialog.Buttons.AddCustom(Resources.BackButton);
      backButton.IsVisible = false;
      var exportButton = exportDialog.Buttons.AddCustom(Resources.ExportButton);
      exportButton.IsVisible = false;
      var closeButton = exportDialog.Buttons.AddCustom(Resources.CloseButton);
      closeButton.IsVisible = false;
      var cancelButton = exportDialog.Buttons.AddCustom(Resources.CancelButton);
      exportDialog.Buttons.Default = nextButton;
      
      // Действия
      var documentCount = 0;
      var docsForExport = new List<Structures.Module.ExportedDocument>();
      var exportRezult = Structures.Module.ExportResult.Create();
      var exportEnabled = false;
      IQueryable<IOfficialDocument> documents = null;
      exportDialog.SetOnButtonClick(
        (h) =>
        {
          h.CloseAfterExecute = h.Button == cancelButton || h.Button == closeButton;
          // Проверить корректность введенного периода
          if (beginPeriodSelect.Value != null && endPeriodSelect.Value != null && beginPeriodSelect.Value > endPeriodSelect.Value)
            h.AddError(isVacationDocuments ? Resources.ExportYearsError : Resources.ExportDatesError);
          // Далее
          if (h.Button == nextButton && h.IsValid)
          {
            businessUnitSelect.IsVisible = false;
            departmentSelect.IsVisible = false;
            employeeSelect.IsVisible = false;
            documentKindSelect.IsVisible = false;
            beginPeriodSelect.IsVisible = false;
            endPeriodSelect.IsVisible = false;
            exportFormatSelect.IsVisible = false;
            nextButton.IsVisible = false;
            fileListButton.IsVisible = true;
            backButton.IsVisible = true;
            exportButton.IsVisible = true;
            // Вызвать функцию поиска по параметрам и вернуть список найденных доков
            documents = Functions.Module.Remote.SearchByRequisites(isVacationDocuments, businessUnitSelect.Value, departmentSelect.Value, employeeSelect.Value,
                                                                   documentKindSelect.Value.ToList(), beginPeriodSelect.Value, endPeriodSelect.Value);
            
            var zipModelFilesExportError = false;
            documentCount = documents.Count();
            if (documentCount > Constants.Module.ExportedDocumentsCountMaxLimit)
            {
              var addErrorMessage = Resources.ExportDialog_Error_DocumentCountLimitFormat(Constants.Module.ExportedDocumentsCountMaxLimit);
              h.AddError(addErrorMessage);
              zipModelFilesExportError = true;
            }
            
            exportEnabled = !zipModelFilesExportError && (documentCount > 0);
            exportDialog.Text = (exportEnabled ? Resources.ExportDialog_SuccessInfo + Environment.NewLine : string.Empty) + Resources.ExportDialog_InstructionFormat(documentCount);
            fileListButton.IsEnabled = documentCount > 0;
            exportButton.IsEnabled = exportEnabled;
            exportDialog.Buttons.Default = exportButton;
          };
          
          // Назад
          if (h.Button == backButton)
          {
            businessUnitSelect.IsVisible = true;
            departmentSelect.IsVisible = true;
            employeeSelect.IsVisible = true;
            documentKindSelect.IsVisible = true;
            beginPeriodSelect.IsVisible = true;
            endPeriodSelect.IsVisible = true;
            exportFormatSelect.IsVisible = true;
            nextButton.IsVisible = true;
            fileListButton.IsVisible = false;
            backButton.IsVisible = false;
            exportButton.IsVisible = false;
            exportDialog.Buttons.Default = nextButton;
            exportDialog.Text = Resources.ExportParamsDialogInstruction;
          }
          
          // Показать документы
          if (h.Button == fileListButton)
          {
            documents.ShowModal();
          }
          
          // Выгрузить
          if (h.Button == exportButton)
          {
            fileListButton.IsVisible = false;
            backButton.IsVisible = false;
            exportButton.IsVisible = false;
            cancelButton.IsVisible = false;
            closeButton.IsVisible = true;
            
            try
            {
              docsForExport = Functions.Module.Remote.PrepareExportHRDocuments(documents.ToList(), exportFormatSelect.Value == Resources.ExportFormatPaperDocuments);
            }
            catch (Exception ex)
            {
              var addErrorMessage = Resources.ExportDialog_Error_Client_NoReason;
              Logger.Error(addErrorMessage, ex);
              h.AddError(addErrorMessage);
              return;
            }
            
            // Сформировать результаты выгрузки и обработать ограничения.
            exportRezult = Functions.Module.Remote.PrepareBodiesAndSignsDocuments(docsForExport);
            var zipModelFilesCount = exportRezult.ZipModels.Count;
            var zipModelFilesSumSize = exportRezult.ZipModels.Sum(m => m.Size);
            var zipModelFilesSumSizeMB = zipModelFilesSumSize / Constants.Module.ConvertMb;

            if (zipModelFilesCount > Constants.Module.ExportedFilesCountMaxLimit)
            {
              var addErrorMessage = Resources.ExportDialog_Error_ExportedFilesLimitFormat(Constants.Module.ExportedFilesCountMaxLimit);
              h.AddError(addErrorMessage);
              return;
            }
            if (zipModelFilesSumSize > Constants.Module.ExportedFilesSizeMaxLimitMb * Constants.Module.ConvertMb)
            {
              var addErrorMessage = Resources.ExportDialog_Error_ExportedSizeLimitFormat(Constants.Module.ExportedFilesSizeMaxLimitMb);
              h.AddError(addErrorMessage);
              return;
            }
            
            IZip zip = null;
            try
            {
              zip = Functions.Module.Remote.CreateZipFromZipModel(exportRezult.ZipModels, exportRezult.ExportedDocuments);
            }
            catch (Exception ex)
            {
              var addErrorMessage = Resources.ExportDialog_Error_Client_NoReason;
              Logger.Error(addErrorMessage, ex);
              h.AddError(addErrorMessage);
              return;
            }
            
            zip.Export();
            // Вывести данные о результатах выгрузки.
            var docWithoutAddendum = exportRezult.ExportedDocuments.Where(d => !d.IsAddendum);
            var successfulExport = docWithoutAddendum.Where(d => !d.IsFaulted);
            var errorDocExport = docWithoutAddendum.Where(d => d.IsFaulted).Count();
            var docExport = successfulExport.Count();
            var docFullComplect = successfulExport.Where(d => d.isComplect && d.NeedConvert).Count();
            var docNoFullComplect = successfulExport.Where(d => !d.isComplect && d.NeedConvert).Count();
            var paperDoc = successfulExport.Where(d => !d.NeedConvert).Count();
            
            exportDialog.Text = Resources.ExportDialog_ResultsInfoFormat(docExport, docFullComplect, docNoFullComplect, paperDoc, errorDocExport);
            closeButton.IsEnabled = true;
            exportDialog.Buttons.Default = closeButton;
          }
        });
      
      exportDialog.Show();
    }
    
    /// <summary>
    /// Выгрузить документы по отпускам.
    /// </summary>
    [Public]
    public virtual void ExportVacationDocuments()
    {
      var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
      users.Add(Users.Current);
      if (Functions.Module.Remote.IsHRAdministrator(users) || Functions.Module.Remote.IsHRDocExportManager(users))
        this.ShowExportDocumentsDialog(true);
      else
        Dialogs.ShowMessage(Resources.CanExportHRDocumentsFormat(Resources.RoleNameHRAdministrator, Resources.RoleNameHRDocExportManager));
    }

    /// <summary>
    /// Выгрузить кадровые документы.
    /// </summary>
    [Public]
    public virtual void ExportDocuments()
    {
      var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
      users.Add(Users.Current);
      if (Functions.Module.Remote.IsHRAdministrator(users) || Functions.Module.Remote.IsHRDocExportManager(users))
        this.ShowExportDocumentsDialog(false);
      else
        Dialogs.ShowMessage(Resources.CanExportHRDocumentsFormat(Resources.RoleNameHRAdministrator, Resources.RoleNameHRDocExportManager));
    }
    #endregion
    
    #region Актуальный график отпусков.
    /// <summary>
    /// Построить актуальный график отпусков.
    /// </summary>
    [Public]
    public virtual void CreateActualVacationReport()
    {
      var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsers.ToList();
      users.Add(Users.Current);
      
      if (!Functions.Module.Remote.IsVacationResponsible(users))
      {
        Dialogs.ShowMessage(Resources.CantCreateActualVacationReportFormat(Resources.RoleNameVacationResponsible));
        return;
      }
      
      var dialog = Dialogs.CreateInputDialog(Resources.ActualShceduleReport);
      dialog.Buttons.Default = Sungero.Core.DialogButtons.Ok;

      var availableDepartments = Functions.Vacation.Remote.GetVisibleDepartments(users);
      var availableBusinessUnits = availableDepartments.Select(d => d.BusinessUnit).Where(bu => bu != null).Distinct().ToList();
      
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(Employees.Current);
      
      var businessUnitSelect = dialog.AddSelect<IBusinessUnit>(Resources.BusinessUnitFieldName, true, businessUnit)
        .From(availableBusinessUnits);
      var departmentSelect = dialog.AddSelectMany<IDepartment>(Resources.DeparmentFieldName, false, Departments.Null)
        .From(availableDepartments.Where(d => Equals(d.BusinessUnit, businessUnit)));
      var employeesDepartment = Functions.Module.Remote.GetFilteredEmployees(businessUnit).Where(emp => availableDepartments.Any(d => emp.Department.Equals(d)));
      var employeeSelect = dialog.AddSelectMany<IEmployee>(Resources.EmployeeFieldName, false, Employees.Null)
        .From(employeesDepartment);
      var yearSelect = dialog.AddDate(Resources.YearFieldName, true, Calendar.Today).AsYear();
      var onlyPaid = dialog.AddBoolean(Resources.OnlyPaidFieldName, true);
      
      businessUnitSelect.SetOnValueChanged(arg =>
                                           {
                                             if (Equals(arg.NewValue, arg.OldValue))
                                               return;
                                             else
                                             {
                                               if (departmentSelect.Value.Any())
                                                 departmentSelect.Value = new List<IDepartment>() { Departments.Null };
                                               if (employeeSelect.Value.Any())
                                                 employeeSelect.Value = new List<IEmployee>() { Employees.Null };
                                             }
                                             if (arg.NewValue == null)
                                               departmentSelect.From(availableDepartments);
                                             else
                                               departmentSelect.From(availableDepartments.Where(d => Equals(d.BusinessUnit, arg.NewValue)));
                                             var employeesDeprartment = Functions.Module.Remote.GetFilteredEmployees(arg.NewValue).Where(em => availableDepartments.Any(d => d.Equals(em.Department)));
                                             employeeSelect.From(employeesDeprartment);
                                           });

      departmentSelect.SetOnValueChanged(arg =>
                                         {
                                           if (Equals(arg.NewValue, arg.OldValue))
                                             return;
                                           else
                                           {
                                             if (employeeSelect.Value.Any())
                                               employeeSelect.Value = new List<IEmployee>() { Employees.Null };
                                             if (arg.NewValue.Any())
                                               employeeSelect.From(Functions.Module.Remote.GetFilteredEmployees(arg.NewValue.ToList()));
                                             else
                                             {
                                               var employeesDeprartment = Functions.Module.Remote.GetFilteredEmployees(businessUnitSelect.Value).Where(em => availableDepartments.Any(d => d.Equals(em.Department)));
                                               employeeSelect.From(employeesDeprartment);
                                             } 
                                           }
                                         });
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        var report = Functions.Module.Remote.BuildActualVacationReportDocument(businessUnitSelect.Value,
                                                                               departmentSelect.Value.ToList(),
                                                                               employeeSelect.Value.ToList(),
                                                                               onlyPaid.Value.Value,
                                                                               yearSelect.Value.Value);
        report.Open();
        Functions.Module.Remote.EnqueueDocumentDeleteJob(Sungero.Docflow.OfficialDocuments.As(report));
      }
    }
    #endregion
    
    #region Настройка системы.
    /// <summary>
    /// Сформировать отчет для проверки настроек системы.
    /// </summary>
    [Public]
    public virtual void CreateSettingsReport()
    {
      var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
      users.Add(Users.Current);
      if (Functions.Module.Remote.IsHRAdministrator(users))
      {
        var report = Functions.Module.Remote.BuildSettingsReport();
        if (report != null)
        {
          report.Open();
          Functions.Module.Remote.EnqueueDocumentDeleteJob(Sungero.Docflow.OfficialDocuments.As(report));
        }
        else
          Dialogs.ShowMessage(Resources.SettingsCorrect);
      }
      else
        Dialogs.ShowMessage(Resources.CanCreateSettingsReportFormat(Resources.RoleNameHRAdministrator));
    }
    
    /// <summary>
    /// Настроить исполнителей ролей по НОР.
    /// </summary>
    [Public]
    public virtual void ConfigureRolesByCompany()
    {
      var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
      users.Add(Users.Current);
      if (!Functions.Module.Remote.IsHRAdministrator(users) || !users.Any(v => v.IncludedIn(Roles.Administrators)))
      {
        Dialogs.ShowMessage(Resources.CanConfigureRolesFormat(Resources.RoleNameHRAdministrator));
        return;
      }
      
      // Диалог выбора НОР.
      var currentBusinessUnit = Employees.Current != null ? Employees.Current.Department.BusinessUnit : BusinessUnits.Null;
      var companyDialog = Dialogs.CreateInputDialog(Resources.RoleConfigurationHeader);
      var companySelected = companyDialog.AddSelect<Sungero.Company.IBusinessUnit>(Resources.BusinessUnitFieldName, true, currentBusinessUnit);
      
      if (companyDialog.Show() != DialogButtons.Ok)
        return;
      
      // Диалог выбора исполнителей ролей.
      var roles = Functions.Module.Remote.GetHRRolesList().OrderBy(r => r.Name);
      var dialog = Dialogs.CreateInputDialog(Resources.RoleConfigurationHeader);
      var selectRecipients = new Dictionary<IRole, ISelectManyDialogValue<IRecipient>>();
      
      var availableValue = Functions.Module.Remote.GetFiltredRecipients(companySelected.Value);
      
      // Создать контролы. Заполнить существующими участниками. Установить фильтр.
      foreach (var role in roles)
      {
        var selectDefaultValue = role.RecipientLinks
          .Select(r => r.Member)
          .Where(r => availableValue.Contains(r))
          .OrderBy(r => r.Name)
          .ToArray();
        var selectItem = dialog
          .AddSelectMany(role.Name, false, selectDefaultValue)
          .From(availableValue);
        selectRecipients.Add(role, selectItem);
      }
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        // Сохранить изменения.
        foreach (var role in roles)
        {
          var anotherMembers = role.RecipientLinks.Select(r => r.Member).Where(r => !availableValue.Contains(r)).ToList();
          var members = selectRecipients[role].Value.Union(anotherMembers).ToList();
          Functions.Module.Remote.SetRecpientsRole(role, members);
        }
      }
    }
    #endregion
    
    #region Отпуска сотрудника.
    /// <summary>
    /// Показать отпуска сотрудника.
    /// </summary>
    [Public]
    public virtual void ShowEmployeeVacation()
    {
      if (Employees.Current != null)
        Dialogs.ShowMessage(string.Format(HRManagement.Resources.Employee, Employees.Current.Name), Functions.Module.Remote.GetVacationFullInfoByEmployee(Employees.Current));
      else
        Dialogs.ShowMessage(HRManagement.Resources.CantSeeVacationsByNonEmployee);
    }
    #endregion
    
    #region Отчет по пересечениям.
    
    /// <summary>
    /// Построить график отпусков за год.
    /// </summary>
    [Public]
    public virtual void ShowVacationScheduleChart()
    {
      if (Employees.Current != null)
        this.VacationScheduleChart(Calendar.Today, Employees.Current);
      else
        this.VacationScheduleChart(Calendar.Today, BusinessUnits.Null, Departments.Null);
    }
    
    /// <summary>
    /// Построить график отпусков за год.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <param name="employee">Работник.</param>
    public void VacationScheduleChart(DateTime year, IEmployee employee)
    {
      year = year != Calendar.SqlMinValue ? year : Calendar.Today;
      var department = Sungero.Company.PublicFunctions.Department.GetDepartment(employee);
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);

      Functions.Module.VacationScheduleChart(year, businessUnit, department);
    }
    
    /// <summary>
    /// Построить график отпусков за год.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <param name="businessUnit">Наша организация.</param>
    /// <param name="department">Подразделение.</param>
    public void VacationScheduleChart(DateTime year, IBusinessUnit businessUnit, IDepartment department)
    {
      var dialog = Dialogs.CreateInputDialog(Resources.VacationChartDialogHeader);
      dialog.Buttons.Default = Sungero.Core.DialogButtons.Ok;
      
      var visibleDepartments = Functions.Vacation.Remote.GetVisibleDepartments();
      var availableBusinessUnits = visibleDepartments.Select(d => d.BusinessUnit).Where(bu => bu != null).Distinct().ToList();
      var availableDepartments = visibleDepartments.Where(d => Equals(d.BusinessUnit, businessUnit)).ToList();
      
      var businessUnitSelect = dialog.AddSelect<IBusinessUnit>(Resources.BusinessUnitFieldName, true, businessUnit)
        .From(availableBusinessUnits);
      var departmentSelect = dialog.AddSelectMany<IDepartment>(Resources.DeparmentFieldName, false, department)
        .From(availableDepartments);
      
      var yearSelect = dialog.AddDate(Resources.YearFieldName, true, year).AsYear();
      var activeOnlySelect = dialog.AddBoolean(DirRX.HRManagement.Resources.EmployeeActiveOnly, true);
      
      businessUnitSelect.SetOnValueChanged(arg =>
                                           {
                                             if (Equals(arg.NewValue, arg.OldValue))
                                               return;
                                             if (departmentSelect.Value.FirstOrDefault() != null && !Equals(arg.NewValue, departmentSelect.Value.FirstOrDefault().BusinessUnit))
                                               departmentSelect.Value = new List<IDepartment>() { Departments.Null };
                                             if (arg.NewValue == null)
                                               availableDepartments = visibleDepartments;
                                             else
                                               availableDepartments = visibleDepartments.Where(d => Equals(d.BusinessUnit, arg.NewValue)).ToList();
                                             departmentSelect.From(availableDepartments);
                                           });
      
      departmentSelect.SetOnValueChanged(arg =>
                                         {
                                           if (!Equals(arg.NewValue, arg.OldValue) && arg.NewValue.FirstOrDefault() != null)
                                             businessUnitSelect.Value = arg.NewValue.First().BusinessUnit;
                                         });
      
      var vacations = new List<IVacation>();
      dialog.SetOnButtonClick((arg) =>
                              {
                                var periodEndNoCalendarError = Sungero.Docflow.PublicFunctions.Module.CheckDateByWorkCalendar(yearSelect.Value.Value);
                                if (!string.IsNullOrWhiteSpace(periodEndNoCalendarError))
                                  arg.AddError(periodEndNoCalendarError, yearSelect);
                                
                                if (departmentSelect.Value.Any())
                                  vacations = Functions.Vacation.Remote.GetBorderPaidVacations(departmentSelect.Value.ToList(), yearSelect.Value.Value, activeOnlySelect.Value.Value)
                                    .ToList();
                                else
                                  vacations = Functions.Vacation.Remote.GetBorderPaidVacations(availableDepartments, yearSelect.Value.Value, activeOnlySelect.Value.Value).ToList();
                                if (!vacations.Any())
                                  arg.AddError(Resources.NoDataToGenerateReport);
                              });
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        var chart = Functions.Module.Remote.BuildApproveVacationScheduleByYear(vacations, yearSelect.Value.Value);
        chart.Open();
        Functions.Module.Remote.EnqueueDocumentDeleteJob(Sungero.Docflow.OfficialDocuments.As(chart));
      }
    }
    
    #endregion
    
    #region Работа с задачами.
    /// <summary>
    /// Создать задачу на планирование по сотруднику.
    /// </summary>
    [Public]
    public virtual void CreateSingleScheduling()
    {
      if (Employees.Current != null)
      {
        var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
        users.Add(Users.Current);
        if (Functions.Module.Remote.IsVacationResponsible(users))
          Functions.Module.Remote.CreateSingleSchedulingTask().Show();
        else
          Dialogs.ShowMessage(Resources.CanSendTaskByRoleFormat(Resources.RoleNameVacationResponsible));
      }
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }
    
    /// <summary>
    /// Отправить задачу на изменения условий труда сотрудника, инициируется руководителем или СОК.
    /// </summary>
    [Public]
    public virtual void StartTransferTaskAsManager()
    {
      if (Employees.Current != null)
      {
        // Получить всех замещаемых текущего пользователя.
        var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
        users.Add(Users.Current);
        // Запуск от имени СОК или его замещающего.
        // Или запуск от имени руководителя или его замещающего.
        if (Functions.Module.Remote.IsHRResponsible(users))
        {
          var task = Functions.Module.Remote.CreateNewTransferTask();
          task.IsHRStarted = true;
          task.Show();
        }
        else if (users.Any(v => Departments.GetAll(d => v.Equals(Users.As(d.Manager))).Any()))
        {
          var task = Functions.Module.Remote.CreateNewTransferTask();
          task.OldDepartment = Employees.Current.Department;
          task.IsManagerStarted = true;
          task.Show();
        }
        else
          Dialogs.ShowMessage(DirRX.HRManagement.Resources.CantSendTaskByManagerErrorFormat(Resources.RoleNameHiringResponsible));
      }
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }

    /// <summary>
    /// Отправить задачу на согласование изменения моих условий труда.
    /// </summary>
    [Public]
    public virtual void StartTransferTaskAsEmployee()
    {
      if (Employees.Current != null)
      {
        var task = Functions.Module.Remote.CreateNewTransferTask();
        task.Employee = Employees.Current;
        task.EmploymentAttachmentGroup.Employees.Add(Employees.Current);
        task.Show();
      }
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }
    
    /// <summary>
    /// Создать задачу на формирование и утверждение графика отпусков.
    /// </summary>
    [Public]
    public virtual void CreateNewVacationScheduleTask()
    {
      if (Employees.Current != null)
      {
        var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
        users.Add(Users.Current);
        if (Functions.Module.Remote.IsVacationResponsible(users))
          Functions.Module.Remote.CreateNewVacationScheduleTask().Show();
        else
          Dialogs.ShowMessage(Resources.CanSendTaskByRoleFormat(Resources.RoleNameVacationResponsible));
      }
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }
    
    /// <summary>
    /// Создать задачу на согласование отпуска.
    /// </summary>
    [Public]
    public virtual void CreateNewVacationApprovalTask()
    {
      if (Employees.Current != null)
        Functions.Module.Remote.CreateNewVacationApprovalTask().Show();
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }
    
    /// <summary>
    /// Создать задачу на перенос отпуска.
    /// </summary>
    [Public]
    public virtual void CreateNewShiftVacationTask()
    {
      if (Employees.Current != null)
        Functions.Module.Remote.CreateNewVacationShiftTask().Show();
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }
    
    /// <summary>
    /// Создать задачу на отзыв из отпуска.
    /// </summary>
    [Public]
    public virtual void CreateNewRecallVacationTask()
    {
      if (Employees.Current != null)
      {
        var task = VacationShiftTasks.As(Functions.Module.Remote.CreateNewVacationShiftTask());
        task.IsVacationRecall = true;
        task.Show();
      }
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }

    /// <summary>
    /// Создать задачу на увольнение.
    /// </summary>
    [Public]
    public virtual void CreateNewDismissalTask()
    {
      if (Employees.Current != null)
        Functions.Module.Remote.CreateNewDismissalTask().Show();
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }

    /// <summary>
    /// Создать задачу на прием.
    /// </summary>
    [Public]
    public virtual void CreateNewHiringTask()
    {
      if (Employees.Current != null)
      {
        var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
        users.Add(Users.Current);
        if (Functions.Module.Remote.IsHRResponsible(users))
          Functions.Module.Remote.CreateNewHiringTask().Show();
        else
          Dialogs.ShowMessage(Resources.CanSendTaskByRoleFormat(Resources.RoleNameHiringResponsible));
      }
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }
    
    /// <summary>
    /// Создать задачу на рассмотрение кандидата.
    /// </summary>
    public virtual void CreateNewRecruitmentTask()
    {
      if (Employees.Current != null)
      {
        var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
        users.Add(Users.Current);
        if (Functions.Module.Remote.IsHRResponsible(users))
          Functions.Module.Remote.CreateNewRecruitmentTask().Show();
        else
          Dialogs.ShowMessage(Resources.CanSendTaskByRoleFormat(Resources.RoleNameHiringResponsible));
      }
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
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
    /// <param name="needAdvancedSign">Признак необходимости утверждающей подписи.</param>
    public static void ApproveDocument(IAssignment assignment, List<IOfficialDocument> documents, string autoComment, Sungero.Domain.Client.ExecuteActionArgs eventArgs, bool needAdvancedSign)
    {
      // Если требуется усиленная подпись, то проверить, есть ли сертификаты для подписания документов
      if (needAdvancedSign && !Functions.Module.Remote.HasCertificates(documents)) {
        eventArgs.AddError(Sungero.Docflow.ApprovalTasks.Resources.CertificateNeededToSign);
        return;
      }
      
      // Добавить в комментарий ЭП результат выполнения задания, если пользователь ничего не указал.
      var activeText = string.IsNullOrWhiteSpace(assignment.ActiveText) ? autoComment : assignment.ActiveText;

      ApproveDocument(assignment.Performer, documents, activeText, eventArgs, needAdvancedSign);
      
    }
    
    /// <summary>
    /// Утвердить документ.
    /// </summary>
    /// <param name="performer">Исполнитель.</param>
    /// <param name="documents">Список документов.</param>
    /// <param name="comment">Комментарий к подписи.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    /// <param name="needAdvancedSign">Признак необходимости утверждающей подписи.</param>
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
            var signatories = new List<Sungero.Docflow.Structures.SignatureSetting.Signatory>();
            if (HROrders.Is(document))
              signatories = Functions.HROrder.Remote.GetSignatories(HROrders.As(document));
            else
              signatories = Functions.HRDocumentBase.Remote.GetSignatories(HRDocumentBases.As(document));

            var signatory = signatories.Any(s => Equals(s.EmployeeId, employee.Id)) && signatories.Any(s => Equals(s.EmployeeId, currentEmployee.Id)) ? employee : currentEmployee;
            
            if (!Sungero.Docflow.PublicFunctions.Module.ApproveWithAddenda(document, new List<Sungero.Docflow.IOfficialDocument>(), null, signatory, false, needAdvancedSign, comment))
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
    /// Подписать уведомление о начале отпуска.
    /// </summary>
    /// <param name="task">Задача на перенос отпуска.</param>
    /// <param name="performer">Пользователь, от чьего имени подписывается документ.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    /// <param name="needAdvancedSign">Признак необходимости утверждающей подписи.</param>
    public virtual void SignVacationNotice(Sungero.Workflow.ITask task, IUser performer, Sungero.Domain.Client.ExecuteActionArgs eventArgs, bool needAdvancedSign)
    {
      var shiftTask = VacationShiftTasks.As(task);
      var appAssignment = shiftTask.ApprovalAssignment;
      if (appAssignment != null)
      {
        var alertTask = VacationAlertTasks.As(appAssignment.Task);
        var vacationNotice = Functions.VacationAlertTask.Remote.GetEmployeeNotice(alertTask);
        var documents = new List<IOfficialDocument>();
        documents.Add(vacationNotice);
        Functions.Module.ApproveDocument(performer, documents, VacationApprovalAssignments.Resources.CommentInVacationNoticeSignature, eventArgs, needAdvancedSign);
      }
    }
    #endregion
    
    #region Прекращение задач.
    /// <summary>
    /// Вывод диалога запроса причины прекращения задачи.
    /// </summary>
    /// <returns>Причина прекращения.</returns>
    [Public]
    public static string GetReasonBeforeAbort()
    {
      var dialog = Dialogs.CreateInputDialog(Sungero.Docflow.ApprovalTasks.Resources.Confirmation);
      var abortingReason = dialog.AddMultilineString(Resources.AbortReason, true);
      dialog.SetOnButtonClick(args =>
                              {
                                if (string.IsNullOrWhiteSpace(abortingReason.Value))
                                  args.AddError(Sungero.Docflow.ApprovalTasks.Resources.EmptyAbortingReason, abortingReason);
                              });
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        return abortingReason.Value;
      }
      return string.Empty;
    }
    #endregion
  }
}