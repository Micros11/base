using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.HRManagement.Structures.Module
{

  /// <summary>
  /// Данные по отпускам сотрудрника.
  /// </summary>
  partial class EmployeeVacationData
  {
    // Детализация по видам и датам.
    public List<DirRX.HRManagement.Structures.Module.VacationDataDetail> VacationDetail { get; set; }
    
    // Осталось запланированных оплачиваемых дней отпуска.
    public string PlanedDaysLeft { get; set; }
    
    // Использовано дней без сохранения з/п.
    public string WithoutPayUsed { get; set; }
  }
  
  /// <summary>
  /// Детализация отпусков сотрудника по видам и датам.
  /// </summary>
  partial class VacationDataDetail
  {
    // Вид отпуска.
    public string VacationKind { get; set; }
    
    // Детализация по датам.
    public List<string> Dates { get; set; }
    
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
  /// Модель архива.
  /// </summary>
  partial class ZipModel
  {
    // ИД документа.
    public int DocumentId { get; set; }
    
    // ИД версии.
    public int VersionId { get; set; }
    
    // Признак, нужно ли выгружать PublicBody.
    public bool IsPublicBody { get; set; }
    
    // Имя файла.
    public string FileName { get; set; }
    
    // Относительный путь до файла.
    public List<string> FolderRelativePath { get; set; }
    
    // ИД подписи.
    public int? SignatureId { get; set; }
    
    // Размер файла.
    public long Size { get; set; }
    
    // Тело файла.
    public byte[] Body { get; set; }
  } 

  /// <summary>
  /// Описание документа для выгрузки.
  /// </summary>
  partial class ExportedDocument
  {
    // ИД документа.
    public int Id { get; set; }
    
    // Признак, является ли документ приложением.
    public bool IsAddendum { get; set; }
    
    // Признак, произошли ли ошибки при выгрузке документа.
    public bool IsFaulted { get; set; }
    
    // Признак, выгружается ли документ для печати.    
    public bool IsPrint { get; set; }
    
    // Ошибка
    public string Error { get; set; }
    
    // Папка самого документа всегда с пустым именем, это фактически корень общий для всех.
    public DirRX.HRManagement.Structures.Module.ExportedFolder Folder { get; set; }
    
    // Имя документа.
    public string Name { get; set; }
    
    // ИД ведущего документа для приложений.
    public int? LeadDocumentId { get; set; }
    
    // Признак полного комплекта подписей на документе.
    public bool isComplect { get; set; }
    
    // Признак необходимости конвертации документа в Pdf.
    public bool NeedConvert { get; set; }
  }
  
  /// <summary>
  /// Папка выгрузки.
  /// </summary>
  partial class ExportedFolder
  {
    // Имя папки.
    public string FolderName { get; set; }
    
    // Папка при подчиненной структуре папок
    public List<DirRX.HRManagement.Structures.Module.ExportedFolder> Folders { get; set; }
    
    // Относительный путь до папки
    public string ParentRelativePath { get; set; }
  }
    
  /// <summary>
  /// Результаты выгрузки.
  /// </summary>
  partial class ExportResult
  {
    // Список с описанием документов.
    public List<DirRX.HRManagement.Structures.Module.ExportedDocument> ExportedDocuments { get; set; }
    
    // Список моделей архива.
    public List<DirRX.HRManagement.Structures.Module.ZipModel> ZipModels { get; set; }
  }
  
  /// <summary>
  /// Результат преобразования документа в PDF.
  /// </summary>
  [Public]
  partial class СonversionToPdfResult
  {
    public bool IsFastConvertion { get; set; }
    
    public bool IsOnConvertion { get; set; }
    
    public bool HasErrors { get; set; }
    
    public bool HasConvertionError { get; set; }
    
    public bool HasLockError { get; set; }
    
    public string ErrorTitle { get; set; }
    
    public string ErrorMessage { get; set; }
  }
  
  /// <summary>
  /// Структура для отправки доступных работнику дней для распределения отпуска по видам.
  /// </summary>
  [Public]
  partial class AvailableVacationDays
  {
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public int Count { get; set; }
  }
  
  /// <summary>
  /// Структура для получения введенных в задаче данных по отгулу/переносу отпуска, используется в задании на доработку параметров.
  /// </summary>
  [Public]
  partial class CurrentDayOff
  {
    public int VacationId { get; set; }
    
    public DateTime Begin { get; set; }
    
    public int Duration { get; set; }
    
    public DateTime End { get; set; }
  }
  
  /// <summary>
  /// Структура для получения введенных в задаче данных по отгулу/переносу отпуска, используется в задании на доработку параметров (даты строковые).
  /// </summary>
  [Public]
  partial class CurrentDayOffStringDate
  {
    public int VacationId { get; set; }
    
    public string Begin { get; set; }
    
    public int Duration { get; set; }
    
    public string End { get; set; }
  }
  
  /// <summary>
  /// Структура для получения данных о задаче, данные будут преобразовываться в json и использоваться в Личном Кабинете в виджете с запущенными задачами.
  /// </summary>
  partial class TaskDataForWidget
  {
    public int Id { get; set; }
    
    public string ProcessName { get; set; }
    
    public string Subject { get; set; }
    
    public DateTime Created { get; set; }
    
    public string Status { get; set; }
  }
    
}