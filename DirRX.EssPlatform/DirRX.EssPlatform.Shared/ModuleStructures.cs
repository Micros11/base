using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.EssPlatform.Structures.Module
{

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
    public List<DirRX.EssPlatform.Structures.Module.ImportRefillData> RefillData { get; set; }
    
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
  
  #region структуры для управления пользователями
  /// <summary>
  /// Результат задания пароля.
  /// </summary>
  partial class LoginResult
  {
    // Зашифрованный пароль.
    public string EncryptedPassword { get; set; }
    
    // Текст ошибки.
    public string Error { get; set; }
  }
  
  /// <summary>
  /// Структура для формирования данных для создания пользователей.
  /// </summary>
  [Public]
  partial class UserActivateJson
  {
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public string name { get; set; }
    
    /// <summary>
    /// Полное имя пользователя.
    /// </summary>
    public string fullName { get; set; }
    
    /// <summary>
    /// Адрес персональной электронной почты.
    /// </summary>
    public string email { get; set; }
    
    /// <summary>
    /// Номер персонального телефона пользователя.
    /// </summary>
    public string phoneNumber { get; set; }
    
    /// <summary>
    /// Способ аутентификации.
    /// </summary>
    public string authenticationProvider { get; set; }
    
    /// <summary>
    /// Дополнительный параметр.
    /// </summary>
    public DirRX.EssPlatform.Structures.Module.IClaim claims { get; set; }
  }
  
  /// <summary>
  /// Дополнительный параметр.
  /// </summary>
  [Public]
  partial class Claim
  {
    /// <summary>
    /// ИД персоны.
    /// </summary>
    public int DirectumRX_PersonId { get; set; }
  }
  
  /// <summary>
  /// Структура для формирования данных для обновления пользователя.
  /// </summary>
  partial class UserPatchJson
  {
    
    /// <summary>
    /// Полное имя пользователя.
    /// </summary>
    public string fullName { get; set; }
    
    /// <summary>
    /// Номер персонального телефона пользователя.
    /// </summary>
    public string phoneNumber { get; set; }
  }
  
  /// <summary>
  /// Структура для формирования данных о результатах создания личного кабинета для пользователей.
  /// </summary>
  [Public]
  partial class CreateEssUsersResults
  {
    /// <summary>
    /// Ошибка при выполнении запроса на создание личного кабинета.
    /// </summary>
    public string Error { get; set; }
    
    /// <summary>
    /// Количество приглашенных пользователей.
    /// </summary>
    public int InvitedUsersCount { get; set; }
    
    /// <summary>
    /// Количество пользователей, у которых не заполнен личный телефон.
    /// </summary>
    public int WithoutPhoneUsersCount { get; set; }
    
    /// <summary>
    /// Количество пользователей, которым уже отправлено приглашение личный кабинет.
    /// </summary>
    public int AlreadyInvitedUsersCount { get; set; }
    
    /// <summary>
    /// Количество пользователей, у которых уже есть личный кабинет.
    /// </summary>
    public int AlreadyAcceptedUsersCount { get; set; }
    
    /// <summary>
    /// Количество пользователей, отправка приглашений которым привела к ошибке.
    /// </summary>
    public int CatchErrorUsersCount { get; set; }
  }

  /// <summary>
  /// Структура с данными о пользователе IdS.
  /// </summary>
  [Public]
  partial class IdSUserInfo
  {
    /// <summary>
    /// Ид пользователя.
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Полное имя пользователя.
    /// </summary>
    public string FullName { get; set; }
    
    /// <summary>
    /// Сокращенное имя пользователя.
    /// </summary>
    public string ShortName { get; set; }
    
    /// <summary>
    /// Электронная почта пользователя.
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Личный телефон пользователя.
    /// </summary>
    public string PhoneNumber { get; set; }
    
    /// <summary>
    /// Метод аутентификации.
    /// </summary>
    public string AuthenticationMethod { get; set; }
  }
  #endregion
  
  #region структура для низкоуровневых запросов
  
  /// <summary>
  /// Структура с данными о результате http-запроса.
  /// </summary>
  [Public]
  partial class HttpRequestResult
  {
    /// <summary>
    /// Код результата запроса.
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Текст.
    /// </summary>
    public string RequestMessage { get; set; }
  }
  #endregion
  
  #region Сообщения-колокольчики
  [Public]
  partial class MessageBrokerNotification
  {
    public DirRX.EssPlatform.Structures.Module.IMessageBrokerNotificationIdentity Identity { get; set; }
    
    public string Title { get; set; }
    
    public string Content { get; set; }
    
    public int Priority { get; set; }
    
    public int DeliveryMethod { get; set; }
    
    public System.Collections.Generic.Dictionary<string, string> Properties { get; set; }
    
    public List<DirRX.EssPlatform.Structures.Module.IMessageBrokerNotificationAttachment> Attachments { get; set; }
  }
  
  // Получатель уведомления.
  [Public]
  partial class MessageBrokerNotificationIdentity
  {
    public string CredentialType { get; set; }
    
    public string CredentialValue { get; set; }
  }

  // вложения в сообщения
  [Public]
  partial class MessageBrokerNotificationAttachment
  {
    public string Title { get; set; }
    
    public string Url { get; set; }
  }
  #endregion

  #region Сервис интеграции
  
  /// <summary>
  /// Информация о подписи.
  /// </summary>
  partial class SignatureInfo
  {
    /// <summary>
    /// ИД подписанта.
    /// </summary>
    public int? SignatoryId {get; set; }
    
    /// <summary>
    /// ФИО подписанта.
    /// </summary>
    public string SignatoryFullName {get; set; }
    
    /// <summary>
    /// Дата подписания.
    /// </summary>
    public DateTime? SigningDate {get; set; }
    
    /// <summary>
    /// Тип подписи.
    /// </summary>
    public string SignatureType {get; set; }
    
    /// <summary>
    /// Комментарий.
    /// </summary>
    public string Comment {get; set; }
    
    /// <summary>
    /// Отпечаток подписи.
    /// </summary>
    public string CertificateThumbprint {get; set; }
  }
  
  // Вложения задачи/задания.
  [Public]
  partial class AttachmentInfo
  {
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public string EntityId { get; set; }
    
    /// <summary>
    /// Отображаемое значение.
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Тип вложения.
    /// </summary>
    public string EntityType { get; set; }
    
    /// <summary>
    /// Имя объекта вложения.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Группа вложения.
    /// </summary>
    public string Group { get; set; }
    
    /// <summary>
    /// Дата изменения.
    /// </summary>
    public DateTime? Modified { get; set; }
    
    /// <summary>
    /// Расширение (только для документов).
    /// </summary>
    public string Extension { get; set; }
    
    /// <summary>
    /// Размер в байтах (только для документов).
    /// </summary>
    public long? Size { get; set; }
  }

  // Структура для заполнения вложениями задачи/задания.
  [Public]
  partial class AttachDocumentsInfo
  {
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public string EntityId { get; set; }
    
    /// <summary>
    /// Группа вложения.
    /// </summary>
    public string Group { get; set; }
  }
  
  // Список должностей персоны.
  [Public]
  partial class PersonPostListInfo
  {
    /// <summary>
    /// Список должностей.
    /// </summary>
    public List<DirRX.EssPlatform.Structures.Module.IPersonPostInfo> positions { get; set; }
  }
  
  // Должность персоны.
  [Public]
  partial class PersonPostInfo
  {
    /// <summary>
    /// Идентификатор сотрудника.
    /// </summary>
    public int id { get; set; }
    
    /// <summary>
    /// Организация.
    /// </summary>
    public string organization { get; set; }
    
    /// <summary>
    /// Подразделение.
    /// </summary>
    public string department { get; set; }
    
    /// <summary>
    /// Должность.
    /// </summary>
    public string title { get; set; }
    
    /// <summary>
    /// Признак должности основного сотрудника.
    /// </summary>
    public bool isPrimary { get; set; }
  }
  
  /// <summary>
  /// Инофрмация об исполнителе задачи/задания.
  /// </summary>
  [Public]
  partial class PerformerInfo
  {
    /// <summary>
    /// Полное ФИО сотрудника.
    /// </summary>
    public string FullName { get; set; }
    
    /// <summary>
    /// Наименование НОР.
    /// </summary>
    public string Organization { get; set; }
    
    /// <summary>
    /// Наименование подразделения.
    /// </summary>
    public string Department { get; set; }
    
    /// <summary>
    /// Наименование должности.
    /// </summary>
    public string Position { get; set; }
  }
  
  #endregion
}