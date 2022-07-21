using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HRDocument;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Shared
{
  partial class HRDocumentFunctions
  {
    /// <summary>
    /// Заполнить имя.
    /// </summary>
    public override void FillName()
    {
      if (_obj.DocumentKind != null && !_obj.DocumentKind.GenerateDocumentName.Value && _obj.Name == HRDocuments.Resources.DocumentNameAutotext)
        _obj.Name = string.Empty;
      
      if (_obj.DocumentKind == null || !_obj.DocumentKind.GenerateDocumentName.Value)
        return;
      
      var name = string.Empty;
      var documentKindGuid = Functions.Module.Remote.GetDocumentKindGuid(_obj.DocumentKind);
      
      using (TenantInfo.Culture.SwitchTo())
      {
        name += _obj.DocumentKind.ShortName;
        
        if (Equals(documentKindGuid, DocKind.VacationScheduleKind))
        {
          // <Вид документа> <Наша организация> за <Год> год.
          name += _obj.BusinessUnit != null ? string.Format(" {0}", _obj.BusinessUnit.Name) : HRDocuments.Resources.BusinessUnitTemplate;
          name += HRDocuments.Resources.For + (_obj.Year != null ? _obj.Year.Value.Year.ToString() : string.Empty) + HRDocuments.Resources.Year;
          
        } 
        else if (Equals(documentKindGuid, DocKind.ScheduledVacationNoticeKind) || Equals(documentKindGuid, DocKind.AcquaintanceListKind))
        {
          // <Вид документа> для <Сотрудник> от <Дата документа>.
          name += HRDocuments.Resources.To;
          name += _obj.Employee != null ? Sungero.Company.PublicFunctions.Employee.GetShortName(_obj.Employee, DeclensionCase.Genitive, false) : HRDocuments.Resources.EmployeeTemplate;

          if (_obj.InitiationDate != null)
            name += HRDocuments.Resources.DateFrom + _obj.InitiationDate.Value.ToString("d");

        } 
        else if (Equals(documentKindGuid, DocKind.EmploymentContractKind)
                   || Equals(documentKindGuid, DocKind.TransferAdditionalAgreementKind) 
                   || Equals(documentKindGuid, DocKind.ChangeWorkConditionsAdditionalAgreementKind) 
                   || Equals(documentKindGuid, DocKind.ElectronicInteractionAgreementKind))
        {
          // <Вид документа> с <Сотрудник> от <Дата документа>.
          name += HRDocuments.Resources.EmployeeWith;
          name += _obj.Employee != null ? Sungero.Company.PublicFunctions.Employee.GetShortName(_obj.Employee, DeclensionCase.Ablative, false) : HRDocuments.Resources.EmployeeTemplate;
          
          if (_obj.RegistrationDate != null)
            name += HRDocuments.Resources.DateFrom + _obj.RegistrationDate.Value.ToString("d");
          
        } 
        else if (Equals(documentKindGuid, DocKind.JoiningStatementKind)
                   || Equals(documentKindGuid, DocKind.ProcPersonalDataAgreementKind)
                   || Equals(documentKindGuid, DocKind.TransferStatementKind) 
                   || Equals(documentKindGuid, DocKind.DismissalStatementKind) 
                   || Equals(documentKindGuid, DocKind.VacationStatementKind)
                   || Equals(documentKindGuid, DocKind.VacationShiftStatementKind)
                   || Equals(documentKindGuid, DocKind.VacationRecallStatementKind)
                   || Equals(documentKindGuid, DocKind.MaterialAidStatementKind)
                   || Equals(documentKindGuid, DocKind.BenefitStatementKind)
                   || Equals(documentKindGuid, DocKind.ChangePersonalDataStatementKind))
        {
          // <Вид документа> от <Сотрудник> от <Дата документа>.
          name += HRDocuments.Resources.DateFrom;
          name += _obj.Employee != null ? Sungero.Company.PublicFunctions.Employee.GetShortName(_obj.Employee, DeclensionCase.Genitive, false) : HRDocuments.Resources.EmployeeTemplate;
          
          if (_obj.InitiationDate != null)
            name += HRDocuments.Resources.DateFrom + _obj.InitiationDate.Value.ToString("d");
        } 
        else if (Equals(documentKindGuid, DocKind.VacationScheduleAcquaintanceListKind))
        {
          // <Вид документа> для подразделения <Подразделение>.
          name += HRDocuments.Resources.To;
          name += _obj.Department != null ? _obj.Department.Name : HRDocuments.Resources.DepartmentTemplate;
        } 
        else
        {
          // <Вид документа>: <Сотрудник> от <Дата документа>.
          name += ": ";
          name += _obj.Employee != null ? Sungero.Company.PublicFunctions.Employee.GetShortName(_obj.Employee, DeclensionCase.Genitive, false) : HRDocuments.Resources.EmployeeTemplate;
          
          if (_obj.RegistrationDate != null)
            name += HRDocuments.Resources.DateFrom + _obj.RegistrationDate.Value.ToString("d");
        }
      }
      
      _obj.Name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
    }
    
    /// <summary>
    /// Установить обязательность свойств.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
      _obj.State.Properties.Subject.IsRequired = false;
      var notVacationSchedule = !Equals(_obj.DocumentKind, Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(DocKind.VacationScheduleKind));
      _obj.State.Properties.Employee.IsRequired = notVacationSchedule;
      _obj.State.Properties.Department.IsRequired = notVacationSchedule;
    }
    
    /// <summary>
    /// Установить видимость свойств в зависимости от вида документа.
    /// </summary>
    /// <param name="documentKind">Вид документа.</param>
    public virtual void SetVisibleProperties(Sungero.Docflow.IDocumentKind documentKind)
    {
      _obj.State.Properties.EmploymentContract.IsVisible = Equals(documentKind, Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(DocKind.TransferAdditionalAgreementKind)) || 
                                                           Equals(documentKind, Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(DocKind.ChangeWorkConditionsAdditionalAgreementKind));
      var notVacationSchedule = !Equals(documentKind, Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(DocKind.VacationScheduleKind));
      var vsAcquaintanceListKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(DocKind.VacationScheduleAcquaintanceListKind);
      var notVacationScheduleAcquaintanceList = !Equals(documentKind, vsAcquaintanceListKind);
      _obj.State.Properties.Employee.IsVisible = notVacationSchedule && notVacationScheduleAcquaintanceList;
      _obj.State.Properties.Department.IsVisible = notVacationSchedule;
    }
    
  }
}