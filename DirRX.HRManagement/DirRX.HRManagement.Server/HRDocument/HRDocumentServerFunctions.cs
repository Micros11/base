using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.HRDocument;

namespace DirRX.HRManagement.Server
{
  partial class HRDocumentFunctions
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
    /// Получить список документов ЛНА для заполнения шаблона приложения к трудовому договору.
    /// </summary>
    /// <param name="document">Приложение к трудовому договору.</param>
    /// <returns>Список документов.</returns>
    [Sungero.Core.Converter("GetLNADocuments")]
    public string GetLNADocuments(IHRDocument document)
    {
      var recordLNA = Functions.Module.GetLNARecord(_obj.Employee);
      var listString = string.Empty;
      for (var i = 1; i <= recordLNA.LNADocs.Count; i++)
      {
        var rowDocument = recordLNA.LNADocs.ElementAt(i - 1).LNADoc;
        listString += string.Format("{0}. {1}{2}", i, rowDocument.Name, Environment.NewLine);
      }
      return listString.Trim();
    }
    
    #region Базовые функции для подписания
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