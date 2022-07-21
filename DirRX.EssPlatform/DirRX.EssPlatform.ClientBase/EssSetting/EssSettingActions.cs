using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatform.EssSetting;

namespace DirRX.EssPlatform.Client
{
  partial class EssSettingActions
  {
    public virtual void CheckServiceConnections(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var isError = false;
      
      // Проверку запускать, если адрес ESS непустой и соответствует формату URL.
      if (string.IsNullOrEmpty(_obj.ESSAddress) || !System.Uri.IsWellFormedUriString(_obj.ESSAddress, UriKind.Absolute))
      {
        e.AddError(DirRX.EssPlatform.EssSettings.Resources.ESSUrlIsInvalid);
        isError = true;
      }
      else
      {
        var essConnectionErrors = DirRX.EssPlatform.PublicFunctions.Module.Remote.CheckESSConnection(_obj.ESSAddress);
        if (!string.IsNullOrEmpty(essConnectionErrors))
        {
          e.AddError(Resources.ESSCheckConnectionErrorFormat(essConnectionErrors));
          isError = true;
        }
      }
      
      // Проверку запускать, если адрес Idintity Service непустой и соответствует формату URL.
      if (string.IsNullOrEmpty(_obj.IdentityServiceAddress) || !System.Uri.IsWellFormedUriString(_obj.IdentityServiceAddress, UriKind.Absolute))
      {
        e.AddError(DirRX.EssPlatform.EssSettings.Resources.IdsUrlInvalid);
        isError = true;
      }
      else
      {
        var loginResult = Functions.EssSetting.Remote.LoginIds(_obj, _obj.IdentityServicePassword, true);
        if (!string.IsNullOrEmpty(loginResult.Error))
        {
          e.AddError(loginResult.Error);
          isError = true;
        }
      }
      
      // Проверку запускать, если адрес MessageBroker непустой и соответствует формату URL.
      if (string.IsNullOrEmpty(_obj.MessageBrokerAddress) || !System.Uri.IsWellFormedUriString(_obj.MessageBrokerAddress, UriKind.Absolute))
      {
        e.AddError(DirRX.EssPlatform.EssSettings.Resources.MBUrlIsInvalid);
        isError = true;
      }
      else
      {
        var connectionErrors = DirRX.EssPlatform.PublicFunctions.Module.Remote.CheckMessageBrokerConnection(_obj.MessageBrokerAddress);
        if (!string.IsNullOrEmpty(connectionErrors))
        {
          e.AddError(Resources.MessageBrokerCheckConnectionErrorFormat(connectionErrors));
          isError = true;
        }
      }

      if (!isError)
        e.AddInformation(DirRX.EssPlatform.EssSettings.Resources.ConnectionEstablished);
    }

    public virtual bool CanCheckServiceConnections(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.AccessRights.CanUpdate() && _obj.IsUsedIdentityService == true;
    }

    public virtual void SetIdsPassword(Sungero.Domain.Client.ExecuteActionArgs e)
    {

      // Установку пароля делать только если строка соответсвует формату URL
      if (!System.Uri.IsWellFormedUriString(_obj.IdentityServiceAddress, UriKind.Absolute))
      {
        e.AddError(DirRX.EssPlatform.EssSettings.Resources.IdsUrlInvalid);
        e.ClearMessageAfterAction = true;
      }
      else
      {
        var dialog = Dialogs.CreateInputDialog(DirRX.EssPlatform.EssSettings.Resources.SetIdsPassword);
        var password = dialog.AddPasswordString(DirRX.EssPlatform.EssSettings.Resources.IdsPassword, true);
        password.MaxLength(Constants.Module.PasswordMaxLength);
        dialog.Buttons.AddOkCancel();
        dialog.Buttons.Default = DialogButtons.Ok;
        
        dialog.SetOnButtonClick(
          x =>
          {
            if (x.Button == DialogButtons.Ok && x.IsValid)
            {
              var loginResult = Functions.EssSetting.Remote.LoginIds(_obj, password.Value, false);
              if (!string.IsNullOrEmpty(loginResult.Error))
              {
                x.AddError(loginResult.Error);
              }
              else
              {
                _obj.IdentityServicePassword = loginResult.EncryptedPassword;
                Dialogs.NotifyMessage(DirRX.EssPlatform.EssSettings.Resources.IdsConnectionEstablished);
              }
            }
          });
        
        dialog.Show();
      }
    }

    public virtual bool CanSetIdsPassword(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.AccessRights.CanUpdate() && !string.IsNullOrEmpty(_obj.IdentityServiceAddress) && !string.IsNullOrEmpty(_obj.IdentityServiceLogin)
        && _obj.IsUsedIdentityService == true;
    }

  }

}