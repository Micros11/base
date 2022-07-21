using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatform.EssSetting;

namespace DirRX.EssPlatform
{
  partial class EssSettingClientHandlers
  {

    public virtual void IsUsedIdentityServiceValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      // Если уже есть подключенные к ЛК сотрудники или сотрудники которым отпрапвлено приглашение или подключенные НОР - не давать снимать галочку.
      if (!e.NewValue.Value)
      {
        if (EssPlatformSolution.Employees.GetAll(emp => emp.PersonalAccountStatusDirRX == EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted ||
                                                 emp.PersonalAccountStatusDirRX == EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteSent).Any())
          e.AddError(DirRX.EssPlatform.EssSettings.Resources.DisconnectIdentityServiceError);
        
        if (EssPlatformSolution.BusinessUnits.GetAll(bu => bu.UseESSDirRX == true).Any())
          e.AddError(DirRX.EssPlatform.EssSettings.Resources.DisconnecErrorBusinessUnit);
      }
      
      if (string.IsNullOrEmpty(_obj.AgreementUrl))
        _obj.AgreementUrl = DirRX.EssPlatform.EssSettings.Resources.DefaultAgreementUrl;
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      Functions.EssSetting.SwitchPropertiesAvailability(_obj);
    }

  }
}