using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatformSolution.Employee;
using System.Text.RegularExpressions;

namespace DirRX.EssPlatformSolution
{
  partial class EmployeeSharedHandlers
  {

    public override void StatusChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      base.StatusChanged(e);
      
      if (!_obj.State.IsInserted && e.NewValue != e.OldValue && e.NewValue == Employee.Status.Active)
        Functions.Employee.Remote.FillPersonalOfficeProperties(_obj);
    }

    public override void PersonChanged(Sungero.Company.Shared.EmployeePersonChangedEventArgs e)
    {
      base.PersonChanged(e);
      
      var newPerson = e.NewValue;
      if (_obj.Status == Employee.Status.Active && newPerson != null && newPerson != e.OldValue)
        Functions.Employee.Remote.FillPersonalOfficeProperties(_obj);
    }

    public virtual void PersonalPhoneDirRXChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      var mainEmployee = DirRX.EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(_obj.Person.Id, null);
      
      if (EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected() && _obj.PersonalAccountStatusDirRX != PersonalAccountStatusDirRX.InviteIsNotSent 
          && mainEmployee != null && mainEmployee.Id == _obj.Id)
      {
        var oldEmployeePhone = string.Empty;
        e.Params.TryGetValue(EssPlatform.Resources.ParameterOldPhoneEmpForUpdateFormat(_obj.Id), out oldEmployeePhone);
        
        if (string.IsNullOrEmpty(oldEmployeePhone) && oldEmployeePhone != e.OldValue)
          e.Params.AddOrUpdate(EssPlatform.Resources.ParameterOldPhoneEmpForUpdateFormat(_obj.Id), e.OldValue);
      }
      
      var errors = EssPlatform.PublicFunctions.Module.Remote.CheckPersonalPhoneNumber(e.NewValue, _obj);
      if (errors.Any())
      {
        var error = EssPlatform.Resources.EmployeePhoneCheckErrorsListFormat(string.Join("\n", errors));
        e.Params.AddOrUpdate(EssPlatform.Resources.ParameterPersonPhoneCheckErrorFormat(_obj.Id), error.ToString());
        Logger.Error(error);
      }
      else
      {
        var error = string.Empty;
        e.Params.TryGetValue(EssPlatform.Resources.ParameterPersonPhoneCheckErrorFormat(_obj.Id), out error);
        if (!string.IsNullOrEmpty(error))
          e.Params.Remove(EssPlatform.Resources.ParameterPersonPhoneCheckErrorFormat(_obj.Id));
        
        var pattern = EssPlatform.PublicConstants.Module.PhonePattern;
        var regExp = new Regex(pattern);
        if(!string.IsNullOrEmpty(e.NewValue) && !regExp.IsMatch(e.NewValue))
        {
          _obj.PersonalPhoneDirRX = EssPlatform.PublicFunctions.Module.PhoneNumberToUniversalFormat(e.NewValue);
        }
      }
      
    }
    
    public virtual void BusinessUnitDirRXChanged(DirRX.EssPlatformSolution.Shared.EmployeeBusinessUnitDirRXChangedEventArgs e)
    {
      var businessUnit = e.NewValue;
      if (businessUnit != e.OldValue && _obj.Department != null && !Equals(businessUnit, _obj.Department.BusinessUnit))
        _obj.Department = null;
    }

    public override void DepartmentChanged(Sungero.Company.Shared.EmployeeDepartmentChangedEventArgs e)
    {
      base.DepartmentChanged(e);
      var department = e.NewValue;
      if (department != null && department.BusinessUnit != null)
        _obj.BusinessUnitDirRX = department.BusinessUnit;
    }

    public virtual void EmailNotifDirRXChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      _obj.State.Properties.MessagesEmailDirRX.IsEnabled = _obj.EmailNotifDirRX.HasValue && _obj.EmailNotifDirRX.Value;
    }

    public virtual void ViberNotifDirRXChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (_obj.ViberNotifDirRX.Value)
        _obj.SmsNotifDirRX = false;
    }

    public virtual void SmsNotifDirRXChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (_obj.SmsNotifDirRX.Value)
        _obj.ViberNotifDirRX = false;
    }
  }

}