using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRSolution.Employee;

namespace DirRX.HRSolution
{
  partial class EmployeeClientHandlers
  {

    public virtual void ConsentDirRXValueInput(Sungero.Presentation.EnumerationValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        e.AddInformation(DirRX.EssPlatformSolution.Employees.Resources.PropertyChangedFormat(_obj.Info.Properties.ConsentDirRX.LocalizedName, _obj.Person.Name));
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      var isSettignsConnected = DirRX.EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected();
      _obj.State.Properties.SignKindDirRX.IsVisible = isSettignsConnected;
      _obj.State.Properties.ConsentDirRX.IsRequired = isSettignsConnected;
      _obj.State.Properties.ConsentDirRX.IsVisible = isSettignsConnected;
      _obj.State.Properties.ConsentDirRX.IsEnabled = Users.Current.IncludedIn(DirRX.HRManagement.PublicConstants.Module.HRRoles.HRAdministrator) || Users.Current.IncludedIn(Roles.Administrators);
    }

  }
}