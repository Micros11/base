using System;
using Sungero.Core;

namespace DirRX.EssPlatform.Constants
{
  public static class Module
  {

    /// <summary>
    /// Pattern для проверки формата номера телефона.
    /// </summary>
    [Public]
    public const string PhonePattern = @"\+[0-9] \([0-9]{3}\) [0-9]{3}-[0-9]{2}-[0-9]{2}";

    /// <summary>
    /// Guid роли "Пользователи ЛК с парольной аутентификацией".
    /// </summary>
    public static readonly Guid PAUsersWithPasswordAuthentication = Guid.Parse("23B43509-2130-43DE-B8FE-1EF89AB02412");

    // Листы документы Excel для импорта оргструктуры.
    public static class ExcelImportWorksheetNames
    {
      /// <summary>
      /// Лист "Наши организации".
      /// </summary>
      public const string BusinessUnits = "НашиОрганизации";
      
      /// <summary>
      /// Лист "Подразделения".
      /// </summary>
      public const string Departments = "Подразделения";
      
      /// <summary>
      /// Лист "Сотрудники".
      /// </summary>
      public const string Employees = "Сотрудники";
    }
    
    // Максимальная длина пароля.
    public const int PasswordMaxLength = 50;

    // Имена сервисов ЛК, используемы в параметре audience в общениии сервисами
    public static class Audiences
    {
      /// <summary>
      /// Имя сервиса сообщений.
      /// </summary>
      public const string MessageBroker = "Directum.Core.MessageBroker";

      /// <summary>
      /// Имя сервиса ЛК.
      /// </summary>
      public const string EssSite = "Directum.Core.EssSite";
      
      /// <summary>
      /// Имя текущей системы.
      /// </summary>
      public const string CurrentSystem = "DirectumRX.HRPro";
    }
    
    // Запросы к Identity Service.
    public static class RequestApiIdentity
    {
      /// <summary>
      /// Запрос на аутентификацию.
      /// </summary>
      public const string Authentication = "api/SignIn?login={0}&password={1}&audience={2}";
      
      /// <summary>
      /// Запрос на активацию пользователя.
      /// </summary>
      public const string ActivateUser = "api/users/activate?audience={0}";

      /// <summary>
      /// Запрос на обновление пользователя.
      /// </summary>
      public const string PatchEssUser = "api/users/{0}";
      
      /// <summary>
      /// Запрос на удаление пользователя.
      /// </summary>
      public const string DeleteEssUser = "api/Users/{0}";
      
      /// <summary>
      /// Запрос на поиск пользователя.
      /// </summary>
      public const string FindEssUser = "api/Users/{0}";

    }

    // Запросы к MessageBroker
    public static class RequestApiMessageBroker
    {
      /// <summary>
      /// Запрос отправки sms.
      /// </summary>
      public const string Sms = "Sms?phone={0}";
      
      /// <summary>
      /// Запрос отправки e-mail.
      /// </summary>
      public const string Email = "email?to={0}&subject={1}";
      
      /// <summary>
      /// Запрос отправки сообщения в Viber.
      /// </summary>
      public const string Viber = "viber?phone={0}";
      
      /// <summary>
      /// Запрос на проверку подключения сервиса обмена cообщениями.
      /// </summary>
      public const string MessageBrokerHealth = "health";
      
      /// <summary>
      /// Запрос на отправку уведомления в ЛК.
      /// </summary>
      public const string MessageBrokerMessages = "Messages";
      
      /// <summary>
      /// Тип свойства, по которому идет идентификация пользователя.
      /// </summary>
      public const string CredentialType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
    }
    
    // Запросы к ЛК
    public static class RequestApiESS
    {
      /// <summary>
      /// Запрос на проверку подключения к ЛК.
      /// </summary>
      public const string ESSHealth = "health";
    }
    
    /// <summary>
    /// GUID прав.
    /// </summary>
    public static class DefaultAccessRightsTypeSid
    {
      /// <summary>
      /// Приглашение в ЛК.
      /// </summary>
      public static readonly Guid CreateEssUsers = Guid.Parse("d7df6c99-027e-4ef0-8a05-6ef3b57ab88f");
    }
    
    /// <summary>
    /// GUID справочника Сотрудники.
    /// </summary>
    public static readonly Guid EmployeeTypeGuid = Guid.Parse("b7905516-2be5-4931-961c-cb38d5677565");
    
    /// <summary>
    /// Допустимое количество попыток обращения к сервисам. До этого количества о неудачной попытке в логи пишем отладочное сообщение, после превышения - ошибку.
    /// </summary>
    public const int SendMessagePossibleRetryCount = 100;
    
    /// <summary>
    /// Допустимое количество попыток отправить приглашения пользователям в ЛК. После превышения пишем ошибку.
    /// </summary>
    public const int ActivateEssUsersPossibleRetryCount = 3;
    
    /// <summary>
    /// Допустимое количество попыток обновить данные сотрудника. После превышения пишем ошибку.
    /// </summary>
    public const int SynchronizeEmployee = 5;
    
  }
}