using System;
using Sungero.Core;

namespace DirRX.HRManagement.Constants
{
  public static class Vacation
  {
    /// <summary>
    /// Режим доступа к записи справочника Отпуска.
    /// </summary>
    public static class VacationAccess
    {
      /// <summary>
      /// Режим доступа Просмотр.
      /// </summary>
      [Public]
      public const string Read = "Read";
      
      /// <summary>
      /// Режим доступа Ограниченное редактирование.
      /// </summary>
      [Public]
      public const string LimitedChange = "LimitedChange";
      
      /// <summary>
      /// Режим доступа Редактирование.
      /// </summary>
      [Public]
      public const string Change = "Change";
      
      /// <summary>
      /// Режим доступа Полный доступ.
      /// </summary>
      [Public]
      public const string FullAccess = "FullAccess";
    }
    
    /// <summary>
    /// Режим видимости к записи справочника Отпуска.
    /// </summary>
    public static class VacationVisible
    {
      /// <summary>
      /// Режим видимости Все.
      /// </summary>
      [Public]
      public const string All = "All";
      
      /// <summary>
      /// Режим видимости Подчиненные НОР.
      /// </summary>
      [Public]
      public const string SubordinateBusinessUnits = "SubordinateBusinessUnits";
      
      /// <summary>
      /// Режим видимости Подчиненные подразделения.
      /// </summary>
      [Public]
      public const string SubordinateDepartments = "SubordinateDepartments";
      
      /// <summary>
      /// Режим видимости Подразделение.
      /// </summary>
      [Public]
      public const string Department = "Department"; 
    }
  }
}