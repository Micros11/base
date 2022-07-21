using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementsSetting;

namespace DirRX.HRLite.Client
{
  partial class StatementsSettingFunctions
  {

    /// <summary>
    /// Проверить наличие дублей в табличных частях "Согласующие" и "Исполнители" настройки заявления.
    /// </summary>
    /// <param name="setting">Настройка заявления.</param>
    /// <param name="isApprovers">Признак проверки "Согласующих".</param>
    /// <returns></returns>
    public static bool CheckDuplicates(IStatementsSetting setting, bool isApprovers)
    {
      var numberUnique = true;
      if (isApprovers)
      {
        if (setting.Approvers.GroupBy(n => n.Number).Any(n => n.Count() > 1))
          numberUnique = false;
      }
      else
      {
        if (setting.Performers.GroupBy(n => n.Number).Any(n => n.Count() > 1))
          numberUnique = false;
      }
      
      return numberUnique;
    }

    /// <summary>
    /// Проверить уникальность номера в табличной части согласующих.
    /// </summary>
    /// <param name="setting">Настройка заявления.</param>
    /// <param name="number">Номер.</param>
    /// <param name="isApprovers">Признак проверки "Согласующих".</param>
    /// <returns>true если номер уникален иначе false</returns>
    public static bool CheckInputNumberUniqueness(IStatementsSetting setting, int number, bool isApprovers)
    {
      var numberUnique = true;
      int inputNumberUnique; 
      
      if (isApprovers)
        inputNumberUnique = setting.Approvers.Where(n => n.Number.Value == number).Select(n => n.Number.Value).Count();
      else
        inputNumberUnique = setting.Performers.Where(n => n.Number.Value == number).Select(n => n.Number.Value).Count();
      if (inputNumberUnique > 1)
        numberUnique = false;
                  
      return numberUnique;
    }

  }
}