using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace DirRX.PersonelUI.Server
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
      var personnelFlowUsersRole = Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.HRManagement.Resources.RoleNamePersonnelFlowUsers, DirRX.HRManagement.Resources.RoleNameDescPersonnelFlowUsers, DirRX.HRManagement.PublicConstants.Module.HRRoles.PersonnelFlowUsers);
      var hasLicense = DirRX.HRManagement.PublicFunctions.Module.Remote.CheckModuleLicense(Constants.Module.ModuleGuid, true);
      
      if (hasLicense && !personnelFlowUsersRole.RecipientLinks.Any())
      {
        var role = Roles.GetAll(g => g.Sid == DirRX.HRManagement.PublicConstants.Module.HRRoles.HRProUsers).FirstOrDefault();
        if (role != null)
        {
          personnelFlowUsersRole.RecipientLinks.AddNew().Member = role;
          personnelFlowUsersRole.Save();
        }
      }
      else if (!hasLicense && personnelFlowUsersRole.RecipientLinks.Any())
      {
        personnelFlowUsersRole.RecipientLinks.Clear();
        personnelFlowUsersRole.Save();
      }
    }
    #endregion
  }
}
