using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;

namespace DirRX.HRLite.Structures.Module
{

  /// <summary>
  /// Данные для вычисления основного сотрудника в рамках НОР.
  /// </summary>
  partial class BusinessUnitPerson
  {
    public Sungero.Parties.IPerson Person { get; set; }
    
    public IBusinessUnit BusinessUnit { get; set; }
  }
  
  /// <summary>
  /// Структура для хранения информации об ознакомлении сотрудника с документом.
  /// </summary>
  partial class AcquaintanceAssigmentStatus
  {
    public bool Signed { get; set; }
    
    public DateTime SignDate { get; set; }
    
    public string PerformerName { get; set; }
    
    public string Commentary { get; set; }
    
    public string DepartmentName { get; set; }
    
    public string JobTitle { get; set; }
  }

  /// <summary>
  /// Информация о некорректно настроенных шаблонов заявлений.
  /// </summary>
  partial class StatementTemplatesError
  {
    // Наша организация.
    public IBusinessUnit BusinessUnit { get; set; }
    
    // Вид документа.
    public IDocumentKind DocumentKind { get; set; }
    
    // Найденные шаблоны для пары НОР-Вид документа.
    public List<IDocumentTemplate> TemplateList { get; set; }
  }
  
  /// <summary>
  /// Информация для валидации подписания.
  /// </summary>
  partial class BeforeSign
  {
    public List<string> Errors { get; set; }
    
    public bool CanApprove { get; set; }
    
    public bool DocumentBodyChanged { get; set; }
  }
  
  /// <summary>
  /// Результат преобразования документа в PDF/A.
  /// </summary>
  [Public]
  partial class СonversionToPdfAResult
  {
    // Признак, что произошли ошибки при конвертации.
    public bool HasErrors { get; set; }
    
    // Текст ошибки.
    public string ErrorMessage { get; set; }
    
    // Признак, что документ был сконвертирован.
    public bool IsConverted { get; set; }
  }
  
  #region Импорт оргструктуры
  /// <summary>
  /// Результат импорта данных с листа Excel.
  /// </summary>
  partial class ImportWorksheetResult
  {
    // Количество записей на листе.
    public int TotalCount { get; set; }
    
    // Ошибки импорта.
    public List<string> Errors { get; set; }
    
    // Данные, которые необходимо дозаполнить после основного импорта.
    public List<DirRX.HRLite.Structures.Module.ImportRefillData> RefillData { get; set; }
    
    // Количество импортированных с листа записей.
    public int ImportedCount { get; set; }
    
    // Количество частично импортированных записей (из всех импортированных).
    public int PartiallyImportedCount { get; set; }
    
    // Количество НОР, которые дополнительно созданы в результате импорта (заполнено только Наименование).
    public int EmptyBusinessUnitCreatedCount { get; set; }
    
    // Количество Подразделений, которые дополнительно созданы в результате импорта (заполнено только Наименование).
    public int EmptyDepartmentCreatedCount { get; set; }
    
    // Количество Персон, которые дополнительно созданы в результате импорта.
    public int PersonCreatedCount { get; set; }
    
    // Количество Должностей, которые дополнительно созданы в результате импорта.
    public int JobTitleCreatedCount { get; set; }
  }
  
  /// <summary>
  /// Результат импорта записи из Excel.
  /// </summary>
  partial class ImportRecordResult
  {
    // Импортированная запись.
    public Sungero.CoreEntities.IDatabookEntry Record { get; set; }
    
    // Ошибка импорта.
    public string Error { get; set; }
    
    // Предупреждения.
    public string Warnings { get; set; }
    
    // Наименование НОР, которое необхоидмо дозаполнить после основного импорта.
    public string RefillBusinessUnitName { get; set; }
    
    // Наименование Подразделения, которое необхоидмо дозаполнить после основного импорта.
    public string RefillDepartmentName { get; set; }
    
    // ФИО руководителя, которого необхоидмо дозаполнить после основного импорта.
    public string RefillEmployeeName { get; set; }
    
    // Признак того, что дополнительно в результате импорта записи была создана НОР (заполнено только Наименование).
    public bool IsEmptyBusinessUnitCreated { get; set; }
    
    // Признак того, что дополнительно в результате импорта записи было создано Подразделение (заполнено только Наименование).
    public bool IsEmptyDepartmentCreated { get; set; }
    
    // Признак того, что дополнительно в результате импорта записи была создана Персона.
    public bool IsPersonCreated { get; set; }
    
    // Признак того, что дополнительно в результате импорта записи была создана Должность.
    public bool IsJobTitleCreated { get; set; }
  }
  
  /// <summary>
  /// Данные о записи, которую необходимо дозаполнить после основого импорта.
  /// </summary>
  partial class ImportRefillData
  {
    // Импортированная запись.
    public Sungero.CoreEntities.IDatabookEntry Record { get; set; }
    
    // Наименование НОР, которую нужно дозаполнить после импорта.
    public string BusinessUnitName { get; set; }
    
    // Наименование Подразделения, которое нужно дозаполнить после импорта.
    public string DepartmentName { get; set; }
    
    // Наименование Сотрудника, которого нужно дозаполнить после импорта.
    public string EmployeeName { get; set; }
  }
  #endregion
}