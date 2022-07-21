using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.ShiftSignDocsAssignment;

namespace DirRX.HRManagement
{
  partial class ShiftSignDocsAssignmentServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      // Выяснить, какая требуется подпись от исполнителя на данном блоке.
      var needAdvancedSign = Functions.Module.NeedAdvancedSign(Sungero.Company.Employees.As(_obj.Performer), _obj.BlockUid,
                                                               new List<string>() { Constants.VacationShiftTask.SignSeoDocAssignmentBlockId },
                                                               new List<string>() { Constants.VacationShiftTask.SignStatementAssignmentBlockId, Constants.VacationShiftTask.EmployeeSignOrderAssignmentBlockId });
      _obj.SignatureType = needAdvancedSign ? SignatureType.Approval.ToString() : SignatureType.Endorsing.ToString();
    }
  }

}