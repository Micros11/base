using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatformSolution.BusinessUnit;

namespace DirRX.EssPlatformSolution
{
  partial class BusinessUnitClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      if (EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected())
      {
        _obj.State.Properties.UseESSDirRX.IsVisible = true;
        _obj.State.Properties.SmsAfterCloseDirRX.IsVisible = true;
        _obj.State.Properties.SmsToOldPhoneDirRX.IsVisible = true;
        _obj.State.Properties.SmsToNewPhoneDirRX.IsVisible = true;
      }
      else
      {
        _obj.State.Properties.SmsAfterCloseDirRX.IsVisible = false;
        _obj.State.Properties.SmsToOldPhoneDirRX.IsVisible = false;
        _obj.State.Properties.SmsToNewPhoneDirRX.IsVisible = false;
        _obj.State.Properties.UseESSDirRX.IsVisible = false;
      }
    }

    public virtual void UseESSDirRXValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      if (e.OldValue.HasValue && e.OldValue.Value && !e.NewValue.Value)
      {
        // Если в данной организации уже есть подключенные к ЛК сотрудники или которым отправлено приглашение в ЛК  - не давать снимать галочку.
        var existEmployeeUseEss = EssPlatformSolution.Employees.GetAll(emp => (emp.PersonalAccountStatusDirRX == EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted
                                                                               || emp.PersonalAccountStatusDirRX == EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteSent)
                                                                       && Equals(emp.Department.BusinessUnit, _obj)).Any();
        
        if (existEmployeeUseEss)
          e.AddError(DirRX.EssPlatformSolution.BusinessUnits.Resources.DisconnectBusinessUnitError);
      }
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      var isEnabledESSSettings = _obj.UseESSDirRX.HasValue && _obj.UseESSDirRX.Value;
      _obj.State.Properties.SmsToOldPhoneDirRX.IsEnabled = isEnabledESSSettings;
      _obj.State.Properties.SmsToOldPhoneDirRX.IsRequired = isEnabledESSSettings;
      _obj.State.Properties.SmsToNewPhoneDirRX.IsEnabled = isEnabledESSSettings;
      _obj.State.Properties.SmsToNewPhoneDirRX.IsRequired = isEnabledESSSettings;
      _obj.State.Properties.SmsAfterCloseDirRX.IsEnabled = isEnabledESSSettings;
      _obj.State.Properties.SmsAfterCloseDirRX.IsRequired = isEnabledESSSettings;
    }

  }
}