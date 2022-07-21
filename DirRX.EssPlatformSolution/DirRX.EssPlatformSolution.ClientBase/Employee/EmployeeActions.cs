using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatformSolution.Employee;

namespace DirRX.EssPlatformSolution.Client
{
  partial class EmployeeActions
  {
    public virtual void DeleteEssUserDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if(_obj.State.IsChanged)
      {
        e.AddError(DirRX.EssPlatformSolution.Employees.Resources.SaveBeforeDeleteESSUser);
        return;
      }
      
      if (!EssPlatform.PublicFunctions.Module.Remote.IsSingleActivePersonEmployee(_obj, _obj.Status))
      {
        var dialog = Dialogs.CreateTaskDialog(DirRX.EssPlatformSolution.Employees.Resources.PersonHasSeveralActiveEmployeesFormat(_obj.Person.Name), MessageType.Warning);
        var ok = dialog.Buttons.AddOk();
        dialog.Buttons.AddCancel();
        dialog.Buttons.Default = ok;
        if (dialog.Show() == DialogButtons.Cancel)
          return;
      }
      
      EssPlatform.PublicFunctions.Module.Remote.DisconnectEssUser(_obj);
      _obj.PersonalAccountStatusDirRX = EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent;
      _obj.Save();
    }

    public virtual bool CanDeleteEssUserDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Действие доступно если:
      //   - у пользователя есть права
      //   - приглашение ему отправлялось
      //   - выбрано Подключить сервисы личного кабинета
      return _obj.AccessRights.CanCreateEssUsersDirRX() &&
        _obj.PersonalAccountStatusDirRX.HasValue &&
        _obj.PersonalAccountStatusDirRX.Value != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent &&
        EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected();
    }

    public virtual void ForceSaveDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      e.Params.AddOrUpdate(EssPlatform.Resources.ParameterIsForceSaveFormat(_obj.Id), true);
      _obj.Save();
    }

    public virtual bool CanForceSaveDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void CreateESSUserDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if(_obj.State.IsChanged)
      {
        e.AddError(DirRX.EssPlatformSolution.Employees.Resources.SaveBeforeCreateESSUser);
        return;
      }
      
      //Отправить приглашение сотруднику можно только при закрытой карточке настроек личного кабинета
      var settings = EssPlatform.PublicFunctions.EssSetting.GetSettings();
      var isLocked = Locks.GetLockInfo(settings).IsLocked;
      if(isLocked)
      {
        e.AddError(DirRX.EssPlatformSolution.Employees.Resources.CloseEssSettings);
        return;
      }
      
      // Сотрудника без НОР или из закрытой НОР к ЛК подключить нельзя.
      // Отправлять приглашение в ЛК можно только сотруднику, организация которого подключена к ЛК.
      var canCreateEssUser = false;
      var businessUnit = _obj.Department.BusinessUnit;
      if (businessUnit != null && businessUnit.Status.Value == Sungero.Company.BusinessUnit.Status.Active)
      {
        var businessUnitO = EssPlatformSolution.BusinessUnits.As(businessUnit);
        canCreateEssUser = businessUnitO.UseESSDirRX.HasValue && businessUnitO.UseESSDirRX.Value;
      }
      if (!canCreateEssUser)
      {
        e.AddError(DirRX.EssPlatformSolution.Employees.Resources.BusinessUnitOfEmployeeIsNotUseEssError);
        return;
      }
      // Проверить, что указан Личный телефон.
      if (string.IsNullOrEmpty(_obj.PersonalPhoneDirRX))
      {
        e.AddError(DirRX.EssPlatformSolution.Employees.Resources.PersonalPhoneRequired);
        return;
      }
      
      try
      {
        EssPlatform.PublicFunctions.Module.Remote.ActivateESSUser(_obj);
        e.Params.AddOrUpdate(EssPlatform.Resources.ParameterOldPhoneEmpForUpdateFormat(_obj.Id), _obj.PersonalPhoneDirRX);
        Dialogs.NotifyMessage(EssPlatform.Resources.ESSInviteSent);
        if (_obj.PersonalAccountStatusDirRX != EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted)
        {
          _obj.PersonalAccountStatusDirRX = EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteSent;
          _obj.SmsNotifDirRX = true;
          _obj.Save();
        }
        else
          Dialogs.NotifyMessage(EssPlatform.Resources.ErrorESSInvite);
        
        var partTimeEmployees = Employees.GetAll(emp => emp.Status == Sungero.Company.Employee.Status.Active && emp.Person.Equals(_obj.Person) && emp.Id != _obj.Id);
        foreach (var partTimeEmployee in partTimeEmployees)
        {
          if (partTimeEmployee.PersonalAccountStatusDirRX != EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted)
          {
            partTimeEmployee.PersonalAccountStatusDirRX = EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteSent;
            partTimeEmployee.SmsNotifDirRX = true;
            partTimeEmployee.Save();
          }
        }
      }
      catch (Exception ex)
      {
        throw AppliedCodeException.Create(EssPlatform.Resources.ActivateEssUserErrorFormat(ex.Message));
      }
    }

    public virtual bool CanCreateESSUserDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Действие доступно если:
      //   - у пользователя есть права
      //   - сотрудник действующий и приглашение ему еще не отправлено
      //   - выбрано Подключить сервисы личного кабинета
      return _obj.AccessRights.CanCreateEssUsersDirRX() &&
        _obj.Status.Value == Sungero.Company.Employee.Status.Active &&
        _obj.PersonalAccountStatusDirRX.HasValue &&
        _obj.PersonalAccountStatusDirRX.Value == DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent &&
        !_obj.State.IsInserted && EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected();
    }

  }

}