using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;
using Sungero.Docflow;
using Sungero.Docflow.ApprovalStage;
using DirRX.HRManagement;
using DismissalConsts = DirRX.HRManagement.Constants.DismissalTask;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;
using HRRoles = DirRX.HRManagement.Constants.Module.HRRoles;
using TaskTypeGuid = DirRX.HRManagement.Constants.Module.TaskTypeGuid;

namespace DirRX.HRManagement.Server
{
  public partial class ModuleInitializer
  {
    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      this.CreateRoles();
      this.CreateDocumentTypes();
      this.CreateDocumentKinds();
      this.GrantRights();
      this.CreateApprovalRule();
      this.CreateVacationKinds();
      this.FillSettingRecord();
    }
    
    #region Создание ролей и выдача прав доступа.
    
    /// <summary>
    /// Создать роли.
    /// </summary>
    public virtual void CreateRoles()
    {
      InitializationLogger.Debug("Init: Create roles.");
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleNameHiringITResponsible, Resources.RoleDescrHiringITResponsible, HRRoles.ITResponsible);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleNameHiringSecurityResponsible, Resources.RoleDescrHiringSecurityResponsible, HRRoles.SecurityResponsible);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleNameHiringSupplyResponsible, Resources.RoleDescrHiringSupplyResponsible, HRRoles.SupplyResponsible);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleNameStaffChief, Resources.RoleDescrStaffChief, HRRoles.StaffChief);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleNameAccounter, Resources.RoleDescrAccounter, HRRoles.Accounter);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleNameVacationResponsible, Resources.RoleDescrVacationResponsible, HRRoles.VacationResponsible);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleNameHRAdministrator, Resources.RoleDescrHRAdministrator, HRRoles.HRAdministrator);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleNameHRDocExportManager, Resources.RoleDescrHRDocExportManager, HRRoles.HRDocExportManager);
      var vacationManagerRole = Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleNameVacationManagers, Resources.RoleDescrVacationManagers, HRRoles.VacationManagers);
      if (!vacationManagerRole.RecipientLinks.Any())
      {
        vacationManagerRole.RecipientLinks.AddNew().Member = Roles.GetAll(g => g.Sid == Sungero.Docflow.PublicConstants.Module.RoleGuid.DepartmentManagersRole).FirstOrDefault();
        vacationManagerRole.Save();
      }
    }
    
    /// <summary>
    /// Выдача прав на объекты модуля.
    /// </summary>
    public virtual void GrantRights()
    {
      #region Выдача прав всем пользователям.
      var allUsers = Roles.AllUsers;
      if (allUsers != null)
      {
        InitializationLogger.Debug("Init: Grant rights to all users.");
        
        // Справочники.
        Vacations.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.FullAccess);
        Vacations.AccessRights.Save();
        HRSettingses.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        HRSettingses.AccessRights.Save();
        VacationDurations.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        VacationDurations.AccessRights.Save();
        PaymentCalendars.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        PaymentCalendars.AccessRights.Save();
        VacationKinds.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        VacationKinds.AccessRights.Save();
        
        // Документы.
        HRDocumentBases.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Create);
        HRDocumentBases.AccessRights.Save();
        
        // Задачи.
        VacationApprovalTasks.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Create);
        VacationApprovalTasks.AccessRights.Save();
        VacationShiftTasks.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Create);
        VacationShiftTasks.AccessRights.Save();
        TransferTasks.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Create);
        TransferTasks.AccessRights.Save();
        DismissalTasks.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Create);
        DismissalTasks.AccessRights.Save();
      }
      #endregion
      
      #region Выдача прав роли "Ответственный за настройку модуля HR-процессы".
      var administratorHR = ModuleFunctions.GetRole(HRRoles.HRSettingsManager);
      if (administratorHR != null)
      {
        InitializationLogger.Debug("Init: Grant rights to HR-processes administrator.");
        
        // Cправочники.
        HRSettingses.AccessRights.Grant(administratorHR, DefaultAccessRightsTypes.FullAccess);
        HRSettingses.AccessRights.Save();
        LNAListses.AccessRights.Grant(administratorHR, DefaultAccessRightsTypes.FullAccess);
        LNAListses.AccessRights.Save();
        VacationDurations.AccessRights.Grant(administratorHR, DefaultAccessRightsTypes.FullAccess);
        VacationDurations.AccessRights.Save();
        PaymentCalendars.AccessRights.Grant(administratorHR, DefaultAccessRightsTypes.FullAccess);
        PaymentCalendars.AccessRights.Save();
        VacationKinds.AccessRights.Grant(administratorHR, DefaultAccessRightsTypes.FullAccess);
        VacationKinds.AccessRights.Save();
      }
      #endregion
      
      #region Выдача прав роли "Специалист по управлению персоналом".
      var respHRRole = ModuleFunctions.GetRole(DirRX.HRLite.PublicConstants.Module.Roles.HRSpecialist);
      if (respHRRole != null)
      {
        InitializationLogger.Debug("Init: Grant rights to HR responsible.");
        
        // Справочники.
        Aspirants.AccessRights.Grant(respHRRole, DefaultAccessRightsTypes.Change);
        Aspirants.AccessRights.Save();
        LNAListses.AccessRights.Grant(respHRRole, DefaultAccessRightsTypes.Change);
        LNAListses.AccessRights.Save();
        TaskSettings.AccessRights.Grant(respHRRole, DefaultAccessRightsTypes.Read);
        TaskSettings.AccessRights.Save();
        Sungero.Company.Employees.AccessRights.Grant(respHRRole, DefaultAccessRightsTypes.Create);
        Sungero.Company.Employees.AccessRights.Save();
        Sungero.Company.Departments.AccessRights.Grant(respHRRole, DefaultAccessRightsTypes.Change);
        Sungero.Company.Departments.AccessRights.Save();
        Sungero.Parties.People.AccessRights.Grant(respHRRole, DefaultAccessRightsTypes.Change);
        Sungero.Parties.People.AccessRights.Save();
        Sungero.CoreEntities.Substitutions.AccessRights.Grant(respHRRole, DefaultAccessRightsTypes.Create);
        Sungero.CoreEntities.Substitutions.AccessRights.Save();
        
        // Документы.
        HROrders.AccessRights.Grant(respHRRole, DefaultAccessRightsTypes.Create);
        HROrders.AccessRights.Save();
        AspirantDocuments.AccessRights.Grant(respHRRole, DefaultAccessRightsTypes.Create);
        AspirantDocuments.AccessRights.Save();
        
        // Задачи.
        HiringTasks.AccessRights.Grant(respHRRole, DefaultAccessRightsTypes.Create);
        HiringTasks.AccessRights.Save();
        RecruitmentTasks.AccessRights.Grant(respHRRole, DefaultAccessRightsTypes.Create);
        RecruitmentTasks.AccessRights.Save();
      }
      #endregion
      
      #region Выдача прав роли "Специалист по работе с графиком отпусков".
      var respVacationRole = ModuleFunctions.GetRole(HRRoles.VacationResponsible);
      if (respVacationRole != null)
      {
        InitializationLogger.Debug("Init: Grant rights to vacations responsible.");
        
        // Документы.
        HROrders.AccessRights.Grant(respVacationRole, DefaultAccessRightsTypes.Create);
        HROrders.AccessRights.Save();
        
        // Задачи.
        VacationSchedulingTasks.AccessRights.Grant(respVacationRole, DefaultAccessRightsTypes.FullAccess);
        VacationSchedulingTasks.AccessRights.Save();
        VacationDepartmentSchedulingTasks.AccessRights.Grant(respVacationRole, DefaultAccessRightsTypes.FullAccess);
        VacationDepartmentSchedulingTasks.AccessRights.Save();
        VacationSingleSchedulingTasks.AccessRights.Grant(respVacationRole, DefaultAccessRightsTypes.Create);
        VacationSingleSchedulingTasks.AccessRights.Save();
        
        // Отчет.
        Reports.AccessRights.Grant(Reports.GetVacationScheduleReport().Info, respVacationRole, DefaultReportAccessRightsTypes.Execute);
      }
      #endregion
      
    }
    
    #endregion
    
    #region Создание видов и типов документов.

    /// <summary>
    /// Создать типы документов для кадрового делопроизводства.
    /// </summary>
    public virtual void CreateDocumentTypes()
    {
      InitializationLogger.Debug("Init: Create document types.");
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(HRManagement.Resources.HRDocumentTypeName, HRDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner, true);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(HRManagement.Resources.HROrderTypeName, HROrder.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner, true);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(HRManagement.Resources.AspirantDocTypeName, AspirantDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner, true);
    }

    /// <summary>
    /// Создать виды документов для кадрового делопроизводства.
    /// </summary>
    public virtual void CreateDocumentKinds()
    {
      InitializationLogger.Debug("Init: Create document kinds.");

      var numerable = Sungero.Docflow.DocumentKind.NumberingType.Numerable;
      var registrable = Sungero.Docflow.DocumentKind.NumberingType.Registrable;
      
      #region Прием.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.EmploymentContractKindName,
                                                                              HRManagement.Resources.EmploymentContractKindName, registrable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.EmploymentContractKind);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.ElectronicInteractionAgreementKindName,
                                                                              HRManagement.Resources.ElectronicInteractionAgreementKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.ElectronicInteractionAgreementKind);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.JoiningStatementKindName,
                                                                              HRManagement.Resources.JoiningStatementKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.JoiningStatementKind);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.ProcPersonalDataAgreementKindName,
                                                                              HRManagement.Resources.ProcPersonalDataAgreementKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.ProcPersonalDataAgreementKind);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.AcquaintanceListKindName,
                                                                              HRManagement.Resources.AcquaintanceListKindName, registrable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.AcquaintanceListKind);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.HiringOrderKindName,
                                                                              HRManagement.Resources.HiringOrderKindName, registrable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HROrder.ClassTypeGuid, null,
                                                                              DocKind.HiringOrderKind);
      #endregion
      
      #region Перевод.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.TransferAdditionalAgreementKindName,
                                                                              HRManagement.Resources.TransferAdditionalAgreementKindShortName, registrable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.TransferAdditionalAgreementKind);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.ChangeWorkConditionsAdditionalAgreementKindName,
                                                                              HRManagement.Resources.ChangeWorkConditionsAdditionalAgreementKindShortName, registrable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.ChangeWorkConditionsAdditionalAgreementKind);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.TransferOrderKindName,
                                                                              HRManagement.Resources.TransferOrderKindName, registrable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HROrder.ClassTypeGuid, null,
                                                                              DocKind.TransferOrderKind);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.ChangeWorkConditionsOrderKindName,
                                                                              HRManagement.Resources.ChangeWorkConditionsOrderKindName, registrable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HROrder.ClassTypeGuid, null,
                                                                              DocKind.ChangeWorkConditionsOrderKind);
      #endregion
      
      #region Увольнение.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.DismissalOrderKindName,
                                                                              HRManagement.Resources.DismissalOrderKindName, registrable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HROrder.ClassTypeGuid, null,
                                                                              DocKind.DismissalOrderKind);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.DismissalStatementKindName,
                                                                              HRManagement.Resources.DismissalStatementKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.DismissalStatementKind);
      #endregion
      
      #region Отпуск.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.VacationOrderKindName,
                                                                              HRManagement.Resources.VacationOrderKindName, registrable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HROrder.ClassTypeGuid, null,
                                                                              DocKind.VacationOrderKind);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.VacationShiftOrderKindName,
                                                                              HRManagement.Resources.VacationShiftOrderKindName, registrable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HROrder.ClassTypeGuid, null,
                                                                              DocKind.VacationShiftOrderKind);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.ScheduledVacationNoticeKindName,
                                                                              HRManagement.Resources.ScheduledVacationNoticeKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.ScheduledVacationNoticeKind);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.VacationStatementKindName,
                                                                              HRManagement.Resources.VacationStatementKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.VacationStatementKind);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.VacationShiftStatementKindName,
                                                                              HRManagement.Resources.VacationShiftStatementKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.VacationShiftStatementKind);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.ScheduleAcquaintanceListKindName,
                                                                              HRManagement.Resources.ScheduleAcquaintanceListKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.VacationScheduleAcquaintanceListKind);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.VacationRecallStatementKindName,
                                                                              HRManagement.Resources.VacationRecallStatementKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.VacationRecallStatementKind);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.VacationRecallOrderKindName,
                                                                              HRManagement.Resources.VacationRecallOrderKindName, registrable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HROrder.ClassTypeGuid, null,
                                                                              DocKind.VacationRecallOrderKind);
      #endregion
      
      #region Подбор.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.AspirantReportKindName,
                                                                              HRManagement.Resources.AspirantReportKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, AspirantDocument.ClassTypeGuid, null,
                                                                              DocKind.AspirantReportKindName);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.OtherAspirantDocKindName,
                                                                              HRManagement.Resources.OtherAspirantDocKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, false, false, AspirantDocument.ClassTypeGuid, null,
                                                                              DocKind.OtherAspirantDoctKindName);
      #endregion
      
      #region Прочие.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.BenefitStatementKindName,
                                                                              HRManagement.Resources.BenefitStatementKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.BenefitStatementKind);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.MaterialAidStatementKindName,
                                                                              HRManagement.Resources.MaterialAidStatementKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.MaterialAidStatementKind);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.ChangePersonalDataStatementKindName,
                                                                              HRManagement.Resources.ChangePersonalDataStatementKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.ChangePersonalDataStatementKind);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(HRManagement.Resources.VacationScheduleKindName,
                                                                              HRManagement.Resources.VacationScheduleKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, HRDocument.ClassTypeGuid, null,
                                                                              DocKind.VacationScheduleKind);
      #endregion
      
    }

    #endregion
    
    #region Создание правила согласования и этапов по умолчанию.
    
    /// <summary>
    /// Создать правила по умолчанию для прочих заявлений.
    /// </summary>
    public virtual void CreateApprovalRule()
    {
      this.CreateChangePersonalDataApprovalRule();
      this.CreateMaterialPaymentsApprovalRule();
    }
    
    /// <summary>
    /// Создать этап согласования.
    /// </summary>
    /// <param name="stageName">Имя этапа.</param>
    /// <param name="stageSubject">Тема этапа.</param>
    /// <param name="recipients">Исполнители.</param>
    /// <returns>Этап согласования.</returns>
    public virtual IApprovalStage CreateSimpleAssignmentStage(string stageName, string stageSubject, List<IRole> recipients)
    {
      var stage = ApprovalStages.Create();
      stage.StageType = StageType.SimpleAgr;
      stage.Name = stageName;
      stage.Subject = stageSubject;
      stage.DeadlineInDays = 1;
      foreach (var recipient in recipients)
        stage.Recipients.AddNew().Recipient = recipient;
      stage.Save();
      return stage;
    }
    
    /// <summary>
    /// Создать правило по умолчанию.
    /// </summary>
    /// <param name="ruleName">Имя правила.</param>
    /// <returns>Правило по умолчанию.</returns>
    public virtual IApprovalRule CreateDefaultRule(string ruleName)
    {
      var rule = Sungero.Docflow.ApprovalRules.Create();
      rule.Status = Sungero.Docflow.ApprovalRuleBase.Status.Active;
      rule.Name = ruleName;
      rule.DocumentFlow = Sungero.Docflow.ApprovalRuleBase.DocumentFlow.Inner;
      rule.IsDefaultRule = true;
      return rule;
    }
    
    /// <summary>
    /// Создать правило согласования для заявлений на изменение персональных данных.
    /// </summary>
    public virtual void CreateChangePersonalDataApprovalRule()
    {
      InitializationLogger.Debug("Init: Create сhange personal data approval rule.");
      
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(DocKind.ChangePersonalDataStatementKind);
      var hasDefaultRule = ApprovalRuleBases.GetAll().Any(r => r.DocumentFlow == Sungero.Docflow.ApprovalRuleBase.DocumentFlow.Inner &&
                                                          r.DocumentKinds.Any(d => Equals(d.DocumentKind, docKind)));
      if (hasDefaultRule)
        return;
      
      var rule = this.CreateDefaultRule(Resources.ChangePersonalDataRuleName);
      rule.DocumentKinds.AddNew().DocumentKind = docKind;
      
      var roles = new List<IRole>();
      roles.Add(Roles.GetAll(r => r.Sid == DirRX.HRLite.PublicConstants.Module.Roles.HRSpecialist).FirstOrDefault());
      var stage = this.CreateSimpleAssignmentStage(Resources.GetEmployeeDocStageName, Resources.GetEmployeeDocStageSubject, roles);
      rule.Stages.AddNew().Stage = stage;
      
      roles.Clear();
      roles.Add(Roles.GetAll(r => r.Sid == HRRoles.ITResponsible).FirstOrDefault());
      roles.Add(Roles.GetAll(r => r.Sid == HRRoles.Accounter).FirstOrDefault());
      stage = this.CreateSimpleAssignmentStage(Resources.ChangeEmployeeDataStageName, Resources.ChangeEmployeeDataStageSubject, roles);
      rule.Stages.AddNew().Stage = stage;
      Sungero.Docflow.PublicFunctions.ApprovalRuleBase.CreateAutoTransitions(rule);
      rule.Save();

    }
    
    /// <summary>
    /// Создать правило согласования для заявлений с денежными выплатами.
    /// </summary>
    public virtual void CreateMaterialPaymentsApprovalRule()
    {
      InitializationLogger.Debug("Init: Create material payments approval rule.");
      var docKinds = Functions.Module.GetMaterialStatementDocKind();
      var hasDefaultRule = ApprovalRuleBases.GetAll().Any(r => r.DocumentFlow == Sungero.Docflow.ApprovalRuleBase.DocumentFlow.Inner &&
                                                          r.DocumentKinds.Any(d => docKinds.Contains(d.DocumentKind)));
      if (hasDefaultRule)
        return;
      
      var rule = this.CreateDefaultRule(Resources.MaterialPaymentsRuleName);
      foreach (var docKind in docKinds)
        rule.DocumentKinds.AddNew().DocumentKind = docKind;
      
      var stages = new List<Enumeration> { StageType.Manager };
      Sungero.Docflow.PublicInitializationFunctions.Module.SetRuleStages(rule, stages);
      
      var roles = new List<IRole>();
      roles.Add(Roles.GetAll(r => r.Sid == HRRoles.Accounter).FirstOrDefault());
      var stage = this.CreateSimpleAssignmentStage(Resources.MaterialPaymentStageName, Resources.MaterialPaymentStageSubject, roles);
      rule.Stages.AddNew().Stage = stage;
      
      Sungero.Docflow.PublicFunctions.ApprovalRuleBase.CreateAutoTransitions(rule);
      rule.Save();

    }
    #endregion
    
    /// <summary>
    /// Заполнить записи настроечного справочника.
    /// </summary>
    public virtual void FillSettingRecord()
    {
      // Задать список соответствия видов документов и шаблонов.
      var kindTemplatePairs = new Dictionary<Guid, string>();
      kindTemplatePairs.Add(DocKind.EmploymentContractKind, "Шаблон трудового договора");
      kindTemplatePairs.Add(DocKind.ElectronicInteractionAgreementKind, "Шаблон соглашения об электронном взаимодействии");
      kindTemplatePairs.Add(DocKind.JoiningStatementKind, "Шаблон заявления о присоединении к электронному взаимодействию");
      kindTemplatePairs.Add(DocKind.ProcPersonalDataAgreementKind, "Шаблон согласия на обработку персональных данных");
      kindTemplatePairs.Add(DocKind.HiringOrderKind, "Шаблон приказа о приеме");
      kindTemplatePairs.Add(DocKind.TransferAdditionalAgreementKind, "Шаблон доп. соглашения при переводе в другое подразделение");
      kindTemplatePairs.Add(DocKind.ChangeWorkConditionsAdditionalAgreementKind, "Шаблон доп. соглашения при изменении условий труда");
      kindTemplatePairs.Add(DocKind.TransferOrderKind, "Шаблон приказа о переводе");
      kindTemplatePairs.Add(DocKind.ChangeWorkConditionsOrderKind, "Шаблон приказа об изменении условий труда");
      kindTemplatePairs.Add(DocKind.DismissalOrderKind, "Шаблон приказа об увольнении");
      kindTemplatePairs.Add(DocKind.DismissalStatementKind, "Шаблон заявления на увольнение");
      kindTemplatePairs.Add(DocKind.VacationOrderKind, "Шаблон приказа об отпуске");
      kindTemplatePairs.Add(DocKind.VacationShiftOrderKind, "Шаблон приказа о переносе отпуска");
      kindTemplatePairs.Add(DocKind.ScheduledVacationNoticeKind, "Шаблон уведомления о начале отпуска");
      kindTemplatePairs.Add(DocKind.VacationStatementKind, "Шаблон заявления на отпуск");
      kindTemplatePairs.Add(DocKind.VacationShiftStatementKind, "Шаблон заявления на перенос отпуска");
      kindTemplatePairs.Add(DocKind.BenefitStatementKind, "Шаблон заявления на выплату пособия");
      kindTemplatePairs.Add(DocKind.MaterialAidStatementKind, "Шаблон заявления на выплату материальной помощи");
      kindTemplatePairs.Add(DocKind.ChangePersonalDataStatementKind, "Шаблон заявления на изменение персональных данных");
      kindTemplatePairs.Add(DocKind.VacationRecallStatementKind, "Шаблон заявления на отзыв из отпуска");
      kindTemplatePairs.Add(DocKind.VacationRecallOrderKind, "Шаблон приказа об отзыве из отпуска");
      
      var settings = HRSettingses.GetAll();
      if (!settings.Any())
      {
        var newRecord = HRSettingses.Create();
        
        // Заполнить настройки значениями по умолчанию.
        newRecord.MinDaysForVacationApproval = 7;
        newRecord.PeoplePercentage = 20;
        newRecord.DaysCountVacationPay = 4;
        newRecord.DaysCountBeforeVacationAlert = 14;
        newRecord.RequiredOnePartDuration = 14;
        newRecord.MainVacationKind = VacationKinds.GetAll(k => k.Sid == Constants.VacationKind.Paid).FirstOrDefault();
        newRecord.VacationCheckDuration = 7;
        newRecord.VacationPlaningDuration = 14;
        newRecord.VacationScheduleAcquaintance = 7;
        newRecord.MonthOfVacationYearBorder = DirRX.HRManagement.HRSettings.MonthOfVacationYearBorder.March;
        newRecord.OriginalDocInStaff = DirRX.HRManagement.HRSettings.OriginalDocInStaff.ElectronicDoc;
        newRecord.OriginalDocInVacation = DirRX.HRManagement.HRSettings.OriginalDocInVacation.ElectronicDoc;
        newRecord.IsScheduleUsed = true;
        
        foreach (var docKind in Sungero.Docflow.DocumentKinds.GetAll())
        {
          var docKindGuid = Functions.Module.GetDocumentKindGuid(docKind);
          if (kindTemplatePairs.ContainsKey(docKindGuid))
          {
            // Добавить новую строку в табличную часть и заполнить поля.
            var newRow = newRecord.TemplateSettings.AddNew();
            newRow.DocKind = docKind;
            newRow.Template = Sungero.Content.ElectronicDocumentTemplates.GetAll(l => l.Name == kindTemplatePairs[docKindGuid]).FirstOrDefault();
          }
        }
        newRecord.Save();
      }
      else
      {
        foreach (var setting in settings)
        {
          foreach (var kindTemplatePair in kindTemplatePairs)
          {
            var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(kindTemplatePair.Key);
            
            // Добавить новую строку в табличную часть и заполнить поля, если такого вида документа нет.
            // Или заполнить существующую строку, если вид документа есть, но шаблон не заполнен.
            if (!setting.TemplateSettings.Any(ts => ts.DocKind.Id == docKind.Id))
            {
              var newRow = setting.TemplateSettings.AddNew();
              newRow.DocKind = docKind;
              newRow.Template = Sungero.Content.ElectronicDocumentTemplates.GetAll(t => t.Name == kindTemplatePair.Value).FirstOrDefault();
            }
            else if (setting.TemplateSettings.Any(ts => ts.DocKind.Id == docKind.Id && ts.Template == null))
            {
              var currentSetting = setting.TemplateSettings.Where(ts => ts.DocKind.Id == docKind.Id).FirstOrDefault();
              currentSetting.Template = Sungero.Content.ElectronicDocumentTemplates.GetAll(t => t.Name == kindTemplatePair.Value).FirstOrDefault();
            }
          }
          
          // Заполнить пустые настройки значениями по умолчанию.
          if (setting.MinDaysForVacationApproval == null)
            setting.MinDaysForVacationApproval = 7;
          if (setting.PeoplePercentage == null)
            setting.PeoplePercentage = 20;
          if (setting.DaysCountVacationPay == null)
            setting.DaysCountVacationPay = 4;
          if (setting.DaysCountBeforeVacationAlert == null)
            setting.DaysCountBeforeVacationAlert = 14;
          if (setting.RequiredOnePartDuration == null)
            setting.RequiredOnePartDuration = 14;
          if (setting.MainVacationKind == null)
            setting.MainVacationKind = VacationKinds.GetAll(k => k.Sid == Constants.VacationKind.Paid).FirstOrDefault();
          if (setting.VacationCheckDuration == null)
            setting.VacationCheckDuration = 7;
          if (setting.VacationPlaningDuration == null)
            setting.VacationPlaningDuration = 14;
          if (setting.VacationScheduleAcquaintance == null)
            setting.VacationScheduleAcquaintance = 7;
          if (setting.MonthOfVacationYearBorder == null)
            setting.MonthOfVacationYearBorder = DirRX.HRManagement.HRSettings.MonthOfVacationYearBorder.March;
          if (setting.OriginalDocInStaff == null)
            setting.OriginalDocInStaff = DirRX.HRManagement.HRSettings.OriginalDocInStaff.ElectronicDoc;
          if (setting.OriginalDocInVacation == null)
            setting.OriginalDocInVacation = DirRX.HRManagement.HRSettings.OriginalDocInVacation.ElectronicDoc;
          if (setting.IsScheduleUsed == null)
            setting.IsScheduleUsed = true;
          setting.Save();
        }
      }
    }
    
    /// <summary>
    /// Создать записи видов отпуска.
    /// </summary>
    public virtual void CreateVacationKinds()
    {
      this.CreateVacationKind(VacationKinds.Resources.PaidVacationKindName, VacationKinds.Resources.PaidVacationShortKindName,
                         Constants.VacationKind.Paid, true, true);
      this.CreateVacationKind(VacationKinds.Resources.VacationWithoutPayKindName, VacationKinds.Resources.VacationWithoutPayShortKindName,
                         Constants.VacationKind.WithoutPay, false, false);
    }
    
    /// <summary>
    /// Создать запись вида отпуска.
    /// </summary>
    /// <param name="kindName">Имя.</param>
    /// <param name="shortName">Сокращенное имя.</param>
    /// <param name="sid">Уникальный ИД, регистрозависимый.</param>
    /// <param name="isRequired">True, если обязательный.</param>
    /// <param name="isPaid">True, если оплачиваемый.</param>
    public virtual void CreateVacationKind(string kindName, string shortName, string sid, bool isRequired, bool isPaid)
    {
      if (!VacationKinds.GetAll(n => Equals(n.Sid, sid)).Any())
      {
        var vacKind = VacationKinds.Create();
        vacKind.Sid = sid;
        vacKind.Name = kindName;
        vacKind.ShortName = shortName;
        vacKind.Required = isRequired;
        vacKind.Paid = isPaid;
        vacKind.Save();
      }
    }
  }
}
