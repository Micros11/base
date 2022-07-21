using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatformSolution.Employee;

namespace DirRX.EssPlatformSolution
{
  partial class EmployeeDepartmentPropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> DepartmentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (_obj.BusinessUnitDirRX != null)
        query = query.Where(d => Equals(d.BusinessUnit, _obj.BusinessUnitDirRX));
      return query;
    }
  }

  partial class EmployeeServerHandlers
  {

    public override IDigestModel GetDigest(Sungero.Domain.GetDigestEventArgs e)
    {
      var digest = UserDigest.Create(_obj);
      if (_obj.BusinessUnitDirRX != null)
        digest.AddEntity(_obj.BusinessUnitDirRX);
      
      if (_obj.Department != null)
        digest.AddEntity(_obj.Department);
      
      if (_obj.JobTitle != null)
        digest.AddLabel(_obj.JobTitle.Name);
      
      if (!string.IsNullOrWhiteSpace(_obj.Phone))
        digest.AddLabel(string.Format("{0} {1}", Sungero.Company.Employees.Resources.PopupPhoneDescription, _obj.Phone));
      
      if (_obj.Department != null)
      {
        var manager = _obj.Department.Manager;
        if (manager == null && _obj.Department.HeadOffice != null)
          manager = _obj.Department.HeadOffice.Manager;
        
        if (manager != null && !Equals(manager, _obj))
          digest.AddEntity(manager, Sungero.Company.Employees.Resources.PopupManagerDescription);
      }
      return digest;
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      if (!_obj.State.IsCopied)
      {
        _obj.PersonalAccountStatusDirRX = EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent;
        _obj.SmsNotifDirRX = false;
        _obj.EmailNotifDirRX = false;
        _obj.ViberNotifDirRX = false;
      }
    }

    public override void BeforeSaveHistory(Sungero.Domain.HistoryEventArgs e)
    {
      base.BeforeSaveHistory(e);
      if (_obj.State.Properties.PersonalAccountStatusDirRX.IsChanged)
      {
        var operation = new Enumeration(EssPlatformSolution.Constants.Company.Employee.StatChanged);
        e.Write(operation, operation, _obj.Info.Properties.PersonalAccountStatusDirRX.GetLocalizedValue(_obj.PersonalAccountStatusDirRX));
      }
    }

    public override void AfterSave(Sungero.Domain.AfterSaveEventArgs e)
    {
      base.AfterSave(e);
      
      //Если включена настройка "Подключить сервисы личного кабинета", то работать с Личным кабинетом.
      if (EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected())
      {
        e.Params.AddOrUpdate(EssPlatform.Resources.ParameterOldPhoneEmpForUpdateFormat(_obj.Id), _obj.PersonalPhoneDirRX);
      }
      
      // Если сохранение выполняется сотрудником и одно из свойств (Приглашение в ЛК, телефоны, email, каналы доставки) изменилось,
      // то обновить данные в других сотрудниках этой персоны.
      var isNeedUpdateEmployees = false;
      e.Params.TryGetValue(DirRX.EssPlatformSolution.Employees.Resources.IsNeedUpdateEmployeesFormat(_obj.Id), out isNeedUpdateEmployees);
      
      if (isNeedUpdateEmployees)
      {
        var employeesToChange = Employees.GetAll(w => w.Status == Sungero.Company.Employee.Status.Active && w.Person.Id == _obj.Person.Id && w.Id != _obj.Id);
        var personalAccountStatus = _obj.PersonalAccountStatusDirRX.Value.Value;
        var personalPhone = _obj.PersonalPhoneDirRX;
        var smsNotification = _obj.SmsNotifDirRX;
        var viberNotification = _obj.ViberNotifDirRX;
        var emailNotification = _obj.EmailNotifDirRX;
        var messagesEmail = _obj.MessagesEmailDirRX;
        
        foreach (var employee in employeesToChange)
        {
          var synchronizeEmployee = EssPlatform.AsyncHandlers.SynchronizeEmployee.Create();
          synchronizeEmployee.employeeId = employee.Id;
          synchronizeEmployee.personalAccountStatus = personalAccountStatus;
          synchronizeEmployee.personalPhone = personalPhone;
          synchronizeEmployee.smsNotification = smsNotification.HasValue && smsNotification.Value;
          synchronizeEmployee.viberNotification = viberNotification.HasValue && viberNotification.Value;
          synchronizeEmployee.emailNotification = emailNotification.HasValue && emailNotification.Value;
          synchronizeEmployee.messagesEmail = messagesEmail;
          synchronizeEmployee.ExecuteAsync();
        }
      }
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      // Если телефон изменился, то он должен быть корректен и уникален для каждого сотрудника одной персоны
      if (_obj.State.Properties.PersonalPhoneDirRX.IsChanged && !string.IsNullOrEmpty(_obj.PersonalPhoneDirRX))
      {
        var error = string.Empty;
        e.Params.TryGetValue(EssPlatform.Resources.ParameterPersonPhoneCheckErrorFormat(_obj.Id), out error);
        if (!string.IsNullOrEmpty(error))
          e.AddError(error);
      }
      
      // Если включена настройка "Подключить сервисы личного кабинета", то работать с Личным кабинетом.
      if (EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected())
      {
        var isSingleActivePersonEmployee = EssPlatform.PublicFunctions.Module.Remote.IsSingleActivePersonEmployee(_obj, _obj.State.Properties.Status.OriginalValue);
        // Если переводим состояние карточки единственного действующего сотрудника персоны в "Закрытая", то при первой попытке сохранить запись вывести сообщение об ошибке.
        if (_obj.State.Properties.Status.IsChanged && _obj.Status == EssPlatformSolution.Employee.Status.Closed && isSingleActivePersonEmployee)
        {
          var isForceSave = e.Params.Contains(EssPlatform.Resources.ParameterIsForceSaveFormat(_obj.Id));
          if (!isForceSave && _obj.PersonalAccountStatusDirRX.Value != EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent)
          {
            e.AddError(EssPlatform.Resources.ConfirmDeleteEssUser, _obj.Info.Actions.ForceSaveDirRX);
            return;
          }
        }
        
        // Если проставлена галочка для рассылки сообщений в ЛК по e-mail, то проверить, что указана почта.
        if (_obj.EmailNotifDirRX.HasValue && _obj.EmailNotifDirRX.Value && string.IsNullOrEmpty(_obj.MessagesEmailDirRX))
          e.AddError(DirRX.EssPlatformSolution.Employees.Resources.EmptyPersonalEMailError);

        // Если пользователь уже приглашен в ЛК, то выполнить ряд дополнительных действий
        if (_obj.PersonalAccountStatusDirRX.Value != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent) {
          var oldPhoneNumber = string.Empty;
          e.Params.TryGetValue(EssPlatform.Resources.ParameterOldPhoneEmpForUpdateFormat(_obj.Id), out oldPhoneNumber);
          // Если изменился номер телефоноа
          if (_obj.State.Properties.PersonalPhoneDirRX.IsChanged) {
            // телефон для ЛК должен быть обязательно заполнен
            if (string.IsNullOrEmpty(_obj.PersonalPhoneDirRX)) {
              e.AddError(DirRX.EssPlatformSolution.Employees.Resources.PersonalPhoneRequired);
              return;
            }
          }
          
          // если изменились ФИО сотрудника, то  нужно обновить данные пользователя в ЛК,
          if (_obj.State.Properties.Name.IsChanged || _obj.State.Properties.PersonalPhoneDirRX.IsChanged)
          {
            EssPlatform.PublicFunctions.Module.UpdateEssUser(_obj, oldPhoneNumber);
          }
        }
        
        // Если единственный действующий сотрудник персоны закрыт, удалить пользователя из личного кабинета.
        if (isSingleActivePersonEmployee && _obj.Status.Value == Sungero.CoreEntities.DatabookEntry.Status.Closed
            && _obj.PersonalAccountStatusDirRX.Value != DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent)
          EssPlatform.PublicFunctions.Module.Remote.DisconnectEssUser(_obj);
      }
      
      // Если сохранение выполняется сотрудником и одно из свойств (Приглашение в ЛК, телефоны, email, каналы доставки) изменилось,
      // то заполнить параметр для обновления данных в других сотрудниках этой персоны.
      var currentEmployee = Employees.Current;
      var propertiesState = _obj.State.Properties;
      var isNeedUpdateEmployees = currentEmployee != null && (propertiesState.PersonalAccountStatusDirRX.IsChanged || propertiesState.PersonalPhoneDirRX.IsChanged
                                                              || propertiesState.SmsNotifDirRX.IsChanged || propertiesState.ViberNotifDirRX.IsChanged
                                                              || propertiesState.EmailNotifDirRX.IsChanged || propertiesState.MessagesEmailDirRX.IsChanged);
      e.Params.AddOrUpdate(DirRX.EssPlatformSolution.Employees.Resources.
                           IsNeedUpdateEmployeesFormat(_obj.Id), isNeedUpdateEmployees);
      
      var personEmployees = Employees.GetAll(w => w.Status == Sungero.Company.Employee.Status.Active && w.Person.Id == _obj.Person.Id && w.Id != _obj.Id);
      var mainPlaceEmploymentType = EmploymentType.MainPlace;
      var employeesWithMainPlace = personEmployees.Where(w => w.EmploymentType == mainPlaceEmploymentType);
      
      if (_obj.EmploymentType == mainPlaceEmploymentType && employeesWithMainPlace.Any())
      {
        e.AddError(DirRX.EssPlatformSolution.Employees.Resources.EmployeeWithMainPlaceEmploymentTypeDouble);
      }
    }
  }

}