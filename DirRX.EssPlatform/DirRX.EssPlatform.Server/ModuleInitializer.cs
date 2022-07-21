using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace DirRX.EssPlatform.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      // Создание настроек модуля.
      CreateEssSettings();
      
      // Создание типов прав для модуля.
      CreateEmployeeAccessRightsType();
      
      // Выдача прав всем пользователям на справочник EssSetting.
      var allUsers = Roles.AllUsers;
      if (allUsers != null)
      {
        GrantRightOnEssSettings(allUsers);
      }
      
      CreatePAUsersWithPasswordAuthenticationRole();
    }
    
    /// <summary>
    /// Создать настройки взаимодействия с ЛК.
    /// </summary>
    public static void CreateEssSettings()
    {
      InitializationLogger.Debug("Init: Create Ess settings.");
      var essSettings = PublicFunctions.EssSetting.GetSettings();
      if (essSettings == null)
        DirRX.EssPlatform.Functions.EssSetting.CreateSettings();
    }
    
    /// <summary>
    /// Создать типы прав для справочника Сотрудники.
    /// </summary>
    public static void CreateEmployeeAccessRightsType()
    {
      InitializationLogger.Debug("Init: Create access rights type for CreateEssUser action in Employees");
      var mask = EssPlatformSolution.EmployeeOperations.CreateEssUsersDirRX;
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateAccessRightsType(Constants.Module.EmployeeTypeGuid.ToString(), DirRX.EssPlatform.Resources.CreateEssUsers.ToString(), mask,
                                                                                  mask, Sungero.CoreEntities.AccessRightsType.AccessRightsTypeArea.Both,
                                                                                  Constants.Module.DefaultAccessRightsTypeSid.CreateEssUsers, false, string.Empty);
    }
    
    /// <summary>
    /// Выдача прав всем пользователям на чтение на справочник EssSetting.
    /// </summary>
    /// <param name="allUsers">Роль "все пользователи".</param>
    public static void GrantRightOnEssSettings(IRole allUsers)
    {
      InitializationLogger.Debug("Init: Grant rights on Ess settings to all users.");
      EssSettings.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
      EssSettings.AccessRights.Save();
    }

    /// <summary>
    /// Создать роль "Пользователи ЛК с парольной аутентификацией".
    /// </summary>
    public void CreatePAUsersWithPasswordAuthenticationRole()
    {
      var roleSid = Constants.Module.PAUsersWithPasswordAuthentication;
      var role = Roles.GetAll(w => w.Sid == roleSid).FirstOrDefault();
      
      if (role == null)
      {
        InitializationLogger.Debug("Init: Create Role Пользователи ЛК с парольной аутентификацией");
        role = Roles.Create();
        role.Sid = roleSid;
        role.Name = DirRX.EssPlatform.Resources.PAUsersWithPasswordAuthenticationRoleName;
        role.Save();
      }
    }
  }
}
