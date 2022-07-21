using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using Sungero.Company;
using DirRX.HRManagement.HROrder;

namespace DirRX.HRManagement.Server
{
  partial class HROrderFunctions
  {
    #region Генерация PDF с отметкой об ЭП
    /// <summary>
    /// Преобразовать документ в PDF и поставить отметку об ЭП.
    /// </summary>
    /// <param name="versionId">Id версии документа.</param>
    /// <returns>Результат преобразования в PDF.</returns>
    /// <remarks>Переопределена функция документа OfficialDocument.</remarks>
    [Remote]
    public override Sungero.Docflow.Structures.OfficialDocument.СonversionToPdfResult ConvertToPdfAndAddSignatureMark(int versionId)
    {
      var result = Functions.Module.ConvertToPdfWithSignatureMarks(_obj, new List<IEmployee>() { _obj.Employee }, new List<string>() { HRManagement.Resources.EmployeeAnchor },
                                                                   new List<IEmployee>() { _obj.OurSignatory }, new List<string>() { HRManagement.Resources.ManagerAnchor });
      return Sungero.Docflow.Structures.OfficialDocument.СonversionToPdfResult.Create(result.IsFastConvertion, result.IsOnConvertion, result.HasErrors, result.HasConvertionError, 
                                                                                      result.HasLockError, result.ErrorTitle, result.ErrorMessage);

    } 
    #endregion
    
    /// <summary>
    /// Получить ответственного за график отпусков.
    /// </summary>
    /// <param name="document">Приказ о переносе отпуска.</param>
    /// <returns>Ответственный за график отпусков.</returns>
    [Sungero.Core.Converter("GetVacationResponsible")]
    public IRecipient GetVacationResponsible(IHROrder document)
    {
      return Functions.Module.GetVacationResponsible(document.BusinessUnit);
    }
    
    #region Базовые функции для подписания
    /// <summary>
    /// Возвращает список подписывающих.
    /// </summary>
    /// <returns>Список тех, кто имеет право подписи.</returns>
    [Remote(IsPure = true)]
    public override List<Sungero.Docflow.Structures.SignatureSetting.Signatory> GetSignatories()
    {
      return base.GetSignatories();
    }
    
    /// <summary>
    /// Заполнить статус согласования "Подписан".
    /// </summary>
    public new void SetInternalApprovalStateToSigned()
    {
      base.SetInternalApprovalStateToSigned();
    }
    
    /// <summary>
    /// Заполнить подписывающего в карточке документа.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    public new void SetDocumentSignatory(Sungero.Company.IEmployee employee)
    {
      base.SetDocumentSignatory(employee);
    }
    #endregion
  }
}