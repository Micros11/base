using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HRSettings;
using ParamKeys = DirRX.HRManagement.Constants.HRSettings.ParamKey;

namespace DirRX.HRManagement
{
  partial class HRSettingsMainVacationKindPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> MainVacationKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(l => l.Paid.Value == true);
    }
  }

  partial class HRSettingsServerHandlers
  {

    public override void AfterSave(Sungero.Domain.AfterSaveEventArgs e)
    {
      e.Params.Remove(ParamKeys.OldBusinessUnit);
    }

    public override void Saving(Sungero.Domain.SavingEventArgs e)
    {
      var oldBusinessUnit = BusinessUnits.Null;
      string oldBusinessUnitGuid;
      if (e.Params.TryGetValue(ParamKeys.OldBusinessUnit, out oldBusinessUnitGuid))
      {
        if (Equals(oldBusinessUnitGuid, Guid.Empty.ToString()))
          oldBusinessUnit = BusinessUnits.Null;
        else
          oldBusinessUnit = BusinessUnits.GetAll(bu => Equals(bu.Sid.ToString(), oldBusinessUnitGuid)).FirstOrDefault();
      }
      else
        oldBusinessUnit = _obj.BusinessUnit;
    }
    
    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.IsVacationAcquaintanceNeeded = false;
      _obj.IsScheduleUsed = false;
      _obj.NeedAdvancedSign = false;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      var isRecordDuplicateExist = HRSettingses.GetAll(l => l.BusinessUnit.Equals(_obj.BusinessUnit) && l.Id != _obj.Id).Any();
      if (_obj.BusinessUnit != null)
      {
        if (isRecordDuplicateExist)
          e.AddError(HRSettingses.Resources.RecordDuplicateError);
        _obj.Name = HRSettingses.Resources.BusinessUnitRecordNameFormat(_obj.BusinessUnit.Name);
      }
      else
      {
        if (isRecordDuplicateExist)
          e.AddError(HRSettingses.Resources.CommonRecordDuplicateError);
        _obj.Name = HRSettingses.Resources.CommonRecordName;
      }
    }
  }
}