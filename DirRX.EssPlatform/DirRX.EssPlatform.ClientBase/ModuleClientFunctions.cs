using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Aspose.Cells;

namespace DirRX.EssPlatform.Client
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Показать роли модуля.
    /// </summary>
    public virtual void ShowEssRoles()
    {
      Functions.Module.Remote.GetEssRoles().ShowModal();
    }
    
    /// <summary>
    /// Показать настройки взаимодействия с ЛК.
    /// </summary>
    public virtual void ShowEssSettings()
    {
      var essSettings = PublicFunctions.EssSetting.GetSettings();
      essSettings.Show();
    }
    
    /// <summary>
    /// Подключение пользователей к сервису.
    /// </summary>
    [Public]
    public virtual void InviteUsers()
    {
      // Если пользователь не входит в группу Администраторы, ему не доступна отправка приглашений.
      if (!Users.Current.IncludedIn(Roles.Administrators))
      {
        Dialogs.ShowMessage(DirRX.EssPlatform.Resources.UserNotAdministrator, MessageType.Information);
        return;
      }
      
      // Если отключена настройка "Подключить сервисы личного кабинета", то последующие проверки выполняться не будут.
      if (!EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected())
      {
        Dialogs.ShowMessage(DirRX.EssPlatform.Resources.NoEssSettings, MessageType.Error);
        return;
      }
      
      var dialog = Dialogs.CreateInputDialog(Resources.BulkSendInvantions);
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
      
      // Настроить фильтрации.
      var businessUnitIds = new List<string>();
      var departmentIds = new List<string>();
      var employeeIds = new List<string>();
      
      // Выбрать НОР. Если НОР изменились, очистить Подразделения и Сотрудников.
      selectBU.SetOnExecute(
        () =>
        {
          var businessUnits = EssPlatformSolution.BusinessUnits.GetAll(bu => bu.Status == Sungero.CoreEntities.DatabookEntry.Status.Active
                                                                       && bu.UseESSDirRX.HasValue
                                                                       && bu.UseESSDirRX.Value);
          var selectbusinessUnits = businessUnits.ShowSelectMany();
          businessUnitString.Value = string.Join("; ", selectbusinessUnits.Select(bu => bu.Name));
          businessUnitIds.Clear();
          businessUnitIds.AddRange(selectbusinessUnits.Select(bu => bu.Id.ToString()));
        });
      
      businessUnitString.SetOnValueChanged(
        e =>
        {
          if (e.OldValue != e.NewValue)
          {
            departmentString.Value = string.Empty;
            departmentIds.Clear();
            employeeString.Value = string.Empty;
            employeeIds.Clear();
          }
        });
      
      // Выбрать Подразделения. Если подразделения изменились, очистить Сотрудников.
      selectDep.SetOnExecute(
        () =>
        {
          var departments = Departments.GetAll(dep => dep.Status == Sungero.CoreEntities.DatabookEntry.Status.Active
                                               && dep.BusinessUnit != null
                                               && dep.BusinessUnit.Status == Sungero.CoreEntities.DatabookEntry.Status.Active
                                               && EssPlatformSolution.BusinessUnits.As(dep.BusinessUnit).UseESSDirRX.HasValue
                                               && EssPlatformSolution.BusinessUnits.As(dep.BusinessUnit).UseESSDirRX.Value).ToList();
          var selectDepartments = departments.ShowSelectMany();
          departmentString.Value = string.Join("; ", selectDepartments.Select(dep => dep.Name));
          departmentIds.Clear();
          departmentIds.AddRange(selectDepartments.Select(dep => dep.Id.ToString()));
        });
      
      departmentString.SetOnValueChanged(
        e =>
        {
          if (e.OldValue != e.NewValue)
          {
            employeeString.Value = string.Empty;
            employeeIds.Clear();
          }
        });
      
      // Выбрать сотрудников.
      selectEmp.SetOnExecute(
        () =>
        {
          var employees = EssPlatformSolution.Employees.GetAll(emp => emp.Status == Sungero.CoreEntities.DatabookEntry.Status.Active
                                                               && emp.Department != null
                                                               && emp.Department.Status == Sungero.CoreEntities.DatabookEntry.Status.Active
                                                               && emp.Department.BusinessUnit != null
                                                               && emp.Department.BusinessUnit.Status == Sungero.CoreEntities.DatabookEntry.Status.Active
                                                               && EssPlatformSolution.BusinessUnits.As(emp.Department.BusinessUnit).UseESSDirRX.HasValue
                                                               && EssPlatformSolution.BusinessUnits.As(emp.Department.BusinessUnit).UseESSDirRX.Value).ToList();
          var selectEmployees = employees.ShowSelectMany();
          employeeString.Value = string.Join("; ", selectEmployees.Select(emp => emp.Name));
          
          employeeIds.Clear();
          employeeIds.AddRange(selectEmployees.Select(emp => emp.Id.ToString()));
        });
      
      // Отправить приглашения.
      if (dialog.Show() == DialogButtons.Ok)
      {
          var asyncHandler = AsyncHandlers.ActivateESSUsers.Create();
          asyncHandler.businessUnitIds = string.Join(",", businessUnitIds.ToArray());
          asyncHandler.departmentIds = string.Join(",", departmentIds.ToArray());
          asyncHandler.employeeIds = string.Join(",", employeeIds.ToArray());         
          asyncHandler.userId = Users.Current.Id;                                               
          asyncHandler.ExecuteAsync();
          
          Dialogs.NotifyMessage(Resources.ESSInviteSentToAsync);
      }
    }
  }
}