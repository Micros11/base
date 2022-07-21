using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.HRLite
{
  partial class AcquaintanceControlReportClientHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Client.BeforeExecuteEventArgs e)
    {
      var dialog = Dialogs.CreateInputDialog(HRAcquaintanceTasks.Resources.AcquaintanceReportDialogName);
      var defaultBusinessUnit = BusinessUnits.GetAll(b => b.Status == Sungero.Company.BusinessUnit.Status.Active).Count() == 1 ? BusinessUnits.GetAll().FirstOrDefault() : BusinessUnits.Null;
      var businessUnit = dialog.AddSelect(HRAcquaintanceTasks.Resources.BusinessUnit, true, defaultBusinessUnit);
      var departments = dialog.AddSelectMany(HRAcquaintanceTasks.Resources.Departments, false, Departments.Null);
      departments.IsEnabled = defaultBusinessUnit != null;
      var excludeSigned = dialog.AddBoolean(HRAcquaintanceTasks.Resources.ExcludeSigned, false);
      businessUnit.SetOnValueChanged((b) =>
                                     {
                                       departments.Value = new List<IDepartment>();
                                       departments.IsEnabled = b.NewValue != null;
                                       if (b.NewValue != null)
                                         departments.From(Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetAllDepartments(b.NewValue));
                                       else
                                         departments.From(new List<Sungero.Company.IDepartment>());
                                     });
      dialog.Buttons.AddOkCancel();
      if (dialog.Show() == DialogButtons.Ok)
      {
        if (departments.Value.Any())
          AcquaintanceControlReport.Departments.AddRange(departments.Value);
        else
          AcquaintanceControlReport.Departments.AddRange(Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetAllDepartments(businessUnit.Value));
        AcquaintanceControlReport.ExcludeSigned = excludeSigned.Value;
      }
      else
        e.Cancel = true;
    }

  }
}