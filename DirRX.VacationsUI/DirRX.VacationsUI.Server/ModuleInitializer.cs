using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace DirRX.VacationsUI.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      this.CreateRoles();
    }
    
    #region Создание ролей и выдача прав.
    /// <summary>
    /// Создать роли.
    /// </summary>
    public virtual void CreateRoles()
    {
      var vacationsUsersRole = Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.HRManagement.Resources.RoleNameVacationsUsers, DirRX.HRManagement.Resources.RoleNameDescVacationsUsers, DirRX.HRManagement.PublicConstants.Module.HRRoles.VacationsUsers);
      var hasLicense = DirRX.HRManagement.PublicFunctions.Module.Remote.CheckModuleLicense(Constants.Module.ModuleGuid, true);
      
      if (hasLicense && !vacationsUsersRole.RecipientLinks.Any())
      {
        var role = Roles.GetAll(g => g.Sid == DirRX.HRManagement.PublicConstants.Module.HRRoles.HRProUsers).FirstOrDefault();
        if (role != null)
        {
          vacationsUsersRole.RecipientLinks.AddNew().Member = role;
          vacationsUsersRole.Save();
        }
      }
      else if (!hasLicense && vacationsUsersRole.RecipientLinks.Any())
      {
        vacationsUsersRole.RecipientLinks.Clear();
        vacationsUsersRole.Save();
      }
    }
    #endregion
  }
}
