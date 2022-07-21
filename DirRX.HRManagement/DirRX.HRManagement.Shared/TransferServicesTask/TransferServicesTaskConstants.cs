using System;
using Sungero.Core;

namespace DirRX.HRManagement.Constants
{
  public static class TransferServicesTask
  {
    /// <summary>
    /// ИД блока осуществления перевода АХО.
    /// </summary>
    public const string AHOServiceBlockId = "7";
    
    /// <summary>
    /// ИД блока осуществления перевода ИТ-службой.
    /// </summary>
    public const string ITServiceBlockId = "6";
    
    /// <summary>
    /// ИД блока осуществления перевода СБ.
    /// </summary>
    public const string SBServiceBlockId = "5";
    
    /// <summary>
    /// ИД блока осуществления перевода Бухгалтерией.
    /// </summary>
    public const string AccountBlockId = "8";
    
    /// <summary>
    /// ИД блока осуществления изменений условий труда Бухгалтерией.
    /// </summary>
    public const string CWCAccountBlockId = "10";
  }
}