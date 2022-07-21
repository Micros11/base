using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;

namespace DirRX.HRManagement
{
  partial class VacationScheduleReportServerHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      VacationScheduleReport.StaffChiefName = string.Empty;
      VacationScheduleReport.StaffChiefJobTitle = string.Empty;
      if (VacationScheduleReport.BuisnessUnit != null) 
      {
        var ceo = Sungero.Company.Employees.As(Functions.Module.GetStaffChief(VacationScheduleReport.BuisnessUnit));
        if (ceo != null) {
          VacationScheduleReport.StaffChiefName = Sungero.Company.PublicFunctions.Employee.GetShortName(ceo, DeclensionCase.Nominative, false);
          if (ceo.JobTitle != null) {
            VacationScheduleReport.StaffChiefJobTitle = ceo.JobTitle.Name;
          }
        }
      }
    }

    public virtual IQueryable<DirRX.HRManagement.IVacation> GetVacation()
    {
      return Functions.Vacation.GetPaidVacations(VacationScheduleReport.BuisnessUnit, VacationScheduleReport.Year.Value, true);
    }
  }
}