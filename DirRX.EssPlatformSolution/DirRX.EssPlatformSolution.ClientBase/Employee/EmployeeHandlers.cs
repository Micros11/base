using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatformSolution.Employee;

namespace DirRX.EssPlatformSolution
{
  partial class EmployeeClientHandlers
  {

    public virtual void EmailNotifDirRXValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      e.AddInformation(DirRX.EssPlatformSolution.Employees.Resources.PropertyChangedFormat(_obj.Info.Properties.EmailNotifDirRX.LocalizedName, _obj.Person.Name));
    }

    public virtual void PersonalAccountStatusDirRXValueInput(Sungero.Presentation.EnumerationValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        e.AddInformation(DirRX.EssPlatformSolution.Employees.Resources.PropertyChangedFormat(_obj.Info.Properties.PersonalAccountStatusDirRX.LocalizedName, _obj.Person.Name));
    }

    public virtual void PersonalPhoneDirRXValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        e.AddInformation(DirRX.EssPlatformSolution.Employees.Resources.PropertyChangedFormat(_obj.Info.Properties.PersonalPhoneDirRX.LocalizedName, _obj.Person.Name));
      var errors = EssPlatform.PublicFunctions.Module.Remote.CheckPersonalPhoneNumber(e.NewValue, _obj);
      if (errors.Any())
      {
        e.AddError(EssPlatform.Resources.EmployeePhoneCheckErrorsListFormat(string.Join("\n", errors)));
      }
    }

    public virtual void ViberNotifDirRXValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      if (!e.NewValue.Value && !_obj.SmsNotifDirRX.Value)
      {
        e.AddError(DirRX.EssPlatformSolution.Employees.Resources.SmsOrViberCheckError);
      }
      
      e.AddInformation(DirRX.EssPlatformSolution.Employees.Resources.PropertyChangedFormat(_obj.Info.Properties.ViberNotifDirRX.LocalizedName, _obj.Person.Name));
    }

    public virtual void SmsNotifDirRXValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      if (!e.NewValue.Value && !_obj.ViberNotifDirRX.Value)
      {
        e.AddError(DirRX.EssPlatformSolution.Employees.Resources.SmsOrViberCheckError);
      }
      
      e.AddInformation(DirRX.EssPlatformSolution.Employees.Resources.PropertyChangedFormat(_obj.Info.Properties.SmsNotifDirRX.LocalizedName, _obj.Person.Name));
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);

      if (_obj.PersonalAccountStatusDirRX != PersonalAccountStatusDirRX.InviteIsNotSent)
        e.Params.AddOrUpdate(EssPlatform.Resources.ParameterOldPhoneEmpForUpdateFormat(_obj.Id), _obj.PersonalPhoneDirRX);
      
      var showESSSettings = EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected();
      _obj.State.Properties.PersonalAccountStatusDirRX.IsVisible = showESSSettings;
      _obj.State.Properties.PersonalPhoneDirRX.IsVisible = showESSSettings;
      _obj.State.Properties.MessagesEmailDirRX.IsVisible = showESSSettings;
      _obj.State.Properties.SmsNotifDirRX.IsVisible = showESSSettings;
      _obj.State.Properties.ViberNotifDirRX.IsVisible = showESSSettings;
      _obj.State.Properties.EmailNotifDirRX.IsVisible = showESSSettings;
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);

      if (!(string.IsNullOrEmpty(_obj.MessagesEmailDirRX) || Sungero.Parties.PublicFunctions.Module.EmailIsValid(_obj.MessagesEmailDirRX)))
        e.AddWarning(_obj.Info.Properties.MessagesEmailDirRX, Sungero.Parties.Resources.WrongEmailFormat);
      
      // Если приглашение в ЛК не добавлено - отключить каналы доставки сообщений
      var enableMessageChannels = _obj.PersonalAccountStatusDirRX.HasValue && _obj.PersonalAccountStatusDirRX.Value != Employee.PersonalAccountStatusDirRX.InviteIsNotSent;
      _obj.State.Properties.SmsNotifDirRX.IsEnabled = enableMessageChannels;
      _obj.State.Properties.ViberNotifDirRX.IsEnabled = enableMessageChannels;
      _obj.State.Properties.EmailNotifDirRX.IsEnabled = enableMessageChannels;
      _obj.State.Properties.MessagesEmailDirRX.IsEnabled = _obj.EmailNotifDirRX.HasValue && _obj.EmailNotifDirRX.Value && enableMessageChannels;
    }

    public virtual void MessagesEmailDirRXValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {
      if (!(string.IsNullOrEmpty(e.NewValue) || Sungero.Parties.PublicFunctions.Module.EmailIsValid(e.NewValue)))
        e.AddError(Sungero.Parties.Resources.WrongEmailFormat);
      else if (!Sungero.Docflow.PublicFunctions.Module.IsASCII(e.NewValue))
        e.AddWarning(Sungero.Docflow.Resources.ASCIIWarning);
      
      if (e.NewValue != e.OldValue)
        e.AddInformation(DirRX.EssPlatformSolution.Employees.Resources.PropertyChangedFormat(_obj.Info.Properties.MessagesEmailDirRX.LocalizedName, _obj.Person.Name));
    }

    public override void StatusValueInput(Sungero.Presentation.EnumerationValueInputEventArgs e)
    {
      base.StatusValueInput(e);
      if (e.NewValue != null && e.NewValue != e.OldValue)
      {
        if (EssPlatform.PublicFunctions.Module.Remote.IsSingleActivePersonEmployee(_obj, _obj.Status) && e.NewValue.Value == Sungero.CoreEntities.DatabookEntry.Status.Closed
            && _obj.PersonalAccountStatusDirRX.Value != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent)
          e.AddWarning(EssPlatform.Resources.ConfirmDeleteEssUser);
      }
    }
  }
}