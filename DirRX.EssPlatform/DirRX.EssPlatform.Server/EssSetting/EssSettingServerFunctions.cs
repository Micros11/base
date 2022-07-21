using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatform.EssSetting;
using DirRX.EssPlatform;

namespace DirRX.EssPlatform.Server
{
  partial class EssSettingFunctions
  {
    
    /// <summary>
    /// Авторизация в сервисе Ids.
    /// </summary>
    /// <param name="password">Пароль.</param>
    /// <param name="passwordIsEncrypted">Пароль зашифрован.</param>
    /// <returns>Структура с зашифрованным паролем и заполненным текстом ошибки, если авторизация не успешна.</returns>
    [Remote]
    public DirRX.EssPlatform.Structures.Module.LoginResult LoginIds(string password, bool passwordIsEncrypted)
    {
      var loginResult = new Structures.Module.LoginResult();
      var token = string.Empty;
      
      // Проверить подключение по логину и паролю.
      try
      {
        if (passwordIsEncrypted && !string.IsNullOrEmpty(password))
          password = Encryption.Decrypt(password);
        
        token = DirRX.EssPlatform.PublicFunctions.Module.Remote.GetAnAuthenticationToken(_obj.IdentityServiceLogin, password, DirRX.EssPlatform.Constants.Module.Audiences.CurrentSystem, _obj.IdentityServiceAddress);
      }
      catch (Exception e)
      {
        if (string.Equals(e.Message, DirRX.EssPlatform.Resources.AuthenticationErrorText))
          loginResult.Error = DirRX.EssPlatform.Resources.LoginError;
        else
          loginResult.Error = DirRX.EssPlatform.Resources.ErrorWhileConnectToIds;
        return loginResult;
      }
      
      if (string.IsNullOrEmpty(token))
      {
        loginResult.Error = DirRX.EssPlatform.Resources.ErrorWhileConnectToIds;
        return loginResult;
      }
      
      var encryptedPassword = Encryption.Encrypt(password ?? string.Empty);
      if (encryptedPassword != _obj.IdentityServicePassword)
      {
        loginResult.EncryptedPassword = encryptedPassword;
      }
      
      return loginResult;
    }

    /// <summary>
    /// Создать настройки взаимодействия с Личным кабинетом.
    /// </summary>
    [Remote]
    public static void CreateSettings()
    {
      var essSettings = EssSettings.Create();
      essSettings.IsUsedIdentityService = false;
      essSettings.Save();
    }
    
    /// <summary>
    /// Проверить включена ли настройка "Подключить сервисы личного кабинета".
    /// </summary>
    /// <returns>Значение свойства Подключить сервисы личного кабинета.</returns>
    [Public, Remote]
    public static bool SettingsConnected()
    {
      var settings = PublicFunctions.EssSetting.GetSettings();
      if (settings != null)        
        return settings.IsUsedIdentityService == true;
      
      return false;
    }

  }
}